using System;

namespace UIAnimatorCore;

[Serializable]
public abstract class TransitionAnimationStep : BaseAnimationStep
{
	public override bool IsEffectStep => false;
}
