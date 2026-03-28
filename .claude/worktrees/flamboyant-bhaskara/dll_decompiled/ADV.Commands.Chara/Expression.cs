using ADV.Commands.Base;

namespace ADV.Commands.Chara;

public class Expression : ADV.Commands.Base.Expression
{
	public override void Do()
	{
		foreach (Data item in ADV.Commands.Base.Expression.Convert(ref args, base.scenario))
		{
			item.Play(base.scenario);
		}
	}
}
