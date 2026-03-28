using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using CharaCustom;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class MaleCharaSelectUI : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private Toggle[] tglFilters = new Toggle[2];

	[SerializeField]
	private Toggle tglSort;

	[SerializeField]
	private Image imgSort;

	[SerializeField]
	private Toggle tglSortKind;

	[SerializeField]
	private Image imgSortKind;

	[SerializeField]
	private GameCharaFileScrollController listCtrl = new GameCharaFileScrollController();

	[SerializeField]
	private MenuItemUI[] itemUISystems = new MenuItemUI[3];

	[SerializeField]
	private CanvasGroup cgSystemBtn;

	[SerializeField]
	private MaleSelectParameterUI parameterUI;

	[SerializeField]
	private MaleSelectParameterUI parameterUI1;

	[SerializeField]
	private GameObject objSecondCharaParameter;

	[Header("")]
	[SerializeField]
	private MaleSelectParameterUI coordinateParameterUI;

	[SerializeField]
	private MaleSelectParameterUI coordinateParameterUI1;

	private List<GameCharaFileInfo> charaLists = new List<GameCharaFileInfo>();

	private List<GameCharaFileInfo> charaMaleLists = new List<GameCharaFileInfo>();

	private List<GameCharaFileInfo> charaFemaleLists = new List<GameCharaFileInfo>();

	private readonly string[] strConfirm = new string[6] { "キャラのメイク画面に移動します\nよろしいですか？", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "" };

	public GameCharaFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		foreach (var value in tglFilters.Select((Toggle tgl, int index) => new { tgl, index }))
		{
			value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
			{
				if (_isON)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					string text = string.Empty;
					if (listCtrl.selectInfo != null)
					{
						text = listCtrl.selectInfo.info.FileName;
						listCtrl.SelectInfoClear();
					}
					charaLists = new List<GameCharaFileInfo>((value.index == 0) ? charaMaleLists : charaFemaleLists);
					if (tglSortKind.isOn)
					{
						SortDate(tglSort.isOn);
					}
					else
					{
						SortName(tglSort.isOn);
					}
					ReDrawListView();
					if (!text.IsNullOrEmpty())
					{
						listCtrl.SetToggle(text);
					}
				}
			});
			value.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		}
		tglSort.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
		{
			imgSort.enabled = !_isON;
			if (tglSortKind.isOn)
			{
				SortDate(_isON);
			}
			else
			{
				SortName(_isON);
			}
			ReDrawListView();
		});
		tglSortKind.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
		{
			imgSortKind.enabled = !_isON;
			if (_isON)
			{
				SortDate(tglSort.isOn);
			}
			else
			{
				SortName(tglSort.isOn);
			}
			ReDrawListView();
		});
		List<Toggle> list = new List<Toggle>();
		list.Add(tglSort);
		list.Add(tglSortKind);
		list.ForEach(delegate(Toggle tgl)
		{
			tgl.OnPointerClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
			});
			tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		});
		itemUISystems[0].btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = strConfirm[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				GameCharaFileInfo info = listCtrl.selectInfo.info;
				global::CharaCustom.CharaCustom.modeNew = false;
				global::CharaCustom.CharaCustom.modeSex = (byte)info.sex;
				global::CharaCustom.CharaCustom.isPlayer = true;
				global::CharaCustom.CharaCustom.editCharaFileName = info.FileName;
				Singleton<HomeSceneManager>.Instance.CharaEventSet();
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "CharaCustom",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
				HomeScene.startCanvas = 2;
				HomeScene.startCharaEdit = 4;
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		});
		itemUISystems[1].btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			Entry(listCtrl.selectInfo.info, 0);
		});
		itemUISystems[2].btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			Entry(listCtrl.selectInfo.info, 1);
		});
		MenuItemUI[] array = itemUISystems;
		foreach (MenuItemUI ui in array)
		{
			ui.btn.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (ui.btn.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.selectFontColor;
					});
				}
			});
			ui.btn.OnPointerExitAsObservable().Subscribe(delegate
			{
				if (ui.btn.IsInteractable())
				{
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.defaultFontColor;
					});
				}
			});
		}
		CreateList();
		listCtrl.onSelect = delegate
		{
			cgSystemBtn.interactable = true;
		};
		listCtrl.onDeSelect = delegate
		{
			cgSystemBtn.interactable = false;
		};
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
		listCtrl.SelectInfoClear();
		SaveData saveData = Singleton<Game>.Instance.saveData;
		SetPlayerParameter(saveData.playerChara, parameterUI);
		SetPlayerParameter(saveData.secondPlayerChara, parameterUI1);
		SetPlayerParameter(saveData.playerChara, coordinateParameterUI);
		SetPlayerParameter(saveData.secondPlayerChara, coordinateParameterUI1);
		base.enabled = true;
	}

	private void SortDate(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
		}
		GameCharaFileInfo gameCharaFileInfo = charaLists.Find((GameCharaFileInfo i) => i.personality == "不明");
		if (gameCharaFileInfo != null)
		{
			charaLists.Remove(gameCharaFileInfo);
		}
		using (new GameSystem.CultureScope())
		{
			if (ascend)
			{
				charaLists = (from n in charaLists
					orderby n.time, n.name, n.personality
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.time descending, n.name descending, n.personality descending
					select n).ToList();
			}
		}
		if (gameCharaFileInfo != null)
		{
			charaLists.Insert(0, gameCharaFileInfo);
		}
	}

	private void SortName(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
		}
		GameCharaFileInfo gameCharaFileInfo = charaLists.Find((GameCharaFileInfo i) => i.personality == "不明");
		if (gameCharaFileInfo != null)
		{
			charaLists.Remove(gameCharaFileInfo);
		}
		using (new GameSystem.CultureScope())
		{
			if (ascend)
			{
				charaLists = (from n in charaLists
					orderby n.name, n.time, n.personality
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.name descending, n.time descending, n.personality descending
					select n).ToList();
			}
		}
		if (gameCharaFileInfo != null)
		{
			charaLists.Insert(0, gameCharaFileInfo);
		}
	}

	private void SetPlayerParameter(PlayerCharaSaveInfo _info, MaleSelectParameterUI _parameterUI)
	{
		if (_info.FileName.IsNullOrEmpty())
		{
			_parameterUI.InitParameter();
			return;
		}
		ChaFileControl chaFileControl = new ChaFileControl();
		string text = UserData.Path + ((_info.Sex == 0) ? "chara/male/" : "chara/female/") + _info.FileName + ".png";
		if (!chaFileControl.LoadCharaFile(text, (byte)_info.Sex))
		{
			chaFileControl.GetLastErrorCode();
			return;
		}
		_parameterUI.SetParameter(new GameCharaFileInfo
		{
			FullPath = text,
			name = chaFileControl.parameter.fullname
		});
	}

	private void Entry(GameCharaFileInfo _info, int _entry)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		PlayerCharaSaveInfo playerCharaSaveInfo = ((_entry == 0) ? saveData.playerChara : saveData.secondPlayerChara);
		if (!(playerCharaSaveInfo.FileName == _info.FileName))
		{
			int sex = playerCharaSaveInfo.Sex;
			playerCharaSaveInfo.FileName = _info.FileName;
			playerCharaSaveInfo.Sex = _info.sex;
			playerCharaSaveInfo.Futanari = _info.futanari;
			if (_entry == 0)
			{
				parameterUI.SetParameter(_info);
				coordinateParameterUI.SetParameter(_info);
			}
			else
			{
				parameterUI1.SetParameter(_info);
				coordinateParameterUI1.SetParameter(_info);
			}
			if (sex != playerCharaSaveInfo.Sex)
			{
				saveData.playerCloths[_entry].file = string.Empty;
				saveData.playerCloths[_entry].sex = 0;
			}
		}
	}

	public void CreateList()
	{
		charaMaleLists = GameCharaFileInfoAssist.CreateCharaFileInfoList(1, useMale: true, useFemale: false, useFemaleFutanariOnly: true, isCheckPlayerSelect: false, isCheckRoomSelect: true);
		charaFemaleLists = GameCharaFileInfoAssist.CreateCharaFileInfoList(1, useMale: false, useFemale: true, useFemaleFutanariOnly: true, isCheckPlayerSelect: false, isCheckRoomSelect: true);
		charaLists = new List<GameCharaFileInfo>(tglFilters[0].isOn ? charaMaleLists : charaFemaleLists);
		if (tglSortKind.isOn)
		{
			SortDate(tglSort.isOn);
		}
		else
		{
			SortName(tglSort.isOn);
		}
		bool active = SaveData.IsAchievementExchangeRelease(5);
		objSecondCharaParameter.SetActiveIfDifferent(active);
		itemUISystems[2].btn.gameObject.SetActiveIfDifferent(active);
	}

	public void ReDrawListView()
	{
		List<int> filters = (from v in tglFilters.Select((Toggle tgl, int index) => new { tgl, index })
			where v.tgl.isOn
			select v.index).ToList();
		listCtrl.Init(charaLists, filters);
	}
}
