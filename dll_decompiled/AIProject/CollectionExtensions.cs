using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AIProject;

public static class CollectionExtensions
{
	public static bool IsNullOrEmpty<T>(this T[] source)
	{
		if (source != null)
		{
			return source.Length == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>(this List<T> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<TKey, TSource>(this Dictionary<TKey, TSource> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<TKey, TSource>(this ReadOnlyDictionary<TKey, TSource> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>(this Queue<T> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}

	public static bool Exists<T>(this T[] source, Predicate<T> predicate)
	{
		foreach (T obj in source)
		{
			if (predicate(obj))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Exists<T>(this List<T> source, Predicate<T> predicate)
	{
		foreach (T item in source)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Exists<TKey, TSource>(this Dictionary<TKey, TSource> source, Predicate<KeyValuePair<TKey, TSource>> predicate)
	{
		foreach (KeyValuePair<TKey, TSource> item in source)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Exists<T>(this Queue<T> source, Predicate<T> predicate)
	{
		foreach (T item in source)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static T[] Range<T>(this T[] source, int start, int count)
	{
		if (start < 0 || count <= 0)
		{
			return null;
		}
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = source[i + start];
		}
		return array;
	}

	public static int Sum<T>(this T[] source, Func<T, int> selector)
	{
		if (source == null)
		{
			return 0;
		}
		int num = 0;
		foreach (T arg in source)
		{
			num += selector(arg);
		}
		return num;
	}

	public static T[] Shuffle<T>(this T[] source)
	{
		if (source == null)
		{
			return null;
		}
		if (source.Length == 0)
		{
			return new T[0];
		}
		int num = source.Length;
		T[] array = new T[num];
		Array.Copy(source, array, num);
		Random random = new Random();
		int num2 = num;
		while (1 < num2)
		{
			num2--;
			int num3 = random.Next(num2 + 1);
			T val = array[num3];
			array[num3] = array[num2];
			array[num2] = val;
		}
		return array;
	}

	public static T Pop<T>(this List<T> source)
	{
		if (source.IsNullOrEmpty())
		{
			return default(T);
		}
		T result = source.FirstOrDefault();
		source.RemoveAt(0);
		return result;
	}

	public static void PushFront<T>(this List<T> source, T item)
	{
		source?.Insert(0, item);
	}

	public static T[] Shuffle<T>(this List<T> source)
	{
		if (source.IsNullOrEmpty())
		{
			return null;
		}
		int count = source.Count;
		T[] array = new T[count];
		Array.Copy(source.ToArray(), array, count);
		Random random = new Random();
		int num = count;
		while (1 < num)
		{
			num--;
			int num2 = random.Next(num + 1);
			T val = array[num2];
			array[num2] = array[num];
			array[num] = val;
		}
		return array;
	}

	public static List<T> Range<T>(this List<T> source, int start, int count)
	{
		if (start < 0 || count <= 0)
		{
			return null;
		}
		List<T> list = new List<T>();
		for (int i = 0; i < count; i++)
		{
			list[i] = source[i + start];
		}
		return list;
	}

	public static int Sum<T>(this List<T> source, Func<T, int> selector)
	{
		if (source == null)
		{
			return 0;
		}
		int num = 0;
		foreach (T item in source)
		{
			num += selector(item);
		}
		return num;
	}

	public static float Sum<T>(this List<T> source, Func<T, float> selector)
	{
		if (source == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (T item in source)
		{
			num += selector(item);
		}
		return num;
	}

	public static T GetElement<T>(this T[] source, int index)
	{
		if (source.IsNullOrEmpty())
		{
			return default(T);
		}
		if (index >= 0 && index < source.Length)
		{
			return source[index];
		}
		return default(T);
	}

	public static T GetElement<T>(this List<T> source, int index)
	{
		if (source.IsNullOrEmpty())
		{
			return default(T);
		}
		if (index >= 0 && index < source.Count)
		{
			return source[index];
		}
		return default(T);
	}

	public static T GetElement<T>(this ReadOnlyCollection<T> source, int index)
	{
		if (source.IsNullOrEmpty())
		{
			return default(T);
		}
		if (index >= 0 && index < source.Count)
		{
			return source[index];
		}
		return default(T);
	}

	public static KeyValuePair<TKey, TValue> Max<TKey, TValue>(this Dictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, float> func)
	{
		float num = 0f;
		KeyValuePair<TKey, TValue> result = default(KeyValuePair<TKey, TValue>);
		foreach (KeyValuePair<TKey, TValue> item in source)
		{
			float num2 = func(item);
			if (!(num2 <= num))
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}
}
