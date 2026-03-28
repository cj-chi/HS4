using System.Collections.Generic;

namespace AIProject.Animal;

public static class CollectionExtensions
{
	public static bool IsNullOrEmpty<T>(this IReadOnlyList<T> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}
}
