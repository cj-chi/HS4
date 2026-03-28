using Illusion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ToggleOnOffEx : Toggle
{
	[SerializeField]
	private Image imgOn;

	[SerializeField]
	private Image imgOff;

	[SerializeField]
	private Image imgOnOver;

	[SerializeField]
	private Image imgOffOver;

	private void Initialize()
	{
		Transform transform = null;
		if (null == imgOn)
		{
			transform = base.transform.Find("imgOn");
			if (null != transform)
			{
				imgOn = transform.GetComponent<Image>();
			}
		}
		if (null == imgOff)
		{
			transform = base.transform.Find("imgOff");
			if (null != transform)
			{
				imgOff = transform.GetComponent<Image>();
			}
		}
		if (null == imgOnOver)
		{
			transform = base.transform.Find("imgOnOver");
			if (null != transform)
			{
				imgOnOver = transform.GetComponent<Image>();
			}
			if (null != imgOnOver)
			{
				imgOnOver.enabled = false;
			}
		}
		if (null == imgOffOver)
		{
			transform = base.transform.Find("imgOffOver");
			if (null != transform)
			{
				imgOffOver = transform.GetComponent<Image>();
			}
			if (null != imgOffOver)
			{
				imgOffOver.enabled = false;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		this.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
		{
			if (null != imgOn)
			{
				imgOn.gameObject.SetActiveIfDifferent(isOn);
			}
			if (null != imgOnOver)
			{
				imgOnOver.gameObject.SetActiveIfDifferent(isOn);
			}
			if (null != imgOff)
			{
				imgOff.gameObject.SetActiveIfDifferent(!isOn);
			}
			if (null != imgOffOver)
			{
				imgOffOver.gameObject.SetActiveIfDifferent(!isOn);
			}
		});
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (null != imgOnOver)
		{
			imgOnOver.enabled = base.interactable;
		}
		if (null != imgOffOver)
		{
			imgOffOver.enabled = base.interactable;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (null != imgOnOver)
		{
			imgOnOver.enabled = false;
		}
		if (null != imgOffOver)
		{
			imgOffOver.enabled = false;
		}
	}
}
