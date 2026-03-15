using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Manager;

public class Resources : Singleton<Resources>
{
	private IConnectableObservable<Unit> _loadMapResourceStream;

	private List<(string, string)> _loadedAssetBundles = new List<(string, string)>();

	private readonly string _mainManifestName = "abdata";

	public IObservable<Unit> LoadMapResourceStream => _loadMapResourceStream;

	public bool LoadAssetBundle { get; set; }

	public void BeginLoadAssetBundle()
	{
		_loadedAssetBundles.Clear();
	}

	public void AddLoadAssetBundle(string assetBundleName, string manifestName)
	{
		if (manifestName.IsNullOrEmpty())
		{
			manifestName = _mainManifestName;
		}
		if (!_loadedAssetBundles.Exists(((string, string) x) => x.Item1 == assetBundleName && x.Item2 == manifestName))
		{
			_loadedAssetBundles.Add((assetBundleName, manifestName));
		}
	}

	public void EndLoadAssetBundle(bool forceRemove = false)
	{
		foreach (var loadedAssetBundle in _loadedAssetBundles)
		{
			AssetBundleManager.UnloadAssetBundle(loadedAssetBundle.Item1, isUnloadForceRefCount: true, null, forceRemove);
		}
		UnityEngine.Resources.UnloadUnusedAssets();
		GC.Collect();
		_loadedAssetBundles.Clear();
	}

	protected override void Awake()
	{
		CheckInstance();
	}

	public void SetupMap()
	{
		_loadMapResourceStream = Observable.FromCoroutine(() => LoadMapResources()).PublishLast();
		_loadMapResourceStream.Connect();
	}

	public IEnumerator LoadMapResources()
	{
		GC.Collect();
		yield return null;
	}

	public void ReleaseMapResources()
	{
		GC.Collect();
	}
}
