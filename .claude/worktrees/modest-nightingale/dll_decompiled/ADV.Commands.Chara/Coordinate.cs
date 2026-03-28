namespace ADV.Commands.Chara;

public class Coordinate : CommandBase
{
	private int no;

	public override string[] ArgsLabel => new string[1] { "No" };

	public override string[] ArgsDefault => new string[1] { "0" };

	public override void Do()
	{
		base.Do();
		int num = 0;
		no = int.Parse(args[num++]);
		base.scenario.commandController.GetChara(no).chaCtrl.ChangeNowCoordinate(reload: true);
	}
}
