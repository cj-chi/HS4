using CharaCustom;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HS2;

public class SearchUIDrag : CustomCanvasSort
{
	public enum CanvasType
	{
		SubWin,
		DrawWin,
		PatternWin,
		ColorWin
	}

	[SerializeField]
	private CanvasType type = CanvasType.ColorWin;

	[SerializeField]
	private RectTransform rtMove;

	[SerializeField]
	private RectTransform rtRect;

	[SerializeField]
	private RectTransform rtRange;

	private CameraControl_Ver2 camCtrl;

	private bool isDrag;

	private SearchBase searchBase => Singleton<SearchBase>.Instance;

	private void Start()
	{
		if (camCtrl == null && (bool)Camera.main)
		{
			camCtrl = Camera.main.GetComponent<CameraControl_Ver2>();
		}
		UpdatePosition();
	}

	public void UpdatePosition()
	{
		switch (type)
		{
		case CanvasType.SubWin:
			rtMove.anchoredPosition = searchBase.customSettingSave.winSubLayout;
			break;
		case CanvasType.DrawWin:
			rtMove.anchoredPosition = searchBase.customSettingSave.winDrawLayout;
			break;
		case CanvasType.PatternWin:
			rtMove.anchoredPosition = searchBase.customSettingSave.winPatternLayout;
			break;
		case CanvasType.ColorWin:
			rtMove.anchoredPosition = searchBase.customSettingSave.winColorLayout;
			break;
		}
	}

	private void CalcDragPos(PointerEventData ped)
	{
		Vector2 vector = new Vector2(rtMove.anchoredPosition.x + ped.delta.x, rtMove.anchoredPosition.y + ped.delta.y);
		vector.x = Mathf.Clamp(vector.x, 0f, rtRange.sizeDelta.x - rtRect.sizeDelta.x);
		vector.y = 0f - Mathf.Clamp(0f - vector.y, 0f, rtRange.sizeDelta.y - rtRect.sizeDelta.y);
		rtMove.anchoredPosition = vector;
		switch (type)
		{
		case CanvasType.SubWin:
			searchBase.customSettingSave.winSubLayout = vector;
			break;
		case CanvasType.DrawWin:
			searchBase.customSettingSave.winDrawLayout = vector;
			break;
		case CanvasType.PatternWin:
			searchBase.customSettingSave.winPatternLayout = vector;
			break;
		case CanvasType.ColorWin:
			searchBase.customSettingSave.winColorLayout = vector;
			break;
		}
	}

	public override void OnPointerDown(PointerEventData ped)
	{
		base.OnPointerDown(ped);
		Input.GetMouseButton(0);
	}

	public override void OnBeginDrag(PointerEventData ped)
	{
		base.OnBeginDrag(ped);
		if (!Input.GetMouseButton(0))
		{
			return;
		}
		isDrag = true;
		if ((bool)camCtrl)
		{
			camCtrl.NoCtrlCondition = () => true;
		}
		CalcDragPos(ped);
	}

	public override void OnDrag(PointerEventData ped)
	{
		base.OnDrag(ped);
		if (isDrag)
		{
			CalcDragPos(ped);
		}
	}

	public override void OnEndDrag(PointerEventData ped)
	{
		base.OnEndDrag(ped);
		if (!isDrag)
		{
			return;
		}
		isDrag = false;
		if ((bool)camCtrl)
		{
			camCtrl.NoCtrlCondition = () => false;
		}
	}
}
