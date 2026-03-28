using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using Map;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Manager;

public class BaseMap : SingletonInitializer<BaseMap>
{
	private static GameObject mobRoot = null;

	private static MapBobList mobList = null;

	private static SunLightInfo _sunLightInfo = null;

	private List<GameObject> emptyMapObject = new List<GameObject>();

	public static IReadOnlyDictionary<int, MapInfo.Param> infoTable { get; private set; }

	public static string MAP_ROOT_NAME { get; } = "Map";

	public static GameObject mapRoot { get; private set; }

	public static bool isMapLoading { get; protected set; }

	public static int no { get; protected set; } = -1;

	public static int prevNo { get; protected set; } = -1;

	private static string MAP_OBJECT_GROUP { get; } = "MapObjectGroup";

	private static string POST_PROCESS_VOLUME_DEFAULT_NAME { get; } = "PostProcessVolume (default)";

	private static string POST_PROCESS_VOLUME_USER_NAME { get; } = "PostProcessVolume (user)";

	private static string POST_PROCESS_VOLUME_COLOR_NAME { get; } = "PostProcessVolume (color)";

	private static string REFLECTION_PROBE_NAME { get; } = "Reflection Probe";

	private static string MOB_OBJECT { get; } = "p_h2_blueman00";

	public static MapInfo.Param Info => infoTable[no];

	public static MapVisibleList mapVisibleList { get; private set; }

	public static Transform mapObjectGroup { get; private set; }

	public static SunLightInfo sunLightInfo
	{
		get
		{
			if (!(mapRoot == null))
			{
				return mapRoot.GetComponentCache(ref _sunLightInfo);
			}
			return null;
		}
	}

	public static PostProcessVolume postProcessVolume_default { get; private set; }

	public static PostProcessVolume postProcessVolume_user { get; private set; }

	public static PostProcessVolume postProcessVolume_color { get; private set; }

	public static ReflectionProbe reflectionProbe { get; private set; }

	private static PostProcessProfile profile { get; set; } = null;

	public void UpdateCameraFog()
	{
	}

	public static void Change(int _no, FadeCanvas.Fade fadeType = FadeCanvas.Fade.InOut, bool isForce = false)
	{
		ChangeAsync(_no, fadeType, isForce).Forget();
	}

	public static void Change(string mapName, FadeCanvas.Fade fadeType = FadeCanvas.Fade.InOut)
	{
		ChangeAsync(ConvertMapNo(mapName), fadeType).Forget();
	}

	public static async UniTask ChangeAsync(int _no, FadeCanvas.Fade fadeType = FadeCanvas.Fade.InOut, bool isForce = false)
	{
		if (!(mapRoot != null) || no != _no || isForce)
		{
			isMapLoading = true;
			prevNo = no;
			no = _no;
			MapInfo.Param info = Info;
			await Scene.LoadBaseSceneAsync(new Scene.Data
			{
				bundleName = info.AssetBundleName,
				levelName = info.AssetName,
				fadeType = fadeType
			});
		}
	}

	public static async UniTask ChangeAsync(string mapName, FadeCanvas.Fade fadeType = FadeCanvas.Fade.InOut)
	{
		await ChangeAsync(ConvertMapNo(mapName), fadeType);
	}

	public static MapInfo.Param GetParam(int no)
	{
		infoTable.TryGetValue(no, out var value);
		return value;
	}

	public static MapInfo.Param GetParam(string mapName)
	{
		return infoTable.Values.FirstOrDefault((MapInfo.Param p) => p.MapNames[0] == mapName);
	}

	public static int ConvertMapNo(string mapName)
	{
		return GetParam(mapName)?.No ?? (-1);
	}

	public static string ConvertMapName(int no)
	{
		MapInfo.Param param = GetParam(no);
		if (param == null)
		{
			return string.Empty;
		}
		return param.MapNames[0];
	}

	public static string ConvertMapNameEnglish(int no)
	{
		MapInfo.Param param = GetParam(no);
		if (param == null)
		{
			return string.Empty;
		}
		return param.MapNames[1];
	}

	public static string ConvertMapAssetName(int no)
	{
		MapInfo.Param param = GetParam(no);
		if (param == null)
		{
			return string.Empty;
		}
		return param.AssetName;
	}

	private static void LoadMapInfo()
	{
		Dictionary<int, MapInfo.Param> dictionary = new Dictionary<int, MapInfo.Param>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(AssetBundleNames.MapListMapinfoPath, subdirCheck: true);
		_ = Singleton<GameSystem>.Instance;
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			foreach (List<MapInfo.Param> item2 in from p in AssetBundleManager.LoadAllAsset(item, typeof(MapInfo)).GetAllAssets<MapInfo>()
				select p.param)
			{
				foreach (MapInfo.Param item3 in item2)
				{
					dictionary[item3.No] = item3;
				}
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		infoTable = dictionary;
	}

	public void SetCycleToSunLight(int _charaLightSet)
	{
		bool flag = false;
		if (sunLightInfo != null)
		{
			flag = sunLightInfo.Set(Camera.main, _charaLightSet);
		}
		UpdateCameraFog();
	}

	public void SetDefaultReflectProbeTextrure()
	{
		if (sunLightInfo != null)
		{
			sunLightInfo.SetDefaultReflectProbe();
		}
	}

	protected override void Initialize()
	{
		SceneManager.activeSceneChanged += Reserve;
		this.OnDestroyAsObservable().Subscribe(delegate
		{
			SceneManager.activeSceneChanged -= Reserve;
		});
		LoadMapInfo();
	}

	public void MapVisible(bool _visible)
	{
		foreach (GameObject item in sunLightInfo ? sunLightInfo.RootObjectMaps : emptyMapObject)
		{
			item.SetActiveIfDifferent(_visible);
		}
	}

	public void EnvironmentLightObjectsVisible(bool _visible)
	{
		foreach (GameObject item in sunLightInfo ? sunLightInfo.EnvironmentLightObjects : emptyMapObject)
		{
			item.SetActiveIfDifferent(_visible);
		}
	}

	public void MobObjectsVisible(bool _visible)
	{
		if ((bool)mobRoot)
		{
			mobRoot.SetActiveIfDifferent(_visible);
		}
	}

	protected virtual void Reserve(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
	{
		if (!isMapLoading)
		{
			return;
		}
		mapRoot = newScene.GetRootGameObjects().FirstOrDefault((GameObject p) => p.name == MAP_ROOT_NAME);
		if (!(mapRoot == null))
		{
			isMapLoading = false;
			int cycleToSunLight = -1;
			List<string> second = new List<string> { "ADV", "ADV_Sample", "ADVCharaLightKakunin" };
			if (Scene.NowSceneNames.Contains("HScene"))
			{
				cycleToSunLight = 0;
			}
			else if (Scene.NowSceneNames.Intersect(second).Any())
			{
				cycleToSunLight = 1;
			}
			SetCycleToSunLight(cycleToSunLight);
			_ = Info;
			mapVisibleList = mapRoot.GetComponent<MapVisibleList>();
			mapObjectGroup = mapRoot.transform.FindLoop(MAP_OBJECT_GROUP);
			Transform transform = mapRoot.transform.FindLoop(POST_PROCESS_VOLUME_DEFAULT_NAME);
			if ((bool)transform)
			{
				postProcessVolume_default = transform.GetComponent<PostProcessVolume>();
			}
			transform = mapRoot.transform.FindLoop(POST_PROCESS_VOLUME_USER_NAME);
			if ((bool)transform)
			{
				postProcessVolume_user = transform.GetComponent<PostProcessVolume>();
			}
			transform = mapRoot.transform.FindLoop(POST_PROCESS_VOLUME_COLOR_NAME);
			if ((bool)transform)
			{
				postProcessVolume_color = transform.GetComponent<PostProcessVolume>();
			}
			transform = mapRoot.transform.FindLoop(REFLECTION_PROBE_NAME);
			if ((bool)transform)
			{
				reflectionProbe = transform.GetComponent<ReflectionProbe>();
			}
			Transform transform2 = mapRoot.transform.FindLoop(MOB_OBJECT);
			if ((bool)transform2)
			{
				mobRoot = transform2.gameObject;
			}
			mobList = mapRoot.GetComponentInChildren<MapBobList>(includeInactive: true);
		}
	}
}
