using UnityEngine;

namespace ADV.Commands.Object;

public class Delete : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Name" };

	public override string[] ArgsDefault => new string[1] { string.Empty };

	public override void Do()
	{
		base.Do();
		string key = args[0];
		UnityEngine.Object.Destroy(base.scenario.commandController.Objects[key]);
		base.scenario.commandController.Objects.Remove(key);
	}
}
