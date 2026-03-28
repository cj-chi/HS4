using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyLocalize;

public class TextInfo : ScriptableObject
{
	[Serializable]
	public class Info
	{
		public int textId = -1;

		public int fontId = -1;

		public int size = 24;

		public string str = "";
	}

	public int culture;

	public List<Info> lstInfo;
}
