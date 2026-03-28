using System.Collections;
using System.Linq;
using AIChara;
using Illusion.Game;
using Manager;
using MessagePack;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsA_Copy : CvsBase
{
	[SerializeField]
	private UI_ToggleEx[] tglSrc;

	[SerializeField]
	private Text[] textSrc;

	[SerializeField]
	private UI_ToggleEx[] tglDst;

	[SerializeField]
	private Text[] textDst;

	[SerializeField]
	private Toggle tglChgParentLR;

	[SerializeField]
	private Button btnCopySlot;

	[SerializeField]
	private Button btnCopy01;

	[SerializeField]
	private Button btnCopy02;

	[SerializeField]
	private Button btnRevLR01;

	[SerializeField]
	private Button btnRevLR02;

	[SerializeField]
	private Button btnRevTB01;

	[SerializeField]
	private Button btnRevTB02;

	private int selSrc;

	private int selDst;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	public void CalculateUI()
	{
		for (int i = 0; i < 20; i++)
		{
			ListInfoBase listInfo = base.chaCtrl.lstCtrl.GetListInfo((ChaListDefine.CategoryNo)base.nowAcs.parts[i].type, base.nowAcs.parts[i].id);
			if (listInfo == null)
			{
				textDst[i].text = CharaCustomDefine.CustomNoneStr[Singleton<GameSystem>.Instance.languageInt];
				textSrc[i].text = CharaCustomDefine.CustomNoneStr[Singleton<GameSystem>.Instance.languageInt];
			}
			else
			{
				TextCorrectLimit.Correct(textDst[i], listInfo.Name, "…");
				textSrc[i].text = textDst[i].text;
			}
		}
	}

	public override void UpdateCustomUI()
	{
		CalculateUI();
	}

	private void CopyAccessory()
	{
		byte[] bytes = MessagePackSerializer.Serialize(base.nowAcs.parts[selSrc]);
		base.nowAcs.parts[selDst] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);
		if (tglChgParentLR.isOn)
		{
			string reverseParent = ChaAccessoryDefine.GetReverseParent(base.nowAcs.parts[selDst].parentKey);
			if ("" != reverseParent)
			{
				base.nowAcs.parts[selDst].parentKey = reverseParent;
			}
		}
		base.chaCtrl.AssignCoordinate();
		Singleton<Character>.Instance.customLoadGCClear = false;
		base.chaCtrl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
		Singleton<Character>.Instance.customLoadGCClear = true;
		CalculateUI();
		base.customBase.ChangeAcsSlotName();
		base.customBase.forceUpdateAcsList = true;
		base.customBase.updateCvsAccessory = true;
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsAcsCopy += UpdateCustomUI;
		tglDst[selDst].isOn = true;
		tglSrc[selSrc].isOn = true;
		tglSrc.Select((UI_ToggleEx p, int index) => new
		{
			tgl = p,
			index = index
		}).ToList().ForEach(p =>
		{
			(from isOn in p.tgl.OnValueChangedAsObservable()
				where isOn
				select isOn).Subscribe(delegate
			{
				selSrc = p.index;
			});
		});
		tglDst.Select((UI_ToggleEx p, int index) => new
		{
			tgl = p,
			index = index
		}).ToList().ForEach(p =>
		{
			(from isOn in p.tgl.OnValueChangedAsObservable()
				where isOn
				select isOn).Subscribe(delegate
			{
				selDst = p.index;
			});
		});
		btnCopySlot.OnClickAsObservable().Subscribe(delegate
		{
			CopyAccessory();
			Utils.Sound.Play(SystemSE.ok_s);
		});
		btnCopySlot.UpdateAsObservable().Subscribe(delegate
		{
			btnCopySlot.interactable = selSrc != selDst;
		});
		btnCopy01.OnClickAsObservable().Subscribe(delegate
		{
			for (int i = 0; i < 3; i++)
			{
				base.nowAcs.parts[selDst].addMove[0, i] = (base.orgAcs.parts[selDst].addMove[0, i] = base.nowAcs.parts[selSrc].addMove[0, i]);
			}
			base.chaCtrl.UpdateAccessoryMoveFromInfo(selDst);
			base.customBase.updateCvsAccessory = true;
			Utils.Sound.Play(SystemSE.ok_s);
		});
		btnCopy01.UpdateAsObservable().Subscribe(delegate
		{
			bool flag = true;
			flag &= null != base.chaCtrl.cmpAccessory[selDst] && null != base.chaCtrl.cmpAccessory[selDst].trfMove01;
			flag &= null != base.chaCtrl.cmpAccessory[selSrc] && null != base.chaCtrl.cmpAccessory[selSrc].trfMove01;
			flag &= selSrc != selDst;
			btnCopy01.interactable = flag;
		});
		btnCopy02.OnClickAsObservable().Subscribe(delegate
		{
			for (int i = 0; i < 3; i++)
			{
				base.nowAcs.parts[selDst].addMove[1, i] = (base.orgAcs.parts[selDst].addMove[1, i] = base.nowAcs.parts[selSrc].addMove[1, i]);
			}
			base.chaCtrl.UpdateAccessoryMoveFromInfo(selDst);
			base.customBase.updateCvsAccessory = true;
			Utils.Sound.Play(SystemSE.ok_s);
		});
		btnCopy02.UpdateAsObservable().Subscribe(delegate
		{
			bool flag = true;
			flag &= null != base.chaCtrl.cmpAccessory[selDst] && null != base.chaCtrl.cmpAccessory[selDst].trfMove02;
			flag &= null != base.chaCtrl.cmpAccessory[selSrc] && null != base.chaCtrl.cmpAccessory[selSrc].trfMove02;
			flag &= selSrc != selDst;
			btnCopy02.interactable = flag;
		});
		btnRevLR01.OnClickAsObservable().Subscribe(delegate
		{
			for (int i = 0; i < 3; i++)
			{
				Vector3 vector = base.nowAcs.parts[selSrc].addMove[0, i];
				if (1 == i)
				{
					vector.y += 180f;
					if (vector.y >= 360f)
					{
						vector.y -= 360f;
					}
				}
				base.nowAcs.parts[selDst].addMove[0, i] = (base.orgAcs.parts[selDst].addMove[0, i] = vector);
			}
			base.chaCtrl.UpdateAccessoryMoveFromInfo(selDst);
			base.customBase.updateCvsAccessory = true;
			Utils.Sound.Play(SystemSE.ok_s);
		});
		btnRevLR01.UpdateAsObservable().Subscribe(delegate
		{
			bool flag = true;
			flag &= null != base.chaCtrl.cmpAccessory[selDst] && null != base.chaCtrl.cmpAccessory[selDst].trfMove01;
			flag &= null != base.chaCtrl.cmpAccessory[selSrc] && null != base.chaCtrl.cmpAccessory[selSrc].trfMove01;
			flag &= selSrc != selDst;
			btnRevLR01.interactable = flag;
		});
		btnRevLR02.OnClickAsObservable().Subscribe(delegate
		{
			for (int i = 0; i < 3; i++)
			{
				Vector3 vector = base.nowAcs.parts[selSrc].addMove[1, i];
				if (1 == i)
				{
					vector.y += 180f;
					if (vector.y >= 360f)
					{
						vector.y -= 360f;
					}
				}
				base.nowAcs.parts[selDst].addMove[1, i] = (base.orgAcs.parts[selDst].addMove[1, i] = vector);
			}
			base.chaCtrl.UpdateAccessoryMoveFromInfo(selDst);
			base.customBase.updateCvsAccessory = true;
			Utils.Sound.Play(SystemSE.ok_s);
		});
		btnRevLR02.UpdateAsObservable().Subscribe(delegate
		{
			bool flag = true;
			flag &= null != base.chaCtrl.cmpAccessory[selDst] && null != base.chaCtrl.cmpAccessory[selDst].trfMove02;
			flag &= null != base.chaCtrl.cmpAccessory[selSrc] && null != base.chaCtrl.cmpAccessory[selSrc].trfMove02;
			flag &= selSrc != selDst;
			btnRevLR02.interactable = flag;
		});
		btnRevTB01.OnClickAsObservable().Subscribe(delegate
		{
			for (int i = 0; i < 3; i++)
			{
				Vector3 vector = base.nowAcs.parts[selSrc].addMove[0, i];
				if (1 == i)
				{
					vector.x += 180f;
					if (vector.x >= 360f)
					{
						vector.x -= 360f;
					}
				}
				base.nowAcs.parts[selDst].addMove[0, i] = (base.orgAcs.parts[selDst].addMove[0, i] = vector);
			}
			base.chaCtrl.UpdateAccessoryMoveFromInfo(selDst);
			base.customBase.updateCvsAccessory = true;
			Utils.Sound.Play(SystemSE.ok_s);
		});
		btnRevTB01.UpdateAsObservable().Subscribe(delegate
		{
			bool flag = true;
			flag &= null != base.chaCtrl.cmpAccessory[selDst] && null != base.chaCtrl.cmpAccessory[selDst].trfMove01;
			flag &= null != base.chaCtrl.cmpAccessory[selSrc] && null != base.chaCtrl.cmpAccessory[selSrc].trfMove01;
			flag &= selSrc != selDst;
			btnRevTB01.interactable = flag;
		});
		btnRevTB02.OnClickAsObservable().Subscribe(delegate
		{
			for (int i = 0; i < 3; i++)
			{
				Vector3 vector = base.nowAcs.parts[selSrc].addMove[1, i];
				if (1 == i)
				{
					vector.x += 180f;
					if (vector.x >= 360f)
					{
						vector.x -= 360f;
					}
				}
				base.nowAcs.parts[selDst].addMove[1, i] = (base.orgAcs.parts[selDst].addMove[1, i] = vector);
			}
			base.chaCtrl.UpdateAccessoryMoveFromInfo(selDst);
			base.customBase.updateCvsAccessory = true;
			Utils.Sound.Play(SystemSE.ok_s);
		});
		btnRevTB02.UpdateAsObservable().Subscribe(delegate
		{
			bool flag = true;
			flag &= null != base.chaCtrl.cmpAccessory[selDst] && null != base.chaCtrl.cmpAccessory[selDst].trfMove02;
			flag &= null != base.chaCtrl.cmpAccessory[selSrc] && null != base.chaCtrl.cmpAccessory[selSrc].trfMove02;
			flag &= selSrc != selDst;
			btnRevTB02.interactable = flag;
		});
	}
}
