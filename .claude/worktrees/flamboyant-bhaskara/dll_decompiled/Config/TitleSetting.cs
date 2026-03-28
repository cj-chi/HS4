using System.IO;
using AIChara;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class TitleSetting : BaseSetting
{
	[Header("タイトルにキャラを立たせるか")]
	[SerializeField]
	private Toggle[] titleCharaLoadToggles;

	[Header("ユーザーのキャラカードを使うか")]
	[SerializeField]
	private Toggle useUserCharaCard;

	[Header("ユーザーキャラカードのファイルまでのフルパス")]
	[SerializeField]
	private Button charaCardFileNameButton;

	[SerializeField]
	private Text charaCardFileNameText;

	[SerializeField]
	private GameObject charaCardFileNameObj;

	[SerializeField]
	private RawImage charaCardImage;

	[SerializeField]
	private Texture2D texNoSelect;

	[Header("キャラ選択関連")]
	[SerializeField]
	private CanvasGroup cgCharaSelect;

	[SerializeField]
	private ConfigCharaSelectUI charaSelectUI;

	private readonly string[] strSelect = new string[5] { "表示する女の子を選択する", "Select girls to show", "Select girls to show", "Select girls to show", "Select girls to show" };

	public override void Init()
	{
		TitleSystem data = Manager.Config.TitleData;
		LinkToggleArray(titleCharaLoadToggles, delegate(int i)
		{
			bool isTitleCharaLoad = i == 0;
			data.isTitleCharaLoad = isTitleCharaLoad;
		});
		LinkToggle(useUserCharaCard, delegate(bool isOn)
		{
			data.isUseUserCharaCard = isOn;
			charaCardFileNameObj.SetActiveIfDifferent(isOn);
		});
		charaCardFileNameButton.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			cgCharaSelect.Enable(enable: true, isUseInteractable: false);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(data.charaCardFileNameFullPath);
			if (fileNameWithoutExtension.IsNullOrEmpty())
			{
				charaSelectUI.ListCtrl.SetTopLine();
			}
			else
			{
				charaSelectUI.ListCtrl.SetToggle(fileNameWithoutExtension, _isSetNowLine: true);
			}
		});
		charaCardFileNameButton.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
			charaCardFileNameText.color = Game.selectFontColor;
		});
		charaCardFileNameButton.OnPointerExitAsObservable().Subscribe(delegate
		{
			charaCardFileNameText.color = Game.defaultFontColor;
		});
		useUserCharaCard.OnValueChangedAsObservable().SubscribeToInteractable(charaCardFileNameButton);
		charaSelectUI.onEntry = delegate(GameCharaFileInfo info)
		{
			data.charaCardFileNameFullPath = info.FullPath;
			charaCardImage.texture = PngAssist.LoadTexture(info.FullPath);
			charaCardFileNameText.text = info.name;
			cgCharaSelect.Enable(enable: false, isUseInteractable: false);
		};
	}

	public override void Setup()
	{
		cgCharaSelect.Enable(enable: false, isUseInteractable: false);
	}

	protected override void ValueToUI()
	{
		TitleSystem data = Manager.Config.TitleData;
		SetToggleUIArray(titleCharaLoadToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? data.isTitleCharaLoad : (!data.isTitleCharaLoad));
		});
		useUserCharaCard.isOn = data.isUseUserCharaCard;
		charaSelectUI.CreateList();
		charaSelectUI.ReDrawListView();
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(data.charaCardFileNameFullPath);
		if (fileNameWithoutExtension.IsNullOrEmpty())
		{
			SetEmpty();
			charaSelectUI.ListCtrl.SetTopLine();
			return;
		}
		ChaFileControl chaFileControl = new ChaFileControl();
		if (chaFileControl.LoadCharaFile(data.charaCardFileNameFullPath, 1))
		{
			charaCardFileNameText.text = chaFileControl.parameter.fullname;
			charaCardImage.texture = PngAssist.LoadTexture(data.charaCardFileNameFullPath);
		}
		else
		{
			SetEmpty();
		}
		charaSelectUI.ListCtrl.SetToggle(fileNameWithoutExtension, _isSetNowLine: true);
		void SetEmpty()
		{
			charaCardFileNameText.text = strSelect[Singleton<GameSystem>.Instance.languageInt];
			charaCardImage.texture = texNoSelect;
		}
	}
}
