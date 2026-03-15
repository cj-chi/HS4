using System;
using System.Collections.Generic;

namespace CharaCustom;

public class CustomCharaFileInfo
{
	public CustomCharaScrollViewInfo cssvi;

	public int index;

	public string FullPath = "";

	public string FileName = "";

	public DateTime time;

	public string name = "";

	public string personality = "";

	public int voice;

	public int height;

	public int bustSize;

	public int hair;

	public int bloodType;

	public int birthMonth = 1;

	public int birthDay = 1;

	public string strBirthDay = "";

	public int sex;

	public byte[] pngData;

	public bool isChangeParameter;

	public int trait;

	public int mind;

	public int hAttribute;

	public Dictionary<int, int> flavorState = new Dictionary<int, int>();

	public bool futanari;

	public bool isInSaveData;

	public string data_uuid = "";

	public CharaCategoryKind cateKind = CharaCategoryKind.Female;
}
