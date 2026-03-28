using System.Linq;
using Config;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsO_SubmenuEx : MonoBehaviour
{
	[SerializeField]
	private CvsO_Fusion coFusion;

	[SerializeField]
	private Button btnFusion;

	[SerializeField]
	private Button btnConfig;

	[SerializeField]
	private Button btnDrawMenu;

	[SerializeField]
	private Button btnDefaultLayout;

	[SerializeField]
	private Button btnUpdatePng;

	[SerializeField]
	private CustomUIDrag[] customUIDrags;

	protected CustomBase customBase => Singleton<CustomBase>.Instance;

	private void Start()
	{
		if ((bool)btnFusion)
		{
			btnFusion.OnClickAsObservable().Subscribe(delegate
			{
				coFusion.UpdateCharasList();
				customBase.customCtrl.showFusionCvs = true;
			});
		}
		if ((bool)btnConfig)
		{
			btnConfig.OnClickAsObservable().Subscribe(delegate
			{
				if (!Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog))
				{
					ConfigWindow.Load();
					customBase.customCtrl.showFileList = false;
					Utils.Sound.Play(SystemSE.ok_s);
				}
			});
		}
		if ((bool)btnDrawMenu)
		{
			btnDrawMenu.OnClickAsObservable().Subscribe(delegate
			{
				customBase.customCtrl.showDrawMenu = true;
			});
		}
		if ((bool)btnDefaultLayout)
		{
			btnDefaultLayout.OnClickAsObservable().Subscribe(delegate
			{
				customBase.customSettingSave.ResetWinLayout();
				if (customUIDrags != null)
				{
					for (int i = 0; i < customUIDrags.Length; i++)
					{
						customUIDrags[i].UpdatePosition();
					}
				}
			});
		}
		if ((bool)btnUpdatePng)
		{
			btnUpdatePng.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
				customBase.customCtrl.updatePng = true;
			});
		}
	}
}
