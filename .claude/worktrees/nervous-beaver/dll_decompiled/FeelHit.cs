using System.Collections.Generic;
using AIChara;
using Manager;
using UnityEngine;

public class FeelHit
{
	public struct FeelHitInfo
	{
		public Vector2 area;

		public float rate;
	}

	public class FeelInfo
	{
		public List<Vector2> lstHit = new List<Vector2>();

		public List<FeelHitInfo> lstHitArea = new List<FeelHitInfo>();

		public List<float> lstChangeTime = new List<float>();

		public List<float> lstChangeDeltaTime = new List<float>();

		private Vector2 tmpv;

		public void CreateHit()
		{
			foreach (FeelHitInfo item in lstHitArea)
			{
				tmpv = Vector2.zero;
				tmpv.x = Random.Range(5, 95 - (int)item.rate);
				tmpv.y = tmpv.x + item.rate;
				lstHit.Add(tmpv);
			}
		}

		public bool ChangeHit(int _state)
		{
			if (_state >= lstChangeTime.Count || _state >= lstChangeDeltaTime.Count || _state >= lstHitArea.Count || _state >= lstHit.Count)
			{
				return false;
			}
			lstChangeDeltaTime[_state] += Time.deltaTime;
			if (lstChangeDeltaTime[_state] >= lstChangeTime[_state])
			{
				lstChangeTime[_state] = Random.Range(lstHitArea[_state].area.x, lstHitArea[_state].area.y);
				lstChangeDeltaTime[_state] = 0f;
				Vector2 value = lstHit[_state];
				value.x = Random.Range(5, 95 - (int)lstHitArea[_state].rate);
				value.y = value.x + lstHitArea[_state].rate;
				lstHit[_state] = value;
			}
			return true;
		}

		public void InitTime()
		{
			lstChangeTime.Clear();
			lstChangeDeltaTime.Clear();
			foreach (FeelHitInfo item2 in lstHitArea)
			{
				float item = Random.Range(item2.area.x, item2.area.y);
				lstChangeTime.Add(item);
				lstChangeDeltaTime.Add(0f);
			}
		}

		public Vector2 Get(int _state)
		{
			if (_state >= lstHit.Count)
			{
				return new Vector2(1.1f, 1.1f);
			}
			return lstHit[_state] * 0.01f;
		}
	}

	private List<FeelInfo>[] lstHitInfo = new List<FeelInfo>[2];

	private ChaFileParameter2 femaleParam;

	private HScene.AnimationListInfo AnimInfo;

	private int[] Resist = new int[3];

	private int EventNo = -1;

	public void FeelHitInit(int _personality = 0)
	{
		if (!HSceneManager.HResourceTables.DicLstHitInfo.TryGetValue(_personality, out lstHitInfo))
		{
			lstHitInfo = new List<FeelInfo>[2]
			{
				new List<FeelInfo>(),
				new List<FeelInfo>()
			};
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			foreach (FeelInfo item in lstHitInfo[i])
			{
				item.CreateHit();
				item.InitTime();
			}
		}
	}

	public void SetFeelCha(ChaControl _female)
	{
		femaleParam = _female.fileParam2;
		Resist[0] = _female.fileGameInfo2.resistH;
		Resist[1] = _female.fileGameInfo2.resistAnal;
		Resist[2] = _female.fileGameInfo2.resistPain;
	}

	public void SetFeelAnimInfo(HScene.AnimationListInfo _info)
	{
		AnimInfo = _info;
	}

	public void SetFeelEventNo()
	{
		EventNo = Singleton<Game>.Instance.eventNo;
	}

	public bool isHit(int _state, int _loop, float _power, int _resist)
	{
		Vector2 hitArea = GetHitArea(_state, _loop, _resist);
		return GlobalMethod.RangeOn(_power, hitArea.x, hitArea.y);
	}

	public Vector2 GetHitArea(int _state, int _loop, int _resist)
	{
		if (_state > 5)
		{
			return new Vector2(-1f, -1f);
		}
		if (EventNo == 50 || EventNo == 51 || EventNo == 52 || EventNo == 53 || EventNo == 54 || EventNo == 55)
		{
			return lstHitInfo[1][_state].Get(_loop);
		}
		if (_loop == 1 && _state != 5)
		{
			switch (_resist)
			{
			case 2:
				if (Resist[2] < 100 && femaleParam.hAttribute != 3)
				{
					return new Vector2(-1f, -1f);
				}
				break;
			case 1:
			{
				bool flag2 = femaleParam.hAttribute == 5;
				flag2 |= femaleParam.hAttribute == 1;
				if (Resist[1] < 100 && !flag2)
				{
					return new Vector2(-1f, -1f);
				}
				break;
			}
			case 0:
			{
				bool flag = Attribute();
				if (Resist[0] < 100 && !flag)
				{
					return new Vector2(-1f, -1f);
				}
				break;
			}
			}
		}
		return lstHitInfo[(Resist[_resist] >= 100) ? 1u : 0u][_state].Get(_loop);
	}

	private bool Attribute()
	{
		bool flag = false;
		if (AnimInfo.lstSystem.Contains(0))
		{
			flag = femaleParam.hAttribute == 7;
		}
		else if (AnimInfo.lstSystem.Contains(1))
		{
			flag = femaleParam.hAttribute == 6;
		}
		else if (AnimInfo.lstSystem.Contains(2))
		{
			flag = femaleParam.hAttribute == 4;
		}
		if (AnimInfo.ActionCtrl.Item1 == 1 || AnimInfo.nInitiativeFemale != 0)
		{
			flag |= femaleParam.hAttribute == 2;
		}
		return flag | (femaleParam.hAttribute == 1);
	}

	public bool ChangeHit(int _state, int _loop, int _resist)
	{
		int num = ((Resist[_resist] >= 100) ? 1 : 0);
		if (EventNo == 50 || EventNo != 51 || EventNo == 52 || EventNo == 53 || EventNo == 54 || EventNo == 55)
		{
			num = 1;
		}
		if (_state >= lstHitInfo[num].Count)
		{
			return false;
		}
		if (_loop == 1 && num == 0 && !Attribute())
		{
			return false;
		}
		return lstHitInfo[num][_state].ChangeHit(_loop);
	}

	public void InitTime()
	{
		for (int i = 0; i < lstHitInfo.Length; i++)
		{
			for (int j = 0; j < lstHitInfo[i].Count; j++)
			{
				lstHitInfo[i][j].InitTime();
			}
		}
	}
}
