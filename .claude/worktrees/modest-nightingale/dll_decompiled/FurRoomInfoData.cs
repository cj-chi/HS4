using System.Linq;
using AIChara;
using AssetBudlePathSet;
using Illusion.Extensions;
using Manager;
using UnityEngine;

public class FurRoomInfoData : ScriptableObject
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

	[Header("Homeから呼んだときの音声")]
	public VociePath[] pathCallVoice;

	[Header("部屋に来たときの音声")]
	public VociePath[] pathRoomStartVoice;

	[Header("探すに合わせたときの音声")]
	public VociePath[] pathSearchVoice;

	[Header("実績に合わせたときの音声")]
	public VociePath[] pathAchievementVoice;

	[Header("ヘルプに合わせたときの音声")]
	public VociePath[] pathHelpVoice;

	[Header("Hに合わせたときの音声")]
	public VociePath[] pathHVoice;

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

	[Header("探すに合わせたときの音声")]
	public VociePath[] pathAngrySearchVoice;

	[Header("実績に合わせたときの音声")]
	public VociePath[] pathAngryAchievementVoice;

	[Header("ヘルプに合わせたときの音声")]
	public VociePath[] pathAngryHelpVoice;

	[Header("Hに合わせたときの音声")]
	public VociePath[] pathAngryHVoice;

	[Header("カスタムに合わせたときの音声")]
	public VociePath[] pathAngryCustomVoice;

	[Header("カスタムとH以外を押したとき音声")]
	public VociePath[] pathAngryClickVoice;

	[Header("カスタムを押したとき音声")]
	public VociePath[] pathAngryCustoClickVoice;

	[Header("部屋から出ていくとき(Home画面に戻る)音声")]
	public VociePath[] pathAngryBackVoice;

	public void SetCallVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathCallVoice.Shuffle().FirstOrDefault();
			Voice.OncePlay(new Voice.Loader
			{
				no = _chara.fileParam2.personality,
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}

	public void SetHVoice(ChaControl _chara, bool _isAngry)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = (_isAngry ? pathAngryHVoice : pathHVoice).Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
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
				no = _chara.fileParam2.personality,
				pitch = _chara.fileParam2.voicePitch,
				bundle = vociePath.path.bundle,
				asset = vociePath.path.file
			});
			_chara.SetVoiceTransform(voiceTransform);
			_chara.ChangeEyebrowPtn(vociePath.eyebrow);
			_chara.ChangeEyesPtn(vociePath.eyes);
			_chara.ChangeMouthPtn(vociePath.mouth);
			_chara.ChangeLookEyesPtn(vociePath.lookEyes);
			_chara.ChangeLookNeckPtn(vociePath.lookNeck);
		}
	}
}
