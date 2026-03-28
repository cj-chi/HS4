using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illusion.Anime;

public class AnimeItemScaleParameter : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int itemID;

		public int mode;

		public float S;

		public float M;

		public float L;
	}

	public List<Param> param = new List<Param>();
}
