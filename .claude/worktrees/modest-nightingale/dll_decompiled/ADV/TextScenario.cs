using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ADV.Commands.Base;
using AIChara;
using Actor;
using Config;
using Illusion.Anime;
using Illusion.Component.UI;
using Illusion.Game.Elements;
using Manager;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace ADV;

[RequireComponent(typeof(CommandController))]
public class TextScenario : MonoBehaviour
{
	public interface IVoice
	{
		int personality { get; }

		string bundle { get; }

		string asset { get; }

		void Convert2D();

		AudioSource Play();

		bool Wait();
	}

	public interface IChara
	{
		int no { get; }

		void Play(TextScenario scenario);

		CharaData GetChara(TextScenario scenario);
	}

	public interface IMotion : IChara
	{
	}

	public interface IExpression : IChara
	{
	}

	public class CurrentCharaData
	{
		public Dictionary<int, string> bundleVoices => _bundleVoices;

		public bool isSkip { get; set; }

		public IVoice karaokePlayer
		{
			set
			{
				_karaokePlayer = new List<IVoice[]> { new IVoice[1] { value } };
			}
		}

		public bool isKaraoke => _karaokePlayer != null;

		public List<IVoice[]> voiceList => _karaokePlayer ?? _voiceList;

		public List<IMotion[]> motionList => _motionList;

		public List<IExpression[]> expressionList => _expressionList;

		private List<IVoice[]> _karaokePlayer { get; set; }

		private List<IVoice[]> _voiceList { get; set; }

		private List<IMotion[]> _motionList { get; set; }

		private List<IExpression[]> _expressionList { get; set; }

		private Dictionary<int, string> _bundleVoices { get; set; }

		public CurrentCharaData()
		{
			_bundleVoices = new Dictionary<int, string>();
		}

		public void CreateVoiceList()
		{
			if (voiceList == null)
			{
				_voiceList = new List<IVoice[]>();
			}
		}

		public void CreateMotionList()
		{
			if (motionList == null)
			{
				_motionList = new List<IMotion[]>();
			}
		}

		public void CreateExpressionList()
		{
			if (expressionList == null)
			{
				_expressionList = new List<IExpression[]>();
			}
		}

		public void Clear()
		{
			_karaokePlayer = null;
			_voiceList = null;
			_motionList = null;
			_expressionList = null;
		}
	}

	public class ParamData
	{
		public bool isPlayer => param is Player;

		public bool isHeroine => param is Heroine;

		public IParams param { get; }

		public CharaParams charaParam { get; }

		private Player player { get; }

		private Heroine heroine { get; }

		public bool isVisible
		{
			get
			{
				if (chaCtrl != null)
				{
					return chaCtrl.visibleAll;
				}
				return false;
			}
			set
			{
				if (chaCtrl != null)
				{
					chaCtrl.visibleAll = value;
				}
			}
		}

		public int sex { get; } = 1;

		public int voiceNo { get; }

		public float voicePitch { get; } = 1f;

		public ChaControl chaCtrl { get; private set; }

		public Transform transform { get; }

		public void Create(GameObject parent, bool isLoad)
		{
			ChaFileControl chaFile = null;
			Action(delegate(Actor.CharaData x)
			{
				chaFile = x.chaFile;
			});
			ChaControl chaCtrl = Singleton<Character>.Instance.CreateChara((byte)sex, parent, 0, chaFile);
			if (isLoad)
			{
				chaCtrl.Load();
				new Controller(chaCtrl);
			}
			this.chaCtrl = chaCtrl;
			Action(delegate(Actor.CharaData x)
			{
				x.SetRoot(chaCtrl.gameObject);
			});
			void Action(Action<Actor.CharaData> action)
			{
				if (!heroine.SafeProc(delegate(Heroine x)
				{
					action(x);
				}))
				{
					player.SafeProc(delegate(Player x)
					{
						action(x);
					});
				}
			}
		}

		public ParamData(IParams param)
		{
			this.param = param;
			Actor.CharaData charaData = param as Actor.CharaData;
			if (param != null)
			{
				if (!(param is Player player))
				{
					if (param is Heroine heroine)
					{
						sex = (this.heroine = heroine).chaFile.parameter.sex;
					}
				}
				else
				{
					sex = (this.player = player).chaFile.parameter.sex;
				}
			}
			charaParam = param?.param as CharaParams;
			voiceNo = charaData?.voiceNo ?? 0;
			voicePitch = charaData?.voicePitch ?? 1f;
			if (charaData != null)
			{
				chaCtrl = charaData.chaCtrl;
			}
			if (chaCtrl != null)
			{
				transform = chaCtrl.transform;
			}
		}
	}

	public class LoopVoicePack
	{
		public int voiceNo { get; private set; }

		public ChaControl chaCtrl { get; private set; }

		public AudioSource audio { get; private set; }

		public LoopVoicePack(int voiceNo, ChaControl chaCtrl, AudioSource audio)
		{
			this.voiceNo = voiceNo;
			this.chaCtrl = chaCtrl;
			this.audio = audio;
		}

		public bool Set()
		{
			if (chaCtrl == null || audio == null)
			{
				return false;
			}
			chaCtrl.SetVoiceTransform(audio);
			return true;
		}
	}

	protected class NextInfo
	{
		public bool isCompleteDisplayText { get; set; }

		public bool isNext { get; set; }

		public bool isSkip { get; set; }

		public void Set(bool isCompleteDisplayText, bool isNext, bool isSkip)
		{
			this.isCompleteDisplayText = isCompleteDisplayText;
			this.isNext = isNext;
			this.isSkip = isSkip;
		}
	}

	[Serializable]
	private sealed class FileOpen
	{
		[SerializeField]
		private List<RootData> fileList = new List<RootData>();

		[SerializeField]
		private List<RootData> rootList = new List<RootData>();

		public List<RootData> FileList => fileList;

		public List<RootData> RootList => rootList;

		public void Clear()
		{
			fileList.Clear();
			rootList.Clear();
		}
	}

	[SerializeField]
	private Regulate _regulate;

	[SerializeField]
	private Info _info = new Info();

	private CrossFade _crossFade;

	[SerializeField]
	private OpenData _openData = new OpenData();

	private CommandController _commandController;

	private TextController _textController;

	private ADVScene _advScene;

	[SerializeField]
	protected RectTransform messageWindow;

	[SerializeField]
	protected Image nextMarker;

	[SerializeField]
	protected Canvas msgWindowCanvas;

	[SerializeField]
	private Transform choices;

	[SerializeField]
	protected bool _isWindowImage = true;

	[SerializeField]
	protected BoolReactiveProperty _isSkip = new BoolReactiveProperty(initialValue: false);

	[SerializeField]
	protected BoolReactiveProperty _isAuto = new BoolReactiveProperty(initialValue: false);

	[SerializeField]
	protected int currentLine;

	[SerializeField]
	protected float autoWaitTime = 3f;

	[SerializeField]
	protected bool _isChoice;

	[SerializeField]
	private bool _isSceneFadeRegulate = true;

	public Regulate regulate => _regulate;

	public Info info => _info;

	public CrossFade crossFade => this.GetCacheObject(ref _crossFade, () => (advScene == null || advScene.advCamera == null) ? null : advScene.advCamera.GetComponent<CrossFade>());

	public bool isFadeAllEnd
	{
		get
		{
			if (!SingletonInitializer<Manager.Scene>.initialized || advScene == null || crossFade == null)
			{
				return true;
			}
			if (!Manager.Scene.IsFadeNow)
			{
				return crossFade.isEnd;
			}
			return false;
		}
	}

	public CurrentCharaData currentCharaData { get; } = new CurrentCharaData();

	public List<Program.Transfer> transferList
	{
		get
		{
			return _transferList;
		}
		set
		{
			_transferList = value;
		}
	}

	private List<Program.Transfer> _transferList { get; set; }

	public string fontColorKey { get; set; }

	public string LoadBundleName
	{
		get
		{
			return _openData.bundle;
		}
		set
		{
			_openData.bundle = value;
		}
	}

	public string LoadAssetName
	{
		get
		{
			return _openData.asset;
		}
		set
		{
			_openData.asset = value;
		}
	}

	public OpenData openData => _openData;

	public bool isBackGroundCommanding { get; set; }

	public CommandController commandController => this.GetComponentCache(ref _commandController);

	public TextController textController
	{
		get
		{
			if (_textControllerChecked)
			{
				return _textController;
			}
			_textControllerChecked = true;
			return this.GetComponentCache(ref _textController);
		}
	}

	private bool _textControllerChecked { get; set; }

	public ADVScene advScene
	{
		get
		{
			if (_advSceneChecked)
			{
				return _advScene;
			}
			_advSceneChecked = true;
			return this.GetComponentCache(ref _advScene);
		}
	}

	private bool _advSceneChecked { get; set; }

	public bool isSelectMessageWindow => messageWindowSelectUI.isSelect;

	public bool isBackGroundCommandProcessing => backCommandList.Count > 0;

	public virtual RectTransform MessageWindow => messageWindow;

	public Transform Choices => choices;

	public bool isWindowImage
	{
		get
		{
			return _isWindowImage;
		}
		set
		{
			_isWindowImage = value;
		}
	}

	public bool isSkip
	{
		get
		{
			return _isSkip.Value;
		}
		set
		{
			_isSkip.Value = value;
		}
	}

	public bool isAuto
	{
		get
		{
			return _isAuto.Value;
		}
		set
		{
			_isAuto.Value = value;
		}
	}

	public int CurrentLine
	{
		get
		{
			return currentLine - 1;
		}
		set
		{
			currentLine = value;
		}
	}

	private float autoWaitTimer { get; set; }

	public bool isChoice
	{
		get
		{
			return _isChoice;
		}
		set
		{
			_isChoice = value;
		}
	}

	public bool isSceneFadeRegulate
	{
		get
		{
			return _isSceneFadeRegulate;
		}
		set
		{
			_isSceneFadeRegulate = value;
		}
	}

	private bool _isStartRun { get; set; }

	protected Image windowImage { get; set; }

	public List<ScenarioData.Param> CommandPacks => commandPacks;

	protected List<ScenarioData.Param> commandPacks { get; } = new List<ScenarioData.Param>();

	private FileOpen fileOpenData { get; } = new FileOpen();

	protected bool isRequestLine { get; set; }

	protected HashSet<int> textHash { get; } = new HashSet<int>();

	private SelectUI messageWindowSelectUI { get; set; }

	public Dictionary<string, ValData> Vars => vars;

	private Dictionary<string, ValData> vars { get; } = new Dictionary<string, ValData>();

	public Dictionary<string, string> Replaces => replaces;

	private Dictionary<string, string> replaces { get; } = new Dictionary<string, string>();

	public IPack package { get; private set; }

	public ParamData player { get; private set; }

	public List<ParamData> heroineList { get; private set; }

	public CharaData currentChara
	{
		get
		{
			return _currentChara;
		}
		set
		{
			_currentChara = value;
		}
	}

	private CharaData _currentChara { get; set; }

	public Heroine currentHeroine
	{
		get
		{
			if (currentChara != null)
			{
				return currentChara.heroine;
			}
			return null;
		}
	}

	private CommandList nowCommandList => _commandController.NowCommandList;

	private CommandList backCommandList => _commandController.BackGroundCommandList;

	public IObservable<Unit> OnInitializedAsync => _single;

	private Illusion.Game.Elements.Single _single { get; set; } = new Illusion.Game.Elements.Single();

	public List<IDisposable> karaokeList => _karaokeList;

	private List<IDisposable> _karaokeList { get; } = new List<IDisposable>();

	private CancellationTokenSource cancellationTokenSource { get; set; }

	public List<LoopVoicePack> loopVoiceList => _loopVoiceList;

	private List<LoopVoicePack> _loopVoiceList { get; } = new List<LoopVoicePack>();

	public static int VOICE_SET_NO { get; } = 1;

	protected NextInfo _nextInfo { get; } = new NextInfo();

	public void CrossFadeStart()
	{
		if (!(crossFade == null) && isFadeAllEnd)
		{
			crossFade.FadeStart(info.anime.play.crossFadeTime);
		}
	}

	public void SetPackage(IPack package)
	{
		this.package = package;
		SetCharacters(package?.param);
	}

	public void SetCharacters(IParams[] param)
	{
		if (param == null)
		{
			return;
		}
		ParamData[] source = param.Select((IParams p) => new ParamData(p)).ToArray();
		heroineList = source.Where((ParamData p) => p.isHeroine).ToList();
		player = source.FirstOrDefault((ParamData p) => p.isPlayer);
		bool isParent = package?.isParent ?? false;
		foreach (var item in heroineList.Select((ParamData data, int index) => new { data, index }))
		{
			commandController.AddChara(item.index, new CharaData(item.data, this, isParent));
		}
		if (player != null)
		{
			commandController.AddChara(-1, new CharaData(player, this, isParent));
		}
		if (commandController.Characters.Any())
		{
			ChangeCurrentChara(commandController.Characters.First().Key);
		}
	}

	public bool ChangeCurrentChara(int no)
	{
		CharaData charaData = (currentChara = commandController.GetChara(no));
		return charaData != null;
	}

	public string ReplaceVars(string arg)
	{
		if (Vars.TryGetValue(arg, out var value))
		{
			return value.o.ToString();
		}
		return arg;
	}

	public string ReplaceText(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		try
		{
			foreach (Match item in Regex.Matches(text, "\\[.*?\\]"))
			{
				if (!item.Success)
				{
					continue;
				}
				string key = string.Empty;
				try
				{
					key = Regex.Replace(item.Value, "\\[|\\]", string.Empty);
				}
				catch (Exception)
				{
				}
				if (Replaces.TryGetValue(key, out var value))
				{
					if (!value.IsNullOrEmpty())
					{
						value = ReplaceText(value);
					}
					if (value == null)
					{
						value = item.Value;
					}
				}
				else
				{
					value = item.Value;
				}
				stringBuilder.Append(text.Substring(num, item.Index - num));
				stringBuilder.Append(value);
				num = item.Index + item.Length;
			}
		}
		catch (Exception)
		{
		}
		stringBuilder.Append(text.Substring(num, text.Length - num));
		return stringBuilder.ToString();
	}

	public virtual void Initialize()
	{
		commandPacks.Clear();
		fileOpenData.Clear();
		_regulate.SetRegulate((Regulate.Control)0);
		if (windowImage == null)
		{
			windowImage = MessageWindow.GetComponent<Image>();
		}
		if (messageWindowSelectUI == null)
		{
			messageWindowSelectUI = MessageWindow.GetOrAddComponent<SelectUI>();
		}
		currentLine = 0;
		autoWaitTimer = 0f;
		commandController.Initialize();
		textController.Clear();
		vars.Clear();
		replaces.Clear();
		_single.Done();
	}

	protected void MemberInit()
	{
		isSkip = false;
		isAuto = false;
		_isSceneFadeRegulate = true;
		_isStartRun = false;
		textHash.Clear();
	}

	public virtual void Release()
	{
		cancellationTokenSource?.Cancel();
		cancellationTokenSource?.Dispose();
		cancellationTokenSource = null;
		loopVoiceList.ForEach(delegate(LoopVoicePack p)
		{
			if (p.audio != null)
			{
				UnityEngine.Object.Destroy(p.audio.gameObject);
			}
		});
		loopVoiceList.Clear();
		karaokeList.ForEach(delegate(IDisposable p)
		{
			p.Dispose();
		});
		karaokeList.Clear();
		_single = new Illusion.Game.Elements.Single();
		commandController.Release();
		info.audio.is2D = false;
		info.audio.isNotMoveMouth = false;
	}

	public void CommandAdd(bool isNext, int line, bool multi, Command command, string[] args)
	{
		List<string> list = new List<string>();
		list.Add("0");
		list.Add(multi.ToString());
		list.Add(command.ToString());
		list.AddRange(args ?? new string[1] { string.Empty });
		ScenarioData.Param item = new ScenarioData.Param(list.ToArray());
		if (commandPacks.Count == line)
		{
			commandPacks.Add(item);
		}
		else
		{
			commandPacks.Insert(line, item);
		}
		if (isNext)
		{
			RequestNextLine();
		}
	}

	public virtual bool LoadFile(string bundle, string asset, bool isClear = true, bool isClearCheck = true, bool isNext = true)
	{
		if (isClear)
		{
			fileOpenData.Clear();
		}
		if (bundle.IsNullOrEmpty())
		{
			bundle = LoadBundleName;
		}
		if (!isClear && isClearCheck && fileOpenData.FileList.Any((RootData p) => p.bundleName == bundle && p.fileName == asset))
		{
			return false;
		}
		openData.Load(bundle, asset);
		if (!isClear)
		{
			commandPacks.InsertRange(currentLine, openData.data.list);
			if (!fileOpenData.FileList.Any((RootData p) => p.bundleName == bundle && p.fileName == asset && p.line == currentLine))
			{
				fileOpenData.FileList.Add(new RootData
				{
					bundleName = bundle,
					fileName = asset,
					line = CurrentLine
				});
			}
		}
		else
		{
			LoadBundleName = bundle;
			LoadAssetName = asset;
			string[] args = new string[5]
			{
				bundle,
				asset,
				bool.FalseString,
				bool.TrueString,
				bool.TrueString
			};
			currentLine = 0;
			commandPacks.Clear();
			CommandAdd(isNext: false, currentLine++, multi: false, Command.Open, args);
			commandPacks.AddRange(openData.data.list);
		}
		if (isNext)
		{
			RequestNextLine();
		}
		openData.ClearData();
		return true;
	}

	public bool SearchTagJumpOrOpenFile(string jump, int localLine)
	{
		string[] array = jump.Split(':');
		if (array.Length == 1)
		{
			if (SearchTag(jump, out var n))
			{
				Jump(n);
				return true;
			}
			return false;
		}
		Open open = new Open();
		open.Set(Command.Open);
		string[] argsDefault = open.ArgsDefault;
		for (int i = 0; i < array.Length && i < argsDefault.Length; i++)
		{
			argsDefault[i] = ReplaceVars(array[i]);
		}
		CommandAdd(isNext: false, localLine + 1, multi: false, open.command, argsDefault);
		return true;
	}

	public bool SearchTag(string tagName, out int n)
	{
		n = commandPacks.TakeWhile(delegate(ScenarioData.Param p)
		{
			if (p.Command != Command.Tag)
			{
				return true;
			}
			return (ReplaceVars(p.Args[0]) != tagName) ? true : false;
		}).Count();
		return n < commandPacks.Count;
	}

	public void Jump(int n)
	{
		currentLine = n;
		RequestNextLine();
	}

	public void ChangeWindow(UnityEngine.UI.Text nameWindow, UnityEngine.UI.Text messageWindow)
	{
		textController.Change(nameWindow, messageWindow);
		UnityEngine.UI.Text text = MessageWindow.GetComponentsInChildren<UnityEngine.UI.Text>(includeInactive: true).FirstOrDefault((UnityEngine.UI.Text p) => p.name == "Message");
		MessageWindow.gameObject.SetActive(text != null && text == messageWindow);
	}

	public virtual void ConfigProc()
	{
		if (Manager.Config.initialized)
		{
			TextSystem textData = Manager.Config.TextData;
			if (windowImage != null)
			{
				Color color = windowImage.color;
				color.a = textData.WindowAlpha;
				windowImage.color = color;
			}
			textController.FontSpeed = textData.FontSpeed;
		}
	}

	public virtual void ChoicesInit()
	{
		isChoice = false;
		if (Choices != null)
		{
			Choices.gameObject.SetActive(value: false);
		}
		nextMarker.SafeProc(delegate(Image p)
		{
			p.enabled = false;
		});
	}

	public void BackGroundCommandProcessEnd()
	{
		backCommandList.ProcessEnd();
	}

	public void VoicePlay(IReadOnlyCollection<IVoice[]> voiceCollection, Action onPlay, Action onEnd)
	{
		this.cancellationTokenSource?.Cancel();
		this.cancellationTokenSource?.Dispose();
		this.cancellationTokenSource = null;
		Manager.Voice.StopAll(isLoopStop: false);
		if (voiceCollection == null)
		{
			return;
		}
		if (_loopVoiceList.Any())
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (IVoice[] item in voiceCollection)
			{
				foreach (IVoice voice in item)
				{
					hashSet.Add(voice.personality);
				}
			}
			foreach (int item2 in hashSet)
			{
				foreach (LoopVoicePack loopVoice in _loopVoiceList)
				{
					if (loopVoice.voiceNo == item2 && loopVoice.audio != null)
					{
						loopVoice.audio.Pause();
					}
				}
			}
		}
		Action onCompleted = delegate
		{
			onCompleted2();
		};
		CancellationTokenSource cancellationTokenSource = (this.cancellationTokenSource = new CancellationTokenSource());
		VoicePlayAsync(voiceCollection, onPlay, onCompleted, cancellationTokenSource.Token).Forget();
		void onCompleted2()
		{
			List<LoopVoicePack> list = new List<LoopVoicePack>();
			foreach (LoopVoicePack loopVoice2 in _loopVoiceList)
			{
				if (!loopVoice2.Set() || loopVoice2.audio == null)
				{
					list.Add(loopVoice2);
				}
				else
				{
					loopVoice2.audio.Play();
				}
			}
			list.ForEach(delegate(LoopVoicePack item)
			{
				_loopVoiceList.Remove(item);
			});
			onEnd?.Invoke();
			this.cancellationTokenSource?.Dispose();
			this.cancellationTokenSource = null;
		}
	}

	private async UniTask VoicePlayAsync(IReadOnlyCollection<IVoice[]> voiceCollection, Action onPlay, Action onCompleted, CancellationToken cancellationToken)
	{
		foreach (IVoice[] voice in voiceCollection)
		{
			IVoice[] array = voice;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			onPlay?.Invoke();
			await UniTask.WaitWhile(() => voice.Any((IVoice p) => p.Wait()), PlayerLoopTiming.Update, cancellationToken);
		}
		onCompleted?.Invoke();
	}

	protected virtual bool StartRun()
	{
		if (_isStartRun)
		{
			return false;
		}
		_isStartRun = true;
		int num = 0;
		if (!_transferList.IsNullOrEmpty())
		{
			Program.Transfer transfer = _transferList[0];
			if (transfer.param.Command == Command.SceneFadeRegulate)
			{
				_isSceneFadeRegulate = bool.Parse(transfer.param.Args[0]);
			}
			foreach (Program.Transfer transfer2 in _transferList)
			{
				int line = ((transfer2.line == -1) ? num : transfer2.line);
				CommandAdd(isNext: false, line, transfer2.param.Multi, transfer2.param.Command, transfer2.param.Args);
				num++;
			}
		}
		if (!LoadBundleName.IsNullOrEmpty() && !LoadAssetName.IsNullOrEmpty())
		{
			string[] args = new string[5]
			{
				LoadBundleName,
				LoadAssetName,
				bool.FalseString,
				bool.TrueString,
				bool.TrueString
			};
			CommandAdd(isNext: true, num, multi: false, Command.Open, args);
		}
		else if (_openData.HasData)
		{
			commandPacks.AddRange(openData.data.list);
			RequestNextLine();
			openData.ClearData();
		}
		return true;
	}

	protected void RequestNextLine()
	{
		StartCoroutine(_RequestNextLine());
	}

	protected virtual IEnumerator _RequestNextLine()
	{
		isRequestLine = true;
		nowCommandList.ProcessEnd();
		textHash.Clear();
		autoWaitTimer = 0f;
		while (true)
		{
			if (commandController.LoadingCharaList.Any())
			{
				yield return null;
				continue;
			}
			bool flag = false;
			if (currentLine < commandPacks.Count)
			{
				ScenarioData.Param param = commandPacks[currentLine++];
				if (param.Command == Command.Log)
				{
					flag = true;
				}
				else
				{
					flag = ((isBackGroundCommanding && param.Command != Command.Task) ? backCommandList.Add(param, CurrentLine) : nowCommandList.Add(param, CurrentLine));
					Command command = param.Command;
					if (command == Command.Text)
					{
						textHash.Add(param.Hash);
					}
				}
			}
			if (!flag)
			{
				break;
			}
		}
		isRequestLine = false;
		foreach (int item in textHash)
		{
			if (AlreadyReadInfo.Add(item) && isSkip)
			{
				isSkip = false;
			}
		}
	}

	protected bool MessageWindowProc(NextInfo nextInfo)
	{
		if (isRequestLine)
		{
			return false;
		}
		backCommandList.Process();
		if (nowCommandList.Process())
		{
			return false;
		}
		if (commandPacks.Count == 0)
		{
			return false;
		}
		if (regulate.control.HasFlag(Regulate.Control.ClickNext))
		{
			nextInfo.isNext = false;
		}
		if (regulate.control.HasFlag(Regulate.Control.Skip))
		{
			nextInfo.isSkip = false;
			isSkip = false;
		}
		if (regulate.control.HasFlag(Regulate.Control.Auto))
		{
			isAuto = false;
		}
		if (regulate.control.HasFlag(Regulate.Control.AutoForce))
		{
			isAuto = true;
		}
		bool isCompleteDisplayText = nextInfo.isCompleteDisplayText;
		bool isNext = nextInfo.isNext;
		bool flag = nextInfo.isSkip;
		if (!isCompleteDisplayText)
		{
			if (isNext || isSkip || flag)
			{
				textController.ForceCompleteDisplayText();
				nowCommandList.ProcessEnd();
			}
			return false;
		}
		autoWaitTimer = Mathf.Min(autoWaitTimer + Time.deltaTime, autoWaitTime);
		bool isProcessing = nowCommandList.Count > 0;
		bool flag2 = textHash.Count > 0;
		bool flag3 = false;
		if (flag || isSkip)
		{
			flag3 = (byte)((flag3 ? 1u : 0u) | 1u) != 0;
		}
		else if (isAuto && flag2)
		{
			flag3 |= autoWaitTimer >= autoWaitTime && !isProcessing;
		}
		nextMarker.SafeProc(delegate(Image p)
		{
			p.enabled = windowImage.enabled && (isChoice || !isProcessing);
		});
		flag3 = flag3 || isNext;
		if (regulate.control.HasFlag(Regulate.Control.Next))
		{
			flag3 = false;
		}
		flag3 = flag3 || (!flag2 && !isProcessing);
		if (flag3)
		{
			nextMarker.SafeProc(delegate(Image p)
			{
				p.enabled = false;
			});
			currentCharaData.Clear();
			RequestNextLine();
		}
		return flag3;
	}

	protected virtual void UpdateBefore()
	{
		bool flag = textController.MessageWindow.text.IsNullOrEmpty();
		if (msgWindowCanvas != null)
		{
			msgWindowCanvas.enabled = !flag;
		}
		if (windowImage != null)
		{
			windowImage.enabled = isWindowImage && !flag;
		}
		textController.FontColor = commandController.fontColor[fontColorKey ?? string.Empty];
	}

	protected virtual bool UpdateRegulate()
	{
		if (Manager.Scene.isReturnTitle)
		{
			return true;
		}
		if (!SingletonInitializer<Manager.Scene>.initialized || !Singleton<Game>.IsInstance())
		{
			return true;
		}
		if (Manager.Scene.IsNowLoading)
		{
			return true;
		}
		if (advScene != null && BaseMap.isMapLoading)
		{
			return true;
		}
		StartRun();
		if (_isSceneFadeRegulate && Manager.Scene.IsFadeNow)
		{
			return true;
		}
		if (Manager.Scene.IsOverlap)
		{
			return true;
		}
		if (Mathf.Max(0, Manager.Scene.NowSceneNames.IndexOf("ADV")) > 0)
		{
			return true;
		}
		return false;
	}

	protected virtual void Awake()
	{
		_regulate = new Regulate(this);
	}

	protected virtual void OnEnable()
	{
		Initialize();
	}

	protected virtual void OnDisable()
	{
		Release();
		MemberInit();
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		UpdateBefore();
		if (!UpdateRegulate())
		{
			KeyInput.Data data = KeyInput.TextNext(isOnWindow: true, isKeyCondition: true);
			_nextInfo.Set(textController.IsCompleteDisplayText, data.isCheck, KeyInput.SkipButton);
			MessageWindowProc(_nextInfo);
		}
	}
}
