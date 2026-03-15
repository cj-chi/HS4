using UnityEngine;
using UnityEngine.UI;

namespace UIAnimatorCore;

public class TransformAndFadeAlphaAnimationStep : TransformAnimationStep
{
	[SerializeField]
	private UIGraphicRendererTargetData[] m_targetGraphicsData;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_animatedAlpha = 0.2f;

	public new static string EditorDisplayName => "Transform & Fade";

	public override string StepTitleDisplay => EditorDisplayName;

	protected override bool SetStepAllValuesFromCurrentState(GameObject[] a_targetObjects)
	{
		base.SetStepAllValuesFromCurrentState(a_targetObjects);
		GrabRendererReferences(a_targetObjects);
		return true;
	}

	protected override bool SetStepDefaultValuesFromCurrentState(GameObject[] a_targetObjects)
	{
		base.SetStepDefaultValuesFromCurrentState(a_targetObjects);
		GrabRendererReferences(a_targetObjects);
		return true;
	}

	protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
	{
		if (m_targetGraphicsData != null)
		{
			base.SetAnimation(a_targetIndex, a_easedProgress);
			m_targetGraphicsData[a_targetIndex].SetFadeProgress(m_animatedAlpha, a_easedProgress);
		}
	}

	private void GrabRendererReferences(GameObject[] a_targetObjects)
	{
		m_targetGraphicsData = new UIGraphicRendererTargetData[a_targetObjects.Length];
		for (int i = 0; i < a_targetObjects.Length; i++)
		{
			if (!(a_targetObjects[i] == null))
			{
				m_targetGraphicsData[i] = new UIGraphicRendererTargetData(a_targetObjects[i].GetComponentsInChildren<Graphic>(includeInactive: true));
			}
		}
	}
}
