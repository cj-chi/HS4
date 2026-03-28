using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsB_ShapeUpper : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssNeckW;

	[SerializeField]
	private CustomSliderSet ssNeckZ;

	[SerializeField]
	private CustomSliderSet ssBodyShoulderW;

	[SerializeField]
	private CustomSliderSet ssBodyShoulderZ;

	[SerializeField]
	private CustomSliderSet ssBodyUpW;

	[SerializeField]
	private CustomSliderSet ssBodyUpZ;

	[SerializeField]
	private CustomSliderSet ssBodyLowW;

	[SerializeField]
	private CustomSliderSet ssBodyLowZ;

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
			ssShape[i].SetSliderValue(base.body.shapeValueBody[shapeIdx[i]]);
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
			ssShape[num].SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.shapeValueBody[shapeIdx[num]]));
		}
	}

	public void Awake()
	{
		shapeIdx = new int[8] { 10, 11, 12, 13, 14, 15, 16, 17 };
		ssShape = new CustomSliderSet[8] { ssNeckW, ssNeckZ, ssBodyShoulderW, ssBodyShoulderZ, ssBodyUpW, ssBodyUpZ, ssBodyLowW, ssBodyLowZ };
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsBodyShapeUpper += UpdateCustomUI;
		for (int i = 0; i < ssShape.Length; i++)
		{
			int idx = shapeIdx[i];
			ssShape[i].onChange = delegate(float value)
			{
				base.body.shapeValueBody[idx] = value;
				base.chaCtrl.SetShapeBodyValue(idx, value);
			};
			ssShape[i].onSetDefaultValue = () => base.defChaCtrl.custom.body.shapeValueBody[idx];
		}
		StartCoroutine(SetInputText());
	}
}
