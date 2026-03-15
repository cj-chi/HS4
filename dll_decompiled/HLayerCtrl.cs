using System.Collections.Generic;
using System.Text;
using AIChara;
using Manager;
using UnityEngine;

public class HLayerCtrl : MonoBehaviour
{
	public struct HLayerInfo
	{
		public int LayerID;

		public float weight;
	}

	private ChaControl[] chaFemales;

	private ChaControl[] chaMales;

	[SerializeField]
	private HSceneFlagCtrl ctrlFlag;

	private ExcelData excelData;

	private List<string> row = new List<string>();

	private AnimatorStateInfo stateInfo;

	private StringBuilder sbAbName = new StringBuilder();

	private StringBuilder stateName = new StringBuilder();

	private Dictionary<int, Dictionary<string, HLayerInfo>[]> LayerInfos;

	public void Init(ChaControl[] _chaFemales, ChaControl[] _chaMales)
	{
		ctrlFlag = Singleton<HSceneFlagCtrl>.Instance;
		chaFemales = _chaFemales;
		chaMales = _chaMales;
		LayerInfos = new Dictionary<int, Dictionary<string, HLayerInfo>[]>();
		LayerInfos.Add(0, new Dictionary<string, HLayerInfo>[2]);
		LayerInfos.Add(1, new Dictionary<string, HLayerInfo>[2]);
		foreach (KeyValuePair<int, Dictionary<string, HLayerInfo>[]> layerInfo in LayerInfos)
		{
			layerInfo.Value[0] = new Dictionary<string, HLayerInfo>();
			layerInfo.Value[1] = new Dictionary<string, HLayerInfo>();
		}
	}

	public void Release()
	{
		foreach (KeyValuePair<int, Dictionary<string, HLayerInfo>[]> layerInfo in LayerInfos)
		{
			Dictionary<string, HLayerInfo>[] value = layerInfo.Value;
			for (int i = 0; i < value.Length; i++)
			{
				value[i]?.Clear();
			}
		}
		LayerInfos.Clear();
		chaFemales = null;
		chaMales = null;
	}

	public void LoadExcel(string animatorName, int _sex, int _id)
	{
		Dictionary<string, HLayerInfo> dictionary = new Dictionary<string, HLayerInfo>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetLayerCtrlListFolder);
		assetBundleNameListFromPath.Sort();
		excelData = null;
		HLayerInfo value = default(HLayerInfo);
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
			{
				continue;
			}
			sbAbName.Clear();
			sbAbName.Append(assetBundleNameListFromPath[i]);
			if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), animatorName))
			{
				continue;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), animatorName);
			Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(sbAbName.ToString());
			if (excelData == null)
			{
				continue;
			}
			int num = 1;
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				stateName.Clear();
				stateName.Append(row[num2++]);
				if (!(stateName.ToString() == ""))
				{
					if (!dictionary.ContainsKey(stateName.ToString()))
					{
						dictionary.Add(stateName.ToString(), default(HLayerInfo));
					}
					int result = 0;
					float result2 = 0f;
					if (int.TryParse(row[num2++], out result))
					{
						float.TryParse(row[num2++], out result2);
					}
					else
					{
						result = 0;
					}
					value.LayerID = result;
					value.weight = result2;
					dictionary[stateName.ToString()] = value;
				}
			}
		}
		LayerInfos[_sex][_id].Clear();
		foreach (KeyValuePair<string, HLayerInfo> item in dictionary)
		{
			LayerInfos[_sex][_id].Add(item.Key, item.Value);
		}
	}

	private void LateUpdate()
	{
		if (chaFemales != null && chaFemales.Length != 0 && !(chaFemales[0] == null))
		{
			setLayer(chaFemales, 1);
			setLayer(chaMales, 0);
		}
	}

	private void setLayer(ChaControl[] charas, int Sex)
	{
		for (int i = 0; i < 2; i++)
		{
			if (charas[i] == null || charas[i].animBody == null || charas[i].animBody.runtimeAnimatorController == null)
			{
				continue;
			}
			stateInfo = charas[i].getAnimatorStateInfo(0);
			bool flag = false;
			foreach (string key in LayerInfos[Sex][i].Keys)
			{
				if (stateInfo.IsName(key))
				{
					flag = true;
					int layerID = LayerInfos[Sex][i][key].LayerID;
					float weight = LayerInfos[Sex][i][key].weight;
					setLayer(charas, Sex, i, layerID, weight);
				}
			}
			if (!flag)
			{
				for (int j = 1; j < charas[i].animBody.layerCount; j++)
				{
					charas[i].setLayerWeight(0f, j);
					ctrlFlag.lstSyncAnimLayers[Sex, i].Remove(j);
				}
			}
		}
	}

	private void setLayer(ChaControl[] charas, int sex, int index, int layer, float weight)
	{
		if (layer != 0)
		{
			if (!ctrlFlag.lstSyncAnimLayers[sex, index].Contains(layer))
			{
				for (int i = 1; i < charas[index].animBody.layerCount; i++)
				{
					if (layer == i)
					{
						charas[index].setLayerWeight(weight, layer);
					}
					else
					{
						charas[index].setLayerWeight(0f, i);
					}
				}
				ctrlFlag.lstSyncAnimLayers[sex, index].Add(layer);
			}
			else if (weight == 0f)
			{
				charas[index].setLayerWeight(0f, layer);
				ctrlFlag.lstSyncAnimLayers[sex, index].Remove(layer);
			}
		}
		else
		{
			for (int j = 1; j < charas[index].animBody.layerCount; j++)
			{
				charas[index].setLayerWeight(0f, j);
			}
		}
	}
}
