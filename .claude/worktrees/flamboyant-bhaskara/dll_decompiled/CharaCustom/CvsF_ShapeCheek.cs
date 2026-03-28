using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsF_ShapeCheek : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssCheekLowY;

	[SerializeField]
	private CustomSliderSet ssCheekLowZ;

	[SerializeField]
	private CustomSliderSet ssCheekLowW;

	[SerializeField]
	private CustomSliderSet ssCheekUpY;

	[SerializeField]
	private CustomSliderSet ssCheekUpZ;

	[SerializeField]
	private CustomSliderSet ssCheekUpW;

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
		shapeIdx = new int[6] { 13, 14, 15, 16, 17, 18 };
		ssShape = new CustomSliderSet[6] { ssCheekLowY, ssCheekLowZ, ssCheekLowW, ssCheekUpY, ssCheekUpZ, ssCheekUpW };
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceShapeCheek += UpdateCustomUI;
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
