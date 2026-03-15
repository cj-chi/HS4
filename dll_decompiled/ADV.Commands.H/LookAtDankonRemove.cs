using UnityEngine;

namespace ADV.Commands.H;

public class LookAtDankonRemove : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		UnityEngine.Object.Destroy(base.scenario.GetComponent<H_Lookat_dan>());
	}
}
