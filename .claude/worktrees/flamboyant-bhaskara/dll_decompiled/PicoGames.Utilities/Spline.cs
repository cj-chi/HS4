using System;
using System.Collections.Generic;
using UnityEngine;

namespace PicoGames.Utilities;

[Serializable]
[AddComponentMenu("PicoGames/Utilities/Spline")]
public class Spline : MonoBehaviour
{
	public enum ControlPointMode
	{
		Free,
		Aligned,
		Mirrored
	}

	[SerializeField]
	public int outputResolution = 1000;

	[SerializeField]
	[HideInInspector]
	public bool hasChanged;

	[SerializeField]
	[HideInInspector]
	private List<Vector3> points = new List<Vector3>(new Vector3[4]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, -1f, 0f),
		new Vector3(0f, -4f, 0f),
		new Vector3(0f, -5f, 0f)
	});

	[SerializeField]
	[HideInInspector]
	private List<ControlPointMode> modes = new List<ControlPointMode>(new ControlPointMode[2]
	{
		ControlPointMode.Mirrored,
		ControlPointMode.Mirrored
	});

	[SerializeField]
	private bool isLooped;

	[SerializeField]
	private bool evenlyDistributPoints = true;

	[SerializeField]
	[HideInInspector]
	private float curveLength = -1f;

	public float CurveLength
	{
		get
		{
			if (curveLength < 0f)
			{
				UpdateCurveLength();
			}
			return curveLength;
		}
	}

	public int ControlCount => (points.Count - 1) / 3;

	public Vector3[] Points => points.ToArray();

	public bool IsLooped
	{
		get
		{
			return isLooped;
		}
		set
		{
			if (value == isLooped)
			{
				return;
			}
			hasChanged = true;
			isLooped = value;
			if (value)
			{
				if (ControlCount < 2)
				{
					AddCurve(0);
					Vector3 vector = GetPoint(1) - GetPoint(0);
					SetControlPoint(1, GetControlPoint(0) - new Vector3(vector.y, 0f - vector.x, 0f) * 1.5f);
					SetPoint(4, GetPoint(points.Count - 2) - new Vector3(vector.y, 0f - vector.x, 0f) * 1.5f);
				}
				evenlyDistributPoints = true;
				modes[0] = modes[modes.Count - 1];
				SetPoint(points.Count - 1, points[0]);
			}
		}
	}

	public bool EvenPointDistribution
	{
		get
		{
			return evenlyDistributPoints;
		}
		set
		{
			evenlyDistributPoints = isLooped || value;
		}
	}

	public void Reset()
	{
		points = new List<Vector3>(new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, -4f, 0f),
			new Vector3(0f, -5f, 0f)
		});
		modes = new List<ControlPointMode>(new ControlPointMode[2]
		{
			ControlPointMode.Mirrored,
			ControlPointMode.Mirrored
		});
		UpdateCurveLength();
		hasChanged = true;
	}

	public void AddCurve(int _atIndex, ControlPointMode _defaultMode = ControlPointMode.Mirrored)
	{
		Vector3 vector = points[_atIndex * 3];
		Vector3 vector2 = points[(_atIndex + 1) * 3];
		Vector3 vector3 = (vector + vector2) * 0.5f;
		Vector3 normalized = (vector2 - vector).normalized;
		points.InsertRange(_atIndex * 3 + 2, new Vector3[3]
		{
			vector3 - normalized,
			vector3,
			vector3 + normalized
		});
		modes.Insert(_atIndex, _defaultMode);
		EnforceMode(_atIndex);
		if (isLooped)
		{
			points[points.Count - 1] = points[0];
			modes[modes.Count - 1] = modes[0];
			EnforceMode(0);
		}
		hasChanged = true;
	}

	public void RemoveCurve(int _index)
	{
		if (_index != 0 && _index != ControlCount)
		{
			points.RemoveRange(_index * 3 - 1, 3);
			modes.RemoveAt(_index);
			hasChanged = true;
		}
	}

	public ControlPointMode GetMode(int _index)
	{
		return modes[(_index + 1) / 3];
	}

	public void SetMode(int _index, ControlPointMode _mode)
	{
		int num = (_index + 1) / 3;
		modes[num] = _mode;
		if (isLooped)
		{
			if (num == 0)
			{
				modes[modes.Count - 1] = _mode;
			}
			else if (num == modes.Count - 1)
			{
				modes[0] = _mode;
			}
		}
		EnforceMode(_index);
		hasChanged = true;
	}

	private void EnforceMode(int _index)
	{
		int num = (_index + 1) / 3;
		ControlPointMode controlPointMode = modes[num];
		if (controlPointMode == ControlPointMode.Free || (!isLooped && (num == 0 || num == modes.Count - 1)))
		{
			return;
		}
		int num2 = num * 3;
		int num3;
		int num4;
		if (_index <= num2)
		{
			num3 = num2 - 1;
			if (num3 < 0)
			{
				num3 = points.Count - 2;
			}
			num4 = num2 + 1;
			if (num4 >= points.Count - 2)
			{
				num4 = 1;
			}
		}
		else
		{
			num3 = num2 + 1;
			if (num3 >= points.Count)
			{
				num3 = 1;
			}
			num4 = num2 - 1;
			if (num4 < 0)
			{
				num4 = points.Count - 2;
			}
		}
		Vector3 vector = points[num2];
		Vector3 vector2 = vector - points[num3];
		if (controlPointMode == ControlPointMode.Aligned)
		{
			vector2 = vector2.normalized * Vector3.Distance(vector, points[num4]);
		}
		points[num4] = vector + vector2;
	}

	public Vector3 GetControlPoint(int _index, Space _space = Space.Self)
	{
		return GetPoint(_index * 3, _space);
	}

	public void SetControlPoint(int _index, Vector3 _position, Space _space = Space.Self)
	{
		SetPoint(_index * 3, _position, _space);
	}

	public void SetPoint(int _index, Vector3 _position, Space _space = Space.Self)
	{
		if (_space == Space.World)
		{
			_position = base.transform.InverseTransformPoint(_position);
		}
		if (_index % 3 == 0)
		{
			Vector3 vector = _position - points[_index];
			if (isLooped)
			{
				if (_index == 0)
				{
					points[1] += vector;
					points[points.Count - 2] += vector;
					points[points.Count - 1] = _position;
				}
				else if (_index == points.Count - 1)
				{
					points[0] = _position;
					points[1] += vector;
					points[_index - 1] += vector;
				}
				else
				{
					points[_index - 1] += vector;
					points[_index + 1] += vector;
				}
			}
			else
			{
				if (_index > 0)
				{
					points[_index - 1] += vector;
				}
				if (_index + 1 < points.Count)
				{
					points[_index + 1] += vector;
				}
			}
		}
		points[_index] = _position;
		EnforceMode(_index);
		UpdateCurveLength();
		hasChanged = true;
	}

	public Vector3 GetPoint(int _index, Space _space = Space.Self)
	{
		if (_space != Space.World)
		{
			return points[_index];
		}
		return base.transform.TransformPoint(points[_index]);
	}

	public Vector3 GetPointOnCurve(float _t)
	{
		int num;
		if (_t >= 1f)
		{
			_t = 1f;
			num = points.Count - 4;
		}
		else
		{
			_t = Mathf.Clamp01(_t) * (float)((points.Count - 1) / 3);
			num = (int)_t;
			_t -= (float)num;
			num *= 3;
		}
		return GetBezierPoint(points[num], points[num + 1], points[num + 2], points[num + 3], _t);
	}

	public SplinePoint[] GetSpacedPointsReversed(float _spacing)
	{
		List<SplinePoint> list = new List<SplinePoint>();
		Vector3 vector = GetPointOnCurve(1f);
		float num = _spacing * _spacing;
		float num2 = 1f / (float)outputResolution;
		list.Add(new SplinePoint(vector, Quaternion.identity));
		for (float num3 = 1f; num3 >= 0f; num3 -= num2)
		{
			Vector3 pointOnCurve = GetPointOnCurve(num3);
			if (Vector3.SqrMagnitude(pointOnCurve - vector) >= num)
			{
				vector = pointOnCurve;
				list.Add(new SplinePoint(vector, Quaternion.identity));
			}
		}
		if (list.Count <= 1)
		{
			return new SplinePoint[0];
		}
		vector = GetPointOnCurve(0f);
		float num4 = Vector3.Distance(vector, list[list.Count - 1].position) / (float)list.Count;
		for (int num5 = list.Count - 1; num5 >= 0; num5--)
		{
			Vector3 normalized = (vector - list[num5].position).normalized;
			if (evenlyDistributPoints)
			{
				list[num5].position += num4 * (float)num5 * normalized;
			}
			list[num5].rotation = Quaternion.FromToRotation(Vector3.up, normalized);
			vector = list[num5].position;
		}
		return list.ToArray();
	}

	private void UpdateCurveLength(int _resolution = 1000)
	{
		float num = 1f / (float)_resolution;
		Vector3 a = GetPointOnCurve(0f);
		curveLength = 0f;
		for (int i = 1; i <= _resolution; i++)
		{
			Vector3 pointOnCurve = GetPointOnCurve((float)i * num);
			curveLength += Vector3.Distance(a, pointOnCurve);
			a = pointOnCurve;
		}
	}

	public static Vector3 GetBezierPoint(Vector3 _p0, Vector3 _p1, Vector3 _p2, Vector3 _p3, float _t)
	{
		_t = Mathf.Clamp01(_t);
		float num = 1f - _t;
		return num * num * num * _p0 + 3f * num * num * _t * _p1 + 3f * num * _t * _t * _p2 + _t * _t * _t * _p3;
	}
}
