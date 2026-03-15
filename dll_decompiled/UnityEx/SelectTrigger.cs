using UnityEngine.EventSystems;

namespace UnityEx;

public class SelectTrigger : UITrigger, ISelectHandler, IEventSystemHandler
{
	public void OnSelect(BaseEventData eventData)
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
