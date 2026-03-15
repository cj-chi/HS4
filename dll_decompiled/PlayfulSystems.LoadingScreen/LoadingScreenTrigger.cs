using UnityEngine;

namespace PlayfulSystems.LoadingScreen;

public class LoadingScreenTrigger : MonoBehaviour
{
	public enum SceneReference
	{
		Number,
		Name
	}

	public bool loadOnStart = true;

	public SceneReference loadSceneFrom;

	public int sceneNumber;

	public string sceneName;

	private void Start()
	{
		if (loadOnStart)
		{
			TriggerLoadScene();
		}
	}

	public void TriggerLoadScene()
	{
		if (loadSceneFrom == SceneReference.Number)
		{
			LoadingScreenProBase.LoadScene(sceneNumber);
		}
		else
		{
			LoadingScreenProBase.LoadScene(sceneName);
		}
	}

	public void LoadScene(int number)
	{
		LoadingScreenProBase.LoadScene(number);
	}

	public void LoadScene(string name)
	{
		LoadingScreenProBase.LoadScene(name);
	}
}
