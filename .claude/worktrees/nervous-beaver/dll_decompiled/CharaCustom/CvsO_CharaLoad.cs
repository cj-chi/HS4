using System.Collections;
using Illusion.Game;
using Manager;
using UnityEngine;

namespace CharaCustom;

public class CvsO_CharaLoad : CvsBase
{
	[SerializeField]
	private CustomCharaWindow charaLoadWin;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = true;
	}

	public void UpdateCharasList()
	{
		charaLoadWin.UpdateWindow(base.customBase.modeNew, base.customBase.modeSex, save: false);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsCharaLoad += UpdateCharasList;
		UpdateCharasList();
		charaLoadWin.btnDisableNotSelect01 = true;
		charaLoadWin.btnDisableNotSelect02 = true;
		charaLoadWin.btnDisableNotSelect03 = true;
		charaLoadWin.onClick03 = delegate(CustomCharaFileInfo info, int flags)
		{
			bool flag = (flags & 1) != 0;
			bool flag2 = (flags & 2) != 0;
			bool flag3 = (flags & 4) != 0;
			bool flag4 = (flags & 8) != 0;
			bool flag5 = (flags & 0x10) != 0 && base.customBase.modeNew;
			string fullPath = info.FullPath;
			base.chaCtrl.chaFile.LoadFileLimited(fullPath, base.chaCtrl.sex, flag, flag2, flag3, flag5, flag4);
			base.chaCtrl.ChangeNowCoordinate();
			Singleton<Character>.Instance.customLoadGCClear = false;
			base.chaCtrl.Reload(!flag4, !flag, !flag3, !flag2);
			Singleton<Character>.Instance.customLoadGCClear = true;
			base.customBase.updateCustomUI = true;
			for (int i = 0; i < 20; i++)
			{
				base.customBase.ChangeAcsSlotName(i);
			}
			base.customBase.SetUpdateToggleSetting();
			base.customBase.forceUpdateAcsList = true;
			Utils.Sound.Play(SystemSE.load);
		};
	}
}
