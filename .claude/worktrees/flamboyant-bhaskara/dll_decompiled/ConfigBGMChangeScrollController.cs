using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Manager;
using SuperScrollView;
using UnityEngine;

[Serializable]
public class ConfigBGMChangeScrollController
{
	public class RowData
	{
		public int line = -1;

		public ConfigBGMChangeComponent.RowInfo row;

		public void MemberInit()
		{
			line = -1;
			row = null;
		}
	}

	public class ScrollData
	{
		public int index;

		public BGMNameInfo.Param info;

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

	public void Init(List<BGMNameInfo.Param> _lst)
	{
		scrollerDatas = _lst.Select((BGMNameInfo.Param value, int idx) => new ScrollData
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
		selectInfo = scrollerDatas.FirstOrDefault((ScrollData d) => d.info.bgmID == _index);
		SetNowSelectToggle();
	}

	private void SetNowSelectToggle()
	{
		for (int i = 0; i < view.ShownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
			if (!(shownItemByIndex == null))
			{
				ConfigBGMChangeComponent component = shownItemByIndex.GetComponent<ConfigBGMChangeComponent>();
				for (int j = 0; j < countPerRow; j++)
				{
					component.SetToggleON(j, IsNowSelectInfo(component.GetListInfo(j)));
				}
			}
		}
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

	private bool IsNowSelectInfo(BGMNameInfo.Param _info)
	{
		if (_info != null && selectInfo != null)
		{
			return selectInfo.info.id == _info.id;
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
				if (scrollData != null)
				{
					scrollData.rowData.row.tgl.SetIsOnWithoutCallback(isOn: false);
					scrollData.rowData.row.text.color = Game.defaultFontColor;
				}
				selectInfo.rowData.row.text.color = Game.selectFontColor;
			}
		}
		else if (IsNowSelectInfo(_data?.info))
		{
			selectInfo.rowData.row.tgl.SetIsOnWithoutCallback(isOn: true);
		}
	}

	private LoopListViewItem2 OnUpdate(LoopListView2 _view, int _index)
	{
		if (_index < 0)
		{
			return null;
		}
		LoopListViewItem2 loopListViewItem = _view.NewListViewItem(original.name);
		ConfigBGMChangeComponent component = loopListViewItem.GetComponent<ConfigBGMChangeComponent>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData data = scrollerDatas.SafeGet(index);
			component.SetData(i, data.info, delegate(bool _isOn)
			{
				OnValueChange(data, _isOn);
			});
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
