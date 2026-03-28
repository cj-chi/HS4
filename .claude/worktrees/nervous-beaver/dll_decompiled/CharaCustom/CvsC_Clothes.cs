using System.Collections;
using System.Collections.Generic;
using AIChara;
using Illusion.Extensions;
using MessagePack;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsC_Clothes : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscClothesType;

	[SerializeField]
	private Button btnColorAllReset;

	[Header("【設定02～04】-------------------")]
	[SerializeField]
	private CustomClothesColorSet[] ccsColorSet;

	[Header("【設定05】----------------------")]
	[SerializeField]
	private CustomSliderSet ssBreak;

	[SerializeField]
	private Toggle tglOption01;

	[SerializeField]
	private Toggle tglOption02;

	private int backSNo = -1;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	public void UpdateClothesList()
	{
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList((new ChaListDefine.CategoryNo[8]
		{
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mo_top : ChaListDefine.CategoryNo.fo_top,
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mo_bot : ChaListDefine.CategoryNo.fo_bot,
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_inner_t,
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_inner_b,
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mo_gloves : ChaListDefine.CategoryNo.fo_gloves,
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_panst,
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_socks,
			(base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mo_shoes : ChaListDefine.CategoryNo.fo_shoes
		})[base.SNo]);
		sscClothesType.CreateList(lst);
	}

	public void RestrictClothesMenu()
	{
		CmpClothes cmpClothes = base.chaCtrl.cmpClothes[base.SNo];
		if (null == cmpClothes)
		{
			ShowOrHideTab(false, 1, 2, 3, 4);
			return;
		}
		ShowOrHideTab(true, 1, 2, 3, 4);
		List<int> list = new List<int>();
		if (!cmpClothes.useColorN01 && !cmpClothes.useColorA01)
		{
			list.Add(1);
		}
		ccsColorSet[0].EnableColorAlpha(cmpClothes.useColorA01);
		if (!cmpClothes.useColorN02 && !cmpClothes.useColorA02)
		{
			list.Add(2);
		}
		ccsColorSet[1].EnableColorAlpha(cmpClothes.useColorA02);
		if (!cmpClothes.useColorN03 && !cmpClothes.useColorA03)
		{
			list.Add(3);
		}
		ccsColorSet[2].EnableColorAlpha(cmpClothes.useColorA03);
		ShowOrHideTab(show: false, list.ToArray());
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if ((bool)ssBreak)
		{
			flag = cmpClothes.useBreak;
			ssBreak.gameObject.SetActiveIfDifferent(flag);
		}
		if ((bool)tglOption01)
		{
			flag2 = cmpClothes.objOpt01 != null && cmpClothes.objOpt01.Length != 0;
			tglOption01.gameObject.SetActiveIfDifferent(flag2);
		}
		if ((bool)tglOption02)
		{
			flag3 = cmpClothes.objOpt02 != null && cmpClothes.objOpt02.Length != 0;
			tglOption02.gameObject.SetActiveIfDifferent(flag3);
		}
		ShowOrHideTab(flag || flag2 || flag3, 4);
	}

	private void CalculateUI()
	{
		ssBreak.SetSliderValue(base.nowClothes.parts[base.SNo].breakRate);
	}

	public override void UpdateCustomUI()
	{
		if (backSNo != base.SNo)
		{
			int[] array = new int[8] { 0, 0, 1, 1, 1, 1, 1, 0 };
			base.customBase.ChangeClothesStateAuto(array[base.SNo]);
			UpdateClothesList();
			for (int i = 0; i < ccsColorSet.Length; i++)
			{
				ccsColorSet[i].Initialize(base.SNo, i);
			}
			backSNo = base.SNo;
		}
		base.UpdateCustomUI();
		CalculateUI();
		sscClothesType.SetToggleID(base.nowClothes.parts[base.SNo].id);
		if ((bool)tglOption01)
		{
			tglOption01.SetIsOnWithoutCallback(!base.nowClothes.parts[base.SNo].hideOpt[0]);
		}
		if ((bool)tglOption02)
		{
			tglOption02.SetIsOnWithoutCallback(!base.nowClothes.parts[base.SNo].hideOpt[1]);
		}
		for (int j = 0; j < ccsColorSet.Length; j++)
		{
			ccsColorSet[j].UpdateCustomUI();
		}
		base.customBase.RestrictSubMenu();
		RestrictClothesMenu();
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssBreak.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.nowClothes.parts[base.SNo].breakRate));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsClothes += UpdateCustomUI;
		base.customBase.RestrictSubMenu();
		UpdateClothesList();
		sscClothesType.SetToggleID(base.nowClothes.parts[base.SNo].id);
		sscClothesType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.nowClothes.parts[base.SNo].id != info.id)
			{
				base.chaCtrl.ChangeClothes(base.SNo, info.id);
				base.orgClothes.parts[base.SNo].id = base.nowClothes.parts[base.SNo].id;
				for (int i = 0; i < 3; i++)
				{
					base.orgClothes.parts[base.SNo].colorInfo[i].baseColor = base.nowClothes.parts[base.SNo].colorInfo[i].baseColor;
					ccsColorSet[i].UpdateCustomUI();
				}
				if (base.SNo == 0 || base.SNo == 2)
				{
					base.customBase.RestrictSubMenu();
				}
				RestrictClothesMenu();
			}
		};
		if ((bool)btnColorAllReset)
		{
			btnColorAllReset.OnClickAsObservable().Subscribe(delegate
			{
				base.chaCtrl.SetClothesDefaultSetting(base.SNo);
				for (int i = 0; i < 3; i++)
				{
					byte[] bytes = MessagePackSerializer.Serialize(base.nowClothes.parts[base.SNo].colorInfo[i]);
					base.orgClothes.parts[base.SNo].colorInfo[i] = MessagePackSerializer.Deserialize<ChaFileClothes.PartsInfo.ColorInfo>(bytes);
				}
				base.chaCtrl.ChangeCustomClothes(base.SNo, updateColor: true, updateTex01: true, updateTex02: true, updateTex03: true);
				for (int j = 0; j < ccsColorSet.Length; j++)
				{
					ccsColorSet[j].UpdateCustomUI();
				}
			});
		}
		ccsColorSet[0].Initialize(base.SNo, 0);
		ccsColorSet[1].Initialize(base.SNo, 1);
		ccsColorSet[2].Initialize(base.SNo, 2);
		ssBreak.onChange = delegate(float value)
		{
			base.nowClothes.parts[base.SNo].breakRate = value;
			base.orgClothes.parts[base.SNo].breakRate = value;
			base.chaCtrl.ChangeBreakClothes(base.SNo);
		};
		ssBreak.onSetDefaultValue = () => 0f;
		if ((bool)tglOption01)
		{
			tglOption01.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				base.nowClothes.parts[base.SNo].hideOpt[0] = !isOn;
				base.orgClothes.parts[base.SNo].hideOpt[0] = !isOn;
			});
		}
		if ((bool)tglOption02)
		{
			tglOption02.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				base.nowClothes.parts[base.SNo].hideOpt[1] = !isOn;
				base.orgClothes.parts[base.SNo].hideOpt[1] = !isOn;
			});
		}
		StartCoroutine(SetInputText());
		backSNo = base.SNo;
	}
}
