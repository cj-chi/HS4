using UnityEngine;

namespace LuxWater;

[RequireComponent(typeof(Camera))]
public class LuxWater_UnderwaterRenderingSlave : MonoBehaviour
{
	private LuxWater_UnderWaterRendering waterrendermanager;

	private bool readyToGo;

	public Camera cam;

	private void OnEnable()
	{
		cam = GetComponent<Camera>();
		Invoke("GetWaterrendermanager", 0f);
	}

	private void GetWaterrendermanager()
	{
		LuxWater_UnderWaterRendering instance = LuxWater_UnderWaterRendering.instance;
		if (instance != null)
		{
			waterrendermanager = instance;
			readyToGo = true;
		}
	}

	private void OnPreCull()
	{
		if (readyToGo)
		{
			waterrendermanager.RenderWaterMask(cam, SecondaryCameraRendering: true);
		}
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (readyToGo)
		{
			waterrendermanager.RenderUnderWater(src, dest, cam, SecondaryCameraRendering: true);
		}
		else
		{
			Graphics.Blit(src, dest);
		}
	}
}
