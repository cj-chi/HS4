using UnityEngine;

namespace ADV.Commands.Chara;

public class AddPosition : CommandBase
{
	public override string[] ArgsLabel => new string[7] { "No", "X", "Y", "Z", "Pitch", "Yaw", "Roll" };

	public override string[] ArgsDefault => new string[1] { int.MaxValue.ToString() };

	public override void Do()
	{
		base.Do();
		int cnt = 0;
		int no = int.Parse(args[cnt++]);
		Transform transform = base.scenario.commandController.GetChara(no).transform;
		if (!base.scenario.commandController.GetV3Dic(args.SafeGet(cnt), out var pos))
		{
			CommandBase.CountAddV3(args, ref cnt, ref pos);
		}
		else
		{
			CommandBase.CountAddV3(ref cnt);
		}
		transform.position += pos;
		if (!base.scenario.commandController.GetV3Dic(args.SafeGet(cnt), out var pos2))
		{
			CommandBase.CountAddV3(args, ref cnt, ref pos2);
		}
		else
		{
			CommandBase.CountAddV3(ref cnt);
		}
		transform.eulerAngles += pos2;
	}
}
