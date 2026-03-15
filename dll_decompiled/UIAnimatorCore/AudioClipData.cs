using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public class AudioClipData
{
	public enum CLIP_TRIGGER_POINT
	{
		START_OF_ANIM_STEP,
		END_OF_ANIM_STEP
	}

	[SerializeField]
	private AudioClip m_clip;

	[SerializeField]
	private CLIP_TRIGGER_POINT m_triggerPoint;

	[SerializeField]
	private AnimStepVariableFloat m_delay;

	[SerializeField]
	private AnimStepVariableFloat m_offsetTime;

	[SerializeField]
	private AnimStepVariableFloat m_volume;

	[SerializeField]
	private AnimStepVariableFloat m_pitch;

	private List<bool> m_clipActivatedStates;

	public AudioClip Clip => m_clip;

	public CLIP_TRIGGER_POINT TriggerPoint => m_triggerPoint;

	public AnimStepVariableFloat Delay => m_delay;

	public AnimStepVariableFloat OffsetTime => m_offsetTime;

	public AnimStepVariableFloat Volume => m_volume;

	public AnimStepVariableFloat Pitch => m_pitch;

	public void Init(int a_numTargets)
	{
		m_delay = new AnimStepVariableFloat(0f, a_numTargets);
		m_offsetTime = new AnimStepVariableFloat(0f, a_numTargets);
		m_volume = new AnimStepVariableFloat(1f, a_numTargets);
		m_pitch = new AnimStepVariableFloat(1f, a_numTargets);
	}

	public void UpdateNumTargets(int a_numTargets)
	{
		m_delay.Initialise(a_numTargets);
		m_offsetTime.Initialise(a_numTargets);
		m_volume.Initialise(a_numTargets);
		m_pitch.Initialise(a_numTargets);
	}

	public bool HasAudioClipActivated(int a_targetIndex)
	{
		if (m_clipActivatedStates == null || a_targetIndex >= m_clipActivatedStates.Count)
		{
			return false;
		}
		return m_clipActivatedStates[a_targetIndex];
	}

	public void MarkAudioClipActivated(int a_targetIndex)
	{
		if (m_clipActivatedStates == null)
		{
			m_clipActivatedStates = new List<bool>();
		}
		if (a_targetIndex >= m_clipActivatedStates.Count)
		{
			for (int i = m_clipActivatedStates.Count; i < a_targetIndex + 1; i++)
			{
				m_clipActivatedStates.Add(item: false);
			}
		}
		m_clipActivatedStates[a_targetIndex] = true;
	}

	public void ResetAll()
	{
		if (m_clipActivatedStates != null)
		{
			for (int i = 0; i < m_clipActivatedStates.Count; i++)
			{
				m_clipActivatedStates[i] = false;
			}
		}
	}
}
