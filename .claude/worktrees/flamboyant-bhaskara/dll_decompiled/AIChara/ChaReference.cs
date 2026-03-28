using System.Collections.Generic;
using UnityEngine;

namespace AIChara;

public class ChaReference : MonoBehaviour
{
	public enum RefObjKey
	{
		HeadParent,
		k_f_shoulderL_00,
		k_f_shoulderR_00,
		k_f_handL_00,
		k_f_handR_00,
		mask_braA,
		mask_braB,
		mask_innerTB,
		mask_innerB,
		mask_panst
	}

	public const ulong FbxTypeBodyBone = 1uL;

	public const ulong FbxTypeHeadBone = 2uL;

	public const ulong FbxTypeInnerT = 7uL;

	public const ulong FbxTypeInnerB = 8uL;

	public const ulong FbxTypePanst = 10uL;

	private Dictionary<int, GameObject> dictRefObj = new Dictionary<int, GameObject>();

	public void Log_ReferenceObjectNull()
	{
		if (dictRefObj == null)
		{
			return;
		}
		foreach (KeyValuePair<int, GameObject> item in dictRefObj)
		{
			if (!(null != item.Value))
			{
				_ = "There is no " + item.Key + ".";
			}
		}
	}

	public void CreateReferenceInfo(ulong flags, GameObject objRef)
	{
		ReleaseRefObject(flags);
		if (null == objRef)
		{
			return;
		}
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(objRef.transform);
		if (flags == 1 || flags == 2)
		{
			return;
		}
		ulong num = flags - 7;
		if (num <= 3)
		{
			switch (num)
			{
			case 0uL:
				dictRefObj[5] = findAssist.GetObjectFromName("o_bra_a");
				dictRefObj[6] = findAssist.GetObjectFromName("o_bra_b");
				dictRefObj[7] = findAssist.GetObjectFromName("o_shorts_a");
				break;
			case 1uL:
				dictRefObj[8] = findAssist.GetObjectFromName("o_shorts_a");
				break;
			case 3uL:
				dictRefObj[9] = findAssist.GetObjectFromName("o_panst_a");
				break;
			case 2uL:
				break;
			}
		}
	}

	public void ReleaseRefObject(ulong flags)
	{
		if (flags == 1 || flags == 2)
		{
			return;
		}
		ulong num = flags - 7;
		if (num <= 3)
		{
			switch (num)
			{
			case 0uL:
				dictRefObj.Remove(5);
				dictRefObj.Remove(6);
				dictRefObj.Remove(7);
				break;
			case 1uL:
				dictRefObj.Remove(8);
				break;
			case 3uL:
				dictRefObj.Remove(9);
				break;
			case 2uL:
				break;
			}
		}
	}

	public void ReleaseRefAll()
	{
		dictRefObj.Clear();
	}

	public GameObject GetReferenceInfo(RefObjKey key)
	{
		ChaControl chaControl = this as ChaControl;
		if (null != chaControl)
		{
			switch (key)
			{
			case RefObjKey.HeadParent:
				if (null == chaControl.cmpBoneBody)
				{
					return null;
				}
				if (null == chaControl.cmpBoneBody.targetEtc.trfHeadParent)
				{
					return null;
				}
				return chaControl.cmpBoneBody.targetEtc.trfHeadParent.gameObject;
			case RefObjKey.k_f_shoulderL_00:
				if (null == chaControl.cmpBoneBody)
				{
					return null;
				}
				if (null == chaControl.cmpBoneBody.targetEtc.trf_k_shoulderL_00)
				{
					return null;
				}
				return chaControl.cmpBoneBody.targetEtc.trf_k_shoulderL_00.gameObject;
			case RefObjKey.k_f_shoulderR_00:
				if (null == chaControl.cmpBoneBody)
				{
					return null;
				}
				if (null == chaControl.cmpBoneBody.targetEtc.trf_k_shoulderR_00)
				{
					return null;
				}
				return chaControl.cmpBoneBody.targetEtc.trf_k_shoulderR_00.gameObject;
			case RefObjKey.k_f_handL_00:
				if (null == chaControl.cmpBoneBody)
				{
					return null;
				}
				if (null == chaControl.cmpBoneBody.targetEtc.trf_k_handL_00)
				{
					return null;
				}
				return chaControl.cmpBoneBody.targetEtc.trf_k_handL_00.gameObject;
			case RefObjKey.k_f_handR_00:
				if (null == chaControl.cmpBoneBody)
				{
					return null;
				}
				if (null == chaControl.cmpBoneBody.targetEtc.trf_k_handR_00)
				{
					return null;
				}
				return chaControl.cmpBoneBody.targetEtc.trf_k_handR_00.gameObject;
			}
		}
		GameObject value = null;
		dictRefObj.TryGetValue((int)key, out value);
		return value;
	}
}
