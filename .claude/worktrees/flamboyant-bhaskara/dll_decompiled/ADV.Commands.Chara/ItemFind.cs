using IllusionUtility.GetUtility;
using UnityEngine;

namespace ADV.Commands.Chara;

public class ItemFind : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "No", "ItemNo", "Name" };

	public override string[] ArgsDefault => new string[3]
	{
		int.MaxValue.ToString(),
		"0",
		"Find"
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		int key = int.Parse(args[num++]);
		string name = args[num++];
		CharaData chara = base.scenario.commandController.GetChara(no);
		Transform transform = chara.chaCtrl.transform.FindLoop(name);
		if (transform != null)
		{
			chara.itemTable[key] = new CharaData.CharaItem(transform.gameObject);
		}
	}
}
