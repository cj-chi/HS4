using System.IO;
using System.Linq;
using Actor;
using Manager;
using UnityEngine;

namespace ADV.Commands.Chara;

public class MobCreateDummy : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "No", "Bundle", "Asset" };

	public override string[] ArgsDefault => new string[3]
	{
		"-100",
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		string text = args[num++].ToLower();
		string assetName = args[num++];
		if (!Path.HasExtension(text))
		{
			text += AssetBundleManager.Extension;
		}
		Heroine param = new Heroine(isRandomize: false);
		AssetBundleManager.LoadAsset(text, assetName, typeof(TextAsset)).GetAsset<TextAsset>();
		Voice.infoTable.Values.FirstOrDefault((VoiceInfo.Param p) => p.Personality == "モブ");
		AssetBundleManager.UnloadAssetBundle(text, isUnloadForceRefCount: false);
		base.scenario.commandController.AddChara(no, new CharaData(new TextScenario.ParamData(param), base.scenario, isParent: true));
	}
}
