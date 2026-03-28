using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
	public class BundlePack
	{
		public AssetBundleManifest AssetBundleManifest
		{
			get
			{
				return _assetBundleManifest;
			}
			set
			{
				_assetBundleManifest = value;
			}
		}

		private AssetBundleManifest _assetBundleManifest { get; set; }

		public Dictionary<string, LoadedAssetBundle> LoadedAssetBundles => _loadedAssetBundles;

		private Dictionary<string, LoadedAssetBundle> _loadedAssetBundles { get; } = new Dictionary<string, LoadedAssetBundle>();

		public ICollection<LoadedAssetBundleDependencies> Dependencies => _dependencies;

		private List<LoadedAssetBundleDependencies> _dependencies { get; } = new List<LoadedAssetBundleDependencies>();

		public string[] Variants
		{
			get
			{
				return _variants;
			}
			set
			{
				_variants = value;
			}
		}

		private string[] _variants { get; set; } = new string[0];
	}

	public static bool initialized { get; private set; }

	public static string MAIN_MANIFEST_NAME { get; } = "abdata";

	public static string Extension { get; } = ".unity3d";

	public static HashSet<string> AllLoadedAssetBundleNames => allLoadedAssetBundleNames;

	private static HashSet<string> allLoadedAssetBundleNames { get; } = new HashSet<string>();

	private static BundlePack MainBundle { get; set; } = null;

	public static Dictionary<string, BundlePack> ManifestBundlePack => manifestBundlePack;

	private static Dictionary<string, BundlePack> manifestBundlePack { get; } = new Dictionary<string, BundlePack>();

	public static string BaseDownloadingURL => baseDownloadingURL;

	private static string baseDownloadingURL { get; set; } = "";

	public static BundlePack ManifestAdd(string manifestAssetBundleName)
	{
		if (manifestBundlePack.ContainsKey(manifestAssetBundleName))
		{
			return null;
		}
		BundlePack bundlePack = new BundlePack();
		manifestBundlePack.Add(manifestAssetBundleName, bundlePack);
		LoadedAssetBundle loadedAssetBundle = LoadAssetBundle(manifestAssetBundleName, manifestAssetBundleName);
		if (loadedAssetBundle == null)
		{
			manifestBundlePack.Remove(manifestAssetBundleName);
			return null;
		}
		AssetBundleLoadAssetOperationSimulation assetBundleLoadAssetOperationSimulation = new AssetBundleLoadAssetOperationSimulation(loadedAssetBundle.Bundle.LoadAsset("AssetBundleManifest", typeof(AssetBundleManifest)));
		if (assetBundleLoadAssetOperationSimulation.IsEmpty())
		{
			manifestBundlePack.Remove(manifestAssetBundleName);
			return null;
		}
		bundlePack.AssetBundleManifest = assetBundleLoadAssetOperationSimulation.GetAsset<AssetBundleManifest>();
		return bundlePack;
	}

	public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, string manifestAssetBundleName = null)
	{
		assetBundleName = assetBundleName ?? string.Empty;
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		BundlePack bundlePack = manifestBundlePack[manifestAssetBundleName];
		bundlePack.LoadedAssetBundles.TryGetValue(assetBundleName, out var value);
		if (value == null)
		{
			return null;
		}
		LoadedAssetBundleDependencies loadedAssetBundleDependencies = bundlePack.Dependencies.FirstOrDefault((LoadedAssetBundleDependencies p) => p.Key == assetBundleName);
		if (loadedAssetBundleDependencies == null)
		{
			return value;
		}
		string[] bundleNames = loadedAssetBundleDependencies.BundleNames;
		foreach (string key in bundleNames)
		{
			bundlePack.LoadedAssetBundles.TryGetValue(key, out var value2);
			if (value2 == null)
			{
				return null;
			}
		}
		return value;
	}

	public static void Initialize(string basePath)
	{
		if (initialized)
		{
			return;
		}
		baseDownloadingURL = basePath;
		if (MainBundle == null)
		{
			MainBundle = ManifestAdd(MAIN_MANIFEST_NAME);
		}
		if (Directory.Exists(basePath))
		{
			foreach (string item in from s in Directory.GetFiles(basePath, "*.*", SearchOption.TopDirectoryOnly)
				where Path.GetExtension(s).IsNullOrEmpty()
				select Path.GetFileNameWithoutExtension(s) into s
				where s != MAIN_MANIFEST_NAME
				select s)
			{
				ManifestAdd(item);
			}
		}
		initialized = true;
	}

	public static LoadedAssetBundle LoadAssetBundle(string assetBundleName, string manifestAssetBundleName = null)
	{
		bool flag = assetBundleName == manifestAssetBundleName;
		if (!flag)
		{
			assetBundleName = RemapVariantName(assetBundleName, manifestAssetBundleName);
		}
		if (!LoadAssetBundleInternal(assetBundleName, manifestAssetBundleName) && !flag)
		{
			LoadDependencies(assetBundleName, manifestAssetBundleName);
		}
		return GetLoadedAssetBundle(assetBundleName, manifestAssetBundleName);
	}

	public static async UniTask<LoadedAssetBundle> LoadAssetBundleAsync(string assetBundleName, string manifestAssetBundleName = null)
	{
		bool isLoadingAssetBundleManifest = assetBundleName == manifestAssetBundleName;
		if (!isLoadingAssetBundleManifest)
		{
			assetBundleName = RemapVariantName(assetBundleName, manifestAssetBundleName);
		}
		if (!(await LoadAssetBundleInternalAsync(assetBundleName, manifestAssetBundleName)) && !isLoadingAssetBundleManifest)
		{
			await LoadDependenciesAsync(assetBundleName, manifestAssetBundleName);
		}
		return GetLoadedAssetBundle(assetBundleName, manifestAssetBundleName);
	}

	private static string RemapVariantName(string assetBundleName, string manifestAssetBundleName = null)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		BundlePack bundlePack = manifestBundlePack[manifestAssetBundleName];
		string[] allAssetBundlesWithVariant = bundlePack.AssetBundleManifest.GetAllAssetBundlesWithVariant();
		if (Array.IndexOf(allAssetBundlesWithVariant, assetBundleName) < 0)
		{
			return assetBundleName;
		}
		string[] array = assetBundleName.Split('.');
		int num = int.MaxValue;
		int num2 = -1;
		for (int i = 0; i < allAssetBundlesWithVariant.Length; i++)
		{
			string[] array2 = allAssetBundlesWithVariant[i].Split('.');
			if (!(array2[0] != array[0]))
			{
				int num3 = Array.IndexOf(bundlePack.Variants, array2[1]);
				if (num3 != -1 && num3 < num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		if (num2 != -1)
		{
			return allAssetBundlesWithVariant[num2];
		}
		return assetBundleName;
	}

	private static bool IsLoadAssetBundleInternalLoaded(string bundle, string manifest, out BundlePack target)
	{
		if (manifest.IsNullOrEmpty())
		{
			manifest = MAIN_MANIFEST_NAME;
		}
		target = manifestBundlePack[manifest];
		target.LoadedAssetBundles.TryGetValue(bundle, out var value);
		if (value != null)
		{
			value.ReferencedCount++;
			return true;
		}
		if (!allLoadedAssetBundleNames.Add(bundle))
		{
			return true;
		}
		return false;
	}

	public static bool LoadAssetBundleInternal(string assetBundleName, string manifestAssetBundleName = null)
	{
		if (IsLoadAssetBundleInternalLoaded(assetBundleName, manifestAssetBundleName, out var target))
		{
			return true;
		}
		target.LoadedAssetBundles.Add(assetBundleName, new LoadedAssetBundle(AssetBundle.LoadFromFile(BaseDownloadingURL + assetBundleName)));
		return false;
	}

	public static async UniTask<bool> LoadAssetBundleInternalAsync(string assetBundleName, string manifestAssetBundleName = null)
	{
		if (IsLoadAssetBundleInternalLoaded(assetBundleName, manifestAssetBundleName, out var targetPack))
		{
			return true;
		}
		AssetBundleCreateRequest ope = AssetBundle.LoadFromFileAsync(BaseDownloadingURL + assetBundleName);
		await ope;
		targetPack.LoadedAssetBundles.Add(assetBundleName, new LoadedAssetBundle(ope.assetBundle));
		return false;
	}

	private static bool LoadDependenciesCheck(string assetBundleName, string manifestAssetBundleName, out string[] dependencies)
	{
		dependencies = null;
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		BundlePack bundlePack = manifestBundlePack[manifestAssetBundleName];
		if ((object)bundlePack.AssetBundleManifest == null)
		{
			return false;
		}
		dependencies = bundlePack.AssetBundleManifest.GetAllDependencies(assetBundleName);
		if (dependencies.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < dependencies.Length; i++)
		{
			dependencies[i] = RemapVariantName(dependencies[i], manifestAssetBundleName);
		}
		LoadedAssetBundleDependencies loadedAssetBundleDependencies = bundlePack.Dependencies.FirstOrDefault((LoadedAssetBundleDependencies p) => p.Key == assetBundleName);
		if (loadedAssetBundleDependencies != null)
		{
			loadedAssetBundleDependencies.ReferencedCount++;
		}
		else
		{
			bundlePack.Dependencies.Add(new LoadedAssetBundleDependencies(assetBundleName, dependencies));
		}
		return true;
	}

	private static void LoadDependencies(string assetBundleName, string manifestAssetBundleName = null)
	{
		if (LoadDependenciesCheck(assetBundleName, manifestAssetBundleName, out var dependencies))
		{
			for (int i = 0; i < dependencies.Length; i++)
			{
				LoadAssetBundleInternal(dependencies[i], manifestAssetBundleName);
			}
		}
	}

	private static async UniTask LoadDependenciesAsync(string assetBundleName, string manifestAssetBundleName = null)
	{
		if (LoadDependenciesCheck(assetBundleName, manifestAssetBundleName, out var dependencies))
		{
			int i = 0;
			while (i < dependencies.Length)
			{
				await LoadAssetBundleInternalAsync(dependencies[i], manifestAssetBundleName);
				int num = i + 1;
				i = num;
			}
		}
	}

	public static void UnloadAssetBundle(AssetBundleData data, bool isUnloadForceRefCount, bool unloadAllLoadedObjects = false)
	{
		UnloadAssetBundle(data.bundle, isUnloadForceRefCount, null, unloadAllLoadedObjects);
	}

	public static void UnloadAssetBundle(AssetBundleManifestData data, bool isUnloadForceRefCount, bool unloadAllLoadedObjects = false)
	{
		UnloadAssetBundle(data.bundle, isUnloadForceRefCount, data.manifest, unloadAllLoadedObjects);
	}

	public static void UnloadAssetBundle(string assetBundleName, bool isUnloadForceRefCount, string manifestAssetBundleName = null, bool unloadAllLoadedObjects = false)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		while (UnloadBundleAndDependencies(assetBundleName, manifestAssetBundleName, unloadAllLoadedObjects) && isUnloadForceRefCount)
		{
		}
	}

	private static bool UnloadBundleAndDependencies(string assetBundleName, string manifestAssetBundleName, bool unloadAllLoadedObjects)
	{
		BundlePack bundlePack = manifestBundlePack[manifestAssetBundleName];
		bool flag = UnloadBundle(assetBundleName, bundlePack, unloadAllLoadedObjects);
		if (flag)
		{
			LoadedAssetBundleDependencies loadedAssetBundleDependencies = bundlePack.Dependencies.FirstOrDefault((LoadedAssetBundleDependencies p) => p.Key == assetBundleName);
			if (loadedAssetBundleDependencies != null && --loadedAssetBundleDependencies.ReferencedCount == 0)
			{
				string[] bundleNames = loadedAssetBundleDependencies.BundleNames;
				for (int num = 0; num < bundleNames.Length; num++)
				{
					UnloadBundle(bundleNames[num], bundlePack, unloadAllLoadedObjects);
				}
				bundlePack.Dependencies.Remove(loadedAssetBundleDependencies);
			}
		}
		return flag;
	}

	private static bool UnloadBundle(string assetBundleName, BundlePack targetPack, bool unloadAllLoadedObjects)
	{
		assetBundleName = assetBundleName ?? string.Empty;
		if (!targetPack.LoadedAssetBundles.TryGetValue(assetBundleName, out var value))
		{
			return false;
		}
		if (--value.ReferencedCount == 0)
		{
			if ((bool)value.Bundle)
			{
				value.Bundle.Unload(unloadAllLoadedObjects);
			}
			targetPack.LoadedAssetBundles.Remove(assetBundleName);
			allLoadedAssetBundleNames.Remove(assetBundleName);
		}
		return true;
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			Initialize(Application.dataPath + "/../" + MAIN_MANIFEST_NAME + "/");
		}
	}

	public static AssetBundleLoadAssetOperation LoadAsset(AssetBundleData data, Type type)
	{
		return LoadAsset(data.bundle, data.asset, type);
	}

	public static AssetBundleLoadAssetOperation LoadAsset(AssetBundleManifestData data, Type type)
	{
		return LoadAsset(data.bundle, data.asset, type, data.manifest);
	}

	public static AssetBundleLoadAssetOperation LoadAsset(string assetBundleName, string assetName, Type type, string manifestAssetBundleName = null)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		BundlePack bundlePack = manifestBundlePack[manifestAssetBundleName];
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = null;
		if (assetBundleLoadAssetOperation == null)
		{
			LoadAssetBundle(assetBundleName, manifestAssetBundleName);
			assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationSimulation(bundlePack.LoadedAssetBundles[assetBundleName].Bundle.LoadAsset(assetName, type));
		}
		return assetBundleLoadAssetOperation;
	}

	public static async UniTask<AssetBundleLoadAssetOperation> LoadAssetAsync(AssetBundleData data, Type type)
	{
		return await LoadAssetAsync(data.bundle, data.asset, type);
	}

	public static async UniTask<AssetBundleLoadAssetOperation> LoadAssetAsync(AssetBundleManifestData data, Type type)
	{
		return await LoadAssetAsync(data.bundle, data.asset, type, data.manifest);
	}

	public static async UniTask<AssetBundleLoadAssetOperation> LoadAssetAsync(string assetBundleName, string assetName, Type type, string manifestAssetBundleName = null)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		BundlePack targetPack = manifestBundlePack[manifestAssetBundleName];
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = null;
		if (assetBundleLoadAssetOperation == null)
		{
			await LoadAssetBundleAsync(assetBundleName, manifestAssetBundleName);
			AssetBundleRequest request = targetPack.LoadedAssetBundles[assetBundleName].Bundle.LoadAssetAsync(assetName, type);
			await request;
			assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationSimulation(request.asset);
		}
		return assetBundleLoadAssetOperation;
	}

	public static AssetBundleLoadAssetOperation LoadAllAsset(AssetBundleData data, Type type)
	{
		return LoadAllAsset(data.bundle, type);
	}

	public static AssetBundleLoadAssetOperation LoadAllAsset(AssetBundleManifestData data, Type type)
	{
		return LoadAllAsset(data.bundle, type, data.manifest);
	}

	public static AssetBundleLoadAssetOperation LoadAllAsset(string assetBundleName, Type type, string manifestAssetBundleName = null)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		BundlePack bundlePack = manifestBundlePack[manifestAssetBundleName];
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = null;
		if (assetBundleLoadAssetOperation == null)
		{
			LoadAssetBundle(assetBundleName, manifestAssetBundleName);
			assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationSimulation(bundlePack.LoadedAssetBundles[assetBundleName].Bundle.LoadAllAssets(type));
		}
		return assetBundleLoadAssetOperation;
	}

	public static async UniTask<AssetBundleLoadAssetOperation> LoadAllAssetAsync(AssetBundleData data, Type type)
	{
		return await LoadAllAssetAsync(data.bundle, type);
	}

	public static async UniTask<AssetBundleLoadAssetOperation> LoadAllAssetAsync(AssetBundleManifestData data, Type type)
	{
		return await LoadAllAssetAsync(data.bundle, type, data.manifest);
	}

	public static async UniTask<AssetBundleLoadAssetOperation> LoadAllAssetAsync(string assetBundleName, Type type, string manifestAssetBundleName = null)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		BundlePack targetPack = manifestBundlePack[manifestAssetBundleName];
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = null;
		if (assetBundleLoadAssetOperation == null)
		{
			await LoadAssetBundleAsync(assetBundleName, manifestAssetBundleName);
			AssetBundleRequest request = targetPack.LoadedAssetBundles[assetBundleName].Bundle.LoadAllAssetsAsync(type);
			await request;
			assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationSimulation(request.allAssets);
		}
		return assetBundleLoadAssetOperation;
	}

	public static void LoadLevel(string assetBundleName, string levelName, bool isAdditive, string manifestAssetBundleName = null)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		LoadAssetBundle(assetBundleName, manifestAssetBundleName);
		SceneManager.LoadScene(levelName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
	}

	public static async UniTask<AsyncOperation> LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive, string manifestAssetBundleName = null)
	{
		if (manifestAssetBundleName.IsNullOrEmpty())
		{
			manifestAssetBundleName = MAIN_MANIFEST_NAME;
		}
		_ = manifestBundlePack[manifestAssetBundleName];
		await LoadAssetBundleAsync(assetBundleName, manifestAssetBundleName);
		return SceneManager.LoadSceneAsync(levelName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
	}
}
