using System.Collections;
using UnityEngine;

namespace PlayfulSystems.LoadingScreen;

public class CanvasGroupFade : MonoBehaviour
{
	private CanvasGroup group;

	public void FadeAlpha(float fromAlpha, float toAlpha, float duration)
	{
		if (group == null)
		{
			group = GetComponent<CanvasGroup>();
		}
		if (group != null)
		{
			if (duration > 0f)
			{
				StopAllCoroutines();
				base.gameObject.SetActive(value: true);
				StartCoroutine(DoFade(fromAlpha, toAlpha, duration));
			}
			else
			{
				group.alpha = toAlpha;
				base.gameObject.SetActive(toAlpha > 0f);
			}
		}
	}

	private IEnumerator DoFade(float fromAlpha, float toAlpha, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			group.alpha = Mathf.Lerp(fromAlpha, toAlpha, time / duration);
			yield return null;
		}
		group.alpha = toAlpha;
		if (toAlpha == 0f)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
