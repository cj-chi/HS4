using System;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEffectPresetData : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int No;

		public string Name;

		public string AssetBundleName;

		public string AssetName;

		public string Manifest;
	}

	public List<Param> param = new List<Param>();
}
