using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_ShapeNose : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssNoseAllY;

	[SerializeField]
	private CustomSliderSet ssNoseAllZ;

	[SerializeField]
	private CustomSliderSet ssNoseAllRotX;

	[SerializeField]
	private CustomSliderSet ssNoseAllW;

	[SerializeField]
	private CustomSliderSet ssNoseBridgeH;

	[SerializeField]
	private CustomSliderSet ssNoseBridgeW;

	[SerializeField]
	private CustomSliderSet ssNoseBridgeForm;

	[SerializeField]
	private CustomSliderSet ssNoseWingW;

	[SerializeField]
	private CustomSliderSet ssNoseWingY;

	[SerializeField]
	private CustomSliderSet ssNoseWingZ;

	[SerializeField]
	private CustomSliderSet ssNoseWingRotX;

	[SerializeField]
	private CustomSliderSet ssNoseWingRotZ;

	[SerializeField]
	private CustomSliderSet ssNoseH;

	[SerializeField]
	private CustomSliderSet ssNoseRotX;

	[SerializeField]
	private CustomSliderSet ssNoseSize;

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
		shapeIdx = new int[15]
		{
			32, 33, 34, 35, 36, 37, 38, 39, 40, 41,
			42, 43, 44, 45, 46
		};
		ssShape = new CustomSliderSet[15]
		{
			ssNoseAllY, ssNoseAllZ, ssNoseAllRotX, ssNoseAllW, ssNoseBridgeH, ssNoseBridgeW, ssNoseBridgeForm, ssNoseWingW, ssNoseWingY, ssNoseWingZ,
			ssNoseWingRotX, ssNoseWingRotZ, ssNoseH, ssNoseRotX, ssNoseSize
		};
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceShapeNose += UpdateCustomUI;
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
