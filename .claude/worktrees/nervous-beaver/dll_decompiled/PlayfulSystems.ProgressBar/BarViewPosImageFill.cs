using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.ProgressBar;

[RequireComponent(typeof(RectTransform))]
public class BarViewPosImageFill : ProgressBarProView
{
	[SerializeField]
	private RectTransform rectTrans;

	[SerializeField]
	private Image referenceImage;

	[Range(-1f, 1f)]
	[SerializeField]
	private float offset;

	public override void UpdateView(float currentValue, float targetValue)
	{
		rectTrans.anchorMin = GetAnchor(currentValue);
		rectTrans.anchorMax = GetAnchor(currentValue);
	}

	private Vector2 GetAnchor(float currentDisplay)
	{
		return referenceImage.fillMethod switch
		{
			Image.FillMethod.Horizontal => GetAnchorHorizontal(currentDisplay, referenceImage.fillOrigin), 
			Image.FillMethod.Vertical => GetAnchorVertical(currentDisplay, referenceImage.fillOrigin), 
			Image.FillMethod.Radial360 => GetAnchorRadial360(currentDisplay, referenceImage.fillOrigin, referenceImage.fillClockwise), 
			_ => Vector2.one * 0.5f, 
		};
	}

	private Vector2 GetAnchorHorizontal(float fillAmount, int fillOrigin)
	{
		float x = ((fillOrigin != 1) ? fillAmount : (1f - fillAmount));
		return new Vector2(x, 0.5f + 0.5f * offset);
	}

	private Vector2 GetAnchorVertical(float fillAmount, int fillOrigin)
	{
		float y = ((fillOrigin != 1) ? fillAmount : (1f - fillAmount));
		return new Vector2(0.5f + 0.5f * offset, y);
	}

	private Vector2 GetAnchorRadial360(float fillAmount, int fillOrigin, bool fillClockwise)
	{
		float num = 360f * (fillClockwise ? (0f - fillAmount) : fillAmount);
		switch (fillOrigin)
		{
		case 0:
			num += (fillClockwise ? (-90f) : 90f);
			break;
		case 3:
			num += (fillClockwise ? (-180f) : 180f);
			break;
		case 2:
			num += (fillClockwise ? (-270f) : 270f);
			break;
		}
		return GetPointOnCircle(num);
	}

	private Vector2 GetPointOnCircle(float degrees)
	{
		float num = 0.25f + 0.25f * offset;
		return new Vector2(0.5f + num * Mathf.Cos((float)Math.PI / 180f * degrees), 0.5f + num * Mathf.Sin((float)Math.PI / 180f * degrees));
	}
}
