using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

internal static class Primitives
{
	[NotNull]
	public static Mesh CreateFullscreenQuad()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, -1f, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 1f),
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		Mesh mesh2 = mesh;
		mesh2.SetIndices(new int[6] { 0, 1, 2, 2, 3, 0 }, MeshTopology.Triangles, 0);
		return mesh2;
	}

	[NotNull]
	public static Mesh CreateWireframeBox(float width, float height, float depth)
	{
		Mesh mesh = new Mesh();
		mesh.Clear();
		Vector3 vector = new Vector3((0f - width) * 0.5f, (0f - height) * 0.5f, depth * 0.5f);
		Vector3 vector2 = new Vector3(width * 0.5f, (0f - height) * 0.5f, depth * 0.5f);
		Vector3 vector3 = new Vector3(width * 0.5f, (0f - height) * 0.5f, (0f - depth) * 0.5f);
		Vector3 vector4 = new Vector3((0f - width) * 0.5f, (0f - height) * 0.5f, (0f - depth) * 0.5f);
		Vector3 vector5 = new Vector3((0f - width) * 0.5f, height * 0.5f, depth * 0.5f);
		Vector3 vector6 = new Vector3(width * 0.5f, height * 0.5f, depth * 0.5f);
		Vector3 vector7 = new Vector3(width * 0.5f, height * 0.5f, (0f - depth) * 0.5f);
		Vector3 vector8 = new Vector3((0f - width) * 0.5f, height * 0.5f, (0f - depth) * 0.5f);
		Vector3[] vertices = new Vector3[8] { vector, vector2, vector3, vector4, vector5, vector6, vector7, vector8 };
		int[] indices = new int[24]
		{
			0, 1, 1, 2, 2, 3, 3, 0, 4, 5,
			5, 6, 6, 7, 7, 4, 0, 4, 1, 5,
			2, 6, 3, 7
		};
		mesh.vertices = vertices;
		mesh.SetIndices(indices, MeshTopology.Lines, 0);
		mesh.RecalculateBounds();
		return mesh;
	}

	[NotNull]
	public static Mesh CreateBox(float width, float height, float depth)
	{
		Mesh mesh = new Mesh();
		mesh.Clear();
		Vector3 vector = new Vector3((0f - width) * 0.5f, (0f - height) * 0.5f, depth * 0.5f);
		Vector3 vector2 = new Vector3(width * 0.5f, (0f - height) * 0.5f, depth * 0.5f);
		Vector3 vector3 = new Vector3(width * 0.5f, (0f - height) * 0.5f, (0f - depth) * 0.5f);
		Vector3 vector4 = new Vector3((0f - width) * 0.5f, (0f - height) * 0.5f, (0f - depth) * 0.5f);
		Vector3 vector5 = new Vector3((0f - width) * 0.5f, height * 0.5f, depth * 0.5f);
		Vector3 vector6 = new Vector3(width * 0.5f, height * 0.5f, depth * 0.5f);
		Vector3 vector7 = new Vector3(width * 0.5f, height * 0.5f, (0f - depth) * 0.5f);
		Vector3 vector8 = new Vector3((0f - width) * 0.5f, height * 0.5f, (0f - depth) * 0.5f);
		Vector3[] vertices = new Vector3[24]
		{
			vector, vector2, vector3, vector4, vector8, vector5, vector, vector4, vector5, vector6,
			vector2, vector, vector7, vector8, vector4, vector3, vector6, vector7, vector3, vector2,
			vector8, vector7, vector6, vector5
		};
		Vector3 up = Vector3.up;
		Vector3 down = Vector3.down;
		Vector3 forward = Vector3.forward;
		Vector3 back = Vector3.back;
		Vector3 left = Vector3.left;
		Vector3 right = Vector3.right;
		Vector3[] normals = new Vector3[24]
		{
			down, down, down, down, left, left, left, left, forward, forward,
			forward, forward, back, back, back, back, right, right, right, right,
			up, up, up, up
		};
		Vector2 vector9 = new Vector2(0f, 0f);
		Vector2 vector10 = new Vector2(1f, 0f);
		Vector2 vector11 = new Vector2(0f, 1f);
		Vector2 vector12 = new Vector2(1f, 1f);
		Vector2[] uv = new Vector2[24]
		{
			vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11,
			vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10,
			vector12, vector11, vector9, vector10
		};
		int[] indices = new int[36]
		{
			3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
			6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
			12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
			23, 21, 20, 23, 22, 21
		};
		int[] indices2 = new int[24]
		{
			0, 1, 1, 2, 2, 3, 3, 0, 20, 21,
			21, 22, 22, 23, 23, 20, 0, 23, 1, 22,
			2, 21, 3, 20
		};
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.subMeshCount = 2;
		mesh.SetIndices(indices, MeshTopology.Triangles, 0);
		mesh.SetIndices(indices2, MeshTopology.Lines, 1);
		mesh.RecalculateBounds();
		return mesh;
	}
}
