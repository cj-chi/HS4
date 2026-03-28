using Studio.SceneAssist;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Studio;

public class ThumbnailNode : PointerAction
{
	protected enum ClickSound
	{
		NoSound,
		OK
	}

	[SerializeField]
	protected Button m_Button;

	[SerializeField]
	private RawImage m_Image;

	[SerializeField]
	protected ClickSound clickSound;

	public Button button => m_Button;

	public RawImage image => m_Image;

	public Texture texture
	{
		get
		{
			return m_Image.texture;
		}
		set
		{
			m_Image.texture = value;
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
			m_Image.color = (value ? Color.white : Color.clear);
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
