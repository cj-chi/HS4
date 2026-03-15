using System;

namespace UIAnimatorCore;

[Serializable]
public abstract class EffectAnimationStep : BaseAnimationStep
{
	public override bool IsEffectStep => true;
}
