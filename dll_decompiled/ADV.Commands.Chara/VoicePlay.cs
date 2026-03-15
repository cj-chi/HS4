using AIChara;
using Illusion;
using Illusion.Extensions;
using Manager;
using UnityEngine;

namespace ADV.Commands.Chara;

public class VoicePlay : CommandBase
{
	private enum Type
	{
		Normal,
		Once,
		Overlap
	}

	public override string[] ArgsLabel => new string[11]
	{
		"No", "Type", "Bundle", "Asset", "Fade", "isLoop", "isAsync", "VoiceNo", "Pitch", "is2D",
		"useADV"
	};

	public override string[] ArgsDefault => new string[11]
	{
		int.MaxValue.ToString(),
		Type.Normal.ToString(),
		string.Empty,
		string.Empty,
		"0",
		bool.FalseString,
		bool.TrueString,
		string.Empty,
		string.Empty,
		bool.FalseString,
		bool.TrueString
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		int num2 = args[num++].Check(ignoreCase: true, Utils.Enum<Type>.Names);
		CharaData chara = base.scenario.commandController.GetChara(no);
		string bundle = args[num++];
		string asset = args[num++];
		float fadeTime = float.Parse(args[num++]);
		bool isLoop = bool.Parse(args[num++]);
		bool flag = bool.Parse(args[num++]);
		int voiceNo = 0;
		bool flag2 = args.SafeProc(num++, delegate(string s)
		{
			voiceNo = int.Parse(s);
		});
		float pitch = 1f;
		bool flag3 = args.SafeProc(num++, delegate(string s)
		{
			pitch = float.Parse(s);
		});
		bool is2D = bool.Parse(args[num++]);
		bool useADV = bool.Parse(args[num++]);
		Voice.Loader loader = new Voice.Loader
		{
			no = voiceNo,
			bundle = bundle,
			asset = asset,
			fadeTime = fadeTime,
			pitch = pitch
		};
		ChaControl chaCtrl = null;
		if (chara != null)
		{
			chaCtrl = chara.chaCtrl;
			if (!flag2)
			{
				loader.no = chara.voiceNo;
			}
			if (!flag3)
			{
				loader.pitch = chara.voicePitch;
			}
			loader.voiceTrans = chara.voiceTrans;
		}
		switch ((Type)num2)
		{
		case Type.Normal:
			if (flag)
			{
				Voice.OncePlayChara(loader, delegate(AudioSource audioSource)
				{
					SetAudioSource(audioSource);
				});
			}
			else
			{
				SetAudioSource(Voice.OncePlayChara(loader));
			}
			break;
		case Type.Once:
			if (flag)
			{
				Voice.OncePlay(loader, delegate(AudioSource audioSource)
				{
					SetAudioSource(audioSource);
				});
			}
			else
			{
				SetAudioSource(Voice.OncePlay(loader));
			}
			break;
		case Type.Overlap:
			if (flag)
			{
				Voice.Play(loader, delegate(AudioSource audioSource)
				{
					SetAudioSource(audioSource);
				});
			}
			else
			{
				SetAudioSource(Voice.Play(loader));
			}
			break;
		}
		void SetAudioSource(AudioSource audioSource)
		{
			if (chaCtrl != null)
			{
				chaCtrl.SetVoiceTransform(audioSource);
			}
			if (audioSource != null)
			{
				if (useADV)
				{
					Manager.Sound.AudioSettingData3DOnly(audioSource, TextScenario.VOICE_SET_NO);
				}
				if (is2D)
				{
					audioSource.spatialBlend = 0f;
				}
				if (isLoop)
				{
					audioSource.loop = isLoop;
					base.scenario.loopVoiceList.Add(new TextScenario.LoopVoicePack(loader.no, chaCtrl, audioSource));
				}
			}
		}
	}
}
