using System;
using System.Collections.Generic;
using AIChara;
using AIProject;
using Illusion.CustomAttributes;
using Manager;
using UniRx;
using UnityEngine;

public class HMotionEyeNeckFemale : MonoBehaviour
{
	[Serializable]
	public struct EyeNeck
	{
		[Label("アニメーション名")]
		public string anim;

		[Label("壊れ")]
		public int state;

		[Label("首コンフィグ無視")]
		public bool isConfigDisregardNeck;

		[Label("目コンフィグ無視")]
		public bool isConfigDisregardEye;

		public bool ExistFaceInfoIya;

		public EyeNeckFace[] faceinfo;

		public void Init()
		{
			anim = "";
			isConfigDisregardNeck = false;
			isConfigDisregardEye = false;
			ExistFaceInfoIya = false;
			faceinfo = new EyeNeckFace[2];
			faceinfo[0].Init();
			faceinfo[1].Init();
		}
	}

	[Serializable]
	public struct EyeNeckFace
	{
		[Label("セリフ無視(首)")]
		public bool isDisregardVoiceNeck;

		[Label("セリフ無視(目)")]
		public bool isDisregardVoiceEye;

		[Label("首挙動")]
		public int Neckbehaviour;

		[Label("目挙動")]
		public int Eyebehaviour;

		[Label("首ターゲット")]
		public int targetNeck;

		[Label("首角度")]
		public Vector3[] NeckRot;

		[Label("頭角度")]
		public Vector3[] HeadRot;

		[Label("目ターゲット")]
		public int targetEye;

		[Label("視線角度")]
		public Vector3[] EyeRot;

		public void Init()
		{
			isDisregardVoiceNeck = false;
			isDisregardVoiceEye = false;
			Neckbehaviour = 0;
			Eyebehaviour = 0;
			targetEye = 0;
			EyeRot = new Vector3[2]
			{
				Vector3.zero,
				Vector3.zero
			};
			targetNeck = 0;
			NeckRot = new Vector3[2]
			{
				Vector3.zero,
				Vector3.zero
			};
			HeadRot = new Vector3[2]
			{
				Vector3.zero,
				Vector3.zero
			};
		}
	}

	[SerializeField]
	private HSceneFlagCtrl hFlag;

	[Label("相手男顔オブジェクト名")]
	public string strMaleHead = "";

	[Label("相手男性器オブジェクト名")]
	public string strMaleGenital = "";

	[Label("相手女顔オブジェクト名")]
	public string strFemaleHead = "";

	[Label("相手女性器オブジェクト名")]
	public string strFemaleGenital = "";

	[SerializeField]
	private List<EyeNeck> lstEyeNeck = new List<EyeNeck>();

	[DisabledGroup("女クラス")]
	[SerializeField]
	private ChaControl chaFemale;

	[DisabledGroup("自分性器オブジェクト")]
	[SerializeField]
	private GameObject objGenitalSelf;

	[DisabledGroup("男1顔オブジェクト")]
	[SerializeField]
	private GameObject objMale1Head;

	[DisabledGroup("男1性器オブジェクト")]
	[SerializeField]
	private GameObject objMale1Genital;

	[DisabledGroup("男2顔オブジェクト")]
	[SerializeField]
	private GameObject objMale2Head;

	[DisabledGroup("男2性器オブジェクト")]
	[SerializeField]
	private GameObject objMale2Genital;

	[DisabledGroup("女相手顔オブジェクト")]
	[SerializeField]
	private GameObject objFemale1Head;

	[DisabledGroup("女相手性器オブジェクト")]
	[SerializeField]
	private GameObject objFemale1Genital;

	private Transform LoopParent;

	private EyeNeck en;

	private EyeNeckFace enFace;

	private int ID = -1;

	private AnimatorStateInfo ai;

	private Transform NeckTrs;

	private Transform HeadTrs;

	private Transform[] EyeTrs;

	private Quaternion BackUpNeck;

	private Quaternion BackUpHead;

	private Quaternion[] BackUpEye;

	private HVoiceCtrl.FaceInfo faceInfo;

	private float ChangeTimeNeck;

	private int NeckType;

	private float ChangeTimeEye;

	private int EyeType;

	private bool[] bFaceInfo = new bool[2];

	private bool[] bFaceInfoOld = new bool[2];

	private float oldYuragi;

	private string OldAnimName;

	private HScene hscene;

	private ExcelData excelData;

	private EyeNeck info;

	private List<string> row = new List<string>();

	private string abName = "";

	private string assetName = "";

	public bool NowEndADV;

	private bool[] BlendInit = new bool[2];

	private Vector3[] BlendNeckRot = new Vector3[2];

	private Vector3 BlendEyeRot = Vector3.zero;

	private bool changeMotion;

	public float NeckLerpSpeed = 100f;

	public float NeckPatternSpeed = 0.1f;

	public float EyeLerpSpeed = 100f;

	private bool EventDisregardEyeNeck;

	private bool oldVoice;

	private bool ReturnNeck;

	private Transform[] getChild;

	private float[] nowleapSpeed = new float[2];

	public int CharID => ID;

	public bool Init(ChaControl _female, int id, HScene hScene)
	{
		Release();
		ID = id;
		chaFemale = _female;
		NeckTrs = chaFemale.neckLookCtrl.neckLookScript.aBones[0].neckBone;
		HeadTrs = chaFemale.neckLookCtrl.neckLookScript.aBones[1].neckBone;
		EyeTrs = new Transform[2]
		{
			chaFemale.eyeLookCtrl.eyeLookScript.eyeObjs[0].eyeTransform,
			chaFemale.eyeLookCtrl.eyeLookScript.eyeObjs[1].eyeTransform
		};
		BackUpNeck = NeckTrs.localRotation;
		BackUpHead = HeadTrs.localRotation;
		BackUpEye = new Quaternion[2]
		{
			EyeTrs[0].localRotation,
			EyeTrs[1].localRotation
		};
		objGenitalSelf = null;
		if ((bool)chaFemale && (bool)chaFemale.objBodyBone)
		{
			LoopParent = chaFemale.objBodyBone.transform;
			if (strFemaleGenital != "")
			{
				objGenitalSelf = GetObjectName(LoopParent, strFemaleGenital);
			}
		}
		EventDisregardEyeNeck = false;
		if (hscene == null)
		{
			hscene = hScene;
		}
		return true;
	}

	public void Release()
	{
		lstEyeNeck.Clear();
		ID = -1;
	}

	private GameObject GetObjectName(Transform top, string name)
	{
		getChild = top.GetComponentsInChildren<Transform>();
		for (int i = 0; i < getChild.Length; i++)
		{
			if (!(getChild[i].name != name))
			{
				return getChild[i].gameObject;
			}
		}
		return null;
	}

	public bool Load(string _assetpath, string _file)
	{
		lstEyeNeck.Clear();
		ChangeTimeEye = 0f;
		nowleapSpeed[1] = 0f;
		BackUpEye[0] = EyeTrs[0].localRotation;
		BackUpEye[1] = EyeTrs[1].localRotation;
		ChangeTimeNeck = 0f;
		nowleapSpeed[0] = 0f;
		BackUpNeck = NeckTrs.localRotation;
		BackUpHead = HeadTrs.localRotation;
		BlendInit[0] = false;
		BlendInit[1] = false;
		if (_file == "")
		{
			return false;
		}
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(_assetpath);
		assetBundleNameListFromPath.Sort();
		abName = "";
		assetName = "";
		excelData = null;
		info.Init();
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
			{
				continue;
			}
			abName = assetBundleNameListFromPath[i];
			assetName = _file;
			if (!GlobalMethod.AssetFileExist(abName, assetName))
			{
				continue;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(abName, assetName);
			Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(abName);
			if (excelData == null)
			{
				continue;
			}
			int num = 3;
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				info = default(EyeNeck);
				info.Init();
				if (row.Count == 0)
				{
					continue;
				}
				string element = row.GetElement(num2++);
				if (element.IsNullOrEmpty())
				{
					continue;
				}
				info.anim = element;
				if (!int.TryParse(row.GetElement(num2++), out info.state))
				{
					info.state = 0;
				}
				info.isConfigDisregardNeck = row.GetElement(num2++) == "1";
				info.isConfigDisregardEye = row.GetElement(num2++) == "1";
				element = row.GetElement(num2++);
				info.faceinfo[0].isDisregardVoiceNeck = !element.IsNullOrEmpty() && element == "1";
				element = row.GetElement(num2++);
				info.faceinfo[0].isDisregardVoiceEye = !element.IsNullOrEmpty() && element == "1";
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[0].Neckbehaviour))
				{
					info.faceinfo[0].Neckbehaviour = 0;
				}
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[0].Eyebehaviour))
				{
					info.faceinfo[0].Eyebehaviour = 0;
				}
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[0].targetNeck))
				{
					info.faceinfo[0].targetNeck = 0;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[0].NeckRot[0] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[0].NeckRot[1] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[0].HeadRot[0] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[0].HeadRot[1] = zero;
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[0].targetEye))
				{
					info.faceinfo[0].targetEye = 0;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[0].EyeRot[0] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[0].EyeRot[1] = zero;
				if (num2 >= row.Count)
				{
					lstEyeNeck.Add(info);
					continue;
				}
				element = row.GetElement(num2++);
				if (element.IsNullOrEmpty())
				{
					lstEyeNeck.Add(info);
					continue;
				}
				info.ExistFaceInfoIya = true;
				info.faceinfo[1].isDisregardVoiceNeck = !element.IsNullOrEmpty() && element == "1";
				element = row.GetElement(num2++);
				info.faceinfo[1].isDisregardVoiceEye = !element.IsNullOrEmpty() && element == "1";
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[1].Neckbehaviour))
				{
					info.faceinfo[1].Neckbehaviour = 0;
				}
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[1].Eyebehaviour))
				{
					info.faceinfo[1].Eyebehaviour = 0;
				}
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[1].targetNeck))
				{
					info.faceinfo[1].targetNeck = 0;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[1].NeckRot[0] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[1].NeckRot[1] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[1].HeadRot[0] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[1].HeadRot[1] = zero;
				if (!int.TryParse(row.GetElement(num2++), out info.faceinfo[1].targetEye))
				{
					info.faceinfo[1].targetEye = 0;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[1].EyeRot[0] = zero;
				if (!float.TryParse(row.GetElement(num2++), out zero.x))
				{
					zero.x = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.y))
				{
					zero.y = 0f;
				}
				if (!float.TryParse(row.GetElement(num2++), out zero.z))
				{
					zero.z = 0f;
				}
				info.faceinfo[1].EyeRot[1] = zero;
				lstEyeNeck.Add(info);
			}
		}
		return true;
	}

	public bool SetPartner(GameObject _objMale1Bone, GameObject _objMale2Bone, GameObject _objFemale1Bone)
	{
		objMale1Head = null;
		objMale2Head = null;
		objFemale1Head = null;
		objMale1Genital = null;
		objMale2Genital = null;
		objFemale1Genital = null;
		if ((bool)_objMale1Bone)
		{
			LoopParent = _objMale1Bone.transform;
			if (strMaleHead != "")
			{
				objMale1Head = GetObjectName(LoopParent, strMaleHead);
			}
			if (strMaleGenital != "")
			{
				objMale1Genital = GetObjectName(LoopParent, strMaleGenital);
			}
		}
		if ((bool)_objMale2Bone)
		{
			LoopParent = _objMale2Bone.transform;
			if (strMaleHead != "")
			{
				objMale2Head = GetObjectName(LoopParent, strMaleHead);
			}
			if (strMaleGenital != "")
			{
				objMale2Genital = GetObjectName(LoopParent, strMaleGenital);
			}
		}
		if ((bool)_objFemale1Bone)
		{
			LoopParent = _objFemale1Bone.transform;
			if (strFemaleHead != "")
			{
				objFemale1Head = GetObjectName(LoopParent, strFemaleHead);
			}
			if (strFemaleGenital != "")
			{
				objFemale1Genital = GetObjectName(LoopParent, strFemaleGenital);
			}
		}
		return true;
	}

	public bool Proc(AnimatorStateInfo _ai, HVoiceCtrl.FaceInfo _faceVoice, int _main)
	{
		if (ID < 0)
		{
			return false;
		}
		bool flag = ((_main == 0) ? Manager.Config.HData.EyeDir0 : Manager.Config.HData.EyeDir1);
		bool flag2 = ((_main == 0) ? Manager.Config.HData.NeckDir0 : Manager.Config.HData.NeckDir1);
		if (EventDisregardEyeNeck)
		{
			flag = false;
			flag2 = false;
		}
		ai = _ai;
		faceInfo = _faceVoice;
		changeMotion = false;
		bool flag3 = Singleton<HSceneManager>.Instance.FemaleState[ID] == ChaFileDefine.State.Broken;
		for (int i = 0; i < lstEyeNeck.Count; i++)
		{
			en = lstEyeNeck[i];
			if (_ai.IsName(en.anim) && (!flag3 || en.state == 2))
			{
				if (en.ExistFaceInfoIya && !flag3)
				{
					bool flag4 = ((_main == 0) ? Singleton<HSceneManager>.Instance.isForce : Singleton<HSceneManager>.Instance.isForceSecond);
					enFace = en.faceinfo[flag4 ? 1 : 0];
				}
				else
				{
					enFace = en.faceinfo[0];
				}
				break;
			}
		}
		if (hscene.ctrlVoice.nowVoices[ID].state == HVoiceCtrl.VoiceKind.startVoice || hscene.ctrlVoice.nowVoices[ID].state == HVoiceCtrl.VoiceKind.voice)
		{
			if (!enFace.isDisregardVoiceEye)
			{
				bFaceInfo[0] = true;
				SetEyesTarget((!flag) ? ((faceInfo != null) ? faceInfo.targetEyeLine : 0) : 0);
				SetBehaviourEyes(flag ? 1 : ((faceInfo != null) ? faceInfo.behaviorEyeLine : 0));
			}
			else
			{
				SetEyesTarget(en.isConfigDisregardEye ? enFace.targetEye : ((!flag) ? enFace.targetEye : 0));
				SetBehaviourEyes(en.isConfigDisregardEye ? enFace.Eyebehaviour : (flag ? 1 : enFace.Eyebehaviour));
			}
			if (!enFace.isDisregardVoiceNeck)
			{
				bFaceInfo[1] = true;
				SetNeckTarget((!flag2) ? ((faceInfo != null) ? faceInfo.targetNeckLine : 0) : 0);
				SetBehaviourNeck(flag2 ? 1 : ((faceInfo != null) ? faceInfo.behaviorNeckLine : 0));
			}
			else
			{
				SetNeckTarget(en.isConfigDisregardNeck ? enFace.targetNeck : ((!flag2) ? enFace.targetNeck : 0));
				SetBehaviourNeck(en.isConfigDisregardNeck ? enFace.Neckbehaviour : (flag2 ? 1 : enFace.Neckbehaviour));
			}
			oldVoice = true;
		}
		else
		{
			bFaceInfo[0] = false;
			bFaceInfo[1] = false;
			SetNeckTarget(en.isConfigDisregardNeck ? enFace.targetNeck : ((!flag2) ? enFace.targetNeck : 0));
			SetBehaviourNeck(en.isConfigDisregardNeck ? enFace.Neckbehaviour : (flag2 ? 1 : enFace.Neckbehaviour));
			SetEyesTarget(en.isConfigDisregardEye ? enFace.targetEye : ((!flag) ? enFace.targetEye : 0));
			SetBehaviourEyes(en.isConfigDisregardEye ? enFace.Eyebehaviour : (flag ? 1 : enFace.Eyebehaviour));
			if (OldAnimName != en.anim)
			{
				ChangeTimeEye = 0f;
				nowleapSpeed[1] = 0f;
				BackUpEye[0] = EyeTrs[0].localRotation;
				BackUpEye[1] = EyeTrs[1].localRotation;
				ChangeTimeNeck = 0f;
				nowleapSpeed[0] = 0f;
				BackUpNeck = NeckTrs.localRotation;
				BackUpHead = HeadTrs.localRotation;
				BlendInit[0] = false;
				BlendInit[1] = false;
				if (!oldVoice)
				{
					changeMotion = true;
				}
				else
				{
					ReturnNeck = true;
				}
			}
			OldAnimName = en.anim;
			oldVoice = false;
		}
		return true;
	}

	private void LateUpdate()
	{
		EyeNeckCalc();
	}

	public void EyeNeckCalc()
	{
		if (chaFemale == null || NowEndADV || ID < 0)
		{
			return;
		}
		bool flag = ((ID == 0) ? Manager.Config.HData.NeckDir0 : Manager.Config.HData.NeckDir1);
		bool flag2 = ((ID == 0) ? Manager.Config.HData.EyeDir0 : Manager.Config.HData.EyeDir1);
		float num = ((!hFlag.nowAnimationInfo.reverseTaii) ? hFlag.motions[ID] : hFlag.motions[ID ^ 1]);
		bool flag3 = Singleton<HSceneManager>.Instance.FemaleState[ID] == ChaFileDefine.State.Broken;
		if (!bFaceInfo[0] || !bFaceInfo[1])
		{
			for (int i = 0; i < lstEyeNeck.Count; i++)
			{
				en = lstEyeNeck[i];
				if (!ai.IsName(en.anim) || (flag3 && en.state != 2))
				{
					continue;
				}
				if (en.ExistFaceInfoIya && !flag3)
				{
					bool flag4 = ((ID == 0) ? Singleton<HSceneManager>.Instance.isForce : Singleton<HSceneManager>.Instance.isForceSecond);
					enFace = en.faceinfo[flag4 ? 1 : 0];
				}
				else
				{
					enFace = en.faceinfo[0];
				}
				if (!flag)
				{
					if (!bFaceInfo[1])
					{
						if (enFace.targetNeck == 7)
						{
							if (oldYuragi != num || !BlendInit[0])
							{
								if (!BlendInit[0])
								{
									BlendInit[0] = true;
								}
								if (!changeMotion)
								{
									ChangeTimeNeck = 0f;
								}
								else
								{
									ChangeTimeNeck = NeckPatternSpeed;
								}
								BlendNeckRot[0].x = Mathf.Lerp(enFace.NeckRot[0].x, enFace.NeckRot[1].x, num);
								BlendNeckRot[0].y = Mathf.Lerp(enFace.NeckRot[0].y, enFace.NeckRot[1].y, num);
								BlendNeckRot[0].z = Mathf.Lerp(enFace.NeckRot[0].z, enFace.NeckRot[1].z, num);
								BlendNeckRot[1].x = Mathf.Lerp(enFace.HeadRot[0].x, enFace.HeadRot[1].x, num);
								BlendNeckRot[1].y = Mathf.Lerp(enFace.HeadRot[0].y, enFace.HeadRot[1].y, num);
								BlendNeckRot[1].z = Mathf.Lerp(enFace.HeadRot[0].z, enFace.HeadRot[1].z, num);
							}
							NeckCalc(BlendNeckRot[0], BlendNeckRot[1]);
						}
					}
					else if (faceInfo != null && faceInfo.targetNeckLine == 7)
					{
						if (oldYuragi != num || !BlendInit[0])
						{
							if (!BlendInit[0])
							{
								BlendInit[0] = true;
							}
							if (!changeMotion)
							{
								ChangeTimeNeck = 0f;
							}
							else
							{
								ChangeTimeNeck = NeckPatternSpeed;
							}
							BlendNeckRot[0].x = Mathf.Lerp(faceInfo.NeckRot[0].x, faceInfo.NeckRot[1].x, num);
							BlendNeckRot[0].y = Mathf.Lerp(faceInfo.NeckRot[0].y, faceInfo.NeckRot[1].y, num);
							BlendNeckRot[0].z = Mathf.Lerp(faceInfo.NeckRot[0].z, faceInfo.NeckRot[1].z, num);
							BlendNeckRot[1].x = Mathf.Lerp(faceInfo.HeadRot[0].x, faceInfo.HeadRot[1].x, num);
							BlendNeckRot[1].y = Mathf.Lerp(faceInfo.HeadRot[0].y, faceInfo.HeadRot[1].y, num);
							BlendNeckRot[1].z = Mathf.Lerp(faceInfo.HeadRot[0].z, faceInfo.HeadRot[1].z, num);
						}
						NeckCalc(BlendNeckRot[0], BlendNeckRot[1]);
					}
				}
				if (!flag2)
				{
					if (!bFaceInfo[0])
					{
						if (enFace.targetEye == 7)
						{
							if (oldYuragi != num || !BlendInit[1])
							{
								if (!BlendInit[1])
								{
									BlendInit[1] = true;
								}
								if (!changeMotion)
								{
									ChangeTimeEye = 0f;
								}
								else
								{
									ChangeTimeEye = 1f;
								}
								BlendEyeRot.x = Mathf.Lerp(enFace.EyeRot[0].x, enFace.EyeRot[1].x, num);
								BlendEyeRot.y = Mathf.Lerp(enFace.EyeRot[0].y, enFace.EyeRot[1].y, num);
								BlendEyeRot.z = Mathf.Lerp(enFace.EyeRot[0].z, enFace.EyeRot[1].z, num);
							}
							EyeCalc(BlendEyeRot);
						}
					}
					else if (faceInfo != null && faceInfo.targetEyeLine == 7)
					{
						if (oldYuragi != num || !BlendInit[1])
						{
							if (!BlendInit[1])
							{
								BlendInit[1] = true;
							}
							if (!changeMotion)
							{
								ChangeTimeEye = 0f;
							}
							else
							{
								ChangeTimeEye = 1f;
							}
							BlendEyeRot.x = Mathf.Lerp(faceInfo.EyeRot[0].x, faceInfo.EyeRot[1].x, num);
							BlendEyeRot.y = Mathf.Lerp(faceInfo.EyeRot[0].y, faceInfo.EyeRot[1].y, num);
							BlendEyeRot.z = Mathf.Lerp(faceInfo.EyeRot[0].z, faceInfo.EyeRot[1].z, num);
						}
						EyeCalc(BlendEyeRot);
					}
				}
				oldYuragi = num;
				if (changeMotion)
				{
					changeMotion = false;
				}
				return;
			}
		}
		else if (faceInfo != null)
		{
			if (!flag && faceInfo.targetNeckLine == 7)
			{
				if (oldYuragi != num || !BlendInit[0])
				{
					if (!BlendInit[0])
					{
						BlendInit[0] = true;
					}
					if (!changeMotion)
					{
						ChangeTimeNeck = 0f;
					}
					else
					{
						ChangeTimeNeck = NeckPatternSpeed;
					}
					BlendNeckRot[0].x = Mathf.Lerp(faceInfo.NeckRot[0].x, faceInfo.NeckRot[1].x, num);
					BlendNeckRot[0].y = Mathf.Lerp(faceInfo.NeckRot[0].y, faceInfo.NeckRot[1].y, num);
					BlendNeckRot[0].z = Mathf.Lerp(faceInfo.NeckRot[0].z, faceInfo.NeckRot[1].z, num);
					BlendNeckRot[1].x = Mathf.Lerp(faceInfo.HeadRot[0].x, faceInfo.HeadRot[1].x, num);
					BlendNeckRot[1].y = Mathf.Lerp(faceInfo.HeadRot[0].y, faceInfo.HeadRot[1].y, num);
					BlendNeckRot[1].z = Mathf.Lerp(faceInfo.HeadRot[0].z, faceInfo.HeadRot[1].z, num);
				}
				NeckCalc(BlendNeckRot[0], BlendNeckRot[1]);
			}
			if (!flag2 && faceInfo.targetEyeLine == 7)
			{
				if (oldYuragi != num || !BlendInit[1])
				{
					if (!BlendInit[1])
					{
						BlendInit[1] = true;
					}
					if (!changeMotion)
					{
						ChangeTimeEye = 0f;
					}
					else
					{
						ChangeTimeEye = 1f;
					}
					BlendEyeRot.x = Mathf.Lerp(faceInfo.EyeRot[0].x, faceInfo.EyeRot[1].x, num);
					BlendEyeRot.y = Mathf.Lerp(faceInfo.EyeRot[0].y, faceInfo.EyeRot[1].y, num);
					BlendEyeRot.z = Mathf.Lerp(faceInfo.EyeRot[0].z, faceInfo.EyeRot[1].z, num);
				}
				EyeCalc(BlendEyeRot);
			}
			oldYuragi = num;
			if (changeMotion)
			{
				changeMotion = false;
			}
			return;
		}
		if (changeMotion)
		{
			changeMotion = false;
		}
		oldYuragi = num;
	}

	private bool SetEyesTarget(int _tag)
	{
		switch (_tag)
		{
		case 1:
			chaFemale.ChangeLookEyesTarget(1, objMale1Head ? objMale1Head.transform : null);
			break;
		case 2:
			chaFemale.ChangeLookEyesTarget(1, objMale1Genital ? objMale1Genital.transform : null);
			break;
		case 3:
			chaFemale.ChangeLookEyesTarget(1, objMale2Head ? objMale2Head.transform : null);
			break;
		case 4:
			chaFemale.ChangeLookEyesTarget(1, objMale2Genital ? objMale2Genital.transform : null);
			break;
		case 5:
			chaFemale.ChangeLookEyesTarget(1, objFemale1Head ? objFemale1Head.transform : null);
			break;
		case 6:
			chaFemale.ChangeLookEyesTarget(1, objFemale1Genital ? objFemale1Genital.transform : null);
			break;
		case 8:
			chaFemale.ChangeLookEyesTarget(1, objGenitalSelf ? objGenitalSelf.transform : null);
			break;
		default:
			chaFemale.ChangeLookEyesTarget(0);
			break;
		case 7:
			break;
		}
		if (EyeType != _tag)
		{
			ChangeTimeEye = 0f;
			nowleapSpeed[1] = 0f;
			BackUpEye[0] = EyeTrs[0].localRotation;
			BackUpEye[1] = EyeTrs[1].localRotation;
		}
		EyeType = _tag;
		return true;
	}

	private bool SetNeckTarget(int _tag)
	{
		if (chaFemale.neckLookCtrl == null)
		{
			return false;
		}
		switch (_tag)
		{
		case 1:
			chaFemale.ChangeLookNeckTarget(1, objMale1Head ? objMale1Head.transform : null);
			break;
		case 2:
			chaFemale.ChangeLookNeckTarget(1, objMale1Genital ? objMale1Genital.transform : null);
			break;
		case 3:
			chaFemale.ChangeLookNeckTarget(1, objMale2Head ? objMale2Head.transform : null);
			break;
		case 4:
			chaFemale.ChangeLookNeckTarget(1, objMale2Genital ? objMale2Genital.transform : null);
			break;
		case 5:
			chaFemale.ChangeLookNeckTarget(1, objFemale1Head ? objFemale1Head.transform : null);
			break;
		case 6:
			chaFemale.ChangeLookNeckTarget(1, objFemale1Genital ? objFemale1Genital.transform : null);
			break;
		case 8:
			chaFemale.ChangeLookNeckTarget(1, objGenitalSelf ? objGenitalSelf.transform : null);
			break;
		default:
			chaFemale.ChangeLookNeckTarget(0);
			break;
		case 7:
			break;
		}
		if (NeckType != _tag)
		{
			ChangeTimeNeck = 0f;
			nowleapSpeed[0] = 0f;
			BackUpNeck = NeckTrs.localRotation;
			BackUpHead = HeadTrs.localRotation;
			if (!((ID == 0) ? Manager.Config.HData.NeckDir0 : Manager.Config.HData.NeckDir1) && bFaceInfo[1] == bFaceInfoOld[1] && !bFaceInfo[1])
			{
				chaFemale.neckLookCtrl.neckLookScript.skipCalc = true;
				Observable.NextFrame().Subscribe(delegate
				{
					chaFemale.neckLookCtrl.neckLookScript.skipCalc = false;
				});
			}
		}
		NeckType = _tag;
		if (!bFaceInfo[1] && bFaceInfoOld[1] && _tag == 7)
		{
			ReturnNeck = true;
		}
		bFaceInfoOld[1] = bFaceInfo[1];
		return true;
	}

	private bool SetBehaviourEyes(int _behaviour)
	{
		switch (_behaviour)
		{
		case 1:
			chaFemale.ChangeLookEyesPtn(1);
			break;
		case 2:
			chaFemale.ChangeLookEyesPtn(2);
			break;
		case 3:
			chaFemale.ChangeLookEyesPtn(1);
			break;
		case 4:
			chaFemale.ChangeLookEyesPtn(3);
			break;
		default:
			chaFemale.ChangeLookEyesPtn(0);
			break;
		}
		if (_behaviour == 3)
		{
			chaFemale.eyeLookCtrl.enabled = false;
		}
		else
		{
			chaFemale.eyeLookCtrl.enabled = true;
		}
		return true;
	}

	private bool SetBehaviourNeck(int _behaviour)
	{
		if (chaFemale.neckLookCtrl == null)
		{
			return false;
		}
		if (!chaFemale.neckLookCtrl.enabled && _behaviour != 3)
		{
			chaFemale.neckLookCtrl.neckLookScript.UpdateCall(0);
		}
		switch (_behaviour)
		{
		case 1:
			chaFemale.ChangeLookNeckPtn(1);
			break;
		case 2:
			chaFemale.ChangeLookNeckPtn(2);
			break;
		case 3:
			chaFemale.ChangeLookNeckPtn(1);
			break;
		case 4:
			chaFemale.ChangeLookNeckPtn(4);
			break;
		default:
			chaFemale.ChangeLookNeckPtn(3);
			break;
		}
		if (_behaviour == 3)
		{
			chaFemale.neckLookCtrl.enabled = false;
		}
		else
		{
			chaFemale.neckLookCtrl.enabled = true;
		}
		return true;
	}

	private void NeckCalc(Vector3 targetNeckRot, Vector3 targetHeadRot)
	{
		if (!(chaFemale.neckLookCtrl == null))
		{
			float deltaTime = Time.deltaTime;
			float num;
			if (ReturnNeck)
			{
				ChangeTimeNeck = Mathf.Clamp(ChangeTimeNeck + deltaTime, 0f, chaFemale.neckLookCtrl.neckLookScript.changeTypeLeapTime);
				num = Mathf.InverseLerp(0f, chaFemale.neckLookCtrl.neckLookScript.changeTypeLeapTime, ChangeTimeNeck);
			}
			else
			{
				ChangeTimeNeck = Mathf.Clamp(ChangeTimeNeck + deltaTime, 0f, NeckPatternSpeed);
				num = Mathf.InverseLerp(0f, NeckPatternSpeed, ChangeTimeNeck);
			}
			if (chaFemale.neckLookCtrl.neckLookScript.changeTypeLerpCurve != null)
			{
				num = chaFemale.neckLookCtrl.neckLookScript.changeTypeLerpCurve.Evaluate(num);
			}
			if (ReturnNeck)
			{
				nowleapSpeed[0] = Mathf.Clamp01(chaFemale.neckLookCtrl.neckLookScript.neckTypeStates[3].leapSpeed * deltaTime);
			}
			else
			{
				nowleapSpeed[0] = Mathf.Clamp01(NeckLerpSpeed * deltaTime);
			}
			Quaternion b = Quaternion.Slerp(BackUpNeck, Quaternion.Euler(targetNeckRot), nowleapSpeed[0]);
			Quaternion b2 = Quaternion.Slerp(NeckTrs.localRotation, b, chaFemale.neckLookCtrl.neckLookScript.calcLerp);
			b2 = Quaternion.Slerp(BackUpNeck, b2, num);
			Quaternion backUpNeck = (NeckTrs.localRotation = b2);
			BackUpNeck = backUpNeck;
			b = Quaternion.Slerp(BackUpHead, Quaternion.Euler(targetHeadRot), nowleapSpeed[0]);
			b2 = Quaternion.Slerp(HeadTrs.localRotation, b, chaFemale.neckLookCtrl.neckLookScript.calcLerp);
			b2 = Quaternion.Slerp(BackUpHead, b2, num);
			backUpNeck = (HeadTrs.localRotation = b2);
			BackUpHead = backUpNeck;
			chaFemale.neckLookCtrl.neckLookScript.SetBoneFixAngle(0, NeckTrs.localRotation);
			chaFemale.neckLookCtrl.neckLookScript.SetBoneFixAngle(1, HeadTrs.localRotation);
			if (ReturnNeck && BackUpNeck.eulerAngles == targetNeckRot && BackUpHead.eulerAngles == targetHeadRot)
			{
				ReturnNeck = false;
				ChangeTimeNeck = 0f;
			}
		}
	}

	private void EyeCalc(Vector3 targetEyeRot)
	{
		float deltaTime = Time.deltaTime;
		ChangeTimeEye = Mathf.Clamp(ChangeTimeEye + deltaTime, 0f, 1f);
		float t = Mathf.InverseLerp(0f, 1f, ChangeTimeEye);
		nowleapSpeed[1] = Mathf.Clamp01(EyeLerpSpeed * deltaTime);
		Quaternion b = Quaternion.Slerp(BackUpEye[0], Quaternion.Euler(targetEyeRot), nowleapSpeed[1]);
		Quaternion b2 = Quaternion.Slerp(EyeTrs[0].localRotation, b, 1f);
		b2 = Quaternion.Slerp(BackUpEye[0], b2, t);
		Quaternion[] backUpEye = BackUpEye;
		Quaternion quaternion = (EyeTrs[0].localRotation = b2);
		backUpEye[0] = quaternion;
		b = Quaternion.Slerp(BackUpEye[1], Quaternion.Euler(targetEyeRot), nowleapSpeed[1]);
		b2 = Quaternion.Slerp(EyeTrs[1].localRotation, b, 1f);
		b2 = Quaternion.Slerp(BackUpEye[1], b2, t);
		Quaternion[] backUpEye2 = BackUpEye;
		quaternion = (EyeTrs[1].localRotation = b2);
		backUpEye2[1] = quaternion;
		float y = EyeTrs[1].localRotation.eulerAngles.y;
		float x = EyeTrs[1].localRotation.eulerAngles.x;
		y = ((y > 180f) ? (y - 360f) : y);
		x = ((x > 180f) ? (x - 360f) : x);
		chaFemale.eyeLookCtrl.eyeLookScript.SetAngleHV(0, y, x);
		chaFemale.eyeLookCtrl.eyeLookScript.SetAngleHV(1, y, x);
	}

	public void SetConfigBehaviour(AnimatorStateInfo _ai, bool neck, bool eye)
	{
		bool flag = Singleton<HSceneManager>.Instance.FemaleState[ID] == ChaFileDefine.State.Broken;
		for (int i = 0; i < lstEyeNeck.Count; i++)
		{
			en = lstEyeNeck[i];
			if (_ai.IsName(en.anim) && (!flag || en.state == 2))
			{
				if (en.ExistFaceInfoIya && !flag)
				{
					bool flag2 = ((ID == 0) ? Singleton<HSceneManager>.Instance.isForce : Singleton<HSceneManager>.Instance.isForceSecond);
					enFace = en.faceinfo[flag2 ? 1 : 0];
				}
				else
				{
					enFace = en.faceinfo[0];
				}
				break;
			}
		}
		if (neck)
		{
			SetNeckTarget(en.isConfigDisregardNeck ? enFace.targetNeck : 0);
			SetBehaviourNeck((!en.isConfigDisregardNeck) ? 1 : enFace.Neckbehaviour);
		}
		if (neck)
		{
			SetEyesTarget(en.isConfigDisregardEye ? enFace.targetEye : 0);
			SetBehaviourEyes((!en.isConfigDisregardEye) ? 1 : enFace.Eyebehaviour);
		}
	}

	public void SetEventDisregard()
	{
		EventDisregardEyeNeck = true;
	}
}
