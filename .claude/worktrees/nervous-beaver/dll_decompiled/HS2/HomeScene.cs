using System.Collections;
using System.Collections.Generic;
using ADV;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UniRx.Async;
using UnityEngine;

namespace HS2;

public class HomeScene : MonoBehaviour
{
	public static int startCanvas = 0;

	public static int startCharaEdit = -1;

	[SerializeField]
	private CharaEditUI charaEditUI;

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
		while (!Singleton<HomeSceneManager>.IsInstance())
		{
			yield return null;
		}
		Scene.DrawLoadingImage(isDraw: true);
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		while (!hm.ConciergeChaCtrl)
		{
			yield return null;
		}
		Game game = Singleton<Game>.Instance;
		Utils.Sound.Play(new Utils.Sound.SettingBGM(BGM.myroom));
		yield return BaseMap.ChangeAsync(1, FadeCanvas.Fade.None).ToCoroutine();
		hm.GetMapAnimationObject();
		bool isConciergeAngry = Singleton<Game>.Instance.isConciergeAngry;
		Singleton<Game>.Instance.GameParameterInit(_isActorCler: false);
		hm.SetModeCanvasGroup(startCanvas);
		if (startCanvas == 3)
		{
			hm.SetCociergeModeCanvasGroup(0);
			hm.SetCameraPosition(4, hm.ConciergeChaCtrl.GetShapeBodyValue(0));
			hm.ConciergeChaCtrl.visibleAll = true;
			hm.ConciergeAnimationPlay(0, _isSameCheck: false);
			Singleton<Game>.Instance.isConciergeAngry = isConciergeAngry;
			if (isConciergeAngry)
			{
				hm.SetSelectCount(hm.ConciergeButtonSelectCountMax);
			}
			hm.ConciergeMenuUI.InitSpecialRoom();
		}
		else if (startCanvas == 2)
		{
			hm.AnimationMenu(_isOpen: true);
			hm.SetCameraPosition(2);
			hm.CharaEditUI.PlayerCoordinateButtonVisible();
			if (startCharaEdit == 4)
			{
				charaEditUI.StartPlayerSelect();
				startCharaEdit = -1;
			}
			else if (startCharaEdit == 2)
			{
				charaEditUI.StartGroupSelect();
				startCharaEdit = -1;
			}
			else if (startCharaEdit == 3)
			{
				charaEditUI.StartCoordinateSelect();
				startCharaEdit = -1;
			}
			for (int i = 0; i < hm.roomList.Length; i++)
			{
				hm.roomList[i] = new List<string>(Singleton<Game>.Instance.saveData.roomList[i]);
			}
		}
		else
		{
			hm.SetCameraPosition(0);
		}
		hm.OCBTutorial.SetActive(_active: false);
		if (Singleton<Game>.Instance.saveData.TutorialNo == 0)
		{
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = false;
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 0;
			global::Tutorial2D.Tutorial2D.Load();
			Singleton<Game>.Instance.saveData.TutorialNo = 1;
			Singleton<Game>.Instance.saveData.Save();
			hm.OCBTutorial.SetActiveToggle(0);
		}
		else if (Singleton<Game>.Instance.saveData.TutorialNo == 1)
		{
			hm.OCBTutorial.SetActiveToggle(0);
		}
		else if (Singleton<Game>.Instance.saveData.TutorialNo == 12)
		{
			Singleton<Game>.Instance.saveData.Save();
			hm.OCBTutorial.SetActiveToggle(7);
		}
		else if (Singleton<Game>.Instance.saveData.TutorialNo == 15)
		{
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = false;
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 5;
			global::Tutorial2D.Tutorial2D.Load();
			SaveData.SetAchievementAchieve(0);
			hm.NoticePreparation();
			Singleton<Game>.Instance.saveData.TutorialNo = -1;
			Singleton<Game>.Instance.saveData.Save();
		}
		else if (Singleton<Game>.Instance.saveData.TutorialNo == 16)
		{
			hm.NoticePreparation();
			SaveData.SetAchievementAchieve(0);
			hm.NoticePreparation();
			Singleton<Game>.Instance.saveData.TutorialNo = -1;
			Singleton<Game>.Instance.saveData.Save();
		}
		else if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == 6)
		{
			game.appendSaveData.AppendTutorialNo = -1;
			game.appendSaveData.mapReleases.Add(17);
			hm.NoticePreparation();
		}
		else if (Singleton<Game>.Instance.appendSaveData.IsAppendStart == 1)
		{
			hm.NoticePreparation();
			hm.SpecialRoomNoticePreparation();
			Singleton<Game>.Instance.appendSaveData.IsAppendStart = -1;
			Singleton<Game>.Instance.appendSaveData.Save();
		}
		else if (Singleton<Game>.Instance.saveData.TutorialNo == -1)
		{
			hm.NoticePreparation();
		}
		hm.HomeUI.DXPlanButtonVisible();
		hm.HomeUI.AppendTutorialStartVisible();
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
