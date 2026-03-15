using System.Collections;
using UnityEngine;

namespace CharaCustom;

public class CvsB_ShapeBreast : CvsBase
{
	[SerializeField]
	private CustomSliderSet ssBustSize;

	[SerializeField]
	private CustomSliderSet ssBustY;

	[SerializeField]
	private CustomSliderSet ssBustRotX;

	[SerializeField]
	private CustomSliderSet ssBustX;

	[SerializeField]
	private CustomSliderSet ssBustRotY;

	[SerializeField]
	private CustomSliderSet ssBustSharp;

	[SerializeField]
	private CustomSliderSet ssAreolaBulge;

	[SerializeField]
	private CustomSliderSet ssNipWeight;

	[SerializeField]
	private CustomSliderSet ssNipStand;

	[SerializeField]
	private CustomSliderSet ssBustSoftness;

	[SerializeField]
	private CustomSliderSet ssBustWeight;

	[SerializeField]
	private CustomSliderSet ssAreolaSize;

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
		ssBustSoftness.SetSliderValue(base.body.bustSoftness);
		ssBustWeight.SetSliderValue(base.body.bustWeight);
		ssAreolaSize.SetSliderValue(base.body.areolaSize);
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
		ssBustSoftness.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.bustSoftness));
		ssBustWeight.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.bustWeight));
		ssAreolaSize.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.areolaSize));
	}

	public void Awake()
	{
		shapeIdx = new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 32 };
		ssShape = new CustomSliderSet[9] { ssBustSize, ssBustY, ssBustRotX, ssBustX, ssBustRotY, ssBustSharp, ssAreolaBulge, ssNipWeight, ssNipStand };
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsBodyShapeBreast += UpdateCustomUI;
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
		ssBustSoftness.onChange = delegate(float value)
		{
			base.body.bustSoftness = value;
			base.chaCtrl.UpdateBustSoftness();
		};
		ssBustSoftness.onSetDefaultValue = () => base.defChaCtrl.custom.body.bustSoftness;
		ssBustWeight.onChange = delegate(float value)
		{
			base.body.bustWeight = value;
			base.chaCtrl.UpdateBustGravity();
		};
		ssBustWeight.onSetDefaultValue = () => base.defChaCtrl.custom.body.bustWeight;
		ssAreolaSize.onChange = delegate(float value)
		{
			base.body.areolaSize = value;
			base.chaCtrl.ChangeNipScale();
		};
		ssAreolaSize.onSetDefaultValue = () => base.defChaCtrl.custom.body.areolaSize;
		StartCoroutine(SetInputText());
	}
}
