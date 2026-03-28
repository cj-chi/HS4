using System;
using System.Linq;
using AIChara;
using CharaCustom;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HS2;

public class SvsCaptureMenu : MonoBehaviour
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
	private SearchColorSet csLight;

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
	private SearchColorSet csBG;

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

	protected SearchBase searchBase => Singleton<SearchBase>.Instance;

	protected ChaListControl lstCtrl => Singleton<Character>.Instance.chaListCtrl;

	protected ChaControl chaCtrl => searchBase.chaCtrl;

	public void BeginCapture()
	{
		backAutoClothesState = searchBase.autoClothesState;
		backClothesNo = searchBase.clothesStateNo;
		backAutoClothesStateNo = searchBase.autoClothesStateNo;
		int clothesStateNo = searchBase.clothesStateNo;
		tglClothesState[clothesStateNo].SetIsOnWithoutCallback(isOn: true);
		for (int i = 0; i < tglClothesState.Length; i++)
		{
			if (i != clothesStateNo)
			{
				tglClothesState[i].SetIsOnWithoutCallback(isOn: false);
			}
		}
		tglAcsState[0].SetIsOnWithoutCallback(searchBase.accessoryDraw);
		tglAcsState[1].SetIsOnWithoutCallback(!searchBase.accessoryDraw);
		tglEyeLook[0].SetIsOnWithoutCallback(searchBase.eyelook == 0);
		tglEyeLook[1].SetIsOnWithoutCallback(1 == searchBase.eyelook);
		tglNeckLook[0].SetIsOnWithoutCallback(searchBase.necklook == 0);
		tglNeckLook[1].SetIsOnWithoutCallback(1 == searchBase.necklook);
		chaCtrl.ChangeEyesBlinkFlag(blink: false);
		chaCtrl.ChangeMouthFixed(fix: true);
		chaCtrl.ChangeMouthOpenMax(0f);
		sldEyeOpen.value = 1f;
		sldMouthOpen.value = 0f;
		chaCtrl.SetEnableShapeHand(0, enable: false);
		chaCtrl.SetShapeHandIndex(0, 0);
		chaCtrl.SetEnableShapeHand(1, enable: false);
		chaCtrl.SetShapeHandIndex(1, 0);
		inpPoseNo.text = searchBase.poseNo.ToString();
		inpEyebrowNo.text = (searchBase.eyebrowPtn + 1).ToString();
		inpEyeNo.text = (searchBase.eyePtn + 1).ToString();
		inpMouthNo.text = (searchBase.mouthPtn + 1).ToString();
		inpHandLNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
		inpHandRNo.text = CharaCustomDefine.CustomHandBaseMsg[Singleton<GameSystem>.Instance.languageInt];
		tglPlayAnime.SetIsOnWithoutCallback(searchBase.playPoseAnime);
		Vector3 localEulerAngles = searchBase.lightCustom.transform.localEulerAngles;
		sldLightRotX.value = ((89f < localEulerAngles.x) ? (localEulerAngles.x - 360f) : localEulerAngles.x);
		sldLightRotY.value = ((180f <= localEulerAngles.y) ? (localEulerAngles.y - 360f) : localEulerAngles.y);
		csLight.SetColor(searchBase.lightCustom.color);
		sldLightPower.value = searchBase.lightCustom.intensity;
		searchBase.drawSaveFrameTop = true;
		tglBFrameDraw.isOn = searchBase.drawSaveFrameBack;
		tglFFrameDraw.isOn = searchBase.drawSaveFrameFront;
		tglBG[0].SetIsOnWithoutCallback(searchBase.searchCtrl.draw3D);
		tglBG[1].SetIsOnWithoutCallback(!searchBase.searchCtrl.draw3D);
		csBG.SetColor(searchBase.searchCtrl.GetBGColor());
		objBGIndex.SetActiveIfDifferent(!searchBase.searchCtrl.draw3D);
		objBGColor.SetActiveIfDifferent(searchBase.searchCtrl.draw3D);
		objBackFrame.SetActiveIfDifferent(!searchBase.searchCtrl.draw3D);
		if (searchBase.searchCtrl.saveMode)
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
		searchBase.autoClothesState = backAutoClothesState;
		searchBase.clothesStateNo = backClothesNo;
		searchBase.autoClothesStateNo = backAutoClothesStateNo;
		searchBase.ChangeClothesState(-1);
		chaCtrl.ChangeEyesBlinkFlag(blink: true);
		chaCtrl.ChangeMouthFixed(fix: false);
		chaCtrl.ChangeEyesOpenMax(1f);
		chaCtrl.ChangeMouthOpenMax(1f);
		chaCtrl.SetEnableShapeHand(0, enable: false);
		chaCtrl.SetEnableShapeHand(1, enable: false);
		searchBase.drawSaveFrameTop = false;
		searchBase.drawMenu.UpdateUI();
	}

	protected virtual void Start()
	{
		searchBase.lstInputField.Add(inpPoseNo);
		searchBase.lstInputField.Add(inpEyebrowNo);
		searchBase.lstInputField.Add(inpEyeNo);
		searchBase.lstInputField.Add(inpMouthNo);
		searchBase.lstInputField.Add(inpHandLNo);
		searchBase.lstInputField.Add(inpHandRNo);
		searchBase.drawSaveFrameBack = true;
		searchBase.drawSaveFrameFront = true;
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
					searchBase.ChangeClothesState(item.idx + 1);
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
					searchBase.accessoryDraw = item.idx == 0;
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
					searchBase.eyelook = item.idx;
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
					searchBase.necklook = item.idx;
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
						searchBase.poseNo = 1;
					}
					else
					{
						searchBase.ChangeAnimationNext(item.idx);
					}
					inpPoseNo.text = searchBase.poseNo.ToString();
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
				searchBase.ChangeAnimationNo(result);
				inpPoseNo.text = searchBase.poseNo.ToString();
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
						searchBase.ChangeEyebrowPtnNext(-1);
					}
					else
					{
						searchBase.ChangeEyebrowPtnNext(item.idx);
					}
					inpEyebrowNo.text = (searchBase.eyebrowPtn + 1).ToString();
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
				searchBase.ChangeEyebrowPtnNo(result);
				inpEyebrowNo.text = (searchBase.eyebrowPtn + 1).ToString();
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
						searchBase.ChangeEyePtnNext(-1);
					}
					else
					{
						searchBase.ChangeEyePtnNext(item.idx);
					}
					inpEyeNo.text = (searchBase.eyePtn + 1).ToString();
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
				searchBase.ChangeEyePtnNo(result);
				inpEyeNo.text = (searchBase.eyePtn + 1).ToString();
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
			if (searchBase.sliderControlWheel)
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
						searchBase.ChangeMouthPtnNext(-1);
					}
					else
					{
						searchBase.ChangeMouthPtnNext(item.idx);
					}
					inpMouthNo.text = (searchBase.mouthPtn + 1).ToString();
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
				searchBase.ChangeMouthPtnNo(result);
				inpMouthNo.text = (searchBase.mouthPtn + 1).ToString();
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
			if (searchBase.sliderControlWheel)
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
				searchBase.playPoseAnime = isOn;
			});
		}
		if ((bool)sldLightRotX)
		{
			sldLightRotX.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				searchBase.lightCustom.transform.localEulerAngles = new Vector3(val, searchBase.lightCustom.transform.localEulerAngles.y, searchBase.lightCustom.transform.localEulerAngles.z);
			});
		}
		sldLightRotX.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (searchBase.sliderControlWheel)
			{
				sldLightRotX.value = Mathf.Clamp(sldLightRotX.value + scl.scrollDelta.y * -0.01f, 0f, 100f);
			}
		});
		if ((bool)sldLightRotY)
		{
			sldLightRotY.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				searchBase.lightCustom.transform.localEulerAngles = new Vector3(searchBase.lightCustom.transform.localEulerAngles.x, val, searchBase.lightCustom.transform.localEulerAngles.z);
			});
		}
		sldLightRotY.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (searchBase.sliderControlWheel)
			{
				sldLightRotY.value = Mathf.Clamp(sldLightRotY.value + scl.scrollDelta.y, -88f, 88f);
			}
		});
		if ((bool)csLight)
		{
			csLight.actUpdateColor = delegate(Color color)
			{
				searchBase.lightCustom.color = color;
			};
		}
		if ((bool)sldLightPower)
		{
			sldLightPower.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				searchBase.lightCustom.intensity = val;
			});
		}
		sldLightPower.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (searchBase.sliderControlWheel)
			{
				sldLightPower.value = Mathf.Clamp(sldLightPower.value + scl.scrollDelta.y, -178f, 178f);
			}
		});
		if ((bool)btnLightReset)
		{
			btnLightReset.OnClickAsObservable().Subscribe(delegate
			{
				searchBase.ResetLightSetting();
				Vector3 localEulerAngles = searchBase.lightCustom.transform.localEulerAngles;
				sldLightRotX.value = ((89f < localEulerAngles.x) ? (localEulerAngles.x - 360f) : localEulerAngles.x);
				sldLightRotY.value = ((180f <= localEulerAngles.y) ? (localEulerAngles.y - 360f) : localEulerAngles.y);
				csLight.SetColor(searchBase.lightCustom.color);
				sldLightPower.value = searchBase.lightCustom.intensity;
			});
		}
		if ((bool)tglBFrameDraw)
		{
			tglBFrameDraw.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				searchBase.drawSaveFrameBack = isOn;
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
					searchBase.saveFrameAssist.ChangeSaveFrameBack((byte)item.idx);
				});
			});
		}
		if ((bool)tglFFrameDraw)
		{
			tglFFrameDraw.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				searchBase.drawSaveFrameFront = isOn;
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
					searchBase.saveFrameAssist.ChangeSaveFrameFront((byte)item.idx);
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
					searchBase.searchCtrl.draw3D = item.idx == 0;
					objBGIndex.SetActiveIfDifferent(!searchBase.searchCtrl.draw3D);
					objBGColor.SetActiveIfDifferent(searchBase.searchCtrl.draw3D);
					objBackFrame.SetActiveIfDifferent(!searchBase.searchCtrl.draw3D);
					searchBase.forceBackFrameHide = searchBase.searchCtrl.draw3D;
					if (!searchBase.searchCtrl.draw3D)
					{
						searchBase.searchCtrl.showColorCvs = false;
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
					searchBase.searchCtrl.ChangeBGImage(item.idx);
				});
			});
		}
		if ((bool)csBG)
		{
			csBG.actUpdateColor = delegate(Color color)
			{
				searchBase.searchCtrl.ChangeBGColor(color);
			};
		}
		if ((bool)btnCancel)
		{
			btnCancel.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				searchBase.searchCtrl.overwriteSavePath = "";
				EndCapture();
				searchBase.searchCtrl.saveMode = false;
				searchBase.searchCtrl.updatePng = false;
			});
		}
		if (!btnSave)
		{
			return;
		}
		btnSave.OnClickAsObservable().Subscribe(delegate
		{
			if (searchBase.searchCtrl.saveMode)
			{
				Utils.Sound.Play(SystemSE.save);
				byte[] pngData = searchBase.searchCtrl.customCap.CapCharaCard(enableBG: true, searchBase.saveFrameAssist, searchBase.searchCtrl.draw3D);
				chaCtrl.chaFile.pngData = pngData;
				string text = "";
				if (searchBase.searchCtrl.overwriteSavePath.IsNullOrEmpty())
				{
					text = ((chaCtrl.sex != 0) ? ("HS2ChaF_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")) : ("HS2ChaM_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")));
				}
				else
				{
					text = searchBase.searchCtrl.overwriteSavePath;
					searchBase.searchCtrl.overwriteSavePath = "";
				}
				chaCtrl.chaFile.InitGameInfoParam();
				chaCtrl.chaFile.SaveCharaFile(text);
				searchBase.updateCvsCharaSaveDelete = true;
				searchBase.updateCvsCharaLoad = true;
			}
			else
			{
				Utils.Sound.Play(SystemSE.photo);
				byte[] pngData2 = searchBase.searchCtrl.customCap.CapCharaCard(enableBG: true, searchBase.saveFrameAssist, searchBase.searchCtrl.draw3D);
				chaCtrl.chaFile.pngData = pngData2;
			}
			EndCapture();
			searchBase.searchCtrl.saveMode = false;
			searchBase.searchCtrl.updatePng = false;
		});
	}
}
