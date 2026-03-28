using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AIChara;
using Illusion.CustomAttributes;
using Manager;
using UnityEngine;

public class HVoiceCtrl : MonoBehaviour
{
	[Serializable]
	public class FaceInfo
	{
		[RangeLabel("目の開き", 0f, 1f)]
		public float openEye = 10f;

		[RangeLabel("口の開き最小", 0f, 1f)]
		public float openMouthMin;

		[RangeLabel("口の開き最大", 0f, 1f)]
		public float openMouthMax = 1f;

		[Label("眉の形")]
		public int eyeBlow;

		[Label("目の形")]
		public int eye;

		[Label("口の形")]
		public int mouth;

		[RangeLabel("涙", 0f, 1f)]
		public float tear;

		[RangeLabel("頬赤", 0f, 1f)]
		public float cheek;

		[Label("ハイライト")]
		public bool highlight;

		[Label("瞬き")]
		public bool blink;

		[Label("首挙動")]
		public int behaviorNeckLine;

		[Label("目挙動")]
		public int behaviorEyeLine;

		[Label("首タゲ")]
		public int targetNeckLine;

		[Label("目タゲ")]
		public int targetEyeLine;

		[Label("視線角度")]
		public Vector3[] EyeRot = new Vector3[2];

		[Label("首角度")]
		public Vector3[] NeckRot = new Vector3[2];

		[Label("頭角度")]
		public Vector3[] HeadRot = new Vector3[2];
	}

	[Serializable]
	public class VoiceListInfo
	{
		[Label("ID")]
		public int id;

		[Label("ファイル名")]
		public string nameFile = "";

		[Label("アセットバンドルパス")]
		public string pathAsset = "";

		[Label("セリフの種類")]
		public int voiceKind;

		[Label("上書き禁止フラグ")]
		public bool notOverWrite;

		[Label("喋った(セリフ時のみ)")]
		public bool isPlay;

		[Label("呼吸グループ(呼吸時のみ)")]
		public int group = -1;

		[Label("おしっこセリフ(呼吸時のみ)")]
		public bool urine;

		[Label("アタリにあたっていない")]
		public List<int> lstNotHitFace = new List<int>();

		[Label("アタリにあたっている")]
		public List<int> lstHitFace = new List<int>();

		[Label("セリフ")]
		public string word = "";
	}

	[Serializable]
	public class BreathList
	{
		[Serializable]
		private struct InspectorBreathList
		{
			public int state;

			public int key;

			public VoiceListInfo value;
		}

		public Dictionary<int, Dictionary<int, VoiceListInfo>> dicdicVoiceList = new Dictionary<int, Dictionary<int, VoiceListInfo>>();

		[SerializeField]
		private List<InspectorBreathList> debugList = new List<InspectorBreathList>();

		public void DebugListSet()
		{
			foreach (KeyValuePair<int, Dictionary<int, VoiceListInfo>> dicdicVoice in dicdicVoiceList)
			{
				int key = dicdicVoice.Key;
				foreach (KeyValuePair<int, VoiceListInfo> item in dicdicVoice.Value)
				{
					debugList.Add(new InspectorBreathList
					{
						state = key,
						key = item.Key,
						value = item.Value
					});
				}
			}
		}
	}

	[Serializable]
	public class VoiceList
	{
		public Dictionary<int, VoiceListInfo>[] dicdicVoiceList = new Dictionary<int, VoiceListInfo>[7]
		{
			new Dictionary<int, VoiceListInfo>(),
			new Dictionary<int, VoiceListInfo>(),
			new Dictionary<int, VoiceListInfo>(),
			new Dictionary<int, VoiceListInfo>(),
			new Dictionary<int, VoiceListInfo>(),
			new Dictionary<int, VoiceListInfo>(),
			new Dictionary<int, VoiceListInfo>()
		};
	}

	[Serializable]
	public struct PlayVoiceinfo
	{
		public int mode;

		public int kind;

		public int voiceID;

		public int state;

		public int voiceKind;
	}

	[Serializable]
	public class ShortVoiceList
	{
		public Dictionary<int, VoiceListInfo> dicShortBreathLists = new Dictionary<int, VoiceListInfo>();
	}

	[Serializable]
	public class BreathVoicePtnInfo
	{
		public List<int> lstConditions = new List<int>();

		public List<int> lstVoice = new List<int>();

		public List<int> lstAnimeID = new List<int>();
	}

	[Serializable]
	public class BreathPtn
	{
		[Label("段階")]
		public int level;

		[Label("アニメーション名")]
		public string anim = "";

		[Label("ループ中1回")]
		public bool onlyOne;

		[Label("ループ中1回がtrueのとき使用")]
		public bool isPlay;

		[Label("強制フラグ")]
		public bool force;

		[Label("表情変更時間最小")]
		public float timeChangeFaceMin = 5f;

		[Label("表情変更時間最題")]
		public float timeChangeFaceMax = 5f;

		[Label("パラメータ条件")]
		public int paramPtn;

		public List<BreathVoicePtnInfo> lstInfo = new List<BreathVoicePtnInfo>();
	}

	[Serializable]
	public class VoicePtnInfo
	{
		public int loadState;

		public List<int> lstAnimList = new List<int>();

		public List<int> lstPlayConditions = new List<int>();

		public int loadListmode = -1;

		public int loadListKind = -1;

		public List<int> lstVoice = new List<int>();
	}

	[Serializable]
	public class VoicePtn
	{
		[Label("キャラの状態")]
		public int condition = -1;

		[Label("掛け合いか")]
		public int howTalk;

		[Label("外部フラグ見る")]
		public bool LookFlag = true;

		[Label("アニメーション名")]
		public string anim = "";

		[Label("パラメータ条件")]
		public int paramPtn;

		public List<VoicePtnInfo> lstInfo = new List<VoicePtnInfo>();
	}

	public struct CheckVoicePtn
	{
		[Label("キャラの状態")]
		public int condition;

		[Label("掛け合いか")]
		public int howTalk;

		[Label("外部フラグ見る")]
		public bool LookFlag;

		[Label("アニメーション名")]
		public string anim;

		[Label("パラメータ条件")]
		public int paramPtn;
	}

	[Serializable]
	public class StartVoicePtn
	{
		[Label("キャラの状態")]
		public int condition = -1;

		[Label("どの体位の開始か")]
		public int nTaii = -1;

		[Label("タイミング名")]
		public string anim = "";

		[Label("タイミング")]
		public int timing;

		[Label("パラメーター制限")]
		public int nParamPtn = -1;

		public List<VoicePtnInfo> lstInfo = new List<VoicePtnInfo>();
	}

	[Serializable]
	public class ShortBreathPtn
	{
		public ValueDictionary<int, int, int, int, List<BreathVoicePtnInfo>> dicInfo = new ValueDictionary<int, int, int, int, List<BreathVoicePtnInfo>>();
	}

	[Serializable]
	public class VoiceAnimationPlayInfo
	{
		[Label("アニメーション名(ハッシュ値)")]
		public int animationHash;

		[Label("再生した")]
		public bool[] isPlays = new bool[2];
	}

	[Serializable]
	public class VoiceAnimationPlay
	{
		public List<VoiceAnimationPlayInfo> lstPlayInfo = new List<VoiceAnimationPlayInfo>();

		[Label("モーションの再生回数")]
		public int Count;

		public int InsideCount;

		public int OutsideCount;

		public int femaleCount;

		public int SameCount;

		public int InMouthCount;

		public int OutMouthCount;

		public void SetAllFlags(bool _play, int hash = -1)
		{
			int num = -1;
			for (int i = 0; i < lstPlayInfo.Count; i++)
			{
				if (hash == lstPlayInfo[i].animationHash)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				lstPlayInfo.Add(new VoiceAnimationPlayInfo
				{
					animationHash = hash
				});
			}
			for (int j = 0; j < lstPlayInfo.Count; j++)
			{
				lstPlayInfo[j].isPlays[0] = _play;
				lstPlayInfo[j].isPlays[1] = _play;
			}
		}

		public void AfterFinish()
		{
			for (int i = 0; i < lstPlayInfo.Count; i++)
			{
				if (lstPlayInfo[i].isPlays[0] || lstPlayInfo[i].isPlays[1])
				{
					lstPlayInfo[i].isPlays[0] = false;
					lstPlayInfo[i].isPlays[1] = false;
				}
			}
			Count++;
		}

		public void SetFinish(int mode)
		{
			switch (mode)
			{
			case 0:
				femaleCount++;
				break;
			case 1:
				InsideCount++;
				break;
			case 2:
				OutsideCount++;
				break;
			case 3:
				femaleCount++;
				InsideCount++;
				SameCount++;
				break;
			case 4:
				InsideCount++;
				InMouthCount++;
				break;
			case 5:
				OutsideCount++;
				OutMouthCount++;
				break;
			}
		}

		public VoiceAnimationPlayInfo GetAnimation(int _animHash)
		{
			for (int i = 0; i < lstPlayInfo.Count; i++)
			{
				if (_animHash == lstPlayInfo[i].animationHash)
				{
					return lstPlayInfo[i];
				}
			}
			return null;
		}
	}

	public enum VoiceKind
	{
		breath,
		breathShort,
		voice,
		startVoice,
		none
	}

	[Serializable]
	public class Voice
	{
		public VoiceKind state = VoiceKind.none;

		[Header("呼吸")]
		public VoiceListInfo breathInfo;

		[Label("呼吸リストの配列番号")]
		public int arrayBreath;

		[Label("呼吸アニメーションステート")]
		public string animBreath;

		[Label("呼吸グループ")]
		public int breathGroup = -1;

		[Label("速い？")]
		public bool speedStateFast;

		[Label("表情変化経過時間")]
		public float timeFaceDelta;

		[Label("表情変化時間")]
		public float timeFace;

		[Label("当たってる？")]
		public bool isGaugeHit;

		[Label("状態")]
		public int breathState = -1;

		[Header("セリフ")]
		public VoiceListInfo voiceInfo;

		[Label("セリフリストの配列番号")]
		public int arrayVoice;

		[Label("セリフリストの番号")]
		public int VoiceListID;

		[Label("セリフリストのシート番号")]
		public int VoiceListSheetID;

		[Label("セリフリストの状態")]
		public int VoiceListState;

		[Label("セリフアニメーションステート")]
		public string animVoice;

		[Header("短い喘ぎ")]
		public VoiceListInfo shortInfo;

		[Label("短い喘ぎの配列番号")]
		public int arrayShort;

		[Header("共通")]
		[Label("上書き禁止")]
		public bool notOverWrite;

		public FaceInfo Face = new FaceInfo();
	}

	private ValueDictionary<int, int, int, int, Dictionary<int, FaceInfo>> dicFaceInfos = new ValueDictionary<int, int, int, int, Dictionary<int, FaceInfo>>();

	public BreathList[] breathLists = new BreathList[2];

	public BreathList[] breathOnaniLists = new BreathList[2];

	private Dictionary<int, Dictionary<int, VoiceList>>[] dicdiclstVoiceList = new Dictionary<int, Dictionary<int, VoiceList>>[2];

	private ShortVoiceList[] ShortBreathLists = new ShortVoiceList[2];

	private ValueDictionary<int, int, int, int, int, string, Dictionary<int, BreathPtn>> dicBreathPtns = new ValueDictionary<int, int, int, int, int, string, Dictionary<int, BreathPtn>>();

	private ValueDictionary<int, int, int, int, int, string, Dictionary<int, BreathPtn>> dicBreathAddPtns = new ValueDictionary<int, int, int, int, int, string, Dictionary<int, BreathPtn>>();

	private Dictionary<int, Dictionary<int, List<VoicePtn>>>[] lstLoadVoicePtn = new Dictionary<int, Dictionary<int, List<VoicePtn>>>[2];

	private Dictionary<int, Dictionary<int, List<VoicePtn>>>[] lstLoadVoiceAddPtn = new Dictionary<int, Dictionary<int, List<VoicePtn>>>[2];

	private List<StartVoicePtn>[] lstLoadStartVoicePtn = new List<StartVoicePtn>[2];

	private List<StartVoicePtn>[] lstLoadStartVoiceAddPtn = new List<StartVoicePtn>[2];

	private ShortBreathPtn[] shortBreathPtns = new ShortBreathPtn[2];

	private ShortBreathPtn[] shortBreathAddPtns = new ShortBreathPtn[2];

	private Dictionary<int, Dictionary<int, VoiceAnimationPlay>> dicdicVoicePlayAnimation = new Dictionary<int, Dictionary<int, VoiceAnimationPlay>>();

	public HSceneFlagCtrl ctrlFlag;

	[SerializeField]
	private BreathList[] breathUseLists = new BreathList[2];

	[SerializeField]
	private BreathList[] breathOnaniUseLists = new BreathList[2];

	[SerializeField]
	private ValueDictionary<int, int, string, Dictionary<int, BreathPtn>> dicBreathUsePtns = new ValueDictionary<int, int, string, Dictionary<int, BreathPtn>>();

	[SerializeField]
	private Dictionary<int, List<VoicePtn>>[] lstVoicePtn = new Dictionary<int, List<VoicePtn>>[2];

	[SerializeField]
	private ValueDictionary<int, int, List<BreathVoicePtnInfo>> dicShortBreathUsePtns = new ValueDictionary<int, int, List<BreathVoicePtnInfo>>();

	public VoiceAnimationPlay playAnimation = new VoiceAnimationPlay();

	[SerializeField]
	private int[] personality = new int[2];

	[SerializeField]
	private float[] voicePitch = new float[2];

	[SerializeField]
	private ChaControl param;

	[SerializeField]
	private ChaControl param_sub;

	[SerializeField]
	private List<int> lstSystem;

	public Voice[] nowVoices = new Voice[2];

	public int nowMode;

	public int nowId;

	private GlobalMethod.FloatBlend[] blendEyes = new GlobalMethod.FloatBlend[2];

	private GlobalMethod.FloatBlend[] blendMouths = new GlobalMethod.FloatBlend[2];

	private GlobalMethod.FloatBlend[] blendMouthMaxs = new GlobalMethod.FloatBlend[2];

	private HSceneManager hSceneManager;

	private float oldHouchiTime;

	public float HouchiTime;

	private bool[] isPlays = new bool[2];

	private StringBuilder sbLoadFile = new StringBuilder();

	private List<string> tmpBreathFilenames = new List<string>();

	private List<string> tmpVoiceFilenames = new List<string>();

	private bool masturbation;

	private bool les;

	private bool multiFemale;

	private bool multiMale;

	public int EventID;

	private bool bAppendEV;

	private readonly string[][] tmpAddPatternName = new string[4][]
	{
		new string[2] { "HBreathPattern_[0-9]{2}_[0-9]{2}_[0-9]{2}", "HBreathPatternEV_[0-9]{2}_[0-9]{2}_[0-9]{2}" },
		new string[2] { "HVoicePattern_[0-9]{2}_[0-9]{2}_[0-9]{2}", "HVoicePatternEV[0-9]{2}_[0-9]{2}_[0-9]{2}_[0-9]{2}" },
		new string[2] { "HShortBreathPattern_[0-9]{2}_[0-9]{2}_[0-9]{2}", "HShortBreathPatternEV_[0-9]{2}_[0-9]{2}_[0-9]{2}" },
		new string[2] { "HStartVoicePattern_[0-9]{2}", "HStartVoicePatternEV[0-9]{2}_[0-9]{2}" }
	};

	private readonly string[] startKindName = new string[4] { "開始", "再開始", "行為変更", "挿入時" };

	private const string numPattarn = "[0-9]{2}";

	private const int AppendEventStart = 50;

	private List<string> lst = new List<string>();

	private List<string> lstBreathAbnames = new List<string>();

	private List<string> lstVoiceAbnames = new List<string>();

	private List<string> lstOnani = new List<string>();

	public bool Masturbation
	{
		private get
		{
			return masturbation;
		}
		set
		{
			masturbation = value;
		}
	}

	public bool Les
	{
		private get
		{
			return les;
		}
		set
		{
			les = value;
		}
	}

	public bool MultiFemale
	{
		private get
		{
			return multiFemale;
		}
		set
		{
			multiFemale = value;
		}
	}

	public bool MultiMale
	{
		private get
		{
			return multiMale;
		}
		set
		{
			multiMale = value;
		}
	}

	public bool SetVoiceList(int _mode, int _id, List<int> _lstSystem)
	{
		lstSystem = _lstSystem;
		nowId = _id;
		nowMode = _mode;
		lstLoadVoicePtn[0].TryGetValue(nowMode, out lstVoicePtn[0]);
		if (lstLoadVoicePtn[1] != null)
		{
			lstLoadVoicePtn[1].TryGetValue(nowMode, out lstVoicePtn[1]);
		}
		playAnimation = new VoiceAnimationPlay();
		if (dicdicVoicePlayAnimation.ContainsKey(nowMode))
		{
			dicdicVoicePlayAnimation[nowMode].TryGetValue(_id, out playAnimation);
		}
		else
		{
			dicdicVoicePlayAnimation.Add(nowMode, new Dictionary<int, VoiceAnimationPlay>());
		}
		if (playAnimation == null)
		{
			playAnimation = new VoiceAnimationPlay();
			dicdicVoicePlayAnimation[nowMode].Add(nowId, playAnimation);
		}
		for (int i = 0; i < 2; i++)
		{
			nowVoices[i].animBreath = "";
			nowVoices[i].animVoice = "";
			nowVoices[i].breathGroup = -1;
		}
		return true;
	}

	public bool SetBreathVoiceList(ChaControl[] _charas, int _mode, int _kind, List<int> _lstSystem, bool _reverse)
	{
		breathUseLists[0] = breathLists[0];
		breathUseLists[1] = breathLists[1];
		breathOnaniUseLists[0] = breathOnaniLists[0];
		breathOnaniUseLists[1] = breathOnaniLists[1];
		int kind = 0;
		if (!CheckBreathSheet(_mode, _kind, ref kind))
		{
			return false;
		}
		switch (_mode)
		{
		case 3:
			if (ctrlFlag.selectAnimationListInfo != null)
			{
				if (ctrlFlag.selectAnimationListInfo.id == 111)
				{
					kind = 7;
				}
			}
			else if (ctrlFlag.nowAnimationInfo.id == 111)
			{
				kind = 7;
			}
			break;
		case 1:
			if (ctrlFlag.selectAnimationListInfo != null)
			{
				if (ctrlFlag.selectAnimationListInfo.id == 107)
				{
					kind = 1;
				}
			}
			else if (ctrlFlag.nowAnimationInfo.id == 107)
			{
				kind = 1;
			}
			break;
		}
		for (int i = 0; i < _charas.Length; i++)
		{
			dicBreathUsePtns[i] = null;
			if (!dicBreathPtns[i].ContainsKey(_mode) || !dicBreathPtns[i][_mode].ContainsKey(kind))
			{
				continue;
			}
			int key = 0;
			if (_mode == 4 || _mode == 5)
			{
				if (_reverse)
				{
					switch (i)
					{
					case 0:
						key = 1;
						break;
					case 1:
						key = 0;
						break;
					}
				}
				else
				{
					key = i;
				}
			}
			if (dicBreathPtns[i][_mode][kind].ContainsKey(key))
			{
				dicBreathUsePtns[i] = dicBreathPtns[i][_mode][kind][key];
			}
		}
		if (kind == 6 || kind == 7)
		{
			kind -= 2;
		}
		for (int j = 0; j < _charas.Length; j++)
		{
			dicShortBreathUsePtns[j] = null;
			if (!shortBreathPtns[j].dicInfo.ContainsKey(_mode) || !shortBreathPtns[j].dicInfo[_mode].ContainsKey(kind))
			{
				continue;
			}
			int key2 = 0;
			if (_mode == 4 || _mode == 5)
			{
				if (_reverse)
				{
					switch (j)
					{
					case 0:
						key2 = 1;
						break;
					case 1:
						key2 = 0;
						break;
					}
				}
				else
				{
					key2 = j;
				}
			}
			_ = shortBreathPtns[j].dicInfo[_mode][kind];
			if (shortBreathPtns[j].dicInfo[_mode][kind].ContainsKey(key2))
			{
				dicShortBreathUsePtns[j] = shortBreathPtns[j].dicInfo[_mode][kind][key2];
			}
		}
		for (int k = 0; k < 2; k++)
		{
			if (dicBreathUsePtns[k] == null)
			{
				dicBreathUsePtns[k] = dicBreathUsePtns.New();
			}
		}
		return true;
	}

	private int CheckPhase(ChaControl _chara, int _main)
	{
		if (_chara == null || _chara.fileGameInfo2 == null)
		{
			return -1;
		}
		if (_main == 1 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 5 && hSceneManager.FemaleState[0] == ChaFileDefine.State.Broken)
		{
			return 3;
		}
		bool flag = false;
		if (EventID != 19 && ctrlFlag.isFaintness)
		{
			flag = ((_main != 0) ? (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 2) : (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1));
		}
		else if (EventID == 19 && ctrlFlag.isFaintnessVoice)
		{
			flag = ((_main != 0) ? (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 2) : (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1));
		}
		ChaFileGameInfo2 fileGameInfo = _chara.fileGameInfo2;
		if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Broken)
		{
			return 4;
		}
		if (flag)
		{
			return 3;
		}
		if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Dependence)
		{
			return 2;
		}
		bool flag2 = false;
		if (!lstSystem.Contains(4))
		{
			flag2 = ((!ctrlFlag.nowAnimationInfo.lstSystem.Contains(3)) ? (fileGameInfo.resistH >= 100) : (fileGameInfo.resistAnal >= 100));
		}
		else
		{
			flag2 = fileGameInfo.resistPain >= 100;
			flag2 |= _chara.fileParam2.hAttribute == 3;
		}
		if (bAppendEV)
		{
			flag2 = true;
		}
		if (flag2)
		{
			return 1;
		}
		return 0;
	}

	private bool CheckBreathSheet(int _mode, int _kind, ref int kind)
	{
		switch (_mode)
		{
		case 3:
			switch (_kind)
			{
			case 4:
				kind = 1;
				break;
			case 5:
				kind = 4;
				break;
			case 6:
				kind = 5;
				break;
			case 3:
				kind = 2;
				break;
			case 2:
				kind = 3;
				break;
			case 1:
			case 7:
				kind = 6;
				break;
			}
			break;
		case 5:
			switch (_kind)
			{
			case 3:
				kind = 0;
				break;
			case 1:
			case 2:
				kind = 1;
				break;
			case 4:
				kind = 2;
				break;
			case 0:
				return false;
			}
			break;
		case 6:
			switch (_kind)
			{
			case 3:
				kind = 0;
				break;
			case 1:
			case 2:
				kind = 1;
				break;
			case 0:
			case 4:
				return false;
			}
			break;
		default:
			kind = 0;
			break;
		}
		return true;
	}

	public bool AfterFinish()
	{
		if (playAnimation == null)
		{
			return false;
		}
		playAnimation.AfterFinish();
		return true;
	}

	public bool SetFinish(int mode)
	{
		if (playAnimation == null)
		{
			return false;
		}
		playAnimation.SetFinish(mode);
		return true;
	}

	public bool Proc(AnimatorStateInfo _ai, params ChaControl[] _females)
	{
		isPlays[0] = false;
		isPlays[1] = false;
		if (oldHouchiTime != HouchiTime)
		{
			oldHouchiTime = HouchiTime;
		}
		else
		{
			oldHouchiTime = (HouchiTime = 0f);
		}
		bool flag = false;
		flag = ctrlFlag.nowAnimationInfo.nPromiscuity > 0;
		if (!flag && Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[1]))
		{
			Manager.Voice.Stop(ctrlFlag.voice.voiceTrs[1]);
		}
		if (flag)
		{
			int num = ((new ShuffleRand(100).Get() >= 50) ? 1 : 0);
			isPlays[num] = StartVoiceProc(_females, num);
		}
		else
		{
			isPlays[0] = StartVoiceProc(_females, 0);
		}
		if (!isPlays[0] && !isPlays[1])
		{
			ctrlFlag.voice.playStart = -1;
		}
		if (flag)
		{
			int num2 = ((new ShuffleRand(100).Get() >= 50) ? 1 : 0);
			isPlays[num2] = VoiceProc(_ai, _females[num2], num2);
		}
		else
		{
			isPlays[0] = VoiceProc(_ai, _females[0], 0);
		}
		for (int i = 0; i < 2; i++)
		{
			if ((i != 1 || flag) && !isPlays[i])
			{
				isPlays[i] = ShortBreathProc(_females[i], i);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if ((j != 1 || flag) && ctrlFlag.voice.onaniEnterLoop == 1 && nowVoices[j].state == VoiceKind.voice && !Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[j]))
			{
				ctrlFlag.voice.onaniEnterLoop = 2;
			}
		}
		for (int k = 0; k < 2; k++)
		{
			if ((k != 1 || flag) && !isPlays[k])
			{
				BreathProc(_ai, _females[k], k);
			}
		}
		for (int l = 0; l < 2; l++)
		{
			if (l != 1 || flag)
			{
				OpenCtrl(_females[l], l);
			}
		}
		nowVoices[0].speedStateFast = ctrlFlag.nowSpeedStateFast;
		if (flag)
		{
			nowVoices[1].speedStateFast = ctrlFlag.nowSpeedStateFast;
		}
		for (int m = 0; m < 2; m++)
		{
			if ((m != 1 || flag) && (nowVoices[m].state == VoiceKind.startVoice || nowVoices[m].state == VoiceKind.voice) && !Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[m]))
			{
				nowVoices[m].state = VoiceKind.none;
			}
		}
		if (ctrlFlag.StartHouchiTime < HouchiTime)
		{
			ctrlFlag.voice.playVoices[0] = true;
			ctrlFlag.voice.playVoices[1] = true;
		}
		return true;
	}

	public bool OpenCtrl(ChaControl _female, int _main)
	{
		if (!_female.visibleAll || _female.objBody == null)
		{
			return false;
		}
		float _ans = 0f;
		if (blendEyes[0] == null)
		{
			blendEyes[0] = new GlobalMethod.FloatBlend();
			blendEyes[1] = new GlobalMethod.FloatBlend();
		}
		if (blendMouths[0] == null)
		{
			blendMouths[0] = new GlobalMethod.FloatBlend();
			blendMouths[1] = new GlobalMethod.FloatBlend();
		}
		if (blendMouthMaxs[0] == null)
		{
			blendMouthMaxs[0] = new GlobalMethod.FloatBlend();
			blendMouthMaxs[1] = new GlobalMethod.FloatBlend();
		}
		if (blendEyes[_main].Proc(ref _ans))
		{
			_female.ChangeEyesOpenMax(_ans);
		}
		FBSCtrlMouth mouthCtrl = _female.mouthCtrl;
		if (mouthCtrl != null)
		{
			float _ans2 = 0f;
			if (blendMouths[_main].Proc(ref _ans2))
			{
				mouthCtrl.OpenMin = _ans2;
			}
			_ans2 = 1f;
			if (blendMouthMaxs[_main].Proc(ref _ans2))
			{
				mouthCtrl.OpenMax = _ans2;
			}
		}
		return true;
	}

	public bool FaceReset(ChaControl _female)
	{
		_female.ChangeEyesOpenMax(1f);
		FBSCtrlMouth mouthCtrl = _female.mouthCtrl;
		if (mouthCtrl != null)
		{
			mouthCtrl.OpenMin = 0f;
		}
		_female.DisableShapeMouth(disable: false);
		return true;
	}

	public bool BreathProc(AnimatorStateInfo _ai, ChaControl _female, int _main, bool _forceSleepIdle = false)
	{
		if (_female == null || !_female.visibleAll || _female.objBody == null)
		{
			return false;
		}
		if ((breathUseLists[_main].dicdicVoiceList.Count == 0) & (breathOnaniUseLists[_main].dicdicVoiceList.Count == 0))
		{
			return false;
		}
		BreathVoicePtnInfo breathVoicePtnInfo = null;
		bool flag = false;
		if (EventID != 19 && ctrlFlag.isFaintness)
		{
			flag = ((_main != 0) ? (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 2) : (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1));
		}
		else if (EventID == 19 && ctrlFlag.isFaintnessVoice)
		{
			flag = ((_main != 0) ? (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 2) : (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1));
		}
		int num = -1;
		if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Broken)
		{
			num = -1;
		}
		else if (flag)
		{
			num = -1;
		}
		else if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Dependence)
		{
			num = -1;
		}
		else if (_main == 1 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 5 && hSceneManager.FemaleState[0] == ChaFileDefine.State.Broken)
		{
			num = -1;
		}
		else
		{
			switch (hSceneManager.FemaleState[_main])
			{
			case ChaFileDefine.State.Blank:
				num = 0;
				break;
			case ChaFileDefine.State.Favor:
				num = 1;
				break;
			case ChaFileDefine.State.Enjoyment:
				num = 2;
				break;
			case ChaFileDefine.State.Slavery:
				num = 3;
				break;
			case ChaFileDefine.State.Aversion:
				num = 4;
				break;
			default:
				return false;
			}
		}
		int num2 = CheckPhase(_female, _main);
		if (num2 < 0)
		{
			return false;
		}
		if (!dicBreathUsePtns.ContainsKey(_main) || !dicBreathUsePtns[_main].ContainsKey(num2))
		{
			return false;
		}
		foreach (KeyValuePair<string, Dictionary<int, BreathPtn>> item in dicBreathUsePtns[_main][num2])
		{
			if (!_ai.IsName(item.Key) && !_forceSleepIdle)
			{
				continue;
			}
			BreathPtn breathPtn = item.Value[num];
			if ((_forceSleepIdle && breathPtn.anim != "D_Idle") || !IsBreathPtnConditions(breathPtn.paramPtn, _main))
			{
				continue;
			}
			if (breathPtn.onlyOne && (breathPtn.anim == nowVoices[_main].animBreath || breathPtn.anim == nowVoices[_main].animVoice))
			{
				break;
			}
			if (nowVoices[_main].state != VoiceKind.breath && ctrlFlag.voice.onaniEnterLoop != 1)
			{
				if (!breathPtn.force && Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[_main]))
				{
					break;
				}
			}
			else
			{
				if (ctrlFlag.isGaugeHit != nowVoices[_main].isGaugeHit)
				{
					if (nowVoices[_main].breathInfo != null)
					{
						SetBreathFace(breathPtn, _main);
						SetFace(nowVoices[_main].Face, _female, _main);
					}
				}
				else
				{
					nowVoices[_main].timeFaceDelta += Time.deltaTime;
					if (nowVoices[_main].timeFaceDelta >= nowVoices[_main].timeFace && nowVoices[_main].breathInfo != null)
					{
						SetBreathFace(breathPtn, _main);
						SetFace(nowVoices[_main].Face, _female, _main);
					}
				}
				nowVoices[_main].isGaugeHit = ctrlFlag.isGaugeHit;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < breathPtn.lstInfo.Count; i++)
			{
				breathVoicePtnInfo = breathPtn.lstInfo[i];
				if (IsPlayBreathVoicePtn(_female, breathVoicePtnInfo, _main))
				{
					list.AddRange(breathVoicePtnInfo.lstVoice.OrderBy((int inf) => Guid.NewGuid()).ToList());
				}
			}
			list = list.OrderBy((int inf) => Guid.NewGuid()).ToList();
			if (list.Count == 0)
			{
				break;
			}
			int num3 = list[0];
			int num4 = 0;
			switch (breathPtn.level)
			{
			case 1:
				num4 = ((_female.fileGameInfo2.Libido < 50 || (nowMode == 3 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item2 == 6)) ? 1 : 2);
				break;
			case 2:
				num4 = 3;
				break;
			case 3:
				num4 = (ctrlFlag.voice.sleep ? 6 : 4);
				break;
			case 4:
				num4 = 5;
				break;
			}
			VoiceListInfo voiceListInfo = breathUseLists[_main].dicdicVoiceList[num4][num3];
			if (Singleton<HSceneFlagCtrl>.IsInstance())
			{
				HScene.AnimationListInfo nowAnimationInfo = Singleton<HSceneFlagCtrl>.Instance.nowAnimationInfo;
				if (nowAnimationInfo.ActionCtrl.Item1 == 3 && nowAnimationInfo.ActionCtrl.Item2 == 5)
				{
					voiceListInfo = breathOnaniUseLists[_main].dicdicVoiceList[num4][num3];
				}
			}
			if ((nowVoices[_main].state != VoiceKind.breath && ctrlFlag.voice.onaniEnterLoop != 1) || (nowVoices[_main].breathInfo != voiceListInfo && nowVoices[_main].breathGroup != voiceListInfo.group) || !Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[_main]))
			{
				AudioSource audioSource = Manager.Voice.OncePlayChara(new Manager.Voice.Loader
				{
					no = personality[_main],
					pitch = voicePitch[_main],
					voiceTrans = ctrlFlag.voice.voiceTrs[_main],
					bundle = voiceListInfo.pathAsset,
					asset = voiceListInfo.nameFile
				});
				if (audioSource != null)
				{
					audioSource.rolloffMode = AudioRolloffMode.Linear;
					_female.SetVoiceTransform(audioSource);
				}
				if (!ctrlFlag.voice.lstUseAsset.Contains(voiceListInfo.pathAsset))
				{
					ctrlFlag.voice.lstUseAsset.Add(voiceListInfo.pathAsset);
				}
				nowVoices[_main].breathInfo = voiceListInfo;
				nowVoices[_main].state = VoiceKind.breath;
				nowVoices[_main].animBreath = breathPtn.anim;
				nowVoices[_main].notOverWrite = nowVoices[_main].breathInfo.notOverWrite;
				nowVoices[_main].breathState = num4;
				nowVoices[_main].arrayBreath = num3;
				nowVoices[_main].breathGroup = voiceListInfo.group;
				SetBreathFace(breathPtn, _main);
				if (ctrlFlag.voice.urines[_main] && voiceListInfo.urine)
				{
					ctrlFlag.voice.urines[_main] = false;
				}
				SetFace(nowVoices[_main].Face, _female, _main);
				if (ctrlFlag.voice.onaniEnterLoop == 1 && breathVoicePtnInfo != null && breathVoicePtnInfo.lstConditions.Contains(10))
				{
					nowVoices[_main].state = VoiceKind.voice;
				}
			}
			break;
		}
		return true;
	}

	private bool IsPlayBreathVoicePtn(ChaControl _female, BreathVoicePtnInfo _lst, int _main)
	{
		if (!IsBreathPtnConditions(_female, _lst, _main))
		{
			return false;
		}
		if (!IsBreathAnimationList(_lst.lstAnimeID, nowId))
		{
			return false;
		}
		return true;
	}

	private bool IsBreathAnimationList(List<int> _lstAnimList, int _idNow)
	{
		if (_lstAnimList.Count == 0)
		{
			return true;
		}
		if (_lstAnimList.Contains(-1))
		{
			return true;
		}
		return _lstAnimList.Contains(_idNow);
	}

	private bool IsBreathPtnConditions(int limitKind, int _main)
	{
		bool flag = false;
		if (EventID != 19 && ctrlFlag.isFaintness)
		{
			flag = ((_main != 0) ? (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 2) : (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1));
		}
		else if (EventID == 19 && ctrlFlag.isFaintnessVoice)
		{
			flag = ((_main != 0) ? (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 2) : (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1));
		}
		if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Broken)
		{
			return limitKind == -1;
		}
		if (flag)
		{
			return limitKind == -1;
		}
		if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Dependence)
		{
			return limitKind == -1;
		}
		if (_main == 1 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 5 && hSceneManager.FemaleState[0] == ChaFileDefine.State.Broken)
		{
			return limitKind == -1;
		}
		return hSceneManager.FemaleState[_main] switch
		{
			ChaFileDefine.State.Blank => limitKind == 0, 
			ChaFileDefine.State.Favor => limitKind == 1, 
			ChaFileDefine.State.Enjoyment => limitKind == 2, 
			ChaFileDefine.State.Slavery => limitKind == 3, 
			ChaFileDefine.State.Aversion => limitKind == 4, 
			_ => false, 
		};
	}

	private bool IsBreathPtnConditions(ChaControl _female, BreathVoicePtnInfo _lst, int _main)
	{
		List<int> lstConditions = _lst.lstConditions;
		for (int i = 0; i < lstConditions.Count; i++)
		{
			switch (lstConditions[i])
			{
			case 0:
				if (ctrlFlag.isGaugeHit || ctrlFlag.isGaugeHit_M)
				{
					return false;
				}
				break;
			case 1:
				if (!ctrlFlag.isGaugeHit && !ctrlFlag.isGaugeHit_M)
				{
					return false;
				}
				break;
			case 2:
				if (_female.fileGameInfo2.Libido >= 50)
				{
					return false;
				}
				break;
			case 3:
				if (_female.fileGameInfo2.Libido < 50)
				{
					return false;
				}
				break;
			case 4:
				if (ctrlFlag.voice.urines[_main])
				{
					return false;
				}
				break;
			case 5:
				if (!ctrlFlag.voice.urines[_main])
				{
					return false;
				}
				break;
			case 6:
				if (ctrlFlag.voice.sleep)
				{
					return false;
				}
				break;
			case 7:
				if (!ctrlFlag.voice.sleep)
				{
					return false;
				}
				break;
			case 8:
				if (ctrlFlag.nowSpeedStateFast)
				{
					return false;
				}
				break;
			case 9:
				if (!ctrlFlag.nowSpeedStateFast)
				{
					return false;
				}
				break;
			case 10:
				if (ctrlFlag.voice.onaniEnterLoop != 1)
				{
					return false;
				}
				break;
			case 11:
				if (ctrlFlag.voice.onaniEnterLoop == 1)
				{
					return false;
				}
				break;
			case 12:
				if (!ctrlFlag.voice.changeTaii)
				{
					return false;
				}
				break;
			case 13:
				if (ctrlFlag.voice.changeTaii)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	private bool SetBreathFace(BreathPtn _ptn, int _main)
	{
		if (ctrlFlag.isGaugeHit)
		{
			if (nowVoices[_main].breathInfo.lstHitFace.Count > 0)
			{
				int num = nowVoices[_main].breathInfo.lstHitFace[UnityEngine.Random.Range(0, nowVoices[_main].breathInfo.lstHitFace.Count)];
				if (CheckFaceList(_main, 0, 0, 0, num))
				{
					nowVoices[_main].Face = dicFaceInfos[_main][0][0][0][num];
				}
			}
		}
		else if (nowVoices[_main].breathInfo.lstNotHitFace.Count > 0)
		{
			int num2 = nowVoices[_main].breathInfo.lstNotHitFace[UnityEngine.Random.Range(0, nowVoices[_main].breathInfo.lstNotHitFace.Count)];
			if (CheckFaceList(_main, 0, 0, 0, num2))
			{
				nowVoices[_main].Face = dicFaceInfos[_main][0][0][0][num2];
			}
		}
		nowVoices[_main].timeFaceDelta = 0f;
		nowVoices[_main].timeFace = UnityEngine.Random.Range(_ptn.timeChangeFaceMin, _ptn.timeChangeFaceMax);
		return true;
	}

	public bool VoiceProc(AnimatorStateInfo _ai, ChaControl _female, int _main)
	{
		if (EventID == 7 || EventID == 32 || !_female.visibleAll || _female.objBody == null)
		{
			ctrlFlag.voice.playVoices[_main] = false;
			return false;
		}
		int num = VoiceProcDetail(_ai, _female, _isFirst: true, _main);
		if (num == 3)
		{
			num = VoiceProcDetail(_ai, _female, _isFirst: false, _main);
		}
		if (num != 2)
		{
			ctrlFlag.voice.playVoices[_main] = false;
			if (num == 1)
			{
				ctrlFlag.voice.playShorts[_main] = -1;
			}
		}
		return num == 1;
	}

	public int VoiceProcDetail(AnimatorStateInfo _ai, ChaControl _female, bool _isFirst, int _main)
	{
		if (!_female.visibleAll || _female.objBody == null)
		{
			return 0;
		}
		int result = 0;
		VoiceAnimationPlayInfo voiceAnimationPlayInfo = playAnimation.GetAnimation(_ai.shortNameHash);
		if (voiceAnimationPlayInfo == null)
		{
			voiceAnimationPlayInfo = new VoiceAnimationPlayInfo();
			voiceAnimationPlayInfo.animationHash = _ai.shortNameHash;
			playAnimation.lstPlayInfo.Add(voiceAnimationPlayInfo);
		}
		if (lstVoicePtn == null || lstVoicePtn[_main] == null)
		{
			return 0;
		}
		if (nowVoices[_main].notOverWrite && Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[_main]))
		{
			return 2;
		}
		if (dicdiclstVoiceList[_main] == null || dicdiclstVoiceList[_main].Count == 0)
		{
			return 0;
		}
		if ((nowMode == 4 || nowMode == 5) && (nowVoices[_main ^ 1].state == VoiceKind.voice || nowVoices[_main ^ 1].state == VoiceKind.startVoice))
		{
			voiceAnimationPlayInfo.isPlays[_main] = true;
			return 0;
		}
		ChaControl info = ((_main == 0) ? param : param_sub);
		bool flag = false;
		foreach (List<VoicePtn> value in lstVoicePtn[_main].Values)
		{
			for (int i = 0; i < value.Count; i++)
			{
				VoicePtn voicePtn = value[i];
				if (!VoicePtnCondition(voicePtn.condition, _main) || !VoicePtnResist(voicePtn.condition, info, _main) || !_ai.IsName(voicePtn.anim) || !VoicePtnParam(voicePtn.paramPtn, _main))
				{
					continue;
				}
				List<int> _lstCategory = new List<int>();
				if (voicePtn.LookFlag)
				{
					if (!ctrlFlag.voice.playVoices[_main])
					{
						break;
					}
				}
				else if (voiceAnimationPlayInfo.isPlays[_main])
				{
					break;
				}
				List<PlayVoiceinfo> list = (from inf in GetPlayListNum(voicePtn.lstInfo, dicdiclstVoiceList[_main], ref _lstCategory, _female, playAnimation, _main)
					orderby Guid.NewGuid()
					select inf).ToList();
				if (list.Count == 0)
				{
					InitListPlayFlag(voicePtn.lstInfo, dicdiclstVoiceList[_main], _lstCategory);
					list = (from inf in GetPlayListNum(voicePtn.lstInfo, dicdiclstVoiceList[_main], ref _lstCategory, _female, playAnimation, _main)
						orderby Guid.NewGuid()
						select inf).ToList();
					if (list.Count == 0)
					{
						result = 3;
						continue;
					}
				}
				int voiceID = list[0].voiceID;
				int mode = list[0].mode;
				int kind = list[0].kind;
				int state = list[0].state;
				if (dicdiclstVoiceList[_main].Count == 0 || !dicdiclstVoiceList[_main].ContainsKey(mode) || !dicdiclstVoiceList[_main][mode].ContainsKey(kind) || dicdiclstVoiceList[_main][mode][kind].dicdicVoiceList[state].Count == 0 || !dicdiclstVoiceList[_main][mode][kind].dicdicVoiceList[state].ContainsKey(voiceID))
				{
					break;
				}
				VoiceListInfo voiceListInfo = dicdiclstVoiceList[_main][mode][kind].dicdicVoiceList[state][voiceID];
				AudioSource audioSource = Manager.Voice.OncePlayChara(new Manager.Voice.Loader
				{
					no = personality[_main],
					pitch = voicePitch[_main],
					voiceTrans = ctrlFlag.voice.voiceTrs[_main],
					bundle = voiceListInfo.pathAsset,
					asset = voiceListInfo.nameFile
				});
				if (audioSource != null)
				{
					audioSource.rolloffMode = AudioRolloffMode.Linear;
					_female.SetVoiceTransform(audioSource);
				}
				if (!ctrlFlag.voice.lstUseAsset.Contains(voiceListInfo.pathAsset))
				{
					ctrlFlag.voice.lstUseAsset.Add(voiceListInfo.pathAsset);
				}
				nowVoices[_main].voiceInfo = voiceListInfo;
				nowVoices[_main].state = VoiceKind.voice;
				nowVoices[_main].animVoice = voicePtn.anim;
				nowVoices[_main].notOverWrite = nowVoices[_main].voiceInfo.notOverWrite;
				nowVoices[_main].arrayVoice = voiceID;
				nowVoices[_main].VoiceListID = mode;
				nowVoices[_main].VoiceListSheetID = kind;
				nowVoices[_main].VoiceListState = state;
				nowVoices[_main].voiceInfo.isPlay = true;
				if (nowVoices[_main].voiceInfo.lstHitFace.Count > 0)
				{
					int num = nowVoices[_main].voiceInfo.lstHitFace[UnityEngine.Random.Range(0, nowVoices[_main].voiceInfo.lstHitFace.Count)];
					if (CheckFaceList(_main, 1, mode, kind, num))
					{
						nowVoices[_main].Face = dicFaceInfos[_main][1][mode][kind][num];
					}
					else
					{
						_ = bAppendEV;
					}
				}
				voiceAnimationPlayInfo.isPlays[_main] = true;
				result = 1;
				SetFace(nowVoices[_main].Face, _female, _main);
				ctrlFlag.voice.dialog = false;
				flag = true;
				break;
			}
			if (flag)
			{
				break;
			}
		}
		return result;
	}

	private List<PlayVoiceinfo> GetPlayListNum(List<VoicePtnInfo> _lst, Dictionary<int, Dictionary<int, VoiceList>> _lstVoice, ref List<int> _lstCategory, ChaControl female, VoiceAnimationPlay nowVoiceInfo, int _main)
	{
		List<PlayVoiceinfo> list = new List<PlayVoiceinfo>();
		for (int i = 0; i < _lst.Count; i++)
		{
			if (VoiceAnimationList(_lst[i].lstAnimList, nowId) && VoicePtnConditions(_lst[i].lstPlayConditions, female, nowVoiceInfo, _main))
			{
				_lstCategory.Add(i);
				list.AddRange(GetPlayNum(_lst[i], _lstVoice));
			}
		}
		return list;
	}

	private bool VoicePtnResist(int _condition, ChaControl info, int _main)
	{
		if (_condition > 2)
		{
			return true;
		}
		bool flag = false;
		if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
		{
			flag = ((!ctrlFlag.nowAnimationInfo.lstSystem.Contains(3)) ? (info.fileGameInfo2.resistH >= 100) : (info.fileGameInfo2.resistAnal >= 100));
		}
		else
		{
			flag = info.fileGameInfo2.resistPain >= 100;
			flag |= info.fileParam2.hAttribute == 3;
		}
		if (bAppendEV)
		{
			flag = true;
		}
		if (flag)
		{
			if (info.fileGameInfo2.Libido >= 80)
			{
				if (_condition != 2)
				{
					return false;
				}
			}
			else if (_condition != 1)
			{
				return false;
			}
		}
		else if (_condition != 0)
		{
			return false;
		}
		return true;
	}

	private bool VoicePtnCondition(int _condition, int main)
	{
		if (_condition == 6)
		{
			return true;
		}
		if (hSceneManager.FemaleState[main] == ChaFileDefine.State.Broken)
		{
			if (_condition != 5)
			{
				return false;
			}
		}
		else if (ctrlFlag.isFaintness && ctrlFlag.isFaintnessVoice)
		{
			if (_condition != 4)
			{
				return false;
			}
		}
		else if (hSceneManager.FemaleState[main] == ChaFileDefine.State.Dependence)
		{
			if (_condition != 3)
			{
				return false;
			}
		}
		else if (main == 1 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 5 && hSceneManager.FemaleState[0] == ChaFileDefine.State.Broken)
		{
			if (_condition != 4)
			{
				return false;
			}
		}
		else if (_condition == 3 || _condition == 4 || _condition == 5)
		{
			return false;
		}
		return true;
	}

	private bool VoicePtnParam(int _paramPtn, int main)
	{
		if (_paramPtn == -1)
		{
			return true;
		}
		switch (hSceneManager.FemaleState[main])
		{
		case ChaFileDefine.State.Blank:
			if (_paramPtn != 0)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Favor:
			if (_paramPtn != 1)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Enjoyment:
			if (_paramPtn != 2)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Slavery:
			if (_paramPtn != 3)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Aversion:
			if (_paramPtn != 4)
			{
				return false;
			}
			break;
		}
		return true;
	}

	private bool StartVoicePtnParam(int _paramPtn, int main)
	{
		if (_paramPtn == -1)
		{
			return true;
		}
		switch (hSceneManager.FemaleState[main])
		{
		case ChaFileDefine.State.Blank:
			if (_paramPtn != 0)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Favor:
			switch (_paramPtn)
			{
			default:
				return false;
			case 1:
				if (hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Favor] >= 80)
				{
					return false;
				}
				break;
			case 2:
			case 3:
				break;
			}
			if (_paramPtn == 2 && hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Favor] < 80)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Enjoyment:
			switch (_paramPtn)
			{
			default:
				return false;
			case 4:
				if (hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Enjoyment] >= 80)
				{
					return false;
				}
				break;
			case 5:
			case 6:
				break;
			}
			if (_paramPtn == 5 && hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Enjoyment] < 80)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Slavery:
			switch (_paramPtn)
			{
			default:
				return false;
			case 7:
				if (hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Slavery] >= 80)
				{
					return false;
				}
				break;
			case 8:
			case 9:
				break;
			}
			if (_paramPtn == 8 && hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Slavery] < 80)
			{
				return false;
			}
			break;
		case ChaFileDefine.State.Aversion:
			switch (_paramPtn)
			{
			default:
				return false;
			case 10:
				if (hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Aversion] >= 80)
				{
					return false;
				}
				break;
			case 11:
			case 12:
				break;
			}
			if (_paramPtn == 11 && hSceneManager.FemaleStateNum[main][ChaFileDefine.State.Aversion] < 80)
			{
				return false;
			}
			break;
		}
		return true;
	}

	private bool VoiceAnimationList(List<int> _lstAnimList, int _idNow)
	{
		if (_lstAnimList.Count == 0)
		{
			return false;
		}
		if (_lstAnimList.Contains(-1))
		{
			return true;
		}
		return _lstAnimList.Contains(_idNow);
	}

	private bool VoicePtnConditions(List<int> _lstConditions, ChaControl female, VoiceAnimationPlay nowVoiceInfo, int _main)
	{
		for (int i = 0; i < _lstConditions.Count; i++)
		{
			switch (_lstConditions[i])
			{
			case 0:
				if (ctrlFlag.StartHouchiTime > HouchiTime)
				{
					return false;
				}
				HouchiTime = 0f;
				break;
			case 1:
				if (female.GetBustSizeKind() != 0)
				{
					return false;
				}
				break;
			case 2:
				if (female.GetBustSizeKind() != 2)
				{
					return false;
				}
				break;
			case 3:
				if (nowVoiceInfo == null || nowVoiceInfo.Count < 1)
				{
					return false;
				}
				break;
			case 4:
				if (_main != 0)
				{
					return false;
				}
				break;
			case 5:
				if (_main != 1)
				{
					return false;
				}
				break;
			case 6:
				if (ctrlFlag.voice.oldFinish != 0)
				{
					return false;
				}
				break;
			case 7:
				if (ctrlFlag.voice.oldFinish != 2)
				{
					return false;
				}
				break;
			case 8:
				if (ctrlFlag.voice.oldFinish != 1)
				{
					return false;
				}
				break;
			case 9:
				if (ctrlFlag.voice.oldFinish != 3)
				{
					return false;
				}
				break;
			case 10:
				if (ctrlFlag.nowSpeedStateFast)
				{
					return false;
				}
				break;
			case 11:
				if (!ctrlFlag.nowSpeedStateFast)
				{
					return false;
				}
				break;
			case 12:
				if (ctrlFlag.feel_f >= 0.5f)
				{
					return false;
				}
				break;
			case 13:
				if (ctrlFlag.feel_f < 0.5f)
				{
					return false;
				}
				break;
			case 14:
				if (EventID != 19)
				{
					return false;
				}
				break;
			case 15:
				if (ctrlFlag.feel_f < 0.75f || ctrlFlag.feel_m < 0.75f)
				{
					return false;
				}
				break;
			case 16:
				if (nowVoiceInfo == null || nowVoiceInfo.femaleCount < 2)
				{
					return false;
				}
				break;
			case 17:
				if (nowVoiceInfo == null || nowVoiceInfo.SameCount < 2)
				{
					return false;
				}
				break;
			case 18:
				if (nowVoiceInfo == null || nowVoiceInfo.OutsideCount < 2)
				{
					return false;
				}
				break;
			case 19:
				if (nowVoiceInfo == null || nowVoiceInfo.InsideCount < 2)
				{
					return false;
				}
				break;
			case 20:
				if (nowVoiceInfo == null || nowVoiceInfo.Count < 1 || (nowVoiceInfo.femaleCount < 1 && nowVoiceInfo.SameCount < 1))
				{
					return false;
				}
				break;
			case 21:
				if (nowVoiceInfo == null || nowVoiceInfo.Count < 1 || nowVoiceInfo.InMouthCount < 1)
				{
					return false;
				}
				break;
			case 22:
				if (nowVoiceInfo == null || nowVoiceInfo.Count < 1 || nowVoiceInfo.OutMouthCount < 1)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	private List<PlayVoiceinfo> GetPlayNum(VoicePtnInfo _lstPlay, Dictionary<int, Dictionary<int, VoiceList>> _lstVoice)
	{
		List<PlayVoiceinfo> list = new List<PlayVoiceinfo>();
		List<PlayVoiceinfo> list2 = new List<PlayVoiceinfo>();
		if (_lstVoice.Count == 0)
		{
			return list;
		}
		int loadListmode = _lstPlay.loadListmode;
		int loadListKind = _lstPlay.loadListKind;
		int loadState = _lstPlay.loadState;
		if (!_lstVoice.ContainsKey(loadListmode) || !_lstVoice[loadListmode].ContainsKey(loadListKind))
		{
			return list;
		}
		Dictionary<int, VoiceListInfo> dictionary = _lstVoice[loadListmode][loadListKind].dicdicVoiceList[loadState];
		bool flag = false;
		PlayVoiceinfo item = default(PlayVoiceinfo);
		for (int i = 0; i < _lstPlay.lstVoice.Count; i++)
		{
			if (!dictionary.ContainsKey(_lstPlay.lstVoice[i]))
			{
				GlobalMethod.DebugLog("再生しようとしている番号がリストにない", 1);
			}
			else if (!dictionary[_lstPlay.lstVoice[i]].isPlay)
			{
				item.mode = loadListmode;
				item.kind = loadListKind;
				item.voiceID = _lstPlay.lstVoice[i];
				item.state = loadState;
				item.voiceKind = dictionary[_lstPlay.lstVoice[i]].voiceKind;
				if (!flag)
				{
					flag = dictionary[_lstPlay.lstVoice[i]].voiceKind == 1;
				}
				list.Add(item);
			}
		}
		if (flag)
		{
			for (int j = 0; j < list.Count; j++)
			{
				int index = j;
				if (list[index].voiceKind != 0)
				{
					list2.Add(list[index]);
				}
			}
		}
		else
		{
			list2.AddRange(list);
		}
		return list2;
	}

	private bool InitListPlayFlag(List<VoicePtnInfo> _lst, Dictionary<int, Dictionary<int, VoiceList>> _lstVoice, List<int> _lstCategory)
	{
		for (int i = 0; i < _lstCategory.Count; i++)
		{
			if (_lst.Count <= _lstCategory[i])
			{
				continue;
			}
			int loadListmode = _lst[_lstCategory[i]].loadListmode;
			int loadListKind = _lst[_lstCategory[i]].loadListKind;
			int loadState = _lst[_lstCategory[i]].loadState;
			for (int j = 0; j < _lst[_lstCategory[i]].lstVoice.Count; j++)
			{
				int key = _lst[_lstCategory[i]].lstVoice[j];
				if (_lstVoice.ContainsKey(loadListmode) && _lstVoice[loadListmode].ContainsKey(loadListKind))
				{
					if (!_lstVoice[loadListmode][loadListKind].dicdicVoiceList[loadState].ContainsKey(key))
					{
						GlobalMethod.DebugLog("再生しようとしている番号がリストにない", 1);
					}
					else
					{
						_lstVoice[loadListmode][loadListKind].dicdicVoiceList[loadState][key].isPlay = false;
					}
				}
			}
		}
		return true;
	}

	private bool StartVoiceProc(ChaControl[] _females, int _nFemale)
	{
		bool result = false;
		List<StartVoicePtn> list = null;
		ChaControl info = _females[_nFemale];
		if (_females[_nFemale] == null || _females[_nFemale].objBody == null)
		{
			ctrlFlag.voice.playStart = -1;
			ctrlFlag.voice.playStartOld = -1;
			return false;
		}
		list = lstLoadStartVoicePtn[_nFemale];
		if (list == null)
		{
			ctrlFlag.voice.playStartOld = -1;
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			if (nowVoices[i].notOverWrite && Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[_nFemale]))
			{
				return false;
			}
		}
		if (dicdiclstVoiceList[_nFemale] == null)
		{
			ctrlFlag.voice.playStartOld = -1;
			return false;
		}
		List<VoicePtnInfo> list2 = null;
		for (int j = 0; j < list.Count; j++)
		{
			StartVoicePtn startVoicePtn = list[j];
			if (!VoicePtnCondition(startVoicePtn.condition, _nFemale) || !VoicePtnResist(startVoicePtn.condition, info, _nFemale) || (startVoicePtn.nTaii >= 0 && startVoicePtn.nTaii != nowMode) || startVoicePtn.timing != ctrlFlag.voice.playStart || !StartVoicePtnParam(startVoicePtn.nParamPtn, _nFemale))
			{
				continue;
			}
			List<int> _lstCategory = new List<int>();
			List<PlayVoiceinfo> list3 = null;
			bool flag = ctrlFlag.voice.playStart > 0 || startVoicePtn.nParamPtn <= 0;
			if (flag)
			{
				list3 = (from inf in GetPlayListNum(startVoicePtn.lstInfo, dicdiclstVoiceList[_nFemale], ref _lstCategory, _nFemale, _females[_nFemale])
					orderby Guid.NewGuid()
					select inf).ToList();
			}
			else if (startVoicePtn.nParamPtn != 0)
			{
				if (list2 == null)
				{
					list2 = new List<VoicePtnInfo>(startVoicePtn.lstInfo);
				}
				else
				{
					list2.AddRange(startVoicePtn.lstInfo);
				}
				if (startVoicePtn.nParamPtn % 3 != 0)
				{
					continue;
				}
				list3 = (from inf in GetPlayListNum(list2, dicdiclstVoiceList[_nFemale], ref _lstCategory, _nFemale, _females[_nFemale])
					orderby Guid.NewGuid()
					select inf).ToList();
			}
			if (list3.Count == 0)
			{
				if (flag)
				{
					InitGetPlayListNum(startVoicePtn.lstInfo, dicdiclstVoiceList[_nFemale], _lstCategory);
					list3 = (from inf in GetPlayListNum(startVoicePtn.lstInfo, dicdiclstVoiceList[_nFemale], ref _lstCategory, _nFemale, _females[_nFemale])
						orderby Guid.NewGuid()
						select inf).ToList();
				}
				else
				{
					InitGetPlayListNum(list2, dicdiclstVoiceList[_nFemale], _lstCategory);
					list3 = (from inf in GetPlayListNum(list2, dicdiclstVoiceList[_nFemale], ref _lstCategory, _nFemale, _females[_nFemale])
						orderby Guid.NewGuid()
						select inf).ToList();
				}
				if (list3.Count == 0)
				{
					if (j == list.Count - 1)
					{
						ctrlFlag.voice.playStart = -1;
						ctrlFlag.voice.playStartOld = -1;
						break;
					}
					continue;
				}
			}
			int mode = list3[0].mode;
			int kind = list3[0].kind;
			int voiceID = list3[0].voiceID;
			int state = list3[0].state;
			if (!dicdiclstVoiceList[_nFemale][mode][kind].dicdicVoiceList[state].ContainsKey(voiceID))
			{
				GlobalMethod.DebugLog("配列外してる[" + _nFemale + "]", 1);
			}
			else
			{
				if (EventID == 7 || EventID == 32)
				{
					break;
				}
				VoiceListInfo voiceListInfo = dicdiclstVoiceList[_nFemale][mode][kind].dicdicVoiceList[state][voiceID];
				AudioSource audioSource = Manager.Voice.OncePlayChara(new Manager.Voice.Loader
				{
					no = personality[_nFemale],
					pitch = voicePitch[_nFemale],
					voiceTrans = ctrlFlag.voice.voiceTrs[_nFemale],
					bundle = voiceListInfo.pathAsset,
					asset = voiceListInfo.nameFile
				});
				if (audioSource != null)
				{
					audioSource.rolloffMode = AudioRolloffMode.Linear;
					_females[_nFemale].SetVoiceTransform(audioSource);
				}
				if (!ctrlFlag.voice.lstUseAsset.Contains(voiceListInfo.pathAsset))
				{
					ctrlFlag.voice.lstUseAsset.Add(voiceListInfo.pathAsset);
				}
				nowVoices[_nFemale].voiceInfo = voiceListInfo;
				VoiceKind[] array = new VoiceKind[4]
				{
					VoiceKind.startVoice,
					VoiceKind.startVoice,
					VoiceKind.voice,
					VoiceKind.voice
				};
				nowVoices[_nFemale].state = array[startVoicePtn.timing];
				nowVoices[_nFemale].animVoice = startVoicePtn.anim;
				nowVoices[_nFemale].notOverWrite = nowVoices[_nFemale].voiceInfo.notOverWrite;
				nowVoices[_nFemale].arrayVoice = voiceID;
				nowVoices[_nFemale].VoiceListID = 7;
				nowVoices[_nFemale].VoiceListSheetID = 0;
				nowVoices[_nFemale].VoiceListState = state;
				if (nowVoices[_nFemale].voiceInfo.lstHitFace.Count > 0)
				{
					int num = nowVoices[_nFemale].voiceInfo.lstHitFace[UnityEngine.Random.Range(0, nowVoices[_nFemale].voiceInfo.lstHitFace.Count)];
					if (CheckFaceList(_nFemale, 3, 7, kind, num))
					{
						nowVoices[_nFemale].Face = dicFaceInfos[_nFemale][3][7][kind][num];
					}
					else
					{
						_ = bAppendEV;
					}
				}
				nowVoices[_nFemale].voiceInfo.isPlay = true;
				result = true;
				ctrlFlag.voice.playStartOld = ctrlFlag.voice.playStart;
				ctrlFlag.voice.playStart = -1;
				SetFace(nowVoices[_nFemale].Face, _females[_nFemale], _nFemale);
			}
			break;
		}
		return result;
	}

	private List<PlayVoiceinfo> GetPlayListNum(List<VoicePtnInfo> _lst, Dictionary<int, Dictionary<int, VoiceList>> _lstVoice, ref List<int> _lstCategory, int _main, ChaControl female)
	{
		new List<PlayVoiceinfo>();
		List<VoicePtnInfo> list = new List<VoicePtnInfo>();
		for (int i = 0; i < _lst.Count; i++)
		{
			int num = i;
			if ((_lst[num].lstAnimList[0] == -1 || _lst[num].lstAnimList.Contains(ctrlFlag.nowAnimationInfo.id)) && StartVoicePtnConditions(_lst[num].lstPlayConditions, female, _main))
			{
				list.Add(_lst[num]);
				_lstCategory.Add(num);
			}
		}
		return StartVoicePtnChageMotion(list, _lstVoice, ref _lstCategory);
	}

	private List<PlayVoiceinfo> StartVoicePtnChageMotion(List<VoicePtnInfo> target, Dictionary<int, Dictionary<int, VoiceList>> _lstVoice, ref List<int> _lstCategory, int _main = -1)
	{
		List<int> list = new List<int>();
		list.AddRange(_lstCategory);
		List<PlayVoiceinfo> list2 = new List<PlayVoiceinfo>();
		List<int> list3 = new List<int>();
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		for (int i = 0; i < target.Count; i++)
		{
			bool flag = false;
			int num = i;
			for (int j = 2; j < 15; j++)
			{
				bool flag2 = target[num].lstPlayConditions.Contains(j);
				if (!flag)
				{
					flag = flag2;
				}
				if (flag2)
				{
					if (!dictionary.ContainsKey(num))
					{
						dictionary.Add(num, new List<int>());
					}
					if (!dictionary[num].Contains(j))
					{
						dictionary[num].Add(j);
					}
				}
			}
			if (flag)
			{
				list3.Add(num);
			}
		}
		_lstCategory.Clear();
		for (int k = 0; k < target.Count; k++)
		{
			int num2 = k;
			if (list3.Count <= 0 || (list3.Contains(num2) && SVPtnConditionPriority(list3, dictionary, num2) == 1))
			{
				_lstCategory.Add(list[num2]);
				list2.AddRange(GetPlayNum(target[num2], _lstVoice));
			}
		}
		return list2;
	}

	private int SVPtnConditionPriority(List<int> ids, Dictionary<int, List<int>> conditions, int _id)
	{
		List<int> check = new List<int>
		{
			8, 4, 5, 9, 10, 12, 2, 3, 11, 0,
			1, 6, 7, 13, 14, 15
		};
		int num = SVPtnConditionPriority(ids, conditions, check);
		if (num < 0)
		{
			return -1;
		}
		int num2 = SVPCheckPriority(conditions[_id], check);
		if (num2 < 0)
		{
			return -1;
		}
		if (num == num2)
		{
			return 1;
		}
		return 0;
	}

	private int SVPtnConditionPriority(List<int> ids, Dictionary<int, List<int>> conditions, List<int> check)
	{
		List<int> conditions2 = conditions[ids[0]];
		int num = SVPCheckPriority(conditions2, check);
		if (num <= 0)
		{
			return num;
		}
		if (ids.Count > 1)
		{
			for (int i = 1; i < ids.Count; i++)
			{
				conditions2 = conditions[ids[i]];
				int num2 = SVPCheckPriority(conditions2, check);
				if (num2 != -1 && num > num2)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private int SVPCheckPriority(List<int> conditions, List<int> check)
	{
		for (int i = 0; i < check.Count; i++)
		{
			if (conditions.Contains(check[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private bool StartVoicePtnConditions(List<int> conditions, ChaControl female, int _main)
	{
		_ = female.fileGameInfo2;
		if (EventID == 19)
		{
			for (int i = 2; i < 15; i++)
			{
				int item = i;
				if (conditions.Contains(item))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < conditions.Count; j++)
		{
			switch (conditions[j])
			{
			case 0:
				if (_main != 0)
				{
					return false;
				}
				break;
			case 1:
				if (_main != 1)
				{
					return false;
				}
				break;
			case 2:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.Anal) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.NameKuwae))
				{
					return false;
				}
				break;
			case 3:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.MaleShot) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.NameKuwae))
				{
					return false;
				}
				break;
			case 4:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.MataIjiri) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.Sonyu))
				{
					return false;
				}
				break;
			case 5:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.AnalIjiri) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.Anal))
				{
					return false;
				}
				break;
			case 6:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.OrgasmF))
				{
					return false;
				}
				break;
			case 7:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.OloopH) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.Sonyu))
				{
					return false;
				}
				break;
			case 8:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.Urine) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.Kunni))
				{
					return false;
				}
				break;
			case 9:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.Kiss) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.NameKuwae))
				{
					return false;
				}
				break;
			case 10:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.Kiss) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.Kunni))
				{
					return false;
				}
				break;
			case 11:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.Inside) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.Kunni))
				{
					return false;
				}
				break;
			case 12:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.InsideMouth) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.Kiss))
				{
					return false;
				}
				break;
			case 13:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.FemaleMain) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.MaleMain))
				{
					return false;
				}
				break;
			case 14:
				if (!ctrlFlag.voice.oldAnimType.Contains(HSceneFlagCtrl.AnimType.MaleMainSonyu) || !ctrlFlag.voice.newAnimType.Contains(HSceneFlagCtrl.AnimType.FemaleMainSonyu))
				{
					return false;
				}
				break;
			case 15:
				if (EventID != 19)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	private void InitGetPlayListNum(List<VoicePtnInfo> _lst, Dictionary<int, Dictionary<int, VoiceList>> _lstVoice, List<int> _lstCategory)
	{
		for (int i = 0; i < _lstCategory.Count; i++)
		{
			if (_lst.Count <= _lstCategory[i])
			{
				GlobalMethod.DebugLog("開始音声のカテゴリ取得に失敗(フラグ再設定時)", 1);
				continue;
			}
			int loadListmode = _lst[_lstCategory[i]].loadListmode;
			int loadListKind = _lst[_lstCategory[i]].loadListKind;
			int loadState = _lst[_lstCategory[i]].loadState;
			for (int j = 0; j < _lst[_lstCategory[i]].lstVoice.Count; j++)
			{
				if (!_lstVoice.ContainsKey(loadListmode) || !_lstVoice[loadListmode].ContainsKey(loadListKind))
				{
					GlobalMethod.DebugLog("開始音声の再生番号取得に失敗(フラグ再設定時)", 1);
					continue;
				}
				int key = _lst[_lstCategory[i]].lstVoice[j];
				if (_lstVoice[loadListmode][loadListKind].dicdicVoiceList.Length <= loadState)
				{
					GlobalMethod.DebugLog("開始音声の再生番号取得に失敗(フラグ再設定時)", 1);
				}
				else if (!_lstVoice[loadListmode][loadListKind].dicdicVoiceList[loadState].ContainsKey(key))
				{
					GlobalMethod.DebugLog("開始音声の再生番号取得に失敗(フラグ再設定時)", 1);
				}
				else
				{
					_lstVoice[loadListmode][loadListKind].dicdicVoiceList[loadState][key].isPlay = false;
				}
			}
		}
	}

	public bool ShortBreathProc(ChaControl _female, int _main)
	{
		if (!_female.visibleAll || _female.objBody == null)
		{
			ctrlFlag.voice.playShorts[_main] = -1;
		}
		if (nowVoices[_main].notOverWrite && Manager.Voice.IsPlay(ctrlFlag.voice.voiceTrs[_main]))
		{
			ctrlFlag.voice.playShorts[_main] = -1;
		}
		if (EventID == 7 || EventID == 32)
		{
			ctrlFlag.voice.playShorts[_main] = -1;
		}
		if (!dicShortBreathUsePtns.ContainsKey(_main) || dicShortBreathUsePtns[_main] == null || dicShortBreathUsePtns[_main].Count == 0)
		{
			ctrlFlag.voice.playShorts[_main] = -1;
		}
		if (ctrlFlag.voice.playShorts[_main] == -1)
		{
			return false;
		}
		if (dicShortBreathUsePtns[_main] != null && !dicShortBreathUsePtns[_main].ContainsKey(ctrlFlag.voice.playShorts[_main]))
		{
			return false;
		}
		List<BreathVoicePtnInfo> list = dicShortBreathUsePtns[_main][ctrlFlag.voice.playShorts[_main]];
		if (list.Count == 0)
		{
			ctrlFlag.voice.playShorts[_main] = -1;
			return false;
		}
		List<int> list2 = new List<int>();
		for (int i = 0; i < list.Count; i++)
		{
			if (IsPlayShortBreathVoicePtn(_female, list[i], _main))
			{
				list2.AddRange(list[i].lstVoice);
			}
		}
		if (list2.Count == 0)
		{
			return false;
		}
		list2 = list2.OrderBy((int inf) => Guid.NewGuid()).ToList();
		int num = list2[0];
		if (!ShortBreathLists[_main].dicShortBreathLists.ContainsKey(num))
		{
			ctrlFlag.voice.playShorts[_main] = -1;
			return false;
		}
		Dictionary<int, VoiceListInfo> dicShortBreathLists = ShortBreathLists[_main].dicShortBreathLists;
		AudioSource audioSource = Manager.Voice.OncePlayChara(new Manager.Voice.Loader
		{
			no = personality[_main],
			pitch = voicePitch[_main],
			voiceTrans = ctrlFlag.voice.voiceTrs[_main],
			bundle = dicShortBreathLists[num].pathAsset,
			asset = dicShortBreathLists[num].nameFile
		});
		if (audioSource != null)
		{
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			_female.SetVoiceTransform(audioSource);
		}
		if (!ctrlFlag.voice.lstUseAsset.Contains(dicShortBreathLists[num].pathAsset))
		{
			ctrlFlag.voice.lstUseAsset.Add(dicShortBreathLists[num].pathAsset);
		}
		nowVoices[_main].shortInfo = dicShortBreathLists[num];
		nowVoices[_main].state = VoiceKind.breathShort;
		nowVoices[_main].notOverWrite = nowVoices[_main].shortInfo.notOverWrite;
		nowVoices[_main].arrayShort = num;
		if (nowVoices[_main].shortInfo.lstHitFace.Count > 0)
		{
			int num2 = nowVoices[_main].shortInfo.lstHitFace[UnityEngine.Random.Range(0, nowVoices[_main].shortInfo.lstHitFace.Count)];
			if (CheckFaceList(_main, 2, 0, 0, num2))
			{
				nowVoices[_main].Face = dicFaceInfos[_main][2][0][0][num2];
			}
		}
		ctrlFlag.voice.playShorts[_main] = -1;
		SetFace(nowVoices[_main].Face, _female, _main);
		return true;
	}

	private bool IsPlayShortBreathVoicePtn(ChaControl _female, BreathVoicePtnInfo _lst, int _main)
	{
		if (!CheckShortVoiceCondition(_female, _lst.lstConditions, _main))
		{
			return false;
		}
		if (!IsShortBreathAnimationList(_lst.lstAnimeID, nowId))
		{
			return false;
		}
		return true;
	}

	private bool IsShortBreathAnimationList(List<int> _lstAnimList, int _idNow)
	{
		if (_lstAnimList.Count == 0)
		{
			return true;
		}
		if (_lstAnimList.Contains(-1))
		{
			return true;
		}
		return _lstAnimList.Contains(_idNow);
	}

	private bool CheckShortVoiceCondition(ChaControl _female, List<int> _lstConditions, int _main)
	{
		if (EventID == 7 || EventID == 32)
		{
			if (!_lstConditions.Contains(6))
			{
				return false;
			}
		}
		else if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Broken)
		{
			if (!_lstConditions.Contains(5))
			{
				return false;
			}
		}
		else if (ctrlFlag.isFaintness && ctrlFlag.isFaintnessVoice)
		{
			if (!_lstConditions.Contains(4))
			{
				return false;
			}
		}
		else if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Dependence)
		{
			if (!_lstConditions.Contains(3))
			{
				return false;
			}
		}
		else if (_main == 1 && ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 5 && hSceneManager.FemaleState[0] == ChaFileDefine.State.Broken)
		{
			if (!_lstConditions.Contains(4))
			{
				return false;
			}
		}
		else
		{
			for (int i = 0; i < _lstConditions.Count; i++)
			{
				switch (_lstConditions[i])
				{
				case 0:
				{
					if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Dependence)
					{
						return false;
					}
					if (ctrlFlag.isFaintness && ctrlFlag.isFaintnessVoice)
					{
						return false;
					}
					if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Broken)
					{
						return false;
					}
					if (EventID == 7 || EventID == 32)
					{
						return false;
					}
					ChaFileGameInfo2 fileGameInfo = _female.fileGameInfo2;
					bool flag = false;
					if (!lstSystem.Contains(4))
					{
						flag = ((!ctrlFlag.nowAnimationInfo.lstSystem.Contains(3)) ? (fileGameInfo.resistH >= 100) : (fileGameInfo.resistAnal >= 100));
					}
					else
					{
						flag = fileGameInfo.resistPain >= 100;
						flag |= _female.fileParam2.hAttribute == 3;
					}
					if (bAppendEV)
					{
						flag = true;
					}
					if (flag)
					{
						return false;
					}
					break;
				}
				case 1:
				{
					if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Dependence)
					{
						return false;
					}
					if (ctrlFlag.isFaintness && ctrlFlag.isFaintnessVoice)
					{
						return false;
					}
					if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Broken)
					{
						return false;
					}
					if (EventID == 7 || EventID == 32)
					{
						return false;
					}
					ChaFileGameInfo2 fileGameInfo3 = _female.fileGameInfo2;
					bool flag3 = false;
					if (!lstSystem.Contains(4))
					{
						flag3 = ((!ctrlFlag.nowAnimationInfo.lstSystem.Contains(3)) ? (fileGameInfo3.resistH >= 100) : (fileGameInfo3.resistAnal >= 100));
					}
					else
					{
						flag3 = fileGameInfo3.resistPain >= 100;
						flag3 |= _female.fileParam2.hAttribute == 3;
					}
					if (bAppendEV)
					{
						flag3 = true;
					}
					if (!flag3)
					{
						return false;
					}
					if (fileGameInfo3.Libido >= 50)
					{
						return false;
					}
					break;
				}
				case 2:
				{
					if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Dependence)
					{
						return false;
					}
					if (ctrlFlag.isFaintness && ctrlFlag.isFaintnessVoice)
					{
						return false;
					}
					if (hSceneManager.FemaleState[_main] == ChaFileDefine.State.Broken)
					{
						return false;
					}
					if (EventID == 7 || EventID == 32)
					{
						return false;
					}
					ChaFileGameInfo2 fileGameInfo2 = _female.fileGameInfo2;
					bool flag2 = false;
					if (!lstSystem.Contains(4))
					{
						flag2 = ((!ctrlFlag.nowAnimationInfo.lstSystem.Contains(3)) ? (fileGameInfo2.resistH >= 100) : (fileGameInfo2.resistAnal >= 100));
					}
					else
					{
						flag2 = fileGameInfo2.resistPain >= 100;
						flag2 |= _female.fileParam2.hAttribute == 3;
					}
					if (bAppendEV)
					{
						flag2 = true;
					}
					if (!flag2)
					{
						return false;
					}
					if (fileGameInfo2.Libido < 50)
					{
						return false;
					}
					break;
				}
				case 3:
					if (hSceneManager.FemaleState[_main] != ChaFileDefine.State.Dependence)
					{
						return false;
					}
					break;
				case 4:
					if (!ctrlFlag.isFaintness || !ctrlFlag.isFaintnessVoice)
					{
						return false;
					}
					if (EventID == 7 || EventID == 32)
					{
						return false;
					}
					break;
				case 5:
					if (hSceneManager.FemaleState[_main] != ChaFileDefine.State.Broken)
					{
						return false;
					}
					break;
				case 6:
					if (EventID != 7 && EventID != 32)
					{
						return false;
					}
					break;
				case 7:
					if (hSceneManager.FemaleState[_main] != ChaFileDefine.State.Blank)
					{
						return false;
					}
					break;
				case 8:
					if (hSceneManager.FemaleState[_main] != ChaFileDefine.State.Favor)
					{
						return false;
					}
					break;
				case 9:
					if (hSceneManager.FemaleState[_main] != ChaFileDefine.State.Enjoyment)
					{
						return false;
					}
					break;
				case 10:
					if (hSceneManager.FemaleState[_main] != ChaFileDefine.State.Slavery)
					{
						return false;
					}
					break;
				case 11:
					if (hSceneManager.FemaleState[_main] != ChaFileDefine.State.Aversion)
					{
						return false;
					}
					break;
				}
			}
		}
		return true;
	}

	private bool SetFace(FaceInfo _face, ChaControl _female, int _main)
	{
		FBSCtrlEyes eyesCtrl = _female.eyesCtrl;
		if (eyesCtrl != null)
		{
			blendEyes[_main].Start(eyesCtrl.OpenMax, _face.openEye, 0.3f);
		}
		FBSCtrlMouth mouthCtrl = _female.mouthCtrl;
		if (mouthCtrl != null)
		{
			blendMouths[_main].Start(mouthCtrl.OpenMin, _face.openMouthMin, 0.3f);
			blendMouthMaxs[_main].Start(mouthCtrl.OpenMax, _face.openMouthMax, 0.3f);
		}
		_female.ChangeEyebrowPtn(_face.eyeBlow);
		_female.ChangeEyesPtn(_face.eye);
		_female.ChangeMouthPtn(_face.mouth);
		if (_face.mouth == 10 || _face.mouth == 13)
		{
			_female.DisableShapeMouth(disable: true);
		}
		else
		{
			_female.DisableShapeMouth(disable: false);
		}
		_female.ChangeTearsRate(_face.tear);
		_female.ChangeHohoAkaRate(_face.cheek);
		_female.HideEyeHighlight(!_face.highlight);
		_female.ChangeEyesBlinkFlag(_face.blink);
		return true;
	}

	private bool CheckFaceList(int main, int breathkind, int mode, int kind, int id)
	{
		if (!dicFaceInfos.ContainsKey(main))
		{
			return false;
		}
		if (!dicFaceInfos[main].ContainsKey(breathkind))
		{
			return false;
		}
		if (!dicFaceInfos[main][breathkind].ContainsKey(mode))
		{
			return false;
		}
		if (!dicFaceInfos[main][breathkind][mode].ContainsKey(kind))
		{
			return false;
		}
		return dicFaceInfos[main][breathkind][mode][kind].ContainsKey(id);
	}

	public void ResetVoice()
	{
		nowVoices[0].animBreath = "";
		nowVoices[1].animBreath = "";
		nowVoices[0].animVoice = "";
		nowVoices[1].animVoice = "";
	}

	public IEnumerator Init(int _personality, float _pitch, ChaControl _param, int _personality_sub = 0, float _pitch_sub = 0f, ChaControl _param_sub = null)
	{
		personality[0] = _personality;
		voicePitch[0] = _pitch;
		personality[1] = _personality_sub;
		voicePitch[1] = _pitch_sub;
		param = _param;
		param_sub = _param_sub;
		EventID = Singleton<Game>.Instance.eventNo;
		bAppendEV = EventID >= 50 && EventID < 56;
		nowVoices[0] = new Voice();
		nowVoices[1] = new Voice();
		dicdiclstVoiceList[0] = new Dictionary<int, Dictionary<int, VoiceList>>();
		dicdiclstVoiceList[1] = new Dictionary<int, Dictionary<int, VoiceList>>();
		ShortBreathLists[0] = new ShortVoiceList();
		ShortBreathLists[1] = new ShortVoiceList();
		shortBreathPtns[0] = new ShortBreathPtn();
		shortBreathPtns[1] = new ShortBreathPtn();
		shortBreathAddPtns[0] = new ShortBreathPtn();
		shortBreathAddPtns[1] = new ShortBreathPtn();
		breathUseLists[0] = new BreathList();
		breathUseLists[1] = new BreathList();
		breathOnaniUseLists[0] = new BreathList();
		breathOnaniUseLists[1] = new BreathList();
		lstLoadVoicePtn[0] = new Dictionary<int, Dictionary<int, List<VoicePtn>>>();
		lstLoadVoicePtn[1] = new Dictionary<int, Dictionary<int, List<VoicePtn>>>();
		lstLoadVoiceAddPtn[0] = new Dictionary<int, Dictionary<int, List<VoicePtn>>>();
		lstLoadVoiceAddPtn[1] = new Dictionary<int, Dictionary<int, List<VoicePtn>>>();
		lstLoadStartVoicePtn[0] = new List<StartVoicePtn>();
		lstLoadStartVoicePtn[1] = new List<StartVoicePtn>();
		lstLoadStartVoiceAddPtn[0] = new List<StartVoicePtn>();
		lstLoadStartVoiceAddPtn[1] = new List<StartVoicePtn>();
		blendEyes[0] = new GlobalMethod.FloatBlend();
		blendEyes[1] = new GlobalMethod.FloatBlend();
		blendMouths[0] = new GlobalMethod.FloatBlend();
		blendMouths[1] = new GlobalMethod.FloatBlend();
		blendMouthMaxs[0] = new GlobalMethod.FloatBlend();
		blendMouthMaxs[1] = new GlobalMethod.FloatBlend();
		dicdicVoicePlayAnimation = new Dictionary<int, Dictionary<int, VoiceAnimationPlay>>();
		for (int i = 0; i < 2; i++)
		{
			dicBreathPtns[i] = dicBreathPtns.New();
			dicBreathUsePtns[i] = dicBreathUsePtns.New();
			dicBreathAddPtns[i] = dicBreathAddPtns.New();
			dicFaceInfos[i] = dicFaceInfos.New();
		}
		hSceneManager = Singleton<HSceneManager>.Instance;
		for (int j = 0; j < breathLists.Length; j++)
		{
			breathLists[j] = new BreathList();
		}
		for (int k = 0; k < breathLists.Length; k++)
		{
			breathOnaniLists[k] = new BreathList();
		}
		lstBreathAbnames.Clear();
		lstBreathAbnames = CommonLib.GetAssetBundleNameListFromPath(hSceneManager.strAssetBreathListFolder);
		lstVoiceAbnames.Clear();
		lstVoiceAbnames = GlobalMethod.GetAssetBundleNameListFromPath(hSceneManager.strAssetVoiceListFolder);
		lstVoiceAbnames.Sort();
		yield return null;
		yield return LoadBreathList();
		for (int l = 0; l < breathLists.Length; l++)
		{
			breathLists[l].DebugListSet();
		}
		yield return LoadVoiceList();
		yield return LoadShortBreathList();
		yield return LoadStartVoicePtnList(hSceneManager.strAssetVoiceListFolder);
		LoadFaceInfo();
	}

	private IEnumerator LoadBreathList()
	{
		for (int i = 0; i < 2; i++)
		{
			if (i == 0 || !(param_sub == null))
			{
				LoadBreath(personality[i], i);
				if (!bAppendEV)
				{
					LoadBreathOnani(personality[i], i);
				}
			}
		}
		yield return null;
		LoadBreathPtn(1, 0, 0);
		LoadBreathPtn(1, 1, 0);
		LoadBreathPtn(1, 2, 0);
		for (int j = 0; j < 6; j++)
		{
			LoadBreathPtn(1, 3, j);
		}
		LoadBreathPtn(1, 1, 1);
		LoadBreathPtn(1, 3, 6);
		LoadBreathPtn(1, 3, 7);
		tmpBreathFilenames.Clear();
		if (EventID != 52)
		{
			foreach (string lstBreathAbname in lstBreathAbnames)
			{
				if (GameSystem.IsPathAdd50(lstBreathAbname))
				{
					string[] allAssetName = AssetBundleCheck.GetAllAssetName(lstBreathAbname, _WithExtension: false);
					foreach (string filename in allAssetName)
					{
						CheckAddPattern(filename, 0, AppendEvent: false, ref tmpBreathFilenames);
					}
				}
			}
			LoadBreathAddPtn(1, 0, 0);
			LoadBreathAddPtn(1, 1, 0);
			LoadBreathAddPtn(1, 2, 0);
			LoadBreathAddPtn(1, 1, 1);
		}
		yield return null;
		if (EventID != 52)
		{
			for (int l = 0; l < 6; l++)
			{
				LoadBreathAddPtn(1, 3, l);
			}
			LoadBreathAddPtn(1, 3, 6);
			LoadBreathAddPtn(1, 3, 7);
		}
		if (MultiFemale)
		{
			LoadBreathPtn(2, 4, 0);
			for (int m = 0; m < 4; m++)
			{
				LoadBreathPtn(2, 5, m);
			}
			if (EventID != 52)
			{
				LoadBreathAddPtn(2, 4, 0);
				for (int n = 0; n < 4; n++)
				{
					LoadBreathAddPtn(2, 5, n);
				}
			}
			yield return null;
		}
		if (MultiMale)
		{
			for (int num = 0; num < 2; num++)
			{
				LoadBreathPtn(1, 6, num);
			}
			if (EventID != 52)
			{
				for (int num2 = 0; num2 < 2; num2++)
				{
					LoadBreathAddPtn(1, 6, num2);
				}
			}
		}
		yield return null;
		yield return null;
	}

	private bool LoadBreath(int _personality, int _main)
	{
		sbLoadFile.Clear();
		if (EventID != 52)
		{
			sbLoadFile.AppendFormat("HBreath_c{0:00}", _personality);
		}
		else
		{
			sbLoadFile.AppendFormat("HBreathEV_c{0:00}", _personality);
		}
		lst.Clear();
		GlobalMethod.LoadAllListTextFromList(lstBreathAbnames, sbLoadFile.ToString(), ref lst);
		StartCoroutine(LoadBreathBase(lst, breathLists[_main]));
		return true;
	}

	private bool LoadBreathOnani(int _personality, int _main)
	{
		sbLoadFile.Clear();
		sbLoadFile.AppendFormat("HBreath_c{0:00}_00", _personality);
		lstOnani.Clear();
		GlobalMethod.LoadAllListTextFromList(lstBreathAbnames, sbLoadFile.ToString(), ref lstOnani);
		StartCoroutine(LoadBreathBase(lstOnani, breathOnaniLists[_main]));
		return true;
	}

	private IEnumerator LoadBreathBase(List<string> _lst, BreathList _breath)
	{
		if (_lst.Count == 0)
		{
			yield break;
		}
		for (int i = 0; i < _lst.Count; i++)
		{
			GlobalMethod.GetListString(_lst[i], out var data);
			int num = data.Length;
			int[] array = new int[7];
			for (int j = 0; j < array.Length; j++)
			{
				if (data[0].Length <= j || data[0][j].IsNullOrEmpty())
				{
					array[j] = 0;
				}
				else if (!int.TryParse(data[0][j], out array[j]))
				{
					array[j] = 0;
				}
			}
			int key = 0;
			for (int k = 1; k < num; k++)
			{
				int num2 = 0;
				for (int l = 0; l < array.Length; l++)
				{
					num2 += array[l];
					if (array[l] != 0 && k - 1 < num2)
					{
						key = l;
						break;
					}
				}
				if (!_breath.dicdicVoiceList.ContainsKey(key))
				{
					_breath.dicdicVoiceList.Add(key, new Dictionary<int, VoiceListInfo>());
				}
				Dictionary<int, VoiceListInfo> dictionary = _breath.dicdicVoiceList[key];
				int key2 = int.Parse(data[k][0]);
				if (!dictionary.ContainsKey(key2))
				{
					dictionary.Add(key2, new VoiceListInfo());
				}
				VoiceListInfo voiceListInfo = dictionary[key2];
				voiceListInfo.pathAsset = data[k][1];
				voiceListInfo.nameFile = data[k][2];
				int.TryParse(data[k][3], out voiceListInfo.group);
				voiceListInfo.notOverWrite = data[k][4] == "1";
				voiceListInfo.urine = data[k][5] == "1";
				voiceListInfo.lstNotHitFace.Clear();
				for (int m = 0; m < 3; m++)
				{
					LoadBreathFace(data, k, m + 6, voiceListInfo.lstNotHitFace);
				}
				voiceListInfo.lstHitFace.Clear();
				LoadBreathFace(data, k, 9, voiceListInfo.lstHitFace);
			}
		}
		yield return null;
	}

	private void LoadBreathFace(string[][] _aastr, int _line, int _idx, List<int> _lst)
	{
		int result = -1;
		if (int.TryParse(_aastr[_line][_idx], out result))
		{
			_lst.Add(result);
		}
	}

	private bool LoadBreathPtn(int personal, int mode, int kind)
	{
		sbLoadFile.Clear();
		if (EventID != 52)
		{
			sbLoadFile.AppendFormat("HBreathPattern_{0:00}_{1:00}", mode, kind);
		}
		else
		{
			sbLoadFile.AppendFormat("HBreathPatternEV_{0:00}_{1:00}", mode, kind);
		}
		string text = GlobalMethod.LoadAllListText(lstBreathAbnames, sbLoadFile.ToString());
		if (text.IsNullOrEmpty())
		{
			return false;
		}
		GlobalMethod.GetListString(text, out var data);
		int num = data.Length;
		int num2 = ((num != 0) ? data[0].Length : 0);
		if (!dicBreathPtns[0].ContainsKey(mode))
		{
			dicBreathPtns[0][mode] = dicBreathPtns[0].New();
		}
		if (!dicBreathPtns[0][mode].ContainsKey(kind))
		{
			dicBreathPtns[0][mode][kind] = dicBreathPtns[0][mode].New();
		}
		if (personal == 2 && (mode == 4 || mode == 5))
		{
			if (!dicBreathPtns[1].ContainsKey(mode))
			{
				dicBreathPtns[1][mode] = dicBreathPtns[1].New();
			}
			if (!dicBreathPtns[1][mode].ContainsKey(kind))
			{
				dicBreathPtns[1][mode][kind] = dicBreathPtns[1][mode].New();
			}
		}
		for (int i = 0; i < num; i++)
		{
			int key = int.Parse(data[i][0]);
			int num3 = int.Parse(data[i][1]);
			string text2 = data[i][2];
			int num4 = int.Parse(data[i][7]);
			List<BreathVoicePtnInfo> list = new List<BreathVoicePtnInfo>();
			for (int j = 8; j < num2; j += 3)
			{
				string text3 = data[i][j];
				if (text3.IsNullOrEmpty())
				{
					break;
				}
				BreathVoicePtnInfo breathVoicePtnInfo = new BreathVoicePtnInfo();
				string[] array = text3.Split(',');
				int result = 0;
				for (int k = 0; k < array.Length; k++)
				{
					if (int.TryParse(array[k], out result))
					{
						breathVoicePtnInfo.lstConditions.Add(result);
					}
				}
				array = data[i][j + 1].Split(',');
				for (int l = 0; l < array.Length; l++)
				{
					if (int.TryParse(array[l], out result))
					{
						breathVoicePtnInfo.lstAnimeID.Add(result);
					}
				}
				array = data[i][j + 2].Split(',');
				for (int m = 0; m < array.Length; m++)
				{
					if (int.TryParse(array[m], out result))
					{
						breathVoicePtnInfo.lstVoice.Add(result);
					}
				}
				list.Add(breathVoicePtnInfo);
			}
			for (int n = 0; n < personal; n++)
			{
				if (n != 1 || mode == 4 || mode == 5)
				{
					ValueDictionary<int, int, string, Dictionary<int, BreathPtn>> valueDictionary = dicBreathPtns[n][mode][kind];
					if (!valueDictionary.ContainsKey(key))
					{
						valueDictionary[key] = valueDictionary.New();
					}
					if (!valueDictionary[key].ContainsKey(num3))
					{
						valueDictionary[key][num3] = valueDictionary[key].New();
					}
					if (!valueDictionary[key][num3].ContainsKey(text2))
					{
						valueDictionary[key][num3].Add(text2, new Dictionary<int, BreathPtn>());
					}
					Dictionary<int, BreathPtn> dictionary = valueDictionary[key][num3][text2];
					if (!dictionary.ContainsKey(num4))
					{
						dictionary.Add(num4, new BreathPtn());
					}
					dictionary[num4].level = num3;
					dictionary[num4].anim = text2;
					dictionary[num4].onlyOne = data[i][3] == "1";
					dictionary[num4].isPlay = false;
					dictionary[num4].force = data[i][4] == "1";
					dictionary[num4].timeChangeFaceMin = float.Parse(data[i][5]);
					dictionary[num4].timeChangeFaceMax = float.Parse(data[i][6]);
					dictionary[num4].paramPtn = num4;
					dictionary[num4].lstInfo = new List<BreathVoicePtnInfo>(list);
				}
			}
		}
		return true;
	}

	private bool LoadBreathAddPtn(int personal, int mode, int kind)
	{
		foreach (string tmpBreathFilename in tmpBreathFilenames)
		{
			if (int.Parse(Regex.Match(tmpBreathFilename, "[0-9]{2,2}").Value) != mode)
			{
				continue;
			}
			string text = GlobalMethod.LoadAllListText(lstBreathAbnames, tmpBreathFilename);
			if (text.IsNullOrEmpty())
			{
				continue;
			}
			GlobalMethod.GetListString(text, out var data);
			int num = data.Length;
			int num2 = ((num != 0) ? data[0].Length : 0);
			if (!dicBreathAddPtns[0].ContainsKey(mode))
			{
				dicBreathAddPtns[0][mode] = dicBreathAddPtns[0].New();
			}
			if (!dicBreathAddPtns[0][mode].ContainsKey(kind))
			{
				dicBreathAddPtns[0][mode][kind] = dicBreathAddPtns[0][mode].New();
			}
			if (personal == 2 && (mode == 4 || mode == 5))
			{
				if (!dicBreathAddPtns[1].ContainsKey(mode))
				{
					dicBreathAddPtns[1][mode] = dicBreathAddPtns[1].New();
				}
				if (!dicBreathAddPtns[1][mode].ContainsKey(kind))
				{
					dicBreathAddPtns[1][mode][kind] = dicBreathAddPtns[1][mode].New();
				}
			}
			for (int i = 0; i < num; i++)
			{
				int key = int.Parse(data[i][0]);
				int num3 = int.Parse(data[i][1]);
				string text2 = data[i][2];
				int num4 = int.Parse(data[i][7]);
				List<BreathVoicePtnInfo> list = new List<BreathVoicePtnInfo>();
				for (int j = 8; j < num2; j += 3)
				{
					string text3 = data[i][j];
					if (text3 == "")
					{
						break;
					}
					BreathVoicePtnInfo bvi = new BreathVoicePtnInfo();
					string[] array = text3.Split(',');
					int result = 0;
					for (int k = 0; k < array.Length; k++)
					{
						if (int.TryParse(array[k], out result))
						{
							bvi.lstConditions.Add(result);
						}
					}
					array = data[i][j + 1].Split(',');
					for (int l = 0; l < array.Length; l++)
					{
						if (int.TryParse(array[l], out result))
						{
							bvi.lstAnimeID.Add(result);
						}
					}
					array = data[i][j + 2].Split(',');
					for (int m = 0; m < array.Length; m++)
					{
						if (int.TryParse(array[m], out result))
						{
							bvi.lstVoice.Add(result);
						}
					}
					BreathVoicePtnInfo breathVoicePtnInfo = list.Find((BreathVoicePtnInfo f) => f.lstConditions.SequenceEqual(bvi.lstConditions) && f.lstAnimeID.SequenceEqual(bvi.lstAnimeID));
					if (breathVoicePtnInfo == null)
					{
						list.Add(bvi);
					}
					else
					{
						breathVoicePtnInfo.lstVoice = new List<int>(bvi.lstVoice);
					}
				}
				for (int num5 = 0; num5 < personal; num5++)
				{
					if (num5 != 1 || mode == 4 || mode == 5)
					{
						ValueDictionary<int, int, string, Dictionary<int, BreathPtn>> valueDictionary = dicBreathAddPtns[num5][mode][kind];
						if (!valueDictionary.ContainsKey(key))
						{
							valueDictionary[key] = valueDictionary.New();
						}
						if (!valueDictionary[key].ContainsKey(num3))
						{
							valueDictionary[key][num3] = valueDictionary[key].New();
						}
						if (!valueDictionary[key][num3].ContainsKey(text2))
						{
							valueDictionary[key][num3].Add(text2, new Dictionary<int, BreathPtn>());
						}
						Dictionary<int, BreathPtn> dictionary = valueDictionary[key][num3][text2];
						if (!dictionary.ContainsKey(num4))
						{
							dictionary.Add(num4, new BreathPtn());
						}
						dictionary[num4].level = num3;
						dictionary[num4].anim = text2;
						dictionary[num4].onlyOne = data[i][3] == "1";
						dictionary[num4].isPlay = false;
						dictionary[num4].force = data[i][4] == "1";
						dictionary[num4].timeChangeFaceMin = float.Parse(data[i][5]);
						dictionary[num4].timeChangeFaceMax = float.Parse(data[i][6]);
						dictionary[num4].paramPtn = num4;
						if (dictionary[num4].lstInfo == null || dictionary[num4].lstInfo.Count == 0)
						{
							dictionary[num4].lstInfo = new List<BreathVoicePtnInfo>(list);
						}
						else
						{
							dictionary[num4].lstInfo.AddRange(list);
						}
					}
				}
			}
		}
		for (int num6 = 0; num6 < personal; num6++)
		{
			if (num6 != 1 || mode == 4 || mode == 5)
			{
				LoadBreathPtnComposition(num6, mode, kind);
			}
		}
		return true;
	}

	private bool LoadBreathPtnComposition(int personal, int mode, int kind)
	{
		if (!dicBreathPtns.ContainsKey(personal))
		{
			return false;
		}
		if (!dicBreathPtns[personal].ContainsKey(mode))
		{
			return false;
		}
		if (!dicBreathPtns[personal][mode].ContainsKey(kind))
		{
			return false;
		}
		ValueDictionary<int, int, string, Dictionary<int, BreathPtn>> valueDictionary = dicBreathPtns[personal][mode][kind];
		if (!dicBreathAddPtns.ContainsKey(personal))
		{
			return false;
		}
		if (!dicBreathAddPtns[personal].ContainsKey(mode))
		{
			return false;
		}
		if (!dicBreathAddPtns[personal][mode].ContainsKey(kind))
		{
			return false;
		}
		ValueDictionary<int, int, string, Dictionary<int, BreathPtn>> valueDictionary2 = dicBreathAddPtns[personal][mode][kind];
		foreach (KeyValuePair<int, ValueDictionary<int, string, Dictionary<int, BreathPtn>>> item in valueDictionary)
		{
			if (!valueDictionary2.ContainsKey(item.Key))
			{
				continue;
			}
			foreach (KeyValuePair<int, ValueDictionary<string, Dictionary<int, BreathPtn>>> item2 in item.Value)
			{
				if (!valueDictionary2[item.Key].ContainsKey(item2.Key))
				{
					continue;
				}
				foreach (KeyValuePair<string, Dictionary<int, BreathPtn>> item3 in item2.Value)
				{
					if (!valueDictionary2[item.Key][item2.Key].ContainsKey(item3.Key))
					{
						continue;
					}
					foreach (KeyValuePair<int, BreathPtn> item4 in item3.Value)
					{
						if (valueDictionary2[item.Key][item2.Key][item3.Key].ContainsKey(item4.Key))
						{
							item4.Value.lstInfo.AddRange(valueDictionary2[item.Key][item2.Key][item3.Key][item4.Key].lstInfo);
						}
					}
				}
			}
		}
		return true;
	}

	private IEnumerator LoadVoiceList()
	{
		if (Masturbation)
		{
			yield break;
		}
		for (int nLoopCharCnt = 0; nLoopCharCnt < 2; nLoopCharCnt++)
		{
			if (nLoopCharCnt != 0 && param_sub == null)
			{
				continue;
			}
			for (int i = 0; i < 2; i++)
			{
				int kind = i;
				LoadVoice(personality[nLoopCharCnt], 0, kind, nLoopCharCnt);
				LoadVoice(personality[nLoopCharCnt], 1, kind, nLoopCharCnt);
				LoadVoice(personality[nLoopCharCnt], 2, kind, nLoopCharCnt);
			}
			yield return null;
			for (int j = 0; j < 12; j++)
			{
				int kind2 = j;
				LoadVoice(personality[nLoopCharCnt], 3, kind2, nLoopCharCnt);
			}
			for (int k = 0; k < 2; k++)
			{
				int kind3 = 12 + k;
				LoadVoice(personality[nLoopCharCnt], 3, kind3, nLoopCharCnt);
			}
			for (int l = 0; l < 2; l++)
			{
				int kind4 = 14 + l;
				LoadVoice(personality[nLoopCharCnt], 3, kind4, nLoopCharCnt);
			}
			for (int m = 0; m < 2; m++)
			{
				int kind5 = m;
				LoadVoice(personality[nLoopCharCnt], 7, kind5, nLoopCharCnt);
			}
			yield return null;
			if (MultiFemale)
			{
				for (int n = 0; n < 2; n++)
				{
					int kind6 = n;
					LoadVoice(personality[nLoopCharCnt], 4, kind6, nLoopCharCnt);
					LoadVoice(personality[nLoopCharCnt], 5, kind6, nLoopCharCnt);
				}
				for (int num = 0; num < 2; num++)
				{
					int kind7 = 2 + num;
					LoadVoice(personality[nLoopCharCnt], 5, kind7, nLoopCharCnt);
				}
			}
			if (MultiMale)
			{
				for (int num2 = 0; num2 < 2; num2++)
				{
					int kind8 = num2;
					LoadVoice(personality[nLoopCharCnt], 6, kind8, nLoopCharCnt);
				}
			}
		}
		yield return null;
		LoadVoicePtn(0, 0, 1);
		LoadVoicePtn(1, 0, 1);
		LoadVoicePtn(2, 0, 1);
		for (int num3 = 0; num3 < 4; num3++)
		{
			int kind9 = num3;
			LoadVoicePtn(3, kind9, 1);
		}
		LoadVoicePtn(3, 4, 1);
		LoadVoicePtn(3, 5, 1);
		tmpVoiceFilenames.Clear();
		if (!bAppendEV)
		{
			foreach (string lstVoiceAbname in lstVoiceAbnames)
			{
				if (GameSystem.IsPathAdd50(lstVoiceAbname))
				{
					string[] allAssetName = AssetBundleCheck.GetAllAssetName(lstVoiceAbname, _WithExtension: false);
					foreach (string filename in allAssetName)
					{
						CheckAddPattern(filename, 1, bAppendEV, ref tmpVoiceFilenames, isAppendList: true);
					}
				}
			}
			LoadVoiceAddPtn(0, 0, 1);
			LoadVoiceAddPtn(1, 0, 1);
			LoadVoiceAddPtn(2, 0, 1);
			for (int num5 = 0; num5 < 4; num5++)
			{
				int kind10 = num5;
				LoadVoiceAddPtn(3, kind10, 1);
			}
			LoadVoiceAddPtn(3, 4, 1);
			LoadVoiceAddPtn(3, 5, 1);
		}
		if (multiFemale)
		{
			LoadVoicePtn(4, 0, 2);
			for (int num6 = 0; num6 < 4; num6++)
			{
				int kind11 = num6;
				LoadVoicePtn(5, kind11, 2);
			}
			if (!bAppendEV)
			{
				LoadVoiceAddPtn(4, 0, 2);
				for (int num7 = 0; num7 < 4; num7++)
				{
					int kind12 = num7;
					LoadVoiceAddPtn(5, kind12, 2);
				}
			}
		}
		if (multiMale)
		{
			for (int num8 = 0; num8 < 2; num8++)
			{
				int kind13 = num8;
				LoadVoicePtn(6, kind13, 1);
			}
			if (!bAppendEV)
			{
				for (int num9 = 0; num9 < 2; num9++)
				{
					int kind14 = num9;
					LoadVoiceAddPtn(6, kind14, 1);
				}
			}
		}
		yield return null;
	}

	private bool LoadVoice(int _personality, int _mode, int _kind, int _famale)
	{
		sbLoadFile.Clear();
		if (!bAppendEV)
		{
			if (_mode < 7)
			{
				sbLoadFile.AppendFormat("HVoice_c{0:00}_{1:00}_{2:00}", _personality, _mode, _kind);
			}
			else
			{
				sbLoadFile.AppendFormat("HvoiceStart_c{0:00}_{1:00}", _personality, _kind);
			}
		}
		else if (_mode < 7)
		{
			sbLoadFile.AppendFormat("HvoiceEV{0:00}_c{1:00}_{2:00}_{3:00}", EventID, _personality, _mode, _kind);
		}
		else
		{
			sbLoadFile.AppendFormat("HvoiceStartEV{0:00}_c{1:00}_{2:00}", EventID, _personality, _kind);
		}
		lst.Clear();
		GlobalMethod.LoadAllListTextFromList(lstVoiceAbnames, sbLoadFile.ToString(), ref lst);
		foreach (string item in lst)
		{
			if (!(item == ""))
			{
				GlobalMethod.GetListString(item, out var data);
				int endY = data.Length;
				int line = 0;
				VoiceList voiceList = null;
				if (!dicdiclstVoiceList[_famale].ContainsKey(_mode))
				{
					dicdiclstVoiceList[_famale].Add(_mode, new Dictionary<int, VoiceList>());
					dicdiclstVoiceList[_famale][_mode].Add(_kind, new VoiceList());
				}
				else if (!dicdiclstVoiceList[_famale][_mode].ContainsKey(_kind))
				{
					dicdiclstVoiceList[_famale][_mode].Add(_kind, new VoiceList());
				}
				voiceList = dicdiclstVoiceList[_famale][_mode][_kind];
				LoadVoiceFace(data, endY, ref line, voiceList.dicdicVoiceList, _mode);
			}
		}
		return true;
	}

	private void LoadVoiceFace(string[][] aastr, int endY, ref int line, Dictionary<int, VoiceListInfo>[] dic, int mode)
	{
		int num = -1;
		bool flag = mode == 3;
		int[] array = (flag ? new int[7] : new int[6]);
		for (int i = 0; i < array.Length; i++)
		{
			if (aastr[line].Length <= i)
			{
				array[i] = 0;
			}
			else if (aastr[line][i].IsNullOrEmpty() || !int.TryParse(aastr[line][i], out array[i]))
			{
				array[i] = 0;
			}
		}
		line++;
		while (line < endY)
		{
			int num2 = 0;
			num = -1;
			if (flag)
			{
				num2 += array[6];
				if (array[6] != 0 && line - 1 < num2)
				{
					num = 6;
				}
			}
			if (num != 6)
			{
				for (int j = 0; j < 6; j++)
				{
					num2 += array[j];
					if (array[j] != 0 && line - 1 < num2)
					{
						num = j;
						break;
					}
				}
			}
			int num3 = 0;
			int num4 = aastr[line].Length;
			if (!int.TryParse(aastr[line][1], out var result))
			{
				line++;
				continue;
			}
			VoiceListInfo voiceListInfo = new VoiceListInfo();
			voiceListInfo.word = aastr[line][num3];
			num3 += 2;
			voiceListInfo.pathAsset = aastr[line][num3++];
			voiceListInfo.nameFile = aastr[line][num3++];
			if (num4 < 5)
			{
				line++;
				continue;
			}
			voiceListInfo.voiceKind = int.Parse(aastr[line][num3++]);
			voiceListInfo.notOverWrite = aastr[line][num3++] == "1";
			int result2 = 0;
			if (!int.TryParse(aastr[line][num3++], out result2))
			{
				result2 = 0;
			}
			voiceListInfo.lstHitFace.Add(result2);
			if (num == -1 || dic.Length <= num)
			{
				line++;
				continue;
			}
			if (!dic[num].ContainsKey(result))
			{
				dic[num].Add(result, voiceListInfo);
			}
			else
			{
				dic[num][result] = voiceListInfo;
			}
			line++;
		}
	}

	private bool LoadVoicePtn(int taii, int kind, int charNum)
	{
		sbLoadFile.Clear();
		if (!bAppendEV)
		{
			sbLoadFile.AppendFormat("HVoicePattern_{0:00}_{1:00}", taii, kind);
		}
		else
		{
			sbLoadFile.AppendFormat("HVoicePatternEV{0:00}_{1:00}_{2:00}", EventID, taii, kind);
		}
		if (!lstLoadVoicePtn[0].ContainsKey(taii))
		{
			lstLoadVoicePtn[0].Add(taii, new Dictionary<int, List<VoicePtn>>());
			lstLoadVoicePtn[0][taii].Add(kind, new List<VoicePtn>());
		}
		else if (!lstLoadVoicePtn[0][taii].ContainsKey(kind))
		{
			lstLoadVoicePtn[0][taii].Add(kind, new List<VoicePtn>());
		}
		if (charNum == 2)
		{
			if (!lstLoadVoicePtn[1].ContainsKey(taii))
			{
				lstLoadVoicePtn[1].Add(taii, new Dictionary<int, List<VoicePtn>>());
				lstLoadVoicePtn[1][taii].Add(kind, new List<VoicePtn>());
			}
			else if (!lstLoadVoicePtn[1][taii].ContainsKey(kind))
			{
				lstLoadVoicePtn[1][taii].Add(kind, new List<VoicePtn>());
			}
		}
		lst.Clear();
		GlobalMethod.LoadAllListTextFromList(lstVoiceAbnames, sbLoadFile.ToString(), ref lst);
		CheckVoicePtn ptn = default(CheckVoicePtn);
		for (int i = 0; i < lst.Count; i++)
		{
			GlobalMethod.GetListString(lst[i], out var data);
			int num = 0;
			int num2 = data.Length;
			while (num < num2)
			{
				int num3 = data[num].Length;
				int result = -1;
				if (!int.TryParse(data[num][0], out result))
				{
					num++;
					continue;
				}
				ptn.condition = result;
				if (!int.TryParse(data[num][1], out result))
				{
					result = 0;
				}
				ptn.howTalk = result;
				ptn.LookFlag = data[num][2] == "0";
				ptn.anim = data[num][3];
				if (!int.TryParse(data[num][4], out result))
				{
					result = -1;
				}
				ptn.paramPtn = result;
				List<VoicePtnInfo> list = new List<VoicePtnInfo>();
				int num4 = 5;
				while (num4 < num3)
				{
					VoicePtnInfo voicePtnInfo = new VoicePtnInfo();
					if (data[num][num4].IsNullOrEmpty())
					{
						break;
					}
					string[] array = data[num][num4++].Split(',');
					for (int j = 0; j < array.Length; j++)
					{
						voicePtnInfo.lstAnimList.Add(int.Parse(array[j]));
					}
					if (data[num][num4].IsNullOrEmpty())
					{
						break;
					}
					array = data[num][num4++].Split(',');
					for (int k = 0; k < array.Length; k++)
					{
						voicePtnInfo.lstPlayConditions.Add(int.Parse(array[k]));
					}
					if (data[num][num4].IsNullOrEmpty())
					{
						break;
					}
					voicePtnInfo.loadListmode = int.Parse(data[num][num4++]);
					if (data[num][num4].IsNullOrEmpty())
					{
						break;
					}
					voicePtnInfo.loadListKind = int.Parse(data[num][num4++]);
					if (data[num][num4].IsNullOrEmpty())
					{
						break;
					}
					array = data[num][num4++].Split(',');
					for (int l = 0; l < array.Length; l++)
					{
						voicePtnInfo.lstVoice.Add(int.Parse(array[l]));
					}
					list.Add(voicePtnInfo);
					voicePtnInfo.loadState = ptn.condition;
				}
				for (int m = 0; m < charNum; m++)
				{
					int num5 = CheckPtn(lstLoadVoicePtn[m][taii][kind], ptn);
					if (num5 == -1)
					{
						lstLoadVoicePtn[m][taii][kind].Add(new VoicePtn
						{
							condition = ptn.condition,
							howTalk = ptn.howTalk,
							LookFlag = ptn.LookFlag,
							anim = ptn.anim,
							paramPtn = ptn.paramPtn,
							lstInfo = new List<VoicePtnInfo>(list)
						});
					}
					else
					{
						lstLoadVoicePtn[m][taii][kind][num5] = new VoicePtn
						{
							condition = ptn.condition,
							howTalk = ptn.howTalk,
							LookFlag = ptn.LookFlag,
							anim = ptn.anim,
							paramPtn = ptn.paramPtn,
							lstInfo = new List<VoicePtnInfo>(list)
						};
					}
				}
				num++;
			}
		}
		return true;
	}

	private int CheckPtn(List<VoicePtn> ptns, CheckVoicePtn ptn)
	{
		int result = -1;
		VoicePtn voicePtn = null;
		for (int i = 0; i < ptns.Count; i++)
		{
			voicePtn = ptns[i];
			if (voicePtn != null && voicePtn.condition == ptn.condition && voicePtn.howTalk == ptn.howTalk && voicePtn.LookFlag == ptn.LookFlag && !(voicePtn.anim != ptn.anim) && voicePtn.paramPtn == ptn.paramPtn)
			{
				result = i;
			}
		}
		return result;
	}

	private bool LoadVoiceAddPtn(int taii, int kind, int charNum)
	{
		CheckVoicePtn ptn = default(CheckVoicePtn);
		foreach (string tmpVoiceFilename in tmpVoiceFilenames)
		{
			if (int.Parse(Regex.Match(tmpVoiceFilename, "[0-9]{2,2}").Value) != taii)
			{
				continue;
			}
			lst.Clear();
			GlobalMethod.LoadAllListTextFromList(lstVoiceAbnames, tmpVoiceFilename, ref lst);
			if (!lstLoadVoiceAddPtn[0].ContainsKey(taii))
			{
				lstLoadVoiceAddPtn[0].Add(taii, new Dictionary<int, List<VoicePtn>>());
				lstLoadVoiceAddPtn[0][taii].Add(kind, new List<VoicePtn>());
			}
			else if (!lstLoadVoiceAddPtn[0][taii].ContainsKey(kind))
			{
				lstLoadVoiceAddPtn[0][taii].Add(kind, new List<VoicePtn>());
			}
			if (charNum == 2)
			{
				if (!lstLoadVoiceAddPtn[1].ContainsKey(taii))
				{
					lstLoadVoiceAddPtn[1].Add(taii, new Dictionary<int, List<VoicePtn>>());
					lstLoadVoiceAddPtn[1][taii].Add(kind, new List<VoicePtn>());
				}
				else if (!lstLoadVoiceAddPtn[1][taii].ContainsKey(kind))
				{
					lstLoadVoiceAddPtn[1][taii].Add(kind, new List<VoicePtn>());
				}
			}
			for (int i = 0; i < lst.Count; i++)
			{
				GlobalMethod.GetListString(lst[i], out var data);
				int num = 0;
				int num2 = data.Length;
				while (num < num2)
				{
					int num3 = data[num].Length;
					int result = -1;
					if (!int.TryParse(data[num][0], out result))
					{
						num++;
						continue;
					}
					ptn.condition = result;
					if (!int.TryParse(data[num][1], out result))
					{
						result = 0;
					}
					ptn.howTalk = result;
					ptn.LookFlag = data[num][2] == "0";
					ptn.anim = data[num][3];
					if (!int.TryParse(data[num][4], out result))
					{
						result = -1;
					}
					ptn.paramPtn = result;
					List<VoicePtnInfo> list = new List<VoicePtnInfo>();
					int num4 = 5;
					while (num4 < num3)
					{
						VoicePtnInfo voicePtnInfo = new VoicePtnInfo();
						if (data[num][num4].IsNullOrEmpty())
						{
							break;
						}
						string[] array = data[num][num4++].Split(',');
						for (int j = 0; j < array.Length; j++)
						{
							voicePtnInfo.lstAnimList.Add(int.Parse(array[j]));
						}
						if (data[num][num4].IsNullOrEmpty())
						{
							break;
						}
						array = data[num][num4++].Split(',');
						for (int k = 0; k < array.Length; k++)
						{
							voicePtnInfo.lstPlayConditions.Add(int.Parse(array[k]));
						}
						if (data[num][num4].IsNullOrEmpty())
						{
							break;
						}
						voicePtnInfo.loadListmode = int.Parse(data[num][num4++]);
						if (data[num][num4].IsNullOrEmpty())
						{
							break;
						}
						voicePtnInfo.loadListKind = int.Parse(data[num][num4++]);
						if (data[num][num4].IsNullOrEmpty())
						{
							break;
						}
						array = data[num][num4++].Split(',');
						for (int l = 0; l < array.Length; l++)
						{
							voicePtnInfo.lstVoice.Add(int.Parse(array[l]));
						}
						list.Add(voicePtnInfo);
						voicePtnInfo.loadState = ptn.condition;
					}
					for (int m = 0; m < charNum; m++)
					{
						int num5 = CheckPtn(lstLoadVoiceAddPtn[m][taii][kind], ptn);
						if (num5 == -1)
						{
							lstLoadVoiceAddPtn[m][taii][kind].Add(new VoicePtn
							{
								condition = ptn.condition,
								howTalk = ptn.howTalk,
								LookFlag = ptn.LookFlag,
								anim = ptn.anim,
								paramPtn = ptn.paramPtn,
								lstInfo = new List<VoicePtnInfo>(list)
							});
						}
						else
						{
							lstLoadVoiceAddPtn[m][taii][kind][num5].lstInfo.AddRange(list);
						}
					}
					num++;
				}
			}
		}
		for (int n = 0; n < charNum; n++)
		{
			if (n != 1 || taii == 4 || taii == 5)
			{
				LoadVoicePtnComposition(n, taii, kind);
			}
		}
		return true;
	}

	private bool LoadVoicePtnComposition(int personal, int mode, int kind)
	{
		if (lstLoadVoicePtn.Length <= personal)
		{
			return false;
		}
		if (!lstLoadVoicePtn[personal].ContainsKey(mode))
		{
			return false;
		}
		if (!lstLoadVoicePtn[personal][mode].ContainsKey(kind))
		{
			return false;
		}
		List<VoicePtn> list = lstLoadVoicePtn[personal][mode][kind];
		if (lstLoadVoiceAddPtn.Length <= personal)
		{
			return false;
		}
		if (!lstLoadVoiceAddPtn[personal].ContainsKey(mode))
		{
			return false;
		}
		if (!lstLoadVoiceAddPtn[personal][mode].ContainsKey(kind))
		{
			return false;
		}
		List<VoicePtn> list2 = lstLoadVoiceAddPtn[personal][mode][kind];
		foreach (VoicePtn item in list)
		{
			foreach (VoicePtn item2 in list2)
			{
				if (item.condition == item2.condition && item.howTalk == item2.howTalk && item.LookFlag == item2.LookFlag && !(item.anim != item2.anim) && item.paramPtn == item2.paramPtn)
				{
					item.lstInfo.AddRange(item2.lstInfo);
				}
			}
		}
		return true;
	}

	private IEnumerator LoadShortBreathList()
	{
		for (int i = 0; i < 2; i++)
		{
			if (i == 0 || !(param_sub == null))
			{
				LoadShortBreath(personality[i], i);
			}
		}
		LoadShortBreathPtn(1, 0, 0);
		LoadShortBreathPtn(1, 1, 0);
		LoadShortBreathPtn(1, 2, 0);
		for (int j = 0; j < 4; j++)
		{
			LoadShortBreathPtn(1, 3, j);
		}
		LoadShortBreathPtn(1, 1, 1);
		LoadShortBreathPtn(1, 3, 4);
		LoadShortBreathPtn(1, 3, 5);
		tmpBreathFilenames.Clear();
		if (EventID != 52)
		{
			foreach (string lstBreathAbname in lstBreathAbnames)
			{
				if (GameSystem.IsPathAdd50(lstBreathAbname))
				{
					string[] allAssetName = AssetBundleCheck.GetAllAssetName(lstBreathAbname, _WithExtension: false);
					foreach (string filename in allAssetName)
					{
						CheckAddPattern(filename, 2, AppendEvent: false, ref tmpBreathFilenames);
					}
				}
			}
			LoadShortBreathAddPtn(1, 0, 0);
			LoadShortBreathAddPtn(1, 1, 0);
			LoadShortBreathAddPtn(1, 2, 0);
			for (int l = 0; l < 4; l++)
			{
				LoadShortBreathAddPtn(1, 3, l);
			}
			LoadShortBreathAddPtn(1, 1, 1);
			LoadShortBreathAddPtn(1, 3, 4);
			LoadShortBreathAddPtn(1, 3, 5);
		}
		if (MultiFemale)
		{
			LoadShortBreathPtn(2, 4, 0);
			for (int m = 0; m < 4; m++)
			{
				LoadShortBreathPtn(2, 5, m);
			}
			if (EventID != 52)
			{
				LoadShortBreathAddPtn(2, 4, 1);
				for (int n = 0; n < 4; n++)
				{
					LoadShortBreathAddPtn(2, 5, n);
				}
			}
		}
		if (MultiMale)
		{
			for (int num = 0; num < 2; num++)
			{
				LoadShortBreathPtn(1, 6, num);
			}
			if (EventID != 52)
			{
				for (int num2 = 0; num2 < 2; num2++)
				{
					LoadShortBreathAddPtn(1, 6, num2);
				}
			}
		}
		yield return null;
	}

	private bool LoadShortBreath(int _personality, int _main)
	{
		sbLoadFile.Clear();
		if (EventID != 52)
		{
			sbLoadFile.AppendFormat("HShort_breath_c{0:00}", _personality);
		}
		else
		{
			sbLoadFile.AppendFormat("HShort_breathEV_c{0:00}", _personality);
		}
		string text = GlobalMethod.LoadAllListText(lstBreathAbnames, sbLoadFile.ToString());
		if (text.IsNullOrEmpty())
		{
			return false;
		}
		GlobalMethod.GetListString(text, out var data);
		int num = data.Length;
		int num2 = ((num != 0) ? data[0].Length : 0);
		for (int i = 0; i < num; i++)
		{
			int result = 0;
			int num3 = 0;
			if (!int.TryParse(data[i][num3++], out result))
			{
				continue;
			}
			if (!ShortBreathLists[_main].dicShortBreathLists.ContainsKey(result))
			{
				ShortBreathLists[_main].dicShortBreathLists.Add(result, new VoiceListInfo());
			}
			VoiceListInfo voiceListInfo = ShortBreathLists[_main].dicShortBreathLists[result];
			voiceListInfo.pathAsset = data[i][num3++];
			voiceListInfo.nameFile = data[i][num3++];
			voiceListInfo.notOverWrite = data[i][num3++] == "1";
			voiceListInfo.lstHitFace.Clear();
			for (int j = num3; j < num2; j++)
			{
				int result2 = -1;
				if (int.TryParse(data[i][j], out result2))
				{
					voiceListInfo.lstHitFace.Add(result2);
				}
			}
		}
		return true;
	}

	private bool LoadShortBreathPtn(int _main, int _mode, int _kind)
	{
		sbLoadFile.Clear();
		if (EventID != 52)
		{
			sbLoadFile.AppendFormat("HShortBreathPattern_{0:00}_{1:00}", _mode, _kind);
		}
		else
		{
			sbLoadFile.AppendFormat("HShortBreathPatternEV_{0:00}_{1:00}", _mode, _kind);
		}
		string text = GlobalMethod.LoadAllListText(lstBreathAbnames, sbLoadFile.ToString());
		if (text.IsNullOrEmpty())
		{
			return false;
		}
		GlobalMethod.GetListString(text, out var data);
		int num = data.Length;
		int num2 = ((num != 0) ? data[0].Length : 0);
		List<BreathVoicePtnInfo> list = new List<BreathVoicePtnInfo>();
		for (int i = 0; i < num; i++)
		{
			int num3 = 0;
			int key = int.Parse(data[i][num3++]);
			int key2 = int.Parse(data[i][num3++]);
			list.Clear();
			for (int j = num3; j < num2; j += 3)
			{
				string text2 = data[i][j];
				if (text2 == "")
				{
					break;
				}
				BreathVoicePtnInfo breathVoicePtnInfo = new BreathVoicePtnInfo();
				string[] array = text2.Split(',');
				int result = 0;
				for (int k = 0; k < array.Length; k++)
				{
					if (int.TryParse(array[k], out result))
					{
						breathVoicePtnInfo.lstConditions.Add(result);
					}
				}
				array = data[i][j + 1].Split(',');
				for (int l = 0; l < array.Length; l++)
				{
					if (int.TryParse(array[l], out result))
					{
						breathVoicePtnInfo.lstAnimeID.Add(result);
					}
				}
				array = data[i][j + 2].Split(',');
				for (int m = 0; m < array.Length; m++)
				{
					if (int.TryParse(array[m], out result))
					{
						breathVoicePtnInfo.lstVoice.Add(result);
					}
				}
				list.Add(breathVoicePtnInfo);
			}
			ValueDictionary<int, int, int, int, List<BreathVoicePtnInfo>> valueDictionary = new ValueDictionary<int, int, int, int, List<BreathVoicePtnInfo>>();
			for (int n = 0; n < _main; n++)
			{
				valueDictionary = shortBreathPtns[n].dicInfo;
				if (!valueDictionary.ContainsKey(_mode))
				{
					valueDictionary[_mode] = valueDictionary.New();
				}
				if (!valueDictionary[_mode].ContainsKey(_kind))
				{
					valueDictionary[_mode][_kind] = valueDictionary[_mode].New();
				}
				if (!valueDictionary[_mode][_kind].ContainsKey(key))
				{
					valueDictionary[_mode][_kind][key] = valueDictionary[_mode][_kind].New();
				}
				valueDictionary[_mode][_kind][key][key2] = new List<BreathVoicePtnInfo>(list);
			}
		}
		return true;
	}

	private bool LoadShortBreathAddPtn(int _main, int _mode, int _kind)
	{
		foreach (string tmpBreathFilename in tmpBreathFilenames)
		{
			if (int.Parse(Regex.Match(tmpBreathFilename, "[0-9]{2,2}").Value) != _mode)
			{
				continue;
			}
			string text = GlobalMethod.LoadAllListText(lstBreathAbnames, tmpBreathFilename);
			if (text.IsNullOrEmpty())
			{
				continue;
			}
			GlobalMethod.GetListString(text, out var data);
			int num = data.Length;
			int num2 = ((num != 0) ? data[0].Length : 0);
			List<BreathVoicePtnInfo> list = new List<BreathVoicePtnInfo>();
			for (int i = 0; i < num; i++)
			{
				int num3 = 0;
				int key = int.Parse(data[i][num3++]);
				int key2 = int.Parse(data[i][num3++]);
				for (int j = num3; j < num2; j += 3)
				{
					string text2 = data[i][j];
					if (text2 == "")
					{
						break;
					}
					BreathVoicePtnInfo bvi = new BreathVoicePtnInfo();
					string[] array = text2.Split(',');
					int result = 0;
					for (int k = 0; k < array.Length; k++)
					{
						if (int.TryParse(array[k], out result))
						{
							bvi.lstConditions.Add(result);
						}
					}
					array = data[i][j + 1].Split(',');
					for (int l = 0; l < array.Length; l++)
					{
						if (int.TryParse(array[l], out result))
						{
							bvi.lstAnimeID.Add(result);
						}
					}
					array = data[i][j + 2].Split(',');
					for (int m = 0; m < array.Length; m++)
					{
						if (int.TryParse(array[m], out result))
						{
							bvi.lstVoice.Add(result);
						}
					}
					BreathVoicePtnInfo breathVoicePtnInfo = list.Find((BreathVoicePtnInfo f) => f.lstConditions.SequenceEqual(bvi.lstConditions) && f.lstAnimeID.SequenceEqual(bvi.lstAnimeID));
					if (breathVoicePtnInfo == null)
					{
						list.Add(bvi);
					}
					else
					{
						breathVoicePtnInfo.lstVoice = new List<int>(bvi.lstVoice);
					}
					for (int num4 = 0; num4 < _main; num4++)
					{
						if (num4 != 1 || _mode == 4 || _mode == 5)
						{
							ValueDictionary<int, int, int, int, List<BreathVoicePtnInfo>> dicInfo = shortBreathAddPtns[num4].dicInfo;
							if (!dicInfo.ContainsKey(_mode))
							{
								dicInfo[_mode] = dicInfo.New();
							}
							if (!dicInfo[_mode].ContainsKey(_kind))
							{
								dicInfo[_mode][_kind] = dicInfo[_mode].New();
							}
							if (!dicInfo[_mode][_kind].ContainsKey(key))
							{
								dicInfo[_mode][_kind][key] = dicInfo[_mode][_kind].New();
							}
							if (!dicInfo[_mode][_kind][key].ContainsKey(key2))
							{
								dicInfo[_mode][_kind][key][key2] = new List<BreathVoicePtnInfo>(list);
							}
							else
							{
								dicInfo[_mode][_kind][key][key2].AddRange(list);
							}
						}
					}
				}
			}
		}
		for (int num5 = 0; num5 < _main; num5++)
		{
			if (num5 != 1 || _mode == 4 || _mode == 5)
			{
				LoadShortBreathPtnComposition(num5, _mode, _kind);
			}
		}
		return true;
	}

	private bool LoadShortBreathPtnComposition(int _main, int _mode, int _kind)
	{
		if (!shortBreathPtns[_main].dicInfo.ContainsKey(_mode))
		{
			return false;
		}
		if (!shortBreathPtns[_main].dicInfo[_mode].ContainsKey(_kind))
		{
			return false;
		}
		ValueDictionary<int, int, List<BreathVoicePtnInfo>> valueDictionary = shortBreathPtns[_main].dicInfo[_mode][_kind];
		if (!shortBreathAddPtns[_main].dicInfo.ContainsKey(_mode))
		{
			return false;
		}
		if (!shortBreathAddPtns[_main].dicInfo[_mode].ContainsKey(_kind))
		{
			return false;
		}
		ValueDictionary<int, int, List<BreathVoicePtnInfo>> valueDictionary2 = shortBreathAddPtns[_main].dicInfo[_mode][_kind];
		foreach (KeyValuePair<int, ValueDictionary<int, List<BreathVoicePtnInfo>>> item in valueDictionary)
		{
			if (!valueDictionary2.ContainsKey(item.Key))
			{
				continue;
			}
			foreach (KeyValuePair<int, List<BreathVoicePtnInfo>> item2 in item.Value)
			{
				if (valueDictionary2[item.Key].ContainsKey(item2.Key))
				{
					item2.Value.AddRange(valueDictionary2[item.Key][item2.Key]);
				}
			}
		}
		return true;
	}

	private IEnumerator LoadStartVoicePtnList(string _pathAssetFolder)
	{
		LoadStartVoicePtn(_pathAssetFolder);
		yield return null;
		tmpVoiceFilenames.Clear();
		foreach (string lstVoiceAbname in lstVoiceAbnames)
		{
			if (GameSystem.IsPathAdd50(lstVoiceAbname))
			{
				string[] allAssetName = AssetBundleCheck.GetAllAssetName(lstVoiceAbname, _WithExtension: false);
				foreach (string filename in allAssetName)
				{
					CheckAddPattern(filename, 3, bAppendEV, ref tmpVoiceFilenames, isAppendList: true);
				}
			}
		}
		LoadStartVoiceAddPtn(_pathAssetFolder);
	}

	private void LoadStartVoicePtn(string _pathAssetFolder)
	{
		sbLoadFile.Clear();
		if (!bAppendEV)
		{
			sbLoadFile.Append("HStartVoicePattern");
		}
		else
		{
			sbLoadFile.AppendFormat("HStartVoicePatternEV{0:00}", EventID);
		}
		lst.Clear();
		GlobalMethod.LoadAllListTextFromList(_pathAssetFolder, sbLoadFile.ToString(), ref lst);
		for (int i = 0; i < lst.Count; i++)
		{
			GlobalMethod.GetListString(lst[i], out var data);
			int num = data.Length;
			for (int j = 0; j < lstLoadStartVoicePtn.Length; j++)
			{
				int num2 = 0;
				while (num2 < num)
				{
					StartVoicePtn startVoicePtn = new StartVoicePtn();
					int num3 = data[num2].Length;
					int result = -1;
					if (!int.TryParse(data[num2][0], out result))
					{
						num2++;
						continue;
					}
					startVoicePtn.condition = result;
					if (!int.TryParse(data[num2][1], out result))
					{
						result = 0;
					}
					startVoicePtn.nTaii = result;
					if (!int.TryParse(data[num2][2], out result))
					{
						result = 0;
					}
					startVoicePtn.timing = result;
					startVoicePtn.anim = startKindName[result];
					if (!int.TryParse(data[num2][3], out result))
					{
						result = -1;
					}
					startVoicePtn.nParamPtn = result;
					int num4 = 4;
					while (num4 < num3)
					{
						VoicePtnInfo voicePtnInfo = new VoicePtnInfo();
						if (data[num2][num4].IsNullOrEmpty())
						{
							break;
						}
						string[] array = data[num2][num4++].Split(',');
						for (int k = 0; k < array.Length; k++)
						{
							voicePtnInfo.lstAnimList.Add(int.Parse(array[k]));
						}
						array = data[num2][num4++].Split(',');
						for (int l = 0; l < array.Length; l++)
						{
							voicePtnInfo.lstPlayConditions.Add(int.Parse(array[l]));
						}
						voicePtnInfo.loadListmode = 7;
						voicePtnInfo.loadListKind = int.Parse(data[num2][num4++]);
						array = data[num2][num4++].Split(',');
						for (int m = 0; m < array.Length; m++)
						{
							voicePtnInfo.lstVoice.Add(int.Parse(array[m]));
						}
						voicePtnInfo.loadState = startVoicePtn.condition;
						startVoicePtn.lstInfo.Add(voicePtnInfo);
					}
					if (lstLoadStartVoicePtn[j] == null)
					{
						lstLoadStartVoicePtn[j] = new List<StartVoicePtn>();
					}
					int num5 = -1;
					for (int n = 0; n < lstLoadStartVoicePtn[j].Count; n++)
					{
						if (lstLoadStartVoicePtn[j][n].condition == startVoicePtn.condition && lstLoadStartVoicePtn[j][n].nTaii == startVoicePtn.nTaii && lstLoadStartVoicePtn[j][n].timing == startVoicePtn.timing && lstLoadStartVoicePtn[j][n].nParamPtn == startVoicePtn.nParamPtn)
						{
							num5 = n;
							break;
						}
					}
					if (num5 < 0)
					{
						lstLoadStartVoicePtn[j].Add(startVoicePtn);
					}
					else
					{
						lstLoadStartVoicePtn[j][num5] = startVoicePtn;
					}
					num2++;
				}
			}
		}
	}

	private void LoadStartVoiceAddPtn(string _pathAssetFolder)
	{
		foreach (string tmpVoiceFilename in tmpVoiceFilenames)
		{
			sbLoadFile.Clear();
			sbLoadFile.Append(tmpVoiceFilename);
			lst.Clear();
			GlobalMethod.LoadAllListTextFromList(_pathAssetFolder, sbLoadFile.ToString(), ref lst);
			for (int i = 0; i < lst.Count; i++)
			{
				GlobalMethod.GetListString(lst[i], out var data);
				int num = 0;
				int num2 = data.Length;
				for (int j = 0; j < lstLoadStartVoiceAddPtn.Length; j++)
				{
					while (num < num2)
					{
						StartVoicePtn startVoicePtn = new StartVoicePtn();
						int num3 = data[num].Length;
						int result = -1;
						if (!int.TryParse(data[num][0], out result))
						{
							num++;
							continue;
						}
						startVoicePtn.condition = result;
						if (!int.TryParse(data[num][1], out result))
						{
							result = 0;
						}
						startVoicePtn.nTaii = result;
						if (!int.TryParse(data[num][2], out result))
						{
							result = 0;
						}
						startVoicePtn.timing = result;
						startVoicePtn.anim = startKindName[result];
						if (!int.TryParse(data[num][3], out result))
						{
							result = -1;
						}
						startVoicePtn.nParamPtn = result;
						int num4 = 4;
						while (num4 < num3)
						{
							VoicePtnInfo voicePtnInfo = new VoicePtnInfo();
							if (data[num][num4].IsNullOrEmpty())
							{
								break;
							}
							string[] array = data[num][num4++].Split(',');
							for (int k = 0; k < array.Length; k++)
							{
								voicePtnInfo.lstAnimList.Add(int.Parse(array[k]));
							}
							array = data[num][num4++].Split(',');
							for (int l = 0; l < array.Length; l++)
							{
								voicePtnInfo.lstPlayConditions.Add(int.Parse(array[l]));
							}
							voicePtnInfo.loadListmode = 7;
							voicePtnInfo.loadListKind = int.Parse(data[num][num4++]);
							array = data[num][num4++].Split(',');
							for (int m = 0; m < array.Length; m++)
							{
								voicePtnInfo.lstVoice.Add(int.Parse(array[m]));
							}
							voicePtnInfo.loadState = startVoicePtn.condition;
							startVoicePtn.lstInfo.Add(voicePtnInfo);
						}
						if (lstLoadStartVoiceAddPtn[j] == null)
						{
							lstLoadStartVoiceAddPtn[j] = new List<StartVoicePtn>();
						}
						int num5 = -1;
						for (int n = 0; n < lstLoadStartVoiceAddPtn[j].Count; n++)
						{
							if (lstLoadStartVoiceAddPtn[j][n].condition == startVoicePtn.condition && lstLoadStartVoiceAddPtn[j][n].nTaii == startVoicePtn.nTaii && lstLoadStartVoiceAddPtn[j][n].timing == startVoicePtn.timing && lstLoadStartVoiceAddPtn[j][n].nParamPtn == startVoicePtn.nParamPtn)
							{
								num5 = n;
								break;
							}
						}
						if (num5 < 0)
						{
							lstLoadStartVoiceAddPtn[j].Add(startVoicePtn);
						}
						else
						{
							lstLoadStartVoiceAddPtn[j][num5] = startVoicePtn;
						}
						num++;
					}
				}
			}
			for (int num6 = 0; num6 < lstLoadStartVoiceAddPtn.Length; num6++)
			{
				LoadStartVoicePtnComposition(num6);
			}
		}
	}

	private bool LoadStartVoicePtnComposition(int personal)
	{
		if (lstLoadStartVoicePtn.Length <= personal)
		{
			return false;
		}
		List<StartVoicePtn> list = lstLoadStartVoicePtn[personal];
		if (lstLoadStartVoiceAddPtn.Length <= personal)
		{
			return false;
		}
		List<StartVoicePtn> list2 = lstLoadStartVoiceAddPtn[personal];
		foreach (StartVoicePtn item in list)
		{
			foreach (StartVoicePtn item2 in list2)
			{
				if (item.condition == item2.condition && item.condition == item2.condition && item.nTaii == item2.nTaii && item.timing == item2.timing && item.nParamPtn == item2.nParamPtn)
				{
					item.lstInfo.AddRange(item2.lstInfo);
				}
			}
		}
		return true;
	}

	private void LoadFaceInfo()
	{
		for (int i = 0; i < personality.Length; i++)
		{
			if (!multiFemale && i == 1)
			{
				continue;
			}
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 0, 0, 0, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 2, 0, 0, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 0, 0, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 0, 1, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 1, 0, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 1, 1, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 2, 0, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 2, 1, i);
			for (int j = 0; j < 12; j++)
			{
				int kind = j;
				LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 3, kind, i);
			}
			for (int k = 0; k < 2; k++)
			{
				int kind2 = 12 + k;
				LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 3, kind2, i);
			}
			for (int l = 0; l < 2; l++)
			{
				int kind3 = 14 + l;
				LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 3, kind3, i);
			}
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 7, 0, i);
			LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 7, 1, i);
			if (multiFemale)
			{
				LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 4, 0, i);
				LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 4, 1, i);
				for (int m = 0; m < 2; m++)
				{
					int kind4 = m;
					LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 5, kind4, i);
				}
				for (int n = 0; n < 2; n++)
				{
					int kind5 = 2 + n;
					LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 5, kind5, i);
				}
			}
			if (multiMale)
			{
				for (int num = 0; num < 2; num++)
				{
					int kind6 = num;
					LoadFaceInfo(hSceneManager.strAssetVoiceFaceListFolder, personality[i], 1, 6, kind6, i);
				}
			}
		}
	}

	private void LoadFaceInfo(string path, int person, int facekind, int category, int kind, int main)
	{
		sbLoadFile.Clear();
		SetLoadFaceName(person, facekind, category, kind);
		string text = GlobalMethod.LoadAllListText(path, sbLoadFile.ToString());
		if (text.IsNullOrEmpty())
		{
			return;
		}
		GlobalMethod.GetListString(text, out var data);
		int key = facekind;
		if (facekind == 1 && category == 7)
		{
			key = 3;
		}
		if (!dicFaceInfos[main].ContainsKey(key))
		{
			dicFaceInfos[main][key] = dicFaceInfos[main].New();
			dicFaceInfos[main][key][category] = dicFaceInfos[main][key].New();
			dicFaceInfos[main][key][category][kind] = new Dictionary<int, FaceInfo>();
		}
		else if (!dicFaceInfos[main][key].ContainsKey(category))
		{
			dicFaceInfos[main][key][category] = dicFaceInfos[main][key].New();
			dicFaceInfos[main][key][category][kind] = new Dictionary<int, FaceInfo>();
		}
		else if (!dicFaceInfos[main][key][category].ContainsKey(kind))
		{
			dicFaceInfos[main][key][category][kind] = new Dictionary<int, FaceInfo>();
		}
		Dictionary<int, FaceInfo> dictionary = dicFaceInfos[main][key][category][kind];
		for (int i = 0; i < data.Length; i++)
		{
			int result = 0;
			if (!int.TryParse(data[i][0], out result))
			{
				continue;
			}
			FaceInfo faceInfo = new FaceInfo();
			int num = 1;
			if (data[i][num].IsNullOrEmpty())
			{
				break;
			}
			float num2 = float.Parse(data[i][num++]);
			if (num2 < 0f)
			{
				break;
			}
			faceInfo.openEye = num2;
			faceInfo.openMouthMin = float.Parse(data[i][num++]);
			faceInfo.openMouthMax = float.Parse(data[i][num++]);
			faceInfo.eyeBlow = int.Parse(data[i][num++]);
			faceInfo.eye = int.Parse(data[i][num++]);
			faceInfo.mouth = int.Parse(data[i][num++]);
			faceInfo.tear = float.Parse(data[i][num++]);
			faceInfo.cheek = float.Parse(data[i][num++]);
			faceInfo.highlight = data[i][num++] == "1";
			faceInfo.blink = data[i][num++] == "1";
			faceInfo.behaviorNeckLine = int.Parse(data[i][num++]);
			faceInfo.behaviorEyeLine = int.Parse(data[i][num++]);
			faceInfo.targetNeckLine = int.Parse(data[i][num++]);
			Vector3 zero = Vector3.zero;
			if (faceInfo.targetNeckLine == 7)
			{
				for (int j = 0; j < 2; j++)
				{
					zero = Vector3.zero;
					if (!float.TryParse(data[i][num++], out zero.x))
					{
						zero.x = 0f;
					}
					if (!float.TryParse(data[i][num++], out zero.y))
					{
						zero.y = 0f;
					}
					if (!float.TryParse(data[i][num++], out zero.z))
					{
						zero.z = 0f;
					}
					faceInfo.NeckRot[j] = zero;
				}
				for (int k = 0; k < 2; k++)
				{
					zero = Vector3.zero;
					if (!float.TryParse(data[i][num++], out zero.x))
					{
						zero.x = 0f;
					}
					if (!float.TryParse(data[i][num++], out zero.y))
					{
						zero.y = 0f;
					}
					if (!float.TryParse(data[i][num++], out zero.z))
					{
						zero.z = 0f;
					}
					faceInfo.HeadRot[k] = zero;
				}
			}
			else
			{
				num += 12;
			}
			faceInfo.targetEyeLine = int.Parse(data[i][num++]);
			if (faceInfo.targetEyeLine == 7)
			{
				for (int l = 0; l < 2; l++)
				{
					zero = Vector3.zero;
					if (!float.TryParse(data[i][num++], out zero.x))
					{
						zero.x = 0f;
					}
					if (!float.TryParse(data[i][num++], out zero.y))
					{
						zero.y = 0f;
					}
					if (!float.TryParse(data[i][num++], out zero.z))
					{
						zero.z = 0f;
					}
					faceInfo.EyeRot[l] = zero;
				}
			}
			if (dictionary.ContainsKey(result))
			{
				dictionary[result] = faceInfo;
			}
			else
			{
				dictionary.Add(result, faceInfo);
			}
		}
	}

	private void CheckAddPattern(string filename, int kind, bool AppendEvent, ref List<string> tmpFilenames, bool isAppendList = false)
	{
		if (Regex.IsMatch(filename, tmpAddPatternName[kind][AppendEvent ? 1 : 0], RegexOptions.IgnoreCase) && (!isAppendList || !bAppendEV || EventID == int.Parse(Regex.Match(filename, "[0-9]{2}").Value)) && !tmpFilenames.Contains(filename))
		{
			tmpFilenames.Add(filename);
		}
	}

	private void SetLoadFaceName(int person, int facekind, int category, int kind)
	{
		sbLoadFile.Clear();
		if (!bAppendEV)
		{
			sbLoadFile.AppendFormat("FacePattern_c{0:00}_{1:00}_{2:00}_{3:00}", person, facekind, category, kind);
		}
		else if (facekind != 1)
		{
			if (EventID != 52)
			{
				sbLoadFile.AppendFormat("FacePattern_c{0:00}_{1:00}_{2:00}_{3:00}", person, facekind, category, kind);
			}
			else
			{
				sbLoadFile.AppendFormat("FaceEV{0:00}_c{1:00}_{2:00}_{3:00}_{4:00}", EventID, person, facekind, category, kind);
			}
		}
		else
		{
			sbLoadFile.AppendFormat("FaceEV{0:00}_c{1:00}_{2:00}_{3:00}_{4:00}", EventID, person, facekind, category, kind);
		}
	}
}
