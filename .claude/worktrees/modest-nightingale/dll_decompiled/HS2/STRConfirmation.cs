using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Config;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UGUI_AssistLibrary;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class STRConfirmation : MonoBehaviour
{
	public class PlanInfo
	{
		public ReactiveProperty<STRCharaFileInfo> rpPartnerChara = new ReactiveProperty<STRCharaFileInfo>(null);

		public STRCoordinateFileInfo partnerCoordinateInfo;

		public STRCoordinateFileInfo playerCoordinateInfo;

		public void MemberInit()
		{
			rpPartnerChara.Value = null;
			partnerCoordinateInfo = null;
			playerCoordinateInfo = null;
		}
	}

	[Serializable]
	public class ButtonInfo
	{
		public Button btn;

		public Text txt;
	}

	[Serializable]
	public class CardInfo
	{
		public Button btn;

		public RawImage rImage;

		public Text txt;
	}

	[SerializeField]
	private ButtonInfo biNo;

	[SerializeField]
	private ButtonInfo biYes;

	[SerializeField]
	private Text txtPlan;

	[SerializeField]
	private RawImage riMap;

	[SerializeField]
	private CardInfo ciPartner;

	[SerializeField]
	private CardInfo ciPlayer;

	[SerializeField]
	private CardInfo ciPartnerCoordinate;

	[SerializeField]
	private CardInfo ciPlayerCoordinate;

	[SerializeField]
	private GameObject objPlayerDontSelect;

	[SerializeField]
	private GameObject objPlayerCoordinateDontSelect;

	[SerializeField]
	private Texture texNoSelect;

	[SerializeField]
	private STRCharaSelectUI partnerSelectUI;

	[SerializeField]
	private STRPlayerCharaSelectUI playerSelectUI;

	[SerializeField]
	private STRCoordinateListUI partnerCoordinateUI;

	[SerializeField]
	private STRCoordinateListUI playerCoordinateUI;

	[SerializeField]
	private CanvasGroupCtrl cgPartner;

	[SerializeField]
	private CanvasGroupCtrl cgPlayer;

	[SerializeField]
	private CanvasGroupCtrl cgPartnerCoordinate;

	[SerializeField]
	private CanvasGroupCtrl cgPlayerCoordinate;

	private PlanInfo planInfo = new PlanInfo();

	public STRCharaSelectUI PartnerSelectUI => partnerSelectUI;

	public STRPlayerCharaSelectUI PlayerSelectUI => playerSelectUI;

	public STRCoordinateListUI PartnerCoordinateUI => partnerCoordinateUI;

	public STRCoordinateListUI PlayerCoordinateUI => playerCoordinateUI;

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
		SpecialTreatmentRoomManager strm = Singleton<SpecialTreatmentRoomManager>.Instance;
		biNo.btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			StartCoroutine(Cancel(_async: false));
		});
		biNo.btn.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (biNo.btn.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				biNo.txt.color = Game.selectFontColor;
			}
		});
		biNo.btn.OnPointerExitAsObservable().Subscribe(delegate
		{
			biNo.txt.color = Game.defaultFontColor;
		});
		biYes.btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == 4)
			{
				strm.OCBTutorial.SetActive(_active: false);
				strm.CGBase.Enable(enable: false);
				strm.OpenADV("adv/scenario/op_append/50/01.unity3d", "3", 1, 1, _isCameraDontMove: true, _isUseCorrectCamera: false, _isCharaBackUpPos: false, _isCameraDontMoveRelease: false, delegate
				{
					Singleton<Game>.Instance.appendSaveData.AppendTutorialNo = 5;
					strm.MainCamera.fieldOfView = 40f;
					PlayPlan();
				});
			}
			else
			{
				PlayPlan();
			}
		});
		biYes.btn.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (biYes.btn.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				biYes.txt.color = Game.selectFontColor;
			}
		});
		biYes.btn.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (biYes.btn.IsInteractable())
			{
				biYes.txt.color = Game.defaultFontColor;
			}
		});
		ciPartner.btn.OnClickAsObservable().Subscribe(delegate
		{
			SelectWindowView(_isPartner: true);
			if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo != -1)
			{
				strm.OCBTutorial.SetActiveToggle(2);
			}
		});
		ciPlayer.btn.OnClickAsObservable().Subscribe(delegate
		{
			SelectWindowView(_isPartner: false, _isPlayer: true);
			if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo != -1)
			{
				strm.OCBTutorial.SetActiveToggle(2);
			}
		});
		ciPartnerCoordinate.btn.OnClickAsObservable().Subscribe(delegate
		{
			SelectWindowView(_isPartner: false, _isPlayer: false, _isPartnerCoordinate: true);
			if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo != -1)
			{
				strm.OCBTutorial.SetActiveToggle(3);
			}
		});
		ciPlayerCoordinate.btn.OnClickAsObservable().Subscribe(delegate
		{
			SelectWindowView(_isPartner: false, _isPlayer: false, _isPartnerCoordinate: false, _isPlayerCoordinate: true);
			if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo != -1)
			{
				strm.OCBTutorial.SetActiveToggle(3);
			}
		});
		partnerSelectUI.OnSelect = delegate(STRCharaFileInfo _info)
		{
			planInfo.rpPartnerChara.Value = _info;
			SetPartnerInfo(_info);
		};
		partnerSelectUI.OnDeSelect = delegate
		{
			planInfo.rpPartnerChara.Value = null;
			SetPartnerInfo(null);
		};
		playerSelectUI.OnSelect = delegate(GameCharaFileInfo _info)
		{
			PlayerCharaSaveInfo playerChara = Singleton<Game>.Instance.saveData.playerChara;
			if (!(playerChara.FileName == _info.FileName))
			{
				int sex = playerChara.Sex;
				playerChara.FileName = _info.FileName;
				playerChara.Sex = _info.sex;
				playerChara.Futanari = _info.futanari;
				SetPlayerInfo();
				playerCoordinateUI.CreateList();
				playerCoordinateUI.InitListSelect(_isInteractable: true);
				if (sex != playerChara.Sex)
				{
					planInfo.playerCoordinateInfo = null;
					SetPlayerCoordinateInfo(null);
				}
			}
		};
		partnerCoordinateUI.OnSelect = delegate(STRCoordinateFileInfo _info)
		{
			planInfo.partnerCoordinateInfo = _info;
			SetPartnerCoordinateInfo(_info);
		};
		partnerCoordinateUI.OnDeSelect = delegate
		{
			planInfo.partnerCoordinateInfo = null;
			SetPartnerCoordinateInfo(null);
		};
		playerCoordinateUI.OnSelect = delegate(STRCoordinateFileInfo _info)
		{
			planInfo.playerCoordinateInfo = _info;
			SetPlayerCoordinateInfo(_info);
		};
		playerCoordinateUI.OnDeSelect = delegate
		{
			planInfo.playerCoordinateInfo = null;
			SetPlayerCoordinateInfo(null);
		};
		planInfo.rpPartnerChara.Select((STRCharaFileInfo c) => c != null).SubscribeToInteractable(biYes.btn);
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !strm.IsADV
			where !Scene.IsFadeNow
			where strm.CGModes[2].alpha > 0.9f
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Cancel());
		});
		AllSelectWindowVisible(_visible: false);
		SetPlayerDontSelect(_isDontSelect: false);
		base.enabled = true;
	}

	private IEnumerator Cancel(bool _async = true)
	{
		SpecialTreatmentRoomManager strm = Singleton<SpecialTreatmentRoomManager>.Instance;
		strm.CGBase.blocksRaycasts = false;
		if (_async)
		{
			yield return null;
		}
		strm.StartFade();
		strm.SetModeCanvasGroup(1);
		strm.CGBase.blocksRaycasts = true;
		strm.nowSelectCategory = SpecialTreatmentRoomManager.PlanCategory.None;
		strm.PlanSelect.AllDeSelectItemUI();
		strm.PlanSelect.RiMapVisible(_isVisible: false);
		AllSelectWindowVisible(_visible: false);
		if (Singleton<Game>.Instance.appendSaveData.AppendTutorialNo != -1)
		{
			strm.OCBTutorial.SetActiveToggle(0);
		}
	}

	public void AllSelectWindowVisible(bool _visible)
	{
		if (_visible)
		{
			cgPartner.Open();
			cgPlayer.Open();
			cgPartnerCoordinate.Open();
			cgPlayerCoordinate.Open();
		}
		else
		{
			cgPartner.Close();
			cgPlayer.Close();
			cgPartnerCoordinate.Close();
			cgPlayerCoordinate.Close();
		}
	}

	public void SetInfo(int _planNo)
	{
		SpecialTreatmentRoomManager instance = Singleton<SpecialTreatmentRoomManager>.Instance;
		GameSystem instance2 = Singleton<GameSystem>.Instance;
		txtPlan.text = "";
		riMap.enabled = false;
		if (instance.dicPlanNameInfo.TryGetValue(_planNo, out var value))
		{
			txtPlan.text = value.name[instance2.languageInt];
			riMap.enabled = true;
			riMap.texture = CommonLib.LoadAsset<Texture2D>(value.bundle, value.asset, clone: false, value.manifest);
			AssetBundleManager.UnloadAssetBundle(value.bundle, isUnloadForceRefCount: true);
		}
		planInfo.MemberInit();
		SetPlayerInfo();
		SaveData saveData = Singleton<Game>.Instance.saveData;
		playerSelectUI.SetPlayerSelect(saveData.playerChara.FileName);
		SetPlayerDontSelect(instance.nowSelectCategory == SpecialTreatmentRoomManager.PlanCategory.Slavery);
	}

	private void SelectWindowView(bool _isPartner = false, bool _isPlayer = false, bool _isPartnerCoordinate = false, bool _isPlayerCoordinate = false)
	{
		if (_isPartner)
		{
			cgPartner.Open();
		}
		else
		{
			cgPartner.Close();
		}
		if (_isPlayer)
		{
			cgPlayer.Open();
		}
		else
		{
			cgPlayer.Close();
		}
		if (_isPartnerCoordinate)
		{
			cgPartnerCoordinate.Open();
		}
		else
		{
			cgPartnerCoordinate.Close();
		}
		if (_isPlayerCoordinate)
		{
			cgPlayerCoordinate.Open();
		}
		else
		{
			cgPlayerCoordinate.Close();
		}
	}

	private void SetPartnerInfo(STRCharaFileInfo _info)
	{
		if (ciPartner.rImage.texture != null && ciPartner.rImage.texture != texNoSelect)
		{
			UnityEngine.Object.Destroy(ciPartner.rImage.texture);
			ciPartner.rImage.texture = null;
		}
		ciPartner.rImage.texture = ((_info != null) ? PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.FullPath)) : texNoSelect);
		ciPartner.txt.text = ((_info != null) ? _info.name : string.Empty);
	}

	private void SetPlayerInfo()
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		string text = "";
		text = SaveData.CreatePlayerPngPath(saveData.playerChara);
		string text2 = "";
		ChaFileControl chaFileControl = new ChaFileControl();
		if (chaFileControl.LoadCharaFile(text))
		{
			text2 = chaFileControl.parameter.fullname;
		}
		if (ciPlayer.rImage.texture != null)
		{
			UnityEngine.Object.Destroy(ciPlayer.rImage.texture);
			ciPlayer.rImage.texture = null;
		}
		ciPlayer.rImage.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(text));
		ciPlayer.txt.text = text2;
	}

	private void SetPartnerCoordinateInfo(STRCoordinateFileInfo _info)
	{
		if (ciPartnerCoordinate.rImage.texture != null && ciPartnerCoordinate.rImage.texture != texNoSelect)
		{
			UnityEngine.Object.Destroy(ciPartnerCoordinate.rImage.texture);
			ciPartnerCoordinate.rImage.texture = null;
		}
		ciPartnerCoordinate.rImage.texture = ((_info != null) ? PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.FullPath)) : texNoSelect);
		ciPartnerCoordinate.txt.text = ((_info != null) ? _info.name : string.Empty);
	}

	private void SetPlayerCoordinateInfo(STRCoordinateFileInfo _info)
	{
		if (ciPlayerCoordinate.rImage.texture != null && ciPlayerCoordinate.rImage.texture != texNoSelect)
		{
			UnityEngine.Object.Destroy(ciPlayerCoordinate.rImage.texture);
			ciPlayerCoordinate.rImage.texture = null;
		}
		ciPlayerCoordinate.rImage.texture = ((_info != null) ? PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.FullPath)) : texNoSelect);
		ciPlayerCoordinate.txt.text = ((_info != null) ? _info.name : string.Empty);
	}

	private void SetPlayerDontSelect(bool _isDontSelect)
	{
		objPlayerDontSelect.SetActiveIfDifferent(_isDontSelect);
		objPlayerCoordinateDontSelect.SetActiveIfDifferent(_isDontSelect);
	}

	private void PlayPlan()
	{
		Game game = Singleton<Game>.Instance;
		HSceneManager instance = Singleton<HSceneManager>.Instance;
		ADVManager instance2 = Singleton<ADVManager>.Instance;
		SpecialTreatmentRoomManager instance3 = Singleton<SpecialTreatmentRoomManager>.Instance;
		Dictionary<string, Game.EventCharaInfo> source = game.tableLobbyEvents[game.saveData.selectGroup];
		game.tableDesireCharas.Clear();
		source.Where((KeyValuePair<string, Game.EventCharaInfo> c) => Game.DesireEventIDs.Contains(c.Value.eventID)).ToList().ForEach(delegate(KeyValuePair<string, Game.EventCharaInfo> c)
		{
			game.tableDesireCharas.Add(Path.GetFileNameWithoutExtension(c.Value.fileName), c.Value.eventID);
		});
		game.eventNo = (int)(50 + instance3.nowSelectCategory);
		game.peepKind = -1;
		game.isConciergeAngry = false;
		game.appendCoordinateFemale = planInfo.partnerCoordinateInfo?.FileName ?? "";
		game.appendCoordinatePlayer = planInfo.playerCoordinateInfo?.FileName ?? "";
		instance2.filenames[0] = planInfo.rpPartnerChara.Value.FileName;
		instance2.filenames[1] = "";
		game.mapNo = SpecialTreatmentRoomManager.mapNos[(int)instance3.nowSelectCategory];
		game.heroineList.Clear();
		instance.females = new ChaControl[2];
		instance.pngFemales[0] = "";
		instance.pngFemales[1] = "";
		instance.mapID = game.mapNo;
		PlayerCharaSaveInfo playerChara = game.saveData.playerChara;
		instance.pngMale = playerChara.FileName;
		instance.bFutanari = playerChara.Futanari;
		instance.pngMaleSecond = "";
		instance.bFutanariSecond = false;
		instance2.advDelivery.Set("0", planInfo.rpPartnerChara.Value.personality, game.eventNo);
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "ADV",
			fadeType = FadeCanvas.Fade.In,
			onLoad = delegate
			{
				Singleton<Character>.Instance.DeleteCharaAll();
			}
		}, isLoadingImageDraw: true);
	}
}
