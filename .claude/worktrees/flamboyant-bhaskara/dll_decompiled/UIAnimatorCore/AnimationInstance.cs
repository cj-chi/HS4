using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public class AnimationInstance
{
	[SerializeField]
	private GameObject[] m_targetObjects;

	[SerializeField]
	private List<BaseAnimationStep> m_animationSteps;

	[SerializeField]
	private int[] m_targetStepIndexes;

	[SerializeField]
	private UIAnimator m_uiAnimatorSubModule;

	[SerializeField]
	private float m_startDelay;

	private double m_timer;

	private bool m_finishedFlag;

	public GameObject[] TargetObjects => m_targetObjects;

	public List<BaseAnimationStep> AnimationSteps => m_animationSteps;

	public bool IsUiAnimatorSubModule => m_uiAnimatorSubModule != null;

	public string Title
	{
		get
		{
			if (IsUiAnimatorSubModule)
			{
				return "[UI Animator] '" + m_uiAnimatorSubModule.name + "'";
			}
			string text = string.Empty;
			if (m_targetObjects != null)
			{
				for (int i = 0; i < m_targetObjects.Length; i++)
				{
					GameObject gameObject = m_targetObjects[i];
					if (gameObject != null)
					{
						text = text + ((i > 0) ? ", " : "") + "'" + gameObject.name + "'";
					}
				}
			}
			if (text == string.Empty)
			{
				text = "*No Target*";
			}
			return text;
		}
	}

	public AnimationInstance(GameObject[] a_animInstanceTargets)
	{
		m_targetObjects = a_animInstanceTargets;
		CheckTargetIndexesArray();
		m_animationSteps = new List<BaseAnimationStep>();
	}

	public AnimationInstance(UIAnimator a_uiAnimatorSubModule)
	{
		m_uiAnimatorSubModule = a_uiAnimatorSubModule;
	}

	public float GetDuration(AnimSetupType a_animType)
	{
		float num = m_startDelay;
		if (IsUiAnimatorSubModule)
		{
			num += m_uiAnimatorSubModule.GetAnimationDuration(a_animType);
		}
		else
		{
			for (int i = 0; i < m_animationSteps.Count; i++)
			{
				if (m_animationSteps[i] != null)
				{
					num += m_animationSteps[i].TotalExecutionDuration;
				}
			}
		}
		return num;
	}

	public void AddNewAnimationStep(BaseAnimationStep a_newAnimStep)
	{
		m_animationSteps.Add(a_newAnimStep);
	}

	public void AddNewAnimationStep(BaseAnimationStep a_newAnimStep, int a_index)
	{
		m_animationSteps.Insert(a_index, a_newAnimStep);
	}

	public void ResetToStart(AnimSetupType a_animType)
	{
		m_timer = 0.0;
		if (IsUiAnimatorSubModule)
		{
			m_uiAnimatorSubModule.ResetToStart(a_animType);
			return;
		}
		if (m_animationSteps != null)
		{
			for (int num = m_animationSteps.Count - 1; num >= 0; num--)
			{
				m_animationSteps[num].ResetToStart(a_animType);
			}
		}
		CheckTargetIndexesArray();
		for (int i = 0; i < m_targetStepIndexes.Length; i++)
		{
			m_targetStepIndexes[i] = 0;
		}
	}

	public void ResetToEnd(AnimSetupType a_animType)
	{
		m_timer = GetDuration(a_animType);
		if (IsUiAnimatorSubModule)
		{
			m_uiAnimatorSubModule.ResetToEnd(a_animType);
			return;
		}
		if (m_animationSteps != null)
		{
			for (int i = 0; i < m_animationSteps.Count; i++)
			{
				m_animationSteps[i].ResetToEnd(a_animType);
			}
		}
		CheckTargetIndexesArray();
		for (int j = 0; j < m_targetStepIndexes.Length; j++)
		{
			m_targetStepIndexes[j] = m_animationSteps.Count;
		}
	}

	public void SetAsMasterState()
	{
		if (!IsUiAnimatorSubModule && m_animationSteps != null)
		{
			for (int i = 0; i < m_animationSteps.Count; i++)
			{
				m_animationSteps[i].SetAsMasterState(m_targetObjects);
			}
		}
	}

	public bool UpdateState(UIAnimator a_uiAnimatorRef, AnimSetupType a_animType, float a_deltaTime)
	{
		m_timer += a_deltaTime;
		if (m_timer > (double)m_startDelay)
		{
			if (IsUiAnimatorSubModule)
			{
				return m_uiAnimatorSubModule.UpdateState(a_animType, a_deltaTime, a_isPrimaryAnimator: false);
			}
			m_finishedFlag = true;
			for (int i = 0; i < m_targetObjects.Length; i++)
			{
				if (m_targetStepIndexes[i] >= m_animationSteps.Count)
				{
					continue;
				}
				if (m_targetStepIndexes[i] > 0 && m_animationSteps[m_targetStepIndexes[i]].WaitForPreviousComplete && !m_animationSteps[m_targetStepIndexes[i]].HasStarted(i) && !m_animationSteps[m_targetStepIndexes[i] - 1].IsCompletelyFinished())
				{
					m_finishedFlag = false;
					continue;
				}
				m_animationSteps[m_targetStepIndexes[i]].UpdateState(a_uiAnimatorRef, i, a_animType, a_deltaTime);
				if (m_animationSteps[m_targetStepIndexes[i]].IsFinished(i))
				{
					m_targetStepIndexes[i]++;
				}
				m_finishedFlag = false;
			}
			return m_finishedFlag;
		}
		return false;
	}

	public void SetAnimationTimer(AnimSetupType a_animType, float a_timerValue, bool a_forceLinearTimings = false)
	{
		if (IsUiAnimatorSubModule)
		{
			m_uiAnimatorSubModule.SetAnimationTimer(a_animType, a_timerValue - m_startDelay);
			return;
		}
		for (int i = 0; i < m_targetObjects.Length; i++)
		{
			float num = m_startDelay;
			for (int j = 0; j < m_animationSteps.Count; j++)
			{
				if (j > 0)
				{
					num = ((!a_forceLinearTimings && !m_animationSteps[j].WaitForPreviousComplete) ? (num + m_animationSteps[j - 1].GetTotalExecutionDuration(i)) : (num + m_animationSteps[j - 1].TotalExecutionDuration));
				}
				if (a_timerValue - num >= 0f || !m_animationSteps[j].IsEffectStep)
				{
					m_animationSteps[j].SetAnimationTimer(i, a_animType, a_timerValue - num, a_forceLinearTimings);
				}
			}
		}
	}

	public void SetAllValuesFromCurrentState()
	{
		if (!IsUiAnimatorSubModule && m_animationSteps != null)
		{
			CheckTargetIndexesArray();
			for (int i = 0; i < m_animationSteps.Count; i++)
			{
				m_animationSteps[i].SetAllValuesFromCurrentState(m_targetObjects);
			}
		}
	}

	public void SetDefaultValuesFromCurrentState()
	{
		if (!IsUiAnimatorSubModule && m_animationSteps != null)
		{
			CheckTargetIndexesArray();
			for (int i = 0; i < m_animationSteps.Count; i++)
			{
				m_animationSteps[i].SetDefaultValuesFromCurrentState(m_targetObjects);
			}
		}
	}

	public void RefreshNumTargetsData()
	{
		if (!IsUiAnimatorSubModule && m_animationSteps != null)
		{
			CheckTargetIndexesArray();
			for (int i = 0; i < m_animationSteps.Count; i++)
			{
				m_animationSteps[i].SetNumTargets(m_targetObjects.Length);
			}
		}
	}

	private void CheckTargetIndexesArray()
	{
		if (m_targetStepIndexes == null || m_targetStepIndexes.Length != m_targetObjects.Length)
		{
			m_targetStepIndexes = new int[m_targetObjects.Length];
		}
	}
}
