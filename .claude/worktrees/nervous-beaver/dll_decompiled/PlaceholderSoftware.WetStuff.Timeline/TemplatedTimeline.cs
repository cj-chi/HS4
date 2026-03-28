using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PlaceholderSoftware.WetStuff.Timeline;

public static class TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding> where TData : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseData, new() where TClip : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseClip where TMixer : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseMixer, new() where TTrack : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseTrack where TBinding : MonoBehaviour
{
	[Serializable]
	public abstract class BaseTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<TMixer>.Create(graph, inputCount);
		}

		public override void GatherProperties([NotNull] PlayableDirector director, IPropertyCollector driver)
		{
			base.GatherProperties(director, driver);
		}
	}

	[Serializable]
	public abstract class BaseMixer : PlayableBehaviour
	{
		private TBinding _trackBinding;

		protected bool FirstFrameHappened { get; private set; }

		protected TIntermediate Default { get; private set; }

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			_trackBinding = playerData as TBinding;
			if (!(_trackBinding == null))
			{
				if (!FirstFrameHappened)
				{
					Default = GetState(_trackBinding);
				}
				ApplyState(Mix(playable, info, _trackBinding), _trackBinding);
				FirstFrameHappened = true;
			}
		}

		public override void OnGraphStop(Playable playable)
		{
			if (FirstFrameHappened)
			{
				ApplyState(Default, _trackBinding);
			}
			FirstFrameHappened = false;
		}

		protected abstract TIntermediate GetState([NotNull] TBinding trackBinding);

		protected abstract void ApplyState(TIntermediate intermediate, [NotNull] TBinding trackBinding);

		protected abstract TIntermediate Mix(Playable playable, FrameData info, [NotNull] TBinding trackBinding);
	}

	[Serializable]
	public abstract class BaseClip : PlayableAsset, ITimelineClipAsset
	{
		[UsedImplicitly]
		[SerializeField]
		public TData Template = new TData();

		public abstract ClipCaps clipCaps { get; }

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			ScriptPlayable<TData> scriptPlayable = ScriptPlayable<TData>.Create(graph, Template);
			Configure(scriptPlayable.GetBehaviour());
			return scriptPlayable;
		}

		protected virtual void Configure(TData data)
		{
		}
	}

	[Serializable]
	public abstract class BaseData : PlayableBehaviour
	{
	}
}
