using System;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class VoiceInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public string Personality;

		public int No;

		public int Sort;

		public bool isPositive;

		public string EnUS;

		public string ZhCN;

		public string ZhTW;

		public string samplebundle;

		public string sampleasset;

		public string Get(int lang)
		{
			return (GameSystem.Language)lang switch
			{
				GameSystem.Language.Japanese => Personality, 
				GameSystem.Language.English => EnUS, 
				GameSystem.Language.SimplifiedChinese => ZhCN, 
				GameSystem.Language.TraditionalChinese => ZhTW, 
				_ => null, 
			};
		}
	}

	public List<Param> param = new List<Param>();
}
