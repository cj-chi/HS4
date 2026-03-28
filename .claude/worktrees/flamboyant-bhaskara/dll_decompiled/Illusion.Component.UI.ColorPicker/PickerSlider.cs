using System;
using System.Linq;
using Illusion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Illusion.Component.UI.ColorPicker;

public class PickerSlider : MonoBehaviour
{
	[Tooltip("RedSlider")]
	[SerializeField]
	protected Slider sliderR;

	[Tooltip("GreenSlider")]
	[SerializeField]
	protected Slider sliderG;

	[Tooltip("BlueSlider")]
	[SerializeField]
	protected Slider sliderB;

	[Tooltip("AlphaSlider")]
	[SerializeField]
	protected Slider sliderA;

	public BoolReactiveProperty useAlpha = new BoolReactiveProperty(initialValue: true);

	[SerializeField]
	protected BoolReactiveProperty _isHSV = new BoolReactiveProperty(initialValue: false);

	private Slider[] sliders;

	private ImagePack[] imgPack;

	private Color _color = Color.white;

	public Color color
	{
		get
		{
			if (isHSV)
			{
				return _color.HSVToRGB();
			}
			return _color;
		}
		set
		{
			_color = ((!isHSV) ? value : value.RGBToHSV());
			SetColor(_color);
		}
	}

	public bool isHSV
	{
		get
		{
			return _isHSV.Value;
		}
		set
		{
			_isHSV.Value = value;
		}
	}

	public event Action<Color> updateColorAction;

	public void ChangeSliderColor()
	{
		for (int i = 0; i < 3; i++)
		{
			ChangeSliderColor(i);
		}
	}

	public void ChangeSliderColor(int index)
	{
		ImagePack imagePack = imgPack[index];
		if (imagePack == null || !imagePack.isTex)
		{
			return;
		}
		Vector2 size = imagePack.size;
		int num = (int)size.x;
		int num2 = (int)size.y;
		Color[] colors = new Color[num2 * num];
		float[] val = new float[3]
		{
			_color[0],
			_color[1],
			_color[2]
		};
		Action<int> action;
		if (!isHSV)
		{
			action = delegate(int i)
			{
				colors[i] = new Color(val[0], val[1], val[2]);
			};
		}
		else
		{
			if (index == 0)
			{
				val[1] = 1f;
				val[2] = 1f;
			}
			action = delegate(int i)
			{
				colors[i] = Color.HSVToRGB(val[0], val[1], val[2]);
			};
		}
		if (num2 > num)
		{
			for (int num3 = 0; num3 < num2; num3++)
			{
				for (int num4 = 0; num4 < num; num4++)
				{
					val[index] = Mathf.InverseLerp(0f, size.y, num3);
					action(num3 * num + num4);
				}
			}
		}
		else
		{
			for (int num5 = 0; num5 < num2; num5++)
			{
				for (int num6 = 0; num6 < num; num6++)
				{
					val[index] = Mathf.InverseLerp(0f, size.x, num6);
					action(num5 * num + num6);
				}
			}
		}
		imagePack.SetPixels(colors);
	}

	public void CalcSliderValue()
	{
		for (int i = 0; i < 3; i++)
		{
			if (!(sliders[i] == null))
			{
				sliders[i].value = _color[i];
			}
		}
		if (sliderA != null)
		{
			sliderA.value = _color.a;
		}
	}

	public virtual void SetColor(Color color)
	{
		_color = color;
		ChangeSliderColor();
		CalcSliderValue();
		this.updateColorAction?.Invoke(this.color);
	}

	protected virtual void Awake()
	{
		sliders = new Slider[3] { sliderR, sliderG, sliderB };
		imgPack = new ImagePack[sliders.Length];
		for (int i = 0; i < sliders.Length; i++)
		{
			Slider slider = sliders[i];
			if (!(slider == null))
			{
				imgPack[i] = new ImagePack(slider.GetOrAddComponent<Image>());
			}
		}
	}

	protected virtual void Start()
	{
		_isHSV.TakeUntilDestroy(this).Subscribe(delegate(bool isOn)
		{
			SetColor((!isOn) ? _color.HSVToRGB() : _color.RGBToHSV());
		});
		sliders.Select((Slider p, int index) => new
		{
			slider = p,
			index = index
		}).ToList().ForEach(p =>
		{
			p.slider.onValueChanged.AsObservable().Subscribe(delegate
			{
				_color[p.index] = p.slider.value;
				SetColor(_color);
			});
		});
		if (sliderA != null)
		{
			useAlpha.TakeUntilDestroy(this).Subscribe(sliderA.gameObject.SetActive);
			sliderA.onValueChanged.AsObservable().Subscribe(delegate(float value)
			{
				_color.a = value;
				this.updateColorAction?.Invoke(color);
			});
		}
	}
}
