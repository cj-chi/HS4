using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_OnMouseOverMessage : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public bool active = true;

	public Image imgComment;

	public Text txtComment;

	public string comment = "";

	private void Start()
	{
		if (null != imgComment)
		{
			imgComment.enabled = false;
		}
		if (null != txtComment)
		{
			txtComment.enabled = false;
		}
	}

	public void OnPointerEnter(PointerEventData ped)
	{
		if (active)
		{
			if (null != imgComment)
			{
				imgComment.enabled = true;
			}
			if (null != txtComment)
			{
				txtComment.enabled = true;
				txtComment.text = comment;
			}
		}
		else
		{
			if (null != imgComment)
			{
				imgComment.enabled = false;
			}
			if (null != txtComment)
			{
				txtComment.enabled = false;
			}
		}
	}

	public void OnPointerExit(PointerEventData ped)
	{
		if (null != imgComment)
		{
			imgComment.enabled = false;
		}
		if (null != txtComment)
		{
			txtComment.enabled = false;
		}
	}

	private void Update()
	{
	}
}
