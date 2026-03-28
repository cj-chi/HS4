using System.Collections;
using ADV;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UniRx.Async;
using UnityEngine;

namespace HS2;

public class LobbyScene : MonoBehaviour
{
	public static int startCanvas;

	private IEnumerator Start()
	{
		base.enabled = false;
		while (!SingletonInitializer<Scene>.initialized)
		{
			yield return null;
		}
		Scene.DrawLoadingImage(isDraw: true);
		while (!Singleton<GameSystem>.IsInstance())
		{
			yield return null;
		}
		while (!Singleton<LobbySceneManager>.IsInstance())
		{
			yield return null;
		}
		LobbySceneManager lm = Singleton<LobbySceneManager>.Instance;
		while (!lm.IsInitialize)
		{
			yield return null;
		}
		Utils.Sound.Play(new Utils.Sound.SettingBGM(BGM.lobby));
		yield return BaseMap.ChangeAsync(0, FadeCanvas.Fade.None).ToCoroutine();
		lm.SetModeCanvasGroup(0);
		lm.SetSelectCanvasGroup(startCanvas != 0);
		lm.SetCharaAnimationAndPosition();
		lm.OCBTutorial.SetActive(_active: false);
		if (Singleton<Game>.Instance.saveData.TutorialNo == 7)
		{
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = false;
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 2;
			global::Tutorial2D.Tutorial2D.Load();
			Singleton<Game>.Instance.saveData.TutorialNo = 8;
			Singleton<Game>.Instance.saveData.Save();
			lm.OCBTutorial.SetActiveToggle(0);
		}
		else if (Singleton<Game>.Instance.saveData.TutorialNo == 8)
		{
			lm.OCBTutorial.SetActiveToggle(0);
		}
		Scene.DrawLoadingImage(isDraw: false);
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
		base.enabled = true;
	}

	private void OnDestroy()
	{
		Setup.Dispose();
	}
}
