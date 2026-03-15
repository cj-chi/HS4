namespace ADV.Commands.Base;

public class WindowImage : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Visible" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
		base.scenario.isWindowImage = bool.Parse(args[0]);
	}
}
