using System;

namespace PlaceholderSoftware.WetStuff.Timeline.Mixers;

internal class SingleMixer : BaseMixer<float>
{
	private float _sum;

	private float _weight;

	public override void Start()
	{
		base.Start();
		_weight = 0f;
		_sum = 0f;
	}

	protected override void MixImpl(float weight, float data)
	{
		_sum += data * weight;
		_weight += weight;
	}

	protected override float GetResult(float defaultValue)
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
