using System;
using System.Collections;
using System.Collections.Generic;
using AIChara;
using CharaCustom;
using Illusion.Anime;
using Illusion.Component.UI;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HS2;

public class FurRoomMenuUI : MonoBehaviour
{
	private enum ConciergeUICategory
	{
		Search,
		Achievement,
		Help,
		H,
		Custom
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
	private Button btnBack;

	[SerializeField]
	private MenuItemUI[] itemUIs;

	[SerializeField]
	private FurRoomAchievementUI conciergeAchievementUI;

	private readonly string[] strCustom = new string[6] { "キャラのメイク画面に移動します\nよろしいですか？", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "" };

	private readonly string[] strSearch = new string[6] { "キャラ検索画面に移動します\nよろしいですか？", "Go to the character search screen.\nIs it OK?", "Go to the character search screen.\nIs it OK?", "Go to the character search screen.\nIs it OK?", "Go to the character search screen.\nIs it OK?", "" };

	private readonly string[] strReturnToHome = new string[6] { "マイルームに戻ります\nよろしいですか？", "Return to my room.\nIs it OK?", "Return to my room.\nIs it OK?", "Return to my room.\nIs it OK?", "Return to my room.\nIs it OK?", "" };

	private readonly string[] strHScene = new string[5] { "Hを開始しますか？", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?" };

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<FurRoomSceneManager>.IsInstance());
		FurRoomSceneManager fm = Singleton<FurRoomSceneManager>.Instance;
		Game game = Singleton<Game>.Instance;
		MenuItemUI[] array = itemUIs;
		foreach (MenuItemUI ui in array)
		{
			ui.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
			{
				if (_isOn)
				{
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.selectFontColor;
					});
					Utils.Sound.Play(SystemSE.ok_s);
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
					{
						AppendSaveData appendSaveData = game.appendSaveData;
						if (fm.ConciergeChaCtrl.fileGameInfo2.hCount < 3 || appendSaveData.SitriSelectCount < 3 || appendSaveData.AppendTutorialNo != -1 || appendSaveData.IsFurSitri3P)
						{
							fm.MapSelectUI.InitList();
							fm.SetCociergeModeCanvasGroup(2);
							DeSelectItemUI(ui);
						}
						else
						{
							GoTo3PH();
						}
						break;
					}
					case 4:
					{
						ui.uiSlide.IsSlideAlways = true;
						ConfirmDialog.Status status = ConfirmDialog.status;
						status.Sentence = strCustom[Singleton<GameSystem>.Instance.languageInt];
						status.Yes = delegate
						{
							Utils.Sound.Play(SystemSE.ok_l);
							StartCoroutine(CustomCoroutine());
						};
						status.No = delegate
						{
							Utils.Sound.Play(SystemSE.cancel);
							DeSelectItemUI(ui);
						};
						ConfirmDialog.Load();
						break;
					}
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
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.defaultFontColor;
					});
					switch (ui.id)
					{
					case 0:
						ButtonSelect(2, delegate
						{
							fm.HomeConciergeInfoData.SetSearchVoice(fm.ConciergeChaCtrl, fm.IsAngry());
						});
						break;
					case 1:
						ButtonSelect(2, delegate
						{
							fm.HomeConciergeInfoData.SetAchievementVoice(fm.ConciergeChaCtrl, fm.IsAngry());
						});
						break;
					case 2:
						ButtonSelect(2, delegate
						{
							fm.HomeConciergeInfoData.SetHelpVoice(fm.ConciergeChaCtrl, fm.IsAngry());
						});
						break;
					case 3:
						ButtonSelect(3, delegate
						{
							fm.HomeConciergeInfoData.SetHVoice(fm.ConciergeChaCtrl, fm.IsAngry());
						});
						break;
					case 4:
						ButtonSelect(2, delegate
						{
							fm.HomeConciergeInfoData.SetCustomVoice(fm.ConciergeChaCtrl, fm.IsAngry());
						});
						break;
					}
				}
			});
		}
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = strReturnToHome[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				StartCoroutine(Back());
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
		});
		SetCustomButtonDraw();
		base.enabled = true;
	}

	private IEnumerator BowCoroutine()
	{
		FurRoomSceneManager instance = Singleton<FurRoomSceneManager>.Instance;
		instance.CGMain.interactable = false;
		instance.HomeConciergeInfoData.SetClickVoice(instance.ConciergeChaCtrl, instance.IsAngry());
		yield return new WaitUntil(() => !Voice.IsPlay());
	}

	private IEnumerator BowCutomCoroutine()
	{
		FurRoomSceneManager instance = Singleton<FurRoomSceneManager>.Instance;
		instance.CGMain.interactable = false;
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
		Singleton<FurRoomSceneManager>.Instance.CGMain.interactable = true;
		yield return null;
	}

	private IEnumerator AchievementCoroutine()
	{
		yield return StartCoroutine(BowCoroutine());
		FurRoomSceneManager instance = Singleton<FurRoomSceneManager>.Instance;
		instance.StartFade();
		instance.SelectCountClear();
		instance.SetCociergeModeCanvasGroup(1);
		DeSelectItemUI(itemUIs[1]);
		conciergeAchievementUI.Init();
		instance.CGMain.interactable = true;
		yield return null;
	}

	private IEnumerator HelpCoroutine()
	{
		yield return StartCoroutine(BowCoroutine());
		FurRoomSceneManager instance = Singleton<FurRoomSceneManager>.Instance;
		instance.SelectCountClear();
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
		FurRoomSceneManager instance = Singleton<FurRoomSceneManager>.Instance;
		instance.ConciergeChaCtrl.visibleAll = false;
		instance.CGMain.interactable = true;
		Controller.Table.Get(instance.ConciergeChaCtrl).itemHandler.DisableItems();
		yield return null;
	}

	private IEnumerator ConciergeAngryCoroutine()
	{
		FurRoomSceneManager fm = Singleton<FurRoomSceneManager>.Instance;
		fm.CGMain.interactable = false;
		fm.HomeConciergeInfoData.SetAngryVoice(fm.ConciergeChaCtrl);
		yield return new WaitUntil(() => !Voice.IsPlay());
		fm.CGMain.interactable = true;
	}

	private IEnumerator Back()
	{
		FurRoomSceneManager instance = Singleton<FurRoomSceneManager>.Instance;
		instance.CGMain.interactable = false;
		instance.HomeConciergeInfoData.SetBackVoice(instance.ConciergeChaCtrl, instance.IsAngry());
		yield return new WaitUntil(() => !Voice.IsPlay());
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "Home",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	private void ButtonSelect(int _animPlay, Action _setVoice)
	{
		FurRoomSceneManager instance = Singleton<FurRoomSceneManager>.Instance;
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
		for (int i = 0; i < array.Length; i++)
		{
			array[i].tgl.SetDoStateTransition(0, instant: true);
		}
	}

	public void GoTo3PH()
	{
		_ = Singleton<FurRoomSceneManager>.Instance;
		Game game = Singleton<Game>.Instance;
		HSceneManager hscene = Singleton<HSceneManager>.Instance;
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strHScene[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			game.peepKind = -1;
			game.mapNo = 2;
			game.heroineList.Clear();
			hscene.mapID = game.mapNo;
			PlayerCharaSaveInfo playerChara = game.saveData.playerChara;
			hscene.pngMale = playerChara.FileName;
			hscene.bFutanari = playerChara.Futanari;
			playerChara = game.saveData.secondPlayerChara;
			hscene.pngMaleSecond = playerChara.FileName;
			hscene.bFutanariSecond = playerChara.Futanari;
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
		};
		status.No = delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			DeSelectItemUI(itemUIs[3]);
		};
		ConfirmDialog.Load();
	}
}
