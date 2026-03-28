using System;
using UnityEngine;

namespace PlayfulSystems;

public static class Utils
{
	public static float EaseSinInOut(float lerp, float start, float change)
	{
		return (0f - change) / 2f * (Mathf.Cos((float)Math.PI * lerp) - 1f) + start;
	}
}
