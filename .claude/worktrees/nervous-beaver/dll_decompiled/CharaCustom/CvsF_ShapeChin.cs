using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_ShapeChin : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssChinW;

	[SerializeField]
	private CustomSliderSet ssChinY;

	[SerializeField]
	private CustomSliderSet ssChinZ;

	[SerializeField]
	private CustomSliderSet ssChinRot;

	[SerializeField]
	private CustomSliderSet ssChinLowY;

	[SerializeField]
	private CustomSliderSet ssChinTipW;

	[SerializeField]
	private CustomSliderSet ssChinTipY;

	[SerializeField]
	private CustomSliderSet ssChinTipZ;

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
		shapeIdx = new int[8] { 5, 6, 7, 8, 9, 10, 11, 12 };
		ssShape = new CustomSliderSet[8] { ssChinW, ssChinY, ssChinZ, ssChinRot, ssChinLowY, ssChinTipW, ssChinTipY, ssChinTipZ };
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceShapeChin += UpdateCustomUI;
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
