namespace ADV.Commands.CameraEffect;

public class SepiaEffect : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "isActive" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
	}
}
