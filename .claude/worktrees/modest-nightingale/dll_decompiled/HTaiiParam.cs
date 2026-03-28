using System;
using System.Collections.Generic;
using UnityEngine;

public class HTaiiParam : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public List<int> judge = new List<int>();

		public GameParameterInfo.MinMax favor = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax enjoyment = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax slavery = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax aversion = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax broken = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax dependence = new GameParameterInfo.MinMax();
	}

	public List<Param> param = new List<Param>();
}
