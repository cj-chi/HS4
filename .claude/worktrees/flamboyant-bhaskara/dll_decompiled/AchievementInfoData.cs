using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementInfoData : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public int point;

		public string[] title;

		public string[] content;
	}

	public List<Param> param = new List<Param>();
}
