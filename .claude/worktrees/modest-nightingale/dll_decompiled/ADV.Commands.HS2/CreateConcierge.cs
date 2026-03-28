using AIChara;
using Actor;
using Illusion.Anime;
using Manager;

namespace ADV.Commands.HS2;

public class CreateConcierge : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "No" };

	public override string[] ArgsDefault => new string[1] { int.MaxValue.ToString() };

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		CharaData charaData = new CharaData(new TextScenario.ParamData(new Heroine(isRandomize: false)), base.scenario, isParent: true);
		charaData.data.Create(base.scenario.commandController.Character.gameObject, isLoad: false);
		ChaControl chaCtrl = charaData.chaCtrl;
		Singleton<Character>.Instance.LoadConciergeCharaFile(chaCtrl);
		chaCtrl.ChangeNowCoordinate();
		chaCtrl.Load();
		new Controller(chaCtrl);
		base.scenario.commandController.AddChara(no, charaData);
	}
}
