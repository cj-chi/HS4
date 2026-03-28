using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illusion.Extensions;
using Manager;
using UnityEngine;

namespace ADV;

public static class Program
{
	public class Transfer
	{
		public int line { get; private set; }

		public ScenarioData.Param param { get; private set; }

		public Transfer(ScenarioData.Param param)
		{
			line = -1;
			this.param = param;
		}

		public static List<Transfer> NewList(bool multi = true, bool isSceneRegulate = false)
		{
			List<Transfer> list = new List<Transfer>();
			list.Add(Create(multi, Command.SceneFadeRegulate, isSceneRegulate.ToString()));
			return list;
		}

		public static Transfer Create(bool multi, Command command, params string[] args)
		{
			return new Transfer(new ScenarioData.Param(multi, command, args));
		}

		public static Transfer VAR(params string[] args)
		{
			return Create(multi: true, Command.VAR, args);
		}

		public static Transfer Open(params string[] args)
		{
			return Create(multi: false, Command.Open, args);
		}

		public static Transfer Close()
		{
			return Create(multi: false, Command.Close, (string[])null);
		}

		public static Transfer Text(params string[] args)
		{
			return Create(multi: false, Command.Text, args);
		}

		public static Transfer Voice(params string[] args)
		{
			return Create(multi: true, Command.Voice, args);
		}

		public static Transfer Motion(params string[] args)
		{
			return Create(multi: true, Command.Motion, args);
		}

		public static Transfer Expression(params string[] args)
		{
			return Create(multi: true, Command.Expression, args);
		}

		public static Transfer ExpressionIcon(params string[] args)
		{
			return Create(multi: true, Command.ExpressionIcon, args);
		}
	}

	public class OpenDataProc
	{
		public Action onLoad { get; set; }

		public Func<IEnumerator> onFadeIn { get; set; }

		public Func<IEnumerator> onFadeOut { get; set; }
	}

	public static string BASE_FOV { get; } = "23";

	public static string ScenarioBundle(string file)
	{
		return AssetBundleNames.AdvScenarioPath + file + ".unity3d";
	}

	public static void SetNull(Transform transform, int version, string mapName, string nullName)
	{
		if (GetNull(version, mapName, nullName, out var position, out var rotation))
		{
			transform.SetPositionAndRotation(position, rotation);
		}
	}

	public static bool GetNull(int version, string mapName, string nullName, out Vector3 position, out Quaternion rotation)
	{
		bool result = false;
		position = Vector3.zero;
		rotation = Quaternion.identity;
		mapName = ((!(mapName == "デフォルト")) ? BaseMap.GetParam(mapName).AssetName.ToLower() : "default");
		string assetBundleName = $"{AssetBundleNames.MapAdvposPath}{version:00}/{mapName}{AssetBundleManager.Extension}";
		GameObject asset = AssetBundleManager.LoadAsset(assetBundleName, nullName, typeof(GameObject)).GetAsset<GameObject>();
		if (asset != null)
		{
			Transform transform = asset.transform;
			position = transform.position;
			rotation = transform.rotation;
			result = true;
		}
		AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: false, null, unloadAllLoadedObjects: true);
		return result;
	}

	public static string FindADVBundleFilePath(string path, string asset = null)
	{
		ScenarioData data;
		return FindADVBundleFilePath(path, asset, out data);
	}

	public static string FindADVBundleFilePath(string path, string asset, out ScenarioData data)
	{
		data = null;
		string text = null;
		foreach (string item in from bundle in CommonLib.GetAssetBundleNameListFromPath("adv/scenario/" + path + "/ ", subdirCheck: true)
			orderby bundle descending
			select bundle)
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			if (asset == null)
			{
				text = item;
			}
			else
			{
				ScenarioData[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(ScenarioData)).GetAllAssets<ScenarioData>();
				foreach (ScenarioData scenarioData in allAssets)
				{
					if (scenarioData.name == asset)
					{
						text = item;
						data = scenarioData;
						break;
					}
				}
				AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
			}
			if (text != null)
			{
				break;
			}
		}
		return text;
	}

	public static string FindADVBundleFilePath(int id, int category, string asset = null)
	{
		ScenarioData data;
		return FindADVBundleFilePath(id, category, asset, out data);
	}

	public static string FindADVBundleFilePath(int id, int category, string asset, out ScenarioData data)
	{
		data = null;
		string text = null;
		string text2 = $"{category:00}";
		string text3 = id.MinusThroughToString("00");
		string path = "adv/scenario/c" + text3 + "/";
		if (id <= -100)
		{
			path = "adv/scenario/etc/";
		}
		else if (id <= -20)
		{
			path = "adv/scenario/op_append/";
		}
		else if (id <= -10)
		{
			path = "adv/scenario/op/";
		}
		foreach (string item in from bundle in CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true)
			orderby bundle descending
			select bundle)
		{
			if (!int.TryParse(Path.GetFileName(Path.GetDirectoryName(item)), out var result) || (result >= 50 && !GameSystem.isAdd50) || Path.GetFileNameWithoutExtension(item) != text2)
			{
				continue;
			}
			if (asset == null)
			{
				text = item;
			}
			else
			{
				ScenarioData[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(ScenarioData)).GetAllAssets<ScenarioData>();
				foreach (ScenarioData scenarioData in allAssets)
				{
					if (scenarioData.name == asset)
					{
						text = item;
						data = scenarioData;
						break;
					}
				}
				AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
			}
			if (text != null)
			{
				break;
			}
		}
		return text;
	}

	public static string FindMessageADVBundleFilePath(string path, string asset = null)
	{
		ScenarioData data;
		return FindMessageADVBundleFilePath(path, asset, out data);
	}

	public static string FindMessageADVBundleFilePath(string path, string asset, out ScenarioData data)
	{
		data = null;
		string text = null;
		foreach (string item in from bundle in CommonLib.GetAssetBundleNameListFromPath("adv/message/" + path + "/", subdirCheck: true)
			orderby bundle descending
			select bundle)
		{
			if (asset == null)
			{
				text = item;
			}
			else
			{
				ScenarioData[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(ScenarioData)).GetAllAssets<ScenarioData>();
				foreach (ScenarioData scenarioData in allAssets)
				{
					if (scenarioData.name == asset)
					{
						text = item;
						data = scenarioData;
						break;
					}
				}
				AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
			}
			if (text != null)
			{
				break;
			}
		}
		return text;
	}

	public static IEnumerator Open(IData scene, IData openData, OpenDataProc proc = null)
	{
		_ = scene;
		yield break;
	}
}
