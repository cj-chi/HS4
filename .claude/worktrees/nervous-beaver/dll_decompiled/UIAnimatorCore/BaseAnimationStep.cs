using System;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public abstract class BaseAnimationStep : MonoBehaviour
{
	private readonly float MIN_DURATION = 0.001f;

	[SerializeField]
	protected AnimStepVariableFloat m_delay = new AnimStepVariableFloat();

	[SerializeField]
	protected AnimStepVariableFloat m_duration = new AnimStepVariableFloat(1f);

	[SerializeField]
	protected EasingEquation m_easing;

	[SerializeField]
	protected AudioClipData[] m_audioClipDatas;

	[SerializeField]
	private int m_numTargets = 1;

	[SerializeField]
	private bool m_waitForPreviousComplete;

	private float[] m_timers;

	public static string EditorDisplayName => "Anim Step";

	public abstract string StepTitleDisplay { get; }

	public virtual bool UseEasing => true;

	public virtual bool IsEffectStep => false;

	public virtual bool SetCustomInitialDuration => false;

	public virtual float CustomInitialDuration => 1f;

	public int NumTargets => m_numTargets;

	public float TotalExecutionDuration => m_delay.LargestValue + Mathf.Max(m_duration.LargestValue, MIN_DURATION);

	public bool WaitForPreviousComplete => m_waitForPreviousComplete;

	public float GetTotalExecutionDuration(int a_targetIndex)
	{
		return m_delay.GetValue(a_targetIndex) + Mathf.Max(m_duration.GetValue(a_targetIndex), MIN_DURATION);
	}

	public bool HasStarted(int a_targetIndex)
	{
		CheckTimerValueArray();
		return m_timers[a_targetIndex] > 0f;
	}

	public bool IsFinished(int a_targetIndex)
	{
		CheckTimerValueArray();
		return m_timers[a_targetIndex] - m_delay.GetValue(a_targetIndex) >= Mathf.Max(m_duration.GetValue(a_targetIndex), MIN_DURATION);
	}

	public bool IsCompletelyFinished()
	{
		CheckTimerValueArray();
		for (int i = 0; i < m_timers.Length; i++)
		{
			if (!IsFinished(i))
			{
				return false;
			}
		}
		return true;
	}

	protected abstract void SetAnimation(int a_targetIndex, float a_easedProgress);

	protected abstract bool SetStepAllValuesFromCurrentState(GameObject[] a_targetObjects);

	protected abstract bool SetStepDefaultValuesFromCurrentState(GameObject[] a_targetObjects);

	public void SetNumTargets(int a_numTargets)
	{
		m_numTargets = a_numTargets;
		CheckTimerValueArray();
	}

	public bool SetAllValuesFromCurrentState(GameObject[] a_targetObjects)
	{
		m_numTargets = a_targetObjects.Length;
		CheckTimerValueArray();
		InitBaseVariables(m_numTargets);
		return SetStepAllValuesFromCurrentState(a_targetObjects);
	}

	public bool SetDefaultValuesFromCurrentState(GameObject[] a_targetObjects)
	{
		m_numTargets = a_targetObjects.Length;
		CheckTimerValueArray();
		InitBaseVariables(m_numTargets);
		return SetStepDefaultValuesFromCurrentState(a_targetObjects);
	}

	private void CheckTimerValueArray()
	{
		if (m_timers == null || m_timers.Length != m_numTargets)
		{
			m_timers = new float[m_numTargets];
		}
	}

	private void SetTimerValues(float a_value)
	{
		CheckTimerValueArray();
		for (int i = 0; i < m_timers.Length; i++)
		{
			m_timers[i] = a_value;
		}
	}

	private void InitBaseVariables(int a_numTargets)
	{
		m_delay.Initialise(a_numTargets);
		m_duration.Initialise(a_numTargets);
		if (m_audioClipDatas != null)
		{
			for (int i = 0; i < m_audioClipDatas.Length; i++)
			{
				m_audioClipDatas[i].UpdateNumTargets(a_numTargets);
			}
		}
	}

	public void ResetToStart(AnimSetupType a_animType)
	{
		for (int i = 0; i < m_numTargets; i++)
		{
			SetAnimationTimer(i, a_animType, 0f);
		}
		if (m_audioClipDatas != null)
		{
			for (int j = 0; j < m_audioClipDatas.Length; j++)
			{
				m_audioClipDatas[j].ResetAll();
			}
		}
	}

	public void ResetToEnd(AnimSetupType a_animType)
	{
		SetTimerValues(TotalExecutionDuration);
		for (int i = 0; i < m_numTargets; i++)
		{
			SetAnimationProgress(i, a_animType, 1f);
		}
	}

	public void SetToMasterState()
	{
		for (int i = 0; i < m_numTargets; i++)
		{
			SetAnimationTimer(i, AnimSetupType.Outro, 0f);
		}
	}

	public void SetAsMasterState(GameObject[] a_targetObjects)
	{
		SetDefaultValuesFromCurrentState(a_targetObjects);
		SetToMasterState();
	}

	public void UpdateState(UIAnimator a_uiAnimatorRef, int a_targetIndex, AnimSetupType a_animType, float a_deltaTime)
	{
		m_timers[a_targetIndex] += a_deltaTime;
		if (m_delay.GetValue(a_targetIndex) > 0f && m_timers[a_targetIndex] <= m_delay.GetValue(a_targetIndex))
		{
			return;
		}
		float num = m_timers[a_targetIndex] - m_delay.GetValue(a_targetIndex);
		if (m_audioClipDatas != null && m_audioClipDatas.Length != 0)
		{
			for (int i = 0; i < m_audioClipDatas.Length; i++)
			{
				if (m_audioClipDatas[i].Clip != null && !m_audioClipDatas[i].HasAudioClipActivated(a_targetIndex) && ((m_audioClipDatas[i].TriggerPoint == AudioClipData.CLIP_TRIGGER_POINT.START_OF_ANIM_STEP && num > m_audioClipDatas[i].Delay.GetValue(a_targetIndex)) || (m_audioClipDatas[i].TriggerPoint == AudioClipData.CLIP_TRIGGER_POINT.END_OF_ANIM_STEP && num > m_duration.GetValue(a_targetIndex) - m_audioClipDatas[i].Delay.GetValue(a_targetIndex))))
				{
					a_uiAnimatorRef.TriggerAudioClip(m_audioClipDatas[i], a_targetIndex);
					m_audioClipDatas[i].MarkAudioClipActivated(a_targetIndex);
				}
			}
		}
		SetAnimationProgress(a_targetIndex, a_animType, num / Mathf.Max(m_duration.GetValue(a_targetIndex), MIN_DURATION));
	}

	public void SetAnimationProgress(int a_targetIndex, AnimSetupType a_animType, float a_progress)
	{
		if (!IsEffectStep && a_animType == AnimSetupType.Outro)
		{
			a_progress = 1f - a_progress;
		}
		if (a_progress < 0f)
		{
			a_progress = 0f;
		}
		else if (a_progress > 1f)
		{
			a_progress = 1f;
		}
		if (UseEasing)
		{
			a_progress = EasingManager.GetEaseProgress(m_easing, a_progress);
		}
		SetAnimation(a_targetIndex, a_progress);
	}

	public void SetAnimationTimer(int a_targetIndex, AnimSetupType a_animType, float a_timerValue, bool a_forceLinearTimings = false)
	{
		CheckTimerValueArray();
		m_timers[a_targetIndex] = a_timerValue;
		if (a_timerValue - m_delay.GetValue((!a_forceLinearTimings) ? a_targetIndex : 0) < 0f)
		{
			SetAnimationProgress(a_targetIndex, a_animType, 0f);
		}
		else
		{
			SetAnimationProgress(a_targetIndex, a_animType, (a_timerValue - m_delay.GetValue((!a_forceLinearTimings) ? a_targetIndex : 0)) / Mathf.Max(m_duration.GetValue((!a_forceLinearTimings) ? a_targetIndex : 0), MIN_DURATION));
		}
	}
}
