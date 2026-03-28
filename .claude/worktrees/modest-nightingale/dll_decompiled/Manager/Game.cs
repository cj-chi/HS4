using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using AIProject;
using Actor;
using CameraEffector;
using HS2;
using UnityEngine;

namespace Manager;

public sealed class Game : Singleton<Game>
{
	public class EventCharaInfo
	{
		public string fileName = string.Empty;

		public string fileNamePartner = string.Empty;

		public int mapID;

		public int eventID;

		public int main = -1;

		public int partner = -1;

		public void MemberInit()
		{
			fileName = string.Empty;
			fileNamePartner = string.Empty;
			mapID = 0;
			eventID = -1;
			main = -1;
			partner = -1;
		}
	}

	public class Expression
	{
		public class Pattern : ICloneable
		{
			public int Ptn { get; set; }

			public bool Blend { get; set; }

			public Pattern()
			{
				Initialize();
			}

			public Pattern(string arg, bool isThrow = false)
			{
				Initialize();
				if (arg.IsNullOrEmpty())
				{
					return;
				}
				string[] array = arg.Split(',');
				int num = 0;
				try
				{
					string element = array.GetElement(num++);
					if (element != null)
					{
						Ptn = int.Parse(element);
					}
					element = array.GetElement(num++);
					if (element != null)
					{
						Blend = bool.Parse(element);
					}
				}
				catch (Exception)
				{
					if (isThrow)
					{
						throw new Exception("Expression Pattern:" + string.Join(",", array));
					}
				}
			}

			public void Initialize()
			{
				Ptn = 0;
				Blend = true;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		protected bool _useEyebrow;

		protected bool _useEyes;

		protected bool _useMouth;

		protected bool _useEyebrowOpen;

		protected bool _useEyesOpen;

		protected bool _useMouthOpen;

		protected bool _useEyesLook;

		protected bool _useHohoAkaRate;

		protected bool _useHighlight;

		protected bool _useTearsLv;

		protected bool _useBlink;

		public bool IsChangeSkip { private get; set; }

		public Pattern Eyebrow { get; private set; }

		public Pattern Eyes { get; private set; }

		public Pattern Mouth { get; private set; }

		public float EyebrowOpen { get; private set; } = 1f;

		public float EyesOpen { get; private set; } = 1f;

		public float MouthOpen { get; private set; } = 1f;

		public int EyesLook { get; private set; }

		public float HohoAkaRate { get; private set; }

		public bool IsHighlight { get; private set; } = true;

		public float TearsRate { get; private set; }

		public bool IsBlink { get; private set; } = true;

		public Expression()
		{
		}

		public Expression(Expression other)
		{
			Copy(other, this);
		}

		public Expression(string[] args, ref int index)
		{
			Initialize(args, ref index);
		}

		public Expression(string[] args)
		{
			int index = 0;
			Initialize(args, ref index);
		}

		public virtual void Initialize(string[] args, ref int index, bool isThrow = false)
		{
			try
			{
				string element = args.GetElement(index++);
				_useEyebrow = !element.IsNullOrEmpty();
				if (_useEyebrow)
				{
					Eyebrow = new Pattern(element, isThrow: true);
				}
				string element2 = args.GetElement(index++);
				_useEyes = !element2.IsNullOrEmpty();
				if (_useEyes)
				{
					Eyes = new Pattern(element2, isThrow: true);
				}
				string element3 = args.GetElement(index++);
				_useMouth = !element3.IsNullOrEmpty();
				if (_useMouth)
				{
					Mouth = new Pattern(element3, isThrow: true);
				}
				string element4 = args.GetElement(index++);
				_useEyebrowOpen = !element4.IsNullOrEmpty();
				if (_useEyebrowOpen)
				{
					EyebrowOpen = float.Parse(element4);
				}
				string element5 = args.GetElement(index++);
				_useEyesOpen = !element5.IsNullOrEmpty();
				if (_useEyesOpen)
				{
					EyesOpen = float.Parse(element5);
				}
				string element6 = args.GetElement(index++);
				_useMouthOpen = !element6.IsNullOrEmpty();
				if (_useMouthOpen)
				{
					MouthOpen = float.Parse(element6);
				}
				string element7 = args.GetElement(index++);
				_useEyesLook = !element7.IsNullOrEmpty();
				if (_useEyesLook)
				{
					EyesLook = int.Parse(element7);
				}
				string element8 = args.GetElement(index++);
				_useHohoAkaRate = !element8.IsNullOrEmpty();
				if (_useHohoAkaRate)
				{
					HohoAkaRate = float.Parse(element8);
				}
				string element9 = args.GetElement(index++);
				_useHighlight = !element9.IsNullOrEmpty();
				if (_useHighlight)
				{
					IsHighlight = bool.Parse(element9);
				}
				string element10 = args.GetElement(index++);
				_useTearsLv = !element10.IsNullOrEmpty();
				if (_useTearsLv)
				{
					TearsRate = float.Parse(element10);
				}
				string element11 = args.GetElement(index++);
				_useBlink = !element11.IsNullOrEmpty();
				if (_useBlink)
				{
					IsBlink = bool.Parse(element11);
				}
			}
			catch (Exception)
			{
				if (isThrow)
				{
					throw new Exception("Expression:" + string.Join(",", args));
				}
			}
		}

		public static void Copy(Expression source, Expression destination)
		{
			destination.Eyebrow = (source?.Eyebrow.Clone() as Pattern) ?? new Pattern();
			destination.Eyes = (source.Eyes?.Clone() as Pattern) ?? new Pattern();
			destination.Mouth = (source.Mouth?.Clone() as Pattern) ?? new Pattern();
			destination.EyebrowOpen = source.EyebrowOpen;
			destination.EyesOpen = source.EyesOpen;
			destination.MouthOpen = source.MouthOpen;
			destination.EyesLook = source.EyesLook;
			destination.HohoAkaRate = source.HohoAkaRate;
			destination.TearsRate = source.TearsRate;
			destination.IsBlink = source.IsBlink;
			destination.IsChangeSkip = source.IsChangeSkip;
			destination._useEyebrow = source._useEyebrow;
			destination._useEyes = source._useEyes;
			destination._useMouth = source._useMouth;
			destination._useEyebrowOpen = source._useEyebrowOpen;
			destination._useEyesOpen = source._useEyesOpen;
			destination._useMouthOpen = source._useMouthOpen;
			destination._useEyesLook = source._useEyesLook;
			destination._useHohoAkaRate = source._useHohoAkaRate;
			destination._useTearsLv = source._useTearsLv;
			destination._useBlink = source._useBlink;
		}

		public void Copy(Expression destination)
		{
			Copy(this, destination);
		}

		public void Change(ChaControl charInfo)
		{
			bool isChangeSkip = IsChangeSkip;
			if (!isChangeSkip || _useEyebrow)
			{
				charInfo.ChangeEyebrowPtn(Eyebrow.Ptn, Eyebrow.Blend);
			}
			if (!isChangeSkip || _useEyes)
			{
				charInfo.ChangeEyesPtn(Eyes.Ptn, Eyes.Blend);
			}
			if (!isChangeSkip || _useMouth)
			{
				charInfo.ChangeMouthPtn(Mouth.Ptn, Mouth.Blend);
			}
			if (!isChangeSkip || _useEyebrowOpen)
			{
				charInfo.ChangeEyebrowOpenMax(EyebrowOpen);
			}
			if (!isChangeSkip || _useEyesOpen)
			{
				charInfo.ChangeEyesOpenMax(EyesOpen);
			}
			if (!isChangeSkip || _useMouthOpen)
			{
				charInfo.ChangeMouthOpenMax(MouthOpen);
			}
			if ((!isChangeSkip || _useEyesLook) && EyesLook != -1)
			{
				charInfo.ChangeLookEyesPtn(EyesLook);
			}
			if (!isChangeSkip || _useHohoAkaRate)
			{
				charInfo.ChangeHohoAkaRate(HohoAkaRate);
			}
			if (!isChangeSkip || _useHighlight)
			{
				charInfo.HideEyeHighlight(!IsHighlight);
			}
			if (!isChangeSkip || _useTearsLv)
			{
				charInfo.ChangeTearsRate(TearsRate);
			}
			if (!isChangeSkip || _useBlink)
			{
				charInfo.ChangeEyesBlinkFlag(_useBlink);
			}
		}
	}

	public static Color defaultFontColor = new Color32(244, 241, 246, byte.MaxValue);

	public static Color selectFontColor = new Color32(58, 61, 67, byte.MaxValue);

	private static bool? _isAdd01 = null;

	public static IReadOnlyList<int> AchievementMapIDList0 = new List<int> { 6, 8, 9, 10 };

	public static IReadOnlyList<int> AchievementMapIDList1 = new List<int> { 12, 13, 14, 15 };

	public static IReadOnlyDictionary<int, int> AppendMapIDTable = new Dictionary<int, int>
	{
		{ 50, 18 },
		{ 51, 19 },
		{ 52, 21 },
		{ 53, 20 },
		{ 54, 23 },
		{ 55, 22 }
	};

	public static IReadOnlyList<int> DirtyEventIDs = new List<int> { 3, 4, 28, 29 };

	public static IReadOnlyList<int> SleepEventIDs = new List<int> { 7, 32 };

	public static IReadOnlyList<int> ToiletEventIDs = new List<int> { 5, 6, 30, 31 };

	public static IReadOnlyList<int> LibidoEventIDs = new List<int> { 8, 9, 10, 11, 12, 13, 14 };

	public string customCharaFileName = string.Empty;

	public Dictionary<string, int> tableDesireCharas = new Dictionary<string, int>();

	public Dictionary<string, EventCharaInfo>[] tableLobbyEvents = new Dictionary<string, EventCharaInfo>[5];

	public Dictionary<string, EventCharaInfo>[] tableHomeEvents = new Dictionary<string, EventCharaInfo>[5];

	public List<Heroine> heroineList = new List<Heroine>();

	public int mapNo;

	public int eventNo = -1;

	public int firstVoiceNo = -1;

	public int peepKind = -1;

	public bool isConciergeAngry;

	public string appendCoordinateFemale = "";

	public string appendCoordinatePlayer = "";

	private ConfigEffectorWet _cameraEffector;

	private Camera _nowCamera;

	private Dictionary<int, ParameterNameInfo.Param> dicPNI = new Dictionary<int, ParameterNameInfo.Param>();

	public static bool isAdd01
	{
		get
		{
			bool? flag = _isAdd01;
			if (!flag.HasValue)
			{
				bool? flag2 = (_isAdd01 = AssetBundleCheck.IsManifest("add01"));
				return flag2.Value;
			}
			return flag == true;
		}
	}

	public static IReadOnlyList<int> DesireEventIDs { get; private set; } = new List<int>();

	public Dictionary<int, AchievementInfoData.Param> infoAchievementDic { get; private set; }

	public Dictionary<int, AchievementInfoData.Param> infoAchievementExchangeDic { get; private set; }

	public Dictionary<int, EventContentInfoData.Param> infoEventContentDic { get; private set; }

	public static IReadOnlyDictionary<int, string> infoStateTable { get; private set; }

	public static IReadOnlyDictionary<int, string> infoTraitTable { get; private set; }

	public static IReadOnlyDictionary<int, string> infoMindTable { get; private set; }

	public static IReadOnlyDictionary<int, string> infoHAttributeTable { get; private set; }

	public static IReadOnlyDictionary<int, PersonalParameterInfo.Param> infoPersonalParameterTable { get; private set; }

	public static IReadOnlyDictionary<int, GameParameterInfo.Param> infoGameParameterTable { get; private set; }

	public static IReadOnlyDictionary<int, BGMNameInfo.Param> infoBGMNameTable { get; private set; }

	public bool IsDebug { get; set; }

	public byte UploaderType { get; set; }

	public string ReserveSceneName { get; set; }

	public GlobalSaveData GlobalData { get; private set; }

	public SaveData saveData { get; private set; } = new SaveData();

	public AppendSaveData appendSaveData { get; private set; } = new AppendSaveData();

	public bool IsAuto { get; set; }

	public Player player { get; set; }

	public ConfigEffectorWet cameraEffector
	{
		get
		{
			Camera camera = nowCamera;
			if (camera == null)
			{
				return null;
			}
			_cameraEffector = camera.GetComponent<ConfigEffectorWet>();
			return _cameraEffector;
		}
	}

	public Camera nowCamera
	{
		get
		{
			if (_nowCamera != Camera.main)
			{
				_nowCamera = Camera.main;
			}
			return _nowCamera;
		}
	}

	public Dictionary<int, Dictionary<string, Expression>> CharaExpTable { get; private set; } = new Dictionary<int, Dictionary<string, Expression>>();

	private Game()
	{
		ReserveSceneName = "";
	}

	protected override void Awake()
	{
		CheckInstance();
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
	}

	private void OnApplicationQuit()
	{
		_ = IsDebug;
	}

	private void LoadAchievementInfo()
	{
		infoAchievementDic = new Dictionary<int, AchievementInfoData.Param>();
		infoAchievementExchangeDic = new Dictionary<int, AchievementInfoData.Param>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(AssetBundleNames.GamedataPath, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		assetBundleNameListFromPath.ForEach(delegate(string file)
		{
			foreach (var item in from p in AssetBundleManager.LoadAllAsset(file, typeof(AchievementInfoData)).GetAllAssets<AchievementInfoData>()
				select (param: p.param, name: p.name))
			{
				foreach (AchievementInfoData.Param item2 in item.param)
				{
					if (item.name == "achievement")
					{
						infoAchievementDic[item2.id] = item2;
					}
					else if (item.name == "exchange")
					{
						infoAchievementExchangeDic[item2.id] = item2;
					}
				}
			}
			AssetBundleManager.UnloadAssetBundle(file, isUnloadForceRefCount: false);
		});
	}

	private void LoadEventContentInfo()
	{
		infoEventContentDic = new Dictionary<int, EventContentInfoData.Param>();
		foreach (EventContentInfoData item in GlobalMethod.LoadAllFolder<EventContentInfoData>(AssetBundleNames.GamedataEventcontentPath, "eventcontent", null, _subdirCheck: true))
		{
			foreach (EventContentInfoData.Param item2 in item.param)
			{
				infoEventContentDic[item2.id] = item2;
			}
		}
	}

	public static byte CharaDataToSex(CharaData charaData)
	{
		return (!(charaData is Player)) ? ((byte)1) : ((byte)0);
	}

	public void SaveGlobalData()
	{
		_ = GlobalData;
	}

	public void LoadGlobalData()
	{
	}

	public void Save()
	{
		saveData.Save();
	}

	public void Load()
	{
		if (!saveData.Load())
		{
			saveData.Initialize();
		}
	}

	public void AppendLoad()
	{
		if (!appendSaveData.Load())
		{
			appendSaveData.Initialize();
		}
	}

	public static void LoadExpExcelData(Dictionary<string, Expression> dic, ExcelData excelData)
	{
		foreach (ExcelData.Param item in excelData.list.Skip(1))
		{
			if (!AIProject.CollectionExtensions.IsNullOrEmpty(item.list))
			{
				Expression value = new Expression(item.list.Skip(1).ToArray())
				{
					IsChangeSkip = true
				};
				dic[item.list[0]] = value;
			}
		}
	}

	public static Expression GetExpression(Dictionary<string, Expression> dic, string key)
	{
		dic.TryGetValue(key, out var value);
		return value;
	}

	public Expression GetExpression(int personality, string key)
	{
		if (!CharaExpTable.TryGetValue(personality, out var value))
		{
			return null;
		}
		return GetExpression(value, key);
	}

	private void LoadParameterNameInfo()
	{
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(AssetBundleNames.EtcetraListParameternamePath, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		assetBundleNameListFromPath.ForEach(delegate(string file)
		{
			if (GameSystem.IsPathAdd50(file))
			{
				foreach (List<ParameterNameInfo.Param> item in from p in AssetBundleManager.LoadAllAsset(file, typeof(ParameterNameInfo)).GetAllAssets<ParameterNameInfo>()
					select p.param)
				{
					foreach (ParameterNameInfo.Param item2 in item)
					{
						dicPNI[item2.id] = item2;
					}
				}
				AssetBundleManager.UnloadAssetBundle(file, isUnloadForceRefCount: false);
			}
		});
		int langage = Singleton<GameSystem>.Instance.languageInt;
		infoStateTable = dicPNI.Where((KeyValuePair<int, ParameterNameInfo.Param> d) => !d.Value.state[langage].IsNullOrEmpty()).Select((KeyValuePair<int, ParameterNameInfo.Param> p, int index) => (index: index, p.Value.state[langage])).ToDictionary(((int index, string) p) => p.index, ((int index, string) p) => p.Item2);
		infoTraitTable = dicPNI.Where((KeyValuePair<int, ParameterNameInfo.Param> d) => !d.Value.trait[langage].IsNullOrEmpty()).Select((KeyValuePair<int, ParameterNameInfo.Param> p, int index) => (index: index, p.Value.trait[langage])).ToDictionary(((int index, string) p) => p.index, ((int index, string) p) => p.Item2);
		infoMindTable = dicPNI.Where((KeyValuePair<int, ParameterNameInfo.Param> d) => !d.Value.mind[langage].IsNullOrEmpty()).Select((KeyValuePair<int, ParameterNameInfo.Param> p, int index) => (index: index, p.Value.mind[langage])).ToDictionary(((int index, string) p) => p.index, ((int index, string) p) => p.Item2);
		infoHAttributeTable = dicPNI.Where((KeyValuePair<int, ParameterNameInfo.Param> d) => !d.Value.hattribute[langage].IsNullOrEmpty()).Select((KeyValuePair<int, ParameterNameInfo.Param> p, int index) => (index: index, p.Value.hattribute[langage])).ToDictionary(((int index, string) p) => p.index, ((int index, string) p) => p.Item2);
	}

	private void LoadBGMNameInfo()
	{
		Dictionary<int, BGMNameInfo.Param> dic = new Dictionary<int, BGMNameInfo.Param>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(AssetBundleNames.GamedataBgmnamePath, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		assetBundleNameListFromPath.ForEach(delegate(string file)
		{
			if (GameSystem.IsPathAdd50(file))
			{
				foreach (List<BGMNameInfo.Param> item in from p in AssetBundleManager.LoadAllAsset(file, typeof(BGMNameInfo)).GetAllAssets<BGMNameInfo>()
					select p.param)
				{
					foreach (BGMNameInfo.Param item2 in item)
					{
						dic[item2.bgmID] = item2;
					}
				}
				AssetBundleManager.UnloadAssetBundle(file, isUnloadForceRefCount: false);
			}
		});
		infoBGMNameTable = dic.ToDictionary((KeyValuePair<int, BGMNameInfo.Param> p) => p.Key, (KeyValuePair<int, BGMNameInfo.Param> p) => p.Value);
	}

	private void LoadPersonalParameterInfo()
	{
		Dictionary<int, PersonalParameterInfo.Param> dic = new Dictionary<int, PersonalParameterInfo.Param>();
		GlobalMethod.LoadAllFolder<PersonalParameterInfo>(AssetBundleNames.EtcetraListPersonalparameterPath, "00", null, _subdirCheck: true).ForEach(delegate(PersonalParameterInfo a)
		{
			a.param.ForEach(delegate(PersonalParameterInfo.Param p)
			{
				dic[p.id] = p;
			});
		});
		infoPersonalParameterTable = dic.ToDictionary((KeyValuePair<int, PersonalParameterInfo.Param> p) => p.Key, (KeyValuePair<int, PersonalParameterInfo.Param> p) => p.Value);
	}

	private void LoadGameParameterInfo()
	{
		Dictionary<int, GameParameterInfo.Param> dic = new Dictionary<int, GameParameterInfo.Param>();
		GlobalMethod.LoadAllFolder<GameParameterInfo>(AssetBundleNames.EtcetraListGameparameterPath, "00", null, _subdirCheck: true).ForEach(delegate(GameParameterInfo a)
		{
			a.param.ForEach(delegate(GameParameterInfo.Param p)
			{
				dic[p.id] = p;
			});
		});
		infoGameParameterTable = dic.ToDictionary((KeyValuePair<int, GameParameterInfo.Param> p) => p.Key, (KeyValuePair<int, GameParameterInfo.Param> p) => p.Value);
	}

	public void GameParameterInit(bool _isActorCler = true)
	{
		if (_isActorCler)
		{
			player = null;
			heroineList.Clear();
		}
		mapNo = 0;
		eventNo = -1;
		peepKind = -1;
		isConciergeAngry = false;
		customCharaFileName = string.Empty;
		tableDesireCharas.Clear();
	}

	public void CharaEventShuffle()
	{
		for (int i = 0; i < saveData.roomList.Length; i++)
		{
			List<string> list = saveData.roomList[i];
			tableHomeEvents[i].Clear();
			tableLobbyEvents[i].Clear();
			foreach (GlobalHS2Calc.MapCharaSelectInfo item in GlobalHS2Calc.PlaceCharaOnTheMap(i))
			{
				EventCharaInfo eventCharaInfo = new EventCharaInfo
				{
					mapID = item.mapID,
					eventID = item.eventID
				};
				string empty = string.Empty;
				string text = string.Empty;
				if (eventCharaInfo.eventID == 2 || eventCharaInfo.eventID == 14)
				{
					empty = item.lstChara.FirstOrDefault(((string, int) lc) => lc.Item2 == 0).Item1;
					text = item.lstChara.FirstOrDefault(((string, int) lc) => lc.Item2 == 1).Item1;
				}
				else
				{
					empty = item.lstChara.FirstOrDefault().Item1;
				}
				int num = list.IndexOf(empty);
				int num2 = list.IndexOf(text);
				if (num != -1 && !tableHomeEvents[i].ContainsKey(empty))
				{
					if (num2 != -1)
					{
						eventCharaInfo.main = 0;
						eventCharaInfo.partner = num2;
						eventCharaInfo.fileNamePartner = text;
					}
					eventCharaInfo.fileName = empty;
					tableHomeEvents[i].Add(empty, eventCharaInfo);
				}
				if (num2 != -1 && !tableHomeEvents[i].ContainsKey(text))
				{
					eventCharaInfo = new EventCharaInfo
					{
						mapID = item.mapID,
						eventID = item.eventID
					};
					eventCharaInfo.main = 1;
					eventCharaInfo.partner = num;
					eventCharaInfo.fileName = text;
					eventCharaInfo.fileNamePartner = empty;
					tableHomeEvents[i].Add(text, eventCharaInfo);
				}
			}
			Dictionary<int, GlobalHS2Calc.MapCharaSelectInfo> source = GlobalHS2Calc.GetEventNo(i);
			tableLobbyEvents[i] = source.ToDictionary((KeyValuePair<int, GlobalHS2Calc.MapCharaSelectInfo> d) => d.Value.lstChara[0].Item1, (KeyValuePair<int, GlobalHS2Calc.MapCharaSelectInfo> d) => new EventCharaInfo
			{
				eventID = d.Value.eventID,
				fileName = d.Value.lstChara[0].Item1
			});
		}
	}

	public bool IsAllNormalState(int _selectRoom)
	{
		return saveData.roomList[_selectRoom].Select((string n) => Load(n)).All((ChaFileControl c) => c.gameinfo2.nowDrawState == ChaFileDefine.State.Blank || c.gameinfo2.hCount == 0);
		static ChaFileControl Load(string _fileName)
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			chaFileControl.LoadCharaFile(_fileName, 1);
			return chaFileControl;
		}
	}

	public bool[] IsAllState(int _selectRoom)
	{
		List<string> list = saveData.roomList[_selectRoom];
		bool[] array = new bool[6];
		for (int i = 0; i < list.Count; i++)
		{
			string filename = list[i];
			ChaFileControl chaFileControl = new ChaFileControl();
			chaFileControl.LoadCharaFile(filename, 1);
			ChaFileGameInfo2 gameinfo = chaFileControl.gameinfo2;
			ChaFileDefine.State nowDrawState = gameinfo.nowDrawState;
			int num = (int)(nowDrawState - 1);
			if (gameinfo.hCount != 0 && nowDrawState != ChaFileDefine.State.Blank && !array[num])
			{
				array[num] = true;
			}
		}
		return array;
	}
}
