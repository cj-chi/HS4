using Manager;

namespace Illusion.Component;

public class ShortcutKey2 : ShortcutKey
{
	private bool IsReglate(string sceneName)
	{
		if (!SingletonInitializer<Scene>.initialized)
		{
			return true;
		}
		string loadSceneName = Scene.LoadSceneName;
		if (loadSceneName == "Init" || loadSceneName == "Logo")
		{
			return true;
		}
		if (Scene.IsNowLoadingFade)
		{
			return true;
		}
		if (Scene.NowSceneNames.Contains(sceneName))
		{
			return true;
		}
		return false;
	}

	public void _GameEnd()
	{
		if (!IsReglate("Exit"))
		{
			ExitDialog.GameEnd(isCheck: true);
		}
	}
}
