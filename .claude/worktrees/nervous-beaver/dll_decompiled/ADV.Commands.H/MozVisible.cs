using System.Linq;
using AIChara;

namespace ADV.Commands.H;

public class MozVisible : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "No", "isVisible" };

	public override string[] ArgsDefault => new string[2]
	{
		"0",
		bool.TrueString
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		string[] argToSplit = GetArgToSplit(num++);
		bool flag = bool.Parse(args[num++]);
		foreach (ChaControl item in from s in argToSplit
			select base.scenario.commandController.GetChara(int.Parse(s)) into p
			select p.chaCtrl)
		{
			item.hideMoz = !flag;
		}
	}
}
