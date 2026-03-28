using AIChara;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsExit : MonoBehaviour
{
	[SerializeField]
	private Button btnExit;

	[SerializeField]
	private PopupCheck popupEndNew;

	[SerializeField]
	private PopupCheck popupEndEdit;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	private ChaControl chaCtrl => customBase.chaCtrl;

	private void Start()
	{
		if (!btnExit)
		{
			return;
		}
		btnExit.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			if (customBase.modeNew)
			{
				popupEndNew.actYes = delegate
				{
					ExitScene(saveChara: false);
				};
				popupEndNew.SetupWindow();
			}
			else
			{
				popupEndEdit.actYes = delegate
				{
					ExitScene(saveChara: true);
				};
				popupEndEdit.actYes2 = delegate
				{
					ExitScene(saveChara: false);
				};
				popupEndEdit.SetupWindow();
			}
		});
	}

	public void ExitScene(bool saveChara)
	{
		if (saveChara)
		{
			if (!customBase.modeNew)
			{
				chaCtrl.chaFile.CopyGameInfo2(customBase.editBackChaCtrl.gameinfo2);
			}
			chaCtrl.chaFile.SaveCharaFile(customBase.editSaveFileName);
		}
		customBase.customCtrl.cvsChangeScene.gameObject.SetActive(value: true);
		customBase.customSettingSave.Save();
		if (customBase.nextSceneName.IsNullOrEmpty())
		{
			Scene.Unload();
			return;
		}
		Scene.LoadReserve(new Scene.Data
		{
			levelName = customBase.nextSceneName,
			isAdd = false,
			isFade = true
		}, isLoadingImageDraw: true);
	}
}
