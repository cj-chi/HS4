using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEx;

public class UITrigger : UIBehaviour
{
	[Serializable]
	public class TriggerEvent : UnityEvent<BaseEventData>
	{
	}

	[SerializeField]
	private List<TriggerEvent> _triggers;

	private readonly List<CanvasGroup> _canvasGroupCache = new List<CanvasGroup>();

	private bool _groupAllowInteraction = true;

	public List<TriggerEvent> Triggers
	{
		get
		{
			return _triggers ?? (_triggers = new List<TriggerEvent>());
		}
		set
		{
			_triggers = value;
		}
	}

	public void AddEvent(TriggerEvent evnt)
	{
		Triggers.Add(evnt);
	}

	protected override void OnCanvasGroupChanged()
	{
		bool flag = true;
		Transform parent = base.transform;
		while (parent != null)
		{
			parent.GetComponents(_canvasGroupCache);
			bool flag2 = false;
			for (int i = 0; i < _canvasGroupCache.Count; i++)
			{
				if (!_canvasGroupCache[i].interactable)
				{
					flag = false;
					flag2 = true;
				}
				if (_canvasGroupCache[i].ignoreParentGroups)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				break;
			}
			parent = parent.parent;
		}
		if (flag != _groupAllowInteraction)
		{
			_groupAllowInteraction = flag;
		}
	}

	public bool IsInteractable()
	{
		return _groupAllowInteraction;
	}
}
