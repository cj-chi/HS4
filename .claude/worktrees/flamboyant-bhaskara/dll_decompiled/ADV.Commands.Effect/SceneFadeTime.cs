using Manager;

namespace ADV.Commands.Effect;

public class SceneFadeTime : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Time" };

	public override string[] ArgsDefault => new string[1] { string.Empty };

	public override void Do()
	{
		base.Do();
		int num = 0;
		float? num2 = null;
		args.SafeProc(num++, delegate(string s)
		{
			float.Parse(s);
		});
		if (num2.HasValue)
		{
			Scene.sceneFadeCanvas.time = num2.Value;
		}
		else
		{
			Scene.sceneFadeCanvas.DefaultTime();
		}
	}
}
