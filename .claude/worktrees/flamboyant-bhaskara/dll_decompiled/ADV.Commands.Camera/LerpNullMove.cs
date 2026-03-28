using UnityEngine;

namespace ADV.Commands.Camera;

public class LerpNullMove : CommandBase
{
	private float time;

	private float timer;

	private Transform nowT;

	private Vector3 startPos;

	private Vector3 endPos;

	private Vector3 startAngle;

	private Vector3 endAngle;

	public override string[] ArgsLabel => new string[3] { "Time", "Start", "End" };

	public override string[] ArgsDefault => new string[3]
	{
		"0",
		string.Empty,
		string.Empty
	};

	private float NowLerp => Mathf.InverseLerp(0f, time, timer);

	public override void Do()
	{
		base.Do();
		nowT = base.scenario.advScene.advCamera.transform;
		int num = 0;
		time = float.Parse(args[num++]);
		string key = args[num++];
		string text = args[num++];
		base.scenario.commandController.NullTable.TryGetValue(key, out var value);
		Transform value2 = null;
		if (!text.IsNullOrEmpty())
		{
			base.scenario.commandController.NullTable.TryGetValue(text, out value2);
		}
		if (value2 == null)
		{
			startPos = nowT.localPosition;
			startAngle = nowT.localEulerAngles;
			endPos = value.position;
			endAngle = value.eulerAngles;
		}
		else
		{
			startPos = value.position;
			startAngle = value.eulerAngles;
			endPos = value2.position;
			endAngle = value2.eulerAngles;
		}
	}

	public override bool Process()
	{
		timer = Mathf.Min(timer + Time.deltaTime, time);
		SetPosAng(NowLerp);
		return timer >= time;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		if (!processEnd)
		{
			timer = time;
			SetPosAng(NowLerp);
		}
	}

	private void SetPosAng(float t)
	{
		nowT.localPosition = CommandBase.LerpV3(startPos, endPos, t);
		nowT.localEulerAngles = CommandBase.LerpAngleV3(startAngle, endAngle, t);
	}
}
