using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Actor;
using CameraEffector;
using HS2;
using Illusion.Anime;
using Illusion.Extensions;
using UnityEngine;

namespace Manager;

public class FurRoomSceneManager : Singleton<FurRoomSceneManager>
{
	public enum ConciergeUIModeKind
	{
		Main,
		Achievement,
		MapSelect
	}

	public enum CameraPositionKind
	{
		Concierge
	}

	[SerializeField]
	private CanvasGroup _cgMain;

	[SerializeField]
	private List<CanvasGroup> _cgConciergeModes = new List<CanvasGroup>();

	[SerializeField]
	private CanvasGroup _cgAchievement;

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private CrossFade crossFade;

	[SerializeField]
	private int conciergeButtonSelectCountMax = 50;

	[SerializeField]
	private FurRoomMapSelectUI _mapSelectUI;

	[SerializeField]
	private ConfigEffectorWet configEffector;

	private int selectCount;

	private Controller ctrl;

	private HomeConciergeInfoData homeConciergeInfoData;

	private ValueDictionary<int, List<TitleCharaStateInfo.Param>> dicCharaState = new ValueDictionary<int, List<TitleCharaStateInfo.Param>>();

	public CanvasGroup CGMain => _cgMain;

	public List<CanvasGroup> CGConciergeModes => _cgConciergeModes;

	public CanvasGroup CGAchievement => _cgAchievement;

	public int ConciergeButtonSelectCountMax => conciergeButtonSelectCountMax;

	public FurRoomMapSelectUI MapSelectUI => _mapSelectUI;

	public ChaControl ConciergeChaCtrl { get; private set; }

	public Heroine ConciergeHeroine { get; private set; }

	public HomeConciergeInfoData HomeConciergeInfoData => homeConciergeInfoData;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<AssetBundleManager>.IsInstance());
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		homeConciergeInfoData = GlobalMethod.LoadAllFolderInOneFile<HomeConciergeInfoData>(AssetBundleNames.FursroomPrefabsPath, "fursroominfodata", null, _subdirCheck: true);
		yield return StartCoroutine(LoadConciergeBody());
		yield return StartCoroutine(LoadList());
		Sound.Listener = cam.transform;
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
		ConciergeChaCtrl = Singleton<Character>.Instance.GetChara(-1);
		if (ConciergeChaCtrl == null)
		{
			ConciergeChaCtrl = Singleton<Character>.Instance.CreateChara(1, Scene.commonSpace, -1);
			Singleton<Character>.Instance.LoadConciergeCharaFile(ConciergeChaCtrl);
			ConciergeChaCtrl.ChangeNowCoordinate();
			ConciergeChaCtrl.releaseCustomInputTexture = false;
			ConciergeChaCtrl.Load();
		}
		ConciergeHeroine = new Heroine(ConciergeChaCtrl.chaFile, isRandomize: false)
		{
			fixCharaID = -1
		};
		ConciergeHeroine.SetRoot(ConciergeChaCtrl.gameObject);
		ConciergeChaCtrl.ChangeEyebrowPtn(0);
		ConciergeChaCtrl.ChangeEyesPtn(0);
		ConciergeChaCtrl.ChangeMouthPtn(0);
		ConciergeChaCtrl.ChangeLookEyesPtn(0);
		ConciergeChaCtrl.ChangeLookEyesTarget(0);
		ConciergeChaCtrl.ChangeLookNeckPtn(1);
		ConciergeChaCtrl.ChangeLookNeckTarget(0);
		ctrl = new Controller(ConciergeChaCtrl);
		ConciergeHeroine.param.Bind(new global::Actor.Actor(ConciergeHeroine));
		GameObject referenceInfo = ConciergeChaCtrl.GetReferenceInfo(ChaReference.RefObjKey.HeadParent);
		if ((bool)referenceInfo)
		{
			configEffector.dof.focalTransform = referenceInfo.transform;
		}
		yield return null;
	}

	private IEnumerator LoadList()
	{
		foreach (TitleCharaStateInfo item in GlobalMethod.LoadAllFolder<TitleCharaStateInfo>(AssetBundleNames.FursroomListPath, "fursroomcamerainfo", null, _subdirCheck: true))
		{
			foreach (TitleCharaStateInfo.Param p in item.param)
			{
				if (!dicCharaState.ContainsKey(p.pose))
				{
					dicCharaState[p.pose] = new List<TitleCharaStateInfo.Param>();
				}
				List<TitleCharaStateInfo.Param> list = dicCharaState[p.pose];
				TitleCharaStateInfo.Param param = list.Find((TitleCharaStateInfo.Param f) => f.id == p.id);
				if (param == null)
				{
					list.Add(new TitleCharaStateInfo.Param(p));
				}
				else
				{
					param.Copy(p);
				}
			}
		}
		yield return null;
	}

	public void SetCociergeModeCanvasGroup(int _index)
	{
		foreach (var (canvasGroup, num) in _cgConciergeModes.ToForEachTuples())
		{
			canvasGroup.Enable(num == _index, isUseInteractable: false);
		}
	}

	public void SetCharaAnimationAndPosition()
	{
		TitleCharaStateInfo.Param param = null;
		param = dicCharaState[Random.Range(0, 2)].Shuffle().FirstOrDefault();
		SetPosition(param);
		if (param != null && ctrl != null)
		{
			ctrl.PlayID(param.animationID);
		}
	}

	private void SetPosition(TitleCharaStateInfo.Param _param)
	{
		if (_param != null && !(ConciergeChaCtrl == null))
		{
			TransformCompositeInfoData transformCompositeInfoData = CommonLib.LoadAsset<TransformCompositeInfoData>(_param.camBundle, _param.camFile);
			if ((bool)transformCompositeInfoData && (bool)ConciergeChaCtrl && (bool)cam)
			{
				transformCompositeInfoData.Reflect(cam.transform, ConciergeChaCtrl.GetShapeBodyValue(0));
			}
			if ((bool)transformCompositeInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.camBundle, isUnloadForceRefCount: true);
			}
			TransformInfoData transformInfoData = CommonLib.LoadAsset<TransformInfoData>(_param.posBundle, _param.posFile);
			if ((bool)transformInfoData && (bool)ConciergeChaCtrl)
			{
				transformInfoData.Reflect(ConciergeChaCtrl.transform);
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.posBundle, isUnloadForceRefCount: true);
			}
			ConciergeChaCtrl.resetDynamicBoneAll = true;
		}
	}

	public void StartFade(bool _isForce = false)
	{
		if (!(crossFade == null) && (crossFade.isEnd || _isForce))
		{
			crossFade.FadeStart();
		}
	}

	public bool ConciergeAnimationPlay(int _id, bool _isSameCheck = true)
	{
		if (ConciergeChaCtrl == null)
		{
			return false;
		}
		ctrl.PlayID(homeConciergeInfoData.animIDs[_id]);
		return true;
	}

	public void SelectCountClear()
	{
		selectCount = 0;
		Singleton<Game>.Instance.isConciergeAngry = false;
	}

	public void AddSelectCount()
	{
		selectCount = Mathf.Min(conciergeButtonSelectCountMax + 1, selectCount + 1);
	}

	public bool IsAngry()
	{
		return selectCount >= conciergeButtonSelectCountMax;
	}

	public bool IsAngryCount()
	{
		return selectCount == conciergeButtonSelectCountMax;
	}
}
