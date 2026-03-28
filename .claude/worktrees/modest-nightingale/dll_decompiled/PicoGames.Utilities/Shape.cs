using System;
using UnityEngine;

namespace PicoGames.Utilities;

public class Shape
{
	private const float SQR_TWO = 1.4142135f;

	public static Vector3[] GetSquare(float _centerScale = 2f)
	{
		return GetPolygon(4, _centerScale);
	}

	public static Vector3[] GetPentagon(float _centerScale = 2f)
	{
		return GetPolygon(5, _centerScale);
	}

	public static Vector3[] GetHexagon(float _centerScale = 2f)
	{
		return GetPolygon(6, _centerScale);
	}

	public static Vector3[] GetHeptagon(float _centerScale = 2f)
	{
		return GetPolygon(7, _centerScale);
	}

	public static Vector3[] GetOctagon(float _centerScale = 2f)
	{
		return GetPolygon(8, _centerScale);
	}

	public static Vector3[] GetNonagon(float _centerScale = 2f)
	{
		return GetPolygon(9, _centerScale);
	}

	public static Vector3[] GetDecagon(float _centerScale = 2f)
	{
		return GetPolygon(10, _centerScale);
	}

	public static Vector3[] GetDodecagon(float _centerScale = 2f)
	{
		return GetPolygon(12, _centerScale);
	}

	public static Vector3[] GetPolygon(int _sides, float _centerScale = 2f)
	{
		return GetRoseCurve(_sides, 1, _centerScale, _unitize: true);
	}

	public static Vector3[] GetStar(float _centerScale = 2f)
	{
		return GetRoseCurve(5, 2, _centerScale, _unitize: true);
	}

	public static Vector3[] GetRoseCurve(int _points, int _detail, float _centerScale, bool _unitize)
	{
		_points = Mathf.Max(3, _points);
		_detail = Mathf.Max(1, _detail);
		Vector3[] _points2 = new Vector3[_points * _detail];
		int num = _points;
		Vector3 vector = Vector3.one * float.MaxValue;
		Vector3 vector2 = Vector3.one * float.MinValue;
		for (int i = 0; i < _points2.Length; i++)
		{
			float num2 = (float)i * ((float)Math.PI * 2f / (float)_points2.Length);
			float num3 = Mathf.Cos(num2 * (float)num) + _centerScale;
			_points2[i] = new Vector3(num3 * Mathf.Cos(num2), num3 * Mathf.Sin(num2), 0f);
			vector = Vector3.Min(vector, _points2[i]);
			vector2 = Vector3.Max(vector2, _points2[i]);
		}
		if (_unitize)
		{
			Unitize(ref _points2, vector, vector2);
		}
		return _points2;
	}

	public static void Unitize(ref Vector3[] _points, Vector3 _min, Vector3 _max)
	{
		float num = Vector3.Distance(_min, _max) / 1.4142135f;
		for (int i = 0; i < _points.Length; i++)
		{
			_points[i] /= num;
		}
	}
}
