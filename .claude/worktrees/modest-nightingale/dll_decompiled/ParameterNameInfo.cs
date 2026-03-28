using System;
using System.Collections.Generic;
using UnityEngine;

public class ParameterNameInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public string[] state;

		public string[] trait;

		public string[] mind;

		public string[] hattribute;
	}

	public List<Param> param = new List<Param>();
}
