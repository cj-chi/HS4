using System.Collections.Generic;
using AIChara;

namespace CoordinateFileSystem;

public class CoordinateFileInfoAssist
{
	private static void AddList(List<CoordinateFileInfo> _list, string path)
	{
		string[] searchPattern = new string[1] { "*.png" };
		FolderAssist folderAssist = new FolderAssist();
		folderAssist.CreateFolderInfoEx(path, searchPattern);
		int fileCount = folderAssist.GetFileCount();
		int num = 0;
		for (int i = 0; i < fileCount; i++)
		{
			ChaFileCoordinate chaFileCoordinate = new ChaFileCoordinate();
			if (!chaFileCoordinate.LoadFile(folderAssist.lstFile[i].FullPath))
			{
				chaFileCoordinate.GetLastErrorCode();
				continue;
			}
			string fileName = folderAssist.lstFile[i].FileName;
			_list.Add(new CoordinateFileInfo
			{
				index = num++,
				name = chaFileCoordinate.coordinateName,
				FullPath = folderAssist.lstFile[i].FullPath,
				FileName = fileName,
				time = folderAssist.lstFile[i].time
			});
		}
	}

	public static List<CoordinateFileInfo> CreateCharaFileInfoList(int _sex = 1)
	{
		List<CoordinateFileInfo> list = new List<CoordinateFileInfo>();
		string path = UserData.Path + ((_sex == 0) ? "coordinate/male/" : "coordinate/female/");
		AddList(list, path);
		return list;
	}
}
