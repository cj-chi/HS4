using System;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Timeline.Mixers;

internal class Vector4Mixer : BaseMixer<Vector4>
{
	private Vector4 _sum;

	private float _weight;

	public override void Start()
	{
		base.Start();
		_weight = 0f;
		_sum = Vector4.zero;
	}

	protected override void MixImpl(float weight, Vector4 data)
	{
		_sum += data * weight;
		_weight += weight;
	}

	protected override Vector4 GetResult(Vector4 defaultValue)
	{
		if (Math.Abs(_weight - 1f) < float.Epsilon)
		{
			return _sum;
		}
		if (_weight > 1f)
		{
			return _sum * (1f / _weight);
		}
		return _sum + defaultValue * (1f - _weight);
	}
}
