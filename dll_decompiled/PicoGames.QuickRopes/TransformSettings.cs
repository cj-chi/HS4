using System;
using UnityEngine;

namespace PicoGames.QuickRopes;

[Serializable]
public struct TransformSettings
{
	[SerializeField]
	public Vector3 position;

	[SerializeField]
	public Vector3 eulerRotation;

	[SerializeField]
	public Vector3 scale;

	[SerializeField]
	public Quaternion rotation
	{
		get
		{
			return Quaternion.Euler(eulerRotation);
		}
		set
		{
			eulerRotation = value.eulerAngles;
		}
	}
}
