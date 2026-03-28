using System.Linq;
using UnityEngine;

namespace ADV.Commands.Camera;

public class Change : Base
{
	public override string[] ArgsLabel => new string[1] { "Name" };

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		UnityEngine.Camera camera = UnityEngine.Object.FindObjectsOfType<UnityEngine.Camera>().FirstOrDefault((UnityEngine.Camera cam) => cam.name == args[0]);
		if (camera != null)
		{
			base.scenario.advScene.SetCamera(camera);
		}
	}
}
