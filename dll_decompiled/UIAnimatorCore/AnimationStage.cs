using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public class AnimationStage
{
	[SerializeField]
	private List<AnimationInstance> m_animationInstances;

	[SerializeField]
	private float m_startDelay;

	[SerializeField]
	private float m_endDelay;

	private double m_timer;

	private bool m_finishedFlag;

	private bool m_finishedStage;

	private float m_cachedTotalDuration;

	public List<AnimationInstance> AnimationInstances => m_animationInstances;

	public float StartDelay => m_startDelay;

	public float EndDelay => m_endDelay;

	public float GetTotalDuration(AnimSetupType a_animType)
	{
		return m_startDelay + GetInstancesDuration(a_animType) + m_endDelay;
	}

	public float GetInstancesDuration(AnimSetupType a_animType)
	{
		float num = 0f;
		for (int i = 0; i < m_animationInstances.Count; i++)
		{
			float duration = m_animationInstances[i].GetDuration(a_animType);
			if (duration > num)
			{
				num = duration;
			}
		}
		return num;
	}

	public AnimationStage()
	{
		m_animationInstances = new List<AnimationInstance>();
	}

	public void AddNewAnimationInstance(GameObject[] a_animTargets)
	{
		m_animationInstances.Add(new AnimationInstance(a_animTargets));
	}

	public void AddNewAnimationInstance(UIAnimator a_uiAnimator)
	{
		m_animationInstances.Add(new AnimationInstance(a_uiAnimator));
	}

	public void ResetToStart(AnimSetupType a_animType)
	{
		m_timer = 0.0;
		m_finishedStage = false;
		m_cachedTotalDuration = GetTotalDuration(a_animType);
		for (int i = 0; i < m_animationInstances.Count; i++)
		{
			m_animationInstances[i].ResetToStart(a_animType);
		}
	}

	public void ResetToEnd(AnimSetupType a_animType)
	{
		m_cachedTotalDuration = GetTotalDuration(a_animType);
		m_timer = m_cachedTotalDuration;
		m_finishedStage = true;
		for (int i = 0; i < m_animationInstances.Count; i++)
		{
			m_animationInstances[i].ResetToEnd(a_animType);
		}
	}

	public bool UpdateState(UIAnimator a_uiAnimatorRef, AnimSetupType a_animType, float a_deltaTime)
	{
		m_timer += a_deltaTime;
		if (m_timer > (double)m_startDelay)
		{
			m_finishedFlag = true;
			if (!m_finishedStage && m_animationInstances != null)
			{
				for (int i = 0; i < m_animationInstances.Count; i++)
				{
					if (!m_animationInstances[i].UpdateState(a_uiAnimatorRef, a_animType, a_deltaTime))
					{
						m_finishedFlag = false;
					}
				}
			}
			if (m_finishedFlag)
			{
				m_finishedStage = true;
			}
			if (m_finishedStage && m_timer > (double)m_cachedTotalDuration)
			{
				return true;
			}
		}
		return false;
	}
}
