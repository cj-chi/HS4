using UnityEngine;

namespace ADV.Commands.Camera;

public class AnimeLoad : Base
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
		string bundle = args[num++];
		string asset = args[num++];
		AssetBundleData assetBundleData = new AssetBundleData(bundle, asset);
		RuntimeAnimatorController asset2 = assetBundleData.GetAsset<RuntimeAnimatorController>();
		if (asset2 != null)
		{
			base.scenario.advScene.advCamera.GetOrAddComponent<Animator>().runtimeAnimatorController = asset2;
		}
		assetBundleData.UnloadBundle();
	}
}
