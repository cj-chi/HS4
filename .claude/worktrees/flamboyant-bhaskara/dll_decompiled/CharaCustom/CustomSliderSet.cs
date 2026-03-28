using System;
using AIChara;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomSliderSet : MonoBehaviour
{
	public Text title;

	public Slider slider;

	public InputField input;

	public Button button;

	public Action<float> onChange;

	public Action onPointerUp;

	public Func<float> onSetDefaultValue;

	public Action onEndSetDefaultValue;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	protected ChaControl chaCtrl => customBase.chaCtrl;

	public void Reset()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			switch (child.name)
			{
			case "Text":
				title = child.GetComponent<Text>();
				break;
			case "Slider":
				slider = child.GetComponent<Slider>();
				break;
			case "SldInputField":
				input = child.GetComponent<InputField>();
				break;
			case "Button":
				button = child.GetComponent<Button>();
				break;
			}
		}
	}

	public void SetSliderValue(float value)
	{
		if ((bool)slider)
		{
			slider.value = value;
		}
	}

	public void SetInputTextValue(string value)
	{
		if ((bool)input)
		{
			input.text = value;
		}
	}

	public void Start()
	{
		customBase.lstInputField.Add(input);
		if ((bool)slider)
		{
			slider.onValueChanged.AsObservable().Subscribe(delegate(float value)
			{
				onChange?.Invoke(value);
				if ((bool)input)
				{
					input.text = CustomBase.ConvertTextFromRate(0, 100, value);
				}
			});
			slider.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
			{
				if (customBase.sliderControlWheel)
				{
					slider.value = Mathf.Clamp(slider.value + scl.scrollDelta.y * -0.01f, 0f, 100f);
				}
			});
			slider.OnPointerUpAsObservable().Subscribe(delegate
			{
				onPointerUp?.Invoke();
			});
		}
		if ((bool)input)
		{
			input.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				if ((bool)slider)
				{
					slider.value = CustomBase.ConvertRateFromText(0, 100, value);
				}
			});
		}
		if (!button)
		{
			return;
		}
		button.onClick.AsObservable().Subscribe(delegate
		{
			if (onSetDefaultValue != null)
			{
				float num = onSetDefaultValue();
				if ((bool)slider)
				{
					if ((bool)input && slider.value != num)
					{
						input.text = CustomBase.ConvertTextFromRate(0, 100, num);
					}
					slider.value = num;
				}
				onChange?.Invoke(num);
				onEndSetDefaultValue?.Invoke();
			}
		});
	}

	private void OnDestroy()
	{
		if (Singleton<CustomBase>.IsInstance())
		{
			customBase.lstInputField.Remove(input);
		}
	}
}
