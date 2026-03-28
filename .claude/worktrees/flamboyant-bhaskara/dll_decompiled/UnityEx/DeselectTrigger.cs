using UnityEngine.EventSystems;

namespace UnityEx;

public class DeselectTrigger : UITrigger, IDeselectHandler, IEventSystemHandler
{
	public void OnDeselect(BaseEventData eventData)
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
