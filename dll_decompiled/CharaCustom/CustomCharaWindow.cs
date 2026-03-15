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

public class CustomCharaWindow : MonoBehaviour
{
	public enum LoadOption
	{
		Face = 1,
		Body = 2,
		Hair = 4,
		Coorde = 8,
		Status = 0x10
	}

	[Serializable]
	public class SortWindow
	{
		public GameObject objWinSort;

		public Button btnCloseWinSort;

		public Toggle[] tglSort;
	}

	public CustomCharaScrollController cscChara;

	[SerializeField]
	private SortWindow winSort;

	[SerializeField]
	private Button btnShowWinSort;

	[SerializeField]
	private Toggle tglSortType;

	[SerializeField]
	private Toggle tglSortOrder;

	[SerializeField]
	private Toggle[] tglLoadOption;

	[SerializeField]
	private Button[] button;

	private IntReactiveProperty _sortType = new IntReactiveProperty(0);

	private IntReactiveProperty _sortOrder = new IntReactiveProperty(1);

	private List<CustomCharaFileInfo> lstChara;

	public Action<CustomCharaFileInfo> onClick01;

	public Action<CustomCharaFileInfo> onClick02;

	public Action<CustomCharaFileInfo, int> onClick03;

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

	public void UpdateWindow(bool modeNew, int sex, bool save, List<CustomCharaFileInfo> _lst = null)
	{
		if (tglLoadOption != null && tglLoadOption.Length > 4 && null != tglLoadOption[4])
		{
			tglLoadOption[4].gameObject.SetActiveIfDifferent(modeNew);
		}
		if (_lst == null)
		{
			lstChara = CustomCharaFileInfoAssist.CreateCharaFileInfoList(sex == 0, 1 == sex, useMyData: true, useDownload: true, usePreset: false, save);
		}
		else
		{
			lstChara = _lst;
		}
		Sort();
	}

	public void UpdateWindowInUploader(bool modeNew, int sex, bool save, List<CustomCharaFileInfo> _lst = null)
	{
		if (tglLoadOption != null && tglLoadOption.Length > 4 && null != tglLoadOption[4])
		{
			tglLoadOption[4].gameObject.SetActiveIfDifferent(modeNew);
		}
		if (_lst == null)
		{
			lstChara = CustomCharaFileInfoAssist.CreateCharaFileInfoList(sex == 0, 1 == sex, useMyData: true, useDownload: false, usePreset: false, save);
		}
		else
		{
			lstChara = _lst;
		}
		Sort();
	}

	public void Sort()
	{
		if (lstChara == null)
		{
			return;
		}
		if (lstChara.Count == 0)
		{
			cscChara.CreateList(lstChara);
			return;
		}
		using (new GameSystem.CultureScope())
		{
			if (sortType == 0)
			{
				if (sortOrder == 0)
				{
					lstChara = (from n in lstChara
						orderby n.time, n.name, n.personality
						select n).ToList();
				}
				else
				{
					lstChara = (from n in lstChara
						orderby n.time descending, n.name descending, n.personality descending
						select n).ToList();
				}
			}
			else if (sortOrder == 0)
			{
				lstChara = (from n in lstChara
					orderby n.name, n.time, n.personality
					select n).ToList();
			}
			else
			{
				lstChara = (from n in lstChara
					orderby n.name descending, n.time descending, n.personality descending
					select n).ToList();
			}
			cscChara.CreateList(lstChara);
		}
	}

	public void SelectInfoClear()
	{
		if (null != cscChara)
		{
			cscChara.SelectInfoClear();
		}
	}

	public CustomCharaScrollController.ScrollData GetSelectInfo()
	{
		if (null != cscChara)
		{
			return cscChara.selectInfo;
		}
		return null;
	}

	public void Start()
	{
		if (null != btnShowWinSort)
		{
			btnShowWinSort.OnClickAsObservable().Subscribe(delegate
			{
				winSort.objWinSort.SetActiveIfDifferent(!winSort.objWinSort.activeSelf);
			});
		}
		if (null != winSort.btnCloseWinSort)
		{
			winSort.btnCloseWinSort.OnClickAsObservable().Subscribe(delegate
			{
				winSort.objWinSort.SetActiveIfDifferent(active: false);
			});
		}
		if (winSort.tglSort != null && winSort.tglSort.Any())
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
		if (null != tglSortType)
		{
			tglSortType.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				sortType = ((!isOn) ? 1 : 0);
			});
		}
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
		if (null != button[0])
		{
			button[0].OnClickAsObservable().Subscribe(delegate
			{
				onClick01?.Invoke(cscChara.selectInfo?.info);
			});
			button[0].UpdateAsObservable().Subscribe(delegate
			{
				button[0].interactable = !btnDisableNotSelect01 || cscChara.selectInfo != null;
			});
		}
		if (null != button[1])
		{
			button[1].OnClickAsObservable().Subscribe(delegate
			{
				onClick02?.Invoke(cscChara.selectInfo?.info);
			});
			button[1].UpdateAsObservable().Subscribe(delegate
			{
				button[1].interactable = !btnDisableNotSelect02 || cscChara.selectInfo != null;
			});
		}
		if (!(null != button[2]))
		{
			return;
		}
		button[2].OnClickAsObservable().Subscribe(delegate
		{
			int num = 0;
			if (tglLoadOption[0].isOn)
			{
				num |= 1;
			}
			if (tglLoadOption[1].isOn)
			{
				num |= 2;
			}
			if (tglLoadOption[2].isOn)
			{
				num |= 4;
			}
			if (tglLoadOption[3].isOn)
			{
				num |= 8;
			}
			if (tglLoadOption[4].isOn)
			{
				num |= 0x10;
			}
			onClick03?.Invoke(cscChara.selectInfo?.info, num);
		});
		button[2].UpdateAsObservable().Subscribe(delegate
		{
			button[2].interactable = !btnDisableNotSelect03 || cscChara.selectInfo != null;
		});
	}
}
