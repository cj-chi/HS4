using System.Linq;
using Actor;
using Manager;

namespace ADV.Commands.Chara;

public class CreateEmpty : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "No" };

	public override string[] ArgsDefault => new string[1] { "-100" };

	public override void Do()
	{
		base.Do();
		Heroine heroine = new Heroine(isRandomize: false);
		VoiceInfo.Param param = Voice.infoTable.Values.FirstOrDefault((VoiceInfo.Param p) => p.Personality == "モブ");
		heroine.fixCharaID = param.No;
		int num = 0;
		int no = int.Parse(args[num++]);
		base.scenario.commandController.AddChara(no, new CharaData(new TextScenario.ParamData(heroine), base.scenario, isParent: true));
	}
}
