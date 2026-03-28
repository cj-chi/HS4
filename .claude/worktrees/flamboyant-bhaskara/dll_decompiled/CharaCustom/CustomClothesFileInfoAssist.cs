using System.Collections.Generic;
using AIChara;

namespace CharaCustom;

public class CustomClothesFileInfoAssist
{
	private static void AddList(List<CustomClothesFileInfo> _list, string path, byte sex, bool preset, ref int idx)
	{
		string[] searchPattern = new string[1] { "*.png" };
		CoordinateCategoryKind coordinateCategoryKind = ((sex == 0) ? CoordinateCategoryKind.Male : CoordinateCategoryKind.Female);
		if (preset)
		{
			coordinateCategoryKind |= CoordinateCategoryKind.Preset;
		}
		FolderAssist folderAssist = new FolderAssist();
		folderAssist.CreateFolderInfoEx(path, searchPattern);
		int fileCount = folderAssist.GetFileCount();
		for (int i = 0; i < fileCount; i++)
		{
			ChaFileCoordinate chaFileCoordinate = new ChaFileCoordinate();
			if (!chaFileCoordinate.LoadFile(folderAssist.lstFile[i].FullPath))
			{
				chaFileCoordinate.GetLastErrorCode();
				continue;
			}
			_list.Add(new CustomClothesFileInfo
			{
				index = idx++,
				name = chaFileCoordinate.coordinateName,
				FullPath = folderAssist.lstFile[i].FullPath,
				FileName = folderAssist.lstFile[i].FileName,
				time = folderAssist.lstFile[i].time,
				cateKind = coordinateCategoryKind
			});
		}
	}

	public static List<CustomClothesFileInfo> CreateClothesFileInfoList(bool useMale, bool useFemale, bool useMyData = true, bool usePreset = true)
	{
		List<CustomClothesFileInfo> list = new List<CustomClothesFileInfo>();
		int idx = 0;
		if (usePreset)
		{
			if (useMale)
			{
				string path = DefaultData.Path + "coordinate/male/";
				AddList(list, path, 0, preset: true, ref idx);
			}
			if (useFemale)
			{
				string path2 = DefaultData.Path + "coordinate/female/";
				AddList(list, path2, 1, preset: true, ref idx);
			}
		}
		if (useMyData)
		{
			if (useMale)
			{
				string path3 = UserData.Path + "coordinate/male/";
				AddList(list, path3, 0, preset: false, ref idx);
			}
			if (useFemale)
			{
				string path4 = UserData.Path + "coordinate/female/";
				AddList(list, path4, 1, preset: false, ref idx);
			}
		}
		return list;
	}
}
