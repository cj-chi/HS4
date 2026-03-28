using System;
using UnityEngine;

namespace PlayfulSystems.LoadingScreen;

public class CameraFade : MonoBehaviour
{
	private Action onFadeDone;

	private const int guiDepth = -1000;

	private GUIStyle backgroundStyle;

	private Texture2D fadeTexture;

	private Color currentColor = new Color(0f, 0f, 0f, 0f);

	private Color targetColor = new Color(0f, 0f, 0f, 0f);

	private Color deltaColor = new Color(0f, 0f, 0f, 0f);

	public void Init()
	{
		fadeTexture = new Texture2D(1, 1);
		backgroundStyle = new GUIStyle();
		backgroundStyle.normal.background = fadeTexture;
	}

	private void OnGUI()
	{
		if (currentColor != targetColor)
		{
			if (Mathf.Abs(currentColor.a - targetColor.a) < Mathf.Abs(deltaColor.a) * Time.deltaTime)
			{
				currentColor = targetColor;
				SetColor(currentColor);
				deltaColor = Color.clear;
				if (onFadeDone != null)
				{
					onFadeDone();
				}
			}
			else
			{
				SetColor(currentColor + deltaColor * Time.deltaTime);
			}
		}
		if (currentColor.a > 0f)
		{
			EnableAnim(active: true);
			GUI.depth = -1000;
			GUI.Label(new Rect(-2f, -2f, Screen.width + 4, Screen.height + 4), fadeTexture, backgroundStyle);
		}
		else
		{
			EnableAnim(active: false);
		}
	}

	private void SetColor(Color newColor)
	{
		currentColor = newColor;
		fadeTexture.SetPixel(0, 0, currentColor);
		fadeTexture.Apply();
	}

	public void StartFadeFrom(Color color, float fadeDuration, Action onFinished = null)
	{
		if (fadeDuration > 0f)
		{
			SetColor(color);
			onFadeDone = onFinished;
			targetColor = new Color(color.r, color.g, color.b, 0f);
			SetDeltaColor(fadeDuration);
			EnableAnim(active: true);
		}
	}

	public void StartFadeTo(Color color, float fadeDuration, Action onFinished = null)
	{
		if (fadeDuration > 0f)
		{
			SetColor(new Color(color.r, color.g, color.b, 0f));
			onFadeDone = onFinished;
			targetColor = color;
			SetDeltaColor(fadeDuration);
			EnableAnim(active: true);
		}
	}

	public void StartFadeFromTo(Color colorStart, Color colorEnd, float fadeDuration, Action onFinished = null)
	{
		if (fadeDuration > 0f)
		{
			SetColor(colorStart);
			onFadeDone = onFinished;
			targetColor = colorEnd;
			SetDeltaColor(fadeDuration);
			EnableAnim(active: true);
		}
	}

	private void EnableAnim(bool active)
	{
		base.enabled = active;
	}

	private void SetDeltaColor(float duration)
	{
		deltaColor = (targetColor - currentColor) / duration;
	}

	public bool IsFading()
	{
		if (base.enabled)
		{
			return currentColor != targetColor;
		}
		return false;
	}
}
