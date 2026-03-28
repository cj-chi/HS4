using System;
using System.Collections.Generic;
using AIChara;
using AIProject;
using Illusion.CustomAttributes;
using IllusionUtility.GetUtility;
using Manager;
using UniRx;
using UnityEngine;

public class HMotionEyeNeckMale : MonoBehaviour
{
	[Serializable]
	public struct EyeNeck
	{
		[Label("アニメーション名")]
		public string anim;

		[Label("目の開き")]
		public int openEye;

		[Label("口の開き")]
		public int openMouth;

		[Label("眉")]
		public int eyebrow;

		[Label("目")]
		public int eye;

		[Label("口")]
		public int mouth;

		[Label("首挙動")]
		public int Neckbehaviour;

		[Label("目挙動")]
		public int Eyebehaviour;

		[Label("目ターゲット")]
		public int targetEye;

		[Label("視線角度")]
		public Vector3[] EyeRot;

		[Label("首ターゲット")]
		public int targetNeck;

		[Label("首角度")]
		public Vector3[] NeckRot;

		[Label("頭角度")]
		public Vector3[] HeadRot;

		public void Init()
		{
			anim = "";
			openEye = 0;
			openMouth = 0;
			eyebrow = 0;
			eye = 0;
			mouth = 0;
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

	[Label("相手女顔オブジェクト名")]
	public string strFemaleHead = "";

	[Label("相手女性器オブジェクト名")]
	public string strFemaleGenital = "";

	[Label("相手男顔オブジェクト名")]
	public string strMaleHead = "";

	[Label("相手男性器オブジェクト名")]
	public string strMaleGenital = "";

	[SerializeField]
	private List<EyeNeck> lstEyeNeck = new List<EyeNeck>();

	[DisabledGroup("男クラス")]
	[SerializeField]
	private ChaControl chaMale;

	[DisabledGroup("女1顔オブジェクト")]
	[SerializeField]
	private GameObject objFemale1Head;

	[DisabledGroup("女1性器オブジェクト")]
	[SerializeField]
	private GameObject objFemale1Genital;

	[DisabledGroup("女2顔オブジェクト")]
	[SerializeField]
	private GameObject objFemale2Head;

	[DisabledGroup("女2性器オブジェクト")]
	[SerializeField]
	private GameObject objFemale2Genital;

	[DisabledGroup("男顔オブジェクト")]
	[SerializeField]
	private GameObject objMaleHead;

	[DisabledGroup("男性器オブジェクト")]
	[SerializeField]
	private GameObject objMaleGenital;

	[DisabledGroup("自分性器オブジェクト")]
	[SerializeField]
	private GameObject objGenitalSelf;

	private Transform LoopParent;

	private EyeNeck en;

	private FBSCtrlMouth mouthCtrl;

	private AnimatorStateInfo ai;

	private Transform NeckTrs;

	private Transform HeadTrs;

	private Transform[] EyeTrs;

	private Quaternion BackUpNeck;

	private Quaternion BackUpHead;

	private Quaternion[] BackUpEye;

	private float ChangeTimeNeck;

	private int NeckType;

	private float ChangeTimeEye;

	private int EyeType;

	private bool bFaceInfo;

	private float oldYuragi;

	private string OldAnimName;

	private ExcelData excelData;

	private EyeNeck info;

	private List<string> row = new List<string>();

	private string abName = "";

	private string assetName = "";

	public bool NowEndADV;

	private bool[] BlendInit = new bool[2];

	private Vector3[] BlendNeckRot = new Vector3[2];

	private Vector3 BlendEyeRot = Vector3.zero;

	public float NeckLerpSpeed = 100f;

	public float NeckPatternSpeed = 0.1f;

	public float EyeLerpSpeed = 100f;

	private bool changeMotion;

	private Transform[] getChild;

	private float[] nowleapSpeed = new float[2];

	public bool Init(ChaControl _male, int id)
	{
		Release();
		chaMale = _male;
		NeckTrs = chaMale.neckLookCtrl.neckLookScript.aBones[0].neckBone;
		HeadTrs = chaMale.neckLookCtrl.neckLookScript.aBones[1].neckBone;
		EyeTrs = new Transform[2]
		{
			chaMale.eyeLookCtrl.eyeLookScript.eyeObjs[0].eyeTransform,
			chaMale.eyeLookCtrl.eyeLookScript.eyeObjs[1].eyeTransform
		};
		BackUpNeck = NeckTrs.localRotation;
		BackUpHead = HeadTrs.localRotation;
		BackUpEye = new Quaternion[2]
		{
			EyeTrs[0].localRotation,
			EyeTrs[1].localRotation
		};
		objGenitalSelf = null;
		if ((bool)chaMale && (bool)chaMale.objBodyBone)
		{
			LoopParent = chaMale.objBodyBone.transform;
			if (strFemaleGenital != "")
			{
				objGenitalSelf = GetObjectName(LoopParent, strFemaleGenital);
			}
		}
		return true;
	}

	public void Release()
	{
		lstEyeNeck.Clear();
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
				if (!element.IsNullOrEmpty())
				{
					info.anim = element;
					info.openEye = int.Parse(row.GetElement(num2++));
					info.openMouth = int.Parse(row.GetElement(num2++));
					info.eyebrow = int.Parse(row.GetElement(num2++));
					info.eye = int.Parse(row.GetElement(num2++));
					info.mouth = int.Parse(row.GetElement(num2++));
					info.Neckbehaviour = int.Parse(row.GetElement(num2++));
					info.Eyebehaviour = int.Parse(row.GetElement(num2++));
					info.targetNeck = int.Parse(row.GetElement(num2++));
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
					info.NeckRot[0] = zero;
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
					info.NeckRot[1] = zero;
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
					info.HeadRot[0] = zero;
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
					info.HeadRot[1] = zero;
					info.targetEye = int.Parse(row.GetElement(num2++));
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
					info.EyeRot[0] = zero;
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
					info.EyeRot[1] = zero;
					lstEyeNeck.Add(info);
				}
			}
		}
		return true;
	}

	public bool SetPartner(GameObject _objFemale1Bone, GameObject _objFemale2Bone, GameObject _objMaleBone)
	{
		objFemale1Head = null;
		objFemale2Head = null;
		objMaleHead = null;
		objFemale1Genital = null;
		objFemale2Genital = null;
		objMaleGenital = null;
		if ((bool)_objFemale1Bone)
		{
			LoopParent = _objFemale1Bone.transform;
			if (strFemaleHead != "")
			{
				objFemale1Head = GetObjectName(LoopParent, strFemaleHead);
			}
			if (strFemaleGenital != "")
			{
				Transform transform = LoopParent.FindLoop(strFemaleGenital);
				objFemale1Genital = ((transform == null) ? null : transform.gameObject);
			}
		}
		if ((bool)_objFemale2Bone)
		{
			LoopParent = _objFemale2Bone.transform;
			if (strFemaleHead != "")
			{
				objFemale2Head = GetObjectName(LoopParent, strFemaleHead);
			}
			if (strFemaleGenital != "")
			{
				objFemale2Genital = GetObjectName(LoopParent, strFemaleGenital);
			}
		}
		if ((bool)_objMaleBone)
		{
			LoopParent = _objMaleBone.transform;
			if (strMaleHead != "")
			{
				objMaleHead = GetObjectName(LoopParent, strMaleHead);
			}
			if (strMaleGenital != "")
			{
				objMaleGenital = GetObjectName(LoopParent, strMaleGenital);
			}
		}
		return true;
	}

	public bool Proc(AnimatorStateInfo _ai)
	{
		ai = _ai;
		changeMotion = false;
		for (int i = 0; i < lstEyeNeck.Count; i++)
		{
			en = lstEyeNeck[i];
			if (_ai.IsName(en.anim))
			{
				chaMale.ChangeEyesOpenMax((float)en.openEye * 0.1f);
				mouthCtrl = chaMale.mouthCtrl;
				if (mouthCtrl != null)
				{
					mouthCtrl.OpenMin = (float)en.openMouth * 0.1f;
				}
				chaMale.ChangeEyebrowPtn(en.eyebrow);
				chaMale.ChangeEyesPtn(en.eye);
				chaMale.ChangeMouthPtn(en.mouth);
				SetEyesTarget(en.targetEye);
				SetEyeBehaviour(en.Eyebehaviour);
				SetNeckTarget(en.targetNeck);
				SetNeckBehaviour(en.Neckbehaviour);
				bFaceInfo = false;
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
					changeMotion = true;
				}
				OldAnimName = en.anim;
				return true;
			}
		}
		bFaceInfo = true;
		return true;
	}

	private void LateUpdate()
	{
		EyeNeckCalc();
	}

	public void EyeNeckCalc()
	{
		if (chaMale == null || NowEndADV || bFaceInfo)
		{
			return;
		}
		float num = hFlag.motions[0];
		for (int i = 0; i < lstEyeNeck.Count; i++)
		{
			en = lstEyeNeck[i];
			if (!ai.IsName(en.anim))
			{
				continue;
			}
			if (en.targetNeck == 7)
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
					BlendNeckRot[0].x = Mathf.Lerp(en.NeckRot[0].x, en.NeckRot[1].x, num);
					BlendNeckRot[0].y = Mathf.Lerp(en.NeckRot[0].y, en.NeckRot[1].y, num);
					BlendNeckRot[0].z = Mathf.Lerp(en.NeckRot[0].z, en.NeckRot[1].z, num);
					BlendNeckRot[1].x = Mathf.Lerp(en.HeadRot[0].x, en.HeadRot[1].x, num);
					BlendNeckRot[1].y = Mathf.Lerp(en.HeadRot[0].y, en.HeadRot[1].y, num);
					BlendNeckRot[1].z = Mathf.Lerp(en.HeadRot[0].z, en.HeadRot[1].z, num);
				}
				NeckCalc(BlendNeckRot[0], BlendNeckRot[1]);
			}
			if (en.targetEye == 7)
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
					BlendEyeRot.x = Mathf.Lerp(en.EyeRot[0].x, en.EyeRot[1].x, num);
					BlendEyeRot.y = Mathf.Lerp(en.EyeRot[0].y, en.EyeRot[1].y, num);
					BlendEyeRot.z = Mathf.Lerp(en.EyeRot[0].z, en.EyeRot[1].z, num);
				}
				EyeCalc(BlendEyeRot);
			}
			if (changeMotion)
			{
				changeMotion = false;
			}
			oldYuragi = num;
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
			chaMale.ChangeLookEyesTarget(1, objFemale1Head ? objFemale1Head.transform : null);
			break;
		case 2:
			chaMale.ChangeLookEyesTarget(1, objFemale1Genital ? objFemale1Genital.transform : null);
			break;
		case 3:
			chaMale.ChangeLookEyesTarget(1, objFemale2Head ? objFemale2Head.transform : null);
			break;
		case 4:
			chaMale.ChangeLookEyesTarget(1, objFemale2Genital ? objFemale2Genital.transform : null);
			break;
		case 5:
			chaMale.ChangeLookEyesTarget(1, objMaleHead ? objMaleHead.transform : null);
			break;
		case 6:
			chaMale.ChangeLookEyesTarget(1, objMaleGenital ? objMaleGenital.transform : null);
			break;
		case 8:
			chaMale.ChangeLookNeckTarget(1, objGenitalSelf ? objGenitalSelf.transform : null);
			break;
		default:
			chaMale.ChangeLookEyesTarget(0);
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
		switch (_tag)
		{
		case 1:
			chaMale.ChangeLookNeckTarget(1, objFemale1Head ? objFemale1Head.transform : null);
			break;
		case 2:
			chaMale.ChangeLookNeckTarget(1, objFemale1Genital ? objFemale1Genital.transform : null);
			break;
		case 3:
			chaMale.ChangeLookNeckTarget(1, objFemale2Head ? objFemale2Head.transform : null);
			break;
		case 4:
			chaMale.ChangeLookNeckTarget(1, objFemale2Genital ? objFemale2Genital.transform : null);
			break;
		case 5:
			chaMale.ChangeLookNeckTarget(1, objMaleHead ? objMaleHead.transform : null);
			break;
		case 6:
			chaMale.ChangeLookNeckTarget(1, objMaleGenital ? objMaleGenital.transform : null);
			break;
		case 8:
			chaMale.ChangeLookNeckTarget(1, objGenitalSelf ? objGenitalSelf.transform : null);
			break;
		default:
			chaMale.ChangeLookNeckTarget(0);
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
			chaMale.neckLookCtrl.neckLookScript.skipCalc = true;
			Observable.NextFrame().Subscribe(delegate
			{
				chaMale.neckLookCtrl.neckLookScript.skipCalc = false;
			});
		}
		NeckType = _tag;
		return true;
	}

	private bool SetNeckBehaviour(int _behaviour)
	{
		if (!chaMale.neckLookCtrl.enabled && _behaviour != 3)
		{
			chaMale.neckLookCtrl.neckLookScript.UpdateCall(0);
		}
		switch (_behaviour)
		{
		case 1:
			chaMale.ChangeLookNeckPtn(1);
			break;
		case 2:
			chaMale.ChangeLookNeckPtn(2);
			break;
		case 3:
			chaMale.ChangeLookNeckPtn(1);
			break;
		case 4:
			chaMale.ChangeLookNeckPtn(4);
			break;
		default:
			chaMale.ChangeLookNeckPtn(3);
			break;
		}
		if (_behaviour == 3)
		{
			chaMale.neckLookCtrl.enabled = false;
		}
		else
		{
			chaMale.neckLookCtrl.enabled = true;
		}
		return true;
	}

	private bool SetEyeBehaviour(int _behaviour)
	{
		switch (_behaviour)
		{
		case 1:
			chaMale.ChangeLookEyesPtn(1);
			break;
		case 2:
			chaMale.ChangeLookEyesPtn(2);
			break;
		case 3:
			chaMale.ChangeLookEyesPtn(1);
			break;
		case 4:
			chaMale.ChangeLookEyesPtn(3);
			break;
		default:
			chaMale.ChangeLookEyesPtn(0);
			break;
		}
		if (_behaviour == 3)
		{
			chaMale.eyeLookCtrl.enabled = false;
		}
		else
		{
			chaMale.eyeLookCtrl.enabled = true;
		}
		return true;
	}

	private void NeckCalc(Vector3 targetNeckRot, Vector3 targetHeadRot)
	{
		float deltaTime = Time.deltaTime;
		ChangeTimeNeck = Mathf.Clamp(ChangeTimeNeck + deltaTime, 0f, NeckPatternSpeed);
		float num = Mathf.InverseLerp(0f, NeckPatternSpeed, ChangeTimeNeck);
		if (chaMale.neckLookCtrl.neckLookScript.changeTypeLerpCurve != null)
		{
			num = chaMale.neckLookCtrl.neckLookScript.changeTypeLerpCurve.Evaluate(num);
		}
		nowleapSpeed[0] = Mathf.Clamp01(NeckLerpSpeed * deltaTime);
		Quaternion b = Quaternion.Slerp(BackUpNeck, Quaternion.Euler(targetNeckRot), nowleapSpeed[0]);
		Quaternion b2 = Quaternion.Slerp(NeckTrs.localRotation, b, chaMale.neckLookCtrl.neckLookScript.calcLerp);
		b2 = Quaternion.Slerp(BackUpNeck, b2, num);
		Quaternion backUpNeck = (NeckTrs.localRotation = b2);
		BackUpNeck = backUpNeck;
		b = Quaternion.Slerp(BackUpHead, Quaternion.Euler(targetHeadRot), nowleapSpeed[0]);
		b2 = Quaternion.Slerp(HeadTrs.localRotation, b, chaMale.neckLookCtrl.neckLookScript.calcLerp);
		b2 = Quaternion.Slerp(BackUpHead, b2, num);
		backUpNeck = (HeadTrs.localRotation = b2);
		BackUpHead = backUpNeck;
		chaMale.neckLookCtrl.neckLookScript.SetBoneFixAngle(0, NeckTrs.localRotation);
		chaMale.neckLookCtrl.neckLookScript.SetBoneFixAngle(1, HeadTrs.localRotation);
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
		chaMale.eyeLookCtrl.eyeLookScript.SetAngleHV(0, EyeTrs[1].localRotation.eulerAngles.y, EyeTrs[1].localRotation.eulerAngles.x);
		chaMale.eyeLookCtrl.eyeLookScript.SetAngleHV(1, EyeTrs[1].localRotation.eulerAngles.y, EyeTrs[1].localRotation.eulerAngles.x);
	}
}
