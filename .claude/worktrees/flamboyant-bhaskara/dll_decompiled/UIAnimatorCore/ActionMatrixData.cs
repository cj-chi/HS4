using System;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public class ActionMatrixData
{
	[SerializeField]
	private bool m_enabled = true;

	[SerializeField]
	private AnimSetupType m_animType;

	[SerializeField]
	private float m_delay;

	public bool IsEnabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}

	public AnimSetupType AnimType
	{
		get
		{
			return m_animType;
		}
		set
		{
			m_animType = value;
		}
	}

	public float Delay
	{
		get
		{
			return m_delay;
		}
		set
		{
			m_delay = value;
		}
	}

	public ActionMatrixData()
	{
	}

	public ActionMatrixData(bool a_enabledByDefault)
	{
		m_enabled = a_enabledByDefault;
	}
}
