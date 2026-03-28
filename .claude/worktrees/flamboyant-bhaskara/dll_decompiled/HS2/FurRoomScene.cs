using System.Collections;
using Illusion.Game;
using Manager;
using UniRx.Async;
using UnityEngine;

namespace HS2;

public class FurRoomScene : MonoBehaviour
{
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
		while (!Singleton<FurRoomSceneManager>.IsInstance())
		{
			yield return null;
		}
		FurRoomSceneManager fm = Singleton<FurRoomSceneManager>.Instance;
		while (!fm.ConciergeChaCtrl)
		{
			yield return null;
		}
		Utils.Sound.Play(new Utils.Sound.SettingBGM(BGM.fur));
		yield return BaseMap.ChangeAsync(2, FadeCanvas.Fade.None).ToCoroutine();
		fm.SetCociergeModeCanvasGroup(0);
		fm.SetCharaAnimationAndPosition();
		fm.ConciergeChaCtrl.visibleAll = true;
		yield return null;
		Scene.DrawLoadingImage(isDraw: false);
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
		base.enabled = true;
		fm.CGMain.interactable = false;
		fm.HomeConciergeInfoData.SetRoomStartVoice(fm.ConciergeChaCtrl);
		yield return new WaitUntil(() => !Voice.IsPlay());
		fm.CGMain.interactable = true;
	}
}
