using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Illusion.Component.UI.ColorPicker;

public class PickerSliderInput : PickerSlider
{
	[Tooltip("RedInputField")]
	[SerializeField]
	private InputField inputR;

	[Tooltip("GreenInputField")]
	[SerializeField]
	private InputField inputG;

	[Tooltip("BlueInputField")]
	[SerializeField]
	private InputField inputB;

	[Tooltip("AlphaInputField")]
	[SerializeField]
	private InputField inputA;

	[Tooltip("R or H")]
	[SerializeField]
	private Text textR;

	[Tooltip("G or S")]
	[SerializeField]
	private Text textG;

	[Tooltip("B or V")]
	[SerializeField]
	private Text textB;

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
		if (base.isHSV)
		{
			inputR.text = ConvertTextFromValue(0, 360, base.color.r);
			inputG.text = ConvertTextFromValue(0, 100, base.color.g);
			inputB.text = ConvertTextFromValue(0, 100, base.color.b);
			inputA.text = ConvertTextFromValue(0, 100, base.color.a);
		}
		else
		{
			inputR.text = ConvertTextFromValue(0, 255, base.color.r);
			inputG.text = ConvertTextFromValue(0, 255, base.color.g);
			inputB.text = ConvertTextFromValue(0, 255, base.color.b);
			inputA.text = ConvertTextFromValue(0, 100, base.color.a);
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
			if (!isOn)
			{
				inputR.text = ConvertTextFromValue(0, 255, base.color.r);
			}
		});
	}
}
