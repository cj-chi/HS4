using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UI;
using UIAnimatorCore;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class LobbyMainUI : MonoBehaviour
{
	public enum LobbyMainMenuCategory
	{
		HStart,
		FirstSelect,
		SecondSelect,
		Option
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
	private MenuItemUI[] itemUIs = new MenuItemUI[5];

	[SerializeField]
	private Button btnBack;

	[SerializeField]
	private RawImage riMap;

	[SerializeField]
	private Text txtMap;

	[SerializeField]
	private UIAnimator mapUIAnimator;

	[SerializeField]
	private LobbySelectUI selectUI;

	[SerializeField]
	private LobbyMapSelectUI mapSelectUI;

	[SerializeField]
	private MenuItemUI itemGotoSpecialRoom = new MenuItemUI();

	private readonly string[] strWarning = new string[6] { "女の子を選択してください", "Please select a female", "Please select a female", "Please select a female", "Please select a female", "" };

	private readonly string[] strGotoSPRoom = new string[6] { "特別待遇室に移動します\nよろしいですか？", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "Go to the special room.\nIs it OK?", "" };

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<LobbySceneManager>.IsInstance());
		LobbySceneManager lm = Singleton<LobbySceneManager>.Instance;
		while (!lm.IsInitialize)
		{
			yield return null;
		}
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
						_ = Singleton<Game>.Instance;
						_ = Singleton<HSceneManager>.Instance;
						if (lm.heroines[0] == null)
						{
							GroupEntryWarningDialog();
							return;
						}
						lm.SetModeCanvasGroup(1);
						lm.CGParameter.alpha = 0f;
						mapSelectUI.InitList(lm.eventNos[lm.heroineRommListIdx[0]]);
						if (Singleton<Game>.Instance.saveData.TutorialNo == 10)
						{
							SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = false;
							SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 3;
							SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.onEndFunc = delegate
							{
								Singleton<Game>.Instance.saveData.TutorialNo = 11;
								lm.OCBTutorial.SetActiveToggle(3);
							};
							global::Tutorial2D.Tutorial2D.Load();
							lm.SetSelectCanvasGroup(_enable: false);
						}
						DeSelectItemUI(ui.id);
						break;
					case 1:
					{
						string oldFileName = lm.heroines[0]?.chaFile.charaFileName ?? "";
						lm.SelectUI.SetEntryCharaNo(0, oldFileName);
						lm.SetSelectCanvasGroup(_enable: true);
						ResetMapListAnime();
						ui.uiSlide.IsSlideAlways = true;
						ui.texts.ForEach(delegate(Text t)
						{
							t.color = Game.selectFontColor;
						});
						if (Singleton<Game>.Instance.saveData.TutorialNo == 8)
						{
							Singleton<Game>.Instance.saveData.TutorialNo = 9;
							lm.OCBTutorial.SetActiveToggle(1);
						}
						break;
					}
					case 2:
					{
						string oldFileName2 = lm.heroines[1]?.chaFile.charaFileName ?? "";
						lm.SelectUI.SetEntryCharaNo(1, oldFileName2);
						lm.SetSelectCanvasGroup(_enable: true);
						ResetMapListAnime();
						ui.uiSlide.IsSlideAlways = true;
						ui.texts.ForEach(delegate(Text t)
						{
							t.color = Game.selectFontColor;
						});
						break;
					}
					case 3:
						ConfigWindow.UnLoadAction = delegate
						{
							ui.tgl.SetIsOnWithoutCallback(isOn: false);
						};
						ConfigWindow.Load();
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
					switch (ui.id)
					{
					case 1:
						if (selectUI.EntryCharaNo == 0)
						{
							selectUI.CancelProc();
						}
						break;
					case 2:
						if (selectUI.EntryCharaNo == 1)
						{
							selectUI.CancelProc();
						}
						break;
					}
				}
			});
			ui.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (ui.tgl.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
				}
			});
		}
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "Home",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (btnBack.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
			}
		});
		itemUIs[0].tgl.interactable = false;
		itemUIs[2].tgl.interactable = false;
		Singleton<Game>.Instance.mapNo = 0;
		LoadMapImage();
		ResetMapListAnime();
		if (lm.heroines[0] != null)
		{
			itemUIs[1].tgl.interactable = false;
			itemUIs[0].tgl.interactable = true;
			btnBack.interactable = false;
		}
		AppendSaveData appendSaveData = Singleton<Game>.Instance.appendSaveData;
		itemGotoSpecialRoom.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
		{
			if (_isOn)
			{
				Utils.Sound.Play(SystemSE.ok_s);
				itemGotoSpecialRoom.uiSlide.IsSlideAlways = true;
				itemGotoSpecialRoom.texts.ForEach(delegate(Text t)
				{
					t.color = Game.selectFontColor;
				});
				_ = Singleton<Game>.Instance;
				ConfirmDialog.Status status = ConfirmDialog.status;
				status.Sentence = strGotoSPRoom[Singleton<GameSystem>.Instance.languageInt];
				status.Yes = delegate
				{
					Utils.Sound.Play(SystemSE.ok_l);
					SpecialTreatmentRoomScene.startCanvas = 0;
					Scene.LoadReserve(new Scene.Data
					{
						levelName = "SpecialTreatmentRoom",
						fadeType = FadeCanvas.Fade.In
					}, isLoadingImageDraw: true);
				};
				status.No = delegate
				{
					Utils.Sound.Play(SystemSE.cancel);
					DeSelectItemUI(itemGotoSpecialRoom);
				};
				ConfirmDialog.Load();
			}
			else
			{
				itemGotoSpecialRoom.uiSlide.IsSlideAlways = false;
				itemGotoSpecialRoom.texts.ForEach(delegate(Text t)
				{
					t.color = Game.defaultFontColor;
				});
			}
		});
		itemGotoSpecialRoom.tgl.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (itemGotoSpecialRoom.tgl.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
			}
		});
		if (GameSystem.isAdd50)
		{
			itemGotoSpecialRoom.tgl.gameObject.SetActiveIfDifferent(appendSaveData.AppendTutorialNo == -1);
		}
		else
		{
			itemGotoSpecialRoom.tgl.gameObject.SetActiveIfDifferent(active: false);
			itemGotoSpecialRoom.texts.ForEach(delegate(Text t)
			{
				t.text = "";
			});
		}
		base.enabled = true;
	}

	public void DeSelectItemUI(int _index)
	{
		MenuItemUI obj = itemUIs[_index];
		obj.uiSlide.IsSlideAlways = false;
		obj.tgl.SetIsOnWithoutCallback(isOn: false);
	}

	public void DeSelectItemUI(MenuItemUI _ui)
	{
		_ui.uiSlide.IsSlideAlways = false;
		_ui.tgl.SetIsOnWithoutCallback(isOn: false);
		_ui.texts.ForEach(delegate(Text t)
		{
			t.color = Game.defaultFontColor;
		});
	}

	public void SetMenuItemInteractable(int _index, bool _interactable)
	{
		itemUIs[_index].tgl.interactable = _interactable;
	}

	private void LoadMapImage()
	{
		if (BaseMap.infoTable.TryGetValue(Singleton<Game>.Instance.mapNo, out var value))
		{
			riMap.texture = CommonLib.LoadAsset<Texture2D>(value.ThumbnailBundle_L, value.ThumbnailAsset_L, clone: false, value.ThumbnailManifest_L);
			txtMap.text = value.MapNames[Singleton<GameSystem>.Instance.languageInt];
			AssetBundleManager.UnloadAssetBundle(value.ThumbnailBundle_L, isUnloadForceRefCount: true);
		}
	}

	public void ResetMapListAnime()
	{
		mapUIAnimator.SetAnimType(AnimSetupType.Outro);
		mapUIAnimator.ResetToEnd();
	}

	private void GroupEntryWarningDialog()
	{
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strWarning[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			DeSelectItemUI(0);
		};
		status.No = null;
		ConfirmDialog.Load();
	}
}
