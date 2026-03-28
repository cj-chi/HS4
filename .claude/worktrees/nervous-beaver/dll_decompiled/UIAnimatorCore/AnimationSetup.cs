using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UIAnimatorCore;

[Serializable]
public class AnimationSetup
{
	[SerializeField]
	private List<AnimationStage> m_animationStages;

	[SerializeField]
	private UnityEvent m_onStartAction;

	[SerializeField]
	private UnityEvent m_onFinishedAction;

	public List<AnimationStage> AnimationStages => m_animationStages;

	public UnityEvent OnFinishedAction => m_onFinishedAction;

	public UnityEvent OnStartAction => m_onStartAction;

	public AnimationSetup()
	{
		m_animationStages = new List<AnimationStage>
		{
			new AnimationStage()
		};
	}
}
