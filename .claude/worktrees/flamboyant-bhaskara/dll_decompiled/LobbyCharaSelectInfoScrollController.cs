using System;
using System.Collections.Generic;
using System.Linq;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using SuperScrollView;
using UnityEngine;

[Serializable]
public class LobbyCharaSelectInfoScrollController
{
	public class RowData
	{
		public int line = -1;

		public LobbyCharaSelectInfoComponent.RowInfo row;

		public void MemberInit()
		{
			line = -1;
			row = null;
		}
	}

	public class ScrollData
	{
		public int index;

		public GameCharaFileInfo info;

		public RowData rowData = new RowData();
	}

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 1;

	private ScrollData[] scrollerDatas;

	public Action<ScrollData> onSelect;

	public Action<ScrollData> onPointEnter;

	public Action onPointExit;

	public int EntryNo { get; set; }

	public ScrollData selectInfo { get; private set; }

	public void Init(List<GameCharaFileInfo> _lst)
	{
		scrollerDatas = _lst.Select((GameCharaFileInfo value, int idx) => new ScrollData
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
	}

	public void SetTopLine()
	{
		if (view.IsInit)
		{
			view.MovePanelToItemIndex(0, 0f);
		}
	}

	public void SetSelectInfo(int _index)
	{
		selectInfo = scrollerDatas.FirstOrDefault((ScrollData d) => d.index == _index);
	}

	public void SetToggle(int _index)
	{
		selectInfo = scrollerDatas.FirstOrDefault((ScrollData d) => d.index == _index);
		view.RefreshAllShownItem();
	}

	public void RefreshShown()
	{
		view.RefreshAllShownItem();
	}

	public void SetLine(int _line)
	{
		view.MovePanelToItemIndex(_line, 0f);
	}

	public ScrollData FindInfoByFileName(string _fileName)
	{
		return scrollerDatas.FirstOrDefault((ScrollData d) => d.info.FileName == _fileName);
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

	private bool IsNowSelectInfo(GameCharaFileInfo _info)
	{
		if (_info != null && selectInfo != null)
		{
			return selectInfo.info.FullPath == _info.FullPath;
		}
		return false;
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
				scrollData?.rowData.row.toggle.SetIsOnWithoutCallback(isOn: false);
				onSelect?.Invoke(selectInfo);
			}
		}
		else if (IsNowSelectInfo(_data?.info))
		{
			selectInfo.rowData.row.toggle.SetIsOnWithoutCallback(isOn: true);
		}
	}

	private LoopListViewItem2 OnUpdate(LoopListView2 _view, int _index)
	{
		if (_index < 0)
		{
			return null;
		}
		LoopListViewItem2 loopListViewItem = _view.NewListViewItem(original.name);
		LobbyCharaSelectInfoComponent component = loopListViewItem.GetComponent<LobbyCharaSelectInfoComponent>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData data = scrollerDatas.SafeGet(index);
			component.SetData(i, data?.info, delegate(bool _isOn)
			{
				OnValueChange(data, _isOn);
			}, delegate
			{
				onPointEnter?.Invoke(data);
			}, delegate
			{
				onPointExit?.Invoke();
			}, EntryNo);
			component.SetToggleON(i, IsNowSelectInfo(data?.info));
			if (data != null)
			{
				if (data.rowData == null)
				{
					data.rowData = new RowData();
				}
				data.rowData.line = i;
				data.rowData.row = component.GetRow(i);
			}
		}
		return loopListViewItem;
	}
}
