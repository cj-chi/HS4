using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_MakeupLip : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscLipType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csLipColor;

	[SerializeField]
	private CustomSliderSet ssLipGloss;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssLipGloss.SetSliderValue(base.makeup.lipGloss);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscLipType.SetToggleID(base.makeup.lipId);
		csLipColor.SetColor(base.makeup.lipColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssLipGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.lipGloss));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsLip += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_lip);
		sscLipType.CreateList(lst);
		sscLipType.SetToggleID(base.makeup.lipId);
		sscLipType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.makeup.lipId != info.id)
			{
				base.makeup.lipId = info.id;
				base.chaCtrl.AddUpdateCMFaceTexFlags(inpBase: false, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: true, inpMole: false);
				base.chaCtrl.CreateFaceTexture();
			}
		};
		csLipColor.actUpdateColor = delegate(Color color)
		{
			base.makeup.lipColor = color;
			base.chaCtrl.AddUpdateCMFaceColorFlags(inpBase: false, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: true, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssLipGloss.onChange = delegate(float value)
		{
			base.makeup.lipGloss = value;
			base.chaCtrl.AddUpdateCMFaceGlossFlags(inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: true);
			base.chaCtrl.CreateFaceTexture();
		};
		ssLipGloss.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.lipGloss;
		StartCoroutine(SetInputText());
	}
}
