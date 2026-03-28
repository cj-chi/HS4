using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimatorCore;

[Serializable]
public class UIGraphicRendererTargetData : BaseRendererTargetData
{
	[SerializeField]
	private Graphic[] m_graphics;

	public override int NumRenderers
	{
		get
		{
			if (m_graphics == null)
			{
				return 0;
			}
			return m_graphics.Length;
		}
	}

	public override float GetRendererAlpha(int a_rendererIndex)
	{
		if (m_graphics == null || a_rendererIndex >= m_graphics.Length || m_graphics[a_rendererIndex] == null)
		{
			return 1f;
		}
		return m_graphics[a_rendererIndex].color.a;
	}

	public override void SetRendererAlpha(int a_rendererIndex, float a_alphaValue)
	{
		if (m_graphics != null && a_rendererIndex < m_graphics.Length && !(m_graphics[a_rendererIndex] == null))
		{
			m_cachedColor = m_graphics[a_rendererIndex].color;
			m_cachedColor.a = a_alphaValue;
			m_graphics[a_rendererIndex].color = m_cachedColor;
		}
	}

	public UIGraphicRendererTargetData(Graphic[] a_graphics)
	{
		m_graphics = new Graphic[a_graphics.Length];
		for (int i = 0; i < a_graphics.Length; i++)
		{
			m_graphics[i] = a_graphics[i];
		}
		SetupMasterData();
	}
}
