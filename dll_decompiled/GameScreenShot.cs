using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Illusion.CustomAttributes;
using Manager;
using UniRx;
using UnityEngine;

public class GameScreenShot : MonoBehaviour
{
	[Button("Capture", "キャプチャー", new object[] { "" })]
	public int excuteCapture;

	[Button("UnityCapture", "Unityキャプチャー", new object[] { "" })]
	public int excuteCaptureEx;

	public bool capExMode;

	public bool modeARGB;

	public Camera[] renderCam;

	public GameScreenShotAssist[] scriptGssAssist;

	public Texture texCapMark;

	public Vector2 texPosition = new Vector2(1152f, 688f);

	public int capRate = 1;

	private string savePath = "";

	private IDisposable captureDisposable;

	public string CreateCaptureFileName()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append(UserData.Create("cap"));
		DateTime now = DateTime.Now;
		stringBuilder.Append(now.Year.ToString("0000"));
		stringBuilder.Append(now.Month.ToString("00"));
		stringBuilder.Append(now.Day.ToString("00"));
		stringBuilder.Append(now.Hour.ToString("00"));
		stringBuilder.Append(now.Minute.ToString("00"));
		stringBuilder.Append(now.Second.ToString("00"));
		stringBuilder.Append(now.Millisecond.ToString("000"));
		stringBuilder.Append(".png");
		return stringBuilder.ToString();
	}

	public void Capture(string path = "")
	{
		if (captureDisposable != null || Scene.IsNowLoadingFade)
		{
			return;
		}
		bool isRenderSetCam = false;
		if (!capExMode)
		{
			if (((IReadOnlyCollection<Camera>)(object)renderCam).IsNullOrEmpty())
			{
				if (Camera.main == null)
				{
					return;
				}
				isRenderSetCam = true;
				renderCam = new Camera[1] { Camera.main };
			}
		}
		else if (scriptGssAssist.Length == 0)
		{
			return;
		}
		savePath = path;
		if (savePath == string.Empty)
		{
			savePath = CreateCaptureFileName();
		}
		captureDisposable = Observable.FromCoroutine(CaptureFunc).Subscribe(delegate
		{
			if (isRenderSetCam)
			{
				renderCam = null;
			}
			captureDisposable = null;
		});
	}

	public void UnityCapture(string path = "")
	{
		savePath = path;
		if ("" == savePath)
		{
			savePath = CreateCaptureFileName();
		}
		ScreenCapture.CaptureScreenshot(savePath, capRate);
	}

	private IEnumerator CaptureFunc()
	{
		GameScreenShotOnGUI shotGUI = null;
		if (texCapMark != null)
		{
			shotGUI = this.GetOrAddComponent<GameScreenShotOnGUI>();
		}
		yield return new WaitForEndOfFrame();
		if (shotGUI != null)
		{
			UnityEngine.Object.Destroy(shotGUI);
		}
		float num = ((capRate == 0) ? 1 : capRate);
		Texture2D texture2D = new Texture2D((int)((float)Screen.width * num), (int)((float)Screen.height * num), modeARGB ? TextureFormat.ARGB32 : TextureFormat.RGB24, mipChain: false);
		int antiAliasing = ((QualitySettings.antiAliasing == 0) ? 1 : QualitySettings.antiAliasing);
		RenderTexture temporary = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, antiAliasing);
		Action action = delegate
		{
			float num4 = (float)Screen.width / 1920f;
			Graphics.DrawTexture(new Rect(texPosition.x * num4, texPosition.y * num4, (float)texCapMark.width * num4, (float)texCapMark.height * num4), texCapMark);
		};
		if (!capExMode)
		{
			Graphics.SetRenderTarget(temporary);
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			Graphics.SetRenderTarget(null);
			bool sRGBWrite = GL.sRGBWrite;
			GL.sRGBWrite = true;
			Camera[] array = renderCam;
			foreach (Camera camera in array)
			{
				if (!(null == camera))
				{
					bool flag = camera.enabled;
					RenderTexture targetTexture = camera.targetTexture;
					Rect rect = camera.rect;
					camera.enabled = true;
					camera.targetTexture = temporary;
					camera.Render();
					camera.targetTexture = targetTexture;
					camera.rect = rect;
					camera.enabled = flag;
				}
			}
			GL.sRGBWrite = sRGBWrite;
			if ((bool)texCapMark)
			{
				Graphics.SetRenderTarget(temporary);
				action();
				Graphics.SetRenderTarget(null);
			}
		}
		else
		{
			bool sRGBWrite2 = GL.sRGBWrite;
			GL.sRGBWrite = true;
			Graphics.Blit(scriptGssAssist[0].rtCamera, temporary);
			GL.sRGBWrite = sRGBWrite2;
			for (int num3 = 1; num3 < scriptGssAssist.Length; num3++)
			{
				Graphics.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), scriptGssAssist[num3].rtCamera);
			}
			if ((bool)texCapMark)
			{
				action();
			}
		}
		RenderTexture.active = temporary;
		texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		byte[] bytes = texture2D.EncodeToPNG();
		RenderTexture.ReleaseTemporary(temporary);
		UnityEngine.Object.Destroy(texture2D);
		File.WriteAllBytes(savePath, bytes);
		yield return null;
	}
}
