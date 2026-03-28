using System;
using System.Collections;
using System.Linq;
using Config;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class STRConfirmation1 : MonoBehaviour
{
	public class PlanInfo
	{
		public int plan;

		public string partnerCardFile;

		public string partnerName;

		public string partnerCoordinateFileName;

		public string partnerCoordinateName;

		public string playerCardFile;

		public string playerName;

		public string playerCoordinateFileName;

		public string playerCoordinateName;
	}

	[SerializeField]
	private Button btnNo;

	[SerializeField]
	private Text txtbtnNo;

	[SerializeField]
	private Button btnYes;

	[SerializeField]
	private Text txtbtnYes;

	[SerializeField]
	private Text txtPlan;

	[SerializeField]
	private RawImage riCharaCard;

	[SerializeField]
	private Text txtCharaName;

	[SerializeField]
	private RawImage riPlayerCard;

	[SerializeField]
	private Text txtPlayerName;

	[SerializeField]
	private RawImage riPartnerCoordinate;

	[SerializeField]
	private Text txtPartnerCoordinateName;

	[SerializeField]
	private RawImage riPlayerCoordinate;

	[SerializeField]
	private Text txtPlayerCoordinateName;

	[SerializeField]
	private RawImage riMap;

	[SerializeField]
	private Texture texNoSelect;

	public Action onEntry;

	public Action onCancel;

	private IEnumerator Start()
	{
		base.enabled = false;
		while (!Singleton<GameSystem>.IsInstance())
		{
			yield return null;
		}
		while (!SingletonInitializer<Scene>.initialized)
		{
			yield return null;
		}
		while (!Singleton<SpecialTreatmentRoomManager1>.IsInstance())
		{
			yield return null;
		}
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		btnNo.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			StartCoroutine(Cancel(_async: false));
		});
		btnNo.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (btnNo.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				txtbtnNo.color = Game.selectFontColor;
			}
		});
		btnNo.OnPointerExitAsObservable().Subscribe(delegate
		{
			txtbtnNo.color = Game.defaultFontColor;
		});
		btnYes.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			onEntry();
		});
		btnYes.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (btnYes.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				txtbtnYes.color = Game.selectFontColor;
			}
		});
		btnYes.OnPointerExitAsObservable().Subscribe(delegate
		{
			txtbtnYes.color = Game.defaultFontColor;
		});
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where Singleton<Game>.Instance.appendSaveData.AppendTutorialNo == -1
			where !strm.IsADV
			where !Scene.IsFadeNow
			where strm.CGModes[2].alpha > 0.9f
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) && !ConfigWindow.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) && !global::Tutorial2D.Tutorial2D.isActive
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) && !ShortcutViewDialog.isActive
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Cancel());
		});
		base.enabled = true;
	}

	private IEnumerator Cancel(bool _async = true)
	{
		SpecialTreatmentRoomManager1 strm = Singleton<SpecialTreatmentRoomManager1>.Instance;
		strm.CGBase.blocksRaycasts = false;
		if (_async)
		{
			yield return null;
		}
		onCancel?.Invoke();
		strm.StartFade();
		strm.SetModeCanvasGroup(1);
		strm.CGBase.blocksRaycasts = true;
	}

	public void SetInfo(PlanInfo _info)
	{
		SpecialTreatmentRoomManager1 instance = Singleton<SpecialTreatmentRoomManager1>.Instance;
		GameSystem instance2 = Singleton<GameSystem>.Instance;
		txtPlan.text = "";
		riMap.enabled = false;
		if (instance.dicPlanNameInfo.TryGetValue(_info.plan, out var value))
		{
			txtPlan.text = value.name[instance2.languageInt];
			riMap.enabled = true;
			riMap.texture = CommonLib.LoadAsset<Texture2D>(value.bundle, value.asset, clone: false, value.manifest);
			AssetBundleManager.UnloadAssetBundle(value.bundle, isUnloadForceRefCount: true);
		}
		if (riCharaCard.texture != null)
		{
			UnityEngine.Object.Destroy(riCharaCard.texture);
			riCharaCard.texture = null;
		}
		riCharaCard.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.partnerCardFile));
		txtCharaName.text = _info.partnerName;
		if (riPartnerCoordinate.texture != null && riPartnerCoordinate.texture != texNoSelect)
		{
			UnityEngine.Object.Destroy(riPartnerCoordinate.texture);
			riPartnerCoordinate.texture = null;
		}
		if (!_info.partnerCoordinateFileName.IsNullOrEmpty())
		{
			riPartnerCoordinate.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.partnerCoordinateFileName));
		}
		else
		{
			riPartnerCoordinate.texture = texNoSelect;
		}
		txtPartnerCoordinateName.text = _info.partnerCoordinateName;
		if (riPlayerCard.texture != null)
		{
			UnityEngine.Object.Destroy(riPlayerCard.texture);
			riPlayerCard.texture = null;
		}
		riPlayerCard.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.playerCardFile));
		txtPlayerName.text = _info.playerName;
		if (riPlayerCoordinate.texture != null && riPlayerCoordinate.texture != texNoSelect)
		{
			UnityEngine.Object.Destroy(riPlayerCoordinate.texture);
			riPlayerCoordinate.texture = null;
		}
		if (!_info.playerCoordinateFileName.IsNullOrEmpty())
		{
			riPlayerCoordinate.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.playerCoordinateFileName));
		}
		else
		{
			riPlayerCoordinate.texture = texNoSelect;
		}
		txtPlayerCoordinateName.text = _info.playerCoordinateName;
	}
}
