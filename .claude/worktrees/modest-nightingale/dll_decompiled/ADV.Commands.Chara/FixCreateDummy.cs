using System.IO;
using Actor;
using Manager;
using UnityEngine;

namespace ADV.Commands.Chara;

public class FixCreateDummy : CommandBase
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
		string text = args[num++];
		Heroine heroine = null;
		if (!int.TryParse(text, out var fixID))
		{
			string text2 = args[num++];
			if (!Path.HasExtension(text))
			{
				text += AssetBundleManager.Extension;
			}
			fixID = int.Parse(Path.GetFileNameWithoutExtension(text2).Substring(1));
			heroine = new Heroine(isRandomize: false);
			AssetBundleManager.LoadAsset(text, text2, typeof(TextAsset)).GetAsset<TextAsset>();
			AssetBundleManager.UnloadAssetBundle(text, isUnloadForceRefCount: false);
		}
		else
		{
			heroine = Singleton<Manager.Game>.Instance.heroineList.Find((Heroine p) => p.fixCharaID == fixID);
		}
		base.scenario.commandController.AddChara(no, new CharaData(new TextScenario.ParamData(heroine), base.scenario, isParent: true));
	}
}
