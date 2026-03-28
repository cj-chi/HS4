using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ADV;
using AIChara;
using Actor;
using CameraEffector;
using CharaCustom;
using HS2;
using Illusion.Anime;
using Illusion.Extensions;
using UniRx;
using UnityEngine;

namespace Manager;

public class LobbySceneManager : Singleton<LobbySceneManager>
{
	public enum LobbyMode
	{
		Main,
		MapSelect
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

		public int personal { get; set; }

		public override List<Program.Transfer> Create()
		{
			List<Program.Transfer> list = base.Create();
			list.Add(Program.Transfer.VAR("int", "personal", personal.ToString()));
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
	private CanvasGroup _cgMain;

	[SerializeField]
	private List<CanvasGroup> _cgModes = new List<CanvasGroup>();

	[SerializeField]
	private CanvasGroup _cgSelect;

	[SerializeField]
	private CanvasGroup cgParameter;

	[SerializeField]
	private LobbyMainUI mainUI;

	[SerializeField]
	private LobbySelectUI selectUI;

	[SerializeField]
	private LobbyParameterUI parameterUI;

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private GameObject objDof;

	[SerializeField]
	private ConfigEffectorWet configEffector;

	[SerializeField]
	private CrossFade crossFade;

	[SerializeField]
	private ObjectCategoryBehaviour _ocbTutorial;

	private bool isADVShow;

	private Controller[] ctrls = new Controller[2];

	private Controller ConciergeCtrl;

	private ValueDictionary<int, int, List<TitleCharaStateInfo.Param>> dicCharaState = new ValueDictionary<int, int, List<TitleCharaStateInfo.Param>>();

	private ValueDictionary<int, int, int, List<TitleCharaStateInfo.Param>> dicCharaState2 = new ValueDictionary<int, int, int, List<TitleCharaStateInfo.Param>>();

	private bool _isInitialize;

	private PackData packData { get; set; }

	public CanvasGroup CGMain => _cgMain;

	public List<CanvasGroup> CGModes => _cgModes;

	public CanvasGroup CGParameter => cgParameter;

	public LobbyMainUI MainUI => mainUI;

	public LobbySelectUI SelectUI => selectUI;

	public LobbyParameterUI ParameterUI => parameterUI;

	public ObjectCategoryBehaviour OCBTutorial => _ocbTutorial;

	public bool IsADVShow => isADVShow;

	public bool isEntry { get; set; }

	public Heroine[] heroines { get; set; } = new Heroine[2];

	public int[] heroineRommListIdx { get; set; } = new int[2] { -1, -1 };

	public ChaControl ConciergeChaCtrl { get; private set; }

	public Heroine ConciergeHeroine { get; private set; }

	public int[] eventNos { get; set; }

	public ValueDictionary<int, int, List<TitleCharaStateInfo.Param>> CharaStateTable => dicCharaState;

	public ValueDictionary<int, int, int, List<TitleCharaStateInfo.Param>> CharaStateTable2 => dicCharaState2;

	public bool IsInitialize => _isInitialize;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<AssetBundleManager>.IsInstance());
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		yield return StartCoroutine(LoadList());
		Game game = Singleton<Game>.Instance;
		SaveData saveData = game.saveData;
		eventNos = Enumerable.Repeat(-1, saveData.roomList[saveData.selectGroup].Count).ToArray();
		if (!MathfEx.IsRange(28, Singleton<ADVManager>.Instance.advDelivery.adv_category, 32, isEqual: true))
		{
			game.tableDesireCharas.Clear();
			foreach (var (text, num) in saveData.roomList[saveData.selectGroup].ToForEachTuples())
			{
				if (new ChaFileControl().LoadCharaFile(text, 1) && game.tableLobbyEvents[saveData.selectGroup].TryGetValue(text, out var value))
				{
					eventNos[num] = value.eventID;
				}
				if (Game.DesireEventIDs.Contains(eventNos[num]))
				{
					game.tableDesireCharas.Add(text, eventNos[num]);
				}
			}
			game.heroineList.Clear();
			game.player = null;
			heroines[0] = null;
			CGParameter.Enable(enable: false, isUseInteractable: false);
		}
		else
		{
			int num2 = SaveData.FindInRoomListIndex(Path.GetFileNameWithoutExtension(game.heroineList[0].chaFile.charaFileName));
			Singleton<Character>.Instance.DeleteChara(Singleton<Character>.Instance.GetChara(99));
			game.player = null;
			heroineRommListIdx[0] = num2;
			if (game.heroineList[0] != null)
			{
				heroines[0] = game.heroineList[0];
				heroines[0].chaCtrl.ChangeLookEyesPtn(1);
				heroines[0].chaCtrl.ChangeLookEyesTarget(0);
				heroines[0].chaCtrl.ChangeLookNeckPtn(1);
				heroines[0].chaCtrl.ChangeLookNeckTarget(0);
				ctrls[0] = new Controller(heroines[0].chaCtrl);
				ParameterUI.SetParameter(heroines[0].chaFile, -1, 0);
			}
			else
			{
				LoadChara(0, Singleton<Game>.Instance.customCharaFileName);
			}
			if (num2 != -1)
			{
				if (MathfEx.IsRange(28, Singleton<ADVManager>.Instance.advDelivery.adv_category, 29, isEqual: true))
				{
					eventNos[num2] = game.eventNo;
				}
				else if (MathfEx.IsRange(30, Singleton<ADVManager>.Instance.advDelivery.adv_category, 31, isEqual: true))
				{
					if (game.peepKind == 5)
					{
						eventNos[num2] = game.eventNo;
					}
					else if (game.peepKind == 6)
					{
						eventNos[num2] = -1;
					}
				}
				else
				{
					eventNos[num2] = -1;
				}
			}
			SetCharaDOFObject(heroines[0]);
			SetCharaAnimationAndPosition();
			CGParameter.Enable(enable: true, isUseInteractable: false);
		}
		heroines[1] = null;
		Sound.Listener = cam.transform;
		isNowADV.Subscribe(delegate(bool _isADV)
		{
			CGMain.Enable(!_isADV);
		});
		yield return StartCoroutine(LoadConciergeBody());
		_isInitialize = true;
	}

	private IEnumerator LoadList()
	{
		foreach (TitleCharaStateInfo item in GlobalMethod.LoadAllFolder<TitleCharaStateInfo>(AssetBundleNames.LobbyListPath, "lobbycharapos", null, _subdirCheck: true))
		{
			foreach (TitleCharaStateInfo.Param p in item.param)
			{
				if (p.number == -1)
				{
					if (!dicCharaState.ContainsKey(p.pose))
					{
						dicCharaState[p.pose] = dicCharaState.New();
					}
					ValueDictionary<int, List<TitleCharaStateInfo.Param>> valueDictionary = dicCharaState[p.pose];
					if (!valueDictionary.ContainsKey(p.state))
					{
						valueDictionary[p.state] = new List<TitleCharaStateInfo.Param>();
					}
					List<TitleCharaStateInfo.Param> list = valueDictionary[p.state];
					TitleCharaStateInfo.Param param = list.Find((TitleCharaStateInfo.Param f) => f.id == p.id);
					if (param == null)
					{
						list.Add(new TitleCharaStateInfo.Param(p));
					}
					else
					{
						param.Copy(p);
					}
					continue;
				}
				if (!dicCharaState2.ContainsKey(p.number))
				{
					dicCharaState2[p.number] = dicCharaState2.New();
				}
				ValueDictionary<int, int, List<TitleCharaStateInfo.Param>> valueDictionary2 = dicCharaState2[p.number];
				if (!valueDictionary2.ContainsKey(p.pose))
				{
					valueDictionary2[p.pose] = valueDictionary2.New();
				}
				ValueDictionary<int, List<TitleCharaStateInfo.Param>> valueDictionary3 = valueDictionary2[p.pose];
				if (!valueDictionary3.ContainsKey(p.state))
				{
					valueDictionary3[p.state] = new List<TitleCharaStateInfo.Param>();
				}
				List<TitleCharaStateInfo.Param> list2 = valueDictionary3[p.state];
				TitleCharaStateInfo.Param param2 = list2.Find((TitleCharaStateInfo.Param f) => f.id == p.id);
				if (param2 == null)
				{
					list2.Add(new TitleCharaStateInfo.Param(p));
				}
				else
				{
					param2.Copy(p);
				}
			}
		}
		yield return null;
	}

	private IEnumerator LoadConciergeBody()
	{
		new ChaFileControl();
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
		ConciergeChaCtrl.ChangeLookNeckPtn(3);
		ConciergeCtrl = new Controller(ConciergeChaCtrl);
		ConciergeHeroine.param.Bind(new global::Actor.Actor(ConciergeHeroine));
		ConciergeChaCtrl.visibleAll = false;
		yield return null;
	}

	public void SetSelectCanvasGroup(bool _enable)
	{
		_cgSelect.Enable(_enable);
	}

	public void SetModeCanvasGroup(int _index)
	{
		foreach (var (canvasGroup, num) in _cgModes.ToForEachTuples())
		{
			if (canvasGroup != null)
			{
				canvasGroup.Enable(num == _index, isUseInteractable: false);
			}
		}
	}

	public void StartFade(bool _isForce = false)
	{
		if (!(crossFade == null) && (crossFade.isEnd || _isForce))
		{
			crossFade.FadeStart();
		}
	}

	public void LoadChara(int _no, string _file, bool _isDeleteOnly = false)
	{
		if (heroines[_no] != null)
		{
			Singleton<Character>.Instance.DeleteChara(heroines[_no].chaCtrl);
		}
		heroines[_no] = null;
		if (!_isDeleteOnly && !_file.IsNullOrEmpty())
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			chaFileControl.LoadCharaFile(_file, 1);
			GameObject parent = ((_no == 0) ? Scene.commonSpace : null);
			ChaControl chaControl = Singleton<Character>.Instance.CreateChara(1, parent, _no, chaFileControl);
			chaControl.ChangeNowCoordinate();
			chaControl.releaseCustomInputTexture = false;
			chaControl.Load();
			chaControl.ChangeLookEyesPtn(1);
			chaControl.ChangeLookNeckPtn(1);
			ctrls[_no] = new Controller(chaControl);
			heroines[_no] = new Heroine(chaFileControl, isRandomize: false);
			heroines[_no].SetRoot(chaControl.gameObject);
			heroines[_no].param.Bind(new global::Actor.Actor(heroines[_no]));
			if (_no != 1)
			{
				SetCharaDOFObject(heroines[_no]);
			}
			heroineRommListIdx[_no] = SaveData.FindInRoomListIndex(Path.GetFileNameWithoutExtension(_file));
		}
	}

	public void SetCharaDOFObject(Heroine _heroine)
	{
		if (_heroine != null && !(_heroine.chaCtrl == null))
		{
			GameObject referenceInfo = _heroine.chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.HeadParent);
			if ((bool)referenceInfo)
			{
				configEffector.dof.focalTransform = referenceInfo.transform;
			}
		}
	}

	public void SetCharaAnimationAndPosition()
	{
		TitleCharaStateInfo.Param param = null;
		if (heroines[0] != null && heroines[0].chaCtrl != null && heroines[1] != null && heroines[1].chaCtrl != null)
		{
			float t = (heroines[0].chaCtrl.GetShapeBodyValue(0) + heroines[1].chaCtrl.GetShapeBodyValue(0)) * 0.5f;
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				param = dicCharaState2[0][0][-1][0];
				SetCameraPosition(param, t);
				param = dicCharaState2[0][0][(int)heroines[0].gameinfo2.nowDrawState][0];
				SetMotionIKAndAnimationPlay(param, heroines[0], ctrls[0]);
				SetCharaPosition(param, heroines[0]);
				param = dicCharaState2[1][0][(int)heroines[1].gameinfo2.nowDrawState][0];
				SetMotionIKAndAnimationPlay(param, heroines[1], ctrls[1]);
				SetCharaPosition(param, heroines[1]);
			}
			else
			{
				param = dicCharaState2[0][1][-1][0];
				SetCameraPosition(param, t);
				param = dicCharaState2[0][1][(heroines[0].gameinfo2.nowDrawState == ChaFileDefine.State.Broken) ? 1 : 0][0];
				SetMotionIKAndAnimationPlay(param, heroines[0], ctrls[0]);
				SetCharaPosition(param, heroines[0]);
				param = dicCharaState2[1][1][(heroines[1].gameinfo2.nowDrawState == ChaFileDefine.State.Broken) ? 1 : 0][0];
				SetMotionIKAndAnimationPlay(param, heroines[1], ctrls[1]);
				SetCharaPosition(param, heroines[1]);
			}
		}
		else
		{
			param = ((heroines[0] == null || !(heroines[0].chaCtrl != null)) ? dicCharaState[-1][-1].Shuffle().FirstOrDefault() : ((UnityEngine.Random.Range(0, 2) != 0) ? dicCharaState[1][(heroines[0].gameinfo2.nowDrawState == ChaFileDefine.State.Broken) ? 1 : 0].Shuffle().FirstOrDefault() : dicCharaState[0][(int)heroines[0].gameinfo2.nowDrawState].Shuffle().FirstOrDefault()));
			SetMotionIKAndAnimationPlay(param, heroines[0], ctrls[0]);
			SetCameraAndCharaPosition(param);
		}
	}

	private void SetMotionIKAndAnimationPlay(TitleCharaStateInfo.Param _param, Heroine _heroine, Controller _ctrl)
	{
		if (_heroine == null)
		{
			return;
		}
		ChaControl chaCtrl = _heroine.chaCtrl;
		if (!(chaCtrl == null) && _param != null)
		{
			if (!PersonalPoseInfoTable.InfoTable.TryGetValue(_heroine.voiceNo, out var value))
			{
				_ctrl.PlayID(0);
				return;
			}
			if (!value.TryGetValue(_param.animationID, out var value2))
			{
				_ctrl.PlayID(0);
				return;
			}
			_ctrl.PlayID(value2.poseIDs.Shuffle().FirstOrDefault());
			Singleton<Game>.Instance.GetExpression(_heroine.voiceNo, value2.exp)?.Change(chaCtrl);
			int ptn = ((_heroine.gameinfo2.nowDrawState != ChaFileDefine.State.Broken) ? 1 : 3);
			_heroine.chaCtrl.ChangeLookNeckPtn(ptn);
			_heroine.chaCtrl.ChangeLookEyesPtn(ptn);
		}
	}

	private void SetCameraAndCharaPosition(TitleCharaStateInfo.Param _param)
	{
		if (_param == null)
		{
			return;
		}
		if (_param.id == -1)
		{
			TransformInfoData transformInfoData = CommonLib.LoadAsset<TransformInfoData>(_param.camBundle, _param.camFile);
			if ((bool)transformInfoData && (bool)cam)
			{
				transformInfoData.Reflect(cam.transform);
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.camBundle, isUnloadForceRefCount: true);
			}
			transformInfoData = CommonLib.LoadAsset<TransformInfoData>(_param.posBundle, _param.posFile);
			if ((bool)transformInfoData && (bool)objDof)
			{
				transformInfoData.Reflect(objDof.transform);
				configEffector.dof.focalTransform = objDof.transform;
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.posBundle, isUnloadForceRefCount: true);
			}
		}
		else
		{
			if (heroines[0] == null)
			{
				return;
			}
			ChaControl chaCtrl = heroines[0].chaCtrl;
			if (!(chaCtrl == null))
			{
				TransformCompositeInfoData transformCompositeInfoData = CommonLib.LoadAsset<TransformCompositeInfoData>(_param.camBundle, _param.camFile);
				if ((bool)transformCompositeInfoData && (bool)chaCtrl && (bool)cam)
				{
					transformCompositeInfoData.Reflect(cam.transform, chaCtrl.GetShapeBodyValue(0));
				}
				if ((bool)transformCompositeInfoData)
				{
					AssetBundleManager.UnloadAssetBundle(_param.camBundle, isUnloadForceRefCount: true);
				}
				TransformInfoData transformInfoData2 = CommonLib.LoadAsset<TransformInfoData>(_param.posBundle, _param.posFile);
				if ((bool)transformInfoData2 && (bool)chaCtrl)
				{
					transformInfoData2.Reflect(chaCtrl.transform);
				}
				if ((bool)transformInfoData2)
				{
					AssetBundleManager.UnloadAssetBundle(_param.posBundle, isUnloadForceRefCount: true);
				}
				chaCtrl.resetDynamicBoneAll = true;
			}
		}
	}

	private void SetCameraPosition(TitleCharaStateInfo.Param _param, float _t)
	{
		if (_param != null)
		{
			TransformCompositeInfoData transformCompositeInfoData = CommonLib.LoadAsset<TransformCompositeInfoData>(_param.camBundle, _param.camFile);
			if ((bool)transformCompositeInfoData && (bool)cam)
			{
				transformCompositeInfoData.Reflect(cam.transform, _t);
			}
			if ((bool)transformCompositeInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.camBundle, isUnloadForceRefCount: true);
			}
		}
	}

	private void SetCharaPosition(TitleCharaStateInfo.Param _param, Heroine _heroine)
	{
		if (_param == null || _heroine == null)
		{
			return;
		}
		ChaControl chaCtrl = _heroine.chaCtrl;
		if (!(chaCtrl == null))
		{
			TransformInfoData transformInfoData = CommonLib.LoadAsset<TransformInfoData>(_param.posBundle, _param.posFile);
			if ((bool)transformInfoData && (bool)chaCtrl)
			{
				transformInfoData.Reflect(chaCtrl.transform);
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.posBundle, isUnloadForceRefCount: true);
			}
			chaCtrl.resetDynamicBoneAll = true;
		}
	}

	public async void OpenADV(string _bundle, string _asset, Heroine _herone, Action _onEnd = null)
	{
		await Setup.LoadAsync(base.transform);
		isNowADV.Value = true;
		Game game = Singleton<Game>.Instance;
		packData = new PackData();
		packData.SetCommandData(game.saveData);
		packData.SetParam(ConciergeHeroine, _herone);
		packData.personal = _herone.personality;
		packData.isParent = true;
		openData.bundle = _bundle;
		openData.asset = _asset;
		packData.onComplete = delegate
		{
			isNowADV.Value = false;
			isADVShow = true;
			_onEnd?.Invoke();
			Controller.Table.Get(ConciergeChaCtrl).itemHandler.DisableItems();
			StartFade();
			SetCharaAnimationAndPosition();
		};
		Setup.Open(openData, packData, _isCameraPosDontMove: true);
	}
}
