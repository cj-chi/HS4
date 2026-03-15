using System;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsCaptureMenu : MonoBehaviour
{
	[Header("【描画】------------------------")]
	[SerializeField]
	private Toggle[] tglClothesState;

	[SerializeField]
	private Toggle[] tglAcsState;

	[Header("【キャラ】----------------------")]
	[SerializeField]
	private Toggle[] tglEyeLook;

	[SerializeField]
	private Toggle[] tglNeckLook;

	[SerializeField]
	private Button[] btnPose;

	[SerializeField]
	private InputField inpPoseNo;

	[SerializeField]
	private Button[] btnEyebrow;

	[SerializeField]
	private InputField inpEyebrowNo;

	[SerializeField]
	private Button[] btnEyePtn;

	[SerializeField]
	private InputField inpEyeNo;

	[SerializeField]
	private Slider sldEyeOpen;

	[SerializeField]
	private Button[] btnMouthPtn;

	[SerializeField]
	private InputField inpMouthNo;

	[SerializeField]
	private Slider sldMouthOpen;

	[SerializeField]
	private Button[] btnHandLPtn;

	[SerializeField]
	private InputField inpHandLNo;

	[SerializeField]
	private Button[] btnHandRPtn;

	[SerializeField]
	private InputField inpHandRNo;

	[SerializeField]
	private UI_ToggleOnOffEx tglPlayAnime;

	[Header("【ライト】----------------------")]
	[SerializeField]
	private Slider sldLightRotX;

	[SerializeField]
	private Slider sldLightRotY;

	[SerializeField]
	private CustomColorSet csLight;

	[SerializeField]
	private Slider sldLightPower;

	[SerializeField]
	private Button btnLightReset;

	[Header("【フレーム】--------------------")]
	[SerializeField]
	private GameObject objBackFrame;

	[SerializeField]
	private Toggle tglBFrameDraw;

	[SerializeField]
	private Button[] btnBFramePtn;

	[SerializeField]
	private Toggle tglFFrameDraw;

	[SerializeField]
	private Button[] btnFFramePtn;

	[Header("【背景】------------------------")]
	[SerializeField]
	private Toggle[] tglBG;

	[SerializeField]
	private GameObject objBGIndex;

	[SerializeField]
	private Button[] btnBGIndex;

	[SerializeField]
	private GameObject objBGColor;

	[SerializeField]
	private CustomColorSet csBG;

	[Header("【終了】------------------------")]
	[SerializeField]
	private Button btnCancel;

	[SerializeField]
	private Button btnSave;

	[SerializeField]
	private Text textSaveName;

	private int handLPtn;

	private int handRPtn;

	private bool backAutoClothesState;

	private int backClothesNo;

	private int backAutoClothesStateNo;

	protected CustomBase customBase => Singleton<CustomBase>.Instance;

	protected ChaListControl lstCtrl => Singleton<Character>.Instance.chaListCtrl;

	protected ChaControl chaCtrl => customBase.chaCtrl;

	public void BeginCapture()
	{
		backAutoClothesState = customBase.autoClothesState;
		backClothesNo = customBase.clothesStateNo;
		backAutoClothesStateNo = customBase.autoClothesStateNo;
		int clothesStateNo = customBase.clothesStateNo;
		tglClothesState[clothesStateNo].SetIsOnWithoutCallback(isOn: true);
		for (int i = 0; i < tglClothesState.Length; i++)
		{
			if (i != clothesStateNo)
			{
				tglClothesState[i].SetIsOnWithoutCallback(isOn: false);
			}
		}
		tglAcsState[0].SetIsOnWithoutCallback(customBase.accessoryDraw);
		tglAcsState[1].SetIsOnWithoutCallback(!customBase.accessoryDraw);
		tglEyeLook[0].SetIsOnWithoutCallback(customBase.eyelook == 0);
		tglEyeLook[1].SetIsOnWithoutCallback(1 == customBase.eyelook);
		tglNeckLook[0].SetIsOnWithoutCallback(customBase.necklook == 0);
		tglNeckLook[1].SetIsOnWithoutCallback(1 == customBase.necklook);
		chaCtrl.ChangeEyesBlinkFlag(blink: false);
		chaCtrl.ChangeMouthFixed(fix: true);
		chaCtrl.ChangeMouthOpenMax(0f);
		sldEyeOpen.value = 1f;
		sldMouthOpen.value = 0f;
		chaCtrl.SetEnableShapeHand(0, enable: false);
		chaCtrl.SetShapeHandIndex(0, 0);
		chaCtrl.SetEnableShapeHand(1, enable: false);
		chaCtrl.SetShapeHandIndex(1, 0);
		inpPoseNo.text = customBase.poseNo.ToString();
		inpEyebrowNo.text = (customBase.eyebrowPtn + 1).ToString();
		inpEyeNo.text = (customBase.eyePtn + 1).ToString();
		inpMouthNo.text = (customBase.mouthPtn + 1).ToString();
		inpHandLNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
		inpHandRNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
		tglPlayAnime.SetIsOnWithoutCallback(customBase.playPoseAnime);
		Vector3 localEulerAngles = customBase.lightCustom.transform.localEulerAngles;
		sldLightRotX.value = ((89f < localEulerAngles.x) ? (localEulerAngles.x - 360f) : localEulerAngles.x);
		sldLightRotY.value = ((180f <= localEulerAngles.y) ? (localEulerAngles.y - 360f) : localEulerAngles.y);
		csLight.SetColor(customBase.lightCustom.color);
		sldLightPower.value = customBase.lightCustom.intensity;
		customBase.drawSaveFrameTop = true;
		tglBFrameDraw.isOn = customBase.drawSaveFrameBack;
		tglFFrameDraw.isOn = customBase.drawSaveFrameFront;
		tglBG[0].SetIsOnWithoutCallback(customBase.customCtrl.draw3D);
		tglBG[1].SetIsOnWithoutCallback(!customBase.customCtrl.draw3D);
		csBG.SetColor(customBase.customCtrl.GetBGColor());
		objBGIndex.SetActiveIfDifferent(!customBase.customCtrl.draw3D);
		objBGColor.SetActiveIfDifferent(customBase.customCtrl.draw3D);
		objBackFrame.SetActiveIfDifferent(!customBase.customCtrl.draw3D);
		if (customBase.customCtrl.saveMode)
		{
			textSaveName.text = CharaCustomDefine.CustomCapSave[Singleton<GameSystem>.Instance.languageInt];
		}
		else
		{
			textSaveName.text = CharaCustomDefine.CustomCapUpdate[Singleton<GameSystem>.Instance.languageInt];
		}
	}

	public void EndCapture()
	{
		customBase.autoClothesState = backAutoClothesState;
		customBase.clothesStateNo = backClothesNo;
		customBase.autoClothesStateNo = backAutoClothesStateNo;
		customBase.ChangeClothesState(-1);
		chaCtrl.ChangeEyesBlinkFlag(blink: true);
		chaCtrl.ChangeMouthFixed(fix: false);
		chaCtrl.ChangeEyesOpenMax(1f);
		chaCtrl.ChangeMouthOpenMax(1f);
		chaCtrl.SetEnableShapeHand(0, enable: false);
		chaCtrl.SetEnableShapeHand(1, enable: false);
		customBase.drawSaveFrameTop = false;
		customBase.drawMenu.UpdateUI();
	}

	protected virtual void Start()
	{
		customBase.lstInputField.Add(inpPoseNo);
		customBase.lstInputField.Add(inpEyebrowNo);
		customBase.lstInputField.Add(inpEyeNo);
		customBase.lstInputField.Add(inpMouthNo);
		customBase.lstInputField.Add(inpHandLNo);
		customBase.lstInputField.Add(inpHandRNo);
		customBase.drawSaveFrameBack = true;
		customBase.drawSaveFrameFront = true;
		if (tglClothesState.Any())
		{
			(from item in tglClothesState.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.val.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					customBase.ChangeClothesState(item.idx + 1);
				});
			});
		}
		if (tglAcsState.Any())
		{
			(from item in tglAcsState.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.val.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					customBase.accessoryDraw = item.idx == 0;
				});
			});
		}
		if (tglEyeLook.Any())
		{
			(from item in tglEyeLook.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.val.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					customBase.eyelook = item.idx;
				});
			});
		}
		if (tglNeckLook.Any())
		{
			(from item in tglNeckLook.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.val.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					customBase.necklook = item.idx;
				});
			});
		}
		if (btnPose.Any())
		{
			(from item in btnPose.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					if (2 == item.idx)
					{
						customBase.poseNo = 1;
					}
					else
					{
						customBase.ChangeAnimationNext(item.idx);
					}
					inpPoseNo.text = customBase.poseNo.ToString();
				});
			});
		}
		if ((bool)inpPoseNo)
		{
			inpPoseNo.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				if (!int.TryParse(value, out var result))
				{
					result = 0;
				}
				customBase.ChangeAnimationNo(result);
				inpPoseNo.text = customBase.poseNo.ToString();
			});
		}
		if (btnEyebrow.Any())
		{
			(from item in btnEyebrow.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					if (2 == item.idx)
					{
						customBase.ChangeEyebrowPtnNext(-1);
					}
					else
					{
						customBase.ChangeEyebrowPtnNext(item.idx);
					}
					inpEyebrowNo.text = (customBase.eyebrowPtn + 1).ToString();
				});
			});
		}
		if ((bool)inpEyebrowNo)
		{
			inpEyebrowNo.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				if (!int.TryParse(value, out var result))
				{
					result = 0;
				}
				customBase.ChangeEyebrowPtnNo(result);
				inpEyebrowNo.text = (customBase.eyebrowPtn + 1).ToString();
			});
		}
		if (btnEyePtn.Any())
		{
			(from item in btnEyePtn.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					if (2 == item.idx)
					{
						customBase.ChangeEyePtnNext(-1);
					}
					else
					{
						customBase.ChangeEyePtnNext(item.idx);
					}
					inpEyeNo.text = (customBase.eyePtn + 1).ToString();
				});
			});
		}
		if ((bool)inpEyeNo)
		{
			inpEyeNo.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				if (!int.TryParse(value, out var result))
				{
					result = 0;
				}
				customBase.ChangeEyePtnNo(result);
				inpEyeNo.text = (customBase.eyePtn + 1).ToString();
			});
		}
		if ((bool)sldEyeOpen)
		{
			sldEyeOpen.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				chaCtrl.ChangeEyesOpenMax(val);
			});
		}
		sldEyeOpen.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (customBase.sliderControlWheel)
			{
				sldEyeOpen.value = Mathf.Clamp(sldEyeOpen.value + scl.scrollDelta.y * -0.01f, 0f, 100f);
			}
		});
		if (btnMouthPtn.Any())
		{
			(from item in btnMouthPtn.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					if (2 == item.idx)
					{
						customBase.ChangeMouthPtnNext(-1);
					}
					else
					{
						customBase.ChangeMouthPtnNext(item.idx);
					}
					inpMouthNo.text = (customBase.mouthPtn + 1).ToString();
				});
			});
		}
		if ((bool)inpMouthNo)
		{
			inpMouthNo.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				if (!int.TryParse(value, out var result))
				{
					result = 0;
				}
				customBase.ChangeMouthPtnNo(result);
				inpMouthNo.text = (customBase.mouthPtn + 1).ToString();
			});
		}
		if ((bool)sldMouthOpen)
		{
			sldMouthOpen.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				chaCtrl.ChangeMouthOpenMax(val);
			});
		}
		sldMouthOpen.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (customBase.sliderControlWheel)
			{
				sldMouthOpen.value = Mathf.Clamp(sldMouthOpen.value + scl.scrollDelta.y * -0.01f, 0f, 100f);
			}
		});
		if (btnHandLPtn.Any())
		{
			(from item in btnHandLPtn.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					int num = chaCtrl.GetShapeIndexHandCount() + 1;
					if (2 == item.idx)
					{
						handLPtn = 0;
					}
					else if (item.idx == 0)
					{
						handLPtn = (handLPtn + num - 1) % num;
					}
					else
					{
						handLPtn = (handLPtn + 1) % num;
					}
					if (handLPtn == 0)
					{
						chaCtrl.SetEnableShapeHand(0, enable: false);
					}
					else
					{
						chaCtrl.SetEnableShapeHand(0, enable: true);
						chaCtrl.SetShapeHandIndex(0, handLPtn - 1);
					}
					if (handLPtn == 0)
					{
						inpHandLNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
					}
					else
					{
						inpHandLNo.text = handLPtn.ToString();
					}
				});
			});
		}
		if ((bool)inpHandLNo)
		{
			inpHandLNo.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				int result;
				if (value == CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt])
				{
					result = 0;
				}
				else if (!int.TryParse(value, out result))
				{
					result = 0;
				}
				handLPtn = result;
				if (handLPtn == 0)
				{
					chaCtrl.SetEnableShapeHand(0, enable: false);
				}
				else
				{
					chaCtrl.SetEnableShapeHand(0, enable: true);
					chaCtrl.SetShapeHandIndex(0, handLPtn - 1);
				}
				if (handLPtn == 0)
				{
					inpHandLNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
				}
				else
				{
					inpHandLNo.text = handLPtn.ToString();
				}
			});
		}
		if (btnHandRPtn.Any())
		{
			(from item in btnHandRPtn.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					int num = chaCtrl.GetShapeIndexHandCount() + 1;
					if (2 == item.idx)
					{
						handRPtn = 0;
					}
					else if (item.idx == 0)
					{
						handRPtn = (handRPtn + num - 1) % num;
					}
					else
					{
						handRPtn = (handRPtn + 1) % num;
					}
					if (handRPtn == 0)
					{
						chaCtrl.SetEnableShapeHand(1, enable: false);
					}
					else
					{
						chaCtrl.SetEnableShapeHand(1, enable: true);
						chaCtrl.SetShapeHandIndex(1, handRPtn - 1);
					}
					if (handRPtn == 0)
					{
						inpHandRNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
					}
					else
					{
						inpHandRNo.text = handRPtn.ToString();
					}
				});
			});
		}
		if ((bool)inpHandRNo)
		{
			inpHandRNo.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				int result;
				if (value == CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt])
				{
					result = 0;
				}
				else if (!int.TryParse(value, out result))
				{
					result = 0;
				}
				handRPtn = result;
				if (handRPtn == 0)
				{
					chaCtrl.SetEnableShapeHand(1, enable: false);
				}
				else
				{
					chaCtrl.SetEnableShapeHand(1, enable: true);
					chaCtrl.SetShapeHandIndex(1, handRPtn - 1);
				}
				if (handRPtn == 0)
				{
					inpHandRNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
				}
				else
				{
					inpHandRNo.text = handRPtn.ToString();
				}
			});
		}
		if ((bool)tglPlayAnime)
		{
			tglPlayAnime.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				customBase.playPoseAnime = isOn;
			});
		}
		if ((bool)sldLightRotX)
		{
			sldLightRotX.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				customBase.lightCustom.transform.localEulerAngles = new Vector3(val, customBase.lightCustom.transform.localEulerAngles.y, customBase.lightCustom.transform.localEulerAngles.z);
			});
		}
		sldLightRotX.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (customBase.sliderControlWheel)
			{
				sldLightRotX.value = Mathf.Clamp(sldLightRotX.value + scl.scrollDelta.y * -0.01f, 0f, 100f);
			}
		});
		if ((bool)sldLightRotY)
		{
			sldLightRotY.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				customBase.lightCustom.transform.localEulerAngles = new Vector3(customBase.lightCustom.transform.localEulerAngles.x, val, customBase.lightCustom.transform.localEulerAngles.z);
			});
		}
		sldLightRotY.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (customBase.sliderControlWheel)
			{
				sldLightRotY.value = Mathf.Clamp(sldLightRotY.value + scl.scrollDelta.y, -88f, 88f);
			}
		});
		if ((bool)csLight)
		{
			csLight.actUpdateColor = delegate(Color color)
			{
				customBase.lightCustom.color = color;
			};
		}
		if ((bool)sldLightPower)
		{
			sldLightPower.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				customBase.lightCustom.intensity = val;
			});
		}
		sldLightPower.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (customBase.sliderControlWheel)
			{
				sldLightPower.value = Mathf.Clamp(sldLightPower.value + scl.scrollDelta.y, -178f, 178f);
			}
		});
		if ((bool)btnLightReset)
		{
			btnLightReset.OnClickAsObservable().Subscribe(delegate
			{
				customBase.ResetLightSetting();
				Vector3 localEulerAngles = customBase.lightCustom.transform.localEulerAngles;
				sldLightRotX.value = ((89f < localEulerAngles.x) ? (localEulerAngles.x - 360f) : localEulerAngles.x);
				sldLightRotY.value = ((180f <= localEulerAngles.y) ? (localEulerAngles.y - 360f) : localEulerAngles.y);
				csLight.SetColor(customBase.lightCustom.color);
				sldLightPower.value = customBase.lightCustom.intensity;
			});
		}
		if ((bool)tglBFrameDraw)
		{
			tglBFrameDraw.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				customBase.drawSaveFrameBack = isOn;
			});
		}
		if (btnBFramePtn.Any())
		{
			(from item in btnBFramePtn.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					customBase.saveFrameAssist.ChangeSaveFrameBack((byte)item.idx);
				});
			});
		}
		if ((bool)tglFFrameDraw)
		{
			tglFFrameDraw.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				customBase.drawSaveFrameFront = isOn;
			});
		}
		if (btnFFramePtn.Any())
		{
			(from item in btnFFramePtn.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					customBase.saveFrameAssist.ChangeSaveFrameFront((byte)item.idx);
				});
			});
		}
		if (tglBG.Any())
		{
			(from item in tglBG.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.val.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					customBase.customCtrl.draw3D = item.idx == 0;
					objBGIndex.SetActiveIfDifferent(!customBase.customCtrl.draw3D);
					objBGColor.SetActiveIfDifferent(customBase.customCtrl.draw3D);
					objBackFrame.SetActiveIfDifferent(!customBase.customCtrl.draw3D);
					customBase.forceBackFrameHide = customBase.customCtrl.draw3D;
					if (!customBase.customCtrl.draw3D)
					{
						customBase.customCtrl.showColorCvs = false;
					}
				});
			});
		}
		if (btnBGIndex.Any())
		{
			(from item in btnBGIndex.Select((Button val, int idx) => new { val, idx })
				where item != null
				select item).ToList().ForEach(item =>
			{
				item.val.OnClickAsObservable().Subscribe(delegate
				{
					customBase.customCtrl.ChangeBGImage(item.idx);
				});
			});
		}
		if ((bool)csBG)
		{
			csBG.actUpdateColor = delegate(Color color)
			{
				customBase.customCtrl.ChangeBGColor(color);
			};
		}
		if ((bool)btnCancel)
		{
			btnCancel.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				customBase.customCtrl.overwriteSavePath = "";
				EndCapture();
				customBase.customCtrl.saveMode = false;
				customBase.customCtrl.updatePng = false;
			});
		}
		if (!btnSave)
		{
			return;
		}
		btnSave.OnClickAsObservable().Subscribe(delegate
		{
			if (customBase.customCtrl.saveMode)
			{
				Utils.Sound.Play(SystemSE.save);
				byte[] pngData = customBase.customCtrl.customCap.CapCharaCard(enableBG: true, customBase.saveFrameAssist, customBase.customCtrl.draw3D);
				chaCtrl.chaFile.pngData = pngData;
				string text = "";
				if (customBase.customCtrl.overwriteSavePath.IsNullOrEmpty())
				{
					text = ((chaCtrl.sex != 0) ? ("HS2ChaF_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")) : ("HS2ChaM_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")));
				}
				else
				{
					text = customBase.customCtrl.overwriteSavePath;
					customBase.customCtrl.overwriteSavePath = "";
				}
				chaCtrl.chaFile.InitGameInfoParam();
				chaCtrl.chaFile.SaveCharaFile(text);
				customBase.updateCvsCharaSaveDelete = true;
				customBase.updateCvsCharaLoad = true;
			}
			else
			{
				Utils.Sound.Play(SystemSE.photo);
				byte[] pngData2 = customBase.customCtrl.customCap.CapCharaCard(enableBG: true, customBase.saveFrameAssist, customBase.customCtrl.draw3D);
				chaCtrl.chaFile.pngData = pngData2;
			}
			EndCapture();
			customBase.customCtrl.saveMode = false;
			customBase.customCtrl.updatePng = false;
		});
	}
}
