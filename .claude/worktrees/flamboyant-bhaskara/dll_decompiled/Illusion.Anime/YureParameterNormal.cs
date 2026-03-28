using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illusion.Anime;

public class YureParameterNormal : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public YureCtrl.Info info = new YureCtrl.Info();
	}

	public List<Param> param = new List<Param>();
}
