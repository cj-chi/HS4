using System;
using System.IO;
using MessagePack;

namespace AIProject;

[MessagePackObject(false)]
public class GlobalSaveData
{
	[Key(0)]
	public bool Cleared { get; set; }

	public void Copy(GlobalSaveData source)
	{
		Cleared = source.Cleared;
	}

	public void SaveFile(byte[] buffer)
	{
		using MemoryStream stream = new MemoryStream(buffer);
		SaveFile(stream);
	}

	public void SaveFile(string path)
	{
		using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
		SaveFile(stream);
	}

	public void SaveFile(Stream stream)
	{
		using BinaryWriter writer = new BinaryWriter(stream);
		SaveFile(writer);
	}

	public void SaveFile(BinaryWriter writer)
	{
		byte[] buffer = MessagePackSerializer.Serialize(this);
		writer.Write(buffer);
	}

	public static GlobalSaveData LoadFile(string fileName)
	{
		GlobalSaveData globalSaveData = new GlobalSaveData();
		if (globalSaveData.Load(fileName))
		{
			return globalSaveData;
		}
		return null;
	}

	public bool Load(string fileName)
	{
		try
		{
			using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (fileStream.Length == 0L)
			{
				return false;
			}
			return Load(fileStream);
		}
		catch (Exception ex)
		{
			_ = ex is FileNotFoundException;
			return false;
		}
	}

	public bool Load(Stream stream)
	{
		using BinaryReader reader = new BinaryReader(stream);
		return Load(reader);
	}

	public bool Load(BinaryReader reader)
	{
		try
		{
			byte[] array = reader.ReadBytes((int)reader.BaseStream.Length);
			if (array.IsNullOrEmpty())
			{
				return false;
			}
			GlobalSaveData source = MessagePackSerializer.Deserialize<GlobalSaveData>(array);
			Copy(source);
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}
}
