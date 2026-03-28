using Illusion;
using Illusion.Extensions;
using Manager;

namespace ADV.Commands.Game;

public class MapChange : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "No", "Fade" };

	public override string[] ArgsDefault => new string[2]
	{
		"0",
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		string text = args[num++];
		if (!int.TryParse(text, out var result))
		{
			result = BaseMap.ConvertMapNo(text);
		}
		FadeCanvas.Fade fadeType = FadeCanvas.Fade.InOut;
		string text2 = args[num++];
		if (!text2.IsNullOrEmpty())
		{
			bool result2;
			if (text2.Check(ignoreCase: true, Utils.Enum<FadeCanvas.Fade>.Names) != -1)
			{
				fadeType = Utils.Enum<FadeCanvas.Fade>.Cast(text2);
			}
			else if (bool.TryParse(text2, out result2) && !result2)
			{
				fadeType = FadeCanvas.Fade.None;
			}
		}
		BaseMap.Change(result, fadeType);
	}
}
