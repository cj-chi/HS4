using System;
using System.Collections.Generic;
using IllusionUtility.SetUtility;
using Manager;
using UnityEngine;

public class ShapeHandInfo : ShapeInfoBase
{
	public enum DstName
	{
		cf_J_Hand_Index01_L,
		cf_J_Hand_Index02_L,
		cf_J_Hand_Index03_L,
		cf_J_Hand_Little01_L,
		cf_J_Hand_Little02_L,
		cf_J_Hand_Little03_L,
		cf_J_Hand_Middle01_L,
		cf_J_Hand_Middle02_L,
		cf_J_Hand_Middle03_L,
		cf_J_Hand_Ring01_L,
		cf_J_Hand_Ring02_L,
		cf_J_Hand_Ring03_L,
		cf_J_Hand_Thumb01_L,
		cf_J_Hand_Thumb02_L,
		cf_J_Hand_Thumb03_L,
		cf_J_Hand_Index01_R,
		cf_J_Hand_Index02_R,
		cf_J_Hand_Index03_R,
		cf_J_Hand_Little01_R,
		cf_J_Hand_Little02_R,
		cf_J_Hand_Little03_R,
		cf_J_Hand_Middle01_R,
		cf_J_Hand_Middle02_R,
		cf_J_Hand_Middle03_R,
		cf_J_Hand_Ring01_R,
		cf_J_Hand_Ring02_R,
		cf_J_Hand_Ring03_R,
		cf_J_Hand_Thumb01_R,
		cf_J_Hand_Thumb02_R,
		cf_J_Hand_Thumb03_R
	}

	public enum SrcName
	{
		cf_J_Hand_Index01_L,
		cf_J_Hand_Index02_L,
		cf_J_Hand_Index03_L,
		cf_J_Hand_Little01_L,
		cf_J_Hand_Little02_L,
		cf_J_Hand_Little03_L,
		cf_J_Hand_Middle01_L,
		cf_J_Hand_Middle02_L,
		cf_J_Hand_Middle03_L,
		cf_J_Hand_Ring01_L,
		cf_J_Hand_Ring02_L,
		cf_J_Hand_Ring03_L,
		cf_J_Hand_Thumb01_L,
		cf_J_Hand_Thumb02_L,
		cf_J_Hand_Thumb03_L,
		cf_J_Hand_Index01_R,
		cf_J_Hand_Index02_R,
		cf_J_Hand_Index03_R,
		cf_J_Hand_Little01_R,
		cf_J_Hand_Little02_R,
		cf_J_Hand_Little03_R,
		cf_J_Hand_Middle01_R,
		cf_J_Hand_Middle02_R,
		cf_J_Hand_Middle03_R,
		cf_J_Hand_Ring01_R,
		cf_J_Hand_Ring02_R,
		cf_J_Hand_Ring03_R,
		cf_J_Hand_Thumb01_R,
		cf_J_Hand_Thumb02_R,
		cf_J_Hand_Thumb03_R
	}

	public const int UPDATE_MASK_HAND_L = 1;

	public const int UPDATE_MASK_HAND_R = 2;

	public int updateMask;

	public override void InitShapeInfo(string manifest, string assetBundleAnmKey, string assetBundleCategory, string anmKeyInfoName, string cateInfoName, Transform trfObj)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		DstName[] array = (DstName[])Enum.GetValues(typeof(DstName));
		for (int i = 0; i < array.Length; i++)
		{
			DstName value = array[i];
			dictionary[value.ToString()] = (int)value;
		}
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		SrcName[] array2 = (SrcName[])Enum.GetValues(typeof(SrcName));
		for (int i = 0; i < array2.Length; i++)
		{
			SrcName value2 = array2[i];
			dictionary2[value2.ToString()] = (int)value2;
		}
		InitShapeInfoBase(manifest, assetBundleAnmKey, assetBundleCategory, anmKeyInfoName, cateInfoName, trfObj, dictionary, dictionary2, Singleton<Character>.Instance.AddLoadAssetBundle);
		base.InitEnd = true;
	}

	public override void ForceUpdate()
	{
		Update();
	}

	public override void Update()
	{
	}

	public override void UpdateAlways()
	{
		if (!base.InitEnd || dictSrc.Count == 0)
		{
			return;
		}
		BoneInfo value = null;
		if ((updateMask & 1) != 0)
		{
			int num = 14;
			for (int i = 0; i <= num; i++)
			{
				if (dictDst.TryGetValue(i, out value))
				{
					value.trfBone.SetLocalRotation(dictSrc[i].vctRot.x, dictSrc[i].vctRot.y, dictSrc[i].vctRot.z);
				}
			}
		}
		if ((updateMask & 2) == 0)
		{
			return;
		}
		int num2 = 29;
		for (int j = 15; j <= num2; j++)
		{
			if (dictDst.TryGetValue(j, out value))
			{
				value.trfBone.SetLocalRotation(dictSrc[j].vctRot.x, dictSrc[j].vctRot.y, dictSrc[j].vctRot.z);
			}
		}
	}
}
