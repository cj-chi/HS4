using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayfulSystems.LoadingScreen;

public abstract class LoadingScreenProBase : MonoBehaviour
{
	public enum BehaviorAfterLoad
	{
		WaitForPlayerInput,
		ContinueAutomatically
	}

	private static int targetSceneIndex = -1;

	private static string targetSceneName = null;

	[Tooltip("Central Config asset.")]
	public LoadingScreenConfig config;

	[Tooltip("If loading additively, set reference to this scene's camera's audio listener, to avoid multiple active audio listeners at any one time. The script will try to auto set this for your convenience.")]
	public AudioListener audioListener;

	[Header("Timing Settings")]
	public BehaviorAfterLoad behaviorAfterLoad;

	[Tooltip("After finishing Loading, wait this much before showing the completion visuals.")]
	public float timeToAutoContinue = 0.25f;

	private AsyncOperation operation;

	private float previousTimescale;

	private float currentProgress;

	private Scene currentScene;

	public static void LoadScene(int levelNum)
	{
		if (!IsLegalLevelIndex(levelNum))
		{
			targetSceneName = null;
			return;
		}
		targetSceneIndex = levelNum;
		targetSceneName = null;
		LoadLoadingScene();
	}

	private static bool IsLegalLevelIndex(int levelNum)
	{
		if (levelNum < 0)
		{
			return levelNum < SceneManager.sceneCountInBuildSettings;
		}
		return true;
	}

	public static void LoadScene(string levelName)
	{
		targetSceneIndex = -1;
		targetSceneName = levelName;
		LoadLoadingScene();
	}

	private static void LoadLoadingScene()
	{
		Application.backgroundLoadingPriority = ThreadPriority.High;
		SceneManager.LoadScene(LoadingScreenConfig.loadingSceneName);
	}

	protected virtual void Start()
	{
		if ((targetSceneName != null || targetSceneIndex != -1) && !(config == null))
		{
			previousTimescale = Time.timeScale;
			Time.timeScale = 1f;
			currentScene = SceneManager.GetActiveScene();
			Init();
			Application.backgroundLoadingPriority = config.loadThreadPriority;
			StartCoroutine(LoadAsync(targetSceneIndex, targetSceneName));
		}
	}

	protected virtual void Init()
	{
	}

	private IEnumerator LoadAsync(int levelNum, string levelName)
	{
		ShowTips();
		ShowSceneInfos();
		ShowStartingVisuals();
		currentProgress = 0f;
		operation = StartOperation(levelNum, levelName);
		if (operation == null)
		{
			yield break;
		}
		operation.allowSceneActivation = CanLoadAdditively();
		while (!IsDoneLoading())
		{
			yield return null;
			SetProgress(operation.progress);
		}
		if (CanLoadAdditively() && audioListener != null)
		{
			audioListener.enabled = false;
		}
		yield return null;
		SetProgress(1f);
		while (!CanShowConfirmation())
		{
			yield return null;
		}
		ShowLoadingDoneVisuals();
		if (behaviorAfterLoad == BehaviorAfterLoad.WaitForPlayerInput)
		{
			while (!Input.anyKey)
			{
				yield return null;
			}
		}
		else
		{
			yield return new WaitForSeconds(timeToAutoContinue);
		}
		ShowEndingVisuals();
		while (!CanActivateTargetScene())
		{
			yield return null;
		}
		ActivateLoadedScene();
	}

	private void SetProgress(float progress)
	{
		if (!(progress <= currentProgress))
		{
			ShowProgressVisuals(progress);
			currentProgress = progress;
		}
	}

	private AsyncOperation StartOperation(int levelNum, string levelName)
	{
		LoadSceneMode mode = (CanLoadAdditively() ? LoadSceneMode.Additive : LoadSceneMode.Single);
		if (string.IsNullOrEmpty(levelName))
		{
			return SceneManager.LoadSceneAsync(levelNum, mode);
		}
		return SceneManager.LoadSceneAsync(levelName, mode);
	}

	private bool CanLoadAdditively()
	{
		return config.loadAdditively;
	}

	protected bool CanLoadAsynchronously()
	{
		return true;
	}

	private bool IsDoneLoading()
	{
		if (CanLoadAdditively())
		{
			return operation.isDone;
		}
		return operation.progress >= 0.9f;
	}

	private void ActivateLoadedScene()
	{
		targetSceneIndex = -1;
		targetSceneName = null;
		if (CanLoadAdditively())
		{
			SceneManager.UnloadSceneAsync(currentScene);
		}
		operation.allowSceneActivation = true;
		Resources.UnloadUnusedAssets();
		Time.timeScale = previousTimescale;
	}

	private void ShowSceneInfos()
	{
		if (!config.showSceneInfos || config.sceneInfos == null || config.sceneInfos.Length == 0)
		{
			DisplaySceneInfo(null);
		}
		else
		{
			DisplaySceneInfo(config.GetSceneInfo(targetSceneName));
		}
	}

	protected virtual void DisplaySceneInfo(SceneInfo info)
	{
	}

	private void ShowTips()
	{
		if (!config.showRandomTip || config.gameTips == null || config.gameTips.Length == 0)
		{
			DisplayGameTip(null);
		}
		else
		{
			DisplayGameTip(config.GetGameTip());
		}
	}

	protected virtual void DisplayGameTip(LoadingTip tip)
	{
	}

	protected virtual void ShowStartingVisuals()
	{
	}

	protected virtual void ShowProgressVisuals(float progress)
	{
	}

	protected virtual void ShowLoadingDoneVisuals()
	{
	}

	protected virtual void ShowEndingVisuals()
	{
	}

	protected virtual bool CanShowConfirmation()
	{
		return true;
	}

	protected virtual bool CanActivateTargetScene()
	{
		return true;
	}
}
