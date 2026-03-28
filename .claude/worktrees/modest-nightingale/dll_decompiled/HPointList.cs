using System;
using System.Collections.Generic;
using Illusion.CustomAttributes;
using UnityEngine;

public class HPointList : MonoBehaviour
{
	[Serializable]
	private class PlaceInfo
	{
		[Label("場所")]
		public int Place;

		[Label("Hポイントグループ")]
		public GameObject HPoints;

		[Label("開始ポイント")]
		public GameObject Start;
	}

	[Serializable]
	public class LoadInfo
	{
		public string Path;

		public string Name;

		public string Manifest;
	}

	public Dictionary<int, List<HPoint>> lst;

	private HPoint[] HPoints;

	[SerializeField]
	private PlaceInfo[] HpointGroup;

	public void Init()
	{
		lst = new Dictionary<int, List<HPoint>>();
		for (int i = 0; i < HpointGroup.Length; i++)
		{
			int num = i;
			if (!(HpointGroup[num].HPoints == null))
			{
				HPoints = HpointGroup[num].HPoints.GetComponentsInChildren<HPoint>();
				lst.Add(HpointGroup[num].Place, new List<HPoint>(HPoints));
			}
		}
	}

	public int GetStartPoint(List<int> place, ref GameObject _Start, ref int choicePlace, int taii, int id)
	{
		int num = -1;
		for (int i = 0; i < place.Count; i++)
		{
			int num2 = place[i];
			if (!lst.ContainsKey(num2))
			{
				continue;
			}
			GameObject gameObject = null;
			for (int j = 0; j < HpointGroup.Length; j++)
			{
				int num3 = j;
				if (HpointGroup[num3].Place == num2)
				{
					gameObject = HpointGroup[num3].Start;
				}
			}
			if (gameObject != null)
			{
				for (int k = 0; k < lst[num2].Count; k++)
				{
					if (!(lst[num2][k].gameObject != gameObject))
					{
						num = k;
						break;
					}
				}
				_Start = gameObject;
			}
			choicePlace = num2;
			if (!lst.ContainsKey(choicePlace) || num < 0)
			{
				break;
			}
			int num4 = taii;
			if (num4 == 6)
			{
				num4 = 5;
			}
			if (!lst[num2][num].Data.notMotion[num4].motionID.Contains(id))
			{
				break;
			}
			int num5 = NewPos(lst[num2], num4, id);
			if (num5 >= 0)
			{
				_Start = lst[num2][num5].gameObject;
				num = num5;
				break;
			}
		}
		return num;
	}

	private int NewPos(List<HPoint> Points, int _taii, int _id)
	{
		int result = -1;
		for (int i = 0; i < Points.Count; i++)
		{
			if (Points[i].Data.notMotion[_taii].motionID.Count == 0)
			{
				result = i;
				break;
			}
			foreach (int item in Points[i].Data.notMotion[_taii].motionID)
			{
				if (item != _id)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}
}
