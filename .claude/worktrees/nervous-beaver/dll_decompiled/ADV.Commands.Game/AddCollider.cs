using UnityEngine;

namespace ADV.Commands.Game;

public class AddCollider : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		GameObject gameObject = base.scenario.currentChara.transform.gameObject;
		CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
		capsuleCollider.center = new Vector3(0f, 0.75f, 0f);
		capsuleCollider.radius = 0.5f;
		capsuleCollider.height = 1.5f;
		capsuleCollider.isTrigger = true;
		gameObject.AddComponent<Rigidbody>();
	}
}
