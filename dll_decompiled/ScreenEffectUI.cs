using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using Illusion.Extensions;
using Manager;
using TMPro;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class ScreenEffectUI : MonoBehaviour
{
	[Serializable]
	private class Selector
	{
		public Button _button;

		public TextMeshProUGUI _text;

		public string text
		{
			get
			{
				return _text.text;
			}
			set
			{
				_text.text = value;
			}
		}
	}

	[Serializable]
	private class InputCombination
	{
		public Slider slider;

		public InputField input;

		public Button buttonDefault;

		public bool interactable
		{
			set
			{
				input.interactable = value;
				slider.interactable = value;
				if ((bool)buttonDefault)
				{
					buttonDefault.interactable = value;
				}
			}
		}

		public string text
		{
			get
			{
				return input.text;
			}
			set
			{
				input.text = value;
				slider.value = GlobalMethod.StringToFloat(value);
			}
		}

		public float value
		{
			get
			{
				return slider.value;
			}
			set
			{
				slider.value = value;
				float num = value;
				num *= 1000f;
				num = Mathf.Floor(num);
				num /= 1000f;
				input.text = num.ToString("0.###");
			}
		}

		public float FogDensityValue
		{
			get
			{
				return slider.value;
			}
			set
			{
				slider.value = value;
				float num = value;
				num *= 1000000f;
				num = Mathf.Floor(num);
				num /= 1000f;
				input.text = num.ToString("0.###");
			}
		}

		public float ColorValue
		{
			get
			{
				return slider.value;
			}
			set
			{
				slider.value = value;
				input.text = ((int)Mathf.Lerp(0f, 255f, value)).ToString();
			}
		}

		public float BloomColorValue
		{
			get
			{
				return slider.value;
			}
			set
			{
				slider.value = value;
				float t = Mathf.InverseLerp(0f, 2f, value);
				input.text = ((int)Mathf.Lerp(0f, 255f, t)).ToString();
			}
		}

		public int IntValue
		{
			set
			{
				slider.value = value;
				input.text = value.ToString();
			}
		}

		public float Min => slider.minValue;

		public float Max => slider.maxValue;
	}

	[Serializable]
	private class EffectInfo
	{
		public GameObject obj;

		public ScreenEffect CtrlScreenEffect;

		public Toggle toggle;

		public Text[] texts;

		public Text text;

		public string title = "";

		public HColorPickerCtrl colorPickerCtrl;

		public bool active
		{
			get
			{
				return obj.activeSelf;
			}
			set
			{
				if (obj.SetActiveIfDifferent(value))
				{
					if (value)
					{
						text.text = title;
					}
					else if (colorPickerCtrl != null)
					{
						colorPickerCtrl.Close();
					}
				}
			}
		}

		public bool isUpdateInfo { get; set; }

		public virtual void Init()
		{
			isUpdateInfo = false;
		}

		public virtual void UpdateInfo()
		{
		}

		public virtual void Apply()
		{
		}

		public virtual void InitSlider()
		{
		}

		public virtual bool GetEnable()
		{
			return false;
		}

		public virtual void LoadPresetParam()
		{
		}
	}

	[Serializable]
	private class PresetInfo : EffectInfo
	{
		[SerializeField]
		[Header("プリセット")]
		private Dropdown presetList;

		[SerializeField]
		private InputField saveName;

		[SerializeField]
		private Button newSave;

		[SerializeField]
		private Button overrideSave;

		[SerializeField]
		private Button load;

		[SerializeField]
		private Button init;

		private string initConfirmText = "すべて初期化しますか？";

		private string initConfirmTextYes = "初期化する";

		private string initConfirmTextNo = "やめる";

		public int PresetID = -1;

		public int userPresetID = -1;

		private EffectInfo[] otherInfos;

		private string SaveName { get; set; }

		public override void Init()
		{
			base.Init();
			saveName.textComponent.text = "未入力";
			saveName.onEndEdit.RemoveAllListeners();
			saveName.onEndEdit.AddListener(OnEndEditSaveName);
			newSave.onClick.RemoveAllListeners();
			newSave.onClick.AddListener(delegate
			{
				if (SaveName == null)
				{
					SaveName = "未入力";
				}
				CtrlScreenEffect.Save(SaveName);
				InitPresetList();
			});
			overrideSave.onClick.RemoveAllListeners();
			overrideSave.onClick.AddListener(delegate
			{
				CtrlScreenEffect.Save(presetList.captionText.text, CtrlScreenEffect.UserFileNames[userPresetID]);
				InitPresetList();
			});
			load.onClick.RemoveAllListeners();
			load.onClick.AddListener(OnClickLoad);
			init.onClick.RemoveAllListeners();
			init.onClick.AddListener(OnClickInit);
		}

		public void SetOtherInfos(EffectInfo[] _others)
		{
			otherInfos = _others;
		}

		public void SetConfirmText(string confirm, string yes, string no)
		{
			initConfirmText = confirm;
			initConfirmTextYes = yes;
			initConfirmTextNo = no;
		}

		public void InitPresetList()
		{
			presetList.ClearOptions();
			List<string> list = new List<string>();
			list.Add("未設定");
			foreach (KeyValuePair<int, ScreenEffect.PresetInfo> screenEffectPresetInfo in HSceneManager.HResourceTables.ScreenEffectPresetInfos)
			{
				list.Add(screenEffectPresetInfo.Value.showName);
			}
			if (CtrlScreenEffect != null)
			{
				list.AddRange(CtrlScreenEffect.GetAllEffectDataName());
			}
			presetList.AddOptions(list);
			presetList.onValueChanged.RemoveAllListeners();
			presetList.onValueChanged.AddListener(OnValueChangedPreset);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			saveName.text = SaveName;
			base.isUpdateInfo = false;
		}

		private void OnValueChangedPreset(int val)
		{
			int num = val - 1;
			if (val != 0)
			{
				if (num < HSceneManager.HResourceTables.ScreenEffectPresetInfos.Count)
				{
					PresetID = num;
					userPresetID = -1;
				}
				else
				{
					PresetID = -1;
					userPresetID = num - HSceneManager.HResourceTables.ScreenEffectPresetInfos.Count;
				}
			}
			else
			{
				PresetID = -1;
				userPresetID = -1;
			}
		}

		private void OnEndEditSaveName(string _text)
		{
			if (!base.isUpdateInfo)
			{
				if (_text.IsNullOrEmpty())
				{
					SaveName = "未入力";
				}
				else
				{
					SaveName = _text;
				}
			}
		}

		private void OnClickLoad()
		{
			if (userPresetID < 0 && PresetID >= 0)
			{
				ScreenEffect.PresetInfo presetInfo = HSceneManager.HResourceTables.ScreenEffectPresetInfos[PresetID];
				CtrlScreenEffect.SetEffectSetting(presetInfo.assetbundle.asset, presetInfo.assetbundle.assetbundle, presetInfo.assetbundle.manifest);
				EffectInfo[] array = otherInfos;
				foreach (EffectInfo obj in array)
				{
					obj.LoadPresetParam();
					obj.UpdateInfo();
				}
			}
			else if (userPresetID >= 0 && PresetID < 0)
			{
				string fileName = CtrlScreenEffect.UserFileNames[userPresetID];
				CtrlScreenEffect.SetEffectSetting(fileName);
				EffectInfo[] array = otherInfos;
				foreach (EffectInfo obj2 in array)
				{
					obj2.LoadPresetParam();
					obj2.UpdateInfo();
				}
			}
		}

		private void OnClickInit()
		{
			if (!ConfirmDialog.active)
			{
				ConfirmDialog.Status status = ConfirmDialog.status;
				status.Sentence = initConfirmText;
				status.YesText = initConfirmTextYes;
				status.NoText = initConfirmTextNo;
				status.Yes = delegate
				{
					AllInit();
				};
				status.No = delegate
				{
				};
				ConfirmDialog.Load();
			}
		}

		private void AllInit()
		{
			if (!(CtrlScreenEffect == null))
			{
				CtrlScreenEffect.ResetAllSetting();
				EffectInfo[] array = otherInfos;
				foreach (EffectInfo obj in array)
				{
					obj.LoadPresetParam();
					obj.UpdateInfo();
				}
			}
		}
	}

	[Serializable]
	private class ColorGradingInfo : EffectInfo
	{
		public Dropdown dropdownLookupTexture;

		public InputCombination icBlend;

		public InputCombination icSaturation;

		public InputCombination icBrightness;

		public InputCombination icContrast;

		private ColorGrading ColorGrading { get; set; }

		private PostProcessVolume PostProcessVolumeBlend { get; set; }

		private ColorGrading ColorGradingBlend { get; set; }

		private float Blend
		{
			set
			{
				PostProcessVolumeBlend.weight = Mathf.Clamp(value, 0f, 1f);
			}
		}

		private float Saturation
		{
			set
			{
				ColorGrading.SafeProc(delegate(ColorGrading _cg)
				{
					_cg.saturation.value = value;
				});
				ColorGradingBlend.SafeProc(delegate(ColorGrading _cg)
				{
					_cg.saturation.value = value;
				});
			}
		}

		private float Brightness
		{
			set
			{
				ColorGrading.SafeProc(delegate(ColorGrading _cg)
				{
					_cg.brightness.value = value;
				});
				ColorGradingBlend.SafeProc(delegate(ColorGrading _cg)
				{
					_cg.brightness.value = value;
				});
			}
		}

		private float Contrast
		{
			set
			{
				ColorGrading.SafeProc(delegate(ColorGrading _cg)
				{
					_cg.contrast.value = value;
				});
				ColorGradingBlend.SafeProc(delegate(ColorGrading _cg)
				{
					_cg.contrast.value = value;
				});
			}
		}

		private int DropDownSelect { get; set; }

		private float cgBlend { get; set; }

		private int cgSaturation { get; set; }

		private int cgBrightness { get; set; }

		private int cgContrast { get; set; }

		public void Init(ColorGrading _colorGrading, PostProcessVolume _postProcessVolume)
		{
			base.Init();
			ColorGrading = _colorGrading;
			PostProcessVolumeBlend = _postProcessVolume;
			ColorGradingBlend = PostProcessVolumeBlend.profile.GetSetting<ColorGrading>();
			dropdownLookupTexture.options = HSceneManager.HResourceTables.ColorFilterInfos.Select((KeyValuePair<int, AssetBundleInfo> v) => new Dropdown.OptionData(v.Value.name)).ToList();
			dropdownLookupTexture.onValueChanged.RemoveAllListeners();
			dropdownLookupTexture.onValueChanged.AddListener(OnValueChangedLookupTexture);
			icBlend.slider.onValueChanged.RemoveAllListeners();
			icBlend.input.onEndEdit.RemoveAllListeners();
			icBlend.buttonDefault.onClick.RemoveAllListeners();
			icSaturation.slider.onValueChanged.RemoveAllListeners();
			icSaturation.input.onEndEdit.RemoveAllListeners();
			icSaturation.buttonDefault.onClick.RemoveAllListeners();
			icBrightness.slider.onValueChanged.RemoveAllListeners();
			icBrightness.input.onEndEdit.RemoveAllListeners();
			icBrightness.buttonDefault.onClick.RemoveAllListeners();
			icContrast.slider.onValueChanged.RemoveAllListeners();
			icContrast.input.onEndEdit.RemoveAllListeners();
			icContrast.buttonDefault.onClick.RemoveAllListeners();
			icBlend.slider.onValueChanged.AddListener(OnValueChangedBlend);
			icBlend.input.onEndEdit.AddListener(OnEndEditBlend);
			icBlend.buttonDefault.onClick.AddListener(OnClickBlend);
			icSaturation.slider.onValueChanged.AddListener(OnValueChangedSaturation);
			icSaturation.input.onEndEdit.AddListener(OnEndEditSaturation);
			icSaturation.buttonDefault.onClick.AddListener(OnClickSaturation);
			icBrightness.slider.onValueChanged.AddListener(OnValueChangedBrightness);
			icBrightness.input.onEndEdit.AddListener(OnEndEditBrightness);
			icBrightness.buttonDefault.onClick.AddListener(OnClickBrightness);
			icContrast.slider.onValueChanged.AddListener(OnValueChangedContrast);
			icContrast.input.onEndEdit.AddListener(OnEndEditContrast);
			icContrast.buttonDefault.onClick.AddListener(OnClickContrast);
		}

		public override void InitSlider()
		{
			OnClickBlend();
			OnClickSaturation();
			OnClickBrightness();
			OnClickContrast();
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			dropdownLookupTexture.value = DropDownSelect;
			icBlend.value = cgBlend;
			icSaturation.IntValue = cgSaturation;
			icBrightness.IntValue = cgBrightness;
			icContrast.IntValue = cgContrast;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			Blend = cgBlend;
			Saturation = cgSaturation;
			Brightness = cgBrightness;
			Contrast = cgContrast;
			SetLookupTexture(DropDownSelect);
		}

		public override void LoadPresetParam()
		{
			cgBlend = PostProcessVolumeBlend.weight;
			cgSaturation = (int)ColorGrading.saturation.value;
			cgBrightness = (int)ColorGrading.brightness.value;
			cgContrast = (int)ColorGrading.contrast.value;
			DropDownSelect = CtrlScreenEffect.NowColoGradingKind;
		}

		public void SetLookupTexture(int _no)
		{
			if (!(ColorGradingBlend == null))
			{
				DropDownSelect = _no;
				if (HSceneManager.HResourceTables.ColorFilterInfos.TryGetValue(_no, out var value))
				{
					Texture x = CommonLib.LoadAsset<Texture>(value.assetbundle, value.asset, clone: false, value.manifest);
					AssetBundleManager.UnloadAssetBundle(value.assetbundle, isUnloadForceRefCount: true);
					ColorGradingBlend.ldrLut.Override(x);
				}
			}
		}

		private void OnValueChangedLookupTexture(int _value)
		{
			if (!base.isUpdateInfo)
			{
				SetLookupTexture(_value);
				CtrlScreenEffect.NowColoGradingKind = DropDownSelect;
			}
		}

		private void OnValueChangedBlend(float _value)
		{
			if (!base.isUpdateInfo)
			{
				cgBlend = _value;
				Blend = _value;
				icBlend.value = _value;
			}
		}

		private void OnEndEditBlend(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float value = (Blend = (cgBlend = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icBlend.Min, icBlend.Max)));
				icBlend.value = value;
			}
		}

		private void OnClickBlend()
		{
			if (!base.isUpdateInfo)
			{
				cgBlend = CtrlScreenEffect.InitColorValue.blend;
				Blend = CtrlScreenEffect.InitColorValue.blend;
				icBlend.value = CtrlScreenEffect.InitColorValue.blend;
			}
		}

		private void OnValueChangedSaturation(float _value)
		{
			if (!base.isUpdateInfo)
			{
				cgSaturation = Mathf.FloorToInt(_value);
				Saturation = _value;
				icSaturation.IntValue = cgSaturation;
			}
		}

		private void OnEndEditSaturation(string _text)
		{
			if (!base.isUpdateInfo)
			{
				int num = (cgSaturation = Mathf.FloorToInt(Mathf.Clamp(GlobalMethod.StringToFloat(_text), icSaturation.Min, icSaturation.Max)));
				Saturation = num;
				icSaturation.IntValue = num;
			}
		}

		private void OnClickSaturation()
		{
			if (!base.isUpdateInfo)
			{
				int intValue = (cgSaturation = Mathf.FloorToInt(Mathf.Clamp(CtrlScreenEffect.InitColorValue.saturation, icSaturation.Min, icSaturation.Max)));
				Saturation = CtrlScreenEffect.InitColorValue.saturation;
				icSaturation.IntValue = intValue;
			}
		}

		private void OnValueChangedBrightness(float _value)
		{
			if (!base.isUpdateInfo)
			{
				cgBrightness = Mathf.FloorToInt(_value);
				Brightness = _value;
				icBrightness.IntValue = cgBrightness;
			}
		}

		private void OnEndEditBrightness(string _text)
		{
			if (!base.isUpdateInfo)
			{
				int num = (cgBrightness = Mathf.FloorToInt(Mathf.Clamp(GlobalMethod.StringToFloat(_text), icBrightness.Min, icBrightness.Max)));
				Brightness = num;
				icBrightness.IntValue = num;
			}
		}

		private void OnClickBrightness()
		{
			if (!base.isUpdateInfo)
			{
				int intValue = (cgBrightness = Mathf.FloorToInt(Mathf.Clamp(CtrlScreenEffect.InitColorValue.brightness, icBrightness.Min, icBrightness.Max)));
				Brightness = CtrlScreenEffect.InitColorValue.brightness;
				icBrightness.IntValue = intValue;
			}
		}

		private void OnValueChangedContrast(float _value)
		{
			if (!base.isUpdateInfo)
			{
				cgContrast = Mathf.FloorToInt(_value);
				Contrast = _value;
				icContrast.IntValue = cgContrast;
			}
		}

		private void OnEndEditContrast(string _text)
		{
			if (!base.isUpdateInfo)
			{
				int num = (cgContrast = Mathf.FloorToInt(Mathf.Clamp(GlobalMethod.StringToFloat(_text), icContrast.Min, icContrast.Max)));
				Contrast = num;
				icContrast.IntValue = num;
			}
		}

		private void OnClickContrast()
		{
			if (!base.isUpdateInfo)
			{
				int intValue = (cgContrast = Mathf.FloorToInt(Mathf.Clamp(CtrlScreenEffect.InitColorValue.contrast, icContrast.Min, icContrast.Max)));
				Contrast = CtrlScreenEffect.InitColorValue.contrast;
				icContrast.IntValue = intValue;
			}
		}
	}

	[Serializable]
	private class AmbientOcclusionInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Image imgColor;

		public Button btColorDef;

		public Button btColor;

		public InputCombination icIntensity;

		public InputCombination icThicknessModeifier;

		private AmbientOcclusion AmbientOcculusion { get; set; }

		private bool enableAmbientOcclusion { get; set; }

		private Color aoColor { get; set; }

		private float aoIntensity { get; set; }

		private float aoThicknessModeifier { get; set; }

		public void Init(AmbientOcclusion _ambientOcculusion)
		{
			base.Init();
			AmbientOcculusion = _ambientOcculusion;
			toggleEnable.onValueChanged.RemoveAllListeners();
			btColorDef.onClick.RemoveAllListeners();
			btColor.onClick.RemoveAllListeners();
			icIntensity.slider.onValueChanged.RemoveAllListeners();
			icIntensity.input.onEndEdit.RemoveAllListeners();
			icIntensity.buttonDefault.onClick.RemoveAllListeners();
			icThicknessModeifier.slider.onValueChanged.RemoveAllListeners();
			icThicknessModeifier.input.onEndEdit.RemoveAllListeners();
			icThicknessModeifier.buttonDefault.onClick.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			btColorDef.onClick.AddListener(OnClickColor);
			btColor.onClick.AddListener(onClickColorSample);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensity);
			icThicknessModeifier.slider.onValueChanged.AddListener(OnValueChangedThicknessModeifier);
			icThicknessModeifier.input.onEndEdit.AddListener(OnEndEditThicknessModeifier);
			icThicknessModeifier.buttonDefault.onClick.AddListener(OnClickThicknessModeifier);
		}

		public override void InitSlider()
		{
			OnClickColor();
			OnClickIntensity();
			OnClickThicknessModeifier();
			enableAmbientOcclusion = CtrlScreenEffect.InitAmbientValue.enable;
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableAmbientOcclusion;
			imgColor.color = aoColor;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("AOcolor"))
			{
				colorPickerCtrl.Open(aoColor, delegate(Color color)
				{
					ChangeColor(color);
				}, "AOcolor", 1);
			}
			icIntensity.value = aoIntensity;
			icThicknessModeifier.value = aoThicknessModeifier;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(AmbientOcculusion == null))
			{
				AmbientOcculusion.color.value = aoColor;
				AmbientOcculusion.intensity.value = aoIntensity;
				AmbientOcculusion.thicknessModifier.value = aoThicknessModeifier;
			}
		}

		public override void LoadPresetParam()
		{
			if (!(AmbientOcculusion == null))
			{
				enableAmbientOcclusion = AmbientOcculusion.active;
				aoColor = AmbientOcculusion.color.value;
				aoIntensity = AmbientOcculusion.intensity.value;
				aoThicknessModeifier = AmbientOcculusion.thicknessModifier.value;
			}
		}

		public override bool GetEnable()
		{
			return enableAmbientOcclusion;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				enableAmbientOcclusion = _value;
			}
		}

		private void OnClickColor()
		{
			if (AmbientOcculusion == null || base.isUpdateInfo)
			{
				return;
			}
			aoColor = CtrlScreenEffect.InitAmbientValue.color;
			AmbientOcculusion.color.value = CtrlScreenEffect.InitAmbientValue.color;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("AOcolor"))
			{
				colorPickerCtrl.Open(aoColor, delegate(Color color)
				{
					ChangeColor(color);
				}, "AOcolor", 1);
			}
			imgColor.color = aoColor;
		}

		private void onClickColorSample()
		{
			if (AmbientOcculusion == null || base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("AOcolor"))
				{
					colorPickerCtrl.Close();
					return;
				}
				colorPickerCtrl.Open(aoColor, delegate(Color color)
				{
					ChangeColor(color);
				}, "AOcolor", 1);
			}
			else
			{
				colorPickerCtrl.Open(aoColor, delegate(Color color)
				{
					ChangeColor(color);
				}, "AOcolor", 1);
			}
		}

		private void ChangeColor(Color color)
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				color = new Color(color.r, color.g, color.b, 1f);
				aoColor = color;
				AmbientOcculusion.color.value = color;
				imgColor.color = aoColor;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				aoIntensity = _value;
				AmbientOcculusion.intensity.value = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				float value = (aoIntensity = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icIntensity.Min, icIntensity.Max));
				AmbientOcculusion.intensity.value = value;
				icIntensity.value = value;
			}
		}

		private void OnClickIntensity()
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				aoIntensity = CtrlScreenEffect.InitAmbientValue.intensity;
				AmbientOcculusion.intensity.value = CtrlScreenEffect.InitAmbientValue.intensity;
				icIntensity.value = CtrlScreenEffect.InitAmbientValue.intensity;
			}
		}

		private void OnValueChangedThicknessModeifier(float _value)
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				aoThicknessModeifier = _value;
				AmbientOcculusion.thicknessModifier.value = _value;
				icThicknessModeifier.value = _value;
			}
		}

		private void OnEndEditThicknessModeifier(string _text)
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				float value = (aoThicknessModeifier = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icThicknessModeifier.Min, icThicknessModeifier.Max));
				AmbientOcculusion.thicknessModifier.value = value;
				icThicknessModeifier.value = value;
			}
		}

		private void OnClickThicknessModeifier()
		{
			if (!(AmbientOcculusion == null) && !base.isUpdateInfo)
			{
				aoThicknessModeifier = CtrlScreenEffect.InitAmbientValue.thicknessModifier;
				AmbientOcculusion.thicknessModifier.value = CtrlScreenEffect.InitAmbientValue.thicknessModifier;
				icThicknessModeifier.value = CtrlScreenEffect.InitAmbientValue.thicknessModifier;
			}
		}
	}

	[Serializable]
	private class BloomInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public InputCombination icIntensity;

		public InputCombination icThreshold;

		public InputCombination icSoftKnee;

		public Toggle toggleClamp;

		public InputCombination icDiffusion;

		public Image imageColor;

		public Button btColor;

		public Button btColorSample;

		private UnityEngine.Rendering.PostProcessing.Bloom Bloom { get; set; }

		private bool enableBloom { get; set; }

		private float bloomIntensity { get; set; }

		private float bloomThreshold { get; set; }

		private float bloomSoftKnee { get; set; }

		private bool bloomClamp { get; set; }

		private float bloomDiffusion { get; set; }

		private Color bloomColor { get; set; }

		public void Init(UnityEngine.Rendering.PostProcessing.Bloom _bloom)
		{
			base.Init();
			Bloom = _bloom;
			toggleEnable.onValueChanged.RemoveAllListeners();
			icIntensity.slider.onValueChanged.RemoveAllListeners();
			icIntensity.input.onEndEdit.RemoveAllListeners();
			icIntensity.buttonDefault.onClick.RemoveAllListeners();
			icThreshold.slider.onValueChanged.RemoveAllListeners();
			icThreshold.input.onEndEdit.RemoveAllListeners();
			icThreshold.buttonDefault.onClick.RemoveAllListeners();
			icSoftKnee.slider.onValueChanged.RemoveAllListeners();
			icSoftKnee.input.onEndEdit.RemoveAllListeners();
			icSoftKnee.buttonDefault.onClick.RemoveAllListeners();
			toggleClamp.onValueChanged.RemoveAllListeners();
			icDiffusion.slider.onValueChanged.RemoveAllListeners();
			icDiffusion.input.onEndEdit.RemoveAllListeners();
			icDiffusion.buttonDefault.onClick.RemoveAllListeners();
			btColor.onClick.RemoveAllListeners();
			btColorSample.onClick.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensityDef);
			icThreshold.slider.onValueChanged.AddListener(OnValueChangedThreshold);
			icThreshold.input.onEndEdit.AddListener(OnEndEditThreshold);
			icThreshold.buttonDefault.onClick.AddListener(OnClickThresholdDef);
			icSoftKnee.slider.onValueChanged.AddListener(OnValueChangedSoftKnee);
			icSoftKnee.input.onEndEdit.AddListener(OnEndEditSoftKnee);
			icSoftKnee.buttonDefault.onClick.AddListener(OnClickSoftKnee);
			toggleClamp.onValueChanged.AddListener(OnValueChangedClamp);
			icDiffusion.slider.onValueChanged.AddListener(OnValueChangedDiffusion);
			icDiffusion.input.onEndEdit.AddListener(OnEndEditDiffusion);
			icDiffusion.buttonDefault.onClick.AddListener(OnClickDiffusion);
			btColor.onClick.AddListener(OnClickColor);
			btColorSample.onClick.AddListener(OnClickColorSample);
		}

		public override void InitSlider()
		{
			OnClickIntensityDef();
			OnClickThresholdDef();
			OnClickSoftKnee();
			OnClickDiffusion();
			OnClickColor();
			enableBloom = CtrlScreenEffect.InitBloomValue.enable;
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableBloom;
			icIntensity.value = bloomIntensity;
			icThreshold.value = bloomThreshold;
			icSoftKnee.value = bloomSoftKnee;
			toggleClamp.isOn = bloomClamp;
			icDiffusion.value = bloomDiffusion;
			imageColor.color = bloomColor;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("BloomColor"))
			{
				colorPickerCtrl.Open(bloomColor, delegate(Color color)
				{
					ChangeValueColor(color);
				}, "BloomColor", 1);
			}
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(Bloom == null))
			{
				Bloom.intensity.value = bloomIntensity;
				Bloom.threshold.value = bloomThreshold;
				Bloom.softKnee.value = bloomSoftKnee;
				Bloom.clamp.overrideState = bloomClamp;
				Bloom.diffusion.value = bloomDiffusion;
				Bloom.color.value = bloomColor;
			}
		}

		public override void LoadPresetParam()
		{
			if (!(Bloom == null))
			{
				enableBloom = Bloom.active;
				bloomIntensity = Bloom.intensity.value;
				bloomThreshold = Bloom.threshold.value;
				bloomSoftKnee = Bloom.softKnee.value;
				bloomClamp = Bloom.clamp.overrideState;
				bloomDiffusion = Bloom.diffusion.value;
				bloomColor = Bloom.color.value;
			}
		}

		public override bool GetEnable()
		{
			return enableBloom;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				enableBloom = _value;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomIntensity = _value;
				Bloom.intensity.value = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				float value = (bloomIntensity = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icIntensity.Min, icIntensity.Max));
				Bloom.intensity.value = value;
				icIntensity.value = value;
			}
		}

		private void OnClickIntensityDef()
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomIntensity = CtrlScreenEffect.InitBloomValue.intensity;
				Bloom.intensity.value = CtrlScreenEffect.InitBloomValue.intensity;
				icIntensity.value = CtrlScreenEffect.InitBloomValue.intensity;
			}
		}

		private void OnValueChangedThreshold(float _value)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomThreshold = _value;
				Bloom.threshold.value = _value;
				icThreshold.value = _value;
			}
		}

		private void OnEndEditThreshold(string _text)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				float value = (bloomThreshold = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icThreshold.Min, icThreshold.Max));
				Bloom.threshold.value = value;
				icThreshold.value = value;
			}
		}

		private void OnClickThresholdDef()
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomThreshold = CtrlScreenEffect.InitBloomValue.threshold;
				Bloom.threshold.value = CtrlScreenEffect.InitBloomValue.threshold;
				icThreshold.value = CtrlScreenEffect.InitBloomValue.threshold;
			}
		}

		private void OnValueChangedSoftKnee(float _value)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomSoftKnee = _value;
				Bloom.softKnee.value = _value;
				icSoftKnee.value = _value;
			}
		}

		private void OnEndEditSoftKnee(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float value = (bloomSoftKnee = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icSoftKnee.Min, icSoftKnee.Max));
				Bloom.softKnee.value = value;
				icSoftKnee.value = value;
			}
		}

		private void OnClickSoftKnee()
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomSoftKnee = CtrlScreenEffect.InitBloomValue.softKnee;
				Bloom.softKnee.value = CtrlScreenEffect.InitBloomValue.softKnee;
				icSoftKnee.value = CtrlScreenEffect.InitBloomValue.softKnee;
			}
		}

		private void OnValueChangedClamp(bool _value)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomClamp = _value;
				Bloom.clamp.overrideState = _value;
			}
		}

		private void OnValueChangedDiffusion(float _value)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomDiffusion = _value;
				Bloom.diffusion.value = _value;
				icDiffusion.value = _value;
			}
		}

		private void OnEndEditDiffusion(string _text)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				float value = (bloomDiffusion = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icDiffusion.Min, icDiffusion.Max));
				Bloom.diffusion.value = value;
				icDiffusion.value = value;
			}
		}

		private void OnClickDiffusion()
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				bloomDiffusion = CtrlScreenEffect.InitBloomValue.diffusion;
				Bloom.diffusion.value = CtrlScreenEffect.InitBloomValue.diffusion;
				icDiffusion.value = CtrlScreenEffect.InitBloomValue.diffusion;
			}
		}

		private void OnClickColor()
		{
			if (Bloom == null || base.isUpdateInfo)
			{
				return;
			}
			bloomColor = CtrlScreenEffect.InitBloomValue.color;
			Bloom.color.value = CtrlScreenEffect.InitBloomValue.color;
			imageColor.color = bloomColor;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("BloomColor"))
			{
				colorPickerCtrl.Open(bloomColor, delegate(Color color)
				{
					ChangeValueColor(color);
				}, "BloomColor", 1);
			}
		}

		private void OnClickColorSample()
		{
			if (Bloom == null || base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("BloomColor"))
				{
					colorPickerCtrl.Close();
					return;
				}
				colorPickerCtrl.Open(bloomColor, delegate(Color color)
				{
					ChangeValueColor(color);
				}, "BloomColor", 1);
			}
			else
			{
				colorPickerCtrl.Open(bloomColor, delegate(Color color)
				{
					ChangeValueColor(color);
				}, "BloomColor", 1);
			}
		}

		private void ChangeValueColor(Color color)
		{
			if (!(Bloom == null) && !base.isUpdateInfo)
			{
				color = new Color(color.r, color.g, color.b, 1f);
				bloomColor = color;
				Bloom.color.value = color;
				imageColor.color = bloomColor;
			}
		}
	}

	[Serializable]
	private class DOFInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public InputCombination icFocalSize;

		public InputCombination icAperture;

		private UnityStandardAssets.ImageEffects.DepthOfField depthOfField { get; set; }

		private bool enableDepth { get; set; }

		private float depthFocalSize { get; set; }

		private float depthAperture { get; set; }

		public void Init(UnityStandardAssets.ImageEffects.DepthOfField _dof)
		{
			base.Init();
			depthOfField = _dof;
			toggleEnable.onValueChanged.RemoveAllListeners();
			icFocalSize.slider.onValueChanged.RemoveAllListeners();
			icFocalSize.input.onEndEdit.RemoveAllListeners();
			icFocalSize.buttonDefault.onClick.RemoveAllListeners();
			icAperture.slider.onValueChanged.RemoveAllListeners();
			icAperture.input.onEndEdit.RemoveAllListeners();
			icAperture.buttonDefault.onClick.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			icFocalSize.slider.onValueChanged.AddListener(OnValueChangedFocalSize);
			icFocalSize.input.onEndEdit.AddListener(OnEndEditFocalSize);
			icFocalSize.buttonDefault.onClick.AddListener(OnClickFocalSizeDef);
			icAperture.slider.onValueChanged.AddListener(OnValueChangedAperture);
			icAperture.input.onEndEdit.AddListener(OnEndEditAperture);
			icAperture.buttonDefault.onClick.AddListener(OnClickApertureDef);
		}

		public override void InitSlider()
		{
			OnClickFocalSizeDef();
			OnClickApertureDef();
			enableDepth = CtrlScreenEffect.InitDoFValue.enable;
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableDepth;
			icFocalSize.value = depthFocalSize;
			icAperture.value = depthAperture;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (depthOfField != null)
			{
				depthOfField.focalSize = depthFocalSize;
				depthOfField.aperture = depthAperture;
			}
		}

		public override void LoadPresetParam()
		{
			if (!(depthOfField == null))
			{
				enableDepth = depthOfField.enabled;
				depthFocalSize = depthOfField.focalSize;
				depthAperture = depthOfField.aperture;
			}
		}

		public override bool GetEnable()
		{
			return enableDepth;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!(depthOfField == null) && !base.isUpdateInfo)
			{
				enableDepth = _value;
			}
		}

		private void OnValueChangedFocalSize(float _value)
		{
			if (!(depthOfField == null) && !base.isUpdateInfo)
			{
				depthFocalSize = _value;
				depthOfField.focalSize = _value;
				icFocalSize.value = _value;
			}
		}

		private void OnEndEditFocalSize(string _text)
		{
			if (!(depthOfField == null) && !base.isUpdateInfo)
			{
				float num = (depthFocalSize = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icFocalSize.Min, icFocalSize.Max));
				depthOfField.focalSize = num;
				icFocalSize.value = num;
			}
		}

		private void OnClickFocalSizeDef()
		{
			if (!(depthOfField == null) && !base.isUpdateInfo)
			{
				depthFocalSize = CtrlScreenEffect.InitDoFValue.focalSize;
				depthOfField.focalSize = CtrlScreenEffect.InitDoFValue.focalSize;
				icFocalSize.value = CtrlScreenEffect.InitDoFValue.focalSize;
			}
		}

		private void OnValueChangedAperture(float _value)
		{
			if (!(depthOfField == null) && !base.isUpdateInfo)
			{
				depthAperture = _value;
				depthOfField.aperture = _value;
				icAperture.value = _value;
			}
		}

		private void OnEndEditAperture(string _text)
		{
			if (!(depthOfField == null) && !base.isUpdateInfo)
			{
				float num = (depthAperture = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icAperture.Min, icAperture.Max));
				depthOfField.aperture = num;
				icAperture.value = num;
			}
		}

		private void OnClickApertureDef()
		{
			if (!(depthOfField == null) && !base.isUpdateInfo)
			{
				depthAperture = CtrlScreenEffect.InitDoFValue.aperture;
				depthOfField.aperture = CtrlScreenEffect.InitDoFValue.aperture;
				icAperture.value = CtrlScreenEffect.InitDoFValue.aperture;
			}
		}
	}

	[Serializable]
	private class VignetteInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public InputCombination icIntensity;

		private Vignette Vignette { get; set; }

		private bool enableVignette { get; set; }

		private float intensity { get; set; }

		public void Init(Vignette _vignette)
		{
			base.Init();
			Vignette = _vignette;
			toggleEnable.onValueChanged.RemoveAllListeners();
			icIntensity.slider.onValueChanged.RemoveAllListeners();
			icIntensity.input.onEndEdit.RemoveAllListeners();
			icIntensity.buttonDefault.onClick.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensityDef);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableVignette;
			icIntensity.value = intensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void InitSlider()
		{
			OnClickIntensityDef();
			enableVignette = CtrlScreenEffect.InitVignetValue.enable;
		}

		public override void Apply()
		{
			_ = Vignette == null;
		}

		public override void LoadPresetParam()
		{
			if (!(Vignette == null))
			{
				enableVignette = Vignette.active;
				intensity = Vignette.intensity.value;
			}
		}

		public override bool GetEnable()
		{
			return enableVignette;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				enableVignette = _value;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!(Vignette == null) && !base.isUpdateInfo)
			{
				intensity = _value;
				Vignette.intensity.value = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!(Vignette == null) && !base.isUpdateInfo)
			{
				float value = (intensity = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icIntensity.Min, icIntensity.Max));
				Vignette.intensity.value = value;
				icIntensity.value = value;
			}
		}

		private void OnClickIntensityDef()
		{
			if (!(Vignette == null) && !base.isUpdateInfo)
			{
				intensity = CtrlScreenEffect.InitVignetValue.intensity;
				Vignette.intensity.value = CtrlScreenEffect.InitVignetValue.intensity;
				icIntensity.value = CtrlScreenEffect.InitVignetValue.intensity;
			}
		}
	}

	[Serializable]
	private class ScreenSpaceReflectionInfo : EffectInfo
	{
		public Toggle toggleEnable;

		private ScreenSpaceReflections ScreenSpaceReflections { get; set; }

		private bool enableSSR { get; set; }

		public void Init(ScreenSpaceReflections _screenSpaceReflections)
		{
			base.Init();
			ScreenSpaceReflections = _screenSpaceReflections;
			toggleEnable.onValueChanged.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableSSR;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void InitSlider()
		{
			enableSSR = CtrlScreenEffect.InitSSRValue;
		}

		public override void Apply()
		{
			_ = ScreenSpaceReflections == null;
		}

		public override void LoadPresetParam()
		{
			enableSSR = ScreenSpaceReflections.active;
		}

		public override bool GetEnable()
		{
			return enableSSR;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				enableSSR = _value;
			}
		}
	}

	[Serializable]
	private class ReflectionProbeInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Dropdown dropdownCubemap;

		public InputCombination icIntensity;

		private ReflectionProbe ReflectionProbe { get; set; }

		private bool enableReflectionProbe { get; set; }

		private int reflectionProbeCubemap { get; set; }

		private float reflectionProbeIntensity { get; set; }

		public void Init(ReflectionProbe _reflectionProbe)
		{
			base.Init();
			ReflectionProbe = _reflectionProbe;
			dropdownCubemap.options = HSceneManager.HResourceTables.ProbeTexInfos.Select((KeyValuePair<int, AssetBundleInfo> v) => new Dropdown.OptionData(v.Value.name)).ToList();
			toggleEnable.onValueChanged.RemoveAllListeners();
			dropdownCubemap.onValueChanged.RemoveAllListeners();
			icIntensity.slider.onValueChanged.RemoveAllListeners();
			icIntensity.input.onEndEdit.RemoveAllListeners();
			icIntensity.buttonDefault.onClick.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			dropdownCubemap.onValueChanged.AddListener(OnValueChangedCubemap);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensity);
		}

		public override void InitSlider()
		{
			OnClickIntensity();
			enableReflectionProbe = CtrlScreenEffect.InitRPValue.enable;
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableReflectionProbe;
			dropdownCubemap.value = reflectionProbeCubemap;
			icIntensity.value = reflectionProbeIntensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (ReflectionProbe != null)
			{
				SetCubemap(reflectionProbeCubemap);
				ReflectionProbe.intensity = reflectionProbeIntensity;
			}
		}

		public override void LoadPresetParam()
		{
			if (ReflectionProbe != null)
			{
				enableReflectionProbe = ReflectionProbe.enabled;
				reflectionProbeCubemap = CtrlScreenEffect.NowReflectionKind;
				reflectionProbeIntensity = ReflectionProbe.intensity;
			}
		}

		public void SetCubemap(int _no)
		{
			reflectionProbeCubemap = _no;
			if (_no != 0)
			{
				if (HSceneManager.HResourceTables.ProbeTexInfos.TryGetValue(_no, out var value))
				{
					Texture customBakedTexture = CommonLib.LoadAsset<Texture>(value.assetbundle, value.asset, clone: false, value.manifest);
					AssetBundleManager.UnloadAssetBundle(value.assetbundle, isUnloadForceRefCount: true);
					ReflectionProbe.customBakedTexture = customBakedTexture;
				}
			}
			else
			{
				SingletonInitializer<BaseMap>.instance.SetDefaultReflectProbeTextrure();
			}
		}

		public override bool GetEnable()
		{
			return enableReflectionProbe;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!(ReflectionProbe == null) && !base.isUpdateInfo)
			{
				enableReflectionProbe = _value;
			}
		}

		private void OnValueChangedCubemap(int _value)
		{
			if (!(ReflectionProbe == null) && !base.isUpdateInfo)
			{
				SetCubemap(_value);
				CtrlScreenEffect.NowReflectionKind = reflectionProbeCubemap;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!(ReflectionProbe == null) && !base.isUpdateInfo)
			{
				reflectionProbeIntensity = _value;
				ReflectionProbe.intensity = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!(ReflectionProbe == null) && !base.isUpdateInfo)
			{
				float num = (reflectionProbeIntensity = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icIntensity.Min, icIntensity.Max));
				ReflectionProbe.intensity = num;
				icIntensity.value = num;
			}
		}

		private void OnClickIntensity()
		{
			if (!(ReflectionProbe == null) && !base.isUpdateInfo)
			{
				reflectionProbeIntensity = CtrlScreenEffect.InitRPValue.intensity;
				ReflectionProbe.intensity = CtrlScreenEffect.InitRPValue.intensity;
				icIntensity.value = CtrlScreenEffect.InitRPValue.intensity;
			}
		}
	}

	[Serializable]
	private class FogInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Toggle toggleExcludeFarPixels;

		public InputCombination icHeight;

		public InputCombination icHeightDensity;

		public Image imgColor;

		public Button btColorDef;

		public Button btColorsample;

		public InputCombination icDensity;

		private GlobalFog GlobalFog { get; set; }

		private bool enableFog { get; set; }

		private bool fogExcludeFarPixels { get; set; }

		private float fogHeight { get; set; }

		private float fogHeightDensity { get; set; }

		private Color fogColor { get; set; }

		private float fogDensity { get; set; }

		public void Init(GlobalFog _fog)
		{
			base.Init();
			GlobalFog = _fog;
			toggleEnable.onValueChanged.RemoveAllListeners();
			toggleExcludeFarPixels.onValueChanged.RemoveAllListeners();
			icHeight.slider.onValueChanged.RemoveAllListeners();
			icHeight.input.onEndEdit.RemoveAllListeners();
			icHeight.buttonDefault.onClick.RemoveAllListeners();
			icHeightDensity.slider.onValueChanged.RemoveAllListeners();
			icHeightDensity.input.onEndEdit.RemoveAllListeners();
			icHeightDensity.buttonDefault.onClick.RemoveAllListeners();
			btColorDef.onClick.RemoveAllListeners();
			btColorsample.onClick.RemoveAllListeners();
			icDensity.slider.onValueChanged.RemoveAllListeners();
			icDensity.input.onEndEdit.RemoveAllListeners();
			icDensity.buttonDefault.onClick.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			toggleExcludeFarPixels.onValueChanged.AddListener(OnValueChangedExcludeFarPixels);
			icHeight.slider.onValueChanged.AddListener(OnValueChangedHeight);
			icHeight.input.onEndEdit.AddListener(OnEndEditHeight);
			icHeight.buttonDefault.onClick.AddListener(OnClickHeight);
			icHeightDensity.slider.onValueChanged.AddListener(OnValueChangedHeightDensity);
			icHeightDensity.input.onEndEdit.AddListener(OnEndEditHeightDensity);
			icHeightDensity.buttonDefault.onClick.AddListener(OnClickHeightDensity);
			btColorDef.onClick.AddListener(OnClickColor);
			btColorsample.onClick.AddListener(onClickColorSample);
			icDensity.slider.onValueChanged.AddListener(OnValueChangedDensity);
			icDensity.input.onEndEdit.AddListener(OnEndEditDensity);
			icDensity.buttonDefault.onClick.AddListener(OnClickDensity);
		}

		public override void InitSlider()
		{
			OnClickHeight();
			OnClickHeightDensity();
			OnClickColor();
			OnClickDensity();
			enableFog = CtrlScreenEffect.InitFogValue.enable;
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableFog;
			toggleExcludeFarPixels.isOn = fogExcludeFarPixels;
			icHeight.value = fogHeight;
			icHeightDensity.value = fogHeightDensity;
			imgColor.color = fogColor;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("FogColor"))
			{
				colorPickerCtrl.Open(fogColor, delegate(Color color)
				{
					ChangeColorSample(color);
				}, "FogColor", 1);
			}
			icDensity.FogDensityValue = fogDensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (GlobalFog != null)
			{
				GlobalFog.excludeFarPixels = fogExcludeFarPixels;
				GlobalFog.height = fogHeight;
				GlobalFog.heightDensity = fogHeightDensity;
			}
			RenderSettings.fogColor = fogColor;
			RenderSettings.fogDensity = fogDensity;
		}

		public override void LoadPresetParam()
		{
			enableFog = RenderSettings.fog;
			fogColor = RenderSettings.fogColor;
			fogDensity = RenderSettings.fogDensity;
			if (GlobalFog != null)
			{
				enableFog = GlobalFog.enabled;
				fogExcludeFarPixels = GlobalFog.excludeFarPixels;
				fogHeight = GlobalFog.height;
				fogHeightDensity = GlobalFog.heightDensity;
			}
		}

		public override bool GetEnable()
		{
			return enableFog;
		}

		public void SetEnable(bool _value, bool _UI = true)
		{
			if (!(GlobalFog == null))
			{
				enableFog = _value;
				if (_UI)
				{
					toggleEnable.isOn = _value;
				}
			}
		}

		public void SetColor(Color _color)
		{
			fogColor = _color;
			RenderSettings.fogColor = _color;
			imgColor.color = _color;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				SetEnable(_value, _UI: false);
			}
		}

		private void OnValueChangedExcludeFarPixels(bool _value)
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				fogExcludeFarPixels = _value;
				GlobalFog.excludeFarPixels = _value;
			}
		}

		private void OnClickColor()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			fogColor = CtrlScreenEffect.InitFogValue.color;
			RenderSettings.fogColor = CtrlScreenEffect.InitFogValue.color;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("FogColor"))
			{
				colorPickerCtrl.Open(fogColor, delegate(Color color)
				{
					ChangeColorSample(color);
				}, "FogColor", 1);
			}
			imgColor.color = fogColor;
		}

		private void onClickColorSample()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("FogColor"))
				{
					colorPickerCtrl.Close();
					return;
				}
				colorPickerCtrl.Open(fogColor, delegate(Color color)
				{
					ChangeColorSample(color);
				}, "FogColor", 1);
			}
			else
			{
				colorPickerCtrl.Open(fogColor, delegate(Color color)
				{
					ChangeColorSample(color);
				}, "FogColor", 1);
			}
		}

		private void ChangeColorSample(Color color)
		{
			if (!base.isUpdateInfo)
			{
				color = new Color(color.r, color.g, color.b, 1f);
				fogColor = color;
				RenderSettings.fogColor = color;
				imgColor.color = fogColor;
			}
		}

		private void OnValueChangedHeight(float _value)
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				fogHeight = _value;
				GlobalFog.height = _value;
				icHeight.value = _value;
			}
		}

		private void OnEndEditHeight(string _text)
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				float num = (fogHeight = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icHeight.Min, icHeight.Max));
				GlobalFog.height = num;
				icHeight.value = num;
			}
		}

		private void OnClickHeight()
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				fogHeight = CtrlScreenEffect.InitFogValue.height;
				GlobalFog.height = CtrlScreenEffect.InitFogValue.height;
				icHeight.value = CtrlScreenEffect.InitFogValue.height;
			}
		}

		private void OnValueChangedHeightDensity(float _value)
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				fogHeightDensity = _value;
				GlobalFog.heightDensity = _value;
				icHeightDensity.value = _value;
			}
		}

		private void OnEndEditHeightDensity(string _text)
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				float num = (fogHeightDensity = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icHeightDensity.Min, icHeightDensity.Max));
				GlobalFog.heightDensity = num;
				icHeightDensity.value = num;
			}
		}

		private void OnClickHeightDensity()
		{
			if (!(GlobalFog == null) && !base.isUpdateInfo)
			{
				fogHeightDensity = CtrlScreenEffect.InitFogValue.heightDensity;
				GlobalFog.heightDensity = CtrlScreenEffect.InitFogValue.heightDensity;
				icHeightDensity.value = CtrlScreenEffect.InitFogValue.heightDensity;
			}
		}

		private void OnValueChangedDensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				fogDensity = _value;
				RenderSettings.fogDensity = _value;
				icDensity.FogDensityValue = _value;
			}
		}

		private void OnEndEditDensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float fogDensityValue = (RenderSettings.fogDensity = (fogDensity = Mathf.Clamp(GlobalMethod.StringToFloat(_text) / 1000f, icDensity.Min, icDensity.Max)));
				icDensity.FogDensityValue = fogDensityValue;
			}
		}

		private void OnClickDensity()
		{
			if (!base.isUpdateInfo)
			{
				fogDensity = CtrlScreenEffect.InitFogValue.density;
				RenderSettings.fogDensity = CtrlScreenEffect.InitFogValue.density;
				icDensity.FogDensityValue = CtrlScreenEffect.InitFogValue.density;
			}
		}
	}

	[Serializable]
	private class SunShaftsInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Image imgThresholdColor;

		public Button btThresholdColor;

		public Button btThresholdColorSample;

		public Image imgShaftsColor;

		public Button btShaftsColor;

		public Button btShaftsColorSample;

		public InputCombination icDistanceFallOff;

		public InputCombination icBlurSize;

		public InputCombination icIntensity;

		private SunShafts sunShafts { get; set; }

		private bool enableSunShafts { get; set; }

		private Color sunThresholdColor { get; set; }

		private Color sunColor { get; set; }

		private float sunDistanceFallOff { get; set; }

		private float sunBlurSize { get; set; }

		private float sunIntensity { get; set; }

		public void Init(SunShafts _sunShafts)
		{
			base.Init();
			sunShafts = _sunShafts;
			toggleEnable.onValueChanged.RemoveAllListeners();
			btThresholdColor.onClick.RemoveAllListeners();
			btThresholdColorSample.onClick.RemoveAllListeners();
			btShaftsColor.onClick.RemoveAllListeners();
			btShaftsColorSample.onClick.RemoveAllListeners();
			icDistanceFallOff.slider.onValueChanged.RemoveAllListeners();
			icDistanceFallOff.input.onEndEdit.RemoveAllListeners();
			icDistanceFallOff.buttonDefault.onClick.RemoveAllListeners();
			icBlurSize.slider.onValueChanged.RemoveAllListeners();
			icBlurSize.input.onEndEdit.RemoveAllListeners();
			icBlurSize.buttonDefault.onClick.RemoveAllListeners();
			icIntensity.slider.onValueChanged.RemoveAllListeners();
			icIntensity.input.onEndEdit.RemoveAllListeners();
			icIntensity.buttonDefault.onClick.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			btThresholdColor.onClick.AddListener(OnClickThresholdColor);
			btThresholdColorSample.onClick.AddListener(OnClickThresholdColorSample);
			btShaftsColor.onClick.AddListener(OnClickShaftsColor);
			btShaftsColorSample.onClick.AddListener(OnClickShaftsColorSample);
			icDistanceFallOff.slider.onValueChanged.AddListener(OnValueChangedDistanceFallOff);
			icDistanceFallOff.input.onEndEdit.AddListener(OnEndEditDistanceFallOff);
			icDistanceFallOff.buttonDefault.onClick.AddListener(OnClickDistanceFallOff);
			icBlurSize.slider.onValueChanged.AddListener(OnValueChangedBlurSize);
			icBlurSize.input.onEndEdit.AddListener(OnEndEditBlurSize);
			icBlurSize.buttonDefault.onClick.AddListener(OnClickBlurSize);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensity);
		}

		public override void InitSlider()
		{
			OnClickThresholdColor();
			OnClickShaftsColor();
			OnClickDistanceFallOff();
			OnClickBlurSize();
			OnClickIntensity();
			enableSunShafts = CtrlScreenEffect.InitSunShaftsValue.enable;
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableSunShafts;
			imgThresholdColor.color = sunThresholdColor;
			imgShaftsColor.color = sunColor;
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("SunThreshold"))
				{
					colorPickerCtrl.Open(sunThresholdColor, delegate(Color color)
					{
						ChangeThresholdColor(color);
					}, "SunThreshold", 1);
				}
				else if (colorPickerCtrl.Check("SunColor"))
				{
					colorPickerCtrl.Open(sunColor, delegate(Color color)
					{
						ChangeValueSunColor(color);
					}, "SunColor", 1);
				}
			}
			icDistanceFallOff.value = sunDistanceFallOff;
			icBlurSize.value = sunBlurSize;
			icIntensity.value = sunIntensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(sunShafts == null))
			{
				sunShafts.sunThreshold = sunThresholdColor;
				sunShafts.sunColor = sunColor;
				sunShafts.maxRadius = sunDistanceFallOff;
				sunShafts.sunShaftBlurRadius = sunBlurSize;
				sunShafts.sunShaftIntensity = sunIntensity;
			}
		}

		public override void LoadPresetParam()
		{
			if (!(sunShafts == null))
			{
				enableSunShafts = sunShafts.enabled;
				sunThresholdColor = sunShafts.sunThreshold;
				sunColor = sunShafts.sunColor;
				sunDistanceFallOff = sunShafts.maxRadius;
				sunBlurSize = sunShafts.sunShaftBlurRadius;
				sunIntensity = sunShafts.sunShaftIntensity;
			}
		}

		public void SetShaftsColor(Color _color)
		{
			if (!(sunShafts == null))
			{
				sunColor = _color;
				imgShaftsColor.color = _color;
				sunShafts.sunColor = _color;
			}
		}

		public override bool GetEnable()
		{
			return enableSunShafts;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				enableSunShafts = _value;
			}
		}

		private void OnClickThresholdColor()
		{
			if (sunShafts == null || base.isUpdateInfo)
			{
				return;
			}
			sunThresholdColor = CtrlScreenEffect.InitSunShaftsValue.sunThreshold;
			sunShafts.sunThreshold = CtrlScreenEffect.InitSunShaftsValue.sunThreshold;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("SunThreshold"))
			{
				colorPickerCtrl.Open(sunThresholdColor, delegate(Color color)
				{
					ChangeThresholdColor(color);
				}, "SunThreshold", 1);
			}
			imgThresholdColor.color = sunThresholdColor;
		}

		private void OnClickThresholdColorSample()
		{
			if (sunShafts == null || base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("SunThreshold"))
				{
					colorPickerCtrl.Close();
				}
				else
				{
					colorPickerCtrl.Open(sunThresholdColor, delegate(Color color)
					{
						ChangeThresholdColor(color);
					}, "SunThreshold", 1);
				}
			}
			else
			{
				colorPickerCtrl.Open(sunThresholdColor, delegate(Color color)
				{
					ChangeThresholdColor(color);
				}, "SunThreshold", 1);
			}
			imgThresholdColor.color = sunThresholdColor;
		}

		private void ChangeThresholdColor(Color color)
		{
			color = new Color(color.r, color.g, color.b, 1f);
			sunThresholdColor = color;
			sunShafts.sunThreshold = color;
			imgThresholdColor.color = sunThresholdColor;
		}

		private void OnClickShaftsColor()
		{
			if (sunShafts == null || base.isUpdateInfo)
			{
				return;
			}
			sunColor = CtrlScreenEffect.InitSunShaftsValue.sunColor;
			sunShafts.sunColor = CtrlScreenEffect.InitSunShaftsValue.sunColor;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("SunColor"))
			{
				colorPickerCtrl.Open(sunColor, delegate(Color color)
				{
					ChangeValueSunColor(color);
				}, "SunColor", 1);
			}
			imgShaftsColor.color = sunColor;
		}

		private void OnClickShaftsColorSample()
		{
			if (sunShafts == null || base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("SunColor"))
				{
					colorPickerCtrl.Close();
					return;
				}
				colorPickerCtrl.Open(sunColor, delegate(Color color)
				{
					ChangeValueSunColor(color);
				}, "SunColor", 1);
			}
			else
			{
				colorPickerCtrl.Open(sunColor, delegate(Color color)
				{
					ChangeValueSunColor(color);
				}, "SunColor", 1);
			}
		}

		private void ChangeValueSunColor(Color color)
		{
			color = new Color(color.r, color.g, color.b, 1f);
			sunColor = color;
			sunShafts.sunColor = color;
			imgShaftsColor.color = sunColor;
		}

		private void OnValueChangedDistanceFallOff(float _value)
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				sunDistanceFallOff = _value;
				sunShafts.maxRadius = _value;
				icDistanceFallOff.value = _value;
			}
		}

		private void OnEndEditDistanceFallOff(string _text)
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				float num = (sunDistanceFallOff = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icDistanceFallOff.Min, icDistanceFallOff.Max));
				sunShafts.maxRadius = num;
				icDistanceFallOff.value = num;
			}
		}

		private void OnClickDistanceFallOff()
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				sunDistanceFallOff = CtrlScreenEffect.InitSunShaftsValue.maxRadius;
				sunShafts.maxRadius = CtrlScreenEffect.InitSunShaftsValue.maxRadius;
				icDistanceFallOff.value = CtrlScreenEffect.InitSunShaftsValue.maxRadius;
			}
		}

		private void OnValueChangedBlurSize(float _value)
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				sunBlurSize = _value;
				sunShafts.sunShaftBlurRadius = _value;
				icBlurSize.value = _value;
			}
		}

		private void OnEndEditBlurSize(string _text)
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				float num = (sunBlurSize = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icBlurSize.Min, icBlurSize.Max));
				sunShafts.sunShaftBlurRadius = num;
				icBlurSize.value = num;
			}
		}

		private void OnClickBlurSize()
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				sunBlurSize = CtrlScreenEffect.InitSunShaftsValue.sunShaftBlurRadius;
				sunShafts.sunShaftBlurRadius = CtrlScreenEffect.InitSunShaftsValue.sunShaftBlurRadius;
				icBlurSize.value = CtrlScreenEffect.InitSunShaftsValue.sunShaftBlurRadius;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				sunIntensity = _value;
				sunShafts.sunShaftIntensity = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				float num = (sunIntensity = Mathf.Clamp(GlobalMethod.StringToFloat(_text), icIntensity.Min, icIntensity.Max));
				sunShafts.sunShaftIntensity = num;
				icIntensity.value = num;
			}
		}

		private void OnClickIntensity()
		{
			if (!(sunShafts == null) && !base.isUpdateInfo)
			{
				sunIntensity = CtrlScreenEffect.InitSunShaftsValue.sunShaftIntensity;
				sunShafts.sunShaftIntensity = CtrlScreenEffect.InitSunShaftsValue.sunShaftIntensity;
				icIntensity.value = CtrlScreenEffect.InitSunShaftsValue.sunShaftIntensity;
			}
		}
	}

	[Serializable]
	private class SelfShadowInfo : EffectInfo
	{
		public Toggle toggleEnable;

		private bool enableShadow { get; set; }

		public override void Init()
		{
			base.Init();
			toggleEnable.onValueChanged.RemoveAllListeners();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = enableShadow;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void LoadPresetParam()
		{
			enableShadow = CtrlScreenEffect.NowSelfShadowEnable;
		}

		public override void InitSlider()
		{
			enableShadow = CtrlScreenEffect.InitSelfShadowValue;
		}

		public override bool GetEnable()
		{
			return enableShadow;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				enableShadow = _value;
				CtrlScreenEffect.NowSelfShadowEnable = enableShadow;
			}
		}
	}

	[Serializable]
	private class EnvironmentLightingInfo : EffectInfo
	{
		public Image imgSkyColor;

		public Button btSkyColor;

		public Button btSkyColorSample;

		public Image imgEquator;

		public Button btEquator;

		public Button btEquatorSample;

		public Image imgGround;

		public Button btGround;

		public Button btGroundSample;

		private Color environmentLightingSkyColor { get; set; }

		private Color environmentLightingEquatorColor { get; set; }

		private Color environmentLightingGroundColor { get; set; }

		public override void Init()
		{
			base.Init();
			btSkyColor.onClick.RemoveAllListeners();
			btSkyColorSample.onClick.RemoveAllListeners();
			btEquator.onClick.RemoveAllListeners();
			btEquatorSample.onClick.RemoveAllListeners();
			btGround.onClick.RemoveAllListeners();
			btGroundSample.onClick.RemoveAllListeners();
			btSkyColor.onClick.AddListener(OnClickSkyColor);
			btEquator.onClick.AddListener(OnClickEquator);
			btGround.onClick.AddListener(OnClickGround);
			btSkyColorSample.onClick.AddListener(onClickSkyColorSample);
			btEquatorSample.onClick.AddListener(onClickEquatorColorsSample);
			btGroundSample.onClick.AddListener(OnClickGroundColorSample);
		}

		public override void InitSlider()
		{
			OnClickSkyColor();
			OnClickEquator();
			OnClickGround();
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			imgSkyColor.color = environmentLightingSkyColor;
			imgEquator.color = environmentLightingEquatorColor;
			imgGround.color = environmentLightingGroundColor;
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("SkyColor"))
				{
					colorPickerCtrl.Open(environmentLightingSkyColor, delegate(Color color)
					{
						ChangeValueSkyColor(color);
					}, "SkyColor", 1);
				}
				else if (colorPickerCtrl.Check("EquatorColor"))
				{
					colorPickerCtrl.Open(environmentLightingSkyColor, delegate(Color color)
					{
						ChangeValueSkyColor(color);
					}, "EquatorColor", 1);
				}
				else if (colorPickerCtrl.Check("GroundColor"))
				{
					colorPickerCtrl.Open(environmentLightingGroundColor, delegate(Color color)
					{
						ChangeValueGroundColors(color);
					}, "GroundColor", 1);
				}
			}
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			RenderSettings.ambientSkyColor = environmentLightingSkyColor;
			RenderSettings.ambientEquatorColor = environmentLightingEquatorColor;
			RenderSettings.ambientGroundColor = environmentLightingGroundColor;
		}

		public override void LoadPresetParam()
		{
			environmentLightingSkyColor = RenderSettings.ambientSkyColor;
			environmentLightingEquatorColor = RenderSettings.ambientEquatorColor;
			environmentLightingGroundColor = RenderSettings.ambientGroundColor;
		}

		private void OnClickSkyColor()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			environmentLightingSkyColor = CtrlScreenEffect.InitEnvironmentValue.sky;
			RenderSettings.ambientSkyColor = CtrlScreenEffect.InitEnvironmentValue.sky;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("SkyColor"))
			{
				colorPickerCtrl.Open(environmentLightingSkyColor, delegate(Color color)
				{
					ChangeValueSkyColor(color);
				}, "SkyColor", 1);
			}
			imgSkyColor.color = environmentLightingSkyColor;
		}

		private void onClickSkyColorSample()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("SkyColor"))
				{
					colorPickerCtrl.Close();
					return;
				}
				colorPickerCtrl.Open(environmentLightingSkyColor, delegate(Color color)
				{
					ChangeValueSkyColor(color);
				}, "SkyColor", 1);
			}
			else
			{
				colorPickerCtrl.Open(environmentLightingSkyColor, delegate(Color color)
				{
					ChangeValueSkyColor(color);
				}, "SkyColor", 1);
			}
		}

		private void ChangeValueSkyColor(Color color)
		{
			color = new Color(color.r, color.g, color.b, 1f);
			environmentLightingSkyColor = color;
			RenderSettings.ambientSkyColor = color;
			imgSkyColor.color = environmentLightingSkyColor;
		}

		private void OnClickEquator()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			environmentLightingEquatorColor = CtrlScreenEffect.InitEnvironmentValue.equator;
			RenderSettings.ambientEquatorColor = CtrlScreenEffect.InitEnvironmentValue.equator;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("EquatorColor"))
			{
				colorPickerCtrl.Open(environmentLightingSkyColor, delegate(Color color)
				{
					ChangeValueSkyColor(color);
				}, "EquatorColor", 1);
			}
			imgEquator.color = environmentLightingEquatorColor;
		}

		private void onClickEquatorColorsSample()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("EquatorColors"))
				{
					colorPickerCtrl.Close();
					return;
				}
				colorPickerCtrl.Open(environmentLightingEquatorColor, delegate(Color color)
				{
					ChangeValueEquatorColors(color);
				}, "EquatorColors", 1);
			}
			else
			{
				colorPickerCtrl.Open(environmentLightingEquatorColor, delegate(Color color)
				{
					ChangeValueEquatorColors(color);
				}, "EquatorColors", 1);
			}
		}

		private void ChangeValueEquatorColors(Color color)
		{
			color = new Color(color.r, color.g, color.b, 1f);
			environmentLightingEquatorColor = color;
			RenderSettings.ambientEquatorColor = color;
			imgEquator.color = environmentLightingEquatorColor;
		}

		private void OnClickGround()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			environmentLightingGroundColor = CtrlScreenEffect.InitEnvironmentValue.ground;
			RenderSettings.ambientGroundColor = CtrlScreenEffect.InitEnvironmentValue.ground;
			if (colorPickerCtrl.isOpen && colorPickerCtrl.Check("GroundColor"))
			{
				colorPickerCtrl.Open(environmentLightingGroundColor, delegate(Color color)
				{
					ChangeValueGroundColors(color);
				}, "GroundColor", 1);
			}
			imgGround.color = environmentLightingGroundColor;
		}

		private void OnClickGroundColorSample()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (colorPickerCtrl.isOpen)
			{
				if (colorPickerCtrl.Check("GroundColor"))
				{
					colorPickerCtrl.Close();
					return;
				}
				colorPickerCtrl.Open(environmentLightingGroundColor, delegate(Color color)
				{
					ChangeValueGroundColors(color);
				}, "GroundColor", 1);
			}
			else
			{
				colorPickerCtrl.Open(environmentLightingGroundColor, delegate(Color color)
				{
					ChangeValueGroundColors(color);
				}, "GroundColor", 1);
			}
		}

		private void ChangeValueGroundColors(Color color)
		{
			color = new Color(color.r, color.g, color.b, 1f);
			environmentLightingGroundColor = color;
			RenderSettings.ambientGroundColor = color;
			imgGround.color = environmentLightingGroundColor;
		}
	}

	[SerializeField]
	private ScreenEffect CtrlScreenEffect;

	private HScreenEffectEnable enables;

	[Header("すべて初期化するときの確認画面のテキスト")]
	[SerializeField]
	private string InitConfirmText = "すべて初期化しますか？";

	[SerializeField]
	private string InitConfirmTextYes = "初期化する";

	[SerializeField]
	private string InitConfirmTextNo = "やめる";

	[SerializeField]
	[Header("一括制御")]
	private PostProcessVolume postProcessVolume;

	[SerializeField]
	[Header("ColorGrading用")]
	private PostProcessVolume postProcessVolumeColor;

	[SerializeField]
	[Header("Reflection Probe制御")]
	private ReflectionProbe reflectionProbe;

	[SerializeField]
	[Header("個別制御")]
	private UnityStandardAssets.ImageEffects.DepthOfField depthOfField;

	[SerializeField]
	private GlobalFog globalFog;

	[SerializeField]
	private SunShafts _sunShafts;

	[SerializeField]
	private PresetInfo presetInfo = new PresetInfo();

	[SerializeField]
	private ColorGradingInfo colorGradingInfo = new ColorGradingInfo();

	[SerializeField]
	private AmbientOcclusionInfo ambientOcclusionInfo = new AmbientOcclusionInfo();

	[SerializeField]
	private BloomInfo bloomInfo = new BloomInfo();

	[SerializeField]
	private DOFInfo dofInfo = new DOFInfo();

	[SerializeField]
	private VignetteInfo vignetteInfo = new VignetteInfo();

	[SerializeField]
	private ScreenSpaceReflectionInfo screenSpaceReflectionInfo = new ScreenSpaceReflectionInfo();

	[SerializeField]
	private ReflectionProbeInfo reflectionProbeInfo = new ReflectionProbeInfo();

	[SerializeField]
	private FogInfo fogInfo = new FogInfo();

	[SerializeField]
	private SunShaftsInfo sunShaftsInfo = new SunShaftsInfo();

	[SerializeField]
	private SelfShadowInfo selfShadowInfo = new SelfShadowInfo();

	[SerializeField]
	private EnvironmentLightingInfo environmentLightingInfo = new EnvironmentLightingInfo();

	private EffectInfo[] effectInfos;

	private Dictionary<int, UIObjectSlideOnCursor> tglSlides = new Dictionary<int, UIObjectSlideOnCursor>();

	private bool isInit;

	private IDisposable update;

	public HScreenEffectEnable Enables => enables;

	public SunShafts sunShafts => _sunShafts;

	public void Init()
	{
		GameObject gameObject = Singleton<HSceneFlagCtrl>.Instance.cameraCtrl?.gameObject;
		if (depthOfField == null)
		{
			depthOfField = gameObject?.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
		}
		if (globalFog == null)
		{
			globalFog = gameObject?.GetComponent<GlobalFog>();
		}
		if (_sunShafts == null)
		{
			_sunShafts = gameObject?.GetComponent<SunShafts>();
		}
		postProcessVolume = CtrlScreenEffect.ProcessVolume;
		postProcessVolumeColor = CtrlScreenEffect.ProcessVolumeColor;
		reflectionProbe = CtrlScreenEffect.ReflectionProbe;
		if (postProcessVolume == null)
		{
			return;
		}
		presetInfo.Init();
		colorGradingInfo.Init(CtrlScreenEffect.ColorGrading, postProcessVolumeColor);
		ambientOcclusionInfo.Init(CtrlScreenEffect.AO);
		bloomInfo.Init(CtrlScreenEffect.Bloom);
		dofInfo.Init(depthOfField);
		vignetteInfo.Init(postProcessVolume.profile.GetSetting<Vignette>());
		screenSpaceReflectionInfo.Init(postProcessVolume.profile.GetSetting<ScreenSpaceReflections>());
		reflectionProbeInfo.Init(CtrlScreenEffect.ReflectionProbe);
		fogInfo.Init(globalFog);
		sunShaftsInfo.Init(sunShafts);
		selfShadowInfo.Init();
		environmentLightingInfo.Init();
		isInit = true;
		effectInfos = new EffectInfo[12]
		{
			presetInfo, colorGradingInfo, ambientOcclusionInfo, bloomInfo, dofInfo, vignetteInfo, screenSpaceReflectionInfo, reflectionProbeInfo, fogInfo, sunShaftsInfo,
			selfShadowInfo, environmentLightingInfo
		};
		for (int i = 0; i < effectInfos.Length; i++)
		{
			effectInfos[i].CtrlScreenEffect = CtrlScreenEffect;
			effectInfos[i].InitSlider();
		}
		presetInfo.InitPresetList();
		presetInfo.SetOtherInfos(effectInfos);
		presetInfo.SetConfirmText(InitConfirmText, InitConfirmTextYes, InitConfirmTextNo);
		if (CtrlScreenEffect.LoadLastEffect)
		{
			CtrlScreenEffect.SetEffectSetting();
			for (int j = 0; j < effectInfos.Length; j++)
			{
				effectInfos[j].LoadPresetParam();
			}
		}
		UpdateInfo();
		UpdateEnable();
		if (update != null)
		{
			update.Dispose();
			update = null;
		}
		tglSlides = new Dictionary<int, UIObjectSlideOnCursor>();
		for (int k = 0; k < effectInfos.Length; k++)
		{
			if (effectInfos[k].toggle != null)
			{
				tglSlides.Add(k, effectInfos[k].toggle.GetComponent<UIObjectSlideOnCursor>());
			}
		}
		update = (from _ in Observable.EveryUpdate().TakeUntilDestroy(this)
			where base.gameObject.activeSelf
			select _).Subscribe(delegate
		{
			UpdateEnable();
			Toggle toggle = null;
			for (int l = 0; l < effectInfos.Length; l++)
			{
				toggle = effectInfos[l].toggle;
				if (toggle != null)
				{
					bool isOn = toggle.isOn;
					UIObjectSlideOnCursor uIObjectSlideOnCursor = tglSlides[l];
					if (uIObjectSlideOnCursor != null)
					{
						uIObjectSlideOnCursor.IsSlideAlways = isOn;
					}
					effectInfos[l].active = isOn;
					if (effectInfos[l].texts != null && effectInfos[l].texts.Length != 0)
					{
						Text[] texts = effectInfos[l].texts;
						for (int m = 0; m < texts.Length; m++)
						{
							texts[m].color = (isOn ? Game.selectFontColor : Game.defaultFontColor);
						}
					}
				}
			}
		});
	}

	public void UpdateInfo()
	{
		EffectInfo[] array = effectInfos;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateInfo();
		}
	}

	public void Apply()
	{
		EffectInfo[] array = effectInfos;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply();
		}
	}

	private void UpdateEnable()
	{
		enables.AO = ambientOcclusionInfo.GetEnable();
		enables.Bloom = bloomInfo.GetEnable();
		enables.DOF = dofInfo.GetEnable();
		enables.Vignette = vignetteInfo.GetEnable();
		enables.SSR = screenSpaceReflectionInfo.GetEnable();
		enables.RP = reflectionProbeInfo.GetEnable();
		enables.Fog = fogInfo.GetEnable();
		enables.SunShaft = sunShaftsInfo.GetEnable();
		enables.SelfShadow = selfShadowInfo.GetEnable();
	}

	public void ChangeMapInit()
	{
		if (!isInit)
		{
			isInit = false;
		}
	}

	public void SetEnableDef()
	{
		GraphicSystem graphicData = Manager.Config.GraphicData;
		SunLightInfo.Info info = BaseMap.sunLightInfo?.info;
		enables.AO = graphicData.SSAO;
		enables.Bloom = graphicData.Bloom;
		enables.DOF = graphicData.DepthOfField;
		enables.Vignette = graphicData.Vignette;
		enables.SSR = graphicData.SSR;
		enables.RP = graphicData.RP;
		enables.Fog = info?.fogUse ?? false;
		enables.SunShaft = info?.sunShaftsUse ?? false;
		enables.SelfShadow = graphicData.SelfShadow;
	}

	public void EndProc()
	{
		if (update != null)
		{
			update.Dispose();
			update = null;
		}
	}
}
