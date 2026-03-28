using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomSkinColorPreset : MonoBehaviour
{
	[Serializable]
	public class ItemInfo
	{
		public Button button;

		public Image image;

		public Color skinColor = Color.white;
	}

	public ItemInfo[] items;

	public Action<Color> onClick;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	protected ChaControl chaCtrl => customBase.chaCtrl;

	public void Reset()
	{
		items = new ItemInfo[7];
		for (int i = 0; i < 7; i++)
		{
			items[i] = new ItemInfo();
			Transform transform = base.transform.Find($"btnSample{i + 1:00}");
			if ((bool)transform)
			{
				items[i].button = transform.GetComponent<Button>();
				Transform transform2 = transform.Find("imgColor");
				if ((bool)transform2)
				{
					items[i].image = transform2.GetComponent<Image>();
				}
			}
		}
	}

	public IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		Dictionary<int, ListInfoBase> categoryInfo = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.preset_skin_color);
		for (int num = 0; num < items.Length; num++)
		{
			if (categoryInfo.TryGetValue(num, out var value))
			{
				items[num].skinColor = Color.HSVToRGB(value.GetInfoFloat(ChaListDefine.KeyType.BaseH), value.GetInfoFloat(ChaListDefine.KeyType.BaseS), value.GetInfoFloat(ChaListDefine.KeyType.BaseV));
				items[num].image.color = Color.HSVToRGB(value.GetInfoFloat(ChaListDefine.KeyType.SampleH), value.GetInfoFloat(ChaListDefine.KeyType.SampleS), value.GetInfoFloat(ChaListDefine.KeyType.SampleV));
			}
		}
		if (!items.Any())
		{
			yield break;
		}
		(from item in items.Select((ItemInfo val, int idx) => new { val, idx })
			where item.val != null && item.val.button != null
			select item).ToList().ForEach(item =>
		{
			item.val.button.OnClickAsObservable().Subscribe(delegate
			{
				onClick?.Invoke(item.val.skinColor);
			});
		});
	}
}
