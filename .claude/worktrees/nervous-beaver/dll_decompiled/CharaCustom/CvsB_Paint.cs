using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsB_Paint : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscPaintType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csPaintColor;

	[SerializeField]
	private CustomSliderSet ssPaintGloss;

	[SerializeField]
	private CustomSliderSet ssPaintMetallic;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscPaintLayout;

	[SerializeField]
	private CustomSliderSet ssPaintW;

	[SerializeField]
	private CustomSliderSet ssPaintH;

	[SerializeField]
	private CustomSliderSet ssPaintX;

	[SerializeField]
	private CustomSliderSet ssPaintY;

	[SerializeField]
	private CustomSliderSet ssPaintRot;

	private Dictionary<int, Vector4> dictPaintLayout;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssPaintGloss.SetSliderValue(base.body.paintInfo[base.SNo].glossPower);
		ssPaintMetallic.SetSliderValue(base.body.paintInfo[base.SNo].metallicPower);
		ssPaintW.SetSliderValue(base.body.paintInfo[base.SNo].layout.x);
		ssPaintH.SetSliderValue(base.body.paintInfo[base.SNo].layout.y);
		ssPaintX.SetSliderValue(base.body.paintInfo[base.SNo].layout.z);
		ssPaintY.SetSliderValue(base.body.paintInfo[base.SNo].layout.w);
		ssPaintRot.SetSliderValue(base.body.paintInfo[base.SNo].rotation);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscPaintType.SetToggleID(base.body.paintInfo[base.SNo].id);
		csPaintColor.SetColor(base.body.paintInfo[base.SNo].color);
		sscPaintLayout.SetToggleID(base.body.paintInfo[base.SNo].layoutId);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssPaintGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.paintInfo[base.SNo].glossPower));
		ssPaintMetallic.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.paintInfo[base.SNo].metallicPower));
		ssPaintW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.paintInfo[base.SNo].layout.x));
		ssPaintH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.paintInfo[base.SNo].layout.y));
		ssPaintX.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.paintInfo[base.SNo].layout.z));
		ssPaintY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.paintInfo[base.SNo].layout.w));
		ssPaintRot.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.paintInfo[base.SNo].rotation));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsBodyPaint += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_paint);
		sscPaintType.CreateList(lst);
		sscPaintType.SetToggleID(base.body.paintInfo[base.SNo].id);
		sscPaintType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.body.paintInfo[base.SNo].id != info.id)
			{
				base.body.paintInfo[base.SNo].id = info.id;
				base.chaCtrl.AddUpdateCMBodyTexFlags(inpBase: false, base.SNo == 0, 1 == base.SNo, inpSunburn: false);
				base.chaCtrl.CreateBodyTexture();
			}
		};
		csPaintColor.actUpdateColor = delegate(Color color)
		{
			base.body.paintInfo[base.SNo].color = color;
			base.chaCtrl.AddUpdateCMBodyColorFlags(inpBase: false, base.SNo == 0, 1 == base.SNo, inpSunburn: false);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintGloss.onChange = delegate(float value)
		{
			base.body.paintInfo[base.SNo].glossPower = value;
			base.chaCtrl.AddUpdateCMBodyGlossFlags(base.SNo == 0, 1 == base.SNo);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintGloss.onSetDefaultValue = () => base.defChaCtrl.custom.body.paintInfo[base.SNo].glossPower;
		ssPaintMetallic.onChange = delegate(float value)
		{
			base.body.paintInfo[base.SNo].metallicPower = value;
			base.chaCtrl.AddUpdateCMBodyGlossFlags(base.SNo == 0, 1 == base.SNo);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintMetallic.onSetDefaultValue = () => base.defChaCtrl.custom.body.paintInfo[base.SNo].metallicPower;
		List<CustomSelectInfo> lst2 = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.bodypaint_layout);
		sscPaintLayout.CreateList(lst2);
		sscPaintLayout.SetToggleID(base.body.paintInfo[base.SNo].layoutId);
		sscPaintLayout.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.body.paintInfo[base.SNo].layoutId != info.id)
			{
				base.body.paintInfo[base.SNo].layoutId = info.id;
				base.chaCtrl.AddUpdateCMBodyLayoutFlags(base.SNo == 0, 1 == base.SNo);
				base.chaCtrl.CreateBodyTexture();
			}
		};
		ssPaintW.onChange = delegate(float value)
		{
			base.body.paintInfo[base.SNo].layout = new Vector4(value, base.body.paintInfo[base.SNo].layout.y, base.body.paintInfo[base.SNo].layout.z, base.body.paintInfo[base.SNo].layout.w);
			base.chaCtrl.AddUpdateCMBodyLayoutFlags(base.SNo == 0, 1 == base.SNo);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintW.onSetDefaultValue = () => base.defChaCtrl.custom.body.paintInfo[base.SNo].layout.x;
		ssPaintH.onChange = delegate(float value)
		{
			base.body.paintInfo[base.SNo].layout = new Vector4(base.body.paintInfo[base.SNo].layout.x, value, base.body.paintInfo[base.SNo].layout.z, base.body.paintInfo[base.SNo].layout.w);
			base.chaCtrl.AddUpdateCMBodyLayoutFlags(base.SNo == 0, 1 == base.SNo);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintH.onSetDefaultValue = () => base.defChaCtrl.custom.body.paintInfo[base.SNo].layout.y;
		ssPaintX.onChange = delegate(float value)
		{
			base.body.paintInfo[base.SNo].layout = new Vector4(base.body.paintInfo[base.SNo].layout.x, base.body.paintInfo[base.SNo].layout.y, value, base.body.paintInfo[base.SNo].layout.w);
			base.chaCtrl.AddUpdateCMBodyLayoutFlags(base.SNo == 0, 1 == base.SNo);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintX.onSetDefaultValue = () => base.defChaCtrl.custom.body.paintInfo[base.SNo].layout.z;
		ssPaintY.onChange = delegate(float value)
		{
			base.body.paintInfo[base.SNo].layout = new Vector4(base.body.paintInfo[base.SNo].layout.x, base.body.paintInfo[base.SNo].layout.y, base.body.paintInfo[base.SNo].layout.z, value);
			base.chaCtrl.AddUpdateCMBodyLayoutFlags(base.SNo == 0, 1 == base.SNo);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintY.onSetDefaultValue = () => base.defChaCtrl.custom.body.paintInfo[base.SNo].layout.w;
		ssPaintRot.onChange = delegate(float value)
		{
			base.body.paintInfo[base.SNo].rotation = value;
			base.chaCtrl.AddUpdateCMBodyLayoutFlags(base.SNo == 0, 1 == base.SNo);
			base.chaCtrl.CreateBodyTexture();
		};
		ssPaintRot.onSetDefaultValue = () => base.defChaCtrl.custom.body.paintInfo[base.SNo].rotation;
		StartCoroutine(SetInputText());
	}
}
