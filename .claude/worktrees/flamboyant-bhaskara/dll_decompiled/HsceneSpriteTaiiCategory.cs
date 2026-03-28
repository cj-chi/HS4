using System.Collections.Generic;
using ReMotion;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

public class HsceneSpriteTaiiCategory : HSceneSpriteCategory
{
	[SerializeField]
	private Text CategoryNameLabel;

	[SerializeField]
	private HSceneFlagCtrl ctrlFlag;

	[SerializeField]
	private HSceneSprite hSceneSprite;

	[SerializeField]
	private RotationScroll hSceneScroll;

	[SerializeField]
	private Button leftArrow;

	[SerializeField]
	private Image imgLeftArrow;

	[SerializeField]
	private PointerDownAction leftArrowAction;

	[SerializeField]
	private PointerDownAction rightArrowAction;

	[SerializeField]
	private Button rightArrow;

	[SerializeField]
	private Image imgRightArrow;

	[Space(2f)]
	[SerializeField]
	private bool CanChangeColor = true;

	[SerializeField]
	[Tooltip("1つ目:通常\n2つ目:体位１つのみ\n3つ目:クリックorスクロール時")]
	private Color[] arrowColor = new Color[3]
	{
		Color.white,
		Color.gray,
		Color.green
	};

	private int[] ArrowChangeState = new int[2] { -1, -1 };

	private float[] ArrowAnimTime = new float[2];

	[SerializeField]
	private float ArrowBigAnimTimeLimit;

	[SerializeField]
	private float ArrowWaitAnimTimeLimit;

	[SerializeField]
	private float ArrowSmallAnimTimeLimit;

	private LinkedListNode<ScrollCylinderNode> target;

	private HRotationScrollNode targetNode;

	private LinkedList<ScrollCylinderNode> lstScrollNode = new LinkedList<ScrollCylinderNode>();

	private Dictionary<ScrollCylinderNode, Image> lstScrollNodeImg = new Dictionary<ScrollCylinderNode, Image>();

	private LinkedListNode<ScrollCylinderNode> tmp;

	private LinkedListNode<ScrollCylinderNode> btOld;

	private Dictionary<int, Image> lstButtonImgs = new Dictionary<int, Image>();

	public void Init()
	{
		lstScrollNode = hSceneScroll.GetList();
		lstScrollNodeImg = new Dictionary<ScrollCylinderNode, Image>();
		foreach (ScrollCylinderNode item in lstScrollNode)
		{
			lstScrollNodeImg.Add(item, item.GetComponent<Image>());
		}
		if (!leftArrow.interactable)
		{
			leftArrow.interactable = true;
			imgLeftArrow.color = arrowColor[0];
		}
		if (!rightArrow.interactable)
		{
			rightArrow.interactable = true;
			imgRightArrow.color = arrowColor[0];
		}
		if (lstScrollNode.Count <= 1)
		{
			leftArrow.interactable = false;
			rightArrow.interactable = false;
		}
		leftArrow.onClick.AddListener(delegate
		{
			btOld = new LinkedListNode<ScrollCylinderNode>(target.Value);
			LinkedListNode<ScrollCylinderNode> linkedListNode;
			if (lstScrollNode.Count >= 3)
			{
				linkedListNode = target.Previous;
			}
			else
			{
				RectTransform component = target.Value.GetComponent<RectTransform>();
				linkedListNode = target.Previous ?? lstScrollNode.Last;
				linkedListNode.Value.GetComponent<RectTransform>().anchoredPosition = component.anchoredPosition + new Vector2((hSceneScroll.scrollMode == ScrollDir.Vertical) ? 0f : ((0f - component.rect.width) * 2f), (hSceneScroll.scrollMode == ScrollDir.Horizontal) ? 0f : (component.rect.height * 2f));
				if (linkedListNode == lstScrollNode.Last)
				{
					LinkedListNode<ScrollCylinderNode> node = new LinkedListNode<ScrollCylinderNode>(lstScrollNode.Last.Value);
					lstScrollNode.RemoveLast();
					lstScrollNode.AddFirst(node);
					linkedListNode = lstScrollNode.First;
				}
			}
			if (btOld.Value == linkedListNode.Next?.Value)
			{
				ArrowChangeState[0] = 0;
				ArrowAnimTime[0] = 0f;
			}
			hSceneScroll.SetTarget(linkedListNode.Value);
		});
		rightArrow.onClick.AddListener(delegate
		{
			btOld = new LinkedListNode<ScrollCylinderNode>(target.Value);
			LinkedListNode<ScrollCylinderNode> linkedListNode;
			if (lstScrollNode.Count >= 3)
			{
				linkedListNode = target.Next;
			}
			else
			{
				RectTransform component = target.Value.GetComponent<RectTransform>();
				linkedListNode = target.Next ?? lstScrollNode.First;
				linkedListNode.Value.GetComponent<RectTransform>().anchoredPosition = component.anchoredPosition + new Vector2((hSceneScroll.scrollMode == ScrollDir.Vertical) ? 0f : (component.rect.width * 2f), (hSceneScroll.scrollMode == ScrollDir.Horizontal) ? 0f : ((0f - component.rect.height) * 2f));
				if (linkedListNode == lstScrollNode.First)
				{
					LinkedListNode<ScrollCylinderNode> node = new LinkedListNode<ScrollCylinderNode>(lstScrollNode.First.Value);
					lstScrollNode.RemoveFirst();
					lstScrollNode.AddLast(node);
					linkedListNode = lstScrollNode.Last;
				}
			}
			if (btOld.Value == linkedListNode.Previous?.Value)
			{
				ArrowChangeState[1] = 0;
				ArrowAnimTime[1] = 0f;
			}
			hSceneScroll.SetTarget(linkedListNode.Value);
		});
		lstButtonImgs = new Dictionary<int, Image>();
		for (int num = 0; num < lstButton.Count; num++)
		{
			lstButtonImgs.Add(num, lstButton[num].GetComponent<Image>());
		}
		if (leftArrowAction != null && !leftArrowAction.listAction.Contains(hSceneSprite.OnClickSliderSelect))
		{
			leftArrowAction.listAction.Add(hSceneSprite.OnClickSliderSelect);
		}
		if (rightArrowAction != null && !rightArrowAction.listAction.Contains(hSceneSprite.OnClickSliderSelect))
		{
			rightArrowAction.listAction.Add(hSceneSprite.OnClickSliderSelect);
		}
	}

	private void Update()
	{
		if (hSceneScroll.GetTarget().Item2 == null)
		{
			return;
		}
		if (lstScrollNode.Count <= 1)
		{
			if (leftArrow.interactable || rightArrow.interactable)
			{
				leftArrow.interactable = false;
				rightArrow.interactable = false;
			}
		}
		else if (!leftArrow.interactable || !rightArrow.interactable)
		{
			leftArrow.interactable = true;
			rightArrow.interactable = true;
		}
		tmp = ((target != null) ? target : new LinkedListNode<ScrollCylinderNode>(null));
		target = lstScrollNode.Find(hSceneScroll.GetTarget().Item2);
		if (target == null && lstScrollNode.Count > 0)
		{
			target = lstScrollNode.First;
			hSceneScroll.SetTarget(lstScrollNode.First.Value);
		}
		if (tmp.Value != null && target != null && target.Value != null)
		{
			targetNode = target.Value as HRotationScrollNode;
			HRotationScrollNode hRotationScrollNode = tmp.Value as HRotationScrollNode;
			if (targetNode != null && hRotationScrollNode != null && targetNode.id != hRotationScrollNode.id)
			{
				if (tmp.Value == target.Previous?.Value)
				{
					ArrowChangeState[1] = 0;
					ArrowAnimTime[1] = 0f;
				}
				else if (tmp.Value == target.Next?.Value)
				{
					ArrowChangeState[0] = 0;
					ArrowAnimTime[0] = 0f;
				}
			}
		}
		foreach (ScrollCylinderNode item in lstScrollNode)
		{
			if (item.gameObject != target.Value.gameObject)
			{
				lstScrollNodeImg[item].raycastTarget = false;
			}
			else
			{
				lstScrollNodeImg[item].raycastTarget = true;
			}
		}
		float deltaTime = Time.deltaTime;
		if (CanChangeColor)
		{
			if (ArrowChangeState[0] != -1)
			{
				ButtonReaction(imgLeftArrow, ref ArrowChangeState[0], ref ArrowAnimTime[0], deltaTime);
			}
			if (ArrowChangeState[1] != -1)
			{
				ButtonReaction(imgRightArrow, ref ArrowChangeState[1], ref ArrowAnimTime[1], deltaTime);
			}
		}
		if (!(targetNode != null))
		{
			return;
		}
		if (CategoryNameLabel != null)
		{
			switch (targetNode.id)
			{
			case 0:
				CategoryNameLabel.text = "愛撫";
				break;
			case 1:
				CategoryNameLabel.text = "奉仕";
				break;
			case 2:
				CategoryNameLabel.text = "挿入";
				break;
			case 3:
				CategoryNameLabel.text = "特殊";
				break;
			case 4:
				CategoryNameLabel.text = "レズ";
				break;
			case 5:
				CategoryNameLabel.text = "女複数";
				break;
			case 6:
				CategoryNameLabel.text = "男複数";
				break;
			case 7:
				CategoryNameLabel.text = "女主導";
				break;
			}
		}
		if (ctrlFlag != null && ctrlFlag.categoryMotionList != targetNode.id && hSceneSprite != null)
		{
			targetNode.transform.SetSiblingIndex(lstScrollNode.Count - 1);
			hSceneSprite.ChangeMotionCategoryShow(targetNode.id);
		}
	}

	public RotationScroll GetHScroll()
	{
		return hSceneScroll;
	}

	private void ButtonReaction(Image _image, ref int ArrowChangeState, ref float ArrowAnimTime, float deltaTime)
	{
		if (ArrowChangeState == 0)
		{
			ArrowAnimTime += deltaTime / ArrowBigAnimTimeLimit;
			float time = Mathf.InverseLerp(0f, 1f, ArrowAnimTime);
			time = EasingFunctions.EaseOutQuint(time, 1f);
			_image.color = new Color(Mathf.Lerp(arrowColor[0].r, arrowColor[2].r, time), Mathf.Lerp(arrowColor[0].g, arrowColor[2].g, time), Mathf.Lerp(arrowColor[0].b, arrowColor[2].b, time), Mathf.Lerp(arrowColor[0].a, arrowColor[2].a, time));
			_image.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, time);
			if (time == 1f)
			{
				ArrowAnimTime = 0f;
				ArrowChangeState = 1;
			}
		}
		else if (ArrowChangeState == 1)
		{
			ArrowAnimTime += deltaTime / ArrowWaitAnimTimeLimit;
			if (Mathf.InverseLerp(0f, 1f, ArrowAnimTime) == 1f)
			{
				ArrowAnimTime = 1f;
				ArrowChangeState = 2;
			}
		}
		else if (ArrowChangeState == 2)
		{
			ArrowAnimTime -= deltaTime / ArrowSmallAnimTimeLimit;
			float time2 = Mathf.InverseLerp(0f, 1f, ArrowAnimTime);
			time2 = EasingFunctions.EaseOutQuint(time2, 1f);
			_image.color = new Color(Mathf.Lerp(arrowColor[0].r, arrowColor[2].r, time2), Mathf.Lerp(arrowColor[0].g, arrowColor[2].g, time2), Mathf.Lerp(arrowColor[0].b, arrowColor[2].b, time2), Mathf.Lerp(arrowColor[0].a, arrowColor[2].a, time2));
			_image.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, time2);
			if (time2 == 0f)
			{
				ArrowAnimTime = 0f;
				ArrowChangeState = -1;
			}
		}
	}

	public override void SetActive(bool _active, int _array = -1)
	{
		Image image = null;
		if (_array < 0)
		{
			for (int i = 0; i < lstButton.Count; i++)
			{
				image = lstButtonImgs[i];
				if (image.enabled != _active)
				{
					image.enabled = _active;
				}
			}
		}
		else if (lstButton.Count > _array)
		{
			image = lstButtonImgs[_array];
			if (image.enabled != _active)
			{
				image.enabled = _active;
			}
		}
	}

	public void EndProc()
	{
		leftArrow.onClick.RemoveAllListeners();
		rightArrow.onClick.RemoveAllListeners();
	}
}
