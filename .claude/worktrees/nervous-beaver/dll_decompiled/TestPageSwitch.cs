using UnityEngine;

public class TestPageSwitch : MonoBehaviour
{
	private int currentPage;

	private void Start()
	{
		ShowPage();
	}

	public void ShiftPage(int offset)
	{
		currentPage += offset;
		if (currentPage >= base.transform.childCount)
		{
			currentPage = 0;
		}
		else if (currentPage < 0)
		{
			currentPage = base.transform.childCount - 1;
		}
		ShowPage();
	}

	private void ShowPage()
	{
		int num = 0;
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(num == currentPage);
			num++;
		}
	}
}
