using System;
using UnityEngine;

public class RadialFillCursor : MonoBehaviour
{
	[Header("Radial Data")]
	public float radius = 8f;

	public float strenght = 1f;

	public float strenghtMax = 10f;

	public float angle = 90f;

	public float rotationAngle = -45f;

	[Header("Input Speed")]
	public float radiusSpeed = 8f;

	public float strenghtSpeed = 8f;

	public float rotationSpeed = 80f;

	public float angleSpeed = 80f;

	[Header("Mesh Data")]
	public int meshAngleSeparation = 50;

	public MeshFilter meshFilter;

	public Color centerColor;

	public Color externalColor;

	private Vector3[] vertices;

	private Color[] colors;

	private int[] triangles;

	private Vector3 tempV3 = Vector3.right;

	private void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 10f;
		base.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
		if (Input.GetKey(KeyCode.W))
		{
			radius += Time.deltaTime * radiusSpeed;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			radius -= Time.deltaTime * radiusSpeed;
		}
		if (radius < 0f)
		{
			radius = 0f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			rotationAngle += Time.deltaTime * rotationSpeed;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			rotationAngle -= Time.deltaTime * rotationSpeed;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			angle += Time.deltaTime * angleSpeed;
		}
		else if (Input.GetKey(KeyCode.E))
		{
			angle -= Time.deltaTime * angleSpeed;
		}
		angle = Mathf.Clamp(angle, 0f, 360f);
		if (Input.GetKey(KeyCode.X))
		{
			strenght += Time.deltaTime * strenghtSpeed;
		}
		else if (Input.GetKey(KeyCode.Z))
		{
			strenght -= Time.deltaTime * strenghtSpeed;
		}
		strenght = Mathf.Clamp(strenght, 0.1f, strenghtMax);
		Color color = centerColor;
		color.a = 0.34f + strenght / strenghtMax * 0.66f;
		centerColor = color;
		if (vertices == null || vertices.Length != meshAngleSeparation + 1)
		{
			vertices = new Vector3[meshAngleSeparation + 2];
			colors = new Color[meshAngleSeparation + 2];
			triangles = new int[meshAngleSeparation * 3 + 3];
		}
		Mesh mesh = meshFilter.mesh;
		if (mesh == null)
		{
			mesh = new Mesh();
			meshFilter.mesh = mesh;
		}
		vertices[0] = Vector3.zero;
		colors[0] = centerColor;
		for (int i = 1; i < meshAngleSeparation + 2; i++)
		{
			float num = angle / (float)meshAngleSeparation * (float)(i - 1) + rotationAngle;
			float num2 = Mathf.Cos((float)Math.PI / 180f * num);
			float num3 = Mathf.Sin((float)Math.PI / 180f * num);
			float x = radius * num2;
			float y = radius * num3;
			tempV3.x = x;
			tempV3.y = y;
			tempV3.z = 0f;
			vertices[i] = tempV3;
			colors[i] = externalColor;
		}
		int num4 = 0;
		for (int j = 0; j < meshAngleSeparation * 3; j += 3)
		{
			triangles[j] = 0;
			triangles[j + 1] = num4 + 2;
			triangles[j + 2] = num4 + 1;
			num4++;
		}
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
	}

	public void Show(bool b)
	{
		meshFilter.gameObject.SetActive(b);
	}
}
