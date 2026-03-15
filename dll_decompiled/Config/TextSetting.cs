using System;
using System.Threading;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class TextSetting : BaseSetting
{
	[Header("文字表示速度")]
	[SerializeField]
	private Slider fontSpeedSlider;

	[Header("文字表示サンプル")]
	[SerializeField]
	private TypefaceAnimatorEx[] ta;

	[Header("TextWindow")]
	[SerializeField]
	private Image imgTextWindow;

	[Header("Windowアルファスライダー")]
	[SerializeField]
	private Slider textWindowAlphaSlider;

	[SerializeField]
	private float autoTime = 3f;

	private IDisposable cancel;

	private void OnDestroy()
	{
		Release();
	}

	private void Release()
	{
		if (cancel != null)
		{
			cancel.Dispose();
		}
	}

	public override void Init()
	{
		TextSystem data = Manager.Config.TextData;
		LinkSlider(fontSpeedSlider, delegate(float value)
		{
			data.FontSpeed = (int)value;
		});
		(from value in fontSpeedSlider.OnValueChangedAsObservable()
			select (int)value).Subscribe(delegate(int value)
		{
			TypefaceAnimatorEx[] array = ta;
			foreach (TypefaceAnimatorEx typefaceAnimatorEx in array)
			{
				typefaceAnimatorEx.isNoWait = value == 100;
				if (!typefaceAnimatorEx.isNoWait)
				{
					typefaceAnimatorEx.timeMode = TypefaceAnimatorEx.TimeMode.Speed;
					typefaceAnimatorEx.speed = value;
				}
			}
		});
		(from isPlaying in (from _ in this.UpdateAsObservable()
				select ta[0].isPlaying).DistinctUntilChanged()
			where !isPlaying
			select isPlaying).Subscribe(delegate
		{
			if (cancel != null)
			{
				cancel.Dispose();
			}
			float autoWaitTimer = 0f;
			cancel = Observable.FromCoroutine((CancellationToken __) => new WaitWhile(delegate
			{
				float num = autoTime;
				autoWaitTimer = Mathf.Min(autoWaitTimer + Time.unscaledDeltaTime, num);
				return autoWaitTimer < num;
			})).Subscribe(delegate
			{
				TypefaceAnimatorEx[] array = ta;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Play();
				}
			});
		});
		textWindowAlphaSlider.onValueChanged.AddListener(delegate(float value)
		{
			data.WindowAlpha = value;
			Color color = imgTextWindow.color;
			color.a = value;
			imgTextWindow.color = color;
		});
	}

	protected override void ValueToUI()
	{
		TextSystem textData = Manager.Config.TextData;
		fontSpeedSlider.value = textData.FontSpeed;
		textWindowAlphaSlider.value = textData.WindowAlpha;
	}
}
