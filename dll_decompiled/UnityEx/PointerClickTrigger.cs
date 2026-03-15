using UnityEngine.EventSystems;

namespace UnityEx;

public class PointerClickTrigger : UITrigger, IPointerClickHandler, IEventSystemHandler
{
	public void OnPointerClick(PointerEventData eventData)
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
