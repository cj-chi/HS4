using System;
using System.Linq;
using Illusion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UI_ToggleGroupCtrl : MonoBehaviour
{
	[Serializable]
	public class ItemInfo
	{
		public Toggle tglItem;

		public CanvasGroup cgItem;
	}

	public ItemInfo[] items;

	public virtual void Start()
	{
		if (!items.Any())
		{
			return;
		}
		(from item in items.Select((ItemInfo val, int idx) => new { val, idx })
			where item.val != null && item.val.tglItem != null
			select item).ToList().ForEach(item =>
		{
			(from isOn in item.val.tglItem.OnValueChangedAsObservable()
				where isOn
				select isOn).Subscribe(delegate
			{
				foreach (var item in items.Select((ItemInfo v, int i) => new { v, i }))
				{
					if (item.i != item.idx && item.v != null)
					{
						CanvasGroup cgItem = item.v.cgItem;
						if (null != cgItem)
						{
							cgItem.Enable(enable: false);
						}
					}
				}
				if (null != item.val.cgItem)
				{
					item.val.cgItem.Enable(enable: true);
				}
			});
		});
	}

	public int GetSelectIndex()
	{
		return items.Select((ItemInfo v, int i) => new { v, i }).FirstOrDefault(x => x.v.tglItem.isOn)?.i ?? (-1);
	}
}
