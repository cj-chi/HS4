using System.Collections.Generic;
using Studio.Anime;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.Events;

namespace Studio.Item;

public class ListNodePool : ObjectPool<global::Studio.Anime.ListNode>
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

	public global::Studio.Anime.ListNode Rent(string _text, UnityAction _action, bool _textSlide = true)
	{
		global::Studio.Anime.ListNode listNode = Rent();
		listNode.transform.SetAsLastSibling();
		listNode.Select = false;
		nodes.Add(listNode);
		listNode.UseSlide = _textSlide;
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
