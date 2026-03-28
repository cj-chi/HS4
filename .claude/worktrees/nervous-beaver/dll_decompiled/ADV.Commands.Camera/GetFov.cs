namespace ADV.Commands.Camera;

public class GetFov : Base
{
	public override string[] ArgsLabel => new string[1] { "Variable" };

	public override string[] ArgsDefault => new string[1] { "Fov" };

	public override void Do()
	{
		base.Do();
		base.scenario.Vars[args[0]] = new ValData(base.scenario.advScene.advCamera.fieldOfView);
	}
}
