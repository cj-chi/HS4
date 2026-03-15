using UnityEngine;
using UnityEngine.EventSystems;

namespace CharaCustom;

public class CustomCanvasSort : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	private CustomCanvasSortControl ccsCtrl;

	[SerializeField]
	private Canvas canvas;

	public virtual void OnPointerDown(PointerEventData ped)
	{
		if (Input.GetMouseButton(0) && null != ccsCtrl)
		{
			ccsCtrl.SortCanvas(canvas);
		}
	}

	public virtual void OnBeginDrag(PointerEventData ped)
	{
	}

	public virtual void OnDrag(PointerEventData ped)
	{
	}

	public virtual void OnEndDrag(PointerEventData ped)
	{
	}
}
