using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects;

public class ME_RenderTextureUtility
{
	private List<RenderTexture> m_TemporaryRTs = new List<RenderTexture>();

	public RenderTexture GetTemporaryRenderTexture(int width, int height, int depthBuffer = 0, RenderTextureFormat format = RenderTextureFormat.ARGBHalf, FilterMode filterMode = FilterMode.Bilinear)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer, format);
		temporary.filterMode = filterMode;
		temporary.wrapMode = TextureWrapMode.Clamp;
		temporary.name = "RenderTextureUtilityTempTexture";
		m_TemporaryRTs.Add(temporary);
		return temporary;
	}

	public void ReleaseTemporaryRenderTexture(RenderTexture rt)
	{
		if (!(rt == null) && m_TemporaryRTs.Contains(rt))
		{
			m_TemporaryRTs.Remove(rt);
			RenderTexture.ReleaseTemporary(rt);
		}
	}

	public void ReleaseAllTemporaryRenderTextures()
	{
		for (int i = 0; i < m_TemporaryRTs.Count; i++)
		{
			RenderTexture.ReleaseTemporary(m_TemporaryRTs[i]);
		}
		m_TemporaryRTs.Clear();
	}
}
