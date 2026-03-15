using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

namespace UI;

public class UIObjectSlideOnCursor : MonoBehaviour
{
	[SerializeField]
	private PointerEnterExitAction pointerEnterExit;

	[SerializeField]
	private RectTransform rctSlide;

	[SerializeField]
	private Vector2 slide = Vector2.zero;

	[SerializeField]
	private Toggle tgl;

	[SerializeField]
	private Button btn;

	private bool isSlideAlways;

	private Vector2 oldPosition;

	public bool IsSlideAlways
	{
		get
		{
			return isSlideAlways;
		}
		set
		{
			if (isSlideAlways != value)
			{
				rctSlide.anchoredPosition = oldPosition + (value ? slide : Vector2.zero);
			}
			isSlideAlways = value;
		}
	}

	private void Start()
	{
		if ((bool)rctSlide)
		{
			oldPosition = rctSlide.anchoredPosition;
		}
		if (!pointerEnterExit)
		{
			return;
		}
		pointerEnterExit.listActionEnter.Add(delegate
		{
			if (((bool)tgl && tgl.IsInteractable()) || ((bool)btn && btn.IsInteractable()) || (tgl == null && btn == null))
			{
				rctSlide.anchoredPosition = oldPosition + slide;
			}
		});
		pointerEnterExit.listActionExit.Add(delegate
		{
			if (!isSlideAlways)
			{
				rctSlide.anchoredPosition = oldPosition;
			}
		});
	}

	public void SetSlideEnable(bool _slide)
	{
		rctSlide.anchoredPosition = oldPosition + (_slide ? slide : Vector2.zero);
	}
}
