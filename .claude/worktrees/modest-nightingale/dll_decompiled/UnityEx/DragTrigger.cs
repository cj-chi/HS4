using UnityEngine.EventSystems;

namespace UnityEx;

public class DragTrigger : UITrigger, IDragHandler, IEventSystemHandler
{
	public void OnDrag(PointerEventData eventData)
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
