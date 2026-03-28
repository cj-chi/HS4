using UnityEngine;

namespace AIProject;

public static class MathExtension
{
	public static bool IsInsideRange(this int source, int min, int max)
	{
		if (source >= min)
		{
			return source <= max;
		}
		return false;
	}

	public static bool GetInsideRange(this float source, float min, float max)
	{
		if (source >= min)
		{
			return source <= max;
		}
		return false;
	}

	public static Vector3 NearestVertexTo(this MeshFilter meshFilter, Vector3 point)
	{
		return NearestVertexTo(meshFilter.transform, meshFilter.mesh, point);
	}

	public static Vector3 NearestVertexTo(this MeshCollider collider, Vector3 point)
	{
		return NearestVertexTo(collider.transform, collider.sharedMesh, point);
	}

	public static Vector3 NearestVertexTo(Transform transform, Mesh mesh, Vector3 point)
	{
		point = transform.InverseTransformPoint(point);
		float num = float.PositiveInfinity;
		Vector3 position = Vector3.zero;
		Vector3[] vertices = mesh.vertices;
		foreach (Vector3 vector in vertices)
		{
			float sqrMagnitude = (point - vector).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				position = vector;
			}
		}
		return transform.TransformPoint(position);
	}
}
