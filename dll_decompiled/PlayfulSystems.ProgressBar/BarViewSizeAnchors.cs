using UnityEngine;

namespace PlayfulSystems.ProgressBar;

[RequireComponent(typeof(RectTransform))]
public class BarViewSizeAnchors : ProgressBarProView
{
	public enum FillType
	{
		LeftToRight,
		RightToLeft,
		TopToBottom,
		BottomToTop
	}

	[SerializeField]
	protected RectTransform rectTrans;

	[SerializeField]
	protected FillType fillType;

	[SerializeField]
	private bool hideOnEmpty = true;

	[SerializeField]
	private bool useDiscreteSteps;

	[SerializeField]
	private int numSteps = 10;

	protected DrivenRectTransformTracker m_Tracker;

	protected bool isDisplaySizeZero;

	public override bool CanUpdateView(float currentValue, float targetValue)
	{
		if (!base.isActiveAndEnabled)
		{
			return isDisplaySizeZero;
		}
		return true;
	}

	public override void UpdateView(float currentValue, float targetValue)
	{
		if (hideOnEmpty && currentValue <= 0f)
		{
			rectTrans.gameObject.SetActive(value: false);
			isDisplaySizeZero = true;
		}
		else
		{
			isDisplaySizeZero = false;
			rectTrans.gameObject.SetActive(value: true);
			SetPivot(0f, currentValue);
		}
	}

	protected void SetPivot(float startEdge, float endEdge)
	{
		if (useDiscreteSteps)
		{
			startEdge = Mathf.Round(startEdge * (float)numSteps) / (float)numSteps;
			endEdge = Mathf.Round(endEdge * (float)numSteps) / (float)numSteps;
		}
		UpdateTracker();
		switch (fillType)
		{
		case FillType.LeftToRight:
			rectTrans.anchorMin = new Vector2(startEdge, rectTrans.anchorMin.y);
			rectTrans.anchorMax = new Vector2(endEdge, rectTrans.anchorMax.y);
			break;
		case FillType.RightToLeft:
			rectTrans.anchorMin = new Vector2(1f - endEdge, rectTrans.anchorMin.y);
			rectTrans.anchorMax = new Vector2(1f - startEdge, rectTrans.anchorMax.y);
			break;
		case FillType.BottomToTop:
			rectTrans.anchorMin = new Vector2(rectTrans.anchorMin.x, startEdge);
			rectTrans.anchorMax = new Vector2(rectTrans.anchorMax.x, endEdge);
			break;
		case FillType.TopToBottom:
			rectTrans.anchorMin = new Vector2(rectTrans.anchorMin.x, 1f - endEdge);
			rectTrans.anchorMax = new Vector2(rectTrans.anchorMax.x, 1f - startEdge);
			break;
		}
	}

	private void UpdateTracker()
	{
		if (fillType == FillType.LeftToRight || fillType == FillType.RightToLeft)
		{
			m_Tracker.Add(this, rectTrans, DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMaxX);
		}
		else
		{
			m_Tracker.Add(this, rectTrans, DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchorMaxY);
		}
	}

	private void OnDisable()
	{
		m_Tracker.Clear();
	}
}
