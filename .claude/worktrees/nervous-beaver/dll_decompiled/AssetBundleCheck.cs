using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class AssetBundleCheck
{
	public static bool IsSimulation => false;

	public static bool IsFile(string assetBundleName, string fileName = "")
	{
		if (!File.Exists(AssetBundleManager.BaseDownloadingURL + assetBundleName))
		{
			return false;
		}
		return true;
	}

	public static bool IsManifest(string manifest)
	{
		return AssetBundleManager.ManifestBundlePack.ContainsKey(manifest);
	}

	public static bool IsManifestOrBundle(string bundle)
	{
		if (!AssetBundleManager.ManifestBundlePack.ContainsKey(bundle))
		{
			return IsFile(bundle);
		}
		return true;
	}

	public static string[] GetAllAssetName(string assetBundleName, bool _WithExtension = true, string manifestAssetBundleName = null, bool isAllCheck = false)
	{
		if (manifestAssetBundleName == null && isAllCheck && AssetBundleManager.AllLoadedAssetBundleNames.Contains(assetBundleName))
		{
			foreach (KeyValuePair<string, AssetBundleManager.BundlePack> item in AssetBundleManager.ManifestBundlePack)
			{
				if (!item.Value.LoadedAssetBundles.TryGetValue(assetBundleName, out var value))
				{
					continue;
				}
				if (_WithExtension)
				{
					return (from file in value.Bundle.GetAllAssetNames()
						select Path.GetFileName(file)).ToArray();
				}
				return (from file in value.Bundle.GetAllAssetNames()
					select Path.GetFileNameWithoutExtension(file)).ToArray();
			}
		}
		LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleName, manifestAssetBundleName);
		AssetBundle assetBundle = ((loadedAssetBundle == null) ? AssetBundle.LoadFromFile(AssetBundleManager.BaseDownloadingURL + assetBundleName) : loadedAssetBundle.Bundle);
		string[] array = null;
		array = ((!_WithExtension) ? (from file in assetBundle.GetAllAssetNames()
			select Path.GetFileNameWithoutExtension(file)).ToArray() : (from file in assetBundle.GetAllAssetNames()
			select Path.GetFileName(file)).ToArray());
		if (loadedAssetBundle == null)
		{
			assetBundle.Unload(unloadAllLoadedObjects: true);
		}
		return array;
	}
}
