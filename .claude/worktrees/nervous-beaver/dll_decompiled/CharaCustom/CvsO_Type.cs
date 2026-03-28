using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsO_Type : CvsBase
{
	[SerializeField]
	private GameObject objTop;

	[SerializeField]
	private GameObject objTemp;

	[SerializeField]
	private CustomSliderSet ssVoiceRate;

	private Toggle[] tglType;

	private AudioSource audioSource;

	private int[] voiceCnt;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssVoiceRate.SetSliderValue(base.parameter2.voiceRate);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		int num = Array.IndexOf(base.customBase.dictPersonality.Keys.ToArray(), base.parameter2.personality);
		tglType[num].SetIsOnWithoutCallback(isOn: true);
		for (int i = 0; i < tglType.Length; i++)
		{
			if (i != num)
			{
				tglType[i].SetIsOnWithoutCallback(isOn: false);
			}
		}
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssVoiceRate.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.parameter2.voiceRate));
	}

	public void PlayVoice()
	{
		if (!base.customBase.playVoiceBackup.playSampleVoice)
		{
			base.customBase.playVoiceBackup.backEyebrowPtn = base.chaCtrl.fileStatus.eyebrowPtn;
			base.customBase.playVoiceBackup.backEyesPtn = base.chaCtrl.fileStatus.eyesPtn;
			base.customBase.playVoiceBackup.backBlink = base.chaCtrl.fileStatus.eyesBlink;
			base.customBase.playVoiceBackup.backEyesOpen = base.chaCtrl.fileStatus.eyesOpenMax;
			base.customBase.playVoiceBackup.backMouthPtn = base.chaCtrl.fileStatus.mouthPtn;
			base.customBase.playVoiceBackup.backMouthFix = base.chaCtrl.fileStatus.mouthFixed;
			base.customBase.playVoiceBackup.backMouthOpen = base.chaCtrl.fileStatus.mouthOpenMax;
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
			base.customBase.playVoiceBackup.playSampleVoice = true;
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

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsType += UpdateCustomUI;
		voiceCnt = new int[base.customBase.dictPersonality.Count];
		tglType = new Toggle[base.customBase.dictPersonality.Keys.Count];
		foreach (var item in base.customBase.dictPersonality.Select((KeyValuePair<int, string> val, int idx) => new { val, idx }))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(objTemp);
			gameObject.name = "tglRbSel_" + item.idx.ToString("00");
			tglType[item.idx] = gameObject.GetComponent<Toggle>();
			ToggleGroup component = objTop.GetComponent<ToggleGroup>();
			tglType[item.idx].group = component;
			gameObject.transform.SetParent(objTop.transform, worldPositionStays: false);
			Transform transform = gameObject.transform.Find("textRbSelect");
			if (null != transform)
			{
				Text component2 = transform.GetComponent<Text>();
				if ((bool)component2)
				{
					component2.text = item.val.Value;
				}
			}
			gameObject.SetActiveIfDifferent(active: true);
		}
		tglType.Select((Toggle p, int idx) => new
		{
			toggle = p,
			index = (byte)idx
		}).ToList().ForEach(p =>
		{
			p.toggle.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
			{
				if (!base.customBase.updateCustomUI && isOn)
				{
					int[] array = base.customBase.dictPersonality.Keys.ToArray();
					base.parameter2.personality = array[p.index];
					PlayVoice();
				}
			});
		});
		ssVoiceRate.onChange = delegate(float value)
		{
			base.parameter2.voiceRate = value;
			if (Manager.Sound.IsPlay(Manager.Sound.Type.SystemSE))
			{
				audioSource.pitch = base.parameter2.voicePitch;
			}
		};
		ssVoiceRate.onPointerUp = delegate
		{
			PlayVoice();
		};
		ssVoiceRate.onSetDefaultValue = () => 0.5f;
		StartCoroutine(SetInputText());
	}
}
