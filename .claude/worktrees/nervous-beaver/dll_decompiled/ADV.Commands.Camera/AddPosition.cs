using UnityEngine;

namespace ADV.Commands.Camera;

public class AddPosition : Base
{
	public override string[] ArgsLabel => new string[4] { "isLocal", "X", "Y", "Z" };

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
			if (!base.scenario.commandController.GetV3Dic(args[cnt], out var pos))
			{
				CommandBase.CountAddV3(args, ref cnt, ref pos);
			}
			pos = advCamera.transform.TransformDirection(pos);
			if (num)
			{
				pos += advCamera.transform.localPosition;
				advCamera.transform.localPosition = pos;
			}
			else
			{
				pos += advCamera.transform.position;
				advCamera.transform.position = pos;
			}
		}
	}
}
