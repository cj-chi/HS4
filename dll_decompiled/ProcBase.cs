using System.Collections.Generic;
using System.Text;
using AIChara;
using Manager;
using UnityEngine;

public class ProcBase
{
	protected struct animParm
	{
		public float[] heights;

		public float breast;

		public float[] m;

		public float speed;
	}

	protected HSceneFlagCtrl ctrlFlag;

	protected ChaControl[] chaFemales;

	protected ChaControl[] chaMales;

	protected CrossFade fade;

	protected ObiCtrl ctrlObi;

	protected HSceneSprite sprite;

	protected HItemCtrl item;

	protected FeelHit feelHit;

	protected HAutoCtrl auto;

	protected HVoiceCtrl voice;

	protected HParticleCtrl particle;

	protected HSeCtrl se;

	protected float[] timeChangeMotions = new float[2];

	protected float[] timeChangeMotionDeltaTimes = new float[2];

	protected ShuffleRand[] randVoicePlays = new ShuffleRand[2];

	protected bool isHeight1Parameter;

	protected List<(int, int, MotionIK)> lstMotionIK = new List<(int, int, MotionIK)>();

	protected YureCtrl[] ctrlYures;

	protected RootmotionOffset[] rootmotionOffsetF;

	protected RootmotionOffset[] rootmotionOffsetM;

	protected int eventNo = -1;

	protected int peepkind = -1;

	public static bool endInit;

	protected static HSceneManager hSceneManager;

	protected int CatID = -1;

	protected StringBuilder sbWarning = new StringBuilder();

	protected AnimatorStateInfo FemaleAi;

	public ProcBase(DeliveryMember _delivery)
	{
		ctrlFlag = _delivery.ctrlFlag;
		chaMales = _delivery.chaMales;
		chaFemales = _delivery.chaFemales;
		fade = _delivery.fade;
		ctrlObi = _delivery.ctrlObi;
		sprite = _delivery.sprite;
		item = _delivery.item;
		feelHit = _delivery.feelHit;
		auto = _delivery.auto;
		voice = _delivery.voice;
		particle = _delivery.particle;
		se = _delivery.se;
		lstMotionIK = _delivery.lstMotionIK;
		ctrlYures = _delivery.ctrlYures;
		rootmotionOffsetF = _delivery.rootmotionOffsetsF;
		rootmotionOffsetM = _delivery.rootmotionOffsetsM;
		eventNo = _delivery.eventNo;
		peepkind = _delivery.peepkind;
		if (hSceneManager == null)
		{
			hSceneManager = Singleton<HSceneManager>.Instance;
		}
		for (int i = 0; i < 2; i++)
		{
			randVoicePlays[i] = new ShuffleRand();
			randVoicePlays[i].Init((i == 0) ? 3 : 2);
		}
	}

	public virtual bool Init(int _modeCtrl)
	{
		endInit = true;
		ctrlFlag.lstSyncAnimLayers[0, 0].Clear();
		ctrlFlag.lstSyncAnimLayers[0, 1].Clear();
		ctrlFlag.lstSyncAnimLayers[1, 0].Clear();
		ctrlFlag.lstSyncAnimLayers[1, 1].Clear();
		ctrlObi.PlayUrine(use: false);
		ctrlFlag.voice.oldFinish = -1;
		if (Singleton<Game>.Instance.eventNo != 19)
		{
			if (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1)
			{
				ctrlFlag.isFaintness = true;
			}
			else
			{
				ctrlFlag.isFaintness = false;
			}
		}
		else
		{
			ctrlFlag.isFaintness = true;
			if (ctrlFlag.FaintnessType == 0 || ctrlFlag.FaintnessType == 1)
			{
				ctrlFlag.isFaintnessVoice = true;
			}
			else
			{
				ctrlFlag.isFaintnessVoice = false;
			}
		}
		return true;
	}

	public virtual bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		return true;
	}

	public virtual bool Proc(int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		return true;
	}

	public virtual void setAnimationParamater()
	{
	}

	public virtual bool ResetMotionSpeed()
	{
		auto.SetSpeed(ctrlFlag.speed);
		return true;
	}

	protected void SetRecoverTaii()
	{
		ctrlFlag.selectAnimationListInfo = ctrlFlag.nowAnimationInfo;
		List<HScene.AnimationListInfo>[] lstAnimInfo = HSceneManager.HResourceTables.lstAnimInfo;
		if (lstAnimInfo == null || ctrlFlag.nowAnimationInfo.nFaintnessLimit == 0)
		{
			return;
		}
		bool flag = ctrlFlag.nowAnimationInfo.nInitiativeFemale != 0;
		int item = ctrlFlag.nowAnimationInfo.ActionCtrl.Item1;
		int item2 = ctrlFlag.nowAnimationInfo.ActionCtrl.Item2;
		HScene.AnimationListInfo animationListInfo = null;
		for (int i = 0; i < lstAnimInfo[item].Count; i++)
		{
			animationListInfo = lstAnimInfo[item][i];
			if (sprite.CheckMotionLimitRecover(animationListInfo) && item == animationListInfo.ActionCtrl.Item1 && (item < 3 || item2 == animationListInfo.ActionCtrl.Item2) && animationListInfo.nPositons.Contains(ctrlFlag.nPlace) && animationListInfo.nFaintnessLimit == 0 && (!flag || animationListInfo.nInitiativeFemale != 0) && (flag || animationListInfo.nInitiativeFemale == 0))
			{
				ctrlFlag.selectAnimationListInfo = animationListInfo;
				return;
			}
		}
		sbWarning.Clear();
		sbWarning.Append("脱力からの回復先がない");
	}
}
