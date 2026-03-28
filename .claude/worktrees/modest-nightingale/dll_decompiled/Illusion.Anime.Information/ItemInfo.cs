using System;
using System.Collections.Generic;

namespace Illusion.Anime.Information;

[Serializable]
public class ItemInfo
{
	public string parentName = "";

	public bool fromEquipedItem;

	public int itemID;

	public bool isSync;

	public List<ItemParentInfo> parentInfos = new List<ItemParentInfo>();
}
