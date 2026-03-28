namespace ADV.Commands.Chara;

public class Create : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "No", "ReadNo" };

	public override string[] ArgsDefault => new string[2]
	{
		int.MaxValue.ToString(),
		"-2"
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		int num2 = int.Parse(args[num++]);
		base.scenario.commandController.GetChara(num2);
		if (num2 != -1)
		{
			base.scenario.ChangeCurrentChara(no);
		}
	}
}
