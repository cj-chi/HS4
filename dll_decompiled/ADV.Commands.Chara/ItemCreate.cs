using IllusionUtility.GetUtility;
using UnityEngine;

namespace ADV.Commands.Chara;

public class ItemCreate : CommandBase
{
	public override string[] ArgsLabel => new string[9] { "No", "ItemNo", "Bundle", "Asset", "Root", "isWorldPositionStays", "Manifest", "Pos", "Ang" };

	public override string[] ArgsDefault => new string[9]
	{
		int.MaxValue.ToString(),
		"0",
		string.Empty,
		string.Empty,
		string.Empty,
		bool.FalseString,
		string.Empty,
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		int key = int.Parse(args[num++]);
		string bundle = args[num++];
		string asset = args[num++];
		string text = args[num++];
		bool worldPositionStays = bool.Parse(args[num++]);
		string manifest = args.SafeGet(num++);
		string text2 = args.SafeGet(num++);
		string text3 = args.SafeGet(num++);
		CharaData chara = base.scenario.commandController.GetChara(no);
		Transform transform = null;
		if (!text.IsNullOrEmpty() && chara.chaCtrl != null)
		{
			transform = chara.chaCtrl.transform.FindLoop(text);
		}
		if (transform == null)
		{
			transform = base.scenario.advScene.transform;
		}
		if (chara.itemTable.TryGetValue(key, out var value))
		{
			value.Delete();
		}
		value = new CharaData.CharaItem();
		value.LoadObject(bundle, asset, transform, worldPositionStays, manifest);
		if (!base.scenario.commandController.GetV3Dic(text2, out var pos))
		{
			int cnt = 0;
			CommandBase.CountAddV3(text2.Split(','), ref cnt, ref pos);
		}
		value.item.transform.localPosition += pos;
		if (!base.scenario.commandController.GetV3Dic(text3, out pos))
		{
			int cnt2 = 0;
			CommandBase.CountAddV3(text3.Split(','), ref cnt2, ref pos);
		}
		value.item.transform.localEulerAngles += pos;
		chara.itemTable[key] = value;
	}
}
