using System;
using System.Collections.Generic;
using System.Linq;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

[Serializable]
public class CustomCharaScrollController : MonoBehaviour
{
	public class ScrollData
	{
		public int index;

		public CustomCharaFileInfo info;

		public Toggle toggle;
	}

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 3;

	[SerializeField]
	private Text text;

	private string SelectName = "";

	private ScrollData[] scrollerDatas;

	public Action<CustomCharaFileInfo> onSelect;

	public Action onDeSelect;

	public ScrollData selectInfo { get; private set; }

	public void CreateList(List<CustomCharaFileInfo> _lst)
	{
		scrollerDatas = _lst.Select((CustomCharaFileInfo value, int idx) => new ScrollData
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
		SelectName = "";
		if ((bool)text)
		{
			text.text = "";
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

	public void SetNowSelectToggle()
	{
		for (int i = 0; i < view.ShownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
			if (!(shownItemByIndex == null))
			{
				CustomCharaScrollViewInfo component = shownItemByIndex.GetComponent<CustomCharaScrollViewInfo>();
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
			selectInfo = _data;
			if (!num)
			{
				return;
			}
			for (int i = 0; i < view.ShownItemCount; i++)
			{
				LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
				if (shownItemByIndex == null)
				{
					continue;
				}
				CustomCharaScrollViewInfo component = shownItemByIndex.GetComponent<CustomCharaScrollViewInfo>();
				for (int j = 0; j < countPerRow; j++)
				{
					if (!IsNowSelectInfo(component.GetListInfo(j)))
					{
						component.SetToggleON(j, _isOn: false);
					}
				}
			}
			onSelect?.Invoke(selectInfo.info);
			if (selectInfo != null)
			{
				SelectName = selectInfo.info.name;
			}
		}
		else if (IsNowSelectInfo(_data?.info))
		{
			selectInfo = null;
			onDeSelect?.Invoke();
		}
	}

	private void OnPointerEnter(string name)
	{
		if ((bool)text)
		{
			text.text = name;
		}
	}

	private void OnPointerExit()
	{
		if ((bool)text)
		{
			text.text = SelectName;
		}
	}

	private bool IsNowSelectInfo(CustomCharaFileInfo _info)
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
		CustomCharaScrollViewInfo component = loopListViewItem.GetComponent<CustomCharaScrollViewInfo>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData data = scrollerDatas.SafeGet(index);
			component.SetData(i, data, delegate(bool _isOn)
			{
				OnValueChange(data, _isOn);
			}, OnPointerEnter, OnPointerExit);
			component.SetToggleON(i, IsNowSelectInfo(data?.info));
		}
		return loopListViewItem;
	}
}
