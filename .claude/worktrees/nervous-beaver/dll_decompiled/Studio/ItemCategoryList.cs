using System.Collections.Generic;
using System.Linq;
using Studio.Anime;
using Studio.Item;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class ItemCategoryList : MonoBehaviour
{
	[SerializeField]
	private Transform transformRoot;

	[SerializeField]
	private GameObject objectPrefab;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private ItemList itemList;

	private ListNodePool listNodePool;

	private Dictionary<int, global::Studio.Anime.ListNode> dicNode;

	private bool isInit;

	private int group = -1;

	private int select = -1;

	public bool active
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			if (base.gameObject.activeSelf != value)
			{
				base.gameObject.SetActive(value);
				if (!base.gameObject.activeSelf)
				{
					itemList.active = false;
				}
			}
		}
	}

	public void InitList(int _group)
	{
		Init();
		listNodePool.Return();
		scrollRect.verticalNormalizedPosition = 1f;
		dicNode.Clear();
		Info.GroupInfo value = null;
		if (Singleton<Info>.Instance.dicItemGroupCategory.TryGetValue(_group, out value))
		{
			foreach (KeyValuePair<int, Info.CategoryInfo> item in value.dicCategory.OrderBy((KeyValuePair<int, Info.CategoryInfo> v) => v.Value.sort))
			{
				if (Singleton<Info>.Instance.ExistItemCategory(_group, item.Key))
				{
					int no = item.Key;
					global::Studio.Anime.ListNode value2 = listNodePool.Rent(item.Value.name, delegate
					{
						OnSelect(no);
					}, _textSlide: false);
					dicNode.Add(item.Key, value2);
				}
			}
		}
		select = -1;
		group = _group;
		active = true;
		itemList.active = false;
	}

	private void OnSelect(int _no)
	{
		int key = select;
		if (Utility.SetStruct(ref select, _no))
		{
			itemList.InitList(group, _no);
			global::Studio.Anime.ListNode value = null;
			if (dicNode.TryGetValue(key, out value) && value != null)
			{
				value.Select = false;
			}
			value = null;
			if (dicNode.TryGetValue(select, out value) && value != null)
			{
				value.Select = true;
			}
		}
	}

	private void Init()
	{
		if (!isInit)
		{
			listNodePool = new ListNodePool(transformRoot, objectPrefab.GetComponent<global::Studio.Anime.ListNode>());
			dicNode = new Dictionary<int, global::Studio.Anime.ListNode>();
			isInit = true;
		}
	}
}
