using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SceneAssist;

public static class AssetBundleCheck
{
	public static bool IsSimulation => false;

	public static string[] FindAllAssetName(string assetBundleName, string _regex, bool _WithExtension = true, RegexOptions _options = RegexOptions.None)
	{
		_regex = _regex.ToLower();
		string[] array = null;
		AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetBundleManager.BaseDownloadingURL + assetBundleName);
		array = ((!_WithExtension) ? (from v in assetBundle.GetAllAssetNames()
			select Path.GetFileNameWithoutExtension(v) into v
			where CheckRegex(v, _regex, _options)
			select v).ToArray() : (from v in assetBundle.GetAllAssetNames()
			select Path.GetFileName(v) into v
			where CheckRegex(v, _regex, _options)
			select v).ToArray());
		assetBundle.Unload(unloadAllLoadedObjects: true);
		return array;
	}

	private static bool CheckRegex(string _value, string _regex, RegexOptions _options)
	{
		return Regex.Match(_value, _regex, _options).Success;
	}

	public static bool FindFile(string _assetBundleName, string _fineName, bool _WithExtension = false)
	{
		_fineName = _fineName.ToLower();
		bool flag = false;
		AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetBundleManager.BaseDownloadingURL + _assetBundleName);
		if (assetBundle == null)
		{
			return false;
		}
		flag = ((!_WithExtension) ? ((from v in assetBundle.GetAllAssetNames()
			select Path.GetFileNameWithoutExtension(v)).ToList().FindIndex((string s) => s == _fineName) != -1) : ((from v in assetBundle.GetAllAssetNames()
			select Path.GetFileName(v)).ToList().FindIndex((string s) => s == _fineName) != -1));
		assetBundle.Unload(unloadAllLoadedObjects: true);
		return flag;
	}
}
