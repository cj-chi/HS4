using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Tutorial2D;

public class Tutorial2D : SingletonInitializer<Tutorial2D>, Scene.IOverlap
{
	[Serializable]
	private class TutorialInfo
	{
		public Sprite[] sprites;

		public VideoClip videoClip;

		public bool isPage => count > 0;

		public int count
		{
			get
			{
				if (!((IReadOnlyCollection<Sprite>)(object)sprites).IsNullOrEmpty())
				{
					return sprites.Count();
				}
				return 0;
			}
		}

		public bool isVideo => videoClip != null;
	}

	[Serializable]
	private class PageUICtrl
	{
		public CanvasGroup canvasGroup;

		public Button buttonNext;

		public Button buttonPrev;

		public bool active
		{
			set
			{
				canvasGroup.Enable(value);
			}
		}
	}

	[Serializable]
	private class KindUICtrl
	{
		public GameObject objRoot;

		public Toggle[] toggles;

		public bool active
		{
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}

		public int select
		{
			set
			{
				if (!objRoot.activeSelf)
				{
					Toggle[] array = toggles;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].isOn = false;
					}
					toggles.SafeProc(value, delegate(Toggle _t)
					{
						_t.isOn = true;
					});
					return;
				}
				toggles.SafeProc(value, delegate(Toggle _t)
				{
					_t.isOn = true;
				});
				if (GameSystem.isAdd50 && value != 6 && value != 7)
				{
					if (!toggles[6].gameObject.activeSelf)
					{
						toggles[6].isOn = false;
					}
					if (!toggles[7].gameObject.activeSelf)
					{
						toggles[7].isOn = false;
					}
				}
			}
		}
	}

	[SerializeField]
	private Image imageTutorial;

	[SerializeField]
	private RawImage rawImageVideo;

	[SerializeField]
	private Slider sliderVideo;

	[SerializeField]
	private SliderHandler sliderHandler;

	[SerializeField]
	private RenderTexture renderTexture;

	[SerializeField]
	private Camera cameraVideo;

	[SerializeField]
	private VideoPlayer videoPlayer;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private TutorialInfo[] tutorialInfos;

	[SerializeField]
	private PageUICtrl pageUICtrl = new PageUICtrl();

	[SerializeField]
	private KindUICtrl kindUICtrl = new KindUICtrl();

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private int _priority;

	[SerializeField]
	private FadeCanvas _fadeCanvas;

	[SerializeField]
	[Header("機能制御")]
	private IntReactiveProperty _nowKind = new IntReactiveProperty(-1);

	[SerializeField]
	private BoolReactiveProperty _isAll = new BoolReactiveProperty(initialValue: false);

	private float timeScale = 1f;

	private bool active;

	public static bool isActive;

	public Action onEndFunc;

	private IntReactiveProperty _page = new IntReactiveProperty(0);

	private TutorialInfo tutorialInfo;

	private Coroutine coroutine;

	private SingleAssignmentDisposable asd;

	private bool isSkip;

	public static Func<bool> DefaultCancellation => () => Scene.Remove(SingletonInitializer<Tutorial2D>.instance);

	Canvas Scene.IOverlap.canvas => _canvas;

	int Scene.IOverlap.order { get; set; }

	int Scene.IOverlap.priority => _priority;

	public int nowKind
	{
		get
		{
			return _nowKind.Value;
		}
		set
		{
			_nowKind.Value = value;
		}
	}

	public bool isAll
	{
		get
		{
			return _isAll.Value;
		}
		set
		{
			_isAll.Value = value;
		}
	}

	private int page
	{
		get
		{
			return _page.Value;
		}
		set
		{
			_page.Value = Mathf.Clamp(value, 0, pageMax - 1);
			if (value == pageMax)
			{
				SceneEnd(isSkip: false);
			}
		}
	}

	private int pageMax
	{
		get
		{
			if (tutorialInfo == null)
			{
				return 0;
			}
			return tutorialInfo.count;
		}
	}

	private bool isVideo
	{
		get
		{
			if (tutorialInfo == null)
			{
				return false;
			}
			return tutorialInfo.isVideo;
		}
	}

	private VideoClip videoClip
	{
		get
		{
			if (tutorialInfo == null)
			{
				return null;
			}
			return tutorialInfo.videoClip;
		}
	}

	public static void Load()
	{
		Scene.Add(SingletonInitializer<Tutorial2D>.instance);
	}

	private void PlaySE(SystemSE se = SystemSE.sel)
	{
		Utils.Sound.Play(se);
	}

	private void SceneEnd(bool isSkip)
	{
		if (isSkip)
		{
			StartCoroutine(EndCoroutine());
			return;
		}
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = "説明画面を終了しますか？";
		status.Yes = delegate
		{
			PlaySE(SystemSE.ok_l);
			onEndFunc?.Invoke();
			onEndFunc = null;
			page = 0;
			DefaultCancellation();
		};
		status.No = delegate
		{
			PlaySE(SystemSE.cancel);
		};
		Time.timeScale = 1f;
		ConfirmDialog.Load();
		Time.timeScale = 0f;
	}

	private IEnumerator EndCoroutine()
	{
		yield return null;
		onEndFunc?.Invoke();
		onEndFunc = null;
		page = 0;
		DefaultCancellation();
	}

	private IEnumerator PlayWait()
	{
		videoPlayer.EnableAudioTrack(0, enabled: true);
		videoPlayer.SetTargetAudioSource(0, audioSource);
		videoPlayer.clip = videoClip;
		videoPlayer.frame = 0L;
		videoPlayer.Prepare();
		while (!videoPlayer.isPrepared)
		{
			yield return null;
		}
		videoPlayer.Play();
		coroutine = null;
	}

	private IEnumerator ReplayWait()
	{
		videoPlayer.frame = (long)Mathf.Lerp(0f, videoPlayer.frameCount, sliderVideo.value);
		videoPlayer.Prepare();
		while (!videoPlayer.isPrepared)
		{
			yield return null;
		}
		long frame = (long)Mathf.Lerp(0f, videoPlayer.frameCount, sliderVideo.value);
		while (frame != videoPlayer.frame)
		{
			yield return null;
		}
		videoPlayer.Play();
		coroutine = null;
	}

	private bool CheckVideoPage(int _page)
	{
		if (nowKind == 6)
		{
			return _page == 3;
		}
		return _page == pageMax - 1;
	}

	protected override void Initialize()
	{
		pageUICtrl.buttonNext.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
			int num2 = page + 1;
			page = num2;
		});
		pageUICtrl.buttonPrev.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
			int num2 = page - 1;
			page = num2;
		});
		for (int num = 0; num < kindUICtrl.toggles.Length; num++)
		{
			int idx = num;
			(from _b in kindUICtrl.toggles[num].OnValueChangedAsObservable().Skip(1)
				where _b
				select _b).Subscribe(delegate
			{
				nowKind = idx;
				Utils.Sound.Play(SystemSE.sel);
			});
		}
		_isAll.SubscribeWithState(kindUICtrl.objRoot, delegate(bool _b, GameObject _obj)
		{
			_obj.SetActiveIfDifferent(_b);
		});
		_page.Where((int _i) => tutorialInfo != null).Subscribe(delegate(int _i)
		{
			imageTutorial.sprite = tutorialInfo.sprites.SafeGet(_i);
			pageUICtrl.buttonPrev.interactable = _i != 0;
			if (CheckVideoPage(_i) && isVideo)
			{
				if (coroutine != null)
				{
					StopCoroutine(coroutine);
				}
				cameraVideo.targetTexture = renderTexture;
				cameraVideo.Render();
				cameraVideo.targetTexture = null;
				rawImageVideo.enabled = true;
				sliderVideo.gameObject.SetActiveIfDifferent(active: true);
				sliderVideo.value = 0f;
				coroutine = Observable.FromCoroutine(PlayWait).StartAsCoroutine();
			}
			else
			{
				if (coroutine != null)
				{
					StopCoroutine(coroutine);
				}
				coroutine = null;
				videoPlayer.Stop();
				rawImageVideo.enabled = false;
				sliderVideo.gameObject.SetActiveIfDifferent(active: false);
			}
		});
		_nowKind.Subscribe(delegate(int _kind)
		{
			TutorialInfo tutorialInfo = tutorialInfos.SafeGet(_kind);
			if (tutorialInfo != null)
			{
				this.tutorialInfo = tutorialInfo;
				kindUICtrl.select = _kind;
				_page.SetValueAndForceNotify(0);
			}
		});
		(from _f in sliderVideo.OnValueChangedAsObservable()
			where !videoPlayer.isPlaying
			select _f).Subscribe(delegate(float _f)
		{
			videoPlayer.frame = (long)Mathf.Lerp(0f, videoPlayer.frameCount, _f);
			isSkip = false;
		});
		sliderHandler.onPointerDown = delegate
		{
			if (asd == null)
			{
				videoPlayer.Pause();
			}
			videoPlayer.frame = (long)Mathf.Lerp(0f, videoPlayer.frameCount, sliderVideo.value);
			isSkip = false;
		};
		sliderHandler.onPointerUp = delegate
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
			}
			coroutine = null;
			if (asd == null && sliderVideo.value < 1f)
			{
				asd = new SingleAssignmentDisposable();
				asd.Disposable = (from _ in this.UpdateAsObservable()
					where videoPlayer.isPrepared && isSkip
					select _).Subscribe(delegate
				{
					videoPlayer.Play();
					if (!asd.IsDisposed)
					{
						asd.Dispose();
					}
					asd = null;
				}).AddTo(this);
			}
		};
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F3)
			where active
			where _fadeCanvas.isEnd
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			select _).Subscribe(delegate
		{
			SceneEnd(_isAll.Value);
		}).AddTo(this);
		(from _ in this.UpdateAsObservable()
			where videoPlayer.isPlaying && coroutine == null && asd == null
			select _).Subscribe(delegate
		{
			sliderVideo.value = Mathf.InverseLerp(0f, videoPlayer.frameCount, videoPlayer.frame);
		});
	}

	private IEnumerator Start()
	{
		videoPlayer.seekCompleted += VideoPlayer_seekCompleted;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return null;
	}

	private void VideoPlayer_seekCompleted(VideoPlayer source)
	{
		isSkip = true;
	}

	void Scene.IOverlap.AddEvent()
	{
		timeScale = Time.timeScale;
		Time.timeScale = 0f;
		isActive = true;
		if (Singleton<GameCursor>.IsInstance())
		{
			Singleton<GameCursor>.Instance.SetCursorLock(setLockFlag: false);
		}
		if (GameSystem.isAdd50 && kindUICtrl.toggles.Length > 6)
		{
			kindUICtrl.toggles[6].gameObject.SetActiveIfDifferent(Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == -1);
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
			isActive = false;
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
			}
		}).Forget();
	}

	private async UniTask StartFade(FadeCanvas.Fade type, Action onEnd)
	{
		await _fadeCanvas.StartFadeAysnc(type);
		onEnd();
	}
}
