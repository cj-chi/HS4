using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEx;

public class RotationScroll : MonoBehaviour
{
	public GameObject Contents;

	[Space]
	[SerializeField]
	public RectTransform SelectRect;

	[Space]
	public RectTransform Atari;

	[SerializeField]
	private float moveTime = 0.05f;

	[SerializeField]
	private LinkedList<ScrollCylinderNode> lstNodes = new LinkedList<ScrollCylinderNode>();

	[SerializeField]
	private (Vector2, ScrollCylinderNode) targetNode;

	[SerializeField]
	private bool InitList;

	[SerializeField]
	private int MaxView = 3;

	private bool onEnter;

	public HRotationScrollNode[] NodeList;

	public ScrollDir scrollMode;

	private Vector2 nowPos;

	private void Start()
	{
		PointerEnterTrigger pointerEnterTrigger = Atari.gameObject.AddComponent<PointerEnterTrigger>();
		UITrigger.TriggerEvent triggerEvent = new UITrigger.TriggerEvent();
		pointerEnterTrigger.Triggers.Add(triggerEvent);
		triggerEvent.AddListener(delegate
		{
			onEnter = true;
		});
		PointerExitTrigger pointerExitTrigger = Atari.gameObject.AddComponent<PointerExitTrigger>();
		triggerEvent = new UITrigger.TriggerEvent();
		pointerExitTrigger.Triggers.Add(triggerEvent);
		triggerEvent.AddListener(delegate
		{
			onEnter = false;
		});
		if (InitList)
		{
			ListNodeSet();
		}
	}

	public void ListNodeSet(List<ScrollCylinderNode> ScrollNodes = null, bool targetInit = true)
	{
		lstNodes.Clear();
		if (ScrollNodes != null)
		{
			for (int i = 0; i < ScrollNodes.Count; i++)
			{
				RectTransform component = ScrollNodes[i].GetComponent<RectTransform>();
				if (ScrollNodes.Count < MaxView)
				{
					component.anchoredPosition = new Vector2(component.rect.width / 2f, (0f - component.rect.height) / 2f) + new Vector2((scrollMode == ScrollDir.Vertical) ? 0f : (component.rect.width / 2f - 10f * (float)(MaxView - 1)), (scrollMode == ScrollDir.Horizontal) ? 0f : ((0f - component.rect.height) * (float)(MaxView - 1))) * i;
				}
				else
				{
					component.anchoredPosition = new Vector2(component.rect.width / 2f, (0f - component.rect.height) / 2f) + new Vector2((scrollMode == ScrollDir.Vertical) ? 0f : (component.rect.width / 2f - 10f), (scrollMode == ScrollDir.Horizontal) ? 0f : (0f - component.rect.height)) * i;
				}
				lstNodes.AddLast(ScrollNodes[i]);
			}
		}
		else
		{
			ScrollCylinderNode[] componentsInChildren = Contents.GetComponentsInChildren<ScrollCylinderNode>();
			ScrollCylinderNode[] array = new ScrollCylinderNode[componentsInChildren.Length];
			for (int j = 0; j < NodeList.Length; j++)
			{
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					if (!(NodeList[j].gameObject != componentsInChildren[k].gameObject))
					{
						componentsInChildren[k].transform.SetSiblingIndex(j);
						array[j] = componentsInChildren[k];
						break;
					}
				}
			}
			int num = 0;
			for (int l = 0; l < array.Length; l++)
			{
				if (!array[l].BG.enabled)
				{
					array[l].ChangeBGAlpha(3);
					array[l].GetComponent<HRotationScrollNode>().ChangeScale(3, onEnter);
					continue;
				}
				RectTransform component2 = array[l].GetComponent<RectTransform>();
				if (array.Length < MaxView)
				{
					component2.anchoredPosition = new Vector2(component2.rect.width / 2f, (0f - component2.rect.height) / 2f) + new Vector2((scrollMode == ScrollDir.Vertical) ? 0f : (component2.rect.width / 2f - 10f * (float)(MaxView - 1)), (scrollMode == ScrollDir.Horizontal) ? 0f : ((0f - component2.rect.height) * (float)(MaxView - 1))) * num;
				}
				else
				{
					component2.anchoredPosition = new Vector2(component2.rect.width / 2f, (0f - component2.rect.height) / 2f) + new Vector2((scrollMode == ScrollDir.Vertical) ? 0f : (component2.rect.width / 2f - 10f), (scrollMode == ScrollDir.Horizontal) ? 0f : (0f - component2.rect.height)) * num;
				}
				num++;
				lstNodes.AddLast(array[l]);
			}
		}
		if (lstNodes.Count == 0)
		{
			return;
		}
		RectTransform ContentsRt = Contents.GetComponent<RectTransform>();
		Vector3 ContentsPosition = ContentsRt.anchoredPosition;
		RectTransform rt2 = Object.Instantiate(ContentsRt);
		Vector3 position2 = ContentsPosition;
		ContentsPosition = LimitPos(ContentsRt, ContentsPosition, (scrollMode == ScrollDir.Vertical) ? 1 : 0);
		ContentsRt.anchoredPosition = ContentsPosition;
		PointerEnterTrigger pointerEnterTrigger = null;
		UITrigger.TriggerEvent triggerEvent = new UITrigger.TriggerEvent();
		foreach (ScrollCylinderNode lstNode in lstNodes)
		{
			pointerEnterTrigger = null;
			pointerEnterTrigger = lstNode.gameObject.GetComponent<PointerEnterTrigger>();
			lstNode.gameObject.SetActive(value: true);
			if (!(pointerEnterTrigger != null))
			{
				pointerEnterTrigger = lstNode.gameObject.AddComponent<PointerEnterTrigger>();
				triggerEvent = new UITrigger.TriggerEvent();
				pointerEnterTrigger.Triggers.Add(triggerEvent);
				triggerEvent.AddListener(delegate
				{
					onEnter = true;
				});
			}
		}
		if (!targetInit)
		{
			if (rt2 != null && rt2.gameObject != null)
			{
				Object.Destroy(rt2.gameObject);
			}
			return;
		}
		if (ContentsRt.rect.width == 0f || ContentsRt.rect.height == 0f)
		{
			Observable.NextFrame().Subscribe(delegate
			{
				InitTargrt(ContentsRt, ContentsPosition);
				Object.Destroy(rt2.gameObject);
			});
			return;
		}
		Observable.NextFrame().Subscribe(delegate
		{
			rt2.sizeDelta = ContentsRt.sizeDelta;
			InitTargrt(rt2, position2);
			Object.Destroy(rt2.gameObject);
		});
	}

	private void Update()
	{
		if (lstNodes.Count != 0)
		{
			MoveContentsNew();
			ChangeNode();
		}
	}

	private void MoveContentsNew()
	{
		if (targetNode.Item2 == null)
		{
			return;
		}
		LinkedListNode<ScrollCylinderNode> linkedListNode = lstNodes.Find(targetNode.Item2);
		float axis = Input.GetAxis("Mouse ScrollWheel");
		RectTransform rectTransform = null;
		if (onEnter && lstNodes.Count > 1)
		{
			if (axis < 0f)
			{
				if (linkedListNode.Next == null)
				{
					LinkedListNode<ScrollCylinderNode> first = lstNodes.First;
					lstNodes.RemoveFirst();
					lstNodes.AddLast(first);
					rectTransform = linkedListNode.Value.GetComponent<RectTransform>();
					RectTransform component = first.Value.GetComponent<RectTransform>();
					if (component != null)
					{
						float x = rectTransform.anchoredPosition.x;
						x += rectTransform.sizeDelta.x / 2f - 10f;
						component.anchoredPosition = new Vector2(x, component.anchoredPosition.y);
					}
				}
				targetNode = (Vector2.zero, linkedListNode.Next.Value);
			}
			else if (axis > 0f)
			{
				if (linkedListNode.Previous == null)
				{
					LinkedListNode<ScrollCylinderNode> last = lstNodes.Last;
					lstNodes.RemoveLast();
					lstNodes.AddFirst(last);
					rectTransform = linkedListNode.Value.GetComponent<RectTransform>();
					RectTransform component2 = last.Value.GetComponent<RectTransform>();
					if (component2 != null)
					{
						float x2 = rectTransform.anchoredPosition.x;
						x2 += rectTransform.sizeDelta.x / 2f - 10f;
						component2.anchoredPosition = new Vector2(x2, component2.anchoredPosition.y);
					}
				}
				targetNode = (Vector2.zero, linkedListNode.Previous.Value);
			}
		}
		rectTransform = targetNode.Item2.GetComponent<RectTransform>();
		if (rectTransform.anchoredPosition.x != 0f)
		{
			float x3 = rectTransform.anchoredPosition.x;
			float currentVelocity = 0f;
			x3 = Mathf.SmoothDamp(x3, 0f, ref currentVelocity, moveTime, float.PositiveInfinity, Time.deltaTime);
			rectTransform.anchoredPosition = new Vector2(x3, rectTransform.anchoredPosition.y);
		}
		if (linkedListNode == null)
		{
			return;
		}
		if (lstNodes.Count >= MaxView)
		{
			RectTransform component3;
			if (linkedListNode.Next != null)
			{
				component3 = linkedListNode.Next.Value.GetComponent<RectTransform>();
				if (component3 != null)
				{
					float x4 = rectTransform.anchoredPosition.x;
					x4 += rectTransform.sizeDelta.x / 2f - 10f;
					component3.anchoredPosition = new Vector2(x4, component3.anchoredPosition.y);
				}
			}
			else
			{
				LinkedListNode<ScrollCylinderNode> first2 = lstNodes.First;
				lstNodes.RemoveFirst();
				lstNodes.AddLast(first2);
				component3 = first2.Value.GetComponent<RectTransform>();
				if (component3 != null)
				{
					float x5 = rectTransform.anchoredPosition.x;
					x5 += rectTransform.sizeDelta.x / 2f - 10f;
					component3.anchoredPosition = new Vector2(x5, component3.anchoredPosition.y);
				}
			}
			if (lstNodes.Count > MaxView)
			{
				if (linkedListNode.Next.Next != null)
				{
					component3 = linkedListNode.Next.Next.Value.GetComponent<RectTransform>();
					if (component3 != null)
					{
						float x6 = rectTransform.anchoredPosition.x;
						x6 += (rectTransform.sizeDelta.x / 2f - 10f) * 2f;
						component3.anchoredPosition = new Vector2(x6, component3.anchoredPosition.y);
					}
				}
				else
				{
					LinkedListNode<ScrollCylinderNode> first3 = lstNodes.First;
					lstNodes.RemoveFirst();
					lstNodes.AddLast(first3);
					component3 = first3.Value.GetComponent<RectTransform>();
					if (component3 != null)
					{
						float x7 = rectTransform.anchoredPosition.x;
						x7 += (rectTransform.sizeDelta.x / 2f - 10f) * 2f;
						component3.anchoredPosition = new Vector2(x7, component3.anchoredPosition.y);
					}
				}
			}
			if (linkedListNode.Previous != null)
			{
				component3 = linkedListNode.Previous.Value.GetComponent<RectTransform>();
				if (component3 != null)
				{
					float x8 = rectTransform.anchoredPosition.x;
					x8 -= rectTransform.sizeDelta.x / 2f - 10f;
					component3.anchoredPosition = new Vector2(x8, component3.anchoredPosition.y);
				}
			}
			else
			{
				LinkedListNode<ScrollCylinderNode> last2 = lstNodes.Last;
				lstNodes.RemoveLast();
				lstNodes.AddFirst(last2);
				component3 = last2.Value.GetComponent<RectTransform>();
				if (component3 != null)
				{
					float x9 = rectTransform.anchoredPosition.x;
					x9 -= rectTransform.sizeDelta.x / 2f - 10f;
					component3.anchoredPosition = new Vector2(x9, component3.anchoredPosition.y);
				}
			}
			if (lstNodes.Count <= MaxView)
			{
				return;
			}
			if (linkedListNode.Previous.Previous != null)
			{
				component3 = linkedListNode.Previous.Previous.Value.GetComponent<RectTransform>();
				if (component3 != null)
				{
					float x10 = rectTransform.anchoredPosition.x;
					x10 -= (rectTransform.sizeDelta.x / 2f - 10f) * 2f;
					component3.anchoredPosition = new Vector2(x10, component3.anchoredPosition.y);
				}
				return;
			}
			LinkedListNode<ScrollCylinderNode> last3 = lstNodes.Last;
			lstNodes.RemoveLast();
			lstNodes.AddFirst(last3);
			component3 = last3.Value.GetComponent<RectTransform>();
			if (component3 != null)
			{
				float x11 = rectTransform.anchoredPosition.x;
				x11 -= (rectTransform.sizeDelta.x / 2f - 10f) * 2f;
				component3.anchoredPosition = new Vector2(x11, component3.anchoredPosition.y);
			}
		}
		else
		{
			if (lstNodes.Count <= 1)
			{
				return;
			}
			if (linkedListNode.Next != null)
			{
				RectTransform component4 = linkedListNode.Next.Value.GetComponent<RectTransform>();
				if (component4 != null)
				{
					float x12 = rectTransform.anchoredPosition.x;
					x12 += (rectTransform.sizeDelta.x / 2f - 10f) * (float)(MaxView / 2 + 1);
					component4.anchoredPosition = new Vector2(x12, component4.anchoredPosition.y);
				}
			}
			if (linkedListNode.Previous != null)
			{
				RectTransform component5 = linkedListNode.Previous.Value.GetComponent<RectTransform>();
				if (component5 != null)
				{
					float x13 = rectTransform.anchoredPosition.x;
					x13 -= (rectTransform.sizeDelta.x / 2f - 10f) * (float)(MaxView / 2 + 1);
					component5.anchoredPosition = new Vector2(x13, component5.anchoredPosition.y);
				}
			}
		}
	}

	private void ChangeNode()
	{
		_ = Time.deltaTime;
		ChangeNodeColor();
		ChangeNodeScl();
	}

	private void ChangeNodeColor()
	{
		LinkedListNode<ScrollCylinderNode> linkedListNode = lstNodes.Find(targetNode.Item2);
		if (linkedListNode == null)
		{
			return;
		}
		foreach (ScrollCylinderNode lstNode in lstNodes)
		{
			if (lstNode == linkedListNode.Value)
			{
				lstNode.ChangeBGAlpha(0);
			}
			else if (lstNode == linkedListNode.Previous?.Value || lstNode == linkedListNode.Next?.Value)
			{
				lstNode.ChangeBGAlpha(1);
			}
			else
			{
				lstNode.ChangeBGAlpha(3);
			}
		}
	}

	private void ChangeNodeScl()
	{
		LinkedListNode<ScrollCylinderNode> linkedListNode = lstNodes.Find(targetNode.Item2);
		if (linkedListNode == null)
		{
			return;
		}
		foreach (ScrollCylinderNode lstNode in lstNodes)
		{
			if (lstNode == linkedListNode.Value)
			{
				lstNode.GetComponent<HRotationScrollNode>()?.ChangeScale(0, onEnter);
			}
			else if (lstNode == linkedListNode.Previous?.Value || lstNode == linkedListNode.Next?.Value)
			{
				lstNode.GetComponent<HRotationScrollNode>()?.ChangeScale(1, onEnter);
			}
			else
			{
				lstNode.GetComponent<HRotationScrollNode>()?.ChangeScale(3, onEnter);
			}
		}
	}

	public LinkedList<ScrollCylinderNode> GetList()
	{
		return lstNodes;
	}

	public (Vector2, ScrollCylinderNode) GetTarget()
	{
		return targetNode;
	}

	public void SetTarget(ScrollCylinderNode target)
	{
		RectTransform component = Contents.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		Vector2 item = default(Vector2);
		foreach (ScrollCylinderNode lstNode in lstNodes)
		{
			if (!(lstNode != target))
			{
				RectTransform component2 = lstNode.GetComponent<RectTransform>();
				item.x = component.anchoredPosition.x - component.rect.width / 2f + component2.anchoredPosition.x;
				item.y = component.anchoredPosition.y + component.rect.height / 2f + component2.anchoredPosition.y;
				item.x = vector.x - item.x;
				item.y = vector.y - item.y;
				targetNode = (item, target);
				break;
			}
		}
	}

	public void SetTarget(int taii)
	{
		RectTransform component = Contents.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		Vector2 item = default(Vector2);
		foreach (ScrollCylinderNode lstNode in lstNodes)
		{
			if (lstNode.GetComponent<HRotationScrollNode>().id == taii)
			{
				RectTransform component2 = lstNode.GetComponent<RectTransform>();
				item.x = component.anchoredPosition.x - component.rect.width / 2f + component2.anchoredPosition.x;
				item.y = component.anchoredPosition.y + component.rect.height / 2f + component2.anchoredPosition.y;
				item.x = vector.x - item.x;
				item.y = vector.y - item.y;
				targetNode = (item, lstNode);
				break;
			}
		}
	}

	private Vector3 LimitPos(RectTransform ContentsRt, Vector3 ContentsPosition, int LimitDir)
	{
		RectTransform component = base.transform.GetComponent<RectTransform>();
		if (LimitDir == 0)
		{
			float num = 0f;
			float num2 = 0f;
			if (component.sizeDelta.y / ContentsRt.rect.height % 2f != 0f)
			{
				num = ContentsPosition.y + ContentsRt.rect.height / 2f - component.sizeDelta.y / 2f;
				num2 = ContentsPosition.y - ContentsRt.rect.height / 2f - (0f - component.sizeDelta.y) / 2f;
			}
			else if (lstNodes.First.Value != null)
			{
				RectTransform component2 = lstNodes.First.Value.GetComponent<RectTransform>();
				num = ContentsPosition.y + ContentsRt.rect.height / 2f - component2.sizeDelta.y / 2f - component.sizeDelta.y / 2f;
				num2 = ContentsPosition.y - ContentsRt.rect.height / 2f + component2.sizeDelta.y / 2f - (0f - component.sizeDelta.y) / 2f;
			}
			if (num <= 0f)
			{
				ContentsPosition.y += Mathf.Abs(num);
			}
			else if (num2 >= 0f)
			{
				ContentsPosition.y -= Mathf.Abs(num2);
			}
		}
		else
		{
			float num3 = 0f;
			float num4 = 0f;
			if (component.sizeDelta.x / ContentsRt.rect.width % 2f != 0f)
			{
				num3 = ContentsPosition.x - ContentsRt.rect.width / 2f - (0f - component.sizeDelta.x) / 2f;
				num4 = ContentsPosition.x + ContentsRt.rect.width / 2f - component.sizeDelta.x / 2f;
			}
			else if (lstNodes.First.Value != null)
			{
				RectTransform component3 = lstNodes.First.Value.GetComponent<RectTransform>();
				num3 = ContentsPosition.x + ContentsRt.rect.width / 2f - component3.sizeDelta.x / 2f - component.sizeDelta.x / 2f;
				num4 = ContentsPosition.x - ContentsRt.rect.width / 2f + component3.sizeDelta.x / 2f - (0f - component.sizeDelta.x) / 2f;
			}
			if (num3 >= 0f)
			{
				ContentsPosition.x -= Mathf.Abs(num3);
			}
			else if (num4 <= 0f)
			{
				ContentsPosition.x += Mathf.Abs(num4);
			}
		}
		return ContentsPosition;
	}

	private void InitTargrt(RectTransform ContentsRt, Vector3 position)
	{
		float num = 0f;
		float num2 = 0f;
		RectTransform selectRect = SelectRect;
		if (selectRect.sizeDelta.x / ContentsRt.rect.width % 2f != 0f)
		{
			num = position.x - ContentsRt.rect.width / 2f - (0f - selectRect.sizeDelta.x) / 2f;
		}
		else if (lstNodes.First.Value != null)
		{
			RectTransform component = lstNodes.First.Value.GetComponent<RectTransform>();
			num = position.x - ContentsRt.rect.width / 2f - component.sizeDelta.x / 2f - (0f - selectRect.sizeDelta.x) / 2f;
		}
		if (selectRect.sizeDelta.y / ContentsRt.rect.height % 2f != 0f)
		{
			num2 = position.y + ContentsRt.rect.height / 2f - selectRect.sizeDelta.y / 2f;
		}
		else if (lstNodes.First.Value != null)
		{
			RectTransform component2 = lstNodes.First.Value.GetComponent<RectTransform>();
			num2 = position.y + ContentsRt.rect.height / 2f - component2.sizeDelta.y / 2f - selectRect.sizeDelta.y / 2f;
		}
		num = position.x - num;
		num2 = position.y - num2;
		ScrollCylinderNode item = null;
		HRotationScrollNode[] nodeList = NodeList;
		foreach (HRotationScrollNode hRotationScrollNode in nodeList)
		{
			if (hRotationScrollNode.gameObject.activeSelf)
			{
				item = hRotationScrollNode;
				break;
			}
		}
		targetNode = (new Vector2(num, num2), item);
	}

	public void ListClear()
	{
		lstNodes.Clear();
	}
}
