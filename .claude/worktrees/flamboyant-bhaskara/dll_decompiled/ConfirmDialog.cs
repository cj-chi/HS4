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

public class ConfirmDialog : SingletonInitializer<ConfirmDialog>, Scene.IOverlap
{
	public class Status
	{
		public string Sentence { get; set; }

		public Action Init { get; set; }

		public Action End { get; set; }

		public Action Yes { get; set; }

		public Action No { get; set; }

		public string YesText { get; set; }

		public string NoText { get; set; }

		public void Clear()
		{
			Sentence = null;
			Init = null;
			End = null;
			Yes = null;
			No = null;
			YesText = null;
			NoText = null;
		}
	}

	[SerializeField]
	private int _priority;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Text _sentence;

	[SerializeField]
	private Button _ok;

	[SerializeField]
	private Button _yes;

	[SerializeField]
	private Button _no;

	[SerializeField]
	private Image _clickBG;

	[SerializeField]
	private Text _okText;

	[SerializeField]
	private Text _yesText;

	[SerializeField]
	private Text _noText;

	[SerializeField]
	private FadeCanvas _fadeCanvas;

	public static Status status { get; } = new Status();

	public static bool active { get; private set; }

	public static Func<bool> DefaultCancellation => () => Scene.Remove(SingletonInitializer<ConfirmDialog>.instance);

	int Scene.IOverlap.order { get; set; }

	int Scene.IOverlap.priority => _priority;

	Canvas Scene.IOverlap.canvas => _canvas;

	private Action _cancellation { get; set; }

	private float timeScale { get; set; } = 1f;

	private string okText { get; set; }

	private string yesText { get; set; }

	private string noText { get; set; }

	public static void Load()
	{
		Scene.Add(SingletonInitializer<ConfirmDialog>.instance);
	}

	void Scene.IOverlap.AddEvent()
	{
		timeScale = Time.timeScale;
		Time.timeScale = 0f;
		_sentence.text = status.Sentence;
		Setup();
		StartFade(FadeCanvas.Fade.In, delegate
		{
			status.Init?.Invoke();
			active = true;
		}).Forget();
	}

	void Scene.IOverlap.RemoveEvent()
	{
		active = false;
		RaycastTargetOFF(_ok);
		RaycastTargetOFF(_yes);
		RaycastTargetOFF(_no);
		StartFade(FadeCanvas.Fade.Out, delegate
		{
			Time.timeScale = timeScale;
			status.End?.Invoke();
			status.Clear();
			_cancellation = null;
			_okText.text = okText;
			_yesText.text = yesText;
			_noText.text = noText;
		}).Forget();
		static void RaycastTargetOFF(Button button)
		{
			button.image.raycastTarget = false;
		}
	}

	private void Setup()
	{
		_cancellation = status.No ?? status.Yes;
		if (status.Yes == null)
		{
			Disable(_ok);
			Disable(_yes);
			Disable(_no);
		}
		else if (status.No == null)
		{
			Enable(_ok);
			Disable(_yes);
			Disable(_no);
		}
		else
		{
			Disable(_ok);
			Enable(_yes);
			Enable(_no);
		}
		if (IsEnable(_ok))
		{
			Enable(_okText, status.YesText);
		}
		else
		{
			Disable(_okText);
		}
		if (IsEnable(_yes))
		{
			Enable(_yesText, status.YesText);
		}
		else
		{
			Disable(_yesText);
		}
		if (IsEnable(_no))
		{
			Enable(_noText, status.NoText);
		}
		else
		{
			Disable(_noText);
		}
	}

	protected override void Initialize()
	{
		okText = _okText.text;
		yesText = _yesText.text;
		noText = _noText.text;
		(from _ in _ok.OnClickAsObservable()
			where active
			select _).Subscribe(delegate
		{
			status.Yes?.Invoke();
			DefaultCancellation();
		});
		(from _ in _yes.OnClickAsObservable()
			where active
			select _).Subscribe(delegate
		{
			status.Yes?.Invoke();
			DefaultCancellation();
		});
		(from _ in _no.OnClickAsObservable()
			where active
			select _).Subscribe(delegate
		{
			status.No?.Invoke();
			DefaultCancellation();
		});
		List<Button> list = new List<Button>();
		list.Add(_ok);
		list.Add(_yes);
		list.Add(_no);
		list.ForEach(delegate(Button bt)
		{
			bt.OnPointerEnterAsObservable().Subscribe(delegate
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
				_cancellation?.Invoke();
				DefaultCancellation();
			});
		}
	}

	private static bool IsEnable(Button button)
	{
		return button.image.enabled;
	}

	private static void Enable(Button button)
	{
		button.image.enabled = true;
		button.image.raycastTarget = true;
	}

	private static void Disable(Button button)
	{
		button.image.enabled = false;
		button.image.raycastTarget = false;
	}

	private static void Enable(Text text, string message)
	{
		text.enabled = true;
		if (!message.IsNullOrEmpty())
		{
			text.text = message;
		}
	}

	private static void Disable(Text text)
	{
		text.enabled = false;
	}

	private async UniTask StartFade(FadeCanvas.Fade type, Action onEnd)
	{
		await _fadeCanvas.StartFadeAysnc(type);
		onEnd();
	}
}
