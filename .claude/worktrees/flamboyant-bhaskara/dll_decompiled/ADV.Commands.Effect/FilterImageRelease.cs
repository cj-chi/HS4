using UnityEngine.UI;

namespace ADV.Commands.Effect;

public class FilterImageRelease : CommandBase
{
	public override string[] ArgsLabel => null;

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		Image filterImage = base.scenario.advScene.filterImage;
		filterImage.enabled = false;
		filterImage.sprite = null;
	}
}
