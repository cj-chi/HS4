using System;
using AIChara;
using Illusion.Extensions;

namespace ADV.Commands.Chara;

public class SiruState : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "No", "Parts", "State" };

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
			result = text.Check(ignoreCase: true, Enum.GetNames(typeof(ChaFileDefine.SiruParts)));
		}
		string text2 = args[num++];
		if (!int.TryParse(text2, out var result2))
		{
			result2 = text2.Check(true, "なし", "少ない", "多い");
		}
		base.scenario.commandController.GetChara(no).chaCtrl.SetSiruFlag((ChaFileDefine.SiruParts)result, (byte)result2);
	}
}
