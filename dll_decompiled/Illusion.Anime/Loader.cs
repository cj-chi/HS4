using System.Collections.Generic;
using System.Linq;
using Illusion.Anime.Information;
using Manager;
using UnityEngine;

namespace Illusion.Anime;

public static class Loader
{
	public class AnimeEventInfo
	{
		public float normalizedTime { get; }

		public int eventID { get; }

		public AnimeEventInfo(float normalizedTime, int eventID)
		{
			this.normalizedTime = normalizedTime;
			this.eventID = eventID;
		}
	}

	public class ActionItemInfo
	{
		public AssetBundleManifestData data { get; }

		public bool exists { get; }

		public AssetBundleData animeData { get; }

		public ActionItemInfo(EventItemParameter.Param param)
		{
			data = param.data;
			exists = param.exists;
			animeData = param.animeData;
		}
	}

	public class ItemScaleInfo
	{
		public int mode { get; }

		public float S { get; }

		public float M { get; }

		public float L { get; }

		public ItemScaleInfo(AnimeItemScaleParameter.Param param)
			: this(param.mode, param.S, param.M, param.L)
		{
		}

		public ItemScaleInfo(int mode, float S, float M, float L)
		{
			this.mode = mode;
			this.S = S;
			this.M = M;
			this.L = L;
		}
	}

	private static Dictionary<int, Dictionary<int, List<AnimeEventInfo>>> _EventKeyTable;

	private static Dictionary<int, Dictionary<int, List<AnimeEventInfo>>> _EventItemKeyTable;

	public static IReadOnlyDictionary<int, PlayState> AnimePlayStateTable { get; private set; }

	public static IReadOnlyDictionary<int, Dictionary<int, List<AnimeEventInfo>>> EventKeyTable => _EventKeyTable;

	public static Dictionary<string, MotionIKData> IKData { get; } = new Dictionary<string, MotionIKData>();

	public static IReadOnlyDictionary<int, ActionItemInfo> EventItemList { get; private set; }

	public static IReadOnlyDictionary<int, Dictionary<int, List<AnimeEventInfo>>> EventItemKeyTable => _EventItemKeyTable;

	public static IReadOnlyDictionary<int, ItemScaleInfo> EventItemScaleTable { get; private set; }

	public static IReadOnlyDictionary<int, Dictionary<int, List<YureCtrl.Info>>> YureHTable { get; private set; }

	public static IReadOnlyDictionary<string, Dictionary<int, YureCtrl.Info>> YureNormalTable { get; private set; }

	public static void LoadAnimePlayStateTable(string path)
	{
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		Dictionary<int, PlayState> dictionary = new Dictionary<int, PlayState>();
		foreach (string item in assetBundleNameListFromPath)
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			AnimePlayStateParameter[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(AnimePlayStateParameter)).GetAllAssets<AnimePlayStateParameter>();
			for (int i = 0; i < allAssets.Length; i++)
			{
				foreach (AnimePlayStateParameter.Param item2 in allAssets[i].param)
				{
					dictionary[item2.poseID] = new PlayState(item2);
				}
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		AnimePlayStateTable = dictionary;
	}

	public static PlayState GetAnimePlayState(int poseID)
	{
		PlayState value = null;
		AnimePlayStateTable?.TryGetValue(poseID, out value);
		return value;
	}

	public static void LoadEventKeyTable(string path)
	{
		LoadEventKeyTable(path, out _EventKeyTable);
	}

	public static void LoadEventKeyTable(string path, out Dictionary<int, Dictionary<int, List<AnimeEventInfo>>> dest)
	{
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		Dictionary<int, Dictionary<int, List<AnimeEventInfo>>> dictionary = new Dictionary<int, Dictionary<int, List<AnimeEventInfo>>>();
		foreach (string item in assetBundleNameListFromPath)
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			AnimeEventParameter[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(AnimeEventParameter)).GetAllAssets<AnimeEventParameter>();
			for (int i = 0; i < allAssets.Length; i++)
			{
				foreach (AnimeEventParameter.Param item2 in allAssets[i].param)
				{
					if (!dictionary.TryGetValue(item2.poseID, out var value))
					{
						value = (dictionary[item2.poseID] = new Dictionary<int, List<AnimeEventInfo>>());
					}
					if (!value.TryGetValue(item2.nameHash, out var value2))
					{
						value2 = (value[item2.nameHash] = new List<AnimeEventInfo>());
					}
					value2.AddRange(item2.parameters.Select((AnimeEventParameter.Parameter x) => new AnimeEventInfo(x.normalizedTime, x.eventID)));
				}
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		dest = dictionary;
	}

	public static void LoadMotionIKDataFile(string file)
	{
		TextAsset[] allAssets = AssetBundleManager.LoadAllAsset(file, typeof(TextAsset)).GetAllAssets<TextAsset>();
		foreach (TextAsset textAsset in allAssets)
		{
			IKData[textAsset.name] = new MotionIKData(textAsset);
		}
		AssetBundleManager.UnloadAssetBundle(file, isUnloadForceRefCount: false);
	}

	public static void LoadMotionIKDataTable(string path)
	{
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			if (GameSystem.IsPathAdd50(item))
			{
				LoadMotionIKDataFile(item);
			}
		}
	}

	public static MotionIKData GetMotionIKData(string stateName)
	{
		IKData.TryGetValue(stateName, out var value);
		return value;
	}

	public static void LoadEventItemTable(string path)
	{
		Dictionary<int, ActionItemInfo> dictionary = new Dictionary<int, ActionItemInfo>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			EventItemParameter[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(EventItemParameter)).GetAllAssets<EventItemParameter>();
			for (int i = 0; i < allAssets.Length; i++)
			{
				foreach (EventItemParameter.Param item2 in allAssets[i].param)
				{
					dictionary[item2.ID] = new ActionItemInfo(item2);
				}
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		EventItemList = dictionary;
	}

	public static void LoadEventItemKeyTable(string path)
	{
		LoadEventKeyTable(path, out _EventItemKeyTable);
	}

	public static void LoadEventItemScaleTable(string path)
	{
		Dictionary<int, ItemScaleInfo> dictionary = new Dictionary<int, ItemScaleInfo>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			AnimeItemScaleParameter[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(AnimeItemScaleParameter)).GetAllAssets<AnimeItemScaleParameter>();
			for (int i = 0; i < allAssets.Length; i++)
			{
				foreach (AnimeItemScaleParameter.Param item2 in allAssets[i].param)
				{
					dictionary[item2.itemID] = new ItemScaleInfo(item2);
				}
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		EventItemScaleTable = dictionary;
	}

	public static void LoadYureH(string path)
	{
		Dictionary<int, Dictionary<int, List<YureCtrl.Info>>> dictionary = new Dictionary<int, Dictionary<int, List<YureCtrl.Info>>>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			YureParameterH[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(YureParameterH)).GetAllAssets<YureParameterH>();
			for (int i = 0; i < allAssets.Length; i++)
			{
				foreach (YureParameterH.Param item2 in allAssets[i].param)
				{
					if (!dictionary.TryGetValue(item2.ID, out var value))
					{
						value = (dictionary[item2.ID] = new Dictionary<int, List<YureCtrl.Info>>());
					}
					if (!value.TryGetValue(item2.info.nameHash, out var value2))
					{
						value2 = (value[item2.info.nameHash] = new List<YureCtrl.Info>());
					}
					value2.Add(item2.info);
				}
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		YureHTable = dictionary;
	}

	public static void LoadYureNormal(string path)
	{
		Dictionary<string, Dictionary<int, YureCtrl.Info>> dictionary = new Dictionary<string, Dictionary<int, YureCtrl.Info>>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			YureParameterNormal[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(YureParameterNormal)).GetAllAssets<YureParameterNormal>();
			foreach (YureParameterNormal yureParameterNormal in allAssets)
			{
				if (!dictionary.TryGetValue(yureParameterNormal.name, out var value))
				{
					value = (dictionary[yureParameterNormal.name] = new Dictionary<int, YureCtrl.Info>());
				}
				foreach (YureParameterNormal.Param item2 in yureParameterNormal.param)
				{
					value[item2.info.nameHash] = item2.info;
				}
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		YureNormalTable = dictionary;
	}
}
