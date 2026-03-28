using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

namespace UI;

public class UIObjectScaleOnCursor : MonoBehaviour
{
	[SerializeField]
	private PointerEnterExitAction pointerEnterExit;

	[SerializeField]
	private RectTransform rctScale;

	[SerializeField]
	private Vector3 addScale = Vector3.zero;

	[Header("Interactable判定")]
	[SerializeField]
	private Toggle tgl;

	[SerializeField]
	private Button btn;

	private bool isScaleAlways;

	private Vector3 oldScale;

	public bool IsScaleAlways
	{
		get
		{
			return isScaleAlways;
		}
		set
		{
			if (isScaleAlways != value)
			{
				rctScale.localScale = oldScale + (value ? addScale : Vector3.zero);
			}
			isScaleAlways = value;
		}
	}

	private void Start()
	{
		if ((bool)rctScale)
		{
			oldScale = rctScale.localScale;
		}
		if (!pointerEnterExit)
		{
			return;
		}
		pointerEnterExit.listActionEnter.Add(delegate
		{
			if (((bool)tgl && tgl.IsInteractable()) || ((bool)btn && btn.IsInteractable()))
			{
				rctScale.localScale = oldScale + addScale;
			}
		});
		pointerEnterExit.listActionExit.Add(delegate
		{
			if (!isScaleAlways)
			{
				rctScale.localScale = oldScale;
			}
		});
	}

	public void SetScaleEnable(bool _scale)
	{
		rctScale.localScale = oldScale + (_scale ? addScale : Vector3.zero);
	}
}
