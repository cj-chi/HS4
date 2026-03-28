using UnityEngine;

namespace UIAnimatorCore;

public class PopAnimationStep : TransitionAnimationStep
{
	private AnimationCurve m_animCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.66f, 1.22f), new Keyframe(1f, 1f));

	[SerializeField]
	private Vector3[] m_masterScales;

	[SerializeField]
	private RectTransform[] m_targetTransforms;

	public new static string EditorDisplayName => "Pop";

	public override string StepTitleDisplay => EditorDisplayName;

	public override bool SetCustomInitialDuration => true;

	public override float CustomInitialDuration => 0.5f;

	public override bool UseEasing => false;

	protected override bool SetStepAllValuesFromCurrentState(GameObject[] a_targetObjects)
	{
		return InitialiseVariableStates(a_targetObjects, a_onlyMasterValues: false);
	}

	protected override bool SetStepDefaultValuesFromCurrentState(GameObject[] a_targetObjects)
	{
		return InitialiseVariableStates(a_targetObjects, a_onlyMasterValues: true);
	}

	private bool InitialiseVariableStates(GameObject[] a_targetObjects, bool a_onlyMasterValues)
	{
		m_targetTransforms = new RectTransform[a_targetObjects.Length];
		if (a_targetObjects.Length == 0)
		{
			return false;
		}
		m_masterScales = new Vector3[a_targetObjects.Length];
		for (int i = 0; i < a_targetObjects.Length; i++)
		{
			if (!(a_targetObjects[i] == null))
			{
				RectTransform component = a_targetObjects[i].GetComponent<RectTransform>();
				m_targetTransforms[i] = component;
				m_masterScales[i] = component.localScale;
			}
		}
		return true;
	}

	protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
	{
		if (m_targetTransforms != null && a_targetIndex < m_targetTransforms.Length && !(m_targetTransforms[a_targetIndex] == null))
		{
			m_targetTransforms[a_targetIndex].localScale = Vector3.LerpUnclamped(Vector3.zero, m_masterScales[a_targetIndex], m_animCurve.Evaluate(a_easedProgress));
		}
	}
}
