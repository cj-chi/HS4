using System;
using System.Collections;
using System.Collections.Generic;
using AIChara;
using CharaCustom;
using GameLoadCharaFileSystem;
using Illusion.Component.UI;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class GroupCharaParameterUI : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private Text txtCharaName;

	[SerializeField]
	private RawImage riCard;

	[SerializeField]
	private Button[] btnFilter = new Button[5];

	[SerializeField]
	private SpriteChangeCtrl spriteCtrl;

	[SerializeField]
	private Text txtPersonality;

	[SerializeField]
	private Text txtTrait;

	[SerializeField]
	private Text txtHAttribute;

	[SerializeField]
	private Image imgHBar;

	[SerializeField]
	private SpriteChangeCtrl sccHBar;

	[SerializeField]
	private Text txtH;

	[SerializeField]
	private Image imgPainBar;

	[SerializeField]
	private SpriteChangeCtrl sccPainBar;

	[SerializeField]
	private Text txtPain;

	[SerializeField]
	private Image imgHipBar;

	[SerializeField]
	private SpriteChangeCtrl sccHipBar;

	[SerializeField]
	private Text txtHip;

	[SerializeField]
	private Image imgBrokenBar;

	[SerializeField]
	private Image imgDependenceBar;

	[SerializeField]
	private MenuItemUI itemUICharaEdit;

	[SerializeField]
	private MenuItemUI itemUICharaReset;

	[SerializeField]
	private int drawSelectUI;

	private readonly string[][] strResist = new string[2][]
	{
		new string[5] { "慣れ", "Nare", "", "", "" },
		new string[5] { "不慣れ", "Funare", "", "", "" }
	};

	private readonly string[] strReset = new string[5] { "ステータスをリセットしますか？", "Reset Status？", "Reset Status？", "Reset Status？", "Reset Status？" };

	private readonly string[] strCustom = new string[6] { "キャラのメイク画面に移動します\nよろしいですか？", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "Go to the character makeup screen.\nIs it OK?", "" };

	private GameCharaFileInfo selectInfo;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		itemUICharaEdit.btn.OnClickAsObservable().Subscribe(delegate
		{
			if (selectInfo != null)
			{
				Utils.Sound.Play(SystemSE.ok_s);
				ConfirmDialog.Status status = ConfirmDialog.status;
				status.Sentence = strCustom[Singleton<GameSystem>.Instance.languageInt];
				status.Yes = delegate
				{
					Utils.Sound.Play(SystemSE.ok_l);
					Singleton<HomeSceneManager>.Instance.CharaEventSet();
					global::CharaCustom.CharaCustom.modeNew = false;
					global::CharaCustom.CharaCustom.modeSex = 1;
					global::CharaCustom.CharaCustom.editCharaFileName = selectInfo.FileName;
					Scene.LoadReserve(new Scene.Data
					{
						levelName = "CharaCustom",
						fadeType = FadeCanvas.Fade.In
					}, isLoadingImageDraw: true);
					HomeScene.startCanvas = 2;
					if (drawSelectUI == 0)
					{
						HomeScene.startCharaEdit = 2;
					}
					else
					{
						HomeScene.startCharaEdit = 3;
					}
				};
				status.No = delegate
				{
					Utils.Sound.Play(SystemSE.cancel);
				};
				ConfirmDialog.Load();
			}
		});
		itemUICharaEdit.btn.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
			if (itemUICharaEdit.btn.IsInteractable())
			{
				itemUICharaEdit.texts.ForEach(delegate(Text t)
				{
					t.color = Game.selectFontColor;
				});
			}
		});
		itemUICharaEdit.btn.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (itemUICharaEdit.btn.IsInteractable())
			{
				itemUICharaEdit.texts.ForEach(delegate(Text t)
				{
					t.color = Game.defaultFontColor;
				});
			}
		});
		itemUICharaReset.btn?.OnClickAsObservable().Subscribe(delegate
		{
			if (selectInfo != null)
			{
				Utils.Sound.Play(SystemSE.ok_s);
				ConfirmDialog.Status status = ConfirmDialog.status;
				status.Sentence = strReset[Singleton<GameSystem>.Instance.languageInt];
				status.Yes = delegate
				{
					Utils.Sound.Play(SystemSE.ok_s);
					ChaFileControl chaFileControl = new ChaFileControl();
					chaFileControl.LoadCharaFile(selectInfo.FileName, 1);
					ChaFileGameInfo2 gameinfo = chaFileControl.gameinfo2;
					gameinfo.MemberInit();
					chaFileControl.InitGameInfoParam();
					chaFileControl.SaveCharaFile(selectInfo.FileName, 1);
					selectInfo.state = gameinfo.nowDrawState;
					selectInfo.resistH = gameinfo.resistH;
					selectInfo.resistPain = gameinfo.resistPain;
					selectInfo.resistAnal = gameinfo.resistAnal;
					selectInfo.broken = gameinfo.Broken;
					selectInfo.dependence = gameinfo.Dependence;
					selectInfo.usedItem = gameinfo.usedItem;
					selectInfo.lockNowState = gameinfo.lockNowState;
					selectInfo.lockBroken = gameinfo.lockBroken;
					selectInfo.lockDependence = gameinfo.lockDependence;
					selectInfo.hcount = gameinfo.hCount;
					SetParameter(selectInfo);
					Singleton<HomeSceneManager>.Instance.CharaEventSetPoint(selectInfo.FileName);
				};
				status.No = delegate
				{
					Utils.Sound.Play(SystemSE.cancel);
				};
				ConfirmDialog.Load();
			}
		});
		itemUICharaReset.btn?.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
		});
		itemUICharaReset.btn?.OnPointerExitAsObservable().Subscribe(delegate
		{
		});
		base.enabled = true;
	}

	public void SetParameter(GameCharaFileInfo _info)
	{
		if (!base.enabled)
		{
			Singleton<HomeSceneManager>.Instance.StartCoroutine(Start());
		}
		selectInfo = _info;
		base.gameObject.SetActiveIfDifferent(active: true);
		txtCharaName.text = _info.name;
		if ((bool)riCard.texture)
		{
			UnityEngine.Object.Destroy(riCard.texture);
			riCard.texture = null;
		}
		riCard.texture = PngAssist.ChangeTextureFromByte(_info.pngData ?? PngFile.LoadPngBytes(_info.FullPath));
		for (int i = 0; i < btnFilter.Length; i++)
		{
			btnFilter[i].interactable = _info.lstFilter.Contains(i + 1);
		}
		spriteCtrl.ChangeValue(GlobalHS2Calc.GetStateIconNum((int)_info.state, _info.voice));
		txtPersonality.text = _info.personality;
		txtTrait.text = Game.infoTraitTable[_info.trait];
		txtHAttribute.text = Game.infoHAttributeTable[_info.hAttribute];
		imgHBar.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _info.resistH));
		sccHBar.ChangeValue((_info.resistH == 100) ? 1 : 0);
		string[] array = new string[2]
		{
			strResist[0][Singleton<GameSystem>.Instance.languageInt],
			strResist[1][Singleton<GameSystem>.Instance.languageInt]
		};
		txtH.text = ((_info.resistH >= 100) ? array[0] : array[1]);
		imgPainBar.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _info.resistPain));
		sccPainBar.ChangeValue((_info.resistPain == 100) ? 1 : 0);
		txtPain.text = ((_info.resistPain >= 100) ? array[0] : array[1]);
		imgHipBar.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _info.resistAnal));
		sccHipBar.ChangeValue((_info.resistAnal == 100) ? 1 : 0);
		txtHip.text = ((_info.resistAnal >= 100) ? array[0] : array[1]);
		imgBrokenBar.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _info.broken));
		imgDependenceBar.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _info.dependence));
	}

	public void InitParameter()
	{
		base.gameObject.SetActiveIfDifferent(active: false);
	}
}
