using System.Collections;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsB_Nip : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscNipType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csNipColor;

	[SerializeField]
	private CustomSliderSet ssNipGloss;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssNipGloss.SetSliderValue(base.body.nipGlossPower);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscNipType.SetToggleID(base.body.nipId);
		csNipColor.SetColor(base.body.nipColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssNipGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.body.nipGlossPower));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsNip += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_nip);
		sscNipType.CreateList(lst);
		sscNipType.SetToggleID(base.body.nipId);
		sscNipType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.body.nipId != info.id)
			{
				base.body.nipId = info.id;
				base.chaCtrl.ChangeNipKind();
			}
		};
		csNipColor.actUpdateColor = delegate(Color color)
		{
			base.body.nipColor = color;
			base.chaCtrl.ChangeNipColor();
		};
		ssNipGloss.onChange = delegate(float value)
		{
			base.body.nipGlossPower = value;
			base.chaCtrl.ChangeNipGloss();
		};
		ssNipGloss.onSetDefaultValue = () => base.defChaCtrl.custom.body.nipGlossPower;
		StartCoroutine(SetInputText());
	}
}
