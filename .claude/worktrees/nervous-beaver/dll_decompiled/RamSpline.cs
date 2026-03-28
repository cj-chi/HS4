using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class RamSpline : MonoBehaviour
{
	public SplineProfile currentProfile;

	public SplineProfile oldProfile;

	public List<RamSpline> beginnigChildSplines = new List<RamSpline>();

	public List<RamSpline> endingChildSplines = new List<RamSpline>();

	public RamSpline beginningSpline;

	public RamSpline endingSpline;

	public int beginningConnectionID;

	public int endingConnectionID;

	public float beginningMinWidth = 0.5f;

	public float beginningMaxWidth = 1f;

	public float endingMinWidth = 0.5f;

	public float endingMaxWidth = 1f;

	public int toolbarInt;

	public bool invertUVDirection;

	public bool uvRotation = true;

	public MeshFilter meshfilter;

	public List<Vector4> controlPoints = new List<Vector4>();

	public List<Quaternion> controlPointsRotations = new List<Quaternion>();

	public List<Quaternion> controlPointsOrientation = new List<Quaternion>();

	public List<Vector3> controlPointsUp = new List<Vector3>();

	public List<Vector3> controlPointsDown = new List<Vector3>();

	public List<float> controlPointsSnap = new List<float>();

	public AnimationCurve meshCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public List<AnimationCurve> controlPointsMeshCurves = new List<AnimationCurve>();

	public bool normalFromRaycast;

	public bool snapToTerrain;

	public LayerMask snapMask = 1;

	public List<Vector3> points = new List<Vector3>();

	public List<Vector3> pointsUp = new List<Vector3>();

	public List<Vector3> pointsDown = new List<Vector3>();

	public List<Vector3> points2 = new List<Vector3>();

	public List<Vector3> verticesBeginning = new List<Vector3>();

	public List<Vector3> verticesEnding = new List<Vector3>();

	public List<Vector3> normalsBeginning = new List<Vector3>();

	public List<Vector3> normalsEnding = new List<Vector3>();

	public List<float> widths = new List<float>();

	public List<float> snaps = new List<float>();

	public List<float> lerpValues = new List<float>();

	public List<Quaternion> orientations = new List<Quaternion>();

	public List<Vector3> tangents = new List<Vector3>();

	public List<Vector3> normalsList = new List<Vector3>();

	public Color[] colors;

	public List<Vector2> colorsFlowMap = new List<Vector2>();

	public float minVal = 0.5f;

	public float maxVal = 0.5f;

	public float width = 4f;

	public int vertsInShape = 3;

	public float traingleDensity = 0.2f;

	public float uvScale = 3f;

	public Material oldMaterial;

	public bool showVertexColors;

	public bool showFlowMap;

	public bool overrideFlowMap;

	public bool drawOnMesh;

	public bool drawOnMeshFlowMap;

	public bool uvScaleOverride;

	public bool debug;

	public Color drawColor = Color.black;

	public bool drawOnMultiple;

	public float flowSpeed = 1f;

	public float flowDirection;

	public AnimationCurve flowFlat = new AnimationCurve(new Keyframe(0f, 0.025f), new Keyframe(0.5f, 0.05f), new Keyframe(1f, 0.025f));

	public AnimationCurve flowWaterfall = new AnimationCurve(new Keyframe(0f, 0.25f), new Keyframe(1f, 0.25f));

	public float opacity = 0.1f;

	public float drawSize = 1f;

	public float length;

	public float fulllength;

	public float minMaxWidth;

	public float uvWidth;

	public float uvBeginning;

	public bool generateMeshParts;

	public int meshPartsCount = 3;

	public List<Transform> meshesPartTransforms = new List<Transform>();

	public float simulatedRiverLength = 100f;

	public int simulatedRiverPoints = 10;

	public float simulationMinStepSize = 1f;

	public AnimationCurve terrainCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public int detailTerrain = 100;

	public int detailTerrainForward = 100;

	public float terrainDepthHeight;

	public float terrainDepthMultiplier = 1f;

	public float terrainAdditionalWidth = 2f;

	public float terrainSmoothMultiplier = 1f;

	public void Start()
	{
		GenerateSpline();
	}

	public static RamSpline CreateSpline(Material splineMaterial = null, List<Vector4> positions = null)
	{
		GameObject obj = new GameObject("RamSpline");
		RamSpline ramSpline = obj.AddComponent<RamSpline>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		if (splineMaterial != null)
		{
			meshRenderer.sharedMaterial = splineMaterial;
		}
		if (positions != null)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				ramSpline.AddPoint(positions[i]);
			}
		}
		return ramSpline;
	}

	public void AddPoint(Vector4 position)
	{
		if (position.w == 0f)
		{
			if (controlPoints.Count > 0)
			{
				position.w = controlPoints[controlPoints.Count - 1].w;
			}
			else
			{
				position.w = width;
			}
		}
		controlPointsRotations.Add(Quaternion.identity);
		controlPoints.Add(position);
		controlPointsSnap.Add(0f);
		controlPointsMeshCurves.Add(new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)));
	}

	public void AddPointAfter(int i)
	{
		Vector4 vector = controlPoints[i];
		if (i < controlPoints.Count - 1 && controlPoints.Count > i + 1)
		{
			Vector4 vector2 = controlPoints[i + 1];
			if (Vector3.Distance(vector2, vector) > 0f)
			{
				vector = (vector + vector2) * 0.5f;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else if (controlPoints.Count > 1 && i == controlPoints.Count - 1)
		{
			Vector4 vector3 = controlPoints[i - 1];
			if (Vector3.Distance(vector3, vector) > 0f)
			{
				vector += vector - vector3;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else
		{
			vector.x += 1f;
		}
		controlPoints.Insert(i + 1, vector);
		controlPointsRotations.Insert(i + 1, Quaternion.identity);
		controlPointsSnap.Insert(i + 1, 0f);
		controlPointsMeshCurves.Insert(i + 1, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)));
	}

	public void ChangePointPosition(int i, Vector3 position)
	{
		ChangePointPosition(i, new Vector4(position.x, position.y, position.z, 0f));
	}

	public void ChangePointPosition(int i, Vector4 position)
	{
		Vector4 vector = controlPoints[i];
		if (position.w == 0f)
		{
			position.w = vector.w;
		}
		controlPoints[i] = position;
	}

	public void RemovePoint(int i)
	{
		if (i < controlPoints.Count)
		{
			controlPoints.RemoveAt(i);
			controlPointsRotations.RemoveAt(i);
			controlPointsMeshCurves.RemoveAt(i);
			controlPointsSnap.RemoveAt(i);
		}
	}

	public void RemovePoints(int fromID = -1)
	{
		for (int num = controlPoints.Count - 1; num > fromID; num--)
		{
			RemovePoint(num);
		}
	}

	public void GenerateBeginningParentBased()
	{
		vertsInShape = (int)Mathf.Round((float)(beginningSpline.vertsInShape - 1) * (beginningMaxWidth - beginningMinWidth) + 1f);
		if (vertsInShape < 1)
		{
			vertsInShape = 1;
		}
		beginningConnectionID = beginningSpline.points.Count - 1;
		float w = beginningSpline.controlPoints[beginningSpline.controlPoints.Count - 1].w;
		w *= beginningMaxWidth - beginningMinWidth;
		Vector4 value = Vector3.Lerp(beginningSpline.pointsDown[beginningConnectionID], beginningSpline.pointsUp[beginningConnectionID], beginningMinWidth + (beginningMaxWidth - beginningMinWidth) * 0.5f) + beginningSpline.transform.position - base.transform.position;
		value.w = w;
		controlPoints[0] = value;
		if (!uvScaleOverride)
		{
			uvScale = beginningSpline.uvScale;
		}
	}

	public void GenerateEndingParentBased()
	{
		if (beginningSpline == null)
		{
			vertsInShape = (int)Mathf.Round((float)(endingSpline.vertsInShape - 1) * (endingMaxWidth - endingMinWidth) + 1f);
			if (vertsInShape < 1)
			{
				vertsInShape = 1;
			}
		}
		endingConnectionID = 0;
		float w = endingSpline.controlPoints[0].w;
		w *= endingMaxWidth - endingMinWidth;
		Vector4 value = Vector3.Lerp(endingSpline.pointsDown[endingConnectionID], endingSpline.pointsUp[endingConnectionID], endingMinWidth + (endingMaxWidth - endingMinWidth) * 0.5f) + endingSpline.transform.position - base.transform.position;
		value.w = w;
		controlPoints[controlPoints.Count - 1] = value;
	}

	public void GenerateSpline(List<RamSpline> generatedSplines = null)
	{
		generatedSplines = new List<RamSpline>();
		if ((bool)beginningSpline)
		{
			GenerateBeginningParentBased();
		}
		if ((bool)endingSpline)
		{
			GenerateEndingParentBased();
		}
		List<Vector4> list = new List<Vector4>();
		for (int i = 0; i < controlPoints.Count; i++)
		{
			if (i > 0)
			{
				if (Vector3.Distance(controlPoints[i], controlPoints[i - 1]) > 0f)
				{
					list.Add(controlPoints[i]);
				}
			}
			else
			{
				list.Add(controlPoints[i]);
			}
		}
		Mesh mesh = new Mesh();
		meshfilter = GetComponent<MeshFilter>();
		if (list.Count < 2)
		{
			mesh.Clear();
			meshfilter.mesh = mesh;
			return;
		}
		controlPointsOrientation = new List<Quaternion>();
		lerpValues.Clear();
		snaps.Clear();
		points.Clear();
		pointsUp.Clear();
		pointsDown.Clear();
		orientations.Clear();
		tangents.Clear();
		normalsList.Clear();
		widths.Clear();
		controlPointsUp.Clear();
		controlPointsDown.Clear();
		verticesBeginning.Clear();
		verticesEnding.Clear();
		normalsBeginning.Clear();
		normalsEnding.Clear();
		if (beginningSpline != null && beginningSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[0] = Quaternion.identity;
		}
		if (endingSpline != null && endingSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[controlPointsRotations.Count - 1] = Quaternion.identity;
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (j <= list.Count - 2)
			{
				CalculateCatmullRomSideSplines(list, j);
			}
		}
		if (beginningSpline != null && beginningSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[0] = Quaternion.Inverse(controlPointsOrientation[0]) * beginningSpline.controlPointsOrientation[beginningSpline.controlPointsOrientation.Count - 1];
		}
		if (endingSpline != null && endingSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[controlPointsRotations.Count - 1] = Quaternion.Inverse(controlPointsOrientation[controlPointsOrientation.Count - 1]) * endingSpline.controlPointsOrientation[0];
		}
		controlPointsOrientation = new List<Quaternion>();
		controlPointsUp.Clear();
		controlPointsDown.Clear();
		for (int k = 0; k < list.Count; k++)
		{
			if (k <= list.Count - 2)
			{
				CalculateCatmullRomSideSplines(list, k);
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			if (l <= list.Count - 2)
			{
				CalculateCatmullRomSplineParameters(list, l);
			}
		}
		for (int m = 0; m < controlPointsUp.Count; m++)
		{
			if (m <= controlPointsUp.Count - 2)
			{
				CalculateCatmullRomSpline(controlPointsUp, m, ref pointsUp);
			}
		}
		for (int n = 0; n < controlPointsDown.Count; n++)
		{
			if (n <= controlPointsDown.Count - 2)
			{
				CalculateCatmullRomSpline(controlPointsDown, n, ref pointsDown);
			}
		}
		GenerateMesh(ref mesh);
		if (generatedSplines == null)
		{
			return;
		}
		generatedSplines.Add(this);
		foreach (RamSpline beginnigChildSpline in beginnigChildSplines)
		{
			if (beginnigChildSpline != null && !generatedSplines.Contains(beginnigChildSpline) && (beginnigChildSpline.beginningSpline == this || beginnigChildSpline.endingSpline == this))
			{
				beginnigChildSpline.GenerateSpline(generatedSplines);
			}
		}
		foreach (RamSpline endingChildSpline in endingChildSplines)
		{
			if (endingChildSpline != null && !generatedSplines.Contains(endingChildSpline) && (endingChildSpline.beginningSpline == this || endingChildSpline.endingSpline == this))
			{
				endingChildSpline.GenerateSpline(generatedSplines);
			}
		}
	}

	private void CalculateCatmullRomSideSplines(List<Vector4> controlPoints, int pos)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[ClampListPos(pos + 2)];
		}
		int num = 0;
		if (pos == controlPoints.Count - 2)
		{
			num = 1;
		}
		for (int i = 0; i <= num; i++)
		{
			Vector3 catmullRomPosition = GetCatmullRomPosition(i, p, p2, p3, p4);
			Vector3 normalized = GetCatmullRomTangent(i, p, p2, p3, p4).normalized;
			Vector3 normalized2 = CalculateNormal(normalized, Vector3.up).normalized;
			Quaternion quaternion = ((!(normalized2 == normalized) || !(normalized2 == Vector3.zero)) ? Quaternion.LookRotation(normalized, normalized2) : Quaternion.identity);
			quaternion *= Quaternion.Lerp(controlPointsRotations[pos], controlPointsRotations[ClampListPos(pos + 1)], i);
			controlPointsOrientation.Add(quaternion);
			Vector3 item = catmullRomPosition + quaternion * (0.5f * controlPoints[pos + i].w * Vector3.right);
			Vector3 item2 = catmullRomPosition + quaternion * (0.5f * controlPoints[pos + i].w * Vector3.left);
			controlPointsUp.Add(item);
			controlPointsDown.Add(item2);
		}
	}

	private void CalculateCatmullRomSplineParameters(List<Vector4> controlPoints, int pos, bool initialPoints = false)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[ClampListPos(pos + 2)];
		}
		int num = Mathf.FloorToInt(1f / traingleDensity);
		float num2 = 1f;
		float num3 = 0f;
		if (pos > 0)
		{
			num3 = 1f;
		}
		for (num2 = num3; num2 <= (float)num; num2 += 1f)
		{
			float t = num2 * traingleDensity;
			CalculatePointParameters(controlPoints, pos, p, p2, p3, p4, t);
		}
		if (num2 < (float)num)
		{
			num2 = num;
			float t2 = num2 * traingleDensity;
			CalculatePointParameters(controlPoints, pos, p, p2, p3, p4, t2);
		}
	}

	private void CalculateCatmullRomSpline(List<Vector3> controlPoints, int pos, ref List<Vector3> points)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[ClampListPos(pos + 2)];
		}
		int num = Mathf.FloorToInt(1f / traingleDensity);
		float num2 = 1f;
		float num3 = 0f;
		if (pos > 0)
		{
			num3 = 1f;
		}
		for (num2 = num3; num2 <= (float)num; num2 += 1f)
		{
			float t = num2 * traingleDensity;
			CalculatePointPosition(controlPoints, pos, p, p2, p3, p4, t, ref points);
		}
		if (num2 < (float)num)
		{
			num2 = num;
			float t2 = num2 * traingleDensity;
			CalculatePointPosition(controlPoints, pos, p, p2, p3, p4, t2, ref points);
		}
	}

	private void CalculatePointPosition(List<Vector3> controlPoints, int pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, ref List<Vector3> points)
	{
		Vector3 catmullRomPosition = GetCatmullRomPosition(t, p0, p1, p2, p3);
		points.Add(catmullRomPosition);
		Vector3 normalized = GetCatmullRomTangent(t, p0, p1, p2, p3).normalized;
		_ = CalculateNormal(normalized, Vector3.up).normalized;
	}

	private void CalculatePointParameters(List<Vector4> controlPoints, int pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 catmullRomPosition = GetCatmullRomPosition(t, p0, p1, p2, p3);
		widths.Add(Mathf.Lerp(controlPoints[pos].w, controlPoints[ClampListPos(pos + 1)].w, t));
		if (controlPointsSnap.Count > pos + 1)
		{
			snaps.Add(Mathf.Lerp(controlPointsSnap[pos], controlPointsSnap[ClampListPos(pos + 1)], t));
		}
		else
		{
			snaps.Add(0f);
		}
		lerpValues.Add((float)pos + t);
		points.Add(catmullRomPosition);
		Vector3 normalized = GetCatmullRomTangent(t, p0, p1, p2, p3).normalized;
		Vector3 normalized2 = CalculateNormal(normalized, Vector3.up).normalized;
		Quaternion item = ((!(normalized2 == normalized) || !(normalized2 == Vector3.zero)) ? Quaternion.LookRotation(normalized, normalized2) : Quaternion.identity);
		item *= Quaternion.Lerp(controlPointsRotations[pos], controlPointsRotations[ClampListPos(pos + 1)], t);
		orientations.Add(item);
		tangents.Add(normalized);
		if (normalsList.Count > 0 && Vector3.Angle(normalsList[normalsList.Count - 1], normalized2) > 90f)
		{
			normalized2 *= -1f;
		}
		normalsList.Add(normalized2);
	}

	private int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = controlPoints.Count - 1;
		}
		if (pos > controlPoints.Count)
		{
			pos = 1;
		}
		else if (pos > controlPoints.Count - 1)
		{
			pos = 0;
		}
		return pos;
	}

	private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 vector = 2f * p1;
		Vector3 vector2 = p2 - p0;
		Vector3 vector3 = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 vector4 = -p0 + 3f * p1 - 3f * p2 + p3;
		return 0.5f * (vector + vector2 * t + vector3 * t * t + vector4 * t * t * t);
	}

	private Vector3 GetCatmullRomTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		return 0.5f * (-p0 + p2 + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t);
	}

	private Vector3 CalculateNormal(Vector3 tangent, Vector3 up)
	{
		Vector3 rhs = Vector3.Cross(up, tangent);
		return Vector3.Cross(tangent, rhs);
	}

	private void GenerateMesh(ref Mesh mesh)
	{
		foreach (Transform meshesPartTransform in meshesPartTransforms)
		{
			if (meshesPartTransform != null)
			{
				UnityEngine.Object.DestroyImmediate(meshesPartTransform.gameObject);
			}
		}
		int num = points.Count - 1;
		int count = points.Count;
		int num2 = vertsInShape * count;
		List<int> list = new List<int>();
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		Vector2[] array3 = new Vector2[num2];
		Vector2[] array4 = new Vector2[num2];
		Vector2[] array5 = new Vector2[num2];
		if (colors == null || colors.Length != num2)
		{
			colors = new Color[num2];
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = Color.black;
			}
		}
		if (colorsFlowMap.Count != num2)
		{
			colorsFlowMap.Clear();
		}
		length = 0f;
		fulllength = 0f;
		if (beginningSpline != null)
		{
			length = beginningSpline.length;
		}
		minMaxWidth = 1f;
		uvWidth = 1f;
		uvBeginning = 0f;
		if (beginningSpline != null)
		{
			minMaxWidth = beginningMaxWidth - beginningMinWidth;
			uvWidth = minMaxWidth * beginningSpline.uvWidth;
			uvBeginning = beginningSpline.uvWidth * beginningMinWidth + beginningSpline.uvBeginning;
		}
		else if (endingSpline != null)
		{
			minMaxWidth = endingMaxWidth - endingMinWidth;
			uvWidth = minMaxWidth * endingSpline.uvWidth;
			uvBeginning = endingSpline.uvWidth * endingMinWidth + endingSpline.uvBeginning;
		}
		for (int j = 0; j < pointsDown.Count; j++)
		{
			float num3 = widths[j];
			if (j > 0)
			{
				fulllength += uvWidth * Vector3.Distance(pointsDown[j], pointsDown[j - 1]) / (uvScale * num3);
			}
		}
		float num4 = Mathf.Round(fulllength);
		for (int k = 0; k < pointsDown.Count; k++)
		{
			float num5 = widths[k];
			int num6 = k * vertsInShape;
			if (k > 0)
			{
				length += uvWidth * Vector3.Distance(pointsDown[k], pointsDown[k - 1]) / (uvScale * num5) / fulllength * num4;
			}
			float num7 = 0f;
			float num8 = 0f;
			for (int l = 0; l < vertsInShape; l++)
			{
				int num9 = num6 + l;
				float num10 = (float)l / (float)(vertsInShape - 1);
				num10 = ((!(num10 < 0.5f)) ? (((num10 - 0.5f) * (1f - maxVal) + 0.5f * maxVal) * 2f) : (num10 * (minVal * 2f)));
				if (k == 0 && beginningSpline != null && beginningSpline.verticesEnding != null && beginningSpline.normalsEnding != null)
				{
					int num11 = (int)((float)beginningSpline.vertsInShape * beginningMinWidth);
					array[num9] = beginningSpline.verticesEnding[Mathf.Clamp(l + num11, 0, beginningSpline.verticesEnding.Count - 1)] + beginningSpline.transform.position - base.transform.position;
				}
				else if (k == pointsDown.Count - 1 && endingSpline != null && endingSpline.verticesBeginning != null && endingSpline.normalsBeginning != null)
				{
					int num12 = (int)((float)endingSpline.vertsInShape * endingMinWidth);
					array[num9] = endingSpline.verticesBeginning[Mathf.Clamp(l + num12, 0, endingSpline.verticesBeginning.Count - 1)] + endingSpline.transform.position - base.transform.position;
				}
				else
				{
					array[num9] = Vector3.Lerp(pointsDown[k], pointsUp[k], num10);
					if (Physics.Raycast(array[num9] + base.transform.position + Vector3.up * 5f, Vector3.down, out var hitInfo, 1000f, snapMask.value))
					{
						array[num9] = Vector3.Lerp(array[num9], hitInfo.point - base.transform.position + new Vector3(0f, 0.1f, 0f), (Mathf.Sin((float)Math.PI * snaps[k] - (float)Math.PI / 2f) + 1f) * 0.5f);
					}
					if (normalFromRaycast && Physics.Raycast(points[k] + base.transform.position + Vector3.up * 5f, Vector3.down, out var hitInfo2, 1000f, snapMask.value))
					{
						array2[num9] = hitInfo2.normal;
					}
					array[num9].y += Mathf.Lerp(controlPointsMeshCurves[Mathf.FloorToInt(lerpValues[k])].Evaluate(num10), controlPointsMeshCurves[Mathf.CeilToInt(lerpValues[k])].Evaluate(num10), lerpValues[k] - Mathf.Floor(lerpValues[k]));
				}
				if (k > 0 && k < 5 && beginningSpline != null && beginningSpline.verticesEnding != null)
				{
					array[num9].y = (array[num9].y + array[num9 - vertsInShape].y) * 0.5f;
				}
				if (k == pointsDown.Count - 1 && endingSpline != null && endingSpline.verticesBeginning != null)
				{
					for (int m = 1; m < 5; m++)
					{
						array[num9 - vertsInShape * m].y = (array[num9 - vertsInShape * (m - 1)].y + array[num9 - vertsInShape * m].y) * 0.5f;
					}
				}
				if (k == 0)
				{
					verticesBeginning.Add(array[num9]);
				}
				if (k == pointsDown.Count - 1)
				{
					verticesEnding.Add(array[num9]);
				}
				if (!normalFromRaycast)
				{
					array2[num9] = orientations[k] * Vector3.up;
				}
				if (k == 0)
				{
					normalsBeginning.Add(array2[num9]);
				}
				if (k == pointsDown.Count - 1)
				{
					normalsEnding.Add(array2[num9]);
				}
				if (l > 0)
				{
					num7 = num10 * uvWidth;
					num8 = num10;
				}
				if (beginningSpline != null || endingSpline != null)
				{
					num7 += uvBeginning;
				}
				num7 /= uvScale;
				float num13 = FlowCalculate(num8, array2[num9].y);
				int num14 = 10;
				if (beginnigChildSplines.Count > 0 && k <= num14)
				{
					float num15 = 0f;
					foreach (RamSpline beginnigChildSpline in beginnigChildSplines)
					{
						if (Mathf.CeilToInt(beginnigChildSpline.endingMaxWidth * (float)(vertsInShape - 1)) >= l && l >= Mathf.CeilToInt(beginnigChildSpline.endingMinWidth * (float)(vertsInShape - 1)))
						{
							num15 = (float)(l - Mathf.CeilToInt(beginnigChildSpline.endingMinWidth * (float)(vertsInShape - 1))) / (float)(Mathf.CeilToInt(beginnigChildSpline.endingMaxWidth * (float)(vertsInShape - 1)) - Mathf.CeilToInt(beginnigChildSpline.endingMinWidth * (float)(vertsInShape - 1)));
							num15 = FlowCalculate(num15, array2[num9].y);
						}
					}
					num13 = ((k <= 0) ? num15 : Mathf.Lerp(num13, num15, 1f - (float)k / (float)num14));
				}
				if (k >= pointsDown.Count - num14 - 1 && endingChildSplines.Count > 0)
				{
					float num16 = 0f;
					foreach (RamSpline endingChildSpline in endingChildSplines)
					{
						if (Mathf.CeilToInt(endingChildSpline.beginningMaxWidth * (float)(vertsInShape - 1)) >= l && l >= Mathf.CeilToInt(endingChildSpline.beginningMinWidth * (float)(vertsInShape - 1)))
						{
							num16 = (float)(l - Mathf.CeilToInt(endingChildSpline.beginningMinWidth * (float)(vertsInShape - 1))) / (float)(Mathf.CeilToInt(endingChildSpline.beginningMaxWidth * (float)(vertsInShape - 1)) - Mathf.CeilToInt(endingChildSpline.beginningMinWidth * (float)(vertsInShape - 1)));
							num16 = FlowCalculate(num16, array2[num9].y);
						}
					}
					num13 = ((k >= pointsDown.Count - 1) ? num16 : Mathf.Lerp(num13, num16, (float)(k - (pointsDown.Count - num14 - 1)) / (float)num14));
				}
				float num17 = (0f - (num8 - 0.5f)) * 0.01f;
				if (uvRotation)
				{
					if (!invertUVDirection)
					{
						array3[num9] = new Vector2(1f - length, num7);
						array4[num9] = new Vector2(1f - length / fulllength, num8);
						array5[num9] = new Vector2(num13, num17);
					}
					else
					{
						array3[num9] = new Vector2(1f + length, num7);
						array4[num9] = new Vector2(1f + length / fulllength, num8);
						array5[num9] = new Vector2(num13, num17);
					}
				}
				else if (!invertUVDirection)
				{
					array3[num9] = new Vector2(num7, 1f - length);
					array4[num9] = new Vector2(num8, 1f - length / fulllength);
					array5[num9] = new Vector2(num17, num13);
				}
				else
				{
					array3[num9] = new Vector2(num7, 1f + length);
					array4[num9] = new Vector2(num8, 1f + length / fulllength);
					array5[num9] = new Vector2(num17, num13);
				}
				if (colorsFlowMap.Count <= num9)
				{
					colorsFlowMap.Add(array5[num9]);
				}
				else if (!overrideFlowMap)
				{
					colorsFlowMap[num9] = array5[num9];
				}
			}
		}
		for (int n = 0; n < num; n++)
		{
			int num18 = n * vertsInShape;
			for (int num19 = 0; num19 < vertsInShape - 1; num19++)
			{
				int item = num18 + num19;
				int item2 = num18 + num19 + vertsInShape;
				int item3 = num18 + num19 + 1 + vertsInShape;
				int item4 = num18 + num19 + 1;
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				list.Add(item3);
				list.Add(item4);
				list.Add(item);
			}
		}
		mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.uv = array3;
		mesh.uv3 = array4;
		mesh.uv4 = colorsFlowMap.ToArray();
		mesh.triangles = list.ToArray();
		mesh.colors = colors;
		mesh.RecalculateTangents();
		meshfilter.mesh = mesh;
		GetComponent<MeshRenderer>().enabled = true;
		if (generateMeshParts)
		{
			GenerateMeshParts(mesh);
		}
	}

	public void GenerateMeshParts(Mesh baseMesh)
	{
		foreach (Transform meshesPartTransform in meshesPartTransforms)
		{
			if (meshesPartTransform != null)
			{
				UnityEngine.Object.DestroyImmediate(meshesPartTransform.gameObject);
			}
		}
		Vector3[] vertices = baseMesh.vertices;
		Vector3[] normals = baseMesh.normals;
		Vector2[] uv = baseMesh.uv;
		Vector2[] uv2 = baseMesh.uv3;
		GetComponent<MeshRenderer>().enabled = false;
		int num = Mathf.RoundToInt((float)(vertices.Length / vertsInShape) / (float)meshPartsCount) * vertsInShape;
		for (int i = 0; i < meshPartsCount; i++)
		{
			GameObject gameObject = new GameObject(base.gameObject.name + "- Mesh part " + i);
			gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			meshesPartTransforms.Add(gameObject.transform);
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			Mesh mesh = new Mesh();
			mesh.Clear();
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector2> list3 = new List<Vector2>();
			List<Vector2> list4 = new List<Vector2>();
			List<Vector2> list5 = new List<Vector2>();
			List<Color> list6 = new List<Color>();
			List<int> list7 = new List<int>();
			for (int j = num * i + ((i > 0) ? (-vertsInShape) : 0); (j < num * (i + 1) && j < vertices.Length) || (i == meshPartsCount - 1 && j < vertices.Length); j++)
			{
				list.Add(vertices[j]);
				list2.Add(normals[j]);
				list3.Add(uv[j]);
				list4.Add(uv2[j]);
				list5.Add(colorsFlowMap[j]);
				list6.Add(colors[j]);
			}
			if (list.Count <= 0)
			{
				continue;
			}
			Vector3 vector = list[0];
			for (int k = 0; k < list.Count; k++)
			{
				list[k] -= vector;
			}
			for (int l = 0; l < list.Count / vertsInShape - 1; l++)
			{
				int num2 = l * vertsInShape;
				for (int m = 0; m < vertsInShape - 1; m++)
				{
					int item = num2 + m;
					int item2 = num2 + m + vertsInShape;
					int item3 = num2 + m + 1 + vertsInShape;
					int item4 = num2 + m + 1;
					list7.Add(item);
					list7.Add(item2);
					list7.Add(item3);
					list7.Add(item3);
					list7.Add(item4);
					list7.Add(item);
				}
			}
			gameObject.transform.position += vector;
			mesh.vertices = list.ToArray();
			mesh.triangles = list7.ToArray();
			mesh.normals = list2.ToArray();
			mesh.uv = list3.ToArray();
			mesh.uv3 = list4.ToArray();
			mesh.uv4 = list5.ToArray();
			mesh.colors = list6.ToArray();
			mesh.RecalculateTangents();
			meshFilter.mesh = mesh;
		}
	}

	private float FlowCalculate(float u, float normalY)
	{
		return Mathf.Lerp(flowWaterfall.Evaluate(u), flowFlat.Evaluate(u), Mathf.Clamp(normalY, 0f, 1f));
	}
}
