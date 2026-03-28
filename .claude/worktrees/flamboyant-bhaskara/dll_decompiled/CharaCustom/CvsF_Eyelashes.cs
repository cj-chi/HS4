using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_Eyelashes : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscEyelashesType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csEyelashesColor;

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
		sscEyelashesType.SetToggleID(base.face.eyelashesId);
		csEyelashesColor.SetColor(base.face.eyelashesColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsEyelashes += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyelash);
		sscEyelashesType.CreateList(lst);
		sscEyelashesType.SetToggleID(base.face.eyelashesId);
		sscEyelashesType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.eyelashesId != info.id)
			{
				base.face.eyelashesId = info.id;
				base.chaCtrl.ChangeEyelashesKind();
			}
		};
		csEyelashesColor.actUpdateColor = delegate(Color color)
		{
			base.face.eyelashesColor = color;
			base.chaCtrl.ChangeEyelashesColor();
		};
		StartCoroutine(SetInputText());
	}
}
