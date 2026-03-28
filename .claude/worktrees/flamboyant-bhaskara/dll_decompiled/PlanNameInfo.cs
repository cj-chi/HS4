using System;
using System.Collections.Generic;
using Illusion.Extensions;
using UnityEngine;

public class PlanNameInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public string[] name;

		public string manifest;

		public string bundle;

		public string asset;

		public Param()
		{
		}

		public Param(Param _param)
		{
			id = _param.id;
			name = new string[_param.name.Length];
			foreach (var (text, num) in _param.name.ToForEachTuples())
			{
				name[num] = text;
			}
			manifest = _param.manifest;
			bundle = _param.bundle;
			asset = _param.asset;
		}
	}

	public List<Param> param = new List<Param>();
}
