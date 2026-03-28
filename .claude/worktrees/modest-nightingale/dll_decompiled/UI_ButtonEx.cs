using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ButtonEx : Button
{
	[SerializeField]
	private Image overImage;

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

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
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
			break;
		case SelectionState.Normal:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text2 in array)
				{
					text2.color = new Color(base.colors.normalColor.r, base.colors.normalColor.g, base.colors.normalColor.b, alpha ? base.colors.normalColor.a : text2.color.a);
				}
			}
			break;
		case SelectionState.Pressed:
			if (this.text != null)
			{
				Text[] array = this.text;
				foreach (Text text4 in array)
				{
					text4.color = new Color(base.colors.pressedColor.r, base.colors.pressedColor.g, base.colors.pressedColor.b, alpha ? base.colors.pressedColor.a : text4.color.a);
				}
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
