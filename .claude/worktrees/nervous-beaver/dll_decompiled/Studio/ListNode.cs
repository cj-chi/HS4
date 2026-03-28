using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Studio;

public class ListNode : PointerAction
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Image imageSelect;

	[SerializeField]
	private Text content;

	[SerializeField]
	private TextMeshProUGUI textMesh;

	public bool interactable
	{
		get
		{
			if (!(button != null))
			{
				return false;
			}
			return button.interactable;
		}
		set
		{
			button.SafeProc(delegate(Button _b)
			{
				_b.interactable = value;
			});
		}
	}

	public bool select
	{
		get
		{
			if (!(imageSelect != null))
			{
				return false;
			}
			return imageSelect.enabled;
		}
		set
		{
			imageSelect.SafeProc(delegate(Image _i)
			{
				_i.enabled = value;
			});
		}
	}

	public Image image
	{
		get
		{
			if (!(button != null))
			{
				return null;
			}
			return button.image;
		}
	}

	public Sprite selectSprite
	{
		set
		{
			imageSelect.SafeProc(delegate(Image _i)
			{
				_i.sprite = value;
			});
		}
	}

	public string text
	{
		get
		{
			if (!(content != null))
			{
				if (!(textMesh != null))
				{
					return "";
				}
				return textMesh.text;
			}
			return content.text;
		}
		set
		{
			content.SafeProc(delegate(Text _t)
			{
				_t.text = value;
			});
			textMesh.SafeProc(delegate(TextMeshProUGUI _t)
			{
				_t.text = value;
			});
		}
	}

	private void SetCoverEnabled(bool _enabled)
	{
		if (button != null)
		{
			_ = button.interactable;
		}
	}

	private void PlaySelectSE()
	{
		if (button != null)
		{
			_ = button.interactable;
		}
	}

	public void AddActionToButton(UnityAction _action)
	{
		button?.onClick.AddListener(_action);
	}

	public void SetButtonAction(UnityAction _action)
	{
		if (!(button == null))
		{
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(_action);
		}
	}

	private void Awake()
	{
		listEnterAction.Add(delegate
		{
			SetCoverEnabled(_enabled: true);
		});
		listEnterAction.Add(PlaySelectSE);
		listExitAction.Add(delegate
		{
			SetCoverEnabled(_enabled: false);
		});
	}
}
