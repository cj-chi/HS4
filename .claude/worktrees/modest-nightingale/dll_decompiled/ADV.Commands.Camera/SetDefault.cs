using UnityEngine;

namespace ADV.Commands.Camera;

public class SetDefault : Base
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		if (!(base.scenario.advScene == null))
		{
			UnityEngine.Camera advCamera = base.scenario.advScene.advCamera;
			UnityEngine.Camera backCamera = base.scenario.advScene.BackCamera;
			if (advCamera != null && backCamera != null)
			{
				Transform transform = backCamera.transform;
				advCamera.transform.SetPositionAndRotation(transform.position, transform.rotation);
				advCamera.fieldOfView = backCamera.fieldOfView;
			}
		}
	}
}
