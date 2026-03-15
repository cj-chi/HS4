using System.Collections.Generic;
using System.Text;
using AIChara;
using Manager;
using UnityEngine;

public class RootmotionOffset
{
	public struct Info
	{
		public Vector3 medium;

		public Vector3 small;
	}

	private ChaControl chara;

	private Dictionary<string, Info> Infos = new Dictionary<string, Info>();

	private Info nowInfo;

	private StringBuilder sbAbName = new StringBuilder();

	private StringBuilder tmpStateName = new StringBuilder();

	private ExcelData excelData;

	private List<string> row = new List<string>();

	public ChaControl Chara
	{
		get
		{
			return chara;
		}
		set
		{
			chara = value;
		}
	}

	public void OffsetInit(string _file)
	{
		if (_file == "")
		{
			return;
		}
		if (sbAbName == null)
		{
			sbAbName = new StringBuilder();
		}
		if (tmpStateName == null)
		{
			tmpStateName = new StringBuilder();
		}
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetRootMotionFolder);
		if (assetBundleNameListFromPath == null || assetBundleNameListFromPath.Count == 0)
		{
			return;
		}
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
			{
				continue;
			}
			int index = i;
			sbAbName.Clear();
			sbAbName.Append(assetBundleNameListFromPath[index]);
			if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), _file))
			{
				continue;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), _file);
			Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(sbAbName.ToString());
			if (!(excelData == null))
			{
				Infos = new Dictionary<string, Info>();
				Vector3[] array = new Vector3[2];
				int num = 2;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int num2 = 0;
					tmpStateName.Clear();
					tmpStateName.Append(row[num2++]);
					array[0].x = float.Parse(row[num2++]);
					array[0].y = float.Parse(row[num2++]);
					array[0].z = float.Parse(row[num2++]);
					array[1].x = float.Parse(row[num2++]);
					array[1].y = float.Parse(row[num2++]);
					array[1].z = float.Parse(row[num2++]);
					Info value = new Info
					{
						small = array[0],
						medium = array[1]
					};
					Infos.Add(tmpStateName.ToString(), value);
				}
			}
		}
	}

	public void Set(string _state)
	{
		if (!(Chara == null) && !(Chara.objTop == null) && !_state.IsNullOrEmpty() && Infos.TryGetValue(_state, out var value))
		{
			nowInfo.medium = value.medium;
			nowInfo.small = value.small;
			Chara.animBody.transform.localPosition = Vector3.zero;
			Chara.animBody.transform.localPosition += OffsetBlend(value);
		}
	}

	private Vector3 OffsetBlend(Info info)
	{
		float shapeBodyValue = Chara.GetShapeBodyValue(0);
		Vector3 zero = Vector3.zero;
		if ((double)shapeBodyValue > 0.5)
		{
			float t = Mathf.InverseLerp(0.5f, 1f, shapeBodyValue);
			for (int i = 0; i < 3; i++)
			{
				zero[i] = Mathf.Lerp(info.medium[i], 0f, t);
			}
		}
		else
		{
			float t2 = Mathf.InverseLerp(0f, 0.5f, shapeBodyValue);
			for (int j = 0; j < 3; j++)
			{
				zero[j] = Mathf.Lerp(info.small[j], info.medium[j], t2);
			}
		}
		return zero;
	}
}
