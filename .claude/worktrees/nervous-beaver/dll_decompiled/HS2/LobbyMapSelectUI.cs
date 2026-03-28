using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class LobbyMapSelectUI : MonoBehaviour
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

	[SerializeField]
	private MaleSelectParameterUI parameterUI1;

	[SerializeField]
	private MaleSelectParameterUI parameterUI2;

	private readonly string[] strHScene = new string[5] { "Hを開始しますか？", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?" };

	public IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<LobbySceneManager>.IsInstance());
		LobbySceneManager lm = Singleton<LobbySceneManager>.Instance;
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
					Game instance = Singleton<Game>.Instance;
					HSceneManager instance2 = Singleton<HSceneManager>.Instance;
					Singleton<Character>.Instance.DeleteChara(lm.ConciergeChaCtrl);
					instance.eventNo = lm.eventNos[lm.heroineRommListIdx[0]];
					int peepKind = instance.peepKind;
					instance.peepKind = -1;
					instance.isConciergeAngry = false;
					instance.mapNo = scrollCtrl.selectInfo.info.No;
					instance2.mapID = instance.mapNo;
					instance.heroineList = new List<Heroine>(lm.heroines.Where((Heroine heroine) => heroine != null));
					if (instance.eventNo == -1)
					{
						instance.eventNo = GlobalHS2Calc.GetGeneralEventNo(instance.heroineList[0].gameinfo2, instance.mapNo);
					}
					bool flag = false;
					if (MathfEx.IsRange(28, Singleton<ADVManager>.Instance.advDelivery.adv_category, 29, isEqual: true) && peepKind == 6)
					{
						flag = true;
						instance.peepKind = peepKind;
						instance.eventNo = -1;
					}
					else if (MathfEx.IsRange(30, Singleton<ADVManager>.Instance.advDelivery.adv_category, 31, isEqual: true) && peepKind == 6)
					{
						flag = true;
						instance.eventNo = -1;
					}
					else if (Singleton<ADVManager>.Instance.advDelivery.adv_category == 32 && peepKind == 6)
					{
						flag = true;
						instance.eventNo = -1;
					}
					instance.saveData.BeforeFemaleName = string.Empty;
					if (instance.eventNo != -1 && !flag)
					{
						if (MathfEx.IsRange(30, instance.eventNo, 31, isEqual: true) && peepKind == 5)
						{
							Singleton<ADVManager>.Instance.advDelivery.Set("9", lm.heroines[0].personality, instance.eventNo);
							instance.peepKind = peepKind;
						}
						else
						{
							Singleton<ADVManager>.Instance.advDelivery.Set("0", lm.heroines[0].personality, instance.eventNo);
						}
						Singleton<ADVManager>.Instance.filenames[0] = lm.heroines[0]?.chaFile.charaFileName ?? "";
						Singleton<HSceneManager>.Instance.pngFemales[1] = lm.heroines[1]?.chaFile.charaFileName ?? "";
						Singleton<ADVManager>.Instance.filenames[1] = "";
						if (lm.heroines[1] != null && lm.heroines[1].chaCtrl != null)
						{
							instance.heroineList.Remove(lm.heroines[1]);
						}
						Scene.LoadReserve(new Scene.Data
						{
							levelName = "ADV",
							fadeType = FadeCanvas.Fade.In
						}, isLoadingImageDraw: true);
					}
					else
					{
						instance2.females[0] = lm.heroines[0].chaCtrl;
						instance2.females[1] = null;
						Singleton<HSceneManager>.Instance.pngFemales[1] = lm.heroines[1]?.chaFile.charaFileName ?? "";
						if (lm.heroines[1] != null && lm.heroines[1].chaCtrl != null)
						{
							instance.heroineList.Remove(lm.heroines[1]);
						}
						instance.saveData.BeforeFemaleName = instance.heroineList[0].chaFile.charaFileName;
						PlayerCharaSaveInfo playerChara = instance.saveData.playerChara;
						instance2.pngMale = playerChara.FileName;
						instance2.bFutanari = playerChara.Futanari;
						playerChara = instance.saveData.secondPlayerChara;
						instance2.pngMaleSecond = playerChara.FileName;
						instance2.bFutanariSecond = playerChara.Futanari;
						Scene.LoadReserve(new Scene.Data
						{
							levelName = "HScene",
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
			where lm.CGMain.interactable
			where lm.CGModes[1].alpha >= 0.9f
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
		LobbySceneManager instance = Singleton<LobbySceneManager>.Instance;
		instance.SetModeCanvasGroup(0);
		instance.CGParameter.alpha = 1f;
	}

	public void InitList(int _eventNo)
	{
		LobbySceneManager instance = Singleton<LobbySceneManager>.Instance;
		int[] array = null;
		List<MapInfo.Param> maps = BaseMap.infoTable.Values.Where((MapInfo.Param map) => map.Draw != -1).ToList();
		bool flag = false;
		if (MathfEx.IsRange(28, Singleton<ADVManager>.Instance.advDelivery.adv_category, 29, isEqual: true) && Singleton<Game>.Instance.peepKind == 6)
		{
			flag = true;
		}
		else if (MathfEx.IsRange(30, Singleton<ADVManager>.Instance.advDelivery.adv_category, 31, isEqual: true) && Singleton<Game>.Instance.peepKind == 5)
		{
			flag = true;
		}
		array = ((Singleton<Game>.Instance.saveData.TutorialNo != -1) ? new int[1] { UnityEngine.Random.Range(0, 2) } : ((_eventNo == -1 || flag) ? (from map in BaseMap.infoTable
			where map.Value.Draw != -1 && map.Value.Events.Contains(-1)
			select map.Key).ToArray() : Singleton<Game>.Instance.infoEventContentDic[_eventNo].meetingLocationMaps));
		array = GlobalHS2Calc.ExcludeAchievementMap(array);
		array = GlobalHS2Calc.ExcludeFursRoomAchievementMap(array);
		maps = GlobalHS2Calc.ExcludeAppendMap(maps);
		scrollCtrl.SelectInfoClear();
		scrollCtrl.Init(maps, array);
		if ((bool)parameterUI1)
		{
			parameterUI1.SetParameter(instance.heroines[0].chaFile);
		}
		if ((bool)parameterUI2)
		{
			if (instance.heroines[1] == null)
			{
				parameterUI2.InitParameter();
			}
			else
			{
				parameterUI2.SetParameter(instance.heroines[1].chaFile);
			}
		}
	}
}
