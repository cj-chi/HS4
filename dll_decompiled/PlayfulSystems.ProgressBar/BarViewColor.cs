using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.ProgressBar;

[RequireComponent(typeof(Graphic))]
public class BarViewColor : ProgressBarProView
{
	[SerializeField]
	protected Graphic graphic;

	[Header("Color Options")]
	[Tooltip("The default color of the bar can be set by the ProgressBar.SetbarColor()")]
	[SerializeField]
	private bool canOverrideColor;

	[SerializeField]
	private Color defaultColor = Color.white;

	[Tooltip("Change color of the bar automatically based on it's value.")]
	[SerializeField]
	private bool useGradient;

	[SerializeField]
	private Gradient barGradient;

	private Color flashColor;

	private float flashcolorAlpha;

	private float currentValue;

	[Header("Color Animation")]
	[SerializeField]
	private bool flashOnGain;

	[SerializeField]
	private Color gainColor = Color.white;

	[SerializeField]
	private bool flashOnLoss;

	[SerializeField]
	private Color lossColor = Color.white;

	[SerializeField]
	private float flashTime = 0.2f;

	private Coroutine colorAnim;

	private void OnEnable()
	{
		flashcolorAlpha = 0f;
		UpdateColor();
	}

	public override void NewChangeStarted(float currentValue, float targetValue)
	{
		if ((flashOnGain || flashOnLoss) && (!(targetValue > currentValue) || flashOnGain) && (!(targetValue < currentValue) || flashOnLoss) && base.gameObject.activeInHierarchy)
		{
			if (colorAnim != null)
			{
				StopCoroutine(colorAnim);
			}
			colorAnim = StartCoroutine(DoBarColorAnim((targetValue < currentValue) ? lossColor : gainColor, flashTime));
		}
	}

	private IEnumerator DoBarColorAnim(Color targetColor, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			SetOverrideColor(targetColor, Utils.EaseSinInOut(time / duration, 1f, -1f));
			UpdateColor();
			time += Time.deltaTime;
			yield return null;
		}
		SetOverrideColor(targetColor, 0f);
		UpdateColor();
		colorAnim = null;
	}

	public override void SetBarColor(Color color)
	{
		if (canOverrideColor)
		{
			defaultColor = color;
			useGradient = false;
			if (colorAnim == null)
			{
				UpdateColor();
			}
		}
	}

	private void SetOverrideColor(Color color, float alpha)
	{
		flashColor = color;
		flashcolorAlpha = alpha;
	}

	public override void UpdateView(float currentValue, float targetValue)
	{
		this.currentValue = currentValue;
		if (colorAnim == null)
		{
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
		graphic.canvasRenderer.SetColor(GetCurrentColor(currentValue));
	}

	private Color GetCurrentColor(float percentage)
	{
		if (flashcolorAlpha >= 1f)
		{
			return flashColor;
		}
		if (flashcolorAlpha <= 0f)
		{
			if (!useGradient)
			{
				return defaultColor;
			}
			return barGradient.Evaluate(percentage);
		}
		return Color.Lerp(useGradient ? barGradient.Evaluate(percentage) : defaultColor, flashColor, flashcolorAlpha);
	}
}
