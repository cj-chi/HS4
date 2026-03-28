using UnityEngine;

namespace ADV.Commands.Camera;

public class LerpAddTarget : LerpAdd
{
	private Transform target;

	public override string[] ArgsLabel => new string[5] { "Time", "Name", "X,Y,Z", "Pitch,Yaw,Roll", "Dir" };

	public override string[] ArgsDefault => null;

	protected override void Analytics(string[] args, TextScenario scenario)
	{
		int num = 0;
		float.TryParse(args[num++], out time);
		target = GetTarget(args[num++].Split(','));
		string obj = args[num++];
		string text = args[num++];
		_ = args[num++];
		string[] array = obj.Split(',');
		for (int i = 0; i < array.Length && i < 3; i++)
		{
			if (float.TryParse(array[i], out var result))
			{
				calcPos[i] = result;
			}
		}
		string[] array2 = text.Split(',');
		for (int j = 0; j < array2.Length && j < 3; j++)
		{
			if (float.TryParse(array2[j], out var result2))
			{
				calcAng[j] = result2;
			}
		}
		if (!(target == null))
		{
			endPos = (initPos = target.position);
			endAng = (initAng = target.eulerAngles);
		}
	}
}
