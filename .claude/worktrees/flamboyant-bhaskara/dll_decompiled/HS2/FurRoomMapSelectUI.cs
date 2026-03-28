using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Actor;
using Config;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class FurRoomMapSelectUI : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private Button btnBack;

	[SerializeField]
	private Button btnPrevious;

	[SerializeField]
	private Button btnNext;

	[SerializeField]
	private MenuItemUI itemUIStart;

	[SerializeField]
	private LobbyMapSelectInfoScrollController scrollCtrl;

	private readonly string[] strHScene = new string[5] { "Hを開始しますか？", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?" };

	public IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<FurRoomSceneManager>.IsInstance());
		FurRoomSceneManager fm = Singleton<FurRoomSceneManager>.Instance;
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Back());
		});
		btnPrevious.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			scrollCtrl.PreviousLine();
		});
		btnNext.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			scrollCtrl.NextLine();
		});
		itemUIStart.btn.OnClickAsObservable().Subscribe(delegate
		{
			if (scrollCtrl.selectInfo != null)
			{
				Utils.Sound.Play(SystemSE.ok_s);
				ConfirmDialog.Status status = ConfirmDialog.status;
				status.Sentence = strHScene[Singleton<GameSystem>.Instance.languageInt];
				status.Yes = delegate
				{
					Utils.Sound.Play(SystemSE.ok_l);
					SaveData.SetAchievementAchieve(4);
					Game game = Singleton<Game>.Instance;
					HSceneManager instance = Singleton<HSceneManager>.Instance;
					game.eventNo = 33;
					game.peepKind = -1;
					game.isConciergeAngry = fm.IsAngry();
					game.mapNo = scrollCtrl.selectInfo.info.No;
					instance.mapID = game.mapNo;
					game.heroineList = new List<Heroine> { fm.ConciergeHeroine };
					instance.females = new ChaControl[2] { fm.ConciergeChaCtrl, null };
					if (fm.ConciergeChaCtrl.fileGameInfo2.hCount < 3 || game.appendSaveData.AppendTutorialNo != -1)
					{
						instance.pngFemales[1] = "";
					}
					else
					{
						instance.pngFemales[1] = (game.appendSaveData.IsFurSitri3P ? Singleton<Character>.Instance.sitriPath : "");
						instance.SecondSitori = game.appendSaveData.IsFurSitri3P;
					}
					PlayerCharaSaveInfo playerChara = game.saveData.playerChara;
					instance.pngMale = playerChara.FileName;
					instance.bFutanari = playerChara.Futanari;
					playerChara = game.saveData.secondPlayerChara;
					instance.pngMaleSecond = playerChara.FileName;
					instance.bFutanariSecond = playerChara.Futanari;
					Dictionary<string, Game.EventCharaInfo> source = game.tableLobbyEvents[game.saveData.selectGroup];
					game.tableDesireCharas.Clear();
					source.Where((KeyValuePair<string, Game.EventCharaInfo> c) => Game.DesireEventIDs.Contains(c.Value.eventID)).ToList().ForEach(delegate(KeyValuePair<string, Game.EventCharaInfo> c)
					{
						game.tableDesireCharas.Add(Path.GetFileNameWithoutExtension(c.Value.fileName), c.Value.eventID);
					});
					if (fm.ConciergeChaCtrl.fileGameInfo2.hCount >= 3)
					{
						Scene.LoadReserve(new Scene.Data
						{
							levelName = "HScene",
							fadeType = FadeCanvas.Fade.In
						}, isLoadingImageDraw: true);
					}
					else
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("0", -1, 33);
						Scene.LoadReserve(new Scene.Data
						{
							levelName = "ADV",
							fadeType = FadeCanvas.Fade.In
						}, isLoadingImageDraw: true);
					}
				};
				status.No = delegate
				{
					Utils.Sound.Play(SystemSE.cancel);
				};
				ConfirmDialog.Load();
			}
		});
		itemUIStart.btn.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (itemUIStart.btn.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				itemUIStart.texts.ForEach(delegate(Text t)
				{
					t.color = Game.selectFontColor;
				});
			}
		});
		itemUIStart.btn.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (itemUIStart.btn.IsInteractable())
			{
				itemUIStart.texts.ForEach(delegate(Text t)
				{
					t.color = Game.defaultFontColor;
				});
			}
		});
		List<Button> list = new List<Button>();
		list.Add(btnBack);
		list.Add(btnPrevious);
		list.Add(btnNext);
		list.ForEach(delegate(Button bt)
		{
			bt.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		});
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !Scene.IsFadeNow
			where Singleton<Game>.Instance.saveData.TutorialNo == -1 || Singleton<Game>.Instance.saveData.TutorialNo != 11
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			where fm.CGMain.interactable
			where fm.CGConciergeModes[2].alpha >= 0.9f
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Back());
		});
		base.enabled = true;
	}

	private IEnumerator Back()
	{
		yield return null;
		Singleton<FurRoomSceneManager>.Instance.SetCociergeModeCanvasGroup(0);
	}

	public void InitList()
	{
		int[] array = null;
		List<MapInfo.Param> maps = BaseMap.infoTable.Values.Where((MapInfo.Param map) => map.Draw != -1).ToList();
		array = Singleton<Game>.Instance.infoEventContentDic[33].meetingLocationMaps;
		array = GlobalHS2Calc.ExcludeAchievementMap(array);
		array = GlobalHS2Calc.ExcludeFursRoomAchievementMap(array);
		maps = GlobalHS2Calc.ExcludeAppendMap(maps);
		scrollCtrl.SelectInfoClear();
		scrollCtrl.Init(maps, array);
	}
}
