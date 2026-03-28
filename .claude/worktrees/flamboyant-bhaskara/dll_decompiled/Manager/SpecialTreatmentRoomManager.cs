using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADV;
using AIChara;
using Actor;
using CameraEffector;
using CharaCustom;
using HS2;
using Illusion.Anime;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Manager;

public class SpecialTreatmentRoomManager : Singleton<SpecialTreatmentRoomManager>
{
	public enum PlanCategory
	{
		None = -1,
		Favor,
		Enjoyment,
		Aversion,
		Slavery,
		Broken,
		Dependence
	}

	public enum Mode
	{
		Main,
		Plan,
		Confirmation
	}

	public enum CameraPositionKind
	{
		Main,
		Plan
	}

	private class PackData : CharaPackData
	{
		public int drawTutorial2D
		{
			get
			{
				if (base.Vars != null && base.Vars.TryGetValue("drawTutorial2D", out var value))
				{
					return (int)value.o;
				}
				return -1;
			}
		}

		public string MapName { get; set; } = "";

		public string EventCGBundle { get; set; } = "";

		public string EventCGName { get; set; } = "";

		public override List<Program.Transfer> Create()
		{
			List<Program.Transfer> list = base.Create();
			list.Add(Program.Transfer.VAR("string", "MapName", MapName));
			list.Add(Program.Transfer.VAR("string", "EventCGBundle", EventCGBundle));
			list.Add(Program.Transfer.VAR("string", "EventCGName", EventCGName));
			return list;
		}

		public override void Receive(TextScenario scenario)
		{
			using (Dictionary<int, List<CommandData>>.Enumerator enumerator = CommandListToTable().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					foreach (CommandData item in enumerator.Current.Value)
					{
						item.ReceiveADV(scenario);
					}
				}
			}
			base.Vars = scenario.Vars;
			base.onComplete?.Invoke();
		}
	}

	[SerializeField]
	private OpenData openData = new OpenData();

	private BoolReactiveProperty isNowADV = new BoolReactiveProperty(initialValue: false);

	[SerializeField]
	private CanvasGroup _cgBase;

	[SerializeField]
	private List<CanvasGroup> _cgModes = new List<CanvasGroup>();

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private CrossFade crossFade;

	[SerializeField]
	private ConfigEffectorWet configEffector;

	[SerializeField]
	private Transform camTrans;

	[SerializeField]
	private Transform transDOFTarget;

	[SerializeField]
	private ObjectCategoryBehaviour _ocbTutorial;

	private bool isMove;

	[SerializeField]
	private float moveSpeed = 0.5f;

	[Header("制御スクリプト")]
	[SerializeField]
	private STRMainMenu _mainMenu;

	[SerializeField]
	private STRPlanSelect _planSelect;

	[SerializeField]
	private STRConfirmation _confirmation;

	private Controller ctrl;

	private Dictionary<int, HomeCameraInfo.Param> dicCameraInfo = new Dictionary<int, HomeCameraInfo.Param>();

	private STRActionInfoData actionInfoData;

	private Animator animMenu;

	[SerializeField]
	private string menuAnimationObjectName = "";

	public static readonly int[] mapNos = new int[6] { 18, 19, 21, 20, 23, 22 };

	private PackData packData { get; set; }

	public bool IsADV => isNowADV.Value;

	public CanvasGroup CGBase => _cgBase;

	public List<CanvasGroup> CGModes => _cgModes;

	public Camera MainCamera => cam;

	public ObjectCategoryBehaviour OCBTutorial => _ocbTutorial;

	public bool IsMove
	{
		get
		{
			return isMove;
		}
		set
		{
			isMove = value;
		}
	}

	public STRMainMenu MainMenu => _mainMenu;

	public STRPlanSelect PlanSelect => _planSelect;

	public STRConfirmation Confirmation => _confirmation;

	public ChaControl ConciergeChaCtrl { get; private set; }

	public Heroine ConciergeHeroine { get; private set; }

	public Dictionary<int, PlanNameInfo.Param> dicPlanNameInfo { get; private set; } = new Dictionary<int, PlanNameInfo.Param>();

	public STRActionInfoData ActionInfoData => actionInfoData;

	public PlanCategory nowSelectCategory { get; set; } = PlanCategory.None;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<AssetBundleManager>.IsInstance());
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		Singleton<Character>.Instance.DeleteCharaAll();
		Singleton<Character>.Instance.EndLoadAssetBundle();
		Singleton<Game>.Instance.heroineList.Clear();
		Singleton<HSceneManager>.Instance.player = null;
		Singleton<HSceneManager>.Instance.females[0] = null;
		Singleton<HSceneManager>.Instance.females[1] = null;
		foreach (HomeCameraInfo item in GlobalMethod.LoadAllFolder<HomeCameraInfo>(AssetBundleNames.SprListPath, "sproomcamerainfo", null, _subdirCheck: true))
		{
			foreach (HomeCameraInfo.Param item2 in item.param)
			{
				dicCameraInfo[item2.id] = new HomeCameraInfo.Param(item2);
			}
		}
		foreach (PlanNameInfo item3 in GlobalMethod.LoadAllFolder<PlanNameInfo>(AssetBundleNames.SprListPath, "planname", null, _subdirCheck: true))
		{
			foreach (PlanNameInfo.Param item4 in item3.param)
			{
				dicPlanNameInfo[item4.id] = new PlanNameInfo.Param(item4);
			}
		}
		actionInfoData = GlobalMethod.LoadAllFolderInOneFile<STRActionInfoData>(AssetBundleNames.SprPrefabsPath, "stractioninfodata", null, _subdirCheck: true);
		yield return StartCoroutine(LoadConciergeBody());
		ConciergeAnimationPlay(0);
		isNowADV.Subscribe(delegate(bool _isADV)
		{
			CGBase.blocksRaycasts = !_isADV;
		});
		Sound.Listener = cam.transform;
		(from _ in this.UpdateAsObservable()
			where isMove
			select _).Subscribe(delegate
		{
			if ((bool)camTrans)
			{
				camTrans.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
			}
		});
	}

	private void OnDestroy()
	{
		if ((bool)ConciergeChaCtrl)
		{
			ConciergeChaCtrl.animBody.runtimeAnimatorController = null;
		}
	}

	private IEnumerator LoadConciergeBody()
	{
		ConciergeChaCtrl = Singleton<Character>.Instance.GetChara(-2);
		if (ConciergeChaCtrl == null)
		{
			ConciergeChaCtrl = Singleton<Character>.Instance.CreateChara(1, Scene.commonSpace, -2);
		}
		Singleton<Character>.Instance.LoadSitriCharaFile(ConciergeChaCtrl);
		ConciergeChaCtrl.ChangeNowCoordinate();
		ConciergeHeroine = new Heroine(ConciergeChaCtrl.chaFile, isRandomize: false)
		{
			fixCharaID = -2
		};
		ConciergeHeroine.SetRoot(ConciergeChaCtrl.gameObject);
		ConciergeChaCtrl.releaseCustomInputTexture = false;
		ConciergeChaCtrl.Load();
		ConciergeChaCtrl.ChangeEyebrowPtn(0);
		ConciergeChaCtrl.ChangeEyesPtn(0);
		ConciergeChaCtrl.ChangeMouthPtn(0);
		ConciergeChaCtrl.ChangeLookEyesPtn(0);
		ConciergeChaCtrl.ChangeLookEyesTarget(0);
		ConciergeChaCtrl.ChangeLookNeckPtn(3);
		ConciergeChaCtrl.ChangeLookNeckTarget(0);
		ctrl = new Controller(ConciergeChaCtrl);
		ConciergeHeroine.param.Bind(new global::Actor.Actor(ConciergeHeroine));
		SetConciergePosition();
		yield return null;
	}

	public void GetMapAnimationObject()
	{
		GameObject mapRoot = BaseMap.mapRoot;
		if (!(mapRoot == null))
		{
			Transform transform = mapRoot.transform.FindLoop(menuAnimationObjectName);
			if ((bool)transform)
			{
				animMenu = transform.GetComponent<Animator>();
			}
		}
	}

	public void AnimationMenu(bool _isOpen)
	{
		if (!(animMenu == null))
		{
			animMenu.Play(_isOpen ? "open" : "close_Idle", 0, 0f);
		}
	}

	public void SetModeCanvasGroup(int _index)
	{
		foreach (var (canvasGroup, num) in _cgModes.ToForEachTuples())
		{
			canvasGroup.Enable(num == _index, isUseInteractable: false);
		}
	}

	public void SetCameraPosition(int _id, float _t = 0.5f)
	{
		if (!dicCameraInfo.TryGetValue(_id, out var value))
		{
			return;
		}
		if (!value.isTransformInfoData)
		{
			TransformInfoData transformInfoData = CommonLib.LoadAsset<TransformInfoData>(value.camBundle, value.camFile);
			if ((bool)transformInfoData && (bool)cam)
			{
				transformInfoData.Reflect(cam.transform);
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(value.camBundle, isUnloadForceRefCount: true);
			}
		}
		else
		{
			TransformCompositeInfoData transformCompositeInfoData = CommonLib.LoadAsset<TransformCompositeInfoData>(value.camBundle, value.camFile);
			if ((bool)transformCompositeInfoData && (bool)cam)
			{
				transformCompositeInfoData.Reflect(cam.transform, _t);
			}
			if ((bool)transformCompositeInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(value.camBundle, isUnloadForceRefCount: true);
			}
		}
		TransformInfoData transformInfoData2 = CommonLib.LoadAsset<TransformInfoData>(value.dofBundle, value.dofFile);
		if ((bool)transformInfoData2 && (bool)configEffector)
		{
			transformInfoData2.Reflect(configEffector.dof.focalTransform);
		}
		if ((bool)transformInfoData2)
		{
			AssetBundleManager.UnloadAssetBundle(value.camBundle, isUnloadForceRefCount: true);
		}
	}

	public void StartFade(bool _isForce = false)
	{
		if (!(crossFade == null) && (crossFade.isEnd || _isForce))
		{
			crossFade.FadeStart();
		}
	}

	public void SetConciergePosition()
	{
		actionInfoData.transformInfoData.Reflect(ConciergeChaCtrl.transform);
		ConciergeChaCtrl.resetDynamicBoneAll = true;
	}

	public bool ConciergeAnimationPlay(int _id, bool _isSameCheck = true)
	{
		if (ConciergeChaCtrl == null)
		{
			return false;
		}
		ctrl.PlayID(actionInfoData.animIDs[_id]);
		return true;
	}

	public async void OpenADV(string _bundle, string _asset, int _advCategory, int _kind, bool _isCameraDontMove, bool _isUseCorrectCamera = true, bool _isCharaBackUpPos = true, bool _isCameraDontMoveRelease = false, Action _onEnd = null)
	{
		await Setup.LoadAsync(base.transform);
		isNowADV.Value = true;
		Game game = Singleton<Game>.Instance;
		int kind = _kind;
		packData = new PackData();
		packData.SetCommandData(game.saveData, game.appendSaveData);
		packData.SetParam(ConciergeHeroine);
		packData.isParent = true;
		openData.bundle = _bundle;
		openData.asset = _asset;
		packData.MapName = BaseMap.ConvertMapName(game.mapNo);
		packData.EventCGName = BaseMap.ConvertMapNameEnglish(game.mapNo) + "_Event" + _advCategory.ToString("00");
		List<string> list = (from ab in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.AdvEventcgPath)
			orderby ab descending
			select ab).ToList();
		packData.EventCGBundle = string.Empty;
		foreach (string item in list)
		{
			if (GameSystem.IsPathAdd50(item) && AssetBundleCheck.GetAllAssetName(item, _WithExtension: false).Contains(packData.EventCGName))
			{
				packData.EventCGBundle = item;
				break;
			}
		}
		packData.onComplete = delegate
		{
			isNowADV.Value = false;
			if (kind == 1)
			{
				GameObject referenceInfo = ConciergeChaCtrl.GetReferenceInfo(ChaReference.RefObjKey.HeadParent);
				if ((bool)referenceInfo && (bool)transDOFTarget)
				{
					transDOFTarget.transform.position = referenceInfo.transform.position;
				}
				configEffector.dof.focalTransform = transDOFTarget;
			}
			Controller.Table.Get(ConciergeChaCtrl).itemHandler.DisableItems();
			_onEnd?.Invoke();
		};
		Setup.Open(openData, packData, _isCameraDontMove, _isUseCorrectCamera, _isCharaBackUpPos, _isCameraDontMoveRelease);
	}
}
