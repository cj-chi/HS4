using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Studio;

internal static class Utility
{
	public static bool SetColor(ref Color currentValue, Color newValue)
	{
		if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
		{
			return false;
		}
		currentValue = newValue;
		return true;
	}

	public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
	{
		if (currentValue.Equals(newValue))
		{
			return false;
		}
		currentValue = newValue;
		return true;
	}

	public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
	{
		if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
		{
			return false;
		}
		currentValue = newValue;
		return true;
	}

	public static void SaveColor(BinaryWriter _writer, Color _color)
	{
		_writer.Write(_color.r);
		_writer.Write(_color.g);
		_writer.Write(_color.b);
		_writer.Write(_color.a);
	}

	public static Color LoadColor(BinaryReader _reader)
	{
		Color result = default(Color);
		result.r = _reader.ReadSingle();
		result.g = _reader.ReadSingle();
		result.b = _reader.ReadSingle();
		result.a = _reader.ReadSingle();
		return result;
	}

	public static T LoadAsset<T>(string _bundle, string _file, string _manifest) where T : UnityEngine.Object
	{
		return CommonLib.LoadAsset<T>(_bundle, _file, clone: true, _manifest);
	}

	public static float StringToFloat(string _text)
	{
		float result = 0f;
		if (!float.TryParse(_text, out result))
		{
			return 0f;
		}
		return result;
	}

	public static string GetCurrentTime()
	{
		DateTime now = DateTime.Now;
		return $"{now.Year}_{now.Month:00}{now.Day:00}_{now.Hour:00}{now.Minute:00}_{now.Second:00}_{now.Millisecond:000}";
	}

	public static Color ConvertColor(int _r, int _g, int _b)
	{
		if (!ColorUtility.TryParseHtmlString($"#{_r:X2}{_g:X2}{_b:X2}", out var color))
		{
			return Color.clear;
		}
		return color;
	}

	public static string GetManifest(string _bundlePath, string _file)
	{
		if (AssetBundleCheck.IsSimulation)
		{
			return "";
		}
		foreach (KeyValuePair<string, AssetBundleManager.BundlePack> item in AssetBundleManager.ManifestBundlePack.Where((KeyValuePair<string, AssetBundleManager.BundlePack> v) => Regex.Match(v.Key, "studio(\\d*)").Success))
		{
			if (item.Value.AssetBundleManifest.GetAllAssetBundles().Any((string _s) => _s == _bundlePath))
			{
				return item.Key;
			}
		}
		return "";
	}
}
