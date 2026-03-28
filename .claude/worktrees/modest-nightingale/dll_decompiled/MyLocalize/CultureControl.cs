using System.Collections.Generic;
using System.Linq;
using Illusion.CustomAttributes;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace MyLocalize;

public class CultureControl : MonoBehaviour
{
	[Button("DebugChangeLocalize", "日本語", new object[] { 0 })]
	public int Japanese;

	[Button("DebugChangeLocalize", "英語", new object[] { 1 })]
	public int English;

	[Button("DebugChangeLocalize", "中国語(簡体)", new object[] { 2 })]
	public int SimplifiedChinese;

	[Button("DebugChangeLocalize", "中国語(繁体)", new object[] { 3 })]
	public int TraditionalChinese;

	[SerializeField]
	protected List<TextCultureComponent> lstCmpCultureText;

	public List<SpriteCultureComponent> lstCmpCultureSprite;

	public Dictionary<int, FontInfo.Info> dictFontInfo { get; private set; }

	public List<MyLocalizeDefine.LocalizeInfo> GetLocalizeInfoList()
	{
		if (lstCmpCultureText == null)
		{
			return null;
		}
		if (lstCmpCultureText.Count == 0)
		{
			return null;
		}
		return lstCmpCultureText.Select((TextCultureComponent v) => new MyLocalizeDefine.LocalizeInfo
		{
			id = v.id,
			fontAsset = v.text.font.name,
			size = v.text.fontSize,
			str = v.text.text
		}).ToList();
	}

	public void ChangeLocalize(MyLocalizeDefine.LocalizeKeyType type, int culture)
	{
		if (lstCmpCultureText == null)
		{
			return;
		}
		string assetBundleName = MyLocalizeDefine.GetAssetBundleName(MyLocalizeDefine.LocalizeKeyType.Font, culture);
		string assetBundleName2 = MyLocalizeDefine.GetAssetBundleName(type, culture);
		FontInfo fontInfo = CommonLib.LoadAsset<FontInfo>(assetBundleName, "FontInfo");
		if (null != fontInfo)
		{
			dictFontInfo = fontInfo.lstInfo.ToDictionary((FontInfo.Info v) => v.id, (FontInfo.Info v) => v);
		}
		TextInfo textInfo = CommonLib.LoadAsset<TextInfo>(assetBundleName2, "TextInfo");
		if (null != textInfo && dictFontInfo != null)
		{
			Dictionary<int, TextInfo.Info> dictTextInfo = textInfo.lstInfo.ToDictionary((TextInfo.Info v) => v.textId, (TextInfo.Info v) => v);
			int count = lstCmpCultureText.Count;
			for (int num = 0; num < count; num++)
			{
				lstCmpCultureText[num].ChangeCulture(dictFontInfo, dictTextInfo);
			}
		}
		SpriteInfo spriteinfo = CommonLib.LoadAsset<SpriteInfo>(assetBundleName2, "SpriteInfo");
		if (null != spriteinfo)
		{
			Dictionary<int, SpriteInfo.SrcInfo> dictSpriteInfo = spriteinfo.lstDstcInfo.ToDictionary((SpriteInfo.DstInfo v) => v.dstId, (SpriteInfo.DstInfo v) => spriteinfo.lstSrcInfo.Find((SpriteInfo.SrcInfo x) => x.id == v.srcId));
			int count2 = lstCmpCultureSprite.Count;
			for (int num2 = 0; num2 < count2; num2++)
			{
				lstCmpCultureSprite[num2].ChangeCulture(dictSpriteInfo);
			}
		}
		AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: false);
		AssetBundleManager.UnloadAssetBundle(assetBundleName2, isUnloadForceRefCount: false);
	}

	protected virtual void Reset()
	{
		TextCultureComponent[] componentsInChildren = GetComponentsInChildren<TextCultureComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate(componentsInChildren[i]);
		}
		Text[] componentsInChildren2 = GetComponentsInChildren<Text>(includeInactive: true);
		lstCmpCultureText = new List<TextCultureComponent>();
		int num = 0;
		Text[] array = componentsInChildren2;
		foreach (Text text in array)
		{
			if (MyLocalizeDefine.CheckLocalizeText(text.text))
			{
				TextCultureComponent textCultureComponent = text.gameObject.AddComponent<TextCultureComponent>();
				textCultureComponent.id = num++;
				textCultureComponent.text = text;
				lstCmpCultureText.Add(textCultureComponent);
			}
		}
	}

	protected virtual void Start()
	{
		int languageInt = Singleton<GameSystem>.Instance.languageInt;
		string assetBundleName = MyLocalizeDefine.GetAssetBundleName(MyLocalizeDefine.LocalizeKeyType.Font, languageInt);
		FontInfo fontInfo = CommonLib.LoadAsset<FontInfo>(assetBundleName, "FontInfo");
		dictFontInfo = fontInfo.lstInfo.ToDictionary((FontInfo.Info v) => v.id, (FontInfo.Info v) => v);
		AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: false);
	}
}
