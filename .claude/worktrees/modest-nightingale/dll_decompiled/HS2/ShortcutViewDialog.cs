using System;
using System.Linq;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HS2;

[DefaultExecutionOrder(100)]
public class ShortcutViewDialog : SingletonInitializer<ShortcutViewDialog>, Scene.IOverlap
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private int _priority;

	[SerializeField]
	private FadeCanvas _fadeCanvas;

	private float timeScale = 1f;

	public static Action UnLoadAction;

	private bool isEnd;

	private bool active;

	public static bool isActive;

	public static Func<bool> DefaultCancellation => () => Scene.Remove(SingletonInitializer<ShortcutViewDialog>.instance);

	Canvas Scene.IOverlap.canvas => _canvas;

	int Scene.IOverlap.order { get; set; }

	int Scene.IOverlap.priority => _priority;

	public float timeScaleChange
	{
		set
		{
			timeScale = value;
		}
	}

	public static void Load()
	{
		Scene.Add(SingletonInitializer<ShortcutViewDialog>.instance);
	}

	public void Unload()
	{
		if (!isEnd)
		{
			isEnd = true;
			UnLoadAction?.Invoke();
			DefaultCancellation();
		}
	}

	private void PlaySE(SystemSE se = SystemSE.sel)
	{
		Utils.Sound.Play(se);
	}

	protected override void Initialize()
	{
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F2)
			where active
			where _fadeCanvas.isEnd
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			select _).Subscribe(delegate
		{
			OnBack();
		});
	}

	private void Setup()
	{
		isEnd = false;
		isActive = true;
	}

	public void OnBack()
	{
		if (!Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog))
		{
			Unload();
		}
	}

	void Scene.IOverlap.AddEvent()
	{
		timeScale = Time.timeScale;
		Time.timeScale = 0f;
		Setup();
		if (Singleton<GameCursor>.IsInstance())
		{
			Singleton<GameCursor>.Instance.SetCursorLock(setLockFlag: false);
		}
		StartFade(FadeCanvas.Fade.In, delegate
		{
			active = true;
		}).Forget();
	}

	void Scene.IOverlap.RemoveEvent()
	{
		EventSystem.current.SetSelectedGameObject(null);
		active = false;
		StartFade(FadeCanvas.Fade.Out, delegate
		{
			Time.timeScale = timeScale;
			UnLoadAction = null;
			isActive = false;
		}).Forget();
	}

	private async UniTask StartFade(FadeCanvas.Fade type, Action onEnd)
	{
		await _fadeCanvas.StartFadeAysnc(type);
		onEnd();
	}
}
