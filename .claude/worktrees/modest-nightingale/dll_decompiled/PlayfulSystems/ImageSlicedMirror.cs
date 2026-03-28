using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace PlayfulSystems;

public class ImageSlicedMirror : Image
{
	private static readonly Vector2[] s_VertScratch = new Vector2[4];

	private static readonly Vector2[] s_UVScratch = new Vector2[4];

	private static readonly float[] s_UVMultiplierScratch = new float[4];

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if (base.overrideSprite == null)
		{
			base.OnPopulateMesh(toFill);
		}
		else if (base.hasBorder && base.type == Type.Sliced)
		{
			GenerateSlicedFilledSprite(toFill);
		}
		else
		{
			base.OnPopulateMesh(toFill);
		}
	}

	private void GenerateSlicedFilledSprite(VertexHelper toFill)
	{
		Vector4 outer;
		Vector4 inner;
		Vector4 padding;
		Vector4 vector;
		if (base.overrideSprite != null)
		{
			outer = DataUtility.GetOuterUV(base.overrideSprite);
			inner = DataUtility.GetInnerUV(base.overrideSprite);
			padding = DataUtility.GetPadding(base.overrideSprite);
			vector = base.overrideSprite.border;
		}
		else
		{
			outer = Vector4.zero;
			inner = Vector4.zero;
			padding = Vector4.zero;
			vector = Vector4.zero;
		}
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		vector = GetAdjustedBorders(vector / base.pixelsPerUnit, pixelAdjustedRect);
		padding /= base.pixelsPerUnit;
		SetSlicedVerts(pixelAdjustedRect, vector, padding);
		SetSlicedUVs(outer, inner, vector);
		toFill.Clear();
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			for (int j = 0; j < 3; j++)
			{
				if (base.fillCenter || i != 1 || j != 1)
				{
					int num2 = j + 1;
					AddQuad(toFill, new Vector2(s_VertScratch[i].x, s_VertScratch[j].y), new Vector2(s_VertScratch[num].x, s_VertScratch[num2].y), color, new Vector2(s_UVScratch[i].x, s_UVScratch[j].y), new Vector2(s_UVScratch[num].x, s_UVScratch[num2].y));
				}
			}
		}
	}

	private void SetSlicedVerts(Rect rect, Vector4 border, Vector4 padding)
	{
		s_VertScratch[0] = new Vector2(padding.x, padding.y);
		s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);
		s_VertScratch[1].x = border.x;
		s_VertScratch[1].y = border.y;
		s_VertScratch[2].x = rect.width - border.z;
		s_VertScratch[2].y = rect.height - border.w;
		for (int i = 0; i < 4; i++)
		{
			s_VertScratch[i].x += rect.x;
			s_VertScratch[i].y += rect.y;
		}
	}

	private void SetSlicedUVs(Vector4 outer, Vector4 inner, Vector4 border)
	{
		bool flag = border.x < base.overrideSprite.border.x || border.z < base.overrideSprite.border.z;
		bool flag2 = border.y < base.overrideSprite.border.y || border.w < base.overrideSprite.border.w;
		if (!flag && !flag2)
		{
			s_UVScratch[0] = new Vector2(outer.x, outer.y);
			s_UVScratch[1] = new Vector2(inner.x, inner.y);
			s_UVScratch[2] = new Vector2(inner.z, inner.w);
			s_UVScratch[3] = new Vector2(outer.z, outer.w);
			return;
		}
		s_UVMultiplierScratch[0] = ((border.x != 0f && flag) ? (border.x / base.overrideSprite.border.x) : 1f);
		s_UVMultiplierScratch[1] = ((border.y != 0f && flag2) ? (border.y / base.overrideSprite.border.y) : 1f);
		s_UVMultiplierScratch[2] = ((border.z != 0f && flag) ? (border.z / base.overrideSprite.border.z) : 1f);
		s_UVMultiplierScratch[3] = ((border.w != 0f && flag2) ? (border.w / base.overrideSprite.border.w) : 1f);
		s_UVScratch[0] = new Vector2(outer.x, outer.y);
		s_UVScratch[1] = new Vector2(inner.x * s_UVMultiplierScratch[0], inner.y * s_UVMultiplierScratch[1]);
		s_UVScratch[2] = new Vector2(outer.z - (outer.z - inner.z) * s_UVMultiplierScratch[2], outer.w - (outer.w - inner.w) * s_UVMultiplierScratch[3]);
		s_UVScratch[3] = new Vector2(outer.z, outer.w);
	}

	private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
	{
		int currentVertCount = vertexHelper.currentVertCount;
		vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y));
		vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y));
		vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y));
		vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y));
		vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
	}

	private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
	{
		for (int i = 0; i <= 1; i++)
		{
			float num = border[i] + border[i + 2];
			if (rect.size[i] < num && num != 0f)
			{
				float num2 = rect.size[i] / num;
				border[i] *= num2;
				border[i + 2] *= num2;
			}
		}
		return border;
	}
}
