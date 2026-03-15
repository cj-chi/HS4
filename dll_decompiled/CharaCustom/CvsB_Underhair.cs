using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsB_Underhair : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscUnderhairType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csUnderhairColor;

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
		sscUnderhairType.SetToggleID(base.body.underhairId);
		csUnderhairColor.SetColor(base.body.underhairColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsUnderhair += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_underhair);
		sscUnderhairType.CreateList(lst);
		sscUnderhairType.SetToggleID(base.body.underhairId);
		sscUnderhairType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.body.underhairId != info.id)
			{
				base.body.underhairId = info.id;
				base.chaCtrl.ChangeUnderHairKind();
			}
		};
		csUnderhairColor.actUpdateColor = delegate(Color color)
		{
			base.body.underhairColor = color;
			base.chaCtrl.ChangeUnderHairColor();
		};
		StartCoroutine(SetInputText());
	}
}
