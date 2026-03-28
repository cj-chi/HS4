using System.Collections;
using System.IO;
using Illusion.Game;
using Manager;
using UnityEngine;

namespace CharaCustom;

public class CvsC_ClothesSave : CvsBase
{
	[SerializeField]
	private CustomClothesWindow clothesLoadWin;

	[SerializeField]
	private CvsC_ClothesInput clothesNameInput;

	[SerializeField]
	private CvsC_CreateCoordinateFile createCoordinateFile;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = true;
	}

	public void UpdateClothesList()
	{
		clothesLoadWin.UpdateWindow(base.customBase.modeNew, base.customBase.modeSex, save: true);
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsClothesSaveDelete += UpdateClothesList;
		UpdateClothesList();
		clothesLoadWin.btnDisableNotSelect01 = true;
		clothesLoadWin.btnDisableNotSelect02 = false;
		clothesLoadWin.btnDisableNotSelect03 = true;
		clothesLoadWin.onClick01 = delegate(CustomClothesFileInfo info)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = CharaCustomDefine.CustomConfirmDelete[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				clothesLoadWin.SelectInfoClear();
				if (File.Exists(info.FullPath))
				{
					File.Delete(info.FullPath);
				}
				base.customBase.updateCvsClothesSaveDelete = true;
				base.customBase.updateCvsClothesLoad = true;
				Singleton<Game>.Instance.saveData.PlayerCoordinateExists();
				Utils.Sound.Play(SystemSE.ok_l);
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		};
		clothesLoadWin.onClick02 = delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			if (null != clothesNameInput)
			{
				clothesNameInput.SetupInputCoordinateNameWindow("");
				clothesNameInput.actEntry = delegate(string buf)
				{
					createCoordinateFile.CreateCoordinateFile("", buf, _overwrite: false);
				};
			}
		};
		clothesLoadWin.onClick03 = delegate(CustomClothesFileInfo info)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = CharaCustomDefine.CustomConfirmOverwrite[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				Utils.Sound.Play(SystemSE.ok_l);
				if (null != clothesNameInput)
				{
					clothesNameInput.SetupInputCoordinateNameWindow(info.name);
					clothesNameInput.actEntry = delegate(string buf)
					{
						createCoordinateFile.CreateCoordinateFile(info.FullPath, buf, _overwrite: true);
					};
				}
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		};
	}
}
