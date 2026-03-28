using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Actor;
using CharaCustom;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using SceneAssist;
using UIAnimatorCore;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class LobbySelectUI : MonoBehaviour
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
	private LobbyCharaSelectInfoScrollController scrollCtrl;

	[SerializeField]
	private Button btnUseItem;

	[SerializeField]
	private UIAnimator itemUIAnimator;

	[SerializeField]
	private Button[] btnItems = new Button[4];

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

	private readonly string[] strConfirm = new string[6] { "キャラのメイク画面に移動します\nよろしいですか？", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "" };

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
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = strConfirm[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				Singleton<Character>.Instance.DeleteCharaAll();
				global::CharaCustom.CharaCustom.modeNew = false;
				global::CharaCustom.CharaCustom.modeSex = 1;
				global::CharaCustom.CharaCustom.editCharaFileName = scrollCtrl.selectInfo.info.FileName;
				Singleton<Game>.Instance.customCharaFileName = global::CharaCustom.CharaCustom.editCharaFileName;
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "CharaCustom",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		});
		btnCustom.interactable = false;
		pointerEnterExitActionCustom.listActionEnter.Clear();
		pointerEnterExitActionCustom.listActionEnter.Add(delegate
		{
			if (btnCustom.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				textCustom.color = Game.selectFontColor;
			}
		});
		pointerEnterExitActionCustom.listActionExit.Clear();
		pointerEnterExitActionCustom.listActionExit.Add(delegate
		{
			if (btnCustom.IsInteractable())
			{
				textCustom.color = Game.defaultFontColor;
			}
		});
		List<Button> list = new List<Button>(btnItems);
		list.Add(btnUseItem);
		list.ForEach(delegate(Button bt)
		{
			bt.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
			});
			bt.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (bt.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
				}
			});
		});
		itemUIAnimator.SetAnimType(AnimSetupType.Outro);
		itemUIAnimator.ResetToEnd();
		SaveData saveData = Singleton<Game>.Instance.saveData;
		GroupListUI.AddList(charaLists, saveData.roomList[saveData.selectGroup]);
		scrollCtrl.Init(charaLists);
		scrollCtrl.onSelect = delegate(LobbyCharaSelectInfoScrollController.ScrollData _data)
		{
			if (entryCharaNo == 0)
			{
				btnUseItem.interactable = _data.info.hcount != 0 && _data.info.usedItem == 0;
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
			if (lm.CGParameter.alpha < 1f)
			{
				lm.ParameterUI.InitParamUIAnimator();
			}
			lm.CGParameter.Enable(enable: true, isUseInteractable: false);
			lm.ParameterUI.SetParameter(_data.info, lm.eventNos[lm.heroineRommListIdx[entryCharaNo]], entryCharaNo);
			if (itemUIAnimator.CurrentAnimType == AnimSetupType.Intro)
			{
				itemUIAnimator.PlayAnimation(AnimSetupType.Outro, delegate
				{
					cgItemWindow.blocksRaycasts = true;
				});
			}
			if (flag)
			{
				lm.OCBTutorial.SetActive(_active: false);
				string bundle = (GameSystem.isAdd50 ? "adv/scenario/op/50/04.unity3d" : "adv/scenario/op/30/04.unity3d");
				lm.OpenADV(bundle, "0", lm.heroines[0], delegate
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
			if (!flag)
			{
				lm.SetCharaAnimationAndPosition();
			}
			btnCustom.interactable = true;
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
		LobbyCharaSelectInfoScrollController.ScrollData scrollData = null;
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
		if (instance.heroines[_entry] != null)
		{
			Heroine heroine = instance.heroines[_entry];
			text = Path.GetFileNameWithoutExtension(heroine.chaFile.charaFileName);
			scrollData = scrollCtrl.FindInfoByFileName(text);
			scrollCtrl.SetSelectInfo(scrollData.index);
			scrollCtrl.SetNowLine();
			instance.ParameterUI.SetParameter(heroine.chaFile, instance.eventNos[instance.heroineRommListIdx[_entry]], _entry);
			if (_entry == 0)
			{
				btnUseItem.interactable = heroine.gameinfo2.hCount != 0 && heroine.gameinfo2.usedItem == 0;
			}
			btnCustom.interactable = true;
		}
		else if (_entry == 1)
		{
			scrollCtrl.SelectInfoClear();
			btnCustom.interactable = false;
		}
		scrollCtrl.EntryNo = _entry;
		scrollCtrl.RefreshShown();
		if (itemUIAnimator.CurrentAnimType == AnimSetupType.Intro)
		{
			itemUIAnimator.PlayAnimation(AnimSetupType.Outro);
		}
		entryCharaNo = _entry;
		oldCharaFileName = _oldFileName;
		SetItemActive();
	}

	private void SetItemActive(bool _forceDisable = false)
	{
		bool flag = SaveData.IsAchievementExchangeRelease(10);
		btnUseItem.gameObject.SetActiveIfDifferent(flag && !_forceDisable);
	}

	private void ItemSelectAction(int _item)
	{
		LobbySceneManager lm = Singleton<LobbySceneManager>.Instance;
		lm.CGMain.blocksRaycasts = false;
		Heroine heroine = lm.heroines[entryCharaNo];
		if (heroine != null)
		{
			heroine.gameinfo2.usedItem = _item + 1;
			string charaFileName = heroine.chaFile.charaFileName;
			heroine.chaFile.SaveCharaFile(charaFileName);
			charaFileName = Path.GetFileNameWithoutExtension(charaFileName);
			LobbyCharaSelectInfoScrollController.ScrollData scrollData = scrollCtrl.FindInfoByFileName(charaFileName);
			if (scrollData != null)
			{
				scrollData.info.usedItem = heroine.gameinfo2.usedItem;
			}
		}
		btnUseItem.interactable = false;
		itemUIAnimator.PlayAnimation(AnimSetupType.Outro, delegate
		{
			lm.CGMain.blocksRaycasts = true;
		});
	}
}
