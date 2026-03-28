using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tutorial2D;

public class SliderHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public Action onPointerDown;

	public Action onPointerUp;

	public void OnPointerDown(PointerEventData eventData)
	{
		onPointerDown?.Invoke();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		onPointerUp?.Invoke();
	}
}
