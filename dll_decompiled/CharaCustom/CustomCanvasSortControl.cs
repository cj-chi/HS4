using System.Linq;
using UnityEngine;

namespace CharaCustom;

public class CustomCanvasSortControl : MonoBehaviour
{
	[SerializeField]
	private Canvas[] sortCanvas;

	private void Start()
	{
		if (sortCanvas != null && sortCanvas.Length != 0)
		{
			for (int i = 0; i < sortCanvas.Length; i++)
			{
				sortCanvas[i].sortingOrder = i + 1;
			}
		}
	}

	public void SortCanvas(Canvas cvs)
	{
		if (1 >= sortCanvas.Length || cvs == sortCanvas.Last())
		{
			return;
		}
		Canvas[] array = new Canvas[sortCanvas.Length];
		int num = 0;
		for (int i = 0; i < sortCanvas.Length; i++)
		{
			if (!(cvs == sortCanvas[i]))
			{
				array[num++] = sortCanvas[i];
			}
		}
		array[array.Length - 1] = cvs;
		sortCanvas = array;
		if (sortCanvas != null && sortCanvas.Length != 0)
		{
			for (int j = 0; j < sortCanvas.Length; j++)
			{
				sortCanvas[j].sortingOrder = j + 1;
			}
		}
	}
}
