namespace ADV.Commands.Base;

public class Task : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "isTask" };

	public override string[] ArgsDefault => new string[1] { bool.FalseString };

	public override void Do()
	{
		base.Do();
		base.scenario.isBackGroundCommanding = bool.Parse(args[0]);
	}
}
