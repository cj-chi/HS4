using System;
using System.Threading.Tasks;

namespace AIProject;

public static class SystemUtil
{
	public static async Task TryProcAsync(Task task)
	{
		try
		{
			await task;
		}
		catch (Exception)
		{
		}
	}

	public static async Task<T> TryProcAsync<T>(Task<T> task)
	{
		try
		{
			return await task;
		}
		catch (Exception)
		{
		}
		return default(T);
	}

	public static bool TryParse(string input, out int result)
	{
		if (!int.TryParse(input, out result))
		{
			return false;
		}
		return true;
	}

	public static bool TryParse(string input, out float result)
	{
		if (!float.TryParse(input, out result))
		{
			return false;
		}
		return true;
	}

	public static bool TryParse<TEnum>(string input, out TEnum result) where TEnum : struct
	{
		if (!Enum.TryParse<TEnum>(input, out result))
		{
			return false;
		}
		return true;
	}

	public static bool SetSafeStruct<T>(ref T destination, T newValue) where T : struct
	{
		if (destination.Equals(newValue))
		{
			return false;
		}
		destination = newValue;
		return true;
	}

	public static bool SetSafeClass<T>(ref T destination, T newValue) where T : class
	{
		if ((destination == null && newValue == null) || (destination != null && destination.Equals(newValue)))
		{
			return false;
		}
		destination = newValue;
		return true;
	}

	public static string Replace(this string source, string newValue, params string[] oldValues)
	{
		if (source.IsNullOrEmpty())
		{
			return source;
		}
		string text = source;
		foreach (string oldValue in oldValues)
		{
			text = text.Replace(oldValue, newValue);
		}
		return text;
	}
}
