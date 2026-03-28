using System;

namespace PlaceholderSoftware.WetStuff;

internal abstract class RcSharedState<T> : IDisposable where T : IDisposable
{
	private static T _sharedState;

	private static uint _count;

	protected T SharedState
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "Cannot access shared state on disposed object");
			}
			return _sharedState;
		}
	}

	public bool IsDisposed { get; private set; }

	protected RcSharedState(Func<T> factory)
	{
		if (_count++ == 0)
		{
			_sharedState = factory();
		}
	}

	public virtual void Dispose()
	{
		if (!IsDisposed)
		{
			if (--_count == 0)
			{
				_sharedState.Dispose();
				_sharedState = default(T);
			}
			IsDisposed = true;
		}
	}
}
