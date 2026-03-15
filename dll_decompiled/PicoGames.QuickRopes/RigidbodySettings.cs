using System;
using UnityEngine;

namespace PicoGames.QuickRopes;

[Serializable]
public struct RigidbodySettings
{
	[Min(0.001f)]
	public float mass;

	[Min(0f)]
	public float drag;

	[Min(0f)]
	public float angularDrag;

	public bool useGravity;

	public bool isKinematic;

	public RigidbodyInterpolation interpolate;

	public CollisionDetectionMode collisionDetection;

	[SerializeField]
	public RigidbodyConstraints constraints;

	[Range(6f, 100f)]
	public int solverCount;
}
