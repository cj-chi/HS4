using System;
using System.Collections.Generic;
using System.Linq;
using SuperScrollView;
using UnityEngine;

[Serializable]
public class ConciergeAchievementItemScrollController
{
	public class RowData
	{
		public int line = -1;

		public ConciergeAchievementItemInfoComponent.RowInfo row;

		public void MemberInit()
		{
			line = -1;
			row = null;
		}
	}

	public class ScrollData
	{
		public int index;

		public (SaveData.AchievementState, AchievementInfoData.Param) info;

		public RowData rowData = new RowData();
	}

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 3;

	private ScrollData[] scrollerDatas;

	public Action<ScrollData> onSelect;

	public ScrollData selectInfo { get; private set; }

	public void Init(List<(SaveData.AchievementState, AchievementInfoData.Param)> _lst)
	{
		scrollerDatas = _lst.Select(((SaveData.AchievementState, AchievementInfoData.Param) value, int idx) => new ScrollData
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

	public void SetToggle(int _index)
	{
		selectInfo = scrollerDatas[_index];
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

	public void SetNowLine()
	{
		int itemIndex = 0;
		if (selectInfo != null)
		{
			itemIndex = selectInfo.index / countPerRow;
		}
		view.MovePanelToItemIndex(itemIndex, 0f);
	}

	private bool IsNowSelectInfo((SaveData.AchievementState, AchievementInfoData.Param) _info)
	{
		if (_info.Item2 != null && selectInfo != null)
		{
			return selectInfo.info.Item2.id == _info.Item2.id;
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
		ConciergeAchievementItemInfoComponent component = loopListViewItem.GetComponent<ConciergeAchievementItemInfoComponent>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData scrollData = scrollerDatas.SafeGet(index);
			component.SetData(i, scrollData.info);
			if (scrollData != null)
			{
				if (scrollData.rowData == null)
				{
					scrollData.rowData = new RowData();
				}
				scrollData.rowData.line = i;
				scrollData.rowData.row = component.GetRow(i);
			}
		}
		return loopListViewItem;
	}
}
