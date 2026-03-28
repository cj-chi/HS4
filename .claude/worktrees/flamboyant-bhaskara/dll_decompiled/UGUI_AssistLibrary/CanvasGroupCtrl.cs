using System;
using Illusion.Extensions;
using ReMotion;
using UniRx;
using UnityEngine;

namespace UGUI_AssistLibrary;

[Serializable]
public class CanvasGroupCtrl
{
	public CanvasGroup canvasGroup;

	public float fadeTime = 0.2f;

	private bool isFadeOpen;

	private bool isFadeClose;

	public void Enable(bool _isEnable)
	{
		canvasGroup.Enable(_isEnable, isUseInteractable: false);
	}

	public void Open()
	{
		if (!isFadeOpen && (isFadeClose || !(canvasGroup.alpha > 0.98f)))
		{
			canvasGroup.blocksRaycasts = false;
			ObservableEasing.Linear(fadeTime, ignoreTimeScale: true).FrameTimeInterval(ignoreTimeScale: true).Subscribe(delegate(TimeInterval<float> x)
			{
				isFadeOpen = true;
				canvasGroup.alpha = x.Value;
			}, delegate
			{
			}, delegate
			{
				canvasGroup.blocksRaycasts = true;
				isFadeOpen = false;
			});
		}
	}

	public void Close()
	{
		if (!isFadeClose && (isFadeOpen || !(canvasGroup.alpha < 0.02f)))
		{
			canvasGroup.blocksRaycasts = false;
			ObservableEasing.Linear(fadeTime, ignoreTimeScale: true).FrameTimeInterval(ignoreTimeScale: true).Subscribe(delegate(TimeInterval<float> x)
			{
				isFadeClose = true;
				canvasGroup.alpha = 1f - x.Value;
			}, delegate
			{
			}, delegate
			{
				isFadeClose = false;
			});
		}
	}
}
