using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using UnityEngine;

namespace Manager;

public class Character : Singleton<Character>
{
	public List<AssetBundleData> lstLoadAssetBundleInfo = new List<AssetBundleData>();

	public bool loadAssetBundle;

	public ChaListControl chaListCtrl { get; private set; }

	public SortedDictionary<int, ChaControl> dictEntryChara { get; private set; }

	public bool enableCharaLoadGCClear { get; set; } = true;

	public bool customLoadGCClear { get; set; }

	public string conciergePath => UserData.Path + "chara/navi/navi.png";

	public string sitriPath => UserData.Path + "chara/navi/sitri.png";

	public bool isMod { get; set; }

	private Material matHairDithering { get; set; }

	public Shader shaderDithering { get; private set; }

	private Material matHairCutout { get; set; }

	public Shader shaderCutout { get; private set; }

	public void BeginLoadAssetBundle()
	{
		lstLoadAssetBundleInfo.Clear();
		loadAssetBundle = true;
	}

	public void AddLoadAssetBundle(string assetBundleName, string manifestName)
	{
		if (manifestName.IsNullOrEmpty())
		{
			manifestName = "abdata";
		}
		lstLoadAssetBundleInfo.Add(new AssetBundleManifestData(assetBundleName, string.Empty, manifestName));
	}

	public void EndLoadAssetBundle(bool forceRemove = false)
	{
		foreach (AssetBundleData item in lstLoadAssetBundleInfo)
		{
			AssetBundleManager.UnloadAssetBundle(item.bundle, isUnloadForceRefCount: true, null, forceRemove);
		}
		UnityEngine.Resources.UnloadUnusedAssets();
		GC.Collect();
		lstLoadAssetBundleInfo.Clear();
		loadAssetBundle = false;
	}

	public ChaControl CreateChara(byte _sex, GameObject parent, int id, ChaFileControl _chaFile = null)
	{
		int num = 0;
		int num2 = 1;
		foreach (KeyValuePair<int, ChaControl> item in dictEntryChara)
		{
			if (item.Value.sex == _sex)
			{
				num2++;
				string text = ((_sex == 0) ? "chaM_" : "chaF_") + num2.ToString("000");
				while (item.Value.name == text)
				{
					num2++;
					text = ((_sex == 0) ? "chaM_" : "chaF_") + num2.ToString("000");
				}
			}
			if (num != item.Key)
			{
				break;
			}
			num++;
		}
		GameObject gameObject = new GameObject(((_sex == 0) ? "chaM_" : "chaF_") + num2.ToString("000"));
		if ((bool)parent)
		{
			gameObject.transform.SetParent(parent.transform, worldPositionStays: false);
		}
		ChaControl chaControl = gameObject.AddComponent<ChaControl>();
		if ((bool)chaControl)
		{
			chaControl.Initialize(_sex, gameObject, id, num, _chaFile);
		}
		dictEntryChara.Add(num, chaControl);
		return chaControl;
	}

	public bool IsChara(ChaControl cha)
	{
		foreach (KeyValuePair<int, ChaControl> item in dictEntryChara)
		{
			if (item.Value == cha)
			{
				return true;
			}
		}
		return false;
	}

	public bool DeleteChara(ChaControl cha, bool entryOnly = false)
	{
		foreach (KeyValuePair<int, ChaControl> item in dictEntryChara)
		{
			if (item.Value == cha)
			{
				if (!entryOnly)
				{
					item.Value.gameObject.name = "Delete_Reserve";
					item.Value.transform.SetParent(null);
					UnityEngine.Object.Destroy(item.Value.gameObject);
				}
				dictEntryChara.Remove(item.Key);
				return true;
			}
		}
		return false;
	}

	public bool DeleteCharaSex(int _sex)
	{
		foreach (KeyValuePair<int, ChaControl> item in dictEntryChara)
		{
			if ((bool)item.Value && item.Value.sex == _sex)
			{
				item.Value.gameObject.name = "Delete_Reserve";
				item.Value.transform.SetParent(null);
				UnityEngine.Object.Destroy(item.Value.gameObject);
			}
		}
		dictEntryChara.Clear();
		return false;
	}

	public void DeleteCharaAll()
	{
		foreach (KeyValuePair<int, ChaControl> item in dictEntryChara)
		{
			if ((bool)item.Value)
			{
				item.Value.gameObject.name = "Delete_Reserve";
				item.Value.transform.SetParent(null);
				UnityEngine.Object.Destroy(item.Value.gameObject);
			}
		}
		dictEntryChara.Clear();
	}

	public List<ChaControl> GetCharaList(byte _sex)
	{
		return (from x in dictEntryChara
			where x.Value.sex == _sex
			select x.Value).ToList();
	}

	public ChaControl GetChara(byte _sex, int _id)
	{
		try
		{
			return dictEntryChara.Where((KeyValuePair<int, ChaControl> s) => s.Value.sex == _sex).First((KeyValuePair<int, ChaControl> v) => v.Value.chaID == _id).Value;
		}
		catch (ArgumentNullException)
		{
		}
		catch (InvalidOperationException)
		{
		}
		return null;
	}

	public ChaControl GetChara(int _id)
	{
		try
		{
			return dictEntryChara.First((KeyValuePair<int, ChaControl> v) => v.Value.chaID == _id).Value;
		}
		catch (ArgumentNullException)
		{
		}
		catch (InvalidOperationException)
		{
		}
		return null;
	}

	public ChaControl GetCharaFromLoadNo(int _no)
	{
		try
		{
			return dictEntryChara.First((KeyValuePair<int, ChaControl> v) => v.Value.loadNo == _no).Value;
		}
		catch (ArgumentNullException)
		{
		}
		catch (InvalidOperationException)
		{
		}
		return null;
	}

	public static void ChangeRootParent(ChaControl cha, Transform trfNewParent)
	{
		if (null != cha)
		{
			cha.transform.SetParent(trfNewParent, worldPositionStays: false);
		}
	}

	public string GetCharaTypeName(int no)
	{
		if (!SingletonInitializer<Voice>.initialized)
		{
			return "不明";
		}
		if (!Voice.infoTable.TryGetValue(no, out var value))
		{
			return "不明";
		}
		return value.Get(Singleton<GameSystem>.Instance.languageInt);
	}

	public void LoadConciergeCharaFile(ChaControl _chara)
	{
		if (!_chara.chaFile.LoadCharaFile(conciergePath, 1))
		{
			_chara.LoadPreset(1, "ill_Default_Navi");
		}
	}

	public void LoadSitriCharaFile(ChaControl _chara)
	{
		if (!_chara.chaFile.LoadCharaFile(sitriPath, 1))
		{
			_chara.LoadPreset(1, "ill_Default_Sitri");
		}
	}

	protected new void Awake()
	{
		if (CheckInstance())
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			chaListCtrl = new ChaListControl();
			dictEntryChara = new SortedDictionary<int, ChaControl>();
			matHairDithering = CommonLib.LoadAsset<Material>("chara/hair_shader_mat.unity3d", "hair_dithering", clone: false, "abdata");
			matHairCutout = CommonLib.LoadAsset<Material>("chara/hair_shader_mat.unity3d", "hair_cutout", clone: false, "abdata");
			shaderDithering = matHairDithering.shader;
			shaderCutout = matHairCutout.shader;
			chaListCtrl.LoadListInfoAll();
		}
	}

	protected void Update()
	{
		if (!CheckInstance())
		{
			return;
		}
		foreach (KeyValuePair<int, ChaControl> item in dictEntryChara)
		{
			item.Value.UpdateForce();
		}
	}

	protected void LateUpdate()
	{
		foreach (ChaControl value in dictEntryChara.Values)
		{
			value.LateUpdateForce();
		}
	}

	protected void OnDestroy()
	{
		chaListCtrl.SaveItemID();
	}
}
