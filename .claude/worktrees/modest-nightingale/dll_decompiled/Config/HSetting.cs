using System.Collections.Generic;
using Illusion.Extensions;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class HSetting : BaseSetting
{
	[Header("ホームでナビ子を呼び出したときのイベントをスキップする")]
	[SerializeField]
	private Toggle[] homeCallConciergeEventSkipToggles;

	[Header("主人公の表示")]
	[SerializeField]
	private Toggle[] drawToggles;

	[Header("主人公の男根")]
	[SerializeField]
	private Toggle[] sonToggles;

	[Header("主人公の服")]
	[SerializeField]
	private Toggle[] clothToggles;

	[Header("主人公のアクセサリー")]
	[SerializeField]
	private Toggle[] accessoryToggles;

	[Header("主人公の靴")]
	[SerializeField]
	private Toggle[] shoesToggles;

	[Header("二人目の主人公の表示")]
	[SerializeField]
	private Toggle[] secondDrawToggles;

	[SerializeField]
	private GameObject objSecondDrawToggle;

	[Header("二人目の主人公の男根")]
	[SerializeField]
	private Toggle[] secondSonToggles;

	[SerializeField]
	private GameObject objSecondSonToggle;

	[Header("二人目の主人公の服")]
	[SerializeField]
	private Toggle[] secondClothToggles;

	[SerializeField]
	private GameObject objSecondClothToggle;

	[Header("二人目の主人公のアクセサリー")]
	[SerializeField]
	private Toggle[] secondAccessoryToggles;

	[SerializeField]
	private GameObject objSecondAccessoryToggle;

	[Header("二人目の主人公の靴")]
	[SerializeField]
	private Toggle[] secondShoesToggles;

	[SerializeField]
	private GameObject objSecondShoesToggle;

	[Header("主人公を単色化")]
	[SerializeField]
	private Toggle[] silhouetteToggles;

	[Header("単色")]
	[SerializeField]
	private UI_SampleColor silhouetteCololr;

	[Header("汁描画")]
	[SerializeField]
	private Toggle[] siruDrawToggles;

	[Header("尿描画")]
	[SerializeField]
	private Toggle[] urineDrawToggles;

	[Header("尿")]
	[SerializeField]
	private Toggle[] urineToggles;

	[SerializeField]
	private GameObject objUrineToggle;

	[Header("潮吹き描画")]
	[SerializeField]
	private Toggle[] sioDrawToggles;

	[Header("潮吹き")]
	[SerializeField]
	private Toggle[] sioToggles;

	[SerializeField]
	private GameObject objSioToggles;

	[Header("快感ゲージ")]
	[SerializeField]
	private Toggle[] gaugeToggles;

	[Header("操作ガイド")]
	[SerializeField]
	private Toggle[] guideToggles;

	[Header("カメラ初期化判断")]
	[SerializeField]
	private Toggle[] initCameraToggles;

	[Header("１人目視線")]
	[SerializeField]
	private Toggle[] eyeDir0Toggles;

	[Header("１人目首の向き")]
	[SerializeField]
	private Toggle[] neckDir0Toggles;

	[Header("２人目視線")]
	[SerializeField]
	private Toggle[] eyeDir1Toggles;

	[SerializeField]
	private GameObject objEyeDir1Toggle;

	[Header("２人目首の向き")]
	[SerializeField]
	private Toggle[] neckDir1Toggles;

	[SerializeField]
	private GameObject objNeckDir1Toggle;

	[Header("脱力制御")]
	[SerializeField]
	private Toggle[] weakStopToggles;

	[SerializeField]
	private GameObject objWeakStopToggle;

	[Header("逃走制御")]
	[SerializeField]
	private Toggle[] escapeStopToggles;

	[SerializeField]
	private GameObject objEscapeStopToggles;

	public override void Init()
	{
		HSystem hdata = Manager.Config.HData;
		LinkToggleArray(homeCallConciergeEventSkipToggles, delegate(int i)
		{
			hdata.HomeCallConciergeEventSkip = i == 0;
		});
		LinkToggleArray(drawToggles, delegate(int i)
		{
			hdata.Visible = i == 0;
		});
		LinkToggleArray(sonToggles, delegate(int i)
		{
			hdata.Son = i == 0;
		});
		LinkToggleArray(clothToggles, delegate(int i)
		{
			hdata.Cloth = i == 0;
		});
		LinkToggleArray(accessoryToggles, delegate(int i)
		{
			hdata.Accessory = i == 0;
		});
		LinkToggleArray(shoesToggles, delegate(int i)
		{
			hdata.Shoes = i == 0;
		});
		LinkToggleArray(secondDrawToggles, delegate(int i)
		{
			hdata.SecondVisible = i == 0;
		});
		LinkToggleArray(secondSonToggles, delegate(int i)
		{
			hdata.SecondSon = i == 0;
		});
		LinkToggleArray(secondClothToggles, delegate(int i)
		{
			hdata.SecondCloth = i == 0;
		});
		LinkToggleArray(secondAccessoryToggles, delegate(int i)
		{
			hdata.SecondAccessory = i == 0;
		});
		LinkToggleArray(secondShoesToggles, delegate(int i)
		{
			hdata.SecondShoes = i == 0;
		});
		LinkToggleArray(silhouetteToggles, delegate(int i)
		{
			hdata.SimpleBody = i == 0;
		});
		silhouetteCololr.actUpdateColor = delegate(Color c)
		{
			hdata.SilhouetteColor = c;
		};
		LinkToggleArray(siruDrawToggles, delegate(int i)
		{
			hdata.SiruDraw = i;
		});
		LinkToggleArray(urineDrawToggles, delegate(int i)
		{
			hdata.UrineDraw = i;
		});
		LinkToggleArray(urineToggles, delegate(int i)
		{
			hdata.Urine = i == 1;
		});
		LinkToggleArray(sioDrawToggles, delegate(int i)
		{
			hdata.SioDraw = i;
		});
		LinkToggleArray(sioToggles, delegate(int i)
		{
			hdata.Sio = i == 1;
		});
		LinkToggleArray(gaugeToggles, delegate(int i)
		{
			hdata.FeelingGauge = i == 0;
		});
		LinkToggleArray(guideToggles, delegate(int i)
		{
			hdata.ActionGuide = i == 0;
		});
		LinkToggleArray(initCameraToggles, delegate(int i)
		{
			hdata.InitCamera = i == 0;
		});
		LinkToggleArray(eyeDir0Toggles, delegate(int i)
		{
			hdata.EyeDir0 = i == 0;
		});
		LinkToggleArray(neckDir0Toggles, delegate(int i)
		{
			hdata.NeckDir0 = i == 0;
		});
		LinkToggleArray(eyeDir1Toggles, delegate(int i)
		{
			hdata.EyeDir1 = i == 0;
		});
		LinkToggleArray(neckDir1Toggles, delegate(int i)
		{
			hdata.NeckDir1 = i == 0;
		});
		LinkToggleArray(weakStopToggles, delegate(int i)
		{
			hdata.WeakStop = i == 1;
		});
		LinkToggleArray(escapeStopToggles, delegate(int i)
		{
			hdata.EscapeStop = i == 1;
		});
	}

	protected override void ValueToUI()
	{
		HSystem hdata = Manager.Config.HData;
		SetToggleUIArray(homeCallConciergeEventSkipToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.HomeCallConciergeEventSkip : (!hdata.HomeCallConciergeEventSkip));
		});
		SetToggleUIArray(drawToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.Visible : (!hdata.Visible));
		});
		SetToggleUIArray(sonToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.Son : (!hdata.Son));
		});
		SetToggleUIArray(clothToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.Cloth : (!hdata.Cloth));
		});
		SetToggleUIArray(accessoryToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.Accessory : (!hdata.Accessory));
		});
		SetToggleUIArray(shoesToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.Shoes : (!hdata.Shoes));
		});
		SetToggleUIArray(secondDrawToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.SecondVisible : (!hdata.SecondVisible));
		});
		SetToggleUIArray(secondSonToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.SecondSon : (!hdata.SecondSon));
		});
		SetToggleUIArray(secondClothToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.SecondCloth : (!hdata.SecondCloth));
		});
		SetToggleUIArray(secondAccessoryToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.SecondAccessory : (!hdata.SecondAccessory));
		});
		SetToggleUIArray(secondShoesToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.SecondShoes : (!hdata.SecondShoes));
		});
		SetToggleUIArray(silhouetteToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.SimpleBody : (!hdata.SimpleBody));
		});
		silhouetteCololr.SetColor(hdata.SilhouetteColor);
		SetToggleUIArray(siruDrawToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = index == hdata.SiruDraw;
		});
		SetToggleUIArray(urineDrawToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = index == hdata.UrineDraw;
		});
		SetToggleUIArray(urineToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? (!hdata.Urine) : hdata.Urine);
		});
		SetToggleUIArray(sioDrawToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = index == hdata.SioDraw;
		});
		SetToggleUIArray(sioToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? (!hdata.Sio) : hdata.Sio);
		});
		SetToggleUIArray(gaugeToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.FeelingGauge : (!hdata.FeelingGauge));
		});
		SetToggleUIArray(guideToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.ActionGuide : (!hdata.ActionGuide));
		});
		SetToggleUIArray(initCameraToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.InitCamera : (!hdata.InitCamera));
		});
		SetToggleUIArray(eyeDir0Toggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.EyeDir0 : (!hdata.EyeDir0));
		});
		SetToggleUIArray(neckDir0Toggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.NeckDir0 : (!hdata.NeckDir0));
		});
		SetToggleUIArray(eyeDir1Toggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.EyeDir1 : (!hdata.EyeDir1));
		});
		SetToggleUIArray(neckDir1Toggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? hdata.NeckDir1 : (!hdata.NeckDir1));
		});
		SetToggleUIArray(weakStopToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? (!hdata.WeakStop) : hdata.WeakStop);
		});
		SetToggleUIArray(escapeStopToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? (!hdata.EscapeStop) : hdata.EscapeStop);
		});
		List<GameObject> obj = new List<GameObject> { objSecondDrawToggle, objSecondSonToggle, objSecondClothToggle, objSecondAccessoryToggle, objSecondShoesToggle };
		bool isAchievement = SaveData.IsAchievementExchangeRelease(5);
		obj.ForEach(delegate(GameObject self)
		{
			self.SetActiveIfDifferent(isAchievement);
		});
		List<GameObject> obj2 = new List<GameObject> { objUrineToggle, objSioToggles };
		isAchievement = SaveData.IsAchievementExchangeRelease(8);
		obj2.ForEach(delegate(GameObject self)
		{
			self.SetActiveIfDifferent(isAchievement);
		});
		List<GameObject> obj3 = new List<GameObject> { objEyeDir1Toggle, objNeckDir1Toggle };
		isAchievement = SaveData.IsAchievementExchangeRelease(4);
		obj3.ForEach(delegate(GameObject self)
		{
			self.SetActiveIfDifferent(isAchievement);
		});
		objWeakStopToggle.SetActiveIfDifferent(SaveData.IsAchievementExchangeRelease(6));
		objEscapeStopToggles.SetActiveIfDifferent(SaveData.IsAchievementExchangeRelease(7));
	}
}
