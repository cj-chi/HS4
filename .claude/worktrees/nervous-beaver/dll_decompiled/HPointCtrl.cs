using System.Collections.Generic;
using Manager;
using UnityEngine;

public class HPointCtrl : Singleton<HPointCtrl>
{
	[SerializeField]
	private HScene hScene;

	private HSceneManager hSceneManager;

	private Camera Cam;

	[SerializeField]
	private HPointList hPointList;

	[SerializeField]
	private HSceneFlagCtrl ctrlFlag;

	public List<HScene.AnimationListInfo>[] lstAnimInfo = new List<HScene.AnimationListInfo>[7];

	public HPointList HPointList
	{
		get
		{
			return hPointList;
		}
		set
		{
			hPointList = value;
		}
	}

	public void InitHPoint()
	{
		hSceneManager = Singleton<HSceneManager>.Instance;
		lstAnimInfo = HSceneManager.HResourceTables.lstAnimInfo;
	}

	public void MovePoint(int MoveKind)
	{
		if (hPointList == null)
		{
			return;
		}
		int nPlace = ctrlFlag.nPlace;
		HScene.AnimationListInfo nowAnimationInfo = ctrlFlag.nowAnimationInfo;
		int num = nowAnimationInfo.ActionCtrl.Item1;
		if (num == 6)
		{
			num = 5;
		}
		if (ctrlFlag.HPointID < 0)
		{
			ctrlFlag.HPointID = 0;
		}
		if (!hPointList.lst.ContainsKey(nPlace) || hPointList.lst[nPlace] == null || hPointList.lst[nPlace].Count <= ctrlFlag.HPointID)
		{
			return;
		}
		int num2 = -1;
		for (int i = 0; i < nowAnimationInfo.nPositons.Count; i++)
		{
			if (nPlace == nowAnimationInfo.nPositons[i])
			{
				num2 = i;
				break;
			}
		}
		hPointList.lst[nPlace][ctrlFlag.HPointID].NowUsing = false;
		if (nowAnimationInfo.nPositons.Count == 1)
		{
			switch (MoveKind)
			{
			case 0:
			{
				int hPointID2 = ctrlFlag.HPointID;
				ctrlFlag.HPointID = GlobalMethod.ValLoop(ctrlFlag.HPointID - 1, hPointList.lst[nPlace].Count);
				while (hPointList.lst[nPlace][ctrlFlag.HPointID].Data.notMotion[num].motionID.Contains(nowAnimationInfo.id))
				{
					ctrlFlag.HPointID = GlobalMethod.ValLoop(ctrlFlag.HPointID - 1, hPointList.lst[nPlace].Count);
					if (ctrlFlag.HPointID == hPointID2)
					{
						break;
					}
				}
				break;
			}
			case 1:
			{
				int hPointID = ctrlFlag.HPointID;
				ctrlFlag.HPointID = GlobalMethod.ValLoop(ctrlFlag.HPointID + 1, hPointList.lst[nPlace].Count);
				while (hPointList.lst[nPlace][ctrlFlag.HPointID].Data.notMotion[num].motionID.Contains(nowAnimationInfo.id))
				{
					ctrlFlag.HPointID = GlobalMethod.ValLoop(ctrlFlag.HPointID + 1, hPointList.lst[nPlace].Count);
					if (ctrlFlag.HPointID == hPointID)
					{
						break;
					}
				}
				break;
			}
			}
		}
		else
		{
			switch (MoveKind)
			{
			case 0:
			{
				bool flag2 = false;
				int num4 = num2;
				do
				{
					if (ctrlFlag.HPointID == 0)
					{
						do
						{
							num2 = GlobalMethod.ValLoop(num2 - 1, nowAnimationInfo.nPositons.Count);
							if (hPointList.lst.ContainsKey(nowAnimationInfo.nPositons[num2]))
							{
								ctrlFlag.HPointID = hPointList.lst[nowAnimationInfo.nPositons[num2]].Count - 1;
							}
							if (num4 == num2)
							{
								flag2 = true;
								break;
							}
						}
						while (!hPointList.lst.ContainsKey(nowAnimationInfo.nPositons[num2]) || ctrlFlag.HPointID < 0);
					}
					else
					{
						ctrlFlag.HPointID--;
					}
				}
				while (hPointList.lst[nowAnimationInfo.nPositons[num2]][ctrlFlag.HPointID].Data.notMotion[num].motionID.Contains(nowAnimationInfo.id) && !flag2);
				break;
			}
			case 1:
			{
				bool flag = false;
				int num3 = num2;
				do
				{
					if (ctrlFlag.HPointID == hPointList.lst[nowAnimationInfo.nPositons[num2]].Count - 1)
					{
						do
						{
							num2 = GlobalMethod.ValLoop(num2 + 1, nowAnimationInfo.nPositons.Count);
							ctrlFlag.HPointID = 0;
							if (num3 == num2)
							{
								flag = true;
								break;
							}
						}
						while (!hPointList.lst.ContainsKey(nowAnimationInfo.nPositons[num2]) || hPointList.lst[nowAnimationInfo.nPositons[num2]].Count == 0);
					}
					else
					{
						ctrlFlag.HPointID++;
					}
				}
				while (hPointList.lst[nowAnimationInfo.nPositons[num2]][ctrlFlag.HPointID].Data.notMotion[num].motionID.Contains(nowAnimationInfo.id) && !flag);
				break;
			}
			}
		}
		nPlace = (ctrlFlag.nPlace = nowAnimationInfo.nPositons[num2]);
		Vector3 pos = Vector3.zero;
		Vector3 rot = Vector3.zero;
		if (num2 >= 0 && nowAnimationInfo.lstOffset.Count > num2 && !nowAnimationInfo.lstOffset[num2].IsNullOrEmpty())
		{
			hScene.LoadMoveOffset(nowAnimationInfo.lstOffset[num2], out pos, out rot);
		}
		hScene.SetMovePositionPoint(hPointList.lst[nPlace][ctrlFlag.HPointID].transform, pos, rot);
		ctrlFlag.nowHPoint = hPointList.lst[nPlace][ctrlFlag.HPointID];
		hPointList.lst[nPlace][ctrlFlag.HPointID].NowUsing = true;
	}

	public void EndProc()
	{
		foreach (KeyValuePair<int, List<HPoint>> item in hPointList.lst)
		{
			foreach (HPoint item2 in item.Value)
			{
				item2.ReInit();
			}
		}
		HPoint.EndSetShowerMasturbation();
	}
}
