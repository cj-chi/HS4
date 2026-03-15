using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class GroupCharaSelectUI : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public int id;

		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private Toggle[] tglFilters = new Toggle[6];

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
	private MenuItemUI[] itemUISystems = new MenuItemUI[2];

	[SerializeField]
	private GroupCharaParameterUI parameterUI;

	[SerializeField]
	private GroupListUI groupListUI;

	private List<GameCharaFileInfo> charaLists = new List<GameCharaFileInfo>();

	public GameCharaFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		foreach (var value in tglFilters.Select((Toggle tgl, int index) => new { tgl, index }))
		{
			value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate
			{
				ReDrawListView();
				if (listCtrl.selectInfo != null && listCtrl.selectInfo.info.lstFilter.Contains(value.index))
				{
					listCtrl.SelectInfoClear();
				}
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
		List<Toggle> list = new List<Toggle>(tglFilters.ToList());
		list.Add(tglSort);
		list.Add(tglSortKind);
		list.ForEach(delegate(Toggle tgl)
		{
			tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
			tgl.OnPointerClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
			});
		});
		itemUISystems[0].btn.OnClickAsObservable().Subscribe(delegate
		{
			Entry(listCtrl.selectInfo.info);
		});
		itemUISystems[0].btn.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (itemUISystems[0].btn.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				itemUISystems[0].texts.ForEach(delegate(Text t)
				{
					t.color = Game.selectFontColor;
				});
			}
		});
		itemUISystems[0].btn.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (itemUISystems[0].btn.IsInteractable())
			{
				itemUISystems[0].texts.ForEach(delegate(Text t)
				{
					t.color = Game.defaultFontColor;
				});
			}
		});
		CreateList();
		listCtrl.onSelect = delegate(GameCharaFileInfo _info)
		{
			EntryLimit();
			parameterUI.SetParameter(_info);
			groupListUI.ListCtrl.selectInfo?.rowData.row.tgl.SetIsOnWithoutCallback(isOn: false);
			groupListUI.ListCtrl.SelectInfoClear();
			SaveData saveData = Singleton<Game>.Instance.saveData;
			HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
			if (saveData.TutorialNo == 3)
			{
				saveData.TutorialNo = 4;
				instance.OCBTutorial.SetActiveToggle(3);
			}
		};
		listCtrl.onDeSelect = delegate
		{
			itemUISystems[0].btn.interactable = false;
			parameterUI.InitParameter();
		};
		listCtrl.onDoubleClick = delegate(GameCharaFileInfo _info)
		{
			if (IsEntryLimit())
			{
				Entry(_info);
			}
		};
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
		listCtrl.SelectInfoClear();
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

	private void Entry(GameCharaFileInfo _info)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		List<string> list = saveData.roomList[saveData.selectGroup];
		HomeSceneManager instance = Singleton<HomeSceneManager>.Instance;
		if (!list.Contains(_info.FileName))
		{
			Utils.Sound.Play(SystemSE.ok_s);
			list.Add(_info.FileName);
			if (!saveData.dicCloths[saveData.selectGroup].ContainsKey(_info.FileName))
			{
				saveData.dicCloths[saveData.selectGroup].Add(_info.FileName, new ClothPngInfo());
			}
			_info.lstFilter.Remove(0);
			_info.lstFilter.Add(saveData.selectGroup + 1);
			parameterUI.SetParameter(_info);
			if (_info.fic != null)
			{
				_info.fic.tgl.SetIsOnWithoutCallback(isOn: false);
				_info.fic.tgl.interactable = false;
			}
			listCtrl.SelectInfoClear();
			groupListUI.Create();
			if (saveData.TutorialNo == 4)
			{
				saveData.TutorialNo = 5;
				instance.OCBTutorial.SetActiveToggle(4);
			}
		}
	}

	private bool IsEntryLimit()
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		return saveData.roomList[saveData.selectGroup].Count < 20;
	}

	private void EntryLimit()
	{
		itemUISystems[0].btn.interactable = IsEntryLimit();
	}

	public void CreateList()
	{
		_ = Singleton<Game>.Instance.saveData;
		charaLists = GameCharaFileInfoAssist.CreateCharaFileInfoList(0, useMale: false, useFemale: true, useFemaleFutanariOnly: false, isCheckPlayerSelect: true);
		SortDate(tglSort.isOn);
	}

	public void ReDrawListView()
	{
		List<int> filters = (from v in tglFilters.Select((Toggle tgl, int index) => new { tgl, index })
			where v.tgl.isOn
			select v.index).ToList();
		listCtrl.Init(charaLists, filters);
	}
}
