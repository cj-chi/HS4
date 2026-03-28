using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion.Game;
using Manager;
using Sound;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ADV.Commands.Chara;

public class KaraokePlay : CommandBase
{
	private abstract class Setting
	{
		public string bundle;

		public string asset;

		public float fadeTime = 0.8f;

		public int settingNo = -1;

		public AudioSource source;
	}

	private class AudioSetting : Setting
	{
		public float time;

		public float panStereo;

		public AudioPlayer Load()
		{
			AssetBundleData assetBundleData = new AssetBundleData(bundle, asset);
			if (!assetBundleData.isFile)
			{
				return new AudioPlayer(this, null);
			}
			source = Utils.Sound.Get(Manager.Sound.Type.BGM, assetBundleData);
			AudioSource audioSource = UnityEngine.Object.Instantiate(source, source.transform.parent);
			audioSource.name = source.name;
			audioSource.panStereo = panStereo;
			Manager.Sound.AudioSettingData(audioSource, settingNo);
			return new AudioPlayer(this, audioSource);
		}
	}

	private class VoiceSetting : Setting
	{
		public int no;

		public bool is2D;

		public Transform voiceTrans;

		public VoicePlayer Load(ChaControl chaCtrl, TextScenario scenario)
		{
			AssetBundleData assetBundleData = new AssetBundleData(bundle, asset);
			if (!assetBundleData.isFile)
			{
				return new VoicePlayer(this, null);
			}
			source = Voice.CreateCache(no, assetBundleData);
			AudioSource audioSource = UnityEngine.Object.Instantiate(source, source.transform.parent);
			if (settingNo < 0)
			{
				if (!is2D)
				{
					audioSource.spatialBlend = ((voiceTrans != null) ? 1 : 0);
				}
				else
				{
					audioSource.spatialBlend = 0f;
				}
			}
			audioSource.name = source.name;
			audioSource.volume = Voice.GetVolume(no);
			Manager.Sound.AudioSettingData3DOnly(audioSource, TextScenario.VOICE_SET_NO);
			if (chaCtrl != null)
			{
				chaCtrl.SetVoiceTransform(scenario.info.audio.isNotMoveMouth ? null : audioSource);
			}
			return new VoicePlayer(this, audioSource);
		}
	}

	private abstract class Player : IDisposable
	{
		protected AudioSource source;

		protected Setting setting;

		protected bool isPlayed;

		protected bool isDisposed;

		public bool isSuccess
		{
			get
			{
				if (source != null)
				{
					return source.clip != null;
				}
				return false;
			}
		}

		public bool isOK
		{
			get
			{
				if (source.clip.loadState != AudioDataLoadState.Loaded)
				{
					return false;
				}
				if (!source.isActiveAndEnabled)
				{
					return false;
				}
				return true;
			}
		}

		public abstract void Play();

		public abstract void Dispose();

		public Player(Setting setting, AudioSource source)
		{
			this.setting = setting;
			this.source = source;
		}
	}

	private class AudioPlayer : Player
	{
		private bool isReleased;

		public AudioSetting audioSetting => setting as AudioSetting;

		public AudioPlayer(AudioSetting setting, AudioSource source)
			: base(setting, source)
		{
		}

		public override void Play()
		{
			if (base.isSuccess)
			{
				Manager.Sound.SetParent(Manager.Sound.Type.BGM, source.transform);
				GameObject currentBGM = Manager.Sound.currentBGM;
				Manager.Sound.currentBGM = source.gameObject;
				source.time = audioSetting.time;
				Manager.Sound.PlayFade(currentBGM, source, setting.fadeTime);
				isPlayed = true;
				source.OnDestroyAsObservable().Subscribe(delegate
				{
					Dispose();
				});
			}
		}

		public override void Dispose()
		{
			if (isDisposed)
			{
				return;
			}
			isDisposed = true;
			if (source == null)
			{
				Release();
				return;
			}
			FadePlayer component = source.GetComponent<FadePlayer>();
			if (component == null)
			{
				UnityEngine.Object.Destroy(source.gameObject);
				Release();
				return;
			}
			component.Stop(setting.fadeTime);
			component.OnDestroyAsObservable().Subscribe(delegate
			{
				Release();
			});
		}

		private void Release()
		{
			if (!isReleased)
			{
				isReleased = true;
				Utils.Sound.Remove(Manager.Sound.Type.BGM, setting.bundle, setting.asset);
			}
		}
	}

	private class VoicePlayer : Player, TextScenario.IVoice
	{
		private bool isReleased;

		private bool isReleaseForce;

		public VoiceSetting vs => setting as VoiceSetting;

		int TextScenario.IVoice.personality => vs.no;

		string TextScenario.IVoice.bundle => vs.bundle;

		string TextScenario.IVoice.asset => vs.asset;

		public VoicePlayer(VoiceSetting vs, AudioSource source)
			: base(vs, source)
		{
			if (!base.isSuccess)
			{
				return;
			}
			source.UpdateAsObservable().Subscribe(delegate
			{
				source.volume = Voice.GetVolume(vs.no);
			});
			Transform vt = source.transform;
			Transform voiceTrans = vs.voiceTrans;
			if (voiceTrans != null)
			{
				source.UpdateAsObservable().TakeUntilDestroy(voiceTrans).Subscribe(delegate
				{
					vt.SetPositionAndRotation(voiceTrans.position, voiceTrans.rotation);
				});
			}
		}

		public override void Play()
		{
			if (!base.isSuccess)
			{
				return;
			}
			Manager.Sound.PlayFade(null, source, setting.fadeTime);
			if (!isPlayed)
			{
				Voice.SetParent(vs.no, source.transform);
				(from _ in source.UpdateAsObservable()
					select source).SkipWhile((AudioSource audio) => audio.isPlaying).Take(1).Subscribe(delegate(AudioSource audio)
				{
					UnityEngine.Object.Destroy(audio.gameObject);
				});
				source.OnDestroyAsObservable().Subscribe(delegate
				{
					Dispose();
				});
			}
			isPlayed = true;
		}

		public override void Dispose()
		{
			if (isDisposed)
			{
				return;
			}
			isDisposed = true;
			if (source == null)
			{
				Release();
				return;
			}
			FadePlayer fadePlayer = null;
			if (isReleaseForce || (fadePlayer = source.GetComponent<FadePlayer>()) == null)
			{
				UnityEngine.Object.Destroy(source.gameObject);
				Release();
				return;
			}
			fadePlayer.Stop(setting.fadeTime);
			fadePlayer.OnDestroyAsObservable().Subscribe(delegate
			{
				Release();
			});
		}

		private void Release()
		{
			if (!isReleased)
			{
				isReleased = true;
				Voice.ReleaseCache(vs.no, setting.bundle, setting.asset);
			}
		}

		void TextScenario.IVoice.Convert2D()
		{
		}

		bool TextScenario.IVoice.Wait()
		{
			if (!base.isSuccess)
			{
				return false;
			}
			if (!isPlayed)
			{
				return true;
			}
			return source.isPlaying;
		}

		AudioSource TextScenario.IVoice.Play()
		{
			if (!isPlayed)
			{
				return source;
			}
			isReleaseForce = true;
			Dispose();
			VoiceSetting voiceSetting = vs;
			return Voice.Play(new Voice.Loader
			{
				no = voiceSetting.no,
				bundle = setting.bundle,
				asset = setting.asset,
				voiceTrans = null,
				pitch = 1f
			});
		}
	}

	private List<Player> playList = new List<Player>();

	public override string[] ArgsLabel => new string[10] { "No", "BundleBG", "AssetBG", "BundleVoice", "AssetVoice", "Fade", "Time", "PanStereo", "VoiceNo", "is2D" };

	public override string[] ArgsDefault => new string[10]
	{
		int.MaxValue.ToString(),
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		"0.8",
		"0",
		"0",
		string.Empty,
		bool.FalseString
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		string bundle = args[num++];
		string asset = args[num++];
		string bundle2 = args[num++];
		string asset2 = args[num++];
		float fadeTime = float.Parse(args[num++]);
		float time = float.Parse(args[num++]);
		float panStereo = float.Parse(args[num++]);
		int voiceNo = 0;
		bool flag = args.SafeProc(num++, delegate(string s)
		{
			voiceNo = int.Parse(s);
		});
		bool is2D = bool.Parse(args[num++]);
		AudioSetting obj = new AudioSetting
		{
			bundle = bundle,
			asset = asset,
			fadeTime = fadeTime,
			panStereo = panStereo,
			time = time
		};
		VoiceSetting voiceSetting = new VoiceSetting
		{
			bundle = bundle2,
			asset = asset2,
			fadeTime = fadeTime,
			no = voiceNo,
			is2D = is2D
		};
		CharaData chara = base.scenario.commandController.GetChara(no);
		if (chara != null)
		{
			if (!flag)
			{
				voiceSetting.no = chara.voiceNo;
			}
			voiceSetting.voiceTrans = chara.voiceTrans;
		}
		AudioPlayer item = obj.Load();
		VoicePlayer voicePlayer = voiceSetting.Load(chara.chaCtrl, base.scenario);
		playList.Add(item);
		playList.Add(voicePlayer);
		base.scenario.karaokeList.ForEach(delegate(IDisposable p)
		{
			p.Dispose();
		});
		base.scenario.karaokeList.Clear();
		base.scenario.karaokeList.Add(voicePlayer);
		base.scenario.currentCharaData.karaokePlayer = voicePlayer;
	}

	public override bool Process()
	{
		base.Process();
		if (playList.Any((Player p) => !p.isSuccess))
		{
			return true;
		}
		if (!playList.All((Player p) => p.isOK))
		{
			return false;
		}
		playList.ForEach(delegate(Player p)
		{
			p.Play();
		});
		return true;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
	}
}
