using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_EyeEtc : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssPupilY;

	[SerializeField]
	private CustomSliderSet ssShadowScale;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssPupilY.SetSliderValue(base.face.pupilY);
		ssShadowScale.SetSliderValue(base.face.whiteShadowScale);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssPupilY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.pupilY));
		ssShadowScale.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.whiteShadowScale));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsEyeEtc += UpdateCustomUI;
		ssPupilY.onChange = delegate(float value)
		{
			base.face.pupilY = value;
			base.chaCtrl.ChangeEyesBasePosY();
		};
		ssPupilY.onSetDefaultValue = () => base.defChaCtrl.custom.face.pupilY;
		ssShadowScale.onChange = delegate(float value)
		{
			base.face.whiteShadowScale = value;
			base.chaCtrl.ChangeEyesShadowRange();
		};
		ssShadowScale.onSetDefaultValue = () => base.defChaCtrl.custom.face.whiteShadowScale;
		StartCoroutine(SetInputText());
	}
}
