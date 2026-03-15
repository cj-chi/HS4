using UnityEngine;

namespace Studio;

public class VoiceEndChecker : MonoBehaviour
{
	public delegate void OnEndFunc();

	public OnEndFunc onEndFunc;

	private void OnDestroy()
	{
		if (!ExitDialog.isGameEnd)
		{
			onEndFunc?.Invoke();
		}
	}
}
