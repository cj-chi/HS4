using System.Collections.Generic;
using UnityEngine;

namespace PicoGames.Utilities;

public class Triangulate
{
	private static Vector3[] shape;

	public static int[] Edge(Vector3[] _shape)
	{
		shape = _shape;
		List<int> list = new List<int>();
		int num = shape.Length;
		if (num < 3)
		{
			return list.ToArray();
		}
		int[] array = new int[num];
		if (Area() > 0f)
		{
			for (int i = 0; i < num; i++)
			{
				array[i] = i;
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				array[j] = num - 1 - j;
			}
		}
		int num2 = num;
		int num3 = 2 * num2;
		int num4 = 0;
		int num5 = num2 - 1;
		while (num2 > 2)
		{
			if (num3-- <= 0)
			{
				return list.ToArray();
			}
			int num6 = num5;
			if (num2 <= num6)
			{
				num6 = 0;
			}
			num5 = num6 + 1;
			if (num2 <= num5)
			{
				num5 = 0;
			}
			int num7 = num5 + 1;
			if (num2 <= num7)
			{
				num7 = 0;
			}
			if (Snip(num6, num5, num7, num2, array))
			{
				int item = array[num6];
				int item2 = array[num5];
				int item3 = array[num7];
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				num4++;
				int num8 = num5;
				for (int k = num5 + 1; k < num2; k++)
				{
					array[num8] = array[k];
					num8++;
				}
				num2--;
				num3 = 2 * num2;
			}
		}
		list.Reverse();
		return list.ToArray();
	}

	private static float Area()
	{
		int num = shape.Length;
		float num2 = 0f;
		int num3 = num - 1;
		int num4 = 0;
		while (num4 < num)
		{
			Vector3 vector = shape[num3];
			Vector3 vector2 = shape[num4];
			num2 += vector.x * vector2.y - vector2.x * vector.y;
			num3 = num4++;
		}
		return num2 * 0.5f;
	}

	private static bool OverlapsPoint(Vector3 _t1, Vector3 _t2, Vector3 _t3, Vector3 _p1)
	{
		float num = _t3.x - _t2.x;
		float num2 = _t3.y - _t2.y;
		float num3 = _t1.x - _t3.x;
		float num4 = _t1.y - _t3.y;
		float num5 = _t2.x - _t1.x;
		float num6 = _t2.y - _t1.y;
		float num7 = _p1.x - _t1.x;
		float num8 = _p1.y - _t1.y;
		float num9 = _p1.x - _t2.x;
		float num10 = _p1.y - _t2.y;
		float num11 = _p1.x - _t3.x;
		float num12 = _p1.y - _t3.y;
		float num13 = num * num10 - num2 * num9;
		float num14 = num5 * num8 - num6 * num7;
		float num15 = num3 * num12 - num4 * num11;
		if (num13 >= 0f && num15 >= 0f)
		{
			return num14 >= 0f;
		}
		return false;
	}

	private static bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector3 t = shape[V[u]];
		Vector3 t2 = shape[V[v]];
		Vector3 t3 = shape[V[w]];
		if (Mathf.Epsilon > (t2.x - t.x) * (t3.y - t.y) - (t2.y - t.y) * (t3.x - t.x))
		{
			return false;
		}
		for (int i = 0; i < n; i++)
		{
			if (i != u && i != v && i != w)
			{
				Vector3 p = shape[V[i]];
				if (OverlapsPoint(t, t2, t3, p))
				{
					return false;
				}
			}
		}
		return true;
	}
}
