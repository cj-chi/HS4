using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Manager;
using MessagePack;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsA_Slot : CvsBase
{
	[SerializeField]
	private CustomChangeMainMenu mainMenu;

	[Header("【設定01】----------------------")]
	[SerializeField]
	private Toggle[] tglType;

	[SerializeField]
	private GameObject objAcsSelect;

	[SerializeField]
	private CustomSelectScrollController sscAcs;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private GameObject[] objColorGrp;

	[SerializeField]
	private Text[] textColorTitle;

	[SerializeField]
	private CustomColorSet[] csColor;

	[SerializeField]
	private CustomSliderSet[] ssGloss;

	[SerializeField]
	private CustomSliderSet[] ssMetallic;

	[SerializeField]
	private Button btnDefaultColor;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomColorSet csHairBaseColor;

	[SerializeField]
	private CustomColorSet csHairTopColor;

	[SerializeField]
	private CustomColorSet csHairUnderColor;

	[SerializeField]
	private CustomColorSet csHairSpecular;

	[SerializeField]
	private CustomSliderSet ssHairMetallic;

	[SerializeField]
	private CustomSliderSet ssHairSmoothness;

	[SerializeField]
	private Button[] btnGetHairColor;

	[Header("【設定04】----------------------")]
	[SerializeField]
	private Toggle[] tglParent;

	[SerializeField]
	private Button btnDefaultParent;

	[Header("【設定05】----------------------")]
	[SerializeField]
	private Toggle tglNoShake;

	[SerializeField]
	private CustomAcsCorrectSet[] acCorrect;

	private int backSNo = -1;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	public void UpdateAcsList(int ForceNo = -1)
	{
		ChaListDefine.CategoryNo[] obj = new ChaListDefine.CategoryNo[14]
		{
			ChaListDefine.CategoryNo.ao_none,
			ChaListDefine.CategoryNo.ao_head,
			ChaListDefine.CategoryNo.ao_ear,
			ChaListDefine.CategoryNo.ao_glasses,
			ChaListDefine.CategoryNo.ao_face,
			ChaListDefine.CategoryNo.ao_neck,
			ChaListDefine.CategoryNo.ao_shoulder,
			ChaListDefine.CategoryNo.ao_chest,
			ChaListDefine.CategoryNo.ao_waist,
			ChaListDefine.CategoryNo.ao_back,
			ChaListDefine.CategoryNo.ao_arm,
			ChaListDefine.CategoryNo.ao_hand,
			ChaListDefine.CategoryNo.ao_leg,
			ChaListDefine.CategoryNo.ao_kokan
		};
		int num = ((-1 == ForceNo) ? (base.nowAcs.parts[base.SNo].type - 350) : ForceNo);
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(obj[num]);
		sscAcs.CreateList(lst);
	}

	public void RestrictAcsMenu()
	{
		if (base.nowAcs.parts[base.SNo].type == 350)
		{
			if ((bool)objAcsSelect)
			{
				objAcsSelect.SetActiveIfDifferent(active: false);
			}
			ShowOrHideTab(false, 1, 2, 3, 4);
			return;
		}
		CmpAccessory cmpAccessory = base.chaCtrl.cmpAccessory[base.SNo];
		if (null == cmpAccessory)
		{
			return;
		}
		if (null != objAcsSelect)
		{
			objAcsSelect.SetActiveIfDifferent(active: true);
		}
		ShowOrHideTab(true, 1, 2, 3, 4);
		if (cmpAccessory.typeHair)
		{
			ShowOrHideTab(false, 1);
		}
		else
		{
			if (!cmpAccessory.useColor01 && !cmpAccessory.useColor02 && !cmpAccessory.useColor03 && (cmpAccessory.rendAlpha == null || cmpAccessory.rendAlpha.Length == 0))
			{
				ShowOrHideTab(false, 1);
			}
			ShowOrHideTab(false, 2);
		}
		if (!cmpAccessory.typeHair)
		{
			int num = 1;
			if (null != objColorGrp[0])
			{
				objColorGrp[0].SetActiveIfDifferent(cmpAccessory.useColor01);
			}
			if (null != objColorGrp[1])
			{
				objColorGrp[1].SetActiveIfDifferent(cmpAccessory.useColor02);
			}
			if (null != objColorGrp[2])
			{
				objColorGrp[2].SetActiveIfDifferent(cmpAccessory.useColor03);
			}
			if (null != objColorGrp[3])
			{
				objColorGrp[3].SetActiveIfDifferent(cmpAccessory.rendAlpha != null && cmpAccessory.rendAlpha.Length != 0);
			}
			if (null != textColorTitle[0] && cmpAccessory.useColor01)
			{
				textColorTitle[0].text = $"{CharaCustomDefine.CustomColorTitle[Singleton<GameSystem>.Instance.languageInt]}{num++}";
			}
			if (null != textColorTitle[1] && cmpAccessory.useColor02)
			{
				textColorTitle[1].text = $"{CharaCustomDefine.CustomColorTitle[Singleton<GameSystem>.Instance.languageInt]}{num++}";
			}
			if (null != textColorTitle[2] && cmpAccessory.useColor03)
			{
				textColorTitle[2].text = $"{CharaCustomDefine.CustomColorTitle[Singleton<GameSystem>.Instance.languageInt]}{num++}";
			}
			if (null != textColorTitle[3] && cmpAccessory.rendAlpha != null && cmpAccessory.rendAlpha.Length != 0)
			{
				textColorTitle[3].text = $"{CharaCustomDefine.CustomColorTitle[Singleton<GameSystem>.Instance.languageInt]}{num++}";
			}
		}
		else
		{
			if (null != csHairTopColor)
			{
				csHairTopColor.gameObject.SetActiveIfDifferent(cmpAccessory.useColor02);
			}
			if (null != csHairUnderColor)
			{
				csHairUnderColor.gameObject.SetActiveIfDifferent(cmpAccessory.useColor03);
			}
		}
	}

	public void SetDefaultColor()
	{
		CmpAccessory cmpAccessory = base.chaCtrl.cmpAccessory[base.SNo];
		if (!(null == cmpAccessory) && !cmpAccessory.typeHair)
		{
			base.chaCtrl.SetAccessoryDefaultColor(base.SNo);
			for (int i = 0; i < 4; i++)
			{
				byte[] bytes = MessagePackSerializer.Serialize(base.nowAcs.parts[base.SNo].colorInfo[i]);
				base.orgAcs.parts[base.SNo].colorInfo[i] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo.ColorInfo>(bytes);
			}
		}
	}

	public void ChangeAcsType(int idx)
	{
		if (base.nowAcs.parts[base.SNo].type - 350 != idx)
		{
			base.nowAcs.parts[base.SNo].type = 350 + idx;
			base.orgAcs.parts[base.SNo].type = base.nowAcs.parts[base.SNo].type;
			base.nowAcs.parts[base.SNo].parentKey = "";
			for (int i = 0; i < 2; i++)
			{
				base.orgAcs.parts[base.SNo].addMove[i, 0] = (base.nowAcs.parts[base.SNo].addMove[i, 0] = Vector3.zero);
				base.orgAcs.parts[base.SNo].addMove[i, 1] = (base.nowAcs.parts[base.SNo].addMove[i, 1] = Vector3.zero);
				base.orgAcs.parts[base.SNo].addMove[i, 2] = (base.nowAcs.parts[base.SNo].addMove[i, 2] = Vector3.one);
			}
			base.chaCtrl.ChangeAccessory(base.SNo, base.nowAcs.parts[base.SNo].type, ChaAccessoryDefine.AccessoryDefaultIndex[idx], "", forceChange: true);
			SetDefaultColor();
			base.chaCtrl.ChangeAccessoryColor(base.SNo);
			base.orgAcs.parts[base.SNo].id = base.nowAcs.parts[base.SNo].id;
			base.orgAcs.parts[base.SNo].parentKey = base.nowAcs.parts[base.SNo].parentKey;
			base.nowAcs.parts[base.SNo].noShake = false;
			base.orgAcs.parts[base.SNo].noShake = false;
			base.customBase.ChangeAcsSlotName(base.SNo);
			UpdateAcsList();
			base.customBase.forceUpdateAcsList = true;
			UpdateCustomUI();
			base.customBase.showAcsControllerAll = base.customBase.chaCtrl.IsAccessory(base.SNo);
		}
	}

	public void ChangeAcsId(int id)
	{
		bool flag = false;
		if (base.chaCtrl.cmpAccessory != null && null != base.chaCtrl.cmpAccessory[base.SNo])
		{
			flag = base.chaCtrl.cmpAccessory[base.SNo].typeHair;
		}
		base.chaCtrl.ChangeAccessory(base.SNo, base.nowAcs.parts[base.SNo].type, id, "");
		SetDefaultColor();
		base.chaCtrl.ChangeAccessoryColor(base.SNo);
		bool flag2 = false;
		if (base.chaCtrl.cmpAccessory != null && null != base.chaCtrl.cmpAccessory[base.SNo])
		{
			flag2 = base.chaCtrl.cmpAccessory[base.SNo].typeHair;
		}
		if (!flag && flag2)
		{
			ChangeHairTypeAccessoryColor(0);
		}
		base.orgAcs.parts[base.SNo].id = base.nowAcs.parts[base.SNo].id;
		base.orgAcs.parts[base.SNo].parentKey = base.nowAcs.parts[base.SNo].parentKey;
		base.nowAcs.parts[base.SNo].noShake = false;
		base.orgAcs.parts[base.SNo].noShake = false;
		base.customBase.ChangeAcsSlotName(base.SNo);
		UpdateCustomUI();
	}

	public void ChangeAcsParent(int idx)
	{
		string text = (from key in Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey))
			where key != "none"
			select key).ToArray()[idx];
		if (base.nowAcs.parts[base.SNo].parentKey != text)
		{
			base.chaCtrl.ChangeAccessoryParent(base.SNo, text);
			base.orgAcs.parts[base.SNo].parentKey = base.nowAcs.parts[base.SNo].parentKey;
		}
	}

	private void CalculateUI()
	{
		CmpAccessory cmpAccessory = base.chaCtrl.cmpAccessory[base.SNo];
		if (null == cmpAccessory)
		{
			return;
		}
		if (!cmpAccessory.typeHair)
		{
			for (int i = 0; i < ssGloss.Length; i++)
			{
				ssGloss[i].SetSliderValue(base.nowAcs.parts[base.SNo].colorInfo[i].glossPower);
			}
			for (int j = 0; j < ssMetallic.Length; j++)
			{
				ssMetallic[j].SetSliderValue(base.nowAcs.parts[base.SNo].colorInfo[j].metallicPower);
			}
		}
		else
		{
			ssHairSmoothness.SetSliderValue(base.nowAcs.parts[base.SNo].colorInfo[0].smoothnessPower);
			ssHairMetallic.SetSliderValue(base.nowAcs.parts[base.SNo].colorInfo[0].metallicPower);
		}
	}

	public override void UpdateCustomUI()
	{
		if (backSNo != base.SNo)
		{
			UpdateAcsList();
			for (int i = 0; i < acCorrect.Length; i++)
			{
				acCorrect[i].Initialize(base.SNo, i);
			}
			backSNo = base.SNo;
		}
		else if (base.customBase.forceUpdateAcsList)
		{
			UpdateAcsList();
			base.customBase.forceUpdateAcsList = false;
		}
		base.customBase.showAcsControllerAll = base.customBase.chaCtrl.IsAccessory(base.SNo);
		if (!mainMenu.IsSelectAccessory())
		{
			base.customBase.showAcsControllerAll = false;
		}
		base.UpdateCustomUI();
		CalculateUI();
		int num = base.nowAcs.parts[base.SNo].type - 350;
		for (int j = 0; j < tglType.Length; j++)
		{
			tglType[j].SetIsOnWithoutCallback(num == j);
		}
		CmpAccessory cmpAccessory = base.chaCtrl.cmpAccessory[base.SNo];
		bool flag = false;
		if (null != cmpAccessory)
		{
			flag = cmpAccessory.typeHair;
		}
		if (flag)
		{
			csHairBaseColor.SetColor(base.nowAcs.parts[base.SNo].colorInfo[0].color);
			csHairTopColor.SetColor(base.nowAcs.parts[base.SNo].colorInfo[1].color);
			csHairUnderColor.SetColor(base.nowAcs.parts[base.SNo].colorInfo[2].color);
			csHairSpecular.SetColor(base.nowAcs.parts[base.SNo].colorInfo[3].color);
		}
		else
		{
			for (int k = 0; k < csColor.Length; k++)
			{
				csColor[k].SetColor(base.nowAcs.parts[base.SNo].colorInfo[k].color);
			}
		}
		sscAcs.SetToggleID(base.nowAcs.parts[base.SNo].id);
		int num2 = ChaAccessoryDefine.GetAccessoryParentInt(base.nowAcs.parts[base.SNo].parentKey) - 1;
		if (0 <= num2)
		{
			for (int l = 0; l < tglParent.Length; l++)
			{
				tglParent[l].SetIsOnWithoutCallback(num2 == l);
			}
		}
		tglNoShake.SetIsOnWithoutCallback(base.nowAcs.parts[base.SNo].noShake);
		bool[] array = new bool[2];
		if (null != base.chaCtrl.cmpAccessory[base.SNo])
		{
			if (null != base.chaCtrl.cmpAccessory[base.SNo].trfMove01)
			{
				array[0] = true;
			}
			if (null != base.chaCtrl.cmpAccessory[base.SNo].trfMove02)
			{
				array[1] = true;
			}
		}
		for (int m = 0; m < acCorrect.Length; m++)
		{
			acCorrect[m].gameObject.SetActiveIfDifferent(array[m]);
			acCorrect[m].UpdateCustomUI();
			base.customBase.showAcsController[m] = array[m];
		}
		RestrictAcsMenu();
		if (null != titleText)
		{
			ListInfoBase listInfo = base.chaCtrl.lstCtrl.GetListInfo((ChaListDefine.CategoryNo)base.nowAcs.parts[base.SNo].type, base.nowAcs.parts[base.SNo].id);
			titleText.text = $"{base.SNo + 1:00} {listInfo.Name}";
		}
	}

	public void ChangeHairTypeAccessoryColor(int hairPartsNo)
	{
		base.nowAcs.parts[base.SNo].colorInfo[0].color = base.hair.parts[hairPartsNo].baseColor;
		base.nowAcs.parts[base.SNo].colorInfo[1].color = base.hair.parts[hairPartsNo].topColor;
		base.nowAcs.parts[base.SNo].colorInfo[2].color = base.hair.parts[hairPartsNo].underColor;
		base.nowAcs.parts[base.SNo].colorInfo[3].color = base.hair.parts[hairPartsNo].specular;
		base.nowAcs.parts[base.SNo].colorInfo[0].smoothnessPower = base.hair.parts[hairPartsNo].smoothness;
		base.nowAcs.parts[base.SNo].colorInfo[0].metallicPower = base.hair.parts[hairPartsNo].metallic;
		base.chaCtrl.ChangeHairTypeAccessoryColor(base.SNo);
		for (int i = 0; i < 4; i++)
		{
			byte[] bytes = MessagePackSerializer.Serialize(base.nowAcs.parts[base.SNo].colorInfo[i]);
			base.orgAcs.parts[base.SNo].colorInfo[i] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo.ColorInfo>(bytes);
		}
		csHairBaseColor.SetColor(base.nowAcs.parts[base.SNo].colorInfo[0].color);
		csHairTopColor.SetColor(base.nowAcs.parts[base.SNo].colorInfo[1].color);
		csHairUnderColor.SetColor(base.nowAcs.parts[base.SNo].colorInfo[2].color);
		csHairSpecular.SetColor(base.nowAcs.parts[base.SNo].colorInfo[3].color);
		ssHairSmoothness.SetSliderValue(base.nowAcs.parts[base.SNo].colorInfo[0].smoothnessPower);
		ssHairMetallic.SetSliderValue(base.nowAcs.parts[base.SNo].colorInfo[0].metallicPower);
	}

	public void ShortcutChangeGuidType(int type)
	{
		bool flag = false;
		CustomAcsCorrectSet[] array = acCorrect;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsDrag())
			{
				flag = true;
			}
		}
		if (!flag)
		{
			array = acCorrect;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ShortcutChangeGuidType(type);
			}
		}
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		CmpAccessory cmpAccessory = base.chaCtrl.cmpAccessory[base.SNo];
		if (null == cmpAccessory)
		{
			yield break;
		}
		if (!cmpAccessory.typeHair)
		{
			for (int num = 0; num < ssGloss.Length; num++)
			{
				ssGloss[num].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.nowAcs.parts[base.SNo].colorInfo[num].glossPower));
			}
			for (int num2 = 0; num2 < ssMetallic.Length; num2++)
			{
				ssMetallic[num2].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.nowAcs.parts[base.SNo].colorInfo[num2].metallicPower));
			}
		}
		else
		{
			ssHairSmoothness.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.nowAcs.parts[base.SNo].colorInfo[0].smoothnessPower));
			ssHairMetallic.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.nowAcs.parts[base.SNo].colorInfo[0].metallicPower));
		}
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.ChangeAcsSlotName();
		base.customBase.actUpdateCvsAccessory += UpdateCustomUI;
		if (tglType.Any())
		{
			(from tgl in tglType.Select((Toggle val, int idx) => new { val, idx })
				where tgl.val != null
				select tgl).ToList().ForEach(tgl =>
			{
				(from isOn in tgl.val.onValueChanged.AsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					ChangeAcsType(tgl.idx);
				});
			});
		}
		UpdateAcsList();
		sscAcs.SetToggleID(base.nowAcs.parts[base.SNo].id);
		sscAcs.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.nowAcs.parts[base.SNo].id != info.id)
			{
				ChangeAcsId(info.id);
			}
		};
		(from item in csColor.Select((CustomColorSet val, int idx) => new { val, idx })
			where item.val != null
			select item).ToList().ForEach(item =>
		{
			item.val.actUpdateColor = delegate(Color color)
			{
				base.nowAcs.parts[base.SNo].colorInfo[item.idx].color = color;
				base.orgAcs.parts[base.SNo].colorInfo[item.idx].color = color;
				base.chaCtrl.ChangeAccessoryColor(base.SNo);
			};
		});
		(from item in ssGloss.Select((CustomSliderSet val, int idx) => new { val, idx })
			where item.val != null
			select item).ToList().ForEach(item =>
		{
			item.val.onChange = delegate(float value)
			{
				base.nowAcs.parts[base.SNo].colorInfo[item.idx].glossPower = value;
				base.orgAcs.parts[base.SNo].colorInfo[item.idx].glossPower = value;
				base.chaCtrl.ChangeAccessoryColor(base.SNo);
			};
			item.val.onSetDefaultValue = () => (null == base.chaCtrl.cmpAccessory[base.SNo]) ? 0f : (item.idx switch
			{
				0 => base.chaCtrl.cmpAccessory[base.SNo].defGlossPower01, 
				1 => base.chaCtrl.cmpAccessory[base.SNo].defGlossPower02, 
				2 => base.chaCtrl.cmpAccessory[base.SNo].defGlossPower03, 
				3 => base.chaCtrl.cmpAccessory[base.SNo].defGlossPower04, 
				_ => 0f, 
			});
		});
		(from item in ssMetallic.Select((CustomSliderSet val, int idx) => new { val, idx })
			where item.val != null
			select item).ToList().ForEach(item =>
		{
			item.val.onChange = delegate(float value)
			{
				base.nowAcs.parts[base.SNo].colorInfo[item.idx].metallicPower = value;
				base.orgAcs.parts[base.SNo].colorInfo[item.idx].metallicPower = value;
				base.chaCtrl.ChangeAccessoryColor(base.SNo);
			};
			item.val.onSetDefaultValue = () => (null == base.chaCtrl.cmpAccessory[base.SNo]) ? 0f : (item.idx switch
			{
				0 => base.chaCtrl.cmpAccessory[base.SNo].defMetallicPower01, 
				1 => base.chaCtrl.cmpAccessory[base.SNo].defMetallicPower02, 
				2 => base.chaCtrl.cmpAccessory[base.SNo].defMetallicPower03, 
				3 => base.chaCtrl.cmpAccessory[base.SNo].defMetallicPower04, 
				_ => 0f, 
			});
		});
		if (null != btnDefaultColor)
		{
			btnDefaultColor.OnClickAsObservable().Subscribe(delegate
			{
				SetDefaultColor();
				base.chaCtrl.ChangeAccessoryColor(base.SNo);
				UpdateCustomUI();
			});
		}
		if ((bool)csHairBaseColor)
		{
			csHairBaseColor.actUpdateColor = delegate(Color color)
			{
				base.nowAcs.parts[base.SNo].colorInfo[0].color = color;
				base.orgAcs.parts[base.SNo].colorInfo[0].color = color;
				base.chaCtrl.ChangeHairTypeAccessoryColor(base.SNo);
			};
		}
		if ((bool)csHairTopColor)
		{
			csHairTopColor.actUpdateColor = delegate(Color color)
			{
				base.nowAcs.parts[base.SNo].colorInfo[1].color = color;
				base.orgAcs.parts[base.SNo].colorInfo[1].color = color;
				base.chaCtrl.ChangeHairTypeAccessoryColor(base.SNo);
			};
		}
		if ((bool)csHairUnderColor)
		{
			csHairUnderColor.actUpdateColor = delegate(Color color)
			{
				base.nowAcs.parts[base.SNo].colorInfo[2].color = color;
				base.orgAcs.parts[base.SNo].colorInfo[2].color = color;
				base.chaCtrl.ChangeHairTypeAccessoryColor(base.SNo);
			};
		}
		if ((bool)csHairSpecular)
		{
			csHairSpecular.actUpdateColor = delegate(Color color)
			{
				base.nowAcs.parts[base.SNo].colorInfo[3].color = color;
				base.orgAcs.parts[base.SNo].colorInfo[3].color = color;
				base.chaCtrl.ChangeHairTypeAccessoryColor(base.SNo);
			};
		}
		if ((bool)ssHairMetallic)
		{
			ssHairMetallic.onChange = delegate(float value)
			{
				base.nowAcs.parts[base.SNo].colorInfo[0].metallicPower = value;
				base.orgAcs.parts[base.SNo].colorInfo[0].metallicPower = value;
				base.chaCtrl.ChangeHairTypeAccessoryColor(base.SNo);
			};
		}
		if ((bool)ssHairSmoothness)
		{
			ssHairSmoothness.onChange = delegate(float value)
			{
				base.nowAcs.parts[base.SNo].colorInfo[0].smoothnessPower = value;
				base.orgAcs.parts[base.SNo].colorInfo[0].smoothnessPower = value;
				base.chaCtrl.ChangeHairTypeAccessoryColor(base.SNo);
			};
		}
		(from item in btnGetHairColor.Select((Button val, int idx) => new { val, idx })
			where item.val != null
			select item).ToList().ForEach(item =>
		{
			item.val.OnClickAsObservable().Subscribe(delegate
			{
				ChangeHairTypeAccessoryColor(item.idx);
			});
		});
		if (tglParent.Any())
		{
			(from tgl in tglParent.Select((Toggle val, int idx) => new { val, idx })
				where tgl.val != null
				select tgl).ToList().ForEach(tgl =>
			{
				(from isOn in tgl.val.onValueChanged.AsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					ChangeAcsParent(tgl.idx);
				});
			});
		}
		if (null != btnDefaultParent)
		{
			btnDefaultParent.OnClickAsObservable().Subscribe(delegate
			{
				string accessoryDefaultParentStr = base.chaCtrl.GetAccessoryDefaultParentStr(base.SNo);
				base.chaCtrl.ChangeAccessoryParent(base.SNo, accessoryDefaultParentStr);
				base.orgAcs.parts[base.SNo].parentKey = base.nowAcs.parts[base.SNo].parentKey;
				UpdateCustomUI();
			});
		}
		tglNoShake.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			base.nowAcs.parts[base.SNo].noShake = isOn;
			base.orgAcs.parts[base.SNo].noShake = isOn;
		});
		GameObject[] array = new GameObject[2]
		{
			base.customBase.objAcs01ControllerTop,
			base.customBase.objAcs02ControllerTop
		};
		for (int num = 0; num < acCorrect.Length; num++)
		{
			acCorrect[num].CreateGuid(array[num]);
			acCorrect[num].Initialize(base.SNo, num);
		}
		StartCoroutine(SetInputText());
		backSNo = base.SNo;
	}
}
