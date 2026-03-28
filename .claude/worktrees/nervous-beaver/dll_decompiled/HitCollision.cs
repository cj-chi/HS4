using System.Collections.Generic;
using UnityEngine;

public class HitCollision : MonoBehaviour
{
	public List<GameObject> lstObj = new List<GameObject>();

	private void Reset()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			lstObj.Add(base.transform.GetChild(i).gameObject);
		}
	}
}
