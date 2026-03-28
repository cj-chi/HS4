using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather;

[ExecuteInEditMode]
[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/AutoRainPuddle/")]
public class AutoRainPuddle : RainPuddle
{
	private const int MinFrameCounter = 10;

	[SerializeField]
	public BaseExternalWetnessSource WetnessSource;

	[SerializeField]
	[Range(0f, 1f)]
	public float RainingSpeed = 0.05f;

	[SerializeField]
	[Range(0f, 1f)]
	public float DryingSpeed = 0.02f;

	private bool _raining;

	private int _frameCounter;

	protected override void Update()
	{
		UpdateState();
		base.Update();
	}

	private void UpdateState()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		float rainIntensity = WetnessSource.RainIntensity;
		bool flag = rainIntensity > 0f;
		float num = (flag ? RainingSpeed : DryingSpeed);
		base.Rate = ((Time.deltaTime <= float.Epsilon) ? 0f : (Mathf.Abs(rainIntensity) * num));
		if (_raining != flag)
		{
			_frameCounter = 0;
		}
		if (_frameCounter > 10)
		{
			if (flag && base.State != RainState.Raining)
			{
				BeginRaining();
			}
			else if (!flag && base.State != RainState.Drying)
			{
				BeginDrying();
			}
		}
		_frameCounter++;
		_raining = flag;
	}
}
