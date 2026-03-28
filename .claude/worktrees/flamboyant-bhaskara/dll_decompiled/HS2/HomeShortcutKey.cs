using System.Collections;
using System.Linq;
using Config;
using Manager;
using Tutorial2D;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace HS2;

public class HomeShortcutKey : MonoBehaviour
{
	private IEnumerator Start()
	{
		base.enabled = false;
		while (!SingletonInitializer<Scene>.initialized)
		{
			yield return null;
		}
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		(from _ in this.UpdateAsObservable()
			where !Scene.IsFadeNow
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			select _).Subscribe(delegate
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ExitDialog.GameEnd(isCheck: true);
			}
			else if (Input.GetKeyDown(KeyCode.F1))
			{
				ConfigWindow.Load();
			}
			else if (Input.GetKeyDown(KeyCode.F2))
			{
				ShortcutViewDialog.Load();
			}
			else if (Input.GetKeyDown(KeyCode.F3) && Singleton<Game>.Instance.saveData.TutorialNo == -1)
			{
				SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = true;
				SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = Singleton<HomeSceneManager>.Instance.HelpPage;
				global::Tutorial2D.Tutorial2D.Load();
			}
		});
		base.enabled = true;
	}
}
