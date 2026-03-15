using Illusion.Extensions;
using Manager;

namespace ADV.Commands.Effect;

public class SceneFade : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Fade" };

	public override string[] ArgsDefault => new string[1] { "in" };

	public override void Do()
	{
		base.Do();
		int num = 0;
		FadeCanvas.Fade fade = (args[num++].Compare("in", ignoreCase: true) ? FadeCanvas.Fade.In : FadeCanvas.Fade.Out);
		Scene.sceneFadeCanvas.StartFade(fade);
	}

	public override bool Process()
	{
		base.Process();
		return Scene.sceneFadeCanvas.isEnd;
	}
}
