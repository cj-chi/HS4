using UnityEngine;

namespace UIAnimatorCore;

public class TransformAnimationStep : TransitionAnimationStep
{
	[SerializeField]
	private AnimStepVariableVector3 m_position = new AnimStepVariableVector3(a_offsettingEnabled: true);

	[SerializeField]
	private AnimStepVariableVector3 m_scale = new AnimStepVariableVector3();

	[SerializeField]
	private AnimStepVariableVector3 m_rotation = new AnimStepVariableVector3();

	[SerializeField]
	private RectTransform[] m_targetTransforms;

	private Vector3 m_tmpVec3;

	public new static string EditorDisplayName => "Transform";

	public override string StepTitleDisplay => EditorDisplayName;

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
		Vector3[] array = new Vector3[a_targetObjects.Length];
		Vector3[] array2 = new Vector3[a_targetObjects.Length];
		Vector3[] array3 = new Vector3[a_targetObjects.Length];
		for (int i = 0; i < a_targetObjects.Length; i++)
		{
			if (!(a_targetObjects[i] == null))
			{
				RectTransform component = a_targetObjects[i].GetComponent<RectTransform>();
				m_targetTransforms[i] = component;
				array[i] = new Vector3(component.anchoredPosition.x, component.anchoredPosition.y, component.localPosition.z);
				array2[i] = component.localScale;
				array3[i] = component.localRotation.eulerAngles;
			}
		}
		if (!a_onlyMasterValues)
		{
			m_position.Initialise(array);
			m_scale.Initialise(array2);
			m_rotation.Initialise(array3);
		}
		else
		{
			m_position.UpdateMasterValues(array);
			m_scale.UpdateMasterValues(array2);
			m_rotation.UpdateMasterValues(array3);
		}
		return true;
	}

	protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
	{
		if (m_targetTransforms != null && a_targetIndex < m_targetTransforms.Length && !(m_targetTransforms[a_targetIndex] == null))
		{
			m_tmpVec3 = m_position.GetValueLerpedToMaster(a_targetIndex, a_easedProgress);
			m_targetTransforms[a_targetIndex].anchoredPosition = m_tmpVec3;
			m_targetTransforms[a_targetIndex].localPosition = new Vector3(m_targetTransforms[a_targetIndex].localPosition.x, m_targetTransforms[a_targetIndex].localPosition.y, m_tmpVec3.z);
			m_targetTransforms[a_targetIndex].localScale = m_scale.GetValueLerpedToMaster(a_targetIndex, a_easedProgress);
			m_targetTransforms[a_targetIndex].localRotation = Quaternion.Euler(m_rotation.GetValueLerpedToMaster(a_targetIndex, a_easedProgress));
		}
	}
}
