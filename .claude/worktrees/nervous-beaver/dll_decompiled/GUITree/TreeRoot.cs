using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GUITree;

[AddComponentMenu("GUITree/Tree Root", 1003)]
[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class TreeRoot : UIBehaviour, ITreeLayoutElement, ILayoutElement, ILayoutGroup, ILayoutController
{
	private RectTransform m_Rect;

	protected DrivenRectTransformTracker m_Tracker;

	private Vector2 m_TotalPreferredSize = Vector2.zero;

	private List<TreeNode> m_TreeLayoutElement = new List<TreeNode>();

	[SerializeField]
	protected float m_Spacing;

	protected RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	protected List<TreeNode> treeChildren => m_TreeLayoutElement;

	public float spacing
	{
		get
		{
			return m_Spacing;
		}
		set
		{
			SetProperty(ref m_Spacing, value);
		}
	}

	public virtual float minWidth => preferredWidth;

	public virtual float preferredWidth => m_TotalPreferredSize[0];

	public virtual float flexibleWidth => preferredWidth;

	public virtual float minHeight => preferredHeight;

	public virtual float preferredHeight => m_TotalPreferredSize[1];

	public virtual float flexibleHeight => preferredHeight;

	public virtual int layoutPriority => 0;

	private bool isRootLayoutGroup
	{
		get
		{
			if (base.transform.parent == null)
			{
				return true;
			}
			return base.transform.parent.GetComponent(typeof(ILayoutGroup)) == null;
		}
	}

	protected void CalcAlongAxis(int axis)
	{
		float num = 0f;
		bool flag = axis != 1;
		for (int i = 0; i < treeChildren.Count; i++)
		{
			float preferredSize = LayoutUtility.GetPreferredSize(treeChildren[i], axis);
			num = ((!flag) ? (num + (preferredSize + spacing)) : Mathf.Max(preferredSize, num));
		}
		if (!flag && treeChildren.Count > 0)
		{
			num -= spacing;
		}
		m_TotalPreferredSize[axis] = num;
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
		m_TreeLayoutElement.Clear();
		for (int i = 0; i < rectTransform.childCount; i++)
		{
			TreeNode component = rectTransform.GetChild(i).GetComponent<TreeNode>();
			if (!(component == null) && component.IsActive())
			{
				m_TreeLayoutElement.Add(component);
			}
		}
		CalcAlongAxis(0);
	}

	public void CalculateLayoutInputVertical()
	{
		CalcAlongAxis(1);
	}

	public void SetLayoutHorizontal()
	{
		m_Tracker.Clear();
	}

	public void SetLayoutVertical()
	{
		float num = 0f;
		for (int i = 0; i < treeChildren.Count; i++)
		{
			TreeNode treeNode = treeChildren[i];
			float preferredSize = LayoutUtility.GetPreferredSize(treeNode, 1);
			m_Tracker.Add(this, treeNode.rectTransform, DrivenTransformProperties.AnchoredPositionY);
			treeNode.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, num, preferredSize);
			num += preferredSize + spacing;
		}
	}

	protected TreeRoot()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		m_Tracker.Clear();
		if (rectTransform != null)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
		base.OnDisable();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		SetDirty();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (isRootLayoutGroup)
		{
			SetDirty();
		}
	}

	protected virtual void OnTransformChildrenChanged()
	{
		SetDirty();
	}

	protected void SetProperty<T>(ref T currentValue, T newValue)
	{
		if (currentValue == null && newValue == null)
		{
			return;
		}
		if (currentValue != null)
		{
			object obj = newValue;
			if (currentValue.Equals(obj))
			{
				return;
			}
		}
		currentValue = newValue;
		SetDirty();
	}

	protected void SetDirty()
	{
		if (IsActive() && rectTransform != null)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}
}
