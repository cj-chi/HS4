using UnityEngine;

namespace UnityStandardAssets.CinematicEffects;

public sealed class ME_MinAttribute : PropertyAttribute
{
	public readonly float min;

	public ME_MinAttribute(float min)
	{
		this.min = min;
	}
}
