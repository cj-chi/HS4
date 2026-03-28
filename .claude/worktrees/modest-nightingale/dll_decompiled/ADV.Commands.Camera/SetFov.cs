namespace ADV.Commands.Camera;

public class SetFov : Base
{
	public override string[] ArgsLabel => new string[1] { "Value" };

	public override string[] ArgsDefault => new string[1] { "60" };

	public override void Do()
	{
		base.Do();
		base.scenario.advScene.advCamera.fieldOfView = float.Parse(args[0]);
	}
}
