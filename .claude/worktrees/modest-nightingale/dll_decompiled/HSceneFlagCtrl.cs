using System;
using System.Collections.Generic;
using AIChara;
using HS2;
using Illusion.CustomAttributes;
using Manager;
using UnityEngine;
using UnityEngine.UI;

public class HSceneFlagCtrl : Singleton<HSceneFlagCtrl>
{
	[Serializable]
	public struct LoopSpeeds
	{
		[Label("弱：最小")]
		public float minLoopSpeedW;

		[Label("弱：最大")]
		public float maxLoopSpeedW;

		[Label("強：最小")]
		public float minLoopSpeedS;

		[Label("強：最大")]
		public float maxLoopSpeedS;

		[Label("絶頂前：最小")]
		public float minLoopSpeedO;

		[Label("絶頂前：最大")]
		public float maxLoopSpeedO;
	}

	[Serializable]
	public struct MasterbationLoopSpeeds
	{
		[Label("弱：最小")]
		public float minLoopSpeedW;

		[Label("弱：最大")]
		public float maxLoopSpeedW;

		[Label("中：最小")]
		public float minLoopSpeedM;

		[Label("中：最大")]
		public float maxLoopSpeedM;

		[Label("強：最小")]
		public float minLoopSpeedS;

		[Label("強：最大")]
		public float maxLoopSpeedS;

		[Label("絶頂前：最小")]
		public float minLoopSpeedO;

		[Label("絶頂前：最大")]
		public float maxLoopSpeedO;
	}

	public enum ClickKind
	{
		None = -1,
		FinishBefore,
		FinishInSide,
		FinishOutSide,
		FinishSame,
		FinishDrink,
		FinishVomit,
		RecoverFaintness,
		Spnking,
		PeepingRestart,
		ParallelShiftInit,
		VerticalShiftInit,
		RotationShiftInit,
		LeaveItToYou,
		MovePointNext,
		MovePointBack,
		SceneEnd
	}

	public enum CharaCtrlKind
	{
		None = -1,
		Parallel,
		Height,
		Rotation
	}

	[Flags]
	public enum JudgeSelect
	{
		Kiss = 1,
		Kokan = 2,
		Breast = 4,
		Anal = 8,
		Pain = 0x10,
		Constraint = 0x20
	}

	[Serializable]
	public class VoiceFlag
	{
		public bool[] playVoices = new bool[2];

		public int[] playShorts = new int[2] { -1, -1 };

		public int oldFinish = -1;

		public int playStart = -1;

		public bool dialog;

		public bool[] urines = new bool[2];

		public bool urineFlag;

		public bool sleep;

		public int onaniEnterLoop;

		public int playStartOld = -1;

		public bool changeTaii;

		public List<AnimType> oldAnimType = new List<AnimType>();

		public List<AnimType> newAnimType = new List<AnimType>();

		public Transform[] voiceTrs = new Transform[2];

		public List<string> lstUseAsset = new List<string>();

		public void MemberInit()
		{
			playVoices = new bool[2];
			playShorts = new int[2] { -1, -1 };
			oldFinish = -1;
			playStart = -1;
			dialog = false;
			urines = new bool[2];
			urineFlag = false;
			sleep = false;
			onaniEnterLoop = 0;
			playStartOld = -1;
			changeTaii = false;
			voiceTrs = new Transform[2];
			lstUseAsset = new List<string>();
			oldAnimType = new List<AnimType>();
			newAnimType = new List<AnimType>();
		}
	}

	public enum AnimType
	{
		Anal,
		NameKuwae,
		MaleShot,
		MataIjiri,
		KokanSonyu,
		AnalIjiri,
		OrgasmF,
		OloopH,
		Sonyu,
		Urine,
		Kunni,
		Kiss,
		Inside,
		InsideMouth,
		FemaleMain,
		MaleMain,
		FemaleMainSonyu,
		MaleMainSonyu
	}

	public readonly int gotoFaintnessCount = 3;

	[Range(0f, 2f)]
	public float speed;

	[Tooltip("0:弱ループ 1:強ループ 2:絶頂前ループ 3:中ループ -1:その他")]
	public int loopType;

	[Range(0f, 1f)]
	public float[] motions;

	[Tooltip("現在の場所を示すID")]
	public int nPlace;

	[Tooltip("Hポイントのマーカーを識別するID")]
	public int HPointID;

	public HPoint nowHPoint;

	[Tooltip("0:Obi\u30001:パーティクル\u30002:なし")]
	public int semenType = 1;

	[SerializeField]
	private List<int> urineIDs = new List<int> { 3 };

	public float StartHouchiTime;

	public bool pointMoveAnimChange;

	public bool nowOrgasm;

	public float changeMotionTimeMin;

	public float changeMotionTimeMax;

	public AnimationCurve changeMotionCurve;

	public float changeMotionMinRate;

	[Range(0f, 1f)]
	public float feel_f;

	[Range(0f, 1f)]
	public float feel_m;

	[Range(0f, 1f)]
	public float speedGuageRate = 0.01f;

	[Tooltip("ホイール一回でどれだけ回したことにするか")]
	public float wheelActionCount = 0.05f;

	[Tooltip("どの汁を制御するか")]
	public string ctrlSiru;

	public int ctrlSiruType;

	public bool isInsert;

	public float changeAutoMotionTimeMin;

	public float changeAutoMotionTimeMax;

	[Range(0f, 1f)]
	public float guageDecreaseRate;

	[Range(0f, 1f)]
	public float feelSpnking;

	[Range(0f, 1f)]
	public float siriakaDecreaseRate;

	[Range(0f, 1f)]
	public float siriakaAddRate;

	public float timeMasturbationChangeSpeed;

	public List<int>[,] lstSyncAnimLayers = new List<int>[2, 2];

	public CameraControl_Ver2 cameraCtrl;

	private InputField nowInputForcus;

	public Light Light;

	public Light[] SubLights = new Light[2];

	public int categoryMotionList = -1;

	public string atariTamaName;

	public int peepingLoopNumY = 3;

	public int peepingOutLoopNumY = 2;

	public int peepingLoopNumW = 3;

	public int peepingOutLoopNumW = 2;

	public float peepingFadeY = 0.62f;

	public float peepingFadeW = 0.75f;

	public LoopSpeeds loopSpeeds = new LoopSpeeds
	{
		minLoopSpeedW = 1f,
		maxLoopSpeedW = 1.6f,
		minLoopSpeedS = 1.4f,
		maxLoopSpeedS = 2f,
		minLoopSpeedO = 1.4f,
		maxLoopSpeedO = 2f
	};

	public MasterbationLoopSpeeds masterbationSpeeds = new MasterbationLoopSpeeds
	{
		minLoopSpeedW = 1f,
		maxLoopSpeedW = 1.6f,
		minLoopSpeedS = 1.4f,
		maxLoopSpeedS = 2f,
		minLoopSpeedO = 1.4f,
		maxLoopSpeedO = 2f,
		minLoopSpeedM = 1f,
		maxLoopSpeedM = 1.6f
	};

	private HScene.AnimationListInfo _selectAnimationListInfo;

	[SerializeField]
	public HScene.AnimationListInfo nowAnimationInfo = new HScene.AnimationListInfo();

	public bool stopFeelFemale;

	public bool stopFeelMale;

	public ClickKind click;

	public CharaCtrlKind kindCharaCtrl = CharaCtrlKind.None;

	[Range(0.001f, 1f)]
	public float charaMoveSpeed = 0.05f;

	[Range(0.5f, 3f)]
	public float charaMoveSpeedAddRate = 1f;

	[Range(0.001f, 10f)]
	public float charaRotaionSpeed = 4f;

	public bool isFaintness;

	public bool isFaintnessVoice;

	public int FaintnessType = -1;

	public int initiative;

	public bool isLeaveItToYou;

	public bool isAutoActionChange;

	public bool isGaugeHit;

	public bool isGaugeHit_M;

	public bool nowSpeedStateFast;

	[Range(0f, 1f)]
	public float rateTuya;

	[Range(0f, 1f)]
	public float rateNipMax = 0.3f;

	[Range(0f, 1f)]
	public float rateNip;

	[Range(0f, 1f)]
	public float rateWet;

	public float feelPain;

	private Dictionary<int, Dictionary<int, int>> EndAddTaiiParam = new Dictionary<int, Dictionary<int, int>>();

	private List<GameParameterInfo.Param> SendParam = new List<GameParameterInfo.Param>();

	private GameParameterInfo.Param SendResultParam;

	private bool[] mindJudge = new bool[4];

	private Dictionary<int, Dictionary<int, int[]>> FinishResistTaii = new Dictionary<int, Dictionary<int, int[]>>();

	public int numOrgasm;

	public int numOrgasmSecond;

	public int numOrgasmTotal;

	[Tooltip("通常絶頂回数もカウントされます 同時絶頂したら、こいつも+1、絶頂回数も+1、中出しか外出しも+1")]
	public int numSameOrgasm;

	public int numInside;

	public int numOutSide;

	public int numDrink;

	public int numVomit;

	public int numOrgasmM2;

	public int numShotM2;

	public int numOrgasmF2;

	public int numShotF2;

	public int numUrine;

	public int numFaintness;

	[EnumFlags]
	public List<JudgeSelect> isJudgeSelect = new List<JudgeSelect>();

	public bool isNotCtrl = true;

	public bool isPainAction;

	public bool isUrine;

	public VoiceFlag voice = new VoiceFlag();

	public List<int> UrineIDs => urineIDs;

	public bool inputForcus
	{
		get
		{
			if (!(nowInputForcus != null))
			{
				return false;
			}
			return nowInputForcus.isFocused;
		}
	}

	public HScene.AnimationListInfo selectAnimationListInfo
	{
		get
		{
			return _selectAnimationListInfo;
		}
		set
		{
			_selectAnimationListInfo = value;
		}
	}

	public void AddOrgasm()
	{
		numOrgasmTotal = Mathf.Clamp(numOrgasmTotal + 1, 0, 999999);
	}

	public void AddTaiiParam()
	{
		if (nowAnimationInfo != null)
		{
			int item = nowAnimationInfo.ActionCtrl.Item1;
			int id = nowAnimationInfo.id;
			int parmID = nowAnimationInfo.ParmID;
			if (!EndAddTaiiParam.ContainsKey(item))
			{
				EndAddTaiiParam.Add(item, new Dictionary<int, int>());
			}
			if (!EndAddTaiiParam[item].ContainsKey(id))
			{
				EndAddTaiiParam[item].Add(id, parmID);
				SetMindJudge();
			}
		}
	}

	public void AddFinishResistTaii(int finish)
	{
		if (nowAnimationInfo == null)
		{
			return;
		}
		int item = nowAnimationInfo.ActionCtrl.Item1;
		int id = nowAnimationInfo.id;
		if (!FinishResistTaii.ContainsKey(item))
		{
			FinishResistTaii.Add(item, new Dictionary<int, int[]>());
		}
		if (!FinishResistTaii[item].ContainsKey(id))
		{
			int num = 0;
			if (nowAnimationInfo.lstSystem.Contains(4))
			{
				num = 2;
			}
			else if (nowAnimationInfo.lstSystem.Contains(3))
			{
				num = 1;
			}
			FinishResistTaii[item].Add(id, new int[2] { finish, num });
		}
	}

	public void EndSetAddTaiiParam(ChaFileGameInfo2 chaInfo)
	{
		HSceneManager.HSceneTables hResourceTables = HSceneManager.HResourceTables;
		if (hResourceTables == null)
		{
			return;
		}
		HTaiiParam.Param value = null;
		foreach (Dictionary<int, int> value2 in EndAddTaiiParam.Values)
		{
			foreach (int value3 in value2.Values)
			{
				if (hResourceTables.taii.TryGetValue(value3, out value))
				{
					bool flag = false;
					if (chaInfo.resistH >= 100)
					{
						flag = chaInfo.nowState switch
						{
							ChaFileDefine.State.Favor => chaInfo.Favor >= 50, 
							ChaFileDefine.State.Enjoyment => chaInfo.Enjoyment >= 50, 
							ChaFileDefine.State.Slavery => chaInfo.Slavery >= 50, 
							_ => false, 
						};
					}
					if (flag)
					{
						SendParam.Add(new GameParameterInfo.Param
						{
							favor = value.favor,
							enjoyment = value.enjoyment,
							slavery = value.slavery,
							aversion = value.aversion,
							broken = value.broken,
							dependence = value.dependence
						});
					}
					else
					{
						SendParam.Add(new GameParameterInfo.Param
						{
							favor = value.favor,
							enjoyment = value.enjoyment,
							slavery = value.slavery,
							aversion = value.aversion,
							broken = value.broken
						});
					}
				}
			}
		}
	}

	public void EndSetAddTraitParam(int trait)
	{
		HSceneManager.HSceneTables hResourceTables = HSceneManager.HResourceTables;
		if (hResourceTables != null)
		{
			GameParameterInfo.Param value = null;
			if (hResourceTables.trait.TryGetValue(trait, out value))
			{
				SendParam.Add(value);
			}
		}
	}

	public void EndSetAddMindParam(int mind)
	{
		HSceneManager.HSceneTables hResourceTables = HSceneManager.HResourceTables;
		if (hResourceTables == null)
		{
			return;
		}
		HSceneManager hSceneManager = Singleton<HSceneManager>.Instance;
		switch (mind)
		{
		case 1:
			if (hSceneManager.maleFinish == 0 && hSceneManager.femaleFinish == 0)
			{
				return;
			}
			break;
		case 2:
			if (hSceneManager.maleFinish == 0)
			{
				return;
			}
			break;
		case 3:
			if (hSceneManager.femaleFinish == 0)
			{
				return;
			}
			break;
		case 4:
		case 5:
		case 6:
		case 7:
			if (!mindJudge[mind - 4])
			{
				return;
			}
			break;
		case 8:
			if (numOutSide == 0)
			{
				return;
			}
			break;
		case 10:
			if (numSameOrgasm == 0)
			{
				return;
			}
			break;
		}
		GameParameterInfo.Param value = null;
		if (hResourceTables.mind.TryGetValue(mind, out value))
		{
			SendParam.Add(value);
		}
	}

	public void ParamCalc(ChaFileGameInfo2 chaInfo, int personal)
	{
		GlobalHS2Calc.CalcParameterH(SendParam, chaInfo, personal);
		GlobalHS2Calc.CalcState(chaInfo, personal);
	}

	public void AfterParamCalc(int id, ChaFileGameInfo2 chaInfo, int personal)
	{
		HSceneManager.HSceneTables hResourceTables = HSceneManager.HResourceTables;
		if (hResourceTables == null)
		{
			return;
		}
		HResultParam.Param value = null;
		if (hResourceTables.result.TryGetValue(id, out value))
		{
			SendResultParam = new GameParameterInfo.Param
			{
				favor = value.favor,
				enjoyment = value.enjoyment,
				slavery = value.slavery,
				aversion = value.aversion,
				broken = value.broken,
				dependence = value.dependence
			};
			GlobalHS2Calc.CalcParameterH(SendResultParam, chaInfo, personal);
			chaInfo.resistH += UnityEngine.Random.Range(value.hResist.min, value.hResist.max + 1);
			chaInfo.resistAnal += UnityEngine.Random.Range(value.analResist.min, value.analResist.max + 1);
			chaInfo.resistPain += UnityEngine.Random.Range(value.painResist.min, value.painResist.max + 1);
			if (chaInfo.resistH > 100)
			{
				chaInfo.resistH = 100;
			}
			if (chaInfo.resistAnal > 100)
			{
				chaInfo.resistAnal = 100;
			}
			if (chaInfo.resistPain > 100)
			{
				chaInfo.resistPain = 100;
			}
		}
	}

	public void FinishResistParamCalc(ChaFileGameInfo2 chaInfo, int Hattribute, int personal)
	{
		HSceneManager.HSceneTables hResourceTables = HSceneManager.HResourceTables;
		if (hResourceTables == null)
		{
			return;
		}
		Dictionary<int, int[]> value = null;
		if (!hResourceTables.resist.TryGetValue(personal, out value))
		{
			return;
		}
		foreach (KeyValuePair<int, Dictionary<int, int[]>> item in FinishResistTaii)
		{
			foreach (KeyValuePair<int, int[]> item2 in item.Value)
			{
				if (item2.Value.Length != 0)
				{
					AddResistTaiiFinish(item2.Value, chaInfo, Hattribute, value);
				}
			}
		}
		int num = Mathf.FloorToInt(feelPain * 100f * 0.05f);
		chaInfo.resistPain += num;
		if (chaInfo.resistPain > 100)
		{
			chaInfo.resistPain = 100;
		}
	}

	private void AddResistTaiiFinish(int[] value, ChaFileGameInfo2 chaInfo, int Hattribute, Dictionary<int, int[]> judge)
	{
		if (judge.ContainsKey(value[0]) && judge[value[0]].Length != 0 && judge[value[0]].Length > value[1])
		{
			int value2 = judge[value[0]][value[1]];
			if (value[1] != 2)
			{
				AddResistCalc(value2, chaInfo, value[1]);
			}
			else if (Hattribute == 3)
			{
				AddResistCalc(value2, chaInfo, value[1]);
			}
		}
	}

	private void AddResistCalc(int value, ChaFileGameInfo2 chaInfo, int mode)
	{
		switch (mode)
		{
		case 0:
			chaInfo.resistH += value;
			if (chaInfo.resistH > 100)
			{
				chaInfo.resistH = 100;
			}
			break;
		case 1:
			chaInfo.resistAnal += value;
			if (chaInfo.resistAnal > 100)
			{
				chaInfo.resistAnal = 100;
			}
			break;
		case 2:
			chaInfo.resistPain += value;
			if (chaInfo.resistPain > 100)
			{
				chaInfo.resistPain = 100;
			}
			break;
		}
	}

	private void SetMindJudge()
	{
		mindJudge[0] = nowAnimationInfo.lstSystem.Contains(2);
		mindJudge[1] = nowAnimationInfo.lstSystem.Contains(3);
		mindJudge[2] = nowAnimationInfo.lstSystem.Contains(1);
		mindJudge[3] = nowAnimationInfo.lstSystem.Contains(0);
	}

	public void SelectInputField(InputField _input)
	{
		nowInputForcus = _input;
	}

	public void DeselectInputField(InputField _input)
	{
		if (nowInputForcus == _input)
		{
			nowInputForcus = null;
		}
	}

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		isNotCtrl = true;
		isFaintness = false;
		FaintnessType = -1;
		lstSyncAnimLayers[0, 0] = new List<int>();
		lstSyncAnimLayers[0, 1] = new List<int>();
		lstSyncAnimLayers[1, 0] = new List<int>();
		lstSyncAnimLayers[1, 1] = new List<int>();
		voice.playShorts = new int[2] { -1, -1 };
		isUrine = false;
		feelPain = 0f;
	}

	public void EndProc()
	{
		nowAnimationInfo = new HScene.AnimationListInfo();
		selectAnimationListInfo = null;
		feel_f = 0f;
		feel_m = 0f;
		initiative = 0;
		isLeaveItToYou = false;
		isAutoActionChange = false;
		isGaugeHit = false;
		isGaugeHit_M = false;
		nowSpeedStateFast = false;
		speed = 0f;
		for (int i = 0; i < motions.Length; i++)
		{
			motions[i] = 0f;
		}
		nPlace = -1;
		HPointID = -1;
		nowHPoint = null;
		stopFeelFemale = false;
		stopFeelMale = false;
		isFaintness = false;
		FaintnessType = -1;
		rateTuya = 0f;
		rateNip = 0f;
		numOrgasm = 0;
		numOrgasmTotal = 0;
		numSameOrgasm = 0;
		numInside = 0;
		numOutSide = 0;
		numDrink = 0;
		numVomit = 0;
		numOrgasmM2 = 0;
		numShotM2 = 0;
		numOrgasmF2 = 0;
		numShotF2 = 0;
		numUrine = 0;
		numFaintness = 0;
		isJudgeSelect.Clear();
		isNotCtrl = true;
		nowOrgasm = false;
		isUrine = false;
		voice.MemberInit();
		EndAddTaiiParam.Clear();
		SendParam.Clear();
		SendResultParam = null;
		mindJudge = new bool[4];
		FinishResistTaii.Clear();
		feelPain = 0f;
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < 2; k++)
			{
				lstSyncAnimLayers[j, k].Clear();
			}
		}
		nowInputForcus = null;
	}
}
