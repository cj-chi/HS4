using UnityEngine;

namespace Obi;

[ExecuteInEditMode]
[RequireComponent(typeof(ObiCollider))]
public class ObiDistanceFieldRenderer : MonoBehaviour
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	public Axis axis;

	[Range(0f, 1f)]
	public float slice = 0.25f;

	public float maxDistance = 0.5f;

	private ObiCollider unityCollider;

	private Material material;

	private Mesh planeMesh;

	private Texture2D cutawayTexture;

	private float sampleSize;

	private int sampleCount;

	private Color boundsColor = new Color(1f, 1f, 1f, 0.5f);

	public void Awake()
	{
		unityCollider = GetComponent<ObiCollider>();
	}

	public void OnEnable()
	{
		material = Object.Instantiate(Resources.Load<Material>("ObiMaterials/DistanceFieldRendering"));
		material.hideFlags = HideFlags.HideAndDontSave;
	}

	public void OnDisable()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		Object.DestroyImmediate(cutawayTexture);
		Object.DestroyImmediate(planeMesh);
		Object.DestroyImmediate(material);
	}

	private void ResizeTexture()
	{
		if (cutawayTexture == null)
		{
			cutawayTexture = new Texture2D(sampleCount, sampleCount, TextureFormat.RHalf, mipChain: false);
			cutawayTexture.wrapMode = TextureWrapMode.Clamp;
			cutawayTexture.hideFlags = HideFlags.HideAndDontSave;
		}
		else
		{
			cutawayTexture.Resize(sampleCount, sampleCount);
		}
	}

	private void CreatePlaneMesh(ObiDistanceField field)
	{
		if (field != null && planeMesh == null)
		{
			float num = (1f - field.FieldBounds.size[0] / (sampleSize * (float)sampleCount)) * 0.5f;
			planeMesh = new Mesh();
			planeMesh.vertices = new Vector3[4]
			{
				new Vector3(-0.5f, -0.5f, 0f),
				new Vector3(0.5f, -0.5f, 0f),
				new Vector3(-0.5f, 0.5f, 0f),
				new Vector3(0.5f, 0.5f, 0f)
			};
			planeMesh.uv = new Vector2[4]
			{
				new Vector2(num, num),
				new Vector2(1f - num, num),
				new Vector2(num, 1f - num),
				new Vector2(1f - num, 1f - num)
			};
			planeMesh.normals = new Vector3[4]
			{
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward
			};
			planeMesh.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		}
	}

	private void RefreshCutawayTexture(ObiDistanceField field)
	{
		if (field == null)
		{
			return;
		}
		Bounds fieldBounds = field.FieldBounds;
		sampleSize = field.EffectiveSampleSize;
		sampleCount = (int)(fieldBounds.size[0] / sampleSize) + 1;
		CreatePlaneMesh(field);
		ResizeTexture();
		float num = (float)sampleCount * slice * sampleSize;
		Vector3 vector = fieldBounds.center - fieldBounds.extents;
		for (int i = 0; i < sampleCount; i++)
		{
			for (int j = 0; j < sampleCount; j++)
			{
				Vector3 vector2 = Vector3.zero;
				switch (axis)
				{
				case Axis.X:
					vector2 = new Vector3(num, (float)j * sampleSize, (float)i * sampleSize);
					break;
				case Axis.Y:
					vector2 = new Vector3((float)i * sampleSize, num, (float)j * sampleSize);
					break;
				case Axis.Z:
					vector2 = new Vector3((float)i * sampleSize, (float)j * sampleSize, num);
					break;
				}
				Vector4 vector3 = vector + vector2;
				float r = Oni.SampleDistanceField(field.OniDistanceField, vector3.x, vector3.y, vector3.z).Remap(0f - maxDistance, maxDistance, 0f, 1f);
				cutawayTexture.SetPixel(i, j, new Color(r, 0f, 0f));
			}
		}
		cutawayTexture.Apply();
	}

	private void DrawCutawayPlane(ObiDistanceField field, Matrix4x4 matrix)
	{
		if (!(field == null))
		{
			RefreshCutawayTexture(field);
			material.mainTexture = cutawayTexture;
			material.SetPass(0);
			Quaternion q = Quaternion.identity;
			Vector3 zero = Vector3.zero;
			zero[(int)axis] = field.FieldBounds.size[0];
			if (axis == Axis.Y)
			{
				q = Quaternion.Euler(90f, 0f, 0f);
			}
			else if (axis == Axis.X)
			{
				q = Quaternion.Euler(0f, -90f, 0f);
			}
			Matrix4x4 matrix4x = Matrix4x4.TRS(field.FieldBounds.center + zero * (slice - 0.5f), q, Vector3.one * field.FieldBounds.size[0]);
			Graphics.DrawMeshNow(planeMesh, matrix * matrix4x);
		}
	}

	public void OnDrawGizmos()
	{
		if (unityCollider != null && unityCollider.distanceField != null && unityCollider.distanceField.Initialized && material != null)
		{
			DrawCutawayPlane(unityCollider.distanceField, base.transform.localToWorldMatrix);
			Gizmos.color = boundsColor;
			Gizmos.DrawWireCube(unityCollider.distanceField.FieldBounds.center, unityCollider.distanceField.FieldBounds.size);
		}
	}
}
