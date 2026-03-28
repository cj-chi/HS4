using System;
using AIChara;
using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Chara;

public class LookNeckTargetChara : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "No", "TargetNo", "Key" };

	public override string[] ArgsDefault => new string[3]
	{
		int.MaxValue.ToString(),
		"0",
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		CharaData chara = base.scenario.commandController.GetChara(int.Parse(args[num++]));
		CharaData chara2 = base.scenario.commandController.GetChara(int.Parse(args[num++]));
		string text = args[num++];
		if (!int.TryParse(text, out var result))
		{
			result = text.Check(ignoreCase: true, Enum.GetNames(typeof(ChaReference.RefObjKey)));
		}
		GameObject referenceInfo = chara2.chaCtrl.GetReferenceInfo((ChaReference.RefObjKey)result);
		chara.chaCtrl.ChangeLookNeckTarget(-1, referenceInfo.transform);
	}
}
