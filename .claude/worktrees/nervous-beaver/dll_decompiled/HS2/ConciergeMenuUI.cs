using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Actor;
using CharaCustom;
using Config;
using Illusion.Anime;
using Illusion.Component.UI;
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

public class ConciergeMenuUI : MonoBehaviour
{
	public enum ConciergeUICategory
	{
		Search,
		Achievement,
		Help,
		H,
		Custom,
		GotoSpecialRoom
	}

	[Serializable]
	public class MenuItemUI
	{
		public int id;

		public ToggleExtension tgl;

		public UIObjectSlideOnCursor uiSlide;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private ButtonExtension btnBack;

	[SerializeField]
	private MenuItemUI[] itemUIs;

	[SerializeField]
	private ConciergeAchievementUI conciergeAchievementUI;

	[SerializeField]
	private PointerEnterExitAction pointerEnterExitNotPlanCharacter;

	[SerializeField]
	private GameObject objNotPlanCharacter;

	private readonly string[] strHScene = new string[5] { "Hを開始しますか？", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?" };

	private readonly string[] strCustom = new string[6] { "キャラメイク画面に移動します\nよろしいですか？", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "" };

	private readonly string[] strSearch = new string[6] { "キャラ検索画面に移動します\nよろしいですか？", "Go to the character search screen.\nIs it OK?", "Go to the character search screen.\nIs it OK?", "Go to the character search screen.\nIs it OK?", "Go to the character search screen.\nIs it OK?", "" };

	private readonly string[] strGotoSPRoom = new string[6] { "特別待遇室に移動します\nよろしいですか？", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "" };

	public bool IsOneTimeStop { get; set; }

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		MenuItemUI[] array = itemUIs;
		foreach (MenuItemUI ui in array)
		{
			ui.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
			{
				if (_isOn)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.selectFontColor;
					});
					switch (ui.id)
					{
					case 0:
					{
						ConfirmDialog.Status status2 = ConfirmDialog.status;
						status2.Sentence = strSearch[Singleton<GameSystem>.Instance.languageInt];
						status2.Yes = delegate
						{
							Utils.Sound.Play(SystemSE.ok_s);
							StartCoroutine(SearchCoroutine());
						};
						status2.No = delegate
						{
							Utils.Sound.Play(SystemSE.cancel);
							DeSelectItemUI(itemUIs[0]);
						};
						ConfirmDialog.Load();
						break;
					}
					case 1:
						ui.uiSlide.IsSlideAlways = true;
						StartCoroutine(AchievementCoroutine());
						break;
					case 2:
						ui.uiSlide.IsSlideAlways = true;
						StartCoroutine(HelpCoroutine());
						break;
					case 3:
						ui.uiSlide.IsSlideAlways = true;
						GoToConciergeH();
						break;
					case 4:
					{
						ConfirmDialog.Status status = ConfirmDialog.status;
						status.Sentence = strCustom[Singleton<GameSystem>.Instance.languageInt];
						status.Yes = delegate
						{
							Utils.Sound.Play(SystemSE.ok_s);
							StartCoroutine(CustomCoroutine());
						};
						status.No = delegate
						{
							Utils.Sound.Play(SystemSE.cancel);
							DeSelectItemUI(itemUIs[4]);
						};
						ConfirmDialog.Load();
						break;
					}
					case 5:
						ui.uiSlide.IsSlideAlways = true;
						GoToSpecialRoom();
						break;
					}
					MenuItemUI[] array2 = itemUIs;
					foreach (MenuItemUI menuItemUI in array2)
					{
						if (menuItemUI != ui)
						{
							menuItemUI.tgl.isOn = false;
						}
					}
				}
				else
				{
					ui.uiSlide.IsSlideAlways = false;
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.defaultFontColor;
					});
				}
			});
			ui.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (ui.tgl.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
					if (IsOneTimeStop)
					{
						IsOneTimeStop = false;
					}
					else
					{
						switch (ui.id)
						{
						case 0:
							ButtonSelect(2, delegate
							{
								hm.HomeConciergeInfoData.SetSearchVoice(hm.ConciergeChaCtrl, hm.IsAngry());
							});
							break;
						case 1:
							ButtonSelect(2, delegate
							{
								hm.HomeConciergeInfoData.SetAchievementVoice(hm.ConciergeChaCtrl, hm.IsAngry());
							});
							break;
						case 2:
							ButtonSelect(2, delegate
							{
								hm.HomeConciergeInfoData.SetHelpVoice(hm.ConciergeChaCtrl, hm.IsAngry());
							});
							break;
						case 3:
							ButtonSelect(3, delegate
							{
								hm.HomeConciergeInfoData.SetHVoice(hm.ConciergeChaCtrl, hm.IsAngry());
							});
							break;
						case 4:
							ButtonSelect(2, delegate
							{
								hm.HomeConciergeInfoData.SetCustomVoice(hm.ConciergeChaCtrl, hm.IsAngry());
							});
							break;
						case 5:
							ButtonSelect(2, delegate
							{
								hm.HomeConciergeInfoData.SetSelectGotoSepcialRoom(hm.ConciergeChaCtrl, hm.IsAngry());
							});
							break;
						}
					}
				}
			});
		}
		if (GameSystem.isAdd50)
		{
			itemUIs[5].tgl.gameObject.SetActiveIfDifferent(active: true);
		}
		else
		{
			itemUIs[5].tgl.gameObject.SetActiveIfDifferent(active: false);
			itemUIs[5].texts.ForEach(delegate(Text t)
			{
				t.text = "";
			});
		}
		objNotPlanCharacter.SetActiveIfDifferent(active: false);
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Back());
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
		});
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where hm.CGMain.blocksRaycasts
			where !Scene.IsFadeNow
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			where hm.CGMain.interactable
			where hm.CGModes[3].alpha >= 0.9f
			where hm.CGAchievement.alpha < 0.1f
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Back());
		});
		base.enabled = true;
	}

	private IEnumerator BowCoroutine()
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.CGMain.interactable = false;
		instance.ConciergeAnimationPlay(1);
		instance.HomeConciergeInfoData.SetClickVoice(instance.ConciergeChaCtrl, instance.IsAngry());
		yield return new WaitUntil(() => !Voice.IsPlay());
	}

	private IEnumerator BowCutomCoroutine()
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.CGMain.interactable = false;
		instance.ConciergeAnimationPlay(1);
		instance.HomeConciergeInfoData.SetCustomClickVoice(instance.ConciergeChaCtrl, instance.IsAngry());
		yield return new WaitUntil(() => !Voice.IsPlay());
	}

	private IEnumerator SearchCoroutine()
	{
		yield return StartCoroutine(BowCoroutine());
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "CharaSearch",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
		HomeScene.startCanvas = 3;
		Singleton<HomeSceneManager>.Instance.CGMain.interactable = true;
		yield return null;
	}

	private IEnumerator AchievementCoroutine()
	{
		yield return StartCoroutine(BowCoroutine());
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.StartFade();
		instance.SetCociergeModeCanvasGroup(1);
		DeSelectItemUI(itemUIs[1]);
		conciergeAchievementUI.Init();
		instance.CGMain.interactable = true;
		yield return null;
	}

	private IEnumerator HelpCoroutine()
	{
		yield return StartCoroutine(BowCoroutine());
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		DeSelectItemUI(itemUIs[2]);
		SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = true;
		SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 0;
		SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.onEndFunc = delegate
		{
			EventSystem.current.SetSelectedGameObject(null);
			SetDefaultToggleTransition(-1);
		};
		global::Tutorial2D.Tutorial2D.Load();
		instance.CGMain.interactable = true;
		yield return null;
	}

	private IEnumerator CustomCoroutine()
	{
		yield return StartCoroutine(BowCutomCoroutine());
		global::CharaCustom.CharaCustom.modeNew = false;
		global::CharaCustom.CharaCustom.editCharaFileName = Singleton<Character>.Instance.conciergePath;
		global::CharaCustom.CharaCustom.modeSex = 1;
		global::CharaCustom.CharaCustom.isConcierge = 0;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "CharaCustom",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
		HomeScene.startCanvas = 3;
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.ConciergeChaCtrl.visibleAll = false;
		instance.CGMain.interactable = true;
		yield return null;
	}

	private IEnumerator ConciergeAngryCoroutine()
	{
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		hm.CGMain.interactable = false;
		hm.ConciergeAnimationPlay(0);
		hm.HomeConciergeInfoData.SetAngryVoice(hm.ConciergeChaCtrl);
		yield return new WaitUntil(() => !Voice.IsPlay());
		hm.CGMain.interactable = true;
		EventSystem.current.SetSelectedGameObject(null);
		SetDefaultToggleTransition(-1);
	}

	private IEnumerator Back()
	{
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		hm.CGMain.blocksRaycasts = false;
		hm.ConciergeAnimationPlay(1);
		hm.HomeConciergeInfoData.SetBackVoice(hm.ConciergeChaCtrl, hm.IsAngry());
		yield return new WaitUntil(() => !Voice.IsPlay());
		hm.SetCameraPosition(0);
		hm.StartFade();
		hm.ConciergeChaCtrl.visibleAll = false;
		Controller.Table.Get(hm.ConciergeChaCtrl).itemHandler.DisableItems();
		hm.SelectCountClear();
		Singleton<HomeSceneManager>.Instance.SetModeCanvasGroup(0);
		hm.CGMain.blocksRaycasts = true;
		hm.NoticePreparation();
		hm.HelpPage = 0;
	}

	private void ButtonSelect(int _animPlay, Action _setVoice)
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.AddSelectCount();
		if (instance.IsAngryCount())
		{
			Singleton<Game>.Instance.isConciergeAngry = true;
			StartCoroutine(ConciergeAngryCoroutine());
		}
		else
		{
			_setVoice?.Invoke();
		}
	}

	public void SetCustomButtonDraw()
	{
		itemUIs[4].tgl.gameObject.SetActiveIfDifferent(active: true);
	}

	private void DeSelectItemUI(MenuItemUI _ui)
	{
		_ui.uiSlide.IsSlideAlways = false;
		_ui.tgl.SetIsOnWithoutCallback(isOn: false);
		_ui.texts.ForEach(delegate(Text t)
		{
			t.color = Game.defaultFontColor;
		});
	}

	public void SetDefaultToggleTransition(int _category)
	{
		if (_category != -1)
		{
			itemUIs[_category].tgl.SetDoStateTransition(0, instant: true);
			return;
		}
		MenuItemUI[] array = itemUIs;
		foreach (MenuItemUI menuItemUI in array)
		{
			if (menuItemUI.tgl.IsInteractable())
			{
				menuItemUI.tgl.SetDoStateTransition(0, instant: true);
			}
		}
	}

	public void GoToConciergeH()
	{
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		Game game = Singleton<Game>.Instance;
		HSceneManager hscene = Singleton<HSceneManager>.Instance;
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strHScene[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			SaveData.SetAchievementAchieve(4);
			game.eventNo = 33;
			game.peepKind = -1;
			game.isConciergeAngry = hm.IsAngry();
			game.mapNo = 1;
			game.heroineList = new List<Heroine> { hm.ConciergeHeroine, null };
			hscene.females = new ChaControl[2] { hm.ConciergeChaCtrl, null };
			hscene.pngFemales[1] = "";
			hscene.mapID = game.mapNo;
			PlayerCharaSaveInfo playerChara = game.saveData.playerChara;
			hscene.pngMale = playerChara.FileName;
			hscene.bFutanari = playerChara.Futanari;
			playerChara = game.saveData.secondPlayerChara;
			hscene.pngMaleSecond = playerChara.FileName;
			hscene.bFutanariSecond = playerChara.Futanari;
			HomeScene.startCanvas = 0;
			HomeScene.startCharaEdit = -1;
			Dictionary<string, Game.EventCharaInfo> source = game.tableLobbyEvents[game.saveData.selectGroup];
			game.tableDesireCharas.Clear();
			source.Where((KeyValuePair<string, Game.EventCharaInfo> c) => Game.DesireEventIDs.Contains(c.Value.eventID)).ToList().ForEach(delegate(KeyValuePair<string, Game.EventCharaInfo> c)
			{
				game.tableDesireCharas.Add(Path.GetFileNameWithoutExtension(c.Value.fileName), c.Value.eventID);
			});
			if (hm.ConciergeChaCtrl.fileGameInfo2.hCount >= 3)
			{
				AppendSaveData appendSaveData = game.appendSaveData;
				if (appendSaveData.AppendTutorialNo != -1 || appendSaveData.SitriSelectCount < 3 || appendSaveData.IsFurSitri3P)
				{
					hscene.pngFemales[1] = (appendSaveData.IsFurSitri3P ? Singleton<Character>.Instance.sitriPath : "");
					hscene.SecondSitori = appendSaveData.IsFurSitri3P;
					Scene.LoadReserve(new Scene.Data
					{
						levelName = "HScene",
						fadeType = FadeCanvas.Fade.In
					}, isLoadingImageDraw: true);
				}
				else
				{
					game.heroineList.Clear();
					Singleton<ADVManager>.Instance.filenames = new string[2]
					{
						Singleton<Character>.Instance.conciergePath,
						Singleton<Character>.Instance.sitriPath
					};
					hscene.females = new ChaControl[2];
					hscene.pngFemales[0] = "";
					hscene.pngFemales[1] = "";
					game.eventNo = 58;
					Singleton<ADVManager>.Instance.advDelivery.Set("0", -1, 58);
					Scene.LoadReserve(new Scene.Data
					{
						levelName = "ADV",
						fadeType = FadeCanvas.Fade.In,
						onLoad = delegate
						{
							Singleton<Character>.Instance.DeleteCharaAll();
						}
					}, isLoadingImageDraw: true);
				}
			}
			else
			{
				Singleton<ADVManager>.Instance.advDelivery.Set("0", -1, 33);
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "ADV",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			}
		};
		status.No = delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			DeSelectItemUI(itemUIs[3]);
		};
		ConfirmDialog.Load();
	}

	public void GoToSpecialRoom()
	{
		_ = Singleton<HomeSceneManager>.Instance;
		_ = Singleton<Game>.Instance;
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strGotoSPRoom[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			StartCoroutine(GotoSPRoomCoroutine());
		};
		status.No = delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			DeSelectItemUI(itemUIs[5]);
		};
		ConfirmDialog.Load();
	}

	private IEnumerator GotoSPRoomCoroutine()
	{
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		Game game = Singleton<Game>.Instance;
		hm.CGMain.blocksRaycasts = false;
		HomeScene.startCanvas = 0;
		HomeScene.startCharaEdit = -1;
		hm.ConciergeAnimationPlay(1);
		hm.HomeConciergeInfoData.SetClickGotoSepcialRoom(hm.ConciergeChaCtrl, hm.IsAngry());
		yield return new WaitUntil(() => !Voice.IsPlay());
		if (game.appendSaveData.AppendTutorialNo == -1)
		{
			SpecialTreatmentRoomScene.startCanvas = 0;
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "SpecialTreatmentRoom",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
			yield break;
		}
		game.eventNo = -20;
		game.peepKind = -1;
		game.isConciergeAngry = hm.IsAngry();
		game.mapNo = 501;
		game.heroineList.Clear();
		Singleton<Character>.Instance.DeleteCharaAll();
		Singleton<Character>.Instance.EndLoadAssetBundle();
		game.heroineList.Clear();
		Singleton<HSceneManager>.Instance.player = null;
		Singleton<HSceneManager>.Instance.females[0] = null;
		Singleton<HSceneManager>.Instance.females[1] = null;
		Singleton<ADVManager>.Instance.filenames[0] = "";
		Singleton<ADVManager>.Instance.filenames[1] = "";
		Singleton<ADVManager>.Instance.advDelivery.Set("0", -20, 0);
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "ADV",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	public void InitSpecialRoom()
	{
		if (!GameSystem.isAdd50)
		{
			return;
		}
		Game instance = Singleton<Game>.Instance;
		itemUIs[5].tgl.gameObject.SetActiveIfDifferent(instance.saveData.TutorialNo == -1 && instance.appendSaveData.IsAppendStart == -1);
		if (instance.appendSaveData.AppendTutorialNo == -1)
		{
			return;
		}
		bool flag = !instance.IsAllNormalState(instance.saveData.selectGroup);
		itemUIs[5].tgl.interactable = flag;
		pointerEnterExitNotPlanCharacter.listActionEnter.Clear();
		pointerEnterExitNotPlanCharacter.listActionExit.Clear();
		if (!flag)
		{
			pointerEnterExitNotPlanCharacter.listActionEnter.Add(delegate
			{
				objNotPlanCharacter.SetActiveIfDifferent(active: true);
			});
			pointerEnterExitNotPlanCharacter.listActionExit.Add(delegate
			{
				objNotPlanCharacter.SetActiveIfDifferent(active: false);
			});
		}
	}
}
