using System.Collections;
using Manager;
using UniRx.Async;
using UnityEngine;

namespace Studio;

public class StartScene : MonoBehaviour
{
	private IEnumerator LoadCoroutine()
	{
		yield return new WaitWhile(() => !AssetBundleManager.initialized);
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		yield return new WaitWhile(() => Scene.IsFadeNow);
		yield return Singleton<Info>.Instance.LoadExcelDataCoroutine();
		yield return null;
		Scene.sceneFadeCanvas.SetColor(Color.black);
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "Studio",
			isFade = true,
			onFadeOut = () => UniTask.Delay(500)
		}, isLoadingImageDraw: false);
	}

	private void Start()
	{
		StartCoroutine(LoadCoroutine());
	}
}
