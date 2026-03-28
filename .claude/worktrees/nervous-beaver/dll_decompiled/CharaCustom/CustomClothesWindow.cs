using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomClothesWindow : MonoBehaviour
{
	[Serializable]
	public class SortWindow
	{
		public GameObject objWinSort;

		public Button btnCloseWinSort;

		public Toggle[] tglSort;
	}

	[SerializeField]
	private CustomClothesScrollController cscClothes;

	[SerializeField]
	private SortWindow winSort;

	[SerializeField]
	private Button btnShowWinSort;

	[SerializeField]
	private Toggle tglSortOrder;

	[SerializeField]
	private Toggle[] tglLoadOption;

	[SerializeField]
	private Button[] button;

	private IntReactiveProperty _sortType = new IntReactiveProperty(0);

	private IntReactiveProperty _sortOrder = new IntReactiveProperty(1);

	private List<CustomClothesFileInfo> lstClothes;

	public Action<CustomClothesFileInfo> onClick01;

	public Action<CustomClothesFileInfo> onClick02;

	public Action<CustomClothesFileInfo> onClick03;

	public bool btnDisableNotSelect01;

	public bool btnDisableNotSelect02;

	public bool btnDisableNotSelect03;

	public int sortType
	{
		get
		{
			return _sortType.Value;
		}
		set
		{
			_sortType.Value = value;
		}
	}

	public int sortOrder
	{
		get
		{
			return _sortOrder.Value;
		}
		set
		{
			_sortOrder.Value = value;
		}
	}

	public void UpdateWindow(bool modeNew, int sex, bool save)
	{
		if (tglLoadOption != null && tglLoadOption.Length > 4 && (bool)tglLoadOption[4])
		{
			tglLoadOption[4].gameObject.SetActiveIfDifferent(modeNew);
		}
		lstClothes = CustomClothesFileInfoAssist.CreateClothesFileInfoList(sex == 0, 1 == sex, useMyData: true, !save);
		Sort();
	}

	public void Sort()
	{
		if (lstClothes == null)
		{
			return;
		}
		if (lstClothes.Count == 0)
		{
			cscClothes.CreateList(lstClothes);
			return;
		}
		using (new GameSystem.CultureScope())
		{
			if (sortType == 0)
			{
				if (sortOrder == 0)
				{
					lstClothes = (from n in lstClothes
						orderby n.time, n.name
						select n).ToList();
				}
				else
				{
					lstClothes = (from n in lstClothes
						orderby n.time descending, n.name descending
						select n).ToList();
				}
			}
			else if (sortOrder == 0)
			{
				lstClothes = (from n in lstClothes
					orderby n.name, n.time
					select n).ToList();
			}
			else
			{
				lstClothes = (from n in lstClothes
					orderby n.name descending, n.time descending
					select n).ToList();
			}
			cscClothes.CreateList(lstClothes);
		}
	}

	public void SelectInfoClear()
	{
		if (null != cscClothes)
		{
			cscClothes.SelectInfoClear();
		}
	}

	public void Start()
	{
		btnShowWinSort.OnClickAsObservable().Subscribe(delegate
		{
			winSort.objWinSort.SetActiveIfDifferent(!winSort.objWinSort.activeSelf);
		});
		winSort.btnCloseWinSort.OnClickAsObservable().Subscribe(delegate
		{
			winSort.objWinSort.SetActiveIfDifferent(active: false);
		});
		if (winSort.tglSort.Any())
		{
			(from tgl in winSort.tglSort.Select((Toggle val, int idx) => new { val, idx })
				where tgl.val != null
				select tgl).ToList().ForEach(tgl =>
			{
				(from isOn in tgl.val.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					sortType = tgl.idx;
				});
			});
		}
		tglSortOrder.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
		{
			sortOrder = ((!isOn) ? 1 : 0);
		});
		_sortType.Subscribe(delegate
		{
			Sort();
		});
		_sortOrder.Subscribe(delegate
		{
			Sort();
		});
		if (button == null || 3 != button.Length)
		{
			return;
		}
		if ((bool)button[0])
		{
			button[0].OnClickAsObservable().Subscribe(delegate
			{
				onClick01?.Invoke(cscClothes.selectInfo?.info);
			});
			button[0].UpdateAsObservable().Subscribe(delegate
			{
				button[0].interactable = !btnDisableNotSelect01 || cscClothes.selectInfo != null;
			});
		}
		if ((bool)button[1])
		{
			button[1].OnClickAsObservable().Subscribe(delegate
			{
				onClick02?.Invoke(cscClothes.selectInfo?.info);
			});
			button[1].UpdateAsObservable().Subscribe(delegate
			{
				button[1].interactable = !btnDisableNotSelect02 || cscClothes.selectInfo != null;
			});
		}
		if ((bool)button[2])
		{
			button[2].OnClickAsObservable().Subscribe(delegate
			{
				onClick03?.Invoke(cscClothes.selectInfo?.info);
			});
			button[2].UpdateAsObservable().Subscribe(delegate
			{
				button[2].interactable = !btnDisableNotSelect03 || cscClothes.selectInfo != null;
			});
		}
	}
}
