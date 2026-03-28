using UnityEngine;

namespace ADV.Commands.Camera;

public class AddRotation : Base
{
	public override string[] ArgsLabel => new string[4] { "isLocal", "Pitch", "Yaw", "Roll" };

	public override string[] ArgsDefault => new string[2]
	{
		bool.FalseString,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		UnityEngine.Camera advCamera = base.scenario.advScene.advCamera;
		if (!(advCamera == null))
		{
			int cnt = 0;
			bool num = bool.Parse(args[cnt++]);
			Vector3 pos = (num ? advCamera.transform.localEulerAngles : advCamera.transform.eulerAngles);
			if (!base.scenario.commandController.GetV3Dic(args[cnt], out pos))
			{
				CommandBase.CountAddV3(args, ref cnt, ref pos);
			}
			if (num)
			{
				pos += advCamera.transform.localEulerAngles;
				advCamera.transform.localEulerAngles = pos;
			}
			else
			{
				pos += advCamera.transform.eulerAngles;
				advCamera.transform.eulerAngles = pos;
			}
		}
	}
}
