using System.Collections;
using ADV;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UniRx.Async;
using UnityEngine;

namespace HS2;

public class SpecialTreatmentRoomScene : MonoBehaviour
{
	public static int startCanvas = 0;

	public static int startCharaEdit = -1;

	private IEnumerator Start()
	{
		base.enabled = false;
		while (!Singleton<GameSystem>.IsInstance())
		{
			yield return null;
		}
		while (!SingletonInitializer<Scene>.initialized)
		{
			yield return null;
		}
		while (!Singleton<SpecialTreatmentRoomManager>.IsInstance())
		{
			yield return null;
		}
		Scene.DrawLoadingImage(isDraw: true);
		SpecialTreatmentRoomManager spm = Singleton<SpecialTreatmentRoomManager>.Instance;
		while (!spm.ConciergeChaCtrl)
		{
			yield return null;
		}
		Utils.Sound.Play(new Utils.Sound.SettingBGM(BGM.sitri));
		yield return BaseMap.ChangeAsync(17, FadeCanvas.Fade.None).ToCoroutine();
		spm.GetMapAnimationObject();
		spm.SetModeCanvasGroup(0);
		spm.SetCameraPosition(0, spm.ConciergeChaCtrl.GetShapeBodyValue(0));
		spm.OCBTutorial.SetActive(_active: false);
		if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == 0)
		{
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = false;
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 6;
			global::Tutorial2D.Tutorial2D.Load();
			Singleton<Game>.Instance.appendSaveData.AppendTutorialNo = 4;
			spm.OCBTutorial.SetActiveToggle(0);
			spm.SetModeCanvasGroup(1);
			spm.SetCameraPosition(1);
			spm.AnimationMenu(_isOpen: true);
			spm.PlanSelect.InitSelect();
		}
		else if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == -1)
		{
			yield return null;
			if (startCanvas == 0)
			{
				spm.ActionInfoData.SetVisitVoice(spm.ConciergeChaCtrl);
				spm.ConciergeAnimationPlay(1);
			}
		}
		Singleton<ADVManager>.Instance.advDelivery.Set("0", 0, 0);
		Scene.DrawLoadingImage(isDraw: false);
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
		base.enabled = true;
	}

	private void OnDestroy()
	{
		Setup.Dispose();
	}
}
