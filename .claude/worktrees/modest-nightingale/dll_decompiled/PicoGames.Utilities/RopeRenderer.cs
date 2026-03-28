using System;
using PicoGames.QuickRopes;
using UnityEngine;

namespace PicoGames.Utilities;

[AddComponentMenu("PicoGames/QuickRopes/Extensions/Rope Renderer")]
[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(QuickRope))]
public class RopeRenderer : MonoBehaviour
{
	public bool showBounds;

	public bool showEdges;

	public bool showNormals;

	[SerializeField]
	private int leafs = 6;

	[SerializeField]
	private int detail = 1;

	[SerializeField]
	private float center = 4f;

	[SerializeField]
	[Min(1f)]
	private int strandCount = 1;

	[SerializeField]
	private float strandOffset = 0.75f;

	[SerializeField]
	private float twistAngle;

	[SerializeField]
	private float radius = 1f;

	[SerializeField]
	private AnimationCurve radiusCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	[SerializeField]
	private Material material;

	[SerializeField]
	private Vector2 uvTile = new Vector2(1f, 1f);

	[SerializeField]
	private Vector2 uvOffset = new Vector2(0f, 0f);

	[SerializeField]
	[HideInInspector]
	private QuickRope rope;

	[SerializeField]
	[HideInInspector]
	private bool flagShapeUpdate = true;

	[SerializeField]
	[HideInInspector]
	private Vector3[] shapeLookup;

	[SerializeField]
	[HideInInspector]
	private int[] shapeTriIndices;

	[SerializeField]
	[HideInInspector]
	private int edgeCount;

	[SerializeField]
	[HideInInspector]
	private Vector3 kUp = Vector3.zero;

	[SerializeField]
	[HideInInspector]
	private Vector3[] positions;

	[SerializeField]
	[HideInInspector]
	private Quaternion[] rotations;

	[SerializeField]
	[HideInInspector]
	private Vector3[] directions;

	[SerializeField]
	[HideInInspector]
	private bool[] isJoints;

	[SerializeField]
	[HideInInspector]
	private Vector3[] vertices;

	[SerializeField]
	[HideInInspector]
	private Vector3[] normals;

	[SerializeField]
	[HideInInspector]
	private int[] triangles;

	[SerializeField]
	[HideInInspector]
	private Vector2[] uvs;

	private GameObject meshObject;

	private Mesh mesh;

	private MeshRenderer mRenderer;

	private MeshFilter mFilter;

	private bool dontRedraw;

	private static int vertIndex;

	private static int triaIndex;

	public int EdgeCount
	{
		get
		{
			return leafs;
		}
		set
		{
			if (leafs != value)
			{
				flagShapeUpdate = true;
				leafs = value;
			}
		}
	}

	public int EdgeDetail
	{
		get
		{
			return detail;
		}
		set
		{
			if (detail != value)
			{
				flagShapeUpdate = true;
				detail = value;
			}
		}
	}

	public float EdgeIndent
	{
		get
		{
			return center;
		}
		set
		{
			if (center != value)
			{
				flagShapeUpdate = true;
				center = value;
			}
		}
	}

	public int StrandCount
	{
		get
		{
			return strandCount;
		}
		set
		{
			if (strandCount != value)
			{
				strandCount = value;
			}
		}
	}

	public float StrandOffset
	{
		get
		{
			return strandOffset;
		}
		set
		{
			if (strandOffset != value)
			{
				strandOffset = value;
			}
		}
	}

	public float StrandTwist
	{
		get
		{
			return twistAngle;
		}
		set
		{
			if (twistAngle != value)
			{
				twistAngle = value;
			}
		}
	}

	public float Radius
	{
		get
		{
			return radius;
		}
		set
		{
			if (radius != value)
			{
				radius = value;
			}
		}
	}

	public AnimationCurve RadiusCurve
	{
		get
		{
			return radiusCurve;
		}
		set
		{
			radiusCurve = value;
		}
	}

	public Material Material
	{
		get
		{
			return material;
		}
		set
		{
			material = value;
		}
	}

	public Vector2 UVOffset
	{
		get
		{
			return uvOffset;
		}
		set
		{
			uvOffset = value;
		}
	}

	public Vector2 UVTile
	{
		get
		{
			return uvTile;
		}
		set
		{
			uvTile = value;
		}
	}

	private void OnDrawGizmos()
	{
		if (mesh != null && showBounds)
		{
			Gizmos.color = Color.gray;
			Gizmos.DrawWireCube(mesh.bounds.center, mesh.bounds.size);
		}
		if (vertices != null && (showEdges || showNormals))
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(vertices[i], Vector3.one * 0.01f);
				if (showNormals)
				{
					Gizmos.color = Color.magenta;
					Gizmos.DrawRay(vertices[i], normals[i]);
				}
			}
		}
		if (!showEdges)
		{
			return;
		}
		Gizmos.color = Color.blue;
		for (int j = 0; j < strandCount; j++)
		{
			for (int k = 0; k < positions.Length; k++)
			{
				for (int l = 0; l < edgeCount + 1; l++)
				{
					Gizmos.DrawLine(vertices[l + k * (edgeCount + 1)], vertices[(l + 1) % edgeCount + k * (edgeCount + 1)]);
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (meshObject != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(mesh);
				UnityEngine.Object.Destroy(meshObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(mesh);
				UnityEngine.Object.DestroyImmediate(meshObject);
			}
		}
	}

	private void OnDisable()
	{
		if (meshObject != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(mesh);
				UnityEngine.Object.Destroy(meshObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(mesh);
				UnityEngine.Object.DestroyImmediate(meshObject);
			}
		}
	}

	private void Start()
	{
		rope = base.gameObject.GetComponent<QuickRope>();
	}

	public void LateUpdate()
	{
		if (!Application.isPlaying || !dontRedraw)
		{
			if (flagShapeUpdate)
			{
				UpdateShape();
			}
			UpdatePositions();
			UpdateRotations();
			RedrawMesh();
			if (base.gameObject.isStatic)
			{
				dontRedraw = true;
			}
		}
	}

	private void VerifyMeshExists()
	{
		if (meshObject == null)
		{
			meshObject = GameObject.Find("Rope_Obj_" + base.gameObject.GetInstanceID());
			if (meshObject == null)
			{
				meshObject = new GameObject("Rope_Obj_" + base.gameObject.GetInstanceID(), typeof(MeshFilter), typeof(MeshRenderer));
			}
			meshObject.hideFlags = HideFlags.HideInHierarchy;
		}
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
		}
		if (mFilter == null)
		{
			mFilter = meshObject.GetComponent<MeshFilter>();
			if (mFilter == null)
			{
				mFilter = meshObject.AddComponent<MeshFilter>();
			}
		}
		if (mRenderer == null)
		{
			mRenderer = meshObject.GetComponent<MeshRenderer>();
			if (mRenderer == null)
			{
				mRenderer = meshObject.AddComponent<MeshRenderer>();
			}
		}
		if (material == null)
		{
			material = new Material(Shader.Find("Standard"));
		}
	}

	private void RedrawMesh()
	{
		strandCount = Mathf.Max(1, strandCount);
		edgeCount = Mathf.Max(3, edgeCount);
		detail = Mathf.Max(1, detail);
		VerifyMeshExists();
		int num = (edgeCount + 1) * positions.Length * strandCount + shapeLookup.Length * strandCount * 2;
		int num2 = 6 * edgeCount * positions.Length * strandCount + shapeTriIndices.Length * strandCount * 2;
		if (vertices == null || vertices.Length != num)
		{
			vertices = new Vector3[num];
		}
		if (normals == null || normals.Length != num)
		{
			normals = new Vector3[num];
		}
		if (uvs == null || uvs.Length != num)
		{
			uvs = new Vector2[num];
		}
		if (triangles == null || triangles.Length != num2)
		{
			triangles = new int[num2];
		}
		Vector3 lhs = Vector3.one * float.MaxValue;
		Vector3 lhs2 = Vector3.one * float.MinValue;
		vertIndex = (triaIndex = 0);
		Matrix4x4 matrix4x = default(Matrix4x4);
		for (int i = 0; i < strandCount; i++)
		{
			float f = 360f / (float)strandCount * (float)i * ((float)Math.PI / 180f);
			Vector3 vector = new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f);
			int num3 = vertIndex;
			for (int j = 0; j < positions.Length; j++)
			{
				float num4 = radiusCurve.Evaluate((float)j * (1f / (float)positions.Length)) * radius;
				matrix4x.SetTRS(positions[j] + ((strandCount > 1) ? (rotations[j] * vector * (num4 * strandOffset)) : Vector3.zero), rotations[j], Vector3.one * num4);
				for (int k = 0; k < edgeCount + 1; k++)
				{
					int num5 = k % shapeLookup.Length;
					vertices[vertIndex] = matrix4x.MultiplyPoint3x4(shapeLookup[num5]);
					normals[vertIndex] = rotations[j] * shapeLookup[num5];
					uvs[vertIndex] = new Vector2((float)k / (float)edgeCount * (float)edgeCount * uvTile.x + uvOffset.x, (float)j / (float)(positions.Length - 1) * (float)positions.Length * uvTile.y + uvOffset.y);
					lhs = Vector3.Min(lhs, vertices[vertIndex]);
					lhs2 = Vector3.Max(lhs2, vertices[vertIndex]);
					if (j < positions.Length - 1 && k < edgeCount && isJoints[j])
					{
						triangles[triaIndex++] = vertIndex;
						triangles[triaIndex++] = vertIndex + 1;
						triangles[triaIndex++] = vertIndex + edgeCount + 1;
						triangles[triaIndex++] = vertIndex + edgeCount + 1;
						triangles[triaIndex++] = vertIndex + 1;
						triangles[triaIndex++] = vertIndex + 1 + edgeCount + 1;
					}
					vertIndex++;
				}
			}
			int num6 = vertIndex - 1;
			for (int l = 0; l < shapeLookup.Length; l++)
			{
				vertices[vertIndex] = vertices[num3 + l];
				vertices[vertIndex + shapeLookup.Length] = vertices[num6 - l];
				normals[vertIndex] = rotations[0] * Vector3.back;
				normals[vertIndex + shapeLookup.Length] = rotations[rotations.Length - 1] * Vector3.forward;
				uvs[vertIndex] = new Vector2(shapeLookup[l].x, shapeLookup[l].y);
				uvs[vertIndex + shapeLookup.Length] = new Vector2(shapeLookup[l].x, shapeLookup[l].y);
				vertIndex++;
			}
			vertIndex += shapeLookup.Length;
			for (int m = 0; m < shapeTriIndices.Length; m++)
			{
				triangles[triaIndex] = num6 + 1 + shapeTriIndices[m];
				triangles[triaIndex + shapeTriIndices.Length] = num6 + shapeLookup.Length + 1 + shapeTriIndices[m];
				triaIndex++;
			}
			triaIndex += shapeTriIndices.Length;
		}
		mesh.Clear();
		mesh.MarkDynamic();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uvs;
		Vector3 size = new Vector3(lhs2.x - lhs.x, lhs2.y - lhs.y, lhs2.z - lhs.z);
		Vector3 vector2 = new Vector3(lhs.x + size.x * 0.5f, lhs.y + size.y * 0.5f, lhs.z + size.z * 0.5f);
		mesh.bounds = new Bounds(vector2, size);
		mFilter.sharedMesh = mesh;
		Material sharedMaterial = material;
		mRenderer.sharedMaterial = sharedMaterial;
	}

	private void UpdatePositions()
	{
		int num = rope.ActiveLinkCount + 1;
		if (positions == null || positions.Length != num)
		{
			Array.Resize(ref positions, num);
			Array.Resize(ref isJoints, num);
		}
		for (int i = 0; i <= rope.ActiveLinkCount; i++)
		{
			positions[i] = rope.Links[i].transform.position;
			isJoints[i] = (rope.Links[i].transform.GetComponent<ConfigurableJoint>() ? true : false);
		}
		if (rope.Spline.IsLooped)
		{
			positions[positions.Length - 1] = positions[0];
		}
		else
		{
			positions[positions.Length - 1] = rope.Links[rope.Links.Length - 1].transform.position;
		}
	}

	private void UpdateShape()
	{
		shapeLookup = Shape.GetRoseCurve(leafs, detail, center, _unitize: true);
		shapeTriIndices = Triangulate.Edge(shapeLookup);
		edgeCount = shapeLookup.Length;
		flagShapeUpdate = false;
	}

	private void UpdateRotations()
	{
		if (rotations == null || rotations.Length != positions.Length)
		{
			Array.Resize(ref rotations, positions.Length);
		}
		if (directions == null || directions.Length != positions.Length)
		{
			Array.Resize(ref directions, positions.Length);
		}
		for (int i = 0; i < positions.Length - 1; i++)
		{
			directions[i].Set(positions[i + 1].x - positions[i].x, positions[i + 1].y - positions[i].y, positions[i + 1].z - positions[i].z);
		}
		directions[directions.Length - 1] = directions[directions.Length - 2];
		Vector3 zero = Vector3.zero;
		Vector3 vector = ((!(kUp == Vector3.zero)) ? kUp : ((directions[0].x == 0f && directions[0].z == 0f) ? Vector3.right : Vector3.up));
		for (int j = 0; j < positions.Length; j++)
		{
			if (j != 0 && j != positions.Length - 1)
			{
				zero.Set(directions[j].x + directions[j - 1].x, directions[j].y + directions[j - 1].y, directions[j].z + directions[j - 1].z);
			}
			else if (positions[0] == positions[positions.Length - 1])
			{
				zero.Set(directions[positions.Length - 1].x + directions[0].x, directions[positions.Length - 1].y + directions[0].y, directions[positions.Length - 1].z + directions[0].z);
			}
			else
			{
				zero.Set(directions[j].x, directions[j].y, directions[j].z);
			}
			if (zero == Vector3.zero)
			{
				rotations[j] = Quaternion.identity;
				continue;
			}
			zero.Normalize();
			Vector3 rhs = Vector3.Cross(vector, zero);
			vector = Vector3.Cross(zero, rhs);
			if (j == 0)
			{
				kUp = vector;
			}
			if (twistAngle != 0f)
			{
				vector = Quaternion.AngleAxis(twistAngle, zero) * vector;
			}
			rotations[j].SetLookRotation(zero, vector);
		}
		if (rope.Spline.IsLooped)
		{
			rotations[rotations.Length - 1] = rotations[0];
		}
	}
}
