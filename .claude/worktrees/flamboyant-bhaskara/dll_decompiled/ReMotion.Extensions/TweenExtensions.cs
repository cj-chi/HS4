using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ReMotion.Extensions;

public static class TweenExtensions
{
	public static Tween<Transform, Vector3> TweenPosition(this Transform transform, Vector3 to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false, bool autoStart = true)
	{
		settings = settings ?? TweenSettings.Default;
		easing = easing ?? settings.DefaultEasing;
		Tween<Transform, Vector3> tween = settings.UseVector3Tween(transform, (Transform x) => x.position, delegate(Transform target, ref Vector3 value)
		{
			target.position = value;
		}, easing, duration, to, isRelativeTo);
		if (autoStart)
		{
			tween.Start();
		}
		return tween;
	}

	public static IObservable<Unit> TweenPositionAsync(this Transform transform, Vector3 to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false)
	{
		return transform.TweenPosition(to, duration, easing, settings, isRelativeTo, autoStart: false).ToObservable();
	}

	public static Tween<Transform, Vector2> TweenPositionXY(this Transform transform, Vector2 to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false, bool autoStart = true)
	{
		settings = settings ?? TweenSettings.Default;
		easing = easing ?? settings.DefaultEasing;
		Tween<Transform, Vector2> tween = settings.UseVector2Tween(transform, (Transform x) => x.position, delegate(Transform target, ref Vector2 value)
		{
			Vector3 position = target.position;
			target.position = new Vector3
			{
				x = value.x,
				y = value.y,
				z = position.z
			};
		}, easing, duration, to, isRelativeTo);
		if (autoStart)
		{
			tween.Start();
		}
		return tween;
	}

	public static IObservable<Unit> TweenPositionXYAsync(this Transform transform, Vector2 to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false)
	{
		return transform.TweenPositionXY(to, duration, easing, settings, isRelativeTo, autoStart: false).ToObservable();
	}

	public static Tween<Transform, float> TweenPositionX(this Transform transform, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false, bool autoStart = true)
	{
		settings = settings ?? TweenSettings.Default;
		easing = easing ?? settings.DefaultEasing;
		Tween<Transform, float> tween = settings.UseFloatTween(transform, (Transform x) => x.position.x, delegate(Transform target, ref float value)
		{
			Vector3 position = target.position;
			target.position = new Vector3
			{
				x = value,
				y = position.y,
				z = position.z
			};
		}, easing, duration, to, isRelativeTo);
		if (autoStart)
		{
			tween.Start();
		}
		return tween;
	}

	public static IObservable<Unit> TweenPositionXAsync(this Transform transform, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false)
	{
		return transform.TweenPositionX(to, duration, easing, settings, isRelativeTo, autoStart: false).ToObservable();
	}

	public static Tween<Transform, float> TweenPositionY(this Transform transform, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false, bool autoStart = true)
	{
		settings = settings ?? TweenSettings.Default;
		easing = easing ?? settings.DefaultEasing;
		Tween<Transform, float> tween = settings.UseFloatTween(transform, (Transform x) => x.position.y, delegate(Transform target, ref float value)
		{
			Vector3 position = target.position;
			target.position = new Vector3
			{
				x = position.x,
				y = value,
				z = position.z
			};
		}, easing, duration, to, isRelativeTo);
		if (autoStart)
		{
			tween.Start();
		}
		return tween;
	}

	public static IObservable<Unit> TweenPositionYAsync(this Transform transform, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false)
	{
		return transform.TweenPositionY(to, duration, easing, settings, isRelativeTo, autoStart: false).ToObservable();
	}

	public static Tween<Transform, float> TweenPositionZ(this Transform transform, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false, bool autoStart = true)
	{
		settings = settings ?? TweenSettings.Default;
		easing = easing ?? settings.DefaultEasing;
		Tween<Transform, float> tween = settings.UseFloatTween(transform, (Transform x) => x.position.z, delegate(Transform target, ref float value)
		{
			Vector3 position = target.position;
			target.position = new Vector3
			{
				x = position.x,
				y = position.y,
				z = value
			};
		}, easing, duration, to, isRelativeTo);
		if (autoStart)
		{
			tween.Start();
		}
		return tween;
	}

	public static IObservable<Unit> TweenPositionZAsync(this Transform transform, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false)
	{
		return transform.TweenPositionZ(to, duration, easing, settings, isRelativeTo, autoStart: false).ToObservable();
	}

	public static Tween<Graphic, float> TweenAlpha(this Graphic graphic, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false, bool autoStart = true)
	{
		settings = settings ?? TweenSettings.Default;
		easing = easing ?? settings.DefaultEasing;
		Tween<Graphic, float> tween = settings.UseFloatTween(graphic, (Graphic x) => x.color.a, delegate(Graphic target, ref float value)
		{
			Color color = target.color;
			target.color = new Color
			{
				r = color.r,
				g = color.g,
				b = color.b,
				a = value
			};
		}, easing, duration, to, isRelativeTo);
		if (autoStart)
		{
			tween.Start();
		}
		return tween;
	}

	public static IObservable<Unit> TweenAlphaAsync(this Graphic graphic, float to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false)
	{
		return graphic.TweenAlpha(to, duration, easing, settings, isRelativeTo, autoStart: false).ToObservable();
	}

	public static Tween<Graphic, Color> TweenColor(this Graphic graphic, Color to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false, bool autoStart = true)
	{
		settings = settings ?? TweenSettings.Default;
		easing = easing ?? settings.DefaultEasing;
		Tween<Graphic, Color> tween = settings.UseColorTween(graphic, (Graphic x) => x.color, delegate(Graphic target, ref Color value)
		{
			target.color = value;
		}, easing, duration, to, isRelativeTo);
		if (autoStart)
		{
			tween.Start();
		}
		return tween;
	}

	public static IObservable<Unit> TweenColorAsync(this Graphic graphic, Color to, float duration, EasingFunction easing = null, TweenSettings settings = null, bool isRelativeTo = false)
	{
		return graphic.TweenColor(to, duration, easing, settings, isRelativeTo, autoStart: false).ToObservable();
	}
}
