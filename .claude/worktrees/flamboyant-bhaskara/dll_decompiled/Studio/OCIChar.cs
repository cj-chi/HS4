using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Illusion;
using Illusion.Extensions;
using Manager;
using RootMotion.FinalIK;
using UnityEngine;

namespace Studio;

public class OCIChar : ObjectCtrlInfo
{
	public class SyncBoneInfo
	{
		private Transform _transform;

		public GameObject GameObject { get; private set; }

		private Transform Transform => _transform ?? (_transform = GameObject.transform);

		public Quaternion Rotation
		{
			set
			{
				Transform.rotation = value;
			}
		}

		public Quaternion LocalRotation
		{
			set
			{
				Transform.localRotation = value;
			}
		}

		public SyncBoneInfo(GameObject _gameObject)
		{
			GameObject = _gameObject;
		}
	}

	public class BoneInfo
	{
		private GameObject m_GameObject;

		private SyncBoneInfo syncBoneInfo;

		public GuideObject guideObject { get; private set; }

		public OIBoneInfo boneInfo { get; private set; }

		public GameObject gameObject => m_GameObject ?? (m_GameObject = guideObject.gameObject);

		public bool active
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeSelf;
			}
			set
			{
				gameObject?.SetActiveIfDifferent(value);
			}
		}

		public OIBoneInfo.BoneGroup boneGroup => boneInfo.group;

		public float scaleRate
		{
			get
			{
				return guideObject.scaleRate;
			}
			set
			{
				guideObject.scaleRate = value;
			}
		}

		public int layer
		{
			set
			{
				guideObject.SetLayer(gameObject, value);
			}
		}

		public Color color
		{
			set
			{
				guideObject.guideSelect.color = value;
			}
		}

		public int boneID { get; private set; }

		public bool boneWeight { get; set; }

		public Vector3 posision => guideObject.transformTarget.position;

		public BoneInfo(GuideObject _guideObject, OIBoneInfo _boneInfo, int _boneID)
		{
			guideObject = _guideObject;
			boneInfo = _boneInfo;
			boneID = _boneID;
			boneWeight = true;
		}

		public void AddSyncBone(GameObject _gameObject)
		{
			syncBoneInfo = new SyncBoneInfo(_gameObject);
			ChangeAmount changeAmount = guideObject.changeAmount;
			changeAmount.onChangeRot = (Action)Delegate.Combine(changeAmount.onChangeRot, (Action)delegate
			{
				syncBoneInfo.LocalRotation = gameObject.transform.localRotation;
			});
		}
	}

	public class IKInfo
	{
		private GameObject m_GameObject;

		public GuideObject guideObject { get; private set; }

		public OIIKTargetInfo targetInfo { get; private set; }

		public Transform baseObject { get; private set; }

		public Transform targetObject { get; private set; }

		public Transform boneObject { get; private set; }

		public GameObject gameObject => m_GameObject ?? (m_GameObject = guideObject.gameObject);

		public bool active
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeSelf;
			}
			set
			{
				gameObject?.SetActiveIfDifferent(value);
			}
		}

		public OIBoneInfo.BoneGroup boneGroup => targetInfo.group;

		public float scaleRate
		{
			get
			{
				return guideObject.scaleRate;
			}
			set
			{
				guideObject.scaleRate = value;
			}
		}

		public int layer
		{
			set
			{
				guideObject.SetLayer(gameObject, value);
			}
		}

		public IKInfo(GuideObject _guideObject, OIIKTargetInfo _targetInfo, Transform _base, Transform _target, Transform _bone)
		{
			guideObject = _guideObject;
			targetInfo = _targetInfo;
			baseObject = _base;
			targetObject = _target;
			boneObject = _bone;
		}

		public void CopyBaseValue()
		{
			targetObject.position = baseObject.position;
			targetObject.eulerAngles = baseObject.eulerAngles;
			guideObject.changeAmount.pos = targetObject.localPosition;
			guideObject.changeAmount.rot = (guideObject.enableRot ? targetObject.localEulerAngles : Vector3.zero);
		}

		public void CopyBone()
		{
			targetObject.position = boneObject.position;
			targetObject.eulerAngles = boneObject.eulerAngles;
			guideObject.changeAmount.pos = targetObject.localPosition;
			guideObject.changeAmount.rot = (guideObject.enableRot ? targetObject.localEulerAngles : Vector3.zero);
		}

		public void CopyBoneRotation()
		{
			targetObject.eulerAngles = boneObject.eulerAngles;
			guideObject.changeAmount.rot = (guideObject.enableRot ? targetObject.localEulerAngles : Vector3.zero);
		}
	}

	public class LookAtInfo
	{
		private GameObject m_GameObject;

		public GuideObject guideObject { get; private set; }

		public LookAtTargetInfo targetInfo { get; private set; }

		public GameObject gameObject => m_GameObject ?? (m_GameObject = guideObject.gameObject);

		public Transform target => guideObject.transformTarget;

		public bool active
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeSelf;
			}
			set
			{
				gameObject?.SetActiveIfDifferent(value);
			}
		}

		public int layer
		{
			set
			{
				guideObject.SetLayer(gameObject, value);
			}
		}

		public LookAtInfo(GuideObject _guideObject, LookAtTargetInfo _targetInfo)
		{
			guideObject = _guideObject;
			targetInfo = _targetInfo;
		}
	}

	public class LoadedAnimeInfo
	{
		public Info.FileInfo baseFile = new Info.FileInfo();

		public Info.FileInfo overrideFile = new Info.FileInfo();

		public bool BaseCheck(string _bundle, string _file)
		{
			return (baseFile.bundlePath != _bundle) | (baseFile.fileName != _file);
		}

		public bool OverrideCheck(string _bundle, string _file)
		{
			return (overrideFile.bundlePath != _bundle) | (overrideFile.fileName != _file);
		}
	}

	public class AccessPointInfo
	{
		public TreeNodeObject root;

		public Dictionary<int, TreeNodeObject> child;

		public AccessPointInfo(TreeNodeObject _root)
		{
			root = _root;
			child = new Dictionary<int, TreeNodeObject>();
		}
	}

	public ChaReference charReference;

	public Dictionary<int, AccessPointInfo> dicAccessPoint = new Dictionary<int, AccessPointInfo>();

	public List<BoneInfo> listBones = new List<BoneInfo>();

	public List<IKInfo> listIKTarget = new List<IKInfo>();

	public LookAtInfo lookAtInfo;

	public ChaControl charInfo;

	public FKCtrl fkCtrl;

	public IKCtrl ikCtrl;

	public FullBodyBipedIK finalIK;

	public NeckLookControllerVer2 neckLookCtrl;

	public DynamicBone[] skirtDynamic;

	private bool[] dynamicBust = new bool[4] { true, true, true, true };

	private bool[] enablePV = new bool[8] { true, true, true, true, true, true, true, true };

	public OptionItemCtrl optionItemCtrl;

	public bool isAnimeMotion;

	public bool isHAnime;

	public CharAnimeCtrl charAnimeCtrl;

	public YureCtrl yureCtrl;

	public string[] animeParam = new string[2] { "height", "Breast" };

	public Dictionary<TreeNodeObject, int> dicAccessoryPoint = new Dictionary<TreeNodeObject, int>();

	private LoadedAnimeInfo _loadedAnimeInfo = new LoadedAnimeInfo();

	public OICharInfo oiCharInfo => objectInfo as OICharInfo;

	public Transform transSon
	{
		get
		{
			if (!charAnimeCtrl)
			{
				return null;
			}
			return charAnimeCtrl.transSon;
		}
		set
		{
			if ((bool)charAnimeCtrl)
			{
				charAnimeCtrl.transSon = value;
			}
		}
	}

	public ChaFileStatus charFileStatus => charInfo.fileStatus;

	public int sex => charInfo.fileParam.sex;

	public int HandAnimeNum => charInfo.GetShapeIndexHandCount();

	public bool DynamicAnimeBustL
	{
		get
		{
			return dynamicBust[0];
		}
		set
		{
			dynamicBust[0] = value;
			UpdateDynamicBonesBust(0);
		}
	}

	public bool DynamicAnimeBustR
	{
		get
		{
			return dynamicBust[1];
		}
		set
		{
			dynamicBust[1] = value;
			UpdateDynamicBonesBust(1);
		}
	}

	public bool DynamicFKBustL
	{
		get
		{
			return dynamicBust[2];
		}
		set
		{
			dynamicBust[2] = value;
			UpdateDynamicBonesBust(0);
		}
	}

	public bool DynamicFKBustR
	{
		get
		{
			return dynamicBust[3];
		}
		set
		{
			dynamicBust[3] = value;
			UpdateDynamicBonesBust(1);
		}
	}

	public VoiceCtrl voiceCtrl => oiCharInfo.voiceCtrl;

	public VoiceCtrl.Repeat voiceRepeat
	{
		get
		{
			return voiceCtrl.repeat;
		}
		set
		{
			voiceCtrl.repeat = value;
		}
	}

	private int neckPtnOld { get; set; }

	protected int breastLayer { get; set; }

	protected LoadedAnimeInfo loadedAnimeInfo => _loadedAnimeInfo;

	public Preparation preparation { get; set; }

	public override float animeSpeed
	{
		get
		{
			return oiCharInfo.animeSpeed;
		}
		set
		{
			oiCharInfo.animeSpeed = value;
			if ((bool)charInfo.animBody)
			{
				charInfo.animBody.speed = value;
			}
		}
	}

	public float animePattern
	{
		get
		{
			return oiCharInfo.animePattern;
		}
		set
		{
			oiCharInfo.animePattern = value;
			if (isAnimeMotion)
			{
				charInfo.setAnimatorParamFloat("motion", oiCharInfo.animePattern);
			}
			if ((bool)optionItemCtrl)
			{
				optionItemCtrl.SetMotion(oiCharInfo.animePattern);
			}
		}
	}

	public float[] animeOptionParam => oiCharInfo.animeOptionParam;

	public float animeOptionParam1
	{
		get
		{
			return oiCharInfo.animeOptionParam[0];
		}
		set
		{
			oiCharInfo.animeOptionParam[0] = value;
			if (isHAnime && !animeParam[0].IsNullOrEmpty())
			{
				charInfo.setAnimatorParamFloat(animeParam[0], value);
			}
		}
	}

	public float animeOptionParam2
	{
		get
		{
			return oiCharInfo.animeOptionParam[1];
		}
		set
		{
			oiCharInfo.animeOptionParam[1] = value;
			if (isHAnime && !animeParam[1].IsNullOrEmpty())
			{
				charInfo.setAnimatorParamFloat(animeParam[1], value);
			}
		}
	}

	public override void OnDelete()
	{
		Singleton<GuideObjectManager>.Instance.Delete(guideObject);
		voiceCtrl.Stop();
		for (int i = 0; i < listBones.Count; i++)
		{
			Singleton<GuideObjectManager>.Instance.Delete(listBones[i].guideObject);
		}
		for (int j = 0; j < listIKTarget.Count; j++)
		{
			Singleton<GuideObjectManager>.Instance.Delete(listIKTarget[j].guideObject);
		}
		Singleton<GuideObjectManager>.Instance.Delete(lookAtInfo.guideObject);
		if (parentInfo != null)
		{
			parentInfo.OnDetachChild(this);
		}
		Studio.DeleteInfo(objectInfo);
	}

	public override void OnAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
		int value = -1;
		if (dicAccessoryPoint.TryGetValue(_parent, out value))
		{
			if (_child.parentInfo == null)
			{
				Studio.DeleteInfo(_child.objectInfo, _delKey: false);
			}
			else
			{
				_child.parentInfo.OnDetachChild(_child);
			}
			if (!oiCharInfo.child[value].Contains(_child.objectInfo))
			{
				oiCharInfo.child[value].Add(_child.objectInfo);
			}
			bool flag = false;
			if (_child is OCIItem)
			{
				flag = (_child as OCIItem).IsParticleArray;
			}
			Transform accessoryParentTransform = charInfo.GetAccessoryParentTransform(value);
			if (!flag)
			{
				_child.guideObject.transformTarget.SetParent(accessoryParentTransform);
			}
			_child.guideObject.parent = accessoryParentTransform;
			_child.guideObject.mode = GuideObject.Mode.World;
			_child.guideObject.moveCalc = GuideMove.MoveCalc.TYPE2;
			if (!flag)
			{
				_child.objectInfo.changeAmount.pos = _child.guideObject.transformTarget.localPosition;
				_child.objectInfo.changeAmount.rot = _child.guideObject.transformTarget.localEulerAngles;
			}
			else if (_child.guideObject.nonconnect)
			{
				_child.objectInfo.changeAmount.pos = _child.guideObject.parent.InverseTransformPoint(_child.guideObject.transformTarget.position);
				Quaternion quaternion = _child.guideObject.transformTarget.rotation * Quaternion.Inverse(_child.guideObject.parent.rotation);
				_child.objectInfo.changeAmount.rot = quaternion.eulerAngles;
			}
			else
			{
				_child.objectInfo.changeAmount.pos = _child.guideObject.parent.InverseTransformPoint(_child.objectInfo.changeAmount.pos);
				Quaternion quaternion2 = Quaternion.Euler(_child.objectInfo.changeAmount.rot) * Quaternion.Inverse(_child.guideObject.parent.rotation);
				_child.objectInfo.changeAmount.rot = quaternion2.eulerAngles;
			}
			_child.guideObject.nonconnect = flag;
			_child.guideObject.calcScale = !flag;
			_child.parentInfo = this;
		}
	}

	public override void OnLoadAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
		int value = -1;
		if (dicAccessoryPoint.TryGetValue(_parent, out value))
		{
			if (_child.parentInfo == null)
			{
				Studio.DeleteInfo(_child.objectInfo, _delKey: false);
			}
			else
			{
				_child.parentInfo.OnDetachChild(_child);
			}
			if (!oiCharInfo.child[value].Contains(_child.objectInfo))
			{
				oiCharInfo.child[value].Add(_child.objectInfo);
			}
			bool flag = false;
			if (_child is OCIItem)
			{
				flag = (_child as OCIItem).IsParticleArray;
			}
			Transform accessoryParentTransform = charInfo.GetAccessoryParentTransform(value);
			if (!flag)
			{
				_child.guideObject.transformTarget.SetParent(accessoryParentTransform);
			}
			_child.guideObject.parent = accessoryParentTransform;
			_child.guideObject.nonconnect = flag;
			_child.guideObject.calcScale = !flag;
			_child.guideObject.mode = GuideObject.Mode.World;
			_child.guideObject.moveCalc = GuideMove.MoveCalc.TYPE2;
			_child.guideObject.changeAmount.OnChange();
			_child.parentInfo = this;
		}
	}

	public override void OnDetach()
	{
		parentInfo.OnDetachChild(this);
		guideObject.parent = null;
		Studio.AddInfo(objectInfo, this);
		guideObject.transformTarget.SetParent(Scene.commonSpace.transform);
		objectInfo.changeAmount.pos = guideObject.transformTarget.localPosition;
		objectInfo.changeAmount.rot = guideObject.transformTarget.localEulerAngles;
		guideObject.mode = GuideObject.Mode.Local;
		guideObject.moveCalc = GuideMove.MoveCalc.TYPE1;
		treeNodeObject.ResetVisible();
	}

	public override void OnSelect(bool _select)
	{
		int layer = LayerMask.NameToLayer(_select ? "Studio/Col" : "Studio/Select");
		lookAtInfo.layer = layer;
		for (int i = 0; i < listBones.Count; i++)
		{
			listBones[i].layer = layer;
		}
		for (int j = 0; j < listIKTarget.Count; j++)
		{
			listIKTarget[j].layer = layer;
		}
	}

	public override void OnDetachChild(ObjectCtrlInfo _child)
	{
		using (Dictionary<int, List<ObjectInfo>>.Enumerator enumerator = oiCharInfo.child.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(_child.objectInfo))
			{
			}
		}
		_child.parentInfo = null;
	}

	public override void OnSavePreprocessing()
	{
		base.OnSavePreprocessing();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using BinaryWriter writer = new BinaryWriter(memoryStream);
			neckLookCtrl.SaveNeckLookCtrl(writer);
			oiCharInfo.neckByteData = memoryStream.ToArray();
		}
		using (MemoryStream memoryStream2 = new MemoryStream())
		{
			using BinaryWriter writer2 = new BinaryWriter(memoryStream2);
			charInfo.eyeLookCtrl.eyeLookScript.SaveAngle(writer2);
			oiCharInfo.eyesByteData = memoryStream2.ToArray();
		}
		AnimatorStateInfo currentAnimatorStateInfo = charInfo.animBody.GetCurrentAnimatorStateInfo(0);
		oiCharInfo.animeNormalizedTime = currentAnimatorStateInfo.normalizedTime;
		oiCharInfo.dicAccessGroup = new Dictionary<int, TreeNodeObject.TreeState>();
		oiCharInfo.dicAccessNo = new Dictionary<int, TreeNodeObject.TreeState>();
		foreach (KeyValuePair<int, AccessPointInfo> item in dicAccessPoint)
		{
			oiCharInfo.dicAccessGroup.Add(item.Key, item.Value.root.treeState);
			foreach (KeyValuePair<int, TreeNodeObject> item2 in item.Value.child)
			{
				oiCharInfo.dicAccessNo.Add(item2.Key, item2.Value.treeState);
			}
		}
	}

	public override void OnVisible(bool _visible)
	{
		charInfo.visibleAll = _visible;
		if ((bool)optionItemCtrl)
		{
			optionItemCtrl.outsideVisible = _visible;
		}
		foreach (BoneInfo listBone in listBones)
		{
			listBone.guideObject.visibleOutside = _visible;
		}
		foreach (IKInfo item in listIKTarget)
		{
			item.guideObject.visibleOutside = _visible;
		}
		if (lookAtInfo != null && (bool)lookAtInfo.guideObject)
		{
			lookAtInfo.guideObject.visibleOutside = _visible;
		}
	}

	public void InitKinematic(GameObject _target, FullBodyBipedIK _finalIK, NeckLookControllerVer2 _neckLook, DynamicBone[] _hairDynamic, DynamicBone[] _skirtDynamic)
	{
		neckLookCtrl = _neckLook;
		neckPtnOld = charFileStatus.neckLookPtn;
		skirtDynamic = _skirtDynamic;
		InitFK(_target);
		for (int i = 0; i < listIKTarget.Count; i++)
		{
			listIKTarget[i].active = false;
		}
		finalIK = _finalIK;
		finalIK.enabled = false;
	}

	public void InitFK(GameObject _target)
	{
		if (fkCtrl == null && _target != null)
		{
			fkCtrl = _target.AddComponent<FKCtrl>();
		}
		fkCtrl.InitBones(this, oiCharInfo, charInfo, charReference);
		fkCtrl.enabled = false;
		for (int i = 0; i < listBones.Count; i++)
		{
			listBones[i].active = false;
		}
	}

	public void ActiveKinematicMode(OICharInfo.KinematicMode _mode, bool _active, bool _force)
	{
		switch (_mode)
		{
		case OICharInfo.KinematicMode.IK:
			if (_force || finalIK.enabled != _active)
			{
				finalIK.enabled = _active;
				oiCharInfo.enableIK = _active;
				for (int j = 0; j < 5; j++)
				{
					ActiveIK((OIBoneInfo.BoneGroup)(1 << j), _active && oiCharInfo.activeIK[j], _force: true);
				}
				if (oiCharInfo.enableIK)
				{
					ActiveKinematicMode(OICharInfo.KinematicMode.FK, _active: false, _force);
				}
			}
			break;
		case OICharInfo.KinematicMode.FK:
			if (_force || fkCtrl.enabled != _active)
			{
				fkCtrl.enabled = _active;
				oiCharInfo.enableFK = _active;
				OIBoneInfo.BoneGroup[] parts = FKCtrl.parts;
				for (int i = 0; i < parts.Length; i++)
				{
					ActiveFK(parts[i], _active && oiCharInfo.activeFK[i], _force: true);
				}
				if (oiCharInfo.enableFK)
				{
					ActiveKinematicMode(OICharInfo.KinematicMode.IK, _active: false, _force);
				}
			}
			break;
		}
		for (int k = 0; k < 4; k++)
		{
			preparation.PvCopy[k] = !oiCharInfo.enableFK && enablePV[k];
		}
	}

	public void ActiveFK(OIBoneInfo.BoneGroup _group, bool _active, bool _force = false)
	{
		OIBoneInfo.BoneGroup[] parts = FKCtrl.parts;
		int i = 0;
		while (i < parts.Length)
		{
			if ((_group & parts[i]) != 0 && (_force || (Utility.SetStruct(ref oiCharInfo.activeFK[i], _active) && oiCharInfo.enableFK)))
			{
				ActiveFKGroup(parts[i], _active);
				foreach (BoneInfo item in listBones.Where((BoneInfo v) => ((v.boneGroup & parts[i]) != 0) & v.boneWeight))
				{
					item.active = ((!_force) ? (oiCharInfo.enableFK & oiCharInfo.activeFK[i]) : _active);
				}
			}
			int num = i + 1;
			i = num;
		}
	}

	public bool IsFKGroup(OIBoneInfo.BoneGroup _group)
	{
		return listBones.Any((BoneInfo v) => (v.boneGroup & _group) != 0);
	}

	public void InitFKBone(OIBoneInfo.BoneGroup _group)
	{
		foreach (BoneInfo item in listBones.Where((BoneInfo v) => (v.boneGroup & _group) != 0))
		{
			item.boneInfo.changeAmount.Reset();
		}
	}

	private void ActiveFKGroup(OIBoneInfo.BoneGroup _group, bool _active)
	{
		switch (_group)
		{
		case OIBoneInfo.BoneGroup.Neck:
			if (_active)
			{
				neckPtnOld = charFileStatus.neckLookPtn;
				ChangeLookNeckPtn(4);
			}
			else
			{
				ChangeLookNeckPtn(neckPtnOld);
			}
			break;
		case OIBoneInfo.BoneGroup.Breast:
			DynamicFKBustL = !_active;
			DynamicFKBustR = !_active;
			break;
		}
		fkCtrl.SetEnable(_group, _active);
		switch (_group)
		{
		case OIBoneInfo.BoneGroup.Hair:
		{
			ChaFileHair.PartsInfo[] parts = charInfo.fileCustom.hair.parts;
			for (int j = 0; j < parts.Length; j++)
			{
				CmpHair cmpHair = charInfo.cmpHair.SafeGet(j);
				if (cmpHair == null)
				{
					continue;
				}
				foreach (KeyValuePair<int, ChaFileHair.PartsInfo.BundleInfo> v in parts[j].dictBundle)
				{
					cmpHair.boneInfo.SafeProc(v.Key, delegate(CmpHair.BoneInfo _info)
					{
						if (!((IReadOnlyCollection<DynamicBone>)(object)_info.dynamicBone).IsNullOrEmpty())
						{
							foreach (DynamicBone item in _info.dynamicBone.Where((DynamicBone _v) => _v != null))
							{
								item.enabled = !_active & !v.Value.noShake;
							}
						}
					});
				}
			}
			break;
		}
		case OIBoneInfo.BoneGroup.Skirt:
			if (!((IReadOnlyCollection<DynamicBone>)(object)skirtDynamic).IsNullOrEmpty())
			{
				for (int i = 0; i < skirtDynamic.Length; i++)
				{
					skirtDynamic[i].enabled = !_active;
				}
			}
			break;
		}
	}

	public void ActiveIK(OIBoneInfo.BoneGroup _group, bool _active, bool _force = false)
	{
		for (int i = 0; i < 5; i++)
		{
			OIBoneInfo.BoneGroup target = (OIBoneInfo.BoneGroup)(1 << i);
			if ((_group & target) == 0 || (!_force && !Utility.SetStruct(ref oiCharInfo.activeIK[i], _active)))
			{
				continue;
			}
			ActiveIKGroup(target, _active);
			foreach (IKInfo item in listIKTarget.Where((IKInfo v) => (v.boneGroup & target) != 0))
			{
				item.active = ((!_force) ? (oiCharInfo.enableIK & oiCharInfo.activeIK[i]) : _active);
			}
		}
	}

	private void ActiveIKGroup(OIBoneInfo.BoneGroup _group, bool _active)
	{
		IKSolverFullBodyBiped solver = finalIK.solver;
		float num = (_active ? 1f : 0f);
		switch (_group)
		{
		case OIBoneInfo.BoneGroup.Body:
			solver.spineMapping.twistWeight = num;
			solver.SetEffectorWeights(FullBodyBipedEffector.Body, num, num);
			break;
		case OIBoneInfo.BoneGroup.RightArm:
			solver.rightArmMapping.weight = num;
			solver.SetEffectorWeights(FullBodyBipedEffector.RightShoulder, num, num);
			solver.SetEffectorWeights(FullBodyBipedEffector.RightHand, num, num);
			break;
		case OIBoneInfo.BoneGroup.LeftArm:
			solver.leftArmMapping.weight = num;
			solver.SetEffectorWeights(FullBodyBipedEffector.LeftShoulder, num, num);
			solver.SetEffectorWeights(FullBodyBipedEffector.LeftHand, num, num);
			break;
		case OIBoneInfo.BoneGroup.RightLeg:
			solver.rightLegMapping.weight = num;
			solver.SetEffectorWeights(FullBodyBipedEffector.RightThigh, num, num);
			solver.SetEffectorWeights(FullBodyBipedEffector.RightFoot, num, num);
			break;
		case OIBoneInfo.BoneGroup.LeftLeg:
			solver.leftLegMapping.weight = num;
			solver.SetEffectorWeights(FullBodyBipedEffector.LeftThigh, num, num);
			solver.SetEffectorWeights(FullBodyBipedEffector.LeftFoot, num, num);
			break;
		}
	}

	public void UpdateFKColor(params OIBoneInfo.BoneGroup[] _parts)
	{
		if (((IReadOnlyCollection<OIBoneInfo.BoneGroup>)(object)_parts).IsNullOrEmpty())
		{
			return;
		}
		foreach (BoneInfo v in listBones)
		{
			int num = Array.FindIndex(_parts, (OIBoneInfo.BoneGroup p) => (p & v.boneGroup) != 0);
			if (num != -1)
			{
				switch (_parts[num])
				{
				case OIBoneInfo.BoneGroup.Hair:
					v.color = Studio.optionSystem.colorFKHair;
					break;
				case OIBoneInfo.BoneGroup.Neck:
					v.color = Studio.optionSystem.colorFKNeck;
					break;
				case OIBoneInfo.BoneGroup.Breast:
					v.color = Studio.optionSystem.colorFKBreast;
					break;
				case OIBoneInfo.BoneGroup.Body:
					v.color = Studio.optionSystem.colorFKBody;
					break;
				case OIBoneInfo.BoneGroup.RightHand:
					v.color = Studio.optionSystem.colorFKRightHand;
					break;
				case OIBoneInfo.BoneGroup.LeftHand:
					v.color = Studio.optionSystem.colorFKLeftHand;
					break;
				case OIBoneInfo.BoneGroup.Skirt:
					v.color = Studio.optionSystem.colorFKSkirt;
					break;
				}
			}
		}
	}

	public void UpdateFKColor(OIBoneInfo.BoneGroup _group)
	{
		foreach (BoneInfo listBone in listBones)
		{
			if ((_group & listBone.boneGroup) != 0)
			{
				switch (_group)
				{
				case OIBoneInfo.BoneGroup.Hair:
					listBone.color = Studio.optionSystem.colorFKHair;
					break;
				case OIBoneInfo.BoneGroup.Neck:
					listBone.color = Studio.optionSystem.colorFKNeck;
					break;
				case OIBoneInfo.BoneGroup.Breast:
					listBone.color = Studio.optionSystem.colorFKBreast;
					break;
				case OIBoneInfo.BoneGroup.Body:
					listBone.color = Studio.optionSystem.colorFKBody;
					break;
				case OIBoneInfo.BoneGroup.RightHand:
					listBone.color = Studio.optionSystem.colorFKRightHand;
					break;
				case OIBoneInfo.BoneGroup.LeftHand:
					listBone.color = Studio.optionSystem.colorFKLeftHand;
					break;
				case OIBoneInfo.BoneGroup.Skirt:
					listBone.color = Studio.optionSystem.colorFKSkirt;
					break;
				}
			}
		}
	}

	public void VisibleFKGuide(bool _visible)
	{
		foreach (BoneInfo listBone in listBones)
		{
			listBone.guideObject.visible = _visible;
		}
	}

	public void VisibleIKGuide(bool _visible)
	{
		foreach (IKInfo item in listIKTarget)
		{
			item.guideObject.visible = _visible;
		}
	}

	public void EnableExpressionCategory(int _category, bool _value)
	{
		oiCharInfo.expression[_category] = _value;
		charInfo.EnableExpressionCategory(_category, _value);
	}

	public void UpdateDynamicBonesBust(int _type = 2)
	{
		if (_type == 0 || _type == 2)
		{
			EnableDynamicBonesBustAndHip(dynamicBust[0] & dynamicBust[2], 0);
		}
		if (_type == 1 || _type == 2)
		{
			EnableDynamicBonesBustAndHip(dynamicBust[1] & dynamicBust[3], 1);
		}
	}

	public void EnableDynamicBonesBustAndHip(bool _enable, int _kind)
	{
		charInfo.cmpBoneBody.EnableDynamicBonesBustAndHip(_enable, _kind);
	}

	public virtual void LoadAnime(int _group, int _category, int _no, float _normalizedTime = 0f)
	{
		Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>> value = null;
		if (!Singleton<Info>.Instance.dicAnimeLoadInfo.TryGetValue(_group, out value))
		{
			return;
		}
		Dictionary<int, Info.AnimeLoadInfo> value2 = null;
		if (!value.TryGetValue(_category, out value2))
		{
			return;
		}
		Info.AnimeLoadInfo value3 = null;
		if (!value2.TryGetValue(_no, out value3))
		{
			return;
		}
		if (loadedAnimeInfo.BaseCheck(value3.bundlePath, value3.fileName))
		{
			charInfo.LoadAnimation(value3.bundlePath, value3.fileName);
			loadedAnimeInfo.baseFile.bundlePath = value3.bundlePath;
			loadedAnimeInfo.baseFile.fileName = value3.fileName;
		}
		if (value3 is Info.HAnimeLoadInfo)
		{
			Info.HAnimeLoadInfo hAnimeLoadInfo = value3 as Info.HAnimeLoadInfo;
			if (hAnimeLoadInfo.overrideFile.Check)
			{
				if (loadedAnimeInfo.OverrideCheck(hAnimeLoadInfo.overrideFile.bundlePath, hAnimeLoadInfo.overrideFile.fileName))
				{
					CommonLib.LoadAsset<RuntimeAnimatorController>(hAnimeLoadInfo.overrideFile.bundlePath, hAnimeLoadInfo.overrideFile.fileName).SafeProc(delegate(RuntimeAnimatorController rac)
					{
						charAnimeCtrl.animator.runtimeAnimatorController = Utils.Animator.SetupAnimatorOverrideController(charAnimeCtrl.animator.runtimeAnimatorController, rac);
					});
					AssetBundleManager.UnloadAssetBundle(hAnimeLoadInfo.overrideFile.bundlePath, isUnloadForceRefCount: true);
					loadedAnimeInfo.overrideFile.bundlePath = hAnimeLoadInfo.overrideFile.bundlePath;
					loadedAnimeInfo.overrideFile.fileName = hAnimeLoadInfo.overrideFile.fileName;
				}
			}
			else
			{
				loadedAnimeInfo.overrideFile.Clear();
			}
			isAnimeMotion = hAnimeLoadInfo.isMotion;
			isHAnime = true;
			animeParam[1] = CheckAnimeParam("Breast1", "Breast", "breast");
			if (!animeParam[1].IsNullOrEmpty())
			{
				charInfo.setAnimatorParamFloat(animeParam[1], charInfo.fileBody.shapeValueBody[1]);
			}
			if (breastLayer != -1)
			{
				charAnimeCtrl.animator.SetLayerWeight(breastLayer, 0f);
				breastLayer = -1;
			}
			if (hAnimeLoadInfo.isBreastLayer)
			{
				charAnimeCtrl.animator.SetLayerWeight(hAnimeLoadInfo.breastLayer, 1f);
				breastLayer = hAnimeLoadInfo.breastLayer;
				charAnimeCtrl.Play(value3.clip, _normalizedTime, hAnimeLoadInfo.breastLayer);
			}
			if (hAnimeLoadInfo.isMotion)
			{
				charInfo.setAnimatorParamFloat("motion", oiCharInfo.animePattern);
			}
			for (int num = 0; num < 8; num++)
			{
				enablePV[num] = hAnimeLoadInfo.pv[num];
				preparation.PvCopy[num] = !oiCharInfo.enableFK && enablePV[num];
			}
			if (!hAnimeLoadInfo.yureFile.Check || !yureCtrl.Load(hAnimeLoadInfo.yureFile.bundlePath, hAnimeLoadInfo.yureFile.fileName, hAnimeLoadInfo.motionID, hAnimeLoadInfo.num))
			{
				yureCtrl.ResetShape();
			}
			charInfo.setAnimatorParamFloat("speed", 1f);
		}
		else
		{
			loadedAnimeInfo.overrideFile.Clear();
			for (int num2 = 0; num2 < 4; num2++)
			{
				enablePV[num2] = true;
				preparation.PvCopy[num2] = !oiCharInfo.enableFK && enablePV[num2];
			}
			isAnimeMotion = false;
			isHAnime = false;
		}
		optionItemCtrl.LoadAnimeItem(value3, value3.clip, charInfo.fileBody.shapeValueBody[0], oiCharInfo.animePattern);
		if (_normalizedTime != 0f)
		{
			charAnimeCtrl.Play(value3.clip, _normalizedTime);
		}
		else
		{
			charAnimeCtrl.Play(value3.clip, 0f);
		}
		animeParam[0] = CheckAnimeParam("height1", "height");
		if (!animeParam[0].IsNullOrEmpty())
		{
			charInfo.setAnimatorParamFloat(animeParam[0], charInfo.fileBody.shapeValueBody[0]);
		}
		animeOptionParam1 = animeOptionParam1;
		animeOptionParam2 = animeOptionParam2;
		charAnimeCtrl.nameHadh = Animator.StringToHash(value3.clip);
		oiCharInfo.animeInfo.Set(_group, _category, _no);
		SetNipStand(oiCharInfo.nipple);
		SetSonLength(oiCharInfo.sonLength);
	}

	public virtual void ChangeHandAnime(int _type, int _ptn)
	{
		oiCharInfo.handPtn[_type] = _ptn;
		if (_ptn != 0)
		{
			charInfo.SetShapeHandValue(_type, _ptn - 1, 0, 0f);
		}
		charInfo.SetEnableShapeHand(_type, _ptn != 0);
	}

	public virtual void RestartAnime()
	{
		Animator animBody = charInfo.animBody;
		int layerCount = animBody.layerCount;
		for (int i = 0; i < layerCount; i++)
		{
			animBody.Play(charInfo.getAnimatorStateInfo(i).shortNameHash, i, 0f);
		}
		optionItemCtrl.PlayAnime();
	}

	private string CheckAnimeParam(params string[] _names)
	{
		AnimatorControllerParameter[] parameters = charInfo.animBody.parameters;
		if (((IReadOnlyCollection<AnimatorControllerParameter>)(object)parameters).IsNullOrEmpty())
		{
			return "";
		}
		int i = 0;
		while (i < _names.Length)
		{
			if (parameters.FirstOrDefault((AnimatorControllerParameter p) => string.CompareOrdinal(p.name, _names[i]) == 0) != null)
			{
				return _names[i];
			}
			int num = i + 1;
			i = num;
		}
		return "";
	}

	private string CheckAnimeParam(string _name1, string _name2)
	{
		AnimatorControllerParameter[] parameters = charInfo.animBody.parameters;
		if (((IReadOnlyCollection<AnimatorControllerParameter>)(object)parameters).IsNullOrEmpty())
		{
			return "";
		}
		if (!CheckParam(parameters, _name1))
		{
			if (!CheckParam(parameters, _name2))
			{
				return "";
			}
			return _name2;
		}
		return _name1;
	}

	private string CheckAnimeParam(string _name1, string _name2, string _name3)
	{
		AnimatorControllerParameter[] parameters = charInfo.animBody.parameters;
		if (((IReadOnlyCollection<AnimatorControllerParameter>)(object)parameters).IsNullOrEmpty())
		{
			return "";
		}
		if (!CheckParam(parameters, _name1))
		{
			if (!CheckParam(parameters, _name2))
			{
				if (!CheckParam(parameters, _name3))
				{
					return "";
				}
				return _name3;
			}
			return _name2;
		}
		return _name1;
	}

	private bool CheckParam(AnimatorControllerParameter[] _parameters, string _name)
	{
		return _parameters.FirstOrDefault((AnimatorControllerParameter _p) => _p.name == _name) != null;
	}

	public virtual void ChangeChara(string _path)
	{
		foreach (BoneInfo item in listBones.Where((BoneInfo v) => v.boneGroup == OIBoneInfo.BoneGroup.Hair).ToList())
		{
			Singleton<GuideObjectManager>.Instance.Delete(item.guideObject);
		}
		listBones = listBones.Where((BoneInfo v) => v.boneGroup != OIBoneInfo.BoneGroup.Hair).ToList();
		int[] array = (from b in oiCharInfo.bones
			where b.Value.@group == OIBoneInfo.BoneGroup.Hair
			select b.Key).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			oiCharInfo.bones.Remove(array[num]);
		}
		skirtDynamic = null;
		charInfo.chaFile.LoadCharaFile(_path, byte.MaxValue, noLoadPng: true);
		charInfo.ChangeNowCoordinate();
		charInfo.Reload();
		for (int num2 = 0; num2 < 2; num2++)
		{
			GameObject gameObject = charInfo.objHair.SafeGet(num2);
			if (gameObject != null)
			{
				AddObjectAssist.ArrangeNames(gameObject.transform);
			}
		}
		treeNodeObject.textName = charInfo.chaFile.parameter.fullname;
		AddObjectAssist.InitHairBone(this, Singleton<Info>.Instance.dicBoneInfo);
		skirtDynamic = AddObjectFemale.GetSkirtDynamic(charInfo.objClothes);
		InitFK(null);
		foreach (var item2 in FKCtrl.parts.Select((OIBoneInfo.BoneGroup p, int i) => new { p, i }))
		{
			ActiveFK(item2.p, oiCharInfo.activeFK[item2.i], oiCharInfo.activeFK[item2.i]);
		}
		ActiveKinematicMode(OICharInfo.KinematicMode.FK, oiCharInfo.enableFK, _force: true);
		UpdateFKColor(OIBoneInfo.BoneGroup.Hair);
		ChangeEyesOpen(charFileStatus.eyesOpenMax);
		ChangeBlink(charFileStatus.eyesBlink);
		ChangeMouthOpen(oiCharInfo.mouthOpen);
	}

	public virtual void SetClothesStateAll(int _state)
	{
	}

	public virtual void SetClothesState(int _id, byte _state)
	{
		charInfo.SetClothesState(_id, _state);
	}

	public virtual void ShowAccessory(int _id, bool _flag)
	{
		charFileStatus.showAccessory[_id] = _flag;
	}

	public virtual void LoadClothesFile(string _path)
	{
		charInfo.ChangeNowCoordinate(_path, reload: true);
		charInfo.AssignCoordinate();
	}

	public virtual void SetSiruFlags(ChaFileDefine.SiruParts _parts, byte _state)
	{
		oiCharInfo.siru[(int)_parts] = _state;
	}

	public virtual byte GetSiruFlags(ChaFileDefine.SiruParts _parts)
	{
		return 0;
	}

	public virtual void SetTuyaRate(float _value)
	{
		charInfo.skinGlossRate = _value;
	}

	public virtual void SetWetRate(float _value)
	{
		charInfo.wetRate = _value;
	}

	public virtual void SetNipStand(float _value)
	{
	}

	public virtual void SetVisibleSimple(bool _flag)
	{
		oiCharInfo.visibleSimple = _flag;
		charInfo.ChangeSimpleBodyDraw(_flag);
	}

	public bool GetVisibleSimple()
	{
		return oiCharInfo.visibleSimple;
	}

	public virtual void SetSimpleColor(Color _color)
	{
		oiCharInfo.simpleColor = _color;
		charInfo.ChangeSimpleBodyColor(_color);
	}

	public virtual void SetVisibleSon(bool _flag)
	{
		oiCharInfo.visibleSon = _flag;
		charFileStatus.visibleSonAlways = _flag;
	}

	public virtual float GetSonLength()
	{
		return oiCharInfo.sonLength;
	}

	public virtual void SetSonLength(float _value)
	{
		oiCharInfo.sonLength = _value;
	}

	public virtual void SetTears(float _state)
	{
		charInfo.ChangeTearsRate(_state);
	}

	public virtual float GetTears()
	{
		return charFileStatus.tearsRate;
	}

	public virtual void SetHohoAkaRate(float _value)
	{
		charInfo.ChangeHohoAkaRate(_value);
	}

	public virtual float GetHohoAkaRate()
	{
		return charInfo.fileStatus.hohoAkaRate;
	}

	public virtual void ChangeLookEyesPtn(int _ptn, bool _force = false)
	{
		int num = (_force ? (-1) : charInfo.fileStatus.eyesLookPtn);
		if (_ptn == 4 && num != 4)
		{
			charInfo.eyeLookCtrl.target = lookAtInfo.target;
			lookAtInfo.active = true;
		}
		else if (num == 4 && _ptn != 4)
		{
			charInfo.eyeLookCtrl.target = Camera.main.transform;
			lookAtInfo.active = false;
		}
		charInfo.ChangeLookEyesPtn(_ptn);
	}

	public virtual void ChangeLookNeckPtn(int _ptn)
	{
		charInfo.ChangeLookNeckPtn(_ptn);
	}

	public virtual void ChangeEyesOpen(float _value)
	{
		charInfo.ChangeEyesOpenMax(_value);
	}

	public virtual void ChangeBlink(bool _value)
	{
		charInfo.ChangeEyesBlinkFlag(_value);
	}

	public virtual void ChangeMouthPtn(int _ptn)
	{
		charInfo.ChangeMouthPtn(_ptn, blend: false);
		switch (_ptn)
		{
		case 10:
		case 13:
			charInfo.DisableShapeMouth(disable: true);
			break;
		case 17:
		case 18:
			charInfo.DisableShapeMouth(disable: true);
			break;
		default:
			charInfo.DisableShapeMouth(disable: false);
			break;
		}
	}

	public virtual void ChangeMouthOpen(float _value)
	{
		oiCharInfo.mouthOpen = _value;
		if (charInfo.mouthCtrl != null)
		{
			charInfo.mouthCtrl.FixedRate = ((voiceCtrl.isPlay && oiCharInfo.lipSync) ? (-1f) : _value);
		}
	}

	public virtual void ChangeLipSync(bool _value)
	{
		oiCharInfo.lipSync = _value;
		charInfo.SetVoiceTransform(_value ? oiCharInfo.voiceCtrl.AudioSource : null);
		ChangeMouthOpen(oiCharInfo.mouthOpen);
	}

	public virtual void SetVoice()
	{
		ChangeLipSync(oiCharInfo.lipSync);
	}

	public virtual void AddVoice(int _group, int _category, int _no)
	{
		voiceCtrl.list.Add(new VoiceCtrl.VoiceInfo(_group, _category, _no));
	}

	public virtual void DeleteVoice(int _index)
	{
		voiceCtrl.list.RemoveAt(_index);
		if (voiceCtrl.index == _index)
		{
			voiceCtrl.index = -1;
			voiceCtrl.Stop();
		}
	}

	public virtual void DeleteAllVoice()
	{
		voiceCtrl.list.Clear();
		voiceCtrl.Stop();
	}

	public virtual bool PlayVoice(int _index)
	{
		return voiceCtrl.Play((_index >= 0) ? _index : 0);
	}

	public virtual void StopVoice()
	{
		voiceCtrl.Stop();
	}
}
