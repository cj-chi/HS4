using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using SuperScrollView;
using UnityEngine;

namespace CoordinateFileSystem;

[Serializable]
public class CoordinateFileScrollController
{
	public class RowData
	{
		public int index = -1;

		public int line = -1;

		public CoordinateFileInfoComponent.RowInfo row;

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

		public CoordinateFileInfo info;

		public RowData rowData = new RowData();
	}

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 3;

	private ScrollData[] scrollerDatas;

	public Action<CoordinateFileInfo> onSelect;

	public Action<CoordinateFileInfo> onDeSelect;

	public ScrollData selectInfo { get; private set; }

	public void Init(List<CoordinateFileInfo> _lst)
	{
		scrollerDatas = _lst.Select((CoordinateFileInfo value, int idx) => new ScrollData
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
		onDeSelect?.Invoke(null);
	}

	public void SetTopLine()
	{
		if (view.IsInit)
		{
			view.MovePanelToItemIndex(0, 0f);
		}
	}

	public void SetToggle(string _fileName)
	{
		selectInfo = scrollerDatas.FirstOrDefault((ScrollData data) => data.info.FileName == _fileName);
		view.RefreshAllShownItem();
	}

	public void RefreshShown()
	{
		view.RefreshAllShownItem();
	}

	public CoordinateFileInfo FindInfoByFileName(string _fileName)
	{
		return scrollerDatas.FirstOrDefault((ScrollData d) => d.info.FileName == _fileName)?.info;
	}

	public void SetNowSelectToggle()
	{
		for (int i = 0; i < view.ShownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
			if (!(shownItemByIndex == null))
			{
				CoordinateFileInfoComponent component = shownItemByIndex.GetComponent<CoordinateFileInfoComponent>();
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
			}
		}
		else if (IsNowSelectInfo(_data?.info))
		{
			onDeSelect?.Invoke(selectInfo.info);
			selectInfo = null;
		}
	}

	private bool IsNowSelectInfo(CoordinateFileInfo _info)
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
		CoordinateFileInfoComponent component = loopListViewItem.GetComponent<CoordinateFileInfoComponent>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData data = scrollerDatas.SafeGet(index);
			component.SetData(i, data?.info, delegate(bool _isOn)
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
