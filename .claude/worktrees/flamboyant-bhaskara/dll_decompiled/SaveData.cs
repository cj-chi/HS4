using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ADV;
using Illusion;
using Manager;
using MessagePack;
using UnityEngine;

[MessagePackObject(false)]
public sealed class SaveData : ICommandData
{
	public enum AchievementState
	{
		AS_NotAchieved,
		AS_Achieve
	}

	[IgnoreMember]
	private const string SaveDataVersion = "0.0.0";

	[IgnoreMember]
	private const string path = "save/";

	[IgnoreMember]
	private const string fileName = "save.dat";

	[Key(1)]
	public Dictionary<int, AchievementState> achievement = new Dictionary<int, AchievementState>();

	[Key(2)]
	public Dictionary<int, AchievementState> achievementExchange = new Dictionary<int, AchievementState>();

	[Key(3)]
	public HashSet<int> achievementAchieve = new HashSet<int>();

	[Key(4)]
	public List<string>[] roomList = new List<string>[5];

	[Key(5)]
	public Dictionary<string, ClothPngInfo>[] dicCloths = new Dictionary<string, ClothPngInfo>[5];

	[Key(6)]
	public int selectGroup;

	[Key(7)]
	public int hCount;

	[Key(8)]
	public HashSet<int> firstHList = new HashSet<int>();

	[Key(9)]
	public Dictionary<int, byte> customIDInfo = new Dictionary<int, byte>();

	[Key(10)]
	public PlayerCharaSaveInfo playerChara = new PlayerCharaSaveInfo();

	[Key(11)]
	public PlayerCharaSaveInfo secondPlayerChara = new PlayerCharaSaveInfo();

	[Key(14)]
	public int hEjaculation;

	[Key(15)]
	public int hOrgasm;

	[Key(16)]
	public List<PlayerCoordinateInfo> playerCloths = new List<PlayerCoordinateInfo>
	{
		new PlayerCoordinateInfo(),
		new PlayerCoordinateInfo()
	};

	[IgnoreMember]
	public bool isNotificationEscape;

	[IgnoreMember]
	public string escapeCharaName = "";

	[IgnoreMember]
	public bool isUpdateInternalParameter;

	[IgnoreMember]
	public bool isRoomConcierge;

	public static string Path => UserData.Path + "save/";

	[Key(0)]
	public Version Version { get; set; } = new Version(0, 0, 0);

	[Key(12)]
	public int TutorialNo { get; set; }

	[Key(13)]
	public string BeforeFemaleName { get; set; } = string.Empty;

	public void Initialize()
	{
		Version = new Version("0.0.0");
		InitAchievement();
		InitAchievementExchange();
		achievementAchieve.Clear();
		for (int i = 0; i < roomList.Length; i++)
		{
			roomList[i] = new List<string>();
		}
		for (int j = 0; j < dicCloths.Length; j++)
		{
			dicCloths[j] = new Dictionary<string, ClothPngInfo>();
		}
		selectGroup = 0;
		hCount = 0;
		hEjaculation = 0;
		hOrgasm = 0;
		firstHList.Clear();
		customIDInfo.Clear();
		playerChara = new PlayerCharaSaveInfo
		{
			FileName = "HS2_ill_M_000",
			Sex = 0
		};
		secondPlayerChara = new PlayerCharaSaveInfo();
		isNotificationEscape = false;
		escapeCharaName = "";
		isUpdateInternalParameter = false;
		isRoomConcierge = false;
		TutorialNo = 0;
		BeforeFemaleName = string.Empty;
		playerCloths = new List<PlayerCoordinateInfo>
		{
			new PlayerCoordinateInfo(),
			new PlayerCoordinateInfo()
		};
	}

	public void AddCustomID(string IDStr, byte flags = 1)
	{
		string[] array = IDStr.Split('/');
		for (int i = 0; i < array.Length; i++)
		{
			int key = int.Parse(array[i]);
			if (customIDInfo.TryGetValue(key, out var value))
			{
				if (value < flags)
				{
					customIDInfo[key] = flags;
				}
			}
			else
			{
				customIDInfo.Add(key, flags);
			}
		}
	}

	public void AddCustomID(int id, byte flags = 1)
	{
		if (customIDInfo.TryGetValue(id, out var value))
		{
			if (value < flags)
			{
				customIDInfo[id] = flags;
			}
		}
		else
		{
			customIDInfo.Add(id, flags);
		}
	}

	public byte CheckCustomID(int id)
	{
		customIDInfo.TryGetValue(id, out var value);
		return value;
	}

	public void RoomListCharaExists()
	{
		List<string>[] array = roomList;
		foreach (List<string> list in array)
		{
			List<string> list2 = new List<string>(list);
			for (int j = 0; j < list2.Count; j++)
			{
				StringBuilder stringBuilder = new StringBuilder(UserData.Path + "chara/female/");
				stringBuilder.Append(list2[j]).Append(".png");
				if (!File.Exists(stringBuilder.ToString()))
				{
					list.Remove(list2[j]);
				}
			}
		}
		for (int k = 0; k < dicCloths.Length; k++)
		{
			foreach (KeyValuePair<string, ClothPngInfo> item in new Dictionary<string, ClothPngInfo>(dicCloths[k]))
			{
				StringBuilder stringBuilder2 = new StringBuilder(UserData.Path + "chara/female/");
				stringBuilder2.Append(item.Key).Append(".png");
				if (!File.Exists(stringBuilder2.ToString()))
				{
					dicCloths[k].Remove(item.Key);
				}
			}
		}
	}

	public static bool IsRoomListChara(string _file)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return false;
		}
		SaveData saveData = Singleton<Game>.Instance.saveData;
		string name = System.IO.Path.GetFileNameWithoutExtension(_file);
		if (!saveData.roomList.Where((List<string> l) => l.Contains(name)).Any() && !(saveData.playerChara.FileName == name))
		{
			return saveData.secondPlayerChara.FileName == name;
		}
		return true;
	}

	public static int FindInRoomListIndex(string _findName)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return -1;
		}
		SaveData saveData = Singleton<Game>.Instance.saveData;
		return saveData.roomList[saveData.selectGroup].FindIndex((string r) => r == _findName);
	}

	public void PlayerCoordinateExists()
	{
		if (playerCloths == null)
		{
			return;
		}
		foreach (PlayerCoordinateInfo playerCloth in playerCloths)
		{
			if (!IsPlayerCoordinateFileExists(playerCloth))
			{
				playerCloth.sex = 0;
				playerCloth.file = string.Empty;
			}
		}
	}

	private bool IsPlayerCoordinateFileExists(PlayerCoordinateInfo _player)
	{
		if (_player == null || _player.file.IsNullOrEmpty())
		{
			return false;
		}
		string text = ((_player.sex == 0) ? "coordinate/male/" : "coordinate/female/");
		StringBuilder stringBuilder = new StringBuilder(UserData.Path + text);
		stringBuilder.Append(_player.file).Append(".png");
		return File.Exists(stringBuilder.ToString());
	}

	public void PlayerExists()
	{
		if (!IsPlayerFileExists(playerChara))
		{
			playerChara.FileName = "HS2_ill_M_000";
			playerChara.Sex = 0;
			playerChara.Futanari = false;
		}
		if (!secondPlayerChara.FileName.IsNullOrEmpty() && !IsPlayerFileExists(secondPlayerChara))
		{
			secondPlayerChara.FileName = "";
			secondPlayerChara.Sex = 0;
			secondPlayerChara.Futanari = false;
			playerCloths[1].file = string.Empty;
			playerCloths[1].sex = 0;
		}
	}

	private bool IsPlayerFileExists(PlayerCharaSaveInfo _player)
	{
		return File.Exists(CreatePlayerPngPath(_player));
	}

	public static string CreatePlayerPngPath(PlayerCharaSaveInfo _player)
	{
		string text = ((_player.Sex == 0) ? "chara/male/" : "chara/female/");
		StringBuilder stringBuilder = new StringBuilder(UserData.Path + text);
		stringBuilder.Append(_player.FileName).Append(".png");
		return stringBuilder.ToString();
	}

	private void InitAchievement()
	{
		AchievementInfoData achievementInfoData = CommonLib.LoadAsset<AchievementInfoData>(AssetBundleNames.GamedataAchievement, "achievement");
		if (!(null == achievementInfoData))
		{
			achievement = achievementInfoData.param.ToDictionary((AchievementInfoData.Param p) => p.id, (AchievementInfoData.Param p) => AchievementState.AS_NotAchieved);
			AssetBundleManager.UnloadAssetBundle(AssetBundleNames.GamedataAchievement, isUnloadForceRefCount: true);
		}
	}

	public static void SetAchievementAchieve(int _idx)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return;
		}
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (saveData.achievement.ContainsKey(_idx) && saveData.achievement[_idx] != AchievementState.AS_Achieve)
		{
			saveData.achievement[_idx] = AchievementState.AS_Achieve;
			if (!saveData.achievementAchieve.Contains(_idx))
			{
				saveData.achievementAchieve.Add(_idx);
			}
		}
	}

	public static bool SetAchievementRelease(int _idx)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return false;
		}
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (!saveData.achievement.ContainsKey(_idx))
		{
			return false;
		}
		AchievementState achievementState = saveData.achievement[_idx];
		saveData.achievement[_idx] = AchievementState.AS_Achieve;
		return achievementState != Singleton<Game>.Instance.saveData.achievement[_idx];
	}

	public static bool IsAchievementRelease(int _idx)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return false;
		}
		if (!Singleton<Game>.Instance.saveData.achievement.TryGetValue(_idx, out var value))
		{
			return false;
		}
		return value == AchievementState.AS_Achieve;
	}

	private void InitAchievementExchange()
	{
		AchievementInfoData achievementInfoData = CommonLib.LoadAsset<AchievementInfoData>(AssetBundleNames.GamedataAchievement, "exchange");
		if (!(null == achievementInfoData))
		{
			achievementExchange = achievementInfoData.param.ToDictionary((AchievementInfoData.Param p) => p.id, (AchievementInfoData.Param p) => AchievementState.AS_NotAchieved);
			AssetBundleManager.UnloadAssetBundle(AssetBundleNames.GamedataAchievement, isUnloadForceRefCount: true);
		}
	}

	public static bool SetAchievementExchangeRelease(int _idx)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return false;
		}
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (!saveData.achievementExchange.ContainsKey(_idx))
		{
			return false;
		}
		AchievementState achievementState = saveData.achievementExchange[_idx];
		saveData.achievementExchange[_idx] = AchievementState.AS_Achieve;
		return achievementState != Singleton<Game>.Instance.saveData.achievementExchange[_idx];
	}

	public static bool IsAchievementExchangeRelease(int _idx)
	{
		if (!Singleton<Game>.IsInstance())
		{
			return false;
		}
		if (!Singleton<Game>.Instance.saveData.achievementExchange.TryGetValue(_idx, out var value))
		{
			return false;
		}
		return value == AchievementState.AS_Achieve;
	}

	public static void AddHCount()
	{
		if (Singleton<Game>.IsInstance())
		{
			SaveData saveData = Singleton<Game>.Instance.saveData;
			saveData.hCount = Mathf.Min(saveData.hCount + 1, 99999);
			SetAchievementAchieve(1);
			if (saveData.hCount >= 10)
			{
				SetAchievementAchieve(5);
			}
		}
	}

	public static void AddHFinish(int _count)
	{
		if (Singleton<Game>.IsInstance())
		{
			SaveData saveData = Singleton<Game>.Instance.saveData;
			saveData.hEjaculation = Mathf.Min(saveData.hEjaculation + _count, 99999);
			if (saveData.hEjaculation >= 10)
			{
				SetAchievementAchieve(6);
			}
		}
	}

	public static void AddHOrgasm(int _count)
	{
		if (Singleton<Game>.IsInstance())
		{
			SaveData saveData = Singleton<Game>.Instance.saveData;
			saveData.hOrgasm = Mathf.Min(saveData.hOrgasm + _count, 99999);
			if (saveData.hOrgasm >= 10)
			{
				SetAchievementAchieve(7);
			}
		}
	}

	public IEnumerable<CommandData> CreateCommandData(string head)
	{
		List<CommandData> list = new List<CommandData>();
		string key = head + "achievementAchieve";
		list.Add(new CommandData(CommandData.Command.Int, key, () => -1, delegate(object o)
		{
			if ((int)o != -1)
			{
				achievementAchieve.Add((int)o);
			}
		}));
		string key2 = head + "escapeCharaName";
		list.Add(new CommandData(CommandData.Command.String, key2, () => escapeCharaName, delegate(object o)
		{
			escapeCharaName = (string)o;
		}));
		string key3 = head + "TutorialNo";
		list.Add(new CommandData(CommandData.Command.Int, key3, () => TutorialNo, delegate(object o)
		{
			TutorialNo = (int)o;
		}));
		return list;
	}

	public void Save()
	{
		Save(UserData.Create("save/"), "save.dat");
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
		return Load(Path + "/", "save.dat");
	}

	public bool Load(string _path, string _fileName)
	{
		return Utils.File.OpenRead(_path + _fileName, delegate(FileStream f)
		{
			using BinaryReader binaryReader = new BinaryReader(f);
			byte[] array = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
			if (!((IReadOnlyCollection<byte>)(object)array).IsNullOrEmpty())
			{
				SaveData source = MessagePackSerializer.Deserialize<SaveData>(array);
				Copy(source);
			}
		});
	}

	public void Copy(SaveData source)
	{
		Version = source.Version;
		TutorialNo = source.TutorialNo;
		achievement = source.achievement.ToDictionary((KeyValuePair<int, AchievementState> a) => a.Key, (KeyValuePair<int, AchievementState> a) => a.Value);
		achievementExchange = source.achievementExchange.ToDictionary((KeyValuePair<int, AchievementState> a) => a.Key, (KeyValuePair<int, AchievementState> a) => a.Value);
		achievementAchieve = new HashSet<int>(source.achievementAchieve);
		for (int num = 0; num < roomList.Length; num++)
		{
			roomList[num] = source.roomList[num].ToList();
		}
		for (int num2 = 0; num2 < dicCloths.Length; num2++)
		{
			dicCloths[num2] = source.dicCloths[num2].ToDictionary((KeyValuePair<string, ClothPngInfo> c) => c.Key, (KeyValuePair<string, ClothPngInfo> c) => c.Value);
		}
		selectGroup = source.selectGroup;
		hCount = source.hCount;
		firstHList = new HashSet<int>(source.firstHList);
		customIDInfo = source.customIDInfo.ToDictionary((KeyValuePair<int, byte> c) => c.Key, (KeyValuePair<int, byte> c) => c.Value);
		playerChara.Copy(source.playerChara);
		secondPlayerChara.Copy(source.secondPlayerChara);
		BeforeFemaleName = source.BeforeFemaleName;
		hEjaculation = source.hEjaculation;
		hOrgasm = source.hOrgasm;
		if (source.playerCloths == null || source.playerCloths.Count == 0)
		{
			playerCloths = new List<PlayerCoordinateInfo>
			{
				new PlayerCoordinateInfo(),
				new PlayerCoordinateInfo()
			};
		}
		else
		{
			playerCloths = new List<PlayerCoordinateInfo>(source.playerCloths);
		}
	}
}
