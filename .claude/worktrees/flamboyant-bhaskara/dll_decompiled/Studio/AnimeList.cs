using System.Collections.Generic;
using Studio.Anime;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Studio;

public class AnimeList : MonoBehaviour
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
	private MPCharCtrl mpCharCtrl;

	private ListNodePool listNodePool;

	private bool isInit;

	private AnimeGroupList.SEX sex = AnimeGroupList.SEX.Unknown;

	private int group = -1;

	private int category = -1;

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
			}
		}
	}

	public void InitList(AnimeGroupList.SEX _sex, int _group, int _category)
	{
		Init();
		listNodePool.Return();
		scrollRect.verticalNormalizedPosition = 1f;
		dicNode.Clear();
		foreach (KeyValuePair<int, Info.AnimeLoadInfo> item in Singleton<Info>.Instance.dicAnimeLoadInfo[_group][_category])
		{
			int no = item.Key;
			global::Studio.Anime.ListNode value = listNodePool.Rent(item.Value.name, delegate
			{
				OnSelect(no);
			});
			dicNode.Add(item.Key, value);
		}
		sex = _sex;
		group = _group;
		category = _category;
		select = -1;
		active = true;
	}

	private void OnSelect(int _no)
	{
		mpCharCtrl.LoadAnime(sex, group, category, _no);
		int key = select;
		if (Utility.SetStruct(ref select, _no))
		{
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
