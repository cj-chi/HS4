using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsB_Sunburn : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscSunburnType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csSunburnColor;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscSunburnType.SetToggleID(base.body.sunburnId);
		csSunburnColor.SetColor(base.body.sunburnColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsSunburn += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList((base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mt_sunburn : ChaListDefine.CategoryNo.ft_sunburn);
		sscSunburnType.CreateList(lst);
		sscSunburnType.SetToggleID(base.body.sunburnId);
		sscSunburnType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.body.sunburnId != info.id)
			{
				base.body.sunburnId = info.id;
				base.chaCtrl.AddUpdateCMBodyTexFlags(inpBase: false, inpPaint01: false, inpPaint02: false, inpSunburn: true);
				base.chaCtrl.CreateBodyTexture();
			}
		};
		csSunburnColor.actUpdateColor = delegate(Color color)
		{
			base.body.sunburnColor = color;
			base.chaCtrl.AddUpdateCMBodyColorFlags(inpBase: false, inpPaint01: false, inpPaint02: false, inpSunburn: true);
			base.chaCtrl.CreateBodyTexture();
		};
		StartCoroutine(SetInputText());
	}
}
