using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_EyeLR : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomColorSet csWhiteColor;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscPupilType;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomColorSet csPupilColor;

	[SerializeField]
	private CustomSliderSet ssPupilEmission;

	[SerializeField]
	private CustomSliderSet ssPupilW;

	[SerializeField]
	private CustomSliderSet ssPupilH;

	[Header("【設定04】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscBlackType;

	[Header("【設定05】----------------------")]
	[SerializeField]
	private CustomColorSet csBlackColor;

	[SerializeField]
	private CustomSliderSet ssBlackW;

	[SerializeField]
	private CustomSliderSet ssBlackH;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssPupilEmission.SetSliderValue(base.face.pupil[base.SNo].pupilEmission);
		ssPupilW.SetSliderValue(base.face.pupil[base.SNo].pupilW);
		ssPupilH.SetSliderValue(base.face.pupil[base.SNo].pupilH);
		ssBlackW.SetSliderValue(base.face.pupil[base.SNo].blackW);
		ssBlackH.SetSliderValue(base.face.pupil[base.SNo].blackH);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscPupilType.SetToggleID(base.face.pupil[base.SNo].pupilId);
		sscBlackType.SetToggleID(base.face.pupil[base.SNo].blackId);
		csWhiteColor.SetColor(base.face.pupil[base.SNo].whiteColor);
		csPupilColor.SetColor(base.face.pupil[base.SNo].pupilColor);
		csBlackColor.SetColor(base.face.pupil[base.SNo].blackColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssPupilEmission.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.pupil[base.SNo].pupilEmission));
		ssPupilW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.pupil[base.SNo].pupilW));
		ssPupilH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.pupil[base.SNo].pupilH));
		ssBlackW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.pupil[base.SNo].blackW));
		ssBlackH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.pupil[base.SNo].blackH));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsEyeLR += UpdateCustomUI;
		csWhiteColor.actUpdateColor = delegate(Color color)
		{
			base.face.pupil[base.SNo].whiteColor = color;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].whiteColor = color;
			}
			base.chaCtrl.ChangeWhiteEyesColor(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eye);
		sscPupilType.CreateList(lst);
		sscPupilType.SetToggleID(base.face.pupil[base.SNo].pupilId);
		sscPupilType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.pupil[base.SNo].pupilId != info.id)
			{
				base.face.pupil[base.SNo].pupilId = info.id;
				if (base.face.pupilSameSetting)
				{
					base.face.pupil[base.SNo ^ 1].pupilId = info.id;
				}
				base.chaCtrl.ChangeEyesKind(base.face.pupilSameSetting ? 2 : base.SNo);
			}
		};
		csPupilColor.actUpdateColor = delegate(Color color)
		{
			base.face.pupil[base.SNo].pupilColor = color;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].pupilColor = color;
			}
			base.chaCtrl.ChangeEyesColor(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		ssPupilEmission.onChange = delegate(float value)
		{
			base.face.pupil[base.SNo].pupilEmission = value;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].pupilEmission = value;
			}
			base.chaCtrl.ChangeEyesEmission(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		ssPupilEmission.onSetDefaultValue = () => base.defChaCtrl.custom.face.pupil[base.SNo].pupilEmission;
		ssPupilW.onChange = delegate(float value)
		{
			base.face.pupil[base.SNo].pupilW = value;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].pupilW = value;
			}
			base.chaCtrl.ChangeEyesWH(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		ssPupilW.onSetDefaultValue = () => base.defChaCtrl.custom.face.pupil[base.SNo].pupilW;
		ssPupilH.onChange = delegate(float value)
		{
			base.face.pupil[base.SNo].pupilH = value;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].pupilH = value;
			}
			base.chaCtrl.ChangeEyesWH(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		ssPupilH.onSetDefaultValue = () => base.defChaCtrl.custom.face.pupil[base.SNo].pupilH;
		List<CustomSelectInfo> lst2 = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyeblack);
		sscBlackType.CreateList(lst2);
		sscBlackType.SetToggleID(base.face.pupil[base.SNo].blackId);
		sscBlackType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.pupil[base.SNo].blackId != info.id)
			{
				base.face.pupil[base.SNo].blackId = info.id;
				if (base.face.pupilSameSetting)
				{
					base.face.pupil[base.SNo ^ 1].blackId = info.id;
				}
				base.chaCtrl.ChangeBlackEyesKind(base.face.pupilSameSetting ? 2 : base.SNo);
			}
		};
		csBlackColor.actUpdateColor = delegate(Color color)
		{
			base.face.pupil[base.SNo].blackColor = color;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].blackColor = color;
			}
			base.chaCtrl.ChangeBlackEyesColor(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		ssBlackW.onChange = delegate(float value)
		{
			base.face.pupil[base.SNo].blackW = value;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].blackW = value;
			}
			base.chaCtrl.ChangeBlackEyesWH(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		ssBlackW.onSetDefaultValue = () => base.defChaCtrl.custom.face.pupil[base.SNo].blackW;
		ssBlackH.onChange = delegate(float value)
		{
			base.face.pupil[base.SNo].blackH = value;
			if (base.face.pupilSameSetting)
			{
				base.face.pupil[base.SNo ^ 1].blackH = value;
			}
			base.chaCtrl.ChangeBlackEyesWH(base.face.pupilSameSetting ? 2 : base.SNo);
		};
		ssBlackH.onSetDefaultValue = () => base.defChaCtrl.custom.face.pupil[base.SNo].blackH;
		StartCoroutine(SetInputText());
	}
}
