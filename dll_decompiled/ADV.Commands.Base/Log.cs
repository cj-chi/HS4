namespace ADV.Commands.Base;

public class Log : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "Type", "Msg" };

	public override string[] ArgsDefault => new string[2]
	{
		"Log",
		string.Empty
	};

	public override void Do()
	{
		base.Do();
	}
}
