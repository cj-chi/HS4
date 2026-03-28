using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

public interface IWetDecal
{
	Matrix4x4 WorldTransform { get; }

	BoundingSphere Bounds { get; }

	IDecalSettings Settings { get; }

	void Step(float dt);
}
