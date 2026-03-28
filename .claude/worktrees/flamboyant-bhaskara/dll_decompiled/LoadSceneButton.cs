using PlayfulSystems.LoadingScreen;
using UnityEngine;

public class LoadSceneButton : MonoBehaviour
{
	public string targetSceneName = "TestScene_0";

	public void SetLoadingScene(string sceneName)
	{
		LoadingScreenConfig.loadingSceneName = sceneName;
		CameraFade cameraFade = base.gameObject.AddComponent<CameraFade>();
		cameraFade.Init();
		cameraFade.StartFadeTo(Color.black, 1f, LoadTargetScene);
	}

	private void LoadTargetScene()
	{
		LoadingScreenProBase.LoadScene(targetSceneName);
	}
}
