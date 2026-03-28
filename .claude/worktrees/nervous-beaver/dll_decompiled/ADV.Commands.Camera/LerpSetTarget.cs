using UnityEngine;

namespace ADV.Commands.Camera;

public class LerpSetTarget : LerpSet
{
	private Transform target;

	public override string[] ArgsLabel => new string[3] { "Time", "Name", "Dir" };

	public override string[] ArgsDefault => new string[3]
	{
		"0",
		string.Empty,
		"0"
	};

	protected override void Analytics(string[] args, TextScenario scenario)
	{
		int num = 0;
		float.TryParse(args[num++], out time);
		target = GetTarget(args[num++].Split(','));
		if (!(target == null))
		{
			calcPos = target.position;
			calcAng = target.eulerAngles;
		}
	}
}
