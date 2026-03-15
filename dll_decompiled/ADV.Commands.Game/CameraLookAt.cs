namespace ADV.Commands.Game;

public class CameraLookAt : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "X", "Y", "Z" };

	public override string[] ArgsDefault => new string[1] { string.Empty };

	public override void Do()
	{
		base.Do();
		int cnt = 0;
		if (!base.scenario.commandController.GetV3Dic(args[cnt], out var pos))
		{
			CommandBase.CountAddV3(args, ref cnt, ref pos);
		}
		base.scenario.advScene.advCamera.transform.LookAt(pos);
	}
}
