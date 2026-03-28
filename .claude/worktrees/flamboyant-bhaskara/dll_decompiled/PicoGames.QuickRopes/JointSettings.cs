using System;
using UnityEngine;

namespace PicoGames.QuickRopes;

[Serializable]
public struct JointSettings
{
	[SerializeField]
	[Min(0.001f)]
	public float breakForce;

	[SerializeField]
	[Min(0.001f)]
	public float breakTorque;

	[SerializeField]
	[Range(0f, 180f)]
	public float twistLimit;

	[SerializeField]
	[Range(0f, 180f)]
	public float swingLimit;

	[SerializeField]
	[Min(0f)]
	public float spring;

	[SerializeField]
	[Min(0f)]
	public float damper;
}
