using UnityEngine;

namespace ADV.Commands.Camera;

public class Active : Base
{
	public override string[] ArgsLabel => new string[1] { "isActive" };

	public override string[] ArgsDefault => new string[1] { bool.TrueString };

	public override void Do()
	{
		base.Do();
		if (!(base.scenario.advScene == null))
		{
			UnityEngine.Camera advCamera = base.scenario.advScene.advCamera;
			if (!(advCamera == null))
			{
				int num = 0;
				bool active = bool.Parse(args[num++]);
				advCamera.gameObject.SetActive(active);
			}
		}
	}
}
