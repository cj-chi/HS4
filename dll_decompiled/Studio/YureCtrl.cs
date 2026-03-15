using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using AIProject;
using UnityEngine;

namespace Studio;

[DefaultExecutionOrder(-1)]
public class YureCtrl : MonoBehaviour
{
	[Serializable]
	public struct BreastShapeInfo
	{
		public bool[] breast;

		public bool nip;

		public void MemberInit()
		{
			breast = new bool[7] { true, true, true, true, true, true, true };
			nip = true;
		}
	}

	[Serializable]
	public class Info
	{
		public string nameAnimation = "";

		public bool[] aIsActive = new bool[4];

		public BreastShapeInfo[] aBreastShape = new BreastShapeInfo[2];

		public int nFemale;
	}

	public List<Info> lstInfo = new List<Info>();

	[Tooltip("動いているかの確認用")]
	public bool[] aIsActive = new bool[4] { true, true, true, true };

	[Tooltip("動いているかの確認用")]
	public BreastShapeInfo[] aBreastShape = new BreastShapeInfo[2];

	private bool[] aYureEnableActive = new bool[4] { true, true, true, true };

	private BreastShapeInfo[] aBreastShapeEnable = new BreastShapeInfo[2];

	private ChaControl _chaControl;

	public int FemaleID { get; private set; }

	public bool IsInit { get; private set; }

	private OCIChar OCIChar { get; set; }

	private ChaControl ChaControl => _chaControl ?? (_chaControl = OCIChar?.charInfo);

	private void LateUpdate()
	{
		if (IsInit && ChaControl != null)
		{
			Proc(ChaControl.getAnimatorStateInfo(0));
		}
	}

	public void Init(OCIChar _ocic)
	{
		OCIChar = _ocic;
		for (int i = 0; i < 2; i++)
		{
			aBreastShape[i].MemberInit();
			aBreastShapeEnable[i].MemberInit();
		}
	}

	public bool Load(string _bundle, string _file, int _motionId, int _femaleID)
	{
		IsInit = false;
		ResetShape();
		if (!GlobalMethod.AssetFileExist(_bundle, _file))
		{
			return false;
		}
		FemaleID = _femaleID;
		lstInfo.Clear();
		ExcelData excelData = CommonLib.LoadAsset<ExcelData>(_bundle, _file);
		string[] array = _file.Split('_');
		bool flag = new string[2] { "ail", "ai3p" }.Contains(array[0]);
		int[] array2 = new int[7] { 2, 3, 4, 5, 6, 7, 8 };
		int work = 0;
		foreach (ExcelData.Param item in excelData.list.Where((ExcelData.Param v) => int.TryParse(v.list.SafeGet(0), out work) && work == _motionId))
		{
			int num = 1;
			Info info = new Info();
			List<string> list = item.list;
			int result = 0;
			if (flag && !int.TryParse(list.GetElement(num++), out result))
			{
				result = 0;
			}
			info.nFemale = result;
			info.nameAnimation = list.GetElement(num++);
			info.aIsActive[0] = list.GetElement(num++) == "1";
			info.aBreastShape[0].MemberInit();
			for (int num2 = 0; num2 < array2.Length; num2++)
			{
				info.aBreastShape[0].breast[num2] = list.GetElement(num++) == "1";
			}
			info.aBreastShape[0].nip = list.GetElement(num++) == "1";
			info.aIsActive[1] = list.GetElement(num++) == "1";
			info.aBreastShape[1].MemberInit();
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				info.aBreastShape[1].breast[num3] = list.GetElement(num++) == "1";
			}
			info.aBreastShape[1].nip = list.GetElement(num++) == "1";
			info.aIsActive[2] = list.GetElement(num++) == "1";
			info.aIsActive[3] = list.GetElement(num++) == "1";
			lstInfo.Add(info);
		}
		IsInit = true;
		return true;
	}

	public bool Proc(AnimatorStateInfo _ai)
	{
		if (!IsInit)
		{
			return false;
		}
		Info info = null;
		if (lstInfo != null)
		{
			for (int i = 0; i < lstInfo.Count; i++)
			{
				if (_ai.IsName(lstInfo[i].nameAnimation) && lstInfo[i].nFemale == FemaleID)
				{
					info = lstInfo[i];
					break;
				}
			}
		}
		if (info != null)
		{
			Active(info.aIsActive);
			Shape(info.aBreastShape);
			return true;
		}
		Active(aYureEnableActive);
		Shape(aBreastShapeEnable);
		return false;
	}

	private void Active(bool[] _aIsActive)
	{
		for (int i = 0; i < aIsActive.Length; i++)
		{
			if (aIsActive[i] != _aIsActive[i])
			{
				switch (i)
				{
				case 0:
					OCIChar.DynamicAnimeBustL = _aIsActive[i];
					break;
				case 1:
					OCIChar.DynamicAnimeBustR = _aIsActive[i];
					break;
				default:
					OCIChar.EnableDynamicBonesBustAndHip(_aIsActive[i], i);
					break;
				}
				aIsActive[i] = _aIsActive[i];
			}
		}
	}

	private void Shape(BreastShapeInfo[] _shapeInfo)
	{
		for (int i = 0; i < 2; i++)
		{
			int lR = i;
			BreastShapeInfo breastShapeInfo = _shapeInfo[i];
			BreastShapeInfo breastShapeInfo2 = aBreastShape[i];
			if (breastShapeInfo.breast != breastShapeInfo2.breast)
			{
				for (int j = 0; j < ChaFileDefine.cf_BustShapeMaskID.Length - 1; j++)
				{
					int num = j;
					if (breastShapeInfo.breast[num] != breastShapeInfo2.breast[num])
					{
						if (breastShapeInfo.breast[num])
						{
							ChaControl.DisableShapeBodyID(lR, num, disable: false);
						}
						else
						{
							ChaControl.DisableShapeBodyID(lR, num, disable: true);
						}
					}
				}
				breastShapeInfo2.breast = breastShapeInfo.breast;
			}
			if (breastShapeInfo.nip != breastShapeInfo2.nip)
			{
				if (breastShapeInfo.nip)
				{
					ChaControl.DisableShapeBodyID(lR, 7, disable: false);
				}
				else
				{
					ChaControl.DisableShapeBodyID(lR, 7, disable: true);
				}
				breastShapeInfo2.nip = breastShapeInfo.nip;
			}
			aBreastShape[i] = breastShapeInfo2;
		}
	}

	public void ResetShape(bool _dynamicBone = true)
	{
		if (ChaControl == null)
		{
			return;
		}
		for (int i = 0; i < ChaFileDefine.cf_BustShapeMaskID.Length; i++)
		{
			ChaControl.DisableShapeBodyID(2, i, disable: false);
		}
		for (int j = 0; j < 2; j++)
		{
			aBreastShape[j].MemberInit();
		}
		if (_dynamicBone)
		{
			for (int k = 0; k < aIsActive.Length; k++)
			{
				aIsActive[k] = true;
			}
			OCIChar.DynamicAnimeBustL = true;
			OCIChar.DynamicAnimeBustR = true;
			OCIChar.EnableDynamicBonesBustAndHip(_enable: true, 2);
			OCIChar.EnableDynamicBonesBustAndHip(_enable: true, 3);
		}
		IsInit = false;
		if (lstInfo != null)
		{
			lstInfo.Clear();
		}
	}
}
