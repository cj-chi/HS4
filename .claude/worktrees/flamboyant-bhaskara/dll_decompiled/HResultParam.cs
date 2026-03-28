using System;
using System.Collections.Generic;
using UnityEngine;

public class HResultParam : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public GameParameterInfo.MinMax favor = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax enjoyment = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax slavery = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax aversion = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax broken = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax dependence = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax dirty = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax tiredness = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax toilet = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax libido = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax hResist = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax analResist = new GameParameterInfo.MinMax();

		public GameParameterInfo.MinMax painResist = new GameParameterInfo.MinMax();
	}

	public List<Param> param = new List<Param>();
}
