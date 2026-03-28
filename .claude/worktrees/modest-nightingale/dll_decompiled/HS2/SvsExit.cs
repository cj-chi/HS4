using AIChara;
using CharaCustom;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class SvsExit : MonoBehaviour
{
	[SerializeField]
	private GameObject objCol;

	[SerializeField]
	private Button btnExit;

	[SerializeField]
	private PopupCheck popupEndNew;

	private SearchBase searchBase => Singleton<SearchBase>.Instance;

	private ChaControl chaCtrl => searchBase.chaCtrl;

	private void Start()
	{
		if (!btnExit)
		{
			return;
		}
		btnExit.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			popupEndNew.actYes = delegate
			{
				ExitScene();
			};
			popupEndNew.SetupWindow();
		});
	}

	public void ExitScene()
	{
		objCol.SetActive(value: true);
		searchBase.customSettingSave.Save();
		Scene.Unload();
	}
}
