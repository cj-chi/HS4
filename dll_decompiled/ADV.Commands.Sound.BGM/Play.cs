using Illusion;
using Illusion.Game;
using UnityEngine;

namespace ADV.Commands.Sound.BGM;

public class Play : CommandBase
{
	public override string[] ArgsLabel => new string[8] { "Bundle", "Asset", "Fade", "isAsync", "Pitch", "PanStereo", "Time", "Volume" };

	public override string[] ArgsDefault => new string[8]
	{
		string.Empty,
		string.Empty,
		string.Empty,
		bool.TrueString,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int cnt = 0;
		string text = args[cnt++];
		Illusion.Game.Utils.Sound.SettingBGM setting;
		if (int.TryParse(text, out var result))
		{
			setting = new Illusion.Game.Utils.Sound.SettingBGM(result);
		}
		else
		{
			result = Illusion.Utils.Enum<Illusion.Game.BGM>.FindIndex(text, ignoreCase: true);
			if (result != -1)
			{
				setting = new Illusion.Game.Utils.Sound.SettingBGM(result);
			}
			else
			{
				setting = new Illusion.Game.Utils.Sound.SettingBGM(text);
			}
		}
		args.SafeProc(cnt++, delegate(string s)
		{
			setting.asset = s;
		});
		args.SafeProc(cnt++, delegate(string s)
		{
			setting.loader.fadeTime = float.Parse(s);
		});
		if (bool.Parse(args[cnt++]))
		{
			Illusion.Game.Utils.Sound.Play(setting, delegate(AudioSource audioSource)
			{
				SetAudioSource(audioSource);
			});
		}
		else
		{
			SetAudioSource(Illusion.Game.Utils.Sound.Play(setting));
		}
		void SetAudioSource(AudioSource audioSource)
		{
			if (!(audioSource == null))
			{
				args.SafeProc(cnt++, delegate(string s)
				{
					audioSource.pitch = float.Parse(s);
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					audioSource.panStereo = float.Parse(s);
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					audioSource.time = float.Parse(s);
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					audioSource.volume = float.Parse(s);
				});
			}
		}
	}
}
