using System.Collections.Generic;
using Illusion.Extensions;
using Studio.Anime;
using Studio.Item;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class ItemList : MonoBehaviour
{
	[SerializeField]
	private Transform transformRoot;

	[SerializeField]
	private GameObject objectNode;

	[SerializeField]
	private ScrollRect scrollRect;

	private ListNodePool listNodePool;

	private bool isInit;

	private int group = -1;

	private int category = -1;

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
				if (!value)
				{
					category = -1;
				}
			}
		}
	}

	public void InitList(int _group, int _category)
	{
		Init();
		if (group == _group && category == _category)
		{
			return;
		}
		listNodePool.Return();
		scrollRect.verticalNormalizedPosition = 1f;
		foreach (KeyValuePair<int, Info.ItemLoadInfo> item in Singleton<Info>.Instance.dicItemLoadInfo[_group][_category])
		{
			int no = item.Key;
			global::Studio.Anime.ListNode listNode = listNodePool.Rent(item.Value.name, delegate
			{
				OnSelect(no);
			});
			switch (Singleton<Info>.Instance.SafeGetItemColorData(_group, _category, item.Key)?.Count ?? 0)
			{
			case 1:
				listNode.TextColor = Color.red;
				break;
			case 2:
				listNode.TextColor = Color.cyan;
				break;
			case 3:
				listNode.TextColor = Color.green;
				break;
			case 4:
				listNode.TextColor = Color.yellow;
				break;
			default:
				listNode.TextColor = Color.white;
				break;
			}
		}
		base.gameObject.SetActiveIfDifferent(active: true);
		group = _group;
		category = _category;
	}

	private void OnSelect(int _no)
	{
		Singleton<Studio>.Instance.AddItem(group, category, _no);
	}

	private void Init()
	{
		if (!isInit)
		{
			listNodePool = new ListNodePool(transformRoot, objectNode.GetComponent<global::Studio.Anime.ListNode>());
			isInit = true;
			group = -1;
			category = -1;
		}
	}
}
