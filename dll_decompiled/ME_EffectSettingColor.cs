using UnityEngine;

public class ME_EffectSettingColor : MonoBehaviour
{
	public Color Color = Color.red;

	private Color previousColor;

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		if (previousColor != Color)
		{
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
		float h = ME_ColorHelper.ColorToHSV(Color).H;
		ME_ColorHelper.ChangeObjectColorByHUE(base.gameObject, h);
		previousColor = Color;
	}
}
