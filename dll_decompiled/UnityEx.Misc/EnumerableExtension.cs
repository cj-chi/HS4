using System;
using System.Collections.Generic;

namespace UnityEx.Misc;

public static class EnumerableExtension
{
	public static Stack<TSource> ToStack<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new Stack<TSource>(source);
	}

	public static Queue<TSource> ToQueue<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new Queue<TSource>(source);
	}
}
