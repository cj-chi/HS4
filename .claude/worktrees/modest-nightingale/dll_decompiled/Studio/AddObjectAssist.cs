using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AIChara;
using Illusion.Component.Correct.Process;
using IllusionUtility.GetUtility;
using Manager;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

namespace Studio;

public static class AddObjectAssist
{
	public static void InitBone(OCIChar _ociChar, Transform _transformRoot, Dictionary<int, Info.BoneInfo> _dicBoneInfo)
	{
		Dictionary<int, OCIChar.BoneInfo> dictionary = new Dictionary<int, OCIChar.BoneInfo>();
		foreach (KeyValuePair<int, Info.BoneInfo> item in _dicBoneInfo)
		{
			if (_ociChar.sex == 1 && item.Value.level == 2)
			{
				continue;
			}
			GameObject gameObject = null;
			int num = item.Value.group;
			if ((uint)(num - 7) <= 2u)
			{
				gameObject = _ociChar.charReference.GetReferenceInfo(ChaReference.RefObjKey.HeadParent).transform.FindLoop(item.Value.bone)?.gameObject;
			}
			else
			{
				gameObject = _transformRoot.FindLoop(item.Value.bone)?.gameObject;
				_ = gameObject == null;
			}
			if (gameObject == null)
			{
				continue;
			}
			OCIChar.BoneInfo value = null;
			if (dictionary.TryGetValue(item.Value.sync, out value))
			{
				value.AddSyncBone(gameObject);
				continue;
			}
			OIBoneInfo value2 = null;
			if (!_ociChar.oiCharInfo.bones.TryGetValue(item.Key, out value2))
			{
				value2 = new OIBoneInfo(Studio.GetNewIndex());
				_ociChar.oiCharInfo.bones.Add(item.Key, value2);
			}
			switch (item.Value.group)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
				value2.group = (OIBoneInfo.BoneGroup)((1 << item.Value.group) | 1);
				break;
			case 7:
			case 8:
			case 9:
				value2.group = OIBoneInfo.BoneGroup.Hair;
				break;
			case 10:
				value2.group = OIBoneInfo.BoneGroup.Neck;
				break;
			case 11:
			case 12:
				value2.group = OIBoneInfo.BoneGroup.Breast;
				break;
			case 13:
				value2.group = OIBoneInfo.BoneGroup.Skirt;
				break;
			default:
				value2.group = (OIBoneInfo.BoneGroup)(1 << item.Value.group);
				break;
			}
			value2.level = item.Value.level;
			GuideObject guideObject = AddBoneGuide(gameObject.transform, value2.dicKey, _ociChar.guideObject, item.Value.name);
			OIBoneInfo.BoneGroup boneGroup = value2.group;
			if (boneGroup == OIBoneInfo.BoneGroup.RightHand || boneGroup == OIBoneInfo.BoneGroup.LeftHand)
			{
				guideObject.scaleSelect = 0.025f;
			}
			OCIChar.BoneInfo boneInfo = new OCIChar.BoneInfo(guideObject, value2, item.Key);
			_ociChar.listBones.Add(boneInfo);
			guideObject.SetActive(_active: false);
			if (item.Value.no == 65)
			{
				_ociChar.transSon = gameObject.transform;
			}
			if (item.Value.sync != -1)
			{
				dictionary.Add(item.Key, boneInfo);
			}
		}
		_ociChar.UpdateFKColor(FKCtrl.parts);
	}

	private static void TransformLoop(Transform _src, List<Transform> _list)
	{
		if (!(_src == null))
		{
			_list.Add(_src);
			for (int i = 0; i < _src.childCount; i++)
			{
				TransformLoop(_src.GetChild(i), _list);
			}
		}
	}

	public static void ArrangeNames(Transform _target)
	{
		foreach (Transform item in _target)
		{
			ArrangeNamesLoop(item);
		}
	}

	private static void ArrangeNamesLoop(Transform _target)
	{
		string name = _target.name;
		if (Regex.Match(name, "c_J_hairF[CLRU]+[a-b]_(\\d*)", RegexOptions.IgnoreCase).Success)
		{
			_target.name = name.Replace("c_J_hairF", "c_J_hair_F");
		}
		else if (Regex.Match(name, "c_J_hairB[CLRU]+[a-b]_(\\d*)", RegexOptions.IgnoreCase).Success)
		{
			_target.name = name.Replace("c_J_hairB", "c_J_hair_B");
		}
		if (_target.childCount == 0)
		{
			return;
		}
		foreach (Transform item in _target)
		{
			ArrangeNamesLoop(item);
		}
	}

	public static void InitHairBone(OCIChar _ociChar, Dictionary<int, Info.BoneInfo> _dicBoneInfo)
	{
		GameObject referenceInfo = _ociChar.charReference.GetReferenceInfo(ChaReference.RefObjKey.HeadParent);
		Dictionary<int, OCIChar.BoneInfo> dictionary = new Dictionary<int, OCIChar.BoneInfo>();
		foreach (KeyValuePair<int, Info.BoneInfo> item in _dicBoneInfo.Where((KeyValuePair<int, Info.BoneInfo> b) => MathfEx.RangeEqualOn(7, b.Value.group, 9)))
		{
			GameObject gameObject = referenceInfo.transform.FindLoop(item.Value.bone)?.gameObject;
			if (gameObject == null)
			{
				continue;
			}
			OCIChar.BoneInfo value = null;
			if (dictionary.TryGetValue(item.Value.sync, out value))
			{
				value.AddSyncBone(gameObject);
				continue;
			}
			OIBoneInfo value2 = null;
			if (!_ociChar.oiCharInfo.bones.TryGetValue(item.Key, out value2))
			{
				value2 = new OIBoneInfo(Studio.GetNewIndex());
				_ociChar.oiCharInfo.bones.Add(item.Key, value2);
			}
			value2.group = OIBoneInfo.BoneGroup.Hair;
			value2.level = item.Value.level;
			GuideObject guideObject = AddBoneGuide(gameObject.transform, value2.dicKey, _ociChar.guideObject, item.Value.name);
			OCIChar.BoneInfo boneInfo = new OCIChar.BoneInfo(guideObject, value2, item.Key);
			_ociChar.listBones.Add(boneInfo);
			guideObject.SetActive(_active: false);
			if (item.Value.sync != -1)
			{
				dictionary.Add(item.Key, boneInfo);
			}
		}
	}

	private static GuideObject AddBoneGuide(Transform _target, int _dicKey, GuideObject _parent, string _name)
	{
		GuideObject guideObject = Singleton<GuideObjectManager>.Instance.Add(_target, _dicKey);
		guideObject.enablePos = false;
		guideObject.enableScale = false;
		guideObject.enableMaluti = false;
		guideObject.calcScale = false;
		guideObject.scaleRate = 0.5f;
		guideObject.scaleRot = 0.025f;
		guideObject.scaleSelect = 0.05f;
		guideObject.parentGuide = _parent;
		return guideObject;
	}

	public static void InitIKTarget(OCIChar _ociChar, bool _addInfo)
	{
		IKSolverFullBodyBiped solver = _ociChar.finalIK.solver;
		BipedReferences references = _ociChar.finalIK.references;
		_ociChar.ikCtrl = _ociChar.preparation.IKCtrl;
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 0, solver.bodyEffector, _usedRot: false, references.pelvis);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 1, solver.leftShoulderEffector, _usedRot: false, references.leftUpperArm);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 2, solver.leftArmChain, _usedRot: false, references.leftForearm);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 3, solver.leftHandEffector, _usedRot: true, references.leftHand);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 4, solver.rightShoulderEffector, _usedRot: false, references.rightUpperArm);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 5, solver.rightArmChain, _usedRot: false, references.rightForearm);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 6, solver.rightHandEffector, _usedRot: true, references.rightHand);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 7, solver.leftThighEffector, _usedRot: false, references.leftThigh);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 8, solver.leftLegChain, _usedRot: false, references.leftCalf);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 9, solver.leftFootEffector, _usedRot: true, references.leftFoot);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 10, solver.rightThighEffector, _usedRot: false, references.rightThigh);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 11, solver.rightLegChain, _usedRot: false, references.rightCalf);
		AddIKTarget(_ociChar, _ociChar.ikCtrl, 12, solver.rightFootEffector, _usedRot: true, references.rightFoot);
		if (_addInfo)
		{
			_ociChar.ikCtrl.InitTarget();
		}
	}

	public static void InitAccessoryPoint(OCIChar _ociChar)
	{
		Dictionary<int, Tuple<int, int>> dictionary = new Dictionary<int, Tuple<int, int>>();
		ExcelData accessoryPointGroup = Singleton<Info>.Instance.accessoryPointGroup;
		int count = accessoryPointGroup.list.Count;
		Dictionary<int, TreeNodeObject> dictionary2 = new Dictionary<int, TreeNodeObject>();
		for (int i = 1; i < count; i++)
		{
			ExcelData.Param param = accessoryPointGroup.list[i];
			int key = int.Parse(param.list[0]);
			string arg = param.list[1];
			string[] array = param.list[2].Split('-');
			dictionary.Add(key, new Tuple<int, int>(int.Parse(array[0]), int.Parse(array[1])));
			TreeNodeObject treeNodeObject = Studio.AddNode($"グループ : {arg}", _ociChar.treeNodeObject);
			treeNodeObject.treeState = ((!_ociChar.oiCharInfo.dicAccessGroup.ContainsKey(key)) ? TreeNodeObject.TreeState.Close : _ociChar.oiCharInfo.dicAccessGroup[key]);
			treeNodeObject.enableChangeParent = false;
			treeNodeObject.enableDelete = false;
			treeNodeObject.enableCopy = false;
			dictionary2.Add(key, treeNodeObject);
			_ociChar.dicAccessPoint.Add(key, new OCIChar.AccessPointInfo(treeNodeObject));
		}
		foreach (KeyValuePair<int, Tuple<int, int>> item in dictionary)
		{
			for (int j = item.Value.Item1; j <= item.Value.Item2; j++)
			{
				TreeNodeObject parent = dictionary2[item.Key];
				TreeNodeObject treeNodeObject2 = Studio.AddNode($"部位 : {ChaAccessoryDefine.AccessoryParentName.SafeGet(j)}", parent);
				treeNodeObject2.treeState = ((!_ociChar.oiCharInfo.dicAccessNo.ContainsKey(j)) ? TreeNodeObject.TreeState.Close : _ociChar.oiCharInfo.dicAccessNo[j]);
				treeNodeObject2.enableChangeParent = false;
				treeNodeObject2.enableDelete = false;
				treeNodeObject2.enableCopy = false;
				treeNodeObject2.baseColor = Utility.ConvertColor(204, 128, 164);
				treeNodeObject2.colorSelect = treeNodeObject2.baseColor;
				_ociChar.dicAccessoryPoint.Add(treeNodeObject2, j);
				OCIChar.AccessPointInfo value = null;
				if (_ociChar.dicAccessPoint.TryGetValue(item.Key, out value))
				{
					value.child.Add(j, treeNodeObject2);
				}
			}
		}
		foreach (KeyValuePair<int, TreeNodeObject> item2 in dictionary2)
		{
			item2.Value.enableAddChild = false;
		}
		Singleton<Studio>.Instance.treeNodeCtrl.RefreshHierachy();
	}

	public static void LoadChild(List<ObjectInfo> _child, ObjectCtrlInfo _parent, TreeNodeObject _parentNode)
	{
		foreach (ObjectInfo item in _child)
		{
			LoadChild(item, _parent, _parentNode);
		}
	}

	public static void LoadChild(Dictionary<int, ObjectInfo> _child, ObjectCtrlInfo _parent = null, TreeNodeObject _parentNode = null)
	{
		foreach (KeyValuePair<int, ObjectInfo> item in _child)
		{
			LoadChild(item.Value, _parent, _parentNode);
		}
	}

	public static void LoadChild(ObjectInfo _child, ObjectCtrlInfo _parent = null, TreeNodeObject _parentNode = null)
	{
		switch (_child.kind)
		{
		case 0:
		{
			OICharInfo oICharInfo = _child as OICharInfo;
			if (oICharInfo.sex == 1)
			{
				AddObjectFemale.Load(oICharInfo, _parent, _parentNode);
			}
			else
			{
				AddObjectMale.Load(oICharInfo, _parent, _parentNode);
			}
			break;
		}
		case 1:
			AddObjectItem.Load(_child as OIItemInfo, _parent, _parentNode);
			break;
		case 2:
			AddObjectLight.Load(_child as OILightInfo, _parent, _parentNode);
			break;
		case 3:
			AddObjectFolder.Load(_child as OIFolderInfo, _parent, _parentNode);
			break;
		case 4:
			AddObjectRoute.Load(_child as OIRouteInfo, _parent, _parentNode);
			break;
		case 5:
			AddObjectCamera.Load(_child as OICameraInfo, _parent, _parentNode);
			break;
		}
	}

	public static void InitLookAt(OCIChar _ociChar)
	{
		bool num = _ociChar.oiCharInfo.lookAtTarget == null;
		if (num)
		{
			_ociChar.oiCharInfo.lookAtTarget = new LookAtTargetInfo(Studio.GetNewIndex());
		}
		Transform lookAtTarget = _ociChar.preparation.LookAtTarget;
		if (num)
		{
			_ociChar.oiCharInfo.lookAtTarget.changeAmount.pos = lookAtTarget.localPosition;
		}
		GuideObject guideObject = Singleton<GuideObjectManager>.Instance.Add(lookAtTarget, _ociChar.oiCharInfo.lookAtTarget.dicKey);
		guideObject.enableRot = false;
		guideObject.enableScale = false;
		guideObject.enableMaluti = false;
		guideObject.scaleRate = 0.5f;
		guideObject.scaleSelect = 0.25f;
		guideObject.parentGuide = _ociChar.guideObject;
		guideObject.changeAmount.OnChange();
		guideObject.mode = GuideObject.Mode.World;
		guideObject.moveCalc = GuideMove.MoveCalc.TYPE2;
		_ociChar.lookAtInfo = new OCIChar.LookAtInfo(guideObject, _ociChar.oiCharInfo.lookAtTarget);
		_ociChar.lookAtInfo.active = false;
	}

	public static void SetupAccessoryDynamicBones(OCIChar _ociChar)
	{
		ChaControl charInfo = _ociChar.charInfo;
		CmpAccessory[] cmpAccessory = charInfo.cmpAccessory;
		if (((IReadOnlyCollection<CmpAccessory>)(object)cmpAccessory).IsNullOrEmpty())
		{
			return;
		}
		ChaFileAccessory.PartsInfo[] parts = charInfo.nowCoordinate.accessory.parts;
		for (int i = 0; i < cmpAccessory.Length; i++)
		{
			if (!(cmpAccessory[i] == null))
			{
				cmpAccessory[i].EnableDynamicBones(!parts[i].noShake);
			}
		}
	}

	public static void DisableComponent(OCIChar _ociChar)
	{
		BaseProcess[] componentsInChildren = _ociChar.charInfo.objAnim.GetComponentsInChildren<BaseProcess>(includeInactive: true);
		if (!((IReadOnlyCollection<BaseProcess>)(object)componentsInChildren).IsNullOrEmpty())
		{
			BaseProcess[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	public static void UpdateState(OCIChar _ociChar, ChaFileStatus _status)
	{
		ChaFileStatus charFileStatus = _ociChar.charFileStatus;
		charFileStatus.Copy(_status);
		for (int i = 0; i < charFileStatus.clothesState.Length; i++)
		{
			_ociChar.SetClothesState(i, charFileStatus.clothesState[i]);
		}
		for (int j = 0; j < charFileStatus.showAccessory.Length; j++)
		{
			_ociChar.ShowAccessory(j, charFileStatus.showAccessory[j]);
		}
		int[] source = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo((_ociChar.sex == 0) ? ChaListDefine.CategoryNo.custom_eyebrow_m : ChaListDefine.CategoryNo.custom_eyebrow_f).Keys.ToArray();
		_ociChar.charInfo.ChangeEyebrowPtn(source.Contains(charFileStatus.eyebrowPtn) ? charFileStatus.eyebrowPtn : 0);
		int[] source2 = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo((_ociChar.sex == 0) ? ChaListDefine.CategoryNo.custom_eye_m : ChaListDefine.CategoryNo.custom_eye_f).Keys.ToArray();
		_ociChar.charInfo.ChangeEyesPtn(source2.Contains(charFileStatus.eyesPtn) ? charFileStatus.eyesPtn : 0);
		_ociChar.ChangeBlink(charFileStatus.eyesBlink);
		_ociChar.ChangeEyesOpen(charFileStatus.eyesOpenMax);
		_ociChar.ChangeMouthPtn(charFileStatus.mouthPtn);
		_ociChar.ChangeMouthOpen(_ociChar.oiCharInfo.mouthOpen);
		_ociChar.ChangeHandAnime(0, _ociChar.oiCharInfo.handPtn[0]);
		_ociChar.ChangeHandAnime(1, _ociChar.oiCharInfo.handPtn[1]);
		_ociChar.ChangeLookEyesPtn(charFileStatus.eyesLookPtn, _force: true);
		if (_ociChar.oiCharInfo.eyesByteData != null)
		{
			using (MemoryStream input = new MemoryStream(_ociChar.oiCharInfo.eyesByteData))
			{
				using BinaryReader reader = new BinaryReader(input);
				_ociChar.charInfo.eyeLookCtrl.eyeLookScript.LoadAngle(reader);
			}
			_ociChar.oiCharInfo.eyesByteData = null;
		}
		if (_ociChar.oiCharInfo.neckByteData != null)
		{
			using (MemoryStream input2 = new MemoryStream(_ociChar.oiCharInfo.neckByteData))
			{
				using BinaryReader reader2 = new BinaryReader(input2);
				_ociChar.neckLookCtrl.LoadNeckLookCtrl(reader2);
			}
			_ociChar.oiCharInfo.neckByteData = null;
		}
		_ociChar.ChangeLookNeckPtn(charFileStatus.neckLookPtn);
		for (int k = 0; k < 5; k++)
		{
			_ociChar.SetSiruFlags((ChaFileDefine.SiruParts)k, _ociChar.oiCharInfo.siru[k]);
		}
		if (_ociChar.sex == 1)
		{
			_ociChar.charInfo.ChangeHohoAkaRate(charFileStatus.hohoAkaRate);
		}
		_ociChar.SetVisibleSon(charFileStatus.visibleSonAlways);
		_ociChar.SetTears(_ociChar.GetTears());
		_ociChar.SetTuyaRate(_ociChar.charInfo.skinGlossRate);
		_ociChar.SetWetRate(_ociChar.charInfo.wetRate);
	}

	private static OCIChar.IKInfo AddIKTarget(OCIChar _ociChar, IKCtrl _ikCtrl, int _no, IKEffector _effector, bool _usedRot, Transform _bone)
	{
		OCIChar.IKInfo iKInfo = AddIKTarget(_ociChar, _ikCtrl, _no, _effector.target, _usedRot, _bone, _isRed: true);
		_effector.positionWeight = 1f;
		_effector.rotationWeight = (_usedRot ? 1f : 0f);
		_effector.target = iKInfo.targetObject;
		return iKInfo;
	}

	private static OCIChar.IKInfo AddIKTarget(OCIChar _ociChar, IKCtrl _ikCtrl, int _no, FBIKChain _chain, bool _usedRot, Transform _bone)
	{
		OCIChar.IKInfo iKInfo = AddIKTarget(_ociChar, _ikCtrl, _no, _chain.bendConstraint.bendGoal, _usedRot, _bone, _isRed: false);
		_chain.bendConstraint.weight = 1f;
		_chain.bendConstraint.bendGoal = iKInfo.targetObject;
		return iKInfo;
	}

	private static OCIChar.IKInfo AddIKTarget(OCIChar _ociChar, IKCtrl _ikCtrl, int _no, Transform _target, bool _usedRot, Transform _bone, bool _isRed)
	{
		OIIKTargetInfo value = null;
		bool flag = !_ociChar.oiCharInfo.ikTarget.TryGetValue(_no, out value);
		if (flag)
		{
			value = new OIIKTargetInfo(Studio.GetNewIndex());
			_ociChar.oiCharInfo.ikTarget.Add(_no, value);
		}
		switch ((OICharInfo.IKTargetEN)_no)
		{
		case OICharInfo.IKTargetEN.Body:
			value.group = OIBoneInfo.BoneGroup.Body;
			break;
		case OICharInfo.IKTargetEN.LeftShoulder:
		case OICharInfo.IKTargetEN.LeftArmChain:
		case OICharInfo.IKTargetEN.LeftHand:
			value.group = OIBoneInfo.BoneGroup.LeftArm;
			break;
		case OICharInfo.IKTargetEN.RightShoulder:
		case OICharInfo.IKTargetEN.RightArmChain:
		case OICharInfo.IKTargetEN.RightHand:
			value.group = OIBoneInfo.BoneGroup.RightArm;
			break;
		case OICharInfo.IKTargetEN.LeftThigh:
		case OICharInfo.IKTargetEN.LeftLegChain:
		case OICharInfo.IKTargetEN.LeftFoot:
			value.group = OIBoneInfo.BoneGroup.LeftLeg;
			break;
		case OICharInfo.IKTargetEN.RightThigh:
		case OICharInfo.IKTargetEN.RightLegChain:
		case OICharInfo.IKTargetEN.RightFoot:
			value.group = OIBoneInfo.BoneGroup.RightLeg;
			break;
		}
		GameObject gameObject = new GameObject(_target.name + "(work)");
		gameObject.transform.SetParent(_ociChar.charInfo.transform);
		GuideObject guideObject = Singleton<GuideObjectManager>.Instance.Add(gameObject.transform, value.dicKey);
		guideObject.mode = GuideObject.Mode.LocalIK;
		guideObject.enableRot = _usedRot;
		guideObject.enableScale = false;
		guideObject.enableMaluti = false;
		guideObject.calcScale = false;
		guideObject.scaleRate = 0.5f;
		guideObject.scaleRot = 0.05f;
		guideObject.scaleSelect = 0.1f;
		guideObject.parentGuide = _ociChar.guideObject;
		guideObject.guideSelect.color = (_isRed ? Color.red : Color.blue);
		guideObject.moveCalc = GuideMove.MoveCalc.TYPE3;
		OCIChar.IKInfo iKInfo = new OCIChar.IKInfo(guideObject, value, _target, gameObject.transform, _bone);
		if (!flag)
		{
			value.changeAmount.OnChange();
		}
		_ikCtrl.addIKInfo = iKInfo;
		_ociChar.listIKTarget.Add(iKInfo);
		guideObject.SetActive(_active: false);
		return iKInfo;
	}

	public static T LoadAsset<T>(string assetBundleName, string assetName, bool clone = false, string manifestName = "") where T : UnityEngine.Object
	{
		if (File.Exists(AssetBundleManager.BaseDownloadingURL + assetBundleName))
		{
			LoadedAssetBundle loadedAssetBundle = AssetBundleManager.LoadAssetBundle(assetBundleName, manifestName);
			if (loadedAssetBundle != null)
			{
				T val = loadedAssetBundle.Bundle.LoadAsset(assetName) as T;
				if (null != val)
				{
					if (clone)
					{
						T val2 = UnityEngine.Object.Instantiate(val);
						val2.name = val.name;
						val = val2;
					}
					return val;
				}
			}
		}
		return CommonLib.LoadAsset<T>(assetBundleName, assetName, clone, manifestName);
	}
}
