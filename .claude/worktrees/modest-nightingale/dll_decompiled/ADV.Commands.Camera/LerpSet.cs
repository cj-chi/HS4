using UnityEngine;

namespace ADV.Commands.Camera;

public class LerpSet : Base
{
	protected Vector3 initPos;

	protected Vector3 initAng;

	protected Vector3 endPos;

	protected Vector3 endAng;

	protected float startTime;

	protected bool isSuccess;

	protected float time;

	protected float timer;

	protected Vector3 calcPos;

	protected Vector3 calcAng;

	private Transform camT;

	public override string[] ArgsLabel => new string[4] { "Time", "X,Y,Z", "Pitch,Yaw,Roll", "Dir" };

	public override string[] ArgsDefault => null;

	protected void Init(TextScenario scenario)
	{
		timer = 0f;
		time = 0f;
		calcPos = new Vector3(float.NaN, float.NaN, float.NaN);
		calcAng = new Vector3(float.NaN, float.NaN, float.NaN);
		camT = scenario.advScene.advCamera.transform;
		endPos = (initPos = camT.position);
		endAng = (initAng = camT.eulerAngles);
	}

	protected virtual void Analytics(string[] args, TextScenario scenario)
	{
		int num = 0;
		float.TryParse(args[num++], out time);
		string obj = args[num++];
		string text = args[num++];
		float result = 0f;
		string[] array = obj.Split(',');
		for (int i = 0; i < array.Length && i < 3; i++)
		{
			if (float.TryParse(array[i], out result))
			{
				calcPos[i] = result;
			}
		}
		string[] array2 = text.Split(',');
		for (int j = 0; j < array2.Length && j < 3; j++)
		{
			if (float.TryParse(array2[j], out result))
			{
				calcAng[j] = result;
			}
		}
	}

	public virtual void Calc()
	{
		for (int i = 0; i < 3; i++)
		{
			if (!float.IsNaN(calcPos[i]))
			{
				endPos[i] = calcPos[i];
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (!float.IsNaN(calcAng[j]))
			{
				endAng[j] = calcAng[j];
			}
		}
	}

	public override void Do()
	{
		base.Do();
		Init(base.scenario);
		Analytics(args, base.scenario);
		Calc();
	}

	public override bool Process()
	{
		base.Process();
		timer = Mathf.Min(timer + Time.deltaTime, time);
		Vector3 position = camT.position;
		Vector3 eulerAngles = camT.eulerAngles;
		float t = Mathf.InverseLerp(0f, time, timer);
		for (int i = 0; i < 3; i++)
		{
			position[i] = Mathf.Lerp(initPos[i], endPos[i], t);
			eulerAngles[i] = Mathf.Lerp(initAng[i], endAng[i], t);
		}
		camT.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));
		return timer >= time;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		camT.SetPositionAndRotation(endPos, Quaternion.Euler(endAng));
	}
}
