using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Tables;

[RequireComponent(typeof(RectTransform))]
public class TableCell : HorizontalLayoutGroup
{
	[Tooltip("How many columns should this cell span?")]
	public int columnSpan = 1;

	[Tooltip("If this property is set, then this cell will ignore the TableLayout CellBackgroundColor/CellBackgroundImage values - allowing you to set specific values for this cell.")]
	public bool dontUseTableCellBackground;

	[Tooltip("If this property is set, then this cell will ignore the TableLayout Global Cell Padding values - allowing you to set specific values for this cell.")]
	public bool overrideGlobalPadding;

	[NonSerialized]
	internal float actualWidth;

	[NonSerialized]
	internal float actualHeight;

	[NonSerialized]
	internal float actualX;

	[NonSerialized]
	internal float actualY;

	protected Image _image;

	[SerializeField]
	private TableLayout m_tableLayout;

	[SerializeField]
	private TableRow m_tableRow;

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

	internal void Initialise(TableLayout tableLayout, TableRow row)
	{
		if (!(m_tableLayout == tableLayout) || !(m_tableRow == row))
		{
			m_tableLayout = tableLayout;
			m_tableRow = row;
			SetDirty();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.useGUILayout = false;
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
	}

	public override void CalculateLayoutInputVertical()
	{
		base.CalculateLayoutInputVertical();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
	}

	public override void SetLayoutHorizontal()
	{
		base.SetLayoutHorizontal();
	}

	public override void SetLayoutVertical()
	{
		base.SetLayoutVertical();
	}

	public void NotifyTableCellPropertiesChanged()
	{
		if (m_tableLayout != null && m_tableLayout.gameObject.activeInHierarchy)
		{
			m_tableLayout.CalculateLayoutInputHorizontal();
		}
	}

	public void SetCellPaddingFromTableLayout()
	{
		base.padding = m_tableLayout.CellPadding;
	}

	public TableRow GetRow()
	{
		return m_tableRow;
	}
}
