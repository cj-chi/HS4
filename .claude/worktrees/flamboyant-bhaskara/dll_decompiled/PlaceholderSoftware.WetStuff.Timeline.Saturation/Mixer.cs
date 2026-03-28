using UnityEngine.Playables;

namespace PlaceholderSoftware.WetStuff.Timeline.Saturation;

internal class Mixer : TemplatedTimeline<Data, Clip, Mixer, Track, float, WetDecal>.BaseMixer
{
	protected override float GetState(WetDecal trackBinding)
	{
		return trackBinding.Settings.Saturation;
	}

	protected override void ApplyState(float intermediate, WetDecal trackBinding)
	{
		trackBinding.Settings.Saturation = intermediate;
	}

	protected override float Mix(Playable playable, FrameData info, WetDecal trackBinding)
	{
		int inputCount = playable.GetInputCount();
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < inputCount; i++)
		{
			float inputWeight = playable.GetInputWeight(i);
			Data behaviour = ((ScriptPlayable<Data>)playable.GetInput(i)).GetBehaviour();
			num2 += behaviour.Saturation * inputWeight;
			num += inputWeight;
		}
		return num2 + base.Default * (1f - num);
	}
}
