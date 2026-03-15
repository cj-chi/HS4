using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_ShapeMouth : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssMouthY;

	[SerializeField]
	private CustomSliderSet ssMouthW;

	[SerializeField]
	private CustomSliderSet ssMouthH;

	[SerializeField]
	private CustomSliderSet ssMouthZ;

	[SerializeField]
	private CustomSliderSet ssMouthUpForm;

	[SerializeField]
	private CustomSliderSet ssMouthLowForm;

	[SerializeField]
	private CustomSliderSet ssMouthCornerForm;

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
		shapeIdx = new int[7] { 47, 48, 49, 50, 51, 52, 53 };
		ssShape = new CustomSliderSet[7] { ssMouthY, ssMouthW, ssMouthH, ssMouthZ, ssMouthUpForm, ssMouthLowForm, ssMouthCornerForm };
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceShapeMouth += UpdateCustomUI;
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
