using UnityEngine;

namespace LuxWater;

[RequireComponent(typeof(Camera))]
public class LuxWater_UnderWaterBlur : MonoBehaviour
{
	[Space(6f)]
	[LuxWater_HelpBtn("h.3a2840a53u5j")]
	public float blurSpread = 0.6f;

	public int blurDownSample = 4;

	public int blurIterations = 4;

	private Vector2[] m_offsets = new Vector2[4];

	private Material blurMaterial;

	private Material blitMaterial;

	private LuxWater_UnderWaterRendering waterrendermanager;

	private bool doBlur;

	private bool initBlur = true;

	private void OnEnable()
	{
		blurMaterial = new Material(Shader.Find("Hidden/Lux Water/BlurEffectConeTap"));
		blitMaterial = new Material(Shader.Find("Hidden/Lux Water/UnderWaterPost"));
		Invoke("GetWaterrendermanagerInstance", 0f);
	}

	private void OnDisable()
	{
		if ((bool)blurMaterial)
		{
			Object.DestroyImmediate(blurMaterial);
		}
		if ((bool)blitMaterial)
		{
			Object.DestroyImmediate(blitMaterial);
		}
	}

	private void GetWaterrendermanagerInstance()
	{
		waterrendermanager = LuxWater_UnderWaterRendering.instance;
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (waterrendermanager == null)
		{
			Graphics.Blit(src, dest);
			return;
		}
		doBlur = waterrendermanager.activeWaterVolume > -1;
		if (doBlur || initBlur)
		{
			initBlur = false;
			int width = src.width / blurDownSample;
			int height = src.height / blurDownSample;
			RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.DefaultHDR);
			DownSample(src, renderTexture);
			for (int i = 0; i < blurIterations; i++)
			{
				RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.DefaultHDR);
				FourTapCone(renderTexture, temporary, i);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary;
			}
			Shader.SetGlobalTexture("_UnderWaterTex", renderTexture);
			Graphics.Blit(src, dest, blitMaterial, 1);
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		else
		{
			Graphics.Blit(src, dest);
		}
	}

	private void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float num = 0.5f + (float)iteration * blurSpread;
		m_offsets[0].x = 0f - num;
		m_offsets[0].y = 0f - num;
		m_offsets[1].x = 0f - num;
		m_offsets[1].y = num;
		m_offsets[2].x = num;
		m_offsets[2].y = num;
		m_offsets[3].x = num;
		m_offsets[3].y = 0f - num;
		if (iteration == 0)
		{
			Graphics.BlitMultiTap(source, dest, blurMaterial, m_offsets);
		}
		else
		{
			Graphics.BlitMultiTap(source, dest, blurMaterial, m_offsets);
		}
	}

	private void DownSample(RenderTexture source, RenderTexture dest)
	{
		float num = 1f;
		m_offsets[0].x = 0f - num;
		m_offsets[0].y = 0f - num;
		m_offsets[1].x = 0f - num;
		m_offsets[1].y = num;
		m_offsets[2].x = num;
		m_offsets[2].y = num;
		m_offsets[3].x = num;
		m_offsets[3].y = 0f - num;
		Graphics.BlitMultiTap(source, dest, blurMaterial, m_offsets);
	}
}
