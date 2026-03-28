using UnityEngine.EventSystems;

namespace UnityEx;

public class PointerEnterTrigger : UITrigger, IPointerEnterHandler, IEventSystemHandler
{
	public void OnPointerEnter(PointerEventData eventData)
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
