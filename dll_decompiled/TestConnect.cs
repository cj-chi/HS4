using System.Collections;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TestConnect : MonoBehaviour
{
	[SerializeField]
	private Button btnDownloaderAIS;

	[SerializeField]
	private Button btnUploaderHS2;

	[SerializeField]
	private Button btnDownloaderHS2;

	[SerializeField]
	private Button btnDownloaderAIS_Direct;

	[SerializeField]
	private Button btnUploaderHS2_Direct;

	[SerializeField]
	private Button btnDownloaderHS2_Direct;

	private bool next;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		if (null != btnDownloaderAIS)
		{
			(from x in btnDownloaderAIS.OnClickAsObservable()
				where !next
				select x).Subscribe(delegate
			{
				Singleton<GameSystem>.Instance.networkSceneName = "Downloader";
				Singleton<GameSystem>.Instance.networkType = 1;
				ChangeScene();
			});
		}
		if (null != btnUploaderHS2)
		{
			(from x in btnUploaderHS2.OnClickAsObservable()
				where !next
				select x).Subscribe(delegate
			{
				Singleton<GameSystem>.Instance.networkSceneName = "Uploader";
				Singleton<GameSystem>.Instance.networkType = 0;
				ChangeScene();
			});
		}
		if (null != btnDownloaderHS2)
		{
			(from x in btnDownloaderHS2.OnClickAsObservable()
				where !next
				select x).Subscribe(delegate
			{
				Singleton<GameSystem>.Instance.networkSceneName = "Downloader";
				Singleton<GameSystem>.Instance.networkType = 0;
				ChangeScene();
			});
		}
		if (null != btnDownloaderAIS_Direct)
		{
			(from x in btnDownloaderAIS_Direct.OnClickAsObservable()
				where !next
				select x).Subscribe(delegate
			{
				Singleton<GameSystem>.Instance.networkSceneName = "Downloader";
				Singleton<GameSystem>.Instance.networkType = 1;
				ChangeSceneDirect("Downloader");
			});
		}
		if (null != btnUploaderHS2_Direct)
		{
			(from x in btnUploaderHS2_Direct.OnClickAsObservable()
				where !next
				select x).Subscribe(delegate
			{
				Singleton<GameSystem>.Instance.networkSceneName = "Uploader";
				Singleton<GameSystem>.Instance.networkType = 0;
				ChangeSceneDirect("Uploader");
			});
		}
		if (null != btnDownloaderHS2_Direct)
		{
			(from x in btnDownloaderHS2_Direct.OnClickAsObservable()
				where !next
				select x).Subscribe(delegate
			{
				Singleton<GameSystem>.Instance.networkSceneName = "Downloader";
				Singleton<GameSystem>.Instance.networkType = 0;
				ChangeSceneDirect("Downloader");
			});
		}
	}

	private void ChangeScene()
	{
		if (!Singleton<GameSystem>.Instance.HandleName.IsNullOrEmpty())
		{
			Scene.LoadReserveAsyncForget(new Scene.Data
			{
				levelName = "NetworkCheckScene",
				isAdd = false,
				isFade = true
			}, isLoadingImageDraw: true);
		}
		else
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "EntryHandleName",
				isAdd = false,
				isFade = true
			}, isLoadingImageDraw: true);
		}
		next = true;
	}

	private void ChangeSceneDirect(string sceneName)
	{
		Scene.LoadReserveAsyncForget(new Scene.Data
		{
			levelName = sceneName,
			isAdd = false,
			isFade = true
		}, isLoadingImageDraw: true);
		next = true;
	}
}
