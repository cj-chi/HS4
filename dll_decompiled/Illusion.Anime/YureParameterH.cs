using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illusion.Anime;

public class YureParameterH : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int ID;

		public YureCtrl.Info info = new YureCtrl.Info();
	}

	public List<Param> param = new List<Param>();
}
