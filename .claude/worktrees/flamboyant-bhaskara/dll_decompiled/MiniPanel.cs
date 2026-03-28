using System;
using System.Collections.Generic;
using System.Linq;
using SpriteToParticlesAsset;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniPanel : MonoBehaviour
{
	public List<SpriteToParticles> PlayableFXs;

	public Button PlayButton;

	public Button PauseButton;

	public Toggle WindButton;

	private int SceneCount = 11;

	public WindZone wind;

	private int currentScene;

	private void Start()
	{
		if (PlayableFXs == null || PlayableFXs.Count <= 0)
		{
			PlayableFXs = UnityEngine.Object.FindObjectsOfType<SpriteToParticles>().ToList();
		}
		if (PlayableFXs == null || PlayableFXs.Count <= 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!wind)
		{
			wind = UnityEngine.Object.FindObjectOfType<WindZone>();
		}
		if (!wind)
		{
			WindButton.gameObject.SetActive(value: false);
		}
		foreach (SpriteToParticles playableFX in PlayableFXs)
		{
			if ((bool)playableFX)
			{
				playableFX.OnAvailableToPlay += BecameAvailableToPlay;
			}
		}
		RefreshButtons();
	}

	public void ReloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void TogglePlay()
	{
		if (PlayableFXs.TrueForAll((SpriteToParticles x) => x.IsPlaying()))
		{
			foreach (SpriteToParticles playableFX in PlayableFXs)
			{
				playableFX.Pause();
			}
		}
		else
		{
			foreach (SpriteToParticles playableFX2 in PlayableFXs)
			{
				playableFX2.Play();
			}
		}
		RefreshButtons();
	}

	public void Stop()
	{
		foreach (SpriteToParticles playableFX in PlayableFXs)
		{
			playableFX.Stop();
		}
		RefreshButtons();
	}

	public void BecameAvailableToPlay()
	{
		RefreshButtons();
	}

	public void RefreshButtons()
	{
		bool flag = PlayableFXs.TrueForAll((SpriteToParticles x) => x.IsPlaying());
		PlayButton.gameObject.SetActive(!flag);
		PauseButton.gameObject.SetActive(flag);
		bool interactable = PlayableFXs.TrueForAll((SpriteToParticles x) => x.IsAvailableToPlay());
		PlayButton.interactable = interactable;
	}

	public void ToggleWind()
	{
		if ((bool)wind)
		{
			wind.gameObject.SetActive(!wind.gameObject.activeInHierarchy);
		}
	}

	public void NextScene()
	{
		currentScene = SceneManager.GetActiveScene().buildIndex;
		currentScene = (currentScene + 1) % SceneCount;
		UnloadCurrentScene();
		Invoke("LoadNextScene", 0.1f);
	}

	public void PreviousScene()
	{
		currentScene = SceneManager.GetActiveScene().buildIndex;
		currentScene = (currentScene - 1 + SceneCount) % SceneCount;
		UnloadCurrentScene();
		Invoke("LoadNextScene", 0.1f);
	}

	private void UnloadCurrentScene()
	{
		foreach (SpriteToParticles playableFX in PlayableFXs)
		{
			UnityEngine.Object.DestroyImmediate(playableFX.gameObject);
		}
		GC.Collect();
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	private void LoadNextScene()
	{
		GC.Collect();
		SceneManager.LoadScene(currentScene);
	}
}
