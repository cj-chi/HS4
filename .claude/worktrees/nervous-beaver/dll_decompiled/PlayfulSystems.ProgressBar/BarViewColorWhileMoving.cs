using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.ProgressBar;

[RequireComponent(typeof(Graphic))]
public class BarViewColorWhileMoving : ProgressBarProView
{
	[SerializeField]
	protected Graphic graphic;

	[SerializeField]
	private Color colorStatic = Color.white;

	[SerializeField]
	private Color colorMoving = Color.blue;

	[SerializeField]
	private float blendTimeOnMove = 0.2f;

	[SerializeField]
	private float blendTimeOnStop = 0.05f;

	private bool isMoving;

	private void OnEnable()
	{
		SetDefaultColor();
	}

	public override void UpdateView(float currentValue, float targetValue)
	{
		bool flag = currentValue != targetValue;
		if (isMoving != flag)
		{
			isMoving = flag;
			graphic.CrossFadeColor(GetCurrentColor(), isMoving ? blendTimeOnMove : blendTimeOnStop, ignoreTimeScale: false, useAlpha: true);
		}
	}

	private Color GetCurrentColor()
	{
		if (!isMoving)
		{
			return colorStatic;
		}
		return colorMoving;
	}

	private void SetDefaultColor()
	{
		graphic.canvasRenderer.SetColor(GetCurrentColor());
	}
}
