using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharaCustom;
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

public class STRPlanSelect : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public Button btn;

		public UIObjectSlideOnCursor uiSlide;

		public List<Text> texts = new List<Text>();

		public Action noCharaPlanEnter;

		public Action noCharaPlanExit;
	}

	[SerializeField]
	private MenuItemUI[] itemUIs;

	[SerializeField]
	private Button btnBack;

	[SerializeField]
	private RawImage riMap;

	[SerializeField]
	private ObjectCategoryBehaviour categoryNoCharactorPlan;

	public void RiMapVisible(bool _isVisible)
	{
		riMap.gameObject.SetActiveIfDifferent(_isVisible);
	}

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
		while (!Singleton<SpecialTreatmentRoomManager>.IsInstance())
		{
			yield return null;
		}
		SpecialTreatmentRoomManager strm = Singleton<SpecialTreatmentRoomManager>.Instance;
		foreach (var item in itemUIs.ToForEachTuples())
		{
			var (value, index) = item;
			value.btn.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
				value.uiSlide.IsSlideAlways = true;
				if (value.btn.IsInteractable())
				{
					value.texts.ForEach(delegate(Text t)
					{
						t.color = Game.selectFontColor;
					});
				}
				switch (index)
				{
				case 0:
					Select(SpecialTreatmentRoomManager.PlanCategory.Favor);
					break;
				case 1:
					Select(SpecialTreatmentRoomManager.PlanCategory.Enjoyment);
					break;
				case 2:
					Select(SpecialTreatmentRoomManager.PlanCategory.Aversion);
					break;
				case 3:
					Select(SpecialTreatmentRoomManager.PlanCategory.Slavery);
					break;
				case 4:
					Select(SpecialTreatmentRoomManager.PlanCategory.Broken);
					break;
				case 5:
					Select(SpecialTreatmentRoomManager.PlanCategory.Dependence);
					break;
				}
			});
			value.btn.OnPointerEnterAsObservable().Subscribe(delegate
			{
				value.noCharaPlanEnter?.Invoke();
				if (value.btn.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
					riMap.gameObject.SetActiveIfDifferent(active: true);
					MapInfo.Param param = BaseMap.infoTable[SpecialTreatmentRoomManager.mapNos[index]];
					riMap.texture = CommonLib.LoadAsset<Texture2D>(param.ThumbnailBundle_L, param.ThumbnailAsset_L, clone: false, param.ThumbnailManifest_L);
					AssetBundleManager.UnloadAssetBundle(param.ThumbnailBundle_L, isUnloadForceRefCount: true, param.ThumbnailManifest_L);
					categoryNoCharactorPlan.SetActive(_active: false);
				}
			});
			value.btn.OnPointerExitAsObservable().Subscribe(delegate
			{
				value.noCharaPlanExit?.Invoke();
			});
		}
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			strm.StartFade();
			strm.SetModeCanvasGroup(0);
			strm.SetCameraPosition(0, strm.ConciergeChaCtrl.GetShapeBodyValue(0));
			strm.AnimationMenu(_isOpen: false);
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (btnBack.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
			}
		});
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
		riMap.gameObject.SetActiveIfDifferent(active: false);
		categoryNoCharactorPlan.SetActive(_active: false);
		base.enabled = true;
	}

	public void DeSelectItemUI(MenuItemUI _ui)
	{
		_ui.uiSlide.IsSlideAlways = false;
		if (_ui.btn.IsInteractable())
		{
			_ui.texts.ForEach(delegate(Text t)
			{
				t.color = Game.defaultFontColor;
			});
		}
	}

	public void AllDeSelectItemUI()
	{
		MenuItemUI[] array = itemUIs;
		foreach (MenuItemUI ui in array)
		{
			DeSelectItemUI(ui);
		}
	}

	private void TutorialSelect()
	{
		SpecialTreatmentRoomManager strm = Singleton<SpecialTreatmentRoomManager>.Instance;
		if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == 1)
		{
			strm.OpenADV("adv/scenario/op_append/50/01.unity3d", "0", 1, 0, _isCameraDontMove: true, _isUseCorrectCamera: false, _isCharaBackUpPos: true, _isCameraDontMoveRelease: true, delegate
			{
				Singleton<Game>.Instance.appendSaveData.AppendTutorialNo = 2;
				strm.OCBTutorial.SetActiveToggle(1);
			});
		}
	}

	private void Select(SpecialTreatmentRoomManager.PlanCategory _category)
	{
		SpecialTreatmentRoomManager instance = Singleton<SpecialTreatmentRoomManager>.Instance;
		instance.StartFade();
		instance.SetModeCanvasGroup(2);
		instance.nowSelectCategory = _category;
		instance.Confirmation.PartnerSelectUI.CreateList((int)(_category + 1));
		instance.Confirmation.PartnerCoordinateUI.ListSelectRelease();
		instance.Confirmation.PlayerCoordinateUI.ListSelectRelease();
		instance.Confirmation.SetInfo((int)instance.nowSelectCategory);
		if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo != -1)
		{
			instance.OCBTutorial.SetActiveToggle(1);
		}
	}

	public void InitSelect()
	{
		AllDeSelectItemUI();
		riMap.gameObject.SetActiveIfDifferent(active: false);
		Game instance = Singleton<Game>.Instance;
		foreach (var item2 in instance.IsAllState(instance.saveData.selectGroup).ToForEachTuples())
		{
			bool item = item2.value;
			int index = item2.index;
			MenuItemUI menuItemUI = itemUIs[index];
			menuItemUI.btn.interactable = item;
			menuItemUI.noCharaPlanEnter = null;
			menuItemUI.noCharaPlanExit = null;
			if (!item)
			{
				menuItemUI.noCharaPlanEnter = delegate
				{
					riMap.gameObject.SetActiveIfDifferent(active: false);
					categoryNoCharactorPlan.SetActiveToggle(index);
				};
			}
		}
	}
}
