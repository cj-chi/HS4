using System;
using PlaceholderSoftware.WetStuff.Timeline.Settings;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

[Serializable]
public class DecalLayerChannel
{
	public enum DecalChannelMode
	{
		Disabled,
		Passthrough,
		SimpleRangeRemap,
		AdvancedRangeRemap
	}

	public const float MinSoftness = 1f / 51f;

	public const float MaxSoftness = 1f;

	[SerializeField]
	[Range(1f / 51f, 1f)]
	[Tooltip("How steep the transition is from wet to dry")]
	private float _inputRangeSoftness;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Threshold for which values are considered wet")]
	private float _inputRangeThreshold;

	[SerializeField]
	[Tooltip("How the input texture data is transformed into wetness values")]
	private DecalChannelMode _mode;

	[SerializeField]
	[MinMax(0, 1)]
	[Tooltip("Limit the minimum and maximum wetness values of the output")]
	private Vector2 _outputRange;

	public DecalChannelMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			OnChanged();
		}
	}

	public float InputRangeThreshold
	{
		get
		{
			return _inputRangeThreshold;
		}
		set
		{
			bool num = Math.Abs(_inputRangeThreshold - value) > float.Epsilon;
			_inputRangeThreshold = value;
			if (num)
			{
				OnChanged();
			}
		}
	}

	public float InputRangeSoftness
	{
		get
		{
			return _inputRangeSoftness;
		}
		set
		{
			if (value < 1f / 51f)
			{
				value = 1f / 51f;
			}
			else if (value > 1f)
			{
				value = 1f;
			}
			bool num = Math.Abs(_inputRangeSoftness - value) > float.Epsilon;
			_inputRangeSoftness = value;
			if (num)
			{
				OnChanged();
			}
		}
	}

	public Vector2 OutputRange
	{
		get
		{
			return _outputRange;
		}
		set
		{
			bool num = (_outputRange - value).SqrMagnitude() > float.Epsilon;
			_outputRange = value;
			if (num)
			{
				OnChanged();
			}
		}
	}

	public event Action Changed;

	public DecalLayerChannel()
	{
		Mode = DecalChannelMode.Disabled;
		InputRangeThreshold = 0.5f;
		InputRangeSoftness = 1f;
		OutputRange = new Vector2(0f, 1f);
	}

	internal void Init()
	{
		this.Changed = null;
	}

	private static Vector2 EvaluateRange(float threshold, float softness)
	{
		float num = 1f - threshold;
		float num2 = Math.Max(softness, 0.0001f);
		float num3 = num * (1f + num2) - num2;
		return new Vector2(num3, num3 + num2);
	}

	public static Vector2 EvaluateInputRange(DecalChannelMode mode, float threshold, float softness)
	{
		return mode switch
		{
			DecalChannelMode.Disabled => new Vector2(1f, 1f), 
			DecalChannelMode.Passthrough => new Vector2(0f, 1f), 
			_ => EvaluateRange(threshold, softness), 
		};
	}

	public Vector2 EvaluateInputRange()
	{
		return EvaluateInputRange(Mode, InputRangeThreshold, InputRangeSoftness);
	}

	public static Vector2 EvaluateOutputRange(DecalChannelMode mode, Vector2 range)
	{
		if (mode != DecalChannelMode.AdvancedRangeRemap)
		{
			return new Vector2(0f, 1f);
		}
		return range;
	}

	public Vector2 EvaluateOutputRange()
	{
		return EvaluateOutputRange(Mode, OutputRange);
	}

	protected virtual void OnChanged()
	{
		this.Changed?.Invoke();
	}

	internal DecalSettingsDataContainer.DecalLayerChannelData Get()
	{
		return new DecalSettingsDataContainer.DecalLayerChannelData(InputRangeThreshold, InputRangeSoftness, OutputRange);
	}

	internal void Apply(DecalSettingsDataContainer.DecalLayerChannelData data)
	{
		InputRangeThreshold = data.InputRangeThreshold;
		InputRangeSoftness = data.InputRangeSoftness;
		OutputRange = data.OutputRange;
	}
}
