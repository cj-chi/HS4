using System.Collections;
using System.Linq;
using Config;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace HS2;

public class TitleShortcutKey : MonoBehaviour
{
	[SerializeField]
	private TitleScene titleScene;

	private IEnumerator Start()
	{
		base.enabled = false;
		while (!SingletonInitializer<Scene>.initialized)
		{
			yield return null;
		}
		(from _ in this.UpdateAsObservable()
			where !Scene.IsFadeNow
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			select _).Subscribe(delegate
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ExitDialog.GameEnd(isCheck: true);
			}
			else if (Input.GetKeyDown(KeyCode.F1))
			{
				ConfigWindow.UnLoadAction = delegate
				{
					StartCoroutine(titleScene.ConfigEnd());
				};
				ConfigWindow.Load();
			}
		});
		base.enabled = true;
	}
}
