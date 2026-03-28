using System.Collections;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class NotificationScene : MonoBehaviour
{
	[SerializeField]
	private Image imageMessage;

	[SerializeField]
	private RectTransform transImage;

	public static Sprite spriteMessage = null;

	public static float waitTime = 1f;

	public static float width = 416f;

	public static float height = 48f;

	private IEnumerator NotificationCoroutine()
	{
		yield return new WaitForSecondsRealtime(waitTime);
		Scene.Unload();
	}

	private void Awake()
	{
		imageMessage.sprite = spriteMessage;
		transImage.sizeDelta = new Vector2(width, height);
		StartCoroutine(NotificationCoroutine());
		width = 416f;
		height = 48f;
	}
}
