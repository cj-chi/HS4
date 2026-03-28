using System;
using System.Collections.Generic;
using AIChara;

namespace GameLoadCharaFileSystem;

public class GameCharaFileInfo
{
	public GameCharaFileInfoComponent.RowInfo fic;

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

	public int[] usePackage;

	public byte[] pngData;

	public bool futanari;

	public ChaFileDefine.State state;

	public byte trait;

	public byte hAttribute;

	public int resistH;

	public int resistPain;

	public int resistAnal;

	public int broken;

	public int dependence;

	public int usedItem;

	public bool lockNowState;

	public bool lockBroken;

	public bool lockDependence;

	public int hcount;

	public HashSet<int> lstFilter = new HashSet<int>();

	public bool isEntry;

	public string data_uuid = "";

	public CategoryKind cateKind = CategoryKind.Female;
}
