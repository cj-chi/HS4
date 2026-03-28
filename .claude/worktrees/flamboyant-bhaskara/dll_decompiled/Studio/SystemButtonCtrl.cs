using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace Studio;

public class SystemButtonCtrl : MonoBehaviour
{
	[Serializable]
	private class CommonInfo
	{
		public CanvasGroup group;

		public Button button;

		public bool active
		{
			set
			{
				group.Enable(value);
				button.image.color = (value ? Color.green : Color.white);
			}
		}
	}

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
				slider.value = Utility.StringToFloat(value);
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
				input.text = value.ToString();
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

		public char OnValidateInput(string _text, int _charIndex, char _addedChar)
		{
			switch (_addedChar)
			{
			case '.':
			{
				int num = ((input.selectionAnchorPosition < input.selectionFocusPosition) ? input.selectionAnchorPosition : input.selectionFocusPosition);
				int num2 = ((input.selectionAnchorPosition < input.selectionFocusPosition) ? input.selectionFocusPosition : input.selectionAnchorPosition);
				if ((num != 0 || _text.Length != num2) && _text.Contains('.'))
				{
					return '\0';
				}
				break;
			}
			default:
				return '\0';
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				break;
			}
			return _addedChar;
		}
	}

	[Serializable]
	private class EffectInfo
	{
		public GameObject obj;

		public Button button;

		public Sprite[] sprite;

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
					button.image.sprite = sprite[value ? 1 : 0];
				}
			}
		}

		public bool isUpdateInfo { get; set; }

		public virtual void Init(Sprite[] _sprite)
		{
			button.onClick.AddListener(OnClickActive);
			sprite = _sprite;
			isUpdateInfo = false;
		}

		public virtual void UpdateInfo()
		{
		}

		public virtual void Apply()
		{
		}

		private void OnClickActive()
		{
			active = !active;
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

		public void Init(Sprite[] _sprite, ColorGrading _colorGrading, PostProcessVolume _postProcessVolume)
		{
			base.Init(_sprite);
			ColorGrading = _colorGrading;
			PostProcessVolumeBlend = _postProcessVolume;
			ColorGradingBlend = PostProcessVolumeBlend.profile.GetSetting<ColorGrading>();
			dropdownLookupTexture.options = Singleton<Info>.Instance.dicColorGradingLoadInfo.Select((KeyValuePair<int, Info.LoadCommonInfo> v) => new Dropdown.OptionData(v.Value.name)).ToList();
			dropdownLookupTexture.onValueChanged.AddListener(OnValueChangedLookupTexture);
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

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			dropdownLookupTexture.value = Singleton<Studio>.Instance.sceneInfo.cgLookupTexture;
			icBlend.value = Singleton<Studio>.Instance.sceneInfo.cgBlend;
			icSaturation.IntValue = Singleton<Studio>.Instance.sceneInfo.cgSaturation;
			icBrightness.IntValue = Singleton<Studio>.Instance.sceneInfo.cgBrightness;
			icContrast.IntValue = Singleton<Studio>.Instance.sceneInfo.cgContrast;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			Blend = Singleton<Studio>.Instance.sceneInfo.cgBlend;
			Saturation = Singleton<Studio>.Instance.sceneInfo.cgSaturation;
			Brightness = Singleton<Studio>.Instance.sceneInfo.cgBrightness;
			Contrast = Singleton<Studio>.Instance.sceneInfo.cgContrast;
			SetLookupTexture(Singleton<Studio>.Instance.sceneInfo.cgLookupTexture);
		}

		public void SetLookupTexture(int _no)
		{
			Singleton<Studio>.Instance.sceneInfo.cgLookupTexture = _no;
			Info.LoadCommonInfo value = null;
			if (Singleton<Info>.Instance.dicColorGradingLoadInfo.TryGetValue(_no, out value))
			{
				Texture x = CommonLib.LoadAsset<Texture>(value.bundlePath, value.fileName);
				ColorGradingBlend.ldrLut.Override(x);
			}
		}

		private void OnValueChangedLookupTexture(int _value)
		{
			if (!base.isUpdateInfo)
			{
				SetLookupTexture(_value);
			}
		}

		private void OnValueChangedBlend(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgBlend = _value;
				Blend = _value;
				icBlend.value = _value;
			}
		}

		private void OnEndEditBlend(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icBlend.Min, icBlend.Max);
				Singleton<Studio>.Instance.sceneInfo.cgBlend = num;
				Blend = num;
				icBlend.value = num;
			}
		}

		private void OnClickBlend()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgBlend = ScreenEffectDefine.ColorGradingBlend;
				Blend = ScreenEffectDefine.ColorGradingBlend;
				icBlend.value = ScreenEffectDefine.ColorGradingBlend;
			}
		}

		private void OnValueChangedSaturation(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgSaturation = Mathf.FloorToInt(_value);
				Saturation = _value;
				icSaturation.IntValue = Singleton<Studio>.Instance.sceneInfo.cgSaturation;
			}
		}

		private void OnEndEditSaturation(string _text)
		{
			if (!base.isUpdateInfo)
			{
				int num = Mathf.FloorToInt(Mathf.Clamp(Utility.StringToFloat(_text), icSaturation.Min, icSaturation.Max));
				Singleton<Studio>.Instance.sceneInfo.cgSaturation = num;
				Saturation = num;
				icSaturation.IntValue = num;
			}
		}

		private void OnClickSaturation()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgSaturation = ScreenEffectDefine.ColorGradingSaturation;
				Saturation = ScreenEffectDefine.ColorGradingSaturation;
				icSaturation.IntValue = ScreenEffectDefine.ColorGradingSaturation;
			}
		}

		private void OnValueChangedBrightness(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgBrightness = Mathf.FloorToInt(_value);
				Brightness = _value;
				icBrightness.IntValue = Singleton<Studio>.Instance.sceneInfo.cgBrightness;
			}
		}

		private void OnEndEditBrightness(string _text)
		{
			if (!base.isUpdateInfo)
			{
				int num = Mathf.FloorToInt(Mathf.Clamp(Utility.StringToFloat(_text), icBrightness.Min, icBrightness.Max));
				Singleton<Studio>.Instance.sceneInfo.cgBrightness = num;
				Brightness = num;
				icBrightness.IntValue = num;
			}
		}

		private void OnClickBrightness()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgBrightness = ScreenEffectDefine.ColorGradingBrightness;
				Brightness = ScreenEffectDefine.ColorGradingBrightness;
				icBrightness.IntValue = ScreenEffectDefine.ColorGradingBrightness;
			}
		}

		private void OnValueChangedContrast(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgContrast = Mathf.FloorToInt(_value);
				Contrast = _value;
				icContrast.IntValue = Singleton<Studio>.Instance.sceneInfo.cgContrast;
			}
		}

		private void OnEndEditContrast(string _text)
		{
			if (!base.isUpdateInfo)
			{
				int num = Mathf.FloorToInt(Mathf.Clamp(Utility.StringToFloat(_text), icContrast.Min, icContrast.Max));
				Singleton<Studio>.Instance.sceneInfo.cgContrast = num;
				Contrast = num;
				icContrast.IntValue = num;
			}
		}

		private void OnClickContrast()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.cgContrast = ScreenEffectDefine.ColorGradingSaturation;
				Contrast = ScreenEffectDefine.ColorGradingSaturation;
				icContrast.IntValue = ScreenEffectDefine.ColorGradingSaturation;
			}
		}
	}

	[Serializable]
	private class AmbientOcclusionInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Button buttonColor;

		public InputCombination icIntensity;

		public InputCombination icThicknessModeifier;

		private AmbientOcclusion AmbientOcculusion { get; set; }

		public void Init(Sprite[] _sprite, AmbientOcclusion _ambientOcculusion)
		{
			base.Init(_sprite);
			AmbientOcculusion = _ambientOcculusion;
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			buttonColor.onClick.AddListener(OnClickColor);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			InputField input = icIntensity.input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(icIntensity.OnValidateInput));
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensity);
			icThicknessModeifier.slider.onValueChanged.AddListener(OnValueChangedThicknessModeifier);
			InputField input2 = icThicknessModeifier.input;
			input2.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input2.onValidateInput, new InputField.OnValidateInput(icThicknessModeifier.OnValidateInput));
			icThicknessModeifier.input.onEndEdit.AddListener(OnEndEditThicknessModeifier);
			icThicknessModeifier.buttonDefault.onClick.AddListener(OnClickThicknessModeifier);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableAmbientOcclusion;
			buttonColor.image.color = Singleton<Studio>.Instance.sceneInfo.aoColor;
			icIntensity.value = Singleton<Studio>.Instance.sceneInfo.aoIntensity;
			icThicknessModeifier.value = Singleton<Studio>.Instance.sceneInfo.aoThicknessModeifier;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(AmbientOcculusion == null))
			{
				AmbientOcculusion.active = Singleton<Studio>.Instance.sceneInfo.enableAmbientOcclusion;
				AmbientOcculusion.color.value = Singleton<Studio>.Instance.sceneInfo.aoColor;
				AmbientOcculusion.intensity.value = Singleton<Studio>.Instance.sceneInfo.aoIntensity;
				AmbientOcculusion.thicknessModifier.value = Singleton<Studio>.Instance.sceneInfo.aoThicknessModeifier;
			}
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableAmbientOcclusion = _value;
				AmbientOcculusion.active = _value;
			}
		}

		private void OnClickColor()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (Singleton<Studio>.Instance.colorPalette.Check("アンビエントオクルージョン"))
			{
				Singleton<Studio>.Instance.colorPalette.visible = false;
				return;
			}
			Singleton<Studio>.Instance.colorPalette.Setup("アンビエントオクルージョン", Singleton<Studio>.Instance.sceneInfo.aoColor, delegate(Color _c)
			{
				Singleton<Studio>.Instance.sceneInfo.aoColor = _c;
				AmbientOcculusion.color.value = _c;
				buttonColor.image.color = _c;
			}, _useAlpha: false);
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.aoIntensity = _value;
				AmbientOcculusion.intensity.value = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icIntensity.Min, icIntensity.Max);
				Singleton<Studio>.Instance.sceneInfo.aoIntensity = num;
				AmbientOcculusion.intensity.value = num;
				icIntensity.value = num;
			}
		}

		private void OnClickIntensity()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.aoIntensity = ScreenEffectDefine.AmbientOcclusionIntensity;
				AmbientOcculusion.intensity.value = ScreenEffectDefine.AmbientOcclusionIntensity;
				icIntensity.value = ScreenEffectDefine.AmbientOcclusionIntensity;
			}
		}

		private void OnValueChangedThicknessModeifier(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.aoThicknessModeifier = _value;
				AmbientOcculusion.thicknessModifier.value = _value;
				icThicknessModeifier.value = _value;
			}
		}

		private void OnEndEditThicknessModeifier(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icThicknessModeifier.Min, icThicknessModeifier.Max);
				Singleton<Studio>.Instance.sceneInfo.aoThicknessModeifier = num;
				AmbientOcculusion.thicknessModifier.value = num;
				icThicknessModeifier.value = num;
			}
		}

		private void OnClickThicknessModeifier()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.aoThicknessModeifier = ScreenEffectDefine.AmbientOcclusionThicknessModeifier;
				AmbientOcculusion.thicknessModifier.value = ScreenEffectDefine.AmbientOcclusionThicknessModeifier;
				icThicknessModeifier.value = ScreenEffectDefine.AmbientOcclusionThicknessModeifier;
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

		public Button buttonColor;

		private UnityEngine.Rendering.PostProcessing.Bloom Bloom { get; set; }

		public void Init(Sprite[] _sprite, UnityEngine.Rendering.PostProcessing.Bloom _bloom)
		{
			base.Init(_sprite);
			Bloom = _bloom;
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			InputField input = icIntensity.input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(icIntensity.OnValidateInput));
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensityDef);
			icThreshold.slider.onValueChanged.AddListener(OnValueChangedThreshold);
			InputField input2 = icThreshold.input;
			input2.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input2.onValidateInput, new InputField.OnValidateInput(icThreshold.OnValidateInput));
			icThreshold.input.onEndEdit.AddListener(OnEndEditThreshold);
			icThreshold.buttonDefault.onClick.AddListener(OnClickThresholdDef);
			icSoftKnee.slider.onValueChanged.AddListener(OnValueChangedSoftKnee);
			InputField input3 = icSoftKnee.input;
			input3.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input3.onValidateInput, new InputField.OnValidateInput(icSoftKnee.OnValidateInput));
			icSoftKnee.input.onEndEdit.AddListener(OnEndEditSoftKnee);
			icSoftKnee.buttonDefault.onClick.AddListener(OnClickSoftKnee);
			toggleClamp.onValueChanged.AddListener(OnValueChangedClamp);
			icDiffusion.slider.onValueChanged.AddListener(OnValueChangedDiffusion);
			InputField input4 = icDiffusion.input;
			input4.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input4.onValidateInput, new InputField.OnValidateInput(icDiffusion.OnValidateInput));
			icDiffusion.input.onEndEdit.AddListener(OnEndEditDiffusion);
			icDiffusion.buttonDefault.onClick.AddListener(OnClickDiffusion);
			buttonColor.onClick.AddListener(OnClickColor);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableBloom;
			icIntensity.value = Singleton<Studio>.Instance.sceneInfo.bloomIntensity;
			icThreshold.value = Singleton<Studio>.Instance.sceneInfo.bloomThreshold;
			icSoftKnee.value = Singleton<Studio>.Instance.sceneInfo.bloomSoftKnee;
			toggleClamp.isOn = Singleton<Studio>.Instance.sceneInfo.bloomClamp;
			icDiffusion.value = Singleton<Studio>.Instance.sceneInfo.bloomDiffusion;
			buttonColor.image.color = Singleton<Studio>.Instance.sceneInfo.bloomColor;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(Bloom == null))
			{
				Bloom.active = Singleton<Studio>.Instance.sceneInfo.enableBloom;
				Bloom.intensity.value = Singleton<Studio>.Instance.sceneInfo.bloomIntensity;
				Bloom.threshold.value = Singleton<Studio>.Instance.sceneInfo.bloomThreshold;
				Bloom.softKnee.value = Singleton<Studio>.Instance.sceneInfo.bloomSoftKnee;
				Bloom.clamp.overrideState = Singleton<Studio>.Instance.sceneInfo.bloomClamp;
				Bloom.diffusion.value = Singleton<Studio>.Instance.sceneInfo.bloomDiffusion;
				Bloom.color.value = Singleton<Studio>.Instance.sceneInfo.bloomColor;
			}
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableBloom = _value;
				Bloom.active = _value;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomIntensity = _value;
				Bloom.intensity.value = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icIntensity.Min, icIntensity.Max);
				Singleton<Studio>.Instance.sceneInfo.bloomIntensity = num;
				Bloom.intensity.value = num;
				icIntensity.value = num;
			}
		}

		private void OnClickIntensityDef()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomIntensity = ScreenEffectDefine.BloomIntensity;
				Bloom.intensity.value = ScreenEffectDefine.BloomIntensity;
				icIntensity.value = ScreenEffectDefine.BloomIntensity;
			}
		}

		private void OnValueChangedThreshold(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomThreshold = _value;
				Bloom.threshold.value = _value;
				icThreshold.value = _value;
			}
		}

		private void OnEndEditThreshold(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icThreshold.Min, icThreshold.Max);
				Singleton<Studio>.Instance.sceneInfo.bloomThreshold = num;
				Bloom.threshold.value = num;
				icThreshold.value = num;
			}
		}

		private void OnClickThresholdDef()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomThreshold = ScreenEffectDefine.BloomThreshold;
				Bloom.threshold.value = ScreenEffectDefine.BloomThreshold;
				icThreshold.value = ScreenEffectDefine.BloomThreshold;
			}
		}

		private void OnValueChangedSoftKnee(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomSoftKnee = _value;
				Bloom.softKnee.value = _value;
				icSoftKnee.value = _value;
			}
		}

		private void OnEndEditSoftKnee(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icSoftKnee.Min, icSoftKnee.Max);
				Singleton<Studio>.Instance.sceneInfo.bloomSoftKnee = num;
				Bloom.softKnee.value = num;
				icSoftKnee.value = num;
			}
		}

		private void OnClickSoftKnee()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomSoftKnee = ScreenEffectDefine.BloomSoftKnee;
				Bloom.softKnee.value = ScreenEffectDefine.BloomSoftKnee;
				icSoftKnee.value = ScreenEffectDefine.BloomSoftKnee;
			}
		}

		private void OnValueChangedClamp(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomClamp = _value;
				Bloom.clamp.overrideState = _value;
			}
		}

		private void OnValueChangedDiffusion(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomDiffusion = _value;
				Bloom.diffusion.value = _value;
				icDiffusion.value = _value;
			}
		}

		private void OnEndEditDiffusion(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icDiffusion.Min, icDiffusion.Max);
				Singleton<Studio>.Instance.sceneInfo.bloomDiffusion = num;
				Bloom.diffusion.value = num;
				icDiffusion.value = num;
			}
		}

		private void OnClickDiffusion()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomDiffusion = ScreenEffectDefine.BloomDiffusion;
				Bloom.diffusion.value = ScreenEffectDefine.BloomDiffusion;
				icDiffusion.value = ScreenEffectDefine.BloomDiffusion;
			}
		}

		private void OnClickColor()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (Singleton<Studio>.Instance.colorPalette.Check("ブルーム"))
			{
				Singleton<Studio>.Instance.colorPalette.visible = false;
				return;
			}
			Singleton<Studio>.Instance.colorPalette.Setup("ブルーム", Singleton<Studio>.Instance.sceneInfo.bloomColor, delegate(Color _c)
			{
				Singleton<Studio>.Instance.sceneInfo.bloomColor = _c;
				Bloom.color.value = _c;
				buttonColor.image.color = _c;
			}, _useAlpha: false);
		}
	}

	[Serializable]
	private class DOFInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Selector selectorForcus;

		public InputCombination icFocalSize;

		public InputCombination icAperture;

		private UnityStandardAssets.ImageEffects.DepthOfField depthOfField { get; set; }

		public void Init(Sprite[] _sprite, UnityStandardAssets.ImageEffects.DepthOfField _dof)
		{
			base.Init(_sprite);
			depthOfField = _dof;
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			selectorForcus._button.onClick.AddListener(OnClickForcus);
			icFocalSize.slider.onValueChanged.AddListener(OnValueChangedFocalSize);
			InputField input = icFocalSize.input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(icFocalSize.OnValidateInput));
			icFocalSize.input.onEndEdit.AddListener(OnEndEditFocalSize);
			icFocalSize.buttonDefault.onClick.AddListener(OnClickFocalSizeDef);
			icAperture.slider.onValueChanged.AddListener(OnValueChangedAperture);
			InputField input2 = icAperture.input;
			input2.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input2.onValidateInput, new InputField.OnValidateInput(icAperture.OnValidateInput));
			icAperture.input.onEndEdit.AddListener(OnEndEditAperture);
			icAperture.buttonDefault.onClick.AddListener(OnClickApertureDef);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableDepth;
			icFocalSize.value = Singleton<Studio>.Instance.sceneInfo.depthFocalSize;
			icAperture.value = Singleton<Studio>.Instance.sceneInfo.depthAperture;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (depthOfField != null)
			{
				depthOfField.enabled = Singleton<Studio>.Instance.sceneInfo.enableDepth;
				depthOfField.focalSize = Singleton<Studio>.Instance.sceneInfo.depthFocalSize;
				depthOfField.aperture = Singleton<Studio>.Instance.sceneInfo.depthAperture;
			}
			Singleton<Studio>.Instance.SetDepthOfFieldForcus(Singleton<Studio>.Instance.sceneInfo.depthForcus);
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableDepth = _value;
				depthOfField.enabled = _value;
			}
		}

		private void OnClickForcus()
		{
			if (!base.isUpdateInfo)
			{
				GuideObject selectObject = Singleton<GuideObjectManager>.Instance.selectObject;
				Singleton<Studio>.Instance.SetDepthOfFieldForcus((selectObject != null) ? selectObject.dicKey : (-1));
			}
		}

		private void OnValueChangedFocalSize(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.depthFocalSize = _value;
				depthOfField.focalSize = _value;
				icFocalSize.value = _value;
			}
		}

		private void OnEndEditFocalSize(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icFocalSize.Min, icFocalSize.Max);
				Singleton<Studio>.Instance.sceneInfo.depthFocalSize = num;
				depthOfField.focalSize = num;
				icFocalSize.value = num;
			}
		}

		private void OnClickFocalSizeDef()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.depthFocalSize = ScreenEffectDefine.DepthOfFieldFocalSize;
				depthOfField.focalSize = ScreenEffectDefine.DepthOfFieldFocalSize;
				icFocalSize.value = ScreenEffectDefine.DepthOfFieldFocalSize;
			}
		}

		private void OnValueChangedAperture(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.depthAperture = _value;
				depthOfField.aperture = _value;
				icAperture.value = _value;
			}
		}

		private void OnEndEditAperture(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icAperture.Min, icAperture.Max);
				Singleton<Studio>.Instance.sceneInfo.depthAperture = num;
				depthOfField.aperture = num;
				icAperture.value = num;
			}
		}

		private void OnClickApertureDef()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.depthAperture = ScreenEffectDefine.DepthOfFieldAperture;
				depthOfField.aperture = ScreenEffectDefine.DepthOfFieldAperture;
				icAperture.value = ScreenEffectDefine.DepthOfFieldAperture;
			}
		}
	}

	[Serializable]
	private class VignetteInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public InputCombination icIntensity = new InputCombination();

		private Vignette Vignette { get; set; }

		private FloatParameter fpIntensity { get; set; }

		private float Intensity
		{
			set
			{
				fpIntensity.value = value;
			}
		}

		public void Init(Sprite[] _sprite, Vignette _vignette)
		{
			base.Init(_sprite);
			Vignette = _vignette;
			fpIntensity = Vignette.intensity;
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			InputField input = icIntensity.input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(icIntensity.OnValidateInput));
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensity);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableVignette;
			icIntensity.value = Singleton<Studio>.Instance.sceneInfo.vignetteIntensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(Vignette == null))
			{
				Vignette.active = Singleton<Studio>.Instance.sceneInfo.enableVignette;
				Intensity = Singleton<Studio>.Instance.sceneInfo.vignetteIntensity;
			}
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableVignette = _value;
				Vignette.active = _value;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.vignetteIntensity = _value;
				Intensity = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icIntensity.Min, icIntensity.Max);
				Singleton<Studio>.Instance.sceneInfo.vignetteIntensity = num;
				Intensity = num;
				icIntensity.value = num;
			}
		}

		private void OnClickIntensity()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.vignetteIntensity = ScreenEffectDefine.VignetteIntensity;
				Intensity = ScreenEffectDefine.VignetteIntensity;
				icIntensity.value = ScreenEffectDefine.VignetteIntensity;
			}
		}
	}

	[Serializable]
	private class ScreenSpaceReflectionInfo : EffectInfo
	{
		public Toggle toggleEnable;

		private ScreenSpaceReflections ScreenSpaceReflections { get; set; }

		public void Init(Sprite[] _sprite, ScreenSpaceReflections _screenSpaceReflections)
		{
			base.Init(_sprite);
			ScreenSpaceReflections = _screenSpaceReflections;
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableSSR;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(ScreenSpaceReflections == null))
			{
				ScreenSpaceReflections.active = Singleton<Studio>.Instance.sceneInfo.enableSSR;
			}
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableSSR = _value;
				ScreenSpaceReflections.active = _value;
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

		private GameObject GameObject { get; set; }

		public void Init(Sprite[] _sprite, ReflectionProbe _reflectionProbe, GameObject _gameObject)
		{
			base.Init(_sprite);
			ReflectionProbe = _reflectionProbe;
			GameObject = _gameObject;
			dropdownCubemap.options = Singleton<Info>.Instance.dicReflectionProbeLoadInfo.Select((KeyValuePair<int, Info.LoadCommonInfo> v) => new Dropdown.OptionData(v.Value.name)).ToList();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			dropdownCubemap.onValueChanged.AddListener(OnValueChangedCubemap);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			InputField input = icIntensity.input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(icIntensity.OnValidateInput));
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensity);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableReflectionProbe;
			dropdownCubemap.value = Singleton<Studio>.Instance.sceneInfo.reflectionProbeCubemap;
			icIntensity.value = Singleton<Studio>.Instance.sceneInfo.reflectionProbeIntensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			GameObject?.SetActiveIfDifferent(Singleton<Studio>.Instance.sceneInfo.enableReflectionProbe);
			if (ReflectionProbe != null)
			{
				SetCubemap(Singleton<Studio>.Instance.sceneInfo.reflectionProbeCubemap);
				ReflectionProbe.intensity = Singleton<Studio>.Instance.sceneInfo.reflectionProbeIntensity;
			}
		}

		public void SetCubemap(int _no)
		{
			Singleton<Studio>.Instance.sceneInfo.reflectionProbeCubemap = _no;
			Info.LoadCommonInfo value = null;
			if (Singleton<Info>.Instance.dicReflectionProbeLoadInfo.TryGetValue(_no, out value))
			{
				Texture customBakedTexture = CommonLib.LoadAsset<Texture>(value.bundlePath, value.fileName);
				ReflectionProbe.customBakedTexture = customBakedTexture;
			}
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableReflectionProbe = _value;
				GameObject?.SetActiveIfDifferent(_value);
			}
		}

		private void OnValueChangedCubemap(int _value)
		{
			if (!base.isUpdateInfo)
			{
				SetCubemap(_value);
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.reflectionProbeIntensity = _value;
				ReflectionProbe.intensity = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icIntensity.Min, icIntensity.Max);
				Singleton<Studio>.Instance.sceneInfo.reflectionProbeIntensity = num;
				ReflectionProbe.intensity = num;
				icIntensity.value = num;
			}
		}

		private void OnClickIntensity()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.reflectionProbeIntensity = ScreenEffectDefine.ReflectionProbeIntensity;
				ReflectionProbe.intensity = ScreenEffectDefine.ReflectionProbeIntensity;
				icIntensity.value = ScreenEffectDefine.ReflectionProbeIntensity;
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

		public Button buttonColor;

		public InputCombination icDensity;

		private GlobalFog GlobalFog { get; set; }

		public void Init(Sprite[] _sprite, GlobalFog _fog)
		{
			base.Init(_sprite);
			GlobalFog = _fog;
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			toggleExcludeFarPixels.onValueChanged.AddListener(OnValueChangedExcludeFarPixels);
			icHeight.slider.onValueChanged.AddListener(OnValueChangedHeight);
			icHeight.input.onEndEdit.AddListener(OnEndEditHeight);
			icHeight.buttonDefault.onClick.AddListener(OnClickHeight);
			icHeightDensity.slider.onValueChanged.AddListener(OnValueChangedHeightDensity);
			InputField input = icHeightDensity.input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(icHeightDensity.OnValidateInput));
			icHeightDensity.input.onEndEdit.AddListener(OnEndEditHeightDensity);
			icHeightDensity.buttonDefault.onClick.AddListener(OnClickHeightDensity);
			buttonColor.onClick.AddListener(OnClickColor);
			icDensity.slider.onValueChanged.AddListener(OnValueChangedDensity);
			InputField input2 = icDensity.input;
			input2.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input2.onValidateInput, new InputField.OnValidateInput(icDensity.OnValidateInput));
			icDensity.input.onEndEdit.AddListener(OnEndEditDensity);
			icDensity.buttonDefault.onClick.AddListener(OnClickDensity);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableFog;
			toggleExcludeFarPixels.isOn = Singleton<Studio>.Instance.sceneInfo.fogExcludeFarPixels;
			icHeight.value = Singleton<Studio>.Instance.sceneInfo.fogHeight;
			icHeightDensity.value = Singleton<Studio>.Instance.sceneInfo.fogHeightDensity;
			buttonColor.image.color = Singleton<Studio>.Instance.sceneInfo.fogColor;
			icDensity.value = Singleton<Studio>.Instance.sceneInfo.fogDensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (GlobalFog != null)
			{
				GlobalFog.enabled = Singleton<Studio>.Instance.sceneInfo.enableFog;
				GlobalFog.excludeFarPixels = Singleton<Studio>.Instance.sceneInfo.fogExcludeFarPixels;
				GlobalFog.height = Singleton<Studio>.Instance.sceneInfo.fogHeight;
				GlobalFog.heightDensity = Singleton<Studio>.Instance.sceneInfo.fogHeightDensity;
			}
			RenderSettings.fog = Singleton<Studio>.Instance.sceneInfo.enableFog;
			RenderSettings.fogColor = Singleton<Studio>.Instance.sceneInfo.fogColor;
			RenderSettings.fogDensity = Singleton<Studio>.Instance.sceneInfo.fogDensity;
		}

		public void SetEnable(bool _value, bool _UI = true)
		{
			Singleton<Studio>.Instance.sceneInfo.enableFog = _value;
			GlobalFog.enabled = _value;
			RenderSettings.fog = _value;
			if (_UI)
			{
				toggleEnable.isOn = _value;
			}
		}

		public void SetColor(Color _color)
		{
			Singleton<Studio>.Instance.sceneInfo.fogColor = _color;
			RenderSettings.fogColor = _color;
			buttonColor.image.color = _color;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				SetEnable(_value, _UI: false);
			}
		}

		private void OnValueChangedExcludeFarPixels(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.fogExcludeFarPixels = _value;
				GlobalFog.excludeFarPixels = _value;
			}
		}

		private void OnClickColor()
		{
			if (!base.isUpdateInfo)
			{
				if (Singleton<Studio>.Instance.colorPalette.Check("フォグ"))
				{
					Singleton<Studio>.Instance.colorPalette.visible = false;
				}
				else
				{
					Singleton<Studio>.Instance.colorPalette.Setup("フォグ", Singleton<Studio>.Instance.sceneInfo.fogColor, SetColor, _useAlpha: false);
				}
			}
		}

		private void OnValueChangedHeight(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.fogHeight = _value;
				GlobalFog.height = _value;
				icHeight.value = _value;
			}
		}

		private void OnEndEditHeight(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icHeight.Min, icHeight.Max);
				Singleton<Studio>.Instance.sceneInfo.fogHeight = num;
				GlobalFog.height = num;
				icHeight.value = num;
			}
		}

		private void OnClickHeight()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.fogHeight = ScreenEffectDefine.FogHeight;
				GlobalFog.height = ScreenEffectDefine.FogHeight;
				icHeight.value = ScreenEffectDefine.FogHeight;
			}
		}

		private void OnValueChangedHeightDensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.fogHeightDensity = _value;
				GlobalFog.heightDensity = _value;
				icHeightDensity.value = _value;
			}
		}

		private void OnEndEditHeightDensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icHeightDensity.Min, icHeightDensity.Max);
				Singleton<Studio>.Instance.sceneInfo.fogHeightDensity = num;
				GlobalFog.heightDensity = num;
				icHeightDensity.value = num;
			}
		}

		private void OnClickHeightDensity()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.fogHeightDensity = ScreenEffectDefine.FogHeightDensity;
				GlobalFog.heightDensity = ScreenEffectDefine.FogHeightDensity;
				icHeightDensity.value = ScreenEffectDefine.FogHeightDensity;
			}
		}

		private void OnValueChangedDensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.fogDensity = _value;
				RenderSettings.fogDensity = _value;
				icDensity.value = _value;
			}
		}

		private void OnEndEditDensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icDensity.Min, icDensity.Max);
				Singleton<Studio>.Instance.sceneInfo.fogDensity = num;
				RenderSettings.fogDensity = num;
				icDensity.value = num;
			}
		}

		private void OnClickDensity()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.fogDensity = ScreenEffectDefine.FogDensity;
				RenderSettings.fogDensity = ScreenEffectDefine.FogDensity;
				icDensity.value = ScreenEffectDefine.FogDensity;
			}
		}
	}

	[Serializable]
	private class SunShaftsInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Selector selectorCaster;

		public Button buttonThresholdColor;

		public Button buttonShaftsColor;

		public InputCombination icDistanceFallOff;

		public InputCombination icBlurSize;

		public InputCombination icIntensity;

		private SunShafts sunShafts { get; set; }

		public void Init(Sprite[] _sprite, SunShafts _sunShafts)
		{
			base.Init(_sprite);
			sunShafts = _sunShafts;
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			selectorCaster._button.onClick.AddListener(OnClickCaster);
			buttonThresholdColor.onClick.AddListener(OnClickThresholdColor);
			buttonShaftsColor.onClick.AddListener(OnClickShaftsColor);
			icDistanceFallOff.slider.onValueChanged.AddListener(OnValueChangedDistanceFallOff);
			InputField input = icDistanceFallOff.input;
			input.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(input.onValidateInput, new InputField.OnValidateInput(icDistanceFallOff.OnValidateInput));
			icDistanceFallOff.input.onEndEdit.AddListener(OnEndEditDistanceFallOff);
			icDistanceFallOff.buttonDefault.onClick.AddListener(OnClickDistanceFallOff);
			icBlurSize.slider.onValueChanged.AddListener(OnValueChangedBlurSize);
			icBlurSize.input.onEndEdit.AddListener(OnEndEditBlurSize);
			icBlurSize.buttonDefault.onClick.AddListener(OnClickBlurSize);
			icIntensity.slider.onValueChanged.AddListener(OnValueChangedIntensity);
			icIntensity.input.onEndEdit.AddListener(OnEndEditIntensity);
			icIntensity.buttonDefault.onClick.AddListener(OnClickIntensity);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableSunShafts;
			buttonThresholdColor.image.color = Singleton<Studio>.Instance.sceneInfo.sunThresholdColor;
			buttonShaftsColor.image.color = Singleton<Studio>.Instance.sceneInfo.sunColor;
			Singleton<Studio>.Instance.SetSunCaster(Singleton<Studio>.Instance.sceneInfo.sunCaster);
			icDistanceFallOff.value = Singleton<Studio>.Instance.sceneInfo.sunDistanceFallOff;
			icBlurSize.value = Singleton<Studio>.Instance.sceneInfo.sunBlurSize;
			icIntensity.value = Singleton<Studio>.Instance.sceneInfo.sunIntensity;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			if (!(sunShafts == null))
			{
				sunShafts.enabled = Singleton<Studio>.Instance.sceneInfo.enableSunShafts;
				sunShafts.sunThreshold = Singleton<Studio>.Instance.sceneInfo.sunThresholdColor;
				sunShafts.sunColor = Singleton<Studio>.Instance.sceneInfo.sunColor;
				sunShafts.maxRadius = Singleton<Studio>.Instance.sceneInfo.sunDistanceFallOff;
				sunShafts.sunShaftBlurRadius = Singleton<Studio>.Instance.sceneInfo.sunBlurSize;
				sunShafts.sunShaftIntensity = Singleton<Studio>.Instance.sceneInfo.sunIntensity;
			}
		}

		public void SetShaftsColor(Color _color)
		{
			Singleton<Studio>.Instance.sceneInfo.sunColor = _color;
			buttonShaftsColor.image.color = _color;
			sunShafts.sunColor = _color;
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableSunShafts = _value;
				sunShafts.enabled = _value;
			}
		}

		private void OnClickThresholdColor()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (Singleton<Studio>.Instance.colorPalette.Check("サンシャフト しきい色"))
			{
				Singleton<Studio>.Instance.colorPalette.visible = false;
				return;
			}
			Singleton<Studio>.Instance.colorPalette.Setup("サンシャフト しきい色", Singleton<Studio>.Instance.sceneInfo.sunThresholdColor, delegate(Color _c)
			{
				Singleton<Studio>.Instance.sceneInfo.sunThresholdColor = _c;
				buttonThresholdColor.image.color = _c;
				sunShafts.sunThreshold = _c;
			}, _useAlpha: false);
		}

		private void OnClickShaftsColor()
		{
			if (!base.isUpdateInfo)
			{
				if (Singleton<Studio>.Instance.colorPalette.Check("サンシャフト 光の色"))
				{
					Singleton<Studio>.Instance.colorPalette.visible = false;
				}
				else
				{
					Singleton<Studio>.Instance.colorPalette.Setup("サンシャフト 光の色", Singleton<Studio>.Instance.sceneInfo.sunColor, SetShaftsColor, _useAlpha: false);
				}
			}
		}

		private void OnClickCaster()
		{
			if (!base.isUpdateInfo)
			{
				GuideObject selectObject = Singleton<GuideObjectManager>.Instance.selectObject;
				Singleton<Studio>.Instance.SetSunCaster((selectObject != null) ? selectObject.dicKey : (-1));
			}
		}

		private void OnValueChangedDistanceFallOff(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.sunDistanceFallOff = _value;
				sunShafts.maxRadius = _value;
				icDistanceFallOff.value = _value;
			}
		}

		private void OnEndEditDistanceFallOff(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icDistanceFallOff.Min, icDistanceFallOff.Max);
				Singleton<Studio>.Instance.sceneInfo.sunDistanceFallOff = num;
				sunShafts.maxRadius = num;
				icDistanceFallOff.value = num;
			}
		}

		private void OnClickDistanceFallOff()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.sunDistanceFallOff = ScreenEffectDefine.SunShaftDistanceFallOff;
				sunShafts.maxRadius = ScreenEffectDefine.SunShaftDistanceFallOff;
				icDistanceFallOff.value = ScreenEffectDefine.SunShaftDistanceFallOff;
			}
		}

		private void OnValueChangedBlurSize(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.sunBlurSize = _value;
				sunShafts.sunShaftBlurRadius = _value;
				icBlurSize.value = _value;
			}
		}

		private void OnEndEditBlurSize(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icBlurSize.Min, icBlurSize.Max);
				Singleton<Studio>.Instance.sceneInfo.sunBlurSize = num;
				sunShafts.sunShaftBlurRadius = num;
				icBlurSize.value = num;
			}
		}

		private void OnClickBlurSize()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.sunBlurSize = ScreenEffectDefine.SunShaftBlurSize;
				sunShafts.sunShaftBlurRadius = ScreenEffectDefine.SunShaftBlurSize;
				icBlurSize.value = ScreenEffectDefine.SunShaftBlurSize;
			}
		}

		private void OnValueChangedIntensity(float _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.sunIntensity = _value;
				sunShafts.sunShaftIntensity = _value;
				icIntensity.value = _value;
			}
		}

		private void OnEndEditIntensity(string _text)
		{
			if (!base.isUpdateInfo)
			{
				float num = Mathf.Clamp(Utility.StringToFloat(_text), icIntensity.Min, icIntensity.Max);
				Singleton<Studio>.Instance.sceneInfo.sunIntensity = num;
				sunShafts.sunShaftIntensity = num;
				icIntensity.value = num;
			}
		}

		private void OnClickIntensity()
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.sunIntensity = ScreenEffectDefine.SunShaftIntensity;
				sunShafts.sunShaftIntensity = ScreenEffectDefine.SunShaftIntensity;
				icIntensity.value = ScreenEffectDefine.SunShaftIntensity;
			}
		}
	}

	[Serializable]
	private class SelfShadowInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public override void Init(Sprite[] _sprite)
		{
			base.Init(_sprite);
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.enableShadow;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			Set(Singleton<Studio>.Instance.sceneInfo.enableShadow);
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.enableShadow = _value;
				Set(_value);
			}
		}

		private void Set(bool _value)
		{
			QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() / 2 * 2 + ((!_value) ? 1 : 0));
		}
	}

	[Serializable]
	private class EnvironmentLightingInfo : EffectInfo
	{
		public Button buttonSkyColor;

		public Button buttonEquator;

		public Button buttonGround;

		public override void Init(Sprite[] _sprite)
		{
			base.Init(_sprite);
			buttonSkyColor.onClick.AddListener(OnClickSkyColor);
			buttonEquator.onClick.AddListener(OnClickEquator);
			buttonGround.onClick.AddListener(OnClickGround);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			buttonSkyColor.image.color = Singleton<Studio>.Instance.sceneInfo.environmentLightingSkyColor;
			buttonEquator.image.color = Singleton<Studio>.Instance.sceneInfo.environmentLightingEquatorColor;
			buttonGround.image.color = Singleton<Studio>.Instance.sceneInfo.environmentLightingGroundColor;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			RenderSettings.ambientSkyColor = Singleton<Studio>.Instance.sceneInfo.environmentLightingSkyColor;
			RenderSettings.ambientEquatorColor = Singleton<Studio>.Instance.sceneInfo.environmentLightingEquatorColor;
			RenderSettings.ambientGroundColor = Singleton<Studio>.Instance.sceneInfo.environmentLightingGroundColor;
		}

		private void OnClickSkyColor()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (Singleton<Studio>.Instance.colorPalette.Check("空の環境光"))
			{
				Singleton<Studio>.Instance.colorPalette.visible = false;
				return;
			}
			Singleton<Studio>.Instance.colorPalette.Setup("空の環境光", Singleton<Studio>.Instance.sceneInfo.environmentLightingSkyColor, delegate(Color _c)
			{
				Singleton<Studio>.Instance.sceneInfo.environmentLightingSkyColor = _c;
				RenderSettings.ambientSkyColor = _c;
				buttonSkyColor.image.color = _c;
			}, _useAlpha: false);
		}

		private void OnClickEquator()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (Singleton<Studio>.Instance.colorPalette.Check("地平線の環境光"))
			{
				Singleton<Studio>.Instance.colorPalette.visible = false;
				return;
			}
			Singleton<Studio>.Instance.colorPalette.Setup("地平線の環境光", Singleton<Studio>.Instance.sceneInfo.environmentLightingEquatorColor, delegate(Color _c)
			{
				Singleton<Studio>.Instance.sceneInfo.environmentLightingEquatorColor = _c;
				RenderSettings.ambientEquatorColor = _c;
				buttonEquator.image.color = _c;
			}, _useAlpha: false);
		}

		private void OnClickGround()
		{
			if (base.isUpdateInfo)
			{
				return;
			}
			if (Singleton<Studio>.Instance.colorPalette.Check("地面の環境光"))
			{
				Singleton<Studio>.Instance.colorPalette.visible = false;
				return;
			}
			Singleton<Studio>.Instance.colorPalette.Setup("地面の環境光", Singleton<Studio>.Instance.sceneInfo.environmentLightingGroundColor, delegate(Color _c)
			{
				Singleton<Studio>.Instance.sceneInfo.environmentLightingGroundColor = _c;
				RenderSettings.ambientGroundColor = _c;
				buttonGround.image.color = _c;
			}, _useAlpha: false);
		}
	}

	[Serializable]
	private class SkyInfo : EffectInfo
	{
		public Toggle toggleEnable;

		public Dropdown dropdownPattern;

		[Header("Sun Source")]
		public Transform transformLight;

		public override void Init(Sprite[] _sprite)
		{
			base.Init(_sprite);
			dropdownPattern.options = Singleton<Info>.Instance.dicSkyPatternInfo.Select((KeyValuePair<int, Info.SkyPatternInfo> v) => new Dropdown.OptionData(v.Value.name)).ToList();
			toggleEnable.onValueChanged.AddListener(OnValueChangedEnable);
			dropdownPattern.onValueChanged.AddListener(OnValueChangedPattern);
		}

		public override void UpdateInfo()
		{
			base.UpdateInfo();
			base.isUpdateInfo = true;
			toggleEnable.isOn = Singleton<Studio>.Instance.sceneInfo.skyInfo.Enable;
			dropdownPattern.value = Singleton<Studio>.Instance.sceneInfo.skyInfo.Pattern;
			Apply();
			base.isUpdateInfo = false;
		}

		public override void Apply()
		{
			SceneInfo.SkyInfo skyInfo = Singleton<Studio>.Instance.sceneInfo.skyInfo;
			Singleton<Studio>.Instance.cameraCtrl.mainCmaera.clearFlags = (skyInfo.Enable ? CameraClearFlags.Skybox : CameraClearFlags.Color);
			SetPattern(skyInfo.Pattern);
		}

		public void SetPattern(int _no)
		{
			Singleton<Studio>.Instance.sceneInfo.skyInfo.Pattern = _no;
			Info.SkyPatternInfo value = null;
			if (Singleton<Info>.Instance.dicSkyPatternInfo.TryGetValue(_no, out value))
			{
				transformLight.eulerAngles = value.Rotation;
				Material material = CommonLib.LoadAsset<Material>(value.bundlePath, value.fileName, clone: true, value.manifest);
				if (material != null)
				{
					RenderSettings.skybox = material;
				}
			}
		}

		private void OnValueChangedEnable(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				Singleton<Studio>.Instance.sceneInfo.skyInfo.Enable = _value;
				Apply();
			}
		}

		private void OnValueChangedPattern(int _value)
		{
			if (!base.isUpdateInfo)
			{
				SetPattern(_value);
			}
		}
	}

	[SerializeField]
	private CommonInfo[] commonInfo = new CommonInfo[5];

	private int select = -1;

	[SerializeField]
	private Sprite spriteSave;

	[SerializeField]
	private Sprite spriteInit;

	[SerializeField]
	[Header("一括制御")]
	private PostProcessVolume postProcessVolume;

	[SerializeField]
	[Header("ColorGrading用")]
	private PostProcessVolume postProcessVolumeColor;

	[SerializeField]
	[Header("Reflection Probe制御")]
	private GameObject objReflectionProbe;

	[SerializeField]
	private ReflectionProbe reflectionProbe;

	[SerializeField]
	[Header("個別制御")]
	private UnityStandardAssets.ImageEffects.DepthOfField depthOfField;

	[SerializeField]
	private GlobalFog globalFog;

	[SerializeField]
	private SunShafts _sunShafts;

	[SerializeField]
	private Sprite[] spriteExpansion;

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

	[SerializeField]
	private SkyInfo skyInfo = new SkyInfo();

	private EffectInfo[] effectInfos;

	private bool isInit;

	public SunShafts sunShafts => _sunShafts;

	public bool visible
	{
		set
		{
			if (value)
			{
				commonInfo.SafeProc(select, delegate(CommonInfo v)
				{
					v.active = true;
				});
				return;
			}
			for (int num = 0; num < commonInfo.Length; num++)
			{
				commonInfo[num].active = false;
			}
		}
	}

	public void OnClickSelect(int _idx)
	{
		if (MathfEx.RangeEqualOn(0, select, commonInfo.Length - 1))
		{
			commonInfo[select].active = false;
			if (select == 2)
			{
				Singleton<Studio>.Instance.SaveOption();
			}
		}
		select = ((select == _idx) ? (-1) : _idx);
		if (MathfEx.RangeEqualOn(0, select, commonInfo.Length - 1))
		{
			commonInfo[select].active = true;
		}
		Singleton<Studio>.Instance.colorPalette.visible = false;
	}

	public void OnClickSave()
	{
		Singleton<Studio>.Instance.colorPalette.visible = false;
		Singleton<Studio>.Instance.SaveScene();
		NotificationScene.spriteMessage = spriteSave;
		NotificationScene.waitTime = 1f;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "StudioNotification",
			isAdd = true
		}, isLoadingImageDraw: false);
	}

	public void OnClickLoad()
	{
		Singleton<Studio>.Instance.colorPalette.visible = false;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "StudioSceneLoad",
			isAdd = true
		}, isLoadingImageDraw: false);
	}

	public void OnClickInit()
	{
		Singleton<Studio>.Instance.colorPalette.visible = false;
		CheckScene.sprite = spriteInit;
		CheckScene.unityActionYes = OnSelectInitYes;
		CheckScene.unityActionNo = OnSelectIniteNo;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "StudioCheck",
			isAdd = true
		}, isLoadingImageDraw: false);
	}

	private void OnSelectInitYes()
	{
		Scene.Unload();
		Singleton<Studio>.Instance.InitScene();
	}

	private void OnSelectIniteNo()
	{
		Scene.Unload();
	}

	public void OnClickEnd()
	{
		Singleton<Studio>.Instance.colorPalette.visible = false;
		ExitDialog.GameEnd(isCheck: false);
	}

	public void Init()
	{
		if (!isInit)
		{
			GameObject gameObject = Camera.main?.gameObject;
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
			for (int i = 0; i < commonInfo.Length; i++)
			{
				commonInfo[i].active = false;
			}
			colorGradingInfo.Init(spriteExpansion, postProcessVolume.profile.GetSetting<ColorGrading>(), postProcessVolumeColor);
			ambientOcclusionInfo.Init(spriteExpansion, postProcessVolume.profile.GetSetting<AmbientOcclusion>());
			bloomInfo.Init(spriteExpansion, postProcessVolume.profile.GetSetting<UnityEngine.Rendering.PostProcessing.Bloom>());
			dofInfo.Init(spriteExpansion, depthOfField);
			vignetteInfo.Init(spriteExpansion, postProcessVolume.profile.GetSetting<Vignette>());
			screenSpaceReflectionInfo.Init(spriteExpansion, postProcessVolume.profile.GetSetting<ScreenSpaceReflections>());
			reflectionProbeInfo.Init(spriteExpansion, reflectionProbe, objReflectionProbe);
			fogInfo.Init(spriteExpansion, globalFog);
			sunShaftsInfo.Init(spriteExpansion, sunShafts);
			selfShadowInfo.Init(spriteExpansion);
			environmentLightingInfo.Init(spriteExpansion);
			skyInfo.Init(spriteExpansion);
			isInit = true;
			effectInfos = new EffectInfo[12]
			{
				colorGradingInfo, ambientOcclusionInfo, bloomInfo, dofInfo, vignetteInfo, screenSpaceReflectionInfo, reflectionProbeInfo, fogInfo, sunShaftsInfo, selfShadowInfo,
				environmentLightingInfo, skyInfo
			};
			UpdateInfo();
		}
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

	public void SetDepthOfFieldForcus(int _key)
	{
		Transform targetObj = Singleton<Studio>.Instance.cameraCtrl.targetObj;
		string text = "注視点";
		ObjectCtrlInfo ctrlInfo = Studio.GetCtrlInfo(_key);
		if (ctrlInfo == null || ctrlInfo.kind != 1)
		{
			Singleton<Studio>.Instance.sceneInfo.depthForcus = -1;
		}
		else
		{
			Singleton<Studio>.Instance.sceneInfo.depthForcus = _key;
			targetObj = (ctrlInfo as OCIItem).objectItem.transform;
			text = (ctrlInfo as OCIItem).treeNodeObject.textName;
		}
		depthOfField.focalTransform = targetObj;
		dofInfo.selectorForcus.text = text;
	}

	public void SetSunCaster(int _key)
	{
		Transform sunTransform = null;
		string text = "なし";
		ObjectCtrlInfo ctrlInfo = Studio.GetCtrlInfo(_key);
		if (ctrlInfo == null || ctrlInfo.kind != 1)
		{
			Singleton<Studio>.Instance.sceneInfo.sunCaster = -1;
		}
		else
		{
			Singleton<Studio>.Instance.sceneInfo.sunCaster = _key;
			sunTransform = (ctrlInfo as OCIItem).objectItem.transform;
			text = (ctrlInfo as OCIItem).treeNodeObject.textName;
		}
		sunShafts.sunTransform = sunTransform;
		sunShaftsInfo.selectorCaster.text = text;
	}

	public void MapDependent()
	{
		fogInfo.SetEnable(Singleton<Studio>.Instance.sceneInfo.enableFog);
	}
}
