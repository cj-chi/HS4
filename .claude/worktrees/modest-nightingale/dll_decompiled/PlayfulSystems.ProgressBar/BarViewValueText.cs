using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.ProgressBar;

[RequireComponent(typeof(Text))]
public class BarViewValueText : ProgressBarProView
{
	[SerializeField]
	private Text text;

	[SerializeField]
	private string prefix = "";

	[SerializeField]
	private float minValue;

	[SerializeField]
	private float maxValue = 100f;

	[SerializeField]
	private int numDecimals;

	[SerializeField]
	private bool showMaxValue;

	[SerializeField]
	private string numberUnit = "%";

	[SerializeField]
	private string suffix = "";

	private float lastDisplayValue;

	public override bool CanUpdateView(float currentValue, float targetValue)
	{
		float roundedDisplayValue = GetRoundedDisplayValue(currentValue);
		if (currentValue >= 0f && Mathf.Approximately(lastDisplayValue, roundedDisplayValue))
		{
			return false;
		}
		lastDisplayValue = GetRoundedDisplayValue(currentValue);
		return true;
	}

	public override void UpdateView(float currentValue, float targetValue)
	{
		text.text = prefix + FormatNumber(GetRoundedDisplayValue(currentValue)) + numberUnit + (showMaxValue ? (" / " + FormatNumber(maxValue) + numberUnit) : "") + suffix;
	}

	private float GetDisplayValue(float num)
	{
		return Mathf.Lerp(minValue, maxValue, num);
	}

	private float GetRoundedDisplayValue(float num)
	{
		float displayValue = GetDisplayValue(num);
		if (numDecimals == 0)
		{
			return Mathf.Round(displayValue);
		}
		float num2 = Mathf.Pow(10f, numDecimals);
		return Mathf.Round(displayValue * num2) / num2;
	}

	private string FormatNumber(float num)
	{
		return num.ToString("N" + numDecimals);
	}
}
