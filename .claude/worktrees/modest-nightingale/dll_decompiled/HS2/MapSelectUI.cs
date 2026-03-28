using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Config;
using Illusion;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using SceneAssist;
using Tutorial2D;
using UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HS2;

public class MapSelectUI : MonoBehaviour
{
	[Serializable]
	private class MapSelectUIInfo
	{
		public GameObject parentObj;

		public GameObject objLock;

		public Button bt;

		public Image imgBtn;

		public Image imgEvent;

		public Image[] imgCollisions;

		public UIObjectSlideOnCursor slideOnCursor;

		public Text text;

		public Text text_E;

		public PointerEnterExitAction enterExitAction;
	}

	[Serializable]
	private class MapSelectThumbnailUI
	{
		public GameObject parentObj;

		public RawImage rawimg;

		public Text text;
	}

	private const int MAX_MAP_SELECT_COUNT = 5;

	[SerializeField]
	private Button btnBack;

	[SerializeField]
	private MapSelectUIInfo[] uiMapSelects = new MapSelectUIInfo[5];

	[SerializeField]
	private Button btnUp;

	[SerializeField]
	private Button btnDown;

	[SerializeField]
	private MapSelectThumbnailUI mapThumbnailUI;

	[SerializeField]
	private MapSelectThumbnailUI firstCharaThumbnailUI;

	[SerializeField]
	private MapSelectThumbnailUI secondCharaThumbnailUI;

	[SerializeField]
	private Texture2D texAnything;

	[SerializeField]
	private Sprite spNoneEvent;

	private MapInfo.Param[] scrollData;

	private List<GlobalHS2Calc.MapCharaSelectInfo> lstMapChara = new List<GlobalHS2Calc.MapCharaSelectInfo>();

	private int startIndex;

	private GlobalHS2Calc.MapCharaSelectInfo mapCharaInfo;

	private string firstCharaFile = string.Empty;

	private string secondCharaFile = string.Empty;

	private ChaFileControl chaFile = new ChaFileControl();

	private bool isMapLoadEnd = true;

	private readonly string[] strGotoMap = new string[6] { "この場所に向かいますか？", "Going to this place?", "Going to this place?", "Going to this place?", "Going to this place?", "" };

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			StartCoroutine(Back());
		});
		btnUp.OnClickAsObservable().Subscribe(delegate
		{
			Illusion.Game.Utils.Sound.Play(SystemSE.ok_s);
			StartCoroutine(ResetMapScroll(startIndex - 1, _async: false));
		});
		btnDown.OnClickAsObservable().Subscribe(delegate
		{
			Illusion.Game.Utils.Sound.Play(SystemSE.ok_s);
			StartCoroutine(ResetMapScroll(startIndex + 1, _async: false));
		});
		List<Button> list = new List<Button>();
		list.Add(btnBack);
		list.Add(btnUp);
		list.Add(btnDown);
		list.ForEach(delegate(Button bt)
		{
			bt.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Illusion.Game.Utils.Sound.Play(SystemSE.sel);
			});
		});
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !Scene.IsFadeNow
			where Singleton<Game>.Instance.saveData.TutorialNo == -1 || Singleton<Game>.Instance.saveData.TutorialNo != 14
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			where hm.CGMain.interactable
			where hm.CGModes[4].alpha >= 0.9f
			select _).Subscribe(delegate
		{
			StartCoroutine(Back());
		});
		mapThumbnailUI.parentObj.SetActiveIfDifferent(active: false);
		firstCharaThumbnailUI.parentObj.SetActiveIfDifferent(active: false);
		secondCharaThumbnailUI.parentObj.SetActiveIfDifferent(active: false);
		base.enabled = true;
	}

	private IEnumerator Back()
	{
		yield return null;
		Illusion.Game.Utils.Sound.Play(SystemSE.cancel);
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.SetModeCanvasGroup(1);
		instance.HelpPage = 0;
	}

	public void InitList()
	{
		Game instance = Singleton<Game>.Instance;
		lstMapChara = instance.tableHomeEvents[instance.saveData.selectGroup].Select((KeyValuePair<string, Game.EventCharaInfo> h) => new GlobalHS2Calc.MapCharaSelectInfo
		{
			mapID = h.Value.mapID,
			eventID = h.Value.eventID,
			lstChara = new List<(string, int)> { (h.Value.fileName, h.Value.main) }
		}).ToList();
		foreach (GlobalHS2Calc.MapCharaSelectInfo mcsi in lstMapChara)
		{
			GlobalHS2Calc.MapCharaSelectInfo mapCharaSelectInfo = lstMapChara.FirstOrDefault((GlobalHS2Calc.MapCharaSelectInfo m) => m.mapID == mcsi.mapID && m.eventID == mcsi.eventID && m != mcsi && m.lstChara[0].Item2 == 1);
			if (mapCharaSelectInfo != null)
			{
				mcsi.lstChara.Add(mapCharaSelectInfo.lstChara[0]);
			}
		}
		GlobalHS2Calc.MapCharaSelectInfo[] array = lstMapChara.ToArray();
		foreach (GlobalHS2Calc.MapCharaSelectInfo mapCharaSelectInfo2 in array)
		{
			if (mapCharaSelectInfo2.lstChara[0].Item2 == 1)
			{
				lstMapChara.Remove(mapCharaSelectInfo2);
			}
		}
		foreach (GlobalHS2Calc.MapCharaSelectInfo item in lstMapChara)
		{
			item.lstChara = item.lstChara.Shuffle().ToList();
		}
		scrollData = BaseMap.infoTable.Values.Where((MapInfo.Param map) => map.Draw == 0 || map.Draw == 2).ToArray();
		List<GlobalHS2Calc.MapCharaSelectInfo> list = lstMapChara.Where((GlobalHS2Calc.MapCharaSelectInfo info) => Game.DesireEventIDs.Contains(info.eventID)).ToList();
		instance.tableDesireCharas.Clear();
		foreach (GlobalHS2Calc.MapCharaSelectInfo item2 in list)
		{
			foreach (var item3 in item2.lstChara)
			{
				instance.tableDesireCharas.Add(item3.Item1, item2.eventID);
			}
		}
		StartCoroutine(ResetMapScroll(0, _async: false, _forceSet: true));
		bool interactable = 5 < scrollData.Length;
		btnUp.interactable = interactable;
		btnDown.interactable = interactable;
		MapSelecCursorExit();
	}

	private IEnumerator ResetMapScroll(int _startIndex, bool _async = true, bool _forceSet = false)
	{
		isMapLoadEnd = false;
		int num = scrollData.Length;
		int num2 = startIndex;
		if (_startIndex < 0)
		{
			_startIndex = 0;
		}
		else if (_startIndex + 5 > num)
		{
			int num3 = _startIndex + 5 - num;
			_startIndex = Mathf.Max(0, _startIndex - num3);
		}
		startIndex = _startIndex;
		if (startIndex == num2 && !_forceSet)
		{
			isMapLoadEnd = true;
			yield break;
		}
		EventSystem.current.SetSelectedGameObject(null);
		SetMapSelectUIRaycast(_raycastTarget: false);
		if (_async)
		{
			yield return null;
		}
		new HashSet<int>(lstMapChara.Select((GlobalHS2Calc.MapCharaSelectInfo info) => info.mapID));
		Game instance = Singleton<Game>.Instance;
		for (int num4 = 0; num4 < 5; num4++)
		{
			int num5 = _startIndex + num4;
			MapInfo.Param mapInfo = scrollData[num5];
			uiMapSelects[num4].text.text = mapInfo.MapNames[0];
			uiMapSelects[num4].text_E.text = mapInfo.MapNames[1];
			GlobalHS2Calc.MapCharaSelectInfo mapCharaSelectInfo = lstMapChara.FirstOrDefault((GlobalHS2Calc.MapCharaSelectInfo l) => l.mapID == mapInfo.No);
			int num6 = -1;
			if (mapCharaSelectInfo == null)
			{
				if (mapInfo.No == 2 && SaveData.IsAchievementExchangeRelease(13))
				{
					num6 = 33;
				}
			}
			else
			{
				num6 = mapCharaSelectInfo.eventID;
			}
			bool flag = num6 != -1;
			if (flag)
			{
				EventContentInfoData.Param param = instance.infoEventContentDic[num6];
				Illusion.Game.Utils.Bundle.LoadSprite(param.bundleEventSprite, param.fileEventSprite, uiMapSelects[num4].imgEvent, isTexSize: false);
			}
			else
			{
				uiMapSelects[num4].imgEvent.sprite = spNoneEvent;
			}
			int[] array = GlobalHS2Calc.ExcludeAchievementMap(new int[1] { mapInfo.No });
			uiMapSelects[num4].objLock.SetActiveIfDifferent(array.Length == 0);
			Image[] imgCollisions = uiMapSelects[num4].imgCollisions;
			for (int num7 = 0; num7 < imgCollisions.Length; num7++)
			{
				imgCollisions[num7].raycastTarget = true;
			}
			uiMapSelects[num4].bt.interactable = flag;
			uiMapSelects[num4].bt.onClick.RemoveAllListeners();
			char[] array2 = (from l in Illusion.Utils.uGUI.HitList(Input.mousePosition)
				where l.gameObject.name.ContainsAny("collision", "imgBase")
				select l).FirstOrDefault().gameObject?.name.ToCharArray();
			int result = -1;
			if (array2 != null && array2.Length != 0 && !int.TryParse(array2[array2.Length - 1].ToString(), out result))
			{
				result = -1;
			}
			if (!flag)
			{
				uiMapSelects[num4].slideOnCursor.SetSlideEnable(_slide: false);
			}
			else if (result == num4)
			{
				uiMapSelects[num4].slideOnCursor.SetSlideEnable(_slide: true);
			}
			if (flag)
			{
				uiMapSelects[num4].bt.onClick.AddListener(delegate
				{
					MapSelecButtonClick(mapInfo);
				});
			}
			PointerEnterExitAction enterExitAction = uiMapSelects[num4].enterExitAction;
			int sel = num4;
			enterExitAction.listActionEnter.Clear();
			if (flag)
			{
				enterExitAction.listActionEnter.Add(delegate
				{
					MapSelecCursorEnter(mapInfo, sel);
				});
			}
			enterExitAction.listActionExit.Clear();
		}
		isMapLoadEnd = true;
	}

	private void MapSelecButtonClick(MapInfo.Param _info)
	{
		Illusion.Game.Utils.Sound.Play(SystemSE.ok_s);
		Game game = Singleton<Game>.Instance;
		SaveData save = game.saveData;
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strGotoMap[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Illusion.Game.Utils.Sound.Play(SystemSE.ok_l);
			SaveData.SetAchievementAchieve(2);
			game.heroineList.Clear();
			save.BeforeFemaleName = string.Empty;
			HomeScene.startCanvas = 0;
			HomeScene.startCharaEdit = -1;
			if (mapCharaInfo == null)
			{
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "FursRoom",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			}
			else if (mapCharaInfo.eventID == 2 || mapCharaInfo.eventID == 14)
			{
				Singleton<Character>.Instance.DeleteChara(Singleton<HomeSceneManager>.Instance.ConciergeChaCtrl);
				game.eventNo = mapCharaInfo.eventID;
				game.peepKind = -1;
				game.isConciergeAngry = false;
				Singleton<ADVManager>.Instance.advDelivery.Set("0", -100, mapCharaInfo.eventID);
				Singleton<Game>.Instance.mapNo = mapCharaInfo.mapID;
				Singleton<ADVManager>.Instance.filenames[0] = firstCharaFile;
				Singleton<ADVManager>.Instance.filenames[1] = secondCharaFile;
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "ADV",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			}
			else
			{
				Singleton<Character>.Instance.DeleteChara(Singleton<HomeSceneManager>.Instance.ConciergeChaCtrl);
				game.eventNo = mapCharaInfo.eventID;
				game.peepKind = -1;
				game.isConciergeAngry = false;
				Singleton<ADVManager>.Instance.advDelivery.Set("0", chaFile.parameter2.personality, mapCharaInfo.eventID);
				Singleton<Game>.Instance.mapNo = mapCharaInfo.mapID;
				Singleton<ADVManager>.Instance.filenames[0] = firstCharaFile;
				Singleton<ADVManager>.Instance.filenames[1] = "";
				Singleton<HSceneManager>.Instance.pngFemales[1] = "";
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "ADV",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			}
		};
		status.No = delegate
		{
			Illusion.Game.Utils.Sound.Play(SystemSE.cancel);
		};
		ConfirmDialog.Load();
	}

	private void MapSelecCursorEnter(MapInfo.Param _info, int _array)
	{
		Illusion.Game.Utils.Sound.Play(SystemSE.sel);
		Game instance = Singleton<Game>.Instance;
		mapThumbnailUI.parentObj.SetActiveIfDifferent(active: true);
		mapThumbnailUI.rawimg.texture = CommonLib.LoadAsset<Texture2D>(_info.ThumbnailBundle_L, _info.ThumbnailAsset_L, clone: false, _info.ThumbnailManifest_L);
		AssetBundleManager.UnloadAssetBundle(_info.ThumbnailBundle_L, isUnloadForceRefCount: true);
		mapCharaInfo = lstMapChara.Find((GlobalHS2Calc.MapCharaSelectInfo lst) => lst.mapID == _info.No);
		firstCharaThumbnailUI.parentObj.SetActiveIfDifferent(mapCharaInfo != null);
		secondCharaThumbnailUI.parentObj.SetActiveIfDifferent(active: false);
		if (mapCharaInfo == null)
		{
			return;
		}
		if (mapCharaInfo.eventID == 2 || mapCharaInfo.eventID == 14)
		{
			secondCharaThumbnailUI.parentObj.SetActiveIfDifferent(active: true);
			firstCharaFile = mapCharaInfo.lstChara.FirstOrDefault(((string, int) lc) => lc.Item2 == 0).Item1;
			secondCharaFile = mapCharaInfo.lstChara.FirstOrDefault(((string, int) lc) => lc.Item2 == 1).Item1;
			if (instance.infoEventContentDic[mapCharaInfo.eventID].draw == 1)
			{
				firstCharaThumbnailUI.rawimg.texture = texAnything;
				secondCharaThumbnailUI.rawimg.texture = texAnything;
				firstCharaThumbnailUI.text.text = "???";
				secondCharaThumbnailUI.text.text = "???";
			}
			else
			{
				ChaFileControl chaFileControl = new ChaFileControl();
				firstCharaThumbnailUI.rawimg.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(UserData.Path + "chara/female/" + firstCharaFile + ".png"));
				chaFileControl.LoadCharaFile(firstCharaFile, 1);
				firstCharaThumbnailUI.text.text = chaFileControl.parameter.fullname;
				secondCharaThumbnailUI.rawimg.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(UserData.Path + "chara/female/" + secondCharaFile + ".png"));
				chaFileControl.LoadCharaFile(secondCharaFile, 1);
				secondCharaThumbnailUI.text.text = chaFileControl.parameter.fullname;
			}
		}
		else
		{
			firstCharaFile = mapCharaInfo.lstChara.FirstOrDefault().Item1;
			chaFile.LoadCharaFile(firstCharaFile, 1);
			if (instance.infoEventContentDic[mapCharaInfo.eventID].draw == 1)
			{
				firstCharaThumbnailUI.rawimg.texture = texAnything;
				firstCharaThumbnailUI.text.text = "???";
			}
			else
			{
				firstCharaThumbnailUI.rawimg.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(UserData.Path + "chara/female/" + firstCharaFile + ".png"));
				firstCharaThumbnailUI.text.text = chaFile.parameter.fullname;
			}
		}
	}

	private void MapSelecCursorExit()
	{
		mapThumbnailUI.parentObj.SetActiveIfDifferent(active: false);
		firstCharaThumbnailUI.parentObj.SetActiveIfDifferent(active: false);
		secondCharaThumbnailUI.parentObj.SetActiveIfDifferent(active: false);
	}

	public void OnScrollMapSelect(BaseEventData _data)
	{
		if (isMapLoadEnd)
		{
			if (Input.mouseScrollDelta.y > 0f)
			{
				StartCoroutine(ResetMapScroll(startIndex - 1));
			}
			else if (Input.mouseScrollDelta.y < 0f)
			{
				StartCoroutine(ResetMapScroll(startIndex + 1));
			}
		}
	}

	private void SetMapSelectUIRaycast(bool _raycastTarget)
	{
		for (int i = 0; i < 5; i++)
		{
			Image[] imgCollisions = uiMapSelects[i].imgCollisions;
			for (int j = 0; j < imgCollisions.Length; j++)
			{
				imgCollisions[j].raycastTarget = _raycastTarget;
			}
		}
	}
}
