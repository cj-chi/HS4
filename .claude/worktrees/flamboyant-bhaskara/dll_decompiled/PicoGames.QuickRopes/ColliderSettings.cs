using System;
using UnityEngine;

namespace PicoGames.QuickRopes;

[Serializable]
public struct ColliderSettings
{
	public enum Direction
	{
		X_Axis,
		Y_Axis,
		Z_Axis
	}

	[SerializeField]
	public QuickRope.ColliderType type;

	[SerializeField]
	public Direction direction;

	[SerializeField]
	public Vector3 center;

	[SerializeField]
	public Vector3 size;

	[SerializeField]
	[Min(0f)]
	public float radius;

	[SerializeField]
	[Min(0f)]
	public float height;

	[SerializeField]
	public PhysicMaterial physicsMaterial;
}
