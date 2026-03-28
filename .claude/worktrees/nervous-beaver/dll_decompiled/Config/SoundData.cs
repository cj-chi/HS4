using System;
using System.Text.RegularExpressions;
using UniRx;

namespace Config;

public class SoundData
{
	public IObservable<SoundData> observer => _volume.Select((int _) => this).Merge(_switch.Select((bool _) => this)).Skip(1);

	public int Volume
	{
		get
		{
			return _volume.Value;
		}
		set
		{
			_volume.Value = value;
		}
	}

	private IntReactiveProperty _volume { get; } = new IntReactiveProperty(100);

	public bool Switch
	{
		get
		{
			return _switch.Value;
		}
		set
		{
			_switch.Value = value;
		}
	}

	private BoolReactiveProperty _switch { get; } = new BoolReactiveProperty(initialValue: true);

	public float GetVolume()
	{
		if (!Switch)
		{
			return 0f;
		}
		return (float)Volume * 0.01f;
	}

	public static implicit operator string(SoundData _sd)
	{
		return $"Volume[{_sd.Volume}] : Switch[{_sd.Switch}]";
	}

	public void Parse(string _str)
	{
		Match match = Regex.Match(_str, "Volume\\[([0-9]*)\\] : Switch\\[([a-zA-Z]*)\\]");
		if (match.Success)
		{
			Volume = int.Parse(match.Groups[1].Value);
			Switch = bool.Parse(match.Groups[2].Value);
		}
	}
}
