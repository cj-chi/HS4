namespace ADV.Commands.HS2;

public class UseCorrectCamera : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Value" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
		int num = 0;
		base.scenario.commandController.useCorrectCamera = bool.Parse(args[num++]);
	}
}
