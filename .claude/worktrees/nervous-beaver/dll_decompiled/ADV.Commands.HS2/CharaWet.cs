namespace ADV.Commands.HS2;

public class CharaWet : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "No", "isWet" };

	public override string[] ArgsDefault => new string[2]
	{
		int.MaxValue.ToString(),
		bool.FalseString
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		base.scenario.commandController.GetChara(no).chaCtrl.wetRate = (bool.Parse(args[num++]) ? 1 : 0);
	}
}
