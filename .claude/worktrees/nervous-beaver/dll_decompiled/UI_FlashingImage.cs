using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_FlashingImage : MonoBehaviour
{
	[SerializeField]
	[Range(0.1f, 5f)]
	private float span = 0.1f;

	private Image image;

	private void Start()
	{
		image = GetComponent<Image>();
		StartCoroutine(DoFlashing());
	}

	private IEnumerator DoFlashing()
	{
		float cnt = 0f;
		span = Mathf.Min(span, 0.1f);
		while (true)
		{
			cnt += Time.deltaTime * (180f / span);
			if (cnt >= 180f)
			{
				cnt -= 180f;
			}
			float a = Mathf.Sin(cnt * ((float)Math.PI / 180f));
			image.color = new Color(image.color.r, image.color.g, image.color.b, a);
			yield return null;
		}
	}
}
