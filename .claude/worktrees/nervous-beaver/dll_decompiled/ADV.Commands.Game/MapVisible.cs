using Manager;

namespace ADV.Commands.Game;

public class MapVisible : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Visible" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
		BaseMap.mapVisibleList.Set(bool.Parse(args[0]));
	}
}
