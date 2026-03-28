using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Studio.Anime;

public class ListNode : PointerAction
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private TextMeshProUGUI textMesh;

	[SerializeField]
	private TextMeshSlideEffect slideEffect;

	private Image imageButton;

	public TextMeshProUGUI TextMeshUGUI => textMesh;

	private Image ImageButton => imageButton ?? (imageButton = button.image);

	public bool Interactable
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

	public bool Select
	{
		set
		{
			ImageButton.SafeProc(delegate(Image _i)
			{
				_i.color = (value ? Color.green : Color.white);
			});
		}
	}

	public string Text
	{
		get
		{
			if (!(textMesh != null))
			{
				return "";
			}
			return textMesh.text;
		}
		set
		{
			textMesh.SafeProc(delegate(TextMeshProUGUI _t)
			{
				_t.text = value;
				if (UseSlide)
				{
					slideEffect?.OnChangedText();
				}
			});
		}
	}

	public Color TextColor
	{
		set
		{
			textMesh.SafeProc(delegate(TextMeshProUGUI _t)
			{
				_t.color = value;
			});
		}
	}

	public Material TextMeshMaterial
	{
		set
		{
			textMesh.fontSharedMaterial = value;
		}
	}

	public bool UseSlide { get; set; } = true;

	public void SetButtonAction(UnityAction _action)
	{
		if (button == null)
		{
			return;
		}
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(_action);
		button.onClick.AddListener(delegate
		{
			if (UseSlide && Studio.optionSystem.autoHide)
			{
				slideEffect?.Stop();
			}
		});
	}
}
