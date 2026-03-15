namespace ADV.Commands.Base;

public class TaskWait : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
	}

	public override bool Process()
	{
		base.Process();
		return !base.scenario.isBackGroundCommandProcessing;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
	}
}
