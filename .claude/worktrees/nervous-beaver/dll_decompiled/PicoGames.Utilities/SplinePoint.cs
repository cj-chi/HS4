using System;
using UnityEngine;

namespace PicoGames.Utilities;

[Serializable]
public class SplinePoint
{
	[SerializeField]
	public Vector3 position;

	[SerializeField]
	public Quaternion rotation;

	public SplinePoint(Vector3 _position, Quaternion _rotation)
	{
		position = _position;
		rotation = _rotation;
	}
}
