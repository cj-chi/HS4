using System.Collections;
using AIChara;
using Illusion.Game;
using Manager;
using MessagePack;
using UnityEngine;

namespace CharaCustom;

public class CvsC_ClothesLoad : CvsBase
{
	[SerializeField]
	private CustomClothesWindow clothesLoadWin;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = true;
	}

	public void UpdateClothesList()
	{
		clothesLoadWin.UpdateWindow(base.customBase.modeNew, base.customBase.modeSex, save: false);
	}

	protected override IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		base.customBase.actUpdateCvsClothesLoad += UpdateClothesList;
		UpdateClothesList();
		clothesLoadWin.btnDisableNotSelect01 = true;
		clothesLoadWin.btnDisableNotSelect02 = true;
		clothesLoadWin.btnDisableNotSelect03 = true;
		clothesLoadWin.onClick01 = delegate(CustomClothesFileInfo info)
		{
			byte[] bytes = MessagePackSerializer.Serialize(base.chaCtrl.nowCoordinate.accessory);
			string fullPath = info.FullPath;
			base.chaCtrl.nowCoordinate.LoadFile(fullPath);
			base.chaCtrl.nowCoordinate.accessory = MessagePackSerializer.Deserialize<ChaFileAccessory>(bytes);
			Singleton<Character>.Instance.customLoadGCClear = false;
			base.chaCtrl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
			Singleton<Character>.Instance.customLoadGCClear = true;
			base.customBase.updateCustomUI = true;
			base.chaCtrl.AssignCoordinate();
			Utils.Sound.Play(SystemSE.load);
		};
		clothesLoadWin.onClick02 = delegate(CustomClothesFileInfo info)
		{
			byte[] bytes = MessagePackSerializer.Serialize(base.chaCtrl.nowCoordinate.clothes);
			string fullPath = info.FullPath;
			base.chaCtrl.nowCoordinate.LoadFile(fullPath);
			base.chaCtrl.nowCoordinate.clothes = MessagePackSerializer.Deserialize<ChaFileClothes>(bytes);
			Singleton<Character>.Instance.customLoadGCClear = false;
			base.chaCtrl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
			Singleton<Character>.Instance.customLoadGCClear = true;
			base.customBase.updateCustomUI = true;
			base.customBase.ChangeAcsSlotName();
			base.customBase.forceUpdateAcsList = true;
			base.chaCtrl.AssignCoordinate();
			Utils.Sound.Play(SystemSE.load);
		};
		clothesLoadWin.onClick03 = delegate(CustomClothesFileInfo info)
		{
			string fullPath = info.FullPath;
			base.chaCtrl.nowCoordinate.LoadFile(fullPath);
			Singleton<Character>.Instance.customLoadGCClear = false;
			base.chaCtrl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
			Singleton<Character>.Instance.customLoadGCClear = true;
			base.customBase.updateCustomUI = true;
			base.customBase.ChangeAcsSlotName();
			base.customBase.forceUpdateAcsList = true;
			base.chaCtrl.AssignCoordinate();
			Utils.Sound.Play(SystemSE.load);
		};
		yield return null;
	}
}
