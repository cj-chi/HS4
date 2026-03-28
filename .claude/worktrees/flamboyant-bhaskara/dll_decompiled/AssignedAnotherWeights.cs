using System;
using System.Collections.Generic;
using System.Linq;
using IllusionUtility.GetUtility;
using UnityEngine;

public class AssignedAnotherWeights
{
	public Dictionary<string, GameObject> dictBone { get; private set; }

	public AssignedAnotherWeights()
	{
		dictBone = new Dictionary<string, GameObject>();
	}

	public void Release()
	{
		dictBone.Clear();
	}

	public void CreateBoneList(GameObject obj, string name)
	{
		dictBone.Clear();
		CreateBoneListLoop(obj, name);
	}

	public void CreateBoneListMultiple(GameObject obj, params string[] names)
	{
		dictBone.Clear();
		foreach (string name in names)
		{
			CreateBoneListLoop(obj, name);
		}
	}

	public void CreateBoneListLoop(GameObject obj, string name)
	{
		if ((string.Compare(obj.name, 0, name, 0, name.Length) == 0 || "" == name) && !dictBone.ContainsKey(obj.name))
		{
			dictBone[obj.name] = obj;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			CreateBoneListLoop(obj.transform.GetChild(i).gameObject, name);
		}
	}

	public void CreateBoneList(GameObject obj, string assetBundleName, string assetName)
	{
		dictBone.Clear();
		if (!AssetBundleCheck.IsFile(assetBundleName, assetName))
		{
			_ = "読み込みエラー\r\nassetBundleName：" + assetBundleName + "\tassetName：" + assetName;
			return;
		}
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(assetBundleName, assetName, typeof(TextAsset));
		if (assetBundleLoadAssetOperation.IsEmpty())
		{
			_ = "読み込みエラー\r\nassetName：" + assetName;
			return;
		}
		YS_Assist.GetListString(assetBundleLoadAssetOperation.GetAsset<TextAsset>().text, out var data);
		int length = data.GetLength(0);
		int length2 = data.GetLength(1);
		if (length != 0 && length2 != 0)
		{
			for (int i = 0; i < length; i++)
			{
				Transform transform = obj.transform.FindLoop(data[i, 0]);
				if ((bool)transform)
				{
					dictBone[data[i, 0]] = transform.gameObject;
				}
			}
		}
		AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: true);
	}

	public void CreateBoneList(GameObject obj, string[] bonename)
	{
		dictBone.Clear();
		for (int i = 0; i < bonename.Length; i++)
		{
			Transform transform = obj.transform.FindLoop(bonename[i]);
			if ((bool)transform)
			{
				dictBone[bonename[i]] = transform.gameObject;
			}
		}
	}

	public void AssignedWeights(GameObject obj, string delTopName, Transform rootBone = null)
	{
		if (dictBone != null && dictBone.Count != 0 && !(null == obj))
		{
			AssignedWeightsLoop(obj.transform, rootBone);
			Transform transform = obj.transform.FindLoop(delTopName);
			if ((bool)transform)
			{
				transform.SetParent(null);
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	private void AssignedWeightsLoop(Transform t, Transform rootBone = null)
	{
		SkinnedMeshRenderer smr = t.GetComponent<SkinnedMeshRenderer>();
		if ((bool)smr)
		{
			int num = smr.bones.Length;
			Transform[] array = new Transform[num];
			GameObject value = null;
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (dictBone.TryGetValue(smr.bones[i].name, out value))
				{
					array[i] = value.transform;
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				array = array.Where((Transform x) => null != x).ToArray();
			}
			if (array.Length != smr.bones.Length)
			{
				new List<Transform>();
				int i2 = 0;
				while (i2 < smr.bones.Length)
				{
					if (null == array.FirstOrDefault((Transform x) => x.name == smr.bones[i2].name))
					{
						Array.Resize(ref array, array.Length + 1);
						array[array.Length - 1] = smr.bones[i2];
					}
					int num2 = i2 + 1;
					i2 = num2;
				}
			}
			smr.bones = array;
			if ((bool)rootBone)
			{
				smr.rootBone = rootBone;
			}
			else if ((bool)smr.rootBone && dictBone.TryGetValue(smr.rootBone.name, out value))
			{
				smr.rootBone = value.transform;
			}
		}
		foreach (Transform item in t.gameObject.transform)
		{
			AssignedWeightsLoop(item, rootBone);
		}
	}

	public void AssignedWeightsAndSetBounds(GameObject obj, string delTopName, Bounds bounds, Transform rootBone = null)
	{
		if (dictBone != null && dictBone.Count != 0 && !(null == obj))
		{
			AssignedWeightsAndSetBoundsLoop(obj.transform, bounds, rootBone);
			Transform transform = obj.transform.FindLoop(delTopName);
			if ((bool)transform)
			{
				transform.SetParent(null);
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	private void AssignedWeightsAndSetBoundsLoop(Transform t, Bounds bounds, Transform rootBone = null)
	{
		SkinnedMeshRenderer smr = t.GetComponent<SkinnedMeshRenderer>();
		if ((bool)smr)
		{
			int num = smr.bones.Length;
			Transform[] array = new Transform[num];
			GameObject value = null;
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (dictBone.TryGetValue(smr.bones[i].name, out value))
				{
					array[i] = value.transform;
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				array = array.Where((Transform x) => null != x).ToArray();
			}
			if (array.Length != smr.bones.Length)
			{
				new List<Transform>();
				int i2 = 0;
				while (i2 < smr.bones.Length)
				{
					if (null == array.FirstOrDefault((Transform x) => x.name == smr.bones[i2].name))
					{
						Array.Resize(ref array, array.Length + 1);
						array[array.Length - 1] = smr.bones[i2];
					}
					int num2 = i2 + 1;
					i2 = num2;
				}
			}
			smr.bones = array;
			smr.localBounds = bounds;
			Cloth component = smr.gameObject.GetComponent<Cloth>();
			if ((bool)rootBone && null == component)
			{
				smr.rootBone = rootBone;
			}
			else if ((bool)smr.rootBone && dictBone.TryGetValue(smr.rootBone.name, out value))
			{
				smr.rootBone = value.transform;
			}
		}
		foreach (Transform item in t.gameObject.transform)
		{
			AssignedWeightsAndSetBoundsLoop(item, bounds, rootBone);
		}
	}
}
