using Illusion.Extensions;

namespace ADV.Commands.Effect;

public class Fade : CommandBase
{
	private SceneFadeCanvas fade;

	public override string[] ArgsLabel => new string[3] { "Fade", "Time", "Type" };

	public override string[] ArgsDefault => new string[3] { "in", "0.5f", "back" };

	public override void Do()
	{
		base.Do();
		int num = 0;
		FadeCanvas.Fade fade = (args[num++].Compare("in", ignoreCase: true) ? FadeCanvas.Fade.In : FadeCanvas.Fade.Out);
		float time = float.Parse(args[num++]);
		if (args[num++].Compare("front", ignoreCase: true))
		{
			this.fade = base.scenario.advScene.fadeFront;
		}
		else
		{
			this.fade = base.scenario.advScene.fadeBack;
		}
		this.fade.time = time;
		this.fade.StartFade(fade);
	}

	public override bool Process()
	{
		base.Process();
		return fade.isEnd;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		if (!processEnd)
		{
			fade.Cancel();
		}
	}
}
