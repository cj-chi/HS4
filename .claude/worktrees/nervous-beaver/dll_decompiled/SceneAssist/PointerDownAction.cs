using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SceneAssist;

public class PointerDownAction : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	public List<UnityAction> listAction = new List<UnityAction>();

	public void OnPointerDown(PointerEventData eventData)
	{
		for (int i = 0; i < listAction.Count; i++)
		{
			listAction[i]();
		}
	}
}
