using System;
using System.Collections.Generic;
using System.IO;
using Manager;
using MessagePack;
using UnityEngine;

namespace AIChara;

public class ChaListControl
{
	private Dictionary<int, Dictionary<int, ListInfoBase>> dictListInfo = new Dictionary<int, Dictionary<int, ListInfoBase>>();

	private List<int> lstItemIsInit = new List<int>();

	private List<int> lstItemIsNew = new List<int>();

	public Dictionary<int, byte> _itemIDInfo { get; set; } = new Dictionary<int, byte>();

	public IReadOnlyDictionary<int, byte> itemIDInfo { get; }

	public ChaListControl()
	{
		ChaListDefine.CategoryNo[] array = (ChaListDefine.CategoryNo[])Enum.GetValues(typeof(ChaListDefine.CategoryNo));
		foreach (ChaListDefine.CategoryNo key in array)
		{
			dictListInfo[(int)key] = new Dictionary<int, ListInfoBase>();
		}
		itemIDInfo = _itemIDInfo;
	}

	public Dictionary<int, ListInfoBase> GetCategoryInfo(ChaListDefine.CategoryNo type)
	{
		Dictionary<int, ListInfoBase> value = null;
		if (!dictListInfo.TryGetValue((int)type, out value))
		{
			return null;
		}
		return new Dictionary<int, ListInfoBase>(value);
	}

	public ListInfoBase GetListInfo(ChaListDefine.CategoryNo type, int id)
	{
		Dictionary<int, ListInfoBase> value = null;
		if (!dictListInfo.TryGetValue((int)type, out value))
		{
			return null;
		}
		ListInfoBase value2 = null;
		if (!value.TryGetValue(id, out value2))
		{
			return null;
		}
		return value2;
	}

	public bool LoadListInfoAll()
	{
		ChaListDefine.CategoryNo[] array = (ChaListDefine.CategoryNo[])Enum.GetValues(typeof(ChaListDefine.CategoryNo));
		Dictionary<int, ListInfoBase> value = null;
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath("list/characustom/");
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAllAsset(assetBundleNameListFromPath[i], typeof(TextAsset));
			if (assetBundleLoadAssetOperation == null)
			{
				_ = "読み込みエラー\r\nassetBundleName：" + assetBundleNameListFromPath[i];
				continue;
			}
			if (assetBundleLoadAssetOperation.IsEmpty())
			{
				AssetBundleManager.UnloadAssetBundle(assetBundleNameListFromPath[i], isUnloadForceRefCount: true);
				continue;
			}
			TextAsset[] allAssets = assetBundleLoadAssetOperation.GetAllAssets<TextAsset>();
			if (allAssets == null || allAssets.Length == 0)
			{
				AssetBundleManager.UnloadAssetBundle(assetBundleNameListFromPath[i], isUnloadForceRefCount: true);
				continue;
			}
			ChaListDefine.CategoryNo[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				ChaListDefine.CategoryNo key = array2[j];
				if (!dictListInfo.TryGetValue((int)key, out value))
				{
					continue;
				}
				TextAsset[] array3 = allAssets;
				foreach (TextAsset textAsset in array3)
				{
					if (!(YS_Assist.GetRemoveStringRight(textAsset.name, "_") != key.ToString() + "_"))
					{
						LoadListInfo(value, textAsset);
					}
				}
			}
			AssetBundleManager.UnloadAssetBundle(assetBundleNameListFromPath[i], isUnloadForceRefCount: true);
		}
		value = null;
		if (dictListInfo.TryGetValue(210, out value))
		{
			ListInfoBase value2 = null;
			if (value.TryGetValue(0, out value2))
			{
				value2.ChangeMainManifest("add35");
				value2.ChangeMainAB("chara/35/fo_head_35.unity3d");
			}
		}
		EntryClothesIsInit();
		LoadItemID();
		UnityEngine.Resources.UnloadUnusedAssets();
		GC.Collect();
		return true;
	}

	public bool ReLoadListInfoAll()
	{
		ChaListDefine.CategoryNo[] array = (ChaListDefine.CategoryNo[])Enum.GetValues(typeof(ChaListDefine.CategoryNo));
		foreach (ChaListDefine.CategoryNo key in array)
		{
			dictListInfo[(int)key] = new Dictionary<int, ListInfoBase>();
		}
		return LoadListInfoAll();
	}

	private bool LoadListInfo(Dictionary<int, ListInfoBase> dictData, string assetBundleName, string assetName)
	{
		TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(assetBundleName, assetName);
		if (null == textAsset)
		{
			return false;
		}
		bool result = LoadListInfo(dictData, textAsset);
		AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: true);
		return result;
	}

	private bool LoadListInfo(Dictionary<int, ListInfoBase> dictData, TextAsset ta)
	{
		if (null == ta)
		{
			return false;
		}
		ChaListData chaListData = MessagePackSerializer.Deserialize<ChaListData>(ta.bytes);
		if (chaListData == null)
		{
			return false;
		}
		foreach (KeyValuePair<int, List<string>> dict in chaListData.dictList)
		{
			int count = dictData.Count;
			ListInfoBase listInfoBase = new ListInfoBase();
			if (listInfoBase.Set(count, chaListData.categoryNo, chaListData.distributionNo, chaListData.lstKey, dict.Value) && (Singleton<GameSystem>.Instance.isSpecial02 || listInfoBase.Distribution != 2) && (Singleton<GameSystem>.Instance.isSpecial03 || listInfoBase.Distribution != 3) && (Singleton<GameSystem>.Instance.isSpecial04 || listInfoBase.Distribution != 4) && (GameSystem.isAdd50 || listInfoBase.Distribution < 50) && !dictData.ContainsKey(listInfoBase.Id))
			{
				dictData[listInfoBase.Id] = listInfoBase;
				int infoInt = listInfoBase.GetInfoInt(ChaListDefine.KeyType.Possess);
				int item = listInfoBase.Category * 1000 + listInfoBase.Id;
				if (1 == infoInt)
				{
					lstItemIsInit.Add(item);
				}
				else if (2 == infoInt)
				{
					lstItemIsNew.Add(item);
				}
			}
		}
		return true;
	}

	public static List<ExcelData.Param> LoadExcelData(string assetBunndlePath, string assetName, int cellS, int rowS)
	{
		if (!AssetBundleCheck.IsFile(assetBunndlePath, assetName))
		{
			return null;
		}
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(assetBunndlePath, assetName, typeof(ExcelData));
		AssetBundleManager.UnloadAssetBundle(assetBunndlePath, isUnloadForceRefCount: true);
		if (assetBundleLoadAssetOperation.IsEmpty())
		{
			return null;
		}
		ExcelData asset = assetBundleLoadAssetOperation.GetAsset<ExcelData>();
		int num = asset.MaxCell - 1;
		return asset.Get(end: new ExcelData.Specify(num, asset.list[num].list.Count - 1), start: new ExcelData.Specify(cellS, rowS));
	}

	public void EntryClothesIsInit()
	{
		for (int i = 0; i < lstItemIsInit.Count; i++)
		{
			AddItemID(lstItemIsInit[i], 2);
		}
		for (int j = 0; j < lstItemIsNew.Count; j++)
		{
			AddItemID(lstItemIsNew[j], 1);
		}
		lstItemIsInit.Clear();
		lstItemIsInit.TrimExcess();
		lstItemIsNew.Clear();
		lstItemIsNew.TrimExcess();
	}

	public void SaveItemID()
	{
		string path = UserData.Path + ChaListDefine.CheckItemFile;
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(ChaListDefine.CheckItemVersion.ToString());
		binaryWriter.Write(_itemIDInfo.Count);
		foreach (KeyValuePair<int, byte> item in _itemIDInfo)
		{
			binaryWriter.Write(item.Key);
			binaryWriter.Write(item.Value);
		}
	}

	public void LoadItemID()
	{
		string path = UserData.Path + ChaListDefine.CheckItemFile;
		if (!File.Exists(path))
		{
			return;
		}
		using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
		using BinaryReader binaryReader = new BinaryReader(input);
		binaryReader.ReadString();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int key = binaryReader.ReadInt32();
			byte b = binaryReader.ReadByte();
			byte value = 0;
			if (_itemIDInfo.TryGetValue(key, out value))
			{
				if (value < b)
				{
					_itemIDInfo[key] = b;
				}
			}
			else
			{
				_itemIDInfo.Add(key, b);
			}
		}
	}

	public void AddItemID(string IDStr, byte flags = 1)
	{
		string[] array = IDStr.Split('/');
		for (int i = 0; i < array.Length; i++)
		{
			int key = int.Parse(array[i]);
			byte value = 0;
			if (_itemIDInfo.TryGetValue(key, out value))
			{
				if (value < flags)
				{
					_itemIDInfo[key] = flags;
				}
			}
			else
			{
				_itemIDInfo.Add(key, flags);
			}
		}
	}

	public void AddItemID(int pid, byte flags = 1)
	{
		byte value = 0;
		if (_itemIDInfo.TryGetValue(pid, out value))
		{
			if (value < flags)
			{
				_itemIDInfo[pid] = flags;
			}
		}
		else
		{
			_itemIDInfo.Add(pid, flags);
		}
	}

	public void AddItemID(int category, int id, byte flags)
	{
		int pid = category * 1000 + id;
		AddItemID(pid, flags);
	}

	public byte CheckItemID(int pid)
	{
		byte value = 0;
		_itemIDInfo.TryGetValue(pid, out value);
		return value;
	}

	public byte CheckItemID(int category, int id)
	{
		int pid = category * 1000 + id;
		return CheckItemID(pid);
	}
}
