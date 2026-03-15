using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Studio;
using UniRx.Async;
using UnityEngine;

namespace WorkSpace.KK;

public class LoadTest : MonoBehaviour
{
	private class FileListInfo
	{
		private class Data
		{
			private string[] files;

			private readonly string manifest = "";

			public string Manifest => manifest;

			public Data(string _path)
			{
				files = Studio.AssetBundleCheck.GetAllFileName(_path);
				foreach (KeyValuePair<string, AssetBundleManager.BundlePack> item in AssetBundleManager.ManifestBundlePack.Where((KeyValuePair<string, AssetBundleManager.BundlePack> v) => Regex.IsMatch(v.Key, "studio(\\d*)")))
				{
					if (item.Value.AssetBundleManifest.GetAllAssetBundles().Contains(_path))
					{
						manifest = item.Key;
						break;
					}
				}
			}

			public bool Contains(string _file)
			{
				return files.Contains(_file);
			}
		}

		private Dictionary<string, Data> dicFile;

		public FileListInfo(List<string> _list)
		{
			dicFile = new Dictionary<string, Data>();
			foreach (string item in _list)
			{
				dicFile.Add(item, new Data(item));
			}
		}

		public bool Check(string _path, string _file)
		{
			Data value = null;
			if (!AssetBundleCheck.IsSimulation)
			{
				_file = _file.ToLower();
			}
			if (!dicFile.TryGetValue(_path, out value))
			{
				return false;
			}
			return value.Contains(_file);
		}

		public string GetManifest(string _path)
		{
			Data value = null;
			if (!dicFile.TryGetValue(_path, out value))
			{
				return "";
			}
			return value.Manifest;
		}
	}

	public class WaitTime
	{
		private const float intervalTime = 0.03f;

		private float nextFrameTime;

		public bool isOver => Time.realtimeSinceStartup >= nextFrameTime;

		public WaitTime()
		{
			Next();
		}

		public void Next()
		{
			nextFrameTime = Time.realtimeSinceStartup + 0.03f;
		}
	}

	private async UniTask LoadInfo()
	{
		await UniTask.WaitWhile(() => !AssetBundleManager.initialized);
		Stopwatch sw = new Stopwatch();
		sw.Start();
		new WaitTime();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath("studio/info/", subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		FileListInfo fli = new FileListInfo(assetBundleNameListFromPath);
		foreach (string item in assetBundleNameListFromPath)
		{
			string manifest = fli.GetManifest(item);
			await LoadAllExcelData(item, manifest);
		}
		sw.Stop();
	}

	private async UniTask<ExcelData[]> LoadAllExcelData(string _path, string _manifest)
	{
		return (await AssetBundleManager.LoadAllAssetAsync(_path, typeof(ExcelData), _manifest)).GetAllAssets<ExcelData>();
	}

	private void Start()
	{
		LoadInfo();
	}
}
