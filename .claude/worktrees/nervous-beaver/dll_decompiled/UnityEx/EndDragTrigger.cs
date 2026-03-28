using UnityEngine.EventSystems;

namespace UnityEx;

public class EndDragTrigger : UITrigger, IEndDragHandler, IEventSystemHandler
{
	public void OnEndDrag(PointerEventData eventData)
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
