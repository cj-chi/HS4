using System;
using System.Collections.Generic;
using UnityEngine;

public class BGMNameInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public int bgmID;

		public string[] name;
	}

	public List<Param> param = new List<Param>();
}
