using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Studio;

public class SortCanvas : Singleton<SortCanvas>
{
	[SerializeField]
	private Canvas[] canvas;

	public static Canvas select
	{
		set
		{
			if (Singleton<SortCanvas>.IsInstance())
			{
				Singleton<SortCanvas>.Instance.OnSelect(value);
			}
		}
	}

	public void OnSelect(Canvas _canvas)
	{
		if (_canvas == null)
		{
			return;
		}
		SortedList<int, Canvas> sortedList = new SortedList<int, Canvas>();
		_canvas.sortingOrder = 10;
		for (int i = 0; i < canvas.Length; i++)
		{
			sortedList.Add(canvas[i].sortingOrder, canvas[i]);
		}
		foreach (var item in sortedList.Select((KeyValuePair<int, Canvas> l, int i2) => new
		{
			Value = l.Value,
			i = i2
		}))
		{
			item.Value.sortingOrder = item.i;
		}
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
