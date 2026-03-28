namespace ADV.Commands.Chara;

public class ItemAnime : CommandBase
{
	public override string[] ArgsLabel => new string[5] { "No", "ItemNo", "Bundle", "Asset", "State" };

	public override string[] ArgsDefault => new string[5]
	{
		int.MaxValue.ToString(),
		"0",
		string.Empty,
		string.Empty,
		"Idle"
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		int key = int.Parse(args[num++]);
		string bundle = args[num++];
		string asset = args[num++];
		string state = args[num++];
		base.scenario.commandController.GetChara(no).itemTable[key].LoadAnimator(bundle, asset, state);
	}
}
