using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADV;
using AIChara;
using Actor;
using CameraEffector;
using CharaCustom;
using HS2;
using Illusion.Anime;
using Illusion.Component.UI;
using Illusion.CustomAttributes;
using Illusion.Extensions;
using Illusion.Game;
using IllusionUtility.GetUtility;
using Tutorial2D;
using UIAnimatorCore;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Manager;

public class HomeSceneManager : Singleton<HomeSceneManager>
{
	public enum HomeMode
	{
		Home,
		GoToRoom,
		CharaEdit,
		CallConcierge,
		MapSelect
	}

	public enum ConciergeUIModeKind
	{
		Main,
		Achievement
	}

	public enum CameraPositionKind
	{
		Start,
		GoToRoom,
		CharaEdit,
		CallConcierge,
		Concierge,
		Config,
		Sleep
	}

	private class PackData : CharaPackData
	{
		public int drawTutorial2D
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("drawTutorial2D", out var value))
				{
					return (int)value.o;
				}
				return -1;
			}
		}

		public override List<Program.Transfer> Create()
		{
			return base.Create();
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

	[Serializable]
	public class SelectGroupUI
	{
		public Text text;

		public SpriteChangeCtrl sccBack;

		public SpriteChangeCtrl sccMark;
	}

	[Serializable]
	public class NoticeUI
	{
		public UIAnimator uiAnimator;

		public SpriteChangeCtrl scc;

		public Text txt;
	}

	[Serializable]
	public class NoticeInfo
	{
		public int mark;

		public string str = string.Empty;
	}

	[SerializeField]
	private OpenData openData = new OpenData();

	private BoolReactiveProperty isNowADV = new BoolReactiveProperty(initialValue: false);

	[SerializeField]
	private CanvasGroup _cgMain;

	[SerializeField]
	private List<CanvasGroup> _cgModes = new List<CanvasGroup>();

	[SerializeField]
	private List<CanvasGroup> _cgConciergeModes = new List<CanvasGroup>();

	[SerializeField]
	private CanvasGroup _cgAchievement;

	[SerializeField]
	private SelectGroupUI selectGroupUI;

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private CrossFade crossFade;

	[SerializeField]
	private int conciergeButtonSelectCountMax = 50;

	[SerializeField]
	private MapSelectUI _mapSelectUI;

	[SerializeField]
	private ConciergeMenuUI _conciergeMenuUI;

	[SerializeField]
	private CharaEditUI _charaEditUI;

	[SerializeField]
	private HomeUI _homeUI;

	[SerializeField]
	private LeaveTheRoomUI _leaveTheRoomUI;

	[SerializeField]
	private ConfigEffectorWet configEffector;

	[SerializeField]
	private Transform camTrans;

	[SerializeField]
	private Transform transDOFTarget;

	[SerializeField]
	private NoticeUI noticeUI;

	[SerializeField]
	[Label("報告画像の表示時間")]
	private float noticeWaitTime = 2f;

	[SerializeField]
	private ObjectCategoryBehaviour _ocbTutorial;

	private int selectCount;

	private bool isMove;

	[SerializeField]
	private float moveSpeed = 0.5f;

	public List<string>[] roomList = new List<string>[5];

	private Controller ctrl;

	private string[][] txHomeSelectGroupString = new string[5][]
	{
		new string[5] { "グループ１が設定中", "1 Selet a Group", "1 Selet a Group", "1 Selet a Group", "" },
		new string[5] { "グループ２が設定中", "2 Selet a Group", "2 Selet a Group", "2 Selet a Group", "" },
		new string[5] { "グループ３が設定中", "3 Selet a Group", "3 Selet a Group", "3 Selet a Group", "" },
		new string[5] { "グループ４が設定中", "4 Selet a Group", "4 Selet a Group", "4 Selet a Group", "" },
		new string[5] { "グループ５が設定中", "5 Selet a Group", "5 Selet a Group", "5 Selet a Group", "" }
	};

	private Dictionary<int, HomeCameraInfo.Param> dicCameraInfo = new Dictionary<int, HomeCameraInfo.Param>();

	private HomeConciergeInfoData homeConciergeInfoData;

	private HomeSceneInfoData homeSceneInfoData;

	private List<NoticeInfo> noticeInfos = new List<NoticeInfo>();

	private bool isDoNotice;

	private Animator animDoor;

	private Animator animDesk;

	private Animator animMenu;

	private PackData packData { get; set; }

	public bool IsADV => isNowADV.Value;

	public CanvasGroup CGMain => _cgMain;

	public List<CanvasGroup> CGModes => _cgModes;

	public List<CanvasGroup> CGConciergeModes => _cgConciergeModes;

	public CanvasGroup CGAchievement => _cgAchievement;

	public int ConciergeButtonSelectCountMax => conciergeButtonSelectCountMax;

	public MapSelectUI MapSelectUI => _mapSelectUI;

	public ConciergeMenuUI ConciergeMenuUI => _conciergeMenuUI;

	public CharaEditUI CharaEditUI => _charaEditUI;

	public HomeUI HomeUI => _homeUI;

	public LeaveTheRoomUI LeaveTheRoomUI => _leaveTheRoomUI;

	public ObjectCategoryBehaviour OCBTutorial => _ocbTutorial;

	public int HelpPage { get; set; }

	public bool IsMove
	{
		get
		{
			return isMove;
		}
		set
		{
			isMove = value;
		}
	}

	public ChaControl ConciergeChaCtrl { get; private set; }

	public Heroine ConciergeHeroine { get; private set; }

	public HomeConciergeInfoData HomeConciergeInfoData => homeConciergeInfoData;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<AssetBundleManager>.IsInstance());
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		Singleton<Character>.Instance.DeleteCharaAll();
		Singleton<Character>.Instance.EndLoadAssetBundle();
		Singleton<Game>.Instance.heroineList.Clear();
		Singleton<HSceneManager>.Instance.player = null;
		Singleton<HSceneManager>.Instance.females[0] = null;
		Singleton<HSceneManager>.Instance.females[1] = null;
		foreach (HomeCameraInfo item in GlobalMethod.LoadAllFolder<HomeCameraInfo>(AssetBundleNames.HomeListPath, "homecamerainfo", null, _subdirCheck: true))
		{
			foreach (HomeCameraInfo.Param item2 in item.param)
			{
				dicCameraInfo[item2.id] = new HomeCameraInfo.Param(item2);
			}
		}
		homeConciergeInfoData = GlobalMethod.LoadAllFolderInOneFile<HomeConciergeInfoData>(AssetBundleNames.HomePrefabsPath, "homeconciergeinfodata", null, _subdirCheck: true);
		yield return StartCoroutine(LoadConciergeBody());
		ConciergeChaCtrl.visibleAll = false;
		isNowADV.Subscribe(delegate(bool _isADV)
		{
			CGMain.blocksRaycasts = !_isADV;
		});
		homeSceneInfoData = GlobalMethod.LoadAllFolderInOneFile<HomeSceneInfoData>(AssetBundleNames.HomePrefabsPath, "HomeSceneInfoData", null, _subdirCheck: true);
		Sound.Listener = cam.transform;
		HelpPage = 0;
		(from _ in this.UpdateAsObservable()
			where isMove
			select _).Subscribe(delegate
		{
			if ((bool)camTrans)
			{
				camTrans.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
			}
		});
		(from _ in this.UpdateAsObservable()
			where !Scene.IsFadeNow
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog || o is global::Tutorial2D.Tutorial2D)
			select _).Subscribe(delegate
		{
			NoticeProc();
		});
	}

	private void OnDestroy()
	{
		if ((bool)ConciergeChaCtrl)
		{
			ConciergeChaCtrl.animBody.runtimeAnimatorController = null;
		}
	}

	private IEnumerator LoadConciergeBody()
	{
		ConciergeChaCtrl = Singleton<Character>.Instance.GetChara(-1);
		if (ConciergeChaCtrl == null)
		{
			ConciergeChaCtrl = Singleton<Character>.Instance.CreateChara(1, Scene.commonSpace, -1);
		}
		Singleton<Character>.Instance.LoadConciergeCharaFile(ConciergeChaCtrl);
		ConciergeChaCtrl.ChangeNowCoordinate();
		ConciergeHeroine = new Heroine(ConciergeChaCtrl.chaFile, isRandomize: false)
		{
			fixCharaID = -1
		};
		ConciergeHeroine.SetRoot(ConciergeChaCtrl.gameObject);
		ConciergeChaCtrl.releaseCustomInputTexture = false;
		ConciergeChaCtrl.Load();
		ConciergeChaCtrl.ChangeEyebrowPtn(0);
		ConciergeChaCtrl.ChangeEyesPtn(0);
		ConciergeChaCtrl.ChangeMouthPtn(0);
		ConciergeChaCtrl.ChangeLookEyesPtn(0);
		ConciergeChaCtrl.ChangeLookNeckPtn(3);
		ctrl = new Controller(ConciergeChaCtrl);
		ConciergeHeroine.param.Bind(new global::Actor.Actor(ConciergeHeroine));
		SetConciergePosition();
		yield return null;
	}

	public void GetMapAnimationObject()
	{
		GameObject mapRoot = BaseMap.mapRoot;
		if (!(mapRoot == null))
		{
			Transform transform = mapRoot.transform.FindLoop(homeSceneInfoData.doorObjName);
			if ((bool)transform)
			{
				animDoor = transform.GetComponent<Animator>();
			}
			transform = mapRoot.transform.FindLoop(homeSceneInfoData.deskObjName);
			if ((bool)transform)
			{
				animDesk = transform.GetComponent<Animator>();
			}
			transform = mapRoot.transform.FindLoop(homeSceneInfoData.menuObjName);
			if ((bool)transform)
			{
				animMenu = transform.GetComponent<Animator>();
			}
		}
	}

	public void AnimationDoor(bool _isOpen)
	{
		if (!(animDoor == null))
		{
			animDoor.Play(_isOpen ? "open" : "close_Idle", 0, 0f);
		}
	}

	public void AnimationDesk(bool _isOpen)
	{
		if (!(animDesk == null))
		{
			animDesk.Play(_isOpen ? "open" : "close_Idle", 0, 0f);
		}
	}

	public bool IsAnimationDesk()
	{
		if (animDesk == null)
		{
			return false;
		}
		return animDesk.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;
	}

	public void AnimationMenu(bool _isOpen)
	{
		if (!(animMenu == null))
		{
			animMenu.Play(_isOpen ? "open" : "close_Idle", 0, 0f);
		}
	}

	public void SetModeCanvasGroup(int _index)
	{
		foreach (var (canvasGroup, num) in _cgModes.ToForEachTuples())
		{
			canvasGroup.Enable(num == _index, isUseInteractable: false);
		}
	}

	public void SetCociergeModeCanvasGroup(int _index)
	{
		foreach (var (canvasGroup, num) in _cgConciergeModes.ToForEachTuples())
		{
			canvasGroup.Enable(num == _index, isUseInteractable: false);
		}
	}

	public void SetSelectGroupText(int _group)
	{
		bool flag = _group >= 0;
		selectGroupUI.text.transform.parent.gameObject.SetActiveIfDifferent(flag);
		if (flag)
		{
			selectGroupUI.text.text = txHomeSelectGroupString[_group][Singleton<GameSystem>.Instance.languageInt];
		}
		selectGroupUI.sccBack.ChangeValue(_group);
		selectGroupUI.sccMark.ChangeValue(_group);
	}

	public void SetCameraPosition(int _id, float _t = 0.5f)
	{
		if (!dicCameraInfo.TryGetValue(_id, out var value))
		{
			return;
		}
		if (!value.isTransformInfoData)
		{
			TransformInfoData transformInfoData = CommonLib.LoadAsset<TransformInfoData>(value.camBundle, value.camFile);
			if ((bool)transformInfoData && (bool)cam)
			{
				transformInfoData.Reflect(cam.transform);
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(value.camBundle, isUnloadForceRefCount: true);
			}
		}
		else
		{
			TransformCompositeInfoData transformCompositeInfoData = CommonLib.LoadAsset<TransformCompositeInfoData>(value.camBundle, value.camFile);
			if ((bool)transformCompositeInfoData && (bool)cam)
			{
				transformCompositeInfoData.Reflect(cam.transform, _t);
			}
			if ((bool)transformCompositeInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(value.camBundle, isUnloadForceRefCount: true);
			}
		}
		TransformInfoData transformInfoData2 = CommonLib.LoadAsset<TransformInfoData>(value.dofBundle, value.dofFile);
		if ((bool)transformInfoData2 && (bool)configEffector)
		{
			transformInfoData2.Reflect(configEffector.dof.focalTransform);
		}
		if ((bool)transformInfoData2)
		{
			AssetBundleManager.UnloadAssetBundle(value.camBundle, isUnloadForceRefCount: true);
		}
	}

	public void StartFade(bool _isForce = false)
	{
		if (!(crossFade == null) && (crossFade.isEnd || _isForce))
		{
			crossFade.FadeStart();
		}
	}

	public void SetConciergePosition()
	{
		homeConciergeInfoData.transformInfoData.Reflect(ConciergeChaCtrl.transform);
		ConciergeChaCtrl.resetDynamicBoneAll = true;
	}

	public bool ConciergeAnimationPlay(int _id, bool _isSameCheck = true)
	{
		if (ConciergeChaCtrl == null)
		{
			return false;
		}
		ctrl.PlayID(homeConciergeInfoData.animIDs[_id]);
		return true;
	}

	public void SelectCountClear()
	{
		selectCount = 0;
		Singleton<Game>.Instance.isConciergeAngry = false;
	}

	public void AddSelectCount()
	{
		selectCount = Mathf.Min(conciergeButtonSelectCountMax + 1, selectCount + 1);
	}

	public void SetSelectCount(int _set)
	{
		selectCount = _set;
	}

	public bool IsAngry()
	{
		return selectCount >= conciergeButtonSelectCountMax;
	}

	public bool IsAngryCount()
	{
		return selectCount == conciergeButtonSelectCountMax;
	}

	public async void OpenADV(string _bundle, string _asset, int _kind, Action _onEnd = null)
	{
		await Setup.LoadAsync(base.transform);
		isNowADV.Value = true;
		Game game = Singleton<Game>.Instance;
		int kind = _kind;
		packData = new PackData();
		packData.SetCommandData(game.saveData);
		packData.SetParam(ConciergeHeroine);
		packData.isParent = true;
		openData.bundle = _bundle;
		openData.asset = _asset;
		packData.onComplete = delegate
		{
			isNowADV.Value = false;
			StartFade();
			if (kind == 1)
			{
				configEffector.dof.focalTransform = transDOFTarget;
			}
			Controller.Table.Get(ConciergeChaCtrl).itemHandler.DisableItems();
			_onEnd?.Invoke();
		};
		Setup.Open(openData, packData, _isCameraPosDontMove: true);
	}

	public void CharaEventSet()
	{
		Game game = Singleton<Game>.Instance;
		SaveData saveData = game.saveData;
		bool[] array = new bool[saveData.roomList.Length];
		for (int i = 0; i < roomList.Length; i++)
		{
			array[i] = !roomList[i].SequenceEqual(saveData.roomList[i]);
		}
		if (!array.Any((bool match) => match))
		{
			return;
		}
		for (int num = 0; num < roomList.Length; num++)
		{
			if (!array[num])
			{
				continue;
			}
			foreach (string s in roomList[num].Except(saveData.roomList[num]).ToList())
			{
				KeyValuePair<string, Game.EventCharaInfo> ans = game.tableHomeEvents[num].FirstOrDefault((KeyValuePair<string, Game.EventCharaInfo> e) => e.Value.fileName == s);
				if (ans.Value != null)
				{
					game.tableHomeEvents[num].Remove(ans.Key);
				}
				ans = game.tableHomeEvents[num].FirstOrDefault((KeyValuePair<string, Game.EventCharaInfo> e) => e.Value.fileNamePartner == s);
				if (ans.Value != null)
				{
					List<int> useMap = (from h in game.tableHomeEvents[num]
						where h.Value.fileName != ans.Value.fileName
						select h.Value.mapID).ToList();
					GlobalHS2Calc.MapCharaSelectInfo mapCharaSelectInfo = GlobalHS2Calc.PlaceCharaOnTheMap(ans.Value.fileName, useMap);
					if (mapCharaSelectInfo.mapID != -1)
					{
						Game.EventCharaInfo value = ans.Value;
						value.mapID = mapCharaSelectInfo.mapID;
						value.main = -1;
						value.partner = -1;
						value.eventID = mapCharaSelectInfo.eventID;
						value.fileNamePartner = string.Empty;
					}
					else
					{
						game.tableHomeEvents[num].Remove(ans.Key);
					}
				}
				ans = game.tableLobbyEvents[num].FirstOrDefault((KeyValuePair<string, Game.EventCharaInfo> e) => e.Value.fileName == s);
				if (ans.Value != null)
				{
					game.tableLobbyEvents[num].Remove(ans.Key);
				}
			}
			foreach (string item in saveData.roomList[num].Except(roomList[num]).ToList())
			{
				int num2 = saveData.roomList[num].IndexOf(item);
				List<int> useMap2 = game.tableHomeEvents[num].Select((KeyValuePair<string, Game.EventCharaInfo> h) => h.Value.mapID).ToList();
				GlobalHS2Calc.MapCharaSelectInfo mapCharaSelectInfo2 = GlobalHS2Calc.PlaceCharaOnTheMap(item, useMap2);
				if (num2 != -1 && mapCharaSelectInfo2.mapID != -1)
				{
					game.tableHomeEvents[num].Add(item, new Game.EventCharaInfo
					{
						mapID = mapCharaSelectInfo2.mapID,
						eventID = mapCharaSelectInfo2.eventID,
						fileName = item
					});
				}
				ChaFileControl chaFileControl = new ChaFileControl();
				if (chaFileControl.LoadCharaFile(item, 1))
				{
					game.tableLobbyEvents[num].Add(item, new Game.EventCharaInfo
					{
						eventID = GlobalHS2Calc.GetEventNo(chaFileControl.gameinfo2, _isDesireCalc: true),
						fileName = item
					});
				}
			}
		}
	}

	public void CharaEventSetPoint(string _fileName)
	{
		Game game = Singleton<Game>.Instance;
		SaveData saveData = game.saveData;
		for (int i = 0; i < roomList.Length; i++)
		{
			KeyValuePair<string, Game.EventCharaInfo> ans = game.tableHomeEvents[i].FirstOrDefault((KeyValuePair<string, Game.EventCharaInfo> e) => e.Value.fileName == _fileName);
			if (ans.Value != null)
			{
				game.tableHomeEvents[i].Remove(ans.Key);
			}
			ans = game.tableHomeEvents[i].FirstOrDefault((KeyValuePair<string, Game.EventCharaInfo> e) => e.Value.fileNamePartner == _fileName);
			if (ans.Value != null)
			{
				List<int> useMap = (from h in game.tableHomeEvents[i]
					where h.Value.fileName != ans.Value.fileName
					select h.Value.mapID).ToList();
				GlobalHS2Calc.MapCharaSelectInfo mapCharaSelectInfo = GlobalHS2Calc.PlaceCharaOnTheMap(ans.Value.fileName, useMap);
				if (mapCharaSelectInfo.mapID != -1)
				{
					Game.EventCharaInfo value = ans.Value;
					value.mapID = mapCharaSelectInfo.mapID;
					value.main = -1;
					value.partner = -1;
					value.eventID = mapCharaSelectInfo.eventID;
					value.fileNamePartner = string.Empty;
				}
				else
				{
					game.tableHomeEvents[i].Remove(ans.Key);
				}
			}
			ans = game.tableLobbyEvents[i].FirstOrDefault((KeyValuePair<string, Game.EventCharaInfo> e) => e.Value.fileName == _fileName);
			if (ans.Value != null)
			{
				game.tableLobbyEvents[i].Remove(ans.Key);
			}
			int num = saveData.roomList[i].IndexOf(_fileName);
			GlobalHS2Calc.MapCharaSelectInfo mapCharaSelectInfo2 = GlobalHS2Calc.PlaceCharaOnTheMap(_useMap: game.tableHomeEvents[i].Select((KeyValuePair<string, Game.EventCharaInfo> h) => h.Value.mapID).ToList(), _fileName: _fileName);
			if (num != -1 && mapCharaSelectInfo2.mapID != -1)
			{
				game.tableHomeEvents[i].Add(_fileName, new Game.EventCharaInfo
				{
					mapID = mapCharaSelectInfo2.mapID,
					eventID = mapCharaSelectInfo2.eventID,
					fileName = _fileName
				});
			}
			ChaFileControl chaFileControl = new ChaFileControl();
			if (chaFileControl.LoadCharaFile(_fileName, 1))
			{
				game.tableLobbyEvents[i].Add(_fileName, new Game.EventCharaInfo
				{
					eventID = GlobalHS2Calc.GetEventNo(chaFileControl.gameinfo2, _isDesireCalc: true),
					fileName = _fileName
				});
			}
		}
	}

	public void NoticePreparation()
	{
		Game game = Singleton<Game>.Instance;
		SaveData saveData = game.saveData;
		if (!isDoNotice)
		{
			noticeInfos.Clear();
		}
		if (!saveData.escapeCharaName.IsNullOrEmpty())
		{
			noticeInfos.Add(new NoticeInfo
			{
				mark = 1,
				str = $"{saveData.escapeCharaName} が逃げ出しました"
			});
			saveData.escapeCharaName = string.Empty;
		}
		int languageInt = Singleton<GameSystem>.Instance.languageInt;
		foreach (int item in saveData.achievementAchieve)
		{
			noticeInfos.Add(new NoticeInfo
			{
				mark = 0,
				str = $"「{game.infoAchievementDic[item].title[languageInt]}」が達成されました"
			});
		}
		saveData.achievementAchieve.Clear();
		AppendSaveData appendSaveData = game.appendSaveData;
		foreach (int mapRelease in appendSaveData.mapReleases)
		{
			noticeInfos.Add(new NoticeInfo
			{
				mark = 0,
				str = $"マップ選択に「{BaseMap.infoTable[mapRelease].MapNames[0]}」が解放されました"
			});
		}
		appendSaveData.mapReleases.Clear();
	}

	public void SpecialRoomNoticePreparation()
	{
		noticeInfos.Add(new NoticeInfo
		{
			mark = 0,
			str = "特別待遇が受けられるようになりました"
		});
	}

	private void NoticeProc()
	{
		if (noticeInfos.Any() && !isDoNotice)
		{
			StartCoroutine(NoticeCoroutine());
		}
	}

	private IEnumerator NoticeCoroutine()
	{
		isDoNotice = true;
		int i = 0;
		while (i < noticeInfos.Count)
		{
			yield return new WaitUntil(() => !isNowADV.Value);
			noticeUI.scc.ChangeValue(noticeInfos[i].mark);
			noticeUI.txt.text = noticeInfos[i].str;
			noticeUI.uiAnimator.PlayAnimation(AnimSetupType.Intro);
			Utils.Sound.Play(SystemSE.achievement);
			yield return new WaitUntil(() => !noticeUI.uiAnimator.IsPlaying);
			yield return new WaitForSeconds(noticeWaitTime);
			noticeUI.uiAnimator.PlayAnimation(AnimSetupType.Outro);
			yield return new WaitUntil(() => !noticeUI.uiAnimator.IsPlaying);
			int num = i + 1;
			i = num;
		}
		isDoNotice = false;
		noticeInfos.Clear();
	}
}
