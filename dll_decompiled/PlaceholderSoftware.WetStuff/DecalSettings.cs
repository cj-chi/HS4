using System;
using PlaceholderSoftware.WetStuff.Debugging;
using PlaceholderSoftware.WetStuff.Timeline.Settings;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

[Serializable]
public class DecalSettings : IDecalSettings
{
	private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(DecalSettings).Name);

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("The distance from the edge of the shape from which saturation begins to fade.")]
	[Range(0f, 1f)]
	private float _edgeFadeoff;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("Jitter sample positions of detail layer textures")]
	private bool _enableJitter;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("How sharply the decal fades around faces facing in different directions")]
	[Range(0.001f, 10f)]
	private float _faceSharpness;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("The detail layer mode")]
	private LayerMode _layerMode;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("The layer projection mode")]
	private ProjectionMode _layerProjection;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("Determines if the decal projects wetness, or if it drys other wet decals")]
	private WetDecalMode _mode;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("Maximum jitter in texels to the detail layer sampling coordinates")]
	[Range(0f, 10f)]
	private float _sampleJitter;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("How wet the decal appears to be")]
	[Range(0f, 1f)]
	private float _saturation;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("The shape of the decal")]
	private DecalShape _shape;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("Per pixel saturation projected down the decal's x axis")]
	private DecalLayer _xLayer;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("Per pixel saturation projected down the decal's y axis")]
	private DecalLayer _yLayer;

	[SerializeField]
	[UsedImplicitly]
	[Tooltip("Per pixel saturation projected down the decal's z axis")]
	private DecalLayer _zLayer;

	public WetDecalMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			OnChanged(requiresRebuild: true);
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
			bool num = Math.Abs(_saturation - value) > float.Epsilon;
			_saturation = value;
			if (num)
			{
				OnChanged(requiresRebuild: false);
			}
		}
	}

	public float SampleJitter
	{
		get
		{
			return _sampleJitter;
		}
		set
		{
			bool num = Math.Abs(value - _sampleJitter) > float.Epsilon;
			_sampleJitter = value;
			if (num)
			{
				OnChanged(requiresRebuild: false);
			}
		}
	}

	public bool EnableJitter
	{
		get
		{
			return _enableJitter;
		}
		set
		{
			_enableJitter = value;
			OnChanged(requiresRebuild: true);
		}
	}

	public LayerMode LayerMode
	{
		get
		{
			return _layerMode;
		}
		set
		{
			_layerMode = value;
			OnChanged(requiresRebuild: true);
		}
	}

	public ProjectionMode LayerProjection
	{
		get
		{
			return _layerProjection;
		}
		set
		{
			_layerProjection = value;
			OnChanged(requiresRebuild: true);
		}
	}

	public float FaceSharpness
	{
		get
		{
			return _faceSharpness;
		}
		set
		{
			bool num = Math.Abs(value - _faceSharpness) > float.Epsilon;
			_faceSharpness = value;
			if (num)
			{
				OnChanged(requiresRebuild: false);
			}
		}
	}

	public DecalLayer XLayer => _xLayer;

	public DecalLayer YLayer => _yLayer;

	public DecalLayer ZLayer => _zLayer;

	public DecalShape Shape
	{
		get
		{
			return _shape;
		}
		set
		{
			_shape = value;
			OnChanged(requiresRebuild: true);
		}
	}

	public float EdgeFadeoff
	{
		get
		{
			return _edgeFadeoff;
		}
		set
		{
			_edgeFadeoff = value;
			OnChanged(requiresRebuild: false);
		}
	}

	public event Action<bool> Changed;

	public DecalSettings()
	{
		_xLayer = new DecalLayer();
		_yLayer = new DecalLayer();
		_zLayer = new DecalLayer();
	}

	public void Init()
	{
		this.Changed = null;
		InitLayer(_xLayer);
		InitLayer(_yLayer);
		InitLayer(_zLayer);
	}

	private void InitLayer([NotNull] DecalLayer layer)
	{
		Log.AssertAndThrowPossibleBug(layer != null, "12987B54-4215-4C3C-AD16-A9AA23EB17A8", "Layer Is Null");
		layer.Init();
		layer.Changed += OnChanged;
	}

	public virtual void OnChanged(bool requiresRebuild)
	{
		this.Changed?.Invoke(requiresRebuild);
	}

	internal DecalSettingsDataContainer.DecalSettingsData Get()
	{
		return new DecalSettingsDataContainer.DecalSettingsData(Saturation, _xLayer.Get(), _yLayer.Get(), _zLayer.Get());
	}

	internal void Apply(DecalSettingsDataContainer.DecalSettingsData data)
	{
		Saturation = data.Saturation;
		_xLayer.Apply(data.XLayer);
		_yLayer.Apply(data.YLayer);
		_zLayer.Apply(data.ZLayer);
	}
}
