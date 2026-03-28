using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Actor;
using HS2;
using Illusion.Anime;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Async;
using UnityEngine;

namespace ADV;

internal class ADVMainScene : MonoBehaviour
{
	private class PackData : CharaPackData
	{
		public bool isHScene
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("isHScene", out var value))
				{
					return (bool)value.o;
				}
				return false;
			}
		}

		public bool isHome
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("isHome", out var value))
				{
					return (bool)value.o;
				}
				return false;
			}
		}

		public bool isLobby
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("isLobby", out var value))
				{
					return (bool)value.o;
				}
				return false;
			}
		}

		public bool isSpecialRoom
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("isSpecialRoom", out var value))
				{
					return (bool)value.o;
				}
				return false;
			}
		}

		public bool isDelete
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("isDelete", out var value))
				{
					return (bool)value.o;
				}
				return false;
			}
		}

		public bool isBroken
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("isBroken", out var value))
				{
					return (bool)value.o;
				}
				return false;
			}
		}

		public int eventNo
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("eventNo", out var value))
				{
					return (int)value.o;
				}
				return -1;
			}
		}

		public int peepKind
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("peepKind", out var value))
				{
					return (int)value.o;
				}
				return -1;
			}
		}

		public int parameterSetNo0
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("parameterSetNo0", out var value))
				{
					return (int)value.o;
				}
				return -1;
			}
		}

		public int parameterSetNo1
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("parameterSetNo1", out var value))
				{
					return (int)value.o;
				}
				return -1;
			}
		}

		public bool isAchievementRelease
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("isAchievementRelease", out var value))
				{
					return (bool)value.o;
				}
				return false;
			}
		}

		public string MapName { get; set; } = "";

		public string EventCGBundle { get; set; } = "";

		public string EventCGName { get; set; } = "";

		public int JumpAnalAndPain { get; set; }

		public bool isConciergeAngry { get; set; }

		public bool isTalkToH { get; set; }

		public bool isReturnFromDependency { get; set; }

		public bool isEscapeFromFemale { get; set; }

		public bool isPeepingMasturbation { get; set; }

		public override List<Program.Transfer> Create()
		{
			List<Program.Transfer> list = base.Create();
			list.Add(Program.Transfer.VAR("string", "MapName", MapName));
			list.Add(Program.Transfer.VAR("string", "EventCGBundle", EventCGBundle));
			list.Add(Program.Transfer.VAR("string", "EventCGName", EventCGName));
			list.Add(Program.Transfer.VAR("int", "JumpAnalAndPain", JumpAnalAndPain.ToString()));
			list.Add(Program.Transfer.VAR("bool", "isConciergeAngry", isConciergeAngry.ToString()));
			list.Add(Program.Transfer.VAR("bool", "isTalkToH", isTalkToH.ToString()));
			list.Add(Program.Transfer.VAR("bool", "isReturnFromDependency", isReturnFromDependency.ToString()));
			list.Add(Program.Transfer.VAR("bool", "isEscapeFromFemale", isEscapeFromFemale.ToString()));
			list.Add(Program.Transfer.VAR("bool", "isPeepingMasturbation", isPeepingMasturbation.ToString()));
			return list;
		}

		public override void Receive(TextScenario scenario)
		{
			using (Dictionary<int, List<CommandData>>.Enumerator enumerator = CommandListToTable().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					foreach (CommandData item in enumerator.Current.Value)
					{
						item.ReceiveADV(scenario);
					}
				}
			}
			base.Vars = scenario.Vars;
			base.onComplete?.Invoke();
		}
	}

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private OpenData openData = new OpenData();

	private PackData packData { get; set; }

	private bool isNowADV { get; set; }

	private async void Start()
	{
		Manager.Sound.Listener = null;
		await UniTask.WaitUntil(() => SingletonInitializer<Scene>.initialized);
		Scene.DrawLoadingImage(isDraw: true);
		await Setup.LoadAsync(base.transform);
		Game game = Singleton<Game>.Instance;
		await BaseMap.ChangeAsync(game.mapNo, FadeCanvas.Fade.None);
		SingletonInitializer<BaseMap>.instance.MobObjectsVisible(game.eventNo == 52);
		LoadChara();
		ChaFileGameInfo2 chaFileGameInfo = ((game.heroineList.Count > 0) ? game.heroineList[0].gameinfo2 : null);
		ADVManager instance = Singleton<ADVManager>.Instance;
		instance.filenames[0] = "";
		instance.filenames[1] = "";
		foreach (Heroine heroine in game.heroineList)
		{
			if (heroine != null)
			{
				new Controller(heroine.chaCtrl);
				heroine.param.Bind(new global::Actor.Actor(heroine));
				heroine.chaCtrl.neckLookCtrl.target = Camera.main.transform;
				heroine.chaCtrl.eyeLookCtrl.target = Camera.main.transform;
			}
		}
		if (Singleton<ADVManager>.Instance.advDelivery.charaID != -10)
		{
			ADVScenarioInit();
		}
		SetWet();
		packData = new PackData();
		packData.SetCommandData(game.saveData, game.appendSaveData);
		packData.SetParam((from p in game.heroineList.OfType<IParams>().Concat(game.player)
			where p != null
			select p).ToArray());
		packData.MapName = BaseMap.ConvertMapName(game.mapNo);
		packData.EventCGName = BaseMap.ConvertMapNameEnglish(game.mapNo) + "_Event" + instance.advDelivery.adv_category.ToString("00");
		List<string> list = (from ab in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.AdvEventcgPath)
			orderby ab descending
			select ab).ToList();
		packData.EventCGBundle = string.Empty;
		foreach (string item in list)
		{
			if (GameSystem.IsPathAdd50(item) && AssetBundleCheck.GetAllAssetName(item, _WithExtension: false).Contains(packData.EventCGName))
			{
				packData.EventCGBundle = item;
				break;
			}
		}
		packData.JumpAnalAndPain = GlobalHS2Calc.AnalAndPain(chaFileGameInfo);
		packData.isConciergeAngry = game.isConciergeAngry;
		SetRandomRate(chaFileGameInfo);
		packData.isParent = true;
		packData.onComplete = delegate
		{
			ADVComplete();
		};
		Open();
		Manager.Sound.Listener = mainCamera.transform;
		Scene.DrawLoadingImage(isDraw: false);
	}

	private void Open()
	{
		isNowADV = true;
		ADVManager instance = Singleton<ADVManager>.Instance;
		openData.FindLoad(instance.advDelivery.asset, instance.advDelivery.charaID, instance.advDelivery.adv_category);
		Setup.Open(openData, packData);
	}

	private void OnDestroy()
	{
		Setup.Dispose();
		AlreadyReadInfo.Save();
	}

	private void LoadChara()
	{
		Game instance = Singleton<Game>.Instance;
		ADVManager instance2 = Singleton<ADVManager>.Instance;
		for (int i = 0; i < instance2.filenames.Length; i++)
		{
			if (!instance2.filenames[i].IsNullOrEmpty())
			{
				if (instance.heroineList.Count > i && instance.heroineList[i] != null)
				{
					Singleton<Character>.Instance.DeleteChara(instance.heroineList[i].chaCtrl);
					instance.heroineList.RemoveAt(i);
				}
				ChaFileControl chaFileControl = new ChaFileControl();
				chaFileControl.LoadCharaFile(instance2.filenames[i], 1);
				ChaControl chaControl = Singleton<Character>.Instance.CreateChara(1, Scene.commonSpace, i, chaFileControl);
				chaControl.ChangeNowCoordinate();
				chaControl.releaseCustomInputTexture = false;
				chaControl.Load();
				instance.heroineList.Insert(i, new Heroine(chaFileControl, isRandomize: false));
				instance.heroineList[i].SetRoot(chaControl.gameObject);
			}
		}
		if (instance.heroineList.Count > 1 && instance.heroineList[1] != null)
		{
			instance.heroineList[1].chaCtrl.visibleAll = true;
		}
		PlayerCharaSaveInfo playerChara = instance.saveData.playerChara;
		if (instance.player == null || !(instance.player.chaCtrl != null))
		{
			ChaControl chaControl2 = Singleton<Character>.Instance.CreateChara((byte)playerChara.Sex, Scene.commonSpace, 99);
			chaControl2.isPlayer = true;
			chaControl2.chaFile.LoadCharaFile(playerChara.FileName);
			chaControl2.ChangeNowCoordinate();
			chaControl2.Load();
			new Controller(chaControl2);
			instance.player = new Player(chaControl2.chaFile, isRandomize: true);
			instance.player.SetRoot(chaControl2.gameObject);
			instance.player.param.Bind(new global::Actor.Actor(instance.player));
		}
		instance.player.chaCtrl.visibleAll = false;
	}

	private void SetRandomRate(ChaFileGameInfo2 _info)
	{
		if (_info == null)
		{
			return;
		}
		if (_info.Libido >= 100)
		{
			packData.isTalkToH = true;
		}
		else if (_info.Libido >= 80)
		{
			if (Random.Range(0, 100) < 50)
			{
				packData.isTalkToH = true;
			}
		}
		else if (_info.Libido >= 50 && Random.Range(0, 100) < 30)
		{
			packData.isTalkToH = true;
		}
		int num = 100 - _info.Dependence;
		if (Random.Range(0, 100) < num)
		{
			packData.isReturnFromDependency = true;
		}
		if (Random.Range(0, 100) < 50)
		{
			packData.isEscapeFromFemale = true;
		}
		if (((_info.nowDrawState == ChaFileDefine.State.Favor && _info.Favor >= 80) || _info.nowDrawState == ChaFileDefine.State.Enjoyment) && _info.Libido >= 80 && _info.resistH >= 100)
		{
			packData.isPeepingMasturbation = true;
		}
	}

	private void ADVScenarioInit()
	{
		Game instance = Singleton<Game>.Instance;
		ADVManager instance2 = Singleton<ADVManager>.Instance;
		if (instance.heroineList.Count > 0)
		{
			_ = instance.heroineList[0].gameinfo2;
		}
		if (instance.heroineList.Count > 1 && instance.heroineList[1] != null)
		{
			_ = instance.heroineList[1].gameinfo2;
		}
		switch (instance2.advDelivery.adv_category)
		{
		case 0:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 1:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 2:
			if (!instance2.advDelivery.asset.ContainsAny("1"))
			{
				SetCoordinate();
			}
			PlayBGM(0);
			break;
		case 3:
			if (instance2.advDelivery.asset.ContainsAny("0"))
			{
				SetCoordinate(_isChange: true, 0);
			}
			PlayBGM(0);
			break;
		case 4:
			if (instance2.advDelivery.asset.ContainsAny("0"))
			{
				SetCoordinate(_isChange: true, 0);
			}
			PlayBGM(0);
			break;
		case 5:
			PlayBGM(0);
			break;
		case 6:
			PlayBGM(0);
			break;
		case 7:
			if (!instance2.advDelivery.asset.ContainsAny("2", "3"))
			{
				SetCoordinate();
			}
			PlayBGM(0);
			break;
		case 8:
			if (!instance2.advDelivery.asset.ContainsAny("2"))
			{
				SetCoordinate(_isChange: true, 0);
			}
			PlayBGM(0);
			break;
		case 9:
			if (!instance2.advDelivery.asset.ContainsAny("2"))
			{
				SetCoordinate(_isChange: true, 0);
			}
			PlayBGM(0);
			break;
		case 10:
			PlayBGM(0);
			break;
		case 11:
			PlayBGM(0);
			break;
		case 12:
			PlayBGM(0);
			break;
		case 13:
			PlayBGM(0);
			break;
		case 14:
			PlayBGM(0);
			break;
		case 15:
			PlayBGM(0);
			break;
		case 16:
			PlayBGM(0);
			break;
		case 17:
			PlayBGM(0);
			break;
		case 18:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 19:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 20:
			PlayBGM(0);
			break;
		case 21:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 22:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 23:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 24:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 25:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 26:
			SetCoordinate();
			PlayBGM(0);
			break;
		case 27:
			PlayBGM(0);
			if (instance.heroineList.Count > 1 && instance.heroineList[1] != null)
			{
				instance.heroineList[1].chaCtrl.visibleAll = false;
			}
			break;
		case 28:
			if (instance2.advDelivery.asset == "0")
			{
				SetCoordinate(_isChange: true, 0);
			}
			else if (instance2.advDelivery.asset.ContainsAny("2", "4"))
			{
				SetCoordinate(_isChange: false, 1);
			}
			PlayBGM(0);
			Singleton<HSceneManager>.Instance.pngFemales[1] = string.Empty;
			break;
		case 29:
			if (instance2.advDelivery.asset == "0")
			{
				SetCoordinate(_isChange: true, 0);
			}
			else if (instance2.advDelivery.asset.ContainsAny("2", "4"))
			{
				SetCoordinate(_isChange: false, 1);
			}
			PlayBGM(0);
			Singleton<HSceneManager>.Instance.pngFemales[1] = string.Empty;
			break;
		case 30:
			if (instance2.advDelivery.asset.ContainsAny("9"))
			{
				SetCoordinate();
			}
			else if (instance2.advDelivery.asset == "2" || instance2.advDelivery.asset == "4")
			{
				SetCoordinate(_isChange: true, 1);
			}
			PlayBGM(0);
			Singleton<HSceneManager>.Instance.pngFemales[1] = string.Empty;
			break;
		case 31:
			if (instance2.advDelivery.asset.ContainsAny("9"))
			{
				SetCoordinate();
			}
			else if (instance2.advDelivery.asset == "2" || instance2.advDelivery.asset == "4")
			{
				SetCoordinate(_isChange: true, 1);
			}
			PlayBGM(0);
			Singleton<HSceneManager>.Instance.pngFemales[1] = string.Empty;
			break;
		case 32:
			if (!instance2.advDelivery.asset.ContainsAny("3", "4"))
			{
				SetCoordinate();
			}
			PlayBGM(0);
			Singleton<HSceneManager>.Instance.pngFemales[1] = string.Empty;
			break;
		case 33:
			PlayBGM(1);
			break;
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
			if (instance2.advDelivery.asset.ContainsAny("0"))
			{
				SetAppendCoordinate();
			}
			break;
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 43:
		case 44:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 56:
		case 57:
		case 58:
			break;
		}
	}

	private void ADVScenarioOPInit()
	{
		switch (Singleton<ADVManager>.Instance.advDelivery.adv_category)
		{
		}
	}

	private void SetCoordinate(bool _isChange = true, int _isForceClothState = -1)
	{
		Game game = Singleton<Game>.Instance;
		SaveData save = game.saveData;
		if (_isChange)
		{
			if (game.mapNo == 3)
			{
				ChangeCoordinate(1);
			}
			else if (game.mapNo == 4 || game.mapNo == 52 || game.mapNo == 53 || game.mapNo == 100 || game.mapNo == 101)
			{
				ChangeCoordinate(0);
			}
		}
		else
		{
			ChangeCoordinate(-1);
		}
		if (_isForceClothState == -1)
		{
			return;
		}
		foreach (Heroine heroine in game.heroineList)
		{
			if (heroine != null && !(heroine.chaCtrl == null))
			{
				heroine.chaCtrl.SetClothesStateAll((byte)((_isForceClothState == 0) ? 2u : 0u));
			}
		}
		void ChangeCoordinate(int _area)
		{
			foreach (Heroine heroine2 in game.heroineList)
			{
				if (heroine2 != null && !(heroine2.chaCtrl == null))
				{
					if (_area < 0)
					{
						heroine2.chaCtrl.ChangeNowCoordinate(reload: true);
					}
					else
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(heroine2.chaFile.charaFileName);
						bool flag = false;
						if (!save.dicCloths[save.selectGroup].ContainsKey(fileNameWithoutExtension) || ((_area == 0) ? save.dicCloths[save.selectGroup][fileNameWithoutExtension].bathFile.IsNullOrEmpty() : save.dicCloths[save.selectGroup][fileNameWithoutExtension].roomWearFile.IsNullOrEmpty()))
						{
							string assetName = ((_area == 0) ? "bath" : "roomwear");
							TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(AssetBundleNames.CustomCustom_Etc, assetName);
							if (!(textAsset == null))
							{
								heroine2.chaCtrl.nowCoordinate.LoadFile(textAsset);
								heroine2.chaCtrl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
								AssetBundleManager.UnloadAssetBundle(AssetBundleNames.CustomCustom_Etc, isUnloadForceRefCount: true);
							}
						}
						else
						{
							ClothPngInfo clothPngInfo = save.dicCloths[save.selectGroup][fileNameWithoutExtension];
							heroine2.chaCtrl.ChangeNowCoordinate((_area == 0) ? clothPngInfo.bathFile : clothPngInfo.roomWearFile, reload: true);
							heroine2.chaCtrl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
						}
					}
				}
			}
		}
	}

	private void SetAppendCoordinate()
	{
		Game instance = Singleton<Game>.Instance;
		if (!instance.appendCoordinateFemale.IsNullOrEmpty())
		{
			Heroine heroine = instance.heroineList[0];
			if (heroine != null && !(heroine.chaCtrl == null))
			{
				_ = instance.appendCoordinateFemale;
				heroine.chaCtrl.ChangeNowCoordinate(instance.appendCoordinateFemale, reload: true);
				heroine.chaCtrl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
			}
		}
	}

	private void SetWet()
	{
		Game instance = Singleton<Game>.Instance;
		ADVManager instance2 = Singleton<ADVManager>.Instance;
		float wetRate = 0f;
		int[] source = new int[2] { 8, 9 };
		if (instance.mapNo == 6)
		{
			wetRate = 1f;
		}
		else if (Game.DirtyEventIDs.Contains(instance2.advDelivery.adv_category))
		{
			if (instance.peepKind == 2 || instance.peepKind == 3 || instance.peepKind == 4)
			{
				wetRate = 1f;
			}
			else if ((instance2.advDelivery.adv_category == 3 || instance2.advDelivery.adv_category == 4) && (instance2.advDelivery.asset == "1" || instance2.advDelivery.asset == "2"))
			{
				wetRate = 1f;
			}
			else if ((instance2.advDelivery.adv_category == 28 || instance2.advDelivery.adv_category == 29) && (instance2.advDelivery.asset == "1" || instance2.advDelivery.asset == "3"))
			{
				wetRate = 1f;
			}
		}
		else if (source.Contains(instance2.advDelivery.adv_category))
		{
			if (instance2.advDelivery.asset == "2")
			{
				wetRate = 1f;
			}
		}
		else if (instance2.advDelivery.adv_category == 27 && instance.peepKind == 4 && (instance.mapNo == 52 || instance.mapNo == 53))
		{
			wetRate = 1f;
		}
		else if (instance2.advDelivery.adv_category == 50 && instance2.advDelivery.asset == "1")
		{
			wetRate = 1f;
		}
		foreach (Heroine heroine in instance.heroineList)
		{
			if (heroine != null && !(heroine.chaCtrl == null))
			{
				heroine.chaCtrl.wetRate = wetRate;
			}
		}
	}

	private void PlayBGM(int _kind)
	{
		Game instance = Singleton<Game>.Instance;
		ChaFileGameInfo2 chaFileGameInfo = ((instance.heroineList.Count > 0) ? instance.heroineList[0].gameinfo2 : null);
		if (_kind == 0 && chaFileGameInfo != null)
		{
			Utils.Sound.Play(new Utils.Sound.SettingBGM((BGM)(5 + chaFileGameInfo.nowDrawState)));
		}
		else if (_kind == 1)
		{
			Utils.Sound.Play(new Utils.Sound.SettingBGM(BGM.fur));
		}
	}

	private void ADVComplete()
	{
		Game instance = Singleton<Game>.Instance;
		ChaFileGameInfo2 chaFileGameInfo = ((instance.heroineList.Count > 0) ? instance.heroineList[0].gameinfo2 : null);
		ChaFileParameter2 chaFileParameter = ((instance.heroineList.Count > 0) ? instance.heroineList[0].parameter2 : null);
		ChaFileGameInfo2 chaFileGameInfo2 = ((instance.heroineList.Count <= 1) ? null : ((instance.heroineList[1] != null) ? instance.heroineList[1].gameinfo2 : null));
		ChaFileParameter2 chaFileParameter2 = ((instance.heroineList.Count <= 1) ? null : ((instance.heroineList[1] != null) ? instance.heroineList[1].parameter2 : null));
		isNowADV = false;
		if (instance.player != null && instance.player.chaCtrl != null)
		{
			instance.player.chaCtrl.visibleAll = false;
			instance.player.chaCtrl.visibleSon = false;
		}
		if (packData.isBroken && chaFileGameInfo != null)
		{
			chaFileGameInfo.Broken = 100;
		}
		instance.firstVoiceNo = packData.eventNo;
		instance.peepKind = packData.peepKind;
		chaFileGameInfo?.map.Add(instance.mapNo);
		chaFileGameInfo2?.map.Add(instance.mapNo);
		AddAppendEventNo();
		if (chaFileGameInfo != null && instance.heroineList[0].chaCtrl.chaID != -1)
		{
			GlobalHS2Calc.CalcParameter(packData.parameterSetNo0, chaFileGameInfo, chaFileParameter.personality);
			GlobalHS2Calc.CalcState(chaFileGameInfo, chaFileParameter.personality);
			instance.heroineList[0].chaFile.SaveCharaFile(instance.heroineList[0].chaFile.charaFileName, 1);
		}
		if (chaFileGameInfo2 != null && packData.parameterSetNo1 != -1 && instance.heroineList[1].chaCtrl.chaID != -1)
		{
			GlobalHS2Calc.CalcParameter(packData.parameterSetNo1, chaFileGameInfo2, chaFileParameter2.personality);
			GlobalHS2Calc.CalcState(chaFileGameInfo2, chaFileParameter2.personality);
			instance.heroineList[1].chaFile.SaveCharaFile(instance.heroineList[1].chaFile.charaFileName, 1);
		}
		if (packData.isDelete)
		{
			CharacterDelete();
		}
		Achievement();
		if (instance.player != null && instance.player.chaCtrl != null)
		{
			ChaFileStatus fileStatus = instance.player.chaCtrl.fileStatus;
			fileStatus.visibleHeadAlways = true;
			fileStatus.visibleBodyAlways = true;
			instance.player.chaCtrl.visibleSon = true;
			fileStatus.visibleGomu = true;
		}
		if (packData.isHome)
		{
			GoToHome();
		}
		else if (packData.isHScene)
		{
			GotoHScene();
		}
		else if (packData.isLobby)
		{
			GotoLobby();
		}
		else if (packData.isSpecialRoom)
		{
			GoToSpecialRoom();
		}
	}

	private void CharacterDelete()
	{
		Game instance = Singleton<Game>.Instance;
		List<string>[] array = new List<string>[5];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<string>(Singleton<Game>.Instance.saveData.roomList[i]);
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(instance.heroineList[0].chaFile.charaFileName);
		ChaFileControl chaFile = instance.heroineList[0].chaFile;
		if (chaFile.LoadCharaFile(fileNameWithoutExtension, 1))
		{
			chaFile.gameinfo2.MemberInit();
			chaFile.InitGameInfoParam();
			chaFile.SaveCharaFile(fileNameWithoutExtension, 1);
			instance.saveData.escapeCharaName = chaFile.parameter.fullname;
		}
		List<string>[] roomList = instance.saveData.roomList;
		foreach (List<string> list in roomList)
		{
			if (list.Contains(fileNameWithoutExtension))
			{
				list.Remove(fileNameWithoutExtension);
			}
		}
		Dictionary<string, ClothPngInfo>[] dicCloths = instance.saveData.dicCloths;
		foreach (Dictionary<string, ClothPngInfo> dictionary in dicCloths)
		{
			if (dictionary.ContainsKey(fileNameWithoutExtension))
			{
				dictionary.Remove(fileNameWithoutExtension);
			}
		}
		instance.heroineList.Clear();
		Singleton<Character>.Instance.DeleteCharaAll();
	}

	private void GoToHome()
	{
		Game game = Singleton<Game>.Instance;
		SaveData save = game.saveData;
		if (save.TutorialNo == 11 || save.TutorialNo == 14)
		{
			int Add = ((save.TutorialNo != 11) ? 1 : 0);
			Observable.NextFrame().Subscribe(delegate
			{
				ADVManager instance = Singleton<ADVManager>.Instance;
				save.TutorialNo++;
				instance.advDelivery.Set("0", -10, 5 + Add);
				game.mapNo = 1;
				BaseMap.Change(game.mapNo, FadeCanvas.Fade.None);
				packData.SetParam(new IParams[1]);
				packData.MapName = BaseMap.ConvertMapName(game.mapNo);
				packData.EventCGName = BaseMap.ConvertMapNameEnglish(game.mapNo) + "_Event" + instance.advDelivery.adv_category.ToString("00");
				List<string> list = (from ab in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.AdvEventcgPath)
					orderby ab descending
					select ab).ToList();
				packData.EventCGBundle = string.Empty;
				foreach (string item in list)
				{
					if (GameSystem.IsPathAdd50(item) && AssetBundleCheck.GetAllAssetName(item, _WithExtension: false).Contains(packData.EventCGName))
					{
						packData.EventCGBundle = item;
						break;
					}
				}
				Singleton<Game>.Instance.CharaEventShuffle();
				Singleton<Character>.Instance.DeleteCharaAll();
				game.heroineList.Clear();
				game.player = null;
				Open();
			});
			return;
		}
		if (game.appendSaveData.AppendTutorialNo == 5)
		{
			Observable.NextFrame().Subscribe(delegate
			{
				ADVManager instance = Singleton<ADVManager>.Instance;
				instance.advDelivery.Set("0", -20, 2);
				game.mapNo = 17;
				BaseMap.Change(game.mapNo, FadeCanvas.Fade.None);
				packData.SetParam(new IParams[1]);
				packData.MapName = BaseMap.ConvertMapName(game.mapNo);
				packData.EventCGName = BaseMap.ConvertMapNameEnglish(game.mapNo) + "_Event" + instance.advDelivery.adv_category.ToString("00");
				List<string> list = (from ab in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.AdvEventcgPath)
					orderby ab descending
					select ab).ToList();
				packData.EventCGBundle = string.Empty;
				foreach (string item2 in list)
				{
					if (GameSystem.IsPathAdd50(item2) && AssetBundleCheck.GetAllAssetName(item2, _WithExtension: false).Contains(packData.EventCGName))
					{
						packData.EventCGBundle = item2;
						break;
					}
				}
				Singleton<Game>.Instance.CharaEventShuffle();
				Singleton<Character>.Instance.DeleteCharaAll();
				game.heroineList.Clear();
				game.player = null;
				game.appendSaveData.AppendTutorialNo = 6;
				Open();
			});
			return;
		}
		if (game.appendSaveData.IsAppendStart == 1)
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "Home",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
			return;
		}
		if (save.TutorialNo == -1)
		{
			if (packData.parameterSetNo0 == 0)
			{
				GlobalHS2Calc.EndOfDayParameter(0);
			}
			else
			{
				GlobalHS2Calc.EndOfDayParameter((!game.heroineList.Any() || game.heroineList[0] == null || !game.heroineList[0].isConcierge) ? 1 : 2);
			}
			GlobalHS2Calc.EscapeCharaEventSet(save.selectGroup);
		}
		Singleton<Game>.Instance.CharaEventShuffle();
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "Home",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	private void GotoHScene()
	{
		Game instance = Singleton<Game>.Instance;
		HSceneManager instance2 = Singleton<HSceneManager>.Instance;
		if (instance.eventNo == 3 || instance.eventNo == 4 || instance.eventNo == 8 || instance.eventNo == 9 || instance.eventNo == 28 || instance.eventNo == 29)
		{
			instance.mapNo = 52;
		}
		else if (instance.eventNo == 5 || instance.eventNo == 10 || (instance.eventNo == 30 && packData.peepKind != 5))
		{
			instance.mapNo = 51;
		}
		else if (instance.eventNo == 6 || instance.eventNo == 11 || (instance.eventNo == 31 && packData.peepKind != 5))
		{
			instance.mapNo = 50;
		}
		instance2.mapID = instance.mapNo;
		instance2.player = instance.player.chaCtrl;
		instance2.females = new ChaControl[2]
		{
			(instance.heroineList.Count > 0) ? instance.heroineList[0].chaCtrl : null,
			(instance.heroineList.Count <= 1) ? null : instance.heroineList[1]?.chaCtrl
		};
		instance2.player = instance.player.chaCtrl;
		PlayerCharaSaveInfo secondPlayerChara = instance.saveData.secondPlayerChara;
		instance2.pngMaleSecond = secondPlayerChara.FileName;
		instance2.bFutanariSecond = secondPlayerChara.Futanari;
		if (instance.heroineList.Count > 0 && instance.heroineList[0] != null)
		{
			instance.saveData.BeforeFemaleName = instance.heroineList[0].chaFile.charaFileName;
		}
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "HScene",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	private void GotoLobby()
	{
		foreach (Heroine heroine in Singleton<Game>.Instance.heroineList)
		{
			if (heroine != null && !(heroine.chaCtrl == null))
			{
				heroine.chaCtrl.ChangeNowCoordinate(reload: true);
			}
		}
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "LobbyScene",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	private void GoToSpecialRoom()
	{
		Game instance = Singleton<Game>.Instance;
		_ = instance.appendSaveData.AppendTutorialNo;
		_ = -1;
		instance.mapNo = 17;
		SpecialTreatmentRoomScene.startCanvas = 0;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "SpecialTreatmentRoom",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	private void Achievement()
	{
		Game instance = Singleton<Game>.Instance;
		int[] source = new int[9] { 3, 4, 5, 6, 7, 28, 29, 30, 31 };
		int[] source2 = new int[2] { 2, 3 };
		if (source.Contains(instance.eventNo) && source2.Contains(instance.peepKind))
		{
			SaveData.SetAchievementAchieve(8);
		}
		if (new int[7] { 8, 9, 10, 11, 12, 13, 14 }.Contains(instance.eventNo) && packData.isHScene)
		{
			SaveData.SetAchievementAchieve(9);
		}
		if (new int[3] { 17, 18, 19 }.Contains(instance.eventNo) && packData.isAchievementRelease)
		{
			SaveData.SetAchievementAchieve(10);
		}
		if (instance.eventNo == 15 && packData.isHScene)
		{
			SaveData.SetAchievementAchieve(11);
		}
	}

	private void AddAppendEventNo()
	{
		Game instance = Singleton<Game>.Instance;
		AppendSaveData appendSaveData = instance.appendSaveData;
		if (!packData.isHome)
		{
			return;
		}
		if (MathfEx.IsRange(50, instance.eventNo, 55, isEqual: true))
		{
			if (!appendSaveData.appendEvents.Contains(instance.eventNo))
			{
				appendSaveData.appendEvents.Add(instance.eventNo);
				int[] array = new int[6] { 18, 19, 21, 20, 23, 22 };
				appendSaveData.mapReleases.Add(array[instance.eventNo - 50]);
			}
		}
		else if (instance.eventNo == 56)
		{
			appendSaveData.SitriSelectCount = Mathf.Min(appendSaveData.SitriSelectCount + 1, 5);
		}
		else if (instance.eventNo == 58 && !appendSaveData.IsFurSitri3P)
		{
			appendSaveData.IsFurSitri3P = true;
		}
	}
}
