using System.Linq;
using Config;
using Illusion.Component.UI;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ADV;

public class ADVButton : MonoBehaviour
{
	private SelectUI[] _selUIs;

	[SerializeField]
	private MainScenario scenario;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private BoolReactiveProperty _visible = new BoolReactiveProperty(initialValue: true);

	[SerializeField]
	private Toggle _skip;

	[SerializeField]
	private Toggle _auto;

	[SerializeField]
	private Button _backLog;

	[SerializeField]
	private Button _voice;

	[SerializeField]
	private Button _config;

	[SerializeField]
	private Button _close;

	public bool isSelect => selUIs.Any((SelectUI p) => p.isSelect);

	public bool isFocus => selUIs.Any((SelectUI p) => p.isFocus);

	private SelectUI[] selUIs => this.GetCache(ref _selUIs, () => new Selectable[6] { _skip, _auto, _backLog, _voice, _config, _close }.Select((Selectable p) => p.GetOrAddComponent<SelectUI>()).ToArray());

	public bool Visible
	{
		set
		{
			_visible.Value = value;
		}
	}

	public Toggle skip => _skip;

	public Toggle auto => _auto;

	public Button backLog => _backLog;

	public Button voice => _voice;

	public Button config => _config;

	public Button close => _close;

	private void VisibleSet(bool isOn)
	{
		if (!(_canvasGroup == null))
		{
			if (isOn)
			{
				_canvasGroup.alpha = 1f;
				_canvasGroup.blocksRaycasts = true;
			}
			else
			{
				_canvasGroup.alpha = 0f;
				_canvasGroup.blocksRaycasts = false;
			}
		}
	}

	private void Awake()
	{
		_skip.isOn = false;
		_auto.isOn = false;
	}

	private void Start()
	{
		_visible.TakeUntilDestroy(this).Subscribe(delegate(bool isOn)
		{
			VisibleSet(isOn);
		});
		_skip.isOn = scenario.isSkip;
		this.ObserveEveryValueChanged((ADVButton _) => !scenario.regulate.control.HasFlag(Regulate.Control.Skip)).SubscribeToInteractable(_skip);
		_skip.OnDisableAsObservable().Subscribe(delegate
		{
			_skip.isOn = false;
		});
		_skip.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			if (isOn)
			{
				Utils.Sound.Play(SystemSE.sel);
			}
			scenario.isSkip = isOn;
		});
		_auto.isOn = scenario.isAuto;
		this.ObserveEveryValueChanged((ADVButton _) => !scenario.regulate.control.HasFlag(Regulate.Control.Auto)).SubscribeToInteractable(_auto);
		_auto.OnDisableAsObservable().Subscribe(delegate
		{
			_auto.isOn = false;
		});
		_auto.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			if (isOn)
			{
				Utils.Sound.Play(SystemSE.sel);
			}
			scenario.isAuto = isOn;
		});
		this.UpdateAsObservable().Subscribe(delegate
		{
			if (scenario.isSkip != _skip.isOn)
			{
				_skip.isOn = scenario.isSkip;
			}
			if (scenario.isAuto != _auto.isOn)
			{
				_auto.isOn = scenario.isAuto;
			}
		});
		this.ObserveEveryValueChanged((ADVButton _) => !scenario.regulate.control.HasFlag(Regulate.Control.Log)).SubscribeToInteractable(_backLog);
		_backLog.OnClickAsObservable().Subscribe(delegate
		{
			scenario.mode = MainScenario.Mode.BackLog;
			Utils.Sound.Play(SystemSE.sel);
		});
		this.ObserveEveryValueChanged((ADVButton _) => voiceRegCheck()).SubscribeToInteractable(_voice);
		_voice.OnClickAsObservable().Subscribe(delegate
		{
			scenario.VoicePlay(scenario.currentCharaData.voiceList);
		});
		(from _ in _config.OnClickAsObservable()
			where !Scene.Overlaps.Any((Scene.IOverlap x) => x is ConfigWindow)
			select _).Subscribe(delegate
		{
			EventSystem.current.SetSelectedGameObject(null);
			ConfigWindow.Load();
			Utils.Sound.Play(SystemSE.sel);
		});
		_close.OnClickAsObservable().Subscribe(delegate
		{
			scenario.mode = MainScenario.Mode.WindowNone;
			Utils.Sound.Play(SystemSE.sel);
		});
		bool voiceRegCheck()
		{
			if (!scenario.isAuto && !scenario.currentCharaData.isKaraoke)
			{
				return !scenario.currentCharaData.voiceList.IsNullOrEmpty();
			}
			return false;
		}
	}
}
