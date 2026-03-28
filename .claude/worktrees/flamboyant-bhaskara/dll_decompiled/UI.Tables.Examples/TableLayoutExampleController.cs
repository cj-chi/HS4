using System.Collections.Generic;
using UnityEngine;

namespace UI.Tables.Examples;

public class TableLayoutExampleController : MonoBehaviour
{
	public List<TableLayout> Examples = new List<TableLayout>();

	public void ShowExample(TableLayout example)
	{
		Examples.ForEach(delegate(TableLayout t)
		{
			if (t != example)
			{
				t.gameObject.SetActive(value: false);
			}
		});
		if (!example.gameObject.activeInHierarchy)
		{
			example.gameObject.SetActive(value: true);
		}
	}
}
