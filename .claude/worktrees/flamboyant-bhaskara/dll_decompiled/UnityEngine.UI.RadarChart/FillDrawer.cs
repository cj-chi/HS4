using System;

namespace UnityEngine.UI.RadarChart;

[AddComponentMenu("UI/RadarChart/Fill")]
public class FillDrawer : BaseDrawer
{
	[Serializable]
	public struct Status
	{
		[Range(0f, 1f)]
		public float fillPercent;

		public Vector2[] points;
	}

	public Status status;

	public static void Reset(ref Status rs)
	{
		rs.fillPercent = 0f;
		rs.points = null;
	}

	public static void Set(VertexHelper vh, Color color, ref Status status)
	{
		Vector2[] points = status.points;
		if (points == null)
		{
			return;
		}
		int segment = points.Length - 1;
		if (segment > 0)
		{
			float fillPercent = status.fillPercent;
			Func<int, Vector2> func = (int i) => Vector2.Lerp(points[segment], points[i], fillPercent);
			for (int num = 0; num < segment; num++)
			{
				Vector2 vector = points[num];
				Vector2 vector2 = ((num + 1 == segment) ? points[0] : points[num + 1]);
				Vector2 vector3 = ((num + 1 == segment) ? func(0) : func(num + 1));
				Vector2 vector4 = func(num);
				vh.AddUIVertexQuad(BaseDrawer.GetVert(color, vector, vector2, vector3, vector4));
			}
		}
	}

	protected override void OnPopulateMeshProcess(VertexHelper vh)
	{
		Set(vh, color, ref status);
	}
}
