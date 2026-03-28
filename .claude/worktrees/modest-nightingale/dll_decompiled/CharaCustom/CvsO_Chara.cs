using System.Collections;
using System.Collections.Generic;
using AIChara;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsO_Chara : CvsBase
{
	[SerializeField]
	private InputField inpName;

	[SerializeField]
	private Button btnRandom;

	[SerializeField]
	private Dropdown ddBirthMonth;

	[SerializeField]
	private Dropdown ddBirthDay;

	private ChaRandomName randomName = new ChaRandomName();

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ddBirthMonth.value = base.parameter.birthMonth - 1;
		ddBirthDay.value = base.parameter.birthDay - 1;
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		inpName.text = base.parameter.fullname;
	}

	public void UpdateBirthDayDD()
	{
		int num = base.parameter.birthDay - 1;
		ddBirthDay.ClearOptions();
		int[] array = new int[12]
		{
			31, 29, 31, 30, 31, 30, 31, 31, 30, 31,
			30, 31
		};
		List<string> list = new List<string>();
		for (int i = 0; i < array[base.parameter.birthMonth - 1]; i++)
		{
			list.Add((i + 1).ToString());
		}
		ddBirthDay.AddOptions(list);
		if (num > array[base.parameter.birthMonth - 1] - 1)
		{
			ddBirthDay.value = 0;
		}
		else
		{
			ddBirthDay.value = num;
		}
		base.parameter.birthDay = (byte)(ddBirthDay.value + 1);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		UpdateBirthDayDD();
		base.customBase.lstInputField.Add(inpName);
		base.customBase.actUpdateCvsChara += UpdateCustomUI;
		if ((bool)inpName)
		{
			inpName.ActivateInputField();
		}
		if ((bool)inpName)
		{
			inpName.OnEndEditAsObservable().Subscribe(delegate(string str)
			{
				base.parameter.fullname = str;
				base.customBase.changeCharaName = true;
			});
		}
		randomName.Initialize();
		if ((bool)btnRandom)
		{
			btnRandom.OnClickAsObservable().Subscribe(delegate
			{
				inpName.text = randomName.GetRandName(base.chaCtrl.sex);
				base.parameter.fullname = inpName.text;
				base.customBase.changeCharaName = true;
			});
		}
		ddBirthMonth.onValueChanged.AddListener(delegate(int idx)
		{
			if (!base.customBase.updateCustomUI && base.parameter.birthMonth != (byte)(idx + 1))
			{
				base.parameter.birthMonth = (byte)(idx + 1);
				UpdateBirthDayDD();
			}
		});
		ddBirthDay.onValueChanged.AddListener(delegate(int idx)
		{
			if (!base.customBase.updateCustomUI && base.parameter.birthDay != (byte)(idx + 1))
			{
				base.parameter.birthDay = (byte)(idx + 1);
			}
		});
	}
}
