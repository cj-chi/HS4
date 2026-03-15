using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_MakeupPaint : CvsBase
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
	private CustomPushScrollController pscPaintLayout;

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
		ssPaintGloss.SetSliderValue(base.makeup.paintInfo[base.SNo].glossPower);
		ssPaintMetallic.SetSliderValue(base.makeup.paintInfo[base.SNo].metallicPower);
		ssPaintW.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.x);
		ssPaintH.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.y);
		ssPaintX.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.z);
		ssPaintY.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.w);
		ssPaintRot.SetSliderValue(base.makeup.paintInfo[base.SNo].rotation);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscPaintType.SetToggleID(base.makeup.paintInfo[base.SNo].id);
		csPaintColor.SetColor(base.makeup.paintInfo[base.SNo].color);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssPaintGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.paintInfo[base.SNo].glossPower));
		ssPaintMetallic.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.paintInfo[base.SNo].metallicPower));
		ssPaintW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.paintInfo[base.SNo].layout.x));
		ssPaintH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.paintInfo[base.SNo].layout.y));
		ssPaintX.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.paintInfo[base.SNo].layout.z));
		ssPaintY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.paintInfo[base.SNo].layout.w));
		ssPaintRot.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.makeup.paintInfo[base.SNo].rotation));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFacePaint += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_paint);
		sscPaintType.CreateList(lst);
		sscPaintType.SetToggleID(base.makeup.paintInfo[base.SNo].id);
		sscPaintType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.makeup.paintInfo[base.SNo].id != info.id)
			{
				base.makeup.paintInfo[base.SNo].id = info.id;
				base.chaCtrl.AddUpdateCMFaceTexFlags(inpBase: false, inpEyeshadow: false, base.SNo == 0, 1 == base.SNo, inpCheek: false, inpLip: false, inpMole: false);
				base.chaCtrl.CreateFaceTexture();
			}
		};
		csPaintColor.actUpdateColor = delegate(Color color)
		{
			base.makeup.paintInfo[base.SNo].color = color;
			base.chaCtrl.AddUpdateCMFaceColorFlags(inpBase: false, inpEyeshadow: false, base.SNo == 0, 1 == base.SNo, inpCheek: false, inpLip: false, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintGloss.onChange = delegate(float value)
		{
			base.makeup.paintInfo[base.SNo].glossPower = value;
			base.chaCtrl.AddUpdateCMFaceGlossFlags(inpEyeshadow: false, base.SNo == 0, 1 == base.SNo, inpCheek: false, inpLip: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintGloss.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.paintInfo[base.SNo].glossPower;
		ssPaintMetallic.onChange = delegate(float value)
		{
			base.makeup.paintInfo[base.SNo].metallicPower = value;
			base.chaCtrl.AddUpdateCMFaceGlossFlags(inpEyeshadow: false, base.SNo == 0, 1 == base.SNo, inpCheek: false, inpLip: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintMetallic.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.paintInfo[base.SNo].metallicPower;
		Dictionary<int, ListInfoBase> categoryInfo = base.lstCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.facepaint_layout);
		dictPaintLayout = categoryInfo.Select((KeyValuePair<int, ListInfoBase> val, int idx) => new
		{
			idx = idx,
			x = val.Value.GetInfoFloat(ChaListDefine.KeyType.Scale),
			y = val.Value.GetInfoFloat(ChaListDefine.KeyType.Scale),
			z = val.Value.GetInfoFloat(ChaListDefine.KeyType.PosX),
			w = val.Value.GetInfoFloat(ChaListDefine.KeyType.PosY)
		}).ToDictionary(v => v.idx, v => new Vector4
		{
			x = v.x,
			y = v.y,
			z = v.z,
			w = v.w
		});
		List<CustomPushInfo> lst2 = CvsBase.CreatePushList(ChaListDefine.CategoryNo.facepaint_layout);
		pscPaintLayout.CreateList(lst2);
		pscPaintLayout.onPush = delegate(CustomPushInfo info)
		{
			if (info != null && dictPaintLayout.TryGetValue(info.id, out var value))
			{
				base.makeup.paintInfo[base.SNo].layout = value;
				base.chaCtrl.AddUpdateCMFaceLayoutFlags(base.SNo == 0, 1 == base.SNo, inpMole: false);
				base.chaCtrl.CreateFaceTexture();
				ssPaintW.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.x);
				ssPaintH.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.y);
				ssPaintX.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.z);
				ssPaintY.SetSliderValue(base.makeup.paintInfo[base.SNo].layout.w);
			}
		};
		ssPaintW.onChange = delegate(float value)
		{
			base.makeup.paintInfo[base.SNo].layout = new Vector4(value, base.makeup.paintInfo[base.SNo].layout.y, base.makeup.paintInfo[base.SNo].layout.z, base.makeup.paintInfo[base.SNo].layout.w);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(base.SNo == 0, 1 == base.SNo, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintW.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.paintInfo[base.SNo].layout.x;
		ssPaintH.onChange = delegate(float value)
		{
			base.makeup.paintInfo[base.SNo].layout = new Vector4(base.makeup.paintInfo[base.SNo].layout.x, value, base.makeup.paintInfo[base.SNo].layout.z, base.makeup.paintInfo[base.SNo].layout.w);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(base.SNo == 0, 1 == base.SNo, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintH.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.paintInfo[base.SNo].layout.y;
		ssPaintX.onChange = delegate(float value)
		{
			base.makeup.paintInfo[base.SNo].layout = new Vector4(base.makeup.paintInfo[base.SNo].layout.x, base.makeup.paintInfo[base.SNo].layout.y, value, base.makeup.paintInfo[base.SNo].layout.w);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(base.SNo == 0, 1 == base.SNo, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintX.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.paintInfo[base.SNo].layout.z;
		ssPaintY.onChange = delegate(float value)
		{
			base.makeup.paintInfo[base.SNo].layout = new Vector4(base.makeup.paintInfo[base.SNo].layout.x, base.makeup.paintInfo[base.SNo].layout.y, base.makeup.paintInfo[base.SNo].layout.z, value);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(base.SNo == 0, 1 == base.SNo, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintY.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.paintInfo[base.SNo].layout.w;
		ssPaintRot.onChange = delegate(float value)
		{
			base.makeup.paintInfo[base.SNo].rotation = value;
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(base.SNo == 0, 1 == base.SNo, inpMole: false);
			base.chaCtrl.CreateFaceTexture();
		};
		ssPaintRot.onSetDefaultValue = () => base.defChaCtrl.custom.face.makeup.paintInfo[base.SNo].rotation;
		StartCoroutine(SetInputText());
	}
}
