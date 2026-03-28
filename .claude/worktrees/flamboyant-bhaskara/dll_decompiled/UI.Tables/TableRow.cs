using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Tables;

[RequireComponent(typeof(RectTransform))]
public class TableRow : MonoBehaviour
{
	public float preferredHeight;

	[NonSerialized]
	internal float actualHeight;

	public bool dontUseTableRowBackground;

	protected Image _image;

	private DrivenRectTransformTracker _tracker;

	[SerializeField]
	private TableLayout m_tableLayout;

	public List<TableCell> Cells => (from tc in GetComponentsInChildren<TableCell>()
		where tc.transform.parent == base.transform
		select tc).ToList();

	public int CellCount => Cells.Count;

	public Image image
	{
		get
		{
			if (_image == null)
			{
				_image = GetComponent<Image>();
			}
			return _image;
		}
	}

	internal void Initialise(TableLayout tableLayout)
	{
		m_tableLayout = tableLayout;
	}

	public void UpdateLayout()
	{
		_tracker.Clear();
		foreach (TableCell cell in Cells)
		{
			RectTransform rectTransform = (RectTransform)cell.transform;
			_tracker.Add(this, rectTransform, DrivenTransformProperties.All);
			rectTransform.pivot = new Vector2(0f, 1f);
			rectTransform.sizeDelta = new Vector2(cell.actualWidth, cell.actualHeight);
			rectTransform.anchoredPosition3D = new Vector3(cell.actualX, cell.actualY, 0f);
			rectTransform.localScale = new Vector3(1f, 1f, 1f);
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(0f, 1f);
			cell.Initialise(m_tableLayout, this);
		}
	}

	public TableCell AddCell(RectTransform cellContent = null)
	{
		GameObject gameObject = TableLayoutUtilities.InstantiatePrefab("TableLayout/Cell");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.SetAsLastSibling();
		gameObject.name = "Cell";
		if (cellContent != null)
		{
			cellContent.SetParent(gameObject.transform);
			cellContent.transform.localScale = new Vector3(1f, 1f, 1f);
			cellContent.localPosition = Vector3.zero;
		}
		return gameObject.GetComponent<TableCell>();
	}

	public TableCell AddCell(TableCell cell)
	{
		cell.transform.SetParent(base.transform);
		cell.transform.SetAsLastSibling();
		cell.transform.localScale = new Vector3(1f, 1f, 1f);
		cell.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
		return cell;
	}

	public void NotifyTableRowPropertiesChanged()
	{
		if (m_tableLayout != null && m_tableLayout.gameObject.activeInHierarchy)
		{
			m_tableLayout.CalculateLayoutInputHorizontal();
		}
	}

	public TableLayout GetTable()
	{
		return m_tableLayout;
	}

	public void ClearCells()
	{
		foreach (TableCell cell in Cells)
		{
			UnityEngine.Object.DestroyImmediate(cell.gameObject);
		}
	}
}
