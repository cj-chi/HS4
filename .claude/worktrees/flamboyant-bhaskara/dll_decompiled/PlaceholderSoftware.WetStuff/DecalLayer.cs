using System;
using PlaceholderSoftware.WetStuff.Rendering;
using PlaceholderSoftware.WetStuff.Timeline.Settings;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

[Serializable]
public class DecalLayer
{
	[SerializeField]
	[UsedImplicitly]
	private DecalLayerChannel _channel2;

	[SerializeField]
	[UsedImplicitly]
	private DecalLayerChannel _channel3;

	[SerializeField]
	[UsedImplicitly]
	private DecalLayerChannel _channel4;

	[SerializeField]
	[UsedImplicitly]
	private DecalLayerChannel _channel1;

	[SerializeField]
	[Tooltip("Texture with 4 channels (RGBA) of saturation")]
	[UsedImplicitly]
	private Texture2D _layerMask;

	[SerializeField]
	[Tooltip("Offset the position of the layer mask image")]
	[UsedImplicitly]
	private Vector2 _layerMaskOffset;

	[SerializeField]
	[Tooltip("Scale the layer mask image")]
	[UsedImplicitly]
	private Vector2 _layerMaskScale;

	[SerializeField]
	[UsedImplicitly]
	private bool _editorSectionFoldout;

	public Vector4 LayerMaskScaleOffset
	{
		get
		{
			return new Vector4(_layerMaskScale.x, _layerMaskScale.y, _layerMaskOffset.x, _layerMaskOffset.y);
		}
		set
		{
			_layerMaskScale = new Vector2(value.x, value.y);
			_layerMaskOffset = new Vector2(value.z, value.w);
			OnChanged(requiresRebuild: true);
		}
	}

	[CanBeNull]
	public Texture2D LayerMask
	{
		get
		{
			return _layerMask;
		}
		set
		{
			_layerMask = value;
			OnChanged(requiresRebuild: true);
		}
	}

	public DecalLayerChannel Channel1 => _channel1;

	public DecalLayerChannel Channel2 => _channel2;

	public DecalLayerChannel Channel3 => _channel3;

	public DecalLayerChannel Channel4 => _channel4;

	public DecalLayerChannelIndexer Channels => new DecalLayerChannelIndexer(this);

	public event Action<bool> Changed;

	public DecalLayer()
	{
		_layerMaskScale = new Vector2(1f, 1f);
		_layerMaskOffset = new Vector2(0f, 0f);
		_channel1 = new DecalLayerChannel
		{
			Mode = DecalLayerChannel.DecalChannelMode.SimpleRangeRemap
		};
		_channel2 = new DecalLayerChannel();
		_channel3 = new DecalLayerChannel();
		_channel4 = new DecalLayerChannel();
	}

	public void Init()
	{
		this.Changed = null;
		_channel1.Init();
		_channel2.Init();
		_channel3.Init();
		_channel4.Init();
		Action value = delegate
		{
			OnChanged(requiresRebuild: false);
		};
		_channel1.Changed += value;
		_channel2.Changed += value;
		_channel3.Changed += value;
		_channel4.Changed += value;
	}

	public void EvaluateInputRange(out Vector4 start, out Vector4 end)
	{
		Vector2 vector = Channel1.EvaluateInputRange();
		Vector2 vector2 = Channel2.EvaluateInputRange();
		Vector2 vector3 = Channel3.EvaluateInputRange();
		Vector2 vector4 = Channel4.EvaluateInputRange();
		start = new Vector4(vector.x, vector2.x, vector3.x, vector4.x);
		end = new Vector4(vector.y, vector2.y, vector3.y, vector4.y);
	}

	public void EvaluateOutputRange(out Vector4 start, out Vector4 end)
	{
		Vector2 vector = Channel1.EvaluateOutputRange();
		Vector2 vector2 = Channel2.EvaluateOutputRange();
		Vector2 vector3 = Channel3.EvaluateOutputRange();
		Vector2 vector4 = Channel4.EvaluateOutputRange();
		start = new Vector4(vector.x, vector2.x, vector3.x, vector4.x);
		end = new Vector4(vector.y, vector2.y, vector3.y, vector4.y);
	}

	internal void EvaluateRanges(out LayerParameters parameters)
	{
		EvaluateInputRange(out parameters.LayerInputStart, out var end);
		parameters.LayerInputExtent = end - parameters.LayerInputStart;
		EvaluateOutputRange(out parameters.LayerOutputStart, out parameters.LayerOutputEnd);
	}

	protected virtual void OnChanged(bool requiresRebuild)
	{
		this.Changed?.Invoke(requiresRebuild);
	}

	internal DecalSettingsDataContainer.DecalLayerData Get()
	{
		return new DecalSettingsDataContainer.DecalLayerData(Channel1.Get(), Channel2.Get(), Channel3.Get(), Channel4.Get());
	}

	public void Apply(DecalSettingsDataContainer.DecalLayerData data)
	{
		Channel1.Apply(data.Channel1);
		Channel2.Apply(data.Channel2);
		Channel3.Apply(data.Channel3);
		Channel4.Apply(data.Channel4);
	}
}
