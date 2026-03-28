using System;
using Illusion.Component.UI.ColorPicker;
using UnityEngine;
using UnityEngine.UI;

public class UI_SampleColor : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private PickerRect rect;

	[SerializeField]
	private PickerSliderInput slider;

	[SerializeField]
	private UI_ColorPresets preset;

	private bool callUpdate;

	public Action<Color> actUpdateColor;

	private void Start()
	{
		if (!(null != image))
		{
			return;
		}
		if (null != rect)
		{
			rect.SetColor(image.color);
			rect.updateColorAction += delegate(Color color)
			{
				UpdateRectColor(color);
			};
		}
		if (null != slider)
		{
			slider.color = image.color;
			slider.SetInputText();
			slider.updateColorAction += delegate(Color color)
			{
				UpdateSliderColor(color);
			};
		}
		if (null != preset)
		{
			preset.color = image.color;
			preset.updateColorAction += delegate(Color color)
			{
				UpdatePresetsColor(color);
			};
		}
	}

	public void SetColor(Color color)
	{
		callUpdate = true;
		image.color = color;
		if (null != rect)
		{
			rect.SetColor(color);
		}
		if (null != slider)
		{
			slider.color = color;
		}
		if (null != preset)
		{
			preset.color = color;
		}
		callUpdate = false;
	}

	public void UpdateRectColor(Color color)
	{
		if (!callUpdate)
		{
			callUpdate = true;
			image.color = color;
			if (null != slider)
			{
				slider.color = color;
			}
			if (null != preset)
			{
				preset.color = color;
			}
			actUpdateColor?.Invoke(color);
			callUpdate = false;
		}
	}

	public void UpdateSliderColor(Color color)
	{
		if (!callUpdate)
		{
			callUpdate = true;
			image.color = color;
			if (null != rect)
			{
				rect.SetColor(color);
			}
			if (null != preset)
			{
				preset.color = color;
			}
			actUpdateColor?.Invoke(color);
			callUpdate = false;
		}
	}

	public void UpdatePresetsColor(Color color)
	{
		if (!callUpdate)
		{
			callUpdate = true;
			image.color = color;
			if (null != rect)
			{
				rect.SetColor(color);
			}
			if (null != slider)
			{
				slider.color = color;
			}
			actUpdateColor?.Invoke(color);
			callUpdate = false;
		}
	}
}
