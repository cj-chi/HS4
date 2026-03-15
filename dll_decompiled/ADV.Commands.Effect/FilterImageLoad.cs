using Illusion.Game;
using UnityEngine.UI;

namespace ADV.Commands.Effect;

public class FilterImageLoad : CommandBase
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
		int num = 0;
		string assetBundleName = args[num++];
		string assetName = args[num++];
		Image filterImage = base.scenario.advScene.filterImage;
		Utils.Bundle.LoadSprite(assetBundleName, assetName, filterImage, isTexSize: false);
		filterImage.enabled = true;
	}
}
