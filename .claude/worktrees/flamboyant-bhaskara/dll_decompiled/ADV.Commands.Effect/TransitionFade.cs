using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Effect;

public class TransitionFade : CommandBase
{
	private EMTransition emtFade;

	private FadeCanvas.Fade fadeType;

	public override string[] ArgsLabel => new string[4] { "Fade", "Time", "Color", "Type" };

	public override string[] ArgsDefault => new string[4]
	{
		"in",
		"0.5f",
		string.Empty,
		string.Empty
	};

	private bool fadeIn => fadeType == FadeCanvas.Fade.In;

	public override void Do()
	{
		base.Do();
		int num = 0;
		if (args[num++].Compare("in", ignoreCase: true))
		{
			fadeType = FadeCanvas.Fade.In;
		}
		else
		{
			fadeType = FadeCanvas.Fade.Out;
		}
		float duration = float.Parse(args[num++]);
		Color? color = null;
		string self = args[num++];
		if (!self.IsNullOrEmpty())
		{
			color = self.GetColorCheck();
		}
		if (args[num++].Compare("front", ignoreCase: true))
		{
			emtFade = base.scenario.advScene.fadeFrontTransition;
		}
		else
		{
			emtFade = base.scenario.advScene.fadeBackTransition;
		}
		if (color.HasValue)
		{
			emtFade.SetColor(color.Value);
		}
		emtFade.duration = duration;
		bool flag = !(emtFade.curve.Evaluate(0f) > 0.5f);
		if ((fadeIn && !flag) || (!fadeIn && flag))
		{
			emtFade.FlipAnimationCurve();
		}
		emtFade.Play();
	}

	public override bool Process()
	{
		base.Process();
		if (!fadeIn)
		{
			return emtFade.threshold <= 0f;
		}
		return emtFade.threshold >= 1f;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		if (!processEnd)
		{
			emtFade.Stop();
			emtFade.Set(1f);
		}
	}
}
