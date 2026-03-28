using UnityEngine.EventSystems;

namespace UnityEx;

public class DropTrigger : UITrigger, IDropHandler, IEventSystemHandler
{
	public void OnDrop(PointerEventData eventData)
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
