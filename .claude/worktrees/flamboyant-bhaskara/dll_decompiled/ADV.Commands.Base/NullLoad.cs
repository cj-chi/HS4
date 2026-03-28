using System.Collections.Generic;
using Manager;
using UnityEngine;

namespace ADV.Commands.Base;

public class NullLoad : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "Version", "Name" };

	public override string[] ArgsDefault => new string[2]
	{
		string.Empty,
		"デフォルト"
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int? version = null;
		args.SafeProc(num++, delegate(string s)
		{
			version = int.Parse(s);
		});
		string text = args[num++];
		text = ((!(text == "デフォルト")) ? BaseMap.GetParam(text).AssetName.ToLower() : "default");
		base.scenario.commandController.ReleaseNull();
		if (version.HasValue)
		{
			string text2 = $"{AssetBundleNames.MapAdvposPath}{version:00}/{text}{AssetBundleManager.Extension}";
			LoadCloneNull(text2);
			AssetBundleManager.UnloadAssetBundle(text2, isUnloadForceRefCount: false);
			return;
		}
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(AssetBundleNames.MapAdvposPath, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			LoadCloneNull(item);
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		void LoadCloneNull(string bundle)
		{
			GameObject[] allAssets = AssetBundleManager.LoadAllAsset(bundle, typeof(GameObject)).GetAllAssets<GameObject>();
			foreach (GameObject gameObject in allAssets)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
				gameObject2.name = gameObject.name;
				base.scenario.commandController.SetNull(gameObject2.transform);
			}
		}
	}
}
