using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UploaderSystem;

namespace Manager;

public sealed class GameSystem : Singleton<GameSystem>
{
	public enum Language
	{
		Japanese,
		English,
		SimplifiedChinese,
		TraditionalChinese
	}

	public class CultureScope : IDisposable
	{
		private CultureInfo culture;

		public CultureScope()
		{
			string cultrureName = Singleton<GameSystem>.Instance.cultrureName;
			culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultrureName);
		}

		void IDisposable.Dispose()
		{
			Thread.CurrentThread.CurrentCulture = culture;
		}
	}

	public class DownloadInfo
	{
		public HashSet<string> hsCharaHS2 = new HashSet<string>();

		public HashSet<string> hsCharaAI = new HashSet<string>();
	}

	public class ApplauseInfo
	{
		public HashSet<string> hsCharaHS2 = new HashSet<string>();

		public HashSet<string> hsCharaAI = new HashSet<string>();
	}

	public const string BGDir = "bg/";

	public const string CardFDir = "cardframe/Front/";

	public const string CardBDir = "cardframe/Back/";

	public const string SystemDir = "system/";

	public const string VersionFile = "version.dat";

	public const string SetupFile = "setup.xml";

	public const string HNFile = "hn.dat";

	public const string UserInfoFile = "userinfo.dat";

	public const string NetSaveFile = "netsave.dat";

	public const string DownloadInfoFile = "dli.sav";

	public const string ApplauseInfoFile = "ali.sav";

	public const int ProductNo = 100;

	public const string CharaFileMark = "【AIS_Chara】";

	public const string ClothesFileMark = "【AIS_Clothes】";

	public const string HousingFileMark = "【AIS_Housing】";

	public const string StudioFileMark = "【AIS_Studio】";

	public const int RoomEntryMax = 20;

	public readonly Version UserInfoFileVersion = new Version(0, 0, 0);

	public readonly Version NetSaveFileVersion = new Version(0, 0, 0);

	private const string Special02File = "/abdata/chara/02/fo_top_02.unity3d";

	private const string Special03File = "/abdata/chara/03/fo_top_03.unity3d";

	private const string Special04File = "/abdata/chara/04/fo_inner_b_04.unity3d";

	private static bool? _isAdd50;

	private const string add50File = "adv/scenario/c10/50/08.unity3d";

	public const string GameSystemVersion = "0.0.0";

	public readonly string[] cultureNames = new string[4] { "ja-JP", "en-US", "zh-CN", "zh-TW" };

	public string networkSceneName = "";

	public int networkType;

	public bool agreePolicy;

	public DownloadInfo downloadInfo = new DownloadInfo();

	public ApplauseInfo applauseInfo = new ApplauseInfo();

	public bool isSpecial02 { get; private set; }

	public bool isSpecial03 { get; private set; }

	public bool isSpecial04 { get; private set; }

	public static bool isAdd50
	{
		get
		{
			bool? flag = _isAdd50;
			if (!flag.HasValue)
			{
				bool? flag2 = (_isAdd50 = CheckIsAppendDisc());
				return flag2.Value;
			}
			return flag == true;
		}
	}

	public string EncryptedMacAddress { get; private set; }

	public string UserUUID { get; private set; }

	public string UserPasswd { get; private set; }

	public string HandleName { get; private set; }

	public Version GameVersion { get; private set; } = new Version("0.0.0");

	public Language language { get; private set; }

	public int languageInt => (int)language;

	public string cultrureName => cultureNames[languageInt];

	private void CheckIsAISpecial()
	{
		string keyName = "Software\\illusion\\AI-Syoujyo\\AI-Syoujyo";
		if (!YS_Assist.IsRegistryKey(keyName))
		{
			return;
		}
		string registryInfoFrom = YS_Assist.GetRegistryInfoFrom(keyName, "InstallDir");
		if (!registryInfoFrom.IsNullOrEmpty())
		{
			registryInfoFrom = registryInfoFrom.TrimEnd('\\');
			if (File.Exists(registryInfoFrom + "/abdata/chara/02/fo_top_02.unity3d"))
			{
				isSpecial02 = true;
			}
			if (File.Exists(registryInfoFrom + "/abdata/chara/03/fo_top_03.unity3d"))
			{
				isSpecial03 = true;
			}
			if (File.Exists(registryInfoFrom + "/abdata/chara/04/fo_inner_b_04.unity3d"))
			{
				isSpecial04 = true;
			}
		}
	}

	private static bool CheckIsAppendDisc()
	{
		string path = AssetBundleManager.BaseDownloadingURL + "add50";
		if (File.Exists(path) && File.ReadAllText(path).Contains("adv/scenario/c10/50/08.unity3d"))
		{
			return true;
		}
		return false;
	}

	public static bool IsPathAdd50(string _path)
	{
		if (!int.TryParse(Path.GetFileNameWithoutExtension(_path), out var result))
		{
			return false;
		}
		if (result >= 50 && !isAdd50)
		{
			return false;
		}
		return true;
	}

	public void SetUserUUID(string uuid)
	{
		UserUUID = uuid;
	}

	public void SaveHandleName(string hn)
	{
		HandleName = hn;
		string path = UserData.Create("system/") + "hn.dat";
		byte[] bytes = YS_Assist.EncryptAES(Encoding.UTF8.GetBytes(HandleName), "illusion", "honeyselect2");
		File.WriteAllBytes(path, bytes);
	}

	public void LoadHandleName()
	{
		string path = UserData.Create("system/") + "hn.dat";
		if (File.Exists(path))
		{
			try
			{
				byte[] bytes = YS_Assist.DecryptAES(File.ReadAllBytes(path), "illusion", "honeyselect2");
				HandleName = Encoding.UTF8.GetString(bytes);
				return;
			}
			catch (Exception)
			{
				HandleName = "";
				return;
			}
		}
		HandleName = "";
	}

	public void GenerateUserInfo(bool forceGenerate = false)
	{
		if (forceGenerate || !LoadIdAndPass())
		{
			UserUUID = YS_Assist.CreateUUID();
			UserPasswd = YS_Assist.GeneratePassword62(16);
			SaveIdAndPass();
		}
	}

	public void SaveIdAndPass()
	{
		File.WriteAllLines(UserData.Create("system/") + "userinfo.dat", new string[3]
		{
			UserInfoFileVersion.ToString(),
			UserUUID,
			UserPasswd
		});
	}

	public bool LoadIdAndPass()
	{
		try
		{
			string path = UserData.Create("system/") + "userinfo.dat";
			if (!File.Exists(path))
			{
				return false;
			}
			string[] array = File.ReadAllLines(path);
			if (((IReadOnlyCollection<string>)(object)array).IsNullOrEmpty())
			{
				return false;
			}
			new Version(array[0]);
			if (3 != array.Length)
			{
				return false;
			}
			if (array[1].IsNullOrEmpty() || array[2].IsNullOrEmpty())
			{
				return false;
			}
			UserUUID = array[1];
			UserPasswd = array[2];
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public void SaveVersion()
	{
		File.WriteAllText(DefaultData.Create("system/") + "version.dat", "0.0.0");
	}

	public void LoadVersion()
	{
		string path = DefaultData.Path + "system//version.dat";
		if (File.Exists(path))
		{
			string text = File.ReadAllText(path);
			if (!text.IsNullOrEmpty())
			{
				GameVersion = new Version(text);
			}
		}
	}

	private void LoadLanguage()
	{
		string text = UserData.Path + "setup.xml";
		if (!File.Exists(text))
		{
			language = Language.Japanese;
			return;
		}
		try
		{
			XElement xElement = XElement.Load(text);
			if (xElement == null)
			{
				return;
			}
			foreach (XElement item in xElement.Elements())
			{
				if (item.Name.ToString() == "Language")
				{
					language = (Language)int.Parse(item.Value);
					break;
				}
			}
		}
		catch (XmlException)
		{
		}
	}

	public void SaveNetworkSetting()
	{
		using FileStream output = new FileStream(UserData.Create("system/") + "netsave.dat", FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(NetSaveFileVersion.ToString());
		binaryWriter.Write(agreePolicy);
	}

	public void LoadNetworkSetting()
	{
		string path = UserData.Path + "system//netsave.dat";
		if (!File.Exists(path))
		{
			return;
		}
		using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
		using BinaryReader binaryReader = new BinaryReader(input);
		new Version(binaryReader.ReadString());
		agreePolicy = binaryReader.ReadBoolean();
	}

	public bool SaveDownloadInfo()
	{
		using (FileStream output = new FileStream(UserData.Create("system/") + "dli.sav", FileMode.Create, FileAccess.Write))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			HashSet<string> hsCharaHS = downloadInfo.hsCharaHS2;
			int count = hsCharaHS.Count;
			binaryWriter.Write(count);
			foreach (string item in hsCharaHS)
			{
				binaryWriter.Write(item);
			}
			HashSet<string> hsCharaAI = downloadInfo.hsCharaAI;
			count = hsCharaAI.Count;
			binaryWriter.Write(count);
			foreach (string item2 in hsCharaAI)
			{
				binaryWriter.Write(item2);
			}
		}
		return true;
	}

	public bool LoadDownloadInfo()
	{
		string path = UserData.Path + "system/dli.sav";
		if (!File.Exists(path))
		{
			return false;
		}
		downloadInfo.hsCharaHS2.Clear();
		downloadInfo.hsCharaAI.Clear();
		using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				downloadInfo.hsCharaHS2.Add(binaryReader.ReadString());
			}
			num = binaryReader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				downloadInfo.hsCharaAI.Add(binaryReader.ReadString());
			}
		}
		return true;
	}

	public void AddDownload(DataType type, bool isHS2, string uuid)
	{
		if (type == DataType.Chara)
		{
			if (isHS2)
			{
				downloadInfo.hsCharaHS2.Add(uuid);
			}
			else
			{
				downloadInfo.hsCharaAI.Add(uuid);
			}
			SaveDownloadInfo();
		}
	}

	public bool IsDownload(DataType type, bool isHS2, string uuid)
	{
		if (type == DataType.Chara)
		{
			if (isHS2)
			{
				return downloadInfo.hsCharaHS2.Contains(uuid);
			}
			return downloadInfo.hsCharaAI.Contains(uuid);
		}
		return false;
	}

	public bool SaveApplauseInfo()
	{
		using (FileStream output = new FileStream(UserData.Create("system/") + "ali.sav", FileMode.Create, FileAccess.Write))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			HashSet<string> hsCharaHS = applauseInfo.hsCharaHS2;
			int count = hsCharaHS.Count;
			binaryWriter.Write(count);
			foreach (string item in hsCharaHS)
			{
				binaryWriter.Write(item);
			}
			HashSet<string> hsCharaAI = applauseInfo.hsCharaAI;
			count = hsCharaAI.Count;
			binaryWriter.Write(count);
			foreach (string item2 in hsCharaAI)
			{
				binaryWriter.Write(item2);
			}
		}
		return true;
	}

	public bool LoadApplauseInfo()
	{
		string path = UserData.Path + "system/ali.sav";
		if (!File.Exists(path))
		{
			return false;
		}
		applauseInfo.hsCharaHS2.Clear();
		applauseInfo.hsCharaAI.Clear();
		using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				applauseInfo.hsCharaHS2.Add(binaryReader.ReadString());
			}
			num = binaryReader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				applauseInfo.hsCharaAI.Add(binaryReader.ReadString());
			}
		}
		return true;
	}

	public void AddApplause(DataType type, bool isHS2, string uuid)
	{
		if (type == DataType.Chara)
		{
			if (isHS2)
			{
				applauseInfo.hsCharaHS2.Add(uuid);
			}
			else
			{
				applauseInfo.hsCharaAI.Add(uuid);
			}
			SaveApplauseInfo();
		}
	}

	public bool IsApplause(DataType type, bool isHS2, string uuid)
	{
		if (type == DataType.Chara)
		{
			if (isHS2)
			{
				return applauseInfo.hsCharaHS2.Contains(uuid);
			}
			return applauseInfo.hsCharaAI.Contains(uuid);
		}
		return false;
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			string path = UserData.Path + "bg/";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			path = UserData.Path + "cardframe/Back/";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			path = UserData.Path + "cardframe/Front/";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			path = UserData.Path + "chara/female/";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			path = UserData.Path + "chara/male/";
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			CheckIsAISpecial();
			CheckIsAppendDisc();
			EncryptedMacAddress = YS_Assist.CreateIrregularStringFromMacAddress();
			GenerateUserInfo();
			LoadHandleName();
			GameVersion = new Version(0, 0, 0);
			LoadVersion();
			LoadLanguage();
			LoadNetworkSetting();
			LoadDownloadInfo();
			LoadApplauseInfo();
		}
	}

	private void OnApplicationQuit()
	{
	}
}
