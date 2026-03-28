using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Manager;
using UnityEngine;

namespace HS2;

public class GlobalHS2Calc
{
	public class MapCharaSelectInfo
	{
		public int mapID;

		public int eventID;

		public List<(string, int)> lstChara = new List<(string, int)>();
	}

	public class CharaInfo
	{
		public int id;

		public ChaFileControl chaFile = new ChaFileControl();

		public int eventID = -1;

		public int mapID = -1;

		public int main = -1;
	}

	public static List<MapCharaSelectInfo> PlaceCharaOnTheMap(int _selectGropu)
	{
		SaveData save = Singleton<Game>.Instance.saveData;
		Dictionary<int, MapCharaSelectInfo> dicMapCharaSelect = new Dictionary<int, MapCharaSelectInfo>();
		List<CharaInfo> list = new List<CharaInfo>();
		List<int> useMap = new List<int>();
		for (int i = 0; i < save.roomList[_selectGropu].Count; i++)
		{
			string filename = save.roomList[_selectGropu][i];
			CharaInfo charaInfo = new CharaInfo
			{
				id = i
			};
			charaInfo.chaFile.LoadCharaFile(filename, 1);
			list.Add(charaInfo);
		}
		SelectEvent(list);
		List<CharaInfo> list2 = list.Where((CharaInfo ci) => ci.eventID == 2 || ci.eventID == 14).Shuffle().ToList();
		List<CharaInfo> list3 = list.Where((CharaInfo ci) => ci.main == -1).Shuffle().ToList();
		for (int num = 0; num < list2.Count; num++)
		{
			CharaInfo charaInfo2 = list2[num];
			List<CharaInfo> list4 = new List<CharaInfo>();
			for (int num2 = 0; num2 < list3.Count; num2++)
			{
				CharaInfo charaInfo3 = list3[num2];
				if (charaInfo3.main != -1 || charaInfo3.eventID == 24 || charaInfo3.eventID == 16)
				{
					continue;
				}
				if (charaInfo2.eventID == 14)
				{
					if (!new List<int> { 3, 4, 5, 6, 7, 21 }.Contains(charaInfo3.eventID) && IsPCSleepWFera(charaInfo3.chaFile.gameinfo2))
					{
						list4.Add(charaInfo3);
					}
				}
				else if (charaInfo2.eventID == 2 && charaInfo3.eventID == 0)
				{
					list4.Add(charaInfo3);
				}
			}
			if (list4.Any())
			{
				CharaInfo charaInfo4 = list4.Shuffle().FirstOrDefault();
				charaInfo4.eventID = charaInfo2.eventID;
				charaInfo4.main = 1;
			}
			else
			{
				charaInfo2.eventID = -1;
				charaInfo2.main = -1;
			}
		}
		list2 = list2.Where((CharaInfo ci) => ci.main == -1).ToList();
		SelectEvent(list2, _isMultipleEvent: false);
		HashSet<int> used = new HashSet<int>();
		List<CharaInfo> list5 = list.Where((CharaInfo lc) => lc.eventID == 24).Shuffle().ToList();
		list5.ForEach(delegate(CharaInfo lc)
		{
			SelectMapID(lc, useMap, used);
		});
		useMap = list5.Select((CharaInfo lc) => lc.mapID).ToList();
		List<CharaInfo> list6 = list.Where((CharaInfo lc) => lc.eventID == 16).Shuffle().ToList();
		used.Clear();
		list6.ForEach(delegate(CharaInfo lc)
		{
			SelectMapID(lc, useMap, used);
		});
		useMap.AddRange(list6.Select((CharaInfo lc) => lc.mapID));
		List<CharaInfo> list7 = list.Where((CharaInfo lc) => lc.eventID == 14 && lc.main == 0).Shuffle().ToList();
		List<CharaInfo> list8 = list.Where((CharaInfo lc) => lc.eventID == 14 && lc.main == 1).Shuffle().ToList();
		if (list7.Count == list8.Count)
		{
			used.Clear();
			List<CharaInfo> list9 = new List<CharaInfo>();
			for (int num3 = 0; num3 < list7.Count; num3++)
			{
				list9.Add(list7[num3]);
				list9.Add(list8[num3]);
			}
			SelectMapIDMultiple(list9, useMap, used);
			useMap.AddRange(list9.Select((CharaInfo lc) => lc.mapID));
		}
		List<CharaInfo> list10 = list.Where((CharaInfo lc) => MathfEx.IsRange(3, lc.eventID, 13, isEqual: true) || lc.eventID == 15 || lc.eventID == 21).Shuffle().ToList();
		used.Clear();
		list10.ForEach(delegate(CharaInfo lc)
		{
			SelectMapID(lc, useMap, used);
		});
		useMap.AddRange(list10.Select((CharaInfo lc) => lc.mapID));
		List<CharaInfo> list11 = list.Where((CharaInfo lc) => MathfEx.IsRange(17, lc.eventID, 19, isEqual: true)).Shuffle().ToList();
		used.Clear();
		list11.ForEach(delegate(CharaInfo lc)
		{
			SelectMapID(lc, useMap, used);
		});
		useMap.AddRange(list11.Select((CharaInfo lc) => lc.mapID));
		List<CharaInfo> list12 = list.Where((CharaInfo lc) => lc.eventID == 2 && lc.main == 0).Shuffle().ToList();
		List<CharaInfo> list13 = list.Where((CharaInfo lc) => lc.eventID == 2 && lc.main == 1).Shuffle().ToList();
		if (list12.Count == list13.Count)
		{
			used.Clear();
			List<CharaInfo> list14 = new List<CharaInfo>();
			for (int num4 = 0; num4 < list12.Count; num4++)
			{
				list14.Add(list12[num4]);
				list14.Add(list13[num4]);
			}
			SelectMapIDMultiple(list14, useMap, used);
			useMap.AddRange(list14.Select((CharaInfo lc) => lc.mapID));
		}
		foreach (CharaInfo item in list.Where((CharaInfo lc) => lc.mapID == -1).ToList())
		{
			ChaFileGameInfo2 gameinfo = item.chaFile.gameinfo2;
			if (gameinfo.nowDrawState == ChaFileDefine.State.Broken)
			{
				item.eventID = 22;
			}
			else if (gameinfo.nowDrawState == ChaFileDefine.State.Dependence)
			{
				item.eventID = 1;
			}
			else
			{
				item.eventID = 0;
			}
		}
		List<CharaInfo> list15 = list.Where((CharaInfo lc) => MathfEx.IsRange(0, lc.eventID, 1, isEqual: true) || lc.eventID == 22).Shuffle().ToList();
		used.Clear();
		list15.ForEach(delegate(CharaInfo lc)
		{
			SelectMapID(lc, useMap, used);
		});
		useMap.AddRange(list15.Select((CharaInfo lc) => lc.mapID));
		list = list.Shuffle().ToList();
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			CharaInfo charaInfo5 = list[num5];
			if (!dicMapCharaSelect.ContainsKey(charaInfo5.mapID))
			{
				dicMapCharaSelect.Add(charaInfo5.mapID, new MapCharaSelectInfo
				{
					eventID = charaInfo5.eventID,
					mapID = charaInfo5.mapID
				});
			}
			AddCharaSelect(charaInfo5, charaInfo5.main);
		}
		return dicMapCharaSelect.Values.ToList();
		void AddCharaSelect(CharaInfo _info, int _main)
		{
			List<(string, int)> lstChara = dicMapCharaSelect[_info.mapID].lstChara;
			if (lstChara.FirstOrDefault(((string, int) l) => l.Item2 == _main).Item1.IsNullOrEmpty())
			{
				lstChara.Add((save.roomList[_selectGropu][_info.id], _info.main));
			}
		}
	}

	public static MapCharaSelectInfo PlaceCharaOnTheMap(string _fileName, List<int> _useMap)
	{
		_ = Singleton<Game>.Instance.saveData;
		new Dictionary<int, MapCharaSelectInfo>();
		new List<CharaInfo>();
		CharaInfo charaInfo = new CharaInfo();
		charaInfo.chaFile.LoadCharaFile(_fileName, 1);
		SelectEvent(charaInfo);
		HashSet<int> used = new HashSet<int>();
		SelectMapID(charaInfo, _useMap, used);
		return new MapCharaSelectInfo
		{
			eventID = charaInfo.eventID,
			mapID = charaInfo.mapID,
			lstChara = { (_fileName, -1) }
		};
	}

	private static void SelectEvent(List<CharaInfo> _charaInfos, bool _isMultipleEvent = true)
	{
		bool flag = Singleton<Game>.Instance.saveData.TutorialNo != -1;
		for (int i = 0; i < _charaInfos.Count; i++)
		{
			CharaInfo charaInfo = _charaInfos[i];
			ChaFileGameInfo2 gameinfo = charaInfo.chaFile.gameinfo2;
			if (gameinfo.hCount == 0)
			{
				charaInfo.eventID = 24;
			}
			else if (gameinfo.escapeFlag == 1 && !flag && !Manager.Config.HData.EscapeStop)
			{
				charaInfo.eventID = 16;
			}
		}
		if (!flag)
		{
			for (int j = 0; j < _charaInfos.Count; j++)
			{
				CharaInfo charaInfo2 = _charaInfos[j];
				ChaFileGameInfo2 gameinfo2 = charaInfo2.chaFile.gameinfo2;
				if (charaInfo2.eventID == -1)
				{
					List<int> list = new List<int>();
					if (IsPeepingBathCategory(gameinfo2))
					{
						list.Add(3);
						list.Add(4);
					}
					if (IsPeepingToiletCategory(gameinfo2))
					{
						list.Add(5);
						list.Add(6);
					}
					if (IsNPCSleep(gameinfo2))
					{
						list.Add(7);
					}
					if (IsDependenceJealousyH(gameinfo2, charaInfo2.chaFile.charaFileName))
					{
						list.Add(21);
					}
					if (IsHappeningCategory(gameinfo2))
					{
						list.Add(17);
						list.Add(18);
						list.Add(19);
					}
					if (list.Any())
					{
						charaInfo2.eventID = list.Shuffle().First();
					}
				}
			}
			for (int k = 0; k < _charaInfos.Count; k++)
			{
				CharaInfo charaInfo3 = _charaInfos[k];
				ChaFileGameInfo2 gameinfo3 = charaInfo3.chaFile.gameinfo2;
				if (charaInfo3.eventID != -1)
				{
					continue;
				}
				List<int> list2 = new List<int>();
				if (IsFemaleIsAttackBathCategory(gameinfo3))
				{
					list2.Add(8);
					list2.Add(9);
				}
				if (IsFemaleIsAttackToiletCategory(gameinfo3))
				{
					list2.Add(10);
					list2.Add(11);
				}
				if (IsPCSleepFera(gameinfo3))
				{
					list2.Add(12);
				}
				if (IsPCSleepInsert(gameinfo3))
				{
					list2.Add(13);
				}
				if (_isMultipleEvent && IsPCSleepWFera(gameinfo3))
				{
					list2.Add(14);
				}
				if (IsComeToTheRoom(gameinfo3))
				{
					list2.Add(15);
				}
				if (IsHappeningCategory(gameinfo3, _isNormalJudge: true))
				{
					list2.Add(17);
					list2.Add(18);
					list2.Add(19);
				}
				if (list2.Any())
				{
					charaInfo3.eventID = list2.Shuffle().First();
					if (charaInfo3.eventID == 14)
					{
						charaInfo3.main = 0;
					}
				}
			}
		}
		for (int l = 0; l < _charaInfos.Count; l++)
		{
			CharaInfo charaInfo4 = _charaInfos[l];
			ChaFileGameInfo2 gameinfo4 = charaInfo4.chaFile.gameinfo2;
			if (charaInfo4.eventID != -1)
			{
				continue;
			}
			if (gameinfo4.nowDrawState == ChaFileDefine.State.Broken)
			{
				charaInfo4.eventID = 22;
				continue;
			}
			if (gameinfo4.nowDrawState == ChaFileDefine.State.Dependence)
			{
				charaInfo4.eventID = 1;
				continue;
			}
			List<int> list3 = new List<int> { 0 };
			if (_isMultipleEvent)
			{
				list3.Add(2);
			}
			charaInfo4.eventID = list3.Shuffle().First();
			if (charaInfo4.eventID == 2)
			{
				charaInfo4.main = 0;
			}
		}
		for (int m = 0; m < _charaInfos.Count; m++)
		{
			CharaInfo charaInfo5 = _charaInfos[m];
			_ = charaInfo5.chaFile.gameinfo2;
			if (!Singleton<Game>.Instance.infoEventContentDic.ContainsKey(charaInfo5.eventID))
			{
				charaInfo5.eventID = 0;
			}
		}
	}

	private static void SelectEvent(CharaInfo _charaInfo)
	{
		bool flag = Singleton<Game>.Instance.saveData.TutorialNo != -1;
		ChaFileGameInfo2 gameinfo = _charaInfo.chaFile.gameinfo2;
		if (gameinfo.hCount == 0)
		{
			_charaInfo.eventID = 24;
		}
		else if (gameinfo.escapeFlag == 1 && !flag && !Manager.Config.HData.EscapeStop)
		{
			_charaInfo.eventID = 16;
		}
		if (!flag)
		{
			if (_charaInfo.eventID != -1)
			{
				EnableEvent();
				return;
			}
			List<int> list = new List<int>();
			if (IsPeepingBathCategory(gameinfo))
			{
				list.Add(3);
				list.Add(4);
			}
			if (IsPeepingToiletCategory(gameinfo))
			{
				list.Add(5);
				list.Add(6);
			}
			if (IsNPCSleep(gameinfo))
			{
				list.Add(7);
			}
			if (IsDependenceJealousyH(gameinfo, _charaInfo.chaFile.charaFileName))
			{
				list.Add(21);
			}
			if (IsHappeningCategory(gameinfo))
			{
				list.Add(17);
				list.Add(18);
				list.Add(19);
			}
			if (list.Any())
			{
				_charaInfo.eventID = list.Shuffle().First();
			}
			if (_charaInfo.eventID != -1)
			{
				EnableEvent();
				return;
			}
			list.Clear();
			if (IsFemaleIsAttackBathCategory(gameinfo))
			{
				list.Add(8);
				list.Add(9);
			}
			if (IsFemaleIsAttackToiletCategory(gameinfo))
			{
				list.Add(10);
				list.Add(11);
			}
			if (IsPCSleepFera(gameinfo))
			{
				list.Add(12);
			}
			if (IsPCSleepInsert(gameinfo))
			{
				list.Add(13);
			}
			if (IsComeToTheRoom(gameinfo))
			{
				list.Add(15);
			}
			if (IsHappeningCategory(gameinfo, _isNormalJudge: true))
			{
				list.Add(17);
				list.Add(18);
				list.Add(19);
			}
			if (list.Any())
			{
				_charaInfo.eventID = list.Shuffle().First();
			}
		}
		if (_charaInfo.eventID != -1)
		{
			EnableEvent();
			return;
		}
		if (gameinfo.nowDrawState == ChaFileDefine.State.Broken)
		{
			_charaInfo.eventID = 22;
		}
		else if (gameinfo.nowDrawState == ChaFileDefine.State.Dependence)
		{
			_charaInfo.eventID = 1;
		}
		else
		{
			_charaInfo.eventID = 0;
		}
		EnableEvent();
		void EnableEvent()
		{
			if (!Singleton<Game>.Instance.infoEventContentDic.ContainsKey(_charaInfo.eventID))
			{
				_charaInfo.eventID = 0;
			}
		}
	}

	private static void SelectMapID(CharaInfo _charaInfo, List<int> _useMap, HashSet<int> _used)
	{
		int[] goToFemaleMaps = Singleton<Game>.Instance.infoEventContentDic[_charaInfo.eventID].goToFemaleMaps;
		goToFemaleMaps = ExcludeAchievementMap(goToFemaleMaps);
		List<int> list = new List<int>();
		if (!((IReadOnlyCollection<int>)(object)goToFemaleMaps).IsNullOrEmpty())
		{
			list = goToFemaleMaps.Except(_useMap).Shuffle().ToList();
		}
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i];
			if (!_used.Contains(num))
			{
				_used.Add(num);
				_charaInfo.mapID = num;
				flag = true;
				break;
			}
		}
		if (!flag && list.Any())
		{
			_charaInfo.mapID = list.Shuffle().FirstOrDefault();
		}
	}

	private static void SelectMapIDMultiple(List<CharaInfo> _charaInfos, List<int> _useMap, HashSet<int> _used)
	{
		for (int i = 0; i < _charaInfos.Count; i += 2)
		{
			int[] goToFemaleMaps = Singleton<Game>.Instance.infoEventContentDic[_charaInfos[i].eventID].goToFemaleMaps;
			goToFemaleMaps = ExcludeAchievementMap(goToFemaleMaps);
			List<int> list = new List<int>();
			if (!((IReadOnlyCollection<int>)(object)goToFemaleMaps).IsNullOrEmpty())
			{
				list = goToFemaleMaps.Except(_useMap).Shuffle().ToList();
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				int num = list[j];
				if (!_used.Contains(num))
				{
					_used.Add(num);
					_charaInfos[i].mapID = num;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_charaInfos[i].mapID = list.Shuffle().FirstOrDefault();
			}
			_charaInfos[i + 1].mapID = _charaInfos[i].mapID;
		}
	}

	public static Dictionary<int, MapCharaSelectInfo> GetEventNo(int _selectGropu)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		Dictionary<int, MapCharaSelectInfo> dictionary = new Dictionary<int, MapCharaSelectInfo>();
		for (int i = 0; i < saveData.roomList[_selectGropu].Count; i++)
		{
			string text = saveData.roomList[_selectGropu][i];
			ChaFileControl chaFileControl = new ChaFileControl();
			chaFileControl.LoadCharaFile(text, 1);
			MapCharaSelectInfo mapCharaSelectInfo = new MapCharaSelectInfo
			{
				eventID = GetEventNo(chaFileControl.gameinfo2, _isDesireCalc: true)
			};
			mapCharaSelectInfo.lstChara.Add((text, -1));
			dictionary[i] = mapCharaSelectInfo;
		}
		return dictionary;
	}

	public static int GetEventNo(ChaFileGameInfo2 _gameInfo, bool _isDesireCalc)
	{
		int num = -1;
		bool flag = Singleton<Game>.Instance.saveData.TutorialNo != -1;
		if (_gameInfo == null)
		{
			return num;
		}
		if (_gameInfo.hCount == 0 || flag)
		{
			num = 24;
		}
		else if (_gameInfo.escapeFlag == 1 && !Manager.Config.HData.EscapeStop)
		{
			num = 16;
		}
		if (num == -1 && _isDesireCalc)
		{
			List<int> list = new List<int>();
			if (IsPeepingBathCategory(_gameInfo, _isGoToSee: false))
			{
				list.Add(28);
				list.Add(29);
			}
			if (IsPeepingToiletCategory(_gameInfo, _isGoToSee: false))
			{
				list.Add(30);
				list.Add(31);
			}
			if (IsNPCSleep(_gameInfo))
			{
				list.Add(32);
			}
			if (list.Any())
			{
				num = list.Shuffle().First();
			}
		}
		if (!Singleton<Game>.Instance.infoEventContentDic.ContainsKey(num))
		{
			num = -1;
		}
		return num;
	}

	public static int GetGeneralEventNo(ChaFileGameInfo2 _gameInfo, int _mapID)
	{
		int num = -1;
		if (_gameInfo == null)
		{
			return num;
		}
		if (_gameInfo.nowDrawState == ChaFileDefine.State.Broken && !_gameInfo.genericBrokenVoice)
		{
			num = 22;
		}
		else if (_gameInfo.nowDrawState == ChaFileDefine.State.Dependence && !_gameInfo.genericDependencepVoice)
		{
			num = 23;
		}
		if (num == -1 && BaseMap.infoTable.TryGetValue(_mapID, out var value) && !_gameInfo.map.Contains(_mapID) && value.isADV)
		{
			num = 25;
		}
		if (num == -1)
		{
			int num2 = 26;
			if (AnalAndPain(_gameInfo) != 0)
			{
				num = num2;
			}
			int num3 = ((_gameInfo.resistH >= 100) ? 1 : 0);
			switch (_gameInfo.nowState)
			{
			case ChaFileDefine.State.Blank:
				if (!_gameInfo.genericVoice[num3][0])
				{
					num = num2;
				}
				break;
			case ChaFileDefine.State.Favor:
				if (_gameInfo.Favor >= 80)
				{
					if (!_gameInfo.genericVoice[num3][3])
					{
						num = num2;
					}
				}
				else if (_gameInfo.Favor >= 50)
				{
					if (!_gameInfo.genericVoice[num3][2])
					{
						num = num2;
					}
				}
				else if (!_gameInfo.genericVoice[num3][1])
				{
					num = num2;
				}
				break;
			case ChaFileDefine.State.Enjoyment:
				if (_gameInfo.Enjoyment >= 80)
				{
					if (!_gameInfo.genericVoice[num3][6])
					{
						num = num2;
					}
				}
				else if (_gameInfo.Enjoyment >= 50)
				{
					if (!_gameInfo.genericVoice[num3][5])
					{
						num = num2;
					}
				}
				else if (!_gameInfo.genericVoice[num3][4])
				{
					num = num2;
				}
				break;
			case ChaFileDefine.State.Aversion:
				if (_gameInfo.Aversion >= 80)
				{
					if (!_gameInfo.genericVoice[num3][12])
					{
						num = num2;
					}
				}
				else if (_gameInfo.Aversion >= 50)
				{
					if (!_gameInfo.genericVoice[num3][11])
					{
						num = num2;
					}
				}
				else if (!_gameInfo.genericVoice[num3][10])
				{
					num = num2;
				}
				break;
			case ChaFileDefine.State.Slavery:
				if (_gameInfo.Slavery >= 80)
				{
					if (!_gameInfo.genericVoice[num3][9])
					{
						num = num2;
					}
				}
				else if (_gameInfo.Slavery >= 50)
				{
					if (!_gameInfo.genericVoice[num3][8])
					{
						num = num2;
					}
				}
				else if (!_gameInfo.genericVoice[num3][7])
				{
					num = num2;
				}
				break;
			}
		}
		return num;
	}

	public static MapCharaSelectInfo GetSleepEvent(int _selectGropu, out List<CharaInfo> _charas)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		new MapCharaSelectInfo();
		new List<int>();
		_charas = new List<CharaInfo>();
		for (int i = 0; i < saveData.roomList[_selectGropu].Count; i++)
		{
			string filename = saveData.roomList[_selectGropu][i];
			CharaInfo charaInfo = new CharaInfo
			{
				id = i
			};
			charaInfo.chaFile.LoadCharaFile(filename, 1);
			_charas.Add(charaInfo);
		}
		SelectSleepEvent(_charas);
		List<CharaInfo> list = _charas.Where((CharaInfo ci) => ci.eventID == 14).Shuffle().ToList();
		List<CharaInfo> list2 = _charas.Where((CharaInfo ci) => ci.main == -1).Shuffle().ToList();
		for (int num = 0; num < list.Count; num++)
		{
			CharaInfo charaInfo2 = list[num];
			List<CharaInfo> list3 = new List<CharaInfo>();
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				CharaInfo charaInfo3 = list2[num2];
				if (charaInfo3.main == -1 && charaInfo2.eventID == 14 && IsPCSleepWFera(charaInfo3.chaFile.gameinfo2))
				{
					list3.Add(charaInfo3);
				}
			}
			if (list3.Any())
			{
				CharaInfo charaInfo4 = list3.Shuffle().FirstOrDefault();
				charaInfo4.eventID = charaInfo2.eventID;
				charaInfo4.main = 1;
			}
			else
			{
				charaInfo2.eventID = -1;
				charaInfo2.main = -1;
			}
		}
		_charas = _charas.Where((CharaInfo lc) => lc.eventID != -1).Shuffle().ToList();
		_charas.ForEach(delegate(CharaInfo c)
		{
			c.mapID = 1;
		});
		_charas = _charas.Shuffle().ToList();
		return GetSleepEventMainCharacter(_selectGropu, _charas);
	}

	public static MapCharaSelectInfo GetSleepEventMainCharacter(int _selectGropu, List<CharaInfo> _charas)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		MapCharaSelectInfo mapCharaSelectInfo = new MapCharaSelectInfo();
		List<int> desireEventIDs = new List<int>(Game.DirtyEventIDs).Concat(Game.SleepEventIDs).Concat(Game.ToiletEventIDs).ToList();
		CharaInfo charaInfo = _charas.FirstOrDefault((CharaInfo c) => !desireEventIDs.Contains(c.eventID) && c.main != 1);
		if (charaInfo == null)
		{
			return null;
		}
		mapCharaSelectInfo.eventID = charaInfo.eventID;
		mapCharaSelectInfo.mapID = 1;
		mapCharaSelectInfo.lstChara.Add((saveData.roomList[_selectGropu][charaInfo.id], charaInfo.chaFile.parameter2.personality));
		if (charaInfo.eventID == 14)
		{
			CharaInfo charaInfo2 = _charas.FirstOrDefault((CharaInfo c) => c.eventID == 14 && c.main == 1);
			if (charaInfo2 == null)
			{
				return null;
			}
			mapCharaSelectInfo.lstChara.Add((saveData.roomList[_selectGropu][charaInfo2.id], charaInfo2.chaFile.parameter2.personality));
		}
		return mapCharaSelectInfo;
	}

	private static void SelectSleepEvent(List<CharaInfo> _charaInfos)
	{
		_ = Singleton<Game>.Instance.saveData.TutorialNo;
		for (int i = 0; i < _charaInfos.Count; i++)
		{
			CharaInfo charaInfo = _charaInfos[i];
			ChaFileGameInfo2 gameinfo = charaInfo.chaFile.gameinfo2;
			List<int> list = new List<int>();
			if (IsPeepingBathCategory(gameinfo))
			{
				list.Add(3);
				list.Add(4);
			}
			if (IsPeepingToiletCategory(gameinfo))
			{
				list.Add(5);
				list.Add(6);
			}
			if (IsNPCSleep(gameinfo))
			{
				list.Add(7);
			}
			if (IsPCSleepFera(gameinfo))
			{
				list.Add(12);
			}
			if (IsPCSleepInsert(gameinfo))
			{
				list.Add(13);
			}
			if (IsPCSleepWFera(gameinfo))
			{
				list.Add(14);
			}
			if (IsComeToTheRoom(gameinfo))
			{
				list.Add(15);
			}
			if (list.Any())
			{
				charaInfo.eventID = list.Shuffle().First();
				if (charaInfo.eventID == 14)
				{
					charaInfo.main = 0;
				}
			}
		}
		for (int j = 0; j < _charaInfos.Count; j++)
		{
			CharaInfo charaInfo2 = _charaInfos[j];
			_ = charaInfo2.chaFile.gameinfo2;
			if (!Singleton<Game>.Instance.infoEventContentDic.ContainsKey(charaInfo2.eventID))
			{
				charaInfo2.eventID = -1;
				charaInfo2.main = -1;
			}
		}
	}

	private static bool IsPeepingBathCategory(ChaFileGameInfo2 _game, bool _isGoToSee = true)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		if (_game.nowDrawState == ChaFileDefine.State.Broken)
		{
			return false;
		}
		if (!_isGoToSee && _game.nowDrawState == ChaFileDefine.State.Dependence)
		{
			return false;
		}
		return _game.Dirty >= 100;
	}

	private static bool IsPeepingToiletCategory(ChaFileGameInfo2 _game, bool _isGoToSee = true)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		if (_game.nowDrawState == ChaFileDefine.State.Broken)
		{
			return false;
		}
		if (!_isGoToSee && _game.nowDrawState == ChaFileDefine.State.Dependence)
		{
			return false;
		}
		return _game.Toilet >= 100;
	}

	private static bool IsNPCSleep(ChaFileGameInfo2 _game)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		if (_game.nowDrawState == ChaFileDefine.State.Broken || _game.nowDrawState == ChaFileDefine.State.Dependence)
		{
			return false;
		}
		if (_game.Tiredness < 80)
		{
			return false;
		}
		return Random.Range(0, 100) < ((_game.Tiredness == 100) ? 100 : 30);
	}

	private static bool IsFemaleIsAttackBathCategory(ChaFileGameInfo2 _game)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		switch (_game.nowDrawState)
		{
		case ChaFileDefine.State.Slavery:
			if (!IsConditions() || _game.Slavery < 80)
			{
				return false;
			}
			return Random.Range(0, 100) < 30;
		default:
			return false;
		}
		bool IsConditions()
		{
			if (_game.resistH < 100)
			{
				return false;
			}
			if (_game.Libido < 50)
			{
				return false;
			}
			return true;
		}
	}

	private static bool IsFemaleIsAttackToiletCategory(ChaFileGameInfo2 _game)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		switch (_game.nowDrawState)
		{
		case ChaFileDefine.State.Enjoyment:
			if (!IsConditions() || _game.Enjoyment < 80)
			{
				return false;
			}
			return Random.Range(0, 100) < 30;
		default:
			return false;
		}
		bool IsConditions()
		{
			if (_game.resistH < 100)
			{
				return false;
			}
			if (_game.Libido < 50)
			{
				return false;
			}
			return true;
		}
	}

	private static bool IsPCSleepFera(ChaFileGameInfo2 _game)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		switch (_game.nowDrawState)
		{
		case ChaFileDefine.State.Slavery:
			if (!IsConditions() || _game.Slavery < 80)
			{
				return false;
			}
			return Random.Range(0, 100) < 30;
		default:
			return false;
		}
		bool IsConditions()
		{
			if (_game.resistH < 100)
			{
				return false;
			}
			if (_game.Libido < 50)
			{
				return false;
			}
			return true;
		}
	}

	private static bool IsPCSleepInsert(ChaFileGameInfo2 _game)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		switch (_game.nowDrawState)
		{
		case ChaFileDefine.State.Enjoyment:
			if (!IsConditions() || _game.Enjoyment < 80)
			{
				return false;
			}
			return Random.Range(0, 100) < 30;
		default:
			return false;
		}
		bool IsConditions()
		{
			if (_game.resistH < 100)
			{
				return false;
			}
			if (_game.Libido < 50)
			{
				return false;
			}
			return true;
		}
	}

	private static bool IsPCSleepWFera(ChaFileGameInfo2 _game)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		if (_game.resistH < 100)
		{
			return false;
		}
		if (_game.Libido < 50)
		{
			return false;
		}
		if (_game.nowDrawState != ChaFileDefine.State.Favor || _game.Favor < 80)
		{
			return false;
		}
		return Random.Range(0, 100) < 30;
	}

	private static bool IsComeToTheRoom(ChaFileGameInfo2 _game)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		if (_game.nowDrawState != ChaFileDefine.State.Favor)
		{
			return false;
		}
		return Random.Range(0, 100) < 20;
	}

	private static bool IsDependenceJealousyH(ChaFileGameInfo2 _game, string _fileName)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		if (_game.nowDrawState != ChaFileDefine.State.Dependence)
		{
			return false;
		}
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (saveData.BeforeFemaleName.IsNullOrEmpty())
		{
			return false;
		}
		if (Path.GetFileNameWithoutExtension(saveData.BeforeFemaleName) == _fileName)
		{
			return false;
		}
		return Random.Range(0, 100) < 30;
	}

	private static bool IsHappeningCategory(ChaFileGameInfo2 _game, bool _isNormalJudge = false)
	{
		if (_game == null)
		{
			return false;
		}
		if (_game.hCount == 0)
		{
			return false;
		}
		if (_game.Libido < 50)
		{
			return false;
		}
		if (_game.nowDrawState == ChaFileDefine.State.Blank || _game.nowDrawState == ChaFileDefine.State.Broken || _game.nowDrawState == ChaFileDefine.State.Dependence)
		{
			return false;
		}
		if (_game.Toilet >= 100)
		{
			return true;
		}
		if (!_isNormalJudge)
		{
			return false;
		}
		return Random.Range(0, 100) < 10;
	}

	public static void EscapeCharaEventSet(int _selectGropu)
	{
		foreach (string item in Singleton<Game>.Instance.saveData.roomList[_selectGropu])
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			if (chaFileControl.LoadCharaFile(item, 1))
			{
				ChaFileGameInfo2 gameinfo = chaFileControl.gameinfo2;
				byte b = 0;
				if (gameinfo.nowState == ChaFileDefine.State.Aversion && gameinfo.Aversion >= 80)
				{
					b = (byte)((Random.Range(0, 100) < ((gameinfo.Aversion >= 100) ? 30 : 10)) ? 1 : 0);
				}
				if (gameinfo.escapeFlag != b)
				{
					gameinfo.escapeFlag = b;
					chaFileControl.SaveCharaFile(item, 1);
				}
			}
		}
	}

	public static int[] ExcludeAchievementMap(int[] _maps)
	{
		int[] array = (SaveData.IsAchievementExchangeRelease(11) ? _maps : _maps?.Except(Game.AchievementMapIDList0).ToArray());
		if (!SaveData.IsAchievementExchangeRelease(12))
		{
			return array?.Except(Game.AchievementMapIDList1).ToArray();
		}
		return array;
	}

	public static int[] ExcludeFursRoomAchievementMap(int[] _maps)
	{
		if (!SaveData.IsAchievementExchangeRelease(13))
		{
			return _maps?.Except(new List<int> { 2 }).ToArray();
		}
		return _maps;
	}

	public static List<MapInfo.Param> ExcludeAppendMap(List<MapInfo.Param> _maps)
	{
		AppendSaveData apSave = Singleton<Game>.Instance.appendSaveData;
		List<int> expect = (from t in Game.AppendMapIDTable
			where !apSave.appendEvents.Contains(t.Key)
			select t.Value).ToList();
		if (apSave.AppendTutorialNo != -1)
		{
			expect.Add(17);
		}
		return _maps.Where((MapInfo.Param p) => !expect.Contains(p.No)).ToList();
	}

	public static bool IsAchievementMap0(int _map)
	{
		if (!SaveData.IsAchievementExchangeRelease(11) || !Game.AchievementMapIDList0.Contains(_map))
		{
			return !Game.AchievementMapIDList0.Contains(_map);
		}
		return true;
	}

	public static bool IsAchievementMap1(int _map)
	{
		if (!SaveData.IsAchievementExchangeRelease(12) || !Game.AchievementMapIDList1.Contains(_map))
		{
			return !Game.AchievementMapIDList1.Contains(_map);
		}
		return true;
	}

	public static int GetStateIconNum(int _nowState, int _personality)
	{
		int num = 0;
		if (_nowState < 6)
		{
			return _nowState;
		}
		return Game.infoPersonalParameterTable[_personality].dependence switch
		{
			2 => 7, 
			4 => 8, 
			_ => 6, 
		};
	}

	public static int AnalAndPain(ChaFileGameInfo2 _info)
	{
		if (_info == null)
		{
			return 0;
		}
		bool flag = !_info.genericAnalVoice && _info.resistAnal >= 100;
		bool flag2 = !_info.genericPainVoice && _info.resistPain >= 100;
		if (flag && !flag2)
		{
			return 1;
		}
		if (!flag && flag2)
		{
			return 2;
		}
		if (flag && flag2)
		{
			return Random.Range(1, 3);
		}
		return 0;
	}

	public static bool IsPeepingFound(ChaFileGameInfo2 _param, int _personality, bool _isDependece = false)
	{
		if (_param == null)
		{
			return false;
		}
		if (_isDependece && _param.nowDrawState == ChaFileDefine.State.Dependence)
		{
			return true;
		}
		if (!Game.infoPersonalParameterTable.TryGetValue(_personality, out var value))
		{
			return false;
		}
		return Random.Range(0, 100) < value.warnings[(int)_param.nowDrawState];
	}

	private static bool CalcNowDependence(ChaFileGameInfo2 _param, int _personality)
	{
		if (_param == null)
		{
			return false;
		}
		if (_param.resistH < 100)
		{
			return false;
		}
		if (_param.nowDrawState != ChaFileDefine.State.Blank && _param.nowDrawState != ChaFileDefine.State.Aversion)
		{
			return _param.nowDrawState != ChaFileDefine.State.Broken;
		}
		return false;
	}

	private static void EndOfDayDependenceCalc(ChaFileGameInfo2 _param, int _personality)
	{
		if (_param != null && CalcNowDependence(_param, _personality) && !_param.lockDependence)
		{
			_param.Dependence += -5;
		}
	}

	private static void CalcParameter(GameParameterInfo.Param _value, ChaFileGameInfo2 _param, int _personality)
	{
		if (_param != null && _value != null && _param.hCount != 0)
		{
			if (_param.nowDrawState != ChaFileDefine.State.Broken && _param.nowDrawState != ChaFileDefine.State.Dependence && !_param.lockNowState)
			{
				_param.Favor += Random.Range(_value.favor.min, _value.favor.max + 1);
				_param.Enjoyment += Random.Range(_value.enjoyment.min, _value.enjoyment.max + 1);
				_param.Slavery += Random.Range(_value.slavery.min, _value.slavery.max + 1);
				_param.Aversion += Random.Range(_value.aversion.min, _value.aversion.max + 1);
			}
			_param.Dirty += Random.Range(_value.dirty.min, _value.dirty.max + 1);
			_param.Tiredness += Random.Range(_value.tiredness.min, _value.tiredness.max + 1);
			_param.Toilet += Random.Range(_value.toilet.min, _value.toilet.max + 1);
			_param.Libido += Random.Range(_value.libido.min, _value.libido.max + 1);
			if (!_param.lockBroken)
			{
				_param.Broken += Random.Range(_value.broken.min, _value.broken.max + 1);
			}
			if (CalcNowDependence(_param, _personality) && !_param.lockDependence)
			{
				_param.Dependence += Random.Range(_value.dependence.min, _value.dependence.max + 1);
			}
			if (!_param.isChangeParameter)
			{
				_param.isChangeParameter = true;
			}
		}
	}

	public static void CalcParameter(int _num, ChaFileGameInfo2 _param, int _personality)
	{
		if (_param != null && Game.infoGameParameterTable.TryGetValue(_num, out var value))
		{
			CalcParameter(value, _param, _personality);
		}
	}

	public static void CalcParameterH(List<GameParameterInfo.Param> _values, ChaFileGameInfo2 _param, int _personality)
	{
		if (_param == null || _values == null || _values.Count == 0)
		{
			return;
		}
		foreach (GameParameterInfo.Param _value in _values)
		{
			CalcParameter(_value, _param, _personality);
		}
	}

	public static void CalcParameterH(GameParameterInfo.Param _value, ChaFileGameInfo2 _param, int _personality)
	{
		if (_param != null && _value != null)
		{
			CalcParameter(_value, _param, _personality);
		}
	}

	public static void EndOfDayParameter(int _num)
	{
		Game instance = Singleton<Game>.Instance;
		SaveData saveData = instance.saveData;
		ChaFileControl chaFileControl = ((instance.heroineList.Any() && instance.heroineList[0] != null) ? instance.heroineList[0].chaFile : null);
		string text = ((chaFileControl != null) ? Path.GetFileNameWithoutExtension(chaFileControl.charaFileName) : string.Empty);
		foreach (string item in saveData.roomList[saveData.selectGroup])
		{
			ChaFileControl chaFileControl2 = new ChaFileControl();
			if (!chaFileControl2.LoadCharaFile(item, 1))
			{
				continue;
			}
			ChaFileGameInfo2 gameinfo = chaFileControl2.gameinfo2;
			int personality = chaFileControl2.parameter2.personality;
			if (gameinfo.usedItem != 0)
			{
				if (gameinfo.usedItem == 1)
				{
					gameinfo.Toilet += 100;
				}
				else if (gameinfo.usedItem == 2)
				{
					gameinfo.Dirty += 100;
				}
				else if (gameinfo.usedItem == 3)
				{
					gameinfo.Tiredness += 100;
				}
				else if (gameinfo.usedItem == 4)
				{
					gameinfo.Libido += 100;
				}
				gameinfo.usedItem = 0;
			}
			bool flag = true;
			if (_num == 1 && instance.heroineList.Any() && !text.IsNullOrEmpty() && item == text)
			{
				flag = false;
			}
			if (flag)
			{
				CalcParameter(_num, gameinfo, personality);
			}
			CalcState(gameinfo, personality);
			chaFileControl2.SaveCharaFile(item, 1);
		}
		ChaFileControl chaFileControl3 = ((instance.heroineList.Count > 1 && instance.heroineList[1] != null) ? instance.heroineList[1].chaFile : null);
		string text2 = ((chaFileControl3 != null) ? Path.GetFileNameWithoutExtension(chaFileControl3.charaFileName) : string.Empty);
		foreach (KeyValuePair<string, int> tableDesireChara in instance.tableDesireCharas)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tableDesireChara.Key);
			if (fileNameWithoutExtension == text || fileNameWithoutExtension == text2)
			{
				continue;
			}
			ChaFileControl chaFileControl4 = new ChaFileControl();
			if (chaFileControl4.LoadCharaFile(tableDesireChara.Key, 1))
			{
				ChaFileGameInfo2 gameinfo2 = chaFileControl4.gameinfo2;
				int personality2 = chaFileControl4.parameter2.personality;
				if (Game.DirtyEventIDs.Contains(tableDesireChara.Value))
				{
					CalcParameter(176, gameinfo2, personality2);
				}
				else if (Game.SleepEventIDs.Contains(tableDesireChara.Value))
				{
					CalcParameter(178, gameinfo2, personality2);
				}
				else if (Game.ToiletEventIDs.Contains(tableDesireChara.Value))
				{
					CalcParameter(177, gameinfo2, personality2);
				}
				else if (Game.LibidoEventIDs.Contains(tableDesireChara.Value))
				{
					CalcParameter(179, gameinfo2, personality2);
				}
				CalcState(gameinfo2, personality2);
				chaFileControl4.SaveCharaFile(tableDesireChara.Key, 1);
			}
		}
		saveData.Save();
		if (GameSystem.isAdd50)
		{
			instance.appendSaveData.Save();
		}
	}

	public static void CalcState(ChaFileGameInfo2 _param, int _personality)
	{
		if (_param == null)
		{
			return;
		}
		if (_param.nowDrawState == ChaFileDefine.State.Broken)
		{
			if (_param.Broken > 0)
			{
				return;
			}
		}
		else if (_param.nowDrawState == ChaFileDefine.State.Dependence && _param.Dependence > 0)
		{
			return;
		}
		List<(ChaFileDefine.State, int)> list = new List<(ChaFileDefine.State, int)>
		{
			(ChaFileDefine.State.Favor, _param.Favor),
			(ChaFileDefine.State.Enjoyment, _param.Enjoyment),
			(ChaFileDefine.State.Slavery, _param.Slavery),
			(ChaFileDefine.State.Aversion, _param.Aversion)
		};
		if (list.Any<(ChaFileDefine.State, int)>(((ChaFileDefine.State id, int state) l) => l.state >= 20))
		{
			CalcState(list, ref _param.nowState);
		}
		else
		{
			_param.nowState = ChaFileDefine.State.Blank;
		}
		if (list.Any<(ChaFileDefine.State, int)>(((ChaFileDefine.State id, int state) l) => l.state >= 50))
		{
			CalcState(list, ref _param.nowDrawState);
		}
		else
		{
			_param.nowDrawState = ChaFileDefine.State.Blank;
		}
		if (_param.Broken >= 100)
		{
			_param.nowState = ChaFileDefine.State.Broken;
			_param.nowDrawState = ChaFileDefine.State.Broken;
		}
		else if (_param.Dependence >= 100)
		{
			_param.nowState = ChaFileDefine.State.Dependence;
			_param.nowDrawState = ChaFileDefine.State.Dependence;
		}
		if (_param.nowDrawState == ChaFileDefine.State.Favor && _param.Favor >= 100)
		{
			SaveData.SetAchievementAchieve(12);
		}
		if (_param.nowDrawState == ChaFileDefine.State.Enjoyment && _param.Enjoyment >= 100)
		{
			SaveData.SetAchievementAchieve(13);
		}
		if (_param.nowDrawState == ChaFileDefine.State.Slavery && _param.Slavery >= 100)
		{
			SaveData.SetAchievementAchieve(14);
		}
		if (_param.nowDrawState == ChaFileDefine.State.Aversion && _param.Aversion >= 100)
		{
			SaveData.SetAchievementAchieve(15);
		}
		if (_param.nowDrawState == ChaFileDefine.State.Broken)
		{
			SaveData.SetAchievementAchieve(16);
		}
		if (_param.nowDrawState == ChaFileDefine.State.Dependence)
		{
			SaveData.SetAchievementAchieve(17);
		}
		void CalcState(List<(ChaFileDefine.State id, int state)> _list, ref ChaFileDefine.State _state)
		{
			List<(ChaFileDefine.State, int)> source = _list.MaxElementsBy(((ChaFileDefine.State id, int state) l) => l.state).ToList();
			foreach (int s in Game.infoPersonalParameterTable[_personality].statusPrioritys)
			{
				if (source.Any<(ChaFileDefine.State, int)>(((ChaFileDefine.State id, int state) m) => m.id == (ChaFileDefine.State)s))
				{
					_state = (ChaFileDefine.State)s;
					break;
				}
			}
		}
	}
}
