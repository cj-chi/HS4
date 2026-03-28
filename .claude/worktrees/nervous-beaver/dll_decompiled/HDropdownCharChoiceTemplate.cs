using System.Collections.Generic;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

public class HDropdownCharChoiceTemplate : MonoBehaviour
{
	private PointerAction[] items;

	private List<ChachoiceNode> nodes;

	private Dictionary<int, RectTransform> itemRTs;

	private HSceneSpriteChaChoice choice;

	private RectTransform contents;

	private RectTransform rt;

	public List<ChachoiceNode> Nodes => nodes;

	public void Start()
	{
		if (GetComponentInParent<Dropdown>() != null)
		{
			items = GetComponentsInChildren<PointerAction>();
			HSceneSprite hSceneSprite = (Singleton<HSceneSprite>.IsInstance() ? Singleton<HSceneSprite>.Instance : null);
			if (hSceneSprite != null)
			{
				for (int i = 0; i < items.Length; i++)
				{
					items[i].listDownAction.Clear();
					items[i].listDownAction.Add(hSceneSprite.OnClickSliderSelect);
				}
			}
		}
		choice = GetComponentInParent<HSceneSpriteChaChoice>();
		rt = GetComponent<RectTransform>();
		contents = base.transform.Find("Viewport/Content").GetComponent<RectTransform>();
		items = contents.GetComponentsInChildren<PointerAction>();
		itemRTs = new Dictionary<int, RectTransform>();
		for (int j = 0; j < items.Length; j++)
		{
			itemRTs.Add(j, items[j].GetComponent<RectTransform>());
		}
		nodes = new List<ChachoiceNode>();
		for (int k = 0; k < items.Length; k++)
		{
			ChachoiceNode node = items[k].GetComponent<ChachoiceNode>();
			if (!(node == null))
			{
				node.ChoiseNo = k;
				if (k > 0)
				{
					CheckCha(k, items.Length, ref node);
				}
				nodes.Add(node);
			}
		}
		choice.SetTemplate(this);
	}

	public void CheckCha(int num, int max, ref ChachoiceNode node)
	{
		switch (max)
		{
		case 2:
			if (choice.Females[1] == null)
			{
				node.ChoiseNo = 2;
			}
			break;
		case 3:
			if (choice.Females[1] == null)
			{
				if (num == 1)
				{
					node.ChoiseNo = 2;
				}
				else
				{
					node.ChoiseNo = 3;
				}
			}
			else if (num != 1)
			{
				node.ChoiseNo = 2;
			}
			break;
		}
		if (node.ChoiseNo >= 4)
		{
			node.ChoiseNo = -1;
		}
	}

	private void Update()
	{
		foreach (ChachoiceNode node in nodes)
		{
			if (node.ChoiseNo < 0)
			{
				node.gameObject.SetActive(value: false);
				continue;
			}
			for (int i = 0; i < 2; i++)
			{
				if (node.ChoiseNo == i)
				{
					node.gameObject.SetActive(choice.FeMaleActive[i]);
				}
			}
			for (int j = 0; j < 2; j++)
			{
				if (node.ChoiseNo == j + 2)
				{
					node.gameObject.SetActive(choice.MaleActive[j]);
				}
			}
		}
		float num = 0f;
		for (int k = 0; k < items.Length; k++)
		{
			if (items[k].gameObject.activeSelf)
			{
				num += itemRTs[k].sizeDelta.y;
			}
		}
		num += 10f;
		rt.sizeDelta = new Vector2(rt.sizeDelta.x, num);
	}
}
