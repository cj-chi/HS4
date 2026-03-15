using System;
using System.Collections.Generic;
using UnityEngine;

public class EventContentInfoData : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public int[] meetingLocationMaps;

		public int[] goToFemaleMaps;

		public int draw;

		public int call;

		public string manifestEventSprite;

		public string bundleEventSprite;

		public string fileEventSprite;

		public string[] eventNames;
	}

	public List<Param> param = new List<Param>();
}
