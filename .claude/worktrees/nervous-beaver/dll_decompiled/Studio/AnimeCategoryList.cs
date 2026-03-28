using System.Collections.Generic;
using System.Linq;
using Studio.Anime;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Studio;

public class AnimeCategoryList : MonoBehaviour
{
	private class ListNodePool : ObjectPool<global::Studio.Anime.ListNode>
	{
		private readonly Transform parent;

		private readonly global::Studio.Anime.ListNode prefab;

		private List<global::Studio.Anime.ListNode> nodes;

		public ListNodePool(Transform _parent, global::Studio.Anime.ListNode _prefab)
		{
			parent = _parent;
			prefab = _prefab;
			nodes = new List<global::Studio.Anime.ListNode>();
		}

		protected override global::Studio.Anime.ListNode CreateInstance()
		{
			return Object.Instantiate(prefab, parent);
		}

		public global::Studio.Anime.ListNode Rent(string _text, UnityAction _action)
		{
			global::Studio.Anime.ListNode listNode = Rent();
			listNode.transform.SetAsLastSibling();
			listNode.Select = false;
			nodes.Add(listNode);
			listNode.Text = _text;
			listNode.SetButtonAction(_action);
			return listNode;
		}

		public void Return()
		{
			foreach (global::Studio.Anime.ListNode node in nodes)
			{
				Return(node);
			}
			nodes.Clear();
		}
	}

	[SerializeField]
	private Transform transformRoot;

	[SerializeField]
	private GameObject objectPrefab;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private AnimeList animeList;

	private ListNodePool listNodePool;

	private bool isInit;

	private AnimeGroupList.SEX sex = AnimeGroupList.SEX.Unknown;

	private int group = -1;

	private int select = -1;

	private Dictionary<int, global::Studio.Anime.ListNode> dicNode;

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
					animeList.active = false;
				}
			}
		}
	}

	public void InitList(AnimeGroupList.SEX _sex, int _group)
	{
		Init();
		listNodePool.Return();
		scrollRect.verticalNormalizedPosition = 1f;
		dicNode.Clear();
		foreach (KeyValuePair<int, Info.CategoryInfo> item in Singleton<Info>.Instance.dicAGroupCategory[_group].dicCategory.OrderBy((KeyValuePair<int, Info.CategoryInfo> v) => v.Value.sort))
		{
			if (Singleton<Info>.Instance.ExistAnimeCategory(_group, item.Key))
			{
				int no = item.Key;
				global::Studio.Anime.ListNode value = listNodePool.Rent(item.Value.name, delegate
				{
					OnSelect(no);
				});
				dicNode.Add(item.Key, value);
			}
		}
		select = -1;
		group = _group;
		sex = _sex;
		active = true;
		animeList.active = false;
	}

	private bool CheckCategory(int _group, int _category, Dictionary<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>> _dic)
	{
		Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>> value = null;
		if (_dic.TryGetValue(_group, out value))
		{
			return value.ContainsKey(_category);
		}
		return false;
	}

	private void OnSelect(int _no)
	{
		int key = select;
		if (Utility.SetStruct(ref select, _no))
		{
			animeList.InitList(sex, group, _no);
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
