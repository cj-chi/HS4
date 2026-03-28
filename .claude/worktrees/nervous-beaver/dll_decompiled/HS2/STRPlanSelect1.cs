using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Config;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UGUI_AssistLibrary;
using UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class STRPlanSelect1 : MonoBehaviour
{
	private enum PlanCategory
	{
		None = -1,
		Favor,
		Enjoyment,
		Aversion,
		Slavery,
		Broken,
		Dependence
	}

	[Serializable]
	public class MenuItemUI
	{
		public Toggle tgl;

		public UIObjectSlideOnCursor uiSlide;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private MenuItemUI[] itemUIs;

	[SerializeField]
	private MenuItemUI itemStart;

	[SerializeField]
	private Button btnBack;

	[SerializeField]
	private CanvasGroupCtrl cgcBase;

	[SerializeField]
	private STRCharaSelectUI1 partnerSelectUI;

	[SerializeField]
	private STRCoordinateListUI1 partnerCoordinateUI;

	[SerializeField]
	private STRCoordinateListUI1 playerCoordinateUI;

	[SerializeField]
	private ToggleGroup toggleGroup;

	private PlanCategory nowSelectCategory = PlanCategory.None;

	public STRCharaSelectUI1 PartnerSelectUI => partnerSelectUI;

	public STRCoordinateListUI1 PartnerCoordinateUI => partnerCoordinateUI;

	public STRCoordinateListUI1 PlayerCoordinateUI => playerCoordinateUI;

	public ReactiveProperty<STRCharaFileInfo1> PartnerFileInfo { get; set; } = new ReactiveProperty<STRCharaFileInfo1>(null);

	private IEnumerator Start()
	{
		base.enabled = false;
		while (!Singleton<GameSystem>.IsInstance())
		{
			yield return null;
		}
		while (!SingletonInitializer<Scene>.initialized)
		{
			yield return null;
		}
		while (!Singleton<SpecialTreatmentRoomManager1>.IsInstance())
		{
			yield return null;
		}
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		foreach (var item in itemUIs.ToForEachTuples())
		{
			var (value, index) = item;
			value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
			{
				if (_isOn)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					value.uiSlide.IsSlideAlways = true;
					if (value.tgl.IsInteractable())
					{
						value.texts.ForEach(delegate(Text t)
						{
							t.color = Game.selectFontColor;
						});
					}
					switch (index)
					{
					case 0:
						cgcBase.Open();
						nowSelectCategory = PlanCategory.Favor;
						partnerSelectUI.CreateList(1);
						partnerCoordinateUI.ListSelectRelease();
						playerCoordinateUI.ListSelectRelease();
						TutorialSelect();
						break;
					case 1:
						cgcBase.Open();
						nowSelectCategory = PlanCategory.Enjoyment;
						partnerSelectUI.CreateList(2);
						partnerCoordinateUI.ListSelectRelease();
						playerCoordinateUI.ListSelectRelease();
						TutorialSelect();
						break;
					case 2:
						cgcBase.Open();
						nowSelectCategory = PlanCategory.Aversion;
						partnerSelectUI.CreateList(3);
						partnerCoordinateUI.ListSelectRelease();
						playerCoordinateUI.ListSelectRelease();
						TutorialSelect();
						break;
					case 3:
						cgcBase.Open();
						nowSelectCategory = PlanCategory.Slavery;
						partnerSelectUI.CreateList(4);
						partnerCoordinateUI.ListSelectRelease();
						playerCoordinateUI.ListSelectRelease(_isInteractable: false);
						TutorialSelect();
						break;
					case 4:
						cgcBase.Open();
						nowSelectCategory = PlanCategory.Broken;
						partnerSelectUI.CreateList(5);
						partnerCoordinateUI.ListSelectRelease();
						playerCoordinateUI.ListSelectRelease();
						TutorialSelect();
						break;
					case 5:
						cgcBase.Open();
						nowSelectCategory = PlanCategory.Dependence;
						partnerSelectUI.CreateList(6);
						partnerCoordinateUI.ListSelectRelease();
						playerCoordinateUI.ListSelectRelease();
						TutorialSelect();
						break;
					}
				}
				else
				{
					value.uiSlide.IsSlideAlways = false;
					if (value.tgl.IsInteractable())
					{
						value.texts.ForEach(delegate(Text t)
						{
							t.color = Game.defaultFontColor;
						});
					}
					if (!toggleGroup.AnyTogglesOn())
					{
						cgcBase.Close();
						partnerSelectUI.ListCtrl.SelectInfoClear();
						PartnerFileInfo.Value = null;
						nowSelectCategory = PlanCategory.None;
					}
				}
			});
			value.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (value.tgl.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
				}
			});
		}
		itemStart.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
		{
			if (_isOn)
			{
				Utils.Sound.Play(SystemSE.ok_s);
				Entry();
				if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == 3)
				{
					strm.OCBTutorial.SetActiveToggle(3);
					strm.OpenADV("adv/scenario/op_append/50/01.unity3d", "2", 1, 0, _isCameraDontMove: true, _isUseCorrectCamera: false, _isCharaBackUpPos: true, _isCameraDontMoveRelease: true, delegate
					{
						Singleton<Game>.Instance.appendSaveData.AppendTutorialNo = 4;
					});
				}
			}
		});
		itemStart.tgl.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
		});
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			strm.StartFade();
			strm.SetModeCanvasGroup(0);
			strm.SetCameraPosition(0, strm.ConciergeChaCtrl.GetShapeBodyValue(0));
			strm.AnimationMenu(_isOpen: false);
			MenuItemUI[] array = itemUIs;
			foreach (MenuItemUI ui in array)
			{
				DeSelectItemUI(ui);
			}
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (btnBack.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
			}
		});
		PartnerFileInfo.Select((STRCharaFileInfo1 s) => s != null).SubscribeToInteractable(itemStart.tgl);
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !strm.IsADV
			where !Scene.IsFadeNow
			where strm.CGModes[1].alpha > 0.9f
			where strm.CGModes[2].alpha < 0.9f
			where Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == -1
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			strm.StartFade();
			strm.SetModeCanvasGroup(0);
			strm.SetCameraPosition(0, strm.ConciergeChaCtrl.GetShapeBodyValue(0));
			strm.AnimationMenu(_isOpen: false);
		});
		cgcBase.Enable(_isEnable: false);
		base.enabled = true;
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

	private void PlayPlan()
	{
		Game game = Singleton<Game>.Instance;
		HSceneManager instance = Singleton<HSceneManager>.Instance;
		ADVManager instance2 = Singleton<ADVManager>.Instance;
		Dictionary<string, Game.EventCharaInfo> source = game.tableLobbyEvents[game.saveData.selectGroup];
		game.tableDesireCharas.Clear();
		(from c in source
			where PartnerFileInfo.Value.FileName != c.Value.fileName
			where Game.DesireEventIDs.Contains(c.Value.eventID)
			select c).ToList().ForEach(delegate(KeyValuePair<string, Game.EventCharaInfo> c)
		{
			game.tableDesireCharas.Add(Path.GetFileNameWithoutExtension(c.Value.fileName), c.Value.eventID);
		});
		game.eventNo = (int)(50 + nowSelectCategory);
		game.peepKind = -1;
		game.isConciergeAngry = false;
		STRCoordinateFileScrollController1.ScrollData selectInfo = partnerCoordinateUI.ListCtrl.selectInfo;
		game.appendCoordinateFemale = ((selectInfo != null) ? selectInfo.info.FileName : "");
		selectInfo = playerCoordinateUI.ListCtrl.selectInfo;
		game.appendCoordinatePlayer = ((selectInfo != null) ? selectInfo.info.FileName : "");
		instance2.filenames[0] = PartnerFileInfo.Value.FileName;
		instance2.filenames[1] = "";
		int[] array = new int[6] { 18, 19, 21, 20, 23, 22 };
		game.mapNo = array[(int)nowSelectCategory];
		game.heroineList.Clear();
		instance.females = new ChaControl[2];
		instance.pngFemales[0] = "";
		instance.pngFemales[1] = "";
		instance.mapID = game.mapNo;
		PlayerCharaSaveInfo playerChara = game.saveData.playerChara;
		instance.pngMale = playerChara.FileName;
		instance.bFutanari = playerChara.Futanari;
		instance.pngMaleSecond = "";
		instance.bFutanariSecond = false;
		instance2.advDelivery.Set("0", PartnerFileInfo.Value.personality, game.eventNo);
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

	private void TutorialSelect()
	{
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == 1)
		{
			strm.OpenADV("adv/scenario/op_append/50/01.unity3d", "0", 1, 0, _isCameraDontMove: true, _isUseCorrectCamera: false, _isCharaBackUpPos: true, _isCameraDontMoveRelease: true, delegate
			{
				Singleton<Game>.Instance.appendSaveData.AppendTutorialNo = 2;
				strm.OCBTutorial.SetActiveToggle(1);
			});
		}
	}

	private void Entry()
	{
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		itemStart.uiSlide.IsSlideAlways = true;
		itemStart.texts.ForEach(delegate(Text t)
		{
			t.color = Game.selectFontColor;
		});
		strm.StartFade();
		strm.SetModeCanvasGroup(2);
		strm.Confirmation.onEntry = delegate
		{
			if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == 4)
			{
				strm.OCBTutorial.SetActive(_active: false);
				strm.CGBase.Enable(enable: false);
				strm.OpenADV("adv/scenario/op_append/50/01.unity3d", "3", 1, 1, _isCameraDontMove: true, _isUseCorrectCamera: false, _isCharaBackUpPos: false, _isCameraDontMoveRelease: false, delegate
				{
					Singleton<Game>.Instance.appendSaveData.AppendTutorialNo = 5;
					strm.MainCamera.fieldOfView = 40f;
					PlayPlan();
				});
			}
			else
			{
				PlayPlan();
			}
		};
		strm.Confirmation.onCancel = delegate
		{
			DeSelectItemUI(itemStart);
		};
		STRCoordinateFileInfo1 sTRCoordinateFileInfo = partnerCoordinateUI.ListCtrl.selectInfo?.info;
		STRCoordinateFileInfo1 sTRCoordinateFileInfo2 = playerCoordinateUI.ListCtrl.selectInfo?.info;
		string text = "";
		text = SaveData.CreatePlayerPngPath(Singleton<Game>.Instance.saveData.playerChara);
		string playerName = "";
		ChaFileControl chaFileControl = new ChaFileControl();
		if (chaFileControl.LoadCharaFile(text))
		{
			playerName = chaFileControl.parameter.fullname;
		}
		STRConfirmation1.PlanInfo info = new STRConfirmation1.PlanInfo
		{
			plan = (int)nowSelectCategory,
			partnerCardFile = PartnerFileInfo.Value.FullPath,
			partnerName = PartnerFileInfo.Value.name,
			partnerCoordinateFileName = ((sTRCoordinateFileInfo != null) ? sTRCoordinateFileInfo.FullPath : ""),
			partnerCoordinateName = ((sTRCoordinateFileInfo != null) ? sTRCoordinateFileInfo.name : ""),
			playerCardFile = text,
			playerName = playerName,
			playerCoordinateFileName = ((sTRCoordinateFileInfo2 != null) ? sTRCoordinateFileInfo2.FullPath : ""),
			playerCoordinateName = ((sTRCoordinateFileInfo2 != null) ? sTRCoordinateFileInfo2.name : "")
		};
		strm.Confirmation.SetInfo(info);
	}

	public void InitSelect()
	{
		cgcBase.Enable(_isEnable: false);
		PartnerFileInfo.Value = null;
		MenuItemUI[] array = itemUIs;
		foreach (MenuItemUI ui in array)
		{
			DeSelectItemUI(ui);
		}
		Game instance = Singleton<Game>.Instance;
		foreach (var (interactable, num) in instance.IsAllState(instance.saveData.selectGroup).ToForEachTuples())
		{
			itemUIs[num].tgl.interactable = interactable;
		}
	}
}
