using UnityEngine;

namespace ADV.Commands.Camera;

public class SetPosition : Base
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
			Vector3 v = (num ? advCamera.transform.localPosition : advCamera.transform.position);
			if (base.scenario.commandController.GetV3Dic(args[cnt], out var pos))
			{
				v = pos;
			}
			else
			{
				CommandBase.CountAddV3(args, ref cnt, ref v);
			}
			if (num)
			{
				advCamera.transform.localPosition = v;
			}
			else
			{
				advCamera.transform.position = v;
			}
		}
	}
}
