using UnityEngine;

namespace CharaCustom;

public class CustomCapture : MonoBehaviour
{
	[SerializeField]
	private Camera camBG;

	public Camera camMain;

	public byte[] CapCharaCard(bool enableBG, SaveFrameAssist saveFrameAssist, bool forceHideBackFrame = false)
	{
		byte[] pngData = null;
		bool flag = !(null == saveFrameAssist) && saveFrameAssist.backFrameDraw;
		if (forceHideBackFrame)
		{
			flag = false;
		}
		bool flag2 = !(null == saveFrameAssist) && saveFrameAssist.frontFrameDraw;
		Camera camBackFrame = ((null == saveFrameAssist) ? null : (flag ? saveFrameAssist.backFrameCam : null));
		Camera camFrontFrame = ((null == saveFrameAssist) ? null : (flag2 ? saveFrameAssist.frontFrameCam : null));
		CreatePng(ref pngData, enableBG ? camBG : null, camBackFrame, camMain, camFrontFrame);
		camBackFrame = ((null != saveFrameAssist) ? saveFrameAssist.backFrameCam : null);
		if (null != camBackFrame)
		{
			camBackFrame.targetTexture = null;
		}
		return pngData;
	}

	public byte[] CapCoordinateCard(bool enableBG, SaveFrameAssist saveFrameAssist, Camera main)
	{
		byte[] pngData = null;
		bool flag = !(null == saveFrameAssist) && saveFrameAssist.backFrameDraw;
		bool flag2 = !(null == saveFrameAssist) && saveFrameAssist.frontFrameDraw;
		Camera camBackFrame = ((null == saveFrameAssist) ? null : (flag ? saveFrameAssist.backFrameCam : null));
		Camera camFrontFrame = ((null == saveFrameAssist) ? null : (flag2 ? saveFrameAssist.frontFrameCam : null));
		CreatePng(ref pngData, enableBG ? camBG : null, camBackFrame, main, camFrontFrame);
		camBackFrame = ((null != saveFrameAssist) ? saveFrameAssist.backFrameCam : null);
		if (null != camBackFrame)
		{
			camBackFrame.targetTexture = null;
		}
		if (null != camMain)
		{
			camMain.targetTexture = null;
		}
		if (null != camBG)
		{
			camBG.targetTexture = null;
		}
		return pngData;
	}

	public static void CreatePng(ref byte[] pngData, Camera _camBG = null, Camera _camBackFrame = null, Camera _camMain = null, Camera _camFrontFrame = null)
	{
		RenderTexture renderTexture = null;
		int num = 1280;
		int num2 = 720;
		int num3 = 504;
		int num4 = 704;
		RenderTexture renderTexture2 = null;
		renderTexture2 = ((QualitySettings.antiAliasing != 0) ? RenderTexture.GetTemporary(num, num2, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing) : RenderTexture.GetTemporary(num, num2, 24));
		bool sRGBWrite = GL.sRGBWrite;
		GL.sRGBWrite = true;
		if (null != _camMain)
		{
			renderTexture = _camMain.targetTexture;
			bool allowHDR = _camMain.allowHDR;
			_camMain.allowHDR = false;
			_camMain.targetTexture = renderTexture2;
			_camMain.Render();
			_camMain.targetTexture = renderTexture;
			_camMain.allowHDR = allowHDR;
		}
		if (null != _camBG)
		{
			bool allowHDR2 = _camBG.allowHDR;
			_camBG.allowHDR = false;
			_camBG.targetTexture = renderTexture2;
			_camBG.Render();
			_camBG.targetTexture = null;
			_camBG.allowHDR = allowHDR2;
		}
		if (null != _camBackFrame)
		{
			_camBackFrame.targetTexture = renderTexture2;
			_camBackFrame.Render();
			_camBackFrame.targetTexture = null;
		}
		if (null != _camFrontFrame)
		{
			_camFrontFrame.targetTexture = renderTexture2;
			_camFrontFrame.Render();
			_camFrontFrame.targetTexture = null;
		}
		GL.sRGBWrite = sRGBWrite;
		Texture2D texture2D = new Texture2D(num3, num4, TextureFormat.RGB24, mipChain: false, linear: true);
		RenderTexture.active = renderTexture2;
		texture2D.ReadPixels(new Rect((float)(num - num3) / 2f, (float)(num2 - num4) / 2f, num3, num4), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(renderTexture2);
		TextureScale.Bilinear(texture2D, num3 / 2, num4 / 2);
		pngData = texture2D.EncodeToPNG();
		Object.Destroy(texture2D);
		texture2D = null;
	}
}
