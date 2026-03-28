using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AIChara;
using Config;
using HS2;
using Illusion;
using Illusion.Anime;
using Illusion.Game;
using Manager;
using Tutorial2D;
using UniRx;
using UniRx.Async;
using UnityEngine;

public class HScene : MonoBehaviour
{
	[Serializable]
	public class LightInfo
	{
		[Tooltip("回すキャラライトオブジェクト")]
		public GameObject objCharaLight;

		[Tooltip("キャラライト")]
		public Light light;

		[Tooltip("補助ライト")]
		public Light[] sublights = new Light[2];

		[Tooltip("初期のライトの回転")]
		public Quaternion initRot = Quaternion.identity;

		[Tooltip("初期の強さ")]
		[Space]
		public float initIntensity = 1f;

		[Tooltip("強さの最小値")]
		[Range(0f, 2f)]
		public float minIntensity;

		[Tooltip("強さの最大値")]
		[Range(0f, 2f)]
		public float maxIntensity = 2f;

		[Tooltip("補助ライトの初期状態")]
		public const bool subLightEnable = true;

		[Tooltip("初期の色")]
		public Color initColor = Color.white;
	}

	[Serializable]
	public class AnimationListInfo
	{
		public int id = -1;

		public string nameAnimation = "";

		public string assetpathBaseM = "";

		public string assetBaseM = "";

		public string assetpathMale = "";

		public string fileMale = "";

		public bool isMaleHitObject;

		public string fileMotionNeckMale;

		public string assetpathBaseM2 = "";

		public string assetBaseM2 = "";

		public string assetpathMale2 = "";

		public string fileMale2 = "";

		public bool isMaleHitObject2;

		public string fileMotionNeckMale2;

		public string assetpathBaseF = "";

		public string assetBaseF = "";

		public string assetpathFemale;

		public string fileFemale;

		public bool isFemaleHitObject;

		public string fileMotionNeckFemale;

		public string fileMotionNeckFemalePlayer;

		public string assetpathBaseF2 = "";

		public string assetBaseF2 = "";

		public string assetpathFemale2 = "";

		public string fileFemale2 = "";

		public bool isFemaleHitObject2;

		public string fileMotionNeckFemale2;

		public (int, int) ActionCtrl = (-1, -1);

		public List<int> nPositons = new List<int>();

		public List<string> lstOffset = new List<string>();

		public bool isNeedItem;

		public int nDownPtn;

		public List<int> nStatePtns = new List<int>();

		public int nFaintnessLimit;

		public int nIyaAction = 1;

		public List<int> Achievments = new List<int>();

		public int nInitiativeFemale;

		public int nBackInitiativeID = -1;

		public List<int> lstSystem = new List<int>();

		public int nMaleSon = -1;

		public int[] nFemaleUpperCloths = new int[2] { -1, -1 };

		public int[] nFemaleLowerCloths = new int[2] { -1, -1 };

		public int nFeelHit;

		public string nameCamera;

		public string fileSiruPaste;

		public string fileSiruPasteSecond = "";

		public string fileSe;

		public int nShortBreahtPlay = 1;

		public HashSet<int> hasVoiceCategory = new HashSet<int>();

		public int nPromiscuity = -1;

		public bool reverseTaii;

		public List<int[]> Event = new List<int[]>();

		public int ParmID = -1;

		public int ReleaseEvent = -1;
	}

	public class StartMotion
	{
		public int mode;

		public int id;

		public StartMotion(int _mode, int _id)
		{
			mode = _mode;
			id = _id;
		}
	}

	public HSceneFlagCtrl ctrlFlag;

	public GameObject objGrondInstantiate;

	public H_Lookat_dan[] ctrlLookAts;

	public HLayerCtrl ctrlLayer;

	public HAutoCtrl ctrlAuto;

	public CollisionCtrl[] ctrlMaleCollisionCtrls;

	public CollisionCtrl[] ctrlFemaleCollisionCtrls;

	public HVoiceCtrl ctrlVoice;

	public HMotionEyeNeckFemale[] ctrlEyeNeckFemale;

	public HMotionEyeNeckMale[] ctrlEyeNeckMale;

	public SiruPasteCtrl[] ctrlSiruPastes;

	public HPointCtrl hPointCtrl;

	public RootmotionOffset[] RootmotionOffsetF;

	public RootmotionOffset[] RootmotionOffsetM;

	[SerializeField]
	private HSceneSpriteChaChoice chaChoice;

	[SerializeField]
	private ScreenEffect se;

	[SerializeField]
	private ScreenEffectUI seUI;

	private HParticleCtrl ctrlParitcle;

	public HitObjectCtrl[] ctrlHitObjectFemales = new HitObjectCtrl[2];

	public HitObjectCtrl[] ctrlHitObjectMales = new HitObjectCtrl[2];

	public LightInfo infoLight = new LightInfo();

	[SerializeField]
	private HSceneSprite sprite;

	[SerializeField]
	private CrossFade fade;

	private ChaControl[] chaFemales = new ChaControl[2];

	private ChaControl[] chaMales = new ChaControl[2];

	private Transform[] chaFemalesTrans = new Transform[2];

	private Transform[] chaMalesTrans = new Transform[2];

	private int mode = -1;

	private int modeCtrl = -1;

	private List<ProcBase> lstProc = new List<ProcBase>();

	[SerializeField]
	private ObiCtrl ctrlObi;

	private HItemCtrl ctrlItem;

	private FeelHit ctrlFeelHit;

	private bool isSyncFirstStep;

	private DynamicBoneReferenceCtrl[] ctrlDynamics = new DynamicBoneReferenceCtrl[2];

	private HSeCtrl ctrlSE;

	private GameObject objGrondCollision;

	[SerializeField]
	private YureCtrl[] ctrlYures = new YureCtrl[2];

	[SerializeField]
	private YureCtrlMale[] ctrlYureMale = new YureCtrlMale[2];

	private bool isTuyaOn;

	private List<string> abName = new List<string>();

	private GameObject objMap;

	private List<AnimationListInfo>[] lstAnimInfo = new List<AnimationListInfo>[7]
	{
		new List<AnimationListInfo>(),
		new List<AnimationListInfo>(),
		new List<AnimationListInfo>(),
		new List<AnimationListInfo>(),
		new List<AnimationListInfo>(),
		new List<AnimationListInfo>(),
		new List<AnimationListInfo>()
	};

	private AnimationListInfo aInfo = new AnimationListInfo();

	public AnimationListInfo StartAnimInfo = new AnimationListInfo();

	private HSceneManager.BaseAnimInfo[,] runtimeAnimatorControllers = new HSceneManager.BaseAnimInfo[2, 3];

	private StartMotion autoMotion;

	private HSceneManager hSceneManager;

	private bool nowStart;

	private bool nullPlayer = true;

	private StringBuilder sbLoadFileName = new StringBuilder();

	public bool NowStateIsEnd;

	private bool nowChangeAnim;

	private Vector3 distanceToContoroler;

	public Transform hitemPlace;

	[SerializeField]
	private Transform particlePlace;

	private RuntimeAnimatorController[] racM;

	private RuntimeAnimatorController[] racF;

	private RuntimeAnimatorController[] HoushiRacM;

	private RuntimeAnimatorController[] HoushiRacF;

	private Dictionary<string, RuntimeAnimatorController> racEtcM = new Dictionary<string, RuntimeAnimatorController>();

	private Dictionary<string, RuntimeAnimatorController> racEtcF = new Dictionary<string, RuntimeAnimatorController>();

	private HashSet<string> hashUseAssetBundleAnimator = new HashSet<string>();

	private Vector3 noShiftPos = Vector3.zero;

	private Quaternion noShiftRot = Quaternion.identity;

	public HSceneSpriteItem HSceneSpriteItem;

	private bool setStartVoice;

	private bool mapVisible = true;

	private bool mapLight = true;

	private bool Bath;

	private bool Room;

	[SerializeField]
	private GameScreenShot gameScreenShot;

	public ScreenEffect screenEffect => se;

	public ScreenEffectUI screenEffectUI => seUI;

	public HParticleCtrl CtrlParticle
	{
		get
		{
			if (ctrlParitcle == null)
			{
				ctrlParitcle = HSceneManager.HResourceTables.hParticle;
			}
			return ctrlParitcle;
		}
	}

	private List<(int sex, int num, MotionIK motionIK)> lstMotionIK { get; } = new List<(int, int, MotionIK)>();

	public bool NowChangeAnim => nowChangeAnim;

	public IEnumerator Start()
	{
		int EventNo = Singleton<Game>.Instance.eventNo;
		int peep = Singleton<Game>.Instance.peepKind;
		hSceneManager = Singleton<HSceneManager>.Instance;
		hSceneManager.SetHFlag();
		hSceneManager.Hscene = this;
		nowStart = true;
		base.enabled = false;
		yield return new WaitUntil(() => HSceneManager.HResourceTables.endHLoad);
		yield return HSceneManager.HResourceTables.LoadHObj();
		noShiftPos = Vector3.zero;
		noShiftRot = Quaternion.identity;
		ctrlFlag.semenType = Manager.Config.HData.SiruDraw;
		racM = new RuntimeAnimatorController[2];
		HoushiRacM = new RuntimeAnimatorController[2];
		racF = new RuntimeAnimatorController[2];
		HoushiRacF = new RuntimeAnimatorController[2];
		racEtcM.Clear();
		racEtcF.Clear();
		hPointCtrl.InitHPoint();
		ctrlFlag.nPlace = hSceneManager.height;
		ctrlFlag.isFaintnessVoice = false;
		bool sleepEvent = EventNo == 7 || EventNo == 32;
		if (sleepEvent || EventNo == 54)
		{
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 1;
			ctrlFlag.isFaintnessVoice = true;
		}
		CreateListAnimationFileName();
		hSceneManager.mapID = Singleton<Game>.Instance.mapNo;
		if (hSceneManager.mapID >= 0)
		{
			yield return BaseMap.ChangeAsync(hSceneManager.mapID, FadeCanvas.Fade.None).ToCoroutine();
			objMap = BaseMap.mapRoot;
			hPointCtrl.HPointList = objMap.GetComponentInChildren<HPointList>();
			hPointCtrl.HPointList?.Init();
			HSceneManager.HResourceTables.HPointInitData(hPointCtrl.HPointList, objMap);
			SingletonInitializer<BaseMap>.instance.MobObjectsVisible(EventNo == 52);
			ctrlFlag.cameraCtrl.loadVanishExcelData("list/map/", hSceneManager.mapID, objMap);
		}
		chaFemales = new ChaControl[2];
		chaMales = new ChaControl[2];
		chaFemalesTrans = new Transform[2];
		chaMalesTrans = new Transform[2];
		Bath = hSceneManager.mapID == 4 || hSceneManager.mapID == 52 || hSceneManager.mapID == 53;
		Room = hSceneManager.mapID == 3;
		GameObject objCommon = GameObject.Find("CommonSpace");
		if (hSceneManager.females[0] == null)
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			if (!hSceneManager.pngFemales[0].IsNullOrEmpty())
			{
				if (!chaFileControl.LoadCharaFile(hSceneManager.pngFemales[0], 1))
				{
					chaFileControl = null;
				}
			}
			else
			{
				chaFileControl = null;
			}
			chaFemales[0] = Singleton<Character>.Instance.CreateChara(1, objCommon, 0, chaFileControl);
			chaFemales[0].Load();
			chaFemalesTrans[0] = chaFemales[0].transform;
			hSceneManager.females[0] = chaFemales[0];
			ChangeCoodinate(EventNo, peep, chaFemales[0]);
			yield return null;
			yield return null;
		}
		else
		{
			chaFemales[0] = hSceneManager.females[0];
			chaFemales[0].visibleAll = true;
			chaFemalesTrans[0] = chaFemales[0].transform;
			Controller controller = Controller.Table.Get(chaFemales[0]);
			controller?.itemHandler.DisableItems();
			controller?.Enable(enabled: false);
			_ = hSceneManager.females[0].transform;
			ChangeCoodinate(EventNo, peep, chaFemales[0]);
		}
		hSceneManager.Personality[0] = chaFemales[0].chaFile.parameter2.personality;
		if (hSceneManager.females[1] == null)
		{
			if (!hSceneManager.pngFemales[1].IsNullOrEmpty())
			{
				ChaFileControl chaFileControl2 = new ChaFileControl();
				if (!chaFileControl2.LoadCharaFile(hSceneManager.pngFemales[1], 1))
				{
					chaFileControl2 = null;
				}
				if (chaFileControl2 != null)
				{
					int id = 1;
					if (hSceneManager.SecondSitori)
					{
						id = -2;
					}
					chaFemales[1] = Singleton<Character>.Instance.CreateChara(1, objCommon, id, chaFileControl2);
					chaFemalesTrans[1] = chaFemales[1].transform;
					hSceneManager.females[1] = chaFemales[1];
					hSceneManager.Personality[1] = chaFemales[1].chaFile.parameter2.personality;
					chaFemales[1].Load();
					chaFemales[1].visibleAll = false;
					Controller controller2 = Controller.Table.Get(chaFemales[1]);
					controller2?.itemHandler.DisableItems();
					controller2?.Enable(enabled: false);
					ChangeCoodinate(EventNo, peep, chaFemales[1], Second: true);
				}
			}
		}
		else
		{
			chaFemales[1] = hSceneManager.females[1];
			chaFemales[1].Load();
			chaFemales[1].visibleAll = false;
			chaFemalesTrans[1] = chaFemales[1].transform;
			Controller controller3 = Controller.Table.Get(chaFemales[1]);
			controller3?.itemHandler.DisableItems();
			controller3?.Enable(enabled: false);
			ChangeCoodinate(EventNo, peep, chaFemales[1], Second: true);
		}
		hSceneManager.SetFemaleState(hSceneManager.females);
		if (hSceneManager.FemaleState[0] == ChaFileDefine.State.Broken)
		{
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 1;
			ctrlFlag.isFaintnessVoice = true;
		}
		else if (hSceneManager.FemaleState[0] == ChaFileDefine.State.Aversion)
		{
			hSceneManager.isForce = true;
		}
		if (hSceneManager.FemaleState[1] == ChaFileDefine.State.Broken)
		{
			ctrlFlag.FaintnessType = ((ctrlFlag.FaintnessType != 1) ? 2 : 0);
			if (ctrlFlag.FaintnessType == 1)
			{
				ctrlFlag.FaintnessType = 0;
			}
			else
			{
				ctrlFlag.FaintnessType = 2;
			}
		}
		else if (hSceneManager.FemaleState[1] == ChaFileDefine.State.Aversion)
		{
			hSceneManager.isForceSecond = true;
		}
		objCommon = base.gameObject;
		if (hSceneManager.player == null)
		{
			ChaFileControl chaFileControl3 = new ChaFileControl();
			if (!hSceneManager.pngMale.IsNullOrEmpty())
			{
				if (!hSceneManager.bFutanari)
				{
					if (!chaFileControl3.LoadCharaFile(hSceneManager.pngMale))
					{
						chaFileControl3 = null;
					}
				}
				else if (!chaFileControl3.LoadCharaFile(hSceneManager.pngMale, 1))
				{
					chaFileControl3 = null;
				}
			}
			else
			{
				chaFileControl3 = null;
			}
			if (!hSceneManager.bFutanari)
			{
				chaMales[0] = Singleton<Character>.Instance.CreateChara(0, objCommon, 99, chaFileControl3);
			}
			else
			{
				chaMales[0] = Singleton<Character>.Instance.CreateChara(1, objCommon, 99, chaFileControl3);
			}
			chaMales[0].Load();
			chaMalesTrans[0] = chaMales[0].transform;
			chaMales[0].isPlayer = true;
			hSceneManager.player = chaMales[0];
			yield return null;
			yield return null;
		}
		else
		{
			chaMales[0] = hSceneManager.player;
			chaMalesTrans[0] = chaMales[0].transform;
			Controller controller4 = Controller.Table.Get(chaMales[0]);
			controller4?.itemHandler.DisableItems();
			controller4?.Enable(enabled: false);
			if (EventNo == 50 || EventNo == 51 || EventNo == 52 || EventNo == 53 || EventNo == 54 || EventNo == 55)
			{
				string appendCoordinatePlayer = Singleton<Game>.Instance.appendCoordinatePlayer;
				if (!appendCoordinatePlayer.IsNullOrEmpty())
				{
					chaMales[0].ChangeNowCoordinate(appendCoordinatePlayer, reload: true);
					chaMales[0].Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
				}
			}
		}
		ChaFileControl chaFileControl4 = new ChaFileControl();
		if (!hSceneManager.pngMaleSecond.IsNullOrEmpty())
		{
			if (!hSceneManager.bFutanariSecond)
			{
				if (!chaFileControl4.LoadCharaFile(hSceneManager.pngMaleSecond))
				{
					chaFileControl4 = null;
				}
			}
			else if (!chaFileControl4.LoadCharaFile(hSceneManager.pngMaleSecond, 1))
			{
				chaFileControl4 = null;
			}
		}
		else
		{
			chaFileControl4 = null;
		}
		bool NullchaMFile = chaFileControl4 == null;
		if (!NullchaMFile)
		{
			if (!hSceneManager.bFutanariSecond)
			{
				chaMales[1] = Singleton<Character>.Instance.CreateChara(0, objCommon, 2, chaFileControl4);
			}
			else
			{
				chaMales[1] = Singleton<Character>.Instance.CreateChara(1, objCommon, 2, chaFileControl4);
			}
			chaMalesTrans[1] = chaMales[1].transform;
			chaMales[1].Load();
			chaMales[1].isPlayer = true;
			chaMales[1].visibleAll = false;
			yield return null;
			yield return null;
		}
		if (hSceneManager.bFutanari)
		{
			chaMales[0].SetShapeBodyValue(0, 0.75f);
		}
		if (hSceneManager.bFutanariSecond)
		{
			chaMales[1].SetShapeBodyValue(0, 0.75f);
		}
		RootmotionOffsetF = new RootmotionOffset[2]
		{
			new RootmotionOffset(),
			new RootmotionOffset()
		};
		RootmotionOffsetM = new RootmotionOffset[2]
		{
			new RootmotionOffset(),
			new RootmotionOffset()
		};
		RootmotionOffsetF[0].Chara = chaFemales[0];
		RootmotionOffsetF[1].Chara = chaFemales[1];
		RootmotionOffsetM[0].Chara = chaMales[0];
		RootmotionOffsetM[1].Chara = chaMales[1];
		hSceneManager.females[0].transform.position = Vector3.zero;
		chaFemalesTrans[0].position = Vector3.zero;
		chaFemalesTrans[0].rotation = Quaternion.identity;
		hSceneManager.females[0].animBody.transform.localPosition = Vector3.zero;
		hSceneManager.females[0].animBody.transform.localRotation = Quaternion.identity;
		for (int num = 0; num < chaFemales.Length; num++)
		{
			ChaControl chaControl = chaFemales[num];
			if (!(chaControl == null) && !(chaControl.objTop == null))
			{
				chaControl.LoadHitObject();
				ctrlFemaleCollisionCtrls[num].Init(chaFemales[num], chaFemales[num].objHitHead, chaFemales[num].objHitBody);
			}
		}
		for (int num2 = 0; num2 < chaMales.Length; num2++)
		{
			ChaControl chaControl2 = chaMales[num2];
			if (!(chaControl2 == null) && !(chaControl2.objTop == null))
			{
				chaControl2.LoadHitObject();
				ctrlMaleCollisionCtrls[num2].Init(chaFemales[0], chaMales[num2].objHitHead, chaMales[num2].objHitBody);
			}
		}
		Game game = Singleton<Game>.Instance;
		ChaFileGameInfo2 fileGameInfo = hSceneManager.females[0].fileGameInfo2;
		if (EventNo < 50)
		{
			if (hSceneManager.females[0].chaID != -1 && fileGameInfo != null)
			{
				Illusion.Game.Utils.Sound.Play(new Illusion.Game.Utils.Sound.SettingBGM((BGM)(5 + fileGameInfo.nowDrawState)));
			}
			else if (hSceneManager.females[0].chaID == -1)
			{
				Illusion.Game.Utils.Sound.Play(new Illusion.Game.Utils.Sound.SettingBGM(BGM.fur));
			}
		}
		else if (EventNo != 56 && EventNo != 58)
		{
			Illusion.Game.Utils.Sound.Play(new Illusion.Game.Utils.Sound.SettingBGM((BGM)(19 + (EventNo - 50))));
		}
		else
		{
			Illusion.Game.Utils.Sound.Play(new Illusion.Game.Utils.Sound.SettingBGM(BGM.sitri));
		}
		List<float[]> shapes = new List<float[]>();
		for (int num3 = 0; num3 < chaFemales.Length; num3++)
		{
			if (!(chaFemales[num3] == null))
			{
				shapes.Add(new float[ChaFileDefine.cf_bodyshapename.Length]);
				for (int num4 = 0; num4 < ChaFileDefine.cf_bodyshapename.Length; num4++)
				{
					shapes[num3][num4] = chaFemales[num3].GetShapeBodyValue(num4);
					chaFemales[num3].SetShapeBodyValue(num4, 0.5f);
				}
				chaFemales[num3].LateUpdateForce();
			}
		}
		ctrlObi = new ObiCtrl(hSceneManager, ctrlFlag);
		ctrlItem = new HItemCtrl();
		ctrlItem.HItemInit(hitemPlace);
		HSceneManager.HResourceTables.HitObjListInit();
		int num5;
		for (int i = 0; i < ctrlHitObjectFemales.Length; i = num5)
		{
			ctrlHitObjectFemales[i] = new HitObjectCtrl();
			if (!(chaFemales[i] == null) && !(chaFemales[i].objBodyBone == null))
			{
				ctrlHitObjectFemales[i].id = i;
				yield return ctrlHitObjectFemales[i].HitObjInit(1, chaFemales[i].objBodyBone, chaFemales[i]);
			}
			num5 = i + 1;
		}
		for (int i = 0; i < ctrlHitObjectMales.Length; i = num5)
		{
			ctrlHitObjectMales[i] = new HitObjectCtrl();
			if (!(chaMales[i] == null) && !(chaMales[i].objBodyBone == null))
			{
				ctrlHitObjectMales[i].id = i;
				yield return ctrlHitObjectMales[i].HitObjInit(0, chaMales[i].objBodyBone, chaMales[i]);
			}
			num5 = i + 1;
		}
		for (int num6 = 0; num6 < chaFemales.Length; num6++)
		{
			if (!(chaFemales[num6] == null))
			{
				for (int num7 = 0; num7 < ChaFileDefine.cf_bodyshapename.Length; num7++)
				{
					chaFemales[num6].SetShapeBodyValue(num7, shapes[num6][num7]);
				}
			}
		}
		for (int num8 = 0; num8 < chaMales.Length; num8++)
		{
			if (!(chaMales[num8] == null))
			{
				ctrlLookAts[num8].DankonInit(chaMales[num8], chaFemales);
			}
		}
		ctrlFeelHit = new FeelHit();
		ctrlFeelHit.FeelHitInit(hSceneManager.Personality[0]);
		ctrlFeelHit.SetFeelCha(chaFemales[0]);
		ctrlFeelHit.SetFeelEventNo();
		ctrlYures[0] = new YureCtrl();
		ctrlYures[1] = new YureCtrl();
		ctrlYures[0].Init();
		ctrlYures[1].Init();
		ctrlYures[0].SetChaControl(chaFemales[0]);
		ctrlYures[0].femaleID = 0;
		if (chaFemales[1] != null && chaFemales[1].objBodyBone != null)
		{
			ctrlYures[1].SetChaControl(chaFemales[1]);
			ctrlYures[1].femaleID = 1;
		}
		ctrlYureMale[0].Init();
		ctrlYureMale[1].Init();
		ctrlYureMale[0].chaMale = chaMales[0];
		ctrlYureMale[0].MaleID = 0;
		ctrlYureMale[1].chaMale = chaMales[1];
		ctrlYureMale[1].MaleID = 1;
		ctrlLayer.Init(chaFemales, chaMales);
		ctrlDynamics[0] = new DynamicBoneReferenceCtrl();
		ctrlDynamics[1] = new DynamicBoneReferenceCtrl();
		ctrlDynamics[0].Init(chaFemales[0]);
		ctrlSE = new HSeCtrl(ctrlFlag);
		bool flag = hSceneManager.females[1] != null;
		ctrlVoice.MultiFemale = flag;
		ctrlVoice.MultiMale = !NullchaMFile;
		if (flag)
		{
			yield return ctrlVoice.Init(hSceneManager.females[0].fileParam2.personality, hSceneManager.females[0].fileParam2.voicePitch, hSceneManager.females[0], hSceneManager.females[1].fileParam2.personality, hSceneManager.females[1].fileParam2.voicePitch, hSceneManager.females[1]);
		}
		else
		{
			yield return ctrlVoice.Init(hSceneManager.females[0].fileParam2.personality, hSceneManager.females[0].fileParam2.voicePitch, hSceneManager.females[0]);
		}
		if (chaFemales[0] != null && chaFemales[0].cmpBoneBody.targetEtc.trfHeadParent != null)
		{
			ctrlFlag.voice.voiceTrs[0] = chaFemales[0].cmpBoneBody.targetEtc.trfHeadParent;
		}
		else
		{
			ctrlFlag.voice.voiceTrs[0] = null;
		}
		if (chaFemales[1] != null && chaFemales[1].cmpBoneBody != null && chaFemales[1].cmpBoneBody.targetEtc != null && chaFemales[1].cmpBoneBody.targetEtc.trfHeadParent != null)
		{
			ctrlFlag.voice.voiceTrs[1] = chaFemales[1].cmpBoneBody.targetEtc.trfHeadParent;
		}
		else
		{
			ctrlFlag.voice.voiceTrs[1] = null;
		}
		bool eventEyeNeckDisregard = EventNo == 3 || EventNo == 4 || EventNo == 5 || EventNo == 6 || EventNo == 28 || EventNo == 29 || EventNo == 30 || EventNo == 31;
		if (eventEyeNeckDisregard)
		{
			eventEyeNeckDisregard = eventEyeNeckDisregard && (peep == 0 || peep == 1);
		}
		eventEyeNeckDisregard = eventEyeNeckDisregard || EventNo == 7 || EventNo == 32;
		ctrlEyeNeckFemale[0].Init(chaFemales[0], 0, this);
		ctrlEyeNeckFemale[0].SetPartner((chaMales[0] != null) ? chaMales[0].objBodyBone : null, (chaMales[1] != null) ? chaMales[1].objBodyBone : null, (chaFemales[1] != null) ? chaFemales[1].objBodyBone : null);
		if (eventEyeNeckDisregard)
		{
			ctrlEyeNeckFemale[0].SetEventDisregard();
		}
		if (chaMales[0] != null && chaMales[0].objBodyBone != null)
		{
			ctrlEyeNeckMale[0].Init(chaMales[0], 0);
			ctrlEyeNeckMale[0].SetPartner(chaFemales[0].objBodyBone, (chaFemales[1] != null) ? chaFemales[1].objBodyBone : null, (chaMales[1] != null) ? chaMales[1].objBodyBone : null);
		}
		yield return null;
		bool flag2 = false;
		if (chaFemales[1] != null && chaFemales[1].objBodyBone != null)
		{
			ctrlEyeNeckFemale[1].Init(chaFemales[1], 1, this);
			ctrlEyeNeckFemale[1].SetPartner((chaMales[0] != null) ? chaMales[0].objBodyBone : null, (chaMales[1] != null) ? chaMales[1].objBodyBone : null, chaFemales[0].objBodyBone);
			flag2 = true;
			if (eventEyeNeckDisregard)
			{
				ctrlEyeNeckFemale[1].SetEventDisregard();
			}
		}
		if (chaMales[1] != null && chaMales[1].objBodyBone != null)
		{
			ctrlEyeNeckMale[1].Init(chaMales[1], 1);
			ctrlEyeNeckMale[1].SetPartner(chaFemales[0].objBodyBone, (chaFemales[1] != null) ? chaFemales[1].objBodyBone : null, (chaMales[0] != null) ? chaMales[0].objBodyBone : null);
			flag2 = true;
		}
		if (flag2)
		{
			yield return null;
		}
		ctrlSiruPastes[0].Init(chaFemales[0]);
		if (chaFemales[1] != null && chaFemales[1].objBodyBone != null)
		{
			ctrlSiruPastes[1].Init(chaFemales[1]);
		}
		HSceneManager.HResourceTables.HparticleInit(particlePlace);
		ctrlObi.SetParticle(CtrlParticle);
		if ((bool)objGrondInstantiate)
		{
			objGrondCollision = UnityEngine.Object.Instantiate(objGrondInstantiate, chaFemales[0].objTop.transform);
			objGrondCollision.name = objGrondInstantiate.name;
			objGrondCollision.transform.localPosition = Vector3.zero;
			objGrondCollision.transform.localRotation = Quaternion.identity;
		}
		runtimeAnimatorControllers = HSceneManager.HResourceTables.HBaseRuntimeAnimatorControllers;
		if (Bath || hSceneManager.mapID == 6)
		{
			ctrlFlag.rateTuya = 1f;
		}
		if (hSceneManager.mapID == 6)
		{
			ctrlFlag.rateWet = 1f;
			float value = 100f;
			for (int num9 = 0; num9 < chaFemales.Length; num9++)
			{
				if (!(chaFemales[num9] == null) && !(chaFemales[num9].objTop == null))
				{
					chaFemales[num9].wetRate = ctrlFlag.rateWet;
					if (HSceneSpriteItem.lstUseSliderToggle[1] != null)
					{
						HSceneSpriteItem.lstSlider[1].value = value;
					}
				}
			}
			for (int num10 = 0; num10 < chaMales.Length; num10++)
			{
				if (!(chaMales[num10] == null) && !(chaMales[num10].objTop == null))
				{
					chaMales[num10].wetRate = 1f;
				}
			}
		}
		if (Bath && game.saveData.playerCloths != null)
		{
			for (int num11 = 0; num11 < chaMales.Length; num11++)
			{
				if (!(chaMales[num11] == null) && !(chaMales[num11].objTop == null) && game.saveData.playerCloths[num11] != null && !game.saveData.playerCloths[num11].file.IsNullOrEmpty())
				{
					chaMales[num11].ChangeNowCoordinate(game.saveData.playerCloths[num11].file, reload: true);
				}
			}
		}
		if (Bath && hSceneManager.mapID != 4)
		{
			ctrlFlag.rateWet = 1f;
			float value2 = 100f;
			for (int num12 = 0; num12 < chaFemales.Length; num12++)
			{
				if (!(chaFemales[num12] == null) && !(chaFemales[num12].objTop == null))
				{
					chaFemales[num12].wetRate = ctrlFlag.rateWet;
					if (HSceneSpriteItem.lstUseSliderToggle[1] != null)
					{
						HSceneSpriteItem.lstSlider[1].value = value2;
					}
				}
			}
			for (int num13 = 0; num13 < chaMales.Length; num13++)
			{
				if (!(chaMales[num13] == null) && !(chaMales[num13].objTop == null))
				{
					chaMales[num13].wetRate = 1f;
				}
			}
		}
		if (EventNo == 50)
		{
			ctrlFlag.rateWet = 1f;
			float value3 = 100f;
			for (int num14 = 0; num14 < chaFemales.Length; num14++)
			{
				if (!(chaFemales[num14] == null) && !(chaFemales[num14].objTop == null))
				{
					chaFemales[num14].wetRate = ctrlFlag.rateWet;
					if (HSceneSpriteItem.lstUseSliderToggle[1] != null)
					{
						HSceneSpriteItem.lstSlider[1].value = value3;
					}
				}
			}
			for (int num15 = 0; num15 < chaMales.Length; num15++)
			{
				if (!(chaMales[num15] == null) && !(chaMales[num15].objTop == null))
				{
					chaMales[num15].wetRate = 1f;
				}
			}
		}
		if (EventNo == 19)
		{
			ctrlFlag.isFaintness = true;
		}
		AnimatorStateInfo animatorStateInfo = chaFemales[0].getAnimatorStateInfo(0);
		ctrlEyeNeckFemale[0].Proc(animatorStateInfo, null, 0);
		ctrlFlag.voice.sleep = sleepEvent;
		ctrlVoice.BreathProc(animatorStateInfo, chaFemales[0], 0, ctrlFlag.voice.sleep);
		Manager.Sound.Listener = ctrlFlag.cameraCtrl.transform;
		ctrlFlag.selectAnimationListInfo = null;
		yield return sprite.Init();
		sprite.StartAnimInfo = StartAnimInfo;
		sprite.setAnimationList(lstAnimInfo);
		sprite.Setting(chaFemales, chaMales);
		bool isLeaveItToYou = ctrlFlag.initiative != 0;
		sprite.MainCategoryOfLeaveItToYou(isLeaveItToYou);
		sprite.SetFinishSelect(mode, modeCtrl);
		yield return null;
		DeliveryMember member = new DeliveryMember
		{
			ctrlFlag = ctrlFlag,
			chaMales = chaMales,
			chaFemales = chaFemales,
			fade = fade,
			ctrlObi = ctrlObi,
			sprite = sprite,
			item = ctrlItem,
			feelHit = ctrlFeelHit,
			auto = ctrlAuto,
			voice = ctrlVoice,
			particle = CtrlParticle,
			se = ctrlSE,
			lstMotionIK = lstMotionIK,
			ctrlYures = ctrlYures,
			rootmotionOffsetsF = RootmotionOffsetF,
			rootmotionOffsetsM = RootmotionOffsetM,
			eventNo = Singleton<Game>.Instance.eventNo,
			peepkind = Singleton<Game>.Instance.peepKind
		};
		lstProc.Add(new Aibu(member));
		lstProc.Add(new Houshi(member));
		(lstProc[1] as Houshi).SetAnimationList(lstAnimInfo[1]);
		lstProc.Add(new Sonyu(member));
		yield return null;
		lstProc.Add(new Spnking(member));
		lstProc.Add(new Masturbation(member));
		lstProc.Add(new Peeping(member));
		yield return null;
		lstProc.Add(new Les(member));
		lstProc.Add(new MultiPlay_F2M1(member));
		(lstProc[7] as MultiPlay_F2M1).SetAnimationList(lstAnimInfo[5]);
		lstProc.Add(new MultiPlay_F1M2(member));
		(lstProc[8] as MultiPlay_F1M2).SetAnimationList(lstAnimInfo[6]);
		if (mode != -1 && modeCtrl != -1 && ProcBase.endInit)
		{
			lstProc[mode].setAnimationParamater();
		}
		yield return null;
		SetStartAnimationInfo();
		nowStart = false;
		sprite.enabled = true;
		nullPlayer = hSceneManager.player == null;
		Light light = ctrlFlag.Light;
		infoLight = new LightInfo();
		infoLight.objCharaLight = light.gameObject;
		infoLight.light = light;
		infoLight.initRot = light.transform.localRotation;
		infoLight.initIntensity = Mathf.InverseLerp(infoLight.minIntensity, infoLight.maxIntensity, light.intensity);
		Color color = light.color;
		if (color.a < 1f)
		{
			color.a = 1f;
		}
		infoLight.initColor = color;
		infoLight.sublights = ctrlFlag.SubLights;
		Light[] sublights = infoLight.sublights;
		foreach (Light light2 in sublights)
		{
			if (!(light2 == null))
			{
				if (!light2.gameObject.activeSelf)
				{
					light2.gameObject.SetActive(value: true);
				}
				if (!light2.enabled)
				{
					light2.enabled = true;
				}
			}
		}
		sprite.SetLightInfo(infoLight);
		NowStateIsEnd = false;
		if (se == null)
		{
			se = ctrlFlag.cameraCtrl.GetComponent<ScreenEffect>();
		}
		se.Init();
		seUI.ChangeMapInit();
		seUI.Init();
		CameraControl_Ver2 cameraCtrl = ctrlFlag.cameraCtrl;
		cameraCtrl.KeyCondition = (BaseCameraControl_Ver2.NoCtrlFunc)Delegate.Combine(cameraCtrl.KeyCondition, (BaseCameraControl_Ver2.NoCtrlFunc)(() => !Singleton<HSceneFlagCtrl>.IsInstance() || !(ctrlFlag != null) || !ctrlFlag.inputForcus));
		setStartVoice = false;
	}

	private void ChangeCoodinate(int EventNo, int peep, ChaControl cha, bool Second = false)
	{
		if (cha.chaID == -1 || cha.chaID == -2)
		{
			return;
		}
		bool flag = EventNo == -1;
		if (!Second)
		{
			if (!flag)
			{
				flag = EventNo == 28 || EventNo == 29 || EventNo == 30 || EventNo == 31 || EventNo == 32;
				flag = flag && (peep == 5 || peep == 6);
			}
		}
		else
		{
			flag = EventNo != 2;
		}
		if (!flag || (!Bath && !Room))
		{
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(cha.chaFile.charaFileName);
		if (fileNameWithoutExtension == null)
		{
			return;
		}
		Dictionary<string, ClothPngInfo> dictionary = Singleton<Game>.Instance.saveData.dicCloths[Singleton<Game>.Instance.saveData.selectGroup];
		if (!dictionary.ContainsKey(fileNameWithoutExtension) || (Bath ? dictionary[fileNameWithoutExtension].bathFile.IsNullOrEmpty() : dictionary[fileNameWithoutExtension].roomWearFile.IsNullOrEmpty()))
		{
			string assetName = (Bath ? "bath" : "roomwear");
			TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(AssetBundleNames.CustomCustom_Etc, assetName);
			if (textAsset != null)
			{
				cha.nowCoordinate.LoadFile(textAsset);
				cha.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
				AssetBundleManager.UnloadAssetBundle(AssetBundleNames.CustomCustom_Etc, isUnloadForceRefCount: true);
			}
		}
		else
		{
			ClothPngInfo clothPngInfo = Singleton<Game>.Instance.saveData.dicCloths[Singleton<Game>.Instance.saveData.selectGroup][fileNameWithoutExtension];
			cha.ChangeNowCoordinate(Bath ? clothPngInfo.bathFile : clothPngInfo.roomWearFile, reload: true);
		}
	}

	private void Update()
	{
		if (!setStartVoice && ctrlFlag.nowAnimationInfo.id != -1)
		{
			SetStartVoice();
			setStartVoice = true;
		}
		if (aInfo != ctrlFlag.nowAnimationInfo)
		{
			aInfo = ctrlFlag.nowAnimationInfo;
		}
		if (ctrlFlag.cameraCtrl.isConfigTargetTex != Manager.Config.CameraData.Look)
		{
			ctrlFlag.cameraCtrl.isConfigTargetTex = Manager.Config.CameraData.Look;
		}
		if (ctrlFlag.rateNip < ctrlFlag.feel_f)
		{
			ctrlFlag.rateNip = ctrlFlag.feel_f;
		}
		float rate = Mathf.Lerp(0f, ctrlFlag.rateNipMax, ctrlFlag.rateNip);
		bool flag = true;
		if (!HSceneSpriteItem.lstUseSliderToggle[0])
		{
			if (ctrlFlag.rateTuya < ctrlFlag.feel_f && flag)
			{
				ctrlFlag.rateTuya = ctrlFlag.feel_f;
			}
		}
		else
		{
			ctrlFlag.rateTuya = HSceneSpriteItem.lstSlider[0].value;
		}
		float num = 0f;
		num = ((!isTuyaOn) ? Mathf.InverseLerp(0f, 100f, ctrlFlag.rateTuya) : 1f);
		if (!flag)
		{
			num = 0f;
		}
		if (HSceneSpriteItem.lstUseSliderToggle[1] != null)
		{
			ctrlFlag.rateWet = HSceneSpriteItem.lstSlider[1].value;
		}
		float wetRate = Mathf.InverseLerp(0f, 100f, ctrlFlag.rateWet);
		for (int i = 0; i < chaFemales.Length && !(chaFemales[i] == null) && !(chaFemales[i].objTop == null); i++)
		{
			chaFemales[i].ChangeNipRate(rate);
			chaFemales[i].skinGlossRate = num;
			chaFemales[i].wetRate = wetRate;
		}
		ChaControl[] array = chaFemales;
		foreach (ChaControl chaControl in array)
		{
			if (!(chaControl == null) && !(chaControl.objBody == null))
			{
				float siriAkaRate = chaControl.siriAkaRate;
				chaControl.ChangeSiriAkaRate(Mathf.Clamp01(siriAkaRate - ctrlFlag.siriakaDecreaseRate * Time.deltaTime));
			}
		}
		array = chaMales;
		foreach (ChaControl chaControl2 in array)
		{
			if (!(chaControl2 == null) && !(chaControl2.objBody == null))
			{
				chaControl2.wetRate = wetRate;
			}
		}
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.SceneEnd)
		{
			EndProc();
		}
		if (NowStateIsEnd)
		{
			return;
		}
		AnimatorStateInfo animatorStateInfo = chaFemales[0].getAnimatorStateInfo(0);
		ctrlVoice.Proc(animatorStateInfo, chaFemales);
		ctrlSiruPastes[0].Proc(animatorStateInfo);
		if (aInfo.nPromiscuity >= 1)
		{
			ctrlSiruPastes[1].Proc(animatorStateInfo);
		}
		if (mode != -1 && modeCtrl != -1 && ProcBase.endInit)
		{
			lstProc[mode].Proc(modeCtrl, aInfo);
		}
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.LeaveItToYou)
		{
			ctrlFlag.initiative = ((ctrlFlag.initiative == 0) ? 1 : 0);
			ctrlFlag.isAutoActionChange = false;
			ctrlAuto.Reset();
			ctrlAuto.AutoAutoLeaveItToYouInit();
			if (ctrlFlag.initiative != 0)
			{
				GetAutoAnimation(_isFirst: false);
				if (ctrlFlag.selectAnimationListInfo == null)
				{
					GetAutoAnimation(_isFirst: true);
				}
			}
			else
			{
				ReturnToNormalFromTheAuto();
			}
		}
		if (ctrlFlag.isAutoActionChange && ctrlFlag.selectAnimationListInfo == null)
		{
			sprite.SetMotionListDraw(_active: false);
			GetAutoAnimation(_isFirst: false);
			if (ctrlFlag.selectAnimationListInfo == null)
			{
				GetAutoAnimation(_isFirst: true);
				if (ctrlFlag.selectAnimationListInfo == null)
				{
					ctrlFlag.isAutoActionChange = false;
				}
			}
			ctrlAuto.SetSpeed(ctrlFlag.speed);
		}
		if (ctrlFlag.selectAnimationListInfo != null && !nowChangeAnim)
		{
			ctrlFlag.voice.playStartOld = -1;
			if (IsAfterIdle(chaFemales[0].animBody) && ctrlFlag.nowAnimationInfo != ctrlFlag.selectAnimationListInfo)
			{
				ctrlFlag.voice.playStart = 3;
			}
			nowChangeAnim = true;
			ChangeAnimVoiceFlag();
			Observable.NextFrame().Subscribe(delegate
			{
				Observable.FromCoroutine(() => ChangeAnimation(ctrlFlag.selectAnimationListInfo, _isForceResetCamera: false, _isForceLoopAction: false, !ctrlFlag.pointMoveAnimChange)).Finally(delegate
				{
					ctrlFlag.selectAnimationListInfo = null;
					ctrlFlag.isAutoActionChange = false;
					if (ctrlFlag.pointMoveAnimChange)
					{
						ctrlFlag.pointMoveAnimChange = false;
					}
					GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: true);
					sprite.ChangeStart = false;
					if (IsIdle(chaFemales[0].animBody) && ctrlFlag.voice.playStartOld != 3)
					{
						ctrlFlag.voice.playStart = 2;
						ctrlFlag.voice.playStartOld = -1;
					}
				}).Subscribe();
			});
		}
		PositionShift();
		for (int num2 = 0; num2 < ctrlHitObjectFemales.Length && (aInfo.nPromiscuity >= 1 || num2 <= 0); num2++)
		{
			if (!(chaFemales[num2] == null) && !(chaFemales[num2].objBodyBone == null))
			{
				ctrlHitObjectFemales[num2].Proc(chaFemales[num2].animBody);
				ctrlEyeNeckFemale[num2].Proc(animatorStateInfo, ctrlVoice.nowVoices[num2].Face, num2);
			}
		}
		for (int num3 = 0; num3 < ctrlHitObjectMales.Length && (aInfo.nPromiscuity == 0 || num3 <= 0); num3++)
		{
			if (!(chaMales[num3] == null) && !(chaMales[num3].objTop == null) && !(chaMales[num3].objBodyBone == null))
			{
				ctrlHitObjectMales[num3].Proc(chaMales[num3].animBody);
				ctrlEyeNeckMale[num3].Proc(animatorStateInfo);
			}
		}
		ctrlDynamics[0].Proc();
		if (aInfo.nPromiscuity >= 1)
		{
			ctrlDynamics[1].Proc();
		}
		ctrlSE.Proc(animatorStateInfo, chaFemales);
		sprite.GuidProc(animatorStateInfo);
		sprite.SetLimitResist(animatorStateInfo);
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.MovePointNext)
		{
			hPointCtrl.MovePoint(1);
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.MovePointBack)
		{
			hPointCtrl.MovePoint(0);
		}
		ShortcutKey();
		bool flag2 = Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog);
		if (!flag2)
		{
			flag2 = Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) || ConfigWindow.isActive;
		}
		if (!flag2)
		{
			flag2 = Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) || ShortcutViewDialog.isActive;
		}
		if (!flag2)
		{
			flag2 = Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) || global::Tutorial2D.Tutorial2D.isActive;
		}
		if (!flag2)
		{
			flag2 = Scene.IsFadeNow;
		}
		if (!flag2)
		{
			if (!GlobalMethod.IsCameraMoveFlag(ctrlFlag.cameraCtrl) && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
			{
				GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: true);
			}
		}
		else if (GlobalMethod.IsCameraMoveFlag(ctrlFlag.cameraCtrl))
		{
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		}
		ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
		ctrlParitcle.Proc();
		if (ctrlFlag.selectAnimationListInfo == null && NowChangeAnim && fade.isEnd)
		{
			nowChangeAnim = false;
		}
	}

	public void SetStartAnimationInfo()
	{
		if (Voice.IsPlay(ctrlFlag.voice.voiceTrs[0]))
		{
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
		}
		int category = 0;
		int id = 0;
		int eventNo = Singleton<Game>.Instance.eventNo;
		StartAnimInfo = null;
		Dictionary<int, StartAnimPatternInfo[]> startPattern = HSceneManager.HResourceTables.StartPattern;
		if (eventNo == 12 || eventNo == 13 || eventNo == 14 || eventNo == 58)
		{
			switch (eventNo)
			{
			case 12:
				category = 1;
				id = 0;
				break;
			case 13:
				category = 2;
				id = 14;
				break;
			case 14:
				category = 5;
				id = 107;
				break;
			case 58:
				category = 5;
				id = 113;
				break;
			}
		}
		else if (startPattern.ContainsKey(eventNo))
		{
			int firstVoiceNo = Singleton<Game>.Instance.firstVoiceNo;
			if (firstVoiceNo >= 0 && startPattern[eventNo].Length != 0 && startPattern[eventNo].Length > firstVoiceNo && startPattern[eventNo][firstVoiceNo].category != -1)
			{
				category = startPattern[eventNo][firstVoiceNo].category;
				id = startPattern[eventNo][firstVoiceNo].id;
			}
			else
			{
				CheckStartBase(ref category, ref id);
			}
		}
		else
		{
			List<int[]> animEventJudgePtn = HSceneManager.HResourceTables.AnimEventJudgePtn;
			bool flag = false;
			foreach (int[] item in animEventJudgePtn)
			{
				if (item[1] != -1)
				{
					if (item[0] != eventNo || item[1] != Singleton<Game>.Instance.peepKind)
					{
						continue;
					}
				}
				else if (item[0] != eventNo)
				{
					continue;
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				CheckStartBase(ref category, ref id);
			}
			else
			{
				CheckStartEvent(eventNo, ref category, ref id);
			}
		}
		foreach (AnimationListInfo item2 in lstAnimInfo[category])
		{
			if (item2.id == id)
			{
				StartAnimInfo = item2;
				break;
			}
		}
		if (StartAnimInfo == null)
		{
			StartAnimInfo = lstAnimInfo[0][0];
		}
		Observable.FromCoroutine(() => StartAnim(StartAnimInfo)).Finally(delegate
		{
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: true);
			Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
			base.enabled = true;
			sprite.SetStartTaii(StartAnimInfo);
		}).Subscribe();
	}

	private void CheckStartBase(ref int category, ref int id)
	{
		List<int[]> randList = new List<int[]>();
		int eventNo = Singleton<Game>.Instance.eventNo;
		int peepKind = Singleton<Game>.Instance.peepKind;
		bool num = (eventNo == 3 || eventNo == 4 || eventNo == 5 || eventNo == 6) && peepKind == 4;
		bool flag = eventNo == 19;
		AnimationListInfo info = null;
		if (num)
		{
			CheckDependencyStart(ref randList);
		}
		else if (flag)
		{
			for (int i = 0; i < lstAnimInfo.Length; i++)
			{
				for (int j = 0; j < lstAnimInfo[i].Count; j++)
				{
					if (sprite.CheckMotionLimit(lstAnimInfo[i][j]) && lstAnimInfo[i][j].nDownPtn != 0 && (lstAnimInfo[i][j].Achievments == null || lstAnimInfo[i][j].Achievments.Count == 0) && lstAnimInfo[i][j].ReleaseEvent == -1)
					{
						randList.Add(new int[2]
						{
							i,
							lstAnimInfo[i][j].id
						});
					}
				}
			}
		}
		else
		{
			int num2 = ((chaFemales[0].fileGameInfo2.resistH >= 100) ? 1 : 0);
			int key = hSceneManager.FemaleState[0] switch
			{
				ChaFileDefine.State.Favor => 0, 
				ChaFileDefine.State.Enjoyment => 1, 
				ChaFileDefine.State.Slavery => 2, 
				ChaFileDefine.State.Aversion => 3, 
				ChaFileDefine.State.Broken => 4, 
				ChaFileDefine.State.Dependence => 5, 
				_ => 6, 
			};
			Dictionary<int, StartAnimInfo> startBase = HSceneManager.HResourceTables.StartBase;
			if (!startBase.ContainsKey(key))
			{
				category = 0;
				id = 0;
				return;
			}
			if (startBase[key] == null)
			{
				category = 0;
				id = 0;
				return;
			}
			if (startBase[key].AnimIDs.Length < num2 || startBase[key].AnimIDs[num2] == null)
			{
				category = 0;
				id = 0;
				return;
			}
			List<int>[] iD = HSceneManager.HResourceTables.StartBase[key].AnimIDs[num2].ID;
			for (int k = 0; k < iD.Length; k++)
			{
				int num3 = k;
				for (int l = 0; l < iD[num3].Count; l++)
				{
					foreach (AnimationListInfo item in lstAnimInfo[num3])
					{
						if (item.id == iD[num3][l])
						{
							info = item;
							break;
						}
					}
					if (sprite.CheckPlace(info))
					{
						randList.Add(new int[2]
						{
							num3,
							iD[num3][l]
						});
					}
				}
			}
		}
		if (randList.Count == 0)
		{
			category = 0;
			id = 0;
		}
		else
		{
			int index = new ShuffleRand(randList.Count).Get();
			category = randList[index][0];
			id = randList[index][1];
		}
	}

	private void CheckDependencyStart(ref List<int[]> randList)
	{
		for (int i = 0; i < lstAnimInfo.Length; i++)
		{
			for (int j = 0; j < lstAnimInfo[i].Count; j++)
			{
				if (sprite.CheckMotionLimit(lstAnimInfo[i][j]) && lstAnimInfo[i][j].nInitiativeFemale != 0 && lstAnimInfo[i][j].ReleaseEvent == -1)
				{
					randList.Add(new int[2]
					{
						i,
						lstAnimInfo[i][j].id
					});
				}
			}
		}
	}

	private void CheckStartEvent(int eventNo, ref int category, ref int id)
	{
		int peepKind = Singleton<Game>.Instance.peepKind;
		if (!(hPointCtrl == null))
		{
			_ = hPointCtrl.HPointList == null;
		}
		List<int[]> list = new List<int[]>();
		for (int i = 0; i < lstAnimInfo.Length; i++)
		{
			int num = i;
			for (int j = 0; j < lstAnimInfo[num].Count; j++)
			{
				int index = j;
				if (sprite.CheckMotionLimit(lstAnimInfo[num][index]) && ((eventNo != 3 && eventNo != 4 && eventNo != 5 && eventNo != 6) || peepKind != 4 || lstAnimInfo[num][index].nInitiativeFemale != 0))
				{
					list.Add(new int[2]
					{
						num,
						lstAnimInfo[num][index].id
					});
				}
			}
		}
		if (list.Count == 0)
		{
			category = 0;
			id = 0;
		}
		else
		{
			int index2 = new ShuffleRand(list.Count).Get();
			category = list[index2][0];
			id = list[index2][1];
		}
	}

	private IEnumerator StartAnim(AnimationListInfo StartAnimInfo)
	{
		yield return ChangeAnimation(StartAnimInfo, _isForceResetCamera: true, _isForceLoopAction: false, _UseFade: false);
	}

	private void EndProc()
	{
		ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
		NowStateIsEnd = true;
		if (se == null)
		{
			se = ctrlFlag.cameraCtrl.GetComponent<ScreenEffect>();
		}
		se.endSave();
		hSceneManager.endStatus = (byte)(ctrlFlag.isFaintness ? 1u : 0u);
		EndProcADV();
	}

	private void LateUpdate()
	{
		if (!ProcBase.endInit)
		{
			return;
		}
		HSystem hData = Manager.Config.HData;
		List<int> list = new List<int>();
		list.Add(0);
		list.Add(2);
		list.Add(4);
		list.Add(1);
		list.Add(3);
		list.Add(5);
		list.Add(6);
		foreach (int item in list)
		{
			if (chaMales[0].IsClothesStateKind(item))
			{
				byte state = 0;
				if (!hData.Cloth)
				{
					state = 2;
				}
				chaMales[0].SetClothesState(item, state);
			}
		}
		chaMales[0].SetAccessoryStateAll(hData.Accessory);
		chaMales[0].SetClothesState(7, (byte)((!hData.Shoes) ? 2u : 0u));
		if (chaMales[1] != null)
		{
			foreach (int item2 in list)
			{
				if (chaMales[1].IsClothesStateKind(item2))
				{
					byte state2 = 0;
					if (!hData.SecondCloth)
					{
						state2 = 2;
					}
					chaMales[1].SetClothesState(item2, state2);
				}
			}
			chaMales[1].SetAccessoryStateAll(hData.SecondAccessory);
			chaMales[1].SetClothesState(7, (byte)((!hData.SecondShoes) ? 2u : 0u));
		}
		ctrlFlag.semenType = Manager.Config.HData.SiruDraw;
		hSceneManager.UrineType = Manager.Config.HData.UrineDraw;
		if (mapVisible != Manager.Config.GraphicData.Map)
		{
			SingletonInitializer<BaseMap>.instance.MapVisible(Manager.Config.GraphicData.Map);
			mapVisible = Manager.Config.GraphicData.Map;
			ctrlFlag.cameraCtrl.thisCamera.clearFlags = (mapVisible ? CameraClearFlags.Skybox : CameraClearFlags.Color);
		}
		if (mapLight != Manager.Config.GraphicData.AmbientLight)
		{
			SingletonInitializer<BaseMap>.instance.EnvironmentLightObjectsVisible(Manager.Config.GraphicData.AmbientLight);
			mapLight = Manager.Config.GraphicData.AmbientLight;
		}
		ctrlFlag.cameraCtrl.thisCamera.backgroundColor = Manager.Config.GraphicData.BackColor;
		ctrlFlag.cameraCtrl.ConfigVanish = Manager.Config.GraphicData.Shield;
		SyncAnimation();
	}

	public void ConfigEnd()
	{
		ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
		NowStateIsEnd = true;
		sprite.ReSetLight();
		hSceneManager.endStatus = (byte)(ctrlFlag.isFaintness ? 1u : 0u);
		if (ctrlEyeNeckFemale[0] != null && chaFemales[0] != null && chaFemales[0].objBodyBone != null)
		{
			ctrlEyeNeckFemale[0].NowEndADV = true;
		}
		if (ctrlEyeNeckFemale[1] != null && chaFemales[1] != null && chaFemales[1].objBodyBone != null)
		{
			ctrlEyeNeckFemale[1].NowEndADV = true;
		}
		if (ctrlEyeNeckMale[0] != null && chaMales[0] != null && chaMales[0].objBodyBone != null)
		{
			ctrlEyeNeckMale[0].NowEndADV = true;
		}
		if (ctrlEyeNeckMale[1] != null && chaMales[1] != null && chaMales[1].objBodyBone != null)
		{
			ctrlEyeNeckMale[1].NowEndADV = true;
		}
		if (hSceneManager.player != null)
		{
			if (!hSceneManager.player.neckLookCtrl.enabled)
			{
				hSceneManager.player.neckLookCtrl.enabled = true;
			}
			if (!hSceneManager.player.eyeLookCtrl.enabled)
			{
				hSceneManager.player.eyeLookCtrl.enabled = true;
			}
			hSceneManager.player.ChangeLookNeckPtn(3);
			hSceneManager.player.ChangeLookNeckTarget(0);
			hSceneManager.player.ChangeLookEyesPtn(0);
			hSceneManager.player.ChangeLookEyesTarget(0);
			hSceneManager.player.ChangeMouthOpenMin(hSceneManager.player.fileStatus.mouthOpenMin);
		}
		if (hSceneManager.females != null)
		{
			for (int i = 0; i < hSceneManager.females.Length; i++)
			{
				if (!(hSceneManager.females[i] == null) && !(hSceneManager.females[i].neckLookCtrl == null))
				{
					if (!hSceneManager.females[i].neckLookCtrl.enabled)
					{
						hSceneManager.females[i].neckLookCtrl.enabled = true;
					}
					if (!hSceneManager.females[i].eyeLookCtrl.enabled)
					{
						hSceneManager.females[i].eyeLookCtrl.enabled = true;
					}
					hSceneManager.females[i].ChangeLookNeckPtn(3);
					hSceneManager.females[i].ChangeLookNeckTarget(0);
					hSceneManager.females[i].ChangeLookEyesPtn(0);
					hSceneManager.females[i].ChangeLookEyesTarget(0);
				}
			}
		}
		for (int j = 0; j < chaMales.Length; j++)
		{
			if (!(chaMales[j] == null) && !(chaMales[j].objBody == null))
			{
				chaMales[j].visibleSon = false;
			}
		}
		ctrlFlag.cameraCtrl.Reset(0);
		ctrlFlag.selectAnimationListInfo = null;
		if (ctrlItem != null)
		{
			ctrlItem.ReleaseItem();
		}
		ctrlFlag.cameraCtrl.visibleForceVanish(_visible: true);
		ctrlObi.EndPlocSolver();
		for (int k = 0; k < ctrlHitObjectFemales.Length; k++)
		{
			if (ctrlHitObjectFemales[k] != null)
			{
				ctrlHitObjectFemales[k].PreEndPloc();
				ctrlHitObjectFemales[k].EndPloc();
			}
		}
		for (int l = 0; l < ctrlHitObjectMales.Length; l++)
		{
			if (ctrlHitObjectMales[l] != null)
			{
				ctrlHitObjectMales[l].PreEndPloc();
				ctrlHitObjectMales[l].EndPloc();
			}
		}
		Observable.TimerFrame(2).Subscribe(delegate
		{
			if (chaMales[1] != null)
			{
				Singleton<Character>.Instance.DeleteChara(chaMales[1]);
			}
			hPointCtrl.EndProc();
		});
	}

	private void OnDisable()
	{
		if (nowStart)
		{
			return;
		}
		hSceneManager.isForce = false;
		hSceneManager.isForceSecond = false;
		for (int i = 0; i < 2; i++)
		{
			if (ctrlDynamics[i] != null)
			{
				ctrlDynamics[i].InitDynamicBoneReferenceBone();
			}
		}
		if (ctrlVoice != null)
		{
			if (chaFemales[0] != null)
			{
				ctrlVoice.FaceReset(chaFemales[0]);
			}
			if (chaFemales[1] != null && chaFemales[1].objTop != null)
			{
				ctrlVoice.FaceReset(chaFemales[1]);
			}
		}
		for (int j = 0; j < ctrlYures.Length; j++)
		{
			if (ctrlYures[j] != null)
			{
				ctrlYures[j].ResetShape();
			}
		}
		for (int k = 0; k < ctrlYureMale.Length; k++)
		{
			if (ctrlYureMale[k] != null)
			{
				ctrlYureMale[k].ResetShape();
			}
		}
		if (ctrlItem != null)
		{
			ctrlItem.ReleaseItem();
		}
		ctrlObi.EndPloc();
	}

	private void OnDestroy()
	{
		if (ctrlSiruPastes != null)
		{
			SiruPasteCtrl[] array = ctrlSiruPastes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Release();
			}
		}
		ChaControl[] array2 = chaFemales;
		foreach (ChaControl chaControl in array2)
		{
			if (!(chaControl == null) && !(chaControl.objBody == null))
			{
				chaControl.playDynamicBoneBust(0, play: true);
				chaControl.playDynamicBoneBust(1, play: true);
				chaControl.fileStatus.skinTuyaRate = 0f;
				chaControl.ChangeEyesOpenMax(1f);
				FBSCtrlMouth mouthCtrl = chaControl.mouthCtrl;
				if (mouthCtrl != null)
				{
					mouthCtrl.OpenMin = 0f;
				}
				bool flag = hSceneManager.mapID == 4;
				if (!flag && Singleton<Game>.IsInstance())
				{
					int eventNo = Singleton<Game>.Instance.eventNo;
					flag = hSceneManager.mapID == 52 || hSceneManager.mapID == 53;
					flag &= (eventNo == 3 || eventNo == 4) && Singleton<Game>.Instance.peepKind == 4;
					flag = flag || eventNo == 50;
				}
				if (!flag)
				{
					chaControl.wetRate = 0f;
				}
				chaControl.DisableShapeMouth(disable: false);
				for (int j = 0; j < 7; j++)
				{
					int id = j;
					chaControl.DisableShapeBodyID(2, id, disable: false);
				}
				chaControl.DisableShapeBodyID(2, 7, disable: false);
			}
		}
		CtrlParticle.EndProc();
		if (HSceneManager.HResourceTables != null)
		{
			HSceneManager.HResourceTables.ReleaceHitObj();
		}
		if ((bool)objGrondCollision)
		{
			UnityEngine.Object.Destroy(objGrondCollision);
			objGrondCollision = null;
		}
		if (SingletonInitializer<Manager.Sound>.initialized)
		{
			Manager.Sound.Stop(Manager.Sound.Type.GameSE2D);
			Manager.Sound.Stop(Manager.Sound.Type.GameSE3D);
		}
		if (SingletonInitializer<Voice>.initialized)
		{
			Voice.StopAll();
		}
		if (Singleton<GameCursor>.IsInstance())
		{
			Singleton<GameCursor>.Instance.SetCursorLock(setLockFlag: false);
		}
		AssetBundleManager.UnloadAssetBundle(hSceneManager.strAssetSE, isUnloadForceRefCount: true);
		for (int k = 0; k < ctrlFlag.voice.lstUseAsset.Count; k++)
		{
			AssetBundleManager.UnloadAssetBundle(ctrlFlag.voice.lstUseAsset[k], isUnloadForceRefCount: true);
		}
		if (Singleton<HSceneManager>.IsInstance())
		{
			foreach (string item in hSceneManager.hashUseAssetBundle)
			{
				AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: true);
			}
			hSceneManager.hashUseAssetBundle.Clear();
		}
		ctrlLayer.Release();
		HMotionEyeNeckFemale[] array3 = ctrlEyeNeckFemale;
		foreach (HMotionEyeNeckFemale hMotionEyeNeckFemale in array3)
		{
			if (!(hMotionEyeNeckFemale == null))
			{
				hMotionEyeNeckFemale.Release();
			}
		}
		HMotionEyeNeckMale[] array4 = ctrlEyeNeckMale;
		foreach (HMotionEyeNeckMale hMotionEyeNeckMale in array4)
		{
			if (!(hMotionEyeNeckMale == null))
			{
				hMotionEyeNeckMale.Release();
			}
		}
		seUI.EndProc();
		mode = -1;
		modeCtrl = -1;
		nowStart = false;
		nullPlayer = true;
		NowStateIsEnd = false;
		nowChangeAnim = false;
		lstMotionIK.Clear();
		lstProc.Clear();
		hSceneManager.females[0] = null;
		hSceneManager.females[1] = null;
		ctrlObi = null;
		runtimeAnimatorControllers = null;
		hSceneManager.SecondSitori = false;
		ctrlFlag.EndProc();
		hSceneManager.Hscene = null;
		hSceneManager.EndHScene();
		hSceneManager = null;
		GC.Collect();
		UnityEngine.Resources.UnloadUnusedAssets();
	}

	private void CreateListAnimationFileName()
	{
		lstAnimInfo = HSceneManager.HResourceTables.lstAnimInfo;
	}

	private void SyncAnimation()
	{
		if (chaFemales[0].animBody == null)
		{
			return;
		}
		AnimatorStateInfo animatorStateInfo = chaFemales[0].getAnimatorStateInfo(0);
		List<int> list = ctrlFlag.lstSyncAnimLayers[0, 0];
		for (int i = 1; i < ctrlFlag.lstSyncAnimLayers.GetLength(0); i++)
		{
			for (int j = 1; j < ctrlFlag.lstSyncAnimLayers.GetLength(1); j++)
			{
				for (int k = 0; k < ctrlFlag.lstSyncAnimLayers[i, j].Count; k++)
				{
					if (!list.Contains(ctrlFlag.lstSyncAnimLayers[i, j][k]))
					{
						list.Add(ctrlFlag.lstSyncAnimLayers[i, j][k]);
					}
				}
			}
		}
		ctrlItem.ParentScaleReject();
		if (chaMales[0] == null || chaMales[0].animBody == null || chaMales[0].objBodyBone == null)
		{
			if (!chaFemales[0].isBlend(0) && ctrlItem.GetItem() != null)
			{
				ctrlItem.syncPlay(animatorStateInfo);
				ctrlItem.Update();
			}
			if ((bool)chaFemales[1])
			{
				chaFemales[1].syncPlay(animatorStateInfo, 0);
			}
			if (list.Count == 0)
			{
				isSyncFirstStep = false;
				return;
			}
			if (!isSyncFirstStep)
			{
				isSyncFirstStep = true;
				return;
			}
			for (int l = 0; l < chaFemales.Length; l++)
			{
				if (chaFemales[l] == null || ((bool)chaFemales[l] && chaFemales[l].animBody == null))
				{
					continue;
				}
				for (int m = 0; m < ctrlFlag.lstSyncAnimLayers[1, l].Count; m++)
				{
					int num = ctrlFlag.lstSyncAnimLayers[1, l][m];
					if (chaFemales[l].animBody.layerCount > num)
					{
						chaFemales[l].syncPlay(animatorStateInfo, num);
					}
				}
			}
			return;
		}
		if (chaFemales[1] != null && chaFemales[1].objTop != null && ctrlFlag.nowAnimationInfo.nPromiscuity > 0)
		{
			chaFemales[1].syncPlay(animatorStateInfo, 0);
		}
		for (int n = 0; n < chaMales.Length && (n != 0 || !ctrlFlag.nowAnimationInfo.fileMale.IsNullOrEmpty()); n++)
		{
			if ((n == 1 && ctrlFlag.nowAnimationInfo.fileMale2.IsNullOrEmpty()) || chaMales[n] == null || chaMales[n].objTop == null)
			{
				continue;
			}
			for (int num2 = 0; num2 < ctrlFlag.lstSyncAnimLayers[0, n].Count; num2++)
			{
				int num3 = ctrlFlag.lstSyncAnimLayers[0, n][num2];
				if (chaMales[n].animBody.layerCount > num3)
				{
					chaMales[num2].syncPlay(animatorStateInfo, 0);
				}
			}
		}
		if (ctrlItem.GetItem() != null)
		{
			ctrlItem.syncPlay(animatorStateInfo);
			ctrlItem.Update();
		}
		if (ctrlFlag.lstSyncAnimLayers[1, 0].Count == 0)
		{
			isSyncFirstStep = false;
			return;
		}
		if (!isSyncFirstStep)
		{
			isSyncFirstStep = true;
			return;
		}
		bool flag = false;
		bool flag2 = false;
		for (int num4 = 0; num4 < chaFemales.Length; num4++)
		{
			if (chaFemales[num4] == null || ((bool)chaFemales[num4] && chaFemales[num4].animBody == null))
			{
				continue;
			}
			for (int num5 = 0; num5 < ctrlFlag.lstSyncAnimLayers[1, num4].Count; num5++)
			{
				int num6 = ctrlFlag.lstSyncAnimLayers[1, num4][num5];
				if (chaFemales[num4].animBody.layerCount > num6)
				{
					chaFemales[num4].syncPlay(animatorStateInfo, num6);
					flag = true;
				}
			}
		}
		for (int num7 = 0; num7 < chaMales.Length; num7++)
		{
			if (chaMales[num7] == null || ((bool)chaMales[num7] && chaMales[num7].animBody == null))
			{
				continue;
			}
			for (int num8 = 0; num8 < ctrlFlag.lstSyncAnimLayers[0, num7].Count; num8++)
			{
				int num9 = ctrlFlag.lstSyncAnimLayers[0, num7][num8];
				if (chaMales[num7].animBody.layerCount > num9)
				{
					chaMales[num7].syncPlay(animatorStateInfo, num9);
					flag2 = true;
				}
			}
		}
		if (flag)
		{
			for (int num10 = 0; num10 < chaFemales.Length; num10++)
			{
				if (!(chaFemales[num10] == null) && (!chaFemales[num10] || !(chaFemales[num10].animBody == null)))
				{
					chaFemales[num10].animBody.Update(0f);
				}
			}
		}
		if (flag2)
		{
			for (int num11 = 0; num11 < chaMales.Length; num11++)
			{
				if (!(chaMales[num11] == null) && (!chaMales[num11] || !(chaMales[num11].animBody == null)))
				{
					chaMales[num11].animBody.Update(0f);
				}
			}
		}
		for (int num12 = 0; num12 < ctrlEyeNeckFemale.Length; num12++)
		{
			if (!(ctrlEyeNeckFemale[num12] == null))
			{
				ctrlEyeNeckFemale[num12].EyeNeckCalc();
			}
		}
		for (int num13 = 0; num13 < ctrlEyeNeckMale.Length; num13++)
		{
			if (!(ctrlEyeNeckMale[num13] == null))
			{
				ctrlEyeNeckMale[num13].EyeNeckCalc();
			}
		}
	}

	public IEnumerator ChangeAnimation(AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction = false, bool _UseFade = true)
	{
		GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		nowChangeAnim = true;
		ProcBase.endInit = false;
		if (_info == null)
		{
			nowChangeAnim = false;
			ProcBase.endInit = true;
			yield break;
		}
		if (ctrlFlag.nowAnimationInfo == _info)
		{
			nowChangeAnim = false;
			ProcBase.endInit = true;
			yield break;
		}
		if (ctrlFlag.isFaintness && _info.nDownPtn == 0 && ctrlFlag.FaintnessType != 2)
		{
			nowChangeAnim = false;
			ProcBase.endInit = true;
			yield break;
		}
		while (CheckSpeek())
		{
			yield return null;
		}
		sbLoadFileName.Clear();
		int oldMode = ctrlFlag.nowAnimationInfo.ActionCtrl.Item1;
		bool isIdle = IsIdle(chaFemales[0].animBody) || _info.ActionCtrl.Item1 != oldMode;
		if (isIdle && _isForceLoopAction)
		{
			isIdle = false;
		}
		bool afterIdle = IsAfterIdle(chaFemales[0].animBody);
		(string, string)[,] oldModeBase = SetOldAnimatorInfo(oldMode);
		ChangeModeCtrl(_info);
		bool NumCharaChange = false;
		ctrlFeelHit.SetFeelAnimInfo(_info);
		CtrlParticle.ReleaseObject();
		Game game = Singleton<Game>.Instance;
		int num;
		for (int s = 0; s < chaMales.Length; s = num)
		{
			bool flag = false;
			switch (s)
			{
			case 0:
				flag = _info.fileMale != "";
				break;
			case 1:
				flag = _info.fileMale2 != "";
				break;
			}
			if (flag)
			{
				if (!(chaMales[s] == null))
				{
					if (chaMales[s].objTop == null)
					{
						chaMales[s].Load();
						chaMales[s].SetShapeBodyValue(0, 0.75f);
						chaMales[s].visibleAll = false;
						chaMales[s].isPlayer = true;
						chaMales[s].LoadHitObject();
						ctrlMaleCollisionCtrls[s].Init(chaFemales[0], chaMales[s].objHitHead, chaMales[s].objHitBody);
						ctrlLookAts[s].DankonInit(chaMales[s], chaFemales);
						if (Bath && game.saveData.playerCloths != null && game.saveData.playerCloths[s] != null && !game.saveData.playerCloths[s].file.IsNullOrEmpty())
						{
							chaMales[s].ChangeNowCoordinate(game.saveData.playerCloths[s].file, reload: true);
						}
						yield return null;
						yield return null;
					}
					NumCharaChange = true;
					if (!ctrlHitObjectMales[s].isInit)
					{
						ctrlHitObjectMales[s].id = s;
						yield return ctrlHitObjectMales[s].HitObjInit(0, chaMales[s].objBodyBone, chaMales[s]);
					}
					ctrlHitObjectMales[s].setActiveObject(val: true);
				}
			}
			else if (!(chaMales[s] == null) && chaMales[s].visibleAll && ctrlHitObjectMales[s] != null)
			{
				if (ctrlHitObjectMales[s].isInit)
				{
					ctrlHitObjectMales[s].setActiveObject(val: false);
				}
				NumCharaChange = true;
				ctrlFlag.lstSyncAnimLayers[0, s].Clear();
			}
			num = s + 1;
		}
		int eventNo = Singleton<Game>.Instance.eventNo;
		int peep = Singleton<Game>.Instance.peepKind;
		for (int s = 0; s < chaFemales.Length; s = num)
		{
			if (!(chaFemales[s] == null))
			{
				bool flag2 = s == 0;
				if (!flag2)
				{
					flag2 = _info.nPromiscuity >= 1;
				}
				if (flag2)
				{
					if (chaFemales[s].objTop == null)
					{
						chaFemales[s].Load();
						chaFemales[s].visibleAll = false;
						ChangeCoodinate(eventNo, peep, chaFemales[s], s == 1);
						chaFemales[s].LoadHitObject();
						ctrlFemaleCollisionCtrls[s].Init(chaFemales[s], chaFemales[s].objHitHead, chaFemales[s].objHitBody);
						yield return null;
						yield return null;
					}
					NumCharaChange = true;
					if (!ctrlHitObjectFemales[s].isInit)
					{
						ctrlHitObjectFemales[s].id = s;
						yield return ctrlHitObjectFemales[s].HitObjInit(1, chaFemales[s].objBodyBone, chaFemales[s]);
					}
				}
				else if (!(chaFemales[s] == null) && chaFemales[s].visibleAll)
				{
					NumCharaChange = true;
					ctrlVoice.nowVoices[s].state = HVoiceCtrl.VoiceKind.breath;
					ctrlFlag.voice.playShorts[s] = -1;
					ctrlFlag.voice.playVoices[s] = false;
					ctrlFlag.lstSyncAnimLayers[1, s].Clear();
				}
			}
			num = s + 1;
		}
		abName = GlobalMethod.GetAssetBundleNameListFromPath(hSceneManager.strAssetIKListFolder);
		yield return null;
		AnimationListInfo tmpAddInfo = null;
		racM[0] = null;
		racM[1] = null;
		HoushiRacM[0] = null;
		HoushiRacM[1] = null;
		hashUseAssetBundleAnimator.Clear();
		for (int s = 0; s < chaMales.Length; num = s + 1, s = num)
		{
			if (chaMales[s] == null || chaMales[s].animBody == null || _info.fileMale == "")
			{
				continue;
			}
			if (_info.id == -1)
			{
				chaMales[s].LoadAnimation(_info.assetpathMale, _info.fileMale);
			}
			else if (mode == 1 && _info.nDownPtn == 1)
			{
				foreach (AnimationListInfo tmpInfo in lstAnimInfo[1])
				{
					bool flag3 = false;
					foreach (int nPositon in tmpInfo.nPositons)
					{
						if (_info.nPositons.Contains(nPositon))
						{
							flag3 = true;
							break;
						}
					}
					if (_info.ActionCtrl.Item2 == tmpInfo.ActionCtrl.Item2 && flag3 && tmpInfo.nFaintnessLimit == 1)
					{
						sbLoadFileName.Clear();
						sbLoadFileName.Append(tmpInfo.fileMale);
						HoushiRacM[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(tmpInfo.assetpathMale, sbLoadFileName.ToString());
						hashUseAssetBundleAnimator.Add(tmpInfo.assetpathMale);
						yield return null;
						tmpAddInfo = tmpInfo;
						break;
					}
				}
				if (!_info.assetBaseM.IsNullOrEmpty() && !racEtcM.ContainsKey(_info.assetBaseM))
				{
					racEtcM.Add(_info.assetBaseM, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseM, _info.assetBaseM));
					hashUseAssetBundleAnimator.Add(_info.assetpathBaseM);
				}
				sbLoadFileName.Clear();
				sbLoadFileName.Append(_info.fileMale);
				racM[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathMale, sbLoadFileName.ToString());
				hashUseAssetBundleAnimator.Add(_info.assetpathMale);
				yield return null;
			}
			else if (_info.nPromiscuity != 0)
			{
				if (s != 0)
				{
					continue;
				}
				if (!_info.assetBaseM.IsNullOrEmpty() && !racEtcM.ContainsKey(_info.assetBaseM))
				{
					racEtcM.Add(_info.assetBaseM, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseM, _info.assetBaseM));
					hashUseAssetBundleAnimator.Add(_info.assetpathBaseM);
				}
				sbLoadFileName.Clear();
				sbLoadFileName.Append(_info.fileMale);
				racM[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathMale, sbLoadFileName.ToString());
				hashUseAssetBundleAnimator.Add(_info.assetpathMale);
			}
			else if (s == 0)
			{
				if (!racEtcM.ContainsKey(_info.assetBaseM))
				{
					racEtcM.Add(_info.assetBaseM, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseM, _info.assetBaseM));
					hashUseAssetBundleAnimator.Add(_info.assetpathBaseM);
				}
				sbLoadFileName.Clear();
				sbLoadFileName.Append(_info.fileMale);
				racM[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathMale, sbLoadFileName.ToString());
				hashUseAssetBundleAnimator.Add(_info.assetpathMale);
			}
			else
			{
				if (!racEtcM.ContainsKey(_info.assetBaseM2))
				{
					racEtcM.Add(_info.assetBaseM2, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseM2, _info.assetBaseM2));
					hashUseAssetBundleAnimator.Add(_info.assetpathBaseM2);
				}
				sbLoadFileName.Clear();
				sbLoadFileName.Append(_info.fileMale2);
				racM[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathMale2, sbLoadFileName.ToString());
				hashUseAssetBundleAnimator.Add(_info.assetpathMale2);
			}
			yield return null;
		}
		racF[0] = null;
		racF[1] = null;
		HoushiRacF[0] = null;
		HoushiRacF[1] = null;
		for (int s = 0; s < chaFemales.Length; s = num)
		{
			if (!(chaFemales[s] == null) && !(chaFemales[s].animBody == null))
			{
				if (_info.id == -1)
				{
					chaFemales[s].LoadAnimation(_info.assetpathFemale, _info.fileFemale);
				}
				else if (mode == 1 && _info.nDownPtn == 1)
				{
					if (tmpAddInfo == null)
					{
						foreach (AnimationListInfo tmpInfo in lstAnimInfo[1])
						{
							bool flag4 = false;
							foreach (int nPositon2 in tmpInfo.nPositons)
							{
								if (_info.nPositons.Contains(nPositon2))
								{
									flag4 = true;
									break;
								}
							}
							if (!(_info.ActionCtrl.Item2 != tmpInfo.ActionCtrl.Item2 || flag4) && tmpInfo.nFaintnessLimit == 1)
							{
								sbLoadFileName.Clear();
								sbLoadFileName.Append(tmpInfo.fileFemale);
								HoushiRacF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(tmpInfo.assetpathFemale, sbLoadFileName.ToString());
								hashUseAssetBundleAnimator.Add(tmpInfo.assetpathFemale);
								yield return null;
								tmpAddInfo = tmpInfo;
								break;
							}
						}
					}
					else
					{
						sbLoadFileName.Clear();
						sbLoadFileName.Append(tmpAddInfo.fileFemale);
						HoushiRacF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(tmpAddInfo.assetpathFemale, sbLoadFileName.ToString());
						hashUseAssetBundleAnimator.Add(tmpAddInfo.assetpathFemale);
						yield return null;
					}
					if (!_info.assetBaseF.IsNullOrEmpty() && !racEtcF.ContainsKey(_info.assetBaseF))
					{
						racEtcF.Add(_info.assetBaseF, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseF, _info.assetBaseF));
						hashUseAssetBundleAnimator.Add(_info.assetpathBaseF);
					}
					sbLoadFileName.Clear();
					sbLoadFileName.Append(_info.fileFemale);
					racF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathFemale, sbLoadFileName.ToString());
					hashUseAssetBundleAnimator.Add(_info.assetpathFemale);
					yield return null;
				}
				else if (_info.nPromiscuity < 1)
				{
					if (!_info.assetBaseF.IsNullOrEmpty() && !racEtcF.ContainsKey(_info.assetBaseF))
					{
						racEtcF.Add(_info.assetBaseF, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseF, _info.assetBaseF));
						hashUseAssetBundleAnimator.Add(_info.assetpathBaseF);
					}
					sbLoadFileName.Clear();
					sbLoadFileName.Append(_info.fileFemale);
					racF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathFemale, sbLoadFileName.ToString());
					hashUseAssetBundleAnimator.Add(_info.assetpathFemale);
				}
				else if (_info.nPromiscuity == 1)
				{
					if (s == 0)
					{
						if (!racEtcF.ContainsKey(_info.assetBaseF))
						{
							racEtcF.Add(_info.assetBaseF, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseF, _info.assetBaseF));
							hashUseAssetBundleAnimator.Add(_info.assetpathBaseF);
						}
						sbLoadFileName.Clear();
						sbLoadFileName.Append(_info.fileFemale);
						racF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathFemale, sbLoadFileName.ToString());
						hashUseAssetBundleAnimator.Add(_info.assetpathFemale);
					}
					else
					{
						if (!racEtcF.ContainsKey(_info.assetBaseF2))
						{
							racEtcF.Add(_info.assetBaseF2, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseF2, _info.assetBaseF2));
							hashUseAssetBundleAnimator.Add(_info.assetpathBaseF2);
						}
						sbLoadFileName.Clear();
						sbLoadFileName.Append(_info.fileFemale2);
						racF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathFemale2, sbLoadFileName.ToString());
						hashUseAssetBundleAnimator.Add(_info.assetpathFemale2);
					}
				}
				else if (_info.nPromiscuity == 2)
				{
					if (s != 0)
					{
						if (!racEtcF.ContainsKey(_info.assetBaseF))
						{
							racEtcF.Add(_info.assetBaseF, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseF, _info.assetBaseF));
							hashUseAssetBundleAnimator.Add(_info.assetpathBaseF);
						}
						sbLoadFileName.Clear();
						sbLoadFileName.Append(_info.fileFemale);
						racF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathFemale, sbLoadFileName.ToString());
						hashUseAssetBundleAnimator.Add(_info.assetpathFemale);
					}
					else
					{
						if (!racEtcF.ContainsKey(_info.assetBaseF2))
						{
							racEtcF.Add(_info.assetBaseF2, CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathBaseF2, _info.assetBaseF2));
							hashUseAssetBundleAnimator.Add(_info.assetpathBaseF2);
						}
						sbLoadFileName.Clear();
						sbLoadFileName.Append(_info.fileFemale2);
						racF[s] = CommonLib.LoadAsset<RuntimeAnimatorController>(_info.assetpathFemale2, sbLoadFileName.ToString());
						hashUseAssetBundleAnimator.Add(_info.assetpathFemale2);
					}
				}
				yield return null;
			}
			num = s + 1;
		}
		lstMotionIK.Clear();
		if (!_info.fileMale.IsNullOrEmpty())
		{
			lstMotionIK.Add((0, 0, new MotionIK(chaMales[0])));
		}
		if (!_info.fileMale2.IsNullOrEmpty())
		{
			lstMotionIK.Add((0, 1, new MotionIK(chaMales[1])));
		}
		if (!_info.fileFemale.IsNullOrEmpty())
		{
			lstMotionIK.Add((1, 0, new MotionIK(chaFemales[0])));
		}
		if (!_info.fileFemale2.IsNullOrEmpty())
		{
			lstMotionIK.Add((1, 1, new MotionIK(chaFemales[1])));
		}
		for (int i = 0; i < lstMotionIK.Count; i++)
		{
			lstMotionIK[i].motionIK.SetPartners(lstMotionIK);
			lstMotionIK[i].motionIK.Reset();
		}
		bool flag5 = ctrlFlag.nowAnimationInfo.id != -1;
		bool flag6 = false;
		for (int j = 0; j < chaMales.Length; j++)
		{
			if ((j == 1 && _info.nPromiscuity != 0) || chaMales[j] == null || chaMales[j].animBody == null || racM[j] == null)
			{
				continue;
			}
			flag6 = true;
			if (_info.ActionCtrl.Item1 < 3)
			{
				bool flag7 = !flag5;
				if (!flag7)
				{
					flag7 = oldMode != _info.ActionCtrl.Item1;
				}
				if (!flag7)
				{
					flag7 = oldModeBase[1, j].Item1 != _info.assetpathBaseM && oldModeBase[1, j].Item2 != _info.assetBaseM;
				}
				if (mode == 0 && flag7)
				{
					if (!_info.assetBaseM.IsNullOrEmpty() && racEtcM.ContainsKey(_info.assetBaseM))
					{
						chaMales[j].animBody.runtimeAnimatorController = racEtcM[_info.assetBaseM];
					}
					else
					{
						chaMales[j].animBody.runtimeAnimatorController = runtimeAnimatorControllers[0, 0].rac;
					}
				}
				else if (mode == 1 && flag7)
				{
					if (!_info.assetBaseM.IsNullOrEmpty() && racEtcM.ContainsKey(_info.assetBaseM))
					{
						chaMales[j].animBody.runtimeAnimatorController = racEtcM[_info.assetBaseM];
					}
					else
					{
						chaMales[j].animBody.runtimeAnimatorController = runtimeAnimatorControllers[0, 1].rac;
					}
				}
				else if (mode == 2 && flag7)
				{
					if (!_info.assetBaseM.IsNullOrEmpty() && racEtcM.ContainsKey(_info.assetBaseM))
					{
						chaMales[j].animBody.runtimeAnimatorController = racEtcM[_info.assetBaseM];
					}
					else
					{
						chaMales[j].animBody.runtimeAnimatorController = runtimeAnimatorControllers[0, 2].rac;
					}
				}
				if (HoushiRacM[j] == null)
				{
					chaMales[j].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(chaMales[j].animBody.runtimeAnimatorController, racM[j]);
				}
				else
				{
					chaMales[j].animBody.runtimeAnimatorController = MixRuntimeControler(chaMales[j].animBody.runtimeAnimatorController, racM[j], HoushiRacM[j]);
				}
			}
			else if (_info.nPromiscuity != 0)
			{
				if (!_info.assetBaseM.IsNullOrEmpty() && racEtcM.ContainsKey(_info.assetBaseM))
				{
					chaMales[j].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(racEtcM[_info.assetBaseM], racM[j]);
				}
				else
				{
					chaMales[j].animBody.runtimeAnimatorController = racM[j];
				}
			}
			else
			{
				string text = ((j == 0) ? _info.assetBaseM : _info.assetBaseM2);
				if (!text.IsNullOrEmpty() && racEtcM.ContainsKey(text))
				{
					chaMales[j].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(racEtcM[text], racM[j]);
				}
				else
				{
					chaMales[j].animBody.runtimeAnimatorController = racM[j];
				}
			}
		}
		bool flag8 = false;
		for (int k = 0; k < chaFemales.Length; k++)
		{
			if ((k == 1 && _info.nPromiscuity < 1) || racF[k] == null)
			{
				continue;
			}
			flag8 = true;
			if (_info.ActionCtrl.Item1 < 3)
			{
				bool flag9 = !flag5;
				if (!flag9)
				{
					flag9 = oldMode != _info.ActionCtrl.Item1;
				}
				if (!flag9)
				{
					flag9 = oldModeBase[1, k].Item1 != _info.assetpathBaseM && oldModeBase[1, k].Item2 != _info.assetBaseM;
				}
				if (mode == 0 && flag9)
				{
					if (!_info.assetBaseF.IsNullOrEmpty() && racEtcF.ContainsKey(_info.assetBaseF))
					{
						chaFemales[k].animBody.runtimeAnimatorController = racEtcF[_info.assetBaseF];
					}
					else
					{
						chaFemales[k].animBody.runtimeAnimatorController = runtimeAnimatorControllers[1, 0].rac;
					}
				}
				else if (mode == 1 && flag9)
				{
					if (!_info.assetBaseF.IsNullOrEmpty() && racEtcF.ContainsKey(_info.assetBaseF))
					{
						chaFemales[k].animBody.runtimeAnimatorController = racEtcF[_info.assetBaseF];
					}
					else
					{
						chaFemales[k].animBody.runtimeAnimatorController = runtimeAnimatorControllers[1, 1].rac;
					}
				}
				else if (mode == 2 && flag9)
				{
					if (!_info.assetBaseF.IsNullOrEmpty() && racEtcF.ContainsKey(_info.assetBaseF))
					{
						chaFemales[k].animBody.runtimeAnimatorController = racEtcF[_info.assetBaseF];
					}
					else
					{
						chaFemales[k].animBody.runtimeAnimatorController = runtimeAnimatorControllers[1, 2].rac;
					}
				}
				if (HoushiRacF[k] == null)
				{
					chaFemales[k].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(chaFemales[k].animBody.runtimeAnimatorController, racF[k]);
				}
				else
				{
					chaFemales[k].animBody.runtimeAnimatorController = MixRuntimeControler(chaFemales[k].animBody.runtimeAnimatorController, racF[k], HoushiRacF[k]);
				}
			}
			else if (_info.nPromiscuity < 1)
			{
				if (!_info.assetBaseF.IsNullOrEmpty() && racEtcF.ContainsKey(_info.assetBaseF))
				{
					chaFemales[k].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(racEtcF[_info.assetBaseF], racF[k]);
				}
				else
				{
					chaFemales[k].animBody.runtimeAnimatorController = racF[k];
				}
			}
			else if (_info.nPromiscuity == 1)
			{
				switch (k)
				{
				case 0:
					chaFemales[k].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(racEtcF[_info.assetBaseF], racF[k]);
					break;
				case 1:
					chaFemales[k].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(racEtcF[_info.assetBaseF2], racF[k]);
					break;
				}
			}
			else if (_info.nPromiscuity == 2)
			{
				switch (k)
				{
				case 0:
					chaFemales[k].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(racEtcF[_info.assetBaseF2], racF[k]);
					break;
				case 1:
					chaFemales[k].animBody.runtimeAnimatorController = Illusion.Utils.Animator.SetupAnimatorOverrideController(racEtcF[_info.assetBaseF], racF[k]);
					break;
				}
			}
		}
		if (!flag6 && !flag8 && _info.id != -1)
		{
			ProcBase.endInit = true;
			nowChangeAnim = false;
			yield break;
		}
		if (_UseFade)
		{
			fade.FadeStart(1.5f);
		}
		if (chaFemales[0] != null)
		{
			if (!_info.fileFemale.IsNullOrEmpty())
			{
				chaFemales[0].visibleAll = true;
			}
			else
			{
				chaFemales[0].visibleAll = false;
			}
		}
		if (chaFemales[1] != null)
		{
			if (!_info.fileFemale2.IsNullOrEmpty())
			{
				chaFemales[1].visibleAll = true;
			}
			else
			{
				chaFemales[1].visibleAll = false;
			}
		}
		if (chaMales[0] != null)
		{
			if (!_info.fileMale.IsNullOrEmpty())
			{
				chaMales[0].visibleAll = true;
			}
			else
			{
				chaMales[0].visibleAll = false;
			}
		}
		if (chaMales[1] != null)
		{
			if (!_info.fileMale2.IsNullOrEmpty())
			{
				chaMales[1].visibleAll = true;
			}
			else
			{
				chaMales[1].visibleAll = false;
			}
		}
		if (hSceneManager.mapID == 6)
		{
			for (int l = 0; l < chaMales.Length; l++)
			{
				if (!(chaMales[l] == null) && !(chaMales[l].objTop == null) && chaMales[l].wetRate < 1f)
				{
					chaMales[l].wetRate = 1f;
				}
			}
		}
		for (int m = 0; m < chaMales.Length && !(chaMales[m] == null) && !(chaMales[m].objTop == null); m++)
		{
			if (chaMales[m].sex == 1 && chaMales[m].fileParam.futanari)
			{
				chaMales[m].visibleSon = true;
			}
			else
			{
				chaMales[m].visibleSon = _info.nMaleSon == 1;
			}
		}
		Vector3 pos = Vector3.zero;
		Vector3 rot = Vector3.zero;
		int num2 = -1;
		int num3 = -1;
		int choicePlace = -1;
		GameObject _Start = null;
		for (int n = 0; n < _info.lstOffset.Count; n++)
		{
			if (ctrlFlag.nPlace == _info.nPositons[n])
			{
				num2 = n;
				break;
			}
		}
		if (num2 >= 0 && !_info.lstOffset[num2].IsNullOrEmpty())
		{
			LoadMoveOffset(_info.lstOffset[num2], out pos, out rot);
		}
		else if (num2 < 0 && hPointCtrl.HPointList != null)
		{
			num3 = hPointCtrl.HPointList.GetStartPoint(_info.nPositons, ref _Start, ref choicePlace, _info.ActionCtrl.Item1, _info.id);
		}
		if (num3 < 0 || choicePlace < -1 || _Start == null)
		{
			if (ctrlFlag.nowHPoint == null)
			{
				SetPosition(Vector3.zero, Quaternion.identity, pos, rot, _FadeStart: false);
			}
			else
			{
				SetPosition(ctrlFlag.nowHPoint.transform.position, ctrlFlag.nowHPoint.transform.rotation, pos, rot, _FadeStart: false);
			}
			if (_info.id >= 0 && num2 >= 0)
			{
			}
		}
		else
		{
			ctrlFlag.nPlace = choicePlace;
			ctrlFlag.HPointID = num3;
			if (ctrlFlag.nowHPoint != null)
			{
				ctrlFlag.nowHPoint.NowUsing = false;
			}
			ctrlFlag.nowHPoint = _Start.GetComponent<HPoint>();
			if (ctrlFlag.nowHPoint != null)
			{
				ctrlFlag.nowHPoint.NowUsing = true;
			}
			for (int num4 = 0; num4 < _info.lstOffset.Count; num4++)
			{
				if (choicePlace == _info.nPositons[num4])
				{
					num2 = num4;
					break;
				}
			}
			if (num2 >= 0 && !_info.lstOffset[num2].IsNullOrEmpty())
			{
				LoadMoveOffset(_info.lstOffset[num2], out pos, out rot);
			}
			SetPosition(_Start.transform.position, _Start.transform.rotation, pos, rot, _FadeStart: false);
		}
		GlobalMethod.setCameraBase(ctrlFlag.cameraCtrl, chaFemalesTrans[0]);
		setCameraLoad(_info, _isForceResetCamera);
		RootmotionOffsetF[0].OffsetInit((!_info.reverseTaii) ? _info.fileFemale : _info.fileFemale2);
		if (chaFemales[1] != null && chaFemales[1].objTop != null)
		{
			RootmotionOffsetF[1].OffsetInit((!_info.reverseTaii) ? _info.fileFemale2 : _info.fileFemale);
		}
		if (chaMales[0] != null && chaMales[0].objTop != null)
		{
			RootmotionOffsetM[0].OffsetInit(_info.fileMale);
		}
		if (chaMales[1] != null && chaMales[1].objTop != null)
		{
			RootmotionOffsetM[1].OffsetInit(_info.fileMale2);
		}
		if (afterIdle)
		{
			ctrlVoice.AfterFinish();
		}
		for (int num5 = 0; num5 < ctrlYures.Length; num5++)
		{
			if (ctrlYures[num5] != null)
			{
				if (chaFemales[num5] != null && chaFemales[num5].objBodyBone != null && ctrlYures[num5].chaFemale == null)
				{
					ctrlYures[num5].SetChaControl(chaFemales[num5]);
					ctrlYures[num5].femaleID = num5;
				}
				if (_info.nPromiscuity < 1 && num5 == 1)
				{
					ctrlYures[num5].Release();
				}
				else
				{
					ctrlYures[num5].Load(_info.id, _info.ActionCtrl.Item1);
				}
			}
		}
		if (chaMales[0] == null || !chaMales[0].visibleAll || chaMales[0].objBody == null)
		{
			ctrlYureMale[0].Release();
		}
		else
		{
			ctrlYureMale[0].Load(_info.id, _info.ActionCtrl.Item1);
		}
		if (chaMales[1] == null || !chaMales[1].visibleAll || chaMales[1].objBody == null)
		{
			ctrlYureMale[1].Release();
		}
		else
		{
			ctrlYureMale[1].Load(_info.id, _info.ActionCtrl.Item1);
		}
		bool flag10 = _info.nPromiscuity == 0;
		for (int num6 = 0; num6 < ctrlLookAts.Length; num6++)
		{
			if (chaMales[num6] == null || chaMales[num6].objBodyBone == null || (!flag10 && num6 > 0))
			{
				ctrlLookAts[num6].Release();
				continue;
			}
			sbLoadFileName.Clear();
			sbLoadFileName.Append(_info.fileMale);
			ctrlLookAts[num6].LoadList(sbLoadFileName.ToString(), _info.ActionCtrl.Item1, num6);
		}
		for (int num7 = 0; num7 < chaMales.Length; num7++)
		{
			if (chaMales[num7] == null || chaMales[num7].animBody == null || !chaMales[num7].visibleAll)
			{
				continue;
			}
			if ((bool)chaMales[num7].objHitBody)
			{
				sbLoadFileName.Clear();
				switch (num7)
				{
				case 0:
					sbLoadFileName.Append(_info.fileMale);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileMale2);
					break;
				}
				ctrlMaleCollisionCtrls[num7].LoadExcel(sbLoadFileName.ToString());
			}
			else
			{
				ctrlMaleCollisionCtrls[num7].Release();
			}
			if (mode != -1)
			{
				for (int num8 = 0; num8 < lstMotionIK.Count; num8++)
				{
					if (lstMotionIK[num8].sex != 0 || lstMotionIK[num8].num != num7)
					{
						continue;
					}
					sbLoadFileName.Clear();
					if (_info.nPromiscuity != 0)
					{
						sbLoadFileName.Append(_info.fileMale);
					}
					else
					{
						switch (num7)
						{
						case 0:
							sbLoadFileName.Append(_info.fileMale);
							break;
						case 1:
							sbLoadFileName.Append(_info.fileMale2);
							break;
						}
					}
					int num9 = -1;
					for (int num10 = 0; num10 < abName.Count; num10++)
					{
						if (GameSystem.IsPathAdd50(abName[num10]))
						{
							if (!GlobalMethod.AssetFileExist(abName[num10], sbLoadFileName.ToString()))
							{
								lstMotionIK[num8].motionIK.Release();
							}
							else
							{
								num9 = num10;
							}
						}
					}
					if (num9 >= 0)
					{
						TextAsset ta = CommonLib.LoadAsset<TextAsset>(abName[num9], sbLoadFileName.ToString());
						AssetBundleManager.UnloadAssetBundle(abName[num9], isUnloadForceRefCount: true);
						lstMotionIK[num8].motionIK.LoadData(ta);
					}
					if (mode != 1 || _info.nDownPtn != 1 || tmpAddInfo == null)
					{
						continue;
					}
					sbLoadFileName.Clear();
					sbLoadFileName.Append(tmpAddInfo.fileMale);
					num9 = -1;
					for (int num11 = 0; num11 < abName.Count; num11++)
					{
						if (GameSystem.IsPathAdd50(abName[num11]) && GlobalMethod.AssetFileExist(abName[num11], sbLoadFileName.ToString()))
						{
							num9 = num11;
						}
					}
					if (num9 >= 0)
					{
						TextAsset ta2 = CommonLib.LoadAsset<TextAsset>(abName[num9], sbLoadFileName.ToString());
						AssetBundleManager.UnloadAssetBundle(abName[num9], isUnloadForceRefCount: true);
						lstMotionIK[num8].motionIK.LoadData(ta2);
					}
				}
			}
			sbLoadFileName.Clear();
			if (num7 == 0)
			{
				sbLoadFileName.Append(_info.fileMale);
			}
			else
			{
				sbLoadFileName.Append(_info.fileMale2);
			}
			ctrlLayer.LoadExcel(sbLoadFileName.ToString(), 0, num7);
			if (chaMales[num7] != null && mode >= 0)
			{
				CtrlParticle.ParticleLoad(chaMales[num7].objBodyBone, num7 * 2);
			}
		}
		for (int num12 = 0; num12 < chaFemales.Length; num12++)
		{
			if (chaFemales[num12] == null || chaFemales[num12].animBody == null || !chaFemales[num12].visibleAll)
			{
				continue;
			}
			if (_info.id != -1 && mode != -1)
			{
				for (int num13 = 0; num13 < lstMotionIK.Count; num13++)
				{
					if (lstMotionIK[num13].sex != 1 || lstMotionIK[num13].num != num12)
					{
						continue;
					}
					sbLoadFileName.Clear();
					if (_info.nPromiscuity < 1)
					{
						sbLoadFileName.Append(_info.fileFemale);
					}
					else if (_info.nPromiscuity == 1)
					{
						switch (num12)
						{
						case 0:
							sbLoadFileName.Append(_info.fileFemale);
							break;
						case 1:
							sbLoadFileName.Append(_info.fileFemale2);
							break;
						}
					}
					else if (_info.nPromiscuity == 2)
					{
						switch (num12)
						{
						case 0:
							sbLoadFileName.Append(_info.fileFemale2);
							break;
						case 1:
							sbLoadFileName.Append(_info.fileFemale);
							break;
						}
					}
					int num14 = -1;
					for (int num15 = 0; num15 < abName.Count; num15++)
					{
						if (GameSystem.IsPathAdd50(abName[num15]))
						{
							if (!GlobalMethod.AssetFileExist(abName[num15], sbLoadFileName.ToString()))
							{
								lstMotionIK[num13].motionIK.Release();
							}
							else
							{
								num14 = num15;
							}
						}
					}
					if (num14 >= 0)
					{
						TextAsset ta3 = CommonLib.LoadAsset<TextAsset>(abName[num14], sbLoadFileName.ToString());
						AssetBundleManager.UnloadAssetBundle(abName[num14], isUnloadForceRefCount: true);
						lstMotionIK[num13].motionIK.LoadData(ta3);
					}
					if (mode != 1 || _info.nDownPtn != 1 || tmpAddInfo == null)
					{
						continue;
					}
					sbLoadFileName.Clear();
					sbLoadFileName.Append(tmpAddInfo.fileFemale);
					num14 = -1;
					for (int num16 = 0; num16 < abName.Count; num16++)
					{
						if (GameSystem.IsPathAdd50(abName[num16]) && GlobalMethod.AssetFileExist(abName[num16], sbLoadFileName.ToString()))
						{
							num14 = num16;
						}
					}
					if (num14 >= 0)
					{
						TextAsset ta4 = CommonLib.LoadAsset<TextAsset>(abName[num14], sbLoadFileName.ToString());
						AssetBundleManager.UnloadAssetBundle(abName[num14], isUnloadForceRefCount: true);
						lstMotionIK[num13].motionIK.LoadData(ta4);
					}
				}
			}
			sbLoadFileName.Clear();
			if (num12 == 0)
			{
				sbLoadFileName.Append(_info.fileFemale);
			}
			else
			{
				sbLoadFileName.Append(_info.fileFemale2);
			}
			ctrlLayer.LoadExcel(sbLoadFileName.ToString(), 1, num12);
			if ((bool)chaFemales[num12].objHitBody)
			{
				sbLoadFileName.Clear();
				if (_info.nPromiscuity < 1)
				{
					sbLoadFileName.Append(_info.fileFemale);
				}
				else if (_info.nPromiscuity == 1)
				{
					switch (num12)
					{
					case 0:
						sbLoadFileName.Append(_info.fileFemale);
						break;
					case 1:
						sbLoadFileName.Append(_info.fileFemale2);
						break;
					}
				}
				else if (_info.nPromiscuity == 2)
				{
					switch (num12)
					{
					case 0:
						sbLoadFileName.Append(_info.fileFemale2);
						break;
					case 1:
						sbLoadFileName.Append(_info.fileFemale);
						break;
					}
				}
				ctrlFemaleCollisionCtrls[num12].LoadExcel(sbLoadFileName.ToString());
			}
			else
			{
				ctrlFemaleCollisionCtrls[num12].Release();
			}
			if (chaFemales[num12] != null && mode >= 0)
			{
				if (num12 == 1)
				{
					CtrlParticle.ParticleLoad(chaFemales[num12].objBodyBone, 3);
					ctrlDynamics[num12].Init(chaFemales[num12]);
				}
				else
				{
					CtrlParticle.ParticleLoad(chaFemales[num12].objBodyBone, 1);
				}
			}
		}
		SetClothStateStartMotion(0, _info);
		if (_info.nPromiscuity >= 1)
		{
			SetClothStateStartMotion(1, _info);
		}
		if (_info.isNeedItem)
		{
			ctrlItem.ReleaseItem();
			yield return null;
			ctrlItem.LoadItem(_info.ActionCtrl.Item1, _info.id, (chaMales[0] != null) ? chaMales[0].objBodyBone : null, chaFemales[0].objBodyBone, (chaMales[1] != null) ? chaMales[1].objBodyBone : null, (chaFemales[1] != null) ? chaFemales[1].objBodyBone : null);
		}
		else
		{
			ctrlItem.ReleaseItem();
		}
		List<HItemCtrl.Item> items = ctrlItem.GetItems();
		int count = items.Count;
		GameObject[] array = new GameObject[count];
		for (int num17 = 0; num17 < count; num17++)
		{
			array[num17] = items[num17].objItem;
		}
		for (int num18 = 0; num18 < lstMotionIK.Count; num18++)
		{
			lstMotionIK[num18].motionIK.items.Clear();
			lstMotionIK[num18].motionIK.SetItems(array);
			lstMotionIK[num18].motionIK.Reset();
		}
		if (mode != -1)
		{
			lstProc[mode].Init(modeCtrl);
			lstProc[mode].SetStartMotion(isIdle, modeCtrl, _info);
			ctrlVoice.HouchiTime = 0f;
		}
		if (sprite != null)
		{
			if (mode == 2)
			{
				sprite.SetFinishSelect(mode, modeCtrl, _info.ActionCtrl.Item1, _info.ActionCtrl.Item2);
			}
			else
			{
				sprite.SetFinishSelect(mode, modeCtrl);
			}
		}
		chaChoice.ChangeChaOptions(_info, sprite.ClothMode);
		if (_info.id == -1)
		{
			ctrlFlag.nowAnimationInfo = _info;
			ProcBase.endInit = true;
			nowChangeAnim = false;
			yield break;
		}
		CtrlParticle.ParticleLoad(ctrlItem.GetItem(), 4);
		for (int num19 = 0; num19 < ctrlHitObjectFemales.Length; num19++)
		{
			if (ctrlHitObjectFemales[num19] == null)
			{
				continue;
			}
			if (_info.nPromiscuity < 1 && num19 == 1)
			{
				ctrlHitObjectFemales[num19].HitObjLoadExcel("");
				continue;
			}
			sbLoadFileName.Clear();
			if (_info.nPromiscuity < 1)
			{
				sbLoadFileName.Append(_info.fileFemale);
			}
			else if (_info.nPromiscuity == 1)
			{
				switch (num19)
				{
				case 0:
					sbLoadFileName.Append(_info.fileFemale);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileFemale2);
					break;
				}
			}
			else if (_info.nPromiscuity == 2)
			{
				switch (num19)
				{
				case 0:
					sbLoadFileName.Append(_info.fileFemale2);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileFemale);
					break;
				}
			}
			ctrlHitObjectFemales[num19].HitObjLoadExcel(sbLoadFileName.ToString());
		}
		for (int num20 = 0; num20 < ctrlHitObjectMales.Length; num20++)
		{
			if (ctrlHitObjectMales[num20] == null)
			{
				continue;
			}
			if (_info.nPromiscuity != 0 && num20 == 1)
			{
				ctrlHitObjectMales[num20].HitObjLoadExcel("");
				continue;
			}
			sbLoadFileName.Clear();
			switch (num20)
			{
			case 0:
				sbLoadFileName.Append(_info.fileMale);
				break;
			case 1:
				sbLoadFileName.Append(_info.fileMale2);
				break;
			}
			ctrlHitObjectMales[num20].HitObjLoadExcel(sbLoadFileName.ToString());
		}
		ctrlObi.SetChara(chaMales, chaFemales, _info.nPromiscuity >= 0);
		ctrlObi.InitSetting(_info.fileFemale, ctrlItem.GetItem());
		ctrlObi.ChangeSetupInfo(ctrlFlag.isFaintness ? 1 : 0);
		for (int num21 = 0; num21 < ctrlDynamics.Length; num21++)
		{
			if (ctrlDynamics[num21] == null)
			{
				continue;
			}
			if (_info.nPromiscuity < 1 && num21 == 1)
			{
				ctrlDynamics[num21].Load(hSceneManager.strAssetDynamicBoneListFolder, "");
				continue;
			}
			sbLoadFileName.Clear();
			if (_info.nPromiscuity < 1)
			{
				sbLoadFileName.Append(_info.fileFemale);
			}
			else if (_info.nPromiscuity == 1)
			{
				switch (num21)
				{
				case 0:
					sbLoadFileName.Append(_info.fileFemale);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileFemale2);
					break;
				}
			}
			else if (_info.nPromiscuity == 2)
			{
				switch (num21)
				{
				case 0:
					sbLoadFileName.Append(_info.fileFemale2);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileFemale);
					break;
				}
			}
			ctrlDynamics[num21].Load(hSceneManager.strAssetDynamicBoneListFolder, sbLoadFileName.ToString());
		}
		GameObject[] objBoneFemale = new GameObject[2]
		{
			chaFemales[0].objBodyBone,
			(chaFemales[1] != null) ? chaFemales[1].objBodyBone : null
		};
		GameObject[] objBoneMale = new GameObject[2]
		{
			(chaMales[0] != null) ? chaMales[0].objBodyBone : null,
			(chaMales[1] != null) ? chaMales[1].objBodyBone : null
		};
		ctrlSE.InitOldMember();
		ctrlSE.Load(hSceneManager.strAssetSEListFolder, _info.fileSe, objBoneMale, objBoneFemale);
		for (int num22 = 0; num22 < ctrlEyeNeckFemale.Length; num22++)
		{
			if (ctrlEyeNeckFemale[num22] == null)
			{
				continue;
			}
			if (_info.nPromiscuity > 0 && ctrlEyeNeckFemale[1].CharID != 1)
			{
				ctrlEyeNeckFemale[1].Init(chaFemales[1], 1, this);
			}
			if (_info.nPromiscuity < 1 && num22 == 1)
			{
				if (NumCharaChange)
				{
					ctrlEyeNeckFemale[num22].SetPartner(null, null, null);
					ctrlEyeNeckFemale[num22].Release();
				}
				continue;
			}
			if (NumCharaChange)
			{
				ctrlEyeNeckFemale[num22].SetPartner((chaMales[0] != null) ? chaMales[0].objBodyBone : null, chaMales[1] ? chaMales[1].objBodyBone : null, chaFemales[num22 ^ 1] ? chaFemales[num22 ^ 1].objBodyBone : null);
			}
			if (chaFemales[num22] == null || chaFemales[num22].objBody == null || !chaFemales[num22].visibleAll)
			{
				continue;
			}
			sbLoadFileName.Clear();
			if (_info.nPromiscuity < 1)
			{
				sbLoadFileName.Append(_info.fileMotionNeckFemale);
			}
			else if (_info.nPromiscuity == 1)
			{
				switch (num22)
				{
				case 0:
					sbLoadFileName.Append(_info.fileMotionNeckFemale);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileMotionNeckFemale2);
					break;
				}
			}
			else if (_info.nPromiscuity == 2)
			{
				switch (num22)
				{
				case 0:
					sbLoadFileName.Append(_info.fileMotionNeckFemale2);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileMotionNeckFemale);
					break;
				}
			}
			ctrlEyeNeckFemale[num22].Load(hSceneManager.strAssetNeckCtrlListFolder, sbLoadFileName.ToString());
		}
		for (int num23 = 0; num23 < ctrlEyeNeckMale.Length; num23++)
		{
			if (ctrlEyeNeckMale[num23] == null)
			{
				continue;
			}
			if (_info.nPromiscuity != 0 && num23 == 1)
			{
				if (NumCharaChange)
				{
					ctrlEyeNeckMale[num23].SetPartner(null, null, null);
					ctrlEyeNeckMale[num23].Release();
				}
				continue;
			}
			if (NumCharaChange)
			{
				ctrlEyeNeckMale[num23].Init(chaMales[num23], num23);
				ctrlEyeNeckMale[num23].SetPartner(chaFemales[0].objBodyBone, chaFemales[1] ? chaFemales[1].objBodyBone : null, chaMales[num23 ^ 1] ? chaMales[num23 ^ 1].objBodyBone : null);
			}
			sbLoadFileName.Clear();
			if (num23 == 0)
			{
				sbLoadFileName.Append(_info.fileMotionNeckMale);
			}
			else
			{
				sbLoadFileName.Append(_info.fileMotionNeckMale2);
			}
			ctrlEyeNeckMale[num23].Load(hSceneManager.strAssetNeckCtrlListFolder, sbLoadFileName.ToString());
		}
		yield return null;
		for (int num24 = 0; num24 < ctrlSiruPastes.Length; num24++)
		{
			if (ctrlSiruPastes[num24] == null)
			{
				continue;
			}
			if (_info.nPromiscuity < 1 && num24 == 1)
			{
				ctrlSiruPastes[num24].Release();
				continue;
			}
			sbLoadFileName.Clear();
			if (_info.ActionCtrl.Item1 != 5)
			{
				sbLoadFileName.Append(_info.fileSiruPaste);
			}
			else
			{
				switch (num24)
				{
				case 0:
					sbLoadFileName.Append(_info.fileSiruPaste);
					break;
				case 1:
					sbLoadFileName.Append(_info.fileSiruPasteSecond);
					break;
				}
			}
			if (!ctrlSiruPastes[num24].isInit)
			{
				ctrlSiruPastes[num24].Init(chaFemales[num24]);
			}
			ctrlSiruPastes[num24].Load(hSceneManager.strAssetSiruPasteListFolder, sbLoadFileName.ToString(), _info.id);
		}
		ctrlFlag.isInsert = false;
		ctrlFlag.nowAnimationInfo = _info;
		ctrlFlag.isGaugeHit = false;
		ctrlFlag.isGaugeHit_M = false;
		ctrlVoice.SetVoiceList(_info.ActionCtrl.Item1, _info.id, _info.lstSystem);
		ctrlVoice.SetBreathVoiceList(chaFemales, _info.ActionCtrl.Item1, _info.ActionCtrl.Item2, _info.lstSystem, _info.reverseTaii);
		for (int num25 = 0; num25 < chaFemales.Length && !(chaFemales[num25] == null) && chaFemales[num25].visibleAll && !(chaFemales[num25].objTop == null); num25++)
		{
			chaFemales[num25].reSetupDynamicBoneBust = true;
			chaFemales[num25].resetDynamicBoneAll = true;
		}
		for (int num26 = 0; num26 < chaMales.Length && !(chaMales[num26] == null) && chaMales[num26].visibleAll && !(chaMales[num26].objTop == null); num26++)
		{
			chaMales[num26].reSetupDynamicBoneBust = true;
			chaMales[num26].resetDynamicBoneAll = true;
		}
		for (int num27 = 0; num27 < chaFemales.Length && (_info.nPromiscuity >= 1 || num27 != 1); num27++)
		{
			chaFemales[num27].eyesCtrl?.SetOpenRateForce(1f);
		}
		ctrlFeelHit.InitTime();
		if (ctrlVoice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || ctrlVoice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice)
		{
			for (int num28 = 0; num28 < chaFemales.Length && (_info.nPromiscuity >= 1 || num28 != 1); num28++)
			{
				Voice.Stop(ctrlFlag.voice.voiceTrs[num28]);
			}
		}
		if (chaFemales[1] != null && (!chaFemales[1].visibleAll || chaFemales[1].objBody == null))
		{
			Voice.Stop(ctrlFlag.voice.voiceTrs[1]);
		}
		for (int num29 = 0; num29 < _info.lstSystem.Count; num29++)
		{
			ctrlFlag.isJudgeSelect.Add((HSceneFlagCtrl.JudgeSelect)(1 << _info.lstSystem[num29]));
		}
		ctrlFlag.voice.dialog = false;
		foreach (string item in hashUseAssetBundleAnimator)
		{
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
		GC.Collect();
		UnityEngine.Resources.UnloadUnusedAssets();
		yield return null;
		if (sprite.OpenClothKind() == 2 && sprite.charaChoice != null)
		{
			sprite.charaChoice.SetMale(val: true);
		}
	}

	private bool CheckSpeek()
	{
		bool[] array = new bool[2]
		{
			(ctrlVoice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || ctrlVoice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice) && Voice.IsPlay(ctrlFlag.voice.voiceTrs[0]),
			false
		};
		if (chaFemales[1] != null && chaFemales[1].visibleAll && chaFemales[1].objBody != null)
		{
			array[1] = (ctrlVoice.nowVoices[1].state == HVoiceCtrl.VoiceKind.voice || ctrlVoice.nowVoices[1].state == HVoiceCtrl.VoiceKind.startVoice) && Voice.IsPlay(ctrlFlag.voice.voiceTrs[1]);
		}
		return array[0] | array[1];
	}

	private (string, string)[,] SetOldAnimatorInfo(int oldMode)
	{
		(string, string)[,] array = new(string, string)[2, 2];
		AnimationListInfo nowAnimationInfo = ctrlFlag.nowAnimationInfo;
		if (nowAnimationInfo.assetpathBaseF.IsNullOrEmpty())
		{
			if (oldMode != -1 && oldMode < 3)
			{
				array[0, 0] = (runtimeAnimatorControllers[1, oldMode].path, runtimeAnimatorControllers[1, oldMode].name);
			}
			else if (oldMode == 3)
			{
				array[0, 0] = ("前回特殊", "前回特殊");
			}
			else
			{
				array[0, 0] = ("前回ベースなし", "前回ベースなし");
			}
		}
		else
		{
			array[0, 0] = (nowAnimationInfo.assetpathBaseF, nowAnimationInfo.assetBaseF);
		}
		if (nowAnimationInfo.assetpathBaseF2.IsNullOrEmpty())
		{
			switch (oldMode)
			{
			default:
				array[0, 1] = ("前回女複数いない", "前回女複数いない");
				break;
			case -1:
			case 4:
			case 5:
				array[0, 1] = ("前回ベースなし", "前回ベースなし");
				break;
			}
		}
		else
		{
			array[0, 1] = (nowAnimationInfo.assetpathBaseF2, nowAnimationInfo.assetBaseF2);
		}
		if (nowAnimationInfo.assetpathBaseM.IsNullOrEmpty())
		{
			if (oldMode != -1 && oldMode < 3)
			{
				array[1, 0] = (runtimeAnimatorControllers[0, oldMode].path, runtimeAnimatorControllers[0, oldMode].name);
			}
			else if (oldMode == 3)
			{
				array[1, 0] = ("前回特殊", "前回特殊");
			}
			else
			{
				array[1, 0] = ("前回ベースなし", "前回ベースなし");
			}
		}
		else
		{
			array[1, 0] = (nowAnimationInfo.assetpathBaseM, nowAnimationInfo.assetBaseM);
		}
		if (nowAnimationInfo.assetpathBaseM2.IsNullOrEmpty())
		{
			if (oldMode != -1 && oldMode != 6)
			{
				array[1, 1] = ("前回男複数いない", "前回男複数いない");
			}
			else
			{
				array[1, 1] = ("前回ベースなし", "前回ベースなし");
			}
		}
		else
		{
			array[1, 1] = (nowAnimationInfo.assetpathBaseM2, nowAnimationInfo.assetBaseM2);
		}
		return array;
	}

	private void ChangeModeCtrl(AnimationListInfo _info)
	{
		if (_info.ActionCtrl.Item1 < 3)
		{
			mode = _info.ActionCtrl.Item1;
		}
		else if (_info.ActionCtrl.Item1 == 3)
		{
			switch (_info.ActionCtrl.Item2)
			{
			case 0:
				mode = 2;
				break;
			case 1:
				mode = 2;
				break;
			case 2:
				mode = 3;
				break;
			case 3:
				mode = 0;
				break;
			case 4:
				mode = 4;
				break;
			case 5:
				mode = 4;
				break;
			case 6:
				mode = 5;
				break;
			case 7:
				mode = 2;
				break;
			}
		}
		else
		{
			mode = _info.ActionCtrl.Item1 + 2;
		}
		modeCtrl = _info.ActionCtrl.Item2;
	}

	private void ChangeAnimVoiceFlag()
	{
		bool flag = IsIdle(chaFemales[0].animBody);
		bool flag2 = false;
		bool flag3 = false;
		if (!flag)
		{
			flag2 = IsAfterIdle(chaFemales[0].animBody);
			if (!flag2)
			{
				flag3 = IsOloop(chaFemales[0].animBody);
			}
		}
		ctrlFlag.voice.oldAnimType.Clear();
		if (flag3)
		{
			if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 1)
			{
				ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.OloopH);
			}
			else if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 4 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 5 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 6)
			{
				ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.OrgasmF);
			}
		}
		if (flag2)
		{
			if (ctrlFlag.voice.oldFinish == 1 || ctrlFlag.voice.oldFinish == 2 || ctrlFlag.voice.oldFinish == 3)
			{
				ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.MaleShot);
				if (ctrlFlag.voice.oldFinish == 1)
				{
					if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 1)
					{
						ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.InsideMouth);
					}
					else if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 4 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 5 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 6)
					{
						ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.Inside);
					}
				}
				else if (ctrlFlag.voice.oldFinish == 3)
				{
					if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 1)
					{
						ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.InsideMouth);
					}
					else if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 4 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 5 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 6)
					{
						ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.Inside);
					}
				}
			}
			if (ctrlFlag.voice.urineFlag)
			{
				ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.Urine);
				ctrlFlag.voice.urineFlag = false;
			}
		}
		if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 0)
		{
			if (!flag)
			{
				if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(0))
				{
					if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(3))
					{
						ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.MataIjiri);
					}
					else
					{
						ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.AnalIjiri);
					}
				}
				if (ctrlFlag.nowAnimationInfo.lstSystem.Contains(0))
				{
					ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.Kiss);
				}
			}
		}
		else if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 2 && ctrlFlag.nowAnimationInfo.lstSystem.Contains(3) && !flag)
		{
			ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.Anal);
		}
		if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 1 || ctrlFlag.nowAnimationInfo.nInitiativeFemale != 0)
		{
			ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.FemaleMain);
			if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 2)
			{
				ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.FemaleMainSonyu);
			}
		}
		else if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 1 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 < 4 && ctrlFlag.nowAnimationInfo.nInitiativeFemale == 0)
		{
			ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.MaleMain);
			if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 2)
			{
				ctrlFlag.voice.oldAnimType.Add(HSceneFlagCtrl.AnimType.MaleMainSonyu);
			}
		}
		ctrlFlag.voice.newAnimType.Clear();
		if (ctrlFlag.selectAnimationListInfo.ActionCtrl.Item1 == 0)
		{
			if (ctrlFlag.selectAnimationListInfo.hasVoiceCategory.Contains(1))
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.Kunni);
			}
			if (ctrlFlag.selectAnimationListInfo.lstSystem.Contains(0))
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.Kiss);
			}
		}
		else if (ctrlFlag.selectAnimationListInfo.ActionCtrl.Item1 == 1)
		{
			if (ctrlFlag.selectAnimationListInfo.hasVoiceCategory.Contains(2) || ctrlFlag.selectAnimationListInfo.hasVoiceCategory.Contains(3))
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.NameKuwae);
			}
			ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.FemaleMain);
		}
		else if (ctrlFlag.selectAnimationListInfo.ActionCtrl.Item1 == 2 && ctrlFlag.selectAnimationListInfo.ActionCtrl.Item2 == 0)
		{
			if (ctrlFlag.selectAnimationListInfo.lstSystem.Contains(1))
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.KokanSonyu);
			}
			else if (ctrlFlag.selectAnimationListInfo.lstSystem.Contains(3))
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.Anal);
			}
			ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.Sonyu);
			if (ctrlFlag.selectAnimationListInfo.nInitiativeFemale == 0)
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.MaleMainSonyu);
			}
			else
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.FemaleMainSonyu);
			}
		}
		if (ctrlFlag.selectAnimationListInfo.nInitiativeFemale != 0)
		{
			ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.FemaleMain);
		}
		else if (ctrlFlag.selectAnimationListInfo.ActionCtrl.Item1 != 1 && ctrlFlag.selectAnimationListInfo.ActionCtrl.Item1 < 4 && ctrlFlag.selectAnimationListInfo.nInitiativeFemale == 0)
		{
			int item = ctrlFlag.selectAnimationListInfo.ActionCtrl.Item1;
			int item2 = ctrlFlag.selectAnimationListInfo.ActionCtrl.Item2;
			if (item != 3)
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.MaleMain);
			}
			else if (item2 != 4 && item2 != 5 && item2 != 6)
			{
				ctrlFlag.voice.newAnimType.Add(HSceneFlagCtrl.AnimType.MaleMain);
			}
		}
	}

	private bool PositionShift()
	{
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.ParallelShiftInit)
		{
			Vector3 offsetpos = chaFemalesTrans[0].position - noShiftPos;
			offsetpos.x = 0f;
			offsetpos.z = 0f;
			SetPosition(noShiftPos, chaFemalesTrans[0].rotation, offsetpos, Vector3.zero, _FadeStart: false);
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.VerticalShiftInit)
		{
			Vector3 offsetpos2 = chaFemalesTrans[0].position - noShiftPos;
			offsetpos2.y = 0f;
			SetPosition(noShiftPos, chaFemalesTrans[0].rotation, offsetpos2, Vector3.zero, _FadeStart: false);
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.RotationShiftInit)
		{
			SetPosition(chaFemalesTrans[0].position, noShiftRot, Vector3.zero, Vector3.zero, _FadeStart: false);
		}
		if (ctrlFlag.kindCharaCtrl == HSceneFlagCtrl.CharaCtrlKind.None)
		{
			return false;
		}
		if (ctrlFlag.kindCharaCtrl == HSceneFlagCtrl.CharaCtrlKind.Parallel)
		{
			Vector3 position = chaFemalesTrans[0].position;
			Vector3 vector = ctrlFlag.cameraCtrl.gameObject.transform.TransformDirection(new Vector3(Input.GetAxis("Mouse X"), 0f, Input.GetAxis("Mouse Y")));
			vector.y = 0f;
			position += vector.normalized * ctrlFlag.charaMoveSpeed * ctrlFlag.charaMoveSpeedAddRate;
			SetPosition(position, chaFemalesTrans[0].rotation, Vector3.zero, Vector3.zero, _FadeStart: false);
			PositionShiftMouseUp();
		}
		else if (ctrlFlag.kindCharaCtrl == HSceneFlagCtrl.CharaCtrlKind.Height)
		{
			Vector3 position2 = chaFemalesTrans[0].position;
			position2 += new Vector3(0f, Input.GetAxis("Mouse Y"), 0f).normalized * ctrlFlag.charaMoveSpeed * ctrlFlag.charaMoveSpeedAddRate;
			SetPosition(position2, chaFemalesTrans[0].rotation, Vector3.zero, Vector3.zero, _FadeStart: false);
			PositionShiftMouseUp();
		}
		else if (ctrlFlag.kindCharaCtrl == HSceneFlagCtrl.CharaCtrlKind.Rotation)
		{
			Vector3 zero = Vector3.zero;
			zero.y = (0f - Input.GetAxis("Mouse X")) * ctrlFlag.charaRotaionSpeed * ctrlFlag.charaMoveSpeedAddRate;
			SetPosition(chaFemalesTrans[0].position, chaFemalesTrans[0].rotation, Vector3.zero, zero, _FadeStart: false);
			PositionShiftMouseUp();
		}
		return true;
	}

	private void PositionShiftMouseUp()
	{
		if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
		{
			ctrlFlag.kindCharaCtrl = HSceneFlagCtrl.CharaCtrlKind.None;
			if (Singleton<GameCursor>.IsInstance())
			{
				Singleton<GameCursor>.Instance.SetCursorLock(setLockFlag: false);
			}
			ctrlFlag.cameraCtrl.isCursorLock = true;
		}
	}

	private bool IsIdle(Animator _anim)
	{
		if (_anim.runtimeAnimatorController == null)
		{
			return true;
		}
		AnimatorStateInfo currentAnimatorStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("Idle") || currentAnimatorStateInfo.IsName("WIdle") || currentAnimatorStateInfo.IsName("SIdle") || currentAnimatorStateInfo.IsName("Insert") || currentAnimatorStateInfo.IsName("D_Idle") || currentAnimatorStateInfo.IsName("D_Insert"))
		{
			return true;
		}
		return false;
	}

	private bool IsAfterIdle(Animator _anim)
	{
		if (_anim.runtimeAnimatorController == null)
		{
			return true;
		}
		AnimatorStateInfo currentAnimatorStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("Orgasm_A") || currentAnimatorStateInfo.IsName("Orgasm_IN_A") || currentAnimatorStateInfo.IsName("Orgasm_OUT_A") || currentAnimatorStateInfo.IsName("Drink_A") || currentAnimatorStateInfo.IsName("Vomit_A") || currentAnimatorStateInfo.IsName("OrgasmM_OUT_A") || currentAnimatorStateInfo.IsName("D_Orgasm_A") || currentAnimatorStateInfo.IsName("D_Orgasm_OUT_A") || currentAnimatorStateInfo.IsName("D_Orgasm_IN_A") || currentAnimatorStateInfo.IsName("D_OrgasmM_OUT_A"))
		{
			return true;
		}
		return false;
	}

	private bool IsOloop(Animator _anim)
	{
		if (_anim.runtimeAnimatorController == null)
		{
			return false;
		}
		AnimatorStateInfo currentAnimatorStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("OLoop") || currentAnimatorStateInfo.IsName("D_OLoop"))
		{
			return true;
		}
		return false;
	}

	public bool setCameraLoad(AnimationListInfo _info, bool _isForceResetCamera)
	{
		if (_isForceResetCamera || Manager.Config.HData.InitCamera)
		{
			GlobalMethod.loadCamera(ctrlFlag.cameraCtrl, hSceneManager.strAssetCameraList, _info.nameCamera);
		}
		else
		{
			GlobalMethod.loadResetCamera(ctrlFlag.cameraCtrl, hSceneManager.strAssetCameraList, _info.nameCamera);
		}
		return true;
	}

	private bool SetPosition(Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart = true)
	{
		if (_trans == null)
		{
			return false;
		}
		for (int i = 0; i < chaMales.Length; i++)
		{
			if (!(chaMales[i] == null) && !(chaMales[i].objBody == null))
			{
				chaMalesTrans[i].position = _trans.position;
				chaMalesTrans[i].rotation = _trans.rotation;
				chaMalesTrans[i].localPosition += _trans.rotation * offsetpos;
				chaMalesTrans[i].localRotation = Quaternion.Euler(offsetrot) * chaMalesTrans[i].localRotation;
				chaMales[i].animBody.transform.localPosition = Vector3.zero;
				chaMales[i].animBody.transform.localRotation = Quaternion.identity;
				chaMales[i].SetPosition(Vector3.zero);
				chaMales[i].SetRotation(Vector3.zero);
				chaMales[i].resetDynamicBoneAll = true;
			}
		}
		if (hSceneManager.player != null && chaMales[0] != null && chaMales[0].objBody != null)
		{
			hSceneManager.player.transform.position = chaMalesTrans[0].position;
		}
		for (int j = 0; j < chaFemales.Length; j++)
		{
			if (!(chaFemales[j] == null) && !(chaFemales[j].objBody == null))
			{
				chaFemalesTrans[j].position = _trans.position;
				chaFemalesTrans[j].rotation = _trans.rotation;
				chaFemalesTrans[j].localPosition += _trans.rotation * offsetpos;
				chaFemalesTrans[j].localRotation = Quaternion.Euler(offsetrot) * chaFemalesTrans[j].localRotation;
				chaFemales[j].animBody.transform.localPosition = Vector3.zero;
				chaFemales[j].animBody.transform.localRotation = Quaternion.identity;
				chaFemales[j].SetPosition(Vector3.zero);
				chaFemales[j].SetRotation(Vector3.zero);
				chaFemales[j].ResetDynamicBoneAll();
				chaFemales[j].resetDynamicBoneAll = true;
				if (hSceneManager.females != null && hSceneManager.females[j] != null)
				{
					hSceneManager.females[j].transform.position = chaFemalesTrans[j].position;
				}
			}
		}
		if (_FadeStart)
		{
			fade.FadeStart(2f);
		}
		ctrlItem.setTransform(_trans);
		return true;
	}

	private bool SetPosition(Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart = true)
	{
		for (int i = 0; i < chaMales.Length; i++)
		{
			if (!(chaMales[i] == null) && !(chaMales[i].objBody == null))
			{
				chaMalesTrans[i].position = pos;
				chaMalesTrans[i].rotation = rot;
				chaMalesTrans[i].localPosition += rot * offsetpos;
				chaMalesTrans[i].localRotation = Quaternion.Euler(offsetrot) * chaMalesTrans[i].localRotation;
				chaMales[i].animBody.transform.localPosition = Vector3.zero;
				chaMales[i].animBody.transform.localRotation = Quaternion.identity;
				chaMales[i].SetPosition(Vector3.zero);
				chaMales[i].SetRotation(Vector3.zero);
				chaMales[i].resetDynamicBoneAll = true;
			}
		}
		for (int j = 0; j < chaFemales.Length; j++)
		{
			if (!(chaFemales[j] == null) && !(chaFemales[j].objBody == null))
			{
				chaFemalesTrans[j].position = pos;
				chaFemalesTrans[j].rotation = rot;
				chaFemalesTrans[j].localPosition += rot * offsetpos;
				chaFemalesTrans[j].localRotation = Quaternion.Euler(offsetrot) * chaFemalesTrans[j].localRotation;
				chaFemales[j].animBody.transform.localPosition = Vector3.zero;
				chaFemales[j].animBody.transform.localRotation = Quaternion.identity;
				chaFemales[j].SetPosition(Vector3.zero);
				chaFemales[j].SetRotation(Vector3.zero);
				chaFemales[j].ResetDynamicBoneAll();
				chaFemales[j].resetDynamicBoneAll = true;
			}
		}
		if (_FadeStart)
		{
			fade.FadeStart();
		}
		ctrlItem.setTransform(chaFemalesTrans[0].position, chaFemalesTrans[0].rotation.eulerAngles);
		return true;
	}

	public bool SetMovePositionPoint(Transform trans, Vector3 offsetpos, Vector3 offsetrot)
	{
		SetPosition(trans, offsetpos, offsetrot, _FadeStart: false);
		GlobalMethod.setCameraBase(ctrlFlag.cameraCtrl, chaFemalesTrans[0]);
		GlobalMethod.loadCamera(ctrlFlag.cameraCtrl, hSceneManager.strAssetCameraList, ctrlFlag.nowAnimationInfo.nameCamera);
		return true;
	}

	private bool ShortcutKey()
	{
		bool flag = sprite.isFade;
		if (!flag)
		{
			flag = sprite.GetFadeKindProc() == HSceneSprite.FadeKindProc.OutEnd;
		}
		if (!flag)
		{
			flag = Scene.IsFadeNow;
		}
		if (!flag)
		{
			flag = Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog);
		}
		if (!flag)
		{
			flag = Scene.Overlaps.Any((Scene.IOverlap o) => o is ConfigWindow) || ConfigWindow.isActive;
		}
		if (!flag)
		{
			flag = Scene.Overlaps.Any((Scene.IOverlap o) => o is ShortcutViewDialog) || ShortcutViewDialog.isActive;
		}
		if (!flag)
		{
			flag = Scene.Overlaps.Any((Scene.IOverlap o) => o is global::Tutorial2D.Tutorial2D) || global::Tutorial2D.Tutorial2D.isActive;
		}
		if (!flag)
		{
			flag = ctrlFlag.inputForcus;
		}
		if (flag)
		{
			return true;
		}
		GlobalMethod.CameraKeyCtrl(ctrlFlag.cameraCtrl, chaFemales);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ExitDialog.Load();
		}
		else if (Input.GetKeyDown(KeyCode.F1))
		{
			ConfigWindow.UnLoadAction = delegate
			{
			};
			ConfigWindow.TitleChangeAction = delegate
			{
				ConfigEnd();
				ConfigWindow.UnLoadAction = null;
			};
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
			ConfigWindow.Load();
		}
		else if (Input.GetKeyDown(KeyCode.F2))
		{
			ShortcutViewDialog.Load();
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		}
		else if (Input.GetKeyDown(KeyCode.F3) && Singleton<Game>.Instance.saveData.TutorialNo == -1)
		{
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.isAll = true;
			SingletonInitializer<global::Tutorial2D.Tutorial2D>.instance.nowKind = 5;
			global::Tutorial2D.Tutorial2D.Load();
			GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
		}
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Manager.Config.HData.EyeDir0 = !Manager.Config.HData.EyeDir0;
				if (chaFemales[0] != null)
				{
					ctrlEyeNeckFemale[0].SetConfigBehaviour(chaFemales[0].getAnimatorStateInfo(0), Manager.Config.HData.NeckDir0, Manager.Config.HData.EyeDir0);
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Manager.Config.HData.NeckDir0 = !Manager.Config.HData.NeckDir0;
				if (chaFemales[0] != null)
				{
					ctrlEyeNeckFemale[0].SetConfigBehaviour(chaFemales[0].getAnimatorStateInfo(0), Manager.Config.HData.NeckDir0, Manager.Config.HData.EyeDir0);
				}
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Manager.Config.HData.EyeDir1 = !Manager.Config.HData.EyeDir1;
				if (chaFemales[1] != null)
				{
					ctrlEyeNeckFemale[1].SetConfigBehaviour(chaFemales[1].getAnimatorStateInfo(0), Manager.Config.HData.NeckDir1, Manager.Config.HData.EyeDir1);
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Manager.Config.HData.NeckDir1 = !Manager.Config.HData.NeckDir1;
				if (chaFemales[1] != null)
				{
					ctrlEyeNeckFemale[1].SetConfigBehaviour(chaFemales[1].getAnimatorStateInfo(0), Manager.Config.HData.NeckDir1, Manager.Config.HData.EyeDir1);
				}
			}
		}
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Manager.Config.HData.Visible = !Manager.Config.HData.Visible;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Manager.Config.HData.Son = !Manager.Config.HData.Son;
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				Manager.Config.HData.Cloth = !Manager.Config.HData.Cloth;
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				Manager.Config.HData.Accessory = !Manager.Config.HData.Accessory;
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				Manager.Config.HData.Shoes = !Manager.Config.HData.Shoes;
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Manager.Config.HData.SecondVisible = !Manager.Config.HData.SecondVisible;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Manager.Config.HData.SecondSon = !Manager.Config.HData.SecondSon;
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				Manager.Config.HData.SecondCloth = !Manager.Config.HData.SecondCloth;
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				Manager.Config.HData.SecondAccessory = !Manager.Config.HData.SecondAccessory;
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				Manager.Config.HData.SecondShoes = !Manager.Config.HData.SecondShoes;
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			Manager.Config.HData.SimpleBody = !Manager.Config.HData.SimpleBody;
		}
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			Manager.Config.HData.SiruDraw = (Manager.Config.HData.SiruDraw + 1) % 3;
		}
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				byte lv = (byte)((!GlobalMethod.CheckFlagsArray(new bool[5]
				{
					chaFemales[0].GetSiruFlag(ChaFileDefine.SiruParts.SiruKao) != 0,
					chaFemales[0].GetSiruFlag(ChaFileDefine.SiruParts.SiruFrontTop) != 0,
					chaFemales[0].GetSiruFlag(ChaFileDefine.SiruParts.SiruFrontBot) != 0,
					chaFemales[0].GetSiruFlag(ChaFileDefine.SiruParts.SiruBackTop) != 0,
					chaFemales[0].GetSiruFlag(ChaFileDefine.SiruParts.SiruBackBot) != 0
				}, 1)) ? 2u : 0u);
				if ((bool)chaFemales[0] && chaFemales[0].visibleAll && chaFemales[0].objBody != null)
				{
					for (ChaFileDefine.SiruParts siruParts = ChaFileDefine.SiruParts.SiruKao; siruParts < (ChaFileDefine.SiruParts)5; siruParts++)
					{
						chaFemales[0].SetSiruFlag(siruParts, lv);
					}
				}
			}
		}
		else if ((bool)chaFemales[1] && chaFemales[1].visibleAll && chaFemales[1].objBody != null && Input.GetKeyDown(KeyCode.Alpha0))
		{
			byte lv2 = (byte)((!GlobalMethod.CheckFlagsArray(new bool[5]
			{
				chaFemales[1].GetSiruFlag(ChaFileDefine.SiruParts.SiruKao) != 0,
				chaFemales[1].GetSiruFlag(ChaFileDefine.SiruParts.SiruFrontTop) != 0,
				chaFemales[1].GetSiruFlag(ChaFileDefine.SiruParts.SiruFrontBot) != 0,
				chaFemales[1].GetSiruFlag(ChaFileDefine.SiruParts.SiruBackTop) != 0,
				chaFemales[1].GetSiruFlag(ChaFileDefine.SiruParts.SiruBackBot) != 0
			}, 1)) ? 2u : 0u);
			for (ChaFileDefine.SiruParts siruParts2 = ChaFileDefine.SiruParts.SiruKao; siruParts2 < (ChaFileDefine.SiruParts)5; siruParts2++)
			{
				chaFemales[1].SetSiruFlag(siruParts2, lv2);
			}
		}
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			int urineDraw = Manager.Config.HData.UrineDraw;
			urineDraw = GlobalMethod.ValLoop(urineDraw + 1, 3);
			Manager.Config.HData.UrineDraw = urineDraw;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			Manager.Config.HData.InitCamera = !Manager.Config.HData.InitCamera;
		}
		if (Input.GetKeyDown(KeyCode.Z))
		{
			Manager.Config.CameraData.Look = !Manager.Config.CameraData.Look;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			Manager.Config.GraphicData.Map = !Manager.Config.GraphicData.Map;
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			Manager.Config.GraphicData.AmbientLight = !Manager.Config.GraphicData.AmbientLight;
		}
		if (Input.GetKeyDown(KeyCode.Space) && sprite != null)
		{
			sprite.UIHide();
		}
		if (Input.GetKeyDown(KeyCode.F11) && gameScreenShot != null)
		{
			Illusion.Game.Utils.Sound.Play(SystemSE.photo);
			gameScreenShot.Capture();
		}
		return true;
	}

	private bool GetAutoAnimation(bool _isFirst)
	{
		autoMotion = ctrlAuto.GetAnimation(lstAnimInfo, ctrlFlag.initiative, _isFirst);
		if (autoMotion == null || lstAnimInfo.Length <= autoMotion.mode || autoMotion.mode == -1)
		{
			return false;
		}
		foreach (AnimationListInfo item in lstAnimInfo[autoMotion.mode])
		{
			if (item.id == autoMotion.id)
			{
				ctrlFlag.selectAnimationListInfo = item;
				break;
			}
		}
		return true;
	}

	private int GetAnimationListModeFromSelectInfo(AnimationListInfo _info)
	{
		if (_info == null)
		{
			return -1;
		}
		int result = -1;
		for (int i = 0; i < lstAnimInfo.Length; i++)
		{
			for (int j = 0; j < lstAnimInfo[i].Count; j++)
			{
				if (lstAnimInfo[i][j] == _info)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	private bool SetStartVoice()
	{
		if (Singleton<Game>.Instance.eventNo != -1)
		{
			return false;
		}
		ctrlFlag.voice.playStart = 0;
		return true;
	}

	private void SetClothStateStartMotion(int _cha, AnimationListInfo info)
	{
		if (chaFemales.Length > _cha)
		{
			byte state = (byte)info.nFemaleUpperCloths[_cha];
			byte state2 = (byte)info.nFemaleLowerCloths[_cha];
			GlobalMethod.SetAllClothState(chaFemales[_cha], _isUpper: true, state);
			GlobalMethod.SetAllClothState(chaFemales[_cha], _isUpper: false, state2);
		}
	}

	private bool ReturnToNormalFromTheAuto()
	{
		int animationListModeFromSelectInfo = GetAnimationListModeFromSelectInfo(ctrlFlag.nowAnimationInfo);
		if (animationListModeFromSelectInfo != -1)
		{
			foreach (AnimationListInfo item in lstAnimInfo[animationListModeFromSelectInfo])
			{
				if (item.id == ctrlFlag.nowAnimationInfo.nBackInitiativeID)
				{
					ctrlFlag.selectAnimationListInfo = item;
					break;
				}
			}
		}
		return true;
	}

	private void EndProcADV()
	{
		int eventNo = Singleton<Game>.Instance.eventNo;
		bool flag = eventNo == 0 || eventNo == 1 || eventNo == 21 || eventNo == 22 || eventNo == 23 || eventNo == 25 || eventNo == 26 || eventNo == -1;
		flag |= (eventNo == 3 || eventNo == 4 || eventNo == 5 || eventNo == 6) && Singleton<Game>.Instance.peepKind == 4;
		bool flag2 = !flag && (eventNo == 17 || eventNo == 18 || eventNo == 19);
		int personality = Singleton<HSceneManager>.Instance.females[0].fileParam2.personality;
		ChaFileGameInfo2 fileGameInfo = chaFemales[0].fileGameInfo2;
		int changeState = -1;
		int changeResist = -1;
		int num = 0;
		foreach (ChaFileDefine.SiruParts value in Enum.GetValues(typeof(ChaFileDefine.SiruParts)))
		{
			if (chaFemales[0].GetSiruFlag(value) == 2)
			{
				num++;
			}
		}
		hSceneManager.maleFinish = ctrlFlag.numInside + ctrlFlag.numOutSide + ctrlFlag.numDrink + ctrlFlag.numVomit;
		hSceneManager.femaleFinish = ctrlFlag.numOrgasmTotal;
		hSceneManager.endStatus = (byte)(ctrlFlag.isFaintness ? 1u : 0u);
		hSceneManager.isCtrl = !ctrlFlag.isNotCtrl;
		ChaControl[] array = chaFemales;
		foreach (ChaControl chaControl in array)
		{
			if (!(chaControl == null) && chaControl.fileGameInfo2 != null)
			{
				chaControl.fileGameInfo2.hCount++;
			}
		}
		array = chaMales;
		foreach (ChaControl chaControl2 in array)
		{
			if (!(chaControl2 == null) && chaControl2.fileGameInfo2 != null)
			{
				chaControl2.fileGameInfo2.hCount++;
			}
		}
		if (flag || eventNo == 24)
		{
			ChaFileDefine.State nowDrawState = chaFemales[0].fileGameInfo2.nowDrawState;
			ctrlFlag.EndSetAddMindParam(chaFemales[0].fileParam2.hAttribute);
			ctrlFlag.EndSetAddTraitParam(chaFemales[0].fileParam2.trait);
			ctrlFlag.EndSetAddTaiiParam(chaFemales[0].fileGameInfo2);
			ctrlFlag.ParamCalc(chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
			ChaFileDefine.State nowDrawState2 = chaFemales[0].fileGameInfo2.nowDrawState;
			if (nowDrawState != nowDrawState2)
			{
				if (nowDrawState == ChaFileDefine.State.Broken)
				{
					changeState = 6;
				}
				else
				{
					switch (nowDrawState2)
					{
					case ChaFileDefine.State.Blank:
						changeState = -1;
						break;
					case ChaFileDefine.State.Favor:
						changeState = 2;
						break;
					case ChaFileDefine.State.Enjoyment:
						changeState = 3;
						break;
					case ChaFileDefine.State.Slavery:
						changeState = 4;
						break;
					case ChaFileDefine.State.Aversion:
						changeState = 5;
						break;
					case ChaFileDefine.State.Broken:
						changeState = 0;
						break;
					case ChaFileDefine.State.Dependence:
						changeState = 1;
						break;
					}
				}
			}
		}
		int resistH = chaFemales[0].fileGameInfo2.resistH;
		ctrlFlag.FinishResistParamCalc(chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.hAttribute, chaFemales[0].fileParam2.personality);
		int resistH2 = chaFemales[0].fileGameInfo2.resistH;
		if (resistH != resistH2 && resistH2 >= 100)
		{
			changeResist = 0;
		}
		int num2 = -1;
		if (flag || eventNo == 24)
		{
			num2 = AfterPtn(personality, fileGameInfo, changeState, changeResist, num);
		}
		if (flag)
		{
			switch (num2)
			{
			case 0:
				Singleton<ADVManager>.Instance.advDelivery.Set("13", personality, 27);
				break;
			case 1:
				Singleton<ADVManager>.Instance.advDelivery.Set("14", personality, 27);
				break;
			case 2:
			case 3:
			case 4:
				Singleton<ADVManager>.Instance.advDelivery.Set("12", personality, 27);
				break;
			case 5:
				Singleton<ADVManager>.Instance.advDelivery.Set("15", personality, 27);
				break;
			case 6:
				Singleton<ADVManager>.Instance.advDelivery.Set("11", personality, 27);
				break;
			case 7:
				Singleton<ADVManager>.Instance.advDelivery.Set("8", personality, 27);
				break;
			case 8:
				Singleton<ADVManager>.Instance.advDelivery.Set("9", personality, 27);
				break;
			case 9:
			case 12:
			case 13:
			case 18:
			case 20:
			case 21:
			case 23:
			case 26:
				Singleton<ADVManager>.Instance.advDelivery.Set("0", personality, 27);
				break;
			case 10:
				Singleton<ADVManager>.Instance.advDelivery.Set("10", personality, 27);
				break;
			case 14:
				Singleton<ADVManager>.Instance.advDelivery.Set("5", personality, 27);
				break;
			case 15:
				Singleton<ADVManager>.Instance.advDelivery.Set("4", personality, 27);
				break;
			case 16:
				Singleton<ADVManager>.Instance.advDelivery.Set("7", personality, 27);
				break;
			case 17:
				Singleton<ADVManager>.Instance.advDelivery.Set("6", personality, 27);
				break;
			case 22:
				Singleton<ADVManager>.Instance.advDelivery.Set("3", personality, 27);
				break;
			case 25:
			case 28:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, 27);
				break;
			case 27:
				Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, 27);
				break;
			}
		}
		else if (flag2)
		{
			Singleton<ADVManager>.Instance.advDelivery.Set("0", personality, 20);
		}
		else
		{
			switch (eventNo)
			{
			case 2:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", -100, 2);
				break;
			case 3:
			case 4:
			case 5:
			case 6:
				if (Singleton<Game>.Instance.peepKind == 0)
				{
					if (!GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality, _isDependece: true))
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("7", personality, eventNo);
					}
					else
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
					}
				}
				else if (Singleton<Game>.Instance.peepKind == 1)
				{
					if (!GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality, _isDependece: true))
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("8", personality, eventNo);
					}
					else
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
					}
				}
				else if (Singleton<Game>.Instance.peepKind == 2)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("5", personality, eventNo);
				}
				else if (Singleton<Game>.Instance.peepKind == 3)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("6", personality, eventNo);
				}
				break;
			case 7:
				if (!GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality))
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
				}
				else
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("3", personality, eventNo);
				}
				break;
			case 8:
			case 9:
			case 10:
			case 11:
				Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
				break;
			case 12:
			case 13:
				Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
				break;
			case 14:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", -100, eventNo);
				break;
			case 15:
				if (ctrlFlag.isNotCtrl)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("4", personality, eventNo);
				}
				else if (hSceneManager.endStatus == 1)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("3", personality, eventNo);
				}
				else
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
				}
				break;
			case 16:
				if (ctrlFlag.numOrgasmTotal >= 5)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("4", personality, eventNo);
				}
				else
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("5", personality, eventNo);
				}
				break;
			case 24:
				Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
				break;
			case 28:
			case 29:
				if (Singleton<Game>.Instance.peepKind == 0)
				{
					if (GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality))
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
						break;
					}
					Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
					Singleton<Game>.Instance.mapNo = 100;
				}
				else if (Singleton<Game>.Instance.peepKind == 1)
				{
					if (GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality))
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("3", personality, eventNo);
						break;
					}
					Singleton<ADVManager>.Instance.advDelivery.Set("4", personality, eventNo);
					Singleton<Game>.Instance.mapNo = 100;
				}
				else if (Singleton<Game>.Instance.peepKind == 2)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("8", personality, eventNo);
				}
				else if (Singleton<Game>.Instance.peepKind == 3)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("9", personality, eventNo);
				}
				else if (Singleton<Game>.Instance.peepKind == 6)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("10", personality, eventNo);
				}
				break;
			case 30:
			case 31:
				if (Singleton<Game>.Instance.peepKind == 0)
				{
					if (GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality))
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
						break;
					}
					Singleton<ADVManager>.Instance.advDelivery.Set("2", personality, eventNo);
					Singleton<Game>.Instance.mapNo = 102;
				}
				else if (Singleton<Game>.Instance.peepKind == 1)
				{
					if (GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality))
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("3", personality, eventNo);
						break;
					}
					Singleton<ADVManager>.Instance.advDelivery.Set("4", personality, eventNo);
					Singleton<Game>.Instance.mapNo = 102;
				}
				else if (Singleton<Game>.Instance.peepKind == 2)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("10", personality, eventNo);
				}
				else if (Singleton<Game>.Instance.peepKind == 3)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("11", personality, eventNo);
				}
				else if (Singleton<Game>.Instance.peepKind == 5)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("12", personality, eventNo);
				}
				break;
			case 32:
				if (!GlobalHS2Calc.IsPeepingFound(fileGameInfo, personality))
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("3", personality, eventNo);
				}
				else
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("4", personality, eventNo);
				}
				break;
			case 33:
				if (!Singleton<Game>.Instance.isConciergeAngry)
				{
					if (fileGameInfo.hCount < 4)
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("1", -1, eventNo);
					}
					else if (hSceneManager.maleFinish == 0 && hSceneManager.femaleFinish == 0)
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("4", -1, eventNo);
					}
					else if (hSceneManager.endStatus == 1)
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("3", -1, eventNo);
					}
					else
					{
						Singleton<ADVManager>.Instance.advDelivery.Set("2", -1, eventNo);
					}
				}
				else if (hSceneManager.maleFinish == 0 && hSceneManager.femaleFinish == 0)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("6", -1, eventNo);
				}
				else
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("5", -1, eventNo);
				}
				break;
			case 50:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
				break;
			case 51:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
				break;
			case 52:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
				break;
			case 53:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
				break;
			case 54:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
				break;
			case 55:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", personality, eventNo);
				break;
			case 56:
				if (fileGameInfo.hCount < 2)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("1", -2, eventNo);
				}
				else if (hSceneManager.maleFinish == 0 && hSceneManager.femaleFinish == 0)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("4", -2, eventNo);
				}
				else if (hSceneManager.endStatus == 1)
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("3", -2, eventNo);
				}
				else
				{
					Singleton<ADVManager>.Instance.advDelivery.Set("2", -2, eventNo);
				}
				break;
			case 58:
				Singleton<ADVManager>.Instance.advDelivery.Set("1", -1, eventNo);
				break;
			}
		}
		array = chaFemales;
		foreach (ChaControl chaControl3 in array)
		{
			if (!(chaControl3 == null) && chaControl3.fileGameInfo2 != null)
			{
				if (chaControl3.chaID == -1)
				{
					chaControl3.chaFile.SaveCharaFile(Singleton<Character>.Instance.conciergePath);
				}
				else if (chaControl3.chaID == -2)
				{
					chaControl3.chaFile.SaveCharaFile(Singleton<Character>.Instance.sitriPath);
				}
				else if (!chaControl3.chaFile.charaFileName.IsNullOrEmpty())
				{
					chaControl3.chaFile.SaveCharaFile(chaControl3.chaFile.charaFileName);
				}
			}
		}
		array = chaMales;
		foreach (ChaControl chaControl4 in array)
		{
			if (!(chaControl4 == null) && chaControl4.fileGameInfo2 != null && !chaControl4.chaFile.charaFileName.IsNullOrEmpty())
			{
				chaControl4.chaFile.SaveCharaFile(chaControl4.chaFile.charaFileName);
			}
		}
		SaveData.AddHCount();
		SaveData.AddHFinish(hSceneManager.maleFinish);
		SaveData.AddHOrgasm(hSceneManager.femaleFinish);
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.In);
		Observable.FromCoroutine(() => EndFade()).Finally(delegate
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "ADV",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: false);
		}).Subscribe();
	}

	private int AfterPtn(int mainPersonal, ChaFileGameInfo2 info, int ChangeState, int ChangeResist, int numSiruPhase2)
	{
		int result = 7;
		bool flag = ctrlFlag.isJudgeSelect.Contains(HSceneFlagCtrl.JudgeSelect.Pain);
		switch (ChangeState)
		{
		case 0:
			ctrlFlag.AfterParamCalc(0, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
			result = 0;
			break;
		case 1:
			ctrlFlag.AfterParamCalc(1, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
			result = 1;
			break;
		default:
			if (info.nowState == ChaFileDefine.State.Broken)
			{
				if (hSceneManager.maleFinish > 0 || hSceneManager.femaleFinish > 0)
				{
					if (hSceneManager.maleFinish <= hSceneManager.femaleFinish)
					{
						ctrlFlag.AfterParamCalc(2, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
						result = 2;
					}
					else if (hSceneManager.maleFinish > hSceneManager.femaleFinish)
					{
						ctrlFlag.AfterParamCalc(3, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
						result = 3;
					}
				}
				else
				{
					ctrlFlag.AfterParamCalc(4, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
					result = 4;
				}
			}
			else if (ChangeState == 6)
			{
				ctrlFlag.AfterParamCalc(5, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 5;
			}
			else if (ChangeResist != -1)
			{
				ctrlFlag.AfterParamCalc(6, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 6;
			}
			else if (hSceneManager.femaleFinish == 0 && hSceneManager.maleFinish == 0)
			{
				ctrlFlag.AfterParamCalc(7, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 7;
			}
			else if (ctrlFlag.isFaintness)
			{
				ctrlFlag.AfterParamCalc(8, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 8;
			}
			else if (ctrlFlag.numSameOrgasm >= 3 || numSiruPhase2 >= 3)
			{
				bool[] check = new bool[2]
				{
					ctrlFlag.numSameOrgasm >= 3,
					numSiruPhase2 >= 3
				};
				RandADV(check, out var ids, out var result2);
				switch (ids[result2])
				{
				case 0:
					ctrlFlag.AfterParamCalc(9, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
					result = 9;
					break;
				case 1:
					ctrlFlag.AfterParamCalc(10, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
					result = 10;
					break;
				}
			}
			else if (hSceneManager.maleFinish >= 3 && hSceneManager.femaleFinish > 0 && hSceneManager.maleFinish > hSceneManager.femaleFinish)
			{
				ctrlFlag.AfterParamCalc(12, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 12;
			}
			else if (hSceneManager.maleFinish > 0 && hSceneManager.femaleFinish >= 3 && hSceneManager.maleFinish <= hSceneManager.femaleFinish)
			{
				ctrlFlag.AfterParamCalc(13, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 13;
			}
			else if (ctrlFlag.numOrgasmM2 >= 2 || ctrlFlag.numShotM2 >= 2 || ctrlFlag.numOrgasmF2 >= 2 || ctrlFlag.numShotF2 >= 2)
			{
				bool[] check2 = new bool[4]
				{
					ctrlFlag.numOrgasmM2 >= 2,
					ctrlFlag.numShotM2 >= 2,
					ctrlFlag.numOrgasmF2 >= 2,
					ctrlFlag.numShotF2 >= 2
				};
				RandADV(check2, out var ids2, out var result3);
				switch (ids2[result3])
				{
				case 0:
					ctrlFlag.AfterParamCalc(14, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
					result = 14;
					break;
				case 1:
					ctrlFlag.AfterParamCalc(15, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
					result = 15;
					break;
				case 2:
					ctrlFlag.AfterParamCalc(16, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
					result = 16;
					break;
				case 3:
					ctrlFlag.AfterParamCalc(17, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
					result = 17;
					break;
				}
			}
			else if (ctrlFlag.numSameOrgasm == 2)
			{
				ctrlFlag.AfterParamCalc(18, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 18;
			}
			else if (hSceneManager.maleFinish == 2 && hSceneManager.femaleFinish > 0 && hSceneManager.maleFinish > hSceneManager.femaleFinish)
			{
				ctrlFlag.AfterParamCalc(20, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 20;
			}
			else if (hSceneManager.femaleFinish == 2 && hSceneManager.maleFinish > 0 && hSceneManager.maleFinish <= hSceneManager.femaleFinish)
			{
				ctrlFlag.AfterParamCalc(21, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 21;
			}
			else if (flag && ctrlFlag.isPainAction && (hSceneManager.femaleFinish != 0 || hSceneManager.maleFinish != 0))
			{
				ctrlFlag.AfterParamCalc(22, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 22;
			}
			else if (ctrlFlag.numSameOrgasm == 1)
			{
				ctrlFlag.AfterParamCalc(23, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 23;
			}
			else if (hSceneManager.maleFinish == 1 && hSceneManager.maleFinish > hSceneManager.femaleFinish)
			{
				ctrlFlag.AfterParamCalc(25, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 25;
			}
			else if (hSceneManager.femaleFinish == 1 && hSceneManager.maleFinish != 0 && hSceneManager.maleFinish <= hSceneManager.femaleFinish)
			{
				ctrlFlag.AfterParamCalc(26, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 26;
			}
			else if (hSceneManager.femaleFinish != 0 && hSceneManager.maleFinish == 0)
			{
				ctrlFlag.AfterParamCalc(27, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 27;
			}
			else if (hSceneManager.maleFinish != 0 && hSceneManager.femaleFinish == 0)
			{
				ctrlFlag.AfterParamCalc(28, chaFemales[0].fileGameInfo2, chaFemales[0].fileParam2.personality);
				result = 28;
			}
			break;
		}
		_ = -1;
		return result;
	}

	private void RandADV(bool[] check, out List<int> ids, out int result)
	{
		ids = new List<int>();
		for (int i = 0; i < check.Length; i++)
		{
			int num = i;
			if (check[num])
			{
				ids.Add(num);
			}
		}
		ShuffleRand shuffleRand = new ShuffleRand(ids.Count);
		result = shuffleRand.Get();
	}

	private IEnumerator EndFade()
	{
		while (Scene.sceneFadeCanvas.canvasGroup.alpha < 1f)
		{
			yield return null;
		}
		se.ResetAllSetting();
		sprite.ReSetLight();
		if (ctrlEyeNeckFemale[0] != null && chaFemales[0] != null && chaFemales[0].objBodyBone != null)
		{
			ctrlEyeNeckFemale[0].NowEndADV = true;
		}
		if (ctrlEyeNeckFemale[1] != null && chaFemales[1] != null && chaFemales[1].objBodyBone != null)
		{
			ctrlEyeNeckFemale[1].NowEndADV = true;
		}
		if (ctrlEyeNeckMale[0] != null && chaMales[0] != null && chaMales[0].objBodyBone != null)
		{
			ctrlEyeNeckMale[0].NowEndADV = true;
		}
		if (ctrlEyeNeckMale[1] != null && chaMales[1] != null && chaMales[1].objBodyBone != null)
		{
			ctrlEyeNeckMale[1].NowEndADV = true;
		}
		if (hSceneManager.player != null)
		{
			if (!hSceneManager.player.neckLookCtrl.enabled)
			{
				hSceneManager.player.neckLookCtrl.enabled = true;
			}
			if (!hSceneManager.player.eyeLookCtrl.enabled)
			{
				hSceneManager.player.eyeLookCtrl.enabled = true;
			}
			hSceneManager.player.ChangeLookNeckPtn(3);
			hSceneManager.player.ChangeLookNeckTarget(0);
			hSceneManager.player.ChangeLookEyesPtn(0);
			hSceneManager.player.ChangeLookEyesTarget(0);
			hSceneManager.player.ChangeMouthOpenMin(hSceneManager.player.fileStatus.mouthOpenMin);
		}
		if (hSceneManager.females != null)
		{
			for (int i = 0; i < hSceneManager.females.Length; i++)
			{
				if (!(hSceneManager.females[i] == null) && !(hSceneManager.females[i].neckLookCtrl == null))
				{
					if (!hSceneManager.females[i].neckLookCtrl.enabled)
					{
						hSceneManager.females[i].neckLookCtrl.enabled = true;
					}
					if (!hSceneManager.females[i].eyeLookCtrl.enabled)
					{
						hSceneManager.females[i].eyeLookCtrl.enabled = true;
					}
					hSceneManager.females[i].ChangeLookNeckPtn(3);
					hSceneManager.females[i].ChangeLookNeckTarget(0);
					hSceneManager.females[i].ChangeLookEyesPtn(0);
					hSceneManager.females[i].ChangeLookEyesTarget(0);
				}
			}
		}
		for (int j = 0; j < chaMales.Length; j++)
		{
			if (!(chaMales[j] == null) && !(chaMales[j].objBody == null))
			{
				chaMales[j].visibleSon = false;
			}
		}
		ctrlFlag.cameraCtrl.Reset(0);
		ctrlFlag.selectAnimationListInfo = null;
		if (ctrlItem != null)
		{
			ctrlItem.ReleaseItem();
		}
		ctrlFlag.cameraCtrl.visibleForceVanish(_visible: true);
		if (chaFemales[1] != null && chaFemales[1].visibleAll)
		{
			chaFemales[1].visibleAll = false;
		}
		if (sprite.objMotionListPanel.activeSelf)
		{
			sprite.objMotionListPanel.SetActive(value: false);
		}
		if (sprite.categoryMain.gameObject.activeSelf)
		{
			sprite.categoryMain.gameObject.SetActive(value: false);
		}
		ctrlObi.EndPlocSolver();
		int i2 = 0;
		while (i2 < ctrlHitObjectFemales.Length)
		{
			if (ctrlHitObjectFemales[i2] != null)
			{
				ctrlHitObjectFemales[i2].PreEndPloc();
				yield return null;
				ctrlHitObjectFemales[i2].EndPloc();
			}
			int num = i2 + 1;
			i2 = num;
		}
		i2 = 0;
		while (i2 < ctrlHitObjectMales.Length)
		{
			if (ctrlHitObjectMales[i2] != null)
			{
				ctrlHitObjectMales[i2].PreEndPloc();
				yield return null;
				ctrlHitObjectMales[i2].EndPloc();
			}
			int num = i2 + 1;
			i2 = num;
		}
		yield return null;
		yield return null;
		Singleton<Character>.Instance.DeleteChara(chaMales[1]);
		hPointCtrl.EndProc();
	}

	private RuntimeAnimatorController MixRuntimeControler(RuntimeAnimatorController src, RuntimeAnimatorController over1, RuntimeAnimatorController over2)
	{
		if (src == null || over1 == null || over2 == null)
		{
			return null;
		}
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(src);
		List<AnimationClip> list = new List<AnimationClip>();
		list.AddRange(new AnimatorOverrideController(over1).animationClips);
		list.AddRange(new AnimatorOverrideController(over2).animationClips);
		foreach (AnimationClip item in list)
		{
			animatorOverrideController[item.name] = item;
		}
		animatorOverrideController.name = over1.name;
		return animatorOverrideController;
	}

	public void LoadMoveOffset(string file, out Vector3 pos, out Vector3 rot)
	{
		GlobalMethod.GetListString(GlobalMethod.LoadAllListText(hSceneManager.strAssetMoveOffsetListFolder, file), out var data);
		pos = Vector3.zero;
		rot = Vector3.zero;
		int length = data.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			int num = 0;
			pos.x = float.Parse(data[i][num++]);
			pos.y = float.Parse(data[i][num++]);
			pos.z = float.Parse(data[i][num++]);
			rot.x = float.Parse(data[i][num++]);
			rot.y = float.Parse(data[i][num++]);
			rot.z = float.Parse(data[i][num++]);
		}
	}

	public ChaControl[] GetFemales()
	{
		return chaFemales;
	}

	public ProcBase GetProcBase()
	{
		if (mode == -1 || lstProc.Count < mode)
		{
			return null;
		}
		return lstProc[mode];
	}

	public ChaControl[] GetMales()
	{
		return chaMales;
	}
}
