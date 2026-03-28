using System.Collections.Generic;
using AIChara;
using Illusion.Extensions;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomHairBundleSet : MonoBehaviour
{
	[SerializeField]
	private CvsH_Hair cvsH_Hair;

	[SerializeField]
	private Text title;

	[SerializeField]
	private CustomSliderSet[] ssMove;

	[SerializeField]
	private CustomSliderSet[] ssRot;

	[SerializeField]
	private Toggle tglNoShake;

	[SerializeField]
	private Toggle tglGuidDraw;

	[SerializeField]
	private GameObject tmpObjGuid;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	private ChaControl chaCtrl => customBase.chaCtrl;

	private ChaFileHair hair => chaCtrl.fileHair;

	private CustomBase.CustomSettingSave.HairCtrlSetting hairCtrlSetting => customBase.customSettingSave.hairCtrlSetting;

	private bool ctrlTogether => hair.ctrlTogether;

	public int parts { get; set; } = -1;

	public int idx { get; set; } = -1;

	public bool reset { get; set; }

	public CustomGuideObject cmpGuid { get; private set; }

	public bool isDrag { get; private set; }

	public void UpdateCustomUI()
	{
		if (-1 != parts && -1 != idx && hair.parts[parts].dictBundle.TryGetValue(idx, out var value))
		{
			ssMove[0].SetSliderValue(value.moveRate.x);
			ssMove[1].SetSliderValue(value.moveRate.y);
			ssMove[2].SetSliderValue(value.moveRate.z);
			ssRot[0].SetSliderValue(value.rotRate.x);
			ssRot[1].SetSliderValue(value.rotRate.y);
			ssRot[2].SetSliderValue(value.rotRate.z);
		}
	}

	public void SetControllerTransform()
	{
		Transform trfCorrect = chaCtrl.cmpHair[parts].boneInfo[idx].trfCorrect;
		if (!(null == trfCorrect) && !(null == cmpGuid))
		{
			cmpGuid.amount.position = trfCorrect.position;
			cmpGuid.amount.rotation = trfCorrect.eulerAngles;
		}
	}

	public void SetHairTransform(bool updateInfo, int ctrlAxisType)
	{
		Transform trfCorrect = chaCtrl.cmpHair[parts].boneInfo[idx].trfCorrect;
		if (null == trfCorrect || null == cmpGuid)
		{
			return;
		}
		int flag = 1;
		switch (ctrlAxisType)
		{
		case 0:
			flag = 1;
			break;
		case 1:
			flag = 2;
			break;
		case 2:
			flag = 4;
			break;
		case 3:
			flag = 7;
			break;
		}
		if (customBase.customSettingSave.hairCtrlSetting.controllerType == 0)
		{
			trfCorrect.position = cmpGuid.amount.position;
			if (updateInfo)
			{
				chaCtrl.SetHairCorrectPosValue(parts, idx, cmpGuid.amount.position, flag);
				chaCtrl.ChangeSettingHairCorrectPos(parts, idx);
			}
		}
		else
		{
			trfCorrect.eulerAngles = cmpGuid.amount.rotation;
			if (updateInfo)
			{
				chaCtrl.SetHairCorrectRotValue(parts, idx, cmpGuid.amount.rotation, flag);
				chaCtrl.ChangeSettingHairCorrectRot(parts, idx);
			}
		}
		UpdateCustomUI();
	}

	public void CreateGuid(GameObject objParent, CmpHair.BoneInfo binfo)
	{
		Transform parent = ((null == objParent) ? null : objParent.transform);
		GameObject gameObject = Object.Instantiate(tmpObjGuid);
		gameObject.SetActiveIfDifferent(active: true);
		gameObject.transform.SetParent(parent);
		cmpGuid = gameObject.GetComponent<CustomGuideObject>();
		cmpGuid.SetMode(hairCtrlSetting.controllerType);
		cmpGuid.speedMove = hairCtrlSetting.controllerSpeed;
		cmpGuid.scaleAxis = hairCtrlSetting.controllerScale;
		cmpGuid.UpdateScale();
		ObjectCategoryBehaviour component = gameObject.GetComponent<ObjectCategoryBehaviour>();
		CustomGuideLimit component2 = component.GetObject(0).GetComponent<CustomGuideLimit>();
		component2.limited = true;
		component2.trfParent = binfo.trfCorrect.parent;
		component2.limitMin = binfo.posMin;
		component2.limitMax = binfo.posMax;
		CustomGuideLimit component3 = component.GetObject(1).GetComponent<CustomGuideLimit>();
		component3.limited = true;
		component3.trfParent = binfo.trfCorrect.parent;
		component3.limitMin = binfo.rotMin;
		component3.limitMax = binfo.rotMax;
	}

	public void Initialize(int _parts, int _idx, int titleNo)
	{
		parts = _parts;
		idx = _idx;
		if (-1 == parts || -1 == idx)
		{
			return;
		}
		if (hair.parts[parts].dictBundle.TryGetValue(idx, out var bi))
		{
			ssMove[0].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, bi.moveRate.x));
			ssMove[1].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, bi.moveRate.y));
			ssMove[2].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, bi.moveRate.z));
			ssRot[0].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, bi.rotRate.x));
			ssRot[1].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, bi.rotRate.y));
			ssRot[2].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, bi.rotRate.z));
			chaCtrl.GetDefaultHairCorrectPosRate(parts, idx, out var vDefPosRate);
			chaCtrl.GetDefaultHairCorrectRotRate(parts, idx, out var vDefRotRate);
			ssMove[0].onChange = delegate(float value)
			{
				Vector3 moveRate = new Vector3(value, bi.moveRate.y, bi.moveRate.z);
				if (ctrlTogether && !reset && !cvsH_Hair.allReset)
				{
					foreach (KeyValuePair<int, ChaFileHair.PartsInfo.BundleInfo> item in hair.parts[parts].dictBundle)
					{
						item.Value.moveRate = moveRate;
					}
					chaCtrl.ChangeSettingHairCorrectPosAll(parts);
					cvsH_Hair.UpdateAllBundleUI(idx);
				}
				else
				{
					bi.moveRate = moveRate;
					chaCtrl.ChangeSettingHairCorrectPos(parts, idx);
				}
			};
			ssMove[0].onSetDefaultValue = delegate
			{
				reset = true;
				return vDefPosRate.x;
			};
			ssMove[0].onEndSetDefaultValue = delegate
			{
				reset = false;
			};
			ssMove[1].onChange = delegate(float value)
			{
				Vector3 moveRate = new Vector3(bi.moveRate.x, value, bi.moveRate.z);
				if (ctrlTogether && !reset && !cvsH_Hair.allReset)
				{
					foreach (KeyValuePair<int, ChaFileHair.PartsInfo.BundleInfo> item2 in hair.parts[parts].dictBundle)
					{
						item2.Value.moveRate = moveRate;
					}
					chaCtrl.ChangeSettingHairCorrectPosAll(parts);
					cvsH_Hair.UpdateAllBundleUI(idx);
				}
				else
				{
					bi.moveRate = moveRate;
					chaCtrl.ChangeSettingHairCorrectPos(parts, idx);
				}
			};
			ssMove[1].onSetDefaultValue = delegate
			{
				reset = true;
				return vDefPosRate.y;
			};
			ssMove[1].onEndSetDefaultValue = delegate
			{
				reset = false;
			};
			ssMove[2].onChange = delegate(float value)
			{
				Vector3 moveRate = new Vector3(bi.moveRate.x, bi.moveRate.y, value);
				if (ctrlTogether && !reset && !cvsH_Hair.allReset)
				{
					foreach (KeyValuePair<int, ChaFileHair.PartsInfo.BundleInfo> item3 in hair.parts[parts].dictBundle)
					{
						item3.Value.moveRate = moveRate;
					}
					chaCtrl.ChangeSettingHairCorrectPosAll(parts);
					cvsH_Hair.UpdateAllBundleUI(idx);
				}
				else
				{
					bi.moveRate = moveRate;
					chaCtrl.ChangeSettingHairCorrectPos(parts, idx);
				}
			};
			ssMove[2].onSetDefaultValue = delegate
			{
				reset = true;
				return vDefPosRate.z;
			};
			ssMove[2].onEndSetDefaultValue = delegate
			{
				reset = false;
			};
			ssRot[0].onChange = delegate(float value)
			{
				Vector3 rotRate = new Vector3(value, bi.rotRate.y, bi.rotRate.z);
				if (ctrlTogether && !reset && !cvsH_Hair.allReset)
				{
					foreach (KeyValuePair<int, ChaFileHair.PartsInfo.BundleInfo> item4 in hair.parts[parts].dictBundle)
					{
						item4.Value.rotRate = rotRate;
					}
					chaCtrl.ChangeSettingHairCorrectRotAll(parts);
					cvsH_Hair.UpdateAllBundleUI(idx);
				}
				else
				{
					bi.rotRate = rotRate;
					chaCtrl.ChangeSettingHairCorrectRot(parts, idx);
				}
			};
			ssRot[0].onSetDefaultValue = delegate
			{
				reset = true;
				return vDefRotRate.x;
			};
			ssRot[0].onEndSetDefaultValue = delegate
			{
				reset = false;
			};
			ssRot[1].onChange = delegate(float value)
			{
				Vector3 rotRate = new Vector3(bi.rotRate.x, value, bi.rotRate.z);
				if (ctrlTogether && !reset && !cvsH_Hair.allReset)
				{
					foreach (KeyValuePair<int, ChaFileHair.PartsInfo.BundleInfo> item5 in hair.parts[parts].dictBundle)
					{
						item5.Value.rotRate = rotRate;
					}
					chaCtrl.ChangeSettingHairCorrectRotAll(parts);
					cvsH_Hair.UpdateAllBundleUI(idx);
				}
				else
				{
					bi.rotRate = rotRate;
					chaCtrl.ChangeSettingHairCorrectRot(parts, idx);
				}
			};
			ssRot[1].onSetDefaultValue = delegate
			{
				reset = true;
				return vDefRotRate.y;
			};
			ssRot[1].onEndSetDefaultValue = delegate
			{
				reset = false;
			};
			ssRot[2].onChange = delegate(float value)
			{
				Vector3 rotRate = new Vector3(bi.rotRate.x, bi.rotRate.y, value);
				if (ctrlTogether && !reset && !cvsH_Hair.allReset)
				{
					foreach (KeyValuePair<int, ChaFileHair.PartsInfo.BundleInfo> item6 in hair.parts[parts].dictBundle)
					{
						item6.Value.rotRate = rotRate;
					}
					chaCtrl.ChangeSettingHairCorrectRotAll(parts);
					cvsH_Hair.UpdateAllBundleUI(idx);
				}
				else
				{
					bi.rotRate = rotRate;
					chaCtrl.ChangeSettingHairCorrectRot(parts, idx);
				}
			};
			ssRot[2].onSetDefaultValue = delegate
			{
				reset = true;
				return vDefRotRate.z;
			};
			ssRot[2].onEndSetDefaultValue = delegate
			{
				reset = false;
			};
			if ((bool)tglNoShake)
			{
				tglNoShake.SetIsOnWithoutCallback(bi.noShake);
				tglNoShake.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					if (bi.noShake != isOn)
					{
						bi.noShake = isOn;
					}
				});
			}
			if ((bool)tglGuidDraw)
			{
				tglGuidDraw.SetIsOnWithoutCallback(isOn: true);
				tglGuidDraw.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					if (null != cmpGuid)
					{
						cmpGuid.gameObject.SetActiveIfDifferent(isOn);
					}
				});
			}
		}
		if ((bool)title)
		{
			title.text = CharaCustomDefine.CustomCorrectTitle[Singleton<GameSystem>.Instance.languageInt] + titleNo.ToString("00");
		}
		UpdateCustomUI();
	}

	private void LateUpdate()
	{
		if (null != cmpGuid && cmpGuid.gameObject.activeInHierarchy)
		{
			if (cmpGuid.isDrag)
			{
				SetHairTransform(updateInfo: false, cmpGuid.ctrlAxisType);
			}
			else if (isDrag)
			{
				SetHairTransform(updateInfo: true, cmpGuid.ctrlAxisType);
			}
			else
			{
				SetControllerTransform();
			}
			isDrag = cmpGuid.isDrag;
			customBase.cursorDraw = !cmpGuid.isDrag;
		}
	}
}
