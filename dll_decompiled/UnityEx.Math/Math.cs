using UnityEngine;

namespace UnityEx.Math;

public static class Math
{
	public static Vector2 NearestPointOnLine(Vector2 s, Vector2 e, Vector2 p)
	{
		Vector2 vector = e - s;
		if (vector.sqrMagnitude == 0f)
		{
			return s;
		}
		float num = (vector.x * (p - s).x + vector.y * (p - s).y) / vector.sqrMagnitude;
		if (num < 0f)
		{
			return s;
		}
		if (num > 1f)
		{
			return e;
		}
		return new Vector2((1f - num) * s.x + num * e.x, (1f - num) * s.y + num * e.y);
	}

	public static float LineToPointDistance(Vector2 s, Vector2 e, Vector2 p)
	{
		return (p - NearestPointOnLine(s, e, p)).magnitude;
	}

	public static bool CheckIntersection(Vector2 sA, Vector2 eA, Vector2 sB, Vector2 eB)
	{
		bool num = ((sA.x - eA.x) * (sB.y - sA.y) + (sA.y - eA.y) * (sA.x - sB.x)) * ((sA.x - eA.x) * (eB.y - sA.y) + (sA.y - eA.y) * (sA.x - eB.x)) < 0f;
		bool flag = ((sB.x - eB.x) * (sA.y - sB.y) + (sB.y - eB.y) * (sB.x - sA.x)) * ((sB.x - eB.x) * (eA.y - sB.y) + (sB.y - eB.y) * (sB.x - eA.x)) < 0f;
		return num && flag;
	}

	public static float Vector2Cross(Vector2 v1, Vector2 v2)
	{
		return v1.x * v2.y - v1.y * v2.x;
	}

	public static bool ColSegments(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2)
	{
		Vector3 vector = s2 - s1;
		float num = Vector2Cross(e1, e2);
		if (num == 0f)
		{
			return false;
		}
		float num2 = Vector2Cross(vector, e1);
		float num3 = Vector2Cross(vector, e2) / num;
		float num4 = num2 / num;
		if (num3 + 1E-05f < 0f || num3 - 1E-05f > 1f || num4 + 1E-05f < 0f || num4 - 1E-05f > 1f)
		{
			return false;
		}
		return true;
	}
}
