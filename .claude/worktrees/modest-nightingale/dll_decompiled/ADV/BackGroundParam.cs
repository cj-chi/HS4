using System.Linq;
using Illusion.Component.UI;
using Manager;
using UnityEngine;

namespace ADV;

public class BackGroundParam : Illusion.Component.UI.BackGroundParam
{
	public bool visibleAll
	{
		get
		{
			return base.visible;
		}
		set
		{
			base.visible = value;
			visibleFog = !value;
			visibleMap = !value;
		}
	}

	public bool visibleFog
	{
		set
		{
		}
	}

	public bool visibleMap
	{
		set
		{
			Scene.Data baseScene = Scene.baseScene;
			if (baseScene != null)
			{
				GameObject gameObject = Scene.GetRootGameObjects(baseScene.levelName).FirstOrDefault((GameObject p) => p.name == BaseMap.MAP_ROOT_NAME);
				if (gameObject != null)
				{
					gameObject.SetActive(value);
				}
			}
		}
	}
}
