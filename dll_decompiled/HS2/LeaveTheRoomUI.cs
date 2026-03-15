using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class LeaveTheRoomUI : MonoBehaviour
{
	private enum LeaveTheRoomCategory
	{
		Leave,
		AroundTheHall,
		GotoSpecialRoom
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
	private Button btnBack;

	[SerializeField]
	private MenuItemUI[] itemUIs;

	private readonly string[] strWarning = new string[6] { "[キャラクター編集]を選び、\n女の子をグループに登録して下さい", "Select [Edit Character] and register\nthe girl to Group", "Select [Edit Character] and register\nthe girl to Group", "Select [Edit Character] and register\nthe girl to Group", "Select [Edit Character] and register\nthe girl to Group", "" };

	private readonly string[] strConfirm = new string[6] { "待ち合わせ場所に移動しますか？", "Go to a meeting place?", "Go to a meeting place?", "Go to a meeting place?", "Go to a meeting place?", "" };

	private readonly string[] strGotoSPRoom = new string[6] { "特別待遇室に移動します\nよろしいですか？", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "" };

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
					ui.uiSlide.IsSlideAlways = true;
					switch (ui.id)
					{
					case 0:
					{
						Game instance = Singleton<Game>.Instance;
						ConfirmDialog.Status status = ConfirmDialog.status;
						if (instance.saveData.roomList[instance.saveData.selectGroup].Any())
						{
							status.Sentence = strConfirm[Singleton<GameSystem>.Instance.languageInt];
							status.Yes = delegate
							{
								Utils.Sound.Play(SystemSE.ok_l);
								StartCoroutine(LeaveTheRoomCoroutine());
							};
							status.No = delegate
							{
								Utils.Sound.Play(SystemSE.cancel);
								DeSelectItemUI(ui);
							};
							ConfirmDialog.Load();
						}
						else
						{
							GroupEntryWarningDialog();
						}
						break;
					}
					case 1:
						Utils.Sound.Play(SystemSE.ok_s);
						hm.HelpPage = 4;
						GoToMapSelect();
						break;
					case 2:
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
				Utils.Sound.Play(SystemSE.sel);
			});
		}
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			StartCoroutine(Back());
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
		});
		itemUIs[2].tgl.gameObject.SetActiveIfDifferent(active: false);
		if (!GameSystem.isAdd50)
		{
			itemUIs[2].texts.ForEach(delegate(Text t)
			{
				t.text = "";
			});
		}
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !Scene.IsFadeNow
			where Singleton<Game>.Instance.saveData.TutorialNo == -1 || (Singleton<Game>.Instance.saveData.TutorialNo != 6 && Singleton<Game>.Instance.saveData.TutorialNo != 7 && Singleton<Game>.Instance.saveData.TutorialNo != 13)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			where hm.CGMain.interactable
			where hm.CGModes[1].alpha >= 0.9f
			select _).Subscribe(delegate
		{
			StartCoroutine(Back());
		});
		base.enabled = true;
	}

	private IEnumerator LeaveTheRoomCoroutine()
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.CGMain.interactable = false;
		instance.AnimationDoor(_isOpen: true);
		instance.IsMove = true;
		Utils.Sound.Play(new Utils.Sound.Setting(Manager.Sound.Type.GameSE2D)
		{
			bundle = "sound/data/se/adv/30.unity3d",
			asset = "door_00"
		});
		HomeScene.startCanvas = 0;
		HomeScene.startCharaEdit = -1;
		if (Singleton<Game>.Instance.saveData.TutorialNo == 7)
		{
			Singleton<ADVManager>.Instance.filenames[0] = "";
			Singleton<ADVManager>.Instance.filenames[1] = "";
			Singleton<ADVManager>.Instance.advDelivery.Set("0", -10, 3);
			Singleton<Game>.Instance.mapNo = 0;
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "ADV",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		}
		else
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "LobbyScene",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		}
		yield break;
	}

	private void GroupEntryWarningDialog()
	{
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strWarning[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			DeSelectItemUI(itemUIs[0]);
		};
		status.No = null;
		ConfirmDialog.Load();
	}

	private IEnumerator Back()
	{
		yield return null;
		Utils.Sound.Play(SystemSE.cancel);
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.SetCameraPosition(0);
		instance.StartFade();
		Singleton<HomeSceneManager>.Instance.SetModeCanvasGroup(0);
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

	private void GoToMapSelect()
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.MapSelectUI.InitList();
		instance.SetModeCanvasGroup(4);
		DeSelectItemUI(itemUIs[1]);
		if (Singleton<Game>.Instance.saveData.TutorialNo == 13)
		{
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = false;
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 4;
			global::Tutorial2D.Tutorial2D.Load();
			Singleton<Game>.Instance.saveData.TutorialNo = 14;
			instance.OCBTutorial.SetActiveToggle(9);
		}
	}

	private void GoToSpecialRoom()
	{
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		_ = Singleton<Game>.Instance;
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strGotoSPRoom[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			hm.AnimationDoor(_isOpen: true);
			hm.IsMove = true;
			Utils.Sound.Play(new Utils.Sound.Setting(Manager.Sound.Type.GameSE2D)
			{
				bundle = "sound/data/se/adv/30.unity3d",
				asset = "door_00"
			});
			GotoSPRoomCoroutine();
		};
		status.No = delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			DeSelectItemUI(itemUIs[2]);
		};
		ConfirmDialog.Load();
	}

	private void GotoSPRoomCoroutine()
	{
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		_ = Singleton<Game>.Instance;
		instance.CGMain.blocksRaycasts = false;
		HomeScene.startCanvas = 0;
		HomeScene.startCharaEdit = -1;
		SpecialTreatmentRoomScene.startCanvas = 0;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "SpecialTreatmentRoom",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	public void InitLeaveRoom()
	{
		if (GameSystem.isAdd50)
		{
			MenuItemUI obj = itemUIs[2];
			GameObjectExtensions.SetActiveIfDifferent(active: Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == -1, self: obj.tgl.gameObject);
		}
	}
}
