using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoordinateFileSystem;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class STRCoordinateListUI : MonoBehaviour
{
	[SerializeField]
	private bool isPlayer;

	[SerializeField]
	private Toggle tglSort;

	[SerializeField]
	private Image imgSort;

	[SerializeField]
	private Toggle tglSortKind;

	[SerializeField]
	private Image imgSortKind;

	[SerializeField]
	private STRCoordinateFileScrollController listCtrl = new STRCoordinateFileScrollController();

	public Action<STRCoordinateFileInfo> OnSelect;

	public Action OnDeSelect;

	private List<STRCoordinateFileInfo> charaLists = new List<STRCoordinateFileInfo>();

	public STRCoordinateFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<SpecialTreatmentRoomManager>.IsInstance());
		tglSort.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
		{
			Utils.Sound.Play(SystemSE.ok_s);
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
			Utils.Sound.Play(SystemSE.ok_s);
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
				if (tgl.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
				}
			});
		});
		CreateList();
		listCtrl.onSelect = delegate(STRCoordinateFileInfo _info)
		{
			OnSelect?.Invoke(_info);
		};
		listCtrl.onDeSelect = delegate
		{
			OnDeSelect?.Invoke();
		};
		listCtrl.Init(charaLists);
		listCtrl.SelectInfoClear();
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
					orderby n.time, n.name
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.time descending, n.name descending
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
					orderby n.name, n.time
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.name descending, n.time descending
					select n).ToList();
			}
		}
	}

	private void Entry(CoordinateFileInfo _info)
	{
		_ = Singleton<Game>.Instance.saveData;
	}

	private void Relelase(CoordinateFileInfo _info)
	{
		_ = Singleton<Game>.Instance.saveData;
	}

	public void CreateList()
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		int num = 0;
		num = ((!isPlayer) ? 1 : saveData.playerChara.Sex);
		charaLists = STRCoordinateFileInfoAssist.CreateCharaFileInfoList(num);
		SortDate(tglSort.isOn);
	}

	public void ReDrawListView()
	{
		listCtrl.Init(charaLists);
	}

	public void InitListSelect(bool _isInteractable)
	{
		listCtrl.Init(charaLists, _isInteractable);
	}

	public void ListSelectRelease(bool _isInteractable = true)
	{
		listCtrl.SelectInfoClear();
		listCtrl.Init(charaLists, _isInteractable);
	}
}
