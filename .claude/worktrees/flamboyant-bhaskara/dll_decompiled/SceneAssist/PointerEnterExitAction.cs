using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SceneAssist;

public class PointerEnterExitAction : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public List<UnityAction> listActionEnter = new List<UnityAction>();

	public List<UnityAction> listActionExit = new List<UnityAction>();

	public void OnPointerEnter(PointerEventData eventData)
	{
		for (int i = 0; i < listActionEnter.Count; i++)
		{
			listActionEnter[i]();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		for (int i = 0; i < listActionExit.Count; i++)
		{
			listActionExit[i]();
		}
	}
}
