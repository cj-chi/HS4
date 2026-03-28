using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Tables;

[RequireComponent(typeof(RectTransform))]
public class TableLayout : LayoutGroup, ILayoutSelfController, ILayoutController
{
	public Sprite RowBackgroundImage;

	public Color RowBackgroundColor = new Color(0f, 0f, 0f, 0f);

	public bool UseAlternateRowBackgroundColors;

	public Color RowBackgroundColorAlternate = new Color(0f, 0f, 0f, 0f);

	public Sprite CellBackgroundImage;

	public Color CellBackgroundColor = new Color(0f, 0f, 0f, 0f);

	public bool UseAlternateCellBackroundColors;

	public Color CellBackgroundColorAlternate = new Color(0f, 0f, 0f, 0f);

	[Tooltip("If this is set, then this TableLayout will automatically add columns if there are more cells than columns on any row (this includes ColumnSpan checks)")]
	public bool AutomaticallyAddColumns = true;

	[Tooltip("If this is set, then this TableLayout will automatically remove any columns with no cells in them in any row (at the END of the row)")]
	public bool AutomaticallyRemoveEmptyColumns = true;

	public List<float> ColumnWidths = new List<float>();

	[Tooltip("If this is set, then the cellpadding set here will override any padding settings set on individual cells")]
	public bool UseGlobalCellPadding = true;

	public RectOffset CellPadding = new RectOffset();

	public float CellSpacing;

	public bool AutoCalculateHeight;

	private DrivenRectTransformTracker _tracker;

	private LayoutElement _layoutElement;

	public List<TableRow> Rows => (from tr in GetComponentsInChildren<TableRow>()
		where tr.transform.parent == base.transform
		select tr).ToList();

	protected override void Awake()
	{
		base.Awake();
		_layoutElement = GetComponent<LayoutElement>();
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		UpdateLayout();
	}

	public override void CalculateLayoutInputVertical()
	{
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
	}

	public override void SetLayoutHorizontal()
	{
	}

	public override void SetLayoutVertical()
	{
	}

	public void UpdateLayout()
	{
		_tracker.Clear();
		Rect rect = base.rectTransform.rect;
		float height = rect.height;
		float width = rect.width;
		List<TableRow> list = Rows.ToList();
		if (!list.Any())
		{
			return;
		}
		List<float> list2 = new List<float>();
		float num = Mathf.Max(0f, height - list.Sum((TableRow r) => r.preferredHeight) - (float)(base.padding.top + base.padding.bottom) - CellSpacing * (float)(list.Count - 1));
		int num2 = list.Count((TableRow r) => r.preferredHeight == 0f);
		float num3 = 0f;
		if (num2 > 0)
		{
			num3 = num / (float)num2;
		}
		foreach (TableRow item in list)
		{
			list2.Add((item.preferredHeight > 0f) ? item.preferredHeight : num3);
		}
		int num4 = list.Max((TableRow r) => r.Cells.Sum((TableCell c) => c.columnSpan));
		if (AutomaticallyRemoveEmptyColumns && num4 < ColumnWidths.Count)
		{
			ColumnWidths.RemoveRange(num4, ColumnWidths.Count - num4);
		}
		float num5 = width - (float)(base.padding.left + base.padding.right) - CellSpacing * (float)(num4 - 1);
		foreach (float columnWidth in ColumnWidths)
		{
			num5 -= columnWidth;
			if (num5 < 0f)
			{
				num5 = 0f;
				break;
			}
		}
		for (int num6 = 0; num6 < num4; num6++)
		{
			if (AutomaticallyAddColumns && ColumnWidths.Count <= num6)
			{
				ColumnWidths.Add(0f);
			}
		}
		int num7 = ColumnWidths.Count((float c) => c == 0f);
		float num8 = 0f;
		if (num7 > 0)
		{
			num8 = num5 / (float)num7;
		}
		List<float> list3 = new List<float>();
		for (int num9 = 0; num9 < num4; num9++)
		{
			list3.Add((ColumnWidths.Count > num9 && ColumnWidths[num9] != 0f) ? ColumnWidths[num9] : num8);
		}
		float num10 = -base.padding.top;
		for (int num11 = 0; num11 < list.Count; num11++)
		{
			TableRow tableRow = list[num11];
			tableRow.Initialise(this);
			if (!tableRow.dontUseTableRowBackground)
			{
				tableRow.image.sprite = RowBackgroundImage;
				tableRow.image.color = ((!UseAlternateRowBackgroundColors || num11 % 2 == 0) ? RowBackgroundColor : RowBackgroundColorAlternate);
			}
			float num12 = (tableRow.actualHeight = list2[num11]);
			RectTransform rectTransform = tableRow.transform as RectTransform;
			_tracker.Add(this, rectTransform, DrivenTransformProperties.All);
			rectTransform.pivot = new Vector2(0f, 1f);
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(0f, 1f);
			rectTransform.localScale = new Vector3(1f, 1f, 1f);
			rectTransform.localRotation = default(Quaternion);
			rectTransform.sizeDelta = new Vector2(width - (float)(base.padding.left + base.padding.right), num12);
			rectTransform.anchoredPosition = new Vector2(base.padding.left, num10);
			num10 -= num12;
			num10 -= CellSpacing;
			float num13 = 0f;
			int num14 = 0;
			foreach (TableCell item2 in tableRow.Cells.ToList())
			{
				float num15 = 0f;
				int num16 = num14 + item2.columnSpan;
				for (int num17 = num14; num17 < num16; num17++)
				{
					num15 += list3[num17];
				}
				num15 += (float)(item2.columnSpan - 1) * CellSpacing;
				float b = width - num13;
				num15 = (item2.actualWidth = Mathf.Min(num15, b));
				item2.actualHeight = num12;
				item2.actualX = num13;
				if (UseGlobalCellPadding && !item2.overrideGlobalPadding)
				{
					item2.padding = new RectOffset(CellPadding.left, CellPadding.right, CellPadding.top, CellPadding.bottom);
				}
				if (!item2.dontUseTableCellBackground)
				{
					item2.image.sprite = CellBackgroundImage;
					item2.image.color = ((!UseAlternateCellBackroundColors || num14 % 2 == 0) ? CellBackgroundColor : CellBackgroundColorAlternate);
				}
				num13 += num15 + CellSpacing;
				num14 = num16;
			}
			tableRow.UpdateLayout();
		}
		if (AutoCalculateHeight)
		{
			base.rectTransform.pivot = new Vector2(base.rectTransform.pivot.x, 1f);
			base.rectTransform.sizeDelta = new Vector2(base.rectTransform.sizeDelta.x, 0f - num10);
			base.rectTransform.anchorMin = new Vector2(base.rectTransform.anchorMin.x, 1f);
			base.rectTransform.anchorMax = new Vector2(base.rectTransform.anchorMax.x, 1f);
			base.rectTransform.anchoredPosition = new Vector2(base.rectTransform.anchoredPosition.x, 0f);
			if (_layoutElement != null)
			{
				_layoutElement.preferredHeight = base.rectTransform.rect.height;
			}
		}
	}

	public TableRow AddRow()
	{
		return AddRow(ColumnWidths.Count);
	}

	public TableRow AddRow(int cells)
	{
		GameObject obj = TableLayoutUtilities.InstantiatePrefab("TableLayout/Row");
		obj.name = "Row";
		obj.transform.SetParent(base.transform);
		obj.transform.SetAsLastSibling();
		TableRow component = obj.GetComponent<TableRow>();
		for (int i = 0; i < cells; i++)
		{
			component.AddCell();
		}
		return component;
	}

	public TableRow AddRow(TableRow row)
	{
		row.transform.SetParent(base.transform);
		row.transform.SetAsLastSibling();
		return row;
	}

	public void ClearRows()
	{
		foreach (TableRow row in Rows)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(row.gameObject);
			}
			else
			{
				Object.DestroyImmediate(row.gameObject);
			}
		}
	}
}
