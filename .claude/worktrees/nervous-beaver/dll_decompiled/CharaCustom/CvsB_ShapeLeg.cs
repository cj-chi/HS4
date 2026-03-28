using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsB_ShapeLeg : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssThighUp;

	[SerializeField]
	private CustomSliderSet ssThighLow;

	[SerializeField]
	private CustomSliderSet ssCalf;

	[SerializeField]
	private CustomSliderSet ssAnkle;

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
		shapeIdx = new int[4] { 25, 26, 27, 28 };
		ssShape = new CustomSliderSet[4] { ssThighUp, ssThighLow, ssCalf, ssAnkle };
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
