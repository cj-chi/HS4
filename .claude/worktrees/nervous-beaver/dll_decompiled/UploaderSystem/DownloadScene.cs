using System.Collections;
using Illusion.Game;
using Manager;
using MyLocalize;
using UniRx;
using UnityEngine;

namespace UploaderSystem;

public class DownloadScene : MonoBehaviour
{
	public DownPhpControl phpCtrl;

	public DownUIControl uiCtrl;

	[SerializeField]
	private DownloaderCultureControl cultureCtrl;

	private NetworkInfo netInfo => Singleton<NetworkInfo>.Instance;

	private NetCacheControl cacheCtrl
	{
		get
		{
			if (!Singleton<NetworkInfo>.IsInstance())
			{
				return null;
			}
			return netInfo.cacheCtrl;
		}
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		if (null != cultureCtrl)
		{
			cultureCtrl.ChangeLocalize(MyLocalizeDefine.LocalizeKeyType.Downloader, Singleton<GameSystem>.Instance.languageInt);
		}
		Observable.FromCoroutine(() => phpCtrl.GetBaseInfo(upload: false)).Subscribe(delegate
		{
		}, delegate
		{
		}, delegate
		{
			uiCtrl.changeSearchSetting = true;
		});
		Utils.Sound.Play(new Utils.Sound.SettingBGM(BGM.network));
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
		base.enabled = true;
	}

	private void Update()
	{
	}
}
