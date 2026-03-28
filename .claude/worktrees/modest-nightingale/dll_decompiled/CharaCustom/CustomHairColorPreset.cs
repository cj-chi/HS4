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

public class CustomHairColorPreset : MonoBehaviour
{
	public class HairColorInfo
	{
		public Color topColor = Color.white;

		public Color baseColor = Color.white;

		public Color underColor = Color.white;

		public Color specular = Color.white;

		public float metallic;

		public float smoothness;
	}

	[Serializable]
	public class ItemInfo
	{
		public Button button;

		public Image image;

		public HairColorInfo colorInfo = new HairColorInfo();
	}

	public ItemInfo[] items;

	public Action<HairColorInfo> onClick;

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
		Dictionary<int, ListInfoBase> categoryInfo = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.preset_hair_color);
		for (int num = 0; num < items.Length; num++)
		{
			if (categoryInfo.TryGetValue(num, out var value))
			{
				items[num].colorInfo.baseColor = Color.HSVToRGB(value.GetInfoFloat(ChaListDefine.KeyType.BaseH), value.GetInfoFloat(ChaListDefine.KeyType.BaseS), value.GetInfoFloat(ChaListDefine.KeyType.BaseV));
				items[num].colorInfo.topColor = Color.HSVToRGB(value.GetInfoFloat(ChaListDefine.KeyType.TopH), value.GetInfoFloat(ChaListDefine.KeyType.TopS), value.GetInfoFloat(ChaListDefine.KeyType.TopV));
				items[num].colorInfo.underColor = Color.HSVToRGB(value.GetInfoFloat(ChaListDefine.KeyType.UnderH), value.GetInfoFloat(ChaListDefine.KeyType.UnderS), value.GetInfoFloat(ChaListDefine.KeyType.UnderV));
				items[num].colorInfo.specular = Color.HSVToRGB(value.GetInfoFloat(ChaListDefine.KeyType.SpecularH), value.GetInfoFloat(ChaListDefine.KeyType.SpecularS), value.GetInfoFloat(ChaListDefine.KeyType.SpecularV));
				items[num].colorInfo.metallic = value.GetInfoFloat(ChaListDefine.KeyType.Metallic);
				items[num].colorInfo.smoothness = value.GetInfoFloat(ChaListDefine.KeyType.Smoothness);
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
				onClick?.Invoke(item.val.colorInfo);
			});
		});
	}
}
