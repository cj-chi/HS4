using System.Collections.Generic;
using AIChara;
using Manager;

namespace CharaCustom;

public class CustomCharaFileInfoAssist
{
	private static void AddList(List<CustomCharaFileInfo> _list, string path, byte sex, bool useMyData, bool useDownload, bool preset, bool _isFindSaveData, ref int idx)
	{
		string[] searchPattern = new string[1] { "*.png" };
		string userUUID = Singleton<GameSystem>.Instance.UserUUID;
		CharaCategoryKind charaCategoryKind = ((sex == 0) ? CharaCategoryKind.Male : CharaCategoryKind.Female);
		if (preset)
		{
			charaCategoryKind |= CharaCategoryKind.Preset;
		}
		FolderAssist folderAssist = new FolderAssist();
		folderAssist.CreateFolderInfoEx(path, searchPattern);
		int fileCount = folderAssist.GetFileCount();
		for (int i = 0; i < fileCount; i++)
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			if (!chaFileControl.LoadCharaFile(folderAssist.lstFile[i].FullPath))
			{
				chaFileControl.GetLastErrorCode();
			}
			else
			{
				if (chaFileControl.parameter.sex != sex)
				{
					continue;
				}
				CharaCategoryKind charaCategoryKind2 = (CharaCategoryKind)0;
				if (!preset)
				{
					if (userUUID == chaFileControl.userID)
					{
						if (!useMyData)
						{
							continue;
						}
						charaCategoryKind2 = CharaCategoryKind.MyData;
					}
					else
					{
						if (!useDownload)
						{
							continue;
						}
						charaCategoryKind2 = CharaCategoryKind.Download;
					}
				}
				string text = "";
				text = ((sex == 0) ? "" : (Voice.infoTable.TryGetValue(chaFileControl.parameter2.personality, out var value) ? value.Get(Singleton<GameSystem>.Instance.languageInt) : "不明"));
				_list.Add(new CustomCharaFileInfo
				{
					index = idx++,
					name = chaFileControl.parameter.fullname,
					personality = text,
					voice = chaFileControl.parameter2.personality,
					height = chaFileControl.custom.GetHeightKind(),
					bustSize = chaFileControl.custom.GetBustSizeKind(),
					hair = chaFileControl.custom.hair.kind,
					birthMonth = chaFileControl.parameter.birthMonth,
					birthDay = chaFileControl.parameter.birthDay,
					strBirthDay = ChaFileDefine.GetBirthdayStr(chaFileControl.parameter.birthMonth, chaFileControl.parameter.birthDay, Singleton<GameSystem>.Instance.language),
					sex = chaFileControl.parameter.sex,
					FullPath = folderAssist.lstFile[i].FullPath,
					FileName = folderAssist.lstFile[i].FileName,
					time = folderAssist.lstFile[i].time,
					isChangeParameter = chaFileControl.gameinfo2.isChangeParameter,
					trait = chaFileControl.parameter2.trait,
					mind = chaFileControl.parameter2.mind,
					hAttribute = chaFileControl.parameter2.hAttribute,
					futanari = chaFileControl.parameter.futanari,
					cateKind = (charaCategoryKind | charaCategoryKind2),
					data_uuid = chaFileControl.dataID,
					isInSaveData = (_isFindSaveData && SaveData.IsRoomListChara(folderAssist.lstFile[i].FileName))
				});
			}
		}
	}

	public static List<CustomCharaFileInfo> CreateCharaFileInfoList(bool useMale, bool useFemale, bool useMyData = true, bool useDownload = true, bool usePreset = true, bool _isFindSaveData = true)
	{
		List<CustomCharaFileInfo> list = new List<CustomCharaFileInfo>();
		int idx = 0;
		if (usePreset)
		{
			if (useMale)
			{
				string path = DefaultData.Path + "chara/male/";
				AddList(list, path, 0, useMyData: false, useDownload: false, preset: true, _isFindSaveData: false, ref idx);
			}
			if (useFemale)
			{
				string path2 = DefaultData.Path + "chara/female/";
				AddList(list, path2, 1, useMyData: false, useDownload: false, preset: true, _isFindSaveData: false, ref idx);
			}
		}
		if (useMyData || useDownload)
		{
			if (useMale)
			{
				string path3 = UserData.Path + "chara/male/";
				AddList(list, path3, 0, useMyData, useDownload, preset: false, _isFindSaveData, ref idx);
			}
			if (useFemale)
			{
				string path4 = UserData.Path + "chara/female/";
				AddList(list, path4, 1, useMyData, useDownload, preset: false, _isFindSaveData, ref idx);
			}
		}
		return list;
	}
}
