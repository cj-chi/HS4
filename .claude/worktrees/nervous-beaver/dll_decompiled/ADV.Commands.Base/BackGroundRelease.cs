namespace ADV.Commands.Base;

public class BackGroundRelease : CommandBase
{
	public override string[] ArgsLabel => new string[1] { "isActive" };

	public override string[] ArgsDefault => new string[1] { bool.FalseString };

	public override void Do()
	{
		base.Do();
		if (base.scenario.advScene == null)
		{
			return;
		}
		bool flag = bool.Parse(args[0]);
		base.scenario.advScene.BGParam.visibleAll = flag;
		if (base.scenario.advScene.advCamera != null)
		{
			CameraEffectorColorMask componentInChildren = base.scenario.advScene.advCamera.GetComponentInChildren<CameraEffectorColorMask>();
			if (componentInChildren != null)
			{
				componentInChildren.Enabled = flag;
			}
		}
		base.scenario.advScene.BGParam.Release(flag);
	}
}
