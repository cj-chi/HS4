using System;
using System.Collections.Generic;
using UnityEngine;

public class SearchAccessoryInfoData : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public int category;

		public int accessoryID;
	}

	public List<Param> param = new List<Param>();
}
