using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_EyeHL : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscEyeHLType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csEyeHLColor;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomSliderSet ssHLW;

	[SerializeField]
	private CustomSliderSet ssHLH;

	[SerializeField]
	private CustomSliderSet ssHLX;

	[SerializeField]
	private CustomSliderSet ssHLY;

	[SerializeField]
	private CustomSliderSet ssHLTilt;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssHLW.SetSliderValue(base.face.hlLayout.x);
		ssHLH.SetSliderValue(base.face.hlLayout.y);
		ssHLX.SetSliderValue(base.face.hlLayout.z);
		ssHLY.SetSliderValue(base.face.hlLayout.w);
		ssHLTilt.SetSliderValue(base.face.hlTilt);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscEyeHLType.SetToggleID(base.face.hlId);
		csEyeHLColor.SetColor(base.face.hlColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssHLW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.hlLayout.x));
		ssHLH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.hlLayout.y));
		ssHLX.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.hlLayout.z));
		ssHLY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.hlLayout.w));
		ssHLTilt.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.hlTilt));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsEyeHL += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eye_hl);
		sscEyeHLType.CreateList(lst);
		sscEyeHLType.SetToggleID(base.face.hlId);
		sscEyeHLType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.hlId != info.id)
			{
				base.face.hlId = info.id;
				base.chaCtrl.ChangeEyesHighlightKind();
			}
		};
		csEyeHLColor.actUpdateColor = delegate(Color color)
		{
			base.face.hlColor = color;
			base.chaCtrl.ChangeEyesHighlightColor();
		};
		ssHLW.onChange = delegate(float value)
		{
			base.face.hlLayout = new Vector4(value, base.face.hlLayout.y, base.face.hlLayout.z, base.face.hlLayout.w);
			base.chaCtrl.ChangeEyesHighlighLayout();
		};
		ssHLW.onSetDefaultValue = () => base.defChaCtrl.custom.face.hlLayout.x;
		ssHLH.onChange = delegate(float value)
		{
			base.face.hlLayout = new Vector4(base.face.hlLayout.x, value, base.face.hlLayout.z, base.face.hlLayout.w);
			base.chaCtrl.ChangeEyesHighlighLayout();
		};
		ssHLH.onSetDefaultValue = () => base.defChaCtrl.custom.face.hlLayout.y;
		ssHLX.onChange = delegate(float value)
		{
			base.face.hlLayout = new Vector4(base.face.hlLayout.x, base.face.hlLayout.y, value, base.face.hlLayout.w);
			base.chaCtrl.ChangeEyesHighlighLayout();
		};
		ssHLX.onSetDefaultValue = () => base.defChaCtrl.custom.face.hlLayout.z;
		ssHLY.onChange = delegate(float value)
		{
			base.face.hlLayout = new Vector4(base.face.hlLayout.x, base.face.hlLayout.y, base.face.hlLayout.z, value);
			base.chaCtrl.ChangeEyesHighlighLayout();
		};
		ssHLY.onSetDefaultValue = () => base.defChaCtrl.custom.face.hlLayout.w;
		ssHLTilt.onChange = delegate(float value)
		{
			base.face.hlTilt = value;
			base.chaCtrl.ChangeEyesHighlighTilt();
		};
		ssHLTilt.onSetDefaultValue = () => base.defChaCtrl.custom.face.hlTilt;
		StartCoroutine(SetInputText());
	}
}
