using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalPoseInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public List<int> poseIDs;

		public string exp;
	}

	public List<Param> param = new List<Param>();
}
