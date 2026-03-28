using Manager;

namespace ADV.Commands.HS2;

public class DOFAperture : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Value" };

	public override string[] ArgsDefault => new string[1] { "0" };

	public override void Do()
	{
		base.Do();
		int num = 0;
		Singleton<Manager.Game>.Instance.cameraEffector.dof.aperture = float.Parse(args[num++]);
	}
}
