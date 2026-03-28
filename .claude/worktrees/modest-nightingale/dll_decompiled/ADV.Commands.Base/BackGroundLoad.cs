namespace ADV.Commands.Base;

public class BackGroundLoad : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "Bundle", "Asset" };

	public override string[] ArgsDefault => new string[2]
	{
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		if (base.scenario.advScene == null)
		{
			return;
		}
		int num = 0;
		string text = args[num++];
		string assetName = args[num++];
		base.scenario.advScene.BGParam.Load(text, assetName);
		if (text.IsNullOrEmpty())
		{
			return;
		}
		base.scenario.advScene.BGParam.visibleAll = true;
		if (base.scenario.advScene.advCamera != null)
		{
			CameraEffectorColorMask componentInChildren = base.scenario.advScene.advCamera.GetComponentInChildren<CameraEffectorColorMask>();
			if (componentInChildren != null)
			{
				componentInChildren.Enabled = true;
			}
		}
	}
}
