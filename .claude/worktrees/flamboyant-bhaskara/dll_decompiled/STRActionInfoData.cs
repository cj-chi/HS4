using System.Linq;
using AIChara;
using AssetBudlePathSet;
using Illusion.Extensions;
using Manager;
using UnityEngine;

public class STRActionInfoData : ScriptableObject
{
	public enum AnimationKind
	{
		Idle,
		Bow
	}

	[Header("アニメーションID  anim_f_motion_00.xlsのポーズID欄参照 ")]
	public int[] animIDs = new int[2];

	[Header("立ち位置")]
	public TransformInfoData transformInfoData;

	[Header("訪れたときの音声")]
	public VociePath[] pathVisit;

	[Header("プラン選択押したときの音声")]
	public VociePath[] pathPlanSelect;

	[Header("シトリーH1回目押したとき")]
	public VociePath[] pathSitriH1;

	[Header("シトリーH2回目押したとき")]
	public VociePath[] pathSitriH2;

	[Header("シトリーH3回目押したとき")]
	public VociePath[] pathSitriH3;

	[Header("シトリーH4回目押したとき")]
	public VociePath[] pathSitriH4;

	[Header("容姿変更押したとき")]
	public VociePath[] pathEdit;

	[Header("ロビー押したとき")]
	public VociePath[] pathLobby;

	[Header("戻る押したとき")]
	public VociePath[] pathBack;

	public void SetVisitVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathVisit.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -2,
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

	public void SetPlanSelectVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathPlanSelect.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -2,
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

	public void SetHVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			int sitriSelectCount = Singleton<Game>.Instance.appendSaveData.SitriSelectCount;
			VociePath[] self = ((sitriSelectCount > 2) ? pathSitriH4 : ((sitriSelectCount > 1) ? pathSitriH3 : ((sitriSelectCount <= 0) ? pathSitriH1 : pathSitriH2)));
			VociePath vociePath = self.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -2,
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

	public void SetEditVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathEdit.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -2,
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

	public void SetLobbyVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathLobby.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -2,
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

	public void SetBackVoice(ChaControl _chara)
	{
		if (!(_chara == null))
		{
			VociePath vociePath = pathBack.Shuffle().FirstOrDefault();
			AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
			{
				no = -2,
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
