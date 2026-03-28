using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_MakeupEyeshadow : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscEyeshadowType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csEyeshadowColor;

	[SerializeField]
	private CustomSliderSet ssEyeshadowGloss;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssEyeshadowGloss.SetSliderValue(base.makeup.eyeshadowGloss);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscEyeshadowType.SetToggleID(base.makeup.eyeshadowId);
		csEyeshadowColor.SetColor(base.makeup.eyeshadowColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssEyeshadowGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.eyeshadowGloss));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsEyeshadow += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyeshadow);
		sscEyeshadowType.CreateList(lst);
		sscEyeshadowType.SetToggleID(base.makeup.eyeshadowId);
		sscEyeshadowType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.makeup.eyeshadowId != info.id)
			{
				base.makeup.eyeshadowId = info.id;
				base.chaCtrl.AddUpdateCMFaceTexFlags(inpBase: false, inpEyeshadow: true, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false, inpMole: false);
				base.chaCtrl.CreateFaceTexture();
			}
		};
		csEyeshadowColor.actUpdateColor = delegate(Color color)
		{
			base.makeup.eyeshadowColor = color;
			base.chaCtrl.AddUpdateCMFaceColorFlags(inpBase: false, inpEyeshadow: true, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssEyeshadowGloss.onChange = delegate(float value)
		{
			base.makeup.eyeshadowGloss = value;
			base.chaCtrl.AddUpdateCMFaceGlossFlags(inpEyeshadow: true, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssEyeshadowGloss.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.eyeshadowGloss;
		StartCoroutine(SetInputText());
	}
}
