using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsB_Nail : CvsBase
{
	[SerializeField]
	private CustomColorSet csNailColor;

	[SerializeField]
	private CustomSliderSet ssNailGloss;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssNailGloss.SetSliderValue(base.body.nailGlossPower);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		csNailColor.SetColor(base.body.nailColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssNailGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.nailGlossPower));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsNail += UpdateCustomUI;
		csNailColor.actUpdateColor = delegate(Color color)
		{
			base.body.nailColor = color;
			base.chaCtrl.ChangeNailColor();
		};
		ssNailGloss.onChange = delegate(float value)
		{
			base.body.nailGlossPower = value;
			base.chaCtrl.ChangeNailGloss();
		};
		ssNailGloss.onSetDefaultValue = () => base.defChaCtrl.custom.body.nailGlossPower;
		StartCoroutine(SetInputText());
	}
}
