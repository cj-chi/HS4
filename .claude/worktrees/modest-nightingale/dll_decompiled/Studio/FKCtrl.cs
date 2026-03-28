using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using IllusionUtility.GetUtility;
using UniRx;
using UnityEngine;

namespace Studio;

public class FKCtrl : MonoBehaviour
{
	private class TargetInfo
	{
		public GameObject gameObject;

		private Transform m_Transform;

		public ChangeAmount changeAmount;

		private BoolReactiveProperty _enable = new BoolReactiveProperty(initialValue: true);

		private OCIChar.SyncBoneInfo syncBoneInfo;

		public Transform transform
		{
			get
			{
				if (m_Transform == null)
				{
					m_Transform = gameObject.transform;
				}
				return m_Transform;
			}
		}

		public OIBoneInfo.BoneGroup group { get; private set; }

		public int level { get; private set; }

		public bool boneWeight { get; set; }

		public int boneID { get; private set; }

		public bool enable
		{
			get
			{
				return _enable.Value;
			}
			set
			{
				_enable.Value = value;
			}
		}

		public TargetInfo(GameObject _gameObject, ChangeAmount _changeAmount, OIBoneInfo.BoneGroup _group, int _level, bool _boneWeight, int _boneID)
		{
			gameObject = _gameObject;
			changeAmount = _changeAmount;
			group = _group;
			level = _level;
			boneWeight = _boneWeight;
			boneID = _boneID;
			if ((group & OIBoneInfo.BoneGroup.Hair) == 0 && (group & OIBoneInfo.BoneGroup.Skirt) == 0 && (group & OIBoneInfo.BoneGroup.Body) == 0)
			{
				return;
			}
			_enable.Subscribe(delegate(bool _b)
			{
				if (!_b)
				{
					transform.localRotation = Quaternion.identity;
					syncBoneInfo.SafeProc(delegate(OCIChar.SyncBoneInfo _sbi)
					{
						_sbi.LocalRotation = Quaternion.identity;
					});
				}
			});
		}

		public void CopyBone()
		{
			changeAmount.rot = transform.localEulerAngles;
		}

		public void AddSyncBone(GameObject _gameObject)
		{
			syncBoneInfo = new OCIChar.SyncBoneInfo(_gameObject);
		}

		public void Update()
		{
			if (enable & boneWeight)
			{
				transform.localRotation = Quaternion.Euler(changeAmount.rot);
				syncBoneInfo.SafeProc(delegate(OCIChar.SyncBoneInfo _sbi)
				{
					_sbi.LocalRotation = transform.localRotation;
				});
			}
		}
	}

	public static OIBoneInfo.BoneGroup[] parts = new OIBoneInfo.BoneGroup[7]
	{
		OIBoneInfo.BoneGroup.Hair,
		OIBoneInfo.BoneGroup.Neck,
		OIBoneInfo.BoneGroup.Breast,
		OIBoneInfo.BoneGroup.Body,
		OIBoneInfo.BoneGroup.RightHand,
		OIBoneInfo.BoneGroup.LeftHand,
		OIBoneInfo.BoneGroup.Skirt
	};

	private Transform m_Transform;

	private List<TargetInfo> listBones = new List<TargetInfo>();

	private new Transform transform
	{
		get
		{
			if (m_Transform == null)
			{
				m_Transform = base.transform;
			}
			return m_Transform;
		}
	}

	private int count { get; set; }

	public void InitBones(OCIChar _ociChar, OICharInfo _info, ChaControl _chaControl, ChaReference _charReference)
	{
		if (_info == null)
		{
			return;
		}
		listBones.Clear();
		Dictionary<int, TargetInfo> dictionary = new Dictionary<int, TargetInfo>();
		foreach (KeyValuePair<int, Info.BoneInfo> v in Singleton<Info>.Instance.dicBoneInfo)
		{
			GameObject gameObject = null;
			bool boneWeight = true;
			int num = v.Value.group;
			if ((uint)(num - 7) <= 2u)
			{
				gameObject = _charReference.GetReferenceInfo(ChaReference.RefObjKey.HeadParent).transform.FindLoop(v.Value.bone)?.gameObject;
			}
			else
			{
				gameObject = transform.FindLoop(v.Value.bone)?.gameObject;
				_ = gameObject == null;
			}
			if (gameObject == null)
			{
				continue;
			}
			TargetInfo value = null;
			if (dictionary.TryGetValue(v.Value.sync, out value))
			{
				value.AddSyncBone(gameObject);
				continue;
			}
			OIBoneInfo value2 = null;
			if (!_info.bones.TryGetValue(v.Key, out value2))
			{
				continue;
			}
			OIBoneInfo.BoneGroup boneGroup = OIBoneInfo.BoneGroup.Body;
			switch (v.Value.group)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
				boneGroup = OIBoneInfo.BoneGroup.Body;
				break;
			case 7:
			case 8:
			case 9:
				boneGroup = OIBoneInfo.BoneGroup.Hair;
				break;
			case 10:
				boneGroup = OIBoneInfo.BoneGroup.Neck;
				break;
			case 11:
			case 12:
				boneGroup = OIBoneInfo.BoneGroup.Breast;
				break;
			case 13:
				boneWeight = false;
				boneWeight |= UsedBone(_chaControl.GetCustomClothesComponent(0), gameObject.transform);
				boneWeight |= UsedBone(_chaControl.GetCustomClothesComponent(1), gameObject.transform);
				boneGroup = OIBoneInfo.BoneGroup.Skirt;
				_ociChar.listBones.Find((OCIChar.BoneInfo _v) => _v.boneID == v.Key).SafeProc(delegate(OCIChar.BoneInfo _v)
				{
					_v.boneWeight = boneWeight;
				});
				break;
			default:
				boneGroup = (OIBoneInfo.BoneGroup)(1 << v.Value.group);
				break;
			}
			TargetInfo targetInfo = new TargetInfo(gameObject, value2.changeAmount, boneGroup, v.Value.level, boneWeight, v.Key);
			listBones.Add(targetInfo);
			if (v.Value.sync != -1)
			{
				dictionary.Add(v.Key, targetInfo);
			}
		}
		count = listBones.Count;
	}

	public void CopyBone()
	{
		foreach (TargetInfo listBone in listBones)
		{
			listBone.CopyBone();
		}
	}

	public void CopyBone(OIBoneInfo.BoneGroup _target)
	{
		foreach (TargetInfo item in listBones.Where((TargetInfo l) => (l.group & _target) != 0))
		{
			item.CopyBone();
		}
	}

	public void SetEnable(OIBoneInfo.BoneGroup _group, bool _enable)
	{
		foreach (TargetInfo item in listBones.Where((TargetInfo l) => (l.group & _group) != 0))
		{
			item.enable = _enable;
		}
	}

	public void ResetUsedBone(OCIChar _ociChar)
	{
		ChaControl charInfo = _ociChar.charInfo;
		foreach (TargetInfo v in listBones.Where((TargetInfo _v) => _v.group == OIBoneInfo.BoneGroup.Skirt))
		{
			bool boneWeight = false;
			boneWeight |= UsedBone(charInfo.GetCustomClothesComponent(0), v.transform);
			boneWeight |= UsedBone(charInfo.GetCustomClothesComponent(1), v.transform);
			_ociChar.listBones.Find((OCIChar.BoneInfo _v) => _v.boneID == v.boneID).SafeProc(delegate(OCIChar.BoneInfo _v)
			{
				_v.boneWeight = boneWeight;
			});
			v.boneWeight = boneWeight;
		}
	}

	private bool UsedBone(CmpClothes _cmpClothes, Transform _transform)
	{
		bool flag = false;
		if (_cmpClothes == null)
		{
			return false;
		}
		flag |= UsedBone(_cmpClothes.rendNormal01, _transform);
		flag |= UsedBone(_cmpClothes.rendNormal02, _transform);
		return flag | UsedBone(_cmpClothes.rendNormal03, _transform);
	}

	private bool UsedBone(Renderer[] _renderers, Transform _transform)
	{
		if (((IReadOnlyCollection<Renderer>)(object)_renderers).IsNullOrEmpty())
		{
			return false;
		}
		foreach (Renderer renderer in _renderers)
		{
			if (UsedBone(renderer as SkinnedMeshRenderer, _transform))
			{
				return true;
			}
		}
		return false;
	}

	private bool UsedBone(SkinnedMeshRenderer _renderer, Transform _transform)
	{
		if (_renderer == null)
		{
			return false;
		}
		int idx = Array.FindIndex(_renderer.bones, (Transform _v) => _v == _transform);
		if (idx >= 0)
		{
			BoneWeight[] boneWeights = _renderer.sharedMesh.boneWeights;
			return boneWeights.Where((BoneWeight v) => v.boneIndex0 == idx).Any((BoneWeight v) => v.weight0 != 0f) | boneWeights.Where((BoneWeight v) => v.boneIndex1 == idx).Any((BoneWeight v) => v.weight1 != 0f) | boneWeights.Where((BoneWeight v) => v.boneIndex2 == idx).Any((BoneWeight v) => v.weight2 != 0f) | boneWeights.Where((BoneWeight v) => v.boneIndex3 == idx).Any((BoneWeight v) => v.weight3 != 0f);
		}
		return false;
	}

	private void LateUpdate()
	{
		for (int i = 0; i < count; i++)
		{
			listBones[i].Update();
		}
	}
}
