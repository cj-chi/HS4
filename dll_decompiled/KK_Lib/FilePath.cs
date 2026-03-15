using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manager;

namespace KK_Lib;

public class FilePath
{
	public enum KindEN
	{
		SelfMade,
		AnotherWork,
		Preset
	}

	public KindEN kind = KindEN.Preset;

	public string path = "";

	public DateTime time;

	public static List<FilePath> GetFiles(string _path)
	{
		List<KeyValuePair<DateTime, string>> list = (from s in Directory.GetFiles(_path, "*.png", SearchOption.TopDirectoryOnly)
			select new KeyValuePair<DateTime, string>(File.GetLastWriteTime(s), s)).ToList();
		using (new GameSystem.CultureScope())
		{
			list.Sort((KeyValuePair<DateTime, string> a, KeyValuePair<DateTime, string> b) => b.Key.CompareTo(a.Key));
		}
		return list.Select((KeyValuePair<DateTime, string> v) => new FilePath(v.Value, v.Key)).ToList();
	}

	public FilePath(string _path, DateTime _time, KindEN _kind = KindEN.Preset)
	{
		path = _path;
		time = _time;
		kind = _kind;
	}
}
