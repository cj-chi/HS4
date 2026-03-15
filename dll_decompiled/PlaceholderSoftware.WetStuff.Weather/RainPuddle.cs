using System;
using System.Collections.Generic;
using PlaceholderSoftware.WetStuff.Debugging;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather;

[ExecuteInEditMode]
[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/RainPuddle/")]
public class RainPuddle : MonoBehaviour
{
	public enum RainState
	{
		Drying,
		Raining
	}

	[Serializable]
	public struct ChannelValues
	{
		[Range(0f, 1f)]
		public float Threshold;

		[Range(0f, 1f)]
		public float Softness;

		[Range(0f, 1f)]
		public float OutMin;

		[Range(0f, 1f)]
		public float OutMax;

		private ChannelValues(float threshold, float softness, float outMin, float outMax)
		{
			Threshold = threshold;
			Softness = softness;
			OutMin = outMin;
			OutMax = outMax;
		}

		public override string ToString()
		{
			return $"T:{Threshold}, S:{Softness}, Out:{OutMin}..{OutMax}";
		}

		public void Apply([NotNull] DecalLayerChannel channel)
		{
			channel.InputRangeThreshold = Threshold;
			channel.InputRangeSoftness = Softness;
			channel.OutputRange = new Vector2(OutMin, OutMax);
		}

		public static ChannelValues Lerp(ChannelValues a, ChannelValues b, ChannelCurves curves, float progress)
		{
			return new ChannelValues(Mathf.Lerp(a.Threshold, b.Threshold, curves.Threshold.Evaluate(progress)), Mathf.Lerp(a.Softness, b.Softness, curves.Softness.Evaluate(progress)), Mathf.Lerp(a.OutMin, b.OutMin, curves.OutputMin.Evaluate(progress)), Mathf.Lerp(a.OutMax, b.OutMax, curves.OutputMax.Evaluate(progress)));
		}
	}

	[Serializable]
	public struct ChannelCurves
	{
		public AnimationCurve Threshold;

		public AnimationCurve Softness;

		public AnimationCurve OutputMin;

		public AnimationCurve OutputMax;

		public static readonly ChannelCurves Default = new ChannelCurves
		{
			Threshold = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
			Softness = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
			OutputMin = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
			OutputMax = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
		};
	}

	[Serializable]
	public struct DecalState
	{
		[Range(0f, 1f)]
		public float Saturation;

		public ChannelValues Red;

		public ChannelValues Blue;

		public ChannelValues Green;

		public ChannelValues Alpha;

		public DecalState(float saturation, ChannelValues red, ChannelValues green, ChannelValues blue, ChannelValues alpha)
		{
			Saturation = saturation;
			Red = red;
			Blue = blue;
			Green = green;
			Alpha = alpha;
		}

		public override string ToString()
		{
			return $"S:{Saturation}, R:{{{Red}}},- G:{{{Green}}}, B:{{{Blue}}}, A:{{{Alpha}}}";
		}

		public void Apply([NotNull] WetDecal decal)
		{
			decal.Settings.Saturation = Saturation;
			Red.Apply(decal.Settings.YLayer.Channel1);
			Blue.Apply(decal.Settings.YLayer.Channel2);
			Green.Apply(decal.Settings.YLayer.Channel3);
			Alpha.Apply(decal.Settings.YLayer.Channel4);
		}

		public static DecalState Lerp(DecalState a, DecalState b, DecalStateCurves curves, float progress)
		{
			return new DecalState(Mathf.Lerp(a.Saturation, b.Saturation, curves.Saturation.Evaluate(progress)), ChannelValues.Lerp(a.Red, b.Red, curves.Red, progress), ChannelValues.Lerp(a.Green, b.Green, curves.Green, progress), ChannelValues.Lerp(a.Blue, b.Blue, curves.Blue, progress), ChannelValues.Lerp(a.Alpha, b.Alpha, curves.Alpha, progress));
		}
	}

	[Serializable]
	public struct DecalStateCurves
	{
		public AnimationCurve Saturation;

		public ChannelCurves Red;

		public ChannelCurves Blue;

		public ChannelCurves Green;

		public ChannelCurves Alpha;

		public static readonly DecalStateCurves Default = new DecalStateCurves
		{
			Saturation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
			Red = ChannelCurves.Default,
			Blue = ChannelCurves.Default,
			Green = ChannelCurves.Default,
			Alpha = ChannelCurves.Default
		};
	}

	private class RainDecal : IDisposable
	{
		private readonly RainPuddle _rain;

		private readonly WetDecal _decal;

		private float _progress;

		private float _rateScale;

		private float _retiredAge;

		private RainDecalState _mode;

		private DecalState _state;

		private DecalState _baseState;

		public bool IsDead
		{
			get
			{
				if (_mode == RainDecalState.Retired)
				{
					return IsComplete;
				}
				return false;
			}
		}

		public bool IsComplete => _progress >= 1f;

		public RainDecal(RainPuddle rain, WetDecal decal)
		{
			_rain = rain;
			_decal = decal;
		}

		public void Rain(float initialProgress = 0f)
		{
			_baseState = _rain.DryState;
			_state = _baseState;
			_progress = initialProgress;
			_rateScale = 1f;
			_mode = RainDecalState.Raining;
			_state.Apply(_decal);
			_decal.gameObject.SetActive(value: true);
		}

		public void Dry()
		{
			if (_progress > float.Epsilon)
			{
				_baseState = _state;
				_rateScale = 1f / _progress;
				_progress = 0f;
			}
			else
			{
				_rateScale = 1f;
				_progress = 1f;
			}
			_mode = RainDecalState.Drying;
		}

		public void Retire()
		{
			if (_mode == RainDecalState.Raining)
			{
				Dry();
			}
			_mode = RainDecalState.Retired;
			_retiredAge = _progress;
		}

		public void Dispose()
		{
			UnityEngine.Object.DestroyImmediate(_decal.gameObject);
		}

		public bool Update()
		{
			if (_decal == null)
			{
				return false;
			}
			_progress = Mathf.Clamp01(_progress + _rain.Rate * Time.deltaTime * _rateScale);
			if (_mode == RainDecalState.Raining)
			{
				_state = DecalState.Lerp(_baseState, _rain.WetState, _rain.Raining, _progress);
			}
			else
			{
				_state = DecalState.Lerp(_baseState, _rain.DryState, _rain.Drying, _progress);
			}
			if (_mode == RainDecalState.Retired)
			{
				float num = (_progress - _retiredAge) / (1f - _retiredAge);
				if (float.IsNaN(num))
				{
					num = 1f;
				}
				_state.Saturation *= _rain.RetireFadeout.Evaluate(num);
			}
			_state.Apply(_decal);
			if (IsDead)
			{
				_decal.gameObject.SetActive(value: false);
			}
			return !IsDead;
		}
	}

	private enum RainDecalState
	{
		Raining,
		Drying,
		Retired
	}

	private static readonly Log Log = Logs.Create(LogCategory.Integration, typeof(RainPuddle).Name);

	[NonSerialized]
	private readonly List<RainDecal> _raining;

	[NonSerialized]
	private readonly List<RainDecal> _drying;

	[NonSerialized]
	private readonly List<RainDecal> _retired;

	[NonSerialized]
	private readonly Stack<RainDecal> _dead;

	public DecalState WetState;

	public DecalState DryState;

	public DecalStateCurves Raining;

	public DecalStateCurves Drying;

	public AnimationCurve RetireFadeout = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public WetDecal DecalPrefab;

	public float Rate { get; set; }

	public RainState State { get; private set; }

	public RainPuddle()
	{
		Rate = 0.05f;
		_raining = new List<RainDecal>();
		_drying = new List<RainDecal>();
		_retired = new List<RainDecal>();
		_dead = new Stack<RainDecal>();
	}

	public void BeginRaining(float initialProgress = 0f)
	{
		Retire(_drying);
		Retire(_raining);
		RainDecal rainDecal = Spawn();
		if (rainDecal != null)
		{
			rainDecal.Rain(initialProgress);
			_raining.Add(rainDecal);
		}
		State = RainState.Raining;
	}

	public void BeginDrying()
	{
		Dry(_raining);
		State = RainState.Drying;
	}

	[CanBeNull]
	private RainDecal Spawn()
	{
		if (_dead.Count > 0)
		{
			return _dead.Pop();
		}
		if (DecalPrefab == null)
		{
			return null;
		}
		WetDecal wetDecal = UnityEngine.Object.Instantiate(DecalPrefab, base.transform);
		wetDecal.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
		return new RainDecal(this, wetDecal);
	}

	protected void Awake()
	{
	}

	protected void OnDestroy()
	{
	}

	protected virtual void Update()
	{
		Update(_raining);
		Update(_drying);
		Update(_retired);
	}

	private void Update([NotNull] List<RainDecal> decals)
	{
		for (int num = decals.Count - 1; num >= 0; num--)
		{
			RainDecal rainDecal = decals[num];
			if (!rainDecal.Update())
			{
				decals.RemoveAt(num);
				if (Application.isPlaying)
				{
					_dead.Push(rainDecal);
				}
				else
				{
					rainDecal.Dispose();
				}
			}
		}
	}

	private void Retire([NotNull] List<RainDecal> decals)
	{
		for (int i = 0; i < decals.Count; i++)
		{
			decals[i].Retire();
			_retired.Add(decals[i]);
		}
		decals.Clear();
	}

	private void Dry([NotNull] List<RainDecal> decals)
	{
		for (int i = 0; i < decals.Count; i++)
		{
			decals[i].Dry();
			_drying.Add(decals[i]);
		}
		decals.Clear();
	}
}
