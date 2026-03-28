using System;
using System.Collections;
using System.Linq;
using Illusion.Extensions;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsSelectWindow : MonoBehaviour
{
	[Serializable]
	public class ItemInfo
	{
		public UI_ButtonEx btnItem;

		public CanvasGroup[] cgItem;

		public CvsBase cvsBase;

		public int No;
	}

	public CanvasGroup cgBaseWindow;

	public UI_ButtonEx btnNewFirst;

	public UI_ButtonEx btnEditFirst;

	public ItemInfo[] items;

	private int backSelect = -1;

	private Text titleText;

	public virtual IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		if (items.Any())
		{
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
								if ((bool)canvasGroup)
								{
									canvasGroup.Enable(enable: false);
								}
							}
						}
					}
					cgItem = item.val.cgItem;
					foreach (CanvasGroup canvasGroup2 in cgItem)
					{
						if ((bool)canvasGroup2)
						{
							canvasGroup2.Enable(enable: true);
						}
					}
					if ((bool)cgBaseWindow)
					{
						cgBaseWindow.Enable(enable: true);
					}
					if (backSelect != item.idx)
					{
						if ((bool)item.val.cvsBase)
						{
							titleText = item.val.btnItem.GetComponentInChildren<Text>();
							if ((bool)titleText && (bool)item.val.cvsBase.titleText)
							{
								item.val.cvsBase.titleText.text = titleText.text;
							}
							item.val.cvsBase.SNo = item.val.No;
							item.val.cvsBase.UpdateCustomUI();
							item.val.cvsBase.ChangeMenuFunc();
						}
						CustomColorCtrl customColorCtrl = Singleton<CustomBase>.Instance.customColorCtrl;
						if ((bool)customColorCtrl)
						{
							customColorCtrl.Close();
						}
						backSelect = item.idx;
					}
				});
			});
		}
		if (Singleton<CustomBase>.Instance.modeNew)
		{
			btnNewFirst?.onClick.Invoke();
		}
		else
		{
			btnEditFirst?.onClick.Invoke();
		}
	}
}
