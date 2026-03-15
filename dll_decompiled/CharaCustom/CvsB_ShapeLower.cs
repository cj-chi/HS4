using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsB_ShapeLower : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssWaistY;

	[SerializeField]
	private CustomSliderSet ssWaistUpW;

	[SerializeField]
	private CustomSliderSet ssWaistUpZ;

	[SerializeField]
	private CustomSliderSet ssWaistLowW;

	[SerializeField]
	private CustomSliderSet ssWaistLowZ;

	[SerializeField]
	private CustomSliderSet ssHip;

	[SerializeField]
	private CustomSliderSet ssHipRotX;

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
		shapeIdx = new int[7] { 18, 19, 20, 21, 22, 23, 24 };
		ssShape = new CustomSliderSet[7] { ssWaistY, ssWaistUpW, ssWaistUpZ, ssWaistLowW, ssWaistLowZ, ssHip, ssHipRotX };
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsBodyShapeWhole += UpdateCustomUI;
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
