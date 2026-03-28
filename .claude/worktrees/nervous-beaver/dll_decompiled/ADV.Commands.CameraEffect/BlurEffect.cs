using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace ADV.Commands.CameraEffect;

public class BlurEffect : CommandBase
{
	private Blur blur;

	private int valueFrom;

	private int valueTo;

	private float time;

	private float timer;

	public override string[] ArgsLabel => new string[3] { "From", "To", "Time" };

	public override string[] ArgsDefault => new string[3] { "0", "10", "0" };

	public override void Do()
	{
		base.Do();
		blur = base.scenario.advScene.advCamera.GetComponent<Blur>();
		int num = 0;
		valueFrom = Mathf.Clamp(int.Parse(args[num++]), 0, 10);
		valueTo = Mathf.Clamp(int.Parse(args[num++]), 0, 10);
		time = float.Parse(args[num++]);
		blur.enabled = true;
		timer = 0f;
	}

	public override bool Process()
	{
		base.Process();
		timer = Mathf.Min(timer + Time.deltaTime, time);
		float t = ((time == 0f) ? 1f : Mathf.InverseLerp(0f, time, timer));
		blur.iterations = (int)Mathf.Lerp(valueFrom, valueTo, t);
		return timer >= time;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		blur.iterations = valueTo;
		if (blur.iterations == 0)
		{
			blur.enabled = false;
		}
	}
}
