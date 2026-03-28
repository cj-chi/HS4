using System;
using System.Collections.Generic;
using Manager;
using Obi;
using UniRx;
using UnityEngine;

[Serializable]
public class HPoint : MonoBehaviour
{
	[Serializable]
	public struct NotMotionInfo
	{
		[HideInInspector]
		public List<int> motionID;
	}

	[Serializable]
	public class HpointData
	{
		public NotMotionInfo[] notMotion = new NotMotionInfo[6];

		public HpointData()
		{
			notMotion = new NotMotionInfo[6];
			for (int i = 0; i < notMotion.Length; i++)
			{
				notMotion[i].motionID = new List<int>();
			}
		}
	}

	private static List<HScene.AnimationListInfo>[] animationLists;

	[Tooltip("登録ID")]
	public int id;

	[Header("対応させるマップオブジェクトの名前")]
	[SerializeField]
	[Header("消すマップオブジェクト")]
	private GameObject[] hideObj;

	[SerializeField]
	[Header("表示するマップオブジェクト")]
	private GameObject[] showObj;

	[SerializeField]
	[Header("シャワーオナニーで消すマップ側のシャワー")]
	private GameObject[] hideShower;

	private BoolReactiveProperty nowUsing = new BoolReactiveProperty(initialValue: false);

	private bool old;

	private Dictionary<int, ObiCollider[]> colHide = new Dictionary<int, ObiCollider[]>();

	private Dictionary<int, ObiCollider[]> colShow = new Dictionary<int, ObiCollider[]>();

	[SerializeField]
	private HpointData data;

	private static bool showerMasturbation;

	public bool NowUsing
	{
		get
		{
			return nowUsing.Value;
		}
		set
		{
			nowUsing.Value = value;
		}
	}

	public HpointData Data => data;

	public void Init(GameObject MapObj)
	{
		if (animationLists == null)
		{
			animationLists = HSceneManager.HResourceTables.lstAnimInfo;
		}
		data = new HpointData();
		HpointData value = new HpointData();
		if (HSceneManager.HResourceTables.loadHPointDatas.TryGetValue(id, out value))
		{
			int num = 0;
			for (int i = 0; i < 7; i++)
			{
				foreach (HScene.AnimationListInfo item in animationLists[i])
				{
					num = i;
					if (num > 4)
					{
						num = 5;
					}
					if (value.notMotion[num].motionID.Contains(item.id))
					{
						data.notMotion[num].motionID.Add(item.id);
					}
				}
			}
		}
		if (hideObj == null && showObj == null)
		{
			return;
		}
		if (hideObj != null)
		{
			colHide = new Dictionary<int, ObiCollider[]>();
			for (int j = 0; j < hideObj.Length; j++)
			{
				if (!(hideObj[j] == null))
				{
					colHide.Add(j, hideObj[j].GetComponentsInChildren<ObiCollider>(includeInactive: true));
				}
			}
		}
		if (hideObj != null)
		{
			colShow = new Dictionary<int, ObiCollider[]>();
			for (int k = 0; k < showObj.Length; k++)
			{
				if (!(showObj[k] == null))
				{
					colShow.Add(k, showObj[k].GetComponentsInChildren<ObiCollider>(includeInactive: true));
				}
			}
		}
		(from val in nowUsing.TakeUntilDestroy(this)
			where val != old
			select val).Subscribe(delegate(bool val)
		{
			HpointObjVisibleChange(val);
		});
	}

	public static void CheckShowerMasturbation()
	{
		showerMasturbation = false;
		Game instance = Singleton<Game>.Instance;
		showerMasturbation = instance.eventNo == 4 || instance.eventNo == 29;
		showerMasturbation &= instance.peepKind == 1;
	}

	public static void EndSetShowerMasturbation()
	{
		showerMasturbation = false;
	}

	private void HpointObjVisibleChange(bool val)
	{
		old = val;
		if (val)
		{
			if (hideObj != null)
			{
				for (int i = 0; i < hideObj.Length; i++)
				{
					if (!(hideObj[i] == null) && hideObj[i].activeSelf)
					{
						hideObj[i].SetActive(value: false);
						ChangeObiCollider(i, colHide, show: false);
					}
				}
			}
			if (showObj != null)
			{
				for (int j = 0; j < showObj.Length; j++)
				{
					if (!(showObj[j] == null) && !showObj[j].activeSelf)
					{
						showObj[j].SetActive(value: true);
						ChangeObiCollider(j, colShow, show: true);
					}
				}
			}
		}
		else
		{
			if (hideObj != null)
			{
				for (int k = 0; k < hideObj.Length; k++)
				{
					if (!(hideObj[k] == null) && !hideObj[k].activeSelf)
					{
						hideObj[k].SetActive(value: true);
						ChangeObiCollider(k, colHide, show: true);
					}
				}
			}
			if (showObj != null)
			{
				for (int l = 0; l < showObj.Length; l++)
				{
					if (!(showObj[l] == null) && showObj[l].activeSelf)
					{
						showObj[l].SetActive(value: false);
						ChangeObiCollider(l, colShow, show: false);
					}
				}
			}
		}
		if (!showerMasturbation || hideShower == null || hideShower.Length == 0)
		{
			return;
		}
		GameObject[] array = hideShower;
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null) && gameObject.activeSelf != !val)
			{
				gameObject.SetActive(!val);
			}
		}
	}

	private void ChangeObiCollider(int id, Dictionary<int, ObiCollider[]> obiCols, bool show)
	{
		if (!obiCols.ContainsKey(id) || obiCols[id] == null || obiCols[id].Length == 0)
		{
			return;
		}
		for (int i = 0; i < obiCols[id].Length; i++)
		{
			if (show)
			{
				obiCols[id][i].Phase = 0;
			}
			else
			{
				obiCols[id][i].Phase = 1;
			}
		}
	}

	public void ReInit()
	{
		for (int i = 0; i < hideObj.Length; i++)
		{
			if (!(hideObj[i] == null) && !hideObj[i].activeSelf)
			{
				hideObj[i].SetActive(value: true);
			}
		}
		for (int j = 0; j < showObj.Length; j++)
		{
			if (!(showObj[j] == null) && showObj[j].activeSelf)
			{
				showObj[j].SetActive(value: false);
			}
		}
		if (hideShower == null || hideShower.Length == 0)
		{
			return;
		}
		GameObject[] array = hideShower;
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null) && !gameObject.activeSelf)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	public int CheckVisible(GameObject obj)
	{
		if (showObj != null)
		{
			for (int i = 0; i < showObj.Length; i++)
			{
				if (showObj[i] == obj)
				{
					return 1;
				}
			}
		}
		if (hideObj != null)
		{
			for (int j = 0; j < hideObj.Length; j++)
			{
				if (hideObj[j] == obj)
				{
					return 2;
				}
			}
		}
		if (showerMasturbation && hideShower != null && hideShower.Length != 0)
		{
			GameObject[] array = hideShower;
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] == obj)
				{
					return 2;
				}
			}
		}
		return 0;
	}
}
