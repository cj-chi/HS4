using Manager;

namespace ADV.Commands.Chara;

public class VoiceWaitAll : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override bool Process()
	{
		base.Process();
		return !Voice.IsPlay();
	}
}
