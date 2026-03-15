using System.Collections;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

internal class EditorRestartHack : MonoBehaviour
{
	private IEnumerator _enumerator;

	private WetStuff _renderer;

	public void Awake()
	{
		base.hideFlags = HideFlags.HideAndDontSave;
	}

	public void Apply(WetStuff decalRenderer)
	{
		Object.DestroyImmediate(this);
	}
}
