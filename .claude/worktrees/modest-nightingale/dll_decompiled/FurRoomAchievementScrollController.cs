using System;
using System.Collections.Generic;
using System.Linq;
using SuperScrollView;
using UnityEngine;

[Serializable]
public class FurRoomAchievementScrollController
{
	public class RowData
	{
		public int line = -1;

		public int index = -1;

		public FurRoomAchievementInfoComponent.RowInfo row;

		public void MemberInit()
		{
			line = -1;
			index = -1;
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

	public ScrollData GetScrollData(int _index)
	{
		return scrollerDatas.SafeGet(_index);
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

	public void UpdateNowDraw()
	{
		for (int i = 0; i < view.ShownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
			if (!(shownItemByIndex == null))
			{
				FurRoomAchievementInfoComponent component = shownItemByIndex.GetComponent<FurRoomAchievementInfoComponent>();
				for (int j = 0; j < countPerRow; j++)
				{
					component.SetDataDraw(component.GetRow(j), component.GetListInfo(j));
				}
			}
		}
	}

	private void OnValueChange(ScrollData _data)
	{
		selectInfo = _data;
		onSelect?.Invoke(selectInfo);
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
		FurRoomAchievementInfoComponent component = loopListViewItem.GetComponent<FurRoomAchievementInfoComponent>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData data = scrollerDatas.SafeGet(index);
			component.SetData(i, data.info, delegate
			{
				OnValueChange(data);
			});
			if (data != null)
			{
				if (data.rowData == null)
				{
					data.rowData = new RowData();
				}
				data.rowData.line = i;
				data.rowData.index = index;
				data.rowData.row = component.GetRow(i);
			}
		}
		return loopListViewItem;
	}
}
