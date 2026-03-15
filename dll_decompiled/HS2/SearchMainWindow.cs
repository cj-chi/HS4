using System.Collections;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class SearchMainWindow : SvsBase
{
	[Header("【肌の種類】------------------------")]
	[SerializeField]
	private Toggle[] tglSkins;

	[Header("【肌の色】------------------------")]
	[SerializeField]
	private Toggle[] tglSkinColors;

	[Header("【身長】------------------------")]
	[SerializeField]
	private Toggle[] tglHeights;

	[Header("【胸】----------------------")]
	[SerializeField]
	private Toggle[] tglBreasts;

	[Header("【体型】----------------------")]
	[SerializeField]
	private Toggle[] tglShapes;

	[Header("【髪型】----------------------")]
	[SerializeField]
	private Toggle[] tglHairs;

	[Header("【性格】----------------------")]
	[SerializeField]
	private Toggle[] tglPersosnals;

	[Header("【ホクロ】----------------------")]
	[SerializeField]
	private Toggle[] tglMoles;

	[Header("【オプション エルフ耳】----------------------")]
	[SerializeField]
	private Toggle tglElf;

	[Header("【オプション メガネ】----------------------")]
	[SerializeField]
	private Toggle tglGlasses;

	[Header("【初期化】----------------------")]
	[SerializeField]
	private Button btnInit;

	[Header("【探してもらう】----------------------")]
	[SerializeField]
	private Button btnSearch;

	[SerializeField]
	private Text txtSearch;

	private AudioSource audioSource;

	private int[] voiceCnt;

	protected override IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		voiceCnt = new int[base.searchBase.dictPersonality.Count];
		btnInit.OnClickAsObservable().Subscribe(delegate
		{
			InitDraw();
		});
		btnSearch.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			int hairPtn = (from t in tglHairs.Select((Toggle val, int idx) => (val: val, idx: idx))
				where t.val.isOn
				select t.idx).First();
			base.searchBase.chaRandom.RandomHair(base.chaCtrl, hairPtn);
			base.searchBase.chaRandom.RandomFace(base.chaCtrl, tglMoles[0].isOn, tglElf.isOn);
			int skin = (from t in tglSkins.Select((Toggle val, int idx) => (val: val, idx: idx))
				where t.val.isOn
				select t.idx).First();
			int skinColor = (from t in tglSkinColors.Select((Toggle val, int idx) => (val: val, idx: idx))
				where t.val.isOn
				select t.idx).First();
			int height = (from t in tglHeights.Select((Toggle val, int idx) => (val: val, idx: idx))
				where t.val.isOn
				select t.idx).First();
			int breast = (from t in tglBreasts.Select((Toggle val, int idx) => (val: val, idx: idx))
				where t.val.isOn
				select t.idx).First();
			int shape = (from t in tglShapes.Select((Toggle val, int idx) => (val: val, idx: idx))
				where t.val.isOn
				select t.idx).First();
			base.searchBase.chaRandom.RandomBody(base.chaCtrl, skin, skinColor, height, breast, shape);
			base.searchBase.chaRandom.RandomClothAndAccessory(base.chaCtrl, tglGlasses.isOn);
			int positive = (from t in tglPersosnals.Select((Toggle val, int idx) => (val: val, idx: idx))
				where t.val.isOn
				select t.idx).First();
			base.searchBase.chaRandom.RandomPersonal(base.chaCtrl, positive);
			base.chaCtrl.Reload();
			base.searchBase.foundFemaleWindow.UpdateUI(base.chaCtrl.chaFile);
			PlayVoice();
		});
		btnSearch.OnPointerEnterAsObservable().Subscribe(delegate
		{
			txtSearch.color = Game.selectFontColor;
		});
		btnSearch.OnPointerExitAsObservable().Subscribe(delegate
		{
			txtSearch.color = Game.defaultFontColor;
		});
		InitDraw();
		yield return null;
	}

	private void InitDraw()
	{
		tglSkins[4].SetIsOnWithoutCallback(isOn: true);
		tglSkinColors[3].SetIsOnWithoutCallback(isOn: true);
		tglHeights[3].SetIsOnWithoutCallback(isOn: true);
		tglBreasts[3].SetIsOnWithoutCallback(isOn: true);
		tglShapes[3].SetIsOnWithoutCallback(isOn: true);
		tglHairs[5].SetIsOnWithoutCallback(isOn: true);
		tglPersosnals[2].SetIsOnWithoutCallback(isOn: true);
		tglMoles[1].SetIsOnWithoutCallback(isOn: true);
		tglElf.SetIsOnWithoutCallback(isOn: false);
		tglGlasses.SetIsOnWithoutCallback(isOn: false);
	}

	private void PlayVoice()
	{
		if (!base.searchBase.playVoiceBackup.playSampleVoice)
		{
			base.searchBase.playVoiceBackup.backEyebrowPtn = base.chaCtrl.fileStatus.eyebrowPtn;
			base.searchBase.playVoiceBackup.backEyesPtn = base.chaCtrl.fileStatus.eyesPtn;
			base.searchBase.playVoiceBackup.backBlink = base.chaCtrl.fileStatus.eyesBlink;
			base.searchBase.playVoiceBackup.backEyesOpen = base.chaCtrl.fileStatus.eyesOpenMax;
			base.searchBase.playVoiceBackup.backMouthPtn = base.chaCtrl.fileStatus.mouthPtn;
			base.searchBase.playVoiceBackup.backMouthFix = base.chaCtrl.fileStatus.mouthFixed;
			base.searchBase.playVoiceBackup.backMouthOpen = base.chaCtrl.fileStatus.mouthOpenMax;
		}
		ListInfoBase listInfo = Singleton<Character>.Instance.chaListCtrl.GetListInfo(ChaListDefine.CategoryNo.cha_sample_voice, base.parameter2.personality);
		if (listInfo != null)
		{
			ChaListDefine.KeyType[] array = new ChaListDefine.KeyType[3]
			{
				ChaListDefine.KeyType.Eyebrow01,
				ChaListDefine.KeyType.Eyebrow02,
				ChaListDefine.KeyType.Eyebrow03
			};
			ChaListDefine.KeyType[] array2 = new ChaListDefine.KeyType[3]
			{
				ChaListDefine.KeyType.Eye01,
				ChaListDefine.KeyType.Eye02,
				ChaListDefine.KeyType.Eye03
			};
			ChaListDefine.KeyType[] array3 = new ChaListDefine.KeyType[3]
			{
				ChaListDefine.KeyType.EyeMax01,
				ChaListDefine.KeyType.EyeMax02,
				ChaListDefine.KeyType.EyeMax03
			};
			ChaListDefine.KeyType[] array4 = new ChaListDefine.KeyType[3]
			{
				ChaListDefine.KeyType.Mouth01,
				ChaListDefine.KeyType.Mouth02,
				ChaListDefine.KeyType.Mouth03
			};
			ChaListDefine.KeyType[] array5 = new ChaListDefine.KeyType[3]
			{
				ChaListDefine.KeyType.MouthMax01,
				ChaListDefine.KeyType.MouthMax02,
				ChaListDefine.KeyType.MouthMax03
			};
			ChaListDefine.KeyType[] array6 = new ChaListDefine.KeyType[3]
			{
				ChaListDefine.KeyType.EyeHiLight01,
				ChaListDefine.KeyType.EyeHiLight02,
				ChaListDefine.KeyType.EyeHiLight03
			};
			ChaListDefine.KeyType[] array7 = new ChaListDefine.KeyType[3]
			{
				ChaListDefine.KeyType.Data01,
				ChaListDefine.KeyType.Data02,
				ChaListDefine.KeyType.Data03
			};
			int num = (voiceCnt[base.parameter2.personality] = (voiceCnt[base.parameter2.personality] + 1) % array.Length);
			base.chaCtrl.ChangeEyebrowPtn(listInfo.GetInfoInt(array[num]));
			base.chaCtrl.ChangeEyesPtn(listInfo.GetInfoInt(array2[num]));
			base.chaCtrl.HideEyeHighlight(("0" == listInfo.GetInfo(array6[num])) ? true : false);
			base.chaCtrl.ChangeEyesBlinkFlag(blink: false);
			base.chaCtrl.ChangeEyesOpenMax(listInfo.GetInfoFloat(array3[num]));
			base.chaCtrl.ChangeMouthPtn(listInfo.GetInfoInt(array4[num]));
			base.chaCtrl.ChangeMouthFixed(fix: false);
			base.chaCtrl.ChangeMouthOpenMax(listInfo.GetInfoFloat(array5[num]));
			base.searchBase.playVoiceBackup.playSampleVoice = true;
			Manager.Sound.Stop(Manager.Sound.Type.SystemSE);
			audioSource = Utils.Sound.Play(new Utils.Sound.Setting(Manager.Sound.Type.SystemSE)
			{
				bundle = listInfo.GetInfo(ChaListDefine.KeyType.MainAB),
				asset = listInfo.GetInfo(array7[num])
			});
			audioSource.pitch = base.parameter2.voicePitch;
			base.chaCtrl.SetVoiceTransform(audioSource);
		}
	}
}
