using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_MakeupCheek : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscCheekType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csCheekColor;

	[SerializeField]
	private CustomSliderSet ssCheekGloss;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssCheekGloss.SetSliderValue(base.makeup.cheekGloss);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscCheekType.SetToggleID(base.makeup.cheekId);
		csCheekColor.SetColor(base.makeup.cheekColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssCheekGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.cheekGloss));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsCheek += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_cheek);
		sscCheekType.CreateList(lst);
		sscCheekType.SetToggleID(base.makeup.cheekId);
		sscCheekType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.makeup.cheekId != info.id)
			{
				base.makeup.cheekId = info.id;
				base.chaCtrl.AddUpdateCMFaceTexFlags(inpBase: false, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: true, inpLip: false, inpMole: false);
				base.chaCtrl.CreateFaceTexture();
			}
		};
		csCheekColor.actUpdateColor = delegate(Color color)
		{
			base.makeup.cheekColor = color;
			base.chaCtrl.AddUpdateCMFaceColorFlags(inpBase: false, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: true, inpLip: false, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssCheekGloss.onChange = delegate(float value)
		{
			base.makeup.cheekGloss = value;
			base.chaCtrl.AddUpdateCMFaceGlossFlags(inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: true, inpLip: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssCheekGloss.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.cheekGloss;
		StartCoroutine(SetInputText());
	}
}
