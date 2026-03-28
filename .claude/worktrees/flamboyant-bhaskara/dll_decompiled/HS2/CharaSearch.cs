using System;
using System.Collections;
using Illusion.CustomAttributes;
using Manager;
using MyLocalize;
using UnityEngine;

namespace HS2;

[DefaultExecutionOrder(-1)]
public class CharaSearch : MonoBehaviour
{
	[Button("ChangeInert", "惰性通常", new object[] { false })]
	public int inert00;

	[Button("ChangeInert", "惰性強", new object[] { true })]
	public int inert01;

	public static bool modeNew = true;

	public static string nextScene = "";

	public static string editCharaFileName = "";

	public static Action actEixt = null;

	[SerializeField]
	private CustomCultureControl cultureCtrl;

	[SerializeField]
	private SearchControl searchCtrl;

	[SerializeField]
	private CanvasGroup cgScene;

	private float shadowDistance = 400f;

	private int backLimit;

	private void ChangeInert(bool h)
	{
		if (null != Singleton<SearchBase>.Instance.chaCtrl)
		{
			Singleton<SearchBase>.Instance.chaCtrl.ChangeBustInert(h);
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
		Singleton<SearchBase>.Instance.cultureControl = cultureCtrl;
		yield return StartCoroutine(searchCtrl.Initialize(nextScene, editCharaFileName));
		if (null != cultureCtrl)
		{
			cultureCtrl.ChangeLocalize(MyLocalizeDefine.LocalizeKeyType.CharaCustom, Singleton<GameSystem>.Instance.languageInt);
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
		if (Singleton<SearchBase>.IsInstance())
		{
			Singleton<SearchBase>.Instance.customSettingSave.Save();
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
		actEixt?.Invoke();
		actEixt = null;
	}
}
