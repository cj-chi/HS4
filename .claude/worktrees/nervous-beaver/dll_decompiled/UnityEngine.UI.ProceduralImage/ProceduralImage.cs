using System;

namespace UnityEngine.UI.ProceduralImage;

[ExecuteInEditMode]
[AddComponentMenu("UI/Procedural Image")]
public class ProceduralImage : Image
{
	[SerializeField]
	private float borderWidth;

	private ProceduralImageModifier modifier;

	private static Material materialInstance;

	[SerializeField]
	private float falloffDistance = 1f;

	public float BorderWidth
	{
		get
		{
			return borderWidth;
		}
		set
		{
			borderWidth = value;
			SetVerticesDirty();
		}
	}

	public float FalloffDistance
	{
		get
		{
			return falloffDistance;
		}
		set
		{
			falloffDistance = value;
			SetVerticesDirty();
		}
	}

	protected ProceduralImageModifier Modifier
	{
		get
		{
			if (modifier == null)
			{
				modifier = GetComponent<ProceduralImageModifier>();
				if (modifier == null)
				{
					ModifierType = typeof(FreeModifier);
				}
			}
			return modifier;
		}
		set
		{
			modifier = value;
		}
	}

	public System.Type ModifierType
	{
		get
		{
			return Modifier.GetType();
		}
		set
		{
			if (GetComponent<ProceduralImageModifier>() != null)
			{
				Object.DestroyImmediate(GetComponent<ProceduralImageModifier>());
			}
			base.gameObject.AddComponent(value);
			Modifier = GetComponent<ProceduralImageModifier>();
			SetAllDirty();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Init();
	}

	private void Init()
	{
		base.preserveAspect = false;
		if (base.sprite == null)
		{
			base.sprite = EmptySprite.Get();
		}
		if (materialInstance == null)
		{
			materialInstance = new Material(Shader.Find("UI/Procedural UI Image"));
		}
		material = materialInstance;
	}

	private Vector4 FixRadius(Vector4 vec)
	{
		Rect rect = base.rectTransform.rect;
		vec = new Vector4(Mathf.Max(vec.x, 0f), Mathf.Max(vec.y, 0f), Mathf.Max(vec.z, 0f), Mathf.Max(vec.w, 0f));
		float num = Mathf.Min(rect.width / (vec.x + vec.y), rect.width / (vec.z + vec.w), rect.height / (vec.x + vec.w), rect.height / (vec.z + vec.y), 1f);
		return vec * num;
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		base.OnPopulateMesh(toFill);
		EncodeAllInfoIntoVertices(toFill, CalculateInfo());
	}

	private ProceduralImageInfo CalculateInfo()
	{
		if (base.sprite == null)
		{
			base.sprite = EmptySprite.Get();
		}
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		Vector3[] array = new Vector3[4];
		base.rectTransform.GetWorldCorners(array);
		float num = Vector3.Distance(array[1], array[2]) / pixelAdjustedRect.width;
		num /= Mathf.Max(0f, falloffDistance);
		Vector4 vector = FixRadius(Modifier.CalculateRadius(pixelAdjustedRect));
		float num2 = Mathf.Min(pixelAdjustedRect.width, pixelAdjustedRect.height);
		return new ProceduralImageInfo(pixelAdjustedRect.width + falloffDistance, pixelAdjustedRect.height + falloffDistance, falloffDistance, num, vector / num2, borderWidth / num2 * 2f);
	}

	private void EncodeAllInfoIntoVertices(VertexHelper vh, ProceduralImageInfo info)
	{
		UIVertex vertex = default(UIVertex);
		Vector2 uv = new Vector2(info.width, info.height);
		Vector2 uv2 = new Vector2(EncodeFloats_0_1_16_16(info.radius.x, info.radius.y), EncodeFloats_0_1_16_16(info.radius.z, info.radius.w));
		Vector2 uv3 = new Vector2((info.borderWidth == 0f) ? 1f : Mathf.Clamp01(info.borderWidth), info.pixelSize);
		for (int i = 0; i < vh.currentVertCount; i++)
		{
			vh.PopulateUIVertex(ref vertex, i);
			vertex.position += ((Vector3)vertex.uv0 - new Vector3(0.5f, 0.5f)) * info.fallOffDistance;
			vertex.uv1 = uv;
			vertex.uv2 = uv2;
			vertex.uv3 = uv3;
			vh.SetUIVertex(vertex, i);
		}
	}

	private float EncodeFloats_0_1_16_16(float a, float b)
	{
		return Vector2.Dot(rhs: new Vector2(1f, 1.5259022E-05f), lhs: new Vector2(Mathf.Floor(a * 65534f) / 65535f, Mathf.Floor(b * 65534f) / 65535f));
	}
}
