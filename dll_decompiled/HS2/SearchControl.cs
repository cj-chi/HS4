using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using CharaCustom;
using Config;
using Illusion;
using Illusion.CustomAttributes;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class SearchControl : MonoBehaviour
{
	[Header("デバッグ ------------------------------")]
	[Button("InitializeSceneUI", "シーンUI初期設定", new object[] { })]
	public int initializesceneui;

	[Header("メンバ -------------------------------")]
	public CameraControl_Ver2 camCtrl;

	[SerializeField]
	private CustomCapture _customCap;

	public Canvas cvsChangeScene;

	[SerializeField]
	private GameObject objMainCanvas;

	[SerializeField]
	private GameObject objSubCanvas;

	[SerializeField]
	private CanvasGroup cvgDrawCanvas;

	[SerializeField]
	private GameObject objCapCanvas;

	[SerializeField]
	private CanvasGroup cvgColorPanel;

	[SerializeField]
	private CanvasGroup cvgShortcut;

	private bool firstUpdate = true;

	private BoolReactiveProperty _saveMode = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updatePng = new BoolReactiveProperty(initialValue: false);

	private List<bool> lstShow = new List<bool>();

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

	private SearchBase searchBase => Singleton<SearchBase>.Instance;

	private ChaControl chaCtrl
	{
		get
		{
			return searchBase.chaCtrl;
		}
		set
		{
			searchBase.chaCtrl = value;
		}
	}

	private ChaFileControl chaFile => chaCtrl.chaFile;

	private ChaFileBody body => chaFile.custom.body;

	private ChaFileFace face => chaFile.custom.face;

	private ChaFileHair hair => chaFile.custom.hair;

	private ChaFemaleRandom chaRandom => searchBase.chaRandom;

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

	public bool showDrawMenu { get; set; } = true;

	public bool showColorCvs { get; set; }

	public bool showFileList { get; set; }

	public bool showShortcut { get; set; }

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

	public IEnumerator Initialize(string _nextScene, string _editCharaFileName = "")
	{
		searchBase.nextSceneName = _nextScene;
		searchBase.searchCtrl = this;
		searchBase.saveFrameAssist.Initialize();
		searchBase.drawSaveFrameTop = false;
		searchBase.drawSaveFrameBack = true;
		searchBase.drawSaveFrameFront = true;
		Singleton<Character>.Instance.DeleteCharaAll();
		Illusion.Game.Utils.Sound.Play(new Illusion.Game.Utils.Sound.SettingBGM(BGM.custom));
		searchBase.defChaCtrl.LoadFromAssetBundle("custom/00/presets_f_00.unity3d", "ill_Default_Female");
		VoiceInfo.Param[] array = Voice.infoTable.Values.Where((VoiceInfo.Param x) => 0 <= x.No).ToArray();
		foreach (VoiceInfo.Param param in array)
		{
			searchBase.dictPersonality[param.No] = param.Get(Singleton<GameSystem>.Instance.languageInt);
		}
		InitializeMapControl();
		LoadChara();
		searchBase.poseNo = 1;
		searchBase.customMotionIK = new MotionIK(chaCtrl);
		searchBase.customMotionIK.SetPartners(searchBase.customMotionIK);
		searchBase.customMotionIK.Reset();
		searchBase.updateCustomUI = true;
		yield return StartCoroutine(chaRandom.Load());
		chaRandom.RandomHair(chaCtrl, -1);
		chaRandom.RandomFace(chaCtrl, _mole: false, _elf: false);
		chaRandom.RandomBody(chaCtrl, -1, -1, -1, -1, -1);
		chaRandom.RandomClothAndAccessory(chaCtrl, _glasses: false);
		chaRandom.RandomPersonal(chaCtrl, -1);
		chaCtrl.Reload();
		searchBase.foundFemaleWindow.UpdateUI(chaCtrl.chaFile);
	}

	private void LoadChara()
	{
		Singleton<Character>.Instance.BeginLoadAssetBundle();
		chaCtrl = Singleton<Character>.Instance.CreateChara(1, base.gameObject, 0);
		chaCtrl.chaFile.pngData = null;
		chaCtrl.chaFile.userID = Singleton<GameSystem>.Instance.UserUUID;
		chaCtrl.chaFile.dataID = YS_Assist.CreateUUID();
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

	public void InitializeSceneUI()
	{
		string[] source = new string[4] { "SearchControl", "dwChara", "menuPicker", "DrawWindow" };
		CanvasGroup[] componentsInChildren = GetComponentsInChildren<CanvasGroup>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			CanvasGroup[] array = componentsInChildren;
			foreach (CanvasGroup canvasGroup in array)
			{
				canvasGroup.Enable(source.Contains(canvasGroup.name));
			}
		}
		string[] source2 = new string[6] { "tgl01", "tglPlay", "RbHSV", "rbPicker", "rbSample", "ToggleH" };
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
				searchBase.svsCapMenu.BeginCapture();
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
				searchBase.svsCapMenu.BeginCapture();
			}
		});
		base.enabled = true;
	}

	private void Update()
	{
		bool flag = Scene.Overlaps.Any((Scene.IOverlap x) => x is ConfigWindow);
		bool flag2 = true;
		lstShow.Clear();
		lstShow.Add(showMainCvs);
		lstShow.Add(!flag);
		flag2 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)objMainCanvas)
		{
			objMainCanvas.SetActiveIfDifferent(flag2);
		}
		if ((bool)objSubCanvas)
		{
			objSubCanvas.SetActiveIfDifferent(flag2);
		}
		lstShow.Clear();
		lstShow.Add(showDrawMenu);
		lstShow.Add(showMainCvs);
		lstShow.Add(!flag);
		flag2 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)cvgDrawCanvas)
		{
			cvgDrawCanvas.Enable(flag2);
		}
		lstShow.Clear();
		lstShow.Add(showColorCvs);
		lstShow.Add(saveMode || updatePng || !showFileList);
		lstShow.Add(!flag);
		flag2 = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)cvgColorPanel)
		{
			cvgColorPanel.Enable(flag2);
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
		searchBase.UpdateIKCalc();
		if (searchBase.playVoiceBackup.playSampleVoice && !Manager.Sound.IsPlay(Manager.Sound.Type.SystemSE))
		{
			chaCtrl.ChangeEyebrowPtn(searchBase.playVoiceBackup.backEyebrowPtn);
			chaCtrl.ChangeEyesPtn(searchBase.playVoiceBackup.backEyesPtn);
			chaCtrl.HideEyeHighlight(hide: false);
			chaCtrl.ChangeEyesBlinkFlag(searchBase.playVoiceBackup.backBlink);
			chaCtrl.ChangeEyesOpenMax(searchBase.playVoiceBackup.backEyesOpen);
			chaCtrl.ChangeMouthPtn(searchBase.playVoiceBackup.backMouthPtn);
			chaCtrl.ChangeMouthFixed(searchBase.playVoiceBackup.backMouthFix);
			chaCtrl.ChangeMouthOpenMax(searchBase.playVoiceBackup.backMouthOpen);
			searchBase.playVoiceBackup.playSampleVoice = false;
		}
		if (showShortcut && (Input.GetKeyDown(KeyCode.F2) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
		{
			showShortcut = false;
			return;
		}
		bool isInputFocused = searchBase.IsInputFocused();
		if (camCtrl.isOutsideTargetTex != Manager.Config.CameraData.Look)
		{
			camCtrl.isOutsideTargetTex = Manager.Config.CameraData.Look;
		}
		if (!firstUpdate && Singleton<CustomBase>.IsInstance() && searchBase.updateCustomUI)
		{
			searchBase.updateCvsCharaSaveDelete = true;
			searchBase.updateCvsCharaLoad = true;
			searchBase.updateCustomUI = false;
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
