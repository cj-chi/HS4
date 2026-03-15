using System;
using UnityEngine;

namespace ReMotion;

public static class EasingFunctions
{
	private const float DefaultOvershoot = 1.70158f;

	private const float DefaultAmplitude = 1.70158f;

	private const float DefaultPeriod = 0f;

	private const float PiDivide2 = (float)Math.PI / 2f;

	private const float PiMultiply2 = (float)Math.PI * 2f;

	public static readonly EasingFunction Linear = Linear_;

	public static readonly EasingFunction EaseInSine = EaseInSine_;

	public static readonly EasingFunction EaseOutSine = EaseOutSine_;

	public static readonly EasingFunction EaseInOutSine = EaseInOutSine_;

	public static readonly EasingFunction EaseInQuad = EaseInQuad_;

	public static readonly EasingFunction EaseOutQuad = EaseOutQuad_;

	public static readonly EasingFunction EaseInOutQuad = EaseInOutQuad_;

	public static readonly EasingFunction EaseInCubic = EaseInCubic_;

	public static readonly EasingFunction EaseOutCubic = EaseOutCubic_;

	public static readonly EasingFunction EaseInOutCubic = EaseInOutCubic_;

	public static readonly EasingFunction EaseInQuart = EaseInQuart_;

	public static readonly EasingFunction EaseOutQuart = EaseOutQuart_;

	public static readonly EasingFunction EaseInOutQuart = EaseInOutQuart_;

	public static readonly EasingFunction EaseInQuint = EaseInQuint_;

	public static readonly EasingFunction EaseOutQuint = EaseOutQuint_;

	public static readonly EasingFunction EaseInOutQuint = EaseInOutQuint_;

	public static readonly EasingFunction EaseInExpo = EaseInExpo_;

	public static readonly EasingFunction EaseOutExpo = EaseOutExpo_;

	public static readonly EasingFunction EaseInOutExpo = EaseInOutExpo_;

	public static readonly EasingFunction EaseInCirc = EaseInCirc_;

	public static readonly EasingFunction EaseOutCirc = EaseOutCirc_;

	public static readonly EasingFunction EaseInOutCirc = EaseInOutCirc_;

	public static readonly EasingFunction EaseInBounce = EaseInBounce_;

	public static readonly EasingFunction EaseOutBounce = EaseOutBounce_;

	public static readonly EasingFunction EaseInOutBounce = EaseInOutBounce_;

	private static readonly EasingFunction defaultEaseInBack = (float time, float duration) => EaseInBack_(time, duration, 1.70158f);

	private static readonly EasingFunction defaultEaseOutBack = (float time, float duration) => EaseOutBack_(time, duration, 1.70158f);

	private static readonly EasingFunction defaultEaseInOutBack = (float time, float duration) => EaseInOutBack_(time, duration, 1.70158f);

	private static readonly EasingFunction defaultEaseInElastic = (float time, float duration) => EaseInElastic_(time, duration, 1.70158f, 0f);

	private static readonly EasingFunction defaultEaseOutElastic = (float time, float duration) => EaseOutElastic_(time, duration, 1.70158f, 0f);

	private static readonly EasingFunction defaultEaseInOutElastic = (float time, float duration) => EaseInOutElastic_(time, duration, 1.70158f, 0f);

	public static EasingFunction EaseInBack(float overshoot = 1.70158f)
	{
		if (overshoot != 1.70158f)
		{
			return (float time, float duration) => EaseInBack_(time, duration, overshoot);
		}
		return defaultEaseInBack;
	}

	public static EasingFunction EaseOutBack(float overshoot = 1.70158f)
	{
		if (overshoot != 1.70158f)
		{
			return (float time, float duration) => EaseOutBack_(time, duration, overshoot);
		}
		return defaultEaseOutBack;
	}

	public static EasingFunction EaseInOutBack(float overshoot = 1.70158f)
	{
		if (overshoot != 1.70158f)
		{
			return (float time, float duration) => EaseInOutBack_(time, duration, overshoot);
		}
		return defaultEaseInOutBack;
	}

	public static EasingFunction EaseInElastic(float amplitude = 1.70158f, float period = 0f)
	{
		if (amplitude != 1.70158f || period != 0f)
		{
			return (float time, float duration) => EaseInElastic_(time, duration, amplitude, period);
		}
		return defaultEaseInElastic;
	}

	public static EasingFunction EaseOutElastic(float amplitude = 1.70158f, float period = 0f)
	{
		if (amplitude != 1.70158f || period != 0f)
		{
			return (float time, float duration) => EaseOutElastic_(time, duration, amplitude, period);
		}
		return defaultEaseOutElastic;
	}

	public static EasingFunction EaseInOutElastic(float amplitude = 1.70158f, float period = 0f)
	{
		if (amplitude != 1.70158f || period != 0f)
		{
			return (float time, float duration) => EaseInOutElastic_(time, duration, amplitude, period);
		}
		return defaultEaseInOutElastic;
	}

	public static EasingFunction Shake(float amplitude = 1f)
	{
		return (float time, float duration) => UnityEngine.Random.Range(0f, amplitude);
	}

	public static EasingFunction AnimationCurve(AnimationCurve animationCurve)
	{
		if (animationCurve.keys.Length == 0)
		{
			return Linear;
		}
		float curveDuration = animationCurve.keys[animationCurve.keys.Length - 1].time;
		return delegate(float time, float duration)
		{
			float time2 = time * curveDuration / duration;
			return animationCurve.Evaluate(time2);
		};
	}

	private static float Linear_(float time, float duration)
	{
		return time / duration;
	}

	private static float EaseInSine_(float time, float duration)
	{
		return -1f * (float)Math.Cos(time / duration * ((float)Math.PI / 2f)) + 1f;
	}

	private static float EaseOutSine_(float time, float duration)
	{
		return (float)Math.Sin(time / duration * ((float)Math.PI / 2f));
	}

	private static float EaseInOutSine_(float time, float duration)
	{
		return -0.5f * ((float)Math.Cos((float)Math.PI * time / duration) - 1f);
	}

	private static float EaseInQuad_(float time, float duration)
	{
		time /= duration;
		return time * time;
	}

	private static float EaseOutQuad_(float time, float duration)
	{
		time /= duration;
		return (0f - time) * (time - 2f);
	}

	private static float EaseInOutQuad_(float time, float duration)
	{
		time /= duration * 0.5f;
		if (time < 1f)
		{
			return 0.5f * time * time;
		}
		time -= 1f;
		return -0.5f * (time * (time - 2f) - 1f);
	}

	private static float EaseInCubic_(float time, float duration)
	{
		time /= duration;
		return time * time * time;
	}

	private static float EaseOutCubic_(float time, float duration)
	{
		time = time / duration - 1f;
		return time * time * time + 1f;
	}

	private static float EaseInOutCubic_(float time, float duration)
	{
		time /= duration * 0.5f;
		if (time < 1f)
		{
			return 0.5f * time * time * time;
		}
		time -= 2f;
		return 0.5f * (time * time * time + 2f);
	}

	private static float EaseInQuart_(float time, float duration)
	{
		time /= duration;
		return time * time * time * time;
	}

	private static float EaseOutQuart_(float time, float duration)
	{
		time = time / duration - 1f;
		return 0f - (time * time * time * time - 1f);
	}

	private static float EaseInOutQuart_(float time, float duration)
	{
		time /= duration * 0.5f;
		if (time < 1f)
		{
			return 0.5f * time * time * time * time;
		}
		time -= 2f;
		return -0.5f * (time * time * time * time - 2f);
	}

	private static float EaseInQuint_(float time, float duration)
	{
		time /= duration;
		return time * time * time * time * time;
	}

	private static float EaseOutQuint_(float time, float duration)
	{
		time = time / duration - 1f;
		return time * time * time * time * time + 1f;
	}

	private static float EaseInOutQuint_(float time, float duration)
	{
		time /= duration * 0.5f;
		if (time < 1f)
		{
			return 0.5f * time * time * time * time * time;
		}
		time -= 2f;
		return 0.5f * (time * time * time * time * time + 2f);
	}

	private static float EaseInExpo_(float time, float duration)
	{
		if (time != 0f)
		{
			return (float)Math.Pow(2.0, 10f * (time / duration - 1f));
		}
		return 0f;
	}

	private static float EaseOutExpo_(float time, float duration)
	{
		if (time == duration)
		{
			return 1f;
		}
		return 0f - (float)Math.Pow(2.0, -10f * time / duration) + 1f;
	}

	private static float EaseInOutExpo_(float time, float duration)
	{
		if (time == 0f)
		{
			return 0f;
		}
		if (time == duration)
		{
			return 1f;
		}
		time /= duration * 0.5f;
		if (time < 1f)
		{
			return 0.5f * (float)Math.Pow(2.0, 10f * (time - 1f));
		}
		return 0.5f * (0f - (float)Math.Pow(2.0, -10f * (time -= 1f)) + 2f);
	}

	private static float EaseInCirc_(float time, float duration)
	{
		time /= duration;
		return 0f - ((float)Math.Sqrt(1f - time * time) - 1f);
	}

	private static float EaseOutCirc_(float time, float duration)
	{
		time = time / duration - 1f;
		return (float)Math.Sqrt(1f - time * time);
	}

	private static float EaseInOutCirc_(float time, float duration)
	{
		time /= duration * 0.5f;
		if (time < 1f)
		{
			return -0.5f * ((float)Math.Sqrt(1f - time * time) - 1f);
		}
		time -= 2f;
		return 0.5f * ((float)Math.Sqrt(1f - time * time) + 1f);
	}

	private static float EaseInBack_(float time, float duration, float overshoot)
	{
		time /= duration;
		return time * time * ((overshoot + 1f) * time - overshoot);
	}

	private static float EaseOutBack_(float time, float duration, float overshoot)
	{
		time = time / duration - 1f;
		return time * time * ((overshoot + 1f) * time + overshoot) + 1f;
	}

	private static float EaseInOutBack_(float time, float duration, float overshoot)
	{
		time /= duration * 0.5f;
		if (time < 1f)
		{
			return 0.5f * (time * time * (((overshoot *= 1.525f) + 1f) * time - overshoot));
		}
		time -= 2f;
		return 0.5f * (time * time * (((overshoot *= 1.525f) + 1f) * time + overshoot) + 2f);
	}

	private static float EaseInElastic_(float time, float duration, float amplitude, float period)
	{
		if (time == 0f)
		{
			return 0f;
		}
		time /= duration;
		if (time == 1f)
		{
			return 1f;
		}
		if (period == 0f)
		{
			period = duration * 0.3f;
		}
		float num;
		if (amplitude < 1f)
		{
			amplitude = 1f;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * (float)Math.Asin(1f / amplitude);
		}
		time -= 1f;
		return 0f - amplitude * (float)Math.Pow(2.0, 10f * time) * (float)Math.Sin((time * duration - num) * ((float)Math.PI * 2f) / period);
	}

	private static float EaseOutElastic_(float time, float duration, float amplitude, float period)
	{
		if (time == 0f)
		{
			return 0f;
		}
		time /= duration;
		if (time == 1f)
		{
			return 1f;
		}
		if (period == 0f)
		{
			period = duration * 0.3f;
		}
		float num;
		if (amplitude < 1f)
		{
			amplitude = 1f;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * (float)Math.Asin(1f / amplitude);
		}
		return amplitude * (float)Math.Pow(2.0, -10f * time) * (float)Math.Sin((time * duration - num) * ((float)Math.PI * 2f) / period) + 1f;
	}

	private static float EaseInOutElastic_(float time, float duration, float amplitude, float period)
	{
		if (time == 0f)
		{
			return 0f;
		}
		time /= duration * 0.5f;
		if (time == 2f)
		{
			return 1f;
		}
		if (period == 0f)
		{
			period = duration * 0.45000002f;
		}
		float num;
		if (amplitude < 1f)
		{
			amplitude = 1f;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * (float)Math.Asin(1f / amplitude);
		}
		if (time < 1f)
		{
			time -= 1f;
			return -0.5f * (amplitude * (float)Math.Pow(2.0, 10f * time) * (float)Math.Sin((time * duration - num) * ((float)Math.PI * 2f) / period));
		}
		time -= 1f;
		return amplitude * (float)Math.Pow(2.0, -10f * time) * (float)Math.Sin((time * duration - num) * ((float)Math.PI * 2f) / period) * 0.5f + 1f;
	}

	private static float EaseInBounce_(float time, float duration)
	{
		return 1f - EaseOutBounce_(duration - time, duration);
	}

	private static float EaseOutBounce_(float time, float duration)
	{
		time /= duration;
		if (time < 0.36363637f)
		{
			return 7.5625f * time * time;
		}
		if (time < 0.72727275f)
		{
			time -= 0.54545456f;
			return 7.5625f * time * time + 0.75f;
		}
		if (time < 0.90909094f)
		{
			time -= 0.8181818f;
			return 7.5625f * time * time + 0.9375f;
		}
		time -= 21f / 22f;
		return 7.5625f * time * time + 63f / 64f;
	}

	private static float EaseInOutBounce_(float time, float duration)
	{
		if (time < duration * 0.5f)
		{
			return EaseInBounce_(time * 2f, duration) * 0.5f;
		}
		return EaseOutBounce_(time * 2f - duration, duration) * 0.5f + 0.5f;
	}
}
