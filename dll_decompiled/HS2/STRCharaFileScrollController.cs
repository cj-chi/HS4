using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

[Serializable]
public class STRCharaFileScrollController
{
	public class RowData
	{
		public int index = -1;

		public int line = -1;

		public STRCharaFileInfoComponent.RowInfo row;

		public void MemberInit()
		{
			index = -1;
			line = -1;
			row = null;
		}
	}

	public class ScrollData
	{
		public int index;

		public STRCharaFileInfo info;

		public RowData rowData = new RowData();
	}

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 3;

	[SerializeField]
	private bool isSameGroupCheck = true;

	[SerializeField]
	private Text txtName;

	private ScrollData[] scrollerDatas;

	public Action<STRCharaFileInfo> onSelect;

	public Action onDeSelect;

	public int CountPerRow => countPerRow;

	public ScrollData selectInfo { get; private set; }

	public void Init(List<STRCharaFileInfo> _lst)
	{
		scrollerDatas = _lst.Select((STRCharaFileInfo value, int idx) => new ScrollData
		{
			index = idx,
			info = value
		}).ToArray();
		int num = ((!((IReadOnlyCollection<ScrollData>)(object)scrollerDatas).IsNullOrEmpty()) ? (scrollerDatas.Length / countPerRow) : 0);
		if (!((IReadOnlyCollection<ScrollData>)(object)scrollerDatas).IsNullOrEmpty() && scrollerDatas.Length % countPerRow > 0)
		{
			num++;
		}
		if (!view.IsInit)
		{
			view.InitListViewAndSize(num, OnUpdate);
		}
		else
		{
			view.ReSetListItemCount(num);
		}
	}

	public void SelectInfoClear()
	{
		selectInfo = null;
		onDeSelect?.Invoke();
		txtName.text = "";
	}

	public void SetTopLine()
	{
		if (view.IsInit)
		{
			view.MovePanelToItemIndex(0, 0f);
		}
	}

	public void SetToggle(int _index)
	{
		selectInfo = scrollerDatas[_index];
		view.RefreshAllShownItem();
	}

	public void RefreshShown()
	{
		view.RefreshAllShownItem();
	}

	public STRCharaFileInfo FindInfoByFileName(string _fileName)
	{
		return scrollerDatas.FirstOrDefault((ScrollData d) => d.info.FileName == _fileName)?.info;
	}

	public void SetToggle(string _fileName, bool _isSetNowLine = false)
	{
		selectInfo = scrollerDatas.FirstOrDefault((ScrollData d) => d.info.FileName == _fileName);
		if (selectInfo != null)
		{
			onSelect?.Invoke(selectInfo.info);
		}
		if (_isSetNowLine)
		{
			SetNowLine();
		}
		view.RefreshAllShownItem();
	}

	public void SetNowSelectToggle()
	{
		for (int i = 0; i < view.ShownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
			if (!(shownItemByIndex == null))
			{
				STRCharaFileInfoComponent component = shownItemByIndex.GetComponent<STRCharaFileInfoComponent>();
				for (int j = 0; j < countPerRow; j++)
				{
					component.SetToggleON(j, IsNowSelectInfo(component.GetListInfo(j)));
				}
			}
		}
	}

	public void SetLine(int _line)
	{
		view.MovePanelToItemIndex(_line, 0f);
	}

	public void SetNowLine()
	{
		int itemIndex = 0;
		if (selectInfo != null)
		{
			itemIndex = selectInfo.index / countPerRow;
		}
		view.MovePanelToItemIndex(itemIndex, 0f);
	}

	private void OnValueChange(ScrollData _data, bool _isOn)
	{
		if (_isOn)
		{
			bool num = !IsNowSelectInfo(_data?.info);
			ScrollData scrollData = selectInfo;
			selectInfo = _data;
			if (num)
			{
				scrollData?.rowData.row.tgl.SetIsOnWithoutCallback(isOn: false);
				onSelect?.Invoke(selectInfo.info);
				txtName.text = selectInfo.info.name;
			}
		}
		else if (IsNowSelectInfo(_data?.info))
		{
			selectInfo = null;
			onDeSelect?.Invoke();
			txtName.text = "";
		}
	}

	private bool IsNowSelectInfo(STRCharaFileInfo _info)
	{
		if (_info != null && selectInfo != null)
		{
			return selectInfo.info.FullPath == _info.FullPath;
		}
		return false;
	}

	private LoopListViewItem2 OnUpdate(LoopListView2 _view, int _index)
	{
		if (_index < 0)
		{
			return null;
		}
		LoopListViewItem2 loopListViewItem = _view.NewListViewItem(original.name);
		STRCharaFileInfoComponent component = loopListViewItem.GetComponent<STRCharaFileInfoComponent>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData data = scrollerDatas.SafeGet(index);
			component.SetData(i, isSameGroupCheck, data?.info, delegate(bool _isOn)
			{
				OnValueChange(data, _isOn);
			});
			component.SetToggleON(i, IsNowSelectInfo(data?.info));
			if (data != null)
			{
				if (data.rowData == null)
				{
					data.rowData = new RowData();
				}
				data.rowData.index = index;
				data.rowData.line = i;
				data.rowData.row = component.GetRow(i);
			}
		}
		return loopListViewItem;
	}
}
