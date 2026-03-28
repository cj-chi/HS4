using Illusion.Extensions;
using Studio.SceneAssist;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Studio;

public class StudioNode : PointerAction
{
	protected enum ClickSound
	{
		NoSound,
		OK
	}

	[SerializeField]
	protected Button m_Button;

	[SerializeField]
	protected Image m_ImageButton;

	[SerializeField]
	protected Text m_Text;

	[SerializeField]
	private TextMeshProUGUI _textMesh;

	[SerializeField]
	protected ClickSound clickSound;

	protected bool m_Select;

	public Button buttonUI => m_Button;

	public Image imageButton => m_ImageButton ?? (m_ImageButton = m_Button.image);

	public Text textUI => m_Text;

	public string text
	{
		get
		{
			return m_Text.text;
		}
		set
		{
			m_Text.SafeProc(delegate(Text _t)
			{
				_t.text = value;
			});
			_textMesh.SafeProc(delegate(TextMeshProUGUI _t)
			{
				_t.text = value;
			});
		}
	}

	public Color TextColor
	{
		set
		{
			m_Text.SafeProc(delegate(Text _t)
			{
				_t.color = value;
			});
			_textMesh.SafeProc(delegate(TextMeshProUGUI _t)
			{
				_t.color = value;
			});
		}
	}

	public bool select
	{
		get
		{
			return m_Select;
		}
		set
		{
			if (Utility.SetStruct(ref m_Select, value))
			{
				imageButton.color = (m_Select ? Color.green : Color.white);
			}
		}
	}

	public bool interactable
	{
		get
		{
			return m_Button.interactable;
		}
		set
		{
			m_Button.interactable = value;
		}
	}

	public bool active
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActiveIfDifferent(value);
		}
	}

	public UnityAction addOnClick
	{
		set
		{
			m_Button.onClick.AddListener(value);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (interactable)
		{
			base.OnPointerEnter(eventData);
		}
	}

	public virtual void Awake()
	{
		ClickSound clickSound = this.clickSound;
		if (clickSound == ClickSound.OK)
		{
			addOnClick = delegate
			{
				Assist.PlayDecisionSE();
			};
		}
	}
}
