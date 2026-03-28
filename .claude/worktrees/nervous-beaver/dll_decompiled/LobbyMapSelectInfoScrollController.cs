using System;
using System.Collections.Generic;
using System.Linq;
using HS2;
using Illusion.Extensions;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LobbyMapSelectInfoScrollController
{
	public class RowData
	{
		public int line = -1;

		public LobbyMapSelectInfoComponent.RowInfo row;

		public void MemberInit()
		{
			line = -1;
			row = null;
		}
	}

	public class ScrollData
	{
		public int index;

		public MapInfo.Param info;

		public bool isEnable = true;

		public bool isLock;

		public RowData rowData = new RowData();
	}

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 3;

	[SerializeField]
	private Text txtMapName;

	[SerializeField]
	private Button btnStart;

	[SerializeField]
	private int startMapIDOffset = 2;

	private ScrollData[] scrollerDatas;

	public Action<ScrollData> onSelect;

	public Action onDeSelect;

	public ScrollData selectInfo { get; private set; }

	public void Init(List<MapInfo.Param> _lst, int[] _useMaps)
	{
		scrollerDatas = _lst.Select((MapInfo.Param value, int idx) => new ScrollData
		{
			index = idx,
			info = value,
			isEnable = (_useMaps == null || _useMaps.Contains(value.No)),
			isLock = (!GlobalHS2Calc.IsAchievementMap0(value.No) || !GlobalHS2Calc.IsAchievementMap1(value.No) || ((value.No != 2 || !SaveData.IsAchievementExchangeRelease(13)) && value.No == 2))
		}).ToArray();
		int num = ((!((IReadOnlyCollection<ScrollData>)(object)scrollerDatas).IsNullOrEmpty()) ? (scrollerDatas.Length / countPerRow) : 0);
		if (!((IReadOnlyCollection<ScrollData>)(object)scrollerDatas).IsNullOrEmpty() && scrollerDatas.Length % countPerRow > 0)
		{
			num++;
		}
		if (!view.IsInit)
		{
			view.InitListView(-1, OnUpdate);
		}
		else
		{
			view.ReSetListItemCount(-1);
		}
		view.mOnSnapNearestChanged = OnSnapTargetChanged;
		List<int> source = (from v in scrollerDatas.ToForEachTuples()
			where !v.value.isLock && v.value.isEnable
			select v.index).ToList();
		SetLine(source.FirstOrDefault());
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

	public void NextLine()
	{
		int itemIndex = 0;
		if (selectInfo != null)
		{
			itemIndex = (selectInfo.index + 1) % scrollerDatas.Length;
		}
		view.MovePanelToItemIndex(itemIndex, 0f);
	}

	public void PreviousLine()
	{
		int itemIndex = 0;
		if (selectInfo != null)
		{
			int num = scrollerDatas.Length;
			itemIndex = ((selectInfo.index - 1) % num + num) % num;
		}
		view.MovePanelToItemIndex(itemIndex, 0f);
	}

	private bool IsNowSelectInfo(MapInfo.Param _info)
	{
		if (_info != null && selectInfo != null)
		{
			return selectInfo.info.No == _info.No;
		}
		return false;
	}

	private void OnClick(ScrollData _data)
	{
		selectInfo = _data;
		onSelect?.Invoke(selectInfo);
	}

	private LoopListViewItem2 OnUpdate(LoopListView2 _view, int _index)
	{
		LoopListViewItem2 loopListViewItem = _view.NewListViewItem(original.name);
		LobbyMapSelectInfoComponent component = loopListViewItem.GetComponent<LobbyMapSelectInfoComponent>();
		if (!loopListViewItem.IsInitHandlerCalled)
		{
			loopListViewItem.IsInitHandlerCalled = true;
		}
		int num = scrollerDatas.Length;
		int index = ((_index - startMapIDOffset) % num + num) % num;
		ScrollData data = scrollerDatas.SafeGet(index);
		component.SetData(0, _isFreeH: true, data, delegate
		{
			OnClick(data);
		});
		if (data != null)
		{
			if (data.rowData == null)
			{
				data.rowData = new RowData();
			}
			data.rowData.row = component.GetRow(0);
		}
		return loopListViewItem;
	}

	private void OnSnapTargetChanged(LoopListView2 listView, LoopListViewItem2 item)
	{
		int indexInShownItemList = listView.GetIndexInShownItemList(item);
		if (indexInShownItemList >= 0)
		{
			LobbyMapSelectInfoComponent.RowInfo row = item.GetComponent<LobbyMapSelectInfoComponent>().GetRow(0);
			selectInfo = row.scrollInfo;
			txtMapName.text = row.info.MapNames[0];
			btnStart.interactable = !row.objLock.activeSelf && row.btn.interactable;
			OnListViewSnapTargetChanged(listView, indexInShownItemList);
		}
	}

	private void OnListViewSnapTargetChanged(LoopListView2 listView, int targetIndex)
	{
		int shownItemCount = listView.ShownItemCount;
		for (int i = 0; i < shownItemCount; i++)
		{
			LobbyMapSelectInfoComponent.RowInfo row = listView.GetShownItemByIndex(i).GetComponent<LobbyMapSelectInfoComponent>().GetRow(0);
			if (i == targetIndex)
			{
				row.btn.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
			}
			else
			{
				row.btn.transform.localScale = Vector3.one;
			}
		}
	}
}
