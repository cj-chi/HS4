using System.Collections.Generic;
using UnityEngine;

namespace AIProject.Animal;

public static class ListExtensions
{
	public static T Rand<T>(this List<T> source)
	{
		if (!source.IsNullOrEmpty())
		{
			return source[Random.Range(0, source.Count)];
		}
		return default(T);
	}

	public static T GetRand<T>(this List<T> source)
	{
		if (source.IsNullOrEmpty())
		{
			return default(T);
		}
		int index = Random.Range(0, source.Count);
		T result = source[index];
		source.RemoveAt(index);
		return result;
	}

	public static T First<T>(this List<T> source)
	{
		if (source.IsNullOrEmpty())
		{
			return default(T);
		}
		return source[0];
	}

	public static T Back<T>(this List<T> source)
	{
		if (source.IsNullOrEmpty())
		{
			return default(T);
		}
		return source[source.Count - 1];
	}

	public static bool AddNonContains<T>(this List<T> source, T value)
	{
		if (source == null)
		{
			return false;
		}
		if (source.Contains(value))
		{
			return false;
		}
		source.Add(value);
		return true;
	}

	public static bool InRange<T>(this List<T> source, int i)
	{
		if (source == null)
		{
			return false;
		}
		if (0 <= i)
		{
			return i < source.Count;
		}
		return false;
	}

	public static void PushFront<T>(this IList<T> self, T item)
	{
		self.Insert(0, item);
	}

	public static T PopFront<T>(this IList<T> self)
	{
		if (self == null || self.Count == 0)
		{
			return default(T);
		}
		T result = self[0];
		self.RemoveAt(0);
		return result;
	}

	public static void PushBack<T>(this IList<T> self, T item)
	{
		self.Add(item);
	}
}
