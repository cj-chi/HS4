using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Tables.Examples;

public class DynamicRowsExampleController : MonoBehaviour
{
	public TableLayout tableLayout;

	public TableRow rowTemplate;

	public RectTransform scrollviewContent;

	public int numberOfRowsToAdd = 50;

	private int numberOfRowsAdded;

	public Font font;

	private void OnEnable()
	{
		StartCoroutine(AddRowsUsingTemplate());
	}

	private IEnumerator AddRowsUsingTemplate()
	{
		while (numberOfRowsAdded <= numberOfRowsToAdd)
		{
			TableRow tableRow = Object.Instantiate(rowTemplate);
			tableRow.gameObject.SetActive(value: true);
			tableLayout.AddRow(tableRow);
			scrollviewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (tableLayout.transform as RectTransform).rect.height);
			tableRow.Cells[0].GetComponentInChildren<Text>().text = "Row " + numberOfRowsAdded;
			tableRow.Cells[1].GetComponentInChildren<Toggle>().isOn = numberOfRowsAdded % 2 == 0;
			numberOfRowsAdded++;
			yield return new WaitForSeconds(0.25f);
		}
	}

	private IEnumerator AddRowsWithoutTemplate()
	{
		while (numberOfRowsAdded <= numberOfRowsToAdd)
		{
			TableRow tableRow = tableLayout.AddRow(0);
			tableRow.preferredHeight = 72f;
			scrollviewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (tableLayout.transform as RectTransform).rect.height);
			GameObject gameObject = new GameObject("Text", typeof(RectTransform));
			tableRow.AddCell(gameObject.transform as RectTransform);
			Text text = gameObject.AddComponent<Text>();
			text.font = font;
			text.text = "Row " + numberOfRowsAdded;
			text.resizeTextForBestFit = true;
			text.alignment = TextAnchor.MiddleCenter;
			tableRow.AddCell();
			tableRow.AddCell();
			numberOfRowsAdded++;
			yield return new WaitForSeconds(0.25f);
		}
	}
}
