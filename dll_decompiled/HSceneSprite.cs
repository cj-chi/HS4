using System;
using System.Collections;
using System.Collections.Generic;
using AIChara;
using Config;
using Manager;
using SceneAssist;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEx;

public class HSceneSprite : Singleton<HSceneSprite>
{
	public enum FadeKind
	{
		Out,
		In,
		OutIn
	}

	public enum FadeKindProc
	{
		None,
		Out,
		OutEnd,
		In,
		InEnd,
		OutIn,
		OutInEnd
	}

	public Toggle objGaugeLockF;

	public Toggle objGaugeLockM;

	public Image imageMGauge;

	public Image imageFGauge;

	public Sprite spMGauge;

	public Sprite spFGauge;

	public Sprite spMGaugeHit;

	public Sprite spFGaugeHit;

	public GameObject buttonEnd;

	public HSceneSpriteFinishCategory categoryFinish;

	public CanvasGroup objClothPanel;

	public HSceneSpriteClothCondition objCloth;

	public HSceneSpriteAccessoryCondition objAccessory;

	public HSceneSpriteCoordinatesCard objClothCard;

	public HSceneSpriteChaChoice charaChoice;

	public HsceneSpriteTaiiCategory categoryMain;

	public PointerDownAction categoryMainDown;

	public GameObject categoryOption;

	public GameObject objMotionListPanel;

	public GameObject objMotionListInstanceButton;

	public GameObject objMotionList;

	public GameObject objLight;

	public HSceneSpriteLightCategory categoryLightDir;

	public AnimationCurve fadeAnimation;

	public HScene.AnimationListInfo StartAnimInfo;

	public RawImage imageFade;

	public float timeFadeBase;

	public bool isFade;

	private HSceneFlagCtrl ctrlFlag;

	private HScene hScene;

	private FadeKind kindFade;

	private FadeKindProc kindFadeProc;

	private float timeFade;

	private float timeFadeTime;

	private ChaControl[] chaFemales;

	private ChaControl[] chaMales;

	public bool usePoint;

	private List<HScene.AnimationListInfo>[] lstAnimInfo;

	private RotationScroll CategoryScroll;

	private List<int> lstFinishVisible = new List<int>();

	private HScene.LightInfo infoMapLight = new HScene.LightInfo();

	[SerializeField]
	private CanvasGroup UIGroup;

	private HSceneManager hSceneManager;

	[SerializeField]
	private int PlayerSex;

	private bool[] canMainCategory = new bool[7];

	private HPointCtrl hPointCtrl;

	public bool ChangeStart;

	private RectTransform rtSelect;

	public RectTransform[] rtScrollViewCalcs = new RectTransform[2];

	public Scrollbar sbMotionList;

	public GameObject objCharaPosition;

	public HSceneSpriteCategory categoryCharaPosition;

	public PointerAction CharaPositionSlider;

	public PointerDownAction[] CharaPositionAction = new PointerDownAction[3];

	public GameObject objItem;

	public GameObject objSystem;

	[SerializeField]
	private GameObject HelpBaseConfig;

	[SerializeField]
	private HOptionButton[] optionClothBts;

	[SerializeField]
	private HOptionButton optionItemBt;

	[SerializeField]
	private HOptionButton optionLightBt;

	[SerializeField]
	private HOptionButton optionSystemBt;

	[SerializeField]
	private HOptionButton optionConfigBt;

	[SerializeField]
	[Tooltip("ゲージの表示切替で消すもの")]
	private GameObject GuageBase;

	[SerializeField]
	private GameObject HelpBase;

	[SerializeField]
	private Text HelpTxt;

	private const string HelpTextDef = "ホイールで開始";

	private string[][] GuidTiming = new string[5][]
	{
		new string[2] { "Idle", "D_Idle" },
		new string[7] { "WLoop", "SLoop", "MLoop", "OLoop", "D_WLoop", "D_SLoop", "D_OLoop" },
		new string[2] { "Orgasm_IN_A", "D_Orgasm_IN_A" },
		new string[7] { "Orgasm_A", "Orgasm_OUT_A", "Drink_A", "Vomit_A", "OrgasmM_OUT_A", "D_Orgasm_A", "D_OrgasmM_OUT_A" },
		new string[3] { "WIdle", "SIdle", "D_Orgasm_A" }
	};

	private PointerAction MotionPointer;

	[SerializeField]
	private PointerAction MotionBack;

	[SerializeField]
	private PointerAction MotionScrollBar;

	[SerializeField]
	private Button objGaugeBG;

	[SerializeField]
	private PointerEnterTrigger objGaugeBGEnterTrigger;

	[SerializeField]
	private PointerExitTrigger objGaugeBGExitTrigger;

	[SerializeField]
	private PointerDownAction objGaugeBGDown;

	[SerializeField]
	private PointerDownAction[] objGaugeLockDown;

	[SerializeField]
	private PointerDownAction endbtDown;

	[SerializeField]
	private HColorPickerCtrl pickerCtrl;

	[SerializeField]
	private Image lightColorSample;

	private List<int[]> JudgeEventPtn = new List<int[]>();

	private int EventNo = -1;

	private int EventPeep = -1;

	private bool ResistPain;

	private bool LimitResistPain;

	public GameObject[] EventHideUIs;

	private bool eventHide;

	public int ClothMode = -1;

	private List<Toggle> tglMotions = new List<Toggle>();

	public IEnumerator Init()
	{
		ResistPain = false;
		LimitResistPain = false;
		SetHelpActive(_active: false);
		ctrlFlag = Singleton<HSceneFlagCtrl>.Instance;
		hScene = ctrlFlag.GetComponent<HScene>();
		hSceneManager = Singleton<HSceneManager>.Instance;
		EventNo = Singleton<Game>.Instance.eventNo;
		EventPeep = Singleton<Game>.Instance.peepKind;
		JudgeEventPtn = new List<int[]>();
		tglMotions.Clear();
		charaChoice.Init();
		charaChoice.SetDownAction(OnClickSliderSelect);
		charaChoice.SetMale(val: false);
		objCloth.Init();
		objCloth.SetDownAction(OnClickSliderSelect);
		objAccessory.Init();
		objAccessory.SetDownAction(OnClickSliderSelect);
		objClothCard.Init();
		objClothCard.SetDownAction(OnClickSliderSelect);
		categoryMain.Init();
		CategoryScroll = categoryMain.GetHScroll();
		ClothMode = -1;
		if (hSceneManager.player != null)
		{
			PlayerSex = hSceneManager.player.sex;
			if (PlayerSex == 1 && hSceneManager.bFutanari)
			{
				PlayerSex = 0;
			}
		}
		hPointCtrl = Singleton<HPointCtrl>.Instance;
		MotionBack.listDownAction.Add(delegate
		{
			OnClickSliderSelect();
		});
		MotionScrollBar.listDownAction.Add(delegate
		{
			OnClickSliderSelect();
		});
		PointerAction[] componentsInChildren = objSystem.GetComponentsInChildren<PointerAction>(includeInactive: true);
		foreach (PointerAction pointerAction in componentsInChildren)
		{
			if (!pointerAction.listDownAction.Contains(OnClickSliderSelect))
			{
				pointerAction.listDownAction.Add(delegate
				{
					OnClickSliderSelect();
				});
			}
		}
		if (!CharaPositionSlider.listDownAction.Contains(OnClickSliderSelect))
		{
			CharaPositionSlider.listDownAction.Add(delegate
			{
				OnClickSliderSelect();
			});
		}
		PointerDownAction[] charaPositionAction = CharaPositionAction;
		foreach (PointerDownAction pointerDownAction in charaPositionAction)
		{
			if (!(pointerDownAction == null) && !pointerDownAction.listAction.Contains(OnClickSliderSelect))
			{
				pointerDownAction.listAction.Add(OnClickSliderSelect);
			}
		}
		foreach (Button item in categoryLightDir.lstButton)
		{
			PointerAction component = item.GetComponent<PointerAction>();
			if (!(component == null) && !component.listDownAction.Contains(OnClickSliderSelect))
			{
				component.listDownAction.Add(delegate
				{
					OnClickSliderSelect();
				});
			}
		}
		charaPositionAction = categoryLightDir.downActions;
		foreach (PointerDownAction pointerDownAction2 in charaPositionAction)
		{
			if (!(pointerDownAction2 == null) && !pointerDownAction2.listAction.Contains(OnClickSliderSelect))
			{
				pointerDownAction2.listAction.Add(OnClickSliderSelect);
			}
		}
		UITrigger.TriggerEvent triggerEvent = new UITrigger.TriggerEvent();
		objGaugeBGEnterTrigger.Triggers.Clear();
		objGaugeBGEnterTrigger.Triggers.Add(triggerEvent);
		triggerEvent.AddListener(delegate
		{
			categoryFinish.onEnter = true;
		});
		triggerEvent = new UITrigger.TriggerEvent();
		objGaugeBGExitTrigger.Triggers.Clear();
		objGaugeBGExitTrigger.Triggers.Add(triggerEvent);
		triggerEvent.AddListener(delegate
		{
			categoryFinish.onEnter = false;
		});
		SetSelectSlider();
		optionItemBt.gameObject.SetActive(SaveData.IsAchievementExchangeRelease(9));
		eventHide = false;
		eventHide = EventNo == 3 || EventNo == 4 || EventNo == 5 || EventNo == 6 || EventNo == 28 || EventNo == 29 || EventNo == 30 || EventNo == 31;
		if (eventHide)
		{
			eventHide &= EventPeep == 0 || EventPeep == 1;
		}
		if (EventHideUIs != null && EventHideUIs.Length != 0)
		{
			GameObject[] eventHideUIs = EventHideUIs;
			foreach (GameObject gameObject in eventHideUIs)
			{
				if (!(gameObject == null))
				{
					gameObject.SetActive(!eventHide);
				}
			}
		}
		if (UIGroup != null)
		{
			UIGroup.alpha = 1f;
			UIGroup.blocksRaycasts = true;
			UIGroup.interactable = true;
		}
		yield return null;
	}

	private void SetSelectSlider()
	{
		if (objGaugeBGDown != null && !objGaugeBGDown.listAction.Contains(OnClickSliderSelectFinish))
		{
			objGaugeBGDown.listAction.Add(OnClickSliderSelectFinish);
		}
		HOptionButton[] array = optionClothBts;
		foreach (HOptionButton hOptionButton in array)
		{
			if (!(hOptionButton.downAction == null) && !hOptionButton.downAction.listAction.Contains(OnClickSliderSelect))
			{
				hOptionButton.downAction.listAction.Add(OnClickSliderSelect);
			}
		}
		HSceneSpriteItem hSceneSpriteItem = hScene.HSceneSpriteItem;
		if (hSceneSpriteItem != null && hSceneSpriteItem.downActions != null)
		{
			PointerDownAction[] downActions = hSceneSpriteItem.downActions;
			foreach (PointerDownAction pointerDownAction in downActions)
			{
				if (!(pointerDownAction == null) && !pointerDownAction.listAction.Contains(OnClickSliderSelect))
				{
					pointerDownAction.listAction.Add(OnClickSliderSelect);
				}
			}
		}
		if (optionItemBt.downAction != null && !optionItemBt.downAction.listAction.Contains(OnClickSliderSelect))
		{
			optionItemBt.downAction.listAction.Add(OnClickSliderSelect);
		}
		if (optionLightBt.downAction != null && !optionLightBt.downAction.listAction.Contains(OnClickSliderSelect))
		{
			optionLightBt.downAction.listAction.Add(OnClickSliderSelect);
		}
		if (optionSystemBt.downAction != null && !optionSystemBt.downAction.listAction.Contains(OnClickSliderSelect))
		{
			optionSystemBt.downAction.listAction.Add(OnClickSliderSelect);
		}
		if (optionConfigBt.downAction != null && !optionConfigBt.downAction.listAction.Contains(OnClickSliderSelect))
		{
			optionConfigBt.downAction.listAction.Add(OnClickSliderSelect);
		}
		if (categoryMainDown != null && !categoryMainDown.listAction.Contains(OnClickSliderSelect))
		{
			categoryMainDown.listAction.Add(OnClickSliderSelect);
		}
		if (objGaugeLockDown != null)
		{
			PointerDownAction[] downActions = objGaugeLockDown;
			foreach (PointerDownAction pointerDownAction2 in downActions)
			{
				if (!pointerDownAction2.listAction.Contains(OnClickSliderSelect))
				{
					pointerDownAction2.listAction.Add(OnClickSliderSelect);
				}
			}
		}
		if (endbtDown != null && !endbtDown.listAction.Contains(OnClickSliderSelect))
		{
			endbtDown.listAction.Add(OnClickSliderSelect);
		}
	}

	private void Update()
	{
		if (chaFemales == null)
		{
			return;
		}
		if (!eventHide)
		{
			GuageBase.SetActive(Manager.Config.HData.FeelingGauge);
		}
		else if (GuageBase.activeSelf)
		{
			GuageBase.SetActive(value: false);
		}
		if (!eventHide)
		{
			HelpBaseActive(Manager.Config.HData.ActionGuide);
		}
		else if (HelpBaseConfig.activeSelf)
		{
			HelpBaseActive(_active: false);
		}
		int item = ctrlFlag.nowAnimationInfo.ActionCtrl.Item1;
		int item2 = ctrlFlag.nowAnimationInfo.ActionCtrl.Item2;
		if (imageFGauge != null)
		{
			bool flag = item == 1;
			flag = flag || (item == 5 && (item2 == 1 || item2 == 2));
			flag = flag || (item == 6 && (item2 == 1 || item2 == 2));
			imageFGauge.fillAmount = ctrlFlag.feel_f;
			if (ctrlFlag.isGaugeHit && imageFGauge.sprite != spFGaugeHit && !flag)
			{
				imageFGauge.sprite = spFGaugeHit;
			}
			else if (!ctrlFlag.isGaugeHit && imageFGauge.sprite != spFGauge)
			{
				imageFGauge.sprite = spFGauge;
			}
		}
		if (imageMGauge != null)
		{
			imageMGauge.fillAmount = ctrlFlag.feel_m;
			if (ctrlFlag.isGaugeHit_M && imageMGauge.sprite != spMGaugeHit)
			{
				imageMGauge.sprite = spMGaugeHit;
			}
			else if (!ctrlFlag.isGaugeHit_M && imageMGauge.sprite != spMGauge)
			{
				imageMGauge.sprite = spMGauge;
			}
		}
		FadeProc();
		if (ctrlFlag.nowOrgasm)
		{
			OrgasmCloseUI();
		}
		bool flag2 = !ctrlFlag.nowOrgasm;
		if (hScene.GetProcBase() is Sonyu sonyu)
		{
			flag2 &= !sonyu.nowInsert;
		}
		categoryMain.SetEnable(flag2);
		if (categoryFinish != null)
		{
			objGaugeBG.interactable = categoryFinish.ActiveID != -1;
		}
		if (!(hScene != null) || !objMotionListPanel.activeSelf)
		{
			return;
		}
		bool flag3 = hScene.NowChangeAnim || ChangeStart;
		foreach (Toggle tglMotion in tglMotions)
		{
			if (flag3)
			{
				if (tglMotion.interactable)
				{
					tglMotion.interactable = false;
				}
			}
			else if (!tglMotion.interactable)
			{
				tglMotion.interactable = true;
			}
		}
	}

	private void OrgasmCloseUI()
	{
		if (objLight != null && objLight.activeSelf)
		{
			objLight.SetActive(value: false);
			optionLightBt.useClicked = false;
		}
		if (objSystem != null && objSystem.activeSelf)
		{
			objSystem.SetActive(value: false);
			optionSystemBt.useClicked = false;
		}
		if (pickerCtrl != null && pickerCtrl.isOpen)
		{
			pickerCtrl.Close();
		}
	}

	public void UIHide()
	{
		if (UIGroup != null)
		{
			if (UIGroup.alpha > 0f)
			{
				UIGroup.alpha = 0f;
				UIGroup.blocksRaycasts = false;
				UIGroup.interactable = false;
			}
			else
			{
				UIGroup.alpha = 1f;
				UIGroup.blocksRaycasts = true;
				UIGroup.interactable = true;
			}
		}
	}

	public void SetLightInfo(HScene.LightInfo _info)
	{
		if (_info == null)
		{
			infoMapLight = null;
			return;
		}
		infoMapLight.objCharaLight = _info.objCharaLight;
		infoMapLight.light = _info.light;
		infoMapLight.initRot = _info.initRot;
		infoMapLight.initIntensity = _info.initIntensity;
		infoMapLight.initColor = _info.initColor;
		infoMapLight.sublights = _info.sublights;
		categoryLightDir.SetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, infoMapLight.initRot.eulerAngles.x / 360f, 0);
		categoryLightDir.SetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, infoMapLight.initRot.eulerAngles.y / 360f, 1);
		categoryLightDir.SetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, infoMapLight.initIntensity, 2);
		Toggle toggle = categoryLightDir.GetToggle();
		toggle?.onValueChanged.RemoveAllListeners();
		toggle?.onValueChanged.AddListener(delegate(bool val)
		{
			OnValueSubEnableChanged(val);
		});
		categoryLightDir.SetValueToggle(val: true);
		float value = categoryLightDir.GetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, 2);
		value = Mathf.Lerp(infoMapLight.minIntensity, infoMapLight.maxIntensity, value);
		Color initColor = infoMapLight.initColor;
		if (initColor.a < 1f)
		{
			initColor.a = 1f;
		}
		lightColorSample.color = initColor;
	}

	public bool IsSpriteOver()
	{
		EventSystem current = EventSystem.current;
		if (current == null)
		{
			return false;
		}
		return current.IsPointerOverGameObject();
	}

	public void setAnimationList(List<HScene.AnimationListInfo>[] _lstAnimInfo)
	{
		lstAnimInfo = _lstAnimInfo;
	}

	public void Setting(ChaControl[] _females, ChaControl[] _males)
	{
		chaFemales = _females;
		chaMales = _males;
		ResistPain = chaFemales[0].fileGameInfo2.resistPain >= 100;
		ResistPain |= chaFemales[0].fileParam2.hAttribute == 3;
		categoryFinish.SetActive(_active: false);
		SetAnimationMenu();
		PointerAction[] componentsInChildren = categoryMain.GetComponentsInChildren<PointerAction>(includeInactive: true);
		foreach (PointerAction obj in componentsInChildren)
		{
			obj.listDownAction.Clear();
			obj.listDownAction.Add(OnClickSliderSelect);
		}
		bool flag = EventNo == 7 || EventNo == 32;
		if (!flag)
		{
			flag = chaFemales[0].fileGameInfo2.nowDrawState == ChaFileDefine.State.Broken;
		}
		hScene.HSceneSpriteItem.SetRecoverEnable(!flag);
	}

	public void RefleshAutoButtom()
	{
		int num = 0;
		for (int i = 0; i < lstAnimInfo.Length; i++)
		{
			for (int j = 0; j < lstAnimInfo[i].Count; j++)
			{
				if (CheckAutoMotionLimit(lstAnimInfo[i][j]))
				{
					num++;
				}
			}
		}
		categoryMain.SetActive(_active: true);
		PointerAction[] componentsInChildren = categoryMain.GetComponentsInChildren<PointerAction>(includeInactive: true);
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].listDownAction.Add(OnClickSliderSelect);
		}
	}

	public void SetFinishSelect(int _mode, int _ctrl, int infomode = -1, int infoctrl = -1)
	{
		switch (_mode)
		{
		case 0:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			break;
		case 1:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			if (ctrlFlag.initiative != 2)
			{
				lstFinishVisible.Add(1);
				if (_ctrl == 1 && !ctrlFlag.isFaintness)
				{
					lstFinishVisible.Add(3);
					lstFinishVisible.Add(4);
				}
				else if (_ctrl == 2)
				{
					lstFinishVisible.Add(3);
					lstFinishVisible.Add(4);
				}
			}
			break;
		case 2:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			if (ctrlFlag.initiative != 2)
			{
				lstFinishVisible.Add(1);
				lstFinishVisible.Add(5);
				if ((infomode == 2 && infoctrl == 0) || (infomode == 3 && (infoctrl == 1 || infoctrl == 7)))
				{
					lstFinishVisible.Add(2);
				}
				else if (infomode == 3 && _ctrl == 0 && ctrlFlag.isFaintness)
				{
					categoryFinish.SetActive(_active: false);
					lstFinishVisible.Clear();
				}
			}
			break;
		case 3:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			break;
		case 4:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			break;
		case 5:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			break;
		case 6:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			break;
		case 7:
		case 8:
			categoryFinish.SetActive(_active: false);
			lstFinishVisible.Clear();
			switch (_ctrl)
			{
			case 1:
			case 2:
				lstFinishVisible.Add(1);
				if (_ctrl == 2 && !ctrlFlag.isFaintness)
				{
					lstFinishVisible.Add(3);
					lstFinishVisible.Add(4);
				}
				break;
			case 3:
			case 4:
				lstFinishVisible.Add(1);
				lstFinishVisible.Add(5);
				if (_ctrl == 3)
				{
					lstFinishVisible.Add(2);
				}
				break;
			}
			break;
		}
	}

	public bool IsFinishVisible(int _num)
	{
		return lstFinishVisible.Contains(_num);
	}

	public void SetEnableCategoryMain(bool _enable, int _array = -1)
	{
		if (_array < 0)
		{
			categoryMain.SetEnable(_enable);
		}
		else if (categoryMain.lstButton.Count > _array)
		{
			categoryMain.SetEnable(_enable, _array);
		}
	}

	public void SetVisibleLeaveItToYou(bool _visible, bool _judgeLeaveItToYou = false)
	{
		SetAnimationMenu();
		CategoryScroll.ListNodeSet(null, targetInit: false);
	}

	public void MainCategoryOfLeaveItToYou(bool _isLeaveItToYou)
	{
		if (!_isLeaveItToYou)
		{
			SetAnimationMenu();
			return;
		}
		bool flag = true;
		for (int i = 0; i < lstAnimInfo.Length; i++)
		{
			int num = 0;
			for (int j = 0; j < lstAnimInfo[i].Count; j++)
			{
				if (chaFemales[1] != null)
				{
					if (i < 4)
					{
						continue;
					}
					if (PlayerSex == 0)
					{
						if (i != 5)
						{
							continue;
						}
					}
					else if (i != 4)
					{
						continue;
					}
				}
				else if (i > 3)
				{
					continue;
				}
				if (ctrlFlag.initiative == 1)
				{
					if (lstAnimInfo[i][j].nInitiativeFemale != 1 && (!flag || lstAnimInfo[i][j].nInitiativeFemale != 2))
					{
						continue;
					}
				}
				else if (ctrlFlag.initiative != 2 || lstAnimInfo[i][j].nInitiativeFemale != 2)
				{
					continue;
				}
				num++;
			}
			if (PlayerSex != -1)
			{
				categoryMain.SetActive(num != 0, i);
			}
		}
		if (PlayerSex == -1)
		{
			categoryMain.SetActive(_active: true);
		}
		if (objMotionListPanel != null && objMotionListPanel.activeSelf)
		{
			SlideOpen component = categoryMain.GetComponent<SlideOpen>();
			if (component != null)
			{
				component.Close();
			}
		}
		CategoryScroll.ListNodeSet(null, targetInit: false);
	}

	public bool IsEnableLeaveItToYou()
	{
		if (!categoryMain.GetActiveButton()[7])
		{
			return false;
		}
		return true;
	}

	public void OnChangePlaySelect(GameObject objClick)
	{
		if (hScene.NowChangeAnim || ChangeStart)
		{
			return;
		}
		foreach (Toggle tglMotion in tglMotions)
		{
			if (tglMotion.interactable)
			{
				tglMotion.interactable = false;
			}
		}
		if (null != objClick)
		{
			HAnimationInfoComponent component = objClick.GetComponent<HAnimationInfoComponent>();
			if (null != component && ctrlFlag.nowAnimationInfo != component.info)
			{
				ctrlFlag.selectAnimationListInfo = component.info;
				ChangeStart = true;
			}
			objClick.GetComponent<Toggle>().isOn = true;
		}
	}

	public void OnChangePlaySelect(Toggle objClick)
	{
		if (!(null != objClick))
		{
			return;
		}
		Text[] componentsInChildren = objClick.GetComponentsInChildren<Text>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			Text[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = (objClick.isOn ? Game.selectFontColor : Game.defaultFontColor);
			}
		}
	}

	public void OnClickMotion(int _motion)
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || isFade)
		{
			return;
		}
		if (objMotionListPanel.activeSelf)
		{
			if (ctrlFlag.categoryMotionList == _motion)
			{
				SetMotionListDraw(_active: false);
				Observable.NextFrame().Subscribe(delegate
				{
					SlideOpen component = objMotionListPanel.GetComponent<SlideOpen>();
					if (component != null)
					{
						component.Close();
					}
				});
			}
			else
			{
				SetMotionListDraw(_active: true, _motion);
			}
			return;
		}
		SetMotionListDraw(_active: true, _motion);
		Observable.NextFrame().Subscribe(delegate
		{
			SlideOpen component = objMotionListPanel.GetComponent<SlideOpen>();
			if (component != null)
			{
				component.Open();
			}
		});
	}

	public void OnClickMotionFemale()
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || isFade)
		{
			return;
		}
		if (objMotionListPanel.activeSelf)
		{
			if (ctrlFlag.categoryMotionList == 7)
			{
				SlideOpen component = objMotionListPanel.GetComponent<SlideOpen>();
				if (component != null)
				{
					component.Close();
				}
				SetMotionListDraw(_active: false);
			}
			else
			{
				SetMotionListDraw(_active: true, 7);
			}
			return;
		}
		SetMotionListDraw(_active: true, 7);
		Observable.NextFrame().Subscribe(delegate
		{
			SlideOpen component2 = objMotionListPanel.GetComponent<SlideOpen>();
			if (component2 != null)
			{
				component2.Open();
			}
		});
	}

	public void ChangeMotionCategoryShow(int category)
	{
		if (!objMotionListPanel.activeSelf)
		{
			return;
		}
		SetMotionListDraw(_active: true, category);
		Observable.NextFrame().Subscribe(delegate
		{
			SlideOpen component = objMotionListPanel.GetComponent<SlideOpen>();
			if (component != null)
			{
				component.SetViewSize();
			}
		});
	}

	public void SetMotionListDraw(bool _active, int _motion = -1)
	{
		ctrlFlag.categoryMotionList = _motion;
		if (_active)
		{
			LoadMotionList(ctrlFlag.categoryMotionList);
		}
	}

	public void OnClickFinish()
	{
		if (!(categoryFinish == null))
		{
			switch (categoryFinish.GetlstActive())
			{
			case 0:
				OnClickFinishBefore();
				break;
			case 1:
				OnClickFinishOutSide();
				break;
			case 2:
				OnClickFinishInSide();
				break;
			case 3:
				OnClickFinishDrink();
				break;
			case 4:
				OnClickFinishVomit();
				break;
			case 5:
				OnClickFinishSame();
				break;
			}
		}
	}

	public void OnClickFinishBefore()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.FinishBefore;
		}
	}

	public void OnClickFinishInSide()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.FinishInSide;
		}
	}

	public void OnClickFinishOutSide()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.FinishOutSide;
		}
	}

	public void OnClickFinishSame()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.FinishSame;
		}
	}

	public void OnClickFinishDrink()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.FinishDrink;
		}
	}

	public void OnClickFinishVomit()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.FinishVomit;
		}
	}

	public void OnClickRecover()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.RecoverFaintness;
		}
	}

	public void OnClickSpanking()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.Spnking;
		}
	}

	public void OnClickSceneEnd()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade && !ConfirmDialog.active && ctrlFlag.click != HSceneFlagCtrl.ClickKind.SceneEnd)
		{
			ConfirmDialog.Status status = ConfirmDialog.status;
			if (eventHide)
			{
				status.Sentence = "覗きを終了しますか？";
			}
			else
			{
				status.Sentence = "Hシーンを終了しますか？";
			}
			status.YesText = "終了する";
			status.NoText = "続ける";
			status.Yes = delegate
			{
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
			};
			status.No = delegate
			{
			};
			ConfirmDialog.Load();
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		}
	}

	public void OnClickStopFeel(int _sex)
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade)
		{
			if (_sex == 0)
			{
				ctrlFlag.stopFeelMale = !ctrlFlag.stopFeelMale;
			}
			else
			{
				ctrlFlag.stopFeelFemale = !ctrlFlag.stopFeelFemale;
			}
		}
	}

	public void OnClickMovePoint(int _dir)
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade)
		{
			ctrlFlag.click = ((_dir == 0) ? HSceneFlagCtrl.ClickKind.MovePointNext : HSceneFlagCtrl.ClickKind.MovePointBack);
		}
	}

	public void OnClickMoveBt()
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade)
		{
			bool activeSelf = objCharaPosition.activeSelf;
			objCharaPosition.SetActive(!activeSelf);
		}
	}

	public void OnClickPositionShift(int _shift)
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade && categoryCharaPosition.GetAllEnable() == 1 && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
		{
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
			ctrlFlag.cameraCtrl.isCursorLock = false;
			switch (_shift)
			{
			case 0:
				ctrlFlag.kindCharaCtrl = HSceneFlagCtrl.CharaCtrlKind.Parallel;
				break;
			case 1:
				ctrlFlag.kindCharaCtrl = HSceneFlagCtrl.CharaCtrlKind.Height;
				break;
			case 2:
				ctrlFlag.kindCharaCtrl = HSceneFlagCtrl.CharaCtrlKind.Rotation;
				break;
			}
		}
	}

	public void OnClickPositionShiftInit(int _shift)
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade && categoryCharaPosition.GetAllEnable() == 1)
		{
			switch (_shift)
			{
			case 0:
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.ParallelShiftInit;
				break;
			case 1:
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.VerticalShiftInit;
				break;
			case 2:
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.RotationShiftInit;
				break;
			}
		}
	}

	public void OnClickLeave()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade && IsEnableLeaveItToYou() && Input.GetMouseButtonUp(0))
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.LeaveItToYou;
		}
	}

	public void OnClickItem()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			bool activeSelf = objItem.activeSelf;
			objItem.SetActive(!activeSelf);
			objSystem.SetActive(value: false);
			objLight.SetActive(value: false);
			ClothPanelClose();
			optionItemBt.useClicked = !activeSelf;
			optionLightBt.useClicked = false;
			optionSystemBt.useClicked = false;
			optionConfigBt.useClicked = false;
			if (pickerCtrl != null && pickerCtrl.isOpen)
			{
				pickerCtrl.Close();
			}
		}
	}

	public void OnClickCloth(int mode)
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || isFade)
		{
			return;
		}
		bool flag = objClothPanel.alpha == 0f;
		CanvasGroup component = objCloth.GetComponent<CanvasGroup>();
		CanvasGroup component2 = objAccessory.GetComponent<CanvasGroup>();
		CanvasGroup component3 = objClothCard.GetComponent<CanvasGroup>();
		CanvasGroup[] array = new CanvasGroup[3] { component, component2, component3 };
		int num = 0;
		if (!flag)
		{
			if (component != null && component.alpha == 1f)
			{
				num = 0;
			}
			else if (component2 != null && component2.alpha == 1f)
			{
				num = 1;
			}
			else if (component3 != null && component3.alpha == 1f)
			{
				num = 2;
			}
			if (mode == num)
			{
				objClothPanel.alpha = 0f;
				objClothPanel.blocksRaycasts = false;
				if (array[mode] != null)
				{
					array[mode].alpha = 0f;
					array[mode].blocksRaycasts = false;
					optionClothBts[mode].useClicked = false;
				}
				ClothMode = -1;
			}
			else
			{
				for (int i = 0; i < 3; i++)
				{
					if (array[i] == null)
					{
						continue;
					}
					if (mode == i)
					{
						array[i].alpha = 1f;
						array[i].blocksRaycasts = true;
						optionClothBts[i].useClicked = true;
						switch (mode)
						{
						case 0:
							objCloth.SetClothCharacter();
							break;
						case 1:
							objAccessory.SetAccessoryCharacter();
							break;
						}
					}
					else
					{
						array[i].alpha = 0f;
						array[i].blocksRaycasts = false;
						optionClothBts[i].useClicked = false;
					}
					ClothMode = mode;
				}
			}
		}
		else
		{
			objClothPanel.alpha = 1f;
			objClothPanel.blocksRaycasts = true;
			if (array[mode] != null)
			{
				array[mode].alpha = 1f;
				array[mode].blocksRaycasts = true;
				optionClothBts[mode].useClicked = true;
				switch (mode)
				{
				case 0:
					objCloth.SetClothCharacter();
					break;
				case 1:
					objAccessory.SetAccessoryCharacter();
					break;
				}
			}
			ClothMode = mode;
		}
		objItem.SetActive(value: false);
		objSystem.SetActive(value: false);
		objLight.SetActive(value: false);
		optionItemBt.useClicked = false;
		optionLightBt.useClicked = false;
		optionSystemBt.useClicked = false;
		optionConfigBt.useClicked = false;
		if (pickerCtrl != null && pickerCtrl.isOpen)
		{
			pickerCtrl.Close();
		}
		if (mode == 2)
		{
			charaChoice.SetMale(val: true);
		}
		else
		{
			charaChoice.SetMale(val: false);
		}
	}

	public int OpenClothKind()
	{
		if (objClothPanel.alpha == 0f)
		{
			return -1;
		}
		CanvasGroup component = objCloth.GetComponent<CanvasGroup>();
		CanvasGroup component2 = objAccessory.GetComponent<CanvasGroup>();
		CanvasGroup component3 = objClothCard.GetComponent<CanvasGroup>();
		if (component != null && component.alpha == 1f)
		{
			return 0;
		}
		if (component2 != null && component2.alpha == 1f)
		{
			return 1;
		}
		if (component3 != null && component3.alpha == 1f)
		{
			return 2;
		}
		return -1;
	}

	public void ClothPanelClose()
	{
		CanvasGroup component = objCloth.GetComponent<CanvasGroup>();
		CanvasGroup component2 = objAccessory.GetComponent<CanvasGroup>();
		CanvasGroup component3 = objClothCard.GetComponent<CanvasGroup>();
		CanvasGroup[] array = new CanvasGroup[3] { component, component2, component3 };
		objClothPanel.alpha = 0f;
		objClothPanel.blocksRaycasts = false;
		for (int i = 0; i < 3; i++)
		{
			if (!(array[i] == null))
			{
				array[i].alpha = 0f;
				array[i].blocksRaycasts = false;
				optionClothBts[i].useClicked = false;
			}
		}
	}

	public void OnClickSystem()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade && !(objSystem == null) && !ctrlFlag.nowOrgasm)
		{
			bool activeSelf = objSystem.activeSelf;
			objSystem.SetActive(!activeSelf);
			objItem.SetActive(value: false);
			objLight.SetActive(value: false);
			ClothPanelClose();
			optionItemBt.useClicked = false;
			optionLightBt.useClicked = false;
			optionSystemBt.useClicked = !activeSelf;
			optionConfigBt.useClicked = false;
			if (pickerCtrl != null && pickerCtrl.isOpen)
			{
				pickerCtrl.Close();
			}
		}
	}

	public void OnClickLight()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			bool activeSelf = objLight.activeSelf;
			objLight.SetActive(!activeSelf);
			if (pickerCtrl != null && pickerCtrl.isOpen)
			{
				pickerCtrl.Close();
			}
			objItem.SetActive(value: false);
			objSystem.SetActive(value: false);
			ClothPanelClose();
			optionItemBt.useClicked = false;
			optionLightBt.useClicked = !activeSelf;
			optionSystemBt.useClicked = false;
			optionConfigBt.useClicked = false;
		}
	}

	public void OnClickLightColor()
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || isFade || pickerCtrl == null)
		{
			return;
		}
		if (pickerCtrl.isOpen && pickerCtrl.Check("ライト"))
		{
			pickerCtrl.Close();
			return;
		}
		Color color = infoMapLight.light.color;
		if (color.a < 1f)
		{
			color.a = 1f;
		}
		pickerCtrl.Open(color, delegate(Color col)
		{
			OnValueLightColorChanged(col);
		}, "ライト");
	}

	public void OnValueLightColorChanged(Color newColor)
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade && infoMapLight != null)
		{
			Color color = infoMapLight.light.color;
			color.r = newColor.r;
			color.g = newColor.g;
			color.b = newColor.b;
			color.a = 1f;
			infoMapLight.light.color = color;
			lightColorSample.color = infoMapLight.light.color;
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		}
	}

	public void OnValueLightDireChanged()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			float x = categoryLightDir.GetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, 0) * 360f;
			float y = categoryLightDir.GetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, 1) * 360f;
			if (infoMapLight != null)
			{
				infoMapLight.objCharaLight.transform.localRotation = Quaternion.Euler(x, y, 0f);
				GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
			}
		}
	}

	public void OnValuePowerChanged()
	{
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade && !isFade)
		{
			float value = categoryLightDir.GetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, 2);
			if (infoMapLight != null && !(infoMapLight.light == null))
			{
				infoMapLight.light.intensity = Mathf.Lerp(infoMapLight.minIntensity, infoMapLight.maxIntensity, value);
				GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
			}
		}
	}

	public void OnValueSubEnableChanged(bool val)
	{
		Light[] sublights = infoMapLight.sublights;
		foreach (Light light in sublights)
		{
			if (!(light == null))
			{
				light.gameObject.SetActive(val);
			}
		}
	}

	public void ReSetLight()
	{
		ReSetLightDir();
		ReSetLightColor();
		ReSetLightPower();
	}

	public void ReSetLightDir()
	{
		if (infoMapLight != null)
		{
			infoMapLight.objCharaLight.transform.localRotation = infoMapLight.initRot;
			categoryLightDir.SetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, infoMapLight.initRot.eulerAngles.x / 360f, 0);
			categoryLightDir.SetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, infoMapLight.initRot.eulerAngles.y / 360f, 1);
		}
	}

	public void ReSetLightDir(int mode)
	{
		if (infoMapLight != null)
		{
			Vector3 eulerAngles = infoMapLight.objCharaLight.transform.localRotation.eulerAngles;
			switch (mode)
			{
			case 0:
				eulerAngles.x = infoMapLight.initRot.eulerAngles.x;
				infoMapLight.objCharaLight.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
				categoryLightDir.SetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, infoMapLight.initRot.eulerAngles.x / 360f, 0);
				break;
			case 1:
				eulerAngles.y = infoMapLight.initRot.eulerAngles.y;
				infoMapLight.objCharaLight.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
				categoryLightDir.SetValue(HSceneSpriteLightCategory.CtrlLightMode.DIRPOWER, infoMapLight.initRot.eulerAngles.y / 360f, 1);
				break;
			}
		}
	}

	public void ReSetLightPower()
	{
		if (infoMapLight != null)
		{
			infoMapLight.light.intensity = Mathf.Lerp(infoMapLight.minIntensity, infoMapLight.maxIntensity, infoMapLight.initIntensity);
		}
	}

	public void ReSetLightColor()
	{
		if (infoMapLight == null)
		{
			return;
		}
		Color initColor = infoMapLight.initColor;
		if (initColor.a < 1f)
		{
			initColor.a = 1f;
		}
		infoMapLight.light.color = initColor;
		if (pickerCtrl.isOpen && pickerCtrl.Check("ライト"))
		{
			pickerCtrl.Open(infoMapLight.light.color, delegate(Color col)
			{
				OnValueLightColorChanged(col);
			}, "ライト");
		}
		lightColorSample.color = infoMapLight.light.color;
	}

	public void OnClickConfig()
	{
		objItem.SetActive(value: false);
		objSystem.SetActive(value: false);
		objLight.SetActive(value: false);
		ClothPanelClose();
		optionItemBt.useClicked = false;
		optionLightBt.useClicked = false;
		optionSystemBt.useClicked = false;
		optionConfigBt.useClicked = true;
		ConfigWindow.UnLoadAction = delegate
		{
			optionConfigBt.useClicked = false;
		};
		ConfigWindow.TitleChangeAction = delegate
		{
			ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
			hScene.NowStateIsEnd = true;
			hScene.ConfigEnd();
			ConfigWindow.UnLoadAction = null;
		};
		ConfigWindow.Load();
		GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
	}

	public void OnValuePositionMoveSpeed(float _value)
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade)
		{
			if (_value > 0.5f)
			{
				ctrlFlag.charaMoveSpeedAddRate = Mathf.Lerp(1f, 3f, Mathf.InverseLerp(0.5f, 1f, _value));
			}
			else
			{
				ctrlFlag.charaMoveSpeedAddRate = Mathf.Lerp(0.05f, 1f, Mathf.InverseLerp(0f, 0.5f, _value));
			}
		}
	}

	public void OnClickSliderSelect()
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade)
		{
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		}
	}

	public void OnClickSliderSelectFinish()
	{
		if ((!SingletonInitializer<Scene>.initialized || (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)) && !isFade && categoryFinish.GetAllActive() != 0)
		{
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		}
	}

	public void FadeState(FadeKind _kind, float _timeFade = -1f)
	{
		isFade = true;
		timeFadeTime = 0f;
		if (_timeFade < 0f)
		{
			timeFade = ((_kind != FadeKind.OutIn) ? timeFadeBase : (timeFadeBase * 2f));
		}
		else
		{
			timeFade = ((_kind != FadeKind.OutIn) ? _timeFade : (_timeFade * 2f));
		}
		kindFade = _kind;
		switch (kindFade)
		{
		case FadeKind.Out:
			kindFadeProc = FadeKindProc.Out;
			break;
		case FadeKind.In:
			kindFadeProc = FadeKindProc.In;
			break;
		case FadeKind.OutIn:
			kindFadeProc = FadeKindProc.OutIn;
			break;
		}
	}

	public FadeKindProc GetFadeKindProc()
	{
		return kindFadeProc;
	}

	private bool FadeProc()
	{
		if (!imageFade)
		{
			return false;
		}
		if (!isFade)
		{
			return false;
		}
		timeFadeTime += Time.deltaTime;
		Color color = imageFade.color;
		float time = Mathf.Clamp01(timeFadeTime / timeFade);
		time = fadeAnimation.Evaluate(time);
		switch (kindFade)
		{
		case FadeKind.Out:
			color.a = time;
			break;
		case FadeKind.In:
			color.a = 1f - time;
			break;
		case FadeKind.OutIn:
			color.a = Mathf.Sin((float)Math.PI / 180f * Mathf.Lerp(0f, 180f, time));
			break;
		}
		imageFade.color = color;
		if (time >= 1f)
		{
			isFade = false;
			switch (kindFade)
			{
			case FadeKind.Out:
				kindFadeProc = FadeKindProc.OutEnd;
				break;
			case FadeKind.In:
				kindFadeProc = FadeKindProc.InEnd;
				break;
			case FadeKind.OutIn:
				kindFadeProc = FadeKindProc.OutInEnd;
				break;
			}
		}
		return true;
	}

	public bool LoadMotionList(int _motion)
	{
		tglMotions.Clear();
		for (int i = 0; i < objMotionList.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(objMotionList.transform.GetChild(i).gameObject);
		}
		JudgeEventPtn = HSceneManager.HResourceTables.AnimEventJudgePtn;
		if (!(hPointCtrl == null))
		{
			_ = hPointCtrl.HPointList == null;
		}
		bool flag = false;
		flag = EventNo == 58;
		if (_motion != 7)
		{
			if (_motion < 0 || lstAnimInfo.Length <= _motion)
			{
				return true;
			}
			rtSelect = null;
			for (int j = 0; j < lstAnimInfo[_motion].Count; j++)
			{
				if (lstAnimInfo[_motion][j].nInitiativeFemale != 0)
				{
					continue;
				}
				if (!flag)
				{
					if (!CheckMotionLimit(lstAnimInfo[_motion][j]))
					{
						continue;
					}
				}
				else if (!CheckMotionLimitAppend3P(lstAnimInfo[_motion][j]))
				{
					continue;
				}
				GameObject objClone = UnityEngine.Object.Instantiate(objMotionListInstanceButton);
				HAnimationInfoComponent hAnimationInfoComponent = objClone.GetComponent<HAnimationInfoComponent>();
				if (hAnimationInfoComponent == null)
				{
					hAnimationInfoComponent = objClone.AddComponent<HAnimationInfoComponent>();
				}
				objClone.SetActive(value: true);
				hAnimationInfoComponent.info = lstAnimInfo[_motion][j];
				objClone.transform.SetParent(objMotionList.transform, worldPositionStays: false);
				hAnimationInfoComponent.text.text = hAnimationInfoComponent.info.nameAnimation;
				MotionPointer = hAnimationInfoComponent.ptrAction;
				MotionPointer.listClickAction.Clear();
				MotionPointer.listDownAction.Clear();
				MotionPointer.listClickAction.Add(delegate
				{
					OnChangePlaySelect(objClone);
				});
				MotionPointer.listDownAction.Add(delegate
				{
					OnClickSliderSelect();
				});
				objClone.SetActive(value: true);
				Toggle tmpToggle = objClone.GetComponent<Toggle>();
				tmpToggle.onValueChanged.RemoveAllListeners();
				tmpToggle.onValueChanged.AddListener(delegate
				{
					OnChangePlaySelect(tmpToggle);
				});
				tglMotions.Add(tmpToggle);
				if (lstAnimInfo[_motion][j] == ctrlFlag.nowAnimationInfo)
				{
					tmpToggle.isOn = true;
					rtSelect = objClone.transform as RectTransform;
				}
			}
			if ((bool)rtSelect)
			{
				StartCoroutine(CalcScrollBarHitCoroutine());
			}
			else
			{
				StartCoroutine(CalcScrollBarInitCoroutine());
			}
		}
		else
		{
			rtSelect = null;
			for (int num = 0; num < lstAnimInfo.Length; num++)
			{
				for (int num2 = 0; num2 < lstAnimInfo[num].Count; num2++)
				{
					if (lstAnimInfo[num][num2].nInitiativeFemale != 0 && CheckMotionLimit(lstAnimInfo[num][num2]))
					{
						GameObject objClone2 = UnityEngine.Object.Instantiate(objMotionListInstanceButton);
						HAnimationInfoComponent hAnimationInfoComponent2 = objClone2.GetComponent<HAnimationInfoComponent>();
						if (hAnimationInfoComponent2 == null)
						{
							hAnimationInfoComponent2 = objClone2.AddComponent<HAnimationInfoComponent>();
						}
						hAnimationInfoComponent2.info = lstAnimInfo[num][num2];
						objClone2.transform.SetParent(objMotionList.transform, worldPositionStays: false);
						hAnimationInfoComponent2.text.text = hAnimationInfoComponent2.info.nameAnimation;
						MotionPointer = hAnimationInfoComponent2.ptrAction;
						MotionPointer.listClickAction.Clear();
						MotionPointer.listDownAction.Clear();
						MotionPointer.listClickAction.Add(delegate
						{
							OnChangePlaySelect(objClone2);
						});
						MotionPointer.listDownAction.Add(delegate
						{
							OnClickSliderSelect();
						});
						objClone2.SetActive(value: true);
						Toggle tmpToggle2 = objClone2.GetComponent<Toggle>();
						tmpToggle2.onValueChanged.RemoveAllListeners();
						tmpToggle2.onValueChanged.AddListener(delegate
						{
							OnChangePlaySelect(tmpToggle2);
						});
						tglMotions.Add(tmpToggle2);
						if (lstAnimInfo[num][num2] == ctrlFlag.nowAnimationInfo)
						{
							tmpToggle2.isOn = true;
							rtSelect = objClone2.transform as RectTransform;
						}
					}
				}
			}
			if ((bool)rtSelect)
			{
				StartCoroutine(CalcScrollBarHitCoroutine());
			}
			else
			{
				StartCoroutine(CalcScrollBarInitCoroutine());
			}
		}
		return true;
	}

	public bool SetAnimationMenu()
	{
		int[] array = new int[lstAnimInfo.Length];
		int num = 0;
		JudgeEventPtn = HSceneManager.HResourceTables.AnimEventJudgePtn;
		if (!(hPointCtrl == null))
		{
			_ = hPointCtrl.HPointList == null;
		}
		for (int i = 0; i < lstAnimInfo.Length; i++)
		{
			if (EventNo == 58 && i != 5)
			{
				continue;
			}
			for (int j = 0; j < lstAnimInfo[i].Count; j++)
			{
				if (CheckMotionLimit(lstAnimInfo[i][j]))
				{
					if (lstAnimInfo[i][j].nInitiativeFemale != 0)
					{
						num++;
					}
					else
					{
						array[i]++;
					}
				}
			}
		}
		for (int k = 0; k < canMainCategory.Length; k++)
		{
			canMainCategory[k] = array[k] != 0;
			categoryMain.SetActive(canMainCategory[k], k);
		}
		categoryMain.SetActive(num > 0, 7);
		if (PlayerSex == -1)
		{
			categoryMain.SetActive(_active: true);
		}
		CategoryScroll.ListNodeSet(null, targetInit: false);
		return true;
	}

	public bool CheckMotionLimit(HScene.AnimationListInfo lstAnimInfo)
	{
		if (chaFemales.Length > 1 && chaFemales[1] == null && (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5))
		{
			return false;
		}
		if (chaMales.Length > 1 && chaMales[1] == null && lstAnimInfo.ActionCtrl.Item1 == 6)
		{
			return false;
		}
		if (EventNo == 19)
		{
			if (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5 || lstAnimInfo.ActionCtrl.Item1 == 6)
			{
				return false;
			}
			if (lstAnimInfo.ActionCtrl.Item1 == 3 && lstAnimInfo.id == 0)
			{
				return false;
			}
		}
		if (!CheckEventLimit(lstAnimInfo.Event))
		{
			return false;
		}
		if (lstAnimInfo.ActionCtrl.Item1 == 3)
		{
			int item = lstAnimInfo.ActionCtrl.Item2;
			if (item == 5 || item == 6)
			{
				bool flag = false;
				foreach (int[] item2 in lstAnimInfo.Event)
				{
					if (item2[1] == -1)
					{
						if (item2[0] == EventNo)
						{
							flag = true;
						}
					}
					else if (item2[0] == EventNo && item2[1] == EventPeep)
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
		}
		if (!CheckPlace(lstAnimInfo))
		{
			return false;
		}
		if (!CheckEventPtn(JudgeEventPtn) && lstAnimInfo.Achievments != null && lstAnimInfo.Achievments.Count > 0 && !CheckAchievement(lstAnimInfo.Achievments))
		{
			return false;
		}
		switch (hSceneManager.FemaleState[0])
		{
		case ChaFileDefine.State.Blank:
			if (!lstAnimInfo.nStatePtns.Contains(0))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Favor:
			if (!lstAnimInfo.nStatePtns.Contains(1))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Enjoyment:
			if (!lstAnimInfo.nStatePtns.Contains(2))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Slavery:
			if (!lstAnimInfo.nStatePtns.Contains(3))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Aversion:
			if (!lstAnimInfo.nStatePtns.Contains(4))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Broken:
			if (!lstAnimInfo.nStatePtns.Contains(5))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Dependence:
			if (!lstAnimInfo.nStatePtns.Contains(6))
			{
				return false;
			}
			break;
		}
		bool flag2 = ctrlFlag.isFaintness && (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1);
		flag2 |= EventNo == 19;
		if (lstAnimInfo.nDownPtn == 0 && flag2)
		{
			return false;
		}
		if (lstAnimInfo.nFaintnessLimit == 1 && !flag2)
		{
			return false;
		}
		if ((ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1) && lstAnimInfo.ActionCtrl.Item1 == 4 && lstAnimInfo.ActionCtrl.Item2 != 1)
		{
			return false;
		}
		if (LimitResistPain && !ResistPain && ctrlFlag.feel_f >= 0.75f && lstAnimInfo.lstSystem.Contains(4))
		{
			return false;
		}
		if (!CheckAppendEV(lstAnimInfo, EventNo))
		{
			return false;
		}
		return true;
	}

	public bool CheckMotionLimitRecover(HScene.AnimationListInfo lstAnimInfo)
	{
		if (chaFemales.Length > 1 && chaFemales[1] == null && (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5))
		{
			return false;
		}
		if (chaMales.Length > 1 && chaMales[1] == null && lstAnimInfo.ActionCtrl.Item1 == 6)
		{
			return false;
		}
		if (EventNo == 19)
		{
			if (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5 || lstAnimInfo.ActionCtrl.Item1 == 6)
			{
				return false;
			}
			if (lstAnimInfo.ActionCtrl.Item1 == 3 && lstAnimInfo.id == 0)
			{
				return false;
			}
		}
		if (!CheckEventLimit(lstAnimInfo.Event))
		{
			return false;
		}
		if (lstAnimInfo.ActionCtrl.Item1 == 3)
		{
			int item = lstAnimInfo.ActionCtrl.Item2;
			if (item == 5 || item == 6)
			{
				bool flag = false;
				foreach (int[] item2 in lstAnimInfo.Event)
				{
					if (item2[1] == -1)
					{
						if (item2[0] == EventNo)
						{
							flag = true;
						}
					}
					else if (item2[0] == EventNo && item2[1] == EventPeep)
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
		}
		if (!CheckPlace(lstAnimInfo))
		{
			return false;
		}
		if (!CheckEventPtn(JudgeEventPtn) && lstAnimInfo.Achievments != null && lstAnimInfo.Achievments.Count > 0 && !CheckAchievement(lstAnimInfo.Achievments))
		{
			return false;
		}
		switch (hSceneManager.FemaleState[0])
		{
		case ChaFileDefine.State.Blank:
			if (!lstAnimInfo.nStatePtns.Contains(0))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Favor:
			if (!lstAnimInfo.nStatePtns.Contains(1))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Enjoyment:
			if (!lstAnimInfo.nStatePtns.Contains(2))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Slavery:
			if (!lstAnimInfo.nStatePtns.Contains(3))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Aversion:
			if (!lstAnimInfo.nStatePtns.Contains(4))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Broken:
			if (!lstAnimInfo.nStatePtns.Contains(5))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Dependence:
			if (!lstAnimInfo.nStatePtns.Contains(6))
			{
				return false;
			}
			break;
		}
		if (lstAnimInfo.nFaintnessLimit != 0)
		{
			return false;
		}
		if (LimitResistPain && !ResistPain && ctrlFlag.feel_f >= 0.75f && lstAnimInfo.lstSystem.Contains(4))
		{
			return false;
		}
		if (!CheckAppendEV(lstAnimInfo, EventNo))
		{
			return false;
		}
		return true;
	}

	private bool CheckAutoMotionLimit(HScene.AnimationListInfo lstAnimInfo)
	{
		if (Singleton<Game>.Instance.eventNo == 7 || Singleton<Game>.Instance.eventNo == 32)
		{
			return false;
		}
		if (chaFemales.Length > 1 && chaFemales[1] == null && (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5))
		{
			return false;
		}
		if (chaMales.Length > 1 && chaMales[1] == null && lstAnimInfo.ActionCtrl.Item1 == 6)
		{
			return false;
		}
		if (!CheckEventLimit(lstAnimInfo.Event))
		{
			return false;
		}
		if (!CheckPlace(lstAnimInfo))
		{
			return false;
		}
		switch (hSceneManager.FemaleState[0])
		{
		case ChaFileDefine.State.Blank:
			if (!lstAnimInfo.nStatePtns.Contains(0))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Favor:
			if (!lstAnimInfo.nStatePtns.Contains(1))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Enjoyment:
			if (!lstAnimInfo.nStatePtns.Contains(2))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Slavery:
			if (!lstAnimInfo.nStatePtns.Contains(3))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Aversion:
			if (!lstAnimInfo.nStatePtns.Contains(4))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Broken:
			if (!lstAnimInfo.nStatePtns.Contains(5))
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Dependence:
			if (!lstAnimInfo.nStatePtns.Contains(6))
			{
				return false;
			}
			break;
		}
		if (ctrlFlag.isFaintness && (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1))
		{
			return false;
		}
		if (lstAnimInfo.nFaintnessLimit == 1)
		{
			return false;
		}
		if ((ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1) && lstAnimInfo.ActionCtrl.Item1 == 4 && lstAnimInfo.ActionCtrl.Item2 != 1)
		{
			return false;
		}
		return true;
	}

	public bool CheckMotionLimitAppend3P(HScene.AnimationListInfo lstAnimInfo)
	{
		if (!CheckPlace(lstAnimInfo))
		{
			return false;
		}
		bool flag = ctrlFlag.isFaintness && (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1);
		if (lstAnimInfo.nDownPtn == 0 && flag)
		{
			return false;
		}
		if (lstAnimInfo.nFaintnessLimit == 1 && !flag)
		{
			return false;
		}
		return true;
	}

	private void SetHelpActive(bool _active)
	{
		if (HelpBase.activeSelf != _active)
		{
			HelpBase.SetActive(_active);
		}
	}

	private void SetHelpText(string _text)
	{
		if (!(HelpTxt == null))
		{
			HelpTxt.text = _text;
		}
	}

	private void ReSetHelpText()
	{
		if (!(HelpTxt == null))
		{
			HelpTxt.text = "ホイールで開始";
		}
	}

	public void GuidProc(AnimatorStateInfo ai)
	{
		if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 3 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item2 == 5)
		{
			SetHelpActive(_active: false);
			return;
		}
		int num = -1;
		if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 3 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item2 == 2)
		{
			for (int i = 0; i < GuidTiming[4].Length; i++)
			{
				if (ai.IsName(GuidTiming[4][i]))
				{
					num = 4;
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < GuidTiming.Length - 1; j++)
			{
				for (int k = 0; k < GuidTiming[j].Length; k++)
				{
					if (ai.IsName(GuidTiming[j][k]))
					{
						num = j;
						break;
					}
				}
				if (num >= 0)
				{
					break;
				}
			}
		}
		if (num < 0)
		{
			SetHelpActive(_active: false);
			return;
		}
		if (num != 1 && (hScene.ctrlVoice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || hScene.ctrlVoice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice || hScene.ctrlVoice.nowVoices[1].state == HVoiceCtrl.VoiceKind.voice || hScene.ctrlVoice.nowVoices[1].state == HVoiceCtrl.VoiceKind.startVoice))
		{
			SetHelpActive(_active: false);
			return;
		}
		if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 2 && hScene.GetProcBase() is Sonyu { nowInsert: not false })
		{
			SetHelpActive(_active: false);
			return;
		}
		if (hScene.NowChangeAnim)
		{
			SetHelpActive(_active: false);
			return;
		}
		if (num != 1 && num != 4 && ctrlFlag.initiative != 0)
		{
			SetHelpActive(_active: false);
			return;
		}
		switch (num)
		{
		case 0:
			ReSetHelpText();
			SetHelpActive(_active: true);
			break;
		case 1:
			SetHelpText("ホイール上下で速度変更");
			SetHelpActive(_active: true);
			break;
		case 2:
			SetHelpText("ホイール上で再開始\nホイール下で引き抜く");
			SetHelpActive(_active: true);
			break;
		case 3:
			SetHelpText("ホイールで再開始");
			SetHelpActive(_active: true);
			break;
		case 4:
			SetHelpText("ホイールで叩く");
			SetHelpActive(_active: true);
			break;
		default:
			SetHelpActive(_active: false);
			break;
		}
	}

	public void HelpBaseActive(bool _active)
	{
		if (!(HelpBaseConfig == null) && HelpBaseConfig.activeSelf != _active)
		{
			HelpBaseConfig.SetActive(_active);
		}
	}

	public bool GetHelpActive()
	{
		if (HelpBaseConfig == null)
		{
			return false;
		}
		return HelpBaseConfig.activeSelf;
	}

	private IEnumerator CalcScrollBarHitCoroutine()
	{
		yield return null;
		yield return null;
		if (!(rtSelect == null))
		{
			float b = rtScrollViewCalcs[1].sizeDelta.y - rtScrollViewCalcs[0].sizeDelta.y;
			sbMotionList.value = 1f - Mathf.InverseLerp(0f, b, Mathf.Abs(rtSelect.anchoredPosition.y));
			yield return null;
		}
	}

	private IEnumerator CalcScrollBarInitCoroutine()
	{
		yield return new WaitForEndOfFrame();
		sbMotionList.value = 1f;
		yield return null;
	}

	public bool CheckPlace(HScene.AnimationListInfo info)
	{
		bool result = false;
		List<int> nPositons = info.nPositons;
		if (hPointCtrl == null || hPointCtrl.HPointList == null)
		{
			return result;
		}
		for (int i = 0; i < nPositons.Count; i++)
		{
			if (CheckPlace(info, nPositons[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool CheckPlace(HScene.AnimationListInfo info, int place)
	{
		bool result = false;
		foreach (KeyValuePair<int, List<HPoint>> item in hPointCtrl.HPointList.lst)
		{
			if (item.Key == place && CheckPlacePointMotion(info, item.Value))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool CheckPlacePointMotion(HScene.AnimationListInfo info, List<HPoint> points)
	{
		int num = info.ActionCtrl.Item1;
		int id = info.id;
		if (num == 6)
		{
			num = 5;
		}
		int num2 = 0;
		foreach (HPoint point in points)
		{
			if (point.Data.notMotion[num].motionID.Count == 0)
			{
				continue;
			}
			foreach (int item in point.Data.notMotion[num].motionID)
			{
				if (item == id)
				{
					num2++;
					break;
				}
			}
		}
		if (points.Count != 0)
		{
			return num2 != points.Count;
		}
		return true;
	}

	private bool CheckAchievement(List<int> Achievement)
	{
		foreach (int item in Achievement)
		{
			if (item != -1 && !SaveData.IsAchievementExchangeRelease(item))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckEventLimit(List<int[]> Event)
	{
		if (EventNo == -1)
		{
			return true;
		}
		if (JudgeEventPtn == null || JudgeEventPtn.Count == 0)
		{
			return true;
		}
		if (!CheckEventPtn(JudgeEventPtn))
		{
			return true;
		}
		return CheckEventPtn(Event);
	}

	private bool CheckEventPtn(List<int[]> JudgePtn)
	{
		foreach (int[] item in JudgePtn)
		{
			if (item[1] != -1)
			{
				if (item[0] != EventNo || item[1] != EventPeep)
				{
					continue;
				}
			}
			else if (item[0] != EventNo)
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public void SetLimitResist(AnimatorStateInfo ai)
	{
		bool flag = LimitResistPain != ctrlFlag.feel_f >= 0.75f;
		if (ai.IsName("OLoop") || ai.IsName("D_OLoop"))
		{
			if (!LimitResistPain)
			{
				LimitResistPain = true;
				if (!ResistPain && ctrlFlag.feel_f >= 0.75f)
				{
					SetMotionListDraw(_active: false);
				}
			}
		}
		else if (LimitResistPain && flag)
		{
			LimitResistPain = false;
			if (!ResistPain && ctrlFlag.feel_f >= 0.75f)
			{
				SetMotionListDraw(_active: false);
			}
		}
	}

	public void SetStartTaii(HScene.AnimationListInfo taii)
	{
		int target = taii.ActionCtrl.Item1;
		if (taii.nInitiativeFemale != 0)
		{
			target = 7;
		}
		CategoryScroll.SetTarget(target);
	}

	public bool CheckAppendEV(HScene.AnimationListInfo info, int eventNo)
	{
		if (info.ReleaseEvent < 0)
		{
			return true;
		}
		if (info.ReleaseEvent == eventNo)
		{
			return true;
		}
		if (info.ReleaseEvent != 58)
		{
			return AppendSaveData.IsApeendEvents(info.ReleaseEvent);
		}
		return Singleton<Game>.Instance.appendSaveData.IsFurSitri3P;
	}
}
