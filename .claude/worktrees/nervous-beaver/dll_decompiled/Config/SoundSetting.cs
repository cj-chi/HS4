using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Config;

public class SoundSetting : BaseSetting
{
	[Serializable]
	private class SoundGroup
	{
		public Toggle toggle;

		public Slider slider;

		public Image image;
	}

	[SerializeField]
	private SoundGroup Master;

	[SerializeField]
	private SoundGroup BGM;

	[SerializeField]
	private SoundGroup ENV;

	[SerializeField]
	private SoundGroup SystemSE;

	[SerializeField]
	private SoundGroup GameSE;

	[Header("カスタム中のBGMを変更する")]
	[SerializeField]
	private Toggle[] cusomBGMToggles;

	[Header("カスタム中のBGMを変更")]
	[SerializeField]
	private Button changeCutomBGMButton;

	[SerializeField]
	private Text changeCutomBGMText;

	[SerializeField]
	private GameObject changeCutomBGMObj;

	[Header("BGM選択関連")]
	[SerializeField]
	private CanvasGroup cgBGMSelect;

	[SerializeField]
	private ConfigBGMSelectUI bgmSelectUI;

	private readonly string[] strSelect = new string[5] { "BGMを選択する", "Select BGM", "Select BGM", "Select BGM", "Select BGM" };

	private void InitSet(SoundGroup sg, SoundData sd)
	{
		sg.toggle.isOn = sd.Switch;
		sg.slider.value = sd.Volume;
	}

	private void InitLink(SoundGroup sg, SoundData sd, bool isSliderEvent)
	{
		LinkToggle(sg.toggle, delegate(bool isOn)
		{
			sd.Switch = isOn;
		});
		sg.toggle.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			sg.image.enabled = !isOn;
		});
		(from b in sg.toggle.OnValueChangedAsObservable()
			select (b)).SubscribeToInteractable(sg.slider);
		if (isSliderEvent)
		{
			LinkSlider(sg.slider, delegate(float value)
			{
				sd.Volume = (int)value;
			});
			return;
		}
		(from _ in sg.slider.OnPointerDownAsObservable()
			where Input.GetMouseButtonDown(0)
			select _).Subscribe(delegate
		{
			EnterSE();
		});
	}

	public override void Init()
	{
		SoundSystem soundData = Manager.Config.SoundData;
		InitLink(Master, soundData.Master, isSliderEvent: true);
		InitLink(ENV, soundData.ENV, isSliderEvent: true);
		InitLink(SystemSE, soundData.SystemSE, isSliderEvent: true);
		InitLink(GameSE, soundData.GameSE, isSliderEvent: true);
		InitLink(BGM, soundData.BGM, isSliderEvent: true);
		TitleSystem title = Manager.Config.TitleData;
		LinkToggleArray(cusomBGMToggles, delegate(int i)
		{
			bool flag = i == 0;
			title.isCustomBGMChange = flag;
			changeCutomBGMObj.SetActiveIfDifferent(flag);
			if (Scene.NowSceneNames.Contains("CharaCustom"))
			{
				if (!title.isCustomBGMChange)
				{
					Utils.Sound.Play(new Utils.Sound.SettingBGM(Illusion.Game.BGM.custom));
				}
				else
				{
					Utils.Sound.Play(new Utils.Sound.SettingBGM((BGM)title.customBGMNo));
				}
			}
		});
		changeCutomBGMButton.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(Illusion.Game.SystemSE.ok_s);
			cgBGMSelect.Enable(enable: true, isUseInteractable: false);
			bgmSelectUI.ListCtrl.SetToggle(title.customBGMNo);
			if (Scene.NowSceneNames.Contains("CharaCustom"))
			{
				Utils.Sound.Play(new Utils.Sound.SettingBGM((BGM)title.customBGMNo));
			}
		});
		changeCutomBGMButton.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(Illusion.Game.SystemSE.sel);
			changeCutomBGMText.color = Game.selectFontColor;
		});
		changeCutomBGMButton.OnPointerExitAsObservable().Subscribe(delegate
		{
			changeCutomBGMText.color = Game.defaultFontColor;
		});
		bgmSelectUI.onEntry = delegate(BGMNameInfo.Param info)
		{
			title.customBGMNo = info.bgmID;
			changeCutomBGMText.text = info.name[Singleton<GameSystem>.Instance.languageInt];
			cgBGMSelect.Enable(enable: false, isUseInteractable: false);
			if (Scene.NowSceneNames.Contains("CharaCustom"))
			{
				Utils.Sound.Play(new Utils.Sound.SettingBGM((BGM)title.customBGMNo));
			}
		};
	}

	protected override void ValueToUI()
	{
		SoundSystem soundData = Manager.Config.SoundData;
		InitSet(Master, soundData.Master);
		InitSet(BGM, soundData.BGM);
		InitSet(ENV, soundData.ENV);
		InitSet(SystemSE, soundData.SystemSE);
		InitSet(GameSE, soundData.GameSE);
		TitleSystem title = Manager.Config.TitleData;
		SetToggleUIArray(cusomBGMToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? title.isCustomBGMChange : (!title.isCustomBGMChange));
		});
		BGMNameInfo.Param value = Game.infoBGMNameTable.FirstOrDefault((KeyValuePair<int, BGMNameInfo.Param> bgm) => bgm.Value.bgmID == title.customBGMNo).Value;
		if (value == null)
		{
			title.customBGMNo = 3;
			value = Game.infoBGMNameTable.FirstOrDefault((KeyValuePair<int, BGMNameInfo.Param> bgm) => bgm.Value.bgmID == title.customBGMNo).Value;
		}
		changeCutomBGMText.text = value.name[Singleton<GameSystem>.Instance.languageInt];
	}
}
