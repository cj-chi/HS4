using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AIChara;
using Illusion.Anime.Information;
using UniRx;
using UniRx.Async;
using UnityEngine;

namespace Illusion.Anime;

public class Controller : IDisposable
{
	public class ActorAnimInfo
	{
		public bool inEnableBlend { get; }

		public float inBlendSec { get; }

		public float inFadeOutTime { get; }

		public bool outEnableBlend { get; }

		public float outBlendSec { get; }

		public int directionType { get; }

		public bool endEnableBlend { get; }

		public float endBlendSec { get; }

		public bool isLoop { get; }

		public int loopMinTime { get; }

		public int loopMaxTime { get; }

		public int layer { get; }

		public string loopStateName { get; }

		public bool hasAction { get; }

		public int randomCount { get; }

		public float oldNormalizedTime { get; set; }

		public ActorAnimInfo(PlayState info)
		{
			if (info == null)
			{
				return;
			}
			directionType = info.DirectionType;
			endEnableBlend = info.EndEnableBlend;
			endBlendSec = info.EndBlendRate;
			PlayState.ActionInfo actionInfo = info.actionInfo;
			if (actionInfo != null)
			{
				hasAction = actionInfo.use;
				randomCount = actionInfo.rate;
			}
			PlayState.PlayStateInfo mainStateInfo = info.MainStateInfo;
			if (mainStateInfo == null)
			{
				return;
			}
			inFadeOutTime = mainStateInfo.FadeOutTime;
			outEnableBlend = mainStateInfo.OutStateInfo.EnableFade;
			outBlendSec = mainStateInfo.OutStateInfo.fadeTime;
			isLoop = mainStateInfo.IsLoop;
			loopMinTime = mainStateInfo.LoopMin;
			loopMaxTime = mainStateInfo.LoopMax;
			PlayState.AnimStateInfo inStateInfo = mainStateInfo.InStateInfo;
			if (inStateInfo != null)
			{
				inEnableBlend = inStateInfo.EnableFade;
				inBlendSec = inStateInfo.fadeTime;
				PlayState.Info[] stateInfos = inStateInfo.stateInfos;
				if (stateInfos != null)
				{
					layer = stateInfos.SafeGet(0)?.layer ?? 0;
					loopStateName = stateInfos.SafeGet(stateInfos.Length - 1).state;
				}
			}
		}
	}

	public abstract class UpdateEvent
	{
		private bool used { get; set; }

		private float normalizedTime { get; }

		public UpdateEvent(float normalizedTime)
		{
			this.normalizedTime = normalizedTime;
		}

		public void Update(float normalizedTime, bool isLoop)
		{
			if (isLoop || (used && this.normalizedTime >= normalizedTime))
			{
				used = false;
			}
			if (!used && this.normalizedTime < normalizedTime)
			{
				used = true;
				Change();
			}
		}

		protected abstract void Change();
	}

	public class AnimatorStateEvent : UpdateEvent
	{
		public int eventID { get; }

		public AnimatorStateEvent(Loader.AnimeEventInfo info)
			: base(info.normalizedTime)
		{
			eventID = info.eventID;
		}

		protected override void Change()
		{
		}
	}

	public static IReadOnlyDictionary<int, Controller> Table => _Table;

	private static Dictionary<int, Controller> _Table { get; } = new Dictionary<int, Controller>();

	public static string HeightName { get; } = "height";

	public bool Enabled => enabled;

	private bool enabled { get; set; } = true;

	private int ID { get; }

	private ChaControl chaCtrl { get; }

	public ItemHandler itemHandler => _itemHandler;

	private ItemHandler _itemHandler { get; }

	private StateObserver _stateObserver { get; }

	private Animator _animator { get; }

	private RuntimeAnimatorController _defaultController { get; set; }

	public MotionIK motionIK => _motionIK;

	private MotionIK _motionIK { get; }

	public YureCtrl yureCtrl => _yureCtrl;

	private YureCtrl _yureCtrl { get; } = new YureCtrl();

	public bool PlayingInAnimation => _inAnimCancellation != null;

	private CancellationTokenSource _inAnimCancellation { get; set; }

	public float heightShape => HeightShape(chaCtrl);

	public ActorAnimInfo AnimInfo { get; private set; }

	private Queue<PlayState.Info> InStates { get; } = new Queue<PlayState.Info>();

	private Queue<PlayState.Info> OutStates { get; } = new Queue<PlayState.Info>();

	private PlayState.Info _maskState { get; set; }

	private List<PlayState.PlayStateInfo> ActionStates { get; } = new List<PlayState.PlayStateInfo>();

	private bool isDisposed => updateDisposable == null;

	private IDisposable updateDisposable { get; set; }

	private Dictionary<int, YureCtrl.Info> YureTable { get; set; }

	private bool PlayingInLocoAnimation => _inLocoAnimCancellation != null;

	private CancellationTokenSource _inLocoAnimCancellation { get; set; }

	private bool PlayingOutAnimation => _outAnimCancellation != null;

	private CancellationTokenSource _outAnimCancellation { get; set; }

	private bool PlayingActAnimation => _actionAnimCancellation != null;

	private CancellationTokenSource _actionAnimCancellation { get; set; }

	private Queue<PlayState.Info> ActionInStates { get; } = new Queue<PlayState.Info>();

	private Queue<PlayState.Info> ActionOutStates { get; } = new Queue<PlayState.Info>();

	private bool PlayingOnceActionAnimation => _onceActionAnimCancellation != null;

	private CancellationTokenSource _onceActionAnimCancellation { get; set; }

	private List<PlayState.Info> OnceActionStates { get; } = new List<PlayState.Info>();

	public static List<T> GetStateEvents<T>(int stateNameHash, IReadOnlyDictionary<int, List<T>> table)
	{
		List<T> value = null;
		table?.TryGetValue(stateNameHash, out value);
		return value;
	}

	public static void CrossFadeAnimation(Animator animator, string stateName, float fadeTime, int layer, float fixedTimeOffset)
	{
		CrossFadeAnimation(animator, Animator.StringToHash(stateName), fadeTime, layer, fixedTimeOffset);
	}

	public static void CrossFadeAnimation(Animator animator, int stateNameHash, float fadeTime, int layer, float fixedTimeOffset)
	{
		animator.CrossFadeInFixedTime(stateNameHash, fadeTime, layer, fixedTimeOffset, 0f);
	}

	public static void PlayAnimation(Animator animator, string stateName, int layer, float normalizedTime)
	{
		PlayAnimation(animator, Animator.StringToHash(stateName), layer, normalizedTime);
	}

	public static void PlayAnimation(Animator animator, int stateNameHash, int layer, float normalizedTime)
	{
		animator.Play(stateNameHash, layer, normalizedTime);
	}

	public static float HeightShape(ChaControl chaCtrl)
	{
		return chaCtrl.GetShapeBodyValue(0);
	}

	public Controller(ChaControl chaCtrl)
	{
		this.chaCtrl = chaCtrl;
		ID = chaCtrl.GetInstanceID();
		if (_Table.TryGetValue(ID, out var value))
		{
			value.Dispose();
		}
		_Table[ID] = this;
		_yureCtrl.SetChaControl(chaCtrl);
		_yureCtrl.Init();
		_motionIK = new MotionIK(chaCtrl);
		_animator = chaCtrl.animBody;
		_itemHandler = new ItemHandler(_animator.transform);
		_itemHandler.PlayAnimationEvent += SetHeightParameter;
		_stateObserver = new StateObserver(_animator);
		SetAnimatorController(_defaultController = _animator.runtimeAnimatorController);
		updateDisposable = (from _ in Observable.EveryUpdate().TakeUntilDestroy(chaCtrl)
			where enabled
			select _).Subscribe((Action<long>)delegate
		{
			UpdateAnimationEvent();
		}, (Action)delegate
		{
			Dispose();
		});
	}

	public void Dispose()
	{
		if (!isDisposed)
		{
			AllAnimationCancel();
			updateDisposable?.Dispose();
			updateDisposable = null;
			_itemHandler.Dispose();
			_Table.Remove(ID);
		}
	}

	public void Enable(bool enabled)
	{
		this.enabled = enabled;
	}

	public void SetAnimatorController(RuntimeAnimatorController rac)
	{
		_stateObserver.nameHash = 0;
		if (_defaultController == null)
		{
			_defaultController = rac;
		}
		if (_animator.runtimeAnimatorController != rac)
		{
			_animator.runtimeAnimatorController = rac;
		}
		if (!enabled)
		{
			_motionIK.SetData(null);
			return;
		}
		string text = ((rac == null) ? string.Empty : rac.name);
		LoadYureInfo(text);
		_motionIK.SetData(Loader.GetMotionIKData(text)?.Copy());
	}

	public void ResetDefaultAnimatorController()
	{
		SetAnimatorController(_defaultController);
	}

	public void SetDefaultAnimatorController(RuntimeAnimatorController rac)
	{
		_defaultController = rac;
	}

	private void CrossFadeScreen()
	{
		if (!(Camera.main == null))
		{
			CrossFade component = Camera.main.GetComponent<CrossFade>();
			if (!(component == null) && component.isEnd)
			{
				component.FadeStart();
			}
		}
	}

	private static AnimatorStateInfo GetStateInfo(Animator animator, int layer = 0)
	{
		return animator.GetCurrentAnimatorStateInfo(layer);
	}

	private static bool IsBlend(float fadeTime)
	{
		return !Mathf.Approximately(0f, fadeTime);
	}

	private static bool IsInTransition(Animator animator, PlayState.Info state, float fadeOutTime)
	{
		if (animator == null)
		{
			return false;
		}
		if (animator.IsInTransition(state.layer))
		{
			return true;
		}
		AnimatorStateInfo stateInfo = GetStateInfo(animator, state.layer);
		if (stateInfo.shortNameHash == state.nameHash)
		{
			return stateInfo.normalizedTime < fadeOutTime;
		}
		return false;
	}

	public void SetHeightParameter(Animator animator)
	{
		if (animator.parameters.Any((AnimatorControllerParameter x) => x.name == HeightName && x.type == AnimatorControllerParameterType.Float))
		{
			animator.SetFloat(HeightName, heightShape);
		}
	}

	public void CrossFadeAnimation(string stateName, float fadeTime, int layer, float fixedTimeOffset)
	{
		CrossFadeAnimation(Animator.StringToHash(stateName), fadeTime, layer, fixedTimeOffset);
	}

	public void CrossFadeAnimation(int stateNameHash, float fadeTime, int layer, float fixedTimeOffset)
	{
		SetHeightParameter(_animator);
		CrossFadeAnimation(_animator, stateNameHash, fadeTime, layer, fixedTimeOffset);
	}

	public void PlayAnimation(string stateName, int layer, float normalizedTime)
	{
		PlayAnimation(Animator.StringToHash(stateName), layer, normalizedTime);
	}

	public void PlayAnimation(int stateNameHash, int layer, float normalizedTime)
	{
		SetHeightParameter(_animator);
		PlayAnimation(_animator, stateNameHash, layer, normalizedTime);
	}

	public void AllAnimationCancel()
	{
		InLocoAnimationCancel();
		InAnimationCancel();
		OutAnimationCancel();
		ActionAnimationCancel();
		OnceActionAnimationCancel();
	}

	public void PlayID(int poseID)
	{
		PlayIDState(poseID, Loader.GetAnimePlayState(poseID));
	}

	public void PlayInAnimation(float fadeTime, float fadeOutTime, int layer)
	{
		PlayInAnimationAsync(fadeTime, fadeOutTime, layer).Forget();
	}

	public void InAnimationCancel()
	{
		_inAnimCancellation?.Cancel();
		_inAnimCancellation?.Dispose();
		_inAnimCancellation = null;
	}

	private async UniTask PlayInAnimationAsync(float fadeTime, float fadeOutTime, int layer)
	{
		if (!IsBlend(fadeTime))
		{
			CrossFadeScreen();
		}
		CancellationTokenSource cancellationTokenSource = (_inAnimCancellation = new CancellationTokenSource());
		CancellationToken token = cancellationTokenSource.Token;
		await PlayQueueAnimationAsync(InStates, fadeTime, layer, fadeOutTime, token);
		_inAnimCancellation?.Dispose();
		_inAnimCancellation = null;
	}

	private async UniTask PlayQueueAnimationAsync(Queue<PlayState.Info> queue, float fadeTime, int layer, float fadeOutTime, CancellationToken cancellationToken)
	{
		while (queue.Count > 0)
		{
			await PlayStateAsync(_animator, queue.Dequeue(), fadeTime, layer, fadeOutTime, cancellationToken);
		}
	}

	private async UniTask PlayStateAsync(Animator animator, PlayState.Info state, float fadeTime, int layer, float fadeOutTime, CancellationToken cancellationToken)
	{
		if (IsBlend(fadeTime))
		{
			CrossFadeAnimation(state.nameHash, fadeTime, layer, 0f);
			_itemHandler.CrossFadeItemAnimation(state.nameHash, fadeTime, layer);
			await UniTask.Delay(TimeSpan.FromSeconds(fadeTime), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
		else
		{
			PlayAnimation(state.nameHash, layer, 0f);
			_itemHandler.PlayItemAnimation(state.nameHash);
			await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
		}
		while (IsInTransition(animator, state, fadeOutTime))
		{
			await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
		}
	}

	private void PlayIDState(int poseID, PlayState stateInfo)
	{
		if (stateInfo != null)
		{
			AllAnimationCancel();
			ActorAnimInfo actorAnimInfo = LoadActionState(poseID, stateInfo);
			PlayInAnimation(actorAnimInfo.inBlendSec, stateInfo.MainStateInfo.FadeOutTime, actorAnimInfo.layer);
		}
	}

	private ActorAnimInfo LoadActionState(int poseID, PlayState playState)
	{
		_itemHandler.LoadEvents(poseID, playState.itemInfoList);
		InitializeStates(playState);
		RuntimeAnimatorController runtimeAnimatorController = _animator.runtimeAnimatorController;
		PlayState.PlayStateInfo mainStateInfo = playState.MainStateInfo;
		AssetBundleData assetBundleInfo = mainStateInfo.AssetBundleInfo;
		if (runtimeAnimatorController == null || assetBundleInfo.asset != runtimeAnimatorController.name || mainStateInfo.InStateInfo.stateInfos.Any((PlayState.Info state) => !_animator.HasState(state.layer, state.nameHash)))
		{
			RuntimeAnimatorController asset = assetBundleInfo.GetAsset<RuntimeAnimatorController>();
			if (asset != null)
			{
				SetAnimatorController(asset);
			}
		}
		return AnimInfo = new ActorAnimInfo(playState);
	}

	private void InitializeStates(PlayState info)
	{
		if (info == null)
		{
			InStates.Clear();
			OutStates.Clear();
			ActionStates.Clear();
			return;
		}
		PlayState.PlayStateInfo mainStateInfo = info.MainStateInfo;
		InitializeStates(mainStateInfo.InStateInfo.stateInfos, mainStateInfo.OutStateInfo.stateInfos);
		if (!info.SubStateInfos.IsNullOrEmpty())
		{
			foreach (PlayState.PlayStateInfo subStateInfo in info.SubStateInfos)
			{
				ActionStates.Add(subStateInfo);
			}
		}
		_maskState = info.MaskStateInfo;
	}

	private void InitializeStates(PlayState.Info[] inStateInfos, PlayState.Info[] outStateInfos)
	{
		InStates.Clear();
		if (!((IReadOnlyCollection<PlayState.Info>)(object)inStateInfos).IsNullOrEmpty())
		{
			PlayState.Info[] array = inStateInfos;
			foreach (PlayState.Info item in array)
			{
				InStates.Enqueue(item);
			}
		}
		OutStates.Clear();
		if (!((IReadOnlyCollection<PlayState.Info>)(object)outStateInfos).IsNullOrEmpty())
		{
			PlayState.Info[] array = outStateInfos;
			foreach (PlayState.Info item2 in array)
			{
				OutStates.Enqueue(item2);
			}
		}
		ActionStates.Clear();
	}

	private void UpdateAnimationEvent()
	{
		bool num = _stateObserver.Update();
		int nameHash = _stateObserver.nameHash;
		_itemHandler.Change(nameHash);
		if (num)
		{
			SetYureInfo(nameHash);
			_motionIK.Calc(nameHash);
		}
		bool isLoop = _stateObserver.isLoop;
		float normalizedTime = _stateObserver.normalizedTime;
		_itemHandler.Update(normalizedTime, isLoop, heightShape);
		_motionIK.ChangeWeight(nameHash, normalizedTime);
	}

	private void LoadYureInfo(string animatorName)
	{
		Dictionary<int, YureCtrl.Info> value = null;
		Loader.YureNormalTable?.TryGetValue(animatorName ?? string.Empty, out value);
		YureTable = value;
	}

	private void SetYureInfo(int stateHashName)
	{
		YureCtrl.Info value = null;
		YureTable?.TryGetValue(stateHashName, out value);
		_yureCtrl.Set(value);
	}

	private void PlayInLocoAnimation(float fadeTime, int layer)
	{
		PlayInLocoAnimationAsync(fadeTime, layer).Forget();
	}

	private void InLocoAnimationCancel()
	{
		_inLocoAnimCancellation?.Cancel();
		_inLocoAnimCancellation?.Dispose();
		_inLocoAnimCancellation = null;
	}

	private async UniTask PlayInLocoAnimationAsync(float fadeTime, int layer)
	{
		bool enableFade = IsBlend(fadeTime);
		if (!enableFade)
		{
			CrossFadeScreen();
		}
		Animator animator = _animator;
		for (int i = 1; i < animator.layerCount; i++)
		{
			animator.SetLayerWeight(i, 0f);
		}
		if ((_maskState?.layer ?? 0) > 0)
		{
			animator.SetLayerWeight(_maskState.layer, 1f);
			if (enableFade)
			{
				CrossFadeAnimation(_maskState.nameHash, fadeTime, _maskState.layer, 0f);
			}
			else
			{
				PlayAnimation(_maskState.nameHash, _maskState.layer, 0f);
			}
		}
		CancellationTokenSource cancellationTokenSource = (_inLocoAnimCancellation = new CancellationTokenSource());
		CancellationToken token = cancellationTokenSource.Token;
		Queue<PlayState.Info> queue = InStates;
		while (queue.Count > 0)
		{
			PlayState.Info state = queue.Dequeue();
			if (enableFade)
			{
				CrossFadeAnimation(state.nameHash, fadeTime, layer, 0f);
				_itemHandler.CrossFadeItemAnimation(state.nameHash, fadeTime, layer);
				await UniTask.Delay(TimeSpan.FromSeconds(fadeTime), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			}
			else
			{
				PlayAnimation(state.nameHash, layer, 0f);
				_itemHandler.PlayItemAnimation(state.nameHash);
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			if (queue.Count == 0)
			{
				break;
			}
			while (IsInTransition(animator, state, 1f))
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
		}
		_inLocoAnimCancellation?.Dispose();
		_inLocoAnimCancellation = null;
	}

	private void PlayOutAnimation(float fadeTime, int layer)
	{
		PlayOutAnimationAsync(fadeTime, layer).Forget();
	}

	private void OutAnimationCancel()
	{
		_outAnimCancellation?.Cancel();
		_outAnimCancellation?.Dispose();
		_outAnimCancellation = null;
	}

	private async UniTask PlayOutAnimationAsync(float fadeTime, int layer)
	{
		CancellationTokenSource cancellationTokenSource = (_outAnimCancellation = new CancellationTokenSource());
		CancellationToken token = cancellationTokenSource.Token;
		await PlayQueueAnimationAsync(OutStates, fadeTime, layer, 1f, token);
		_outAnimCancellation?.Dispose();
		_outAnimCancellation = null;
	}

	private void PlayActionAnimation(int layer)
	{
		PlayActionAnimationAsync(layer).Forget();
	}

	private void ActionAnimationCancel()
	{
		_actionAnimCancellation?.Cancel();
		_actionAnimCancellation?.Dispose();
		_actionAnimCancellation = null;
	}

	private async UniTask PlayActionAnimationAsync(int layer)
	{
		PlayState.PlayStateInfo stateInfo = ActionStates[UnityEngine.Random.Range(0, ActionStates.Count)];
		AnimatorStateInfo prevState = GetStateInfo(_animator, layer);
		ActionInStates.Clear();
		if (!((IReadOnlyCollection<PlayState.Info>)(object)stateInfo.InStateInfo.stateInfos).IsNullOrEmpty())
		{
			PlayState.Info[] stateInfos = stateInfo.InStateInfo.stateInfos;
			foreach (PlayState.Info item in stateInfos)
			{
				ActionInStates.Enqueue(item);
			}
		}
		ActionOutStates.Clear();
		if (!((IReadOnlyCollection<PlayState.Info>)(object)stateInfo.OutStateInfo.stateInfos).IsNullOrEmpty())
		{
			PlayState.Info[] stateInfos = stateInfo.OutStateInfo.stateInfos;
			foreach (PlayState.Info item2 in stateInfos)
			{
				ActionOutStates.Enqueue(item2);
			}
		}
		CancellationTokenSource cancellationTokenSource = (_actionAnimCancellation = new CancellationTokenSource());
		CancellationToken token = cancellationTokenSource.Token;
		await PlayQueueAnimationAsync(ActionInStates, stateInfo.InStateInfo.fadeTime, layer, 1f, token);
		float elapsedTime = 0f;
		float loopDuration = UnityEngine.Random.Range(stateInfo.LoopMin, stateInfo.LoopMax);
		while (true)
		{
			float num;
			elapsedTime = (num = elapsedTime + Time.deltaTime);
			if (!(num < loopDuration))
			{
				break;
			}
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		await PlayQueueAnimationAsync(ActionOutStates, stateInfo.OutStateInfo.fadeTime, layer, 1f, token);
		if (stateInfo.OutStateInfo.EnableFade)
		{
			CrossFadeAnimation(prevState.shortNameHash, stateInfo.OutStateInfo.fadeTime, layer, 0f);
		}
		else
		{
			PlayAnimation(prevState.shortNameHash, layer, 0f);
		}
		await UniTask.Yield(PlayerLoopTiming.Update, token);
		_actionAnimCancellation?.Dispose();
		_actionAnimCancellation = null;
	}

	private void PlayOnceActionAnimation(float fadeTime, int layer)
	{
		PlayOnceActionAnimationAsync(fadeTime, layer).Forget();
	}

	private void OnceActionAnimationCancel()
	{
		_onceActionAnimCancellation?.Cancel();
		_onceActionAnimCancellation?.Dispose();
		_onceActionAnimCancellation = null;
	}

	private async UniTask PlayOnceActionAnimationAsync(float fadeTime, int layer)
	{
		PlayState.Info state = OnceActionStates[UnityEngine.Random.Range(0, OnceActionStates.Count)];
		Animator animator = _animator;
		AnimatorStateInfo prevState = GetStateInfo(animator, state.layer);
		CancellationTokenSource cancellationTokenSource = (_onceActionAnimCancellation = new CancellationTokenSource());
		CancellationToken token = cancellationTokenSource.Token;
		if (IsBlend(fadeTime))
		{
			CrossFadeAnimation(state.nameHash, fadeTime, layer, 0f);
			await UniTask.Delay(TimeSpan.FromSeconds(fadeTime), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		}
		else
		{
			CrossFadeScreen();
			PlayAnimation(state.nameHash, state.layer, 0f);
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		while (IsInTransition(animator, state, 1f))
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		if (animator != null)
		{
			animator.Play(prevState.shortNameHash, state.layer, 0f);
		}
		await UniTask.Yield(PlayerLoopTiming.Update, token);
		_onceActionAnimCancellation?.Dispose();
		_onceActionAnimCancellation = null;
	}

	private void EndStates()
	{
		if (!AnimInfo.endEnableBlend)
		{
			CrossFadeScreen();
		}
		if (_animator.runtimeAnimatorController.name != _defaultController.name)
		{
			ResetDefaultAnimatorController();
		}
	}
}
