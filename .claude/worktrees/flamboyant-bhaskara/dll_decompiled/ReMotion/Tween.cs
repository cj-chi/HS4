using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ReMotion;

public abstract class Tween<TObject, TProperty> : ITween where TObject : class
{
	private readonly TObject target;

	private readonly TweenGetter<TObject, TProperty> getter;

	private readonly TweenSetter<TObject, TProperty> setter;

	private readonly EasingFunction easingFunction;

	private readonly float duration;

	private readonly bool isRelativeTo;

	private TProperty from;

	private TProperty to;

	private TProperty difference;

	private TProperty originalFrom;

	private TProperty originalTo;

	private Subject<Unit> completedEvent;

	private float delayTime;

	private float currentTime;

	private int repeatCount;

	public TweenSettings Settings { get; private set; }

	public TweenStatus Status { get; private set; }

	public Tween(TweenSettings settings, TObject target, TweenGetter<TObject, TProperty> getter, TweenSetter<TObject, TProperty> setter, EasingFunction easingFunction, float duration, TProperty to, bool isRelativeTo)
	{
		Settings = settings;
		this.target = target;
		this.getter = getter;
		this.setter = setter;
		this.duration = duration;
		this.easingFunction = easingFunction;
		originalTo = to;
		this.isRelativeTo = isRelativeTo;
	}

	public void Reset()
	{
		from = originalFrom;
		if (isRelativeTo)
		{
			to = AddOperator(from, originalTo);
		}
		else
		{
			to = originalTo;
		}
		difference = GetDifference(from, to);
		currentTime = 0f;
		repeatCount = 0;
	}

	public Tween<TObject, TProperty> Start()
	{
		originalFrom = getter(target);
		delayTime = 0f;
		StartCore();
		return this;
	}

	public Tween<TObject, TProperty> Start(TProperty from, float delay)
	{
		originalFrom = from;
		if (delay <= 0f)
		{
			delay = 0f;
		}
		delayTime = delay;
		StartCore();
		return this;
	}

	public Tween<TObject, TProperty> Start(TProperty from, float delay, bool isRelativeFrom)
	{
		originalFrom = (isRelativeFrom ? AddOperator(getter(target), from) : from);
		if (delay <= 0f)
		{
			delay = 0f;
		}
		delayTime = delay;
		StartCore();
		return this;
	}

	public Tween<TObject, TProperty> StartFrom(TProperty from)
	{
		originalFrom = from;
		delayTime = 0f;
		StartCore();
		return this;
	}

	public Tween<TObject, TProperty> StartFromRelative(TProperty from)
	{
		originalFrom = AddOperator(getter(target), from);
		delayTime = 0f;
		StartCore();
		return this;
	}

	public Tween<TObject, TProperty> StartAfter(float delay)
	{
		originalFrom = getter(target);
		if (delay <= 0f)
		{
			delay = 0f;
		}
		delayTime = delay;
		StartCore();
		return this;
	}

	private void StartCore()
	{
		Reset();
		switch (Status)
		{
		case TweenStatus.Stopped:
			Status = TweenStatus.Running;
			TweenEngine.Instance.Add(this);
			break;
		default:
			Status = TweenStatus.Running;
			break;
		}
	}

	public void Stop()
	{
		TweenStatus status = Status;
		if (status != TweenStatus.Stopped)
		{
			_ = status - 1;
			_ = 2;
			Status = TweenStatus.WaitingToStop;
		}
	}

	public void Pause()
	{
		switch (Status)
		{
		case TweenStatus.Running:
		case TweenStatus.WaitingToStop:
			Status = TweenStatus.Pausing;
			break;
		case TweenStatus.Stopped:
		case TweenStatus.Pausing:
			break;
		}
	}

	public void Resume()
	{
		switch (Status)
		{
		case TweenStatus.Pausing:
			Status = TweenStatus.Running;
			break;
		case TweenStatus.Stopped:
		case TweenStatus.Running:
		case TweenStatus.WaitingToStop:
			break;
		}
	}

	public void PauseOrResume()
	{
		switch (Status)
		{
		case TweenStatus.Pausing:
			Status = TweenStatus.Running;
			break;
		case TweenStatus.Running:
			Status = TweenStatus.Pausing;
			break;
		case TweenStatus.Stopped:
		case TweenStatus.WaitingToStop:
			break;
		}
	}

	public IObservable<Unit> ToObservable(bool stopWhenDisposed = true)
	{
		if (completedEvent == null)
		{
			completedEvent = new Subject<Unit>();
		}
		if (Status == TweenStatus.Running)
		{
			IObservable<Unit> observable = completedEvent.FirstOrDefault();
			if (!stopWhenDisposed)
			{
				return observable;
			}
			return observable.DoOnCancel(delegate
			{
				Stop();
			});
		}
		return Observable.Defer(delegate
		{
			if (Status == TweenStatus.Stopped)
			{
				Start();
			}
			IObservable<Unit> observable2 = completedEvent.FirstOrDefault();
			return (!stopWhenDisposed) ? observable2 : observable2.DoOnCancel(delegate
			{
				Stop();
			});
		});
	}

	public void AttachSafe(GameObject gameObject)
	{
		gameObject.OnDestroyAsObservable().Subscribe(delegate
		{
			Stop();
		});
	}

	public void AttachSafe(Component component)
	{
		component.OnDestroyAsObservable().Subscribe(delegate
		{
			Stop();
		});
	}

	protected abstract TProperty AddOperator(TProperty left, TProperty right);

	protected abstract TProperty GetDifference(TProperty from, TProperty to);

	protected abstract void CreateValue(ref TProperty from, ref TProperty difference, ref float ratio, out TProperty value);

	public bool MoveNext(ref float deltaTime, ref float unscaledDeltaTime)
	{
		switch (Status)
		{
		case TweenStatus.Pausing:
			return true;
		default:
			Status = TweenStatus.Stopped;
			return false;
		case TweenStatus.Running:
		{
			if (delayTime != 0f)
			{
				delayTime -= (Settings.IsIgnoreTimeScale ? unscaledDeltaTime : deltaTime);
				if (!(delayTime <= 0f))
				{
					return true;
				}
				delayTime = 0f;
			}
			float num = (Settings.IsIgnoreTimeScale ? (currentTime += unscaledDeltaTime) : (currentTime += deltaTime));
			bool flag = false;
			if (num >= duration)
			{
				num = duration;
				flag = true;
			}
			float ratio = easingFunction(num, duration);
			CreateValue(ref from, ref difference, ref ratio, out var value);
			setter(target, ref value);
			if (flag)
			{
				repeatCount++;
				switch (Settings.LoopType)
				{
				case LoopType.Restart:
					from = originalFrom;
					currentTime = 0f;
					break;
				case LoopType.CycleOnce:
					if (repeatCount == 2)
					{
						return false;
					}
					goto case LoopType.Cycle;
				case LoopType.Cycle:
				{
					TProperty val = from;
					from = to;
					to = val;
					difference = GetDifference(from, to);
					currentTime = 0f;
					break;
				}
				default:
					if (completedEvent != null)
					{
						completedEvent.OnNext(Unit.Default);
					}
					return false;
				}
			}
			return true;
		}
		}
	}
}
