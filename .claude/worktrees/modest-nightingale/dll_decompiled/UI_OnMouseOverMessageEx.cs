using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_OnMouseOverMessageEx : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image imgMessage;

	[SerializeField]
	private Text textMessage;

	[SerializeField]
	private string[] strMessage;

	public int showMsgNo;

	private void Start()
	{
		if (null != imgMessage)
		{
			imgMessage.enabled = false;
		}
		if (null != textMessage)
		{
			textMessage.enabled = false;
		}
	}

	public void ChangeMessage(params string[] msg)
	{
		strMessage = msg;
	}

	public void OnPointerEnter(PointerEventData ped)
	{
		if (null != imgMessage)
		{
			imgMessage.enabled = true;
		}
		if (null != textMessage)
		{
			textMessage.enabled = true;
			if (strMessage != null && showMsgNo < strMessage.Length && strMessage[showMsgNo] != null)
			{
				textMessage.text = strMessage[showMsgNo];
			}
		}
	}

	public void OnPointerExit(PointerEventData ped)
	{
		if (null != imgMessage)
		{
			imgMessage.enabled = false;
		}
		if (null != textMessage)
		{
			textMessage.enabled = false;
		}
	}
}
