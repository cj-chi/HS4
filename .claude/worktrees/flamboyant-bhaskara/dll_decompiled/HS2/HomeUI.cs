using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ADV;
using Config;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class HomeUI : MonoBehaviour
{
	private enum HomeMenuCategory
	{
		GoToRoom,
		CharaEdit,
		CallConcierge,
		Option,
		Sleep,
		DXPlan
	}

	[Serializable]
	public class MenuItemUI
	{
		public int id;

		public Toggle tgl;

		public UIObjectSlideOnCursor uiSlide;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private MenuItemUI[] itemUIs;

	[SerializeField]
	private Button btnTitle;

	[SerializeField]
	private GameObject objAppendTutorialStart;

	private readonly string[] strWarning = new string[6] { "[キャラクター編集]を選び、\n女の子をグループに登録して下さい", "Select [Edit Character] and register\nthe girl to Group", "Select [Edit Character] and register\nthe girl to Group", "Select [Edit Character] and register\nthe girl to Group", "Select [Edit Character] and register\nthe girl to Group", "" };

	private readonly string[] strToTitle = new string[5] { "タイトルに戻りますか？", "Return to title?", "Return to title?", "Return to title?", "Return to title?" };

	private readonly string[] strSleeep = new string[5] { "寝ますか？", "Do you sleep?", "Do you sleep?", "Do you sleep?", "Do you sleep?" };

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
					switch (ui.id)
					{
					case 0:
					{
						Game instance = Singleton<Game>.Instance;
						SaveData saveData = instance.saveData;
						if (saveData.roomList[instance.saveData.selectGroup].Any())
						{
							ui.tgl.SetIsOnWithoutCallback(isOn: false);
							if (saveData.TutorialNo == 6)
							{
								Singleton<HomeSceneManager>.Instance.SetModeCanvasGroup(1);
								hm.OCBTutorial.SetActive(_active: false);
								hm.OpenADV("adv/scenario/op/30/02.unity3d", "0", 0, delegate
								{
									hm.SetCameraPosition(1);
									Singleton<Game>.Instance.saveData.TutorialNo = 7;
									hm.OCBTutorial.SetActiveToggle(6);
								});
							}
							else if (saveData.TutorialNo == 12)
							{
								hm.StartFade();
								Singleton<HomeSceneManager>.Instance.SetModeCanvasGroup(1);
								hm.SetCameraPosition(1);
								Singleton<Game>.Instance.saveData.TutorialNo = 13;
								hm.OCBTutorial.SetActiveToggle(8);
							}
							else
							{
								hm.StartFade();
								Singleton<HomeSceneManager>.Instance.SetModeCanvasGroup(1);
								hm.SetCameraPosition(1);
								hm.LeaveTheRoomUI.InitLeaveRoom();
							}
						}
						else
						{
							ui.texts.ForEach(delegate(Text t)
							{
								t.color = Game.selectFontColor;
							});
							ui.uiSlide.IsSlideAlways = true;
							GroupEntryWarningDialog();
						}
						break;
					}
					case 1:
					{
						for (int num2 = 0; num2 < hm.roomList.Length; num2++)
						{
							hm.roomList[num2] = new List<string>(Singleton<Game>.Instance.saveData.roomList[num2]);
						}
						hm.StartFade();
						hm.SetModeCanvasGroup(2);
						hm.SetCameraPosition(2);
						hm.AnimationMenu(_isOpen: true);
						ui.tgl.SetIsOnWithoutCallback(isOn: false);
						hm.HelpPage = 1;
						hm.CharaEditUI.PlayerCoordinateButtonVisible();
						if (Singleton<Game>.Instance.saveData.TutorialNo == 1)
						{
							hm.StartFade();
							hm.OCBTutorial.SetActive(_active: false);
							hm.OCBTutorial.SetActiveToggle(1);
							hm.OpenADV("adv/scenario/op/30/01.unity3d", "0", 0, delegate
							{
								hm.SetCameraPosition(2);
								SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = false;
								SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 1;
								global::Tutorial2D.Tutorial2D.Load();
								Singleton<Game>.Instance.saveData.TutorialNo = 2;
							});
						}
						break;
					}
					case 2:
						ui.texts.ForEach(delegate(Text t)
						{
							t.color = Game.selectFontColor;
						});
						hm.HelpPage = 5;
						if (Manager.Config.HData.HomeCallConciergeEventSkip)
						{
							StartCoroutine(CallConciergeCoroutine());
						}
						else
						{
							CallConcierge();
						}
						SaveData.SetAchievementAchieve(3);
						break;
					case 3:
						StartCoroutine(OpenOption());
						break;
					case 4:
						ui.texts.ForEach(delegate(Text t)
						{
							t.color = Game.selectFontColor;
						});
						Sleep();
						break;
					case 5:
						ui.texts.ForEach(delegate(Text t)
						{
							t.color = Game.selectFontColor;
						});
						BuyDXPlan();
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
				}
			});
			ui.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		}
		btnTitle.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = strToTitle[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				Utils.Sound.Play(SystemSE.ok_l);
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "Title",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		});
		btnTitle.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
		});
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(0)
			where hm.IsADV
			where !Scene.IsFadeNow
			where Singleton<Game>.Instance.saveData.TutorialNo == -1
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			where Setup._advScene != null
			select _).Subscribe(delegate
		{
			Setup._advScene.Release();
		});
		base.enabled = true;
	}

	private IEnumerator CallConciergeCoroutine()
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.CGMain.blocksRaycasts = false;
		instance.StartFade();
		instance.SetCameraPosition(4, instance.ConciergeChaCtrl.GetShapeBodyValue(0));
		instance.ConciergeChaCtrl.visibleAll = true;
		instance.StartFade();
		instance.ConciergeAnimationPlay(1, _isSameCheck: false);
		instance.HomeConciergeInfoData.SetRoomStartVoice(instance.ConciergeChaCtrl);
		instance.SetCociergeModeCanvasGroup(0);
		instance.SetModeCanvasGroup(3);
		MenuItemUI obj = itemUIs[2];
		obj.tgl.SetIsOnWithoutCallback(isOn: false);
		obj.texts.ForEach(delegate(Text t)
		{
			t.color = Game.defaultFontColor;
		});
		instance.CGMain.blocksRaycasts = true;
		instance.ConciergeMenuUI.InitSpecialRoom();
		yield return null;
	}

	private void CallConcierge()
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.CGMain.alpha = 0f;
		instance.StartFade();
		instance.OpenADV("adv/scenario/c-1/30/35.unity3d", "0", 1, delegate
		{
			StartCoroutine(CallConciergeADVEnd());
		});
	}

	private IEnumerator CallConciergeADVEnd()
	{
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		hm.SetCameraPosition(4, hm.ConciergeChaCtrl.GetShapeBodyValue(0));
		hm.ConciergeChaCtrl.visibleAll = true;
		hm.ConciergeAnimationPlay(1, _isSameCheck: false);
		hm.HomeConciergeInfoData.SetRoomStartVoice(hm.ConciergeChaCtrl);
		yield return null;
		hm.ConciergeMenuUI.IsOneTimeStop = true;
		hm.ConciergeMenuUI.InitSpecialRoom();
		hm.CGMain.alpha = 1f;
		hm.SetCociergeModeCanvasGroup(0);
		hm.SetModeCanvasGroup(3);
		MenuItemUI obj = itemUIs[2];
		obj.tgl.SetIsOnWithoutCallback(isOn: false);
		obj.texts.ForEach(delegate(Text t)
		{
			t.color = Game.defaultFontColor;
		});
	}

	private IEnumerator OpenOption()
	{
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		hm.CGMain.blocksRaycasts = false;
		hm.StartFade();
		hm.SetCameraPosition(5);
		hm.AnimationDesk(_isOpen: true);
		yield return null;
		yield return new WaitUntil(() => !hm.IsAnimationDesk());
		ConfigWindow.UnLoadAction = delegate
		{
			hm.SetCameraPosition(0);
			hm.AnimationDesk(_isOpen: false);
		};
		ConfigWindow.Load();
		hm.CGMain.blocksRaycasts = true;
		MenuItemUI obj = itemUIs[3];
		obj.tgl.SetIsOnWithoutCallback(isOn: false);
		obj.texts.ForEach(delegate(Text t)
		{
			t.color = Game.defaultFontColor;
		});
	}

	private void GroupEntryWarningDialog()
	{
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strWarning[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			MenuItemUI obj = itemUIs[0];
			obj.uiSlide.IsSlideAlways = false;
			obj.tgl.SetIsOnWithoutCallback(isOn: false);
			obj.texts.ForEach(delegate(Text t)
			{
				t.color = Game.defaultFontColor;
			});
		};
		status.No = null;
		ConfirmDialog.Load();
	}

	private void Sleep()
	{
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strSleeep[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			_ = Singleton<HomeSceneManager>.Instance;
			Game game = Singleton<Game>.Instance;
			SaveData saveData = game.saveData;
			ADVManager instance = Singleton<ADVManager>.Instance;
			saveData.BeforeFemaleName = string.Empty;
			List<GlobalHS2Calc.CharaInfo> _charas;
			GlobalHS2Calc.MapCharaSelectInfo sleepEvent = GlobalHS2Calc.GetSleepEvent(saveData.selectGroup, out _charas);
			game.tableDesireCharas.Clear();
			if (sleepEvent == null)
			{
				instance.filenames[0] = "";
				instance.filenames[1] = "";
				instance.advDelivery.Set("0", -100, 36);
				game.mapNo = 1;
				Scene.sceneFadeCanvas.SetColor(Color.black);
			}
			else
			{
				instance.filenames[0] = sleepEvent.lstChara[0].Item1;
				instance.filenames[1] = ((sleepEvent.eventID == 14) ? sleepEvent.lstChara[1].Item1 : string.Empty);
				Singleton<HSceneManager>.Instance.pngFemales[1] = string.Empty;
				instance.advDelivery.Set("0", (sleepEvent.eventID == 14) ? (-100) : sleepEvent.lstChara[0].Item2, sleepEvent.eventID);
				game.mapNo = sleepEvent.mapID;
				game.eventNo = sleepEvent.eventID;
			}
			game.tableDesireCharas.Clear();
			_charas.Where((GlobalHS2Calc.CharaInfo c) => Game.DesireEventIDs.Contains(c.eventID)).ToList().ForEach(delegate(GlobalHS2Calc.CharaInfo c)
			{
				game.tableDesireCharas.Add(Path.GetFileNameWithoutExtension(c.chaFile.charaFileName), c.eventID);
			});
			Singleton<Character>.Instance.DeleteCharaAll();
			HomeScene.startCanvas = 0;
			HomeScene.startCharaEdit = -1;
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "ADV",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		};
		status.No = delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			MenuItemUI obj = itemUIs[4];
			obj.tgl.SetIsOnWithoutCallback(isOn: false);
			obj.texts.ForEach(delegate(Text t)
			{
				t.color = Game.defaultFontColor;
			});
		};
		ConfirmDialog.Load();
	}

	public void DXPlanButtonVisible()
	{
		MenuItemUI menuItemUI = itemUIs[5];
		Game instance = Singleton<Game>.Instance;
		if (GameSystem.isAdd50)
		{
			menuItemUI.tgl.gameObject.SetActiveIfDifferent(Singleton<Game>.Instance.saveData.TutorialNo == -1 && instance.appendSaveData.IsAppendStart == 0);
		}
		else
		{
			menuItemUI.tgl.gameObject.SetActiveIfDifferent(active: false);
		}
	}

	public void AppendTutorialStartVisible()
	{
		Game instance = Singleton<Game>.Instance;
		objAppendTutorialStart.SetActiveIfDifferent(instance.saveData.TutorialNo == -1 && instance.appendSaveData.IsAppendStart == -1 && instance.appendSaveData.AppendTutorialNo != -1);
	}

	private void BuyDXPlan()
	{
		Singleton<ADVManager>.Instance.filenames[0] = "";
		Singleton<ADVManager>.Instance.filenames[1] = "";
		Singleton<ADVManager>.Instance.advDelivery.Set("0", -1, 57);
		Singleton<Game>.Instance.mapNo = 1;
		Singleton<Game>.Instance.appendSaveData.IsAppendStart = 1;
		HomeScene.startCanvas = 0;
		HomeScene.startCharaEdit = -1;
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
