using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_ShapeEar : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssEarSize;

	[SerializeField]
	private CustomSliderSet ssEarRotY;

	[SerializeField]
	private CustomSliderSet ssEarRotZ;

	[SerializeField]
	private CustomSliderSet ssEarUpForm;

	[SerializeField]
	private CustomSliderSet ssEarLowForm;

	private CustomSliderSet[] ssShape;

	private int[] shapeIdx;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		for (int i = 0; i < ssShape.Length; i++)
		{
			ssShape[i].SetSliderValue(base.face.shapeValueFace[shapeIdx[i]]);
		}
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		for (int num = 0; num < ssShape.Length; num++)
		{
			ssShape[num].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.shapeValueFace[shapeIdx[num]]));
		}
	}

	public void Awake()
	{
		shapeIdx = new int[5] { 54, 55, 56, 57, 58 };
		ssShape = new CustomSliderSet[5] { ssEarSize, ssEarRotY, ssEarRotZ, ssEarUpForm, ssEarLowForm };
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceShapeEar += UpdateCustomUI;
		for (int i = 0; i < ssShape.Length; i++)
		{
			int idx = shapeIdx[i];
			ssShape[i].onChange = delegate(float value)
			{
				base.face.shapeValueFace[idx] = value;
				base.chaCtrl.SetShapeFaceValue(idx, value);
			};
			ssShape[i].onSetDefaultValue = () => base.defChaCtrl.custom.face.shapeValueFace[idx];
		}
		StartCoroutine(SetInputText());
	}
}
