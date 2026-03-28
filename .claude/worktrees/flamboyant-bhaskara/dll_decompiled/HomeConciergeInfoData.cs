using System.Linq;
using AIChara;
using AssetBudlePathSet;
using Illusion.Extensions;
using Manager;
using UnityEngine;

public class HomeConciergeInfoData : ScriptableObject
{
	public enum AnimationKind
	{
		Idle,
		Bow,
		Select,
		Shy,
		Angry
	}

	[Header("アニメーションID  anim_f_motion_00.xlsのポーズID欄参照 ")]
	public int[] animIDs = new int[3];

	[Header("立ち位置")]
	public TransformInfoData transformInfoData;

	[Header("電話で呼んだときの音声")]
	public VociePath[] pathCallVoice;

	[Header("部屋に入る前の音声")]
	public VociePath[] pathFrontOfTheRoomVoice;

	[Header("部屋に来たときの音声")]
	public VociePath[] pathRoomStartVoice;

	[Header("探すに合わせたときの音声")]
	public VociePath[] pathSearchVoice;

	[Header("実績に合わせたときの音声")]
	public VociePath[] pathAchievementVoice;

	[Header("ヘルプに合わせたときの音声")]
	public VociePath[] pathHelpVoice;

	[Header("Hに合わせたときH回数 一回目の音声")]
	public VociePath[] pathHVoice1;

	[Header("Hに合わせたときH回数 二回目の音声")]
	public VociePath[] pathHVoice2;

	[Header("Hに合わせたときH回数 三回目の音声")]
	public VociePath[] pathHVoice3;

	[Header("Hに合わせたときH回数 四回目以降の音声")]
	public VociePath[] pathHVoice4;

	[Header("カスタムに合わせたときの音声")]
	public VociePath[] pathCustomVoice;

	[Header("カスタムとH以外を押したとき音声")]
	public VociePath[] pathClickVoice;

	[Header("カスタムを押したとき音声")]
	public VociePath[] pathCustoClickVoice;

	[Header("部屋から出ていくとき(Home画面に戻る)音声")]
	public VociePath[] pathBackVoice;

	[Header("怒ったとき音声")]
	public VociePath[] pathAngryVoice;

	[Header("怒ったとき探すに合わせたときの音声")]
	public VociePath[] pathAngrySearchVoice;

	[Header("怒ったとき実績に合わせたときの音声")]
	public VociePath[] pathAngryAchievementVoice;

	[Header("怒ったときヘルプに合わせたときの音声")]
	public VociePath[] pathAngryHelpVoice;

	[Header("怒ったときHに合わせたときの音声")]
	public VociePath[] pathAngryHVoice;

	[Header("怒ったときカスタムに合わせたときの音声")]
	public VociePath[] pathAngryCustomVoice;

	[Header("怒ったときカスタムとH以外を押したとき音声")]
	public VociePath[] pathAngryClickVoice;

	[Header("怒ったときカスタムを押したとき音声")]
	public VociePath[] pathAngryCustoClickVoice;

	[Header("怒ったとき部屋から出ていくとき(Home画面に戻る)音声")]
	public VociePath[] pathAngryBackVoice;

	[Header("特別待遇室に行くに合わせたときの音声")]
	public VociePath[] pathSelectGotoSpecialRoom;

	[Header("特別待遇室に行くを選択したときの音声")]
	public VociePath[] pathClickGotoSpecialRoom;

	[Header("怒ったとき特別待遇室に行くに合わせたときの音声")]
	public VociePath[] pathAngrySelectGotoSpecialRoom;

	[Header("怒ったとき特別待遇室に行くを選択したときの音声")]
	public VociePath[] pathAngryClickGotoSpecialRoom;

	public void SetCallVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathCallVoice.Shuffle().FirstOrDefault();
			Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
		}
	}

	public void SetFrontOfTheRoomVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathFrontOfTheRoomVoice.Shuffle().FirstOrDefault();
			Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
		}
	}

	public void SetRoomStartVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathRoomStartVoice.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetAngryVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathAngryVoice.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetSearchVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngrySearchVoice : pathSearchVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetAchievementVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryAchievementVoice : pathAchievementVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetHelpVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryHelpVoice : pathHelpVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetHVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			int hCount = _chara.fileGameInfo2.hCount;
			VociePath[] self = (_isAngry ? pathAngryHVoice : ((hCount > 2) ? pathHVoice4 : ((hCount > 1) ? pathHVoice3 : ((hCount <= 0) ? pathHVoice1 : pathHVoice2))));
			VociePath vociePath = self.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetCustomVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryCustomVoice : pathCustomVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetClickVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryClickVoice : pathClickVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetCustomClickVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryCustoClickVoice : pathCustoClickVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetBackVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryBackVoice : pathBackVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -1,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetSelectGotoSepcialRoom(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngrySelectGotoSpecialRoom : pathSelectGotoSpecialRoom).Shuffle().FirstOrDefault();
			if (vociePath != null)
			{
				AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
				{
					no = -1,
					pitch = _chara.fileParam2.voicePitch,
					bundle = vociePath.path.bundle,
					asset = vociePath.path.file
				});
				_chara.SetVoiceTransform(voiceTransform);
				_chara.ChangeEyebrowPtn(vociePath.eyebrow);
				_chara.ChangeEyesPtn(vociePath.eyes);
				_chara.ChangeMouthPtn(vociePath.mouth);
				_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
				_chara.ChangeLookEyesPtn(vociePath.lookEyes);
				_chara.ChangeLookNeckPtn(vociePath.lookNeck);
			}
		}
	}

	public void SetClickGotoSepcialRoom(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryClickGotoSpecialRoom : pathClickGotoSpecialRoom).Shuffle().FirstOrDefault();
			if (vociePath != null)
			{
				AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
				{
					no = -1,
					pitch = _chara.fileParam2.voicePitch,
					bundle = vociePath.path.bundle,
					asset = vociePath.path.file
				});
				_chara.SetVoiceTransform(voiceTransform);
				_chara.ChangeEyebrowPtn(vociePath.eyebrow);
				_chara.ChangeEyesPtn(vociePath.eyes);
				_chara.ChangeMouthPtn(vociePath.mouth);
				_chara.ChangeMouthOpenMax(vociePath.mouthOpenMax);
				_chara.ChangeLookEyesPtn(vociePath.lookEyes);
				_chara.ChangeLookNeckPtn(vociePath.lookNeck);
			}
		}
	}
}
