using UnityEngine;
using UnityEngine.UI;

public class bl_ApplySettingsPro : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler HUDCanvas;

	[SerializeField]
	private GameObject[] FPSObject;

	private int[] ShadowCascadeOptions = new int[3] { 0, 2, 4 };

	private bl_BrightnessImage BrightnessImage;

	private void Start()
	{
		if (Object.FindObjectOfType<bl_BrightnessImage>() != null)
		{
			BrightnessImage = Object.FindObjectOfType<bl_BrightnessImage>();
		}
		LoadAndApply();
	}

	public void ShadowProjectionType(bool b)
	{
		if (b)
		{
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
		}
		else
		{
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;
		}
	}

	private void LoadAndApply()
	{
		int num = PlayerPrefs.GetInt("GameName.AntiAliasing");
		int num2 = PlayerPrefs.GetInt("GameName.AnisoTropic");
		int num3 = PlayerPrefs.GetInt("GameName.BlendWeight");
		int qualityLevel = PlayerPrefs.GetInt("GameName.QualityLevel");
		int num4 = PlayerPrefs.GetInt("GameName.ResolutionScreen");
		int num5 = PlayerPrefs.GetInt("GameName.VSyncCount");
		int masterTextureLimit = PlayerPrefs.GetInt("GameName.TextureLimit", 0);
		int num6 = PlayerPrefs.GetInt("GameName.ShadowCascade", 0);
		bool active = PlayerPrefs.GetInt("GameName.ShowFPS", 0) == 1;
		float volume = PlayerPrefs.GetFloat("GameName.Volumen", 1f);
		float num7 = PlayerPrefs.GetFloat("GameName.ShadowDistance");
		bool b = PlayerPrefs.GetInt("GameName.ShadowProjection", 0) == 1;
		bool num8 = AllOptionsKeyPro.IntToBool(PlayerPrefs.GetInt("GameName.ShadowEnable"));
		float value = PlayerPrefs.GetFloat("GameName.Brightness", 1f);
		bool realtimeReflectionProbes = AllOptionsKeyPro.IntToBool(PlayerPrefs.GetInt("GameName.RealtimeReflection", 1));
		float lodBias = PlayerPrefs.GetFloat("GameName.LoadBias", 1f);
		float num9 = PlayerPrefs.GetFloat("GameName.HudScale", 0f);
		QualitySettings.shadowDistance = num7;
		AudioListener.volume = volume;
		AudioListener.pause = PlayerPrefs.GetInt("GameName.PauseAudio", 0) == 1;
		ShadowProjectionType(b);
		QualitySettings.masterTextureLimit = masterTextureLimit;
		QualitySettings.shadowCascades = ShadowCascadeOptions[num6];
		QualitySettings.SetQualityLevel(qualityLevel);
		QualitySettings.realtimeReflectionProbes = realtimeReflectionProbes;
		QualitySettings.shadowDistance = (num8 ? num7 : 0f);
		QualitySettings.lodBias = lodBias;
		if (BrightnessImage != null)
		{
			BrightnessImage.SetValue(value);
		}
		if (HUDCanvas != null)
		{
			HUDCanvas.matchWidthOrHeight = 1f - num9;
		}
		if (FPSObject != null)
		{
			GameObject[] fPSObject = FPSObject;
			for (int i = 0; i < fPSObject.Length; i++)
			{
				fPSObject[i].SetActive(active);
			}
		}
		switch (num2)
		{
		case 0:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			break;
		case 1:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			break;
		case 2:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			break;
		}
		switch (num)
		{
		case 0:
			QualitySettings.antiAliasing = 0;
			break;
		case 1:
			QualitySettings.antiAliasing = 2;
			break;
		case 2:
			QualitySettings.antiAliasing = 4;
			break;
		case 3:
			QualitySettings.antiAliasing = 8;
			break;
		}
		switch (num5)
		{
		case 0:
			QualitySettings.vSyncCount = 0;
			break;
		case 1:
			QualitySettings.vSyncCount = 1;
			break;
		case 2:
			QualitySettings.vSyncCount = 2;
			break;
		}
		switch (num3)
		{
		case 0:
			QualitySettings.blendWeights = BlendWeights.OneBone;
			break;
		case 1:
			QualitySettings.blendWeights = BlendWeights.TwoBones;
			break;
		case 2:
			QualitySettings.blendWeights = BlendWeights.FourBones;
			break;
		}
		Screen.SetResolution(Screen.resolutions[num4].width, Screen.resolutions[num4].height, fullscreen: false);
	}
}
