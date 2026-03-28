using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_Beard : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscBeardType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csBeardColor;

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
		sscBeardType.SetToggleID(base.face.beardId);
		csBeardColor.SetColor(base.face.beardColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsBeard += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.mt_beard);
		sscBeardType.CreateList(lst);
		sscBeardType.SetToggleID(base.face.beardId);
		sscBeardType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.beardId != info.id)
			{
				base.face.beardId = info.id;
				base.chaCtrl.ChangeBeardKind();
			}
		};
		csBeardColor.actUpdateColor = delegate(Color color)
		{
			base.face.beardColor = color;
			base.chaCtrl.ChangeBeardColor();
		};
		StartCoroutine(SetInputText());
	}
}
