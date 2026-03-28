using System.Collections.Generic;
using System.Linq;
using AIChara;
using Manager;

namespace GameLoadCharaFileSystem;

public class GameCharaFileInfoAssist
{
	private static void AddList(List<GameCharaFileInfo> _list, string path, int filterKind, byte sex, bool useMyData, bool useDownload, bool futanariOnly, bool preset, bool isCheckPlayerSelect, bool isCheckRoomSelect)
	{
		string[] searchPattern = new string[1] { "*.png" };
		string userUUID = Singleton<GameSystem>.Instance.UserUUID;
		SaveData saveData = Singleton<Game>.Instance.saveData;
		CategoryKind categoryKind = ((sex == 0) ? CategoryKind.Male : CategoryKind.Female);
		if (preset)
		{
			categoryKind |= CategoryKind.Preset;
		}
		FolderAssist folderAssist = new FolderAssist();
		folderAssist.CreateFolderInfoEx(path, searchPattern);
		int fileCount = folderAssist.GetFileCount();
		int num = 0;
		for (int i = 0; i < fileCount; i++)
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			if (!chaFileControl.LoadCharaFile(folderAssist.lstFile[i].FullPath))
			{
				chaFileControl.GetLastErrorCode();
			}
			else
			{
				if (chaFileControl.parameter.sex != sex || (sex == 1 && futanariOnly && !chaFileControl.parameter.futanari))
				{
					continue;
				}
				CategoryKind categoryKind2 = (CategoryKind)0;
				if (!preset)
				{
					if (userUUID == chaFileControl.userID || chaFileControl.userID == "illusion-2019-1025-xxxx-aisyoujyocha")
					{
						if (!useMyData)
						{
							continue;
						}
						categoryKind2 = CategoryKind.MyData;
					}
					else
					{
						if (!useDownload)
						{
							continue;
						}
						categoryKind2 = CategoryKind.Download;
					}
				}
				string text = "";
				text = ((sex == 0) ? "" : (Voice.infoTable.TryGetValue(chaFileControl.parameter2.personality, out var value) ? value.Personality : "不明"));
				string fileName = folderAssist.lstFile[i].FileName;
				HashSet<int> hashSet = null;
				if (filterKind == 0)
				{
					hashSet = new HashSet<int>(from anon in saveData.roomList.Select((List<string> name, int index) => new { name, index })
						where anon.name.Contains(fileName)
						select anon.index + 1);
					if (hashSet.Count == 0)
					{
						hashSet.Add(0);
					}
				}
				else
				{
					hashSet = new HashSet<int> { sex };
				}
				_list.Add(new GameCharaFileInfo
				{
					index = num++,
					name = chaFileControl.parameter.fullname,
					personality = text,
					voice = chaFileControl.parameter2.personality,
					hair = chaFileControl.custom.hair.kind,
					birthMonth = chaFileControl.parameter.birthMonth,
					birthDay = chaFileControl.parameter.birthDay,
					strBirthDay = chaFileControl.parameter.strBirthDay,
					sex = chaFileControl.parameter.sex,
					FullPath = folderAssist.lstFile[i].FullPath,
					FileName = fileName,
					time = folderAssist.lstFile[i].time,
					futanari = chaFileControl.parameter.futanari,
					state = chaFileControl.gameinfo2.nowDrawState,
					trait = chaFileControl.parameter2.trait,
					hAttribute = chaFileControl.parameter2.hAttribute,
					resistH = chaFileControl.gameinfo2.resistH,
					resistPain = chaFileControl.gameinfo2.resistPain,
					resistAnal = chaFileControl.gameinfo2.resistAnal,
					broken = chaFileControl.gameinfo2.Broken,
					dependence = chaFileControl.gameinfo2.Dependence,
					usedItem = chaFileControl.gameinfo2.usedItem,
					lockNowState = chaFileControl.gameinfo2.lockNowState,
					lockBroken = chaFileControl.gameinfo2.lockBroken,
					lockDependence = chaFileControl.gameinfo2.lockDependence,
					hcount = chaFileControl.gameinfo2.hCount,
					lstFilter = hashSet,
					isEntry = ((!isCheckPlayerSelect) ? (isCheckRoomSelect && saveData.roomList.Where((List<string> l) => l.Contains(fileName)).Any()) : ((saveData.playerChara.FileName == fileName && saveData.playerChara.Sex == sex) || (saveData.secondPlayerChara.FileName == fileName && saveData.secondPlayerChara.Sex == sex))),
					cateKind = (categoryKind | categoryKind2),
					data_uuid = chaFileControl.dataID
				});
			}
		}
	}

	public static List<GameCharaFileInfo> CreateCharaFileInfoList(int filterKind, bool useMale, bool useFemale, bool useFemaleFutanariOnly = false, bool isCheckPlayerSelect = false, bool isCheckRoomSelect = false, bool useMyData = true, bool useDownload = true)
	{
		List<GameCharaFileInfo> list = new List<GameCharaFileInfo>();
		if (useMale)
		{
			string path = UserData.Path + "chara/male/";
			AddList(list, path, filterKind, 0, useMyData, useDownload, useFemaleFutanariOnly, preset: false, isCheckPlayerSelect: false, isCheckRoomSelect: false);
		}
		if (useFemale)
		{
			string path2 = UserData.Path + "chara/female/";
			AddList(list, path2, filterKind, 1, useMyData, useDownload, useFemaleFutanariOnly, preset: false, isCheckPlayerSelect, isCheckRoomSelect);
		}
		return list;
	}
}
