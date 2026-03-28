using System.Collections.Generic;
using System.Linq;
using ADV.Commands.Base;

namespace ADV.Commands.Chara;

public class Motion : ADV.Commands.Base.Motion
{
	public override void Do()
	{
		IReadOnlyCollection<Data> readOnlyCollection = ADV.Commands.Base.Motion.Convert(ref args, base.scenario, ArgsLabel.Length);
		if (readOnlyCollection.Any())
		{
			base.scenario.CrossFadeStart();
		}
		foreach (Data item in readOnlyCollection)
		{
			item.Play(base.scenario);
		}
	}
}
