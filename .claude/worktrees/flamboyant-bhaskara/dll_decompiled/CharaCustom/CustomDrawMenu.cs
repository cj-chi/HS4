using System.Collections;
using System.Linq;
using Illusion.Extensions;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomDrawMenu : CvsBase
{
	[SerializeField]
	private Button btnClose;

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
	private Button[] btnMouthPtn;

	[SerializeField]
	private InputField inpMouthNo;

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

	private bool backAutoClothesState;

	private int backClothesNo;

	private int backAutoClothesStateNo;

	private bool backAcsDraw = true;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
		base.customBase.customCtrl.showDrawMenu = true;
		UpdateUI();
	}

	public void UpdateUI()
	{
		int num = base.customBase.clothesStateNo + 1;
		if (base.customBase.autoClothesState)
		{
			num = 0;
		}
		tglClothesState[num].SetIsOnWithoutCallback(isOn: true);
		for (int i = 0; i < tglClothesState.Length; i++)
		{
			if (i != num)
			{
				tglClothesState[i].SetIsOnWithoutCallback(isOn: false);
			}
		}
		tglAcsState[0].SetIsOnWithoutCallback(base.customBase.accessoryDraw);
		tglAcsState[1].SetIsOnWithoutCallback(!base.customBase.accessoryDraw);
		tglEyeLook[0].SetIsOnWithoutCallback(base.customBase.eyelook == 0);
		tglEyeLook[1].SetIsOnWithoutCallback(1 == base.customBase.eyelook);
		tglNeckLook[0].SetIsOnWithoutCallback(base.customBase.necklook == 0);
		tglNeckLook[1].SetIsOnWithoutCallback(1 == base.customBase.necklook);
		inpPoseNo.text = base.customBase.poseNo.ToString();
		inpEyebrowNo.text = (base.customBase.eyebrowPtn + 1).ToString();
		inpEyeNo.text = (base.customBase.eyePtn + 1).ToString();
		inpMouthNo.text = (base.customBase.mouthPtn + 1).ToString();
		tglPlayAnime.SetIsOnWithoutCallback(base.customBase.playPoseAnime);
		Vector3 localEulerAngles = base.customBase.lightCustom.transform.localEulerAngles;
		sldLightRotX.value = ((89f < localEulerAngles.x) ? (localEulerAngles.x - 360f) : localEulerAngles.x);
		sldLightRotY.value = ((180f <= localEulerAngles.y) ? (localEulerAngles.y - 360f) : localEulerAngles.y);
		csLight.SetColor(base.customBase.lightCustom.color);
		sldLightPower.value = base.customBase.lightCustom.intensity;
		tglBG[0].SetIsOnWithoutCallback(base.customBase.customCtrl.draw3D);
		tglBG[1].SetIsOnWithoutCallback(!base.customBase.customCtrl.draw3D);
		csBG.SetColor(base.customBase.customCtrl.GetBGColor());
		objBGIndex.SetActiveIfDifferent(!base.customBase.customCtrl.draw3D);
		objBGColor.SetActiveIfDifferent(base.customBase.customCtrl.draw3D);
	}

	public void ChangeClothesStateForCapture(bool capture)
	{
		if (capture)
		{
			backAutoClothesState = base.customBase.autoClothesState;
			backClothesNo = base.customBase.clothesStateNo;
			backAutoClothesStateNo = base.customBase.autoClothesStateNo;
			base.customBase.clothesStateNo = 0;
			base.customBase.ChangeClothesState(-1);
			tglClothesState[1].SetIsOnWithoutCallback(isOn: true);
			for (int i = 0; i < tglClothesState.Length; i++)
			{
				if (i != 1)
				{
					tglClothesState[i].SetIsOnWithoutCallback(isOn: false);
				}
			}
			backAcsDraw = base.customBase.accessoryDraw;
			base.customBase.accessoryDraw = true;
			return;
		}
		base.customBase.autoClothesState = backAutoClothesState;
		base.customBase.clothesStateNo = backClothesNo;
		base.customBase.autoClothesStateNo = backAutoClothesStateNo;
		base.customBase.ChangeClothesState(-1);
		int num = base.customBase.clothesStateNo + 1;
		if (base.customBase.autoClothesState)
		{
			num = 0;
		}
		tglClothesState[num].SetIsOnWithoutCallback(isOn: true);
		for (int j = 0; j < tglClothesState.Length; j++)
		{
			if (j != num)
			{
				tglClothesState[j].SetIsOnWithoutCallback(isOn: false);
			}
		}
		base.customBase.accessoryDraw = backAcsDraw;
	}

	protected override IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		base.customBase.lstInputField.Add(inpPoseNo);
		base.customBase.lstInputField.Add(inpEyebrowNo);
		base.customBase.lstInputField.Add(inpEyeNo);
		base.customBase.lstInputField.Add(inpMouthNo);
		if (tglClothesState.Any())
		{
			(from item in tglClothesState.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.val.onValueChanged.AsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					base.customBase.ChangeClothesState(item.idx);
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
					base.customBase.accessoryDraw = item.idx == 0;
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
					base.customBase.eyelook = item.idx;
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
					base.customBase.necklook = item.idx;
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
						base.customBase.poseNo = 1;
					}
					else
					{
						base.customBase.ChangeAnimationNext(item.idx);
					}
					inpPoseNo.text = base.customBase.poseNo.ToString();
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
				base.customBase.ChangeAnimationNo(result);
				inpPoseNo.text = base.customBase.poseNo.ToString();
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
						base.customBase.ChangeEyebrowPtnNext(-1);
					}
					else
					{
						base.customBase.ChangeEyebrowPtnNext(item.idx);
					}
					inpEyebrowNo.text = (base.customBase.eyebrowPtn + 1).ToString();
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
				base.customBase.ChangeEyebrowPtnNo(result);
				inpEyebrowNo.text = (base.customBase.eyebrowPtn + 1).ToString();
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
						base.customBase.ChangeEyePtnNext(-1);
					}
					else
					{
						base.customBase.ChangeEyePtnNext(item.idx);
					}
					inpEyeNo.text = (base.customBase.eyePtn + 1).ToString();
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
				base.customBase.ChangeEyePtnNo(result);
				inpEyeNo.text = (base.customBase.eyePtn + 1).ToString();
			});
		}
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
						base.customBase.ChangeMouthPtnNext(-1);
					}
					else
					{
						base.customBase.ChangeMouthPtnNext(item.idx);
					}
					inpMouthNo.text = (base.customBase.mouthPtn + 1).ToString();
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
				base.customBase.ChangeMouthPtnNo(result);
				inpMouthNo.text = (base.customBase.mouthPtn + 1).ToString();
			});
		}
		if ((bool)tglPlayAnime)
		{
			tglPlayAnime.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				base.customBase.playPoseAnime = isOn;
			});
		}
		Vector3 veclight = base.customBase.lightCustom.transform.localEulerAngles;
		if ((bool)sldLightRotX)
		{
			sldLightRotX.value = ((89f < veclight.x) ? (veclight.x - 360f) : veclight.x);
			sldLightRotX.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				base.customBase.lightCustom.transform.localEulerAngles = new Vector3(val, base.customBase.lightCustom.transform.localEulerAngles.y, base.customBase.lightCustom.transform.localEulerAngles.z);
			});
		}
		sldLightRotX.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (base.customBase.sliderControlWheel)
			{
				sldLightRotX.value = Mathf.Clamp(sldLightRotX.value + scl.scrollDelta.y, -88f, 88f);
			}
		});
		if ((bool)sldLightRotY)
		{
			sldLightRotY.value = ((180f <= veclight.y) ? (veclight.y - 360f) : veclight.y);
			sldLightRotY.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				base.customBase.lightCustom.transform.localEulerAngles = new Vector3(base.customBase.lightCustom.transform.localEulerAngles.x, val, base.customBase.lightCustom.transform.localEulerAngles.z);
			});
		}
		sldLightRotY.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (base.customBase.sliderControlWheel)
			{
				sldLightRotY.value = Mathf.Clamp(sldLightRotY.value + scl.scrollDelta.y, -178f, 178f);
			}
		});
		if ((bool)csLight)
		{
			csLight.actUpdateColor = delegate(Color color)
			{
				base.customBase.lightCustom.color = color;
			};
		}
		if ((bool)sldLightPower)
		{
			sldLightPower.OnValueChangedAsObservable().Subscribe(delegate(float val)
			{
				base.customBase.lightCustom.intensity = val;
			});
		}
		sldLightPower.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (base.customBase.sliderControlWheel)
			{
				sldLightPower.value = Mathf.Clamp(sldLightPower.value + scl.scrollDelta.y * -0.01f, 0f, 100f);
			}
		});
		if ((bool)btnLightReset)
		{
			btnLightReset.OnClickAsObservable().Subscribe(delegate
			{
				base.customBase.ResetLightSetting();
				veclight = base.customBase.lightCustom.transform.localEulerAngles;
				sldLightRotX.value = ((89f < veclight.x) ? (veclight.x - 360f) : veclight.x);
				sldLightRotY.value = ((180f <= veclight.y) ? (veclight.y - 360f) : veclight.y);
				csLight.SetColor(base.customBase.lightCustom.color);
				sldLightPower.value = base.customBase.lightCustom.intensity;
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
					base.customBase.customCtrl.draw3D = item.idx == 0;
					objBGIndex.SetActiveIfDifferent(!base.customBase.customCtrl.draw3D);
					objBGColor.SetActiveIfDifferent(base.customBase.customCtrl.draw3D);
					base.customBase.forceBackFrameHide = base.customBase.customCtrl.draw3D;
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
					base.customBase.customCtrl.ChangeBGImage(item.idx);
				});
			});
		}
		if ((bool)csBG)
		{
			csBG.actUpdateColor = delegate(Color color)
			{
				base.customBase.customCtrl.ChangeBGColor(color);
			};
		}
		if ((bool)btnClose)
		{
			btnClose.OnClickAsObservable().Subscribe(delegate
			{
				base.customBase.customCtrl.showDrawMenu = false;
			});
		}
		UpdateUI();
		yield return null;
	}
}
