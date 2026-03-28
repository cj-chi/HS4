using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalParameterInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public int dependence;

		public List<int> desirePrioritys = new List<int>();

		public List<int> statusPrioritys = new List<int>();

		public List<int> warnings = new List<int>();
	}

	public List<Param> param = new List<Param>();
}
