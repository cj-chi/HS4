using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsB_Skin : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscSkinType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomSliderSet ssDetailPower;

	[SerializeField]
	private CustomSelectScrollController sscDetailType;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomColorSet csSkinColor;

	[SerializeField]
	private CustomSliderSet ssSkinGloss;

	[SerializeField]
	private CustomSliderSet ssSkinMetallic;

	[SerializeField]
	private CustomSkinColorPreset hcPreset;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssDetailPower.SetSliderValue(base.body.detailPower);
		ssSkinGloss.SetSliderValue(base.body.skinGlossPower);
		ssSkinMetallic.SetSliderValue(base.body.skinMetallicPower);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscSkinType.SetToggleID(base.body.skinId);
		sscDetailType.SetToggleID(base.body.detailId);
		csSkinColor.SetColor(base.body.skinColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssDetailPower.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.detailPower));
		ssSkinGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.skinGlossPower));
		ssSkinMetallic.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.skinMetallicPower));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsBodySkinType += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList((base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mt_skin_b : ChaListDefine.CategoryNo.ft_skin_b);
		sscSkinType.CreateList(lst);
		sscSkinType.SetToggleID(base.body.skinId);
		sscSkinType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.body.skinId != info.id)
			{
				base.body.skinId = info.id;
				base.chaCtrl.AddUpdateCMBodyTexFlags(inpBase: true, inpPaint01: false, inpPaint02: false, inpSunburn: false);
				base.chaCtrl.CreateBodyTexture();
			}
		};
		ssDetailPower.onChange = delegate(float value)
		{
			base.body.detailPower = value;
			base.chaCtrl.ChangeBodyDetailPower();
		};
		ssDetailPower.onSetDefaultValue = () => base.defChaCtrl.custom.body.detailPower;
		List<CustomSelectInfo> lst2 = CvsBase.CreateSelectList((base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mt_detail_b : ChaListDefine.CategoryNo.ft_detail_b);
		sscDetailType.CreateList(lst2);
		sscDetailType.SetToggleID(base.body.detailId);
		sscDetailType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.body.detailId != info.id)
			{
				base.body.detailId = info.id;
				base.chaCtrl.AddUpdateCMBodyTexFlags(inpBase: true, inpPaint01: false, inpPaint02: false, inpSunburn: false);
				base.chaCtrl.CreateBodyTexture();
			}
		};
		csSkinColor.actUpdateColor = delegate(Color color)
		{
			base.body.skinColor = color;
			base.chaCtrl.AddUpdateCMBodyColorFlags(inpBase: true, inpPaint01: false, inpPaint02: false, inpSunburn: false);
			base.chaCtrl.CreateBodyTexture();
			base.chaCtrl.AddUpdateCMFaceColorFlags(inpBase: true, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssSkinGloss.onChange = delegate(float value)
		{
			base.body.skinGlossPower = value;
			base.chaCtrl.ChangeBodyGlossPower();
			base.chaCtrl.ChangeFaceGlossPower();
		};
		ssSkinGloss.onSetDefaultValue = () => base.defChaCtrl.custom.body.skinGlossPower;
		ssSkinMetallic.onChange = delegate(float value)
		{
			base.body.skinMetallicPower = value;
			base.chaCtrl.ChangeBodyMetallicPower();
			base.chaCtrl.ChangeFaceMetallicPower();
		};
		ssSkinMetallic.onSetDefaultValue = () => base.defChaCtrl.custom.body.skinMetallicPower;
		hcPreset.onClick = delegate(Color color)
		{
			base.body.skinColor = color;
			base.chaCtrl.AddUpdateCMBodyColorFlags(inpBase: true, inpPaint01: false, inpPaint02: false, inpSunburn: false);
			base.chaCtrl.CreateBodyTexture();
			base.chaCtrl.AddUpdateCMFaceColorFlags(inpBase: true, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
			csSkinColor.SetColor(color);
		};
		StartCoroutine(SetInputText());
	}
}
