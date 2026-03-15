using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class bl_AllOptionsPro : MonoBehaviour
{
	[Header("Panels")]
	[SerializeField]
	private GameObject[] Panels;

	[SerializeField]
	private Button[] PanelButtons;

	[SerializeField]
	private Animator PanelAnimator;

	[Header("Settings")]
	public bool ApplyOnStart;

	public bool AutoApplyResolution = true;

	public bool SaveOnDisable = true;

	public bool AnimateHidePanel = true;

	public int StartWindow;

	[SerializeField]
	[Range(0f, 8f)]
	private int DefaultQuality = 3;

	[SerializeField]
	[Range(0f, 15f)]
	private int DefaultResolution = 7;

	[SerializeField]
	[Range(0f, 3f)]
	private int DefaultAntiAliasing = 1;

	[SerializeField]
	[Range(0f, 2f)]
	private int DefaultAnisoTropic = 1;

	[SerializeField]
	[Range(0f, 2f)]
	private int DefaultVSync = 1;

	[SerializeField]
	[Range(0f, 2f)]
	private int DefaultBlendWeight = 1;

	[SerializeField]
	[Range(0f, 100f)]
	private int DefaultShadowDistance = 40;

	[SerializeField]
	[Range(0f, 1f)]
	private int DefaultBrightness = 1;

	[SerializeField]
	[Range(0.01f, 3f)]
	private int DefaultLoadBias = 1;

	[Header("Options Name")]
	[SerializeField]
	private string[] AntiAliasingNames = new string[4] { "X0", "X2", "X4", "X8" };

	[SerializeField]
	private string[] VSyncNames = new string[3] { "Don't Sync", "Every V Blank", "Every Second V Blank" };

	[SerializeField]
	private string[] TextureQualityNames = new string[4] { "FULL RES", "HALF RES", "QUARTER RES", "EIGHTH RES" };

	[SerializeField]
	private string[] ShadowCascadeNames = new string[3] { "NO CASCADES", "TWO CASCADES", "FOUR CASCADES" };

	[Header("References")]
	[SerializeField]
	private GameObject SettingsPanel;

	[SerializeField]
	private Animator ContentAnim;

	public Text QualityText;

	private int CurrentQuality;

	public Text AnisotropicText;

	private int CurrentAS;

	public Text AntiAliasingText;

	private int CurrentAA;

	public Text vSyncText;

	private int CurrentVSC;

	public Text blendWeightsText;

	private int CurrentBW;

	public Text ResolutionText;

	private int CurrentRS;

	[SerializeField]
	private Text FullScreenOnText;

	private bool useFullScreen;

	[SerializeField]
	private Text TextureLimitText;

	private int CurrentTL;

	[SerializeField]
	private Text RealtimeReflectionText;

	private bool _realtimeReflection;

	[SerializeField]
	private Text LoadBiasText;

	private float _lodBias;

	[SerializeField]
	private Text ShadowCascadeText;

	private int CurrentSC = 2;

	private int[] ShadowCascadeOptions = new int[3] { 0, 2, 4 };

	[SerializeField]
	private Text ShowFPSText;

	private bool _showFPS;

	[SerializeField]
	private Text ShadowDistanceText;

	[SerializeField]
	private Slider ShadowDistanceSlider;

	private float cacheShadowDistance;

	[SerializeField]
	private Slider BrightnessSlider;

	[SerializeField]
	private Slider LoadBiasSlider;

	[SerializeField]
	private Slider HUDScaleFactor;

	[SerializeField]
	private Text HudScaleText;

	[SerializeField]
	private Text BrightnessText;

	private float _brightness;

	[SerializeField]
	private Text ShadowProjectionText;

	private bool shadowProjection;

	[SerializeField]
	private Text ShadowEnebleText;

	private bool _shadowEnable;

	[SerializeField]
	private Text PauseText;

	private bool _isPauseSound;

	[SerializeField]
	private Text VolumenText;

	[SerializeField]
	private Slider VolumenSlider;

	[SerializeField]
	private Text TitlePanelText;

	[SerializeField]
	private CanvasScaler HUDCanvas;

	[SerializeField]
	private GameObject[] FPSObject;

	private bl_BrightnessImage BrightnessImage;

	private float _hudScale;

	private bool Show;

	private float _volumen;

	private void Awake()
	{
		if (Object.FindObjectOfType<bl_BrightnessImage>() != null)
		{
			BrightnessImage = Object.FindObjectOfType<bl_BrightnessImage>();
		}
		if ((bool)HUDCanvas)
		{
			_hudScale = 1f - HUDCanvas.matchWidthOrHeight;
		}
	}

	private void Start()
	{
		if (ApplyOnStart)
		{
			LoadAndApply();
		}
		ChangeWindow(StartWindow, anim: false);
		ChangeSelectionButton(PanelButtons[StartWindow]);
		SettingsPanel.SetActive(value: false);
	}

	private void OnDisable()
	{
		if (SaveOnDisable)
		{
			SaveOptions();
		}
	}

	private void OnApplicationQuit()
	{
		if (SaveOnDisable)
		{
			SaveOptions();
		}
	}

	public void ChangeWindow(int _id)
	{
		PanelAnimator.Play("Change", 0, 0f);
		StartCoroutine(WaitForSwichet(_id));
	}

	public void ChangeWindow(int _id, bool anim)
	{
		if (anim)
		{
			PanelAnimator.Play("Change", 0, 0f);
		}
		StartCoroutine(WaitForSwichet(_id));
	}

	public void ChangeSelectionButton(Button b)
	{
		for (int i = 0; i < PanelButtons.Length; i++)
		{
			PanelButtons[i].interactable = true;
		}
		b.interactable = false;
	}

	public void ShowMenu()
	{
		Show = !Show;
		if (Show)
		{
			StopCoroutine("HideAnimate");
			SettingsPanel.SetActive(value: true);
			ContentAnim.SetBool("Show", value: true);
		}
		else if (AnimateHidePanel)
		{
			StartCoroutine("HideAnimate");
		}
		else
		{
			SettingsPanel.SetActive(value: false);
		}
	}

	public void GameQuality(bool mas)
	{
		if (mas)
		{
			CurrentQuality = (CurrentQuality + 1) % QualitySettings.names.Length;
		}
		else if (CurrentQuality != 0)
		{
			CurrentQuality = (CurrentQuality - 1) % QualitySettings.names.Length;
		}
		else
		{
			CurrentQuality = QualitySettings.names.Length - 1;
		}
		QualityText.text = QualitySettings.names[CurrentQuality].ToUpper();
		QualitySettings.SetQualityLevel(CurrentQuality);
	}

	public void AntiStropic(bool b)
	{
		if (b)
		{
			CurrentAS = (CurrentAS + 1) % 3;
		}
		else if (CurrentAS != 0)
		{
			CurrentAS = (CurrentAS - 1) % 3;
		}
		else
		{
			CurrentAS = 2;
		}
		switch (CurrentAS)
		{
		case 0:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			AnisotropicText.text = AnisotropicFiltering.Disable.ToString().ToUpper();
			break;
		case 1:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			AnisotropicText.text = AnisotropicFiltering.Enable.ToString().ToUpper();
			break;
		case 2:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			AnisotropicText.text = AnisotropicFiltering.ForceEnable.ToString().ToUpper();
			break;
		}
	}

	public void FullScreenMode(bool use)
	{
		useFullScreen = use;
		FullScreenOnText.text = (useFullScreen ? "ON" : "OFF");
	}

	public void AntiAliasing(bool b)
	{
		CurrentAA = (b ? ((CurrentAA + 1) % 4) : ((CurrentAA != 0) ? ((CurrentAA - 1) % 4) : (CurrentAA = 3)));
		AntiAliasingText.text = AntiAliasingNames[CurrentAA].ToUpper();
		switch (CurrentAA)
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
	}

	public void ShowFPS()
	{
		_showFPS = !_showFPS;
		ShowFPSText.text = (_showFPS ? "ON" : "OFF");
		if (FPSObject != null)
		{
			GameObject[] fPSObject = FPSObject;
			for (int i = 0; i < fPSObject.Length; i++)
			{
				fPSObject[i].SetActive(_showFPS);
			}
		}
	}

	public void PauseSound(bool b)
	{
		_isPauseSound = b;
		string text = (_isPauseSound ? "ON" : "OFF");
		PauseText.text = text;
	}

	public void VSyncCount(bool b)
	{
		CurrentVSC = (b ? ((CurrentVSC + 1) % 3) : ((CurrentVSC != 0) ? ((CurrentVSC - 1) % 3) : (CurrentVSC = 2)));
		vSyncText.text = VSyncNames[CurrentVSC].ToUpper();
		switch (CurrentVSC)
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
	}

	public void TextureQuality(bool b)
	{
		CurrentTL = (b ? ((CurrentTL + 1) % 3) : ((CurrentTL != 0) ? ((CurrentTL - 1) % 3) : (CurrentTL = 3)));
		QualitySettings.masterTextureLimit = CurrentTL;
		TextureLimitText.text = TextureQualityNames[CurrentTL];
	}

	public void ShadowCascades(bool b)
	{
		CurrentSC = (b ? ((CurrentSC + 1) % 3) : ((CurrentSC != 0) ? ((CurrentSC - 1) % 3) : (CurrentSC = 3)));
		QualitySettings.shadowCascades = ShadowCascadeOptions[CurrentSC];
		ShadowCascadeText.text = ShadowCascadeNames[CurrentSC];
	}

	public void blendWeights(bool b)
	{
		CurrentBW = (b ? ((CurrentBW + 1) % 3) : ((CurrentBW != 0) ? ((CurrentBW - 1) % 3) : (CurrentBW = 2)));
		switch (CurrentBW)
		{
		case 0:
			QualitySettings.blendWeights = BlendWeights.OneBone;
			blendWeightsText.text = BlendWeights.OneBone.ToString().ToUpper();
			break;
		case 1:
			QualitySettings.blendWeights = BlendWeights.TwoBones;
			blendWeightsText.text = BlendWeights.TwoBones.ToString().ToUpper();
			break;
		case 2:
			QualitySettings.blendWeights = BlendWeights.FourBones;
			blendWeightsText.text = BlendWeights.FourBones.ToString().ToUpper();
			break;
		}
	}

	public void SetBrightness(float v)
	{
		if (!(BrightnessImage == null))
		{
			_brightness = v;
			BrightnessImage.SetValue(v);
			BrightnessSlider.value = v;
			BrightnessText.text = string.Format("{0}%", (v * 100f).ToString("F0"));
		}
	}

	public void SetLodBias(float value)
	{
		QualitySettings.lodBias = value;
		_lodBias = value;
		LoadBiasText.text = string.Format("{0}", value.ToString("F2"));
	}

	public void ShadowDistance(float value)
	{
		if (_shadowEnable)
		{
			QualitySettings.shadowDistance = value;
		}
		ShadowDistanceText.text = string.Format("{0}m", value.ToString("F0"));
		cacheShadowDistance = value;
	}

	public void SetShadowEnable(bool enable)
	{
		QualitySettings.shadowDistance = (enable ? cacheShadowDistance : 0f);
		_shadowEnable = enable;
		ShadowEnebleText.text = (enable ? "ENABLE" : "DISABLE");
	}

	public void SetRealTimeReflection(bool b)
	{
		QualitySettings.realtimeReflectionProbes = b;
		_realtimeReflection = b;
		RealtimeReflectionText.text = (_realtimeReflection ? "ENABLE" : "DISABLE");
	}

	public void SetHUDScale(float value)
	{
		if (!(HUDCanvas == null))
		{
			HUDCanvas.matchWidthOrHeight = 1f - value;
			_hudScale = value;
			HudScaleText.text = string.Format("{0}", value.ToString("F2"));
		}
	}

	public void Resolution(bool b)
	{
		CurrentRS = (b ? ((CurrentRS + 1) % Screen.resolutions.Length) : ((CurrentRS != 0) ? ((CurrentRS - 1) % Screen.resolutions.Length) : (CurrentRS = Screen.resolutions.Length - 1)));
		ResolutionText.text = Screen.resolutions[CurrentRS].width + " X " + Screen.resolutions[CurrentRS].height;
	}

	public void Volumen(float v)
	{
		AudioListener.volume = v;
		_volumen = v;
		VolumenText.text = (_volumen * 100f).ToString("00") + "%";
	}

	public void ShadowProjectionType(bool b)
	{
		if (b)
		{
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
			ShadowProjectionText.text = ShadowProjection.StableFit.ToString().ToUpper();
		}
		else
		{
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;
			ShadowProjectionText.text = ShadowProjection.CloseFit.ToString().ToUpper();
		}
	}

	public void ApplyResolution()
	{
		bool fullscreen = AutoApplyResolution && useFullScreen;
		Screen.SetResolution(Screen.resolutions[CurrentRS].width, Screen.resolutions[CurrentRS].height, fullscreen);
	}

	private void LoadAndApply()
	{
		bl_Input.Instance.InitInput();
		CurrentAA = PlayerPrefs.GetInt("GameName.AntiAliasing", DefaultAntiAliasing);
		CurrentAS = PlayerPrefs.GetInt("GameName.AnisoTropic", DefaultAnisoTropic);
		CurrentBW = PlayerPrefs.GetInt("GameName.BlendWeight", DefaultBlendWeight);
		CurrentQuality = PlayerPrefs.GetInt("GameName.QualityLevel", DefaultQuality);
		CurrentRS = PlayerPrefs.GetInt("GameName.ResolutionScreen", DefaultResolution);
		CurrentVSC = PlayerPrefs.GetInt("GameName.VSyncCount", DefaultVSync);
		CurrentTL = PlayerPrefs.GetInt("GameName.TextureLimit", 0);
		CurrentSC = PlayerPrefs.GetInt("GameName.ShadowCascade", 0);
		_showFPS = PlayerPrefs.GetInt("GameName.ShowFPS", 0) == 1;
		_volumen = PlayerPrefs.GetFloat("GameName.Volumen", 1f);
		float value = PlayerPrefs.GetFloat("GameName.ShadowDistance", DefaultShadowDistance);
		shadowProjection = PlayerPrefs.GetInt("GameName.ShadowProjection", 0) == 1;
		PauseSound(PlayerPrefs.GetInt("GameName.PauseAudio", 0) == 1);
		useFullScreen = PlayerPrefs.GetInt("GameName.ResolutionMode", 0) == 1;
		_shadowEnable = AllOptionsKeyPro.IntToBool(PlayerPrefs.GetInt("GameName.ShadowEnable"));
		_brightness = PlayerPrefs.GetFloat("GameName.Brightness", DefaultBrightness);
		_realtimeReflection = AllOptionsKeyPro.IntToBool(PlayerPrefs.GetInt("GameName.RealtimeReflection", 1));
		_lodBias = PlayerPrefs.GetFloat("GameName.LoadBias", DefaultLoadBias);
		_hudScale = PlayerPrefs.GetFloat("GameName.HudScale", _hudScale);
		SetBrightness(_brightness);
		ShadowDistance(value);
		ShadowDistanceSlider.value = value;
		Volumen(_volumen);
		VolumenSlider.value = _volumen;
		ShadowProjectionType(shadowProjection);
		SetShadowEnable(_shadowEnable);
		SetRealTimeReflection(_realtimeReflection);
		SetLodBias(_lodBias);
		SetHUDScale(_hudScale);
		ApplyResolution();
		QualitySettings.shadowCascades = ShadowCascadeOptions[CurrentSC];
		ShadowCascadeText.text = ShadowCascadeNames[CurrentSC].ToUpper();
		QualityText.text = QualitySettings.names[CurrentQuality].ToUpper();
		QualitySettings.SetQualityLevel(CurrentQuality);
		FullScreenOnText.text = (useFullScreen ? "ON" : "OFF");
		ShowFPSText.text = (_showFPS ? "ON" : "OFF");
		if (FPSObject != null)
		{
			GameObject[] fPSObject = FPSObject;
			for (int i = 0; i < fPSObject.Length; i++)
			{
				fPSObject[i].SetActive(_showFPS);
			}
		}
		BrightnessSlider.value = _brightness;
		LoadBiasSlider.value = _lodBias;
		HUDScaleFactor.value = _hudScale;
		switch (CurrentAS)
		{
		case 0:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			AnisotropicText.text = AnisotropicFiltering.Disable.ToString().ToUpper();
			break;
		case 1:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			AnisotropicText.text = AnisotropicFiltering.Enable.ToString().ToUpper();
			break;
		case 2:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			AnisotropicText.text = AnisotropicFiltering.ForceEnable.ToString().ToUpper();
			break;
		}
		switch (CurrentAA)
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
		AntiAliasingText.text = AntiAliasingNames[CurrentAA].ToUpper();
		switch (CurrentVSC)
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
		vSyncText.text = VSyncNames[CurrentVSC].ToUpper();
		switch (CurrentBW)
		{
		case 0:
			QualitySettings.blendWeights = BlendWeights.OneBone;
			blendWeightsText.text = BlendWeights.OneBone.ToString().ToUpper();
			break;
		case 1:
			QualitySettings.blendWeights = BlendWeights.TwoBones;
			blendWeightsText.text = BlendWeights.TwoBones.ToString().ToUpper();
			break;
		case 2:
			QualitySettings.blendWeights = BlendWeights.FourBones;
			blendWeightsText.text = BlendWeights.FourBones.ToString().ToUpper();
			break;
		}
		QualitySettings.masterTextureLimit = CurrentTL;
		TextureLimitText.text = TextureQualityNames[CurrentTL];
		ResolutionText.text = Screen.resolutions[CurrentRS].width + " X " + Screen.resolutions[CurrentRS].height;
		bool fullscreen = AutoApplyResolution && useFullScreen;
		Screen.SetResolution(Screen.resolutions[CurrentRS].width, Screen.resolutions[CurrentRS].height, fullscreen);
	}

	private IEnumerator WaitForSwichet(int _id)
	{
		yield return StartCoroutine(WaitForRealSeconds(0.25f));
		for (int i = 0; i < Panels.Length; i++)
		{
			Panels[i].SetActive(value: false);
		}
		Panels[_id].SetActive(value: true);
		if (TitlePanelText != null)
		{
			TitlePanelText.text = Panels[_id].name.ToUpper();
		}
	}

	public static IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}

	private IEnumerator HideAnimate()
	{
		if (ContentAnim != null)
		{
			ContentAnim.SetBool("Show", value: false);
			yield return new WaitForSeconds(ContentAnim.GetCurrentAnimatorStateInfo(0).length);
			SettingsPanel.SetActive(value: false);
		}
		else
		{
			SettingsPanel.SetActive(value: false);
		}
	}

	public void SaveOptions()
	{
		PlayerPrefs.SetInt("GameName.AnisoTropic", CurrentAS);
		PlayerPrefs.SetInt("GameName.AntiAliasing", CurrentAA);
		PlayerPrefs.SetInt("GameName.BlendWeight", CurrentBW);
		PlayerPrefs.SetInt("GameName.QualityLevel", CurrentQuality);
		PlayerPrefs.SetInt("GameName.ResolutionScreen", CurrentRS);
		PlayerPrefs.SetInt("GameName.VSyncCount", CurrentVSC);
		PlayerPrefs.SetInt("GameName.AnisoTropic", CurrentAS);
		PlayerPrefs.SetInt("GameName.TextureLimit", CurrentTL);
		PlayerPrefs.SetInt("GameName.ShadowCascade", CurrentSC);
		PlayerPrefs.SetFloat("GameName.Volumen", _volumen);
		PlayerPrefs.SetFloat("GameName.ShadowDistance", cacheShadowDistance);
		PlayerPrefs.SetInt("GameName.ShadowProjection", shadowProjection ? 1 : 0);
		PlayerPrefs.SetInt("GameName.ShowFPS", _showFPS ? 1 : 0);
		PlayerPrefs.SetInt("GameName.PauseAudio", _isPauseSound ? 1 : 0);
		PlayerPrefs.SetInt("GameName.ResolutionMode", useFullScreen ? 1 : 0);
		PlayerPrefs.SetInt("GameName.ShadowEnable", AllOptionsKeyPro.BoolToInt(_shadowEnable));
		PlayerPrefs.SetFloat("GameName.Brightness", _brightness);
		PlayerPrefs.SetInt("GameName.RealtimeReflection", AllOptionsKeyPro.BoolToInt(_realtimeReflection));
		PlayerPrefs.SetFloat("GameName.LoadBias", _lodBias);
		PlayerPrefs.SetFloat("GameName.HudScale", _hudScale);
	}
}
