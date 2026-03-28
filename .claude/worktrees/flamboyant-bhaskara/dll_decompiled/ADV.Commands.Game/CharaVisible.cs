using System;
using AIChara;
using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Game;

public class CharaVisible : CommandBase
{
	private enum Target
	{
		All,
		Head,
		Body,
		Son,
		Gomu
	}

	public override string[] ArgsLabel => new string[4] { "No", "Target", "isActive", "Stand" };

	public override string[] ArgsDefault => new string[4]
	{
		int.MaxValue.ToString(),
		Target.All.ToString(),
		bool.TrueString,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		CharaData chara = base.scenario.commandController.GetChara(no);
		ChaControl chaCtrl = chara.chaCtrl;
		int num2 = args[num++].Check(ignoreCase: true, Enum.GetNames(typeof(Target)));
		bool flag = bool.Parse(args[num++]);
		args.SafeProc(num++, delegate(string findName)
		{
			Transform transform = base.scenario.commandController.characterStandNulls[findName];
			chara.transform.SetPositionAndRotation(transform.position, transform.rotation);
		});
		ChaFileStatus fileStatus = chaCtrl.fileStatus;
		switch ((Target)num2)
		{
		case Target.All:
			chaCtrl.visibleAll = flag;
			break;
		case Target.Head:
			fileStatus.visibleHeadAlways = flag;
			break;
		case Target.Body:
			fileStatus.visibleBodyAlways = flag;
			break;
		case Target.Son:
			chaCtrl.visibleSon = flag;
			break;
		case Target.Gomu:
			fileStatus.visibleGomu = flag;
			break;
		}
	}
}
