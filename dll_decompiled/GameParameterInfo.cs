using System;
using System.Collections.Generic;
using UnityEngine;

public class GameParameterInfo : ScriptableObject
{
	[Serializable]
	public class MinMax
	{
		public int min;

		public int max;
	}

	[Serializable]
	public class Param
	{
		public int id;

		public MinMax favor = new MinMax();

		public MinMax enjoyment = new MinMax();

		public MinMax slavery = new MinMax();

		public MinMax aversion = new MinMax();

		public MinMax broken = new MinMax();

		public MinMax dependence = new MinMax();

		public MinMax dirty = new MinMax();

		public MinMax tiredness = new MinMax();

		public MinMax toilet = new MinMax();

		public MinMax libido = new MinMax();
	}

	public List<Param> param = new List<Param>();
}
