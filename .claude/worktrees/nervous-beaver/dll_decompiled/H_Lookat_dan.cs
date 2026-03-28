using System;
using System.Collections.Generic;
using System.Text;
using AIChara;
using AIProject;
using IllusionUtility.GetUtility;
using Manager;
using UnityEngine;

public class H_Lookat_dan : MonoBehaviour
{
	[Serializable]
	public struct ShapeInfo
	{
		public int shape;

		public Vector3 minPos;

		public Vector3 middlePos;

		public Vector3 maxPos;

		public bool bUse;
	}

	[Serializable]
	public class MotionLookAtList
	{
		public string strMotion;

		public int numFemale;

		public string strLookAtNull;

		public bool bTopStick;

		public bool bManual;

		public ShapeInfo[] lstShape = new ShapeInfo[10];

		public MotionLookAtList()
		{
			for (int i = 0; i < lstShape.Length; i++)
			{
				lstShape[i].bUse = false;
			}
		}
	}

	private ChaControl[] females;

	private ChaControl male;

	private StringBuilder assetName = new StringBuilder();

	public List<MotionLookAtList> lstLookAt = new List<MotionLookAtList>();

	public string nameBaseBone = "cm_J_dan101_00";

	public string nameTop = "cm_J_dan109_00";

	public string nameRefBone = "";

	public string strPlayMotion = "";

	public Transform transLookAtNull;

	public bool bTopStick;

	public bool bManual;

	public ShapeInfo[] lstShape = new ShapeInfo[10];

	public int numFemale;

	[SerializeField]
	public H_LookAtDan_Info dan_Info = new H_LookAtDan_Info();

	public (GameObject, Transform) DanBase;

	public GameObject DanTop;

	public GameObject DanBaseR;

	public void DankonInit(ChaControl _male, ChaControl[] _females)
	{
		females = _females;
		male = _male;
		if (!(male.objBodyBone == null))
		{
			Transform transform = male.objBodyBone.transform;
			if (transform != null)
			{
				Transform transform2 = transform.FindLoop(nameBaseBone);
				DanBase = (transform2.gameObject, transform2);
				DanTop = transform.FindLoop(nameTop).gameObject;
				DanBaseR = (nameRefBone.IsNullOrEmpty() ? DanBase.Item2.parent.gameObject : transform.FindLoop(nameRefBone).gameObject);
				dan_Info.SetUpAxisTransform(DanBaseR.transform);
			}
			dan_Info.SetLookAtTransform(DanBase.Item2);
		}
	}

	public bool LoadList(string _pathFile, int mode = -1, int maleID = -1)
	{
		Release();
		if (_pathFile == "")
		{
			return false;
		}
		assetName.Append(_pathFile);
		if (GlobalMethod.StartsWith(_pathFile, "ai"))
		{
			assetName.Replace("_m_", "_");
		}
		else if (GlobalMethod.StartsWith(_pathFile, "h2"))
		{
			if (mode == 6)
			{
				assetName.Replace("_m1_", "_");
				assetName.Append($"_{maleID + 1:00}");
			}
			else
			{
				assetName.Replace("_m_", "_");
			}
		}
		if (!HSceneManager.HResourceTables.DicLstLookAtDan.TryGetValue(assetName.ToString(), out var value))
		{
			value = new List<MotionLookAtList>();
		}
		lstLookAt = new List<MotionLookAtList>(value);
		if (lstLookAt.Count != 0)
		{
			setInfo(lstLookAt[0]);
		}
		return true;
	}

	public void LoadDankonList(string _pathAssetFolder, string _pathFile)
	{
		lstLookAt.Clear();
		List<string> list = new List<string>();
		ShapeInfo shapeInfo = default(ShapeInfo);
		List<ExcelData> list2 = GlobalMethod.LoadAllFolder<ExcelData>(_pathAssetFolder, _pathFile);
		for (int i = 0; i < list2.Count; i++)
		{
			int num = 3;
			while (num < list2[i].MaxCell)
			{
				list = list2[i].list[num++].list;
				MotionLookAtList motionLookAtList = new MotionLookAtList();
				int num2 = 0;
				motionLookAtList.strMotion = list.GetElement(num2++);
				if (int.TryParse(list.GetElement(num2++), out var result))
				{
					motionLookAtList.numFemale = result;
				}
				motionLookAtList.strLookAtNull = list.GetElement(num2++);
				motionLookAtList.bTopStick = list.GetElement(num2++) == "1";
				motionLookAtList.bManual = list.GetElement(num2++) == "1";
				int num3 = 0;
				for (int j = num2; j < list.Count; j += 10)
				{
					int num4 = 0;
					if (!int.TryParse(list.GetElement(j + num4++), out var result2))
					{
						break;
					}
					shapeInfo.shape = result2;
					shapeInfo.minPos = new Vector3(float.Parse(list.GetElement(j + num4++)), float.Parse(list.GetElement(j + num4++)), float.Parse(list.GetElement(j + num4++)));
					shapeInfo.middlePos = new Vector3(float.Parse(list.GetElement(j + num4++)), float.Parse(list.GetElement(j + num4++)), float.Parse(list.GetElement(j + num4++)));
					shapeInfo.maxPos = new Vector3(float.Parse(list.GetElement(j + num4++)), float.Parse(list.GetElement(j + num4++)), float.Parse(list.GetElement(j + num4++)));
					shapeInfo.bUse = true;
					motionLookAtList.lstShape[num3++] = shapeInfo;
				}
				lstLookAt.Add(motionLookAtList);
			}
		}
	}

	public void Release()
	{
		lstLookAt.Clear();
		assetName.Clear();
		strPlayMotion = "";
		transLookAtNull = null;
		bTopStick = false;
		dan_Info.SetTargetTransform(null);
	}

	private void LateUpdate()
	{
		if (male == null || females == null || male.objBodyBone == null || females[0].objBodyBone == null)
		{
			return;
		}
		setLookAt();
		if (lstShape != null && transLookAtNull != null)
		{
			Vector3 position = transLookAtNull.position;
			for (int i = 0; i < lstShape.Length; i++)
			{
				if (lstShape[i].bUse)
				{
					float shapeBodyValue = females[numFemale].GetShapeBodyValue(lstShape[i].shape);
					Vector3 direction = ((shapeBodyValue >= 0.5f) ? Vector3.Lerp(lstShape[i].middlePos, lstShape[i].maxPos, Mathf.InverseLerp(0.5f, 1f, shapeBodyValue)) : Vector3.Lerp(lstShape[i].minPos, lstShape[i].middlePos, Mathf.InverseLerp(0f, 0.5f, shapeBodyValue)));
					direction = transLookAtNull.TransformDirection(direction);
					position += direction;
				}
			}
			transLookAtNull.position = position;
		}
		LookAtProc(this);
	}

	private bool setLookAt()
	{
		AnimatorStateInfo animatorStateInfo = females[0].getAnimatorStateInfo(0);
		if (animatorStateInfo.IsName(strPlayMotion))
		{
			return true;
		}
		foreach (MotionLookAtList item in lstLookAt)
		{
			if (animatorStateInfo.IsName(item.strMotion))
			{
				setInfo(item);
				break;
			}
		}
		return true;
	}

	private bool setInfo(MotionLookAtList _list)
	{
		if (_list == null)
		{
			return false;
		}
		if (females[_list.numFemale].objBodyBone == null)
		{
			transLookAtNull = null;
			lstShape = null;
			return false;
		}
		strPlayMotion = _list.strMotion;
		numFemale = _list.numFemale;
		if (_list.strLookAtNull == "")
		{
			transLookAtNull = null;
			lstShape = null;
		}
		else
		{
			Transform transform = females[_list.numFemale].objBodyBone.transform.FindLoop(_list.strLookAtNull);
			transLookAtNull = ((transform != null) ? transform.transform : null);
			lstShape = _list.lstShape;
		}
		bTopStick = _list.bTopStick;
		bManual = _list.bManual;
		Transform item = DanBase.Item2;
		dan_Info.SetTargetTransform(transLookAtNull);
		dan_Info.SetOldRotation((item != null) ? item.rotation : Quaternion.identity);
		return true;
	}

	public bool LookAtProc(H_Lookat_dan h_Lookat_Dan)
	{
		if (h_Lookat_Dan.DanBase.Item1 == null)
		{
			return false;
		}
		if (h_Lookat_Dan.transLookAtNull == null)
		{
			return false;
		}
		if (!h_Lookat_Dan.bManual)
		{
			h_Lookat_Dan.DanBase.Item2.LookAt(h_Lookat_Dan.transLookAtNull);
		}
		else
		{
			h_Lookat_Dan.dan_Info.ManualCalc();
		}
		if (h_Lookat_Dan.bTopStick && h_Lookat_Dan.DanTop != null)
		{
			h_Lookat_Dan.DanTop.transform.position = h_Lookat_Dan.transLookAtNull.position;
		}
		return true;
	}
}
