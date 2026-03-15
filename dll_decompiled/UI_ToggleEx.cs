using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ToggleEx : Toggle
{
	[SerializeField]
	private Image baseImageEx;

	[SerializeField]
	private Image overImage;

	[SerializeField]
	private Color selectedColor = Color.white;

	[SerializeField]
	private Text[] text;

	[SerializeField]
	private bool alpha;

	protected override void Awake()
	{
		if (text == null)
		{
			text = GetComponentsInChildren<Text>();
		}
	}

	protected override void Start()
	{
		base.Start();
		this.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
		{
			if (text != null)
			{
				Text[] array = text;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = (isOn ? selectedColor : base.colors.normalColor);
				}
			}
		});
		if (null != overImage)
		{
			overImage.enabled = false;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (null != overImage)
		{
			overImage.enabled = false;
		}
	}

	public void SetTextColor(int state)
	{
		switch (state)
		{
		case 0:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text3 in array)
				{
					text3.color = new Color(base.colors.highlightedColor.r, base.colors.highlightedColor.g, base.colors.highlightedColor.b, alpha ? base.colors.highlightedColor.a : text3.color.a);
				}
			}
			break;
		case 1:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text2 in array)
				{
					text2.color = new Color(base.colors.normalColor.r, base.colors.normalColor.g, base.colors.normalColor.b, alpha ? base.colors.normalColor.a : text2.color.a);
				}
			}
			break;
		case 2:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text4 in array)
				{
					text4.color = new Color(base.colors.pressedColor.r, base.colors.pressedColor.g, base.colors.pressedColor.b, alpha ? base.colors.pressedColor.a : text4.color.a);
				}
			}
			break;
		case 3:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text in array)
				{
					text.color = new Color(base.colors.disabledColor.r, base.colors.disabledColor.g, base.colors.disabledColor.b, alpha ? base.colors.disabledColor.a : text.color.a);
				}
			}
			break;
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (base.isOn)
		{
			return;
		}
		switch (state)
		{
		case SelectionState.Highlighted:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text3 in array)
				{
					text3.color = new Color(base.colors.highlightedColor.r, base.colors.highlightedColor.g, base.colors.highlightedColor.b, alpha ? base.colors.highlightedColor.a : text3.color.a);
				}
			}
			if (null != baseImageEx)
			{
				baseImageEx.color = base.colors.highlightedColor;
			}
			break;
		case SelectionState.Normal:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text4 in array)
				{
					text4.color = new Color(base.colors.normalColor.r, base.colors.normalColor.g, base.colors.normalColor.b, alpha ? base.colors.normalColor.a : text4.color.a);
				}
			}
			if (null != baseImageEx)
			{
				baseImageEx.color = base.colors.normalColor;
			}
			break;
		case SelectionState.Pressed:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text2 in array)
				{
					text2.color = new Color(base.colors.pressedColor.r, base.colors.pressedColor.g, base.colors.pressedColor.b, alpha ? base.colors.pressedColor.a : text2.color.a);
				}
			}
			if (null != baseImageEx)
			{
				baseImageEx.color = base.colors.pressedColor;
			}
			break;
		case SelectionState.Disabled:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text in array)
				{
					text.color = new Color(base.colors.disabledColor.r, base.colors.disabledColor.g, base.colors.disabledColor.b, alpha ? base.colors.disabledColor.a : text.color.a);
				}
			}
			if (null != baseImageEx)
			{
				baseImageEx.color = base.colors.disabledColor;
			}
			break;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (null != overImage)
		{
			overImage.enabled = base.interactable;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (null != overImage)
		{
			overImage.enabled = false;
		}
	}
}
