using Manager;

namespace ADV.Commands.Chara;

public class VoiceStopAll : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		Voice.StopAll();
	}
}
