using UnityEngine;

namespace ADV.Commands.Camera;

public class AnimeRelease : Base
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		UnityEngine.Camera advCamera = base.scenario.advScene.advCamera;
		if (!(advCamera == null))
		{
			UnityEngine.Object.Destroy(advCamera.GetComponent<Animator>());
		}
	}
}
