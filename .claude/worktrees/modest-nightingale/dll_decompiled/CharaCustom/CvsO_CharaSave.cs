using System.Collections;
using System.IO;
using Illusion.Game;
using Manager;
using UnityEngine;

namespace CharaCustom;

public class CvsO_CharaSave : CvsBase
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
		charaLoadWin.UpdateWindow(base.customBase.modeNew, base.customBase.modeSex, save: true);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsCharaSaveDelete += UpdateCharasList;
		UpdateCharasList();
		charaLoadWin.btnDisableNotSelect01 = true;
		charaLoadWin.btnDisableNotSelect02 = false;
		charaLoadWin.btnDisableNotSelect03 = true;
		charaLoadWin.onClick01 = delegate(CustomCharaFileInfo info)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			if (info.isChangeParameter)
			{
				status.Sentence = CharaCustomDefine.CustomConfirmDeleteWithIncludeParam[Singleton<GameSystem>.Instance.languageInt];
			}
			else
			{
				status.Sentence = CharaCustomDefine.CustomConfirmDelete[Singleton<GameSystem>.Instance.languageInt];
			}
			status.Yes = delegate
			{
				charaLoadWin.SelectInfoClear();
				if (File.Exists(info.FullPath))
				{
					File.Delete(info.FullPath);
				}
				base.customBase.updateCvsCharaSaveDelete = true;
				base.customBase.updateCvsCharaLoad = true;
				Singleton<Game>.Instance.saveData.RoomListCharaExists();
				Singleton<Game>.Instance.saveData.PlayerExists();
				Utils.Sound.Play(SystemSE.ok_l);
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		};
		charaLoadWin.onClick02 = delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			base.customBase.customCtrl.saveMode = true;
		};
		charaLoadWin.onClick03 = delegate(CustomCharaFileInfo info, int flags)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			if (info.isChangeParameter)
			{
				status.Sentence = CharaCustomDefine.CustomConfirmOverwriteWithInitializeParam[Singleton<GameSystem>.Instance.languageInt];
			}
			else
			{
				status.Sentence = CharaCustomDefine.CustomConfirmOverwrite[Singleton<GameSystem>.Instance.languageInt];
			}
			status.Yes = delegate
			{
				base.customBase.customCtrl.overwriteSavePath = info.FullPath;
				base.customBase.customCtrl.saveMode = true;
				Utils.Sound.Play(SystemSE.ok_l);
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		};
	}
}
