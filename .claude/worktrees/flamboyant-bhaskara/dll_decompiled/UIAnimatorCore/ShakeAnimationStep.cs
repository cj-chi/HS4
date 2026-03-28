using UnityEngine;

namespace UIAnimatorCore;

public class ShakeAnimationStep : EffectAnimationStep
{
	[SerializeField]
	private AnimStepVariableFloat m_shakeAmount = new AnimStepVariableFloat(2.5f);

	[SerializeField]
	private Vector2[] m_masterPositions;

	[SerializeField]
	private RectTransform[] m_targetTransforms;

	public new static string EditorDisplayName => "Shake";

	public override string StepTitleDisplay => EditorDisplayName;

	public override bool UseEasing => false;

	public override bool SetCustomInitialDuration => true;

	public override float CustomInitialDuration => 0.3f;

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
		m_shakeAmount.Initialise(a_targetObjects.Length);
		m_masterPositions = new Vector2[a_targetObjects.Length];
		for (int i = 0; i < a_targetObjects.Length; i++)
		{
			if (!(a_targetObjects[i] == null))
			{
				RectTransform component = a_targetObjects[i].GetComponent<RectTransform>();
				m_targetTransforms[i] = component;
				m_masterPositions[i] = new Vector2(component.anchoredPosition.x, component.anchoredPosition.y);
			}
		}
		return true;
	}

	protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
	{
		if (m_targetTransforms != null && a_targetIndex < m_targetTransforms.Length && !(m_targetTransforms[a_targetIndex] == null))
		{
			if (a_easedProgress == 0f || a_easedProgress == 1f)
			{
				m_targetTransforms[a_targetIndex].anchoredPosition = m_masterPositions[a_targetIndex];
			}
			else
			{
				m_targetTransforms[a_targetIndex].anchoredPosition = m_masterPositions[a_targetIndex] + Random.insideUnitCircle * m_shakeAmount.GetValue(a_targetIndex);
			}
		}
	}
}
