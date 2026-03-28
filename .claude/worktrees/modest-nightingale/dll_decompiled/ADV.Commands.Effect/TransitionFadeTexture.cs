using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Effect;

public class TransitionFadeTexture : CommandBase
{
	public override string[] ArgsLabel => new string[4] { "Bundle", "Asset", "Type", "isMain" };

	public override string[] ArgsDefault => new string[4]
	{
		string.Empty,
		string.Empty,
		string.Empty,
		bool.FalseString
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		string text = args[num++];
		string text2 = args[num++];
		EMTransition eMTransition = ((!args[num++].Compare("front", ignoreCase: true)) ? base.scenario.advScene.fadeBackTransition : base.scenario.advScene.fadeFrontTransition);
		int num2;
		Texture2D texture2D;
		if (!text.IsNullOrEmpty())
		{
			num2 = (text2.IsNullOrEmpty() ? 1 : 0);
			if (num2 == 0)
			{
				texture2D = AssetBundleManager.LoadAsset(text, text2, typeof(Texture2D)).GetAsset<Texture2D>();
				goto IL_0091;
			}
		}
		else
		{
			num2 = 1;
		}
		texture2D = null;
		goto IL_0091;
		IL_0091:
		if (bool.Parse(args[num++]))
		{
			eMTransition.SetTexture(texture2D);
		}
		else
		{
			eMTransition.SetGradationTexture(texture2D);
		}
		if (num2 == 0)
		{
			AssetBundleManager.UnloadAssetBundle(text, isUnloadForceRefCount: false);
		}
	}
}
