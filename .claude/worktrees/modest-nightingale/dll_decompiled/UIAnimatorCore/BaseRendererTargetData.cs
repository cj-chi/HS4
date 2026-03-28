using System;
using UnityEngine;

namespace UIAnimatorCore;

[Serializable]
public abstract class BaseRendererTargetData
{
	[SerializeField]
	private float[] m_masterAlphas;

	[SerializeField]
	private bool[] m_activeStates;

	protected Color m_cachedColor;

	public abstract int NumRenderers { get; }

	public abstract float GetRendererAlpha(int a_rendererIndex);

	public abstract void SetRendererAlpha(int a_rendererIndex, float a_alphaValue);

	public void SetupMasterData()
	{
		m_masterAlphas = new float[NumRenderers];
		m_activeStates = new bool[NumRenderers];
		for (int i = 0; i < NumRenderers; i++)
		{
			m_masterAlphas[i] = GetRendererAlpha(i);
			m_activeStates[i] = true;
		}
	}

	public void SetToMasterValues()
	{
		for (int i = 0; i < NumRenderers; i++)
		{
			SetRendererAlpha(i, m_masterAlphas[i]);
		}
	}

	public void SetFadeProgress(float a_animatedAlpha, float a_progress)
	{
		for (int i = 0; i < NumRenderers; i++)
		{
			if (m_activeStates[i])
			{
				SetRendererAlpha(i, Mathf.LerpUnclamped(a_animatedAlpha, m_masterAlphas[i], a_progress));
			}
		}
	}
}
