using System;
using System.Collections;
using System.IO;
using System.Text;
using Illusion.CustomAttributes;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

[DefaultExecutionOrder(13000)]
public class GameScreenShot : MonoBehaviour
{
	[Button("Capture", "キャプチャー", new object[] { "" })]
	public int excuteCapture;

	[Button("UnityCapture", "Unityキャプチャー", new object[] { "" })]
	public int excuteCaptureEx;

	[SerializeField]
	private bool _modeARGB;

	[SerializeField]
	private int _capRate = 1;

	[SerializeField]
	private Camera[] renderCam;

	[SerializeField]
	private Image imageCap;

	[SerializeField]
	private bool _capMark = true;

	public Action captureBeforeFunc;

	public Action captureAfterFunc;

	private string savePath = "";

	private IDisposable captureDisposable;

	public bool modeARGB
	{
		get
		{
			return _modeARGB;
		}
		set
		{
			_modeARGB = value;
		}
	}

	public int capRate
	{
		get
		{
			return _capRate;
		}
		set
		{
			_capRate = value;
		}
	}

	public bool capMark
	{
		get
		{
			return _capMark;
		}
		set
		{
			_capMark = value;
		}
	}

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
		if (captureDisposable == null && !Scene.IsNowLoadingFade)
		{
			savePath = path;
			if (savePath == string.Empty)
			{
				savePath = CreateCaptureFileName();
			}
			captureDisposable = Observable.FromCoroutine(CaptureFunc).Subscribe((Action<Unit>)delegate
			{
				Utils.Sound.Play(SystemSE.photo);
			}, (Action)delegate
			{
				captureDisposable = null;
			});
		}
	}

	public void UnityCapture(string path = "")
	{
		savePath = path;
		if ("" == savePath)
		{
			savePath = CreateCaptureFileName();
		}
	}

	public byte[] CreatePngScreen(int _width, int _height, bool _ARGB = false, bool _cap = false)
	{
		Texture2D texture2D = new Texture2D(_width, _height, _ARGB ? TextureFormat.ARGB32 : TextureFormat.RGB24, mipChain: false);
		int antiAliasing = ((QualitySettings.antiAliasing == 0) ? 1 : QualitySettings.antiAliasing);
		RenderTexture temporary = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, antiAliasing);
		if (_cap)
		{
			imageCap.enabled = true;
		}
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
				int cullingMask = camera.cullingMask;
				camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Studio/Camera"));
				bool flag = camera.enabled;
				RenderTexture targetTexture = camera.targetTexture;
				Rect rect = camera.rect;
				camera.enabled = true;
				camera.targetTexture = temporary;
				camera.Render();
				camera.targetTexture = targetTexture;
				camera.rect = rect;
				camera.enabled = flag;
				camera.cullingMask = cullingMask;
			}
		}
		if (_cap)
		{
			imageCap.enabled = false;
		}
		GL.sRGBWrite = sRGBWrite;
		RenderTexture.active = temporary;
		texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		byte[] result = texture2D.EncodeToPNG();
		RenderTexture.ReleaseTemporary(temporary);
		UnityEngine.Object.Destroy(texture2D);
		texture2D = null;
		UnityEngine.Resources.UnloadUnusedAssets();
		return result;
	}

	private IEnumerator CaptureFunc()
	{
		if (captureBeforeFunc != null)
		{
			captureBeforeFunc();
		}
		yield return new WaitForEndOfFrame();
		float num = ((_capRate == 0) ? 1 : _capRate);
		byte[] bytes = CreatePngScreen((int)((float)Screen.width * num), (int)((float)Screen.height * num), _modeARGB, _capMark);
		File.WriteAllBytes(savePath, bytes);
		if (captureAfterFunc != null)
		{
			captureAfterFunc();
		}
		yield return null;
	}
}
