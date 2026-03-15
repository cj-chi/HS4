namespace ADV.Commands.EventCG;

public class Release : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "isMotionContinue" };

	public override string[] ArgsDefault => new string[1] { bool.FalseString };

	public override void Do()
	{
		base.Do();
		if (!Common.Release(base.scenario))
		{
			return;
		}
		bool isMotionContinue = false;
		args.SafeProc(0, delegate(string s)
		{
			isMotionContinue = bool.Parse(s);
		});
		if (!isMotionContinue)
		{
			foreach (CharaData value in base.scenario.commandController.Characters.Values)
			{
				if (!(value.chaCtrl == null) && value.chaCtrl.loadEnd)
				{
					value.animeController.ResetDefaultAnimatorController();
				}
			}
		}
		base.scenario.commandController.useCorrectCamera = true;
	}
}
