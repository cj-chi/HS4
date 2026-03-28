using System;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public class AnimStepVariableFloat : BaseAnimStepVariable
{
	[SerializeField]
	private float m_from;

	[SerializeField]
	private float m_to;

	[SerializeField]
	private float[] m_masterValues;

	[SerializeField]
	private int m_numTargets;

	public float MinValue
	{
		get
		{
			if (m_type != VariableType.Range)
			{
				return m_from;
			}
			if (!(m_from > m_to))
			{
				return m_from;
			}
			return m_to;
		}
	}

	public float LargestValue
	{
		get
		{
			if (m_type != VariableType.Range)
			{
				return m_from;
			}
			if (!(m_from > m_to))
			{
				return m_to;
			}
			return m_from;
		}
	}

	public AnimStepVariableFloat()
	{
		m_offsettingEnabled = false;
		m_type = VariableType.Single;
	}

	public AnimStepVariableFloat(bool a_offsettingEnabled)
	{
		m_offsettingEnabled = a_offsettingEnabled;
		m_type = ((!m_offsettingEnabled) ? VariableType.Single : VariableType.Offset);
	}

	public AnimStepVariableFloat(VariableType a_startType, bool a_offsettingEnabled)
	{
		m_type = a_startType;
		m_offsettingEnabled = a_offsettingEnabled;
	}

	public AnimStepVariableFloat(float a_startValue, int a_numTargets = 1)
	{
		Initialise(new float[1] { a_startValue });
		m_offsettingEnabled = false;
		m_type = VariableType.Single;
		m_numTargets = a_numTargets;
	}

	public void Initialise(float[] a_masterValues)
	{
		m_from = a_masterValues[0];
		m_to = a_masterValues[0];
		m_masterValues = a_masterValues;
		m_numTargets = m_masterValues.Length;
	}

	public void Initialise(int a_numTargets)
	{
		m_numTargets = a_numTargets;
	}

	public float GetValue(int a_targetIndex)
	{
		if (m_type == VariableType.Single || (m_type == VariableType.Range && m_numTargets == 1))
		{
			return m_from;
		}
		if (m_type == VariableType.Offset)
		{
			return m_masterValues[a_targetIndex] + (m_from - m_masterValues[0]);
		}
		if (m_type == VariableType.Range)
		{
			if (m_masterValues.Length > 1)
			{
				return m_masterValues[a_targetIndex] + Mathf.LerpUnclamped(m_from - m_masterValues[0], m_to - m_masterValues[0], (float)a_targetIndex / (float)(m_masterValues.Length - 1));
			}
			return Mathf.LerpUnclamped(m_from, m_to, (float)a_targetIndex / (float)(m_numTargets - 1));
		}
		return m_from;
	}
}
