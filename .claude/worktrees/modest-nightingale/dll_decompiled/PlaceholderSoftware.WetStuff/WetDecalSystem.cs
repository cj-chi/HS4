using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PlaceholderSoftware.WetStuff.Datastructures;
using PlaceholderSoftware.WetStuff.Rendering;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

internal class WetDecalSystem : RcSharedState<WetDecalSystem.State>
{
	public struct DecalRenderHandle : IDisposable
	{
		private readonly uint _id;

		private readonly DecalRenderInstance _instance;

		public bool IsValid
		{
			get
			{
				if (_instance != null)
				{
					return _id == _instance.Epoch;
				}
				return false;
			}
		}

		internal DecalRenderHandle([NotNull] DecalRenderInstance instance)
		{
			_instance = instance;
			_id = instance.Epoch;
		}

		public void UpdateProperties(bool batchPropertiesChanged)
		{
			if (IsValid)
			{
				_instance.UpdateProperties(batchPropertiesChanged);
				return;
			}
			throw new ObjectDisposedException("This handle has already been disposed");
		}

		public void Dispose()
		{
			if (IsValid)
			{
				_instance.Dispose();
				return;
			}
			throw new ObjectDisposedException("This handle has already been disposed");
		}
	}

	internal sealed class DecalRenderInstance : IDisposable
	{
		private static readonly Pool<DecalRenderInstance> Pool = new Pool<DecalRenderInstance>(256, () => new DecalRenderInstance());

		private MaterialBatch _batch;

		private InstanceProperties _properties;

		private WetDecalSystem _system;

		internal uint Epoch { get; private set; }

		public IWetDecal Decal { get; private set; }

		public MaterialPropertyBlock PropertyBlock { get; private set; }

		public InstanceProperties Properties => _properties;

		public void Dispose()
		{
			if (_batch != null)
			{
				_batch.Remove(this);
				_batch = null;
			}
			Epoch++;
			if (Epoch != uint.MaxValue)
			{
				Pool.Put(this);
			}
		}

		[NotNull]
		internal static DecalRenderInstance Create([NotNull] IWetDecal decal, [NotNull] WetDecalSystem system)
		{
			DecalRenderInstance decalRenderInstance = Pool.Get();
			decalRenderInstance._system = system;
			decalRenderInstance.Initialize(decal);
			return decalRenderInstance;
		}

		private void Initialize(IWetDecal decal)
		{
			Decal = decal;
			if (PropertyBlock != null)
			{
				PropertyBlock.Clear();
			}
			UpdateProperties(batchPropertiesChanged: true);
		}

		private static Vector2? Jitter(float amount, [CanBeNull] Texture texture)
		{
			if (texture == null)
			{
				return null;
			}
			return new Vector2(amount / (float)texture.width, amount / (float)texture.height);
		}

		private static Vector2 Min(Vector2? a, Vector2? b, Vector2? c)
		{
			Vector2 vector = a ?? b ?? c ?? Vector2.zero;
			if (a.HasValue)
			{
				vector = Vector2.Min(vector, a.Value);
			}
			if (b.HasValue)
			{
				vector = Vector2.Min(vector, b.Value);
			}
			if (c.HasValue)
			{
				vector = Vector2.Min(vector, c.Value);
			}
			return vector;
		}

		public void UpdateProperties(bool batchPropertiesChanged)
		{
			IDecalSettings settings = Decal.Settings;
			if (batchPropertiesChanged)
			{
				MaterialPermutation permutation = new MaterialPermutation(settings.Mode, settings.LayerMode, settings.LayerProjection, settings.Shape, settings.EnableJitter);
				Vector2 sampleJitter = Min((settings.LayerMode == LayerMode.Triplanar) ? Jitter(settings.SampleJitter, settings.XLayer.LayerMask) : ((Vector2?)null), (settings.LayerMode != LayerMode.None) ? Jitter(settings.SampleJitter, settings.YLayer.LayerMask) : ((Vector2?)null), (settings.LayerMode == LayerMode.Triplanar) ? Jitter(settings.SampleJitter, settings.ZLayer.LayerMask) : ((Vector2?)null));
				MaterialProperties properties = new MaterialProperties(settings.XLayer.LayerMask, settings.XLayer.LayerMaskScaleOffset, settings.YLayer.LayerMask, settings.YLayer.LayerMaskScaleOffset, settings.ZLayer.LayerMask, settings.ZLayer.LayerMaskScaleOffset, _system.SharedState.BlueNoiseRGBA, new Vector2(29f, 31f), sampleJitter);
				if (_batch != null)
				{
					_batch.Remove(this);
				}
				_batch = _system.SharedState.FindBatch(permutation, properties);
				_batch.Add(this);
			}
			_properties.Saturation = settings.Saturation;
			_properties.Fadeout = settings.EdgeFadeoff;
			_properties.EdgeSharpness = settings.FaceSharpness;
			if (settings.LayerMode != LayerMode.None)
			{
				settings.YLayer.EvaluateRanges(out _properties.YLayer);
			}
			if (settings.LayerMode == LayerMode.Triplanar)
			{
				settings.XLayer.EvaluateRanges(out _properties.XLayer);
				settings.ZLayer.EvaluateRanges(out _properties.ZLayer);
			}
			if (!PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableInstancing)
			{
				if (PropertyBlock == null)
				{
					PropertyBlock = new MaterialPropertyBlock();
				}
				PropertyBlock.Clear();
				_properties.LoadInto(PropertyBlock, settings.LayerMode);
			}
		}
	}

	private struct MaterialBatchId : IEquatable<MaterialBatchId>
	{
		private readonly MaterialPermutation _permutation;

		private readonly MaterialProperties _properties;

		public MaterialBatchId(MaterialPermutation permutation, MaterialProperties properties)
		{
			_permutation = permutation;
			_properties = properties;
		}

		public bool Equals(MaterialBatchId other)
		{
			if (_permutation.Equals(other._permutation))
			{
				return _properties.Equals(other._properties);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is MaterialBatchId)
			{
				return Equals((MaterialBatchId)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (_permutation.GetHashCode() * 397) ^ _properties.GetHashCode();
		}

		public static bool operator ==(MaterialBatchId left, MaterialBatchId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MaterialBatchId left, MaterialBatchId right)
		{
			return !left.Equals(right);
		}
	}

	internal class MaterialBatch : IDisposable
	{
		private readonly List<DecalRenderInstance> _decals;

		private readonly Material _material;

		public BoundingSphere[] BoundingSpheres { get; private set; }

		public ReadOnlyCollection<DecalRenderInstance> Decals { get; private set; }

		public MaterialProperties Properties { get; private set; }

		public MaterialPermutation Permutation { get; private set; }

		public MaterialBatch(Shader shader, MaterialPermutation permutation, MaterialProperties properties)
		{
			Properties = properties;
			Permutation = permutation;
			_decals = new List<DecalRenderInstance>();
			BoundingSpheres = new BoundingSphere[16];
			Decals = new ReadOnlyCollection<DecalRenderInstance>(_decals);
			Material material = new Material(shader)
			{
				hideFlags = HideFlags.DontSave
			};
			if (permutation.LayerMode == LayerMode.Single)
			{
				material.EnableKeyword("LAYERS_SINGLE");
			}
			if (permutation.LayerMode == LayerMode.Triplanar)
			{
				material.EnableKeyword("LAYERS_TRIPLANAR");
			}
			if (permutation.LayerProjectionMode == ProjectionMode.Local)
			{
				material.EnableKeyword("LAYER_PROJECTION_LOCAL");
			}
			else
			{
				material.EnableKeyword("LAYER_PROJECTION_WORLD");
			}
			if (permutation.EnableJitter)
			{
				material.EnableKeyword("JITTER_LAYERS");
			}
			if (permutation.Shape == DecalShape.Sphere)
			{
				material.EnableKeyword("SHAPE_CIRCLE");
			}
			else
			{
				material.EnableKeyword("SHAPE_SQUARE");
			}
			properties.LoadInto(material);
			_material = material;
		}

		public void Dispose()
		{
			for (int num = _decals.Count - 1; num >= 0; num--)
			{
				_decals[num].Dispose();
			}
			UnityEngine.Object.DestroyImmediate(_material);
		}

		[NotNull]
		public Material GetMaterial(bool instanced)
		{
			if (_material.enableInstancing != instanced)
			{
				_material.enableInstancing = instanced;
			}
			return _material;
		}

		internal void Add(DecalRenderInstance decal)
		{
			_decals.Add(decal);
		}

		internal void Remove(DecalRenderInstance decal)
		{
			_decals.Remove(decal);
		}

		public void Update()
		{
			if (BoundingSpheres.Length < _decals.Count)
			{
				int num;
				for (num = BoundingSpheres.Length; num < _decals.Count; num *= 2)
				{
				}
				BoundingSpheres = new BoundingSphere[num];
			}
			for (int i = 0; i < _decals.Count; i++)
			{
				BoundingSpheres[i] = _decals[i].Decal.Bounds;
			}
		}
	}

	internal class State : IDisposable
	{
		private readonly Dictionary<MaterialBatchId, MaterialBatch> _batches;

		private readonly List<MaterialBatch> _batchesList;

		private readonly Shader _dryMaskShader;

		private readonly Shader _wetMaskShader;

		public uint BatchEpoch { get; private set; }

		public Texture2D BlueNoiseRGBA { get; private set; }

		public ReadOnlyCollection<MaterialBatch> Batches { get; private set; }

		public int LastUpdated { get; set; }

		public List<IWetDecal> ToUpdate { get; set; }

		public List<IWetDecal> Updating { get; set; }

		public State()
		{
			_batches = new Dictionary<MaterialBatchId, MaterialBatch>();
			_batchesList = new List<MaterialBatch>();
			_wetMaskShader = Shader.Find("WetStuff/WetSurfaceMask");
			_dryMaskShader = Shader.Find("WetStuff/DrySurfaceMask");
			BlueNoiseRGBA = (Texture2D)Resources.Load("FreeBlueNoiseTextures/128_128/LDR_RGBA_2");
			Batches = new ReadOnlyCollection<MaterialBatch>(_batchesList);
			LastUpdated = 0;
			ToUpdate = new List<IWetDecal>();
			Updating = new List<IWetDecal>();
		}

		public void Dispose()
		{
			foreach (MaterialBatch batches in _batchesList)
			{
				batches.Dispose();
			}
			_batches.Clear();
			_batchesList.Clear();
		}

		public MaterialBatch FindBatch(MaterialPermutation permutation, MaterialProperties properties)
		{
			MaterialBatchId key = new MaterialBatchId(permutation, properties);
			if (!_batches.TryGetValue(key, out var value))
			{
				value = new MaterialBatch(permutation.SelectShader(_wetMaskShader, _dryMaskShader), permutation, properties);
				_batches[key] = value;
				_batchesList.Add(value);
				BatchEpoch++;
			}
			return value;
		}
	}

	internal ReadOnlyCollection<MaterialBatch> Batches => base.SharedState.Batches;

	internal uint BatchEpoch => base.SharedState.BatchEpoch;

	public WetDecalSystem()
		: base((Func<State>)(() => new State()))
	{
	}

	public DecalRenderHandle Add([NotNull] IWetDecal decal)
	{
		return new DecalRenderHandle(DecalRenderInstance.Create(decal, this));
	}

	public void QueueForUpdate(IWetDecal decal)
	{
		base.SharedState.ToUpdate.Add(decal);
	}

	public void Update()
	{
		State sharedState = base.SharedState;
		if (sharedState.LastUpdated != Time.frameCount)
		{
			List<IWetDecal> updating = sharedState.Updating;
			sharedState.Updating = sharedState.ToUpdate;
			sharedState.ToUpdate = updating;
			float dt = (Application.isPlaying ? Time.deltaTime : 0f);
			for (int i = 0; i < sharedState.Updating.Count; i++)
			{
				sharedState.Updating[i].Step(dt);
			}
			sharedState.Updating.Clear();
			for (int j = 0; j < sharedState.Batches.Count; j++)
			{
				sharedState.Batches[j].Update();
			}
			sharedState.LastUpdated = Time.frameCount;
		}
	}
}
