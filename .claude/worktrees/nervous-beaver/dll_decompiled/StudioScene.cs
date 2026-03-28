using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Manager;
using Studio;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StudioScene : MonoBehaviour
{
	[Serializable]
	private class DrawInfo
	{
		public Canvas canvasMainMenu;

		public CanvasGroup canvasMainMenuG;

		public SystemButtonCtrl systemButtonCtrl;

		public Canvas canvasList;

		public Canvas canvasColor;

		public GameObject objOption;

		public GameObject objCamera;

		public Image imageMenu;

		public Sprite[] spriteMenu;

		public Image imageWork;

		public Sprite[] spriteWork;

		public Image imageMove;

		public Sprite[] spriteMove;

		public Image imageColor;

		public Sprite[] spriteColor;

		public Image imageOption;

		public Sprite[] spriteOption;

		public Image imageCamera;

		public Sprite[] spriteCamera;

		public Canvas canvasSystemMenu;
	}

	[Serializable]
	private class ModeInfo
	{
		public Image[] imageMode;

		public Sprite[] spriteModeMove;

		public Sprite[] spriteModeRotate;

		public Sprite[] spriteModeScale;

		public Canvas canvasInput;
	}

	[Serializable]
	public class CameraInfo
	{
		public Image imageCamera;

		public Sprite[] spriteCamera;

		public Studio.CameraControl cameraCtrl;

		public Image imageCenter;

		public Image imageAxis;

		public Image imageAxisTrans;

		public Image imageAxisCenter;

		public Image imageGimmick;

		public Camera cameraSub;

		public PhysicsRaycaster physicsRaycaster;

		public Camera cameraUI;

		public bool center
		{
			get
			{
				return cameraCtrl.isOutsideTargetTex;
			}
			set
			{
				if (cameraCtrl.isOutsideTargetTex != value)
				{
					cameraCtrl.isOutsideTargetTex = value;
					imageCenter.color = (cameraCtrl.isOutsideTargetTex ? Color.green : Color.white);
				}
			}
		}

		public bool axis
		{
			set
			{
				Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxis = value;
				physicsRaycaster.enabled = value;
				imageAxis.color = (value ? Color.green : Color.white);
				cameraCtrl.ReflectOption();
			}
		}

		public bool axisTrans
		{
			set
			{
				Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxisTranslation = value;
				imageAxisTrans.color = (value ? Color.green : Color.white);
			}
		}

		public bool axisCenter
		{
			set
			{
				Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxisCenter = value;
				imageAxisCenter.color = (value ? Color.green : Color.white);
			}
		}

		public bool Gimmick
		{
			set
			{
				Singleton<global::Studio.Studio>.Instance.workInfo.visibleGimmick = value;
				imageGimmick.color = (value ? Color.green : Color.white);
			}
		}

		public void Init()
		{
			imageCenter.color = (cameraCtrl.isOutsideTargetTex ? Color.green : Color.white);
			imageAxis.color = (cameraSub.enabled ? Color.green : Color.white);
		}
	}

	[Serializable]
	private class MapInfo
	{
		public MapCtrl mapCtrl;

		public Button button;

		private Image m_Image;

		private bool m_Active;

		private Image image
		{
			get
			{
				if (m_Image == null)
				{
					m_Image = button.image;
				}
				return m_Image;
			}
		}

		public bool active
		{
			get
			{
				return m_Active;
			}
			set
			{
				m_Active = value;
				Update();
			}
		}

		public void Update()
		{
			image.color = (m_Active ? Color.green : Color.white);
			mapCtrl.active = m_Active;
		}

		public void OnChangeMap()
		{
			button.interactable = Singleton<global::Studio.Studio>.Instance.sceneInfo.mapInfo.no != -1;
			if (!button.interactable && active)
			{
				active = false;
			}
			else
			{
				mapCtrl.Reflect();
			}
		}
	}

	[Serializable]
	private class InputInfo
	{
		public ObjectCtrl objectCtrl;

		public Button button;

		private Image _image;

		private bool _active;

		private BoolReactiveProperty _outsideVisible = new BoolReactiveProperty(initialValue: true);

		private Image image
		{
			get
			{
				if (_image == null)
				{
					_image = button.image;
				}
				return _image;
			}
		}

		public bool active
		{
			get
			{
				return _active;
			}
			set
			{
				_active = value;
				Update();
			}
		}

		public bool outsideVisible
		{
			get
			{
				return _outsideVisible.Value;
			}
			set
			{
				_outsideVisible.Value = value;
			}
		}

		public void Init()
		{
			_outsideVisible.Subscribe(delegate
			{
				UpdateVisible();
			});
		}

		public void Update()
		{
			image.color = (_active ? Color.green : Color.white);
			objectCtrl.active = _active;
		}

		public void OnVisible(bool _value)
		{
			button.interactable = _value;
			UpdateVisible();
		}

		private void UpdateVisible()
		{
			objectCtrl.active = button.interactable & active & outsideVisible;
		}
	}

	[SerializeField]
	private DrawInfo drawInfo = new DrawInfo();

	[SerializeField]
	private ModeInfo modeInfo = new ModeInfo();

	public CameraInfo cameraInfo = new CameraInfo();

	[SerializeField]
	private Button buttonUndo;

	[SerializeField]
	private Button buttonRedo;

	[SerializeField]
	private MapInfo mapInfo = new MapInfo();

	[SerializeField]
	private Text textAdjustment;

	[SerializeField]
	private OptionCtrl optionCtrl;

	[SerializeField]
	private InputInfo inputInfo = new InputInfo();

	[RuntimeInitializeOnLoadMethod]
	private static void Exit()
	{
		Application.wantsToQuit += delegate
		{
			if (Application.isEditor || Scene.IsNowLoadingFade)
			{
				return true;
			}
			List<string> nowSceneNames = Scene.NowSceneNames;
			if (nowSceneNames.Contains("StudioExit"))
			{
				return true;
			}
			if (nowSceneNames.Contains("StudioNotification") || nowSceneNames.Contains("StudioWait") || nowSceneNames.Contains("StudioStart"))
			{
				return false;
			}
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "StudioExit",
				isAdd = true
			}, isLoadingImageDraw: false);
			return false;
		};
	}

	public void OnClickDraw(int _no)
	{
		Singleton<global::Studio.Studio>.Instance.workInfo.visibleFlags[_no] = !Singleton<global::Studio.Studio>.Instance.workInfo.visibleFlags[_no];
		SortCanvas.select = drawInfo.canvasSystemMenu;
		UpdateUI(_no);
	}

	private void UpdateUI(int _no)
	{
		bool flag = Singleton<global::Studio.Studio>.Instance.workInfo.visibleFlags[_no];
		switch (_no)
		{
		case 0:
			drawInfo.canvasMainMenuG.Enable(flag);
			drawInfo.systemButtonCtrl.visible = flag;
			drawInfo.imageMenu.sprite = drawInfo.spriteMenu[(!flag) ? 1u : 0u];
			break;
		case 1:
			drawInfo.canvasList.enabled = flag;
			drawInfo.imageWork.sprite = drawInfo.spriteWork[(!flag) ? 1u : 0u];
			break;
		case 2:
			Singleton<GuideObjectManager>.Instance.guideInput.outsideVisible = flag;
			drawInfo.imageMove.sprite = drawInfo.spriteMove[(!flag) ? 1u : 0u];
			break;
		case 3:
			Singleton<global::Studio.Studio>.Instance.colorPalette.outsideVisible = flag;
			drawInfo.imageColor.sprite = drawInfo.spriteColor[(!flag) ? 1u : 0u];
			break;
		case 4:
			drawInfo.objOption.SetActive(flag);
			if (!flag)
			{
				mapInfo.mapCtrl.active = false;
				inputInfo.outsideVisible = false;
			}
			else
			{
				mapInfo.Update();
				inputInfo.outsideVisible = true;
			}
			drawInfo.imageOption.sprite = drawInfo.spriteOption[(!flag) ? 1u : 0u];
			break;
		case 5:
			drawInfo.objCamera.SetActive(flag);
			drawInfo.imageCamera.sprite = drawInfo.spriteCamera[(!flag) ? 1u : 0u];
			break;
		}
	}

	private void UpdateUI()
	{
		for (int i = 0; i < 6; i++)
		{
			UpdateUI(i);
		}
		cameraInfo.center = Singleton<global::Studio.Studio>.Instance.workInfo.visibleCenter;
		cameraInfo.axis = Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxis;
		cameraInfo.axisTrans = Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxisTranslation;
		cameraInfo.axisCenter = Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxisCenter;
		cameraInfo.Gimmick = Singleton<global::Studio.Studio>.Instance.workInfo.visibleGimmick;
		cameraInfo.imageCamera.sprite = cameraInfo.spriteCamera[Singleton<global::Studio.Studio>.Instance.workInfo.useAlt ? 1 : 0];
		mapInfo.Update();
	}

	public void OnClickMode(int _mode)
	{
		SortCanvas.select = modeInfo.canvasInput;
		Singleton<GuideObjectManager>.Instance.mode = _mode;
	}

	private void Instance_ModeChangeEvent(object sender, EventArgs e)
	{
		int mode = Singleton<GuideObjectManager>.Instance.mode;
		modeInfo.imageMode[0].sprite = modeInfo.spriteModeMove[(mode == 0) ? 1u : 0u];
		modeInfo.imageMode[1].sprite = modeInfo.spriteModeRotate[(mode == 1) ? 1u : 0u];
		modeInfo.imageMode[2].sprite = modeInfo.spriteModeScale[(mode == 2) ? 1u : 0u];
	}

	private bool NoCtrlCondition()
	{
		if (!Singleton<global::Studio.Studio>.IsInstance() || !Singleton<global::Studio.Studio>.Instance.workInfo.useAlt)
		{
			return false;
		}
		if (!Input.GetKey(KeyCode.LeftAlt))
		{
			return !Input.GetKey(KeyCode.RightAlt);
		}
		return false;
	}

	private bool KeyCondition()
	{
		if (!Singleton<global::Studio.Studio>.IsInstance())
		{
			return true;
		}
		return !Singleton<global::Studio.Studio>.Instance.isInputNow;
	}

	public void OnClickCamera()
	{
		bool flag = !Singleton<global::Studio.Studio>.Instance.workInfo.useAlt;
		Singleton<global::Studio.Studio>.Instance.workInfo.useAlt = flag;
		cameraInfo.imageCamera.sprite = cameraInfo.spriteCamera[flag ? 1 : 0];
	}

	public void OnClickSaveCamera(int _no)
	{
		Singleton<global::Studio.Studio>.Instance.sceneInfo.cameraData[_no] = cameraInfo.cameraCtrl.Export();
	}

	public void OnClickLoadCamera(int _no)
	{
		cameraInfo.cameraCtrl.Import(Singleton<global::Studio.Studio>.Instance.sceneInfo.cameraData[_no]);
	}

	public void OnClickCenter()
	{
		bool flag = !Singleton<global::Studio.Studio>.Instance.workInfo.visibleCenter;
		Singleton<global::Studio.Studio>.Instance.workInfo.visibleCenter = flag;
		cameraInfo.center = flag;
	}

	public void OnClickAxis()
	{
		bool flag = !Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxis;
		Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxis = flag;
		cameraInfo.axis = flag;
	}

	public void OnClickAxisTrans()
	{
		cameraInfo.axisTrans = !Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxisTranslation;
		Singleton<GuideObjectManager>.Instance.SetVisibleTranslation();
	}

	public void OnClickAxisCenter()
	{
		cameraInfo.axisCenter = !Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxisCenter;
		Singleton<GuideObjectManager>.Instance.SetVisibleCenter();
	}

	public void OnClickGimmick()
	{
		cameraInfo.Gimmick = !Singleton<global::Studio.Studio>.Instance.workInfo.visibleGimmick;
		Singleton<global::Studio.Studio>.Instance.SetVisibleGimmick();
	}

	public void OnClickTarget()
	{
	}

	public void OnClickUndo()
	{
		Singleton<UndoRedoManager>.Instance.Undo();
	}

	public void OnClickRedo()
	{
		Singleton<UndoRedoManager>.Instance.Redo();
	}

	private void Instance_CanUndoChange(object sender, EventArgs e)
	{
		buttonUndo.interactable = Singleton<UndoRedoManager>.Instance.CanUndo;
	}

	private void Instance_CanRedoChange(object sender, EventArgs e)
	{
		buttonRedo.interactable = Singleton<UndoRedoManager>.Instance.CanRedo;
	}

	private void ChangeScale()
	{
		float num = Input.GetAxis("Mouse X") * 0.1f;
		OptionCtrl.InputCombination inputSize = optionCtrl.inputSize;
		global::Studio.Studio.optionSystem.manipulateSize = Mathf.Clamp(global::Studio.Studio.optionSystem.manipulateSize + num, inputSize.min, inputSize.max);
		Singleton<GuideObjectManager>.Instance.SetScale();
		optionCtrl.UpdateUIManipulateSize();
	}

	public void OnClickMap()
	{
		mapInfo.active = !mapInfo.active;
	}

	private void CreatePatternList()
	{
		PatternSelectListCtrl pslc = Singleton<global::Studio.Studio>.Instance.patternSelectListCtrl;
		List<ListInfoBase> list = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_pattern).Values.ToList();
		list.ForEach(delegate(ListInfoBase info)
		{
			pslc.AddList(info.Id, info.Name, info.GetInfo(ChaListDefine.KeyType.ThumbAB), info.GetInfo(ChaListDefine.KeyType.ThumbTex));
		});
		pslc.AddOutside(list.Max((ListInfoBase l) => l.Id) + 1);
		pslc.Create(null);
	}

	public void OnClickInput()
	{
		inputInfo.active = !inputInfo.active;
	}

	private IEnumerator Start()
	{
		if (textAdjustment != null)
		{
			textAdjustment.font.material.mainTexture.filterMode = FilterMode.Point;
		}
		if (!Singleton<Info>.Instance.isLoadList)
		{
			yield return new WaitWhile(() => !AssetBundleManager.initialized);
			Wait.Active = true;
			yield return Singleton<Info>.Instance.LoadExcelDataCoroutine();
			yield return new WaitWhile(() => !Manager.Config.initialized);
			Wait.Active = false;
		}
		cameraInfo.Init();
		Studio.CameraControl cameraCtrl = cameraInfo.cameraCtrl;
		cameraCtrl.noCtrlCondition = (Studio.CameraControl.NoCtrlFunc)Delegate.Combine(cameraCtrl.noCtrlCondition, new Studio.CameraControl.NoCtrlFunc(NoCtrlCondition));
		Studio.CameraControl cameraCtrl2 = cameraInfo.cameraCtrl;
		cameraCtrl2.keyCondition = (Studio.CameraControl.NoCtrlFunc)Delegate.Combine(cameraCtrl2.keyCondition, new Studio.CameraControl.NoCtrlFunc(KeyCondition));
		Singleton<GuideObjectManager>.Instance.ModeChangeEvent += Instance_ModeChangeEvent;
		Instance_ModeChangeEvent(null, null);
		Singleton<UndoRedoManager>.Instance.CanRedoChange += Instance_CanRedoChange;
		Singleton<UndoRedoManager>.Instance.CanUndoChange += Instance_CanUndoChange;
		buttonUndo.interactable = false;
		buttonRedo.interactable = false;
		global::Studio.Studio instance = Singleton<global::Studio.Studio>.Instance;
		instance.onChangeMap = (Action)Delegate.Combine(instance.onChangeMap, new Action(mapInfo.OnChangeMap));
		CreatePatternList();
		inputInfo.Init();
		GuideInput guideInput = Singleton<GuideObjectManager>.Instance.guideInput;
		guideInput.onVisible = (GuideInput.OnVisible)Delegate.Combine(guideInput.onVisible, new GuideInput.OnVisible(inputInfo.OnVisible));
		inputInfo.active = false;
		optionCtrl.Init();
		Singleton<global::Studio.Studio>.Instance.Init();
		Singleton<global::Studio.Studio>.Instance.cameraCtrl.SetCamera(new Vector3(0f, 10f, 0f), Quaternion.Euler(15f, 180f, 0f), 25f);
		Camera main = Camera.main;
		if (main != null)
		{
			main.backgroundColor = Manager.Config.GraphicData.BackColor;
		}
		Manager.Sound.Listener = main?.transform;
		UpdateUI();
		(from _ in this.UpdateAsObservable()
			where !cameraInfo.cameraCtrl.isControlNow
			where Input.GetKey(KeyCode.B) && Input.GetMouseButton(1)
			select _).Subscribe(delegate
		{
			ChangeScale();
		}).AddTo(this);
		if (global::Studio.Studio.optionSystem.startupLoad)
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "StudioSceneLoad",
				isAdd = true
			}, isLoadingImageDraw: false);
		}
	}
}
