using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

[Serializable]
public class CustomSelectScrollController : MonoBehaviour
{
	public class ScrollData
	{
		public int index;

		public CustomSelectInfo info = new CustomSelectInfo();

		public Toggle toggle;
	}

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 5;

	[SerializeField]
	private Text text;

	private string SelectName = "";

	private ScrollData[] scrollerDatas;

	public Action<CustomSelectInfo> onSelect;

	public Action onDeSelect;

	public ScrollData selectInfo { get; private set; }

	public void Start()
	{
	}

	public void CreateList(List<CustomSelectInfo> _lst)
	{
		scrollerDatas = _lst.Select((CustomSelectInfo value, int idx) => new ScrollData
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

	public void SetToggle(int _index)
	{
		selectInfo = scrollerDatas[_index];
		view.RefreshAllShownItem();
	}

	public void SetToggleID(int _id)
	{
		ScrollData scrollData = scrollerDatas.FirstOrDefault((ScrollData x) => x.info.id == _id);
		if (scrollData != null)
		{
			selectInfo = scrollData;
			view.RefreshAllShownItem();
		}
	}

	private void SetNowSelectToggle()
	{
		for (int i = 0; i < view.ShownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
			if (!(shownItemByIndex == null))
			{
				CustomSelectScrollViewInfo component = shownItemByIndex.GetComponent<CustomSelectScrollViewInfo>();
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
				CustomSelectScrollViewInfo component = shownItemByIndex.GetComponent<CustomSelectScrollViewInfo>();
				for (int j = 0; j < countPerRow; j++)
				{
					if (!IsNowSelectInfo(component.GetListInfo(j)))
					{
						component.SetToggleON(j, _isOn: false);
					}
					else
					{
						component.SetNewFlagOff(j);
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
			_data.toggle.SetIsOnWithoutCallback(isOn: true);
		}
	}

	private void OnPointerEnter(string name, int fontSize)
	{
		if ((bool)text)
		{
			if (null != Singleton<CustomBase>.Instance.cultureControl && Singleton<CustomBase>.Instance.cultureControl.dictFontInfo.TryGetValue(0, out var value))
			{
				text.font = value.font;
			}
			text.fontSize = fontSize;
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

	private bool IsNowSelectInfo(CustomSelectInfo _info)
	{
		if (_info != null && selectInfo != null)
		{
			return selectInfo.info == _info;
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
		CustomSelectScrollViewInfo component = loopListViewItem.GetComponent<CustomSelectScrollViewInfo>();
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
