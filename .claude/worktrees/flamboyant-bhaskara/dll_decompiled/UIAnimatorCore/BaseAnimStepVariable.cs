using System;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public abstract class BaseAnimStepVariable
{
	public enum VariableType
	{
		Offset,
		Single,
		Range
	}

	[SerializeField]
	protected VariableType m_type;

	[SerializeField]
	protected bool m_offsettingEnabled;
}
