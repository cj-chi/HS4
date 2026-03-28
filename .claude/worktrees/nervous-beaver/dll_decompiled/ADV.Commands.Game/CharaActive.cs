using UnityEngine;

namespace ADV.Commands.Game;

public class CharaActive : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "No", "isActive", "Stand" };

	public override string[] ArgsDefault => new string[3]
	{
		int.MaxValue.ToString(),
		bool.TrueString,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		CharaData chara = base.scenario.commandController.GetChara(no);
		bool visibleAll = bool.Parse(args[num++]);
		args.SafeProc(num++, delegate(string findName)
		{
			Transform transform = base.scenario.commandController.characterStandNulls[findName];
			chara.transform.SetPositionAndRotation(transform.position, transform.rotation);
		});
		if (0 == 0)
		{
			chara.chaCtrl.visibleAll = visibleAll;
		}
	}
}
