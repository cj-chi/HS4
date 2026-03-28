using AIOcontrol;
using UnityEngine;

public class NewDemoSky : MonoBehaviour
{
	public Material[] sky = new Material[4];

	public float transitionSec = 10f;

	private static float dayTime;

	public AIOSun sample;

	private float oldTime;

	private bool timeControl;

	private bool cloudControl;

	public ReflectionProbe ball;

	public float reflectionRate = 0.1f;

	private void Awake()
	{
	}

	private void Start()
	{
		timeControl = true;
		cloudControl = true;
		sample.ambientOverrider = true;
	}

	private void Update()
	{
		if (!(reflectionRate > 0.1f))
		{
			ball.RenderProbe();
		}
	}

	public void TimeControl(bool enable)
	{
		timeControl = enable;
		if (!timeControl)
		{
			sample.masterTimeScale = 1f;
		}
	}

	public void AIOtime(float t)
	{
		if (timeControl)
		{
			sample.SetAIOTime(t);
		}
	}

	public void SpeedChange(float t)
	{
		if (timeControl)
		{
			sample.masterTimeScale = t;
		}
	}

	public void CloudsControl(bool enable)
	{
		cloudControl = enable;
	}

	public void CloudsD(float d)
	{
		if (cloudControl && RenderSettings.skybox.HasProperty("_CloudsDensity"))
		{
			RenderSettings.skybox.SetFloat("_CloudsDensity", d);
		}
	}

	public void Cloudst(float t)
	{
		if (cloudControl && RenderSettings.skybox.HasProperty("_CloudsThickness"))
		{
			RenderSettings.skybox.SetFloat("_CloudsThickness", t);
		}
	}

	public void AmbientControl(bool enable)
	{
		sample.ambientOverrider = enable;
	}

	public void AmbientHue(float H)
	{
		sample.ambientIntensity = H;
	}

	public void AmbientBright(float B)
	{
		sample.ambientOffset = B;
	}

	public void transition(int i)
	{
		if (!sample.IsTransition)
		{
			sample.SkyboxLerp(sky[i], transitionSec);
		}
	}
}
