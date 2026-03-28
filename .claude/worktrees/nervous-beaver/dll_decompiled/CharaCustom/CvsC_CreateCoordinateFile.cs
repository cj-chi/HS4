using System;
using System.Collections;
using AIChara;
using Illusion.Extensions;
using IllusionUtility.SetUtility;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsC_CreateCoordinateFile : MonoBehaviour
{
	[SerializeField]
	private RawImage imgDummy;

	[SerializeField]
	private CustomRender custom2DRender;

	[SerializeField]
	private CustomRender custom3DRender;

	[SerializeField]
	private CustomDrawMenu customDrawMenu;

	[SerializeField]
	private GameObject objMap3D;

	[SerializeField]
	private GameObject objBackCamera;

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private Camera coordinateCamera;

	private string saveCoordinateName = "";

	private string saveCoordinateFileName = "";

	private bool saveOverwrite;

	private int backPoseNo;

	private float backPosePos;

	protected CustomBase customBase => Singleton<CustomBase>.Instance;

	protected ChaControl chaCtrl => customBase.chaCtrl;

	public void CreateCoordinateFile(string _savePath, string _coordinateName, bool _overwrite)
	{
		saveCoordinateName = _coordinateName;
		saveCoordinateFileName = _savePath;
		saveOverwrite = _overwrite;
		StartCoroutine(CreateCoordinateFileCoroutine());
	}

	public void CreateCoordinateFileBefore()
	{
		if (customBase.customCtrl.draw3D)
		{
			custom3DRender.update = false;
			RenderTexture renderTexture = custom3DRender.GetRenderTexture();
			imgDummy.texture = renderTexture;
		}
		else
		{
			custom2DRender.update = false;
			RenderTexture renderTexture2 = custom2DRender.GetRenderTexture();
			imgDummy.texture = renderTexture2;
		}
		imgDummy.transform.parent.gameObject.SetActiveIfDifferent(active: true);
		chaCtrl.ChangeSettingMannequin(mannequin: true);
		customDrawMenu.ChangeClothesStateForCapture(capture: true);
		Texture2D tex = PngAssist.LoadTexture2DFromAssetBundle("custom/custom_etc.unity3d", "coordinate_front");
		customBase.saveFrameAssist.ChangeSaveFrameTexture(1, tex);
		backPoseNo = customBase.poseNo;
		if (null != chaCtrl.animBody)
		{
			backPosePos = chaCtrl.getAnimatorStateInfo(0).normalizedTime;
			customBase.animationPos = 0f;
			customBase.ChangeAnimationNo(0, mannequin: true);
			chaCtrl.resetDynamicBoneAll = true;
		}
		customBase.updateCustomUI = true;
		for (int i = 0; i < 20; i++)
		{
			customBase.ChangeAcsSlotName(i);
		}
		customBase.SetUpdateToggleSetting();
		customBase.forceUpdateAcsList = true;
	}

	public IEnumerator CreateCoordinateFileCoroutine()
	{
		CreateCoordinateFileBefore();
		yield return new WaitForEndOfFrame();
		ObservableYieldInstruction<byte[]> ret = Observable.FromCoroutine((IObserver<byte[]> res) => CreateCoordinatePng(res)).ToYieldInstruction(throwOnError: false);
		yield return ret;
		if (saveOverwrite)
		{
			chaCtrl.chaFile.coordinate.pngData = ret.Result;
			chaCtrl.chaFile.coordinate.coordinateName = saveCoordinateName;
			chaCtrl.chaFile.coordinate.SaveFile(saveCoordinateFileName, (int)Singleton<GameSystem>.Instance.language);
		}
		else
		{
			string path = string.Concat(str2: (chaCtrl.sex != 0) ? ("HS2CoordeF_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")) : ("HS2CoordeM_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")), str0: UserData.Path, str1: (chaCtrl.sex == 0) ? "coordinate/male/" : "coordinate/female/", str3: ".png");
			chaCtrl.chaFile.coordinate.pngData = ret.Result;
			chaCtrl.chaFile.coordinate.coordinateName = saveCoordinateName;
			chaCtrl.chaFile.coordinate.SaveFile(path, (int)Singleton<GameSystem>.Instance.language);
		}
		customBase.updateCvsClothesSaveDelete = true;
		customBase.updateCvsClothesLoad = true;
		customBase.updateCustomUI = true;
		for (int num = 0; num < 20; num++)
		{
			customBase.ChangeAcsSlotName(num);
		}
		customBase.SetUpdateToggleSetting();
		customBase.forceUpdateAcsList = true;
		yield return null;
		objMap3D.SetActiveIfDifferent(customBase.customCtrl.draw3D);
		objBackCamera.SetActiveIfDifferent(!customBase.customCtrl.draw3D);
		imgDummy.transform.parent.gameObject.SetActiveIfDifferent(active: false);
		if (customBase.customCtrl.draw3D)
		{
			custom3DRender.update = true;
		}
		else
		{
			custom2DRender.update = true;
		}
	}

	public IEnumerator CreateCoordinatePng(IObserver<byte[]> observer)
	{
		mainCamera.gameObject.SetActiveIfDifferent(active: false);
		objMap3D.SetActiveIfDifferent(active: false);
		objBackCamera.SetActiveIfDifferent(active: false);
		if ((bool)coordinateCamera)
		{
			coordinateCamera.enabled = true;
		}
		if ((bool)coordinateCamera)
		{
			coordinateCamera.gameObject.SetActiveIfDifferent(active: true);
			if (chaCtrl.sex == 0)
			{
				coordinateCamera.transform.SetPosition(0f, 10.5f, 46.9f);
				coordinateCamera.transform.SetRotation(2.62f, -180f, 0f);
			}
			else
			{
				coordinateCamera.transform.SetPosition(0f, 10.2f, 45.6f);
				coordinateCamera.transform.SetRotation(2.62f, -180f, 0f);
			}
		}
		customBase.drawSaveFrameTop = true;
		bool drawSaveFrameFront = customBase.drawSaveFrameFront;
		customBase.drawSaveFrameFront = true;
		byte[] value = customBase.customCtrl.customCap.CapCoordinateCard(enableBG: false, customBase.saveFrameAssist, coordinateCamera);
		customBase.saveFrameAssist.ChangeSaveFrameFront(2);
		customBase.drawSaveFrameFront = drawSaveFrameFront;
		customBase.drawSaveFrameTop = false;
		if ((bool)coordinateCamera)
		{
			coordinateCamera.enabled = false;
		}
		if ((bool)coordinateCamera)
		{
			coordinateCamera.gameObject.SetActiveIfDifferent(active: false);
		}
		mainCamera.gameObject.SetActiveIfDifferent(active: true);
		chaCtrl.ChangeSettingMannequin(mannequin: false);
		customDrawMenu.ChangeClothesStateForCapture(capture: false);
		if (null != chaCtrl.animBody)
		{
			customBase.animationPos = backPosePos;
			customBase.ChangeAnimationNo(backPoseNo);
			chaCtrl.resetDynamicBoneAll = true;
		}
		observer.OnNext(value);
		observer.OnCompleted();
		yield break;
	}
}
