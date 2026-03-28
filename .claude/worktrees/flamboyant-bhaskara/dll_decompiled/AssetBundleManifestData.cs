using System;
using UniRx.Async;
using UnityEngine;

[Serializable]
public class AssetBundleManifestData : AssetBundleData
{
	[SerializeField]
	private string _manifest = "";

	public string manifest
	{
		get
		{
			return _manifest;
		}
		set
		{
			_manifest = value;
		}
	}

	public new bool isEmpty
	{
		get
		{
			if (!base.isEmpty)
			{
				return manifest.IsNullOrEmpty();
			}
			return true;
		}
	}

	public override LoadedAssetBundle LoadedBundle => AssetBundleManager.GetLoadedAssetBundle(base.bundle, _manifest);

	public AssetBundleManifestData()
	{
	}

	public AssetBundleManifestData(string bundle, string asset)
		: base(bundle, asset)
	{
	}

	public AssetBundleManifestData(string bundle, string asset, string manifest)
		: base(bundle, asset)
	{
		_manifest = manifest;
	}

	public bool Check(string bundle, string asset, string manifest)
	{
		if (manifest.IsNullOrEmpty() || !(_manifest != manifest))
		{
			return Check(bundle, asset);
		}
		return true;
	}

	public override AssetBundleLoadAssetOperation LoadBundle<T>()
	{
		object obj;
		if (base.isFile)
		{
			obj = base._request;
			if (obj == null)
			{
				return base._request = AssetBundleManager.LoadAsset(this, typeof(T));
			}
		}
		else
		{
			obj = null;
		}
		return (AssetBundleLoadAssetOperation)obj;
	}

	public override async UniTask<AssetBundleLoadAssetOperation> LoadBundleAsync<T>()
	{
		AssetBundleLoadAssetOperation result;
		if (!base.isFile)
		{
			result = null;
		}
		else
		{
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = base._request;
			if (assetBundleLoadAssetOperation == null)
			{
				AssetBundleLoadAssetOperation assetBundleLoadAssetOperation2 = (base._request = await AssetBundleManager.LoadAssetAsync(this, typeof(T)));
				assetBundleLoadAssetOperation = assetBundleLoadAssetOperation2;
			}
			result = assetBundleLoadAssetOperation;
		}
		return result;
	}

	public override AssetBundleLoadAssetOperation LoadAllBundle<T>()
	{
		object obj;
		if (base.isFile)
		{
			obj = base._request;
			if (obj == null)
			{
				return base._request = AssetBundleManager.LoadAllAsset(this, typeof(T));
			}
		}
		else
		{
			obj = null;
		}
		return (AssetBundleLoadAssetOperation)obj;
	}

	public override async UniTask<AssetBundleLoadAssetOperation> LoadAllBundleAsync<T>()
	{
		AssetBundleLoadAssetOperation result;
		if (!base.isFile)
		{
			result = null;
		}
		else
		{
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = base._request;
			if (assetBundleLoadAssetOperation == null)
			{
				AssetBundleLoadAssetOperation assetBundleLoadAssetOperation2 = (base._request = await AssetBundleManager.LoadAllAssetAsync(this, typeof(T)));
				assetBundleLoadAssetOperation = assetBundleLoadAssetOperation2;
			}
			result = assetBundleLoadAssetOperation;
		}
		return result;
	}

	public override void UnloadBundle(bool isUnloadForceRefCount = false, bool unloadAllLoadedObjects = false)
	{
		if (base._request != null)
		{
			AssetBundleManager.UnloadAssetBundle(this, isUnloadForceRefCount, unloadAllLoadedObjects);
		}
		base._request = null;
	}
}
