using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CommonLib
{
	public static List<string> GetAssetBundleNameListFromPath(string path, bool subdirCheck = false)
	{
		List<string> result = new List<string>();
		if (!AssetBundleCheck.IsSimulation)
		{
			string path2 = AssetBundleManager.BaseDownloadingURL + path;
			if (subdirCheck)
			{
				List<string> list = new List<string>();
				GetAllFiles(path2, "*.unity3d", list);
				result = list.Select((string s) => s.Replace(AssetBundleManager.BaseDownloadingURL, "")).ToList();
			}
			else
			{
				if (!Directory.Exists(path2))
				{
					return result;
				}
				result = (from s in Directory.GetFiles(path2, "*.unity3d")
					select s.Replace(AssetBundleManager.BaseDownloadingURL, "")).ToList();
			}
		}
		return result;
	}

	public static void GetAllFiles(string path, string searchPattern, List<string> lst)
	{
		if (Directory.Exists(path))
		{
			lst.AddRange(Directory.GetFiles(path, searchPattern));
			string[] directories = Directory.GetDirectories(path);
			for (int i = 0; i < directories.Length; i++)
			{
				GetAllFiles(directories[i], searchPattern, lst);
			}
		}
	}

	public static void CopySameNameTransform(Transform trfDst, Transform trfSrc)
	{
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(trfDst);
		Dictionary<string, GameObject> dictObjName = findAssist.dictObjName;
		FindAssist findAssist2 = new FindAssist();
		findAssist2.Initialize(trfSrc);
		Dictionary<string, GameObject> dictObjName2 = findAssist2.dictObjName;
		GameObject value = null;
		foreach (KeyValuePair<string, GameObject> item in dictObjName)
		{
			if (dictObjName2.TryGetValue(item.Key, out value))
			{
				item.Value.transform.localPosition = value.transform.localPosition;
				item.Value.transform.localRotation = value.transform.localRotation;
				item.Value.transform.localScale = value.transform.localScale;
			}
		}
	}

	public static T LoadAsset<T>(string assetBundleName, string assetName, bool clone = false, string manifestName = "") where T : Object
	{
		if (AssetBundleCheck.IsSimulation)
		{
			manifestName = "";
		}
		if (!AssetBundleCheck.IsFile(assetBundleName, assetName))
		{
			_ = "読み込みエラー\r\nassetBundleName：" + assetBundleName + "\tassetName：" + assetName;
			return null;
		}
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(assetBundleName, assetName, typeof(T), manifestName.IsNullOrEmpty() ? null : manifestName);
		if (assetBundleLoadAssetOperation.IsEmpty())
		{
			_ = "読み込みエラー\r\nassetName：" + assetName;
			return null;
		}
		T val = assetBundleLoadAssetOperation.GetAsset<T>();
		if (null != val && clone)
		{
			T val2 = Object.Instantiate(val);
			val2.name = val.name;
			val = val2;
		}
		return val;
	}
}
