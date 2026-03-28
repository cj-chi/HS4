using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Actor;
using CharaCustom;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using Manager;
using SceneAssist;
using UIAnimatorCore;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class LobbySelectUI1 : MonoBehaviour
{
	private enum ItemKind
	{
		Urine,
		Perspiration,
		Sleep,
		Aphrodisiac
	}

	[SerializeField]
	private CanvasGroup cgItemWindow;

	[SerializeField]
	private LobbyCharaSelectInfoScrollController1 scrollCtrl;

	[SerializeField]
	private Button btnUseItem;

	[SerializeField]
	private UIAnimator itemUIAnimator;

	[SerializeField]
	private Button[] btnItems = new Button[4];

	[SerializeField]
	private RawImage riCard;

	[SerializeField]
	private GameObject objCard;

	[SerializeField]
	private Button btnCustom;

	[SerializeField]
	private Text textCustom;

	[SerializeField]
	private PointerEnterExitAction pointerEnterExitActionCustom;

	[SerializeField]
	private LobbyMainUI lobbyMainUI;

	private int entryCharaNo;

	private List<GameCharaFileInfo> charaLists = new List<GameCharaFileInfo>();

	private string oldCharaFileName = "";

	public int EntryCharaNo => entryCharaNo;

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
		btnUseItem.OnClickAsObservable().Subscribe(delegate
		{
			cgItemWindow.blocksRaycasts = false;
			itemUIAnimator.PlayAnimation((itemUIAnimator.CurrentAnimType != AnimSetupType.Outro) ? AnimSetupType.Outro : AnimSetupType.Intro, delegate
			{
				cgItemWindow.blocksRaycasts = true;
			});
		});
		btnItems[0].OnClickAsObservable().Subscribe(delegate
		{
			ItemSelectAction(0);
		});
		btnItems[1].OnClickAsObservable().Subscribe(delegate
		{
			ItemSelectAction(1);
		});
		btnItems[2].OnClickAsObservable().Subscribe(delegate
		{
			ItemSelectAction(2);
		});
		btnItems[3].OnClickAsObservable().Subscribe(delegate
		{
			ItemSelectAction(3);
		});
		btnCustom.OnClickAsObservable().Subscribe(delegate
		{
			Singleton<Character>.Instance.DeleteCharaAll();
			global::CharaCustom.CharaCustom.modeNew = false;
			global::CharaCustom.CharaCustom.modeSex = 1;
			global::CharaCustom.CharaCustom.editCharaFileName = scrollCtrl.selectInfo.info.FileName;
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "CharaCustom",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		});
		pointerEnterExitActionCustom.listActionEnter.Clear();
		pointerEnterExitActionCustom.listActionEnter.Add(delegate
		{
			textCustom.color = Game.selectFontColor;
		});
		pointerEnterExitActionCustom.listActionExit.Clear();
		pointerEnterExitActionCustom.listActionExit.Add(delegate
		{
			textCustom.color = Game.defaultFontColor;
		});
		itemUIAnimator.SetAnimType(AnimSetupType.Outro);
		itemUIAnimator.ResetToEnd();
		objCard.SetActiveIfDifferent(active: false);
		SaveData saveData = Singleton<Game>.Instance.saveData;
		GroupListUI.AddList(charaLists, saveData.roomList[saveData.selectGroup]);
		scrollCtrl.Init(charaLists);
		scrollCtrl.onSelect = delegate(LobbyCharaSelectInfoScrollController1.ScrollData _data)
		{
			LoadSelectCard(_data);
			if (entryCharaNo == 0)
			{
				btnUseItem.interactable = _data.info.usedItem == 0;
			}
			else
			{
				btnUseItem.interactable = false;
			}
			bool flag = false;
			if (Singleton<Game>.Instance.saveData.TutorialNo == 9)
			{
				flag = true;
			}
			lm.LoadChara(entryCharaNo, scrollCtrl.selectInfo.info.FullPath);
			if (entryCharaNo == 0 && !flag)
			{
				lm.SetCharaAnimationAndPosition();
			}
			if (lm.CGParameter.alpha < 1f)
			{
				lm.ParameterUI.InitParamUIAnimator();
			}
			lm.CGParameter.Enable(enable: true, isUseInteractable: false);
			lm.ParameterUI.SetParameter(_data.info, lm.eventNos[lm.heroineRommListIdx[entryCharaNo]], entryCharaNo);
			if (flag)
			{
				_data.rowData.row.interactableAlphaChanger.IsTextColorChange = false;
				lm.OCBTutorial.SetActive(_active: false);
				lm.OpenADV("adv/scenario/op/30/04.unity3d", "0", lm.heroines[0], delegate
				{
					Singleton<Game>.Instance.saveData.TutorialNo = 10;
					lm.OCBTutorial.SetActiveToggle(2);
				});
			}
			if (entryCharaNo == 0 && lm.heroines[1] != null && scrollCtrl.selectInfo.info.FullPath.Contains(lm.heroines[1].chaFile.charaFileName))
			{
				Singleton<Character>.Instance.DeleteChara(lm.heroines[1].chaCtrl);
				lm.heroines[1] = null;
			}
			lm.isEntry = true;
			lobbyMainUI.SetMenuItemInteractable(0, _interactable: true);
			if (SaveData.IsAchievementExchangeRelease(4))
			{
				lobbyMainUI.SetMenuItemInteractable(2, _interactable: true);
			}
			_data.rowData.row.text.color = Game.selectFontColor;
		};
		scrollCtrl.onPointEnter = delegate(LobbyCharaSelectInfoScrollController1.ScrollData _data)
		{
			LoadSelectCard(_data);
		};
		scrollCtrl.onPointExit = delegate
		{
			if (scrollCtrl.selectInfo != null)
			{
				LoadSelectCard(scrollCtrl.selectInfo);
			}
			else
			{
				objCard.SetActiveIfDifferent(active: false);
			}
		};
		base.enabled = true;
	}

	private IEnumerator PlayADV(Action _onEnd)
	{
		LobbySceneManager lm = Singleton<LobbySceneManager>.Instance;
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.In);
		yield return new WaitWhile(() => !Scene.IsFadeEnd);
		lm.OpenADV("adv/scenario/etc/30/34.unity3d", "0", lm.heroines[0], _onEnd);
	}

	private IEnumerator ReturnADV(LobbyCharaSelectInfoScrollController1.RowData _row)
	{
		yield return null;
		_row.row.interactableAlphaChanger.IsTextColorChange = true;
	}

	public void CancelProc()
	{
		LobbySceneManager instance = Singleton<LobbySceneManager>.Instance;
		bool flag = false;
		if (entryCharaNo == 1)
		{
			instance.CGParameter.Enable(enable: true, isUseInteractable: false);
			instance.ParameterUI.SetParameter(instance.heroines[0].chaFile, instance.eventNos[instance.heroineRommListIdx[0]], 0);
		}
		if (instance.heroines[0] == null || flag)
		{
			instance.SetCharaAnimationAndPosition();
		}
		objCard.SetActiveIfDifferent(active: false);
		btnUseItem.interactable = false;
		if (itemUIAnimator.CurrentAnimType == AnimSetupType.Intro)
		{
			itemUIAnimator.PlayAnimation(AnimSetupType.Outro);
		}
		instance.SetSelectCanvasGroup(_enable: false);
	}

	public void SetEntryCharaNo(int _entry, string _oldFileName)
	{
		LobbySceneManager instance = Singleton<LobbySceneManager>.Instance;
		LobbyCharaSelectInfoScrollController1.ScrollData scrollData = null;
		string text = "";
		if (instance.heroines[0] != null)
		{
			text = Path.GetFileNameWithoutExtension(instance.heroines[0].chaFile.charaFileName);
			scrollData = scrollCtrl.FindInfoByFileName(text);
			scrollData.info.isEntry = _entry != 0;
		}
		else if (scrollCtrl.selectInfo != null)
		{
			scrollCtrl.SelectInfoClear();
		}
		btnUseItem.interactable = false;
		bool flag = instance.heroines[_entry] != null;
		objCard.SetActiveIfDifferent(flag);
		if (flag)
		{
			text = Path.GetFileNameWithoutExtension(instance.heroines[_entry].chaFile.charaFileName);
			scrollData = scrollCtrl.FindInfoByFileName(text);
			scrollCtrl.SetSelectInfo(scrollData.index);
			scrollCtrl.SetNowLine();
			instance.ParameterUI.SetParameter(instance.heroines[_entry].chaFile, instance.eventNos[instance.heroineRommListIdx[_entry]], _entry);
			LoadSelectCard(scrollData);
			if (_entry == 0)
			{
				btnUseItem.interactable = instance.heroines[_entry].gameinfo2.usedItem == 0;
			}
		}
		else if (_entry == 1)
		{
			scrollCtrl.SelectInfoClear();
		}
		scrollCtrl.RefreshShown();
		if (itemUIAnimator.CurrentAnimType == AnimSetupType.Intro)
		{
			itemUIAnimator.PlayAnimation(AnimSetupType.Outro);
		}
		entryCharaNo = _entry;
		oldCharaFileName = _oldFileName;
		bool active = SaveData.IsAchievementExchangeRelease(0);
		btnUseItem.gameObject.SetActiveIfDifferent(active);
	}

	private void ItemSelectAction(int _item)
	{
		LobbySceneManager lm = Singleton<LobbySceneManager>.Instance;
		lm.CGMain.blocksRaycasts = false;
		Heroine heroine = lm.heroines[entryCharaNo];
		if (heroine != null)
		{
			heroine.gameinfo2.usedItem = _item + 1;
			heroine.chaFile.SaveCharaFile(heroine.chaFile.charaFileName);
		}
		btnUseItem.interactable = false;
		itemUIAnimator.PlayAnimation(AnimSetupType.Outro, delegate
		{
			lm.CGMain.blocksRaycasts = true;
		});
	}

	private void LoadSelectCard(LobbyCharaSelectInfoScrollController1.ScrollData _data)
	{
		objCard.SetActiveIfDifferent(active: true);
		if (riCard.texture != null)
		{
			UnityEngine.Object.Destroy(riCard.texture);
			riCard.texture = null;
		}
		if (_data.info.pngData != null || !_data.info.FullPath.IsNullOrEmpty())
		{
			riCard.texture = PngAssist.ChangeTextureFromByte(_data.info.pngData ?? PngFile.LoadPngBytes(_data.info.FullPath));
		}
	}
}
