using System;
using UnityEngine;

namespace UIAnimatorCore;

public class EasingManager
{
	public static float GetEaseProgress(EasingEquation ease_type, float linear_progress)
	{
		return ease_type switch
		{
			EasingEquation.Linear => linear_progress, 
			EasingEquation.BackEaseIn => BackEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.BackEaseInOut => BackEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.BackEaseOut => BackEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.BackEaseOutIn => BackEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.BounceEaseIn => BounceEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.BounceEaseInOut => BounceEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.BounceEaseOut => BounceEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.BounceEaseOutIn => BounceEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CircEaseIn => CircEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CircEaseInOut => CircEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CircEaseOut => CircEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CircEaseOutIn => CircEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CubicEaseIn => CubicEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CubicEaseInOut => CubicEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CubicEaseOut => CubicEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.CubicEaseOutIn => CubicEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ElasticEaseIn => ElasticEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ElasticEaseInOut => ElasticEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ElasticEaseOut => ElasticEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ElasticEaseOutIn => ElasticEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ExpoEaseIn => ExpoEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ExpoEaseInOut => ExpoEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ExpoEaseOut => ExpoEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.ExpoEaseOutIn => ExpoEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuadEaseIn => QuadEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuadEaseInOut => QuadEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuadEaseOut => QuadEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuadEaseOutIn => QuadEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuartEaseIn => QuartEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuartEaseInOut => QuartEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuartEaseOut => QuartEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuartEaseOutIn => QuartEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuintEaseIn => QuintEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuintEaseInOut => QuintEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuintEaseOut => QuintEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.QuintEaseOutIn => QuintEaseOutIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.SineEaseIn => SineEaseIn(linear_progress, 0f, 1f, 1f), 
			EasingEquation.SineEaseInOut => SineEaseInOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.SineEaseOut => SineEaseOut(linear_progress, 0f, 1f, 1f), 
			EasingEquation.SineEaseOutIn => SineEaseOutIn(linear_progress, 0f, 1f, 1f), 
			_ => linear_progress, 
		};
	}

	public static EasingEquation GetEaseTypeOpposite(EasingEquation ease_type)
	{
		return ease_type switch
		{
			EasingEquation.Linear => EasingEquation.Linear, 
			EasingEquation.BackEaseIn => EasingEquation.BackEaseOut, 
			EasingEquation.BackEaseInOut => EasingEquation.BackEaseOutIn, 
			EasingEquation.BackEaseOut => EasingEquation.BackEaseIn, 
			EasingEquation.BackEaseOutIn => EasingEquation.BackEaseInOut, 
			EasingEquation.BounceEaseIn => EasingEquation.BounceEaseOut, 
			EasingEquation.BounceEaseInOut => EasingEquation.BounceEaseOutIn, 
			EasingEquation.BounceEaseOut => EasingEquation.BounceEaseIn, 
			EasingEquation.BounceEaseOutIn => EasingEquation.BounceEaseInOut, 
			EasingEquation.CircEaseIn => EasingEquation.CircEaseOut, 
			EasingEquation.CircEaseInOut => EasingEquation.CircEaseOutIn, 
			EasingEquation.CircEaseOut => EasingEquation.CircEaseIn, 
			EasingEquation.CircEaseOutIn => EasingEquation.CircEaseInOut, 
			EasingEquation.CubicEaseIn => EasingEquation.CubicEaseOut, 
			EasingEquation.CubicEaseInOut => EasingEquation.CubicEaseOutIn, 
			EasingEquation.CubicEaseOut => EasingEquation.CubicEaseIn, 
			EasingEquation.CubicEaseOutIn => EasingEquation.CubicEaseInOut, 
			EasingEquation.ElasticEaseIn => EasingEquation.ElasticEaseOut, 
			EasingEquation.ElasticEaseInOut => EasingEquation.ElasticEaseOutIn, 
			EasingEquation.ElasticEaseOut => EasingEquation.ElasticEaseIn, 
			EasingEquation.ElasticEaseOutIn => EasingEquation.ElasticEaseInOut, 
			EasingEquation.ExpoEaseIn => EasingEquation.ExpoEaseOut, 
			EasingEquation.ExpoEaseInOut => EasingEquation.ExpoEaseOutIn, 
			EasingEquation.ExpoEaseOut => EasingEquation.ExpoEaseIn, 
			EasingEquation.ExpoEaseOutIn => EasingEquation.ExpoEaseInOut, 
			EasingEquation.QuadEaseIn => EasingEquation.QuadEaseOut, 
			EasingEquation.QuadEaseInOut => EasingEquation.QuadEaseOutIn, 
			EasingEquation.QuadEaseOut => EasingEquation.QuadEaseIn, 
			EasingEquation.QuadEaseOutIn => EasingEquation.QuadEaseInOut, 
			EasingEquation.QuartEaseIn => EasingEquation.QuartEaseOut, 
			EasingEquation.QuartEaseInOut => EasingEquation.QuartEaseOutIn, 
			EasingEquation.QuartEaseOut => EasingEquation.QuartEaseIn, 
			EasingEquation.QuartEaseOutIn => EasingEquation.QuartEaseInOut, 
			EasingEquation.QuintEaseIn => EasingEquation.QuintEaseOut, 
			EasingEquation.QuintEaseInOut => EasingEquation.QuintEaseOutIn, 
			EasingEquation.QuintEaseOut => EasingEquation.QuintEaseIn, 
			EasingEquation.QuintEaseOutIn => EasingEquation.QuintEaseInOut, 
			EasingEquation.SineEaseIn => EasingEquation.SineEaseOut, 
			EasingEquation.SineEaseInOut => EasingEquation.SineEaseOutIn, 
			EasingEquation.SineEaseOut => EasingEquation.SineEaseIn, 
			EasingEquation.SineEaseOutIn => EasingEquation.SineEaseInOut, 
			_ => EasingEquation.Linear, 
		};
	}

	public static float Linear(float t, float b, float c, float d)
	{
		return c * t / d + b;
	}

	public static float ExpoEaseOut(float t, float b, float c, float d)
	{
		if (t != d)
		{
			return c * (0f - Mathf.Pow(2f, -10f * t / d) + 1f) + b;
		}
		return b + c;
	}

	public static float ExpoEaseIn(float t, float b, float c, float d)
	{
		if (t != 0f)
		{
			return c * Mathf.Pow(2f, 10f * (t / d - 1f)) + b;
		}
		return b;
	}

	public static float ExpoEaseInOut(float t, float b, float c, float d)
	{
		if (t == 0f)
		{
			return b;
		}
		if (t == d)
		{
			return b + c;
		}
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
		}
		return c / 2f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
	}

	public static float ExpoEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return ExpoEaseOut(t * 2f, b, c / 2f, d);
		}
		return ExpoEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float CircEaseOut(float t, float b, float c, float d)
	{
		return c * Mathf.Sqrt(1f - (t = t / d - 1f) * t) + b;
	}

	public static float CircEaseIn(float t, float b, float c, float d)
	{
		return (0f - c) * (Mathf.Sqrt(1f - (t /= d) * t) - 1f) + b;
	}

	public static float CircEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
		}
		return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
	}

	public static float CircEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return CircEaseOut(t * 2f, b, c / 2f, d);
		}
		return CircEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float QuadEaseOut(float t, float b, float c, float d)
	{
		return (0f - c) * (t /= d) * (t - 2f) + b;
	}

	public static float QuadEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t + b;
	}

	public static float QuadEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t + b;
		}
		return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
	}

	public static float QuadEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return QuadEaseOut(t * 2f, b, c / 2f, d);
		}
		return QuadEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float SineEaseOut(float t, float b, float c, float d)
	{
		return c * Mathf.Sin(t / d * ((float)Math.PI / 2f)) + b;
	}

	public static float SineEaseIn(float t, float b, float c, float d)
	{
		return (0f - c) * Mathf.Cos(t / d * ((float)Math.PI / 2f)) + c + b;
	}

	public static float SineEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * Mathf.Sin((float)Math.PI * t / 2f) + b;
		}
		return (0f - c) / 2f * (Mathf.Cos((float)Math.PI * (t -= 1f) / 2f) - 2f) + b;
	}

	public static float SineEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return SineEaseOut(t * 2f, b, c / 2f, d);
		}
		return SineEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float CubicEaseOut(float t, float b, float c, float d)
	{
		return c * ((t = t / d - 1f) * t * t + 1f) + b;
	}

	public static float CubicEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t * t + b;
	}

	public static float CubicEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t * t + b;
		}
		return c / 2f * ((t -= 2f) * t * t + 2f) + b;
	}

	public static float CubicEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return CubicEaseOut(t * 2f, b, c / 2f, d);
		}
		return CubicEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float QuartEaseOut(float t, float b, float c, float d)
	{
		return (0f - c) * ((t = t / d - 1f) * t * t * t - 1f) + b;
	}

	public static float QuartEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t * t * t + b;
	}

	public static float QuartEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t * t * t + b;
		}
		return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
	}

	public static float QuartEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return QuartEaseOut(t * 2f, b, c / 2f, d);
		}
		return QuartEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float QuintEaseOut(float t, float b, float c, float d)
	{
		return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
	}

	public static float QuintEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t * t * t * t + b;
	}

	public static float QuintEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t * t * t * t + b;
		}
		return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
	}

	public static float QuintEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return QuintEaseOut(t * 2f, b, c / 2f, d);
		}
		return QuintEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float ElasticEaseOut(float t, float b, float c, float d)
	{
		if ((t /= d) == 1f)
		{
			return b + c;
		}
		float num = d * 0.3f;
		float num2 = num / 4f;
		return c * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * d - num2) * ((float)Math.PI * 2f) / num) + c + b;
	}

	public static float ElasticEaseIn(float t, float b, float c, float d)
	{
		if ((t /= d) == 1f)
		{
			return b + c;
		}
		float num = d * 0.3f;
		float num2 = num / 4f;
		return 0f - c * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - num2) * ((float)Math.PI * 2f) / num) + b;
	}

	public static float ElasticEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) == 2f)
		{
			return b + c;
		}
		float num = d * 0.45000002f;
		float num2 = num / 4f;
		if (t < 1f)
		{
			return -0.5f * (c * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - num2) * ((float)Math.PI * 2f) / num)) + b;
		}
		return c * Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t * d - num2) * ((float)Math.PI * 2f) / num) * 0.5f + c + b;
	}

	public static float ElasticEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return ElasticEaseOut(t * 2f, b, c / 2f, d);
		}
		return ElasticEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float BounceEaseOut(float t, float b, float c, float d)
	{
		if ((t /= d) < 0.36363637f)
		{
			return c * (7.5625f * t * t) + b;
		}
		if (t < 0.72727275f)
		{
			return c * (7.5625f * (t -= 0.54545456f) * t + 0.75f) + b;
		}
		if (t < 0.90909094f)
		{
			return c * (7.5625f * (t -= 0.8181818f) * t + 0.9375f) + b;
		}
		return c * (7.5625f * (t -= 21f / 22f) * t + 63f / 64f) + b;
	}

	public static float BounceEaseIn(float t, float b, float c, float d)
	{
		return c - BounceEaseOut(d - t, 0f, c, d) + b;
	}

	public static float BounceEaseInOut(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return BounceEaseIn(t * 2f, 0f, c, d) * 0.5f + b;
		}
		return BounceEaseOut(t * 2f - d, 0f, c, d) * 0.5f + c * 0.5f + b;
	}

	public static float BounceEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return BounceEaseOut(t * 2f, b, c / 2f, d);
		}
		return BounceEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}

	public static float BackEaseOut(float t, float b, float c, float d)
	{
		return c * ((t = t / d - 1f) * t * (2.70158f * t + 1.70158f) + 1f) + b;
	}

	public static float BackEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t * (2.70158f * t - 1.70158f) + b;
	}

	public static float BackEaseInOut(float t, float b, float c, float d)
	{
		float num = 1.70158f;
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * (t * t * (((num *= 1.525f) + 1f) * t - num)) + b;
		}
		return c / 2f * ((t -= 2f) * t * (((num *= 1.525f) + 1f) * t + num) + 2f) + b;
	}

	public static float BackEaseOutIn(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return BackEaseOut(t * 2f, b, c / 2f, d);
		}
		return BackEaseIn(t * 2f - d, b + c / 2f, c / 2f, d);
	}
}
