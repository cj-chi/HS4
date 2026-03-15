using Manager;
using UnityEngine;

namespace ADV.Commands.CameraEffect;

public class DOFTargetSet : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "Value" };

	public override string[] ArgsDefault => new string[1] { "0" };

	public override void Do()
	{
		base.Do();
		int num = 0;
		Transform focalTransform = Singleton<Manager.Game>.Instance.cameraEffector.dof.focalTransform;
		Vector3 localPosition = focalTransform.localPosition;
		localPosition.z = float.Parse(args[num++]);
		focalTransform.localPosition = localPosition;
	}
}
