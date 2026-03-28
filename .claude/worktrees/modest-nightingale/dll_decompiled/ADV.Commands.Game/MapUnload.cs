using Manager;

namespace ADV.Commands.Game;

public class MapUnload : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		Scene.UnloadBaseScene();
	}
}
