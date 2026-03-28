using System.Collections.Generic;
using System.IO;
using Illusion;

namespace ADV;

public static class AlreadyReadInfo
{
	private static string Path { get; }

	private static string FileName { get; }

	private static HashSet<int> read { get; }

	static AlreadyReadInfo()
	{
		Path = "save";
		FileName = "read.dat";
		read = new HashSet<int>();
		read.Add(0);
		Load();
	}

	public static bool Add(int i)
	{
		return read.Add(i);
	}

	public static void Save()
	{
		Utils.File.OpenWrite(UserData.Create(Path) + FileName, isAppend: false, delegate(FileStream f)
		{
			using BinaryWriter binaryWriter = new BinaryWriter(f);
			binaryWriter.Write(read.Count);
			foreach (int item in read)
			{
				binaryWriter.Write(item);
			}
		});
	}

	private static bool Load()
	{
		return Utils.File.OpenRead(UserData.Path + Path + "/" + FileName, delegate(FileStream f)
		{
			using BinaryReader binaryReader = new BinaryReader(f);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				read.Add(binaryReader.ReadInt32());
			}
		});
	}
}
