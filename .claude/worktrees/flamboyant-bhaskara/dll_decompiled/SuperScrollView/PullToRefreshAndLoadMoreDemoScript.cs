using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SuperScrollView;

public class PullToRefreshAndLoadMoreDemoScript : MonoBehaviour
{
	public LoopListView2 mLoopListView;

	private LoadingTipStatus mLoadingTipStatus1;

	private LoadingTipStatus mLoadingTipStatus2;

	private float mDataLoadedTipShowLeftTime;

	private float mLoadingTipItemHeight1 = 100f;

	private float mLoadingTipItemHeight2 = 100f;

	private int mLoadMoreCount = 20;

	private Button mScrollToButton;

	private Button mAddItemButton;

	private Button mSetCountButton;

	private InputField mScrollToInput;

	private InputField mAddItemInput;

	private InputField mSetCountInput;

	private Button mBackButton;

	private void Start()
	{
		mLoopListView.InitListView(DataSourceMgr.Get.TotalItemCount + 2, OnGetItemByIndex);
		mLoopListView.mOnDragingAction = OnDraging;
		mLoopListView.mOnEndDragAction = OnEndDrag;
		mScrollToButton = GameObject.Find("ButtonPanel/buttonGroup2/ScrollToButton").GetComponent<Button>();
		mScrollToInput = GameObject.Find("ButtonPanel/buttonGroup2/ScrollToInputField").GetComponent<InputField>();
		mScrollToButton.onClick.AddListener(OnJumpBtnClicked);
		mBackButton = GameObject.Find("ButtonPanel/BackButton").GetComponent<Button>();
		mBackButton.onClick.AddListener(OnBackBtnClicked);
	}

	private void OnBackBtnClicked()
	{
		SceneManager.LoadScene("Menu");
	}

	private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
	{
		if (index < 0)
		{
			return null;
		}
		LoopListViewItem2 loopListViewItem = null;
		if (index == 0)
		{
			loopListViewItem = listView.NewListViewItem("ItemPrefab0");
			UpdateLoadingTip1(loopListViewItem);
			return loopListViewItem;
		}
		if (index == DataSourceMgr.Get.TotalItemCount + 1)
		{
			loopListViewItem = listView.NewListViewItem("ItemPrefab1");
			UpdateLoadingTip2(loopListViewItem);
			return loopListViewItem;
		}
		int num = index - 1;
		ItemData itemDataByIndex = DataSourceMgr.Get.GetItemDataByIndex(num);
		if (itemDataByIndex == null)
		{
			return null;
		}
		loopListViewItem = listView.NewListViewItem("ItemPrefab2");
		ListItem2 component = loopListViewItem.GetComponent<ListItem2>();
		if (!loopListViewItem.IsInitHandlerCalled)
		{
			loopListViewItem.IsInitHandlerCalled = true;
			component.Init();
		}
		if (index == DataSourceMgr.Get.TotalItemCount)
		{
			loopListViewItem.Padding = 0f;
		}
		component.SetItemData(itemDataByIndex, num);
		return loopListViewItem;
	}

	private void UpdateLoadingTip1(LoopListViewItem2 item)
	{
		if (item == null)
		{
			return;
		}
		ListItem0 component = item.GetComponent<ListItem0>();
		if (!(component == null))
		{
			if (mLoadingTipStatus1 == LoadingTipStatus.None)
			{
				component.mRoot.SetActive(value: false);
				item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
			}
			else if (mLoadingTipStatus1 == LoadingTipStatus.WaitRelease)
			{
				component.mRoot.SetActive(value: true);
				component.mText.text = "Release to Refresh";
				component.mArrow.SetActive(value: true);
				component.mWaitingIcon.SetActive(value: false);
				item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight1);
			}
			else if (mLoadingTipStatus1 == LoadingTipStatus.WaitLoad)
			{
				component.mRoot.SetActive(value: true);
				component.mArrow.SetActive(value: false);
				component.mWaitingIcon.SetActive(value: true);
				component.mText.text = "Loading ...";
				item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight1);
			}
			else if (mLoadingTipStatus1 == LoadingTipStatus.Loaded)
			{
				component.mRoot.SetActive(value: true);
				component.mArrow.SetActive(value: false);
				component.mWaitingIcon.SetActive(value: false);
				component.mText.text = "Refreshed Success";
				item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight1);
			}
		}
	}

	private void OnDraging()
	{
		OnDraging1();
		OnDraging2();
	}

	private void OnEndDrag()
	{
		OnEndDrag1();
		OnEndDrag2();
	}

	private void OnDraging1()
	{
		if (mLoopListView.ShownItemCount == 0 || (mLoadingTipStatus1 != LoadingTipStatus.None && mLoadingTipStatus1 != LoadingTipStatus.WaitRelease))
		{
			return;
		}
		LoopListViewItem2 shownItemByItemIndex = mLoopListView.GetShownItemByItemIndex(0);
		if (shownItemByItemIndex == null)
		{
			return;
		}
		if (mLoopListView.ScrollRect.content.anchoredPosition3D.y < 0f - mLoadingTipItemHeight1)
		{
			if (mLoadingTipStatus1 == LoadingTipStatus.None)
			{
				mLoadingTipStatus1 = LoadingTipStatus.WaitRelease;
				UpdateLoadingTip1(shownItemByItemIndex);
				shownItemByItemIndex.CachedRectTransform.anchoredPosition3D = new Vector3(0f, mLoadingTipItemHeight1, 0f);
			}
		}
		else if (mLoadingTipStatus1 == LoadingTipStatus.WaitRelease)
		{
			mLoadingTipStatus1 = LoadingTipStatus.None;
			UpdateLoadingTip1(shownItemByItemIndex);
			shownItemByItemIndex.CachedRectTransform.anchoredPosition3D = new Vector3(0f, 0f, 0f);
		}
	}

	private void OnEndDrag1()
	{
		if (mLoopListView.ShownItemCount == 0 || (mLoadingTipStatus1 != LoadingTipStatus.None && mLoadingTipStatus1 != LoadingTipStatus.WaitRelease))
		{
			return;
		}
		LoopListViewItem2 shownItemByItemIndex = mLoopListView.GetShownItemByItemIndex(0);
		if (!(shownItemByItemIndex == null))
		{
			mLoopListView.OnItemSizeChanged(shownItemByItemIndex.ItemIndex);
			if (mLoadingTipStatus1 == LoadingTipStatus.WaitRelease)
			{
				mLoadingTipStatus1 = LoadingTipStatus.WaitLoad;
				UpdateLoadingTip1(shownItemByItemIndex);
				DataSourceMgr.Get.RequestRefreshDataList(OnDataSourceRefreshFinished);
			}
		}
	}

	private void OnDataSourceRefreshFinished()
	{
		if (mLoopListView.ShownItemCount != 0 && mLoadingTipStatus1 == LoadingTipStatus.WaitLoad)
		{
			mLoadingTipStatus1 = LoadingTipStatus.Loaded;
			mDataLoadedTipShowLeftTime = 0.7f;
			LoopListViewItem2 shownItemByItemIndex = mLoopListView.GetShownItemByItemIndex(0);
			if (!(shownItemByItemIndex == null))
			{
				UpdateLoadingTip1(shownItemByItemIndex);
				mLoopListView.RefreshAllShownItem();
			}
		}
	}

	private void Update()
	{
		if (mLoopListView.ShownItemCount == 0 || mLoadingTipStatus1 != LoadingTipStatus.Loaded)
		{
			return;
		}
		mDataLoadedTipShowLeftTime -= Time.deltaTime;
		if (mDataLoadedTipShowLeftTime <= 0f)
		{
			mLoadingTipStatus1 = LoadingTipStatus.None;
			LoopListViewItem2 shownItemByItemIndex = mLoopListView.GetShownItemByItemIndex(0);
			if (!(shownItemByItemIndex == null))
			{
				UpdateLoadingTip1(shownItemByItemIndex);
				shownItemByItemIndex.CachedRectTransform.anchoredPosition3D = new Vector3(0f, 0f - mLoadingTipItemHeight1, 0f);
				mLoopListView.OnItemSizeChanged(0);
			}
		}
	}

	private void UpdateLoadingTip2(LoopListViewItem2 item)
	{
		if (item == null)
		{
			return;
		}
		ListItem0 component = item.GetComponent<ListItem0>();
		if (!(component == null))
		{
			if (mLoadingTipStatus2 == LoadingTipStatus.None)
			{
				component.mRoot.SetActive(value: false);
				item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
			}
			else if (mLoadingTipStatus2 == LoadingTipStatus.WaitRelease)
			{
				component.mRoot.SetActive(value: true);
				component.mText.text = "Release to Load More";
				component.mArrow.SetActive(value: true);
				component.mWaitingIcon.SetActive(value: false);
				item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight2);
			}
			else if (mLoadingTipStatus2 == LoadingTipStatus.WaitLoad)
			{
				component.mRoot.SetActive(value: true);
				component.mArrow.SetActive(value: false);
				component.mWaitingIcon.SetActive(value: true);
				component.mText.text = "Loading ...";
				item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight2);
			}
		}
	}

	private void OnDraging2()
	{
		if (mLoopListView.ShownItemCount == 0 || (mLoadingTipStatus2 != LoadingTipStatus.None && mLoadingTipStatus2 != LoadingTipStatus.WaitRelease))
		{
			return;
		}
		LoopListViewItem2 shownItemByItemIndex = mLoopListView.GetShownItemByItemIndex(DataSourceMgr.Get.TotalItemCount + 1);
		if (shownItemByItemIndex == null)
		{
			return;
		}
		LoopListViewItem2 shownItemByItemIndex2 = mLoopListView.GetShownItemByItemIndex(DataSourceMgr.Get.TotalItemCount);
		if (shownItemByItemIndex2 == null)
		{
			return;
		}
		if (mLoopListView.GetItemCornerPosInViewPort(shownItemByItemIndex2).y + mLoopListView.ViewPortSize >= mLoadingTipItemHeight2)
		{
			if (mLoadingTipStatus2 == LoadingTipStatus.None)
			{
				mLoadingTipStatus2 = LoadingTipStatus.WaitRelease;
				UpdateLoadingTip2(shownItemByItemIndex);
			}
		}
		else if (mLoadingTipStatus2 == LoadingTipStatus.WaitRelease)
		{
			mLoadingTipStatus2 = LoadingTipStatus.None;
			UpdateLoadingTip2(shownItemByItemIndex);
		}
	}

	private void OnEndDrag2()
	{
		if (mLoopListView.ShownItemCount == 0 || (mLoadingTipStatus2 != LoadingTipStatus.None && mLoadingTipStatus2 != LoadingTipStatus.WaitRelease))
		{
			return;
		}
		LoopListViewItem2 shownItemByItemIndex = mLoopListView.GetShownItemByItemIndex(DataSourceMgr.Get.TotalItemCount + 1);
		if (!(shownItemByItemIndex == null))
		{
			mLoopListView.OnItemSizeChanged(shownItemByItemIndex.ItemIndex);
			if (mLoadingTipStatus2 == LoadingTipStatus.WaitRelease)
			{
				mLoadingTipStatus2 = LoadingTipStatus.WaitLoad;
				UpdateLoadingTip2(shownItemByItemIndex);
				DataSourceMgr.Get.RequestLoadMoreDataList(mLoadMoreCount, OnDataSourceLoadMoreFinished);
			}
		}
	}

	private void OnDataSourceLoadMoreFinished()
	{
		if (mLoopListView.ShownItemCount != 0 && mLoadingTipStatus2 == LoadingTipStatus.WaitLoad)
		{
			mLoadingTipStatus2 = LoadingTipStatus.None;
			mLoopListView.SetListItemCount(DataSourceMgr.Get.TotalItemCount + 2, resetPos: false);
			mLoopListView.RefreshAllShownItem();
		}
	}

	private void OnJumpBtnClicked()
	{
		int result = 0;
		if (int.TryParse(mScrollToInput.text, out result) && result >= 0)
		{
			result++;
			mLoopListView.MovePanelToItemIndex(result, 0f);
		}
	}
}
