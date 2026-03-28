using System;
using System.Collections;
using Illusion.CustomAttributes;
using Illusion.Game;
using Manager;
using MyLocalize;
using UnityEngine;

namespace CharaCustom;

[DefaultExecutionOrder(-1)]
public class CharaCustom : MonoBehaviour
{
	[Button("ChangeInert", "惰性通常", new object[] { false })]
	public int inert00;

	[Button("ChangeInert", "惰性強", new object[] { true })]
	public int inert01;

	public static bool modeNew = true;

	public static byte modeSex = 1;

	public static string nextScene = "";

	public static string editCharaFileName = "";

	public static int isConcierge = -1;

	public static bool isPlayer = false;

	public static Action actEixt = null;

	[SerializeField]
	private CustomCultureControl cultureCtrl;

	[SerializeField]
	private CustomControl customCtrl;

	[SerializeField]
	private CanvasGroup cgScene;

	private float shadowDistance = 400f;

	private int backLimit;

	private void ChangeInert(bool h)
	{
		if (null != Singleton<CustomBase>.Instance.chaCtrl)
		{
			Singleton<CustomBase>.Instance.chaCtrl.ChangeBustInert(h);
		}
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		shadowDistance = QualitySettings.shadowDistance;
		backLimit = QualitySettings.masterTextureLimit;
		if (QualitySettings.GetQualityLevel() / 2 == 0)
		{
			QualitySettings.masterTextureLimit = 1;
		}
		else
		{
			QualitySettings.masterTextureLimit = 0;
		}
		Singleton<Character>.Instance.customLoadGCClear = true;
		Singleton<CustomBase>.Instance.cultureControl = cultureCtrl;
		customCtrl.Initialize(modeSex, modeNew, nextScene, editCharaFileName, isConcierge, isPlayer);
		if (null != cultureCtrl)
		{
			cultureCtrl.ChangeLocalize(MyLocalizeDefine.LocalizeKeyType.CharaCustom, Singleton<GameSystem>.Instance.languageInt);
		}
		if (!Manager.Config.TitleData.isCustomBGMChange)
		{
			Utils.Sound.Play(new Utils.Sound.SettingBGM(BGM.custom));
		}
		else
		{
			Utils.Sound.Play(new Utils.Sound.SettingBGM((BGM)Manager.Config.TitleData.customBGMNo));
		}
		cgScene.blocksRaycasts = true;
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
		base.enabled = true;
	}

	private void Update()
	{
		if (QualitySettings.shadowDistance != 120f)
		{
			QualitySettings.shadowDistance = 120f;
		}
	}

	private void OnDestroy()
	{
		if (Singleton<CustomBase>.IsInstance())
		{
			Singleton<CustomBase>.Instance.customSettingSave.Save();
		}
		if (Singleton<Character>.IsInstance())
		{
			Singleton<Character>.Instance.chaListCtrl.SaveItemID();
			Singleton<Character>.Instance.DeleteCharaAll();
			Singleton<Character>.Instance.EndLoadAssetBundle();
			Singleton<Character>.Instance.customLoadGCClear = false;
		}
		QualitySettings.shadowDistance = shadowDistance;
		QualitySettings.masterTextureLimit = backLimit;
		nextScene = "";
		editCharaFileName = "";
		isConcierge = -1;
		isPlayer = false;
		actEixt?.Invoke();
		actEixt = null;
	}
}
