using System;
using System.Collections.Generic;
using PlaceholderSoftware.WetStuff.Debugging;
using PlaceholderSoftware.WetStuff.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff;

internal class WetDecalRenderer : IDisposable
{
	public class DecalBatch : IDisposable, IComparable<DecalBatch>
	{
		private const int MaxInstancesPerBatch = 1023;

		private readonly Mesh _box;

		private readonly BatchCulling _culling;

		private readonly MaterialPropertyBlock _instancingPropertyBlock;

		private float[] _edgeFadeout;

		private float[] _edgeSharpness;

		private Matrix4x4[] _matrices;

		private float[] _saturation;

		private Vector4[] _xLayerInputExtent;

		private Vector4[] _xLayerInputStart;

		private Vector4[] _xLayerOutputEnd;

		private Vector4[] _xLayerOutputStart;

		private Vector4[] _yLayerInputExtent;

		private Vector4[] _yLayerInputStart;

		private Vector4[] _yLayerOutputEnd;

		private Vector4[] _yLayerOutputStart;

		private Vector4[] _zLayerInputExtent;

		private Vector4[] _zLayerInputStart;

		private Vector4[] _zLayerOutputEnd;

		private Vector4[] _zLayerOutputStart;

		public WetDecalSystem.MaterialBatch Batch { get; private set; }

		public DecalBatch([NotNull] Camera camera, [NotNull] Mesh box, [NotNull] WetDecalSystem.MaterialBatch batch)
		{
			Batch = batch;
			_box = box;
			_culling = new BatchCulling(camera, batch);
			_instancingPropertyBlock = new MaterialPropertyBlock();
			_matrices = new Matrix4x4[16];
		}

		public int CompareTo(DecalBatch other)
		{
			return Batch.Permutation.RenderOrder.CompareTo(other.Batch.Permutation.RenderOrder);
		}

		public void Dispose()
		{
			if (_culling != null)
			{
				_culling.Dispose();
			}
		}

		public void Update()
		{
			_culling.Update();
		}

		public void PrepareDraw(CommandBuffer cmd)
		{
			Draw(cmd, 0, (!PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableStencil) ? 1 : 3);
			Draw(cmd, 1, PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableStencil ? 2 : 0);
		}

		private int Draw(CommandBuffer cmd, int distanceBand, int shaderPass)
		{
			ArraySegment<int> arraySegment = _culling.Cull(distanceBand);
			if (PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableInstancing)
			{
				for (int i = 0; i < arraySegment.Count; i += 1023)
				{
					int count = Math.Min(1023, arraySegment.Count - i);
					ArraySegment<int> visible = new ArraySegment<int>(arraySegment.Array, arraySegment.Offset + i, count);
					DrawInstanced(cmd, visible, shaderPass);
				}
			}
			else
			{
				for (int j = 0; j < arraySegment.Count; j++)
				{
					WetDecalSystem.DecalRenderInstance decal = Batch.Decals[arraySegment.Array[arraySegment.Offset + j]];
					DrawSingle(cmd, decal, shaderPass);
				}
			}
			return arraySegment.Count;
		}

		private static void EnsureArrayCapacity<T>([NotNull] ref T[] array, int count, int maxGrowth = 1023)
		{
			if (array == null)
			{
				array = new T[count];
			}
			else if (array.Length < count)
			{
				array = new T[Math.Max(count, Math.Min(array.Length * 2, maxGrowth))];
			}
		}

		private void DrawSingle([NotNull] CommandBuffer cmd, [NotNull] WetDecalSystem.DecalRenderInstance decal, int shaderPass)
		{
			if (decal == null)
			{
				throw new ArgumentNullException("decal");
			}
			cmd.DrawMesh(_box, decal.Decal.WorldTransform, Batch.GetMaterial(instanced: false), 0, shaderPass, decal.PropertyBlock);
		}

		private void DrawInstanced([NotNull] CommandBuffer cmd, ArraySegment<int> visible, int shaderPass)
		{
			if (visible.Count >= 1)
			{
				EnsureArrayCapacity(ref _matrices, visible.Count);
				EnsureArrayCapacity(ref _saturation, visible.Count);
				EnsureArrayCapacity(ref _edgeFadeout, visible.Count);
				EnsureArrayCapacity(ref _edgeSharpness, visible.Count);
				if (Batch.Permutation.LayerMode == LayerMode.None)
				{
					_yLayerInputStart = null;
					_yLayerInputExtent = null;
					_yLayerOutputStart = null;
					_yLayerOutputEnd = null;
				}
				else
				{
					EnsureArrayCapacity(ref _yLayerInputStart, visible.Count);
					EnsureArrayCapacity(ref _yLayerInputExtent, visible.Count);
					EnsureArrayCapacity(ref _yLayerOutputStart, visible.Count);
					EnsureArrayCapacity(ref _yLayerOutputEnd, visible.Count);
				}
				if (Batch.Permutation.LayerMode != LayerMode.Triplanar)
				{
					_xLayerInputStart = null;
					_xLayerInputExtent = null;
					_xLayerOutputStart = null;
					_xLayerOutputEnd = null;
					_zLayerInputStart = null;
					_zLayerInputExtent = null;
					_zLayerOutputStart = null;
					_zLayerOutputEnd = null;
				}
				else
				{
					EnsureArrayCapacity(ref _xLayerInputStart, visible.Count);
					EnsureArrayCapacity(ref _xLayerInputExtent, visible.Count);
					EnsureArrayCapacity(ref _xLayerOutputStart, visible.Count);
					EnsureArrayCapacity(ref _xLayerOutputEnd, visible.Count);
					EnsureArrayCapacity(ref _zLayerInputStart, visible.Count);
					EnsureArrayCapacity(ref _zLayerInputExtent, visible.Count);
					EnsureArrayCapacity(ref _zLayerOutputStart, visible.Count);
					EnsureArrayCapacity(ref _zLayerOutputEnd, visible.Count);
				}
				LayerArrays? x = LayerArrays.Create(_xLayerInputStart, _xLayerInputExtent, _xLayerOutputStart, _xLayerOutputEnd);
				LayerArrays? y = LayerArrays.Create(_yLayerInputStart, _yLayerInputExtent, _yLayerOutputStart, _yLayerOutputEnd);
				LayerArrays? z = LayerArrays.Create(_zLayerInputStart, _zLayerInputExtent, _zLayerOutputStart, _zLayerOutputEnd);
				for (int i = 0; i < visible.Count; i++)
				{
					WetDecalSystem.DecalRenderInstance decalRenderInstance = Batch.Decals[visible.Array[visible.Offset + i]];
					decalRenderInstance.Properties.LoadInto(i, _saturation, _edgeFadeout, _edgeSharpness, x, y, z);
					_matrices[i] = decalRenderInstance.Decal.WorldTransform;
				}
				_instancingPropertyBlock.Clear();
				InstanceProperties.LoadInto(_instancingPropertyBlock, _saturation, _edgeFadeout, _edgeSharpness, x, y, z);
				Material material = Batch.GetMaterial(instanced: true);
				cmd.DrawMeshInstanced(_box, 0, material, shaderPass, _matrices, visible.Count, _instancingPropertyBlock);
			}
		}
	}

	private static readonly int MaskId = Shader.PropertyToID("_WetDecalSaturationMask");

	private static readonly Log Log = Logs.Create(LogCategory.Graphics, typeof(WetDecalRenderer).Name);

	private readonly List<DecalBatch> _batches;

	private readonly Mesh _box;

	private readonly Camera _camera;

	private readonly Dictionary<WetDecalSystem.MaterialBatch, DecalBatch> _registeredBatches;

	private readonly WetDecalSystem _system;

	private uint? _batchEpoch;

	public Camera Camera => _camera;

	public WetDecalRenderer([NotNull] Camera camera)
	{
		Log.AssertAndThrowPossibleBug(camera, "178CD4B2-1FD8-4BF2-B14D-EC9CA50436CA", "camera is null");
		_camera = camera;
		_system = new WetDecalSystem();
		_batches = new List<DecalBatch>();
		_registeredBatches = new Dictionary<WetDecalSystem.MaterialBatch, DecalBatch>();
		_box = Primitives.CreateBox(1f, 1f, 1f);
		_box.hideFlags = HideFlags.DontSave;
	}

	public void Dispose()
	{
		for (int i = 0; i < _batches.Count; i++)
		{
			_batches[i].Dispose();
		}
		_batches.Clear();
		_registeredBatches.Clear();
		if (_box != null)
		{
			UnityEngine.Object.DestroyImmediate(_box);
		}
		_system.Dispose();
	}

	public void Update()
	{
		_system.Update();
		if (_batchEpoch != _system.BatchEpoch)
		{
			for (int i = 0; i < _system.Batches.Count; i++)
			{
				WetDecalSystem.MaterialBatch materialBatch = _system.Batches[i];
				if (!_registeredBatches.ContainsKey(materialBatch))
				{
					DecalBatch decalBatch = new DecalBatch(_camera, _box, materialBatch);
					_batches.Add(decalBatch);
					_registeredBatches[materialBatch] = decalBatch;
				}
			}
			_batches.Sort();
			_batchEpoch = _system.BatchEpoch;
		}
		for (int j = 0; j < _batches.Count; j++)
		{
			_batches[j].Update();
		}
	}

	public void RecordCommandBuffer([NotNull] CommandBuffer cmd)
	{
		if (XRSettings.enabled && Camera.stereoEnabled)
		{
			RenderTextureDescriptor eyeTextureDesc = XRSettings.eyeTextureDesc;
			eyeTextureDesc.colorFormat = RenderTextureFormat.RFloat;
			eyeTextureDesc.sRGB = false;
			cmd.GetTemporaryRT(MaskId, eyeTextureDesc, FilterMode.Point);
		}
		else
		{
			cmd.GetTemporaryRT(MaskId, -1, -1, 24, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear, 1);
		}
		cmd.SetRenderTarget(MaskId, BuiltinRenderTextureType.CameraTarget);
		cmd.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
		for (int i = 0; i < _batches.Count; i++)
		{
			_batches[i].PrepareDraw(cmd);
		}
	}
}
