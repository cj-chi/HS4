using Manager;

namespace ADV.Commands.Effect;

public class SceneFadeWait : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override bool Process()
	{
		base.Process();
		return Scene.sceneFadeCanvas.isEnd;
	}
}
