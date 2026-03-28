namespace ADV.Commands.Camera;

public class Lock : Base
{
	public override string[] ArgsLabel => new string[1] { "isLock" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
		if (!(base.scenario.advScene == null))
		{
			base.scenario.advScene.isCameraLock = bool.Parse(args[0]);
		}
	}
}
