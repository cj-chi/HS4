using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Game;
using Manager;
using SuperScrollView;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UploaderSystem;

[Serializable]
public class NetSelectHNScrollController : MonoBehaviour
{
	public class ScrollData
	{
		public int index;

		public NetworkInfo.SelectHNInfo info = new NetworkInfo.SelectHNInfo();
	}

	public DownUIControl uiCtrl;

	[SerializeField]
	private Button btnClose;

	[SerializeField]
	private LoopListView2 view;

	[SerializeField]
	private GameObject original;

	[SerializeField]
	private int countPerRow = 3;

	[SerializeField]
	private Text text;

	private ScrollData[] scrollerDatas;

	private bool noProc;

	private bool skip;

	private NetworkInfo netInfo => Singleton<NetworkInfo>.Instance;

	public ScrollData selectInfo { get; private set; }

	public void Start()
	{
		if ((bool)btnClose)
		{
			btnClose.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				ShowSelectHNWindow(show: false);
			});
		}
	}

	public void Init()
	{
		List<ScrollData> list = new List<ScrollData>();
		foreach (KeyValuePair<int, NetworkInfo.UserInfo> n in netInfo.dictUserInfo)
		{
			int num = netInfo.lstCharaInfo.Where((NetworkInfo.CharaInfo x) => x.user_idx == n.Key).ToArray().Length;
			if (num != 0)
			{
				ScrollData scrollData = new ScrollData();
				scrollData.info.userIdx = n.Key;
				scrollData.info.drawname = TextCorrectLimit.CorrectString(text, $"({num}) {n.Value.handleName}", "…");
				scrollData.info.handlename = n.Value.handleName;
				list.Add(scrollData);
			}
		}
		using (new GameSystem.CultureScope())
		{
			list = list.OrderBy((ScrollData scrollData2) => scrollData2.info.handlename).ToList();
		}
		scrollerDatas = list.ToArray();
		int num2 = ((!((IReadOnlyCollection<ScrollData>)(object)scrollerDatas).IsNullOrEmpty()) ? (scrollerDatas.Length / countPerRow) : 0);
		if (!((IReadOnlyCollection<ScrollData>)(object)scrollerDatas).IsNullOrEmpty() && scrollerDatas.Length % countPerRow > 0)
		{
			num2++;
		}
		if (!view.IsInit)
		{
			view.InitListView(num2, OnUpdate);
		}
		else if (!view.SetListItemCount(num2))
		{
			view.RefreshAllShownItem();
		}
	}

	public void ShowSelectHNWindow(bool show)
	{
		if (show)
		{
			int hnidx = uiCtrl.searchSortHNIdx;
			noProc = true;
			selectInfo = ((-1 == hnidx) ? null : scrollerDatas.FirstOrDefault((ScrollData d) => d.info.userIdx == hnidx));
			SetNowSelectToggle();
			noProc = false;
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void SetNowSelectToggle()
	{
		for (int i = 0; i < view.ShownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
			if (!(shownItemByIndex == null))
			{
				NetSelectHNScrollViewInfo component = shownItemByIndex.GetComponent<NetSelectHNScrollViewInfo>();
				for (int j = 0; j < countPerRow; j++)
				{
					component.SetToggleON(IsNowSelectInfo(component.GetListInfo()));
				}
			}
		}
	}

	private void OnValueChanged(bool _isOn, int _idx)
	{
		if (skip)
		{
			return;
		}
		skip = true;
		if (!noProc)
		{
			uiCtrl.searchSortHNIdx = (_isOn ? scrollerDatas[_idx].info.userIdx : (-1));
			uiCtrl.changeSearchSetting = true;
		}
		if (_isOn)
		{
			bool num = !IsNowSelectInfo(scrollerDatas[_idx].info);
			selectInfo = scrollerDatas[_idx];
			if (num)
			{
				for (int i = 0; i < view.ShownItemCount; i++)
				{
					LoopListViewItem2 shownItemByIndex = view.GetShownItemByIndex(i);
					if (shownItemByIndex == null)
					{
						continue;
					}
					NetSelectHNScrollViewInfo component = shownItemByIndex.GetComponent<NetSelectHNScrollViewInfo>();
					for (int j = 0; j < countPerRow; j++)
					{
						if (!IsNowSelectInfo(component.GetListInfo()))
						{
							component.SetToggleON(_isOn: false);
						}
					}
				}
			}
		}
		else if (IsNowSelectInfo(scrollerDatas[_idx].info))
		{
			selectInfo = null;
		}
		skip = false;
	}

	private bool IsNowSelectInfo(NetworkInfo.SelectHNInfo _info)
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
		NetSelectHNScrollViewInfo component = loopListViewItem.GetComponent<NetSelectHNScrollViewInfo>();
		for (int i = 0; i < countPerRow; i++)
		{
			int index = _index * countPerRow + i;
			NetworkInfo.SelectHNInfo selectHNInfo = scrollerDatas.SafeGet(index)?.info;
			component.SetData(selectHNInfo, delegate(bool _isOn)
			{
				OnValueChanged(_isOn, index);
			});
			noProc = true;
			component.SetToggleON(IsNowSelectInfo(selectHNInfo));
			noProc = false;
		}
		return loopListViewItem;
	}
}
