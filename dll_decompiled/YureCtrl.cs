using System;
using System.Collections.Generic;
using AIChara;
using AIProject;
using Manager;
using UniRx;
using UnityEngine;

[Serializable]
public class YureCtrl
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
		private int? _nameHash;

		public string nameAnimation = "";

		public bool[] aIsActive = new bool[4];

		public BreastShapeInfo[] aBreastShape = new BreastShapeInfo[2];

		public int nFemale;

		public int nameHash
		{
			get
			{
				int? num = _nameHash;
				if (!num.HasValue)
				{
					int? num2 = (_nameHash = Animator.StringToHash(nameAnimation));
					return num2.Value;
				}
				return num.GetValueOrDefault();
			}
		}
	}

	public int femaleID;

	[Tooltip("動いているかの確認用")]
	public bool[] aIsActive = new bool[4] { true, true, true, true };

	[Tooltip("動いているかの確認用")]
	public BreastShapeInfo[] aBreastShape = new BreastShapeInfo[2];

	public List<Info> lstInfo { get; } = new List<Info>();

	public ChaControl chaFemale { get; private set; }

	public bool isInit { get; private set; }

	public Info info { get; private set; }

	private bool[] aYureEnableActive { get; } = new bool[4] { true, true, true, true };

	private BreastShapeInfo[] aBreastShapeEnable { get; } = new BreastShapeInfo[2];

	public void SetChaControl(ChaControl chaControl)
	{
		chaFemale = chaControl;
	}

	public void Init()
	{
		for (int i = 0; i < 2; i++)
		{
			aBreastShape[i].MemberInit();
			aBreastShapeEnable[i].MemberInit();
		}
		Release();
	}

	public void Release()
	{
		lstInfo.Clear();
		Set(null);
	}

	public void ResetShape()
	{
		if (!(chaFemale == null))
		{
			for (int i = 0; i < ChaFileDefine.cf_BustShapeMaskID.Length; i++)
			{
				chaFemale.DisableShapeBodyID(2, i, disable: false);
			}
			for (int j = 0; j < aIsActive.Length; j++)
			{
				aIsActive[j] = true;
				chaFemale.playDynamicBoneBust((ChaControlDefine.DynamicBoneKind)j, play: true);
			}
		}
	}

	public void Set(Info info)
	{
		this.info = info;
		isInit = info != null;
		if (isInit)
		{
			Active(info.aIsActive);
			Shape(info.aBreastShape);
		}
		else
		{
			Active(aYureEnableActive);
			Shape(aBreastShapeEnable);
		}
	}

	public void Proc(string stateName)
	{
		Proc(Animator.StringToHash(stateName));
	}

	public void Proc(int nameHash)
	{
		Set(lstInfo.Find((Info l) => l.nameHash == nameHash && l.nFemale == femaleID));
	}

	private void Active(bool[] _aIsActive)
	{
		for (int i = 0; i < aIsActive.Length; i++)
		{
			if (aIsActive[i] != _aIsActive[i])
			{
				chaFemale.playDynamicBoneBust((ChaControlDefine.DynamicBoneKind)i, _aIsActive[i]);
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
					if (breastShapeInfo.breast[j] != breastShapeInfo2.breast[j])
					{
						chaFemale.DisableShapeBodyID(lR, j, !breastShapeInfo.breast[j]);
					}
				}
				breastShapeInfo2.breast = breastShapeInfo.breast;
			}
			if (breastShapeInfo.nip != breastShapeInfo2.nip)
			{
				chaFemale.DisableShapeBodyID(lR, 7, !breastShapeInfo.nip);
				breastShapeInfo2.nip = breastShapeInfo.nip;
			}
			aBreastShape[i] = breastShapeInfo2;
		}
	}

	public bool Load(int _motionId, int category)
	{
		isInit = false;
		for (int i = 0; i < aIsActive.Length; i++)
		{
			aIsActive[i] = true;
		}
		for (int j = 0; j < 2; j++)
		{
			aBreastShape[j].MemberInit();
		}
		if ((bool)chaFemale)
		{
			for (int k = 0; k < aIsActive.Length; k++)
			{
				chaFemale.playDynamicBoneBust((ChaControlDefine.DynamicBoneKind)k, play: true);
			}
			Observable.EveryEndOfFrame().Take(1).Subscribe(delegate
			{
				chaFemale.GetDynamicBoneBustAndHip(ChaControlDefine.DynamicBoneKind.BreastL).enabled = false;
				chaFemale.GetDynamicBoneBustAndHip(ChaControlDefine.DynamicBoneKind.BreastR).enabled = false;
				chaFemale.GetDynamicBoneBustAndHip(ChaControlDefine.DynamicBoneKind.HipL).enabled = false;
				chaFemale.GetDynamicBoneBustAndHip(ChaControlDefine.DynamicBoneKind.HipR).enabled = false;
			});
			for (int num = 0; num < ChaFileDefine.cf_BustShapeMaskID.Length; num++)
			{
				chaFemale.DisableShapeBodyID(2, num, disable: false);
			}
		}
		List<Info> value = null;
		if (HSceneManager.HResourceTables.DicDicYure.TryGetValue(category, out var value2))
		{
			value2.TryGetValue(_motionId, out value);
		}
		lstInfo.Clear();
		if (value != null)
		{
			lstInfo.AddRange(value);
		}
		isInit = true;
		return true;
	}

	public bool Load(string _assetPath, string _asset)
	{
		isInit = false;
		for (int i = 0; i < aIsActive.Length; i++)
		{
			aIsActive[i] = true;
		}
		for (int j = 0; j < 2; j++)
		{
			aBreastShape[j].MemberInit();
		}
		if ((bool)chaFemale)
		{
			for (int k = 0; k < aIsActive.Length; k++)
			{
				chaFemale.playDynamicBoneBust((ChaControlDefine.DynamicBoneKind)k, play: true);
			}
			for (int l = 0; l < ChaFileDefine.cf_BustShapeMaskID.Length; l++)
			{
				int id = l;
				chaFemale.DisableShapeBodyID(2, id, disable: false);
			}
		}
		lstInfo.Clear();
		ExcelData excelData = CommonLib.LoadAsset<ExcelData>(_assetPath, _asset);
		if (excelData == null)
		{
			return false;
		}
		int num = 1;
		while (num < excelData.MaxCell)
		{
			List<string> list = excelData.list[num++].list;
			int num2 = 0;
			int result = -1;
			if (int.TryParse(list.GetElement(num2++), out result))
			{
				Info info = new Info
				{
					nFemale = 0,
					nameAnimation = list.GetElement(num2++)
				};
				info.aIsActive[0] = list.GetElement(num2++) == "1";
				info.aBreastShape[0].MemberInit();
				for (int m = 0; m < info.aBreastShape[0].breast.Length; m++)
				{
					info.aBreastShape[0].breast[m] = list.GetElement(num2++) == "1";
				}
				info.aBreastShape[0].nip = list.GetElement(num2++) == "1";
				info.aIsActive[1] = list.GetElement(num2++) == "1";
				info.aBreastShape[1].MemberInit();
				for (int n = 0; n < info.aBreastShape[1].breast.Length; n++)
				{
					info.aBreastShape[1].breast[n] = list.GetElement(num2++) == "1";
				}
				info.aBreastShape[1].nip = list.GetElement(num2++) == "1";
				info.aIsActive[2] = list.GetElement(num2++) == "1";
				info.aIsActive[3] = list.GetElement(num2++) == "1";
				lstInfo.Add(info);
			}
		}
		isInit = true;
		return true;
	}
}
