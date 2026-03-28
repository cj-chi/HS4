using System;
using System.Collections.Generic;
using PlaceholderSoftware.WetStuff.Extensions;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

[RequireComponent(typeof(ParticleSystem))]
[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/ParticleWetSplatter/")]
public class ParticleWetSplatter : MonoBehaviour
{
	private class DecalSettingsSaturationProxy : IDecalSettings
	{
		private readonly IDecalSettings _settings;

		public float SaturationMultiplier { get; set; }

		public WetDecalMode Mode => _settings.Mode;

		public float Saturation => _settings.Saturation * SaturationMultiplier;

		public float SampleJitter => _settings.SampleJitter;

		public bool EnableJitter => _settings.EnableJitter;

		public float FaceSharpness => _settings.FaceSharpness;

		public ProjectionMode LayerProjection => _settings.LayerProjection;

		public LayerMode LayerMode => _settings.LayerMode;

		public DecalLayer XLayer => _settings.XLayer;

		public DecalLayer YLayer => _settings.YLayer;

		public DecalLayer ZLayer => _settings.ZLayer;

		public DecalShape Shape => _settings.Shape;

		public float EdgeFadeoff => _settings.EdgeFadeoff;

		public DecalSettingsSaturationProxy(IDecalSettings settings)
		{
			_settings = settings;
		}
	}

	private class Splatter : IWetDecal
	{
		private readonly DecalSettingsSaturationProxy _settings;

		private readonly ParticleWetSplatter _splatters;

		private Matrix4x4 _localTransform;

		private Transform _parent;

		private float _remainingLifetime;

		private WetDecalSystem.DecalRenderHandle _render;

		private int _settingsDirty;

		private float _totalLifetime;

		public DecalSettingsSaturationProxy Settings => _settings;

		public float AgeingRate { get; set; }

		public bool IsActive => _remainingLifetime > 0f;

		public Matrix4x4 WorldTransform => _parent.localToWorldMatrix * _localTransform;

		public BoundingSphere Bounds
		{
			get
			{
				Matrix4x4 worldTransform = WorldTransform;
				Vector4 column = worldTransform.GetColumn(3);
				Vector3 vector = new Vector3(worldTransform.m00, worldTransform.m11, worldTransform.m22);
				return new BoundingSphere(new Vector3(column.x, column.y, column.z), vector.magnitude);
			}
		}

		IDecalSettings IWetDecal.Settings => _settings;

		public Splatter([NotNull] ParticleWetSplatter splatters, [NotNull] DecalSettings settings)
		{
			_splatters = splatters;
			_settings = new DecalSettingsSaturationProxy(settings);
			settings.Changed += DecalSettingsChanged;
		}

		public void Step(float dt)
		{
			float num = _splatters.Core.Saturation;
			if (_splatters.Lifetime.Enabled)
			{
				_remainingLifetime -= dt * AgeingRate;
				num *= _splatters.Lifetime.Saturation.Evaluate((_totalLifetime - _remainingLifetime) / _totalLifetime);
			}
			if (_settingsDirty > 0 || Math.Abs(_settings.SaturationMultiplier - num) > float.Epsilon)
			{
				_settings.SaturationMultiplier = num;
				_render.UpdateProperties(_settingsDirty > 1);
				_settingsDirty = 0;
			}
		}

		private void DecalSettingsChanged(bool rebuild)
		{
			_settingsDirty = ((!rebuild) ? 1 : 2);
		}

		public void Reset()
		{
			_totalLifetime = 0f;
			_remainingLifetime = 0f;
			AgeingRate = 1f;
			if (_render.IsValid)
			{
				_render.Dispose();
			}
		}

		public void Initialize(Vector3 position, Vector3 normal, Vector3 velocity, [NotNull] Transform parent)
		{
			Vector3 decalSize = _splatters.Core.DecalSize;
			if (_splatters.RandomizeSize.Enabled)
			{
				decalSize *= UnityEngine.Random.Range(_splatters.RandomizeSize.MinInflation, _splatters.RandomizeSize.MaxInflation);
			}
			float angle = Mathf.Acos(Vector3.Dot(Vector3.up, normal)) * 57.29578f;
			Vector3 normalized = Vector3.Cross(Vector3.up, normal).normalized;
			Quaternion q = Quaternion.AngleAxis(angle, normalized);
			position += normal * _splatters.Core.VerticalOffset;
			if (_splatters.ImpactVelocity.Enabled)
			{
				Vector3 vector = velocity - Vector3.Dot(velocity, normal) * normal;
				float magnitude = vector.magnitude;
				Vector3 vector2 = vector / magnitude;
				Quaternion quaternion = Quaternion.LookRotation(vector2, Vector3.up);
				q *= quaternion;
				float num = _splatters.ImpactVelocity.Scale.Evaluate(magnitude);
				decalSize.z *= num;
				float num2 = _splatters.ImpactVelocity.Offset.Evaluate(magnitude);
				position += vector2 * num2 * decalSize.z;
			}
			if (_splatters.RandomizeOrientation.Enabled)
			{
				q *= Quaternion.AngleAxis((UnityEngine.Random.value - 0.5f) * _splatters.RandomizeOrientation.RandomDegrees, Vector3.up);
			}
			Matrix4x4 matrix4x = Matrix4x4.TRS(position, q, decalSize);
			_parent = parent;
			_localTransform = parent.worldToLocalMatrix * matrix4x;
			_totalLifetime = (_splatters.Lifetime.Enabled ? UnityEngine.Random.Range(_splatters.Lifetime.MinLifetime, _splatters.Lifetime.MaxLifetime) : 1f);
			_remainingLifetime = _totalLifetime;
			AgeingRate = 1f;
			_render = _splatters._decalSystem.Add(this);
		}
	}

	[Serializable]
	public class BaseToggleableSettings
	{
		[SerializeField]
		private bool _enabled;

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}
	}

	[Serializable]
	public class CoreSettings
	{
		[Tooltip("Chance of a decal being created when a particle impact occurs")]
		[SerializeField]
		private float _decalChance;

		[Tooltip("Base size of the decals")]
		[SerializeField]
		private Vector3 _decalSize = new Vector3(1f, 0.2f, 1f);

		[Tooltip("How wet the decals appear to be")]
		[SerializeField]
		private float _saturation;

		[Tooltip("Decal position offset from the particle impact point")]
		[SerializeField]
		private float _verticalOffset;

		public Vector3 DecalSize
		{
			get
			{
				return _decalSize;
			}
			set
			{
				_decalSize = value;
			}
		}

		public float VerticalOffset
		{
			get
			{
				return _verticalOffset;
			}
			set
			{
				_verticalOffset = value;
			}
		}

		public float DecalChance
		{
			get
			{
				return _decalChance;
			}
			set
			{
				_decalChance = value;
			}
		}

		public float Saturation
		{
			get
			{
				return _saturation;
			}
			set
			{
				_saturation = value;
			}
		}
	}

	[Serializable]
	public class LimitSettings : BaseToggleableSettings
	{
		[Tooltip("Change the chance of a decal being created as more decals are visible")]
		[SerializeField]
		private AnimationCurve _decalChance = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.75f, 0.5f), new Keyframe(1f, 0f));

		[Tooltip("Maximum number of decals which may be visible at once")]
		[SerializeField]
		private int _maxDecals = 10;

		public int MaxDecals
		{
			get
			{
				return _maxDecals;
			}
			set
			{
				_maxDecals = value;
			}
		}

		public AnimationCurve DecalChance
		{
			get
			{
				return _decalChance;
			}
			set
			{
				_decalChance = value;
			}
		}
	}

	[Serializable]
	public class LifetimeSettings : BaseToggleableSettings
	{
		[Tooltip("Maximum lifetime of decals")]
		[SerializeField]
		private float _maxLifetime = 60f;

		[Tooltip("Minimum lifetime of decals")]
		[SerializeField]
		private float _minLifetime = 30f;

		[Tooltip("Change in saturation over the decal lifetime")]
		[SerializeField]
		private AnimationCurve _saturation = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0.9f), new Keyframe(1f, 0f));

		public float MinLifetime
		{
			get
			{
				return _minLifetime;
			}
			set
			{
				_minLifetime = value;
			}
		}

		public float MaxLifetime
		{
			get
			{
				return _maxLifetime;
			}
			set
			{
				_maxLifetime = value;
			}
		}

		public AnimationCurve Saturation
		{
			get
			{
				return _saturation;
			}
			set
			{
				_saturation = value;
			}
		}
	}

	[Serializable]
	public class ImpactVelocitySettings : BaseToggleableSettings
	{
		[Tooltip("Position offset along particle impact direction based on the impact velocity")]
		[SerializeField]
		private AnimationCurve _offset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0f, 0.25f));

		[Tooltip("Horizontal scale to apply to decals based on the impact velocity")]
		[SerializeField]
		private AnimationCurve _scale = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1.5f));

		public AnimationCurve Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		public AnimationCurve Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
			}
		}
	}

	[Serializable]
	public class RandomizeSizeSettings : BaseToggleableSettings
	{
		[Tooltip("Maximum amount to multiply with base scale")]
		[SerializeField]
		private float _maxInflation = 1.25f;

		[Tooltip("Minimum amount to multiply with base scale")]
		[SerializeField]
		private float _minInflation = 0.75f;

		public float MinInflation
		{
			get
			{
				return _minInflation;
			}
			set
			{
				_minInflation = value;
			}
		}

		public float MaxInflation
		{
			get
			{
				return _maxInflation;
			}
			set
			{
				_maxInflation = value;
			}
		}
	}

	[Serializable]
	public class RandomizeOrientationSettings : BaseToggleableSettings
	{
		[Tooltip("Maximum number of degrees to randomly rotate decal")]
		[SerializeField]
		private float _randomDegrees = 180f;

		public float RandomDegrees
		{
			get
			{
				return _randomDegrees;
			}
			set
			{
				_randomDegrees = value;
			}
		}
	}

	[Serializable]
	public class RecyclingSettings : BaseToggleableSettings
	{
		[Tooltip("How much to artifically accelerate the ageing rate of a random decal when a decal is not created due to the decal limit")]
		[SerializeField]
		private float _maxAcceleratedAgeing;

		[Tooltip("Threshold below which an active decal may be stolen to create new decals when needed")]
		[SerializeField]
		private float _stealThreshold;

		public float MaxAcceleratedAgeing
		{
			get
			{
				return _maxAcceleratedAgeing;
			}
			set
			{
				_maxAcceleratedAgeing = value;
			}
		}

		public float StealThreshold
		{
			get
			{
				return _stealThreshold;
			}
			set
			{
				_stealThreshold = value;
			}
		}
	}

	[ItemCanBeNull]
	private readonly List<Splatter> _activeSplatters;

	private readonly List<ParticleCollisionEvent> _collisionEvents;

	private readonly List<Splatter> _inactiveSplatters;

	private WetDecalSystem _decalSystem;

	private DateTime _lastCleanedNullsUtc = DateTime.MinValue;

	private int _nonPlacedByChance;

	private ParticleSystem _particleSystem;

	private int _totalDecalCount;

	[SerializeField]
	private CoreSettings _core = new CoreSettings();

	[SerializeField]
	private LimitSettings _limit = new LimitSettings();

	[SerializeField]
	private ImpactVelocitySettings _impactVelocity = new ImpactVelocitySettings();

	[SerializeField]
	private RandomizeSizeSettings _randomizeSize = new RandomizeSizeSettings();

	[SerializeField]
	private RandomizeOrientationSettings _randomizeOrientation = new RandomizeOrientationSettings();

	[SerializeField]
	private RecyclingSettings _recycling = new RecyclingSettings();

	[SerializeField]
	private LifetimeSettings _lifetime = new LifetimeSettings();

	[NonSerialized]
	private float _templatesWeightSum;

	[NonSerialized]
	private ParticleWetSplatterTemplate[] _templates;

	public CoreSettings Core => _core;

	public LimitSettings Limit => _limit;

	public ImpactVelocitySettings ImpactVelocity => _impactVelocity;

	public RandomizeSizeSettings RandomizeSize => _randomizeSize;

	public RandomizeOrientationSettings RandomizeOrientation => _randomizeOrientation;

	public RecyclingSettings Recycling => _recycling;

	public LifetimeSettings Lifetime => _lifetime;

	public ParticleWetSplatter()
	{
		_collisionEvents = new List<ParticleCollisionEvent>();
		_activeSplatters = new List<Splatter>();
		_inactiveSplatters = new List<Splatter>();
	}

	protected virtual void Start()
	{
		_particleSystem = GetComponent<ParticleSystem>();
	}

	protected virtual void OnEnable()
	{
		if (_decalSystem == null)
		{
			_decalSystem = new WetDecalSystem();
		}
		_templates = GetComponents<ParticleWetSplatterTemplate>();
		_templatesWeightSum = 0f;
		for (int i = 0; i < _templates.Length; i++)
		{
			_templatesWeightSum += _templates[i].Probability;
		}
	}

	protected virtual void OnDisable()
	{
		for (int i = 0; i < _activeSplatters.Count; i++)
		{
			_activeSplatters[i]?.Reset();
		}
		_inactiveSplatters.AddRange(_activeSplatters);
		_activeSplatters.Clear();
	}

	protected virtual void OnDestroy()
	{
		_decalSystem.Dispose();
	}

	protected virtual void Update()
	{
		for (int i = 0; i < _collisionEvents.Count; i++)
		{
			float num = Core.DecalChance;
			if (Limit.Enabled)
			{
				num *= Limit.DecalChance.Evaluate((float)_activeSplatters.Count / (float)Limit.MaxDecals);
				num *= Math.Min(2f, Mathf.Pow(1.1f, _nonPlacedByChance));
			}
			if (UnityEngine.Random.value > num)
			{
				_nonPlacedByChance++;
				continue;
			}
			ParticleCollisionEvent particleCollisionEvent = _collisionEvents[i];
			if (particleCollisionEvent.colliderComponent == null)
			{
				continue;
			}
			Splatter orCreateSplatter = GetOrCreateSplatter();
			if (orCreateSplatter != null)
			{
				_nonPlacedByChance = 0;
				orCreateSplatter.Initialize(particleCollisionEvent.intersection, particleCollisionEvent.normal, particleCollisionEvent.velocity, particleCollisionEvent.colliderComponent.transform);
				_activeSplatters.Add(orCreateSplatter);
			}
			else if (_activeSplatters.Count > 0 && Recycling.Enabled && Recycling.MaxAcceleratedAgeing > 0f)
			{
				int index = UnityEngine.Random.Range(0, _activeSplatters.Count);
				Splatter splatter = _activeSplatters[index];
				if (splatter != null)
				{
					splatter.AgeingRate = 1f + Math.Max(splatter.AgeingRate - 1f, Recycling.MaxAcceleratedAgeing * UnityEngine.Random.value);
				}
			}
		}
		_collisionEvents.Clear();
		int num2 = 0;
		for (int num3 = _activeSplatters.Count - 1; num3 >= 0; num3--)
		{
			Splatter splatter2 = _activeSplatters[num3];
			if (splatter2 == null)
			{
				num2++;
			}
			else if (splatter2.IsActive)
			{
				splatter2.Step(Time.deltaTime);
			}
			else
			{
				splatter2.Reset();
				_inactiveSplatters.Add(splatter2);
				num2++;
				_activeSplatters[num3] = null;
			}
		}
		if (num2 > 0)
		{
			int num4 = 25;
			if (Limit.Enabled && Recycling.Enabled)
			{
				num4 = Math.Min(num4, Limit.MaxDecals / 16);
			}
			if (num2 > num4 || DateTime.UtcNow - _lastCleanedNullsUtc > TimeSpan.FromSeconds(5.0))
			{
				_activeSplatters.RemoveNulls(num2);
				_lastCleanedNullsUtc = DateTime.UtcNow;
			}
		}
	}

	public void Clear()
	{
		_nonPlacedByChance = 0;
		_totalDecalCount = 0;
		for (int num = _activeSplatters.Count - 1; num >= 0; num--)
		{
			_activeSplatters[num]?.Reset();
		}
		_activeSplatters.Clear();
		_inactiveSplatters.Clear();
	}

	protected virtual void OnParticleCollision(GameObject other)
	{
		_particleSystem.GetCollisionEvents(other, _collisionEvents);
	}

	[CanBeNull]
	private DecalSettings ChooseSettings()
	{
		if (_templates.Length == 0)
		{
			return null;
		}
		float num = 0f;
		float num2 = UnityEngine.Random.value * _templatesWeightSum;
		for (int i = 0; i < _templates.Length - 1; i++)
		{
			ParticleWetSplatterTemplate particleWetSplatterTemplate = _templates[i];
			if (num2 + num < particleWetSplatterTemplate.Probability)
			{
				return particleWetSplatterTemplate.Settings;
			}
			num += particleWetSplatterTemplate.Probability;
		}
		return _templates[_templates.Length - 1].Settings;
	}

	[CanBeNull]
	private Splatter GetOrCreateSplatter()
	{
		if (_inactiveSplatters.Count > 0)
		{
			int index = _inactiveSplatters.Count - 1;
			Splatter result = _inactiveSplatters[index];
			_inactiveSplatters.RemoveAt(index);
			return result;
		}
		if (_limit.Enabled && _totalDecalCount >= _limit.MaxDecals)
		{
			if (!_recycling.Enabled || _recycling.StealThreshold <= 0f)
			{
				return null;
			}
			for (int i = 0; i < Math.Min(25, _activeSplatters.Count); i++)
			{
				Splatter splatter = _activeSplatters[i];
				if (splatter != null && splatter.Bounds.radius * splatter.Settings.Saturation < _recycling.StealThreshold)
				{
					splatter.Reset();
					_activeSplatters.RemoveAt(i);
					return splatter;
				}
			}
			return null;
		}
		DecalSettings decalSettings = ChooseSettings();
		if (decalSettings == null)
		{
			return null;
		}
		_totalDecalCount++;
		return new Splatter(this, decalSettings);
	}

	private void DrawGizmo(bool selected)
	{
		for (int i = 0; i < _activeSplatters.Count; i++)
		{
			Splatter splatter = _activeSplatters[i];
			if (splatter != null)
			{
				Color color = new Color(0f, 0.7f, 1f, 1f);
				Gizmos.matrix = splatter.WorldTransform;
				color.a = (selected ? 0.3f : 0.1f);
				color.a *= (base.isActiveAndEnabled ? 0.15f : 0.1f);
				Gizmos.color = color;
				Gizmos.DrawCube(Vector3.zero, Vector3.one);
				color.a = (selected ? 0.5f : 0.2f);
				color.a *= (base.isActiveAndEnabled ? 1f : 0.75f);
				Gizmos.color = color;
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		DrawGizmo(selected: true);
	}
}
