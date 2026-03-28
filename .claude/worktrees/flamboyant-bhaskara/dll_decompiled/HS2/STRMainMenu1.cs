using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Actor;
using CharaCustom;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class STRMainMenu1 : MonoBehaviour
{
	private enum MenuCategory
	{
		Plan,
		H,
		Edit,
		Lobby
	}

	[Serializable]
	public class MenuItemUI
	{
		public Toggle tgl;

		public UIObjectSlideOnCursor uiSlide;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private MenuItemUI[] itemUIs;

	[SerializeField]
	private Button btnBack;

	private readonly string[] strHScene = new string[5] { "Hを開始しますか？", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?", "Do you want to start H ?" };

	private readonly string[] strLobby = new string[5] { "ロビーに移動しますか？", "Shall I take you to the lobby?", "Shall I take you to the lobby?", "Shall I take you to the lobby?", "Shall I take you to the lobby?" };

	private readonly string[] strHome = new string[6] { "マイルームに戻ります\nよろしいですか？", "Return to my room.\nIs it OK?", "Return to my room.\nIs it OK?", "Return to my room.\nIs it OK?", "Return to my room.\nIs it OK?", "" };

	private readonly string[] strCustom = new string[6] { "キャラのメイク画面に移動します\nよろしいですか？", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "" };

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
		while (!Singleton<SpecialTreatmentRoomManager1>.IsInstance())
		{
			yield return null;
		}
		Game instance = Singleton<Game>.Instance;
		foreach (var item in itemUIs.ToForEachTuples())
		{
			var (value, index) = item;
			value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
			{
				if (_isOn)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					value.uiSlide.IsSlideAlways = true;
					value.texts.ForEach(delegate(Text t)
					{
						t.color = Game.selectFontColor;
					});
					ConfirmDialog.Status status = ConfirmDialog.status;
					switch (index)
					{
					case 0:
						StartCoroutine(PlanCoroutine());
						break;
					case 1:
						GoToH();
						break;
					case 2:
						status.Sentence = strCustom[Singleton<GameSystem>.Instance.languageInt];
						status.Yes = delegate
						{
							Utils.Sound.Play(SystemSE.ok_l);
							StartCoroutine(CustomCoroutine());
						};
						status.No = delegate
						{
							Utils.Sound.Play(SystemSE.cancel);
							DeSelectItemUI(itemUIs[2]);
						};
						ConfirmDialog.Load();
						break;
					case 3:
						status.Sentence = strLobby[Singleton<GameSystem>.Instance.languageInt];
						status.Yes = delegate
						{
							Utils.Sound.Play(SystemSE.ok_s);
							StartCoroutine(GoToLobby());
						};
						status.No = delegate
						{
							Utils.Sound.Play(SystemSE.cancel);
							DeSelectItemUI(itemUIs[3]);
						};
						ConfirmDialog.Load();
						break;
					}
				}
				else
				{
					value.uiSlide.IsSlideAlways = false;
					value.texts.ForEach(delegate(Text t)
					{
						t.color = Game.defaultFontColor;
					});
				}
			});
			value.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		}
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = strHome[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
				StartCoroutine(Back());
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (btnBack.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
			}
		});
		if (GameSystem.isAdd50)
		{
			itemUIs[0].tgl.interactable = !instance.IsAllNormalState(instance.saveData.selectGroup);
		}
		base.enabled = true;
	}

	private IEnumerator BowCoroutine()
	{
		SpecialTreatmentRoomManager1 instance = Singleton<SpecialTreatmentRoomManager1>.Instance;
		instance.CGBase.blocksRaycasts = false;
		instance.ConciergeAnimationPlay(1);
		yield return new WaitUntil(() => !Voice.IsPlay());
	}

	private IEnumerator PlanCoroutine()
	{
		SpecialTreatmentRoomManager1 instance = Singleton<SpecialTreatmentRoomManager1>.Instance;
		instance.CGBase.blocksRaycasts = false;
		instance.ActionInfoData.SetPlanSelectVoice(instance.ConciergeChaCtrl);
		instance.StartFade();
		instance.SetModeCanvasGroup(1);
		instance.SetCameraPosition(1);
		instance.AnimationMenu(_isOpen: true);
		DeSelectItemUI(itemUIs[0]);
		instance.PlanSelect.InitSelect();
		instance.CGBase.blocksRaycasts = true;
		yield return null;
	}

	private IEnumerator CustomCoroutine()
	{
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		strm.CGBase.blocksRaycasts = false;
		strm.ActionInfoData.SetEditVoice(strm.ConciergeChaCtrl);
		yield return new WaitUntil(() => !Voice.IsPlay());
		global::CharaCustom.CharaCustom.modeNew = false;
		global::CharaCustom.CharaCustom.editCharaFileName = Singleton<Character>.Instance.sitriPath;
		global::CharaCustom.CharaCustom.modeSex = 1;
		global::CharaCustom.CharaCustom.isConcierge = 1;
		SpecialTreatmentRoomScene1.startCanvas = 1;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "CharaCustom",
			fadeType = FadeCanvas.Fade.In,
			onLoad = delegate
			{
				Singleton<Character>.Instance.DeleteChara(strm.ConciergeChaCtrl);
			}
		}, isLoadingImageDraw: true);
		strm.CGBase.blocksRaycasts = true;
		yield return null;
	}

	private IEnumerator GoToLobby()
	{
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		strm.CGBase.blocksRaycasts = false;
		strm.ConciergeAnimationPlay(1);
		strm.ActionInfoData.SetLobbyVoice(strm.ConciergeChaCtrl);
		yield return new WaitUntil(() => !Voice.IsPlay());
		strm.CGBase.blocksRaycasts = true;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "LobbyScene",
			fadeType = FadeCanvas.Fade.In,
			onLoad = delegate
			{
				Singleton<Character>.Instance.DeleteChara(strm.ConciergeChaCtrl);
			}
		}, isLoadingImageDraw: true);
	}

	private IEnumerator Back()
	{
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		strm.CGBase.blocksRaycasts = false;
		strm.ConciergeAnimationPlay(1);
		strm.ActionInfoData.SetBackVoice(strm.ConciergeChaCtrl);
		yield return new WaitUntil(() => !Voice.IsPlay());
		strm.CGBase.blocksRaycasts = true;
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "Home",
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: true);
	}

	private void DeSelectItemUI(MenuItemUI _ui)
	{
		_ui.uiSlide.IsSlideAlways = false;
		_ui.tgl.SetIsOnWithoutCallback(isOn: false);
		_ui.texts.ForEach(delegate(Text t)
		{
			t.color = Game.defaultFontColor;
		});
	}

	private void GoToH()
	{
		SpecialTreatmentRoomManager1 instance = Singleton<SpecialTreatmentRoomManager1>.Instance;
		AppendSaveData appendSaveData = Singleton<Game>.Instance.appendSaveData;
		if (appendSaveData.SitriSelectCount < 2)
		{
			instance.ActionInfoData.SetHVoice(instance.ConciergeChaCtrl);
			appendSaveData.SitriSelectCount++;
			itemUIs[1].tgl.interactable = false;
			DeSelectItemUI(itemUIs[1]);
			return;
		}
		if (appendSaveData.SitriSelectCount < 3)
		{
			StartCoroutine(GoToHCoroutine());
			return;
		}
		_ = Singleton<HSceneManager>.Instance;
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = strHScene[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			StartCoroutine(GoToHCoroutine());
		};
		status.No = delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			DeSelectItemUI(itemUIs[1]);
		};
		ConfirmDialog.Load();
	}

	private IEnumerator GoToHCoroutine()
	{
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		Game game = Singleton<Game>.Instance;
		AppendSaveData apSave = game.appendSaveData;
		HSceneManager hscene = Singleton<HSceneManager>.Instance;
		strm.CGBase.blocksRaycasts = false;
		strm.ActionInfoData.SetHVoice(strm.ConciergeChaCtrl);
		yield return new WaitUntil(() => !Voice.IsPlay());
		Dictionary<string, Game.EventCharaInfo> source = game.tableLobbyEvents[game.saveData.selectGroup];
		game.tableDesireCharas.Clear();
		source.Where((KeyValuePair<string, Game.EventCharaInfo> c) => Game.DesireEventIDs.Contains(c.Value.eventID)).ToList().ForEach(delegate(KeyValuePair<string, Game.EventCharaInfo> c)
		{
			game.tableDesireCharas.Add(Path.GetFileNameWithoutExtension(c.Value.fileName), c.Value.eventID);
		});
		game.eventNo = 56;
		game.peepKind = -1;
		game.isConciergeAngry = false;
		game.mapNo = 17;
		game.heroineList = new List<Heroine> { strm.ConciergeHeroine, null };
		hscene.females = new ChaControl[2] { strm.ConciergeChaCtrl, null };
		hscene.pngFemales[1] = (apSave.IsFurSitri3P ? Singleton<Character>.Instance.conciergePath : "");
		hscene.mapID = game.mapNo;
		PlayerCharaSaveInfo playerChara = game.saveData.playerChara;
		hscene.pngMale = playerChara.FileName;
		hscene.bFutanari = playerChara.Futanari;
		playerChara = game.saveData.secondPlayerChara;
		hscene.pngMaleSecond = playerChara.FileName;
		hscene.bFutanariSecond = playerChara.Futanari;
		if (apSave.SitriSelectCount >= 3)
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "HScene",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		}
		else
		{
			Singleton<ADVManager>.Instance.advDelivery.Set("0", -2, 56);
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "ADV",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		}
		yield return null;
	}
}
