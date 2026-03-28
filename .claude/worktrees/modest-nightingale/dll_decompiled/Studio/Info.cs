using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Studio;

public class Info : Singleton<Info>
{
	public class FileInfo
	{
		public string manifest = "";

		public string bundlePath = "";

		public string fileName = "";

		public bool Check => !bundlePath.IsNullOrEmpty() & !fileName.IsNullOrEmpty();

		public void Clear()
		{
			manifest = "";
			bundlePath = "";
			fileName = "";
		}
	}

	public class LoadCommonInfo : FileInfo
	{
		public string name = "";
	}

	public class CategoryInfo
	{
		public int sort;

		public string name = "";
	}

	public class GroupInfo
	{
		public int sort;

		public string name = "";

		public Dictionary<int, CategoryInfo> dicCategory = new Dictionary<int, CategoryInfo>();
	}

	public class BoneInfo
	{
		public int no;

		public string bone = "";

		public string name = "";

		public int group = -1;

		public int level;

		public int sync = -1;

		public BoneInfo(int _no, string _bone, List<string> _lst)
		{
			no = _no;
			bone = _bone;
			int num = 2;
			name = _lst.SafeGet(num++);
			if (!int.TryParse(_lst.SafeGet(num++), out group))
			{
				group = 0;
			}
			if (!int.TryParse(_lst.SafeGet(num++), out level))
			{
				level = 0;
			}
			if (!int.TryParse(_lst.SafeGet(num++), out sync))
			{
				sync = -1;
			}
		}
	}

	public class ItemLoadInfo : LoadCommonInfo
	{
		public List<string> bones;

		public ItemLoadInfo(List<string> _lst)
		{
			name = _lst[3];
			manifest = _lst[4];
			bundlePath = _lst[5];
			fileName = _lst[6];
		}
	}

	public class AccessoryGroupInfo
	{
		public string Name { get; private set; } = "";

		public int[] Targets { get; private set; }

		public AccessoryGroupInfo(string _name, string _targets)
		{
			Name = _name;
			Targets = (from s in _targets.Split('-')
				select int.Parse(s)).ToArray();
		}
	}

	public class LightLoadInfo : LoadCommonInfo
	{
		public enum Target
		{
			All,
			Chara,
			Map
		}

		public int no;

		public Target target;
	}

	public class ParentageInfo
	{
		public string parent = "";

		public string child = "";
	}

	public class OptionItemInfo : FileInfo
	{
		public FileInfo anmInfo;

		public FileInfo anmOveride;

		public ParentageInfo[] parentageInfo;

		public bool counterScale;

		public bool isAnimeSync = true;
	}

	public class AnimeLoadInfo : LoadCommonInfo
	{
		public int sort;

		public string clip = "";

		public List<OptionItemInfo> option;

		public static List<OptionItemInfo> LoadOption(List<string> _list, int _start, bool _animeSync)
		{
			List<OptionItemInfo> list = new List<OptionItemInfo>();
			int num = _start;
			while (true)
			{
				OptionItemInfo info = new OptionItemInfo();
				if (!_list.SafeProc(num++, delegate(string _s)
				{
					info.bundlePath = _s;
				}) || !_list.SafeProc(num++, delegate(string _s)
				{
					info.fileName = _s;
				}) || !_list.SafeProc(num++, delegate(string _s)
				{
					info.manifest = _s;
				}))
				{
					break;
				}
				info.anmInfo = new FileInfo();
				info.anmInfo.bundlePath = _list.SafeGet(num++);
				info.anmInfo.fileName = _list.SafeGet(num++);
				info.anmOveride = new FileInfo();
				info.anmOveride.bundlePath = _list.SafeGet(num++);
				info.anmOveride.fileName = _list.SafeGet(num++);
				info.parentageInfo = AnalysisParentageInfo(_list.SafeGet(num++));
				bool.TryParse(_list.SafeGet(num++), out info.counterScale);
				if (_animeSync && !bool.TryParse(_list.SafeGet(num++), out info.isAnimeSync))
				{
					info.isAnimeSync = true;
				}
				list.Add(info);
			}
			return list;
		}

		private static ParentageInfo[] AnalysisParentageInfo(string _str)
		{
			if (_str.IsNullOrEmpty())
			{
				return null;
			}
			string[] array = _str.Split(',');
			List<ParentageInfo> list = new List<ParentageInfo>();
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('/');
				ParentageInfo parentageInfo = new ParentageInfo();
				parentageInfo.parent = array2[0];
				if (array2.Length > 1)
				{
					parentageInfo.child = array2[1];
				}
				list.Add(parentageInfo);
			}
			return list.ToArray();
		}
	}

	public class HAnimeLoadInfo : AnimeLoadInfo
	{
		public FileInfo overrideFile;

		public int breastLayer = -1;

		public FileInfo yureFile;

		public int motionID;

		public int num;

		public bool isMotion;

		public bool[] pv;

		public bool isBreastLayer => breastLayer != -1;

		public HAnimeLoadInfo(int _startIdx, List<string> _list)
		{
			int num = _startIdx;
			int.TryParse(_list.SafeGet(0), out sort);
			name = _list.SafeGet(num++);
			bundlePath = _list.SafeGet(num++);
			fileName = _list.SafeGet(num++);
			overrideFile = new FileInfo
			{
				bundlePath = _list.SafeGet(num++),
				fileName = _list.SafeGet(num++)
			};
			clip = _list.SafeGet(num++);
			int.TryParse(_list.SafeGet(num++), out breastLayer);
			yureFile = new FileInfo
			{
				bundlePath = _list.SafeGet(num++),
				fileName = _list.SafeGet(num++)
			};
			int.TryParse(_list.SafeGet(num++), out motionID);
			int.TryParse(_list.SafeGet(num++), out this.num);
			isMotion = bool.Parse(_list.SafeGet(num++));
			pv = Enumerable.Repeat(element: true, 8).ToArray();
			for (int i = 0; i < 8; i++)
			{
				bool result = true;
				if (bool.TryParse(_list.SafeGet(num++), out result))
				{
					pv[i] = result;
				}
			}
			option = AnimeLoadInfo.LoadOption(_list, num++, _animeSync: false);
		}
	}

	public class MapLoadInfo : LoadCommonInfo
	{
		public FileInfo vanish;

		public MapLoadInfo(List<string> _list)
		{
			name = _list[1];
			bundlePath = _list[2];
			fileName = _list[3];
			manifest = _list.SafeGet(4);
			vanish = new FileInfo();
			vanish.bundlePath = _list.SafeGet(5);
			vanish.fileName = _list.SafeGet(6);
		}
	}

	public class SkyPatternInfo : LoadCommonInfo
	{
		public Vector3 Rotation { get; }

		public SkyPatternInfo(List<string> _list)
		{
			int num = 1;
			name = _list[num++];
			Vector3 zero = Vector3.zero;
			float.TryParse(_list.SafeGet(num++), out zero.x);
			float.TryParse(_list.SafeGet(num++), out zero.y);
			float.TryParse(_list.SafeGet(num++), out zero.z);
			Rotation = zero;
			bundlePath = _list.SafeGet(num++);
			fileName = _list.SafeGet(num++);
			manifest = _list.SafeGet(num++);
		}
	}

	private class FileCheck
	{
		private Dictionary<string, bool> dicConfirmed;

		public FileCheck()
		{
			dicConfirmed = new Dictionary<string, bool>();
		}

		public bool Check(string _path)
		{
			if (_path.IsNullOrEmpty())
			{
				return false;
			}
			bool value = false;
			if (dicConfirmed.TryGetValue(_path, out value))
			{
				return value;
			}
			value = !AssetBundleCheck.IsSimulation && File.Exists(AssetBundleManager.BaseDownloadingURL + _path);
			dicConfirmed.Add(_path, value);
			return value;
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

	private class FileListInfo
	{
		private class Data
		{
			private string[] files;

			private readonly string manifest = "";

			public string Manifest => manifest;

			public Data(string _path)
			{
				files = AssetBundleCheck.GetAllFileName(_path);
				foreach (KeyValuePair<string, AssetBundleManager.BundlePack> item in AssetBundleManager.ManifestBundlePack.Where((KeyValuePair<string, AssetBundleManager.BundlePack> v) => Regex.Match(v.Key, "studio(\\d*)").Success))
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

	private delegate void LoadAnimeInfoCoroutineFunc(ExcelData _ed, Dictionary<int, Dictionary<int, Dictionary<int, AnimeLoadInfo>>> _dic);

	public Dictionary<int, BoneInfo> dicBoneInfo = new Dictionary<int, BoneInfo>();

	public Dictionary<int, GroupInfo> dicItemGroupCategory = new Dictionary<int, GroupInfo>();

	public Dictionary<int, Dictionary<int, Dictionary<int, ItemLoadInfo>>> dicItemLoadInfo = new Dictionary<int, Dictionary<int, Dictionary<int, ItemLoadInfo>>>();

	public Dictionary<int, Dictionary<int, Dictionary<int, ItemColorData.ColorData>>> dicItemColorData = new Dictionary<int, Dictionary<int, Dictionary<int, ItemColorData.ColorData>>>();

	public Dictionary<int, AccessoryGroupInfo> dicAccessoryGroup = new Dictionary<int, AccessoryGroupInfo>();

	private ExcelData m_AccessoryPointGroup;

	public Dictionary<int, LightLoadInfo> dicLightLoadInfo = new Dictionary<int, LightLoadInfo>();

	public Dictionary<int, GroupInfo> dicAGroupCategory = new Dictionary<int, GroupInfo>();

	public Dictionary<int, Dictionary<int, Dictionary<int, AnimeLoadInfo>>> dicAnimeLoadInfo = new Dictionary<int, Dictionary<int, Dictionary<int, AnimeLoadInfo>>>();

	public Dictionary<int, GroupInfo> dicVoiceGroupCategory = new Dictionary<int, GroupInfo>();

	public Dictionary<int, Dictionary<int, Dictionary<int, LoadCommonInfo>>> dicVoiceLoadInfo = new Dictionary<int, Dictionary<int, Dictionary<int, LoadCommonInfo>>>();

	public Dictionary<int, LoadCommonInfo> dicBGMLoadInfo = new Dictionary<int, LoadCommonInfo>();

	public Dictionary<int, LoadCommonInfo> dicENVLoadInfo = new Dictionary<int, LoadCommonInfo>();

	public Dictionary<int, MapLoadInfo> dicMapLoadInfo = new Dictionary<int, MapLoadInfo>();

	public Dictionary<int, LoadCommonInfo> dicColorGradingLoadInfo = new Dictionary<int, LoadCommonInfo>();

	public Dictionary<int, LoadCommonInfo> dicReflectionProbeLoadInfo = new Dictionary<int, LoadCommonInfo>();

	public Dictionary<int, SkyPatternInfo> dicSkyPatternInfo = new Dictionary<int, SkyPatternInfo>();

	private FileCheck fileCheck;

	private WaitTime waitTime;

	public int AccessoryPointNum { get; private set; }

	public int[] AccessoryPointsIndex
	{
		get
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (KeyValuePair<int, AccessoryGroupInfo> item in dicAccessoryGroup)
			{
				hashSet.UnionWith(Enumerable.Range(item.Value.Targets[0], item.Value.Targets[1] - item.Value.Targets[0] + 1));
			}
			return hashSet.ToArray();
		}
	}

	public ExcelData accessoryPointGroup => m_AccessoryPointGroup;

	public bool isLoadList { get; private set; }

	public bool IsInstallAI { get; private set; }

	public bool IsInstallAdd { get; private set; }

	private void CheckIsAI()
	{
		string keyName = "Software\\illusion\\AI-Syoujyo\\AI-Syoujyo";
		if (!YS_Assist.IsRegistryKey(keyName))
		{
			return;
		}
		string registryInfoFrom = YS_Assist.GetRegistryInfoFrom(keyName, "InstallDir");
		if (!registryInfoFrom.IsNullOrEmpty())
		{
			registryInfoFrom = registryInfoFrom.TrimEnd('\\');
			if (File.Exists(registryInfoFrom + "/AI-Syoujyo.exe"))
			{
				IsInstallAI = true;
			}
		}
	}

	private void CheckIsAdd()
	{
		IsInstallAdd = AssetBundleCheck.IsManifest("add50");
	}

	public IEnumerator LoadExcelDataCoroutine()
	{
		if (isLoadList)
		{
			yield break;
		}
		fileCheck = new FileCheck();
		waitTime = new WaitTime();
		dicBoneInfo.Clear();
		dicItemGroupCategory.Clear();
		dicItemLoadInfo.Clear();
		dicLightLoadInfo.Clear();
		dicAGroupCategory.Clear();
		dicAnimeLoadInfo.Clear();
		dicVoiceGroupCategory.Clear();
		dicVoiceLoadInfo.Clear();
		dicColorGradingLoadInfo.Clear();
		dicReflectionProbeLoadInfo.Clear();
		dicSkyPatternInfo.Clear();
		if (waitTime.isOver)
		{
			yield return null;
			waitTime.Next();
		}
		List<string> pathList = CommonLib.GetAssetBundleNameListFromPath("studio/info/", subdirCheck: true);
		pathList.Sort();
		if (waitTime.isOver)
		{
			yield return null;
			waitTime.Next();
		}
		FileListInfo fli = new FileListInfo(pathList);
		int i = 0;
		while (i < pathList.Count)
		{
			string bundlePath = pathList[i];
			string fileName = Path.GetFileNameWithoutExtension(bundlePath);
			string manifest = fli.GetManifest(bundlePath);
			int result = 0;
			if (!int.TryParse(fileName, out result) || result < 50 || IsInstallAdd)
			{
				if (fli.Check(bundlePath, "AccessoryPointGroup_" + fileName))
				{
					LoadAccessoryGroupInfo(LoadExcelData(bundlePath, "AccessoryPointGroup_" + fileName, manifest));
				}
				if (fli.Check(bundlePath, "Bone_" + fileName))
				{
					LoadBoneInfo(LoadExcelData(bundlePath, "Bone_" + fileName, manifest), dicBoneInfo);
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				if (fli.Check(bundlePath, "ItemGroup_" + fileName))
				{
					LoadItemGroupInfo(LoadExcelData(bundlePath, "ItemGroup_" + fileName, manifest));
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				LoadItemCategoryInfo(bundlePath, "ItemCategory_(\\d*)_(\\d*)");
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				yield return LoadItemLoadInfoCoroutine(bundlePath, "ItemList_(\\d*)_(\\d*)_(\\d*)");
				LoadItemBoneInfo(bundlePath, "ItemBoneList_(\\d*)_(\\d*)");
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				LoadItemColorData(bundlePath, "ItemData_(\\d*)");
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				if (fli.Check(bundlePath, "Light_" + fileName))
				{
					LoadLightLoadInfo(LoadExcelData(bundlePath, "Light_" + fileName, manifest));
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				if (fli.Check(bundlePath, "AnimeGroup_" + fileName))
				{
					LoadAnimeGroupInfo(LoadExcelData(bundlePath, "AnimeGroup_" + fileName, manifest));
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				LoadAnimeCategoryInfo(bundlePath, "AnimeCategory_(\\d*)_(\\d*)");
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				yield return LoadAnimeLoadInfoCoroutine(bundlePath, "Anime_(\\d*)_(\\d*)_(\\d*)", dicAnimeLoadInfo, LoadAnimeLoadInfo);
				yield return LoadAnimeLoadInfoCoroutine(bundlePath, "HAnime_(\\d*)_(\\d*)_(\\d*)", dicAnimeLoadInfo, LoadHAnimeLoadInfo);
				if (fli.Check(bundlePath, "VoiceGroup_" + fileName))
				{
					LoadAnimeGroupInfo(LoadExcelData(bundlePath, "VoiceGroup_" + fileName, manifest), dicVoiceGroupCategory);
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				LoadAnimeCategoryInfo(bundlePath, "VoiceCategory_(\\d*)_(\\d*)", dicVoiceGroupCategory);
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				yield return LoadVoiceLoadInfoCoroutine(bundlePath, "Voice_(\\d*)_(\\d*)_(\\d*)");
				if (fli.Check(bundlePath, "BGM_" + fileName))
				{
					LoadBGMLoadInfo(LoadExcelData(bundlePath, "BGM_" + fileName, manifest));
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				if (fli.Check(bundlePath, "Map_" + fileName))
				{
					LoadMapLoadInfo(LoadExcelData(bundlePath, "Map_" + fileName, manifest));
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				if (fli.Check(bundlePath, "Filter_" + fileName))
				{
					LoadSoundLoadInfo(LoadExcelData(bundlePath, "Filter_" + fileName, manifest), dicColorGradingLoadInfo);
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				if (fli.Check(bundlePath, "Probe_" + fileName))
				{
					LoadSoundLoadInfo(LoadExcelData(bundlePath, "Probe_" + fileName, manifest), dicReflectionProbeLoadInfo);
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				if (fli.Check(bundlePath, "StudioSky_" + fileName))
				{
					LoadSkyPatternInfo(LoadExcelData(bundlePath, "StudioSky_" + fileName, manifest));
				}
				if (waitTime.isOver)
				{
					yield return null;
					waitTime.Next();
				}
				AssetBundleManager.UnloadAssetBundle(bundlePath, isUnloadForceRefCount: true);
			}
			int num = i + 1;
			i = num;
		}
		fileCheck = null;
		waitTime = null;
		isLoadList = true;
	}

	public LoadCommonInfo GetVoiceInfo(int _group, int _category, int _no)
	{
		Dictionary<int, Dictionary<int, LoadCommonInfo>> value = null;
		if (!dicVoiceLoadInfo.TryGetValue(_group, out value))
		{
			return null;
		}
		Dictionary<int, LoadCommonInfo> value2 = null;
		if (!value.TryGetValue(_category, out value2))
		{
			return null;
		}
		LoadCommonInfo value3 = null;
		if (!value2.TryGetValue(_no, out value3))
		{
			return null;
		}
		return value3;
	}

	public ItemColorData.ColorData SafeGetItemColorData(int _group, int _category, int _id)
	{
		Dictionary<int, Dictionary<int, ItemColorData.ColorData>> value = null;
		if (!dicItemColorData.TryGetValue(_group, out value))
		{
			return null;
		}
		Dictionary<int, ItemColorData.ColorData> value2 = null;
		if (!value.TryGetValue(_category, out value2))
		{
			return null;
		}
		ItemColorData.ColorData value3 = null;
		if (!value2.TryGetValue(_id, out value3))
		{
			return null;
		}
		return value3;
	}

	public bool ExistItemGroup(int _group)
	{
		Dictionary<int, Dictionary<int, ItemLoadInfo>> value = null;
		if (!dicItemLoadInfo.TryGetValue(_group, out value))
		{
			return false;
		}
		return value.Sum((KeyValuePair<int, Dictionary<int, ItemLoadInfo>> _v) => _v.Value.Count) != 0;
	}

	public bool ExistItemCategory(int _group, int _category)
	{
		Dictionary<int, Dictionary<int, ItemLoadInfo>> value = null;
		if (!dicItemLoadInfo.TryGetValue(_group, out value))
		{
			return false;
		}
		Dictionary<int, ItemLoadInfo> value2 = null;
		if (!value.TryGetValue(_category, out value2))
		{
			return false;
		}
		return value2.Count != 0;
	}

	public bool ExistAnimeGroup(int _group)
	{
		Dictionary<int, Dictionary<int, AnimeLoadInfo>> value = null;
		if (!dicAnimeLoadInfo.TryGetValue(_group, out value))
		{
			return false;
		}
		return value.Sum((KeyValuePair<int, Dictionary<int, AnimeLoadInfo>> _v) => _v.Value.Count) != 0;
	}

	public bool ExistAnimeCategory(int _group, int _category)
	{
		Dictionary<int, Dictionary<int, AnimeLoadInfo>> value = null;
		if (!dicAnimeLoadInfo.TryGetValue(_group, out value))
		{
			return false;
		}
		Dictionary<int, AnimeLoadInfo> value2 = null;
		if (!value.TryGetValue(_category, out value2))
		{
			return false;
		}
		return value2.Count != 0;
	}

	private ExcelData LoadExcelData(string _bundlePath, string _fileName, string _manifest)
	{
		if (AssetBundleCheck.IsSimulation && !AssetBundleCheck.FindFile(_bundlePath, _fileName))
		{
			return null;
		}
		return CommonLib.LoadAsset<ExcelData>(_bundlePath, _fileName, clone: false, _manifest);
	}

	private ExcelData LoadExcelDataFromBundle(string _bundlePath, string _fileName, string _manifest)
	{
		return AddObjectAssist.LoadAsset<ExcelData>(_bundlePath, _fileName, clone: false, _manifest);
	}

	private string[] FindAllAssetName(string _bundlePath, string _regex, out string _manifest)
	{
		string[] result = null;
		if (AssetBundleCheck.IsSimulation)
		{
			result = AssetBundleCheck.FindAllAssetName(_bundlePath, _regex, _WithExtension: false, RegexOptions.IgnoreCase);
		}
		else
		{
			foreach (KeyValuePair<string, AssetBundleManager.BundlePack> item in AssetBundleManager.ManifestBundlePack.Where((KeyValuePair<string, AssetBundleManager.BundlePack> v) => Regex.Match(v.Key, "studio(\\d*)").Success))
			{
				if (!item.Value.AssetBundleManifest.GetAllAssetBundles().Contains(_bundlePath))
				{
					continue;
				}
				LoadedAssetBundle value = null;
				if (!item.Value.LoadedAssetBundles.TryGetValue(_bundlePath, out value))
				{
					value = AssetBundleManager.LoadAssetBundle(_bundlePath, item.Key);
					if (value == null)
					{
						break;
					}
				}
				_manifest = item.Key;
				return (from s in value.Bundle.GetAllAssetNames()
					select Path.GetFileNameWithoutExtension(s) into s
					where Regex.Match(s, _regex, RegexOptions.IgnoreCase).Success
					select s).ToArray();
			}
		}
		_manifest = "";
		return result;
	}

	private string[] FindAllAssetNameFromBundle(string _bundlePath, string _regex, out string _manifest)
	{
		using (IEnumerator<KeyValuePair<string, AssetBundleManager.BundlePack>> enumerator = (from v in AssetBundleManager.ManifestBundlePack
			where Regex.Match(v.Key, "studio(\\d*)").Success
			where v.Value.AssetBundleManifest.GetAllAssetBundles().Contains(_bundlePath)
			select v).GetEnumerator())
		{
			KeyValuePair<string, AssetBundleManager.BundlePack> current;
			LoadedAssetBundle value;
			if (enumerator.MoveNext())
			{
				current = enumerator.Current;
				value = null;
				if (current.Value.LoadedAssetBundles.TryGetValue(_bundlePath, out value))
				{
					goto IL_0095;
				}
				value = AssetBundleManager.LoadAssetBundle(_bundlePath, current.Key);
				if (value != null)
				{
					goto IL_0095;
				}
			}
			goto end_IL_0054;
			IL_0095:
			_manifest = current.Key;
			return (from s in value.Bundle.GetAllAssetNames()
				select Path.GetFileNameWithoutExtension(s) into s
				where Regex.Match(s, _regex, RegexOptions.IgnoreCase).Success
				select s).ToArray();
			end_IL_0054:;
		}
		_manifest = "";
		return null;
	}

	private void LoadAccessoryGroupInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int result = -1;
			if (int.TryParse(item.SafeGet(0), out result))
			{
				string self = item.SafeGet(1);
				if (!self.IsNullOrEmpty())
				{
					dicAccessoryGroup[result] = new AccessoryGroupInfo(self, item.SafeGet(2));
				}
			}
		}
		m_AccessoryPointGroup = _ed;
		int num = 0;
		foreach (KeyValuePair<int, AccessoryGroupInfo> item2 in dicAccessoryGroup)
		{
			int num2 = item2.Value.Targets.SafeGet(0);
			int num3 = item2.Value.Targets.SafeGet(1);
			for (int num4 = num2; num4 <= num3; num4++)
			{
				num++;
			}
		}
		AccessoryPointNum = num;
	}

	private void LoadBoneInfo(ExcelData _ed, Dictionary<int, BoneInfo> _dic)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = -1;
			if (int.TryParse(item[num++], out result))
			{
				string text = item[num++];
				if (!text.IsNullOrEmpty())
				{
					_dic[result] = new BoneInfo(result, text, item);
				}
			}
		}
	}

	private IEnumerator LoadItemLoadInfoCoroutine(string _bundlePath, string _regex)
	{
		string manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			yield break;
		}
		string strB = _regex.Split('_')[0].ToLower();
		SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, string>>> sortedDictionary = new SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, string>>>();
		for (int i = 0; i < array.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(array[i]);
			if (fileNameWithoutExtension.Split('_')[0].ToLower().CompareTo(strB) != 0)
			{
				continue;
			}
			Match match = Regex.Match(fileNameWithoutExtension, _regex, RegexOptions.IgnoreCase);
			int num = int.Parse(match.Groups[1].Value);
			int num2 = int.Parse(match.Groups[2].Value);
			int num3 = int.Parse(match.Groups[3].Value);
			switch (num2)
			{
			case 1:
				switch (num3)
				{
				case 1:
				case 2:
				case 3:
				case 4:
				case 7:
					if (!IsInstallAI)
					{
						continue;
					}
					break;
				case 5:
				case 6:
					if (num != 10 && !IsInstallAI)
					{
						continue;
					}
					break;
				}
				break;
			case 2:
				if (num != 13 && !IsInstallAI)
				{
					continue;
				}
				break;
			case 3:
				if ((uint)(num3 - 15) <= 2u && !IsInstallAI)
				{
					continue;
				}
				break;
			case 9:
				if (num3 == 38 && !IsInstallAI)
				{
					continue;
				}
				break;
			case 4:
			case 12:
				if (!IsInstallAI)
				{
					continue;
				}
				break;
			}
			if (num != 50 || IsInstallAdd)
			{
				if (!sortedDictionary.ContainsKey(num2))
				{
					sortedDictionary.Add(num2, new SortedDictionary<int, SortedDictionary<int, string>>());
				}
				if (!sortedDictionary[num2].ContainsKey(num3))
				{
					sortedDictionary[num2].Add(num3, new SortedDictionary<int, string>());
				}
				sortedDictionary[num2][num3].Add(num, array[i]);
			}
		}
		foreach (KeyValuePair<int, SortedDictionary<int, SortedDictionary<int, string>>> item in sortedDictionary)
		{
			foreach (KeyValuePair<int, SortedDictionary<int, string>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, string> item3 in item2.Value)
				{
					LoadItemLoadInfo(LoadExcelData(_bundlePath, item3.Value, manifest));
					if (waitTime.isOver)
					{
						yield return null;
						waitTime.Next();
					}
				}
			}
		}
	}

	private void SortDictionary(string[] files, string _regex, SortedDictionary<int, SortedDictionary<int, string>> _sortDic)
	{
		string strB = _regex.Split('_')[0].ToLower();
		for (int i = 0; i < files.Length; i++)
		{
			if (files[i].Split('_')[0].ToLower().CompareTo(strB) == 0)
			{
				Match match = Regex.Match(files[i], _regex, RegexOptions.IgnoreCase);
				int key = int.Parse(match.Groups[1].Value);
				int key2 = int.Parse(match.Groups[2].Value);
				if (!_sortDic.ContainsKey(key))
				{
					_sortDic.Add(key, new SortedDictionary<int, string>());
				}
				_sortDic[key].Add(key2, files[i]);
			}
		}
	}

	private void LoadItemLoadInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int result = -1;
			if (!int.TryParse(item.SafeGet(0), out result))
			{
				break;
			}
			int key = int.Parse(item[1]);
			int key2 = int.Parse(item[2]);
			if (!dicItemLoadInfo.ContainsKey(key))
			{
				dicItemLoadInfo[key] = new Dictionary<int, Dictionary<int, ItemLoadInfo>>();
			}
			if (!dicItemLoadInfo[key].ContainsKey(key2))
			{
				dicItemLoadInfo[key][key2] = new Dictionary<int, ItemLoadInfo>();
			}
			dicItemLoadInfo[key][key2][result] = new ItemLoadInfo(item);
		}
	}

	private void LoadItemBoneInfo(string _bundlePath, string _regex)
	{
		string _manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out _manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			return;
		}
		SortedDictionary<int, SortedDictionary<int, string>> sortedDictionary = new SortedDictionary<int, SortedDictionary<int, string>>();
		SortDictionary(array, _regex, sortedDictionary);
		foreach (KeyValuePair<int, SortedDictionary<int, string>> item in sortedDictionary)
		{
			foreach (KeyValuePair<int, string> item2 in item.Value)
			{
				LoadItemBoneInfo(LoadExcelData(_bundlePath, item2.Value, _manifest), item.Key, item2.Key);
			}
		}
	}

	private void LoadItemColorData(string _bundlePath, string _regex)
	{
		string _manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out _manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			return;
		}
		string[] array2 = array;
		foreach (string assetName in array2)
		{
			ItemColorData itemColorData = CommonLib.LoadAsset<ItemColorData>(_bundlePath, assetName, clone: false, _manifest);
			if (itemColorData == null || itemColorData.ColorDatas == null)
			{
				break;
			}
			foreach (ItemColorData.ColorData colorData in itemColorData.ColorDatas)
			{
				Dictionary<int, Dictionary<int, ItemColorData.ColorData>> value = null;
				if (!dicItemColorData.TryGetValue(colorData.Group, out value))
				{
					value = new Dictionary<int, Dictionary<int, ItemColorData.ColorData>>();
					dicItemColorData.Add(colorData.Group, value);
				}
				Dictionary<int, ItemColorData.ColorData> value2 = null;
				if (!value.TryGetValue(colorData.Category, out value2))
				{
					value2 = new Dictionary<int, ItemColorData.ColorData>();
					value.Add(colorData.Category, value2);
				}
				value2[colorData.ID] = new ItemColorData.ColorData(colorData);
			}
		}
	}

	private void LoadItemBoneInfo(ExcelData _ed, int _group, int _category)
	{
		if (_ed == null)
		{
			return;
		}
		Dictionary<int, Dictionary<int, ItemLoadInfo>> value = null;
		if (!dicItemLoadInfo.TryGetValue(_group, out value))
		{
			return;
		}
		Dictionary<int, ItemLoadInfo> value2 = null;
		if (!value.TryGetValue(_category, out value2))
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			ItemLoadInfo value3 = null;
			int result = -1;
			if (int.TryParse(item.SafeGet(0), out result) && value2.TryGetValue(result, out value3))
			{
				value3.bones = (from s in item.Skip(1)
					where !s.IsNullOrEmpty()
					select s).ToList();
			}
		}
	}

	private void LoadLightLoadInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			LightLoadInfo lightLoadInfo = new LightLoadInfo();
			lightLoadInfo.no = int.Parse(item[num++]);
			lightLoadInfo.name = item[num++];
			lightLoadInfo.manifest = item[num++];
			lightLoadInfo.bundlePath = item[num++];
			lightLoadInfo.fileName = item[num++];
			lightLoadInfo.target = (LightLoadInfo.Target)int.Parse(item[num++]);
			dicLightLoadInfo[lightLoadInfo.no] = lightLoadInfo;
		}
	}

	private void LoadAnimeGroupInfo(ExcelData _ed, Dictionary<int, GroupInfo> _dic)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = 0;
			if (!int.TryParse(item.SafeGet(num++), out result))
			{
				continue;
			}
			int result2 = -1;
			if (int.TryParse(item.SafeGet(num++), out result2))
			{
				string text = item[num++];
				GroupInfo value = null;
				if (_dic.TryGetValue(result2, out value))
				{
					value.sort = result;
					value.name = text;
					continue;
				}
				value = new GroupInfo
				{
					sort = result,
					name = text
				};
				value.name = text;
				_dic.Add(result2, value);
			}
		}
	}

	private void LoadAnimeCategoryInfo(string _bundlePath, string _regex, Dictionary<int, GroupInfo> _dic)
	{
		string _manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out _manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			return;
		}
		string strB = _regex.Split('_')[0].ToLower();
		SortedDictionary<int, SortedDictionary<int, string>> sortedDictionary = new SortedDictionary<int, SortedDictionary<int, string>>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Split('_')[0].ToLower().CompareTo(strB) == 0)
			{
				Match match = Regex.Match(array[i], _regex, RegexOptions.IgnoreCase);
				int key = int.Parse(match.Groups[1].Value);
				int key2 = int.Parse(match.Groups[2].Value);
				if (!sortedDictionary.ContainsKey(key2))
				{
					sortedDictionary.Add(key2, new SortedDictionary<int, string>());
				}
				sortedDictionary[key2].Add(key, array[i]);
			}
		}
		foreach (KeyValuePair<int, SortedDictionary<int, string>> item in sortedDictionary)
		{
			if (!_dic.ContainsKey(item.Key))
			{
				continue;
			}
			foreach (KeyValuePair<int, string> item2 in item.Value)
			{
				LoadAnimeCategoryInfo(LoadExcelData(_bundlePath, item2.Value, _manifest), _dic[item.Key]);
			}
		}
	}

	private void LoadAnimeCategoryInfo(ExcelData _ed, GroupInfo _info)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = 0;
			if (int.TryParse(item.SafeGet(num++), out result))
			{
				int result2 = -1;
				if (int.TryParse(item.SafeGet(num++), out result2))
				{
					_info.dicCategory[result2] = new CategoryInfo
					{
						sort = result,
						name = item.SafeGet(num++)
					};
				}
			}
		}
	}

	private void LoadItemGroupInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = 0;
			if (!int.TryParse(item.SafeGet(num++), out result))
			{
				continue;
			}
			int result2 = -1;
			if (int.TryParse(item.SafeGet(num++), out result2))
			{
				string text = item[num++];
				GroupInfo value = null;
				if (dicItemGroupCategory.TryGetValue(result2, out value))
				{
					value.sort = result;
					value.name = text;
					continue;
				}
				value = new GroupInfo
				{
					sort = result,
					name = text
				};
				value.name = text;
				dicItemGroupCategory.Add(result2, value);
			}
		}
	}

	private void LoadItemCategoryInfo(string _bundlePath, string _regex)
	{
		string _manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out _manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			return;
		}
		string strB = _regex.Split('_')[0].ToLower();
		SortedDictionary<int, SortedDictionary<int, string>> sortedDictionary = new SortedDictionary<int, SortedDictionary<int, string>>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Split('_')[0].ToLower().CompareTo(strB) == 0)
			{
				Match match = Regex.Match(array[i], _regex, RegexOptions.IgnoreCase);
				int key = int.Parse(match.Groups[1].Value);
				int key2 = int.Parse(match.Groups[2].Value);
				if (!sortedDictionary.ContainsKey(key2))
				{
					sortedDictionary.Add(key2, new SortedDictionary<int, string>());
				}
				sortedDictionary[key2].Add(key, array[i]);
			}
		}
		foreach (KeyValuePair<int, SortedDictionary<int, string>> item in sortedDictionary)
		{
			if (!dicItemGroupCategory.ContainsKey(item.Key))
			{
				continue;
			}
			foreach (KeyValuePair<int, string> item2 in item.Value)
			{
				LoadItemCategoryInfo(LoadExcelData(_bundlePath, item2.Value, _manifest), item.Key);
			}
		}
	}

	private void LoadItemCategoryInfo(ExcelData _ed, int _group)
	{
		if (_ed == null)
		{
			return;
		}
		GroupInfo groupInfo = dicItemGroupCategory[_group];
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = 0;
			if (int.TryParse(item.SafeGet(num++), out result))
			{
				int result2 = -1;
				if (int.TryParse(item.SafeGet(num++), out result2))
				{
					groupInfo.dicCategory[result2] = new CategoryInfo
					{
						sort = result,
						name = item.SafeGet(num++)
					};
				}
			}
		}
	}

	private void LoadAnimeGroupInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = 0;
			if (!int.TryParse(item.SafeGet(num++), out result))
			{
				continue;
			}
			int result2 = -1;
			if (int.TryParse(item.SafeGet(num++), out result2))
			{
				string text = item[num++];
				GroupInfo value = null;
				if (dicAGroupCategory.TryGetValue(result2, out value))
				{
					value.sort = result;
					value.name = text;
					continue;
				}
				value = new GroupInfo
				{
					sort = result,
					name = text
				};
				dicAGroupCategory.Add(result2, value);
			}
		}
	}

	private void LoadAnimeCategoryInfo(string _bundlePath, string _regex)
	{
		string _manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out _manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			return;
		}
		string strB = _regex.Split('_')[0].ToLower();
		SortedDictionary<int, SortedDictionary<int, string>> sortedDictionary = new SortedDictionary<int, SortedDictionary<int, string>>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Split('_')[0].ToLower().CompareTo(strB) == 0)
			{
				Match match = Regex.Match(array[i], _regex, RegexOptions.IgnoreCase);
				int key = int.Parse(match.Groups[1].Value);
				int key2 = int.Parse(match.Groups[2].Value);
				if (!sortedDictionary.ContainsKey(key2))
				{
					sortedDictionary.Add(key2, new SortedDictionary<int, string>());
				}
				sortedDictionary[key2].Add(key, array[i]);
			}
		}
		foreach (KeyValuePair<int, SortedDictionary<int, string>> item in sortedDictionary)
		{
			if (!dicAGroupCategory.ContainsKey(item.Key))
			{
				continue;
			}
			foreach (KeyValuePair<int, string> item2 in item.Value)
			{
				LoadAnimeCategoryInfo(LoadExcelData(_bundlePath, item2.Value, _manifest), item.Key);
			}
		}
	}

	private void LoadAnimeCategoryInfo(ExcelData _ed, int _group)
	{
		if (_ed == null)
		{
			return;
		}
		GroupInfo groupInfo = dicAGroupCategory[_group];
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = 0;
			if (int.TryParse(item.SafeGet(num++), out result))
			{
				int result2 = -1;
				if (int.TryParse(item.SafeGet(num++), out result2) && (_group != 0 || !MathfEx.RangeEqualOn(1, result2, 12) || IsInstallAI))
				{
					groupInfo.dicCategory[result2] = new CategoryInfo
					{
						sort = result,
						name = item.SafeGet(num++)
					};
				}
			}
		}
	}

	private IEnumerator LoadAnimeLoadInfoCoroutine(string _bundlePath, string _regex, Dictionary<int, Dictionary<int, Dictionary<int, AnimeLoadInfo>>> _dic, LoadAnimeInfoCoroutineFunc _func)
	{
		string manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			yield break;
		}
		string strB = _regex.Split('_')[0].ToLower();
		SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, string>>> sortedDictionary = new SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, string>>>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Split('_')[0].ToLower().CompareTo(strB) != 0)
			{
				continue;
			}
			Match match = Regex.Match(array[i], _regex, RegexOptions.IgnoreCase);
			int num = int.Parse(match.Groups[1].Value);
			int key = int.Parse(match.Groups[2].Value);
			int key2 = int.Parse(match.Groups[3].Value);
			if (num != 50 || IsInstallAdd)
			{
				if (!sortedDictionary.ContainsKey(key))
				{
					sortedDictionary.Add(key, new SortedDictionary<int, SortedDictionary<int, string>>());
				}
				if (!sortedDictionary[key].ContainsKey(key2))
				{
					sortedDictionary[key].Add(key2, new SortedDictionary<int, string>());
				}
				sortedDictionary[key][key2].Add(num, array[i]);
			}
		}
		foreach (KeyValuePair<int, SortedDictionary<int, SortedDictionary<int, string>>> item in sortedDictionary)
		{
			foreach (KeyValuePair<int, SortedDictionary<int, string>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, string> item3 in item2.Value)
				{
					_func(LoadExcelData(_bundlePath, item3.Value, manifest), _dic);
					if (waitTime.isOver)
					{
						yield return null;
						waitTime.Next();
					}
				}
			}
		}
	}

	private void LoadAnimeLoadInfo(ExcelData _ed, Dictionary<int, Dictionary<int, Dictionary<int, AnimeLoadInfo>>> _dic)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(2)
			select v.list)
		{
			if (!fileCheck.Check(item.SafeGet(5)))
			{
				continue;
			}
			int num = 0;
			int result = 0;
			if (!int.TryParse(item.SafeGet(num++), out result))
			{
				continue;
			}
			int result2 = -1;
			if (!int.TryParse(item.SafeGet(num++), out result2))
			{
				continue;
			}
			int num2 = int.Parse(item.SafeGet(num++));
			int num3 = int.Parse(item.SafeGet(num++));
			if (num2 == 0)
			{
				if (num3 == 0)
				{
					if (result2 != 0 && !IsInstallAI)
					{
						continue;
					}
				}
				else if (MathfEx.RangeEqualOn(1, num3, 12) && !IsInstallAI)
				{
					continue;
				}
			}
			if (!_dic.ContainsKey(num2))
			{
				_dic.Add(num2, new Dictionary<int, Dictionary<int, AnimeLoadInfo>>());
			}
			if (!_dic[num2].ContainsKey(num3))
			{
				_dic[num2].Add(num3, new Dictionary<int, AnimeLoadInfo>());
			}
			_dic[num2][num3][result2] = new AnimeLoadInfo
			{
				sort = result,
				name = item.SafeGet(num++),
				bundlePath = item.SafeGet(num++),
				fileName = item.SafeGet(num++),
				clip = item.SafeGet(num++),
				option = AnimeLoadInfo.LoadOption(item, num++, _animeSync: true)
			};
		}
	}

	private void LoadHAnimeLoadInfo(ExcelData _ed, Dictionary<int, Dictionary<int, Dictionary<int, AnimeLoadInfo>>> _dic)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(2)
			select v.list)
		{
			if (!fileCheck.Check(item.SafeGet(5)))
			{
				continue;
			}
			int result = 0;
			if (!int.TryParse(item.SafeGet(0), out result))
			{
				continue;
			}
			int result2 = 0;
			if (int.TryParse(item.SafeGet(1), out result2))
			{
				int key = int.Parse(item.SafeGet(2));
				int key2 = int.Parse(item.SafeGet(3));
				if (!_dic.ContainsKey(key))
				{
					_dic.Add(key, new Dictionary<int, Dictionary<int, AnimeLoadInfo>>());
				}
				if (!_dic[key].ContainsKey(key2))
				{
					_dic[key].Add(key2, new Dictionary<int, AnimeLoadInfo>());
				}
				_dic[key][key2][result2] = new HAnimeLoadInfo(4, item);
			}
		}
	}

	private IEnumerator LoadVoiceLoadInfoCoroutine(string _bundlePath, string _regex)
	{
		string manifest = "";
		string[] array = FindAllAssetName(_bundlePath, _regex, out manifest);
		if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
		{
			yield break;
		}
		string strB = _regex.Split('_')[0].ToLower();
		SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, string>>> sortedDictionary = new SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, string>>>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Split('_')[0].ToLower().CompareTo(strB) == 0)
			{
				Match match = Regex.Match(array[i], _regex, RegexOptions.IgnoreCase);
				int key = int.Parse(match.Groups[1].Value);
				int key2 = int.Parse(match.Groups[2].Value);
				int key3 = int.Parse(match.Groups[3].Value);
				if (!sortedDictionary.ContainsKey(key))
				{
					sortedDictionary.Add(key, new SortedDictionary<int, SortedDictionary<int, string>>());
				}
				if (!sortedDictionary[key].ContainsKey(key2))
				{
					sortedDictionary[key].Add(key2, new SortedDictionary<int, string>());
				}
				sortedDictionary[key][key2].Add(key3, array[i]);
			}
		}
		foreach (KeyValuePair<int, SortedDictionary<int, SortedDictionary<int, string>>> item in sortedDictionary)
		{
			foreach (KeyValuePair<int, SortedDictionary<int, string>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, string> item3 in item2.Value)
				{
					LoadVoiceLoadInfo(LoadExcelData(_bundlePath, item3.Value, manifest));
					if (waitTime.isOver)
					{
						yield return null;
						waitTime.Next();
					}
				}
			}
		}
	}

	private void LoadVoiceLoadInfo(ExcelData _ed)
	{
		foreach (List<string> item in from v in _ed.list.Skip(2)
			select v.list)
		{
			LoadVoiceLoadInfo(item);
		}
	}

	private LoadCommonInfo LoadVoiceLoadInfo(List<string> _list)
	{
		int num = 0;
		LoadCommonInfo loadCommonInfo = new LoadCommonInfo();
		int result = -1;
		if (!int.TryParse(_list[num++], out result))
		{
			return null;
		}
		int result2 = -1;
		if (!int.TryParse(_list[num++], out result2))
		{
			return null;
		}
		int result3 = -1;
		if (!int.TryParse(_list[num++], out result3))
		{
			return null;
		}
		loadCommonInfo.name = _list[num++];
		loadCommonInfo.bundlePath = _list[num++];
		loadCommonInfo.fileName = _list[num++];
		if (!dicVoiceLoadInfo.ContainsKey(result2))
		{
			dicVoiceLoadInfo.Add(result2, new Dictionary<int, Dictionary<int, LoadCommonInfo>>());
		}
		if (!dicVoiceLoadInfo[result2].ContainsKey(result3))
		{
			dicVoiceLoadInfo[result2].Add(result3, new Dictionary<int, LoadCommonInfo>());
		}
		dicVoiceLoadInfo[result2][result3][result] = loadCommonInfo;
		return loadCommonInfo;
	}

	private void LoadSoundLoadInfo(ExcelData _ed, Dictionary<int, LoadCommonInfo> _dic)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = -1;
			if (int.TryParse(item[num++], out result))
			{
				_dic[result] = new LoadCommonInfo
				{
					name = item[num++],
					bundlePath = item[num++],
					fileName = item[num++]
				};
			}
		}
	}

	private void LoadBGMLoadInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(1)
			select v.list)
		{
			int num = 0;
			int result = -1;
			if (int.TryParse(item[num++], out result) && (result >= 100 || IsInstallAI))
			{
				dicBGMLoadInfo[result] = new LoadCommonInfo
				{
					name = item[num++],
					bundlePath = item[num++],
					fileName = item[num++]
				};
			}
		}
	}

	private void LoadMapLoadInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(2)
			select v.list)
		{
			int num = 0;
			int result = -1;
			if (int.TryParse(item[num++], out result) && ((uint)result > 1u || IsInstallAI))
			{
				dicMapLoadInfo[result] = new MapLoadInfo(item);
			}
		}
	}

	private void LoadSkyPatternInfo(ExcelData _ed)
	{
		if (_ed == null)
		{
			return;
		}
		foreach (List<string> item in from v in _ed.list.Skip(2)
			select v.list)
		{
			int num = 0;
			int result = -1;
			if (int.TryParse(item[num++], out result))
			{
				dicSkyPatternInfo[result] = new SkyPatternInfo(item);
			}
		}
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			Object.DontDestroyOnLoad(base.gameObject);
			CheckIsAI();
			CheckIsAdd();
			isLoadList = false;
		}
	}
}
