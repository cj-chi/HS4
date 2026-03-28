using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UniRx.Async;
using UnityEngine;

[Serializable]
public class AssetBundleData
{
	[SerializeField]
	protected string _bundle = "";

	[SerializeField]
	protected string _asset = "";

	protected static bool isSimulation => false;

	public string bundle => _bundle;

	public string asset => _asset;

	protected AssetBundleLoadAssetOperation _request { get; set; }

	public bool isEmpty
	{
		get
		{
			if (!_bundle.IsNullOrEmpty())
			{
				return _asset.IsNullOrEmpty();
			}
			return true;
		}
	}

	public virtual LoadedAssetBundle LoadedBundle => AssetBundleManager.GetLoadedAssetBundle(_bundle);

	private string bundlePath => AssetBundleManager.BaseDownloadingURL + _bundle;

	public bool isFile
	{
		get
		{
			if (LoadedBundle != null)
			{
				return true;
			}
			if (File.Exists(bundlePath))
			{
				return true;
			}
			return false;
		}
	}

	public virtual string[] AllAssetNames
	{
		get
		{
			LoadedAssetBundle loadedBundle = LoadedBundle;
			AssetBundle assetBundle = null;
			assetBundle = ((loadedBundle == null) ? AssetBundle.LoadFromFile(bundlePath) : loadedBundle.Bundle);
			string[] result = (from file in assetBundle.GetAllAssetNames()
				select Path.GetFileNameWithoutExtension(file)).ToArray();
			if (loadedBundle == null)
			{
				assetBundle.Unload(unloadAllLoadedObjects: true);
			}
			return result;
		}
	}

	[Conditional("DATA_LOAD_ERROR")]
	public static void LogError(string str)
	{
	}

	public static List<string> GetAssetBundleNameListFromPath(string path, bool subdirCheck = false)
	{
		List<string> result = new List<string>();
		string basePath = AssetBundleManager.BaseDownloadingURL;
		string path2 = basePath + path;
		if (!Directory.Exists(path2))
		{
			return result;
		}
		string searchPattern = "*" + AssetBundleManager.Extension;
		string[] source = ((!subdirCheck) ? Directory.GetFiles(path2, searchPattern) : Directory.GetFiles(path2, searchPattern, SearchOption.AllDirectories));
		return source.Select((string s) => s.Replace(basePath, string.Empty)).ToList();
	}

	public void SetBundle(string bundle)
	{
		_bundle = bundle;
	}

	public void SetAsset(string asset)
	{
		_asset = asset;
	}

	public void ClearRequest()
	{
		_request = null;
	}

	public AssetBundleData()
	{
	}

	public AssetBundleData(string bundle, string asset)
	{
		_bundle = bundle;
		_asset = asset;
	}

	public bool Check(string bundle, string asset)
	{
		if (asset.IsNullOrEmpty() || !(_asset != asset))
		{
			if (!bundle.IsNullOrEmpty())
			{
				return _bundle != bundle;
			}
			return false;
		}
		return true;
	}

	public virtual AssetBundleLoadAssetOperation LoadBundle<T>() where T : UnityEngine.Object
	{
		if (!isFile)
		{
			return null;
		}
		return _request ?? (_request = AssetBundleManager.LoadAsset(this, typeof(T)));
	}

	public virtual async UniTask<AssetBundleLoadAssetOperation> LoadBundleAsync<T>() where T : UnityEngine.Object
	{
		if (!isFile)
		{
			return null;
		}
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = _request;
		if (assetBundleLoadAssetOperation == null)
		{
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation2 = (_request = await AssetBundleManager.LoadAssetAsync(this, typeof(T)));
			assetBundleLoadAssetOperation = assetBundleLoadAssetOperation2;
		}
		return assetBundleLoadAssetOperation;
	}

	public virtual AssetBundleLoadAssetOperation LoadAllBundle<T>() where T : UnityEngine.Object
	{
		if (!isFile)
		{
			return null;
		}
		return _request ?? (_request = AssetBundleManager.LoadAllAsset(this, typeof(T)));
	}

	public virtual async UniTask<AssetBundleLoadAssetOperation> LoadAllBundleAsync<T>() where T : UnityEngine.Object
	{
		if (!isFile)
		{
			return null;
		}
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = _request;
		if (assetBundleLoadAssetOperation == null)
		{
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation2 = (_request = await AssetBundleManager.LoadAllAssetAsync(this, typeof(T)));
			assetBundleLoadAssetOperation = assetBundleLoadAssetOperation2;
		}
		return assetBundleLoadAssetOperation;
	}

	public virtual T GetAsset<T>() where T : UnityEngine.Object
	{
		if (_request == null)
		{
			_request = LoadBundle<T>();
		}
		if (_request == null)
		{
			return null;
		}
		if (_request.IsEmpty())
		{
			return null;
		}
		return _request.GetAsset<T>();
	}

	public virtual T[] GetAllAssets<T>() where T : UnityEngine.Object
	{
		if (_request == null)
		{
			_request = LoadAllBundle<T>();
		}
		if (_request == null)
		{
			return null;
		}
		if (_request.IsEmpty())
		{
			return null;
		}
		return _request.GetAllAssets<T>();
	}

	public async UniTask<T> GetAssetAsync<T>() where T : UnityEngine.Object
	{
		if (_request == null)
		{
			_request = await LoadBundleAsync<T>();
		}
		if (_request == null)
		{
			return null;
		}
		if (_request.IsEmpty())
		{
			return null;
		}
		return _request.GetAsset<T>();
	}

	public async UniTask<T[]> GetAllAssetsAsync<T>() where T : UnityEngine.Object
	{
		if (_request == null)
		{
			_request = await LoadBundleAsync<T>();
		}
		if (_request == null)
		{
			return null;
		}
		if (_request.IsEmpty())
		{
			return null;
		}
		return _request.GetAllAssets<T>();
	}

	public virtual void UnloadBundle(bool isUnloadForceRefCount = false, bool unloadAllLoadedObjects = false)
	{
		if (_request != null)
		{
			AssetBundleManager.UnloadAssetBundle(this, isUnloadForceRefCount, unloadAllLoadedObjects);
		}
		_request = null;
	}

	[Conditional("DATA_LOAD_ERROR")]
	private void _LogErrorBundle(string bundle)
	{
	}

	[Conditional("DATA_LOAD_ERROR")]
	private void _LogErrorAsset(string asset, Type type)
	{
	}
}
