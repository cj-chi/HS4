using UnityEngine;

namespace PlayfulSystems.LoadingScreen;

public class LoadingScreenConfig : ScriptableObject
{
	public static string loadingSceneName = "PS-LoadingScene_5";

	[Header("Loading Behavior")]
	[Tooltip("Loading additively means that the scene is loaded in the background in addition to the loading scene and is then turned off as the loading scene is unloaded.")]
	public bool loadAdditively;

	[Tooltip("Lower priority means a background operation will run less often and will take up less time, but will progress more slowly.")]
	public ThreadPriority loadThreadPriority;

	[Header("Scene Infos")]
	public bool showSceneInfos;

	public SceneInfo[] sceneInfos;

	[Header("Game Tips")]
	public bool showRandomTip = true;

	public LoadingTip[] gameTips;

	public virtual SceneInfo GetSceneInfo(string targetSceneName)
	{
		for (int i = 0; i < sceneInfos.Length; i++)
		{
			if (sceneInfos[i].sceneName == targetSceneName)
			{
				return sceneInfos[i];
			}
		}
		return null;
	}

	public virtual LoadingTip GetGameTip()
	{
		return gameTips[Random.Range(0, gameTips.Length)];
	}
}
