using System.Linq;
using Illusion.Game;
using UniRx;
using UnityEngine;

namespace CharaCustom;

public class CustomChangeMainMenu : UI_ToggleGroupCtrl
{
	[SerializeField]
	private CanvasGroup cvgClothesSave;

	[SerializeField]
	private CanvasGroup cvgClothesLoad;

	[SerializeField]
	private CanvasGroup cvgCharaSave;

	[SerializeField]
	private CanvasGroup cvgCharaLoad;

	[SerializeField]
	private CvsA_Slot cvsA_Slot;

	[SerializeField]
	private CvsH_Hair cvsH_Hair;

	protected CustomBase customBase => Singleton<CustomBase>.Instance;

	public override void Start()
	{
		base.Start();
		if (!items.Any())
		{
			return;
		}
		(from item in items.Select((ItemInfo val, int idx) => new { val, idx })
			where item.val != null && item.val.tglItem != null
			select item).ToList().ForEach(item =>
		{
			(from isOn in item.val.tglItem.OnValueChangedAsObservable().Skip(1)
				where isOn
				select isOn).Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
				ChangeWindowSetting(item.idx);
			});
		});
	}

	public bool IsSelectAccessory()
	{
		return items[4].tglItem.isOn;
	}

	public void ChangeWindowSetting(int no)
	{
		switch (no)
		{
		case 0:
			customBase.ChangeClothesStateAuto(0);
			customBase.customCtrl.showColorCvs = false;
			customBase.customCtrl.showFileList = false;
			customBase.customCtrl.showPattern = false;
			customBase.showAcsControllerAll = false;
			customBase.showHairController = false;
			break;
		case 1:
			customBase.ChangeClothesStateAuto(2);
			customBase.customCtrl.showColorCvs = false;
			customBase.customCtrl.showFileList = false;
			customBase.customCtrl.showPattern = false;
			customBase.showAcsControllerAll = false;
			customBase.showHairController = false;
			break;
		case 2:
			customBase.ChangeClothesStateAuto(0);
			customBase.customCtrl.showColorCvs = false;
			customBase.customCtrl.showFileList = false;
			customBase.customCtrl.showPattern = false;
			customBase.showAcsControllerAll = false;
			if ((bool)cvsH_Hair && (bool)customBase.chaCtrl)
			{
				customBase.showHairController = null != customBase.chaCtrl.cmpHair[cvsH_Hair.SNo];
			}
			break;
		case 3:
			customBase.ChangeClothesStateAuto(0);
			customBase.customCtrl.showColorCvs = false;
			if (cvgClothesSave.alpha == 1f || cvgClothesLoad.alpha == 1f)
			{
				customBase.customCtrl.showFileList = true;
			}
			else
			{
				customBase.customCtrl.showFileList = false;
			}
			customBase.customCtrl.showPattern = false;
			customBase.showAcsControllerAll = false;
			customBase.showHairController = false;
			break;
		case 4:
			customBase.ChangeClothesStateAuto(0);
			customBase.customCtrl.showColorCvs = false;
			customBase.customCtrl.showFileList = false;
			customBase.customCtrl.showPattern = false;
			customBase.showHairController = false;
			if ((bool)cvsA_Slot && (bool)customBase.chaCtrl)
			{
				customBase.showAcsControllerAll = customBase.chaCtrl.IsAccessory(cvsA_Slot.SNo);
			}
			break;
		case 5:
			customBase.ChangeClothesStateAuto(0);
			customBase.customCtrl.showColorCvs = false;
			if (cvgCharaSave.alpha == 1f || cvgCharaLoad.alpha == 1f)
			{
				customBase.customCtrl.showFileList = true;
			}
			else
			{
				customBase.customCtrl.showFileList = false;
			}
			customBase.customCtrl.showPattern = false;
			customBase.showAcsControllerAll = false;
			customBase.showHairController = false;
			break;
		}
	}
}
