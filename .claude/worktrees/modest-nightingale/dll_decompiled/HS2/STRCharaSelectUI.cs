using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameLoadCharaFileSystem;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class STRCharaSelectUI : MonoBehaviour
{
	[SerializeField]
	private Toggle tglSort;

	[SerializeField]
	private Image imgSort;

	[SerializeField]
	private Toggle tglSortKind;

	[SerializeField]
	private Image imgSortKind;

	[SerializeField]
	private STRCharaFileScrollController listCtrl = new STRCharaFileScrollController();

	public Action<STRCharaFileInfo> OnSelect;

	public Action OnDeSelect;

	private STRCharaFileInfoAssist charaAssist = new STRCharaFileInfoAssist();

	private List<STRCharaFileInfo> charaLists = new List<STRCharaFileInfo>();

	private List<int> filters = new List<int> { 0, 1, 2, 3, 4, 5 };

	public STRCharaFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<SpecialTreatmentRoomManager>.IsInstance());
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
			tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
			tgl.OnPointerClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
			});
		});
		listCtrl.onSelect = delegate(STRCharaFileInfo _info)
		{
			OnSelect?.Invoke(_info);
		};
		listCtrl.onDeSelect = delegate
		{
			OnDeSelect?.Invoke();
		};
		base.enabled = true;
	}

	private void SortDate(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
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
	}

	private void SortName(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
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
	}

	private void Entry(GameCharaFileInfo _info)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		List<string> list = saveData.roomList[saveData.selectGroup];
		_ = Singleton<HomeSceneManager>.Instance;
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
			listCtrl.SelectInfoClear();
		}
	}

	public void CreateList(int _state)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		charaLists = charaAssist.CreateCharaFileInfoList(_state, saveData.roomList[saveData.selectGroup]);
		SortDate(tglSort.isOn);
		listCtrl.SelectInfoClear();
		listCtrl.Init(charaLists);
	}

	public void ReDrawListView()
	{
		listCtrl.Init(charaLists);
	}
}
