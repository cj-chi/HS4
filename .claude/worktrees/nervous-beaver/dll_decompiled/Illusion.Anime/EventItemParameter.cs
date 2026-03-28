using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illusion.Anime;

public class EventItemParameter : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int ID;

		public AssetBundleManifestData data;

		public bool exists;

		public AssetBundleData animeData;
	}

	public List<Param> param = new List<Param>();
}
