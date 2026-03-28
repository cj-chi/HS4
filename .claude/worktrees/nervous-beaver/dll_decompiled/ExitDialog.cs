using System;
using System.Collections.Generic;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExitDialog : SingletonInitializer<ExitDialog>, Scene.IOverlap
{
	[SerializeField]
	private int _priority;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Button _yes;

	[SerializeField]
	private Button _no;

	[SerializeField]
	private Image _clickBG;

	[SerializeField]
	private FadeCanvas _fadeCanvas;

	public static bool active { get; private set; }

	public static bool isGameEnd { get; private set; } = false;

	private static bool _isGameEndCheck { get; set; } = true;

	private static string[] _checkScenes { get; } = new string[2] { "Init", "Logo" };

	int Scene.IOverlap.order { get; set; }

	int Scene.IOverlap.priority => _priority;

	Canvas Scene.IOverlap.canvas => _canvas;

	private float timeScale { get; set; } = 1f;

	public static void Load()
	{
		Scene.Add(SingletonInitializer<ExitDialog>.instance);
	}

	public static void GameEnd(bool isCheck)
	{
		_isGameEndCheck = isCheck;
		Application.Quit();
	}

	void Scene.IOverlap.AddEvent()
	{
		_no.image.raycastTarget = true;
		StartFade(FadeCanvas.Fade.In, delegate
		{
			timeScale = Time.timeScale;
			Time.timeScale = 0f;
			active = true;
		}).Forget();
	}

	void Scene.IOverlap.RemoveEvent()
	{
		active = false;
		_no.image.raycastTarget = false;
		StartFade(FadeCanvas.Fade.Out, delegate
		{
			Time.timeScale = timeScale;
		}).Forget();
	}

	protected override void Initialize()
	{
		_yes.OnClickAsObservable().Take(1).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_l);
			GameEnd(isCheck: false);
		});
		(from _ in _no.OnClickAsObservable()
			where active
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			Scene.Remove(this);
		});
		List<Button> list = new List<Button>();
		list.Add(_yes);
		list.Add(_no);
		list.ForEach(delegate(Button b)
		{
			b.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		});
		if (_clickBG != null)
		{
			(from data in _clickBG.OnPointerClickAsObservable()
				where active
				where data.button == PointerEventData.InputButton.Right
				select data).Subscribe(delegate
			{
				_no.onClick.Invoke();
			});
		}
	}

	private async UniTask StartFade(FadeCanvas.Fade type, Action onEnd)
	{
		await _fadeCanvas.StartFadeAysnc(type);
		onEnd();
	}
}
