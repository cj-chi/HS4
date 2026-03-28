using System;
using System.Linq;
using Illusion.CustomAttributes;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Config;

[DefaultExecutionOrder(100)]
public class ConfigWindow : SingletonInitializer<ConfigWindow>, Scene.IOverlap
{
	[Serializable]
	public class ShortCutGroup
	{
		public string title;

		public string name;

		public int visibleNo;

		[SerializeField]
		private RectTransform[] _trans;

		public RectTransform trans => _trans[visibleNo];

		public void VisibleUpdate()
		{
			RectTransform[] array = _trans;
			foreach (RectTransform rectTransform in array)
			{
				rectTransform.gameObject.SetActiveIfDifferent(rectTransform == trans);
			}
		}
	}

	[Label("コンフィグ項目を写す場所")]
	[SerializeField]
	private RectTransform mainWindow;

	[Label("ショートカットボタンの場所")]
	[SerializeField]
	private RectTransform shortCutButtonBackGround;

	[Label("ショートカットボタンのプレファブ")]
	[SerializeField]
	private Button shortCutButtonPrefab;

	[Tooltip("ショートカットのリンク")]
	[SerializeField]
	private ShortCutGroup[] shortCuts;

	[Tooltip("初めに設定されているボタン")]
	[SerializeField]
	private Button[] buttons;

	[Tooltip("背景")]
	[SerializeField]
	private Image imgBackGroud;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private int _priority;

	[SerializeField]
	private FadeCanvas _fadeCanvas;

	[SerializeField]
	private CanvasGroup cgSelectChara;

	[SerializeField]
	private CanvasGroup cgSelectBGM;

	private BaseSetting[] settings;

	private float timeScale = 1f;

	public static Action UnLoadAction = null;

	public static Action TitleChangeAction = null;

	public static Color backGroundColor = new Color32(0, 0, 0, 200);

	private readonly string[] localizeIsInit = new string[5] { "設定を初期化しますか？", "", "", "", "" };

	private readonly string[] localizeIsTitle = new string[5] { "タイトルに戻りますか？", "", "", "", "" };

	private bool isEnd;

	private bool active;

	public static bool isActive = false;

	private Button btnTitle;

	private GraphicSetting graphicSetting;

	public static Func<bool> DefaultCancellation => () => Scene.Remove(SingletonInitializer<ConfigWindow>.instance);

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
		Scene.Add(SingletonInitializer<ConfigWindow>.instance);
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
		bool isAdd50 = GameSystem.isAdd50;
		ShortCutGroup[] array = shortCuts;
		foreach (ShortCutGroup shortCutGroup in array)
		{
			if (isAdd50 || !(shortCutGroup.name == "Append"))
			{
				shortCutGroup.VisibleUpdate();
			}
		}
		var list = shortCuts.Where((ShortCutGroup s) => isAdd50 || (!isAdd50 && s.name != "Append")).Select(delegate(ShortCutGroup p, int siblingIndex)
		{
			Button button = UnityEngine.Object.Instantiate(shortCutButtonPrefab, shortCutButtonBackGround, worldPositionStays: false);
			button.GetComponentInChildren<Text>(includeInactive: true).text = p.title;
			button.transform.SetSiblingIndex(siblingIndex);
			ConfigSubWindowComponent comp = button.GetComponentInChildren<ConfigSubWindowComponent>(includeInactive: true);
			comp.selectAction.listActionEnter.Add(delegate
			{
				comp.objSelect.SetActiveIfDifferent(active: true);
				PlaySE();
			});
			comp.selectAction.listActionExit.Add(delegate
			{
				comp.objSelect.SetActiveIfDifferent(active: false);
			});
			return new
			{
				bt = button,
				name = p.name
			};
		}).ToList();
		settings = (from p in shortCuts
			where isAdd50 || (!isAdd50 && p.name != "Append")
			select p.trans.GetComponent<BaseSetting>()).ToArray();
		BaseSetting[] array2 = settings;
		foreach (BaseSetting obj in array2)
		{
			obj.Init();
			obj.UIPresenter();
		}
		if (isAdd50)
		{
			shortCuts.FirstOrDefault((ShortCutGroup s) => s.name == "Append")?.trans.gameObject.SetActiveIfDifferent(active: true);
		}
		float spacing = mainWindow.GetComponent<VerticalLayoutGroup>().spacing;
		list.ForEach(sc =>
		{
			sc.bt.OnClickAsObservable().Subscribe(delegate
			{
				ShortCutGroup[] array3 = shortCuts.TakeWhile((ShortCutGroup p) => p.name != sc.name).ToArray();
				float y = Mathf.Min(array3.Sum((ShortCutGroup p) => p.trans.sizeDelta.y) + spacing * (float)array3.Length, mainWindow.rect.height - (mainWindow.parent as RectTransform).rect.height);
				Vector2 anchoredPosition = mainWindow.anchoredPosition;
				anchoredPosition.y = y;
				mainWindow.anchoredPosition = anchoredPosition;
			});
		});
		graphicSetting = (from sc in shortCuts
			select sc.trans.GetComponent<GraphicSetting>() into sc
			where sc != null
			select sc).FirstOrDefault();
		list.ForEach(sc =>
		{
			sc.bt.OnClickAsObservable().Subscribe(delegate
			{
				PlaySE(SystemSE.ok_s);
			});
		});
		buttons.ToList().ForEach(delegate(Button bt)
		{
			this.ObserveEveryValueChanged((ConfigWindow _) => !Scene.IsNowLoadingFade).SubscribeToInteractable(bt);
			bt.OnPointerEnterAsObservable().Subscribe(delegate
			{
				PlaySE();
			});
			ConfigSubWindowComponent comp = bt.GetComponentInChildren<ConfigSubWindowComponent>(includeInactive: true);
			if ((bool)comp)
			{
				comp.selectAction.listActionEnter.Add(delegate
				{
					comp.objSelect.SetActiveIfDifferent(active: true);
				});
				comp.selectAction.listActionExit.Add(delegate
				{
					comp.objSelect.SetActiveIfDifferent(active: false);
				});
			}
		});
		btnTitle = buttons.FirstOrDefault((Button p) => p.name == "btnTitle");
		if ((bool)btnTitle)
		{
			(from _ in btnTitle.OnClickAsObservable()
				where !isEnd
				select _).Subscribe(delegate
			{
				OnTitle();
			});
		}
		Button backButton = buttons.FirstOrDefault((Button p) => p.name == "btnClose");
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F1)
			where active
			where _fadeCanvas.isEnd
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where cgSelectChara.alpha <= 0f && cgSelectBGM.alpha <= 0f
			where backButton.interactable && !isEnd
			select _).Subscribe(delegate
		{
			OnBack();
		});
		if ((bool)imgBackGroud)
		{
			imgBackGroud.color = backGroundColor;
		}
	}

	private void Setup()
	{
		isEnd = false;
		isActive = true;
		BaseSetting[] array = settings;
		foreach (BaseSetting obj in array)
		{
			obj.Setup();
			obj.UIPresenter();
		}
		if ((bool)btnTitle)
		{
			btnTitle.interactable = Scene.NowSceneNames[0] == "Title" || Singleton<Game>.Instance.saveData.TutorialNo == -1;
		}
	}

	public void OnDefault()
	{
		PlaySE(SystemSE.ok_s);
		Action yes = delegate
		{
			PlaySE(SystemSE.ok_s);
			Voice.ResetConfig();
			Manager.Config.Reset();
			Setup();
		};
		ConfirmDialog.Status status = ConfirmDialog.status;
		status.Sentence = localizeIsInit[Singleton<GameSystem>.Instance.languageInt];
		status.Yes = yes;
		status.No = delegate
		{
			PlaySE(SystemSE.cancel);
		};
		ConfirmDialog.Load();
	}

	public void OnTitle()
	{
		PlaySE(SystemSE.ok_l);
		bool flag = false;
		string text = "Title";
		if (Scene.NowSceneNames[0] == text)
		{
			flag = true;
		}
		if (!flag)
		{
			ReturnTitle();
		}
		else
		{
			Unload();
		}
	}

	public void OnGameEnd()
	{
		PlaySE(SystemSE.ok_s);
		Utils.Scene.GameEnd(isCheck: true);
	}

	public void OnBack()
	{
		if (!Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog))
		{
			PlaySE(SystemSE.cancel);
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
			TitleChangeAction = null;
			isActive = false;
		}).Forget();
	}

	private async UniTask StartFade(FadeCanvas.Fade type, Action onEnd)
	{
		await _fadeCanvas.StartFadeAysnc(type);
		onEnd();
	}

	private void ReturnTitle(bool skipCheck = false, bool isForce = false)
	{
		Utils.Scene.ReturnTitle(Result, skipCheck, isForce);
		static void Result()
		{
			TitleChangeAction?.Invoke();
			DefaultCancellation();
		}
	}
}
