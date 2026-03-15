using System;
using UnityEngine;

namespace PlayfulSystems.LoadingScreen;

[Serializable]
public class SceneInfo
{
	public string sceneName;

	[Tooltip("Images are loaded from Resources/ScenePreviews/. Leave empty to keep default background in Loading Scene.")]
	public string imageName;

	public string header;

	[Multiline(4)]
	public string description;
}
