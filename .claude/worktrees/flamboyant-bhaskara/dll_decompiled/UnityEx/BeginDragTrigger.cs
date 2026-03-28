using UnityEngine.EventSystems;

namespace UnityEx;

public class BeginDragTrigger : UITrigger, IBeginDragHandler, IEventSystemHandler
{
	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!base.isActiveAndEnabled || !IsInteractable())
		{
			return;
		}
		foreach (TriggerEvent trigger in base.Triggers)
		{
			trigger.Invoke(eventData);
		}
	}
}
