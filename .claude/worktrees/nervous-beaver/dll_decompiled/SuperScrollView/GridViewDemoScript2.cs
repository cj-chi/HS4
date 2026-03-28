using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SuperScrollView;

public class GridViewDemoScript2 : MonoBehaviour
{
	public LoopGridView mLoopGridView;

	private Button mScrollToButton;

	private Button mAddItemButton;

	private Button mSetCountButton;

	private InputField mScrollToInput;

	private InputField mAddItemInput;

	private InputField mSetCountInput;

	private Button mBackButton;

	private void Start()
	{
		mLoopGridView.InitGridView(DataSourceMgr.Get.TotalItemCount, OnGetItemByRowColumn);
		mSetCountButton = GameObject.Find("ButtonPanel/buttonGroup1/SetCountButton").GetComponent<Button>();
		mScrollToButton = GameObject.Find("ButtonPanel/buttonGroup2/ScrollToButton").GetComponent<Button>();
		mAddItemButton = GameObject.Find("ButtonPanel/buttonGroup3/AddItemButton").GetComponent<Button>();
		mSetCountInput = GameObject.Find("ButtonPanel/buttonGroup1/SetCountInputField").GetComponent<InputField>();
		mScrollToInput = GameObject.Find("ButtonPanel/buttonGroup2/ScrollToInputField").GetComponent<InputField>();
		mAddItemInput = GameObject.Find("ButtonPanel/buttonGroup3/AddItemInputField").GetComponent<InputField>();
		mScrollToButton.onClick.AddListener(OnJumpBtnClicked);
		mAddItemButton.onClick.AddListener(OnAddItemBtnClicked);
		mSetCountButton.onClick.AddListener(OnSetItemCountBtnClicked);
		mBackButton = GameObject.Find("ButtonPanel/BackButton").GetComponent<Button>();
		mBackButton.onClick.AddListener(OnBackBtnClicked);
	}

	private void OnBackBtnClicked()
	{
		SceneManager.LoadScene("Menu");
	}

	private LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int itemIndex, int row, int column)
	{
		ItemData itemDataByIndex = DataSourceMgr.Get.GetItemDataByIndex(itemIndex);
		if (itemDataByIndex == null)
		{
			return null;
		}
		LoopGridViewItem loopGridViewItem = gridView.NewListViewItem("ItemPrefab0");
		ListItem18 component = loopGridViewItem.GetComponent<ListItem18>();
		if (!loopGridViewItem.IsInitHandlerCalled)
		{
			loopGridViewItem.IsInitHandlerCalled = true;
			component.Init();
		}
		component.SetItemData(itemDataByIndex, itemIndex, row, column);
		return loopGridViewItem;
	}

	private void OnJumpBtnClicked()
	{
		int result = 0;
		if (int.TryParse(mScrollToInput.text, out result))
		{
			mLoopGridView.MovePanelToItemByIndex(result);
		}
	}

	private void OnAddItemBtnClicked()
	{
		int result = 0;
		if (int.TryParse(mAddItemInput.text, out result))
		{
			mLoopGridView.SetListItemCount(result + mLoopGridView.ItemTotalCount, resetPos: false);
		}
	}

	private void OnSetItemCountBtnClicked()
	{
		int result = 0;
		if (int.TryParse(mSetCountInput.text, out result))
		{
			if (result > DataSourceMgr.Get.TotalItemCount)
			{
				result = DataSourceMgr.Get.TotalItemCount;
			}
			if (result < 0)
			{
				result = 0;
			}
			mLoopGridView.SetListItemCount(result);
		}
	}
}
