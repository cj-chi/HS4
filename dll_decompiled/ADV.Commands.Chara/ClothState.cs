using System;
using AIChara;
using Illusion.Extensions;

namespace ADV.Commands.Chara;

public class ClothState : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "No", "Kind", "State" };

	public override string[] ArgsDefault => new string[3]
	{
		"0",
		ChaFileDefine.ClothesKind.top.ToString(),
		"0"
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		string text = args[num++];
		if (!int.TryParse(text, out var result))
		{
			result = text.Check(ignoreCase: true, Enum.GetNames(typeof(ChaFileDefine.ClothesKind)));
		}
		int num2 = int.Parse(args[num++]);
		base.scenario.commandController.GetChara(no).chaCtrl.SetClothesState(result, (byte)num2);
	}
}
