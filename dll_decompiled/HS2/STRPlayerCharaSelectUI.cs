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

public class STRPlayerCharaSelectUI : MonoBehaviour
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

	public Action<GameCharaFileInfo> OnSelect;

	public Action OnDeSelect;

	private List<GameCharaFileInfo> charaLists = new List<GameCharaFileInfo>();

	private List<GameCharaFileInfo> charaMaleLists = new List<GameCharaFileInfo>();

	private List<GameCharaFileInfo> charaFemaleLists = new List<GameCharaFileInfo>();

	public GameCharaFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<SpecialTreatmentRoomManager>.IsInstance());
		foreach (var value in tglFilters.Select((Toggle tgl, int index) => new { tgl, index }))
		{
			value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
			{
				if (_isON)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					if (listCtrl.selectInfo != null)
					{
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
					SaveData saveData = Singleton<Game>.Instance.saveData;
					SetPlayerSelect(saveData.playerChara.FileName);
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
		CreateList();
		listCtrl.onSelect = delegate(GameCharaFileInfo _info)
		{
			OnSelect?.Invoke(_info);
		};
		listCtrl.onDeSelect = delegate
		{
			OnDeSelect?.Invoke();
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

	public void SetPlayerSelect(string _fileName)
	{
		listCtrl.SetToggle(_fileName, _isSetNowLine: false, _isOnSelectProc: false);
		listCtrl.selectInfo?.rowData.row.tgl.SetIsOnWithoutCallback(isOn: true);
		listCtrl.SetNowSelectName();
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
	}

	public void ReDrawListView()
	{
		List<int> filters = (from v in tglFilters.Select((Toggle tgl, int index) => new { tgl, index })
			where v.tgl.isOn
			select v.index).ToList();
		listCtrl.Init(charaLists, filters);
	}
}
