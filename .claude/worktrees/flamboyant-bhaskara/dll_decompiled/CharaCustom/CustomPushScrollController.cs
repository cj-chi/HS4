using System;
using System.Collections.Generic;
using System.Linq;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

[Serializable]
public class CustomPushScrollController : MonoBehaviour
{
	public class ScrollData
	{
		public int index;

		public CustomPushInfo info = new CustomPushInfo();
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

	public Action<CustomPushInfo> onPush;

	public void Start()
	{
	}

	public void CreateList(List<CustomPushInfo> _lst)
	{
		scrollerDatas = _lst.Select((CustomPushInfo value, int idx) => new ScrollData
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

	public void SetTopLine()
	{
		if (view.IsInit)
		{
			view.MovePanelToItemIndex(0, 0f);
		}
	}

	public void SetLine(int _line)
	{
		view.MovePanelToItemIndex(_line, 0f);
	}

	private void OnClick(ScrollData _data)
	{
		onPush?.Invoke(_data?.info);
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

	private LoopListViewItem2 OnUpdate(LoopListView2 _view, int _index)
	{
		if (_index < 0)
		{
			return null;
		}
		LoopListViewItem2 loopListViewItem = _view.NewListViewItem(original.name);
		CustomPushScrollViewInfo component = loopListViewItem.GetComponent<CustomPushScrollViewInfo>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			ScrollData data = scrollerDatas.SafeGet(index);
			component.SetData(i, data?.info, delegate
			{
				OnClick(data);
			}, OnPointerEnter, OnPointerExit);
		}
		return loopListViewItem;
	}
}
