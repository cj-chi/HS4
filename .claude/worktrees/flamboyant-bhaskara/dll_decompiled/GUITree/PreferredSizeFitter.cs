using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GUITree;

[AddComponentMenu("GUITree/Preferred Size Fitter", 1001)]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class PreferredSizeFitter : UIBehaviour, ITreeLayoutElement, ILayoutElement, ILayoutSelfController, ILayoutController
{
	[SerializeField]
	private float m_PreferredWidth = -1f;

	[SerializeField]
	private float m_PreferredHeight = -1f;

	[NonSerialized]
	private RectTransform m_Rect;

	private DrivenRectTransformTracker m_Tracker;

	public virtual float minWidth => m_PreferredWidth;

	public virtual float minHeight => m_PreferredHeight;

	public virtual float preferredWidth
	{
		get
		{
			return m_PreferredWidth;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_PreferredWidth, value))
			{
				SetDirty();
			}
		}
	}

	public virtual float preferredHeight
	{
		get
		{
			return m_PreferredHeight;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_PreferredHeight, value))
			{
				SetDirty();
			}
		}
	}

	public virtual float flexibleWidth => m_PreferredWidth;

	public virtual float flexibleHeight => m_PreferredHeight;

	public virtual int layoutPriority => int.MaxValue;

	private RectTransform rectTransform
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

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	protected PreferredSizeFitter()
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

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	private void HandleSelfFittingAlongAxis(int axis)
	{
		m_Tracker.Add(this, rectTransform, (axis == 0) ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY);
		rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(this, axis));
	}

	public virtual void SetLayoutHorizontal()
	{
		m_Tracker.Clear();
		HandleSelfFittingAlongAxis(0);
	}

	public virtual void SetLayoutVertical()
	{
		HandleSelfFittingAlongAxis(1);
	}

	protected void SetDirty()
	{
		if (IsActive() && rectTransform != null)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}
}
