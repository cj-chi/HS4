using System;
using UnityEngine;

namespace MyLocalize;

public static class MyLocalizeDefine
{
	public enum LocalizeKeyType
	{
		Font,
		CharaCustom,
		NetworkCheck,
		EntryHandleName,
		Uploader,
		Downloader
	}

	[Serializable]
	public class LocalizeInfo
	{
		public int id;

		public string fontAsset = "";

		public int size = 24;

		public string str = "";
	}

	public const string CustomLocalizePath = "/Illusion/Scripts/Scene/_Localize/AssetBundle/";

	public static readonly string[] LocalizeKeyName = new string[6] { "Font/", "CharaCustom/", "NetworkCheck/", "EntryHandleName/", "Uploader/", "Downloader/" };

	public static readonly string[] AssetBundleKeyName = new string[6] { "localize/font/", "localize/characustom/", "localize/networkcheck/", "localize/entryhandlename/", "localize/uploader/", "localize/downloader/" };

	public static readonly string[] cultureDir = new string[4] { "JP/", "US/", "CN/", "TW/" };

	public static readonly string[] cultureNames = new string[4] { "ja-JP", "en-US", "zh-CN", "zh-TW" };

	public const string FontInfoName = "FontInfo";

	public const string TextInfoName = "TextInfo";

	public const string SpriteInfoName = "SpriteInfo";

	public const string LocalizeExcelName = "LocalizeText.xls";

	public static readonly string[] AssetBundleNameXXX = new string[4] { "jp.unity3d", "us.unity3d", "cn.unity3d", "tw.unity3d" };

	public static string GetFontInfoPath(int culture, bool full = false)
	{
		if (full)
		{
			return Application.dataPath + "/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[0] + cultureDir[culture] + "FontInfo.asset";
		}
		return "Assets/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[0] + cultureDir[culture] + "FontInfo.asset";
	}

	public static string GetTextInfoPath(LocalizeKeyType type, int culture, bool full = false)
	{
		if (full)
		{
			return Application.dataPath + "/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[(int)type] + cultureDir[culture] + "TextInfo.asset";
		}
		return "Assets/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[(int)type] + cultureDir[culture] + "TextInfo.asset";
	}

	public static string GetLocalizeExcelPath(LocalizeKeyType type, bool full = false)
	{
		if (full)
		{
			return Application.dataPath + "/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[(int)type] + "LocalizeText.xls";
		}
		return "Assets/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[(int)type] + "LocalizeText.xls";
	}

	public static string GetSpriteInfoPath(LocalizeKeyType type, int culture, bool full = false)
	{
		if (full)
		{
			return Application.dataPath + "/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[(int)type] + cultureDir[culture] + "SpriteInfo.asset";
		}
		return "Assets/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[(int)type] + cultureDir[culture] + "SpriteInfo.asset";
	}

	public static string GetAssetBundleDir(LocalizeKeyType type, int culture)
	{
		return ("Assets/Illusion/Scripts/Scene/_Localize/AssetBundle/" + LocalizeKeyName[(int)type] + cultureDir[culture]).TrimEnd('/');
	}

	public static string GetAssetBundleName(LocalizeKeyType type, int culture)
	{
		return AssetBundleKeyName[(int)type] + AssetBundleNameXXX[culture];
	}

	public static bool CheckLocalizeText(string str)
	{
		if ("" == str)
		{
			return false;
		}
		if (int.TryParse(str, out var _))
		{
			return false;
		}
		if (float.TryParse(str, out var _))
		{
			return false;
		}
		return true;
	}
}
