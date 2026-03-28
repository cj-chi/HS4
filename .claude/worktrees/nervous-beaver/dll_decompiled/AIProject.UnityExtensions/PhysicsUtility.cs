using System.Diagnostics;
using UnityEngine;

namespace AIProject.UnityExtensions;

public static class PhysicsUtility
{
	public static bool CheckPointInPolygon(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
	{
		Vector3 lhs = b - a;
		Vector3 rhs = p - b;
		Vector3 lhs2 = c - b;
		Vector3 rhs2 = p - c;
		Vector3 lhs3 = a - c;
		Vector3 rhs3 = p - a;
		Vector3 lhs4 = Vector3.Cross(lhs, rhs);
		Vector3 rhs4 = Vector3.Cross(lhs2, rhs2);
		Vector3 rhs5 = Vector3.Cross(lhs3, rhs3);
		float num = Vector3.Dot(lhs4, rhs4);
		float num2 = Vector3.Dot(lhs4, rhs5);
		if (num > 0f)
		{
			return num2 > 0f;
		}
		return false;
	}

	public static bool CheckInsideFOV(Vector2 angle, Transform transform, Transform target, float viewDistance)
	{
		return CheckInsideFOV(angle, transform, target.position, viewDistance);
	}

	public static bool CheckInsideFOV(Vector2 angle, Transform transform, Vector3 targetPosition, float viewDistance)
	{
		if (Vector3.Distance(transform.position, targetPosition) > viewDistance)
		{
			return false;
		}
		Vector2 vector = angle / 2f;
		Vector3 vector2 = targetPosition - transform.position;
		Vector3 to = Vector3.Normalize(new Vector3(vector2.x, 0f, vector2.y));
		float num = Vector3.Angle(transform.forward, to);
		if (num > 180f)
		{
			num = Mathf.Abs(360f - num);
		}
		if (num > vector.x)
		{
			return false;
		}
		return true;
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawWireFOV(float hAngle, float vAngle, Vector3 position, Quaternion rotation, float viewDistance)
	{
	}
}
