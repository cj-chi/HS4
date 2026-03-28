using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIAnimatorCore;

[AddComponentMenu("UI/UI Animator", 0)]
[ExecuteInEditMode]
public class UIAnimator : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private ActionMatrixData m_playOnEnableAMD;

	[SerializeField]
	private ActionMatrixData m_startPoseAMD;

	[SerializeField]
	private ActionMatrixData m_afterIntroAMD = new ActionMatrixData(a_enabledByDefault: false);

	[SerializeField]
	private ActionMatrixData m_afterLoopAMD = new ActionMatrixData(a_enabledByDefault: false);

	[SerializeField]
	private ActionMatrixData m_onPointerEnterAMD = new ActionMatrixData(a_enabledByDefault: false);

	[SerializeField]
	private ActionMatrixData m_onPointerExitAMD = new ActionMatrixData(a_enabledByDefault: false);

	[SerializeField]
	private PlayTimeMode m_timeMode = PlayTimeMode.REAL_TIME;

	[SerializeField]
	private List<AnimationSetup> m_animationSetups;

	[SerializeField]
	private float m_timer;

	[SerializeField]
	private int m_currentAnimSetupIndex;

	[SerializeField]
	private Transform m_audioSourceContainer;

	[SerializeField]
	private List<AudioSource> m_audioSources;

	[SerializeField]
	private bool m_playLoopAnimInfinitely = true;

	[SerializeField]
	private int m_numLoopIterations;

	private const float MAX_FRAME_DELTA = 0.05f;

	private Action m_onFinishAction;

	private int m_currentLoopIterationCount;

	private int m_currentStageIndex;

	private bool m_playingAnimation;

	private float m_lastRealtime;

	private bool m_paused;

	private bool m_resetAudioEnabledStateChange;

	private static bool s_isAudioEnabled = true;

	private static bool s_audioEnabledStateChanged = false;

	public List<AnimationSetup> AnimationSetups => m_animationSetups;

	public List<AnimationStage> CurrentAnimStages
	{
		get
		{
			if (m_animationSetups != null && m_animationSetups.Count > m_currentAnimSetupIndex)
			{
				return m_animationSetups[m_currentAnimSetupIndex].AnimationStages;
			}
			return null;
		}
	}

	public PlayTimeMode TimeMode
	{
		get
		{
			return m_timeMode;
		}
		set
		{
			m_timeMode = value;
		}
	}

	public float Timer => m_timer;

	public AnimSetupType CurrentAnimType => (AnimSetupType)m_currentAnimSetupIndex;

	public bool IsPlaying => m_playingAnimation;

	public bool Paused
	{
		get
		{
			return m_paused;
		}
		set
		{
			m_paused = value;
		}
	}

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			if (m_playOnEnableAMD.IsEnabled)
			{
				PlayAnimation(m_playOnEnableAMD.AnimType, m_playOnEnableAMD.Delay);
				return;
			}
			CheckDataInit();
			SetAnimType(m_startPoseAMD.AnimType);
		}
	}

	private void Start()
	{
		CheckDataInit();
		if (Application.isPlaying)
		{
			ResetToStart();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_onPointerEnterAMD.IsEnabled)
		{
			PlayAnimation(m_onPointerEnterAMD.AnimType, m_onPointerEnterAMD.Delay);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_onPointerExitAMD.IsEnabled)
		{
			PlayAnimation(m_onPointerExitAMD.AnimType, m_onPointerExitAMD.Delay);
		}
	}

	private void CheckDataInit()
	{
		if (m_animationSetups == null || m_animationSetups.Count < 3)
		{
			m_animationSetups = new List<AnimationSetup>
			{
				new AnimationSetup(),
				new AnimationSetup(),
				new AnimationSetup()
			};
		}
	}

	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (m_playingAnimation && !m_paused)
		{
			if (UpdateState((m_timeMode == PlayTimeMode.REAL_TIME) ? (Time.realtimeSinceStartup - m_lastRealtime) : Time.deltaTime))
			{
				AnimSetupType currentAnimType = CurrentAnimType;
				m_playingAnimation = false;
				if (m_onFinishAction != null)
				{
					m_onFinishAction();
					m_onFinishAction = null;
				}
				if (m_animationSetups[m_currentAnimSetupIndex].OnFinishedAction != null)
				{
					m_animationSetups[m_currentAnimSetupIndex].OnFinishedAction.Invoke();
				}
				if (currentAnimType == AnimSetupType.Intro && m_afterIntroAMD.IsEnabled)
				{
					PlayAnimation(m_afterIntroAMD.AnimType, m_afterIntroAMD.Delay);
				}
				else if (currentAnimType == AnimSetupType.Loop && m_afterLoopAMD.IsEnabled)
				{
					PlayAnimation(m_afterLoopAMD.AnimType, m_afterLoopAMD.Delay);
				}
			}
			if (m_timeMode == PlayTimeMode.REAL_TIME)
			{
				m_lastRealtime = Time.realtimeSinceStartup;
			}
		}
		if (m_resetAudioEnabledStateChange)
		{
			s_audioEnabledStateChanged = false;
			m_resetAudioEnabledStateChange = false;
		}
		if (!s_audioEnabledStateChanged || m_audioSources == null)
		{
			return;
		}
		for (int i = 0; i < m_audioSources.Count; i++)
		{
			if (m_audioSources[i].isPlaying)
			{
				m_audioSources[i].mute = !s_isAudioEnabled;
			}
		}
		m_resetAudioEnabledStateChange = true;
	}

	public void SetPlayOnEnable(bool a_playOnEnable, AnimSetupType a_animToPlay)
	{
		SetPlayOnEnable(a_playOnEnable, a_animToPlay, 0f);
	}

	public void SetPlayOnEnable(bool a_playOnEnable, AnimSetupType a_animToPlay, float a_delay)
	{
		m_playOnEnableAMD.IsEnabled = a_playOnEnable;
		m_playOnEnableAMD.AnimType = a_animToPlay;
		m_playOnEnableAMD.Delay = a_delay;
	}

	public void SetPlayAfterIntro(bool a_playAfterIntro, AnimSetupType a_animToPlay)
	{
		SetPlayAfterIntro(a_playAfterIntro, a_animToPlay, 0f);
	}

	public void SetPlayAfterIntro(bool a_playAfterIntro, AnimSetupType a_animToPlay, float a_delay)
	{
		m_afterIntroAMD.IsEnabled = a_playAfterIntro;
		m_afterIntroAMD.AnimType = a_animToPlay;
		m_afterIntroAMD.Delay = a_delay;
	}

	public void SetPlayAfterLoop(bool a_playAfterLoop, AnimSetupType a_animToPlay)
	{
		SetPlayAfterLoop(a_playAfterLoop, a_animToPlay, 0f);
	}

	public void SetPlayAfterLoop(bool a_playAfterLoop, AnimSetupType a_animToPlay, float a_delay)
	{
		m_afterLoopAMD.IsEnabled = a_playAfterLoop;
		m_afterLoopAMD.AnimType = a_animToPlay;
		m_afterLoopAMD.Delay = a_delay;
	}

	public void SetPlayOnPointerEnter(bool a_playOnPointerEnter, AnimSetupType a_animToPlay)
	{
		SetPlayOnPointerEnter(a_playOnPointerEnter, a_animToPlay, 0f);
	}

	public void SetPlayOnPointerEnter(bool a_playOnPointerEnter, AnimSetupType a_animToPlay, float a_delay)
	{
		m_onPointerEnterAMD.IsEnabled = a_playOnPointerEnter;
		m_onPointerEnterAMD.AnimType = a_animToPlay;
		m_onPointerEnterAMD.Delay = a_delay;
	}

	public void SetPlayOnPointerExit(bool a_playOnPointerExit, AnimSetupType a_animToPlay)
	{
		SetPlayOnPointerExit(a_playOnPointerExit, a_animToPlay, 0f);
	}

	public void SetPlayOnPointerExit(bool a_playOnPointerExit, AnimSetupType a_animToPlay, float a_delay)
	{
		m_onPointerExitAMD.IsEnabled = a_playOnPointerExit;
		m_onPointerExitAMD.AnimType = a_animToPlay;
		m_onPointerExitAMD.Delay = a_delay;
	}

	public void PlayAnimation(AnimSetupType a_animType)
	{
		PlayAnimation(a_animType, 0f, null);
	}

	public void PlayAnimation(AnimSetupType a_animType, float a_delay)
	{
		PlayAnimation(a_animType, a_delay, null);
	}

	public void PlayAnimation(AnimSetupType a_animType, Action a_onFinish)
	{
		PlayAnimation(a_animType, 0f, a_onFinish);
	}

	public void PlayAnimation(AnimSetupType a_animType, float a_delay, Action a_onFinish)
	{
		CheckDataInit();
		SetAnimType(a_animType);
		m_onFinishAction = a_onFinish;
		if (a_delay > 0f)
		{
			StartCoroutine(PlayAnimationAfterDelay(a_animType, a_delay, a_onFinish));
		}
		else
		{
			PlayCurrentAnimationSetup();
		}
	}

	private IEnumerator PlayAnimationAfterDelay(AnimSetupType a_animType, float a_delay, Action a_onFinish)
	{
		yield return new WaitForSeconds(a_delay);
		PlayCurrentAnimationSetup();
	}

	private void PlayCurrentAnimationSetup()
	{
		if (m_timeMode == PlayTimeMode.REAL_TIME)
		{
			m_lastRealtime = Time.realtimeSinceStartup;
		}
		m_playingAnimation = true;
		if (m_animationSetups[m_currentAnimSetupIndex].OnStartAction != null)
		{
			m_animationSetups[m_currentAnimSetupIndex].OnStartAction.Invoke();
		}
	}

	private List<AnimationStage> GetAnimationStepStages(AnimSetupType a_animType)
	{
		if (m_animationSetups != null && m_animationSetups.Count > (int)a_animType)
		{
			return m_animationSetups[(int)a_animType].AnimationStages;
		}
		return null;
	}

	public void SetAnimType(AnimSetupType a_animType)
	{
		m_currentAnimSetupIndex = (int)a_animType;
		ResetToDefault();
		if (Application.isPlaying)
		{
			ResetToStart();
		}
	}

	public void GrabCurrentStateAsMaster()
	{
		if (m_animationSetups == null)
		{
			return;
		}
		for (int i = 0; i < m_animationSetups.Count; i++)
		{
			for (int j = 0; j < m_animationSetups[i].AnimationStages.Count; j++)
			{
				if (m_animationSetups[i].AnimationStages[j].AnimationInstances != null)
				{
					for (int k = 0; k < m_animationSetups[i].AnimationStages[j].AnimationInstances.Count; k++)
					{
						m_animationSetups[i].AnimationStages[j].AnimationInstances[k].SetAsMasterState();
					}
				}
			}
		}
	}

	public void ResetToDefault()
	{
		ResetToStart(AnimSetupType.Outro);
		ResetToStart(AnimSetupType.Loop);
		ResetToEnd(AnimSetupType.Intro);
	}

	public void ResetToStart()
	{
		ResetToStart(CurrentAnimType);
	}

	public void ResetToStart(AnimSetupType a_animSetupType)
	{
		m_currentStageIndex = 0;
		m_timer = 0f;
		if (m_animationSetups == null || (int)a_animSetupType >= m_animationSetups.Count)
		{
			return;
		}
		if (a_animSetupType != AnimSetupType.Loop)
		{
			m_currentLoopIterationCount = 0;
		}
		List<AnimationStage> animationStages = m_animationSetups[(int)a_animSetupType].AnimationStages;
		if (animationStages != null)
		{
			for (int i = 0; i < animationStages.Count; i++)
			{
				animationStages[i].ResetToStart(a_animSetupType);
			}
		}
		if (!Application.isPlaying)
		{
			ForceStopAllAudioSources();
		}
	}

	public void ResetToEnd()
	{
		ResetToEnd(CurrentAnimType);
	}

	public void ResetToEnd(AnimSetupType a_animSetupType)
	{
		if (m_animationSetups == null || (int)a_animSetupType >= m_animationSetups.Count)
		{
			return;
		}
		List<AnimationStage> animationStages = m_animationSetups[(int)a_animSetupType].AnimationStages;
		m_currentStageIndex = animationStages.Count - 1;
		m_timer = GetAnimationDuration();
		if (animationStages != null)
		{
			for (int i = 0; i < animationStages.Count; i++)
			{
				animationStages[i].ResetToEnd(a_animSetupType);
			}
		}
	}

	public void ForceStopAllAudioSources()
	{
		if (m_audioSources == null)
		{
			return;
		}
		for (int i = 0; i < m_audioSources.Count; i++)
		{
			if (m_audioSources[i] == null)
			{
				m_audioSources.RemoveAt(i);
				i--;
			}
			else if (m_audioSources[i].isPlaying)
			{
				m_audioSources[i].Stop();
			}
		}
	}

	public static void SetUIAudioState(bool a_audioIsPlaying)
	{
		if (s_isAudioEnabled != a_audioIsPlaying)
		{
			s_isAudioEnabled = a_audioIsPlaying;
			s_audioEnabledStateChanged = true;
		}
	}

	public bool UpdateState(AnimSetupType a_animType, float a_deltaTime)
	{
		m_currentAnimSetupIndex = (int)a_animType;
		return UpdateState(a_deltaTime, a_isPrimaryAnimator: true);
	}

	public bool UpdateState(AnimSetupType a_animType, float a_deltaTime, bool a_isPrimaryAnimator)
	{
		m_currentAnimSetupIndex = (int)a_animType;
		return UpdateState(a_deltaTime, a_isPrimaryAnimator);
	}

	public bool UpdateState(float a_deltaTime)
	{
		return UpdateState(a_deltaTime, a_isPrimaryAnimator: true);
	}

	public bool UpdateState(float a_deltaTime, bool a_isPrimaryAnimator)
	{
		if (a_deltaTime > 0.05f)
		{
			a_deltaTime = 0.05f;
		}
		List<AnimationStage> currentAnimStages = CurrentAnimStages;
		if (currentAnimStages == null)
		{
			return true;
		}
		if (currentAnimStages.Count <= m_currentStageIndex)
		{
			if (!(CurrentAnimType == AnimSetupType.Loop && a_isPrimaryAnimator) || (!m_playLoopAnimInfinitely && m_numLoopIterations > 0 && m_currentLoopIterationCount >= m_numLoopIterations - 1))
			{
				return true;
			}
			ResetToStart();
			m_currentLoopIterationCount++;
		}
		m_timer += a_deltaTime;
		if (currentAnimStages[m_currentStageIndex].UpdateState(this, CurrentAnimType, a_deltaTime))
		{
			m_currentStageIndex++;
		}
		return false;
	}

	public void SetAnimationTimer(AnimSetupType a_animType, float a_timerValue, bool a_forceLinearTimings = false)
	{
		m_currentAnimSetupIndex = (int)a_animType;
		SetAnimationTimer(a_timerValue, a_forceLinearTimings);
	}

	public void SetAnimationTimer(float a_timerValue, bool a_forceLinearTimings = false)
	{
		List<AnimationStage> currentAnimStages = CurrentAnimStages;
		if (currentAnimStages == null)
		{
			return;
		}
		m_timer = a_timerValue;
		AnimSetupType currentAnimType = CurrentAnimType;
		float num = a_timerValue;
		float num2 = 0f;
		m_currentStageIndex = 0;
		for (int i = 0; i < currentAnimStages.Count; i++)
		{
			num2 = currentAnimStages[i].GetTotalDuration(currentAnimType);
			if (num > num2)
			{
				for (int j = 0; j < currentAnimStages[i].AnimationInstances.Count; j++)
				{
					for (int k = 0; k < currentAnimStages[i].AnimationInstances[j].AnimationSteps.Count; k++)
					{
						if (!currentAnimStages[i].AnimationInstances[j].AnimationSteps[k].IsEffectStep)
						{
							currentAnimStages[i].AnimationInstances[j].AnimationSteps[k].ResetToEnd(currentAnimType);
						}
					}
				}
			}
			else if (num >= 0f)
			{
				for (int l = 0; l < currentAnimStages[i].AnimationInstances.Count; l++)
				{
					currentAnimStages[i].AnimationInstances[l].SetAnimationTimer(currentAnimType, num - currentAnimStages[i].StartDelay, a_forceLinearTimings);
				}
				m_currentStageIndex = i;
			}
			else
			{
				for (int m = 0; m < currentAnimStages[i].AnimationInstances.Count; m++)
				{
					for (int n = 0; n < currentAnimStages[i].AnimationInstances[m].AnimationSteps.Count; n++)
					{
						if (!currentAnimStages[i].AnimationInstances[m].AnimationSteps[n].IsEffectStep)
						{
							currentAnimStages[i].AnimationInstances[m].AnimationSteps[n].ResetToStart(currentAnimType);
						}
					}
				}
			}
			num -= num2;
		}
	}

	public void TriggerAudioClip(AudioClipData a_audioClipData, int a_targetIndex)
	{
		if (a_audioClipData.Clip == null)
		{
			return;
		}
		if (m_audioSources == null)
		{
			m_audioSources = new List<AudioSource>();
		}
		AudioSource audioSource = null;
		for (int i = 0; i < m_audioSources.Count; i++)
		{
			if (m_audioSources[i] == null)
			{
				m_audioSources.RemoveAt(i);
				i--;
			}
			else if (!m_audioSources[i].isPlaying)
			{
				audioSource = m_audioSources[i];
				break;
			}
		}
		if (audioSource == null)
		{
			audioSource = GetNewAudioSource();
		}
		audioSource.clip = a_audioClipData.Clip;
		audioSource.playOnAwake = false;
		audioSource.spatialBlend = 0f;
		audioSource.volume = a_audioClipData.Volume.GetValue(a_targetIndex);
		audioSource.pitch = a_audioClipData.Pitch.GetValue(a_targetIndex);
		audioSource.time = a_audioClipData.OffsetTime.GetValue(a_targetIndex);
		audioSource.mute = !s_isAudioEnabled;
		audioSource.Play();
	}

	private AudioSource GetNewAudioSource()
	{
		if (m_audioSourceContainer == null)
		{
			m_audioSourceContainer = new GameObject("UI Animator - Audio").transform;
			m_audioSourceContainer.SetParent(base.transform);
			m_audioSourceContainer.localPosition = Vector3.zero;
		}
		GameObject obj = new GameObject("UI Animator - AudioSource");
		obj.transform.SetParent(m_audioSourceContainer);
		obj.transform.localPosition = Vector3.zero;
		AudioSource audioSource = (AudioSource)obj.AddComponent(typeof(AudioSource));
		m_audioSources.Add(audioSource);
		return audioSource;
	}

	public void AddNewAnimationInstance(GameObject[] a_newTargets, int a_stageIndex = -1)
	{
		List<AnimationStage> currentAnimStages = CurrentAnimStages;
		if (currentAnimStages.Count == 0)
		{
			currentAnimStages.Add(new AnimationStage());
		}
		if (a_stageIndex < 0 || a_stageIndex >= currentAnimStages.Count)
		{
			a_stageIndex = currentAnimStages.Count - 1;
		}
		currentAnimStages[a_stageIndex].AddNewAnimationInstance(a_newTargets);
	}

	public float GetAnimationDuration()
	{
		return GetAnimationDuration(CurrentAnimType);
	}

	public float GetAnimationDuration(AnimSetupType a_animType)
	{
		CheckDataInit();
		List<AnimationStage> animationStages = m_animationSetups[(int)a_animType].AnimationStages;
		float num = 0f;
		if (animationStages != null)
		{
			for (int i = 0; i < animationStages.Count; i++)
			{
				num += animationStages[i].GetTotalDuration(a_animType);
			}
		}
		return num;
	}
}
