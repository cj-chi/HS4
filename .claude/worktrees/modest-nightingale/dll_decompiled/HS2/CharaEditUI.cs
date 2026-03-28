using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharaCustom;
using Config;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using ReMotion;
using Tutorial2D;
using UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class CharaEditUI : MonoBehaviour
{
	public enum HomeMenuCategory
	{
		FemaleCustom,
		MaleCustom,
		GroupEdit,
		CoordinateSelect,
		UsePlayerSelect,
		PlayerCoordinateSelect
	}

	[Serializable]
	public class MenuItemUI
	{
		public int id;

		public Toggle tgl;

		public UIObjectSlideOnCursor uiSlide;

		public List<Text> texts = new List<Text>();
	}

	[Serializable]
	public class CanvasGroupCtrl
	{
		public CanvasGroup canvasGroup;

		public float fadeTime = 0.2f;

		private bool isFadeOpen;

		private bool isFadeClose;

		public void Enable(bool _isEnable)
		{
			canvasGroup.Enable(_isEnable, isUseInteractable: false);
		}

		public void Open()
		{
			if (!isFadeOpen && (isFadeClose || !(canvasGroup.alpha > 0.98f)))
			{
				canvasGroup.blocksRaycasts = false;
				ObservableEasing.Linear(fadeTime, ignoreTimeScale: true).FrameTimeInterval(ignoreTimeScale: true).Subscribe(delegate(TimeInterval<float> x)
				{
					isFadeOpen = true;
					canvasGroup.alpha = x.Value;
				}, delegate
				{
				}, delegate
				{
					canvasGroup.blocksRaycasts = true;
					isFadeOpen = false;
				});
			}
		}

		public void Close()
		{
			if (!isFadeClose && (isFadeOpen || !(canvasGroup.alpha < 0.02f)))
			{
				canvasGroup.blocksRaycasts = false;
				ObservableEasing.Linear(fadeTime, ignoreTimeScale: true).FrameTimeInterval(ignoreTimeScale: true).Subscribe(delegate(TimeInterval<float> x)
				{
					isFadeClose = true;
					canvasGroup.alpha = 1f - x.Value;
				}, delegate
				{
				}, delegate
				{
					isFadeClose = false;
				});
			}
		}
	}

	[SerializeField]
	private GroupCharaSelectUI groupCharaSelectUI;

	[SerializeField]
	private GroupListUI groupListUI;

	[SerializeField]
	private CoordinateGroupListUI coordinateGroupListUI;

	[SerializeField]
	private CoordinateListUI coordinateListUI;

	[SerializeField]
	private MaleCharaSelectUI maleCharaSelectUI;

	[SerializeField]
	private PlayerCoordinateListUI playerCoordinateUI;

	[SerializeField]
	private PlayerCoordinateListUI playerCoordinateUI1;

	[SerializeField]
	private Button btnBack;

	[SerializeField]
	private MenuItemUI[] itemUIs;

	[SerializeField]
	private CanvasGroupCtrl cgcGroup;

	[SerializeField]
	private CanvasGroupCtrl cgcCoordinate;

	[SerializeField]
	private CanvasGroupCtrl cgcPlayerSelect;

	[SerializeField]
	private CanvasGroupCtrl cgcPlayerCoordinateSelect;

	private readonly string[] strConfirm = new string[6] { "キャラのメイク画面に移動します\nよろしいですか？", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "" };

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
					ItemSelect(ui);
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
					case 2:
						cgcGroup.Close();
						break;
					case 3:
						cgcCoordinate.Close();
						break;
					case 4:
						cgcPlayerSelect.Close();
						break;
					case 5:
						cgcPlayerCoordinateSelect.Close();
						break;
					}
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
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !Scene.IsFadeNow
			where !Singleton<HomeSceneManager>.Instance.IsADV
			where Singleton<Game>.Instance.saveData.TutorialNo == -1 || Singleton<Game>.Instance.saveData.TutorialNo > 5
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			where hm.CGMain.interactable
			where hm.CGModes[2].alpha >= 0.9f
			select _).Subscribe(delegate
		{
			StartCoroutine(Back());
		});
		cgcGroup.Enable(_isEnable: false);
		cgcCoordinate.Enable(_isEnable: false);
		cgcPlayerSelect.Enable(_isEnable: false);
		cgcPlayerCoordinateSelect.Enable(_isEnable: false);
		base.enabled = true;
	}

	private IEnumerator Back()
	{
		Utils.Sound.Play(SystemSE.cancel);
		cgcGroup.Close();
		cgcCoordinate.Close();
		cgcPlayerSelect.Close();
		cgcPlayerCoordinateSelect.Close();
		yield return null;
		DeSelectItemUI(itemUIs[2]);
		DeSelectItemUI(itemUIs[3]);
		DeSelectItemUI(itemUIs[4]);
		DeSelectItemUI(itemUIs[5]);
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		instance.SetCameraPosition(0);
		instance.StartFade();
		instance.AnimationMenu(_isOpen: false);
		Singleton<HomeSceneManager>.Instance.SetModeCanvasGroup(0);
		instance.HelpPage = 0;
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (saveData.TutorialNo == 5)
		{
			saveData.TutorialNo = 6;
			instance.OCBTutorial.SetActiveToggle(5);
		}
		instance.CharaEventSet();
	}

	private void ItemSelect(MenuItemUI _ui)
	{
		Utils.Sound.Play(SystemSE.ok_s);
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		_ui.uiSlide.IsSlideAlways = true;
		_ui.texts.ForEach(delegate(Text t)
		{
			t.color = Game.selectFontColor;
		});
		switch (_ui.id)
		{
		case 0:
		{
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = strConfirm[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				Utils.Sound.Play(SystemSE.ok_l);
				hm.CharaEventSet();
				global::CharaCustom.CharaCustom.modeNew = true;
				global::CharaCustom.CharaCustom.modeSex = 1;
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "CharaCustom",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
				HomeScene.startCanvas = 2;
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				DeSelectItemUI(_ui);
			};
			ConfirmDialog.Load();
			break;
		}
		case 1:
		{
			ConfirmDialog.Status status2 = ConfirmDialog.status;
			status2.Sentence = strConfirm[Singleton<GameSystem>.Instance.languageInt];
			status2.Yes = delegate
			{
				Utils.Sound.Play(SystemSE.ok_l);
				hm.CharaEventSet();
				global::CharaCustom.CharaCustom.modeNew = true;
				global::CharaCustom.CharaCustom.modeSex = 0;
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "CharaCustom",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
				HomeScene.startCanvas = 2;
			};
			status2.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				DeSelectItemUI(_ui);
			};
			ConfirmDialog.Load();
			break;
		}
		case 2:
			groupCharaSelectUI.CreateList();
			groupCharaSelectUI.ListCtrl.SelectInfoClear();
			groupListUI.ListCtrl.SelectInfoClear();
			groupListUI.ListCtrl.SetNowSelectToggle();
			groupCharaSelectUI.ReDrawListView();
			cgcGroup.Open();
			if (Singleton<Game>.Instance.saveData.TutorialNo == 2)
			{
				Singleton<Game>.Instance.saveData.TutorialNo = 3;
				hm.OCBTutorial.SetActiveToggle(2);
			}
			break;
		case 3:
			coordinateGroupListUI.Create(CoordinateGroupListUI.CreateKind.SaveGroup);
			coordinateListUI.ListSelectRelease();
			coordinateListUI.selectGroup = Singleton<Game>.Instance.saveData.selectGroup;
			cgcCoordinate.Open();
			break;
		case 4:
			maleCharaSelectUI.CreateList();
			maleCharaSelectUI.ListCtrl.SelectInfoClear();
			maleCharaSelectUI.ReDrawListView();
			cgcPlayerSelect.Open();
			break;
		case 5:
			playerCoordinateUI.InitListSelect();
			playerCoordinateUI1.InitListSelect();
			cgcPlayerCoordinateSelect.Open();
			break;
		}
		MenuItemUI[] array = itemUIs;
		foreach (MenuItemUI menuItemUI in array)
		{
			if (menuItemUI != _ui)
			{
				menuItemUI.tgl.isOn = false;
			}
		}
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

	public void StartPlayerSelect()
	{
		_ = Singleton<HomeSceneManager>.Instance;
		MenuItemUI obj = itemUIs[4];
		obj.uiSlide.IsSlideAlways = true;
		obj.tgl.SetIsOnWithoutCallback(isOn: true);
		obj.texts.ForEach(delegate(Text t)
		{
			t.color = Game.selectFontColor;
		});
		maleCharaSelectUI.CreateList();
		maleCharaSelectUI.ListCtrl.SelectInfoClear();
		maleCharaSelectUI.ReDrawListView();
		cgcPlayerSelect.Open();
	}

	public void StartGroupSelect()
	{
		_ = Singleton<HomeSceneManager>.Instance;
		MenuItemUI obj = itemUIs[2];
		obj.uiSlide.IsSlideAlways = true;
		obj.tgl.SetIsOnWithoutCallback(isOn: true);
		obj.texts.ForEach(delegate(Text t)
		{
			t.color = Game.selectFontColor;
		});
		groupCharaSelectUI.CreateList();
		groupCharaSelectUI.ListCtrl.SelectInfoClear();
		groupListUI.ListCtrl.SelectInfoClear();
		groupListUI.ListCtrl.SetNowSelectToggle();
		groupCharaSelectUI.ReDrawListView();
		cgcGroup.Open();
	}

	public void StartCoordinateSelect()
	{
		_ = Singleton<HomeSceneManager>.Instance;
		MenuItemUI obj = itemUIs[3];
		obj.uiSlide.IsSlideAlways = true;
		obj.tgl.SetIsOnWithoutCallback(isOn: true);
		obj.texts.ForEach(delegate(Text t)
		{
			t.color = Game.selectFontColor;
		});
		coordinateGroupListUI.Create(CoordinateGroupListUI.CreateKind.SaveGroup);
		coordinateListUI.ListSelectRelease();
		cgcCoordinate.Open();
	}

	public void PlayerCoordinateButtonVisible()
	{
		itemUIs[5].tgl.gameObject.SetActiveIfDifferent(Singleton<Game>.Instance.saveData.TutorialNo == -1);
	}
}
