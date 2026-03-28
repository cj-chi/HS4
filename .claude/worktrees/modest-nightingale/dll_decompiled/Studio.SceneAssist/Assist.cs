using Manager;
using UnityEngine;

namespace Studio.SceneAssist;

public class Assist
{
	public static string AssetBundleSystemSE => "sound/data/systemse.unity3d";

	public static AudioSource PlayDecisionSE()
	{
		return Manager.Sound.Play(new Manager.Sound.Loader
		{
			type = Manager.Sound.Type.SystemSE,
			bundle = AssetBundleSystemSE,
			asset = "sse_00_02",
			fadeTime = 0f
		});
	}

	public static AudioSource PlayCancelSE()
	{
		return Manager.Sound.Play(new Manager.Sound.Loader
		{
			type = Manager.Sound.Type.SystemSE,
			bundle = AssetBundleSystemSE,
			asset = "sse_00_04",
			fadeTime = 0f
		});
	}
}
