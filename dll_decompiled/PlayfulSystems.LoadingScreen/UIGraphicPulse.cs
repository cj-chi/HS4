using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.LoadingScreen;

[RequireComponent(typeof(Graphic))]
public class UIGraphicPulse : MonoBehaviour
{
	public Graphic gfx;

	public bool doPulse = true;

	public Color defaultColor = Color.white;

	public Color pulseColor = Color.grey;

	public float pulseDuration = 2f;

	private bool isPulsing;

	private float pulseTime;

	private void Update()
	{
		if (isPulsing != doPulse)
		{
			SetPulsing(doPulse);
		}
		if (isPulsing)
		{
			pulseTime += Time.deltaTime;
			gfx.color = Color.Lerp(defaultColor, pulseColor, GetAlpha());
		}
	}

	private void SetPulsing(bool state)
	{
		isPulsing = state;
		if (!isPulsing)
		{
			gfx.color = defaultColor;
		}
		else
		{
			pulseTime = 0f;
		}
	}

	private float GetAlpha()
	{
		return 0.5f + 0.5f * Mathf.Sin((pulseTime + pulseDuration / 4f) / (float)Math.PI * 20f / pulseDuration);
	}
}
