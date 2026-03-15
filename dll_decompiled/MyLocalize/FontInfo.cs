using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyLocalize;

public class FontInfo : ScriptableObject
{
	[Serializable]
	public class Info
	{
		public int id;

		public Font font;
	}

	public List<Info> lstInfo;
}
