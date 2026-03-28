using Illusion.Extensions;
using Manager;
using UnityEngine;

namespace ADV.Commands.Effect;

public class SceneFadeColor : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Color" };

	public override string[] ArgsDefault => new string[1] { string.Empty };

	public override void Do()
	{
		base.Do();
		int num = 0;
		Color? color = null;
		args.SafeProc(num++, delegate(string s)
		{
			color = s.GetColorCheck();
		});
		if (color.HasValue)
		{
			Scene.sceneFadeCanvas.SetColor(color.Value);
		}
		else
		{
			Scene.sceneFadeCanvas.DefaultColor();
		}
	}
}
