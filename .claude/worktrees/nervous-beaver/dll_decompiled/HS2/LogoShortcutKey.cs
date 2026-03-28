using System.Collections;
using System.Linq;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace HS2;

public class LogoShortcutKey : MonoBehaviour
{
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
			select _).Subscribe(delegate
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ExitDialog.GameEnd(isCheck: true);
			}
		});
		base.enabled = true;
	}
}
