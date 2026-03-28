using UnityEngine.EventSystems;

namespace UnityEx;

public class PointerExitTrigger : UITrigger, IPointerExitHandler, IEventSystemHandler
{
	public void OnPointerExit(PointerEventData eventData)
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
