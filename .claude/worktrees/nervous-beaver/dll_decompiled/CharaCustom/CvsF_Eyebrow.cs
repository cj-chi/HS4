using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_Eyebrow : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscEyebrowType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csEyebrowColor;

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
		sscEyebrowType.SetToggleID(base.face.eyebrowId);
		csEyebrowColor.SetColor(base.face.eyebrowColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsEyebrow += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyebrow);
		sscEyebrowType.CreateList(lst);
		sscEyebrowType.SetToggleID(base.face.eyebrowId);
		sscEyebrowType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.eyebrowId != info.id)
			{
				base.face.eyebrowId = info.id;
				base.chaCtrl.ChangeEyebrowKind();
			}
		};
		csEyebrowColor.actUpdateColor = delegate(Color color)
		{
			base.face.eyebrowColor = color;
			base.chaCtrl.ChangeEyebrowColor();
		};
		StartCoroutine(SetInputText());
	}
}
