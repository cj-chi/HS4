using System;
using System.Collections.Generic;
using Illusion;
using Illusion.Extensions;
using Manager;

namespace AIChara;

[Serializable]
public class ListInfoBase
{
	private Dictionary<int, string> _dictInfo = new Dictionary<int, string>();

	public int ListIndex => GetInfoInt(ChaListDefine.KeyType.ListIndex);

	public int Category => GetInfoInt(ChaListDefine.KeyType.Category);

	public int Distribution => GetInfoInt(ChaListDefine.KeyType.DistributionNo);

	public int Id => GetInfoInt(ChaListDefine.KeyType.ID);

	public int Kind => GetInfoInt(ChaListDefine.KeyType.Kind);

	public string Name => Singleton<GameSystem>.Instance.language switch
	{
		GameSystem.Language.Japanese => GetInfo(ChaListDefine.KeyType.Name), 
		GameSystem.Language.English => GetInfo(ChaListDefine.KeyType.EN_US), 
		GameSystem.Language.SimplifiedChinese => GetInfo(ChaListDefine.KeyType.ZH_CN), 
		GameSystem.Language.TraditionalChinese => GetInfo(ChaListDefine.KeyType.ZH_TW), 
		_ => GetInfo(ChaListDefine.KeyType.Name), 
	};

	public int FontSize => Singleton<GameSystem>.Instance.language switch
	{
		GameSystem.Language.Japanese => GetInfoInt(ChaListDefine.KeyType.JA_JP_PT), 
		GameSystem.Language.English => GetInfoInt(ChaListDefine.KeyType.EN_US_PT), 
		GameSystem.Language.SimplifiedChinese => GetInfoInt(ChaListDefine.KeyType.ZH_CN_PT), 
		GameSystem.Language.TraditionalChinese => GetInfoInt(ChaListDefine.KeyType.ZH_TW_PT), 
		_ => 26, 
	};

	public IReadOnlyDictionary<int, string> dictInfo { get; }

	public ListInfoBase()
	{
		dictInfo = _dictInfo;
	}

	public bool Set(int entryCnt, int _cateNo, int _distNo, List<string> lstKey, List<string> lstData)
	{
		string[] names = Utils.Enum<ChaListDefine.KeyType>.Names;
		int key = names.Check("ListIndex");
		_dictInfo[key] = entryCnt.ToString();
		int key2 = names.Check("Category");
		_dictInfo[key2] = _cateNo.ToString();
		int key3 = names.Check("DistributionNo");
		_dictInfo[key3] = _distNo.ToString();
		for (int i = 0; i < lstKey.Count; i++)
		{
			int key4 = names.Check(lstKey[i]);
			_dictInfo[key4] = lstData[i];
		}
		return true;
	}

	public void ChangeListIndex(int index)
	{
		_ = Utils.Enum<ChaListDefine.KeyType>.Names;
		int key = 0;
		_dictInfo[key] = index.ToString();
	}

	public void ChangeMainManifest(string mainmanifset)
	{
		int key = 14;
		if (_dictInfo.ContainsKey(key))
		{
			_dictInfo[key] = mainmanifset;
		}
	}

	public void ChangeMainAB(string mainab)
	{
		int key = 15;
		if (_dictInfo.ContainsKey(key))
		{
			_dictInfo[key] = mainab;
		}
	}

	public int GetInfoInt(ChaListDefine.KeyType keyType)
	{
		if (!int.TryParse(GetInfo(keyType), out var result))
		{
			return -1;
		}
		return result;
	}

	public float GetInfoFloat(ChaListDefine.KeyType keyType)
	{
		if (!float.TryParse(GetInfo(keyType), out var result))
		{
			return -1f;
		}
		return result;
	}

	public string GetInfo(ChaListDefine.KeyType keyType)
	{
		if (!_dictInfo.TryGetValue((int)keyType, out var value))
		{
			return "0";
		}
		return value;
	}
}
