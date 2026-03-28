namespace ADV.Commands.Camera;

public class Aspect : Base
{
	public override string[] ArgsLabel => new string[1] { "isAspect" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
		if (!(base.scenario.advScene == null))
		{
			base.scenario.advScene.isAspect = bool.Parse(args[0]);
		}
	}
}
