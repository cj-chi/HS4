namespace ADV.Commands.HS2;

public class DOFDefault : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		if (base.scenario.advScene != null)
		{
			base.scenario.advScene.DefaultDOF();
		}
	}
}
