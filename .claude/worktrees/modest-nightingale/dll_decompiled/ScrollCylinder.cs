using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEx;

public class ScrollCylinder : MonoBehaviour
{
	public GameObject Contents;

	private RectTransform ContentsRt;

	private RectTransform selfRt;

	[Space]
	public RectTransform Atari;

	[Space]
	public ScrollCylinderNode blankObject;

	[Space]
	[SerializeField]
	private GameObject cursor;

	private RectTransform cursorRT;

	[SerializeField]
	private float cursorInitPos;

	[SerializeField]
	private float cursorMoveRange;

	private float cursorTime;

	private int cursorMovePtn;

	[SerializeField]
	private float cursorFirstHurfAnimTimeLimit;

	[SerializeField]
	private float cursorLaterHurfAnimTimeLimit;

	[Space]
	public float moveVal = 0.05f;

	[SerializeField]
	private float moveTime = 0.05f;

	[SerializeField]
	private List<ScrollCylinderNode> lstNodes = new List<ScrollCylinderNode>();

	[SerializeField]
	private List<ScrollCylinderNode> lstBlankNodes = new List<ScrollCylinderNode>();

	[SerializeField]
	private (int, Vector2, ScrollCylinderNode) targetNode;

	private Subject<int> _onValueChange;

	[SerializeField]
	private bool InitList;

	private bool onEnter;

	private GameObject OldRect;

	public bool enableInternalScroll = true;

	public IObservable<int> OnValueChangeAsObservable()
	{
		return _onValueChange ?? (_onValueChange = new Subject<int>());
	}

	private void Awake()
	{
		OldRect = new GameObject("Rect2");
		OldRect.AddComponent<RectTransform>();
		OldRect.transform.SetParent(base.transform, worldPositionStays: false);
		OldRect.transform.localPosition = Vector3.zero;
		OldRect.SetActive(value: false);
	}

	private void Start()
	{
		selfRt = base.transform.GetComponent<RectTransform>();
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

	public void BlankSet(ScrollCylinderNode Node, bool AddLast = false)
	{
		if (!AddLast)
		{
			lstBlankNodes.Clear();
		}
		if (blankObject == null)
		{
			return;
		}
		Vector2 sizeDelta = Node.SelfRt.sizeDelta;
		RectTransform rectTransform = selfRt;
		int[] array = new int[2]
		{
			(int)(rectTransform.rect.height / sizeDelta.y),
			(int)(rectTransform.rect.width / sizeDelta.x)
		};
		int num = Mathf.Max(array[0], array[1]);
		if (lstBlankNodes.Count == ((num % 2 != 0) ? (num - 1) : num))
		{
			return;
		}
		array[0] /= 2;
		array[1] /= 2;
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < array[i]; j++)
			{
				ScrollCylinderNode scrollCylinderNode = UnityEngine.Object.Instantiate(blankObject);
				scrollCylinderNode.transform.SetParent(Contents.transform, worldPositionStays: false);
				scrollCylinderNode.gameObject.SetActive(value: true);
				lstBlankNodes.Add(scrollCylinderNode);
			}
		}
	}

	public void ClearBlank()
	{
		foreach (ScrollCylinderNode lstBlankNode in lstBlankNodes)
		{
			UnityEngine.Object.Destroy(lstBlankNode.gameObject);
		}
		lstBlankNodes.Clear();
	}

	public void ListNodeSet(List<ScrollCylinderNode> hSceneScrollNodes = null)
	{
		lstNodes.Clear();
		if (hSceneScrollNodes != null)
		{
			lstNodes.AddRange(hSceneScrollNodes);
		}
		else
		{
			lstNodes.AddRange(Contents.GetComponentsInChildren<ScrollCylinderNode>());
		}
		if (lstNodes.Count == 0)
		{
			return;
		}
		if (cursor != null)
		{
			cursor.SetActive(lstNodes.Count != 0);
		}
		BlankSet(lstNodes[0], AddLast: true);
		for (int i = 0; i < lstNodes.Count; i++)
		{
			int index = i;
			lstNodes[index].transform.localScale = new Vector3(1f, 1f, 1f);
		}
		for (int j = 0; j < lstBlankNodes.Count; j++)
		{
			int index2 = j;
			lstBlankNodes[index2].transform.localScale = new Vector3(1f, 1f, 1f);
		}
		ContentsRt = Contents.GetComponent<RectTransform>();
		Vector3 ContentsPosition = ContentsRt.anchoredPosition;
		if (!OldRect.activeSelf)
		{
			OldRect.SetActive(value: true);
		}
		RectTransform rt2 = OldRect.GetComponent<RectTransform>();
		OldRectSet(ContentsRt, ref rt2);
		Vector3 position2 = ContentsPosition;
		ContentsPosition = LimitPos(ContentsRt, ContentsPosition);
		ContentsRt.anchoredPosition = ContentsPosition;
		UITrigger.TriggerEvent triggerEvent = new UITrigger.TriggerEvent();
		for (int k = 0; k < lstNodes.Count; k++)
		{
			int index3 = k;
			PointerEnterTrigger component = lstNodes[index3].gameObject.GetComponent<PointerEnterTrigger>();
			lstNodes[index3].gameObject.SetActive(value: true);
			if (!(component != null))
			{
				PointerEnterTrigger pointerEnterTrigger = lstNodes[index3].gameObject.AddComponent<PointerEnterTrigger>();
				triggerEvent = new UITrigger.TriggerEvent();
				pointerEnterTrigger.Triggers.Add(triggerEvent);
				triggerEvent.AddListener(delegate
				{
					onEnter = true;
				});
			}
		}
		if (ContentsRt.rect.width == 0f || ContentsRt.rect.height == 0f)
		{
			Observable.NextFrame().Subscribe(delegate
			{
				InitTargrt(ContentsRt, ContentsPosition);
				OldRect.SetActive(value: false);
			});
			return;
		}
		Observable.NextFrame().Subscribe(delegate
		{
			rt2.sizeDelta = ContentsRt.sizeDelta;
			InitTargrt(rt2, position2);
			OldRect.SetActive(value: false);
		});
	}

	private void Update()
	{
		if (lstNodes.Count != 0)
		{
			MoveContents();
			ChangeNode();
		}
	}

	private void MoveContents()
	{
		Vector3 contentsPosition = ContentsRt.anchoredPosition;
		if (enableInternalScroll)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (onEnter)
			{
				if (axis < 0f && targetNode.Item1 < lstNodes.Count - 1)
				{
					int num = targetNode.Item1 + 1;
					RectTransform rectTransform = lstNodes[num].SelfRt;
					Vector2 item = default(Vector2);
					item.x = ContentsRt.anchoredPosition.x - ContentsRt.sizeDelta.x / 2f + rectTransform.anchoredPosition.x;
					item.y = ContentsRt.anchoredPosition.y + ContentsRt.sizeDelta.y / 2f + rectTransform.anchoredPosition.y;
					item.x = contentsPosition.x - item.x;
					item.y = contentsPosition.y - item.y;
					targetNode = (num, item, lstNodes[num]);
					_onValueChange?.OnNext(num);
				}
				else if (axis > 0f && targetNode.Item1 > 0)
				{
					int num2 = targetNode.Item1 - 1;
					RectTransform rectTransform2 = lstNodes[num2].SelfRt;
					Vector2 item2 = default(Vector2);
					item2.x = ContentsRt.anchoredPosition.x - ContentsRt.sizeDelta.x / 2f + rectTransform2.anchoredPosition.x;
					item2.y = ContentsRt.anchoredPosition.y + ContentsRt.sizeDelta.y / 2f + rectTransform2.anchoredPosition.y;
					item2.x = contentsPosition.x - item2.x;
					item2.y = contentsPosition.y - item2.y;
					targetNode = (num2, item2, lstNodes[num2]);
					_onValueChange?.OnNext(num2);
				}
			}
		}
		Vector2 item3 = targetNode.Item2;
		float currentVelocity = 0f;
		contentsPosition.x = Mathf.SmoothDamp(contentsPosition.x, item3.x, ref currentVelocity, moveTime, float.PositiveInfinity, Time.deltaTime);
		currentVelocity = 0f;
		contentsPosition.y = Mathf.SmoothDamp(contentsPosition.y, item3.y, ref currentVelocity, moveTime, float.PositiveInfinity, Time.deltaTime);
		contentsPosition = LimitPos(ContentsRt, contentsPosition);
		ContentsRt.anchoredPosition = contentsPosition;
	}

	private void ChangeNode()
	{
		float deltaTime = Time.deltaTime;
		ChangeNodeColor();
		ChangeNodeScl();
		if (cursor != null)
		{
			CursorMove(deltaTime);
		}
	}

	private void ChangeNodeColor()
	{
		for (int i = 0; i < lstNodes.Count; i++)
		{
			int num = i;
			if (num == targetNode.Item1)
			{
				lstNodes[num].ChangeBGAlpha(0);
			}
			else if (num == targetNode.Item1 - 1 || num == targetNode.Item1 + 1)
			{
				lstNodes[num].ChangeBGAlpha(1);
			}
			else if (num == targetNode.Item1 - 2 || num == targetNode.Item1 + 2)
			{
				lstNodes[num].ChangeBGAlpha(2);
			}
			else
			{
				lstNodes[num].ChangeBGAlpha(3);
			}
		}
		int num2 = lstBlankNodes.Count / 2;
		for (int j = 0; j < lstBlankNodes.Count; j++)
		{
			if (lstNodes.Count == 1)
			{
				if (j == num2 - 2 || j == num2 + 1)
				{
					lstBlankNodes[j].ChangeBGAlpha(2);
				}
				else if (j == num2 - 1 || j == num2)
				{
					lstBlankNodes[j].ChangeBGAlpha(1);
				}
			}
			else if (targetNode.Item1 == 0)
			{
				if (j == num2 - 2)
				{
					lstBlankNodes[j].ChangeBGAlpha(2);
				}
				else if (j == num2 - 1)
				{
					lstBlankNodes[j].ChangeBGAlpha(1);
				}
				else
				{
					lstBlankNodes[j].ChangeBGAlpha(3);
				}
			}
			else if (targetNode.Item1 == lstNodes.Count - 1)
			{
				if (j == num2)
				{
					lstBlankNodes[j].ChangeBGAlpha(1);
				}
				else if (j == num2 + 1)
				{
					lstBlankNodes[j].ChangeBGAlpha(2);
				}
				else
				{
					lstBlankNodes[j].ChangeBGAlpha(3);
				}
			}
			else
			{
				lstBlankNodes[j].ChangeBGAlpha(3);
			}
		}
	}

	private void ChangeNodeScl()
	{
		for (int i = 0; i < lstNodes.Count; i++)
		{
			int num = i;
			if (num == targetNode.Item1)
			{
				lstNodes[num].ChangeScale(0);
			}
			else if (num == targetNode.Item1 - 1 || num == targetNode.Item1 + 1)
			{
				lstNodes[num].ChangeScale(1);
			}
			else if (num == targetNode.Item1 - 2 || num == targetNode.Item1 + 2)
			{
				lstNodes[num].ChangeScale(2);
			}
			else
			{
				lstNodes[num].ChangeScale(3);
			}
		}
		int num2 = lstBlankNodes.Count / 2;
		for (int j = 0; j < lstBlankNodes.Count; j++)
		{
			if (lstNodes.Count == 1)
			{
				if (j == num2 - 2 || j == num2 + 1)
				{
					lstBlankNodes[j].ChangeScale(2);
				}
				else if (j == num2 - 1 || j == num2)
				{
					lstBlankNodes[j].ChangeScale(1);
				}
			}
			else if (targetNode.Item1 == 0)
			{
				if (j == num2 - 2)
				{
					lstBlankNodes[j].ChangeScale(2);
				}
				else if (j == num2 - 1)
				{
					lstBlankNodes[j].ChangeScale(1);
				}
				else
				{
					lstBlankNodes[j].ChangeScale(3);
				}
			}
			else if (targetNode.Item1 == lstNodes.Count - 1)
			{
				if (j == num2)
				{
					lstBlankNodes[j].ChangeScale(1);
				}
				else if (j == num2 + 1)
				{
					lstBlankNodes[j].ChangeScale(2);
				}
				else
				{
					lstBlankNodes[j].ChangeScale(3);
				}
			}
			else
			{
				lstBlankNodes[j].ChangeScale(3);
			}
		}
	}

	private void CursorMove(float deltaTime)
	{
		float num = 0f;
		if (cursorMovePtn == 0)
		{
			cursorTime += deltaTime / cursorFirstHurfAnimTimeLimit;
			num = Mathf.InverseLerp(0f, 1f, cursorTime);
			if (num == 1f)
			{
				cursorMovePtn = 1;
			}
		}
		else
		{
			cursorTime -= deltaTime / cursorLaterHurfAnimTimeLimit;
			num = Mathf.InverseLerp(0f, 1f, cursorTime);
			if (num == 0f)
			{
				cursorMovePtn = 0;
			}
		}
		Vector3 zero = Vector3.zero;
		if (cursorRT == null)
		{
			cursorRT = cursor.GetComponent<RectTransform>();
		}
		zero = cursorRT.anchoredPosition;
		zero.x = Mathf.Lerp(cursorInitPos - cursorMoveRange / 2f, cursorInitPos + cursorMoveRange / 2f, num);
		cursorRT.anchoredPosition = zero;
	}

	public List<ScrollCylinderNode> GetList()
	{
		return lstNodes;
	}

	public (int, Vector2, ScrollCylinderNode) GetTarget()
	{
		return targetNode;
	}

	public void SetTarget(ScrollCylinderNode target)
	{
		RectTransform rectTransform = selfRt;
		Vector3 vector = rectTransform.anchoredPosition;
		Vector2 item = default(Vector2);
		for (int i = 0; i < lstNodes.Count; i++)
		{
			int num = i;
			if (!(lstNodes[num] != target))
			{
				RectTransform rectTransform2 = lstNodes[num].SelfRt;
				item.x = rectTransform.anchoredPosition.x - rectTransform.rect.width / 2f + rectTransform2.anchoredPosition.x;
				item.y = rectTransform.anchoredPosition.y + rectTransform.rect.height / 2f + rectTransform2.anchoredPosition.y;
				item.x = vector.x - item.x;
				item.y = vector.y - item.y;
				targetNode = (num, item, target);
				_onValueChange?.OnNext(num);
				break;
			}
		}
	}

	private Vector3 LimitPos(RectTransform ContentsRt, Vector3 ContentsPosition)
	{
		RectTransform rectTransform = selfRt;
		float num = 0f;
		float num2 = 0f;
		if (rectTransform.sizeDelta.y / ContentsRt.rect.height % 2f != 0f)
		{
			num = ContentsPosition.y + ContentsRt.rect.height / 2f - rectTransform.sizeDelta.y / 2f;
			num2 = ContentsPosition.y - ContentsRt.rect.height / 2f - (0f - rectTransform.sizeDelta.y) / 2f;
		}
		else if (lstNodes[0] != null)
		{
			RectTransform rectTransform2 = lstNodes[0].SelfRt;
			num = ContentsPosition.y + ContentsRt.rect.height / 2f - rectTransform2.sizeDelta.y / 2f - rectTransform.sizeDelta.y / 2f;
			num2 = ContentsPosition.y - ContentsRt.rect.height / 2f + rectTransform2.sizeDelta.y / 2f - (0f - rectTransform.sizeDelta.y) / 2f;
		}
		if (num <= 0f)
		{
			ContentsPosition.y += Mathf.Abs(num);
		}
		else if (num2 >= 0f)
		{
			ContentsPosition.y -= Mathf.Abs(num2);
		}
		float num3 = 0f;
		float num4 = 0f;
		if (rectTransform.sizeDelta.x / ContentsRt.rect.width % 2f != 0f)
		{
			num3 = ContentsPosition.x - ContentsRt.rect.width / 2f - (0f - rectTransform.sizeDelta.x) / 2f;
			num4 = ContentsPosition.x + ContentsRt.rect.width / 2f - rectTransform.sizeDelta.x / 2f;
		}
		else if (lstNodes[0] != null)
		{
			RectTransform rectTransform3 = lstNodes[0].SelfRt;
			num3 = ContentsPosition.x + ContentsRt.rect.width / 2f - rectTransform3.sizeDelta.x / 2f - rectTransform.sizeDelta.x / 2f;
			num4 = ContentsPosition.x - ContentsRt.rect.width / 2f + rectTransform3.sizeDelta.x / 2f - (0f - rectTransform.sizeDelta.x) / 2f;
		}
		if (num3 >= 0f)
		{
			ContentsPosition.x -= Mathf.Abs(num3);
		}
		else if (num4 <= 0f)
		{
			ContentsPosition.x += Mathf.Abs(num4);
		}
		return ContentsPosition;
	}

	private void InitTargrt(RectTransform ContentsRt, Vector3 position)
	{
		float num = 0f;
		float num2 = 0f;
		RectTransform rectTransform = selfRt;
		if (rectTransform.sizeDelta.x / ContentsRt.rect.width % 2f != 0f)
		{
			num = position.x - ContentsRt.rect.width / 2f - (0f - rectTransform.sizeDelta.x) / 2f;
		}
		else if (lstNodes[0] != null)
		{
			RectTransform rectTransform2 = lstNodes[0].SelfRt;
			num = position.x - ContentsRt.rect.width / 2f - rectTransform2.sizeDelta.x / 2f - (0f - rectTransform.sizeDelta.x) / 2f;
		}
		if (rectTransform.sizeDelta.y / ContentsRt.rect.height % 2f != 0f)
		{
			num2 = position.y + ContentsRt.rect.height / 2f - rectTransform.sizeDelta.y / 2f;
		}
		else if (lstNodes[0] != null)
		{
			RectTransform rectTransform3 = lstNodes[0].SelfRt;
			num2 = position.y + ContentsRt.rect.height / 2f - rectTransform3.sizeDelta.y / 2f - rectTransform.sizeDelta.y / 2f;
		}
		num = position.x - num;
		num2 = position.y - num2;
		targetNode = (0, new Vector2(num, num2), lstNodes[0]);
		_onValueChange?.OnNext(0);
	}

	private void OldRectSet(RectTransform src, ref RectTransform newRect)
	{
		newRect.anchoredPosition = src.anchoredPosition;
		newRect.anchoredPosition3D = src.anchoredPosition3D;
		newRect.anchorMax = src.anchorMax;
		newRect.anchorMin = src.anchorMin;
		newRect.offsetMax = src.offsetMax;
		newRect.offsetMin = src.offsetMin;
		newRect.pivot = src.pivot;
		newRect.sizeDelta = src.sizeDelta;
	}
}
