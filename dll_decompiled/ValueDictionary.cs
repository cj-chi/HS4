using System.Collections.Generic;

public class ValueDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
}
public class ValueDictionary<TKey1, TKey2, TValue> : Dictionary<TKey1, ValueDictionary<TKey2, TValue>>
{
}
public class ValueDictionary<TKey1, TKey2, TKey3, TValue> : Dictionary<TKey1, ValueDictionary<TKey2, TKey3, TValue>>
{
}
public class ValueDictionary<TKey1, TKey2, TKey3, TKey4, TValue> : Dictionary<TKey1, ValueDictionary<TKey2, TKey3, TKey4, TValue>>
{
}
public class ValueDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TValue> : Dictionary<TKey1, ValueDictionary<TKey2, TKey3, TKey4, TKey5, TValue>>
{
}
public class ValueDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue> : Dictionary<TKey1, ValueDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>>
{
}
