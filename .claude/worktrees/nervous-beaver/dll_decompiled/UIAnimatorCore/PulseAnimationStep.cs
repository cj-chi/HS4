using UnityEngine;

namespace UIAnimatorCore;

public class PulseAnimationStep : EffectAnimationStep
{
	[SerializeField]
	private AnimStepVariableFloat m_pulseScale = new AnimStepVariableFloat(1.25f);

	[SerializeField]
	private AnimStepVariableFloat m_rotationAngle = new AnimStepVariableFloat(0f);

	[SerializeField]
	private Vector3[] m_masterScales;

	[SerializeField]
	private Vector3[] m_masterRotations;

	[SerializeField]
	private RectTransform[] m_targetTransforms;

	private float m_tempEasedProgress;

	public new static string EditorDisplayName => "Pulse";

	public override string StepTitleDisplay => EditorDisplayName;

	public override bool UseEasing => false;

	public override bool SetCustomInitialDuration => true;

	public override float CustomInitialDuration => 0.2f;

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
		m_pulseScale.Initialise(a_targetObjects.Length);
		m_rotationAngle.Initialise(a_targetObjects.Length);
		m_masterScales = new Vector3[a_targetObjects.Length];
		m_masterRotations = new Vector3[a_targetObjects.Length];
		for (int i = 0; i < a_targetObjects.Length; i++)
		{
			if (!(a_targetObjects[i] == null))
			{
				RectTransform component = a_targetObjects[i].GetComponent<RectTransform>();
				m_targetTransforms[i] = component;
				m_masterScales[i] = component.localScale;
				m_masterRotations[i] = component.localRotation.eulerAngles;
			}
		}
		return true;
	}

	protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
	{
		if (m_targetTransforms != null && a_targetIndex < m_targetTransforms.Length && !(m_targetTransforms[a_targetIndex] == null))
		{
			if (a_easedProgress < 0.5f)
			{
				m_tempEasedProgress = EasingManager.GetEaseProgress(EasingEquation.SineEaseOut, a_easedProgress / 0.5f);
				m_targetTransforms[a_targetIndex].localScale = Vector3.LerpUnclamped(m_masterScales[a_targetIndex], m_masterScales[a_targetIndex] * m_pulseScale.GetValue(a_targetIndex), m_tempEasedProgress);
				m_targetTransforms[a_targetIndex].localRotation = Quaternion.Euler(Vector3.LerpUnclamped(m_masterRotations[a_targetIndex], m_masterRotations[a_targetIndex] + new Vector3(0f, 0f, m_rotationAngle.GetValue(a_targetIndex)), m_tempEasedProgress));
			}
			else
			{
				m_tempEasedProgress = EasingManager.GetEaseProgress(EasingEquation.SineEaseIn, (a_easedProgress - 0.5f) / 0.5f);
				m_targetTransforms[a_targetIndex].localScale = Vector3.LerpUnclamped(m_masterScales[a_targetIndex] * m_pulseScale.GetValue(a_targetIndex), m_masterScales[a_targetIndex], m_tempEasedProgress);
				m_targetTransforms[a_targetIndex].localRotation = Quaternion.Euler(Vector3.LerpUnclamped(m_masterRotations[a_targetIndex] + new Vector3(0f, 0f, m_rotationAngle.GetValue(a_targetIndex)), m_masterRotations[a_targetIndex], m_tempEasedProgress));
			}
		}
	}
}
