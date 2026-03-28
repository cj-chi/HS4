using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.ProgressBar;

[RequireComponent(typeof(Image))]
public class BarViewSizeImageFill : ProgressBarProView
{
	[SerializeField]
	protected Image image;

	[SerializeField]
	private bool hideOnEmpty = true;

	[SerializeField]
	private bool useDiscreteSteps;

	[SerializeField]
	private int numSteps = 10;

	private bool isDisplaySizeZero;

	public override bool CanUpdateView(float currentValue, float targetValue)
	{
		if (!base.isActiveAndEnabled)
		{
			return isDisplaySizeZero;
		}
		return true;
	}

	public override void UpdateView(float currentValue, float targetValue)
	{
		if (hideOnEmpty && currentValue <= 0f)
		{
			image.gameObject.SetActive(value: false);
			isDisplaySizeZero = true;
		}
		else
		{
			isDisplaySizeZero = false;
			image.gameObject.SetActive(value: true);
			image.fillAmount = GetDisplayValue(currentValue);
		}
	}

	private float GetDisplayValue(float display)
	{
		if (!useDiscreteSteps)
		{
			return display;
		}
		return Mathf.Round(display * (float)numSteps) / (float)numSteps;
	}
}
