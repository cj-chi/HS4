using System;
using System.Collections.Generic;
using System.IO;
using ADV;
using Illusion;
using Manager;
using MessagePack;

[MessagePackObject(false)]
public sealed class AppendSaveData : ICommandData
{
	[IgnoreMember]
	private const string SaveDataVersion = "0.0.0";

	[IgnoreMember]
	private const string path = "save/";

	[IgnoreMember]
	private const string fileName = "save_append.dat";

	[Key(1)]
	public List<int> appendEvents = new List<int>(16);

	[Key(6)]
	public List<int> mapReleases = new List<int>(16);

	public static string Path => UserData.Path + "save/";

	[Key(0)]
	public Version Version { get; set; } = new Version(0, 0, 0);

	[Key(2)]
	public int AppendTutorialNo { get; set; }

	[Key(3)]
	public bool IsFurSitri3P { get; set; }

	[Key(4)]
	public int IsAppendStart { get; set; }

	[Key(5)]
	public int SitriSelectCount { get; set; }

	[IgnoreMember]
	public bool IsStartNotice { get; set; }

	public void Initialize()
	{
		Version = new Version("0.0.0");
		appendEvents = new List<int>(16);
		AppendTutorialNo = 0;
		IsFurSitri3P = false;
		IsAppendStart = 0;
		SitriSelectCount = 0;
		mapReleases = new List<int>(16);
		IsStartNotice = false;
	}

	public static void AddApeendEvents(int _event)
	{
		if (Singleton<Game>.IsInstance())
		{
			AppendSaveData appendSaveData = Singleton<Game>.Instance.appendSaveData;
			if (!appendSaveData.appendEvents.Contains(_event))
			{
				appendSaveData.appendEvents.Add(_event);
			}
		}
	}

	public static bool IsApeendEvents(int _event)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return false;
		}
		return Singleton<Game>.Instance.appendSaveData.appendEvents.Contains(_event);
	}

	public IEnumerable<CommandData> CreateCommandData(string head)
	{
		List<CommandData> list = new List<CommandData>();
		string key = head + "AppendTutorialNo";
		list.Add(new CommandData(CommandData.Command.Int, key, () => AppendTutorialNo, delegate(object o)
		{
			AppendTutorialNo = (int)o;
		}));
		return list;
	}

	public void Save()
	{
		Save(UserData.Create("save/"), "save_append.dat");
	}

	public void Save(string _path, string _fileName)
	{
		Utils.File.OpenWrite(_path + _fileName, isAppend: false, delegate(FileStream f)
		{
			using BinaryWriter binaryWriter = new BinaryWriter(f);
			byte[] buffer = MessagePackSerializer.Serialize(this);
			binaryWriter.Write(buffer);
		});
	}

	public bool Load()
	{
		return Load(Path + "/", "save_append.dat");
	}

	public bool Load(string _path, string _fileName)
	{
		return Utils.File.OpenRead(_path + _fileName, delegate(FileStream f)
		{
			using BinaryReader binaryReader = new BinaryReader(f);
			byte[] array = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
			if (!((IReadOnlyCollection<byte>)(object)array).IsNullOrEmpty())
			{
				AppendSaveData source = MessagePackSerializer.Deserialize<AppendSaveData>(array);
				Copy(source);
			}
		});
	}

	public void Copy(AppendSaveData source)
	{
		Version = source.Version;
		if (source.appendEvents != null)
		{
			appendEvents.Clear();
			appendEvents.AddRange(source.appendEvents);
		}
		AppendTutorialNo = source.AppendTutorialNo;
		IsFurSitri3P = source.IsFurSitri3P;
		IsAppendStart = source.IsAppendStart;
		SitriSelectCount = source.SitriSelectCount;
		if (source.mapReleases != null)
		{
			mapReleases.Clear();
			mapReleases.AddRange(source.mapReleases);
		}
	}
}
