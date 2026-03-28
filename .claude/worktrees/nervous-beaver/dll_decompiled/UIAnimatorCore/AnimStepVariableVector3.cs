using System;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public class AnimStepVariableVector3 : BaseAnimStepVariable
{
	[SerializeField]
	private Vector3 m_from;

	[SerializeField]
	private Vector3 m_to;

	[SerializeField]
	private Vector3[] m_masterValues;

	public AnimStepVariableVector3()
	{
		m_offsettingEnabled = false;
		m_type = VariableType.Single;
	}

	public AnimStepVariableVector3(VariableType a_startType)
	{
		m_offsettingEnabled = false;
		m_type = a_startType;
	}

	public AnimStepVariableVector3(bool a_offsettingEnabled)
	{
		m_offsettingEnabled = a_offsettingEnabled;
		m_type = ((!m_offsettingEnabled) ? VariableType.Single : VariableType.Offset);
	}

	public AnimStepVariableVector3(VariableType a_startType, bool a_offsettingEnabled)
	{
		m_type = a_startType;
		m_offsettingEnabled = a_offsettingEnabled;
	}

	public void Initialise(Vector3[] a_masterValues)
	{
		m_from = a_masterValues[0];
		m_to = a_masterValues[0];
		m_masterValues = a_masterValues;
	}

	public void UpdateMasterValues(Vector3[] a_masterValues)
	{
		m_masterValues = a_masterValues;
	}

	public Vector3 GetValue(int a_targetIndex)
	{
		if (m_type == VariableType.Single)
		{
			return m_from;
		}
		if (m_type == VariableType.Offset)
		{
			return m_masterValues[a_targetIndex] + (m_from - m_masterValues[0]);
		}
		if (m_type == VariableType.Range && m_masterValues.Length > 1)
		{
			return m_masterValues[a_targetIndex] + Vector3.LerpUnclamped(m_from - m_masterValues[0], m_to - m_masterValues[0], (float)a_targetIndex / (float)(m_masterValues.Length - 1));
		}
		return m_from;
	}

	public Vector3 GetValueLerpedToMaster(int a_targetIndex, float a_progress)
	{
		return Vector3.LerpUnclamped(GetValue(a_targetIndex), m_masterValues[a_targetIndex], a_progress);
	}
}
