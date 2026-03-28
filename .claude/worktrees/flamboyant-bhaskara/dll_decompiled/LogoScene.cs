using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illusion.Extensions;
using Manager;
using UnityEngine;

public class LogoScene : MonoBehaviour
{
	[SerializeField]
	private float waitTime = 2f;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		string path = UserData.Create("chara/navi/") + "navi.png";
		if (!File.Exists(path))
		{
			TextAsset textAsset = CommonLib.LoadAsset<TextAsset>("custom/00/presets_f_00.unity3d", "ill_Default_Navi");
			if (null != textAsset)
			{
				File.WriteAllBytes(path, textAsset.bytes);
			}
			AssetBundleManager.UnloadAssetBundle("custom/00/presets_f_00.unity3d", isUnloadForceRefCount: true);
		}
		if (GameSystem.isAdd50)
		{
			path = UserData.Create("chara/navi/") + "sitri.png";
			if (!File.Exists(path))
			{
				TextAsset textAsset2 = CommonLib.LoadAsset<TextAsset>("custom/50/presets_f_50.unity3d", "ill_Default_Sitri");
				if (null != textAsset2)
				{
					File.WriteAllBytes(path, textAsset2.bytes);
				}
				AssetBundleManager.UnloadAssetBundle("custom/50/presets_f_50.unity3d", isUnloadForceRefCount: true);
			}
		}
		Singleton<Game>.Instance.saveData.RoomListCharaExists();
		Singleton<Game>.Instance.saveData.PlayerCoordinateExists();
		Singleton<Game>.Instance.saveData.PlayerExists();
		List<AudioClip> clipList = new List<AudioClip>();
		List<string> assetBundleNameListFromPath = AssetBundleData.GetAssetBundleNameListFromPath("sound/data/systemse/brandcall/", subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		assetBundleNameListFromPath.ForEach(delegate(string file)
		{
			if (GameSystem.IsPathAdd50(file))
			{
				clipList.AddRange(AssetBundleManager.LoadAllAsset(file, typeof(AudioClip)).GetAllAssets<AudioClip>());
				AssetBundleManager.UnloadAssetBundle(file, isUnloadForceRefCount: true);
			}
		});
		yield return new WaitWhile(() => Scene.IsFadeNow);
		clipList.RemoveAll((AudioClip p) => p == null);
		AudioClip audioClip = clipList.Shuffle().FirstOrDefault();
		AudioSource source = null;
		if (audioClip != null)
		{
			source = Manager.Sound.Play(Manager.Sound.Type.SystemSE, audioClip);
		}
		yield return new WaitForSecondsRealtime(waitTime);
		if (source != null)
		{
			yield return new WaitWhile(() => source.isPlaying);
			Object.Destroy(source.gameObject);
		}
		clipList.ForEach(UnityEngine.Resources.UnloadAsset);
		clipList.Clear();
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "Title",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
		base.enabled = true;
	}
}
