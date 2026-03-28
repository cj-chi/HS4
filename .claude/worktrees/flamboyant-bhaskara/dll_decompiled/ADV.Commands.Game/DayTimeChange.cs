using Illusion.Extensions;
using Manager;

namespace ADV.Commands.Game;

public class DayTimeChange : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "TimeZoon" };

	public override string[] ArgsDefault => new string[1] { "昼" };

	public override void Do()
	{
		base.Do();
		string value = args[0];
		new string[3] { "昼", "夕方", "夜" }.Check(value);
		BaseMap.sunLightInfo.Set(base.scenario.advScene.advCamera, 1);
		SingletonInitializer<BaseMap>.instance.UpdateCameraFog();
	}
}
