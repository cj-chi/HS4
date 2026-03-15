using System.Collections;
using Manager;
using UnityEngine;

namespace Studio;

public class Map : Singleton<Map>
{
	[SerializeField]
	private Light lightSunSource;

	public GameObject MapRoot { get; private set; }

	public bool isLoading { get; private set; }

	public int no { get; private set; } = -1;

	private MapComponent MapComponent { get; set; }

	public bool IsOption
	{
		get
		{
			if (MapComponent != null)
			{
				return MapComponent.CheckOption;
			}
			return false;
		}
	}

	public bool VisibleOption
	{
		get
		{
			return Singleton<Studio>.Instance.sceneInfo.mapInfo.option;
		}
		set
		{
			Singleton<Studio>.Instance.sceneInfo.mapInfo.option = value;
			MapComponent?.SetOptionVisible(value);
		}
	}

	public bool IsLight
	{
		get
		{
			if (MapComponent != null)
			{
				return MapComponent.IsLight;
			}
			return false;
		}
	}

	public bool VisibleLight
	{
		get
		{
			return Singleton<Studio>.Instance.sceneInfo.mapInfo.light;
		}
		set
		{
			Singleton<Studio>.Instance.sceneInfo.mapInfo.light = value;
			MapComponent?.SetLightVisible(value);
		}
	}

	public IEnumerator LoadMapCoroutine(int _no, bool _wait = false)
	{
		if (!Singleton<Info>.Instance.dicMapLoadInfo.ContainsKey(_no))
		{
			ReleaseMap();
		}
		else if (no != _no)
		{
			MapComponent = null;
			if (_wait)
			{
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "StudioWait",
					isAdd = true
				}, isLoadingImageDraw: false);
			}
			isLoading = true;
			no = _no;
			Info.MapLoadInfo data = Singleton<Info>.Instance.dicMapLoadInfo[_no];
			Scene.LoadBaseScene(new Scene.Data
			{
				bundleName = data.bundlePath,
				levelName = data.fileName,
				manifestName = (AssetBundleCheck.IsSimulation ? null : (data.manifest.IsNullOrEmpty() ? null : data.manifest)),
				fadeType = FadeCanvas.Fade.None,
				onLoad = delegate
				{
					OnLoadAfter(data.fileName);
				}
			});
			yield return new WaitWhile(() => isLoading);
			if (_wait)
			{
				Scene.Unload();
			}
		}
	}

	public void LoadMap(int _no)
	{
		if (!Singleton<Info>.Instance.dicMapLoadInfo.ContainsKey(_no))
		{
			ReleaseMap();
		}
		else if (no != _no)
		{
			MapComponent = null;
			isLoading = true;
			no = _no;
			Info.MapLoadInfo data = Singleton<Info>.Instance.dicMapLoadInfo[_no];
			Scene.LoadBaseScene(new Scene.Data
			{
				bundleName = data.bundlePath,
				levelName = data.fileName,
				manifestName = (AssetBundleCheck.IsSimulation ? null : (data.manifest.IsNullOrEmpty() ? null : data.manifest)),
				fadeType = FadeCanvas.Fade.None,
				onLoad = delegate
				{
					OnLoadAfter(data.fileName);
				}
			});
		}
	}

	public void ReleaseMap()
	{
		if (Singleton<Map>.IsInstance())
		{
			MapRoot = null;
			no = -1;
			MapComponent = null;
			Scene.UnloadBaseScene();
		}
	}

	private void OnLoadAfter(string _levelName)
	{
		MapRoot = Scene.GetScene(_levelName).GetRootGameObjects().SafeGet(0);
		MapComponent = MapRoot?.GetComponentInChildren<MapComponent>();
		if (MapComponent != null)
		{
			MapComponent.SetupSea();
		}
		if (Singleton<MapCtrl>.IsInstance())
		{
			Singleton<MapCtrl>.Instance.Reflect();
		}
		RenderSettings.sun = lightSunSource;
		if (Singleton<Studio>.IsInstance())
		{
			Singleton<Studio>.Instance.systemButtonCtrl.Apply();
		}
		isLoading = false;
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			Object.DontDestroyOnLoad(base.gameObject);
			isLoading = false;
			no = -1;
			MapRoot = null;
		}
	}
}
