using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illusion.Anime;

public class AnimeSEEventParameter : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int poseID;

		public int nameHash;

		public List<Parameter> parameters = new List<Parameter>();
	}

	[Serializable]
	public class Parameter
	{
		public float normalizedTime;

		public int eventID;

		public int ClipID;

		public string Root;
	}

	public List<Param> param = new List<Param>();
}
