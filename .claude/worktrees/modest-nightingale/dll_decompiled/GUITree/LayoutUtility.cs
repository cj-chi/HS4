using UnityEngine;
using UnityEngine.UI;

namespace GUITree;

public static class LayoutUtility
{
	public static float GetMinSize(ILayoutElement _element, int _axis)
	{
		if (_axis != 0)
		{
			return GetMinHeight(_element);
		}
		return GetMinWidth(_element);
	}

	public static float GetPreferredSize(ILayoutElement _element, int _axis)
	{
		if (_axis != 0)
		{
			return GetPreferredHeight(_element);
		}
		return GetPreferredWidth(_element);
	}

	public static float GetFlexibleSize(ILayoutElement _element, int _axis)
	{
		if (_axis != 0)
		{
			return GetFlexibleHeight(_element);
		}
		return GetFlexibleWidth(_element);
	}

	public static float GetMinWidth(ILayoutElement _element)
	{
		return _element?.minWidth ?? 0f;
	}

	public static float GetPreferredWidth(ILayoutElement _element)
	{
		if (_element != null)
		{
			return Mathf.Max(Mathf.Max(_element.minWidth, _element.preferredWidth), 0f);
		}
		return 0f;
	}

	public static float GetFlexibleWidth(ILayoutElement _element)
	{
		return _element?.flexibleWidth ?? 0f;
	}

	public static float GetMinHeight(ILayoutElement _element)
	{
		return _element?.minHeight ?? 0f;
	}

	public static float GetPreferredHeight(ILayoutElement _element)
	{
		if (_element != null)
		{
			return Mathf.Max(Mathf.Max(_element.minHeight, _element.preferredHeight), 0f);
		}
		return 0f;
	}

	public static float GetFlexibleHeight(ILayoutElement _element)
	{
		return _element?.flexibleHeight ?? 0f;
	}
}
