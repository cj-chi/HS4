using System;
using System.Collections.Generic;

namespace PlaceholderSoftware.WetStuff.Datastructures;

internal class Pool<T> : IRecycler<T> where T : class
{
	private readonly Func<T> _factory;

	private readonly Stack<T> _items;

	private readonly int _maxSize;

	public int Count => _items.Count;

	public int Capacity => _maxSize;

	public Pool(int maxSize, Func<T> factory)
	{
		_maxSize = maxSize;
		_factory = factory;
		_items = new Stack<T>(maxSize);
	}

	void IRecycler<T>.Recycle([NotNull] T item)
	{
		Put(item);
	}

	public T Get()
	{
		if (_items.Count > 0)
		{
			return _items.Pop();
		}
		return _factory();
	}

	public void Put([NotNull] T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (_items.Count < _maxSize)
		{
			_items.Push(item);
		}
	}
}
