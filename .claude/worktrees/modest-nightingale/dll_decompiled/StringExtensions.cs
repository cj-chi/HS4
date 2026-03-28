using System.Collections.Generic;
using System.Linq;

internal static class StringExtensions
{
	public static bool ContainsAll(this string str, IEnumerable<string> needles)
	{
		if (str == null)
		{
			return false;
		}
		return needles.All(str.Contains);
	}

	public static bool ContainsAny(this string str, IEnumerable<string> needles)
	{
		if (str == null)
		{
			return false;
		}
		return needles.Any(str.Contains);
	}

	public static bool ContainsAll(this string str, params string[] needles)
	{
		return str.ContainsAll(needles.AsEnumerable());
	}

	public static bool ContainsAny(this string str, params string[] needles)
	{
		return str.ContainsAny(needles.AsEnumerable());
	}
}
