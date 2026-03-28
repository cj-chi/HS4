using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Effect;

public class FadeColor : CommandBase
{
	private SceneFadeCanvas fade;

	public override string[] ArgsLabel => new string[2] { "Color", "Type" };

	public override string[] ArgsDefault => new string[2]
	{
		string.Empty,
		"back"
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		Color? color = null;
		args.SafeProc(num++, delegate(string s)
		{
			color = s.GetColorCheck();
		});
		if (args[num++].Compare("front", ignoreCase: true))
		{
			fade = base.scenario.advScene.fadeFront;
		}
		else
		{
			fade = base.scenario.advScene.fadeBack;
		}
		if (color.HasValue)
		{
			fade.SetColor(color.Value);
		}
		else
		{
			fade.DefaultColor();
		}
	}
}
