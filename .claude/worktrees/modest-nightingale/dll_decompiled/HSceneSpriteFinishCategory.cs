using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HSceneSpriteFinishCategory : HSceneSpriteCategory
{
	private int activeID = -1;

	private List<int> lstActive = new List<int>();

	public int ActiveID => activeID;

	public bool onEnter { get; set; }

	private void Update()
	{
		if (lstActive.Count == 0)
		{
			if (ActiveID == -1)
			{
				return;
			}
			activeID = -1;
			{
				foreach (Button item in lstButton)
				{
					item.gameObject.SetActive(value: false);
				}
				return;
			}
		}
		if (ActiveID == -1)
		{
			activeID = 0;
			while (!lstActive.Contains(ActiveID))
			{
				activeID = GlobalMethod.ValLoop(ActiveID + 1, lstButton.Count);
				if (ActiveID == 0)
				{
					break;
				}
			}
		}
		if (onEnter)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			int num = activeID;
			if (axis > 0f)
			{
				do
				{
					activeID = GlobalMethod.ValLoop(ActiveID - 1, lstButton.Count);
				}
				while (!lstActive.Contains(activeID) && num != activeID);
			}
			else if (axis < 0f)
			{
				do
				{
					activeID = GlobalMethod.ValLoop(ActiveID + 1, lstButton.Count);
				}
				while (!lstActive.Contains(activeID) && num != activeID);
			}
		}
		for (int i = 0; i < lstButton.Count; i++)
		{
			if (ActiveID != i && lstButton[i].gameObject.activeSelf)
			{
				lstButton[i].gameObject.SetActive(value: false);
			}
			else if (ActiveID == i && !lstButton[i].gameObject.activeSelf)
			{
				lstButton[i].gameObject.SetActive(value: true);
			}
		}
	}

	public override void SetActive(bool _active, int _array = -1)
	{
		if (_array == -1 && !_active)
		{
			for (int i = 0; i < lstButton.Count; i++)
			{
				lstButton[i].gameObject.SetActive(value: false);
			}
			lstActive.Clear();
			activeID = -1;
			return;
		}
		if (_active)
		{
			if (!lstActive.Contains(_array))
			{
				lstActive.Add(_array);
			}
		}
		else
		{
			if (lstButton[_array].gameObject.activeSelf)
			{
				lstButton[_array].gameObject.SetActive(value: false);
			}
			if (ActiveID > 0 && ActiveID == _array)
			{
				activeID = -1;
			}
			if (lstActive.Contains(_array))
			{
				lstActive.IndexOf(_array);
				lstActive.Remove(_array);
			}
		}
		if (!lstActive.Contains(activeID))
		{
			activeID = -1;
		}
	}

	public int GetlstActive()
	{
		return activeID;
	}
}
