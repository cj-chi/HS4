using System;
using ADV.EventCG;
using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.EventCG;

public class Setting : CommandBase
{
	private Data data;

	public override string[] ArgsLabel => new string[3] { "Bundle", "Asset", "No" };

	public override string[] ArgsDefault => new string[3]
	{
		string.Empty,
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		Common.Release(base.scenario);
		int num = 0;
		string bundle = args[num++];
		string asset = args[num++];
		int? no = null;
		args.SafeProc(num++, delegate(string s)
		{
			no = int.Parse(s);
		});
		Action action = delegate
		{
			GameObject asset2 = AssetBundleManager.LoadAsset(bundle, asset, typeof(GameObject)).GetAsset<GameObject>();
			data = UnityEngine.Object.Instantiate(asset2, base.scenario.commandController.EventCGRoot, worldPositionStays: false).GetComponent<Data>();
			if (no.HasValue)
			{
				Transform transform = data.transform;
				Transform transform2 = base.scenario.commandController.GetChara(no.Value).backup.transform;
				transform.position += transform2.position;
				transform.rotation *= transform2.rotation;
			}
			data.name = asset2.name;
			AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: false);
		};
		if (!bundle.IsNullOrEmpty())
		{
			action();
		}
		else
		{
			foreach (string item in CommonLib.GetAssetBundleNameListFromPath("adv/eventcg/", subdirCheck: true))
			{
				if (asset.Check(ignoreCase: true, AssetBundleCheck.GetAllAssetName(item, _WithExtension: false)) != -1)
				{
					bundle = item;
					action();
					break;
				}
			}
		}
		base.scenario.commandController.useCorrectCamera = false;
		data.camRoot = base.scenario.advScene.advCamera.transform;
		CommandController commandController = base.scenario.commandController;
		data.SetChaRoot(commandController.Character, commandController.Characters);
		data.Next(0, commandController.Characters);
	}
}
