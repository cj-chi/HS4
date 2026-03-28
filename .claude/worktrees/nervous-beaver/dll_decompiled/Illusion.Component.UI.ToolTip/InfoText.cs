using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Illusion.Component.UI.ToolTip;

public class InfoText : UIBehaviour
{
	[SerializeField]
	private Text text;

	[SerializeField]
	private bool rectCheck = true;

	private RectTransform _rectTransform;

	public string data
	{
		get
		{
			if (text == null)
			{
				return null;
			}
			if (rectCheck && isWidth && isHeight)
			{
				return null;
			}
			return text.text;
		}
	}

	private RectTransform rectTransform => this.GetComponentCache(ref _rectTransform);

	private bool isWidth => text.preferredWidth <= rectTransform.rect.width;

	private bool isHeight => text.preferredHeight <= rectTransform.rect.height;

	[ContextMenu("Setup")]
	private void Setup()
	{
		text = GetComponentInChildren<Text>();
	}
}
