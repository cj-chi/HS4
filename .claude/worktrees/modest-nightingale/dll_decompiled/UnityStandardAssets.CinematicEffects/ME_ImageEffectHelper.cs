using UnityEngine;

namespace UnityStandardAssets.CinematicEffects;

public static class ME_ImageEffectHelper
{
	public static bool supportsDX11
	{
		get
		{
			if (SystemInfo.graphicsShaderLevel >= 50)
			{
				return SystemInfo.supportsComputeShaders;
			}
			return false;
		}
	}

	public static bool IsSupported(Shader s, bool needDepth, bool needHdr, MonoBehaviour effect)
	{
		if (s == null || !s.isSupported)
		{
			return false;
		}
		if (!SystemInfo.supportsImageEffects)
		{
			return false;
		}
		if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			return false;
		}
		if (needHdr && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			return false;
		}
		return true;
	}

	public static Material CheckShaderAndCreateMaterial(Shader s)
	{
		if (s == null || !s.isSupported)
		{
			return null;
		}
		return new Material(s)
		{
			hideFlags = HideFlags.DontSave
		};
	}
}
