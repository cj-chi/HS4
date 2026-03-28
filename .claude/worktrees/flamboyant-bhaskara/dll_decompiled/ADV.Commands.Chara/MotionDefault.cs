namespace ADV.Commands.Chara;

public class MotionDefault : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "No" };

	public override string[] ArgsDefault => new string[3]
	{
		int.MaxValue.ToString(),
		"0",
		"1"
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		base.scenario.commandController.GetChara(no).animeController.ResetDefaultAnimatorController();
	}
}
