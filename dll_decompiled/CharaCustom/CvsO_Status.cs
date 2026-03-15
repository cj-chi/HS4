using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsO_Status : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private GameObject objTop01;

	[SerializeField]
	private GameObject objTemp01;

	private Toggle[] tglType01;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private GameObject objTop02;

	[SerializeField]
	private GameObject objTemp02;

	private Toggle[] tglType02;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private GameObject objTop03;

	[SerializeField]
	private GameObject objTemp03;

	private Toggle[] tglType03;

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
		int num = 0;
		if (Game.infoTraitTable != null && Game.infoTraitTable.ContainsKey(base.parameter2.trait))
		{
			num = base.parameter2.trait;
		}
		tglType01[num].SetIsOnWithoutCallback(isOn: true);
		for (int i = 0; i < tglType01.Length; i++)
		{
			if (i != num)
			{
				tglType01[i].SetIsOnWithoutCallback(isOn: false);
			}
		}
		num = 0;
		if (Game.infoMindTable != null && Game.infoMindTable.ContainsKey(base.parameter2.mind))
		{
			num = base.parameter2.mind;
		}
		tglType02[num].SetIsOnWithoutCallback(isOn: true);
		for (int j = 0; j < tglType02.Length; j++)
		{
			if (j != num)
			{
				tglType02[j].SetIsOnWithoutCallback(isOn: false);
			}
		}
		num = 0;
		if (Game.infoHAttributeTable != null && Game.infoHAttributeTable.ContainsKey(base.parameter2.hAttribute))
		{
			num = base.parameter2.hAttribute;
		}
		tglType03[num].SetIsOnWithoutCallback(isOn: true);
		for (int k = 0; k < tglType03.Length; k++)
		{
			if (k != num)
			{
				tglType03[k].SetIsOnWithoutCallback(isOn: false);
			}
		}
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsStatus += UpdateCustomUI;
		tglType01 = new Toggle[Game.infoTraitTable.Keys.Count()];
		foreach (var item in Game.infoTraitTable.Select((KeyValuePair<int, string> val, int idx) => new { val, idx }))
		{
			GameObject gameObject = Object.Instantiate(objTemp01);
			gameObject.name = "tglRbSel_" + item.idx.ToString("00");
			tglType01[item.idx] = gameObject.GetComponent<Toggle>();
			ToggleGroup component = objTop01.GetComponent<ToggleGroup>();
			tglType01[item.idx].group = component;
			gameObject.transform.SetParent(objTop01.transform, worldPositionStays: false);
			Transform transform = gameObject.transform.Find("textRbSelect");
			if (null != transform)
			{
				Text component2 = transform.GetComponent<Text>();
				if ((bool)component2)
				{
					component2.text = item.val.Value;
				}
			}
			gameObject.SetActiveIfDifferent(active: true);
		}
		tglType01.Select((Toggle p, int idx) => new
		{
			toggle = p,
			index = (byte)idx
		}).ToList().ForEach(p =>
		{
			p.toggle.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
			{
				if (!base.customBase.updateCustomUI && isOn)
				{
					int[] array = Game.infoTraitTable.Keys.ToArray();
					base.parameter2.trait = (byte)array[p.index];
				}
			});
		});
		tglType02 = new Toggle[Game.infoMindTable.Keys.Count()];
		foreach (var item2 in Game.infoMindTable.Select((KeyValuePair<int, string> val, int idx) => new { val, idx }))
		{
			GameObject gameObject2 = Object.Instantiate(objTemp02);
			gameObject2.name = "tglRbSel_" + item2.idx.ToString("00");
			tglType02[item2.idx] = gameObject2.GetComponent<Toggle>();
			ToggleGroup component3 = objTop02.GetComponent<ToggleGroup>();
			tglType02[item2.idx].group = component3;
			gameObject2.transform.SetParent(objTop02.transform, worldPositionStays: false);
			Transform transform2 = gameObject2.transform.Find("textRbSelect");
			if (null != transform2)
			{
				Text component4 = transform2.GetComponent<Text>();
				if ((bool)component4)
				{
					component4.text = item2.val.Value;
				}
			}
			gameObject2.SetActiveIfDifferent(active: true);
		}
		tglType02.Select((Toggle p, int idx) => new
		{
			toggle = p,
			index = (byte)idx
		}).ToList().ForEach(p =>
		{
			p.toggle.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
			{
				if (!base.customBase.updateCustomUI && isOn)
				{
					int[] array = Game.infoMindTable.Keys.ToArray();
					base.parameter2.mind = (byte)array[p.index];
				}
			});
		});
		tglType03 = new Toggle[Game.infoHAttributeTable.Keys.Count()];
		foreach (var item3 in Game.infoHAttributeTable.Select((KeyValuePair<int, string> val, int idx) => new { val, idx }))
		{
			GameObject gameObject3 = Object.Instantiate(objTemp03);
			gameObject3.name = "tglRbSel_" + item3.idx.ToString("00");
			tglType03[item3.idx] = gameObject3.GetComponent<Toggle>();
			ToggleGroup component5 = objTop03.GetComponent<ToggleGroup>();
			tglType03[item3.idx].group = component5;
			gameObject3.transform.SetParent(objTop03.transform, worldPositionStays: false);
			Transform transform3 = gameObject3.transform.Find("textRbSelect");
			if (null != transform3)
			{
				Text component6 = transform3.GetComponent<Text>();
				if ((bool)component6)
				{
					component6.text = item3.val.Value;
				}
			}
			gameObject3.SetActiveIfDifferent(active: true);
		}
		tglType03.Select((Toggle p, int idx) => new
		{
			toggle = p,
			index = (byte)idx
		}).ToList().ForEach(p =>
		{
			p.toggle.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
			{
				if (!base.customBase.updateCustomUI && isOn)
				{
					int[] array = Game.infoHAttributeTable.Keys.ToArray();
					base.parameter2.hAttribute = (byte)array[p.index];
				}
			});
		});
	}
}
