using System.Collections.Generic;

namespace PlaceholderSoftware.WetStuff.Extensions;

internal static class IListExtensions
{
	public static void RemoveNulls<T>([NotNull][ItemCanBeNull] this IList<T> list, int nulls) where T : class
	{
		if (nulls == 0)
		{
			return;
		}
		int num = 0;
		int i;
		for (i = 0; i < list.Count && list[i] != null; i++)
		{
		}
		for (; i < list.Count; i++)
		{
			if (num >= nulls)
			{
				break;
			}
			if (list[i] == null)
			{
				num++;
			}
			else
			{
				list[i - num] = list[i];
			}
		}
		for (; i < list.Count; i++)
		{
			list[i - num] = list[i];
		}
		for (int j = 0; j < num; j++)
		{
			list.RemoveAt(list.Count - 1);
		}
	}
}
