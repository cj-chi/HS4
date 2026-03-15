using AIChara;
using Manager;

namespace ADV.Commands.CameraEffect;

public class DOFTarget : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "No" };

	public override string[] ArgsDefault => new string[1] { int.MaxValue.ToString() };

	public override void Do()
	{
		base.Do();
		int num = 0;
		Singleton<Manager.Game>.Instance.cameraEffector.dof.focalTransform = base.scenario.commandController.GetChara(int.Parse(args[num++])).chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.HeadParent).transform;
	}
}
