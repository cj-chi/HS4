using System;
using PlaceholderSoftware.WetStuff.Debugging;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

internal class BatchCulling : IDisposable
{
	private static readonly Log Log = Logs.Create(LogCategory.Graphics, typeof(BatchCulling).Name);

	private readonly WetDecalSystem.MaterialBatch _batch;

	private readonly Camera _camera;

	private readonly CullingGroup _culling;

	private readonly float[] _distanceThresholds;

	private int[] _visibleIndices;

	public BatchCulling([NotNull] Camera camera, [NotNull] WetDecalSystem.MaterialBatch batch)
	{
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}
		if (batch == null)
		{
			throw new ArgumentNullException("batch");
		}
		_camera = camera;
		_batch = batch;
		_visibleIndices = new int[8];
		_distanceThresholds = new float[2] { camera.nearClipPlane, camera.farClipPlane };
		_culling = new CullingGroup
		{
			targetCamera = camera
		};
		_culling.SetBoundingDistances(_distanceThresholds);
		_culling.SetDistanceReferencePoint(camera.transform);
		_culling.SetBoundingSpheres(batch.BoundingSpheres);
		_culling.SetBoundingSphereCount(batch.Decals.Count);
	}

	public void Dispose()
	{
		_culling.Dispose();
	}

	public void Update()
	{
		_culling.SetBoundingSpheres(_batch.BoundingSpheres);
		_culling.SetBoundingSphereCount(_batch.Decals.Count);
		_distanceThresholds[0] = _camera.nearClipPlane;
		_distanceThresholds[1] = _camera.farClipPlane;
	}

	private static T[] EnsureArrayCapacity<T>([NotNull] T[] array, int count)
	{
		if (array.Length < count)
		{
			int newSize = Math.Max(count * 2, array.Length * 2);
			Array.Resize(ref array, newSize);
		}
		return array;
	}

	public ArraySegment<int> Cull(int distanceBand)
	{
		_visibleIndices = EnsureArrayCapacity(_visibleIndices, _batch.Decals.Count);
		int count = _culling.QueryIndices(visible: true, distanceBand, _visibleIndices, 0);
		return new ArraySegment<int>(_visibleIndices, 0, count);
	}
}
