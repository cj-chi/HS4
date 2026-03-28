using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyLocalize;

public class SpriteInfo : ScriptableObject
{
	[Serializable]
	public class SrcInfo
	{
		public int id;

		public Sprite sprite;
	}

	[Serializable]
	public class DstInfo
	{
		public int dstId;

		public string name = "";

		public int srcId;
	}

	public List<SrcInfo> lstSrcInfo;

	public List<DstInfo> lstDstcInfo;
}
