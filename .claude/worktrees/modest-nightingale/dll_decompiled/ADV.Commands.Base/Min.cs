using System.Linq;
using UnityEngine;

namespace ADV.Commands.Base;

public class Min : CommandBase
{
	private string answer;

	public override string[] ArgsLabel => new string[3] { "Answer", "A", "B" };

	public override string[] ArgsDefault => new string[3] { "Answer", "0", "0" };

	public override void ConvertBeforeArgsProc()
	{
		base.ConvertBeforeArgsProc();
		answer = args[0];
	}

	public override void Do()
	{
		base.Do();
		int cnt = 1;
		float num = Mathf.Min((from s in GetArgToSplitLast(cnt)
			select float.Parse(s)).ToArray());
		base.scenario.Vars[answer] = new ValData(num);
	}
}
