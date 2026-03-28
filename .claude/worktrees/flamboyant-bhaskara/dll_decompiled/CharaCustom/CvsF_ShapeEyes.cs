using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_ShapeEyes : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssEyeY;

	[SerializeField]
	private CustomSliderSet ssEyeX;

	[SerializeField]
	private CustomSliderSet ssEyeZ;

	[SerializeField]
	private CustomSliderSet ssEyeW;

	[SerializeField]
	private CustomSliderSet ssEyeH;

	[SerializeField]
	private CustomSliderSet ssEyeRotZ;

	[SerializeField]
	private CustomSliderSet ssEyeRotY;

	[SerializeField]
	private CustomSliderSet ssEyeInX;

	[SerializeField]
	private CustomSliderSet ssEyeOutX;

	[SerializeField]
	private CustomSliderSet ssEyeInY;

	[SerializeField]
	private CustomSliderSet ssEyeOutY;

	[SerializeField]
	private CustomSliderSet ssEyelidForm01;

	[SerializeField]
	private CustomSliderSet ssEyelidForm02;

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
		shapeIdx = new int[13]
		{
			19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
			29, 30, 31
		};
		ssShape = new CustomSliderSet[13]
		{
			ssEyeY, ssEyeX, ssEyeZ, ssEyeW, ssEyeH, ssEyeRotZ, ssEyeRotY, ssEyeInX, ssEyeOutX, ssEyeInY,
			ssEyeOutY, ssEyelidForm01, ssEyelidForm02
		};
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceShapeEyes += UpdateCustomUI;
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
