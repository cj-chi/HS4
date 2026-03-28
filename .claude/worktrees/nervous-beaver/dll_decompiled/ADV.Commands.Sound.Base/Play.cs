using Illusion.Game;
using Manager;
using UnityEngine;

namespace ADV.Commands.Sound.Base;

public abstract class Play : CommandBase
{
	private Manager.Sound.Type type;

	private bool isWait;

	private bool isStop;

	private Transform transform;

	private float timer;

	private Vector3? move;

	private float? stopTime;

	private bool loadEnd;

	public override string[] ArgsLabel => new string[17]
	{
		"Bundle", "Asset", "Fade", "isName", "isAsync", "SettingNo", "isWait", "isStop", "isLoop", "Pitch",
		"PanStereo", "SpatialBlend", "Time", "Volume", "Pos", "Move", "Stop"
	};

	public override string[] ArgsDefault => new string[17]
	{
		string.Empty,
		string.Empty,
		"0",
		bool.TrueString,
		bool.TrueString,
		"-1",
		bool.FalseString,
		bool.FalseString,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty
	};

	public Play(Manager.Sound.Type type)
	{
		this.type = type;
	}

	public override void Do()
	{
		base.Do();
		Utils.Sound.Setting setting = new Utils.Sound.Setting(type);
		int cnt = 0;
		setting.bundle = args[cnt++];
		setting.asset = args[cnt++];
		setting.loader.fadeTime = float.Parse(args[cnt++]);
		setting.loader.isAssetEqualPlay = bool.Parse(args[cnt++]);
		bool num = bool.Parse(args[cnt++]);
		setting.loader.settingNo = int.Parse(args[cnt++]);
		isWait = bool.Parse(args[cnt++]);
		isStop = bool.Parse(args[cnt++]);
		if (num)
		{
			Utils.Sound.Play(setting, delegate(AudioSource audioSource)
			{
				SetAudioSouce(audioSource);
			});
		}
		else
		{
			SetAudioSouce(Utils.Sound.Play(setting));
		}
		void SetAudioSouce(AudioSource audioSource)
		{
			loadEnd = true;
			if (!(audioSource == null))
			{
				transform = audioSource.transform;
				args.SafeProc(cnt++, delegate(string s)
				{
					audioSource.loop = bool.Parse(s);
				});
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
					audioSource.spatialBlend = float.Parse(s);
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					audioSource.time = float.Parse(s);
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					audioSource.volume = float.Parse(s);
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					if (!base.scenario.commandController.V3Dic.TryGetValue(s, out var value))
					{
						int cnt2 = 0;
						CommandBase.CountAddV3(s.Split(','), ref cnt2, ref value);
					}
					transform.position = value;
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					if (!base.scenario.commandController.V3Dic.TryGetValue(s, out var value))
					{
						int cnt2 = 0;
						CommandBase.CountAddV3(s.Split(','), ref cnt2, ref value);
					}
					move = value;
				});
				args.SafeProc(cnt++, delegate(string s)
				{
					stopTime = float.Parse(s);
				});
			}
		}
	}

	public override bool Process()
	{
		base.Process();
		if (!isWait)
		{
			return true;
		}
		if (!loadEnd)
		{
			return false;
		}
		if (transform == null)
		{
			return true;
		}
		if (!Manager.Sound.IsPlay(type, transform))
		{
			return true;
		}
		if (move.HasValue)
		{
			transform.Translate(move.Value * Time.deltaTime);
		}
		if (stopTime.HasValue && timer >= stopTime.Value)
		{
			return true;
		}
		return false;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		if (isStop)
		{
			Manager.Sound.Stop(transform);
		}
	}
}
