using UnityEngine;

namespace ADV.Commands.Game;

public class ColliderSetActive : CommandBase
{
	private bool isEnabled;

	public override string[] ArgsLabel => new string[1] { "isEnabled" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
		isEnabled = bool.Parse(args[0]);
		base.scenario.currentChara.transform.GetComponent<Collider>().enabled = isEnabled;
	}
}
