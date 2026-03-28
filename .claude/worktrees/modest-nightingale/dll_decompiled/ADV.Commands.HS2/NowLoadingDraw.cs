using Manager;

namespace ADV.Commands.HS2;

public class NowLoadingDraw : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Visible" };

	public override string[] ArgsDefault => new string[1] { bool.FalseString };

	public override void Do()
	{
		base.Do();
		Scene.DrawLoadingImage(bool.Parse(args[0]));
	}
}
