using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.ProgressBar;

[RequireComponent(typeof(Image))]
public class BarViewSizeAnchorsShadow : BarViewSizeAnchors
{
	public enum ShadowType
	{
		Gaining,
		Losing
	}

	[SerializeField]
	private ShadowType shadowType;

	public override void UpdateView(float currentValue, float targetValue)
	{
		if (Mathf.Approximately(currentValue, targetValue) || (shadowType == ShadowType.Gaining && targetValue < currentValue) || (shadowType == ShadowType.Losing && targetValue > currentValue))
		{
			rectTrans.gameObject.SetActive(value: false);
			isDisplaySizeZero = true;
			return;
		}
		isDisplaySizeZero = false;
		rectTrans.gameObject.SetActive(value: true);
		if (shadowType == ShadowType.Gaining)
		{
			SetPivot(0f, targetValue);
		}
		else
		{
			SetPivot(targetValue, currentValue);
		}
	}
}
