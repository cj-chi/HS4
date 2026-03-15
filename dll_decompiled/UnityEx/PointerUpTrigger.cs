using UnityEngine.EventSystems;

namespace UnityEx;

public class PointerUpTrigger : UITrigger, IPointerUpHandler, IEventSystemHandler
{
	public void OnPointerUp(PointerEventData eventData)
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
