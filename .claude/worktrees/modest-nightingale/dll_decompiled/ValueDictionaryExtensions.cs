public static class ValueDictionaryExtensions
{
	public static ValueDictionary<TKey2, TValue> New<TKey1, TKey2, TValue>(this ValueDictionary<TKey1, TKey2, TValue> dictionary)
	{
		return new ValueDictionary<TKey2, TValue>();
	}

	public static ValueDictionary<TKey2, TKey3, TValue> New<TKey1, TKey2, TKey3, TValue>(this ValueDictionary<TKey1, TKey2, TKey3, TValue> dictionary)
	{
		return new ValueDictionary<TKey2, TKey3, TValue>();
	}

	public static ValueDictionary<TKey2, TKey3, TKey4, TValue> New<TKey1, TKey2, TKey3, TKey4, TValue>(this ValueDictionary<TKey1, TKey2, TKey3, TKey4, TValue> dictionary)
	{
		return new ValueDictionary<TKey2, TKey3, TKey4, TValue>();
	}

	public static ValueDictionary<TKey2, TKey3, TKey4, TKey5, TValue> New<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(this ValueDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TValue> dictionary)
	{
		return new ValueDictionary<TKey2, TKey3, TKey4, TKey5, TValue>();
	}

	public static ValueDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue> New<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue>(this ValueDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue> dictionary)
	{
		return new ValueDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>();
	}
}
