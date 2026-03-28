using PlaceholderSoftware.WetStuff.Debugging;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/WetDecal/")]
public class WetDecal : MonoBehaviour, IWetDecal
{
	private enum UpdateType
	{
		None,
		Normal,
		Rebuild
	}

	private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(WetDecal).Name);

	[SerializeField]
	[UsedImplicitly]
	private DecalSettings _settings;

	private WetDecalSystem _system;

	private Transform _transform;

	private WetDecalSystem.DecalRenderHandle _render;

	private UpdateType _requiredUpdate;

	public DecalSettings Settings => _settings;

	private Transform TransformCache
	{
		get
		{
			if (_transform == null)
			{
				_transform = base.transform;
			}
			return _transform;
		}
	}

	IDecalSettings IWetDecal.Settings => _settings;

	BoundingSphere IWetDecal.Bounds => new BoundingSphere(TransformCache.position, TransformCache.lossyScale.magnitude);

	Matrix4x4 IWetDecal.WorldTransform => TransformCache.localToWorldMatrix;

	public WetDecal()
	{
		_settings = new DecalSettings
		{
			Saturation = 0.5f,
			EdgeFadeoff = 0.1f,
			LayerProjection = ProjectionMode.Local,
			FaceSharpness = 1f,
			LayerMode = LayerMode.None
		};
	}

	public void Step(float dt)
	{
		if (_render.IsValid)
		{
			_render.UpdateProperties(_requiredUpdate >= UpdateType.Rebuild);
		}
		_requiredUpdate = UpdateType.None;
	}

	private void OnSettingsChanged(bool rebuild)
	{
		if (this != null && base.isActiveAndEnabled)
		{
			RequireUpdate((!rebuild) ? UpdateType.Normal : UpdateType.Rebuild);
		}
	}

	protected virtual void OnValidate()
	{
		if (_system != null)
		{
			RequireUpdate(UpdateType.Rebuild);
		}
	}

	protected virtual void OnEnable()
	{
		_settings.Init();
		_settings.Changed += OnSettingsChanged;
		if (_system == null)
		{
			_system = new WetDecalSystem();
		}
		_render = _system.Add(this);
		RequireUpdate(UpdateType.Rebuild, immediate: true);
	}

	protected virtual void OnDisable()
	{
		if (_render.IsValid)
		{
			_render.Dispose();
		}
	}

	protected virtual void OnDestroy()
	{
		if (_system != null)
		{
			_system.Dispose();
		}
	}

	private void RequireUpdate(UpdateType type, bool immediate = false)
	{
		UpdateType requiredUpdate = _requiredUpdate;
		if (_requiredUpdate < type)
		{
			_requiredUpdate = type;
		}
		if (immediate)
		{
			Step(0f);
		}
		else if (requiredUpdate == UpdateType.None)
		{
			_system.QueueForUpdate(this);
		}
	}

	private void DrawGizmo(bool selected)
	{
		Color color = new Color(0f, 0.7f, 1f, 1f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		if (Settings.Shape == DecalShape.Cube)
		{
			color.a = (selected ? 0.3f : 0.1f);
			color.a *= (base.isActiveAndEnabled ? 0.15f : 0.1f);
			Gizmos.color = color;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}
		else if (Settings.Shape == DecalShape.Sphere)
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0f);
			Gizmos.DrawSphere(Vector3.zero, 0.5f);
		}
		else
		{
			Log.Error("Unknown decal shape: '{0}'", Settings.Shape);
		}
		color.a = (selected ? 0.5f : 0.2f);
		color.a *= (base.isActiveAndEnabled ? 1f : 0.75f);
		Gizmos.color = color;
		if (Settings.Shape == DecalShape.Cube)
		{
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
		else if (Settings.Shape == DecalShape.Sphere)
		{
			Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
		}
	}

	protected virtual void OnDrawGizmos()
	{
		DrawGizmo(selected: false);
	}

	protected virtual void OnDrawGizmosSelected()
	{
		DrawGizmo(selected: true);
	}
}
