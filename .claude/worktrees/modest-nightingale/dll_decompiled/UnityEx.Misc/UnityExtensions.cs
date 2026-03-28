using System.Linq;
using UnityEngine;

namespace UnityEx.Misc;

public static class UnityExtensions
{
	public static void SetActive(this Component component, bool isActive)
	{
		component.gameObject.SetActive(isActive);
	}

	public static void SetActiveSafe(this GameObject gameObject, bool isActive)
	{
		if (isActive)
		{
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(value: true);
			}
		}
		else if (gameObject.activeSelf)
		{
			gameObject.SetActive(value: false);
		}
	}

	public static void SetActiveSafe(this Component component, bool isActive)
	{
		if (component == null)
		{
			return;
		}
		if (isActive)
		{
			if (!component.gameObject.activeSelf)
			{
				component.gameObject.SetActive(value: true);
			}
		}
		else if (component.gameObject.activeSelf)
		{
			component.gameObject.SetActive(value: false);
		}
	}

	public static Transform Initiate(this Transform t, Transform parent)
	{
		t.SetParent(parent, worldPositionStays: false);
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = Vector3.one;
		return t;
	}

	public static Transform[] GetChildren(this Transform t, bool active)
	{
		return (from c in t.GetComponentsInChildren<Transform>(active)
			where c != t.transform
			select c).ToArray();
	}

	public static Transform[] GetOnlyChildren(this Transform t)
	{
		return (from c in Enumerable.Range(0, t.childCount)
			select t.GetChild(c)).ToArray();
	}

	public static Transform GetRoot(this Transform t)
	{
		if (t.parent == null)
		{
			return t;
		}
		return t.parent.GetRoot();
	}

	public static Color ToRGBA(uint rgba)
	{
		float num = 0.003921569f;
		Color black = Color.black;
		black.r = num * (float)((rgba >> 24) & 0xFF);
		black.g = num * (float)((rgba >> 16) & 0xFF);
		black.b = num * (float)((rgba >> 8) & 0xFF);
		black.a = num * (float)(rgba & 0xFF);
		return black;
	}

	public static uint ToHexRGBA(this Color source)
	{
		Color32 color = source;
		return (uint)(0 + (color.r << 24) + (color.g << 16) + (color.b << 8) + color.a);
	}

	public static void AddBlend(this Color source, Color destination)
	{
		uint num = source.ToHexRGBA();
		uint num2 = destination.ToHexRGBA();
		source = ToRGBA(((num & 0xFEFEFEFEu) >> 1) + ((num2 & 0xFEFEFEFEu) >> 1) + (num & num2 & 0x1010101));
	}

	public static bool Contains(this LayerMask source, int layer)
	{
		if ((int)source == ((int)source | (1 << layer)))
		{
			return layer != 0;
		}
		return false;
	}
}
