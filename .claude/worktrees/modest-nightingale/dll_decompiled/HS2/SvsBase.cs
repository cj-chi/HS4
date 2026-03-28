using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using CharaCustom;
using Illusion.CustomAttributes;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class SvsBase : MonoBehaviour
{
	[Serializable]
	public class ItemInfo
	{
		public UI_ToggleEx tglItem;

		public CanvasGroup cgItem;
	}

	[Button("ReacquireTab", "タブ再取得", new object[] { })]
	public int reacquireTab;

	[SerializeField]
	private UI_ToggleEx[] tglTab;

	public Text titleText;

	public ItemInfo[] items;

	protected SearchBase searchBase => Singleton<SearchBase>.Instance;

	protected ChaListControl lstCtrl => Singleton<Character>.Instance.chaListCtrl;

	protected ChaControl chaCtrl => searchBase.chaCtrl;

	protected ChaFileFace face => chaCtrl.fileFace;

	protected ChaFileBody body => chaCtrl.fileBody;

	protected ChaFileHair hair => chaCtrl.fileHair;

	protected ChaFileFace.MakeupInfo makeup => chaCtrl.fileFace.makeup;

	protected ChaFileClothes orgClothes => chaCtrl.chaFile.coordinate.clothes;

	protected ChaFileClothes nowClothes => chaCtrl.nowCoordinate.clothes;

	protected ChaFileAccessory orgAcs => chaCtrl.chaFile.coordinate.accessory;

	protected ChaFileAccessory nowAcs => chaCtrl.nowCoordinate.accessory;

	protected ChaFileParameter parameter => chaCtrl.chaFile.parameter;

	protected ChaFileParameter2 parameter2 => chaCtrl.chaFile.parameter2;

	protected ChaFileGameInfo gameinfo => chaCtrl.chaFile.gameinfo;

	protected ChaFileControl defChaCtrl => searchBase.defChaCtrl;

	public int SNo { get; set; }

	public void ReacquireTab()
	{
		tglTab = null;
		List<UI_ToggleEx> list = new List<UI_ToggleEx>();
		Transform transform = base.transform.FindLoop("SelectMenu");
		if (!transform)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			Transform transform2 = transform.Find($"tgl{i + 1:00}");
			if ((bool)transform2)
			{
				UI_ToggleEx component = transform2.GetComponent<UI_ToggleEx>();
				if ((bool)component)
				{
					list.Add(component);
				}
			}
		}
		if (list.Count != 0)
		{
			tglTab = list.ToArray();
		}
	}

	public void ShowOrHideTab(bool show, params int[] no)
	{
		if (tglTab.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < no.Length; i++)
		{
			if (tglTab.Length > no[i])
			{
				if (!show)
				{
					flag |= tglTab[no[i]].isOn;
				}
				tglTab[no[i]].gameObject.SetActiveIfDifferent(show);
			}
		}
		if (!show)
		{
			if (flag)
			{
				tglTab[0].isOn = true;
			}
			for (int j = 0; j < no.Length; j++)
			{
				tglTab[no[j]].SetIsOnWithoutCallback(isOn: false);
			}
		}
	}

	public virtual void UpdateCustomUI()
	{
	}

	public virtual void ChangeMenuFunc()
	{
	}

	public static List<CustomSelectInfo> CreateSelectList(ChaListDefine.CategoryNo cateNo, ChaListDefine.KeyType limitKey = ChaListDefine.KeyType.Unknown)
	{
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		Dictionary<int, ListInfoBase> categoryInfo = chaListCtrl.GetCategoryInfo(cateNo);
		int[] array = categoryInfo.Keys.ToArray();
		List<CustomSelectInfo> list = new List<CustomSelectInfo>();
		for (int i = 0; i < categoryInfo.Count; i++)
		{
			if (99 != categoryInfo[array[i]].GetInfoInt(ChaListDefine.KeyType.Possess))
			{
				bool newItem = false;
				byte b = chaListCtrl.CheckItemID(categoryInfo[array[i]].Category, categoryInfo[array[i]].Id);
				if (1 == b)
				{
					newItem = true;
				}
				list.Add(new CustomSelectInfo
				{
					category = categoryInfo[array[i]].Category,
					id = categoryInfo[array[i]].Id,
					limitIndex = ((limitKey == ChaListDefine.KeyType.Unknown) ? (-1) : categoryInfo[array[i]].GetInfoInt(limitKey)),
					name = categoryInfo[array[i]].Name,
					fontSize = ((categoryInfo[array[i]].FontSize == 0) ? 24 : categoryInfo[array[i]].FontSize),
					assetBundle = categoryInfo[array[i]].GetInfo(ChaListDefine.KeyType.ThumbAB),
					assetName = categoryInfo[array[i]].GetInfo(ChaListDefine.KeyType.ThumbTex),
					newItem = newItem
				});
			}
		}
		return list;
	}

	public int GetSelectTab()
	{
		return items.Select((ItemInfo v, int i) => new { v, i }).FirstOrDefault(x => x.v.tglItem.isOn)?.i ?? (-1);
	}

	public static List<CustomPushInfo> CreatePushList(ChaListDefine.CategoryNo cateNo)
	{
		Dictionary<int, ListInfoBase> categoryInfo = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(cateNo);
		int[] array = categoryInfo.Keys.ToArray();
		List<CustomPushInfo> list = new List<CustomPushInfo>();
		for (int i = 0; i < categoryInfo.Count; i++)
		{
			list.Add(new CustomPushInfo
			{
				category = categoryInfo[array[i]].Category,
				id = categoryInfo[array[i]].Id,
				name = categoryInfo[array[i]].Name,
				fontSize = ((categoryInfo[array[i]].FontSize == 0) ? 24 : categoryInfo[array[i]].FontSize),
				assetBundle = categoryInfo[array[i]].GetInfo(ChaListDefine.KeyType.ThumbAB),
				assetName = categoryInfo[array[i]].GetInfo(ChaListDefine.KeyType.ThumbTex)
			});
		}
		return list;
	}

	protected virtual void Reset()
	{
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(base.transform);
		GameObject objectFromName = findAssist.GetObjectFromName("textWinTitle");
		if ((bool)objectFromName)
		{
			titleText = objectFromName.GetComponent<Text>();
		}
		List<ItemInfo> list = new List<ItemInfo>();
		for (int i = 0; i < 5; i++)
		{
			GameObject objectFromName2 = findAssist.GetObjectFromName($"tgl{i + 1:00}");
			if ((bool)objectFromName2)
			{
				GameObject objectFromName3 = findAssist.GetObjectFromName($"Setting{i + 1:00}");
				if ((bool)objectFromName3)
				{
					UI_ToggleEx component = objectFromName2.GetComponent<UI_ToggleEx>();
					CanvasGroup component2 = objectFromName3.GetComponent<CanvasGroup>();
					list.Add(new ItemInfo
					{
						tglItem = component,
						cgItem = component2
					});
				}
			}
		}
		if (1 < list.Count())
		{
			items = list.ToArray();
		}
	}

	protected virtual IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		if (!items.Any())
		{
			yield break;
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
						if ((bool)cgItem)
						{
							cgItem.Enable(enable: false);
						}
					}
				}
				if ((bool)item.val.cgItem)
				{
					item.val.cgItem.Enable(enable: true);
				}
				searchBase.searchCtrl.showColorCvs = false;
			});
		});
	}
}
