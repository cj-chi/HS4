using System;
using System.Linq;
using Illusion.Extensions;
using UniRx;
using UnityEngine;

public class UI_ButtonExGroupCtrl : MonoBehaviour
{
	[Serializable]
	public class ItemInfo
	{
		public UI_ButtonEx btnItem;

		public CanvasGroup[] cgItem;
	}

	public ItemInfo[] items;

	public virtual void Start()
	{
		if (!items.Any())
		{
			return;
		}
		(from item in items.Select((ItemInfo val, int idx) => new { val, idx })
			where item.val != null && item.val.btnItem != null
			select item).ToList().ForEach(item =>
		{
			item.val.btnItem.OnClickAsObservable().Subscribe(delegate
			{
				ItemInfo[] array = items;
				CanvasGroup[] cgItem;
				foreach (ItemInfo itemInfo in array)
				{
					if (itemInfo != null && itemInfo.btnItem != item.val.btnItem)
					{
						cgItem = itemInfo.cgItem;
						foreach (CanvasGroup canvasGroup in cgItem)
						{
							if (null != canvasGroup)
							{
								canvasGroup.Enable(enable: false);
							}
						}
					}
				}
				cgItem = item.val.cgItem;
				foreach (CanvasGroup canvasGroup2 in cgItem)
				{
					if (null != canvasGroup2)
					{
						canvasGroup2.Enable(enable: true);
					}
				}
			});
		});
	}
}
