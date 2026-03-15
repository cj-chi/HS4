using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_ShapeEyebrow : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssEyebrowW;

	[SerializeField]
	private CustomSliderSet ssEyebrowH;

	[SerializeField]
	private CustomSliderSet ssEyebrowX;

	[SerializeField]
	private CustomSliderSet ssEyebrowY;

	[SerializeField]
	private CustomSliderSet ssEyebrowTilt;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssEyebrowW.SetSliderValue(base.face.eyebrowLayout.z);
		ssEyebrowH.SetSliderValue(base.face.eyebrowLayout.w);
		ssEyebrowX.SetSliderValue(base.face.eyebrowLayout.x);
		ssEyebrowY.SetSliderValue(base.face.eyebrowLayout.y);
		ssEyebrowTilt.SetSliderValue(base.face.eyebrowTilt);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssEyebrowW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.eyebrowLayout.z));
		ssEyebrowH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.eyebrowLayout.w));
		ssEyebrowX.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.eyebrowLayout.x));
		ssEyebrowY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.eyebrowLayout.y));
		ssEyebrowTilt.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.eyebrowTilt));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceShapeEyebrow += UpdateCustomUI;
		ssEyebrowW.onChange = delegate(float value)
		{
			base.face.eyebrowLayout = new Vector4(base.face.eyebrowLayout.x, base.face.eyebrowLayout.y, value, base.face.eyebrowLayout.w);
			base.chaCtrl.ChangeEyebrowLayout();
		};
		ssEyebrowW.onSetDefaultValue = () => base.defChaCtrl.custom.face.eyebrowLayout.z;
		ssEyebrowH.onChange = delegate(float value)
		{
			base.face.eyebrowLayout = new Vector4(base.face.eyebrowLayout.x, base.face.eyebrowLayout.y, base.face.eyebrowLayout.z, value);
			base.chaCtrl.ChangeEyebrowLayout();
		};
		ssEyebrowH.onSetDefaultValue = () => base.defChaCtrl.custom.face.eyebrowLayout.w;
		ssEyebrowX.onChange = delegate(float value)
		{
			base.face.eyebrowLayout = new Vector4(value, base.face.eyebrowLayout.y, base.face.eyebrowLayout.z, base.face.eyebrowLayout.w);
			base.chaCtrl.ChangeEyebrowLayout();
		};
		ssEyebrowX.onSetDefaultValue = () => base.defChaCtrl.custom.face.eyebrowLayout.x;
		ssEyebrowY.onChange = delegate(float value)
		{
			base.face.eyebrowLayout = new Vector4(base.face.eyebrowLayout.x, value, base.face.eyebrowLayout.z, base.face.eyebrowLayout.w);
			base.chaCtrl.ChangeEyebrowLayout();
		};
		ssEyebrowY.onSetDefaultValue = () => base.defChaCtrl.custom.face.eyebrowLayout.y;
		ssEyebrowTilt.onChange = delegate(float value)
		{
			base.face.eyebrowTilt = value;
			base.chaCtrl.ChangeEyebrowTilt();
		};
		ssEyebrowTilt.onSetDefaultValue = () => base.defChaCtrl.custom.face.eyebrowTilt;
		StartCoroutine(SetInputText());
	}
}
