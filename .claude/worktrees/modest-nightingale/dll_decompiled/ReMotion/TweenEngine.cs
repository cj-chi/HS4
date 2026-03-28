using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ReMotion;

internal class TweenEngine
{
	internal static TweenEngine Instance = new TweenEngine();

	private const int InitialSize = 16;

	private readonly object runningAndQueueLock = new object();

	private readonly object arrayLock = new object();

	private readonly Action<Exception> unhandledExceptionCallback;

	private int tail;

	private bool running;

	private ITween[] tweens = new ITween[16];

	private Queue<ITween> waitQueue = new Queue<ITween>();

	private TweenEngine()
	{
		unhandledExceptionCallback = delegate
		{
		};
		MainThreadDispatcher.StartUpdateMicroCoroutine(RunEveryFrame());
	}

	public static void AddTween(ITween tween)
	{
		Instance.Add(tween);
	}

	private IEnumerator RunEveryFrame()
	{
		while (true)
		{
			yield return null;
			Instance.Run(Time.deltaTime, Time.unscaledDeltaTime);
		}
	}

	public void Add(ITween tween)
	{
		lock (runningAndQueueLock)
		{
			if (running)
			{
				waitQueue.Enqueue(tween);
				return;
			}
		}
		lock (arrayLock)
		{
			if (tweens.Length == tail)
			{
				Array.Resize(ref tweens, checked(tail * 2));
			}
			tweens[tail++] = tween;
		}
	}

	public void Run(float deltaTime, float unscaledDeltaTime)
	{
		lock (runningAndQueueLock)
		{
			running = true;
		}
		lock (arrayLock)
		{
			int num = tail - 1;
			for (int i = 0; i < tweens.Length; i++)
			{
				ITween tween = tweens[i];
				if (tween != null)
				{
					try
					{
						if (!tween.MoveNext(ref deltaTime, ref unscaledDeltaTime))
						{
							tweens[i] = null;
							goto IL_0101;
						}
					}
					catch (Exception obj)
					{
						tweens[i] = null;
						try
						{
							unhandledExceptionCallback(obj);
						}
						catch
						{
						}
						goto IL_0101;
					}
					continue;
				}
				goto IL_0101;
				IL_0101:
				while (i < num)
				{
					ITween tween2 = tweens[num];
					if (tween2 != null)
					{
						try
						{
							if (!tween2.MoveNext(ref deltaTime, ref unscaledDeltaTime))
							{
								tweens[num] = null;
								num--;
								continue;
							}
							tweens[i] = tween2;
							tweens[num] = null;
							num--;
						}
						catch (Exception obj3)
						{
							tweens[num] = null;
							num--;
							try
							{
								unhandledExceptionCallback(obj3);
							}
							catch
							{
							}
							continue;
						}
						goto IL_010e;
					}
					num--;
				}
				tail = i;
				break;
				IL_010e:;
			}
			lock (runningAndQueueLock)
			{
				running = false;
				while (waitQueue.Count != 0)
				{
					if (tweens.Length == tail)
					{
						Array.Resize(ref tweens, checked(tail * 2));
					}
					tweens[tail++] = waitQueue.Dequeue();
				}
			}
		}
	}
}
