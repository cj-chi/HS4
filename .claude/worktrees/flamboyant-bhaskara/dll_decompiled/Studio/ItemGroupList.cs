using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class ItemGroupList : MonoBehaviour
{
	[SerializeField]
	private Transform transformRoot;

	[SerializeField]
	private GameObject objectPrefab;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private ItemCategoryList itemCategoryList;

	[SerializeField]
	private ItemList itemList;

	private int select = -1;

	private Dictionary<int, Image> dicNode;

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
					itemCategoryList.active = false;
				}
			}
		}
	}

	public void InitList()
	{
		int childCount = transformRoot.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Object.Destroy(transformRoot.GetChild(i).gameObject);
		}
		transformRoot.DetachChildren();
		scrollRect.verticalNormalizedPosition = 1f;
		dicNode = new Dictionary<int, Image>();
		foreach (KeyValuePair<int, Info.GroupInfo> item in Singleton<Info>.Instance.dicItemGroupCategory.OrderBy((KeyValuePair<int, Info.GroupInfo> _v) => _v.Value.sort))
		{
			if (Singleton<Info>.Instance.ExistItemGroup(item.Key))
			{
				GameObject gameObject = Object.Instantiate(objectPrefab);
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(value: true);
				}
				gameObject.transform.SetParent(transformRoot, worldPositionStays: false);
				ListNode component = gameObject.GetComponent<ListNode>();
				int no = item.Key;
				component.AddActionToButton(delegate
				{
					OnSelect(no);
				});
				component.text = item.Value.name;
				dicNode.Add(item.Key, gameObject.GetComponent<Image>());
			}
		}
		select = -1;
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		itemCategoryList.active = false;
		itemList.active = false;
	}

	private void OnSelect(int _no)
	{
		int key = select;
		if (Utility.SetStruct(ref select, _no))
		{
			itemCategoryList.InitList(_no);
			Image value = null;
			if (dicNode.TryGetValue(key, out value) && value != null)
			{
				value.color = Color.white;
			}
			value = null;
			if (dicNode.TryGetValue(select, out value) && value != null)
			{
				value.color = Color.green;
			}
		}
	}

	private void Start()
	{
		InitList();
	}
}
