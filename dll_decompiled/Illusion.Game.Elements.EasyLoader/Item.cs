using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using UnityEngine;

namespace Illusion.Game.Elements.EasyLoader;

[Serializable]
public class Item
{
	[Serializable]
	public class Data : AssetBundleData
	{
		public enum Type
		{
			None,
			Head,
			Neck,
			LeftHand,
			RightHand,
			LeftFoot,
			RightFoot,
			a_n_headside,
			k_f_handL_00,
			k_f_handR_00,
			chara,
			k_f_shoulderL_00,
			k_f_shoulderR_00
		}

		public Type type;

		public Vector3 offsetPos = Vector3.zero;

		public Vector3 offsetAngle = Vector3.zero;

		public AssetBundleData motion = new AssetBundleData();

		public string state = "";

		public static Transform GetParent(Type type, ChaControl chaCtrl)
		{
			return type switch
			{
				Type.Head => chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.HeadParent).transform, 
				Type.k_f_handL_00 => chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.k_f_handL_00).transform, 
				Type.k_f_handR_00 => chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.k_f_handR_00).transform, 
				Type.chara => chaCtrl.objTop.transform, 
				Type.k_f_shoulderL_00 => chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.k_f_shoulderL_00).transform, 
				Type.k_f_shoulderR_00 => chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.k_f_shoulderR_00).transform, 
				_ => null, 
			};
		}

		public GameObject Load(ChaControl chaCtrl)
		{
			GameObject gameObject = LoadModel(chaCtrl);
			Animator component = gameObject.GetComponent<Animator>();
			if (component != null)
			{
				RuntimeAnimatorController runtimeAnimatorController = motion.GetAsset<RuntimeAnimatorController>();
				if (runtimeAnimatorController != null)
				{
					component.runtimeAnimatorController = runtimeAnimatorController;
				}
			}
			return gameObject;
		}

		private GameObject LoadModel(ChaControl chaCtrl)
		{
			Transform transform = GetParent(type, chaCtrl);
			if (transform == null)
			{
				transform = chaCtrl.transform.root;
			}
			GameObject gameObject = GetAsset<GameObject>();
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, transform, worldPositionStays: false);
			gameObject2.transform.localPosition += offsetPos;
			gameObject2.transform.localEulerAngles += offsetAngle;
			gameObject2.name = gameObject.name;
			UnloadBundle();
			return gameObject2;
		}
	}

	public Data[] data = new Data[0];

	[SerializeField]
	private List<GameObject> _itemObjectList = new List<GameObject>();

	public List<GameObject> itemObjectList => _itemObjectList;

	public void Visible(bool visible)
	{
		_itemObjectList.ForEach(delegate(GameObject item)
		{
			item.SetActive(visible);
		});
	}

	public void Setting(ChaControl chaCtrl, bool isItemClear = true)
	{
		if (isItemClear)
		{
			_itemObjectList.ForEach(delegate(GameObject item)
			{
				UnityEngine.Object.Destroy(item);
			});
			_itemObjectList.Clear();
		}
		_itemObjectList.AddRange(data.Select((Data item) => item.Load(chaCtrl)));
	}
}
