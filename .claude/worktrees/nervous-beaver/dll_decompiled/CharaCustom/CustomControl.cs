using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Config;
using Illusion;
using Illusion.CustomAttributes;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomControl : MonoBehaviour
{
	[Serializable]
	public class SliderScrollRaycast
	{
		public Image[] imgScrollRaycast;

		public void ChangeActiveRaycast(bool enable)
		{
			if (imgScrollRaycast == null)
			{
				return;
			}
			Image[] array = imgScrollRaycast;
			foreach (Image image in array)
			{
				if (!(null == image))
				{
					image.raycastTarget = enable;
				}
			}
		}
	}

	[Serializable]
	public class HideTrial
	{
		public GameObject[] objHide;
	}

	[Serializable]
	public class HideTrialOnly
	{
		public GameObject[] objHide;
	}

	[Serializable]
	public class DisplayByCondition
	{
		public GameObject[] objFemale;

		public GameObject[] objMale;

		public GameObject[] objNew;

		public GameObject[] objEdit;
	}

	[Header("デバッグ ------------------------------")]
	[Button("InitializeScneUI", "シーンUI初期設定", new object[] { })]
	public int initializescneui;

	[Header("メンバ -------------------------------")]
	public CameraControl_Ver2 camCtrl;

	[SerializeField]
	private CustomCapture _customCap;

	[SerializeField]
	private CvsA_Slot cvsA_Slot;

	[SerializeField]
	private CvsH_Hair cvsH_Hair;

	public Canvas cvsChangeScene;

	[SerializeField]
	private Text textFullName;

	[SerializeField]
	private GameObject objMainCanvas;

	[SerializeField]
	private GameObject objSubCanvas;

	[SerializeField]
	private CanvasGroup cvgDrawCanvas;

	[SerializeField]
	private CanvasGroup cvgFusionCanvas;

	[SerializeField]
	private GameObject objCapCanvas;

	[SerializeField]
	private CanvasGroup cvgColorPanel;

	[SerializeField]
	private CanvasGroup cvgPattern;

	[SerializeField]
	private CanvasGroup cvgShortcut;

	[SerializeField]
	private CanvasGroup cvsInputCoordinate;

	private bool firstUpdate = true;

	private BoolReactiveProperty _saveMode = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updatePng = new BoolReactiveProperty(initialValue: false);

	private List<bool> lstShow = new List<bool>();

	private int isConcierge = -1;

	private bool isPlayer;

	[Header("スクロールのRaycast -------------------")]
	public SliderScrollRaycast sliderScrollRaycast;

	[Header("条件による表示 ------------------------")]
	[SerializeField]
	private HideTrial hideTrial;

	[Header("条件による表示 ------------------------")]
	[SerializeField]
	private HideTrialOnly hideTrialOnly;

	[Header("条件による表示 ------------------------")]
	[SerializeField]
	private DisplayByCondition hideByCondition;

	[Header("条件による表示 ------------------------")]
	[SerializeField]
	private GameObject objFutanari;

	[Header("背景関連 ------------------------------")]
	[SerializeField]
	private BackgroundCtrl bgCtrl;

	[SerializeField]
	private BoolReactiveProperty _draw3D = new BoolReactiveProperty(initialValue: true);

	[SerializeField]
	private GameObject obj2DTop;

	[SerializeField]
	private GameObject obj3DTop;

	[SerializeField]
	private Renderer rendBG;

	public CustomCapture customCap => _customCap;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	private ChaControl chaCtrl
	{
		get
		{
			return customBase.chaCtrl;
		}
		set
		{
			customBase.chaCtrl = value;
		}
	}

	private ChaFileControl chaFile => chaCtrl.chaFile;

	private ChaFileBody body => chaFile.custom.body;

	private ChaFileFace face => chaFile.custom.face;

	private ChaFileHair hair => chaFile.custom.hair;

	private bool modeNew
	{
		get
		{
			return customBase.modeNew;
		}
		set
		{
			customBase.modeNew = value;
		}
	}

	private byte modeSex
	{
		get
		{
			return customBase.modeSex;
		}
		set
		{
			customBase.modeSex = value;
		}
	}

	public bool saveMode
	{
		get
		{
			return _saveMode.Value;
		}
		set
		{
			_saveMode.Value = value;
		}
	}

	public string overwriteSavePath { get; set; } = "";

	public bool updatePng
	{
		get
		{
			return _updatePng.Value;
		}
		set
		{
			_updatePng.Value = value;
		}
	}

	public bool showMainCvs { get; set; } = true;

	public bool showFusionCvs { get; set; }

	public bool showDrawMenu { get; set; } = true;

	public bool showColorCvs { get; set; }

	public bool showFileList { get; set; }

	public bool showPattern { get; set; }

	public bool showShortcut { get; set; }

	public bool showInputCoordinate { get; set; }

	public bool draw3D
	{
		get
		{
			return _draw3D.Value;
		}
		set
		{
			_draw3D.Value = value;
		}
	}

	public void Initialize(byte _sex, bool _new, string _nextScene, string _editCharaFileName = "", int _isConcierge = -1, bool _isPlayer = false)
	{
		modeSex = _sex;
		modeNew = _new;
		customBase.nextSceneName = _nextScene;
		isConcierge = _isConcierge;
		isPlayer = _isPlayer;
		if (!modeNew)
		{
			customBase.editSaveFileName = _editCharaFileName;
		}
		customBase.customCtrl = this;
		customBase.saveFrameAssist.Initialize();
		customBase.drawSaveFrameTop = false;
		customBase.drawSaveFrameBack = true;
		customBase.drawSaveFrameFront = true;
		customBase.centerDraw = Manager.Config.CameraData.Look;
		if (modeNew)
		{
			customBase.defChaCtrl.LoadFromAssetBundle((modeSex == 0) ? "custom/00/presets_m_00.unity3d" : "custom/00/presets_f_00.unity3d", (modeSex == 0) ? "ill_Default_Male" : "ill_Default_Female");
		}
		else
		{
			if (isConcierge == 0)
			{
				customBase.defChaCtrl.LoadFromAssetBundle("custom/00/presets_f_00.unity3d", "ill_Default_Navi");
			}
			else if (isConcierge == 1)
			{
				customBase.defChaCtrl.LoadFromAssetBundle("custom/50/presets_f_50.unity3d", "ill_Default_Sitri");
			}
			else
			{
				customBase.defChaCtrl.LoadCharaFile(customBase.editSaveFileName, modeSex);
			}
			customBase.editBackChaCtrl.LoadCharaFile(customBase.editSaveFileName, modeSex);
		}
		VoiceInfo.Param[] array = Voice.infoTable.Values.Where((VoiceInfo.Param x) => 0 <= x.No).ToArray();
		foreach (VoiceInfo.Param param in array)
		{
			customBase.dictPersonality[param.No] = param.Get(Singleton<GameSystem>.Instance.languageInt);
		}
		InitializeMapControl();
		LoadChara();
		customBase.poseNo = 1;
		customBase.customMotionIK = new MotionIK(chaCtrl);
		customBase.customMotionIK.SetPartners(customBase.customMotionIK);
		customBase.customMotionIK.Reset();
		if (modeSex == 0)
		{
			GameObject[] objMale = hideByCondition.objMale;
			foreach (GameObject gameObject in objMale)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActiveIfDifferent(active: false);
				}
			}
		}
		else
		{
			GameObject[] objMale = hideByCondition.objFemale;
			foreach (GameObject gameObject2 in objMale)
			{
				if ((bool)gameObject2)
				{
					gameObject2.SetActiveIfDifferent(active: false);
				}
			}
		}
		if (modeNew)
		{
			GameObject[] objMale = hideByCondition.objNew;
			foreach (GameObject gameObject3 in objMale)
			{
				if ((bool)gameObject3)
				{
					gameObject3.SetActiveIfDifferent(active: false);
				}
			}
		}
		else
		{
			GameObject[] objMale = hideByCondition.objEdit;
			foreach (GameObject gameObject4 in objMale)
			{
				if ((bool)gameObject4)
				{
					gameObject4.SetActiveIfDifferent(active: false);
				}
			}
			if ((isPlayer || isConcierge != -1) && null != objFutanari)
			{
				objFutanari.SetActiveIfDifferent(active: false);
			}
		}
		if (hideTrialOnly != null && hideTrialOnly.objHide != null)
		{
			GameObject[] objMale = hideTrialOnly.objHide;
			for (int num = 0; num < objMale.Length; num++)
			{
				objMale[num].SetActiveIfDifferent(active: false);
			}
		}
		customBase.forceUpdateAcsList = true;
		customBase.updateCustomUI = true;
	}

	private void LoadChara()
	{
		Singleton<Character>.Instance.BeginLoadAssetBundle();
		if (modeNew)
		{
			chaCtrl = Singleton<Character>.Instance.CreateChara(modeSex, base.gameObject, 0);
			chaCtrl.chaFile.pngData = null;
			chaCtrl.chaFile.userID = Singleton<GameSystem>.Instance.UserUUID;
			chaCtrl.chaFile.dataID = YS_Assist.CreateUUID();
		}
		else
		{
			chaCtrl = Singleton<Character>.Instance.CreateChara(modeSex, base.gameObject, 0);
			if (isConcierge == 0)
			{
				if (!chaCtrl.chaFile.LoadCharaFile(customBase.editSaveFileName, modeSex))
				{
					chaCtrl.chaFile.LoadFromAssetBundle("custom/00/presets_f_00.unity3d", "ill_Default_Navi");
				}
			}
			else if (isConcierge == 1)
			{
				if (!chaCtrl.chaFile.LoadCharaFile(customBase.editSaveFileName, modeSex))
				{
					chaCtrl.chaFile.LoadFromAssetBundle("custom/50/presets_f_50.unity3d", "ill_Default_Sitri");
				}
			}
			else
			{
				chaCtrl.chaFile.LoadCharaFile(customBase.editSaveFileName, modeSex);
			}
			chaCtrl.ChangeNowCoordinate();
		}
		chaCtrl.releaseCustomInputTexture = false;
		chaCtrl.Load();
		chaCtrl.ChangeEyebrowPtn(0);
		chaCtrl.ChangeEyesPtn(0);
		chaCtrl.ChangeMouthPtn(0);
		chaCtrl.ChangeLookEyesPtn(1);
		chaCtrl.ChangeLookNeckPtn(0);
		chaCtrl.hideMoz = true;
		chaCtrl.fileStatus.visibleSon = false;
	}

	public void InitializeScneUI()
	{
		string[] source = new string[16]
		{
			"CustomControl", "MainMenu", "SubMenuFace", "SettingWindow", "WinFace", "DefaultWin", "F_FaceType", "B_ShapeWhole", "H_Hair", "C_Clothes",
			"A_Slot", "O_Chara", "dwChara", "Setting01", "menuPicker", "DrawWindow"
		};
		CanvasGroup[] componentsInChildren = GetComponentsInChildren<CanvasGroup>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			CanvasGroup[] array = componentsInChildren;
			foreach (CanvasGroup canvasGroup in array)
			{
				canvasGroup.Enable(source.Contains(canvasGroup.name));
			}
		}
		string[] source2 = new string[23]
		{
			"tglFace", "SameSettingEyes", "AutoHairColor", "SameHairColor", "ControlTogether", "imgRbCol00", "imgRB00", "tgl01", "tglControl", "tglCtrlMove",
			"tglDay", "tglChangeParentLR", "TglType01", "tglPlay", "TglLoadType01", "TglLoadType02", "TglLoadType03", "TglLoadType04", "TglLoadType05", "RbHSV",
			"rbPicker", "rbSample", "ToggleH"
		};
		Toggle[] componentsInChildren2 = GetComponentsInChildren<Toggle>(includeInactive: true);
		if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
		{
			Toggle[] array2 = componentsInChildren2;
			foreach (Toggle toggle in array2)
			{
				toggle.isOn = source2.Contains(toggle.name);
			}
		}
	}

	public void UpdateCharaNameText()
	{
		textFullName.text = chaCtrl.chaFile.parameter.fullname;
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		if (null == camCtrl)
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
			if ((bool)gameObject)
			{
				camCtrl = gameObject.GetComponent<CameraControl_Ver2>();
			}
		}
		_saveMode.Subscribe(delegate(bool m)
		{
			showMainCvs = !m;
			if ((bool)objCapCanvas)
			{
				objCapCanvas.SetActiveIfDifferent(m);
			}
			if (m)
			{
				customBase.cvsCapMenu.BeginCapture();
			}
		});
		_updatePng.Subscribe(delegate(bool m)
		{
			showMainCvs = !m;
			if ((bool)objCapCanvas)
			{
				objCapCanvas.SetActiveIfDifferent(m);
			}
			if (m)
			{
				customBase.cvsCapMenu.BeginCapture();
			}
		});
		base.enabled = true;
	}

	private void Update()
	{
		bool flag = Scene.Overlaps.Any((Scene.IOverlap x) => x is ConfigWindow) || ConfigWindow.isActive;
		bool flag2 = Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog);
		bool flag3 = true;
		lstShow.Clear();
		lstShow.Add(showMainCvs);
		lstShow.Add(!flag);
		lstShow.Add(!showFusionCvs);
		lstShow.Add(!showInputCoordinate);
		flag3 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)objMainCanvas)
		{
			objMainCanvas.SetActiveIfDifferent(flag3);
		}
		if ((bool)objSubCanvas)
		{
			objSubCanvas.SetActiveIfDifferent(flag3);
		}
		lstShow.Clear();
		lstShow.Add(showFusionCvs);
		lstShow.Add(!flag);
		flag3 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)cvgFusionCanvas)
		{
			cvgFusionCanvas.Enable(flag3);
		}
		lstShow.Clear();
		lstShow.Add(showDrawMenu);
		lstShow.Add(!showFusionCvs);
		lstShow.Add(!showFileList);
		lstShow.Add(!flag);
		flag3 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)cvgDrawCanvas)
		{
			cvgDrawCanvas.Enable(flag3);
		}
		lstShow.Clear();
		lstShow.Add(showColorCvs);
		lstShow.Add(!showFusionCvs);
		lstShow.Add(saveMode || updatePng || !showFileList);
		lstShow.Add(!flag);
		flag3 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)cvgColorPanel)
		{
			cvgColorPanel.Enable(flag3);
		}
		lstShow.Clear();
		lstShow.Add(showPattern);
		lstShow.Add(!flag);
		flag3 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)cvgPattern)
		{
			cvgPattern.Enable(flag3);
		}
		lstShow.Clear();
		lstShow.Add(showInputCoordinate);
		flag3 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)cvsInputCoordinate)
		{
			cvsInputCoordinate.Enable(flag3);
		}
		if ((bool)cvgShortcut)
		{
			cvgShortcut.Enable(showShortcut);
		}
		if (saveMode || updatePng)
		{
			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
			{
				if ((bool)camCtrl)
				{
					camCtrl.NoCtrlCondition = () => false;
				}
			}
			else if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && Illusion.Utils.uGUI.isMouseHit && (bool)camCtrl)
			{
				camCtrl.NoCtrlCondition = () => true;
			}
		}
		else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
		{
			if ((bool)camCtrl)
			{
				camCtrl.NoCtrlCondition = () => false;
			}
		}
		else if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && Illusion.Utils.uGUI.isMouseHit && (bool)camCtrl)
		{
			camCtrl.NoCtrlCondition = () => true;
		}
		customBase.UpdateIKCalc();
		if (customBase.playVoiceBackup.playSampleVoice && !Manager.Sound.IsPlay(Manager.Sound.Type.SystemSE))
		{
			chaCtrl.ChangeEyebrowPtn(customBase.playVoiceBackup.backEyebrowPtn);
			chaCtrl.ChangeEyesPtn(customBase.playVoiceBackup.backEyesPtn);
			chaCtrl.HideEyeHighlight(hide: false);
			chaCtrl.ChangeEyesBlinkFlag(customBase.playVoiceBackup.backBlink);
			chaCtrl.ChangeEyesOpenMax(customBase.playVoiceBackup.backEyesOpen);
			chaCtrl.ChangeMouthPtn(customBase.playVoiceBackup.backMouthPtn);
			chaCtrl.ChangeMouthFixed(customBase.playVoiceBackup.backMouthFix);
			chaCtrl.ChangeMouthOpenMax(customBase.playVoiceBackup.backMouthOpen);
			customBase.playVoiceBackup.playSampleVoice = false;
		}
		if (showShortcut && (Input.GetKeyDown(KeyCode.F2) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
		{
			showShortcut = false;
			return;
		}
		bool isInputFocused = customBase.IsInputFocused();
		if (!isInputFocused && "CharaCustom" == Scene.NowSceneNames[0] && !showShortcut && !flag2 && !flag)
		{
			if (Input.GetKeyDown(KeyCode.F2))
			{
				showShortcut = true;
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (!Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog))
				{
					ExitDialog.GameEnd(isCheck: true);
				}
			}
			else if (Input.GetKeyDown(KeyCode.Z))
			{
				Manager.Config.CameraData.Look = !Manager.Config.CameraData.Look;
				customBase.centerDraw = Manager.Config.CameraData.Look;
			}
			else if (Input.GetKeyDown(KeyCode.W))
			{
				if (customBase.objAcs01ControllerTop.activeSelf || customBase.objAcs02ControllerTop.activeSelf)
				{
					cvsA_Slot.ShortcutChangeGuidType(0);
				}
				else if (customBase.objHairControllerTop.activeSelf)
				{
					cvsH_Hair.ShortcutChangeGuidType(0);
				}
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				if (customBase.objAcs01ControllerTop.activeSelf || customBase.objAcs02ControllerTop.activeSelf)
				{
					cvsA_Slot.ShortcutChangeGuidType(1);
				}
				else if (customBase.objHairControllerTop.activeSelf)
				{
					cvsH_Hair.ShortcutChangeGuidType(1);
				}
			}
			else if (Input.GetKeyDown(KeyCode.F1) && !Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow))
			{
				Illusion.Game.Utils.Sound.Play(SystemSE.ok_s);
				ConfigWindow.Load();
			}
		}
		if (!firstUpdate && Singleton<CustomBase>.IsInstance() && customBase.updateCustomUI)
		{
			customBase.changeCharaName = true;
			customBase.updateCvsFaceType = true;
			customBase.updateCvsFaceShapeWhole = true;
			customBase.updateCvsFaceShapeChin = true;
			customBase.updateCvsFaceShapeCheek = true;
			customBase.updateCvsFaceShapeEyebrow = true;
			customBase.updateCvsFaceShapeEyes = true;
			customBase.updateCvsFaceShapeNose = true;
			customBase.updateCvsFaceShapeMouth = true;
			customBase.updateCvsFaceShapeEar = true;
			customBase.updateCvsMole = true;
			customBase.updateCvsEyeLR = true;
			customBase.updateCvsEyeEtc = true;
			customBase.updateCvsEyeHL = true;
			customBase.updateCvsEyebrow = true;
			customBase.updateCvsEyelashes = true;
			customBase.updateCvsEyeshadow = true;
			customBase.updateCvsCheek = true;
			customBase.updateCvsLip = true;
			customBase.updateCvsFacePaint = true;
			customBase.updateCvsBeard = true;
			customBase.updateCvsBodyShapeWhole = true;
			customBase.updateCvsBodyShapeBreast = true;
			customBase.updateCvsBodyShapeUpper = true;
			customBase.updateCvsBodyShapeLower = true;
			customBase.updateCvsBodyShapeArm = true;
			customBase.updateCvsBodyShapeLeg = true;
			customBase.updateCvsBodySkinType = true;
			customBase.updateCvsSunburn = true;
			customBase.updateCvsNip = true;
			customBase.updateCvsUnderhair = true;
			customBase.updateCvsNail = true;
			customBase.updateCvsBodyPaint = true;
			customBase.updateCvsFutanari = true;
			customBase.updateCvsHair = true;
			customBase.updateCvsClothes = true;
			customBase.updateCvsClothesSaveDelete = true;
			customBase.updateCvsClothesLoad = true;
			customBase.updateCvsAccessory = true;
			customBase.updateCvsAcsCopy = true;
			customBase.updateCvsChara = true;
			customBase.updateCvsType = true;
			customBase.updateCvsStatus = true;
			customBase.updateCvsCharaSaveDelete = true;
			customBase.updateCvsCharaLoad = true;
			customBase.updateCustomUI = false;
		}
		if ((bool)camCtrl)
		{
			camCtrl.KeyCondition = () => !isInputFocused;
		}
		firstUpdate = false;
	}

	public void ChangeBGImage(int bf)
	{
		if ((bool)bgCtrl)
		{
			bgCtrl.ChangeBGImage((byte)bf);
		}
	}

	public void ChangeBGColor(Color color)
	{
		if (!(null == rendBG))
		{
			rendBG.material.SetColor(ChaShader.Color2, color);
		}
	}

	public Color GetBGColor()
	{
		if (null == rendBG)
		{
			return Color.white;
		}
		return rendBG.material.GetColor(ChaShader.Color2);
	}

	public void InitializeMapControl()
	{
		_draw3D.Subscribe(delegate(bool isOn)
		{
			if ((bool)obj2DTop)
			{
				obj2DTop.SetActiveIfDifferent(!isOn);
			}
			if ((bool)obj3DTop)
			{
				obj3DTop.SetActiveIfDifferent(isOn);
			}
		});
	}
}
