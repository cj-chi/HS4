using Illusion.Component.UI.ColorPicker;
using TMPro;
using UniRx;
using UnityEngine;

namespace Studio;

public class PickerSliderInput : PickerSlider
{
	[Tooltip("RedInputField")]
	[SerializeField]
	private TMP_InputField inputR;

	[Tooltip("GreenInputField")]
	[SerializeField]
	private TMP_InputField inputG;

	[Tooltip("BlueInputField")]
	[SerializeField]
	private TMP_InputField inputB;

	[Tooltip("AlphaInputField")]
	[SerializeField]
	private TMP_InputField inputA;

	[Tooltip("R or H")]
	[SerializeField]
	private TextMeshProUGUI textR;

	[Tooltip("G or S")]
	[SerializeField]
	private TextMeshProUGUI textG;

	[Tooltip("B or V")]
	[SerializeField]
	private TextMeshProUGUI textB;

	public string ConvertTextFromValue(int min, int max, float value)
	{
		return ((int)Mathf.Lerp(min, max, value)).ToString();
	}

	public float ConvertValueFromText(int min, int max, string buf)
	{
		if (buf.IsNullOrEmpty())
		{
			return 0f;
		}
		if (!int.TryParse(buf, out var result))
		{
			return 0f;
		}
		return Mathf.InverseLerp(min, max, result);
	}

	public void SetInputText()
	{
		float[] array = new float[4] { sliderR.value, sliderG.value, sliderB.value, sliderA.value };
		int num = 0;
		if (base.isHSV)
		{
			inputR.text = ConvertTextFromValue(0, 360, array[num++]);
			inputG.text = ConvertTextFromValue(0, 100, array[num++]);
			inputB.text = ConvertTextFromValue(0, 100, array[num++]);
			inputA.text = ConvertTextFromValue(0, 100, array[num++]);
		}
		else
		{
			inputR.text = ConvertTextFromValue(0, 255, array[num++]);
			inputG.text = ConvertTextFromValue(0, 255, array[num++]);
			inputB.text = ConvertTextFromValue(0, 255, array[num++]);
			inputA.text = ConvertTextFromValue(0, 100, array[num++]);
		}
	}

	protected override void Start()
	{
		base.Start();
		sliderR.onValueChanged.AsObservable().Subscribe(delegate(float value)
		{
			if (base.isHSV)
			{
				inputR.text = ConvertTextFromValue(0, 360, value);
			}
			else
			{
				inputR.text = ConvertTextFromValue(0, 255, value);
			}
		});
		inputR.onEndEdit.AddListener(delegate(string s)
		{
			if (base.isHSV)
			{
				sliderR.value = ConvertValueFromText(0, 360, s);
			}
			else
			{
				sliderR.value = ConvertValueFromText(0, 255, s);
			}
		});
		sliderG.onValueChanged.AsObservable().Subscribe(delegate(float value)
		{
			if (base.isHSV)
			{
				inputG.text = ConvertTextFromValue(0, 100, value);
			}
			else
			{
				inputG.text = ConvertTextFromValue(0, 255, value);
			}
		});
		inputG.onEndEdit.AddListener(delegate(string s)
		{
			if (base.isHSV)
			{
				sliderG.value = ConvertValueFromText(0, 100, s);
			}
			else
			{
				sliderG.value = ConvertValueFromText(0, 255, s);
			}
		});
		sliderB.onValueChanged.AsObservable().Subscribe(delegate(float value)
		{
			if (base.isHSV)
			{
				inputB.text = ConvertTextFromValue(0, 100, value);
			}
			else
			{
				inputB.text = ConvertTextFromValue(0, 255, value);
			}
		});
		inputB.onEndEdit.AddListener(delegate(string s)
		{
			if (base.isHSV)
			{
				sliderB.value = ConvertValueFromText(0, 100, s);
			}
			else
			{
				sliderB.value = ConvertValueFromText(0, 255, s);
			}
		});
		sliderA.onValueChanged.AsObservable().Subscribe(delegate(float value)
		{
			if (base.isHSV)
			{
				inputA.text = ConvertTextFromValue(0, 100, value);
			}
			else
			{
				inputA.text = ConvertTextFromValue(0, 100, value);
			}
		});
		inputA.onEndEdit.AddListener(delegate(string s)
		{
			if (base.isHSV)
			{
				sliderA.value = ConvertValueFromText(0, 100, s);
			}
			else
			{
				sliderA.value = ConvertValueFromText(0, 100, s);
			}
		});
		_isHSV.TakeUntilDestroy(this).Subscribe(delegate(bool isOn)
		{
			float[] array = new float[3] { sliderR.value, sliderG.value, sliderB.value };
			int num = 0;
			if (isOn)
			{
				textR.text = "色合い";
				textG.text = "鮮やかさ";
				textB.text = "明るさ";
				inputR.text = ConvertTextFromValue(0, 360, array[num++]);
				inputG.text = ConvertTextFromValue(0, 100, array[num++]);
				inputB.text = ConvertTextFromValue(0, 100, array[num++]);
			}
			else
			{
				textR.text = "赤";
				textG.text = "緑";
				textB.text = "青";
				inputR.text = ConvertTextFromValue(0, 255, array[num++]);
				inputG.text = ConvertTextFromValue(0, 255, array[num++]);
				inputB.text = ConvertTextFromValue(0, 255, array[num++]);
			}
		});
	}
}
