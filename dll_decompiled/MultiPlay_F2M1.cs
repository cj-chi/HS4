using System.Collections.Generic;
using AIChara;
using Manager;
using UnityEngine;

public class MultiPlay_F2M1 : ProcBase
{
	private float[] timeMotions = new float[2];

	private bool[] enableMotions = new bool[2];

	private bool[] allowMotions = new bool[2] { true, true };

	private Vector2[] lerpMotions = new Vector2[2]
	{
		Vector2.zero,
		Vector2.zero
	};

	private float[] lerpTimes = new float[2];

	private List<HScene.AnimationListInfo> lstAnimation;

	private int nextPlay;

	private bool oldHit;

	private int finishMotion;

	private bool finishMorS;

	private animParm animPar;

	private int resist;

	private int addFeel = 1;

	public MultiPlay_F2M1(DeliveryMember _delivery)
		: base(_delivery)
	{
		animPar.heights = new float[2];
		animPar.m = new float[2];
		CatID = 7;
	}

	public override bool Init(int _modeCtrl)
	{
		base.Init(_modeCtrl);
		return true;
	}

	public void SetAnimationList(List<HScene.AnimationListInfo> _list)
	{
		lstAnimation = _list;
	}

	public override bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (_isIdle)
		{
			if (_infoAnimList.nDownPtn != 0)
			{
				if (_modeCtrl != 4)
				{
					setPlay(ctrlFlag.isFaintness ? "D_Idle" : "Idle", _isFade: false);
				}
				else
				{
					setPlay(ctrlFlag.isFaintness ? "D_OrgasmM_OUT_A" : "Idle", _isFade: false);
				}
			}
			else
			{
				setPlay("Idle", _isFade: false);
			}
			voice.HouchiTime = 0f;
			ctrlFlag.loopType = -1;
		}
		else
		{
			if (ctrlFlag.feel_f >= 0.75f)
			{
				if (_infoAnimList.nDownPtn != 0)
				{
					setPlay(ctrlFlag.isFaintness ? "D_OLoop" : "OLoop", _isFade: false);
				}
				else
				{
					setPlay("OLoop", _isFade: false);
				}
				ctrlFlag.loopType = 2;
			}
			else
			{
				if (_infoAnimList.nDownPtn != 0)
				{
					setPlay(ctrlFlag.isFaintness ? "D_WLoop" : "WLoop", _isFade: false);
				}
				else
				{
					setPlay("WLoop", _isFade: false);
				}
				ctrlFlag.loopType = 0;
			}
			ctrlFlag.speed = 0f;
			ctrlFlag.motions[0] = 0f;
			ctrlFlag.motions[1] = 0f;
			ctrlFlag.nowSpeedStateFast = false;
			auto.SetSpeed(0f);
		}
		if (_infoAnimList.lstSystem.Contains(4))
		{
			resist = 2;
		}
		else if (_infoAnimList.lstSystem.Contains(3))
		{
			resist = 1;
		}
		else
		{
			resist = 0;
		}
		addFeel = 1;
		if (chaFemales[0].fileParam2.hAttribute != 3 && _infoAnimList.lstSystem.Contains(4) && chaFemales[0].fileGameInfo2.resistPain < 100)
		{
			addFeel = 0;
		}
		ctrlFlag.voice.changeTaii = true;
		nextPlay = 0;
		oldHit = false;
		isHeight1Parameter = chaFemales[0].IsParameterInAnimator("height1");
		return true;
	}

	public override bool Proc(int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (chaFemales[0].objTop == null)
		{
			return false;
		}
		FemaleAi = chaFemales[0].getAnimatorStateInfo(0);
		if (ctrlFlag.initiative == 0)
		{
			switch (_modeCtrl)
			{
			case 0:
				ManualAibu(FemaleAi, _infoAnimList);
				break;
			case 1:
			case 2:
				ManualHoushi(FemaleAi, _modeCtrl, _infoAnimList);
				break;
			case 3:
			case 4:
				ManualSonyu(FemaleAi, _modeCtrl, _infoAnimList);
				break;
			}
		}
		else
		{
			switch (_modeCtrl)
			{
			case 0:
				AutoAibu(FemaleAi, _infoAnimList);
				break;
			case 1:
			case 2:
				AutoHoushi(FemaleAi, _modeCtrl, _infoAnimList);
				break;
			case 3:
			case 4:
				AutoSonyu(FemaleAi, _modeCtrl, _infoAnimList);
				break;
			}
		}
		for (int i = 0; i < 2; i++)
		{
			if (enableMotions[i])
			{
				timeMotions[i] = Mathf.Clamp(timeMotions[i] + Time.deltaTime, 0f, lerpTimes[i]);
				float time = Mathf.Clamp01(timeMotions[i] / lerpTimes[i]);
				time = ctrlFlag.changeMotionCurve.Evaluate(time);
				ctrlFlag.motions[i] = Mathf.Lerp(lerpMotions[i].x, lerpMotions[i].y, time);
				if (time >= 1f)
				{
					enableMotions[i] = false;
				}
			}
		}
		SetFinishCategoryEnable(FemaleAi, _modeCtrl);
		if (eventNo >= 50 && eventNo < 56)
		{
			addFeel = 1;
		}
		if (_modeCtrl == 3 || _modeCtrl == 4)
		{
			ctrlObi.Proc(FemaleAi, ctrlFlag.isInsert);
		}
		else
		{
			ctrlObi.Proc(FemaleAi);
		}
		ctrlObi.PlayUrine(FemaleAi);
		ctrlObi.PlayUrine(FemaleAi, 1);
		switch (_modeCtrl)
		{
		case 0:
			if (ctrlFlag.click != HSceneFlagCtrl.ClickKind.RecoverFaintness)
			{
				break;
			}
			if (FemaleAi.IsName("D_Idle") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop") || FemaleAi.IsName("D_OLoop") || FemaleAi.IsName("D_Orgasm_A"))
			{
				setPlay("Orgasm_A");
				ctrlFlag.isFaintness = false;
				ctrlFlag.FaintnessType = -1;
				ctrlFlag.isFaintnessVoice = false;
				sprite.SetVisibleLeaveItToYou(_visible: true, _judgeLeaveItToYou: true);
				ctrlFlag.numOrgasm = 0;
				sprite.SetAnimationMenu();
				ctrlFlag.isGaugeHit = false;
				ctrlFlag.isGaugeHit_M = false;
				sprite.SetMotionListDraw(_active: false);
				ctrlFlag.nowOrgasm = false;
				ctrlObi.PlayUrine(use: false);
				if (voice.playAnimation != null)
				{
					voice.playAnimation.SetAllFlags(_play: true);
				}
			}
			else
			{
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
			}
			break;
		case 1:
		case 2:
			if (ctrlFlag.click != HSceneFlagCtrl.ClickKind.RecoverFaintness)
			{
				break;
			}
			if (FemaleAi.IsName("D_Idle") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop") || FemaleAi.IsName("D_OLoop") || FemaleAi.IsName("D_Orgasm_OUT_A"))
			{
				SetRecoverTaii();
				if (ctrlFlag.nowAnimationInfo == ctrlFlag.selectAnimationListInfo)
				{
					setPlay("Orgasm_OUT_A");
				}
				ctrlFlag.isFaintness = false;
				ctrlFlag.FaintnessType = -1;
				ctrlFlag.isFaintnessVoice = false;
				sprite.SetVisibleLeaveItToYou(_visible: true, _judgeLeaveItToYou: true);
				ctrlFlag.numOrgasm = 0;
				sprite.SetAnimationMenu();
				ctrlFlag.isGaugeHit = false;
				ctrlFlag.isGaugeHit_M = false;
				sprite.SetMotionListDraw(_active: false);
				ctrlFlag.nowOrgasm = false;
				ctrlObi.PlayUrine(use: false);
			}
			else
			{
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
			}
			break;
		case 3:
		case 4:
			if (ctrlFlag.click != HSceneFlagCtrl.ClickKind.RecoverFaintness)
			{
				break;
			}
			if (FemaleAi.IsName("D_Idle") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop") || FemaleAi.IsName("D_OLoop") || FemaleAi.IsName("D_Orgasm_IN_A") || FemaleAi.IsName("D_OrgasmM_OUT_A"))
			{
				if (_modeCtrl == 3)
				{
					setPlay("Orgasm_IN_A");
				}
				else
				{
					setPlay("OrgasmM_OUT_A");
				}
				ctrlFlag.isFaintness = false;
				ctrlFlag.FaintnessType = -1;
				ctrlFlag.isFaintnessVoice = false;
				ctrlFlag.numOrgasm = 0;
				sprite.SetFinishSelect(7, _modeCtrl);
				sprite.SetVisibleLeaveItToYou(_visible: true, _judgeLeaveItToYou: true);
				ctrlObi.ChangeSetupInfo(0);
				sprite.SetAnimationMenu();
				ctrlFlag.isGaugeHit = false;
				ctrlFlag.isGaugeHit_M = false;
				sprite.SetMotionListDraw(_active: false);
				ctrlFlag.nowOrgasm = false;
				ctrlObi.PlayUrine(use: false);
				if (voice.playAnimation != null)
				{
					voice.playAnimation.SetAllFlags(_play: true);
				}
			}
			else
			{
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
			}
			break;
		}
		setAnimationParamater();
		return true;
	}

	private bool ManualAibu(AnimatorStateInfo _ai, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			StartProcTrigger(num);
			StartAibuProc(_isReStart: false, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("WLoop"))
		{
			LoopAibuProc(0, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			LoopAibuProc(1, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			OLoopAibuProc(0, num, _infoAnimList);
		}
		else if (_ai.IsName("Orgasm"))
		{
			OrgasmAibuProc(0, _ai.normalizedTime);
		}
		else if (_ai.IsName("Orgasm_A"))
		{
			StartProcTrigger(num);
			StartAibuProc(_isReStart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			FaintnessStartProcTrigger(num);
			FaintnessStartAibuProc(_start: true, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopAibuProc(0, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopAibuProc(1, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopAibuProc(1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_Orgasm"))
		{
			OrgasmAibuProc(1, _ai.normalizedTime);
		}
		else if (_ai.IsName("D_Orgasm_A"))
		{
			FaintnessStartProcTrigger(num);
			FaintnessStartAibuProc(_start: false, num);
		}
		return true;
	}

	private bool ManualHoushi(AnimatorStateInfo _ai, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			StartProcTrigger(num);
			StartHoushiProc(0, _restart: false, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("WLoop"))
		{
			LoopHoushiProc(0, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			LoopHoushiProc(1, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			OLoopHoushiProc(0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("Orgasm_OUT"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Orgasm_OUT_A");
		}
		else if (_ai.IsName("Orgasm_IN"))
		{
			SetAfterInsideFinishAnimation(0, _ai.normalizedTime);
		}
		else if (_ai.IsName("Drink_IN"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Drink", _isSpriteSet: false, _isFade: false);
		}
		else if (_ai.IsName("Drink"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Drink_A");
		}
		else if (_ai.IsName("Vomit_IN"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Vomit", _isSpriteSet: false, _isFade: false);
		}
		else if (_ai.IsName("Vomit"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Vomit_A");
		}
		else if (_ai.IsName("Orgasm_OUT_A"))
		{
			StartProcTrigger(num);
			StartHoushiProc(0, _restart: true, num);
		}
		else if (_ai.IsName("Drink_A"))
		{
			StartProcTrigger(num);
			StartHoushiProc(0, _restart: true, num);
		}
		else if (_ai.IsName("Vomit_A"))
		{
			StartProcTrigger(num);
			StartHoushiProc(0, _restart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			StartProcTrigger(num);
			StartHoushiProc(1, _restart: false, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopHoushiProc(0, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopHoushiProc(1, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopHoushiProc(1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_Orgasm_OUT"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "D_Orgasm_OUT_A");
		}
		else if (_ai.IsName("D_Orgasm_OUT_A"))
		{
			StartProcTrigger(num);
			StartHoushiProc(1, _restart: true, num);
		}
		return true;
	}

	private bool ManualSonyu(AnimatorStateInfo _ai, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			StartProcTrigger(num);
			StartSonyuProc(_restart: false, 0, _modeCtrl, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("Insert"))
		{
			InsertProc(_ai.normalizedTime, 0);
		}
		else if (_ai.IsName("WLoop"))
		{
			LoopSonyuProc(0, 0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			LoopSonyuProc(1, 0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			OLoopSonyuProc(0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("OrgasmF_IN"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 0, _modeCtrl, 0);
		}
		else if (_ai.IsName("OrgasmM_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 0, _modeCtrl, _finishMorS: false);
		}
		else if (_ai.IsName("OrgasmS_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 0, _modeCtrl, _finishMorS: true);
		}
		else if (_ai.IsName("Orgasm_IN_A"))
		{
			AfterTheInsideWaitingProc(0, num, _modeCtrl);
		}
		else if (_ai.IsName("Pull"))
		{
			PullProc(_ai.normalizedTime, 0);
		}
		else if (_ai.IsName("Drop"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 4f, 0, _modeCtrl, 2);
		}
		else if (_ai.IsName("OrgasmM_OUT"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 0, _modeCtrl, 2);
		}
		else if (_ai.IsName("OrgasmM_OUT_A"))
		{
			StartProcTrigger(num);
			StartSonyuProc(_restart: true, 0, _modeCtrl, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			StartProcTrigger(num);
			StartSonyuProc(_restart: false, 1, _modeCtrl, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("D_Insert"))
		{
			InsertProc(_ai.normalizedTime, 1);
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopSonyuProc(0, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopSonyuProc(1, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopSonyuProc(1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_OrgasmF_IN"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 1, _modeCtrl, 0);
		}
		else if (_ai.IsName("D_OrgasmM_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 1, _modeCtrl, _finishMorS: false);
		}
		else if (_ai.IsName("D_OrgasmS_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 1, _modeCtrl, _finishMorS: true);
		}
		else if (_ai.IsName("D_Orgasm_IN_A"))
		{
			AfterTheInsideWaitingProc(1, num, _modeCtrl);
		}
		else if (_ai.IsName("D_Pull"))
		{
			PullProc(_ai.normalizedTime, 1);
		}
		else if (_ai.IsName("D_Drop"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 4f, 1, _modeCtrl, 2);
		}
		else if (_ai.IsName("D_OrgasmM_OUT"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 1, _modeCtrl, 2);
		}
		else if (_ai.IsName("D_OrgasmM_OUT_A"))
		{
			StartProcTrigger(num);
			StartSonyuProc(_restart: true, 1, _modeCtrl, num);
		}
		return true;
	}

	private bool AutoAibu(AnimatorStateInfo _ai, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			AutoStartProcTrigger(_start: false, num);
			AutoStartAibuProc(_isReStart: false, num);
		}
		else if (_ai.IsName("WLoop"))
		{
			AutoLoopAibuProc(0, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			AutoLoopAibuProc(1, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			AutoOLoopAibuProc(0, num, _infoAnimList);
		}
		else if (_ai.IsName("Orgasm"))
		{
			OrgasmAibuProc(0, _ai.normalizedTime);
		}
		else if (_ai.IsName("Orgasm_A"))
		{
			AutoStartProcTrigger(_start: true, num);
			AutoStartAibuProc(_isReStart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			FaintnessStartProcTrigger(num);
			FaintnessStartAibuProc(_start: true, num);
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopAibuProc(0, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopAibuProc(1, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopAibuProc(1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_Orgasm"))
		{
			OrgasmAibuProc(1, _ai.normalizedTime);
		}
		else if (_ai.IsName("D_Orgasm_A"))
		{
			FaintnessStartProcTrigger(num);
			FaintnessStartAibuProc(_start: false, num);
		}
		return true;
	}

	private bool AutoHoushi(AnimatorStateInfo _ai, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			AutoStartProcTrigger(_start: false, num);
			AutoStartHoushiProc(0, _restart: false, num);
		}
		else if (_ai.IsName("WLoop"))
		{
			AutoLoopHoushiProc(0, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			AutoLoopHoushiProc(1, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			AutoOLoopHoushiProc(0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("Orgasm_OUT"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Orgasm_OUT_A");
		}
		else if (_ai.IsName("Orgasm_IN"))
		{
			SetAfterInsideFinishAnimation(0, _ai.normalizedTime);
		}
		else if (_ai.IsName("Drink_IN"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Drink", _isSpriteSet: false, _isFade: false);
		}
		else if (_ai.IsName("Drink"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Drink_A");
		}
		else if (_ai.IsName("Vomit_IN"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Vomit", _isSpriteSet: false, _isFade: false);
		}
		else if (_ai.IsName("Vomit"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "Vomit_A");
		}
		else if (_ai.IsName("Orgasm_OUT_A"))
		{
			AutoStartProcTrigger(_start: true, num);
			AutoStartHoushiProc(0, _restart: true, num);
		}
		else if (_ai.IsName("Drink_A"))
		{
			AutoStartProcTrigger(_start: true, num);
			AutoStartHoushiProc(0, _restart: true, num);
		}
		else if (_ai.IsName("Vomit_A"))
		{
			AutoStartProcTrigger(_start: true, num);
			AutoStartHoushiProc(0, _restart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			StartProcTrigger(num);
			StartHoushiProc(1, _restart: false, num);
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopHoushiProc(0, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopHoushiProc(1, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopHoushiProc(1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_Orgasm_OUT"))
		{
			SetNextFinishAnimation(_ai.normalizedTime, "D_Orgasm_OUT_A");
		}
		else if (_ai.IsName("D_Orgasm_OUT_A"))
		{
			StartProcTrigger(num);
			StartHoushiProc(1, _restart: true, num);
		}
		return true;
	}

	private bool AutoSonyu(AnimatorStateInfo _ai, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			AutoStartProcTrigger(_start: false, num);
			AutoStartSonyuProc(_restart: false, 0, _modeCtrl, num);
		}
		else if (_ai.IsName("Insert"))
		{
			InsertProc(_ai.normalizedTime, 0);
		}
		else if (_ai.IsName("WLoop"))
		{
			AutoLoopSonyuProc(0, 0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			AutoLoopSonyuProc(1, 0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			AutoOLoopProc(0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("OrgasmF_IN"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 0, _modeCtrl, 0);
		}
		else if (_ai.IsName("OrgasmM_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 0, _modeCtrl, _finishMorS: false);
		}
		else if (_ai.IsName("OrgasmS_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 0, _modeCtrl, _finishMorS: true);
		}
		else if (_ai.IsName("Orgasm_IN_A"))
		{
			AutoAfterTheInsideWaitingProc(0, num);
		}
		else if (_ai.IsName("Pull"))
		{
			PullProc(_ai.normalizedTime, 0);
		}
		else if (_ai.IsName("Drop"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 4f, 0, _modeCtrl, 2);
		}
		else if (_ai.IsName("OrgasmM_OUT"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 0, _modeCtrl, 2);
		}
		else if (_ai.IsName("OrgasmM_OUT_A"))
		{
			AutoStartProcTrigger(_start: true, num);
			AutoStartSonyuProc(_restart: true, 0, _modeCtrl, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			StartProcTrigger(num);
			StartSonyuProc(_restart: false, 1, _modeCtrl, num);
		}
		else if (_ai.IsName("D_Insert"))
		{
			InsertProc(_ai.normalizedTime, 1);
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopSonyuProc(0, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopSonyuProc(1, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopSonyuProc(1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_OrgasmF_IN"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 1, _modeCtrl, 0);
		}
		else if (_ai.IsName("D_OrgasmM_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 1, _modeCtrl, _finishMorS: false);
		}
		else if (_ai.IsName("D_OrgasmS_IN"))
		{
			FinishNextAnimationByMorS(_ai.normalizedTime, 1f, 1, _modeCtrl, _finishMorS: true);
		}
		else if (_ai.IsName("D_Orgasm_IN_A"))
		{
			AfterTheInsideWaitingProc(1, num, _modeCtrl);
		}
		else if (_ai.IsName("D_Pull"))
		{
			PullProc(_ai.normalizedTime, 1);
		}
		else if (_ai.IsName("D_Drop"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 4f, 1, _modeCtrl, 2);
		}
		else if (_ai.IsName("D_OrgasmM_OUT"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 1f, 1, _modeCtrl, 2);
		}
		else if (_ai.IsName("D_OrgasmM_OUT_A"))
		{
			StartProcTrigger(num);
			StartSonyuProc(_restart: true, 1, _modeCtrl, num);
		}
		else if (_ai.IsName("Vomit"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 3f, 0, _modeCtrl, (!finishMorS) ? 1 : 0);
		}
		else if (_ai.IsName("D_Vomit"))
		{
			AfterTheNextWaitingAnimation(_ai.normalizedTime, 3f, 1, _modeCtrl, (!finishMorS) ? 1 : 0);
		}
		return true;
	}

	public override void setAnimationParamater()
	{
		animPar.breast = chaFemales[0].GetShapeBodyValue(1);
		HSceneFlagCtrl.LoopSpeeds loopSpeeds = ctrlFlag.loopSpeeds;
		float num = 0f;
		switch (ctrlFlag.loopType)
		{
		case 0:
			num = Mathf.Lerp(loopSpeeds.minLoopSpeedW, loopSpeeds.maxLoopSpeedW, ctrlFlag.speed);
			break;
		case 1:
			num = ctrlFlag.speed - 1f;
			num = Mathf.Lerp(loopSpeeds.minLoopSpeedS, loopSpeeds.maxLoopSpeedS, num);
			break;
		case 2:
			num = Mathf.Lerp(loopSpeeds.minLoopSpeedO, loopSpeeds.maxLoopSpeedO, ctrlFlag.speed);
			break;
		default:
			num = ctrlFlag.speed + 1f;
			break;
		}
		animPar.speed = num;
		animPar.m[0] = ctrlFlag.motions[0];
		animPar.m[1] = ctrlFlag.motions[1];
		for (int i = 0; i < chaFemales.Length; i++)
		{
			if (chaFemales[i].visibleAll && !(chaFemales[i].objBodyBone == null))
			{
				animPar.heights[i] = chaFemales[i].GetShapeBodyValue(0);
			}
		}
		for (int j = 0; j < chaFemales.Length; j++)
		{
			if (chaFemales[j].visibleAll && !(chaFemales[j].objTop == null))
			{
				chaFemales[j].setAnimatorParamFloat("height", animPar.heights[j]);
				chaFemales[j].setAnimatorParamFloat("speed", animPar.speed);
				chaFemales[j].setAnimatorParamFloat("motion", animPar.m[0]);
				chaFemales[j].setAnimatorParamFloat("motion1", animPar.m[1]);
				chaFemales[j].setAnimatorParamFloat("breast", animPar.breast);
				if (isHeight1Parameter)
				{
					chaFemales[j].setAnimatorParamFloat("height1", animPar.heights[j ^ 1]);
				}
			}
		}
		if (chaMales[0].objBodyBone != null)
		{
			chaMales[0].setAnimatorParamFloat("height", animPar.heights[0]);
			chaMales[0].setAnimatorParamFloat("speed", animPar.speed);
			chaMales[0].setAnimatorParamFloat("motion", animPar.m[0]);
			chaMales[0].setAnimatorParamFloat("breast", animPar.breast);
			if (isHeight1Parameter)
			{
				chaMales[0].setAnimatorParamFloat("height1", animPar.heights[1]);
			}
		}
		if (item.GetItem() != null)
		{
			item.setAnimatorParamFloat("height", animPar.heights[0]);
			item.setAnimatorParamFloat("speed", animPar.speed);
			item.setAnimatorParamFloat("motion", animPar.m[0]);
			if (isHeight1Parameter)
			{
				item.setAnimatorParamFloat("height1", animPar.heights[1]);
			}
		}
	}

	private void setPlay(string _playAnimation, bool _isFade = true)
	{
		chaFemales[0].animBody.Play(_playAnimation, 0, 0f);
		rootmotionOffsetF[0].Set(_playAnimation);
		if (chaFemales[1].visibleAll && chaFemales[1].objTop != null)
		{
			chaFemales[1].animBody.Play(_playAnimation, 0, 0f);
			rootmotionOffsetF[1].Set(_playAnimation);
		}
		if (chaMales[0].objTop != null)
		{
			chaMales[0].animBody.Play(_playAnimation, 0, 0f);
			rootmotionOffsetM[0].Set(_playAnimation);
		}
		for (int i = 0; i < lstMotionIK.Count; i++)
		{
			lstMotionIK[i].Item3.Calc(_playAnimation);
		}
		if (item != null)
		{
			item.setPlay(_playAnimation, 0);
		}
		if (_isFade)
		{
			fade.FadeStart(1f);
		}
		if (ctrlYures != null)
		{
			if (ctrlYures[0] != null)
			{
				ctrlYures[0].Proc(_playAnimation);
			}
			if (ctrlYures[1] != null && chaFemales[1].visibleAll && chaFemales[1].objTop != null)
			{
				ctrlYures[1].Proc(_playAnimation);
			}
		}
		if (ctrlFlag.voice.changeTaii)
		{
			ctrlFlag.voice.changeTaii = false;
		}
	}

	private bool StartProcTrigger(float _wheel)
	{
		if (_wheel == 0f || nextPlay != 0)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
			{
				Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
				voice.ResetVoice();
			}
		}
		nextPlay = 1;
		return true;
	}

	private bool StartAibuProc(bool _isReStart, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (!_isReStart)
			{
				nextPlay = 3;
			}
			else
			{
				nextPlay = 2;
				ctrlFlag.voice.playStart = 1;
			}
			return false;
		}
		if (nextPlay == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
				{
					if (wheel == 0f)
					{
						return false;
					}
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					voice.ResetVoice();
				}
			}
		}
		nextPlay = 0;
		setPlay("WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.motions[1] = 0f;
		ctrlFlag.nowSpeedStateFast = false;
		for (int j = 0; j < 2; j++)
		{
			timeMotions[j] = 0f;
			timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
			timeChangeMotionDeltaTimes[j] = 0f;
		}
		ctrlFlag.isNotCtrl = false;
		oldHit = false;
		feelHit.InitTime();
		if (_isReStart)
		{
			voice.AfterFinish();
		}
		return true;
	}

	private bool FaintnessStartProcTrigger(float _wheel)
	{
		if (_wheel == 0f || nextPlay != 0)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
			{
				Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
				voice.ResetVoice();
			}
		}
		nextPlay = 1;
		return true;
	}

	private bool FaintnessStartAibuProc(bool _start, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (_start)
			{
				nextPlay = 3;
			}
			else
			{
				nextPlay = 2;
				if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item2 == 0)
				{
					ctrlFlag.voice.playStart = 1;
				}
			}
			return false;
		}
		if (nextPlay == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
				{
					if (wheel == 0f)
					{
						return false;
					}
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					voice.ResetVoice();
				}
			}
		}
		nextPlay = 0;
		setPlay("D_WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.motions[1] = 0f;
		ctrlFlag.nowSpeedStateFast = false;
		ctrlFlag.isNotCtrl = false;
		oldHit = false;
		feelHit.InitTime();
		voice.AfterFinish();
		return true;
	}

	private bool LoopAibuProc(int _loop, int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			setPlay((_state == 0) ? "OLoop" : "D_OLoop");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 2;
			ctrlFlag.nowSpeedStateFast = false;
			ctrlFlag.feel_f = 0.75f;
			oldHit = false;
			ctrlFlag.isGaugeHit = false;
		}
		else
		{
			ctrlFlag.speed += _wheel;
			if (_loop == 0)
			{
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
			}
			else
			{
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 1.5f;
			}
			if (_state == 0)
			{
				for (int i = 0; i < 2; i++)
				{
					if (!chaFemales[i].visibleAll || chaFemales[i].objBodyBone == null)
					{
						continue;
					}
					timeChangeMotionDeltaTimes[i] += Time.deltaTime;
					if (!(timeChangeMotions[i] <= timeChangeMotionDeltaTimes[i]) || enableMotions[i])
					{
						continue;
					}
					timeChangeMotions[i] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
					timeChangeMotionDeltaTimes[i] = 0f;
					enableMotions[i] = true;
					timeMotions[i] = 0f;
					float num2 = 0f;
					if (allowMotions[i])
					{
						num2 = 1f - ctrlFlag.motions[i];
						num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] + Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 1f);
						if (num2 >= 1f)
						{
							allowMotions[i] = false;
						}
					}
					else
					{
						num2 = ctrlFlag.motions[i];
						num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] - Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 0f);
						if (num2 <= 0f)
						{
							allowMotions[i] = true;
						}
					}
					lerpMotions[i] = new Vector2(ctrlFlag.motions[i], num2);
					lerpTimes[i] = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
				}
			}
			if (_loop == 0)
			{
				if (ctrlFlag.speed > 1f && ctrlFlag.loopType == 0)
				{
					setPlay((_state == 0) ? "SLoop" : "D_SLoop");
					ctrlFlag.nowSpeedStateFast = false;
					feelHit.InitTime();
					ctrlFlag.loopType = 1;
				}
				ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 2f);
			}
			else
			{
				if (ctrlFlag.speed < 1f && ctrlFlag.loopType == 1)
				{
					setPlay((_state == 0) ? "WLoop" : "D_WLoop");
					ctrlFlag.nowSpeedStateFast = true;
					feelHit.InitTime();
					ctrlFlag.loopType = 0;
				}
				ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 2f);
			}
			ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, _loop, (_loop == 0) ? ctrlFlag.speed : (ctrlFlag.speed - 1f), resist);
			if (ctrlFlag.isGaugeHit)
			{
				feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
				num = Time.deltaTime * ctrlFlag.speedGuageRate;
				num *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					num = 0f;
				}
				ctrlFlag.feel_f += num;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					ctrlFlag.feel_f = 0.74f;
				}
			}
			if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit)
			{
				if (randVoicePlays[0].Get() == 0)
				{
					ctrlFlag.voice.playVoices[0] = true;
				}
				else if (randVoicePlays[1].Get() == 0)
				{
					ctrlFlag.voice.playVoices[1] = true;
				}
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0))
				{
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3)
					{
						ctrlFlag.voice.playShorts[0] = 0;
					}
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 2)
					{
						ctrlFlag.voice.playShorts[1] = 0;
					}
				}
				ctrlFlag.voice.dialog = false;
			}
			oldHit = ctrlFlag.isGaugeHit;
			if (ctrlFlag.feel_f >= 0.75f)
			{
				setPlay((_state == 0) ? "OLoop" : "D_OLoop");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = 2;
				ctrlFlag.nowSpeedStateFast = false;
				oldHit = false;
				feelHit.InitTime();
			}
		}
		return true;
	}

	private bool OLoopAibuProc(int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
		ctrlFlag.speed = Mathf.Clamp01(ctrlFlag.speed + _wheel);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, 2, ctrlFlag.speed, resist);
		if (ctrlFlag.isGaugeHit)
		{
			num = Time.deltaTime * ctrlFlag.speedGuageRate;
			num *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
			ctrlFlag.feel_f += num;
			ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
		}
		if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit)
		{
			if (randVoicePlays[0].Get() == 0)
			{
				ctrlFlag.voice.playVoices[0] = true;
			}
			else if (randVoicePlays[1].Get() == 0)
			{
				ctrlFlag.voice.playVoices[1] = true;
			}
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0))
			{
				if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3)
				{
					ctrlFlag.voice.playShorts[0] = 0;
				}
				if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 2)
				{
					ctrlFlag.voice.playShorts[1] = 0;
				}
			}
			ctrlFlag.voice.dialog = false;
		}
		oldHit = ctrlFlag.isGaugeHit;
		if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
		{
			setPlay((_state == 0) ? "Orgasm" : "D_Orgasm");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.voice.oldFinish = 0;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(1);
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.numOrgasmF2++;
			ctrlFlag.voice.dialog = false;
			ctrlFlag.rateNip = 1f;
			bool sio = Manager.Config.HData.Sio;
			bool urine = Manager.Config.HData.Urine;
			if (sio)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else
			{
				bool flag = false;
				switch (resist)
				{
				case 0:
					flag = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag)
				{
					flag &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
				{
					particle.Play(0);
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
					{
						particle.Play(1);
					}
				}
			}
			bool flag2 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag2 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag2 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag2 = false;
			}
			if (urine || flag2)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					if (ProcBase.hSceneManager.UrineType == 1)
					{
						particle.Play(3);
					}
					else if (ProcBase.hSceneManager.UrineType == 0)
					{
						ctrlObi.PlayUrine(use: true, 1);
					}
					ctrlFlag.voice.urines[1] = true;
				}
				ctrlFlag.numUrine++;
			}
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
		}
		return true;
	}

	private bool GotoFaintnessAibu(int _state)
	{
		bool flag = !Manager.Config.HData.WeakStop;
		if (_state == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
		{
			setPlay("D_Orgasm_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 0;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.numFaintness = Mathf.Clamp(ctrlFlag.numFaintness + 1, 0, 999999);
			sprite.SetVisibleLeaveItToYou(_visible: false);
		}
		else
		{
			setPlay((_state == 0) ? "Orgasm_A" : "D_Orgasm_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
		}
		ctrlFlag.nowOrgasm = false;
		ctrlObi.PlayUrine(use: false);
		return true;
	}

	private bool OrgasmAibuProc(int _state, float _normalizedTime)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		GotoFaintnessAibu(_state);
		return true;
	}

	private bool StartHoushiProc(int _state, bool _restart, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (!_restart)
			{
				nextPlay = 3;
			}
			else
			{
				nextPlay = 2;
				ctrlFlag.voice.playStart = 1;
			}
			return false;
		}
		if (nextPlay == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
				{
					if (wheel == 0f)
					{
						return false;
					}
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					voice.ResetVoice();
				}
			}
		}
		nextPlay = 0;
		setPlay((_state == 0) ? "WLoop" : "D_WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.isNotCtrl = false;
		ctrlFlag.nowSpeedStateFast = false;
		oldHit = false;
		for (int j = 0; j < 2; j++)
		{
			ctrlFlag.motions[j] = 0f;
			timeMotions[j] = 0f;
			timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
			timeChangeMotionDeltaTimes[j] = 0f;
		}
		feelHit.InitTime();
		if (_restart)
		{
			voice.AfterFinish();
		}
		if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 3)
		{
			if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
			{
				ctrlFlag.voice.playShorts[0] = 2;
			}
			else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
			{
				ctrlFlag.voice.playShorts[0] = 1;
			}
		}
		if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 2)
		{
			if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
			{
				ctrlFlag.voice.playShorts[1] = 2;
			}
			else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
			{
				ctrlFlag.voice.playShorts[1] = 1;
			}
		}
		return true;
	}

	private bool LoopHoushiProc(int _loop, int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			setPlay((_state == 0) ? "OLoop" : "D_OLoop");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 2;
			ctrlFlag.nowSpeedStateFast = false;
			ctrlFlag.feel_m = 0.75f;
			oldHit = false;
			for (int i = 0; i < chaFemales.Length; i++)
			{
				if (!(chaFemales[i].objBodyBone == null) && (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice))
				{
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
				}
			}
			ctrlFlag.voice.dialog = false;
			feelHit.InitTime();
			ctrlFlag.isGaugeHit = false;
		}
		else
		{
			ctrlFlag.speed += _wheel;
			if (_loop == 0)
			{
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
			}
			else
			{
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 1.5f;
			}
			for (int j = 0; j < 2; j++)
			{
				if (!chaFemales[j].visibleAll || chaFemales[j].objBodyBone == null)
				{
					continue;
				}
				timeChangeMotionDeltaTimes[j] += Time.deltaTime;
				if (!(timeChangeMotions[j] <= timeChangeMotionDeltaTimes[j]) || enableMotions[j])
				{
					continue;
				}
				timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[j] = 0f;
				enableMotions[j] = true;
				timeMotions[j] = 0f;
				float num = 0f;
				if (allowMotions[j])
				{
					num = 1f - ctrlFlag.motions[j];
					num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[j] + Random.Range(ctrlFlag.changeMotionMinRate, num)) : 1f);
					if (num >= 1f)
					{
						allowMotions[j] = false;
					}
				}
				else
				{
					num = ctrlFlag.motions[j];
					num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[j] - Random.Range(ctrlFlag.changeMotionMinRate, num)) : 0f);
					if (num <= 0f)
					{
						allowMotions[j] = true;
					}
				}
				lerpMotions[j] = new Vector2(ctrlFlag.motions[j], num);
				lerpTimes[j] = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
			}
			if (_loop == 0)
			{
				if (ctrlFlag.speed > 1f && ctrlFlag.loopType == 0)
				{
					setPlay((_state == 0) ? "SLoop" : "D_SLoop");
					ctrlFlag.nowSpeedStateFast = false;
					for (int k = 0; k < 2; k++)
					{
						if (chaFemales[k].visibleAll && !(chaFemales[k].objBodyBone == null) && (voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.startVoice))
						{
							Voice.Stop(ctrlFlag.voice.voiceTrs[k]);
						}
					}
					ctrlFlag.voice.dialog = false;
					feelHit.InitTime();
					ctrlFlag.loopType = 1;
				}
				ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 2f);
			}
			else
			{
				if (ctrlFlag.speed < 1f && ctrlFlag.loopType == 1)
				{
					setPlay((_state == 0) ? "WLoop" : "D_WLoop");
					ctrlFlag.nowSpeedStateFast = true;
					for (int l = 0; l < 2; l++)
					{
						if (chaFemales[l].visibleAll && !(chaFemales[l].objBodyBone == null) && (voice.nowVoices[l].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[l].state == HVoiceCtrl.VoiceKind.startVoice))
						{
							Voice.Stop(ctrlFlag.voice.voiceTrs[l]);
						}
					}
					ctrlFlag.voice.dialog = false;
					feelHit.InitTime();
					ctrlFlag.loopType = 0;
				}
				ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 2f);
			}
			ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, _loop, (_loop == 0) ? ctrlFlag.speed : (ctrlFlag.speed - 1f), resist);
			if (ctrlFlag.isGaugeHit)
			{
				feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
			}
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			float num2 = 0f;
			num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
			num2 *= (ctrlFlag.isGaugeHit ? 2f : 1f) * (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
			ctrlFlag.feel_m += num2;
			ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
			if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit)
			{
				if (randVoicePlays[0].Get() == 0)
				{
					ctrlFlag.voice.playVoices[0] = true;
				}
				else if (randVoicePlays[1].Get() == 0)
				{
					ctrlFlag.voice.playVoices[1] = true;
				}
				ctrlFlag.voice.dialog = false;
			}
			oldHit = ctrlFlag.isGaugeHit;
			if (ctrlFlag.feel_m >= 0.75f)
			{
				setPlay((_state == 0) ? "OLoop" : "D_OLoop");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = 2;
				ctrlFlag.nowSpeedStateFast = false;
				for (int m = 0; m < 2; m++)
				{
					if (!chaFemales[m].visibleAll && !(chaFemales[m].objBodyBone == null) && (voice.nowVoices[m].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[m].state == HVoiceCtrl.VoiceKind.startVoice))
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[m]);
					}
				}
				ctrlFlag.voice.dialog = false;
				oldHit = false;
				feelHit.InitTime();
			}
		}
		return true;
	}

	private bool OLoopHoushiProc(int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		ctrlFlag.speed = Mathf.Clamp01(ctrlFlag.speed + _wheel);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, 2, ctrlFlag.speed, resist);
		ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
		float num = 0f;
		num = Time.deltaTime * ctrlFlag.speedGuageRate;
		num *= (ctrlFlag.isGaugeHit ? 2f : 1f) * (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
		ctrlFlag.feel_m += num;
		ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
		if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit)
		{
			if (randVoicePlays[0].Get() == 0)
			{
				ctrlFlag.voice.playVoices[0] = true;
			}
			else if (randVoicePlays[1].Get() == 0)
			{
				ctrlFlag.voice.playVoices[1] = true;
			}
			ctrlFlag.voice.dialog = false;
		}
		oldHit = ctrlFlag.isGaugeHit;
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide)
		{
			setPlay((_state == 0) ? "Orgasm_OUT" : "D_Orgasm_OUT");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.numShotF2++;
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(5);
			ctrlFlag.voice.dialog = false;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishDrink && _modeCtrl != 1)
		{
			setPlay("Orgasm_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.voice.dialog = false;
			finishMotion = 0;
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(4);
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishVomit && _modeCtrl != 1)
		{
			setPlay("Orgasm_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.voice.dialog = false;
			finishMotion = 1;
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(4);
		}
		return true;
	}

	private bool SetNextFinishAnimation(float _normalizedTime, string _nextAnimation, bool _isSpriteSet = true, bool _isFade = true)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		setPlay(_nextAnimation, _isFade);
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = -1;
		if (_isSpriteSet)
		{
			ctrlFlag.nowOrgasm = false;
			ctrlObi.PlayUrine(use: false);
		}
		return true;
	}

	private bool SetAfterInsideFinishAnimation(int _state, float _normalizedTime)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		if (finishMotion == 0)
		{
			ctrlFlag.numDrink = Mathf.Clamp(ctrlFlag.numDrink + 1, 0, 999999);
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			setPlay("Drink_IN");
		}
		else if (finishMotion == 1)
		{
			ctrlFlag.numVomit = Mathf.Clamp(ctrlFlag.numVomit + 1, 0, 999999);
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			setPlay("Vomit_IN");
		}
		ctrlFlag.numShotF2++;
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = -1;
		return true;
	}

	private bool StartSonyuProc(bool _restart, int _state, int _modeCtrl, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (!_restart)
			{
				nextPlay = 3;
			}
			else
			{
				nextPlay = 2;
				ctrlFlag.voice.playStart = 1;
			}
			return false;
		}
		if (nextPlay == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
				{
					if (wheel == 0f)
					{
						return false;
					}
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					voice.ResetVoice();
				}
			}
		}
		if (_modeCtrl == 3)
		{
			setPlay((_state == 0) ? "Insert" : "D_Insert");
			ctrlFlag.loopType = -1;
			if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 3)
			{
				if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
				{
					ctrlFlag.voice.playShorts[0] = 2;
				}
				else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
				{
					ctrlFlag.voice.playShorts[0] = 1;
				}
			}
			if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 2)
			{
				if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
				{
					ctrlFlag.voice.playShorts[1] = 2;
				}
				else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
				{
					ctrlFlag.voice.playShorts[1] = 1;
				}
			}
		}
		else
		{
			setPlay((_state == 0) ? "WLoop" : "D_WLoop");
			ctrlFlag.loopType = 0;
		}
		nextPlay = 0;
		ctrlFlag.speed = 0f;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.motions[1] = 0f;
		ctrlFlag.isNotCtrl = false;
		ctrlFlag.nowSpeedStateFast = false;
		timeMotions[0] = 0f;
		timeMotions[1] = 0f;
		oldHit = false;
		feelHit.InitTime();
		if (_state == 0)
		{
			for (int j = 0; j < 2; j++)
			{
				timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[j] = 0f;
			}
		}
		if (_restart)
		{
			voice.AfterFinish();
		}
		return true;
	}

	private bool InsertProc(float _normalizedTime, int _state)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		setPlay((_state == 0) ? "WLoop" : "D_WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		return true;
	}

	private bool LoopSonyuProc(int _loop, int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f && _modeCtrl == 3)
		{
			string[] array = new string[2] { "OrgasmM_IN", "D_OrgasmM_IN" };
			setPlay(array[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.numShotF2++;
			ctrlFlag.voice.dialog = false;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f)
		{
			string[] array2 = new string[2] { "OrgasmM_OUT", "D_OrgasmM_OUT" };
			setPlay(array2[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.numShotF2++;
			ctrlFlag.voice.dialog = false;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			string[] array3 = new string[2] { "OLoop", "D_OLoop" };
			setPlay(array3[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 2;
			ctrlFlag.feel_f = 0.75f;
			if (ctrlFlag.feel_m <= 0.75f)
			{
				ctrlFlag.feel_m = 0.75f;
			}
			ctrlFlag.nowSpeedStateFast = false;
			oldHit = false;
			feelHit.InitTime();
			for (int i = 0; i < 2; i++)
			{
				if (chaFemales[i].visibleAll && chaFemales[i].objTop != null)
				{
					if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice)
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					}
					else if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					}
				}
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
		}
		else
		{
			ctrlFlag.speed += _wheel;
			if (_loop == 0)
			{
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
			}
			else
			{
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 1.5f;
			}
			for (int j = 0; j < 2; j++)
			{
				if (!chaFemales[j].visibleAll || chaFemales[j].objBodyBone == null)
				{
					continue;
				}
				timeChangeMotionDeltaTimes[j] += Time.deltaTime;
				if (!(timeChangeMotions[j] <= timeChangeMotionDeltaTimes[j]) || enableMotions[j] || _state != 0)
				{
					continue;
				}
				timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[j] = 0f;
				enableMotions[j] = true;
				timeMotions[j] = 0f;
				float num = 0f;
				if (allowMotions[j])
				{
					num = 1f - ctrlFlag.motions[j];
					num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[j] + Random.Range(ctrlFlag.changeMotionMinRate, num)) : 1f);
					if (num >= 1f)
					{
						allowMotions[j] = false;
					}
				}
				else
				{
					num = ctrlFlag.motions[j];
					num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[j] - Random.Range(ctrlFlag.changeMotionMinRate, num)) : 0f);
					if (num <= 0f)
					{
						allowMotions[j] = true;
					}
				}
				lerpMotions[j] = new Vector2(ctrlFlag.motions[j], num);
				lerpTimes[j] = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
			}
			if (_loop == 0)
			{
				if (ctrlFlag.speed > 1f && ctrlFlag.loopType == 0)
				{
					setPlay((_state == 0) ? "SLoop" : "D_SLoop");
					ctrlFlag.nowSpeedStateFast = false;
					feelHit.InitTime();
					ctrlFlag.loopType = 1;
				}
				ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 2f);
			}
			else
			{
				if (ctrlFlag.speed < 1f && ctrlFlag.loopType == 1)
				{
					setPlay((_state == 0) ? "WLoop" : "D_WLoop");
					ctrlFlag.nowSpeedStateFast = true;
					feelHit.InitTime();
					ctrlFlag.loopType = 0;
				}
				ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 2f);
			}
			float num2 = 0f;
			if (_state != 1 || _modeCtrl != 4)
			{
				num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
				num2 *= (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
				ctrlFlag.feel_m += num2;
				ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
			}
			ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, _loop, (_loop == 0) ? ctrlFlag.speed : (ctrlFlag.speed - 1f), resist);
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			float num3 = 0f;
			if (ctrlFlag.isGaugeHit)
			{
				feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
				num3 = Time.deltaTime * ctrlFlag.speedGuageRate;
				num3 *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					num3 = 0f;
				}
				ctrlFlag.feel_f += num3;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					ctrlFlag.feel_f = 0.74f;
				}
			}
			if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit)
			{
				if (randVoicePlays[0].Get() == 0)
				{
					ctrlFlag.voice.playVoices[0] = true;
				}
				else if (randVoicePlays[1].Get() == 0)
				{
					ctrlFlag.voice.playVoices[1] = true;
				}
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && _modeCtrl != 4)
				{
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3)
					{
						ctrlFlag.voice.playShorts[0] = 0;
					}
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 2)
					{
						ctrlFlag.voice.playShorts[1] = 0;
					}
				}
				ctrlFlag.voice.dialog = false;
			}
			oldHit = ctrlFlag.isGaugeHit;
			if (ctrlFlag.feel_f >= 0.75f)
			{
				setPlay((_state == 0) ? "OLoop" : "D_OLoop");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = 2;
				ctrlFlag.nowSpeedStateFast = false;
				oldHit = false;
				feelHit.InitTime();
				for (int k = 0; k < 2; k++)
				{
					if (chaFemales[k].visibleAll && !(chaFemales[k].objTop == null) && (voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.startVoice))
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[k]);
					}
				}
			}
		}
		return true;
	}

	private bool OLoopSonyuProc(int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			if (ctrlFlag.feel_m <= 0.75f)
			{
				ctrlFlag.feel_m = 0.75f;
				ctrlFlag.isGaugeHit = false;
				ctrlFlag.isGaugeHit_M = false;
			}
			return true;
		}
		float num = 0f;
		if (_state != 1 || _modeCtrl != 4)
		{
			num = Time.deltaTime * ctrlFlag.speedGuageRate;
			num *= (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
			ctrlFlag.feel_m += num;
			ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
		}
		ctrlFlag.speed = Mathf.Clamp01(ctrlFlag.speed + _wheel);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, 2, ctrlFlag.speed, resist);
		ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
		float num2 = 0f;
		if (ctrlFlag.isGaugeHit)
		{
			num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
			num2 *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
			ctrlFlag.feel_f += num2;
			ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
		}
		if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit)
		{
			if (randVoicePlays[0].Get() == 0)
			{
				ctrlFlag.voice.playVoices[0] = true;
			}
			else if (randVoicePlays[1].Get() == 0)
			{
				ctrlFlag.voice.playVoices[1] = true;
			}
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && _modeCtrl != 4)
			{
				if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3)
				{
					ctrlFlag.voice.playShorts[0] = 0;
				}
				if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 2)
				{
					ctrlFlag.voice.playShorts[1] = 0;
				}
			}
			ctrlFlag.voice.dialog = false;
		}
		oldHit = ctrlFlag.isGaugeHit;
		if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
		{
			setPlay((_state == 0) ? "OrgasmF_IN" : "D_OrgasmF_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(1);
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 0;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.voice.dialog = false;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.numOrgasmF2++;
			ctrlFlag.rateNip = 1f;
			bool sio = Manager.Config.HData.Sio;
			bool urine = Manager.Config.HData.Urine;
			if (sio)
			{
				particle.Play(0);
				if ((bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else
			{
				bool flag = false;
				switch (resist)
				{
				case 0:
					flag = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag)
				{
					flag &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
				{
					particle.Play(0);
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
					{
						particle.Play(1);
					}
				}
			}
			bool flag2 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag2 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag2 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag2 = false;
			}
			if (urine || flag2)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					if (ProcBase.hSceneManager.UrineType == 1)
					{
						particle.Play(3);
					}
					else if (ProcBase.hSceneManager.UrineType == 0)
					{
						ctrlObi.PlayUrine(use: true, 1);
					}
					ctrlFlag.voice.urines[1] = true;
				}
				ctrlFlag.numUrine++;
			}
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f && _modeCtrl == 3)
		{
			setPlay((_state == 0) ? "OrgasmM_IN" : "D_OrgasmM_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.numShotF2++;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.voice.dialog = false;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f)
		{
			setPlay((_state == 0) ? "OrgasmM_OUT" : "D_OrgasmM_OUT");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.numShotF2++;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.voice.dialog = false;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishSame && ctrlFlag.feel_m >= 0.75f)
		{
			setPlay((_state == 0) ? "OrgasmS_IN" : "D_OrgasmS_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(2);
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			ctrlFlag.numSameOrgasm = Mathf.Clamp(ctrlFlag.numSameOrgasm + 1, 0, 999999);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.numOrgasmF2++;
			ctrlFlag.voice.dialog = false;
			sprite.objMotionListPanel.SetActive(value: false);
			if (_modeCtrl == 3)
			{
				ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			}
			else
			{
				ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			}
			ctrlFlag.voice.oldFinish = 3;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.numShotF2++;
			bool urine2 = Manager.Config.HData.Urine;
			if (Manager.Config.HData.Sio)
			{
				particle.Play(0);
				if ((bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else
			{
				bool flag3 = false;
				switch (resist)
				{
				case 0:
					flag3 = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag3 = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag3 = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag3)
				{
					flag3 &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag3)
				{
					particle.Play(0);
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
					{
						particle.Play(1);
					}
				}
			}
			bool flag4 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag4 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag4 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag4 = false;
			}
			if (urine2 || flag4)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					if (ProcBase.hSceneManager.UrineType == 1)
					{
						particle.Play(3);
					}
					else if (ProcBase.hSceneManager.UrineType == 0)
					{
						ctrlObi.PlayUrine(use: true, 1);
					}
					ctrlFlag.voice.urines[1] = true;
				}
				ctrlFlag.numUrine++;
			}
		}
		return true;
	}

	private bool AfterTheNextWaitingAnimation(float _normalizedTime, float _loopCount, int _state, int _modeCtrl, int _nextAfter)
	{
		if (_normalizedTime < _loopCount)
		{
			return false;
		}
		switch (_nextAfter)
		{
		case 0:
			GotoFaintnessSonyu(_state, _modeCtrl, (_modeCtrl != 3) ? 1 : 0);
			break;
		case 1:
			setPlay((_state == 0) ? "Orgasm_IN_A" : "D_Orgasm_IN_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			break;
		case 2:
			setPlay((_state == 0) ? "OrgasmM_OUT_A" : "D_OrgasmM_OUT_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isInsert = false;
			break;
		}
		ctrlFlag.nowOrgasm = false;
		ctrlObi.PlayUrine(use: false);
		return true;
	}

	private bool GotoFaintnessSonyu(int _state, int _modeCtrl, int _nextAfter)
	{
		bool flag = !Manager.Config.HData.WeakStop;
		if (_state == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
		{
			setPlay((_nextAfter == 0) ? "D_Orgasm_IN_A" : "D_OrgasmM_OUT_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 0;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.numFaintness = Mathf.Clamp(ctrlFlag.numFaintness + 1, 0, 999999);
			sprite.SetVisibleLeaveItToYou(_visible: false);
			ctrlObi.ChangeSetupInfo(1);
			sprite.SetAnimationMenu();
			sprite.SetFinishSelect(7, _modeCtrl);
		}
		else
		{
			setPlay((_state != 0) ? ((_nextAfter == 0) ? "D_Orgasm_IN_A" : "D_OrgasmM_OUT_A") : ((_nextAfter == 0) ? "Orgasm_IN_A" : "OrgasmM_OUT_A"));
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
		}
		return true;
	}

	private bool FinishNextAnimationByMorS(float _normalizedTime, float _loopCount, int _state, int _modeCtrl, bool _finishMorS)
	{
		AfterTheNextWaitingAnimation(_normalizedTime, _loopCount, _state, _modeCtrl, (!_finishMorS) ? 1 : 0);
		return true;
	}

	private bool AfterTheInsideWaitingProc(int _state, float _wheel, int _modeCtrl)
	{
		for (int i = 0; i < 2; i++)
		{
			if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
			{
				if (_wheel == 0f)
				{
					return false;
				}
				Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
				voice.ResetVoice();
			}
		}
		switch (nextPlay)
		{
		case 0:
			if (_wheel < 0f)
			{
				setPlay((_state == 0) ? "Pull" : "D_Pull");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = -1;
				for (int l = 0; l < 2; l++)
				{
					ctrlFlag.motions[l] = 0f;
					timeMotions[l] = 0f;
				}
				sprite.objMotionListPanel.SetActive(value: false);
				ctrlFlag.nowOrgasm = true;
				voice.AfterFinish();
				oldHit = false;
				feelHit.InitTime();
			}
			else if (_wheel > 0f)
			{
				ctrlFlag.voice.playStart = 1;
				nextPlay = 1;
			}
			break;
		case 1:
		{
			setPlay((_state == 0) ? "WLoop" : "D_WLoop");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 0;
			ctrlFlag.nowSpeedStateFast = false;
			for (int j = 0; j < 2; j++)
			{
				ctrlFlag.motions[j] = 0f;
				timeMotions[j] = 0f;
			}
			if (_state == 0)
			{
				for (int k = 0; k < 2; k++)
				{
					timeChangeMotions[k] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
					timeChangeMotionDeltaTimes[k] = 0f;
				}
			}
			voice.AfterFinish();
			oldHit = false;
			feelHit.InitTime();
			nextPlay = 0;
			break;
		}
		}
		return true;
	}

	private bool PullProc(float _normalizedTime, int _state)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		if (ctrlFlag.isInsert)
		{
			setPlay((_state == 0) ? "Drop" : "D_Drop");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
		}
		else
		{
			setPlay((_state == 0) ? "OrgasmM_OUT_A" : "D_OrgasmM_OUT_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.nowOrgasm = false;
			ctrlObi.PlayUrine(use: false);
		}
		return true;
	}

	private void SetFinishCategoryEnable(AnimatorStateInfo _ai, int _modeCtrl)
	{
		bool flag = _ai.IsName("WLoop") || _ai.IsName("SLoop") || _ai.IsName("D_WLoop") || _ai.IsName("D_SLoop") || _ai.IsName("OLoop") || _ai.IsName("D_OLoop");
		bool flag2 = ctrlFlag.feel_m >= 0.75f && flag;
		bool flag3 = ctrlFlag.initiative == 0;
		switch (_modeCtrl)
		{
		case 0:
			flag3 &= ctrlFlag.feel_f < 0.75f;
			break;
		case 1:
		case 2:
			flag3 &= ctrlFlag.feel_m < 0.75f;
			break;
		case 3:
		case 4:
			flag3 &= ctrlFlag.feel_f < 0.75f || ctrlFlag.feel_m < 0.75f;
			break;
		}
		flag3 = flag3 && flag;
		sprite.categoryFinish.SetActive(flag3, 0);
		if (sprite.IsFinishVisible(1))
		{
			sprite.categoryFinish.SetActive(flag2 && _modeCtrl != 0, 1);
		}
		if (sprite.IsFinishVisible(5))
		{
			sprite.categoryFinish.SetActive(flag2 && ctrlFlag.feel_f >= 0.75f && (_modeCtrl == 3 || _modeCtrl == 4), 5);
		}
		if (sprite.IsFinishVisible(2))
		{
			sprite.categoryFinish.SetActive(flag2 && _modeCtrl == 3, 2);
		}
		if (sprite.IsFinishVisible(3))
		{
			sprite.categoryFinish.SetActive(flag2 && _modeCtrl == 2, 3);
		}
		if (sprite.IsFinishVisible(4))
		{
			sprite.categoryFinish.SetActive(flag2 && _modeCtrl == 2, 4);
		}
	}

	private HScene.AnimationListInfo RecoverFaintnessAi()
	{
		for (int i = 0; i < lstAnimation.Count; i++)
		{
			if (lstAnimation[i].nPositons.Contains(ProcBase.hSceneManager.height))
			{
				return lstAnimation[i];
			}
		}
		sbWarning.Clear();
		if (lstAnimation.Count == 0)
		{
			sbWarning.Append("RecoverFaintnessAi：失敗\n");
			return null;
		}
		sbWarning.Append("RecoverFaintnessAi：失敗\n").Append("回復後の体位を").Append(lstAnimation[0].nameAnimation)
			.Append("に設定");
		return lstAnimation[0];
	}

	private bool AutoStartProcTrigger(bool _start, float wheel)
	{
		if (wheel == 0f || nextPlay != 0)
		{
			return false;
		}
		if (!_start)
		{
			if (!auto.IsStart())
			{
				return false;
			}
		}
		else if (!auto.IsReStart())
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
			{
				Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
				voice.ResetVoice();
			}
		}
		nextPlay = 1;
		return true;
	}

	private bool AutoStartAibuProc(bool _isReStart, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (!_isReStart)
			{
				nextPlay = 3;
			}
			else
			{
				nextPlay = 2;
				ctrlFlag.voice.playStart = 1;
			}
			return false;
		}
		if (nextPlay == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
				{
					if (wheel == 0f)
					{
						return false;
					}
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					voice.ResetVoice();
				}
			}
		}
		nextPlay = 0;
		if (!_isReStart || (_isReStart && !auto.IsChangeActionAtRestart()))
		{
			setPlay("WLoop");
		}
		else
		{
			ctrlFlag.isAutoActionChange = true;
		}
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.motions[1] = 0f;
		ctrlFlag.nowSpeedStateFast = false;
		oldHit = false;
		for (int j = 0; j < 2; j++)
		{
			timeMotions[j] = 0f;
			timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
			timeChangeMotionDeltaTimes[j] = 0f;
		}
		ctrlFlag.isNotCtrl = false;
		auto.Reset();
		feelHit.InitTime();
		if (_isReStart)
		{
			voice.AfterFinish();
		}
		return true;
	}

	private bool AutoLoopAibuProc(int _loop, int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			setPlay((_state == 0) ? "OLoop" : "D_OLoop");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 2;
			ctrlFlag.feel_f = 0.75f;
			ctrlFlag.nowSpeedStateFast = false;
			auto.SetSpeed(0f);
			oldHit = false;
			feelHit.InitTime();
			ctrlFlag.isGaugeHit = false;
		}
		else
		{
			if (_state == 0)
			{
				for (int i = 0; i < 2; i++)
				{
					if (!chaFemales[i].visibleAll || chaFemales[i].objBodyBone == null)
					{
						continue;
					}
					timeChangeMotionDeltaTimes[i] += Time.deltaTime;
					if (!(timeChangeMotions[i] <= timeChangeMotionDeltaTimes[i]) || enableMotions[i])
					{
						continue;
					}
					timeChangeMotions[i] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
					timeChangeMotionDeltaTimes[i] = 0f;
					enableMotions[i] = true;
					timeMotions[i] = 0f;
					float num = 0f;
					if (allowMotions[i])
					{
						num = 1f - ctrlFlag.motions[i];
						num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] + Random.Range(ctrlFlag.changeMotionMinRate, num)) : 1f);
						if (num >= 1f)
						{
							allowMotions[i] = false;
						}
					}
					else
					{
						num = ctrlFlag.motions[i];
						num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] - Random.Range(ctrlFlag.changeMotionMinRate, num)) : 0f);
						if (num <= 0f)
						{
							allowMotions[i] = true;
						}
					}
					lerpMotions[i] = new Vector2(ctrlFlag.motions[i], num);
					lerpTimes[i] = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
				}
			}
			feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
			Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, _loop, resist);
			if (auto.ChangeLoopMotion(_loop == 1))
			{
				setPlay((_loop != 0) ? ((_state == 0) ? "WLoop" : "D_WLoop") : ((_state == 0) ? "SLoop" : "D_SLoop"));
				for (int j = 0; j < 2; j++)
				{
					if (voice.nowVoices[j].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[j].state == HVoiceCtrl.VoiceKind.startVoice)
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[j]);
					}
				}
				feelHit.InitTime();
				ctrlFlag.loopType = ((_loop == 0) ? 1 : 0);
			}
			else
			{
				auto.ChangeSpeed(_loop == 1, hitArea);
				if (auto.AddSpeed(_wheel, _loop))
				{
					setPlay((_loop != 0) ? ((_state == 0) ? "WLoop" : "D_WLoop") : ((_state == 0) ? "SLoop" : "D_SLoop"));
					for (int k = 0; k < 2; k++)
					{
						if (voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.startVoice)
						{
							Voice.Stop(ctrlFlag.voice.voiceTrs[k]);
						}
					}
					feelHit.InitTime();
					ctrlFlag.loopType = ((_loop == 0) ? 1 : 0);
				}
			}
			if (_loop == 0)
			{
				ctrlFlag.speed = auto.GetSpeed(_loop: false);
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
			}
			else
			{
				ctrlFlag.speed = auto.GetSpeed(_loop: true);
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 1.5f;
			}
			ctrlFlag.isGaugeHit = GlobalMethod.RangeOn((_loop == 0) ? ctrlFlag.speed : (ctrlFlag.speed - 1f), hitArea.x, hitArea.y);
			float num2 = 0f;
			if (_state == 1)
			{
				if (ctrlFlag.isGaugeHit)
				{
					num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
					num2 *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
					if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
					{
						num2 = 0f;
					}
					ctrlFlag.feel_f += num2;
					ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
					if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
					{
						ctrlFlag.feel_f = 0.74f;
					}
				}
			}
			else
			{
				num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
				num2 *= (ctrlFlag.isGaugeHit ? 1f : 0.3f) * (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					num2 = 0f;
				}
				ctrlFlag.feel_f += num2;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					ctrlFlag.feel_f = 0.74f;
				}
			}
			if (ctrlFlag.selectAnimationListInfo == null)
			{
				ctrlFlag.isAutoActionChange = auto.IsChangeActionAtLoop();
			}
			if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit && !ctrlFlag.isAutoActionChange)
			{
				if (randVoicePlays[0].Get() == 0)
				{
					ctrlFlag.voice.playVoices[0] = true;
				}
				else if (randVoicePlays[1].Get() == 0)
				{
					ctrlFlag.voice.playVoices[1] = true;
				}
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0))
				{
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3)
					{
						ctrlFlag.voice.playShorts[0] = 0;
					}
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 2)
					{
						ctrlFlag.voice.playShorts[1] = 0;
					}
				}
			}
			oldHit = ctrlFlag.isGaugeHit;
			if (ctrlFlag.feel_f >= 0.75f)
			{
				setPlay((_state == 0) ? "OLoop" : "D_OLoop");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = 2;
				ctrlFlag.nowSpeedStateFast = false;
				oldHit = false;
				auto.SetSpeed(0f);
				feelHit.InitTime();
			}
		}
		return true;
	}

	private bool AutoOLoopAibuProc(int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, 2, resist);
		auto.ChangeSpeed(_loop: false, hitArea);
		auto.AddSpeed(_wheel, 2);
		ctrlFlag.speed = auto.GetSpeed(_loop: false);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		ctrlFlag.isGaugeHit = GlobalMethod.RangeOn(ctrlFlag.speed, hitArea.x, hitArea.y);
		float num = 0f;
		if (_state == 1)
		{
			if (ctrlFlag.isGaugeHit)
			{
				num = Time.deltaTime * ctrlFlag.speedGuageRate;
				num *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
				ctrlFlag.feel_f += num;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
			}
		}
		else
		{
			num = Time.deltaTime * ctrlFlag.speedGuageRate;
			num *= (ctrlFlag.isGaugeHit ? 1f : 0.3f) * (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
			ctrlFlag.feel_f += num;
			ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
		}
		if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit && !ctrlFlag.isAutoActionChange)
		{
			if (randVoicePlays[0].Get() == 0)
			{
				ctrlFlag.voice.playVoices[0] = true;
			}
			else if (randVoicePlays[1].Get() == 0)
			{
				ctrlFlag.voice.playVoices[1] = true;
			}
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0))
			{
				ctrlFlag.voice.playShorts[0] = 0;
				ctrlFlag.voice.playShorts[1] = 0;
			}
		}
		oldHit = ctrlFlag.isGaugeHit;
		if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
		{
			setPlay((_state == 0) ? "Orgasm" : "D_Orgasm");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(1);
			ctrlFlag.voice.oldFinish = 0;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.numOrgasmF2++;
			ctrlFlag.rateNip = 1f;
			bool sio = Manager.Config.HData.Sio;
			bool urine = Manager.Config.HData.Urine;
			if (sio)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else
			{
				bool flag = false;
				switch (resist)
				{
				case 0:
					flag = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag)
				{
					flag &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
				{
					particle.Play(0);
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
					{
						particle.Play(1);
					}
				}
			}
			bool flag2 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag2 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag2 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag2 = false;
			}
			if (urine || flag2)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					if (ProcBase.hSceneManager.UrineType == 1)
					{
						particle.Play(3);
					}
					else if (ProcBase.hSceneManager.UrineType == 0)
					{
						ctrlObi.PlayUrine(use: true, 1);
					}
					ctrlFlag.voice.urines[1] = true;
				}
				ctrlFlag.numUrine++;
			}
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
		}
		return true;
	}

	private bool AutoStartHoushiProc(int _state, bool _restart, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (!_restart)
			{
				nextPlay = 3;
			}
			else
			{
				nextPlay = 2;
			}
			return false;
		}
		if (nextPlay == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
				{
					if (wheel != 0f)
					{
						return false;
					}
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					voice.ResetVoice();
				}
			}
		}
		if (!_restart || (_restart && !auto.IsChangeActionAtRestart()))
		{
			setPlay((_state == 0) ? "WLoop" : "D_WLoop");
		}
		else
		{
			ctrlFlag.isAutoActionChange = true;
		}
		nextPlay = 0;
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.isNotCtrl = false;
		ctrlFlag.nowSpeedStateFast = false;
		oldHit = false;
		for (int j = 0; j < 2; j++)
		{
			ctrlFlag.motions[j] = 0f;
			timeMotions[j] = 0f;
			timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
			timeChangeMotionDeltaTimes[j] = 0f;
		}
		feelHit.InitTime();
		if (_restart)
		{
			voice.AfterFinish();
		}
		if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 3)
		{
			if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
			{
				ctrlFlag.voice.playShorts[0] = 2;
			}
			else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
			{
				ctrlFlag.voice.playShorts[0] = 1;
			}
		}
		if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 2)
		{
			if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
			{
				ctrlFlag.voice.playShorts[1] = 2;
			}
			else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
			{
				ctrlFlag.voice.playShorts[1] = 1;
			}
		}
		auto.Reset();
		return true;
	}

	private bool AutoLoopHoushiProc(int _loop, int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			setPlay((_state == 0) ? "OLoop" : "D_OLoop");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 2;
			ctrlFlag.nowSpeedStateFast = false;
			ctrlFlag.feel_m = 0.75f;
			oldHit = false;
			feelHit.InitTime();
			ctrlFlag.isGaugeHit = false;
		}
		else
		{
			for (int i = 0; i < 2; i++)
			{
				if (!chaFemales[i].visibleAll || chaFemales[i].objBodyBone == null)
				{
					continue;
				}
				timeChangeMotionDeltaTimes[i] += Time.deltaTime;
				if (!(timeChangeMotions[i] <= timeChangeMotionDeltaTimes[i]) || enableMotions[i])
				{
					continue;
				}
				timeChangeMotions[i] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[i] = 0f;
				enableMotions[i] = true;
				timeMotions[i] = 0f;
				float num = 0f;
				if (allowMotions[i])
				{
					num = 1f - ctrlFlag.motions[i];
					num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] + Random.Range(ctrlFlag.changeMotionMinRate, num)) : 1f);
					if (num >= 1f)
					{
						allowMotions[i] = false;
					}
				}
				else
				{
					num = ctrlFlag.motions[i];
					num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] - Random.Range(ctrlFlag.changeMotionMinRate, num)) : 0f);
					if (num <= 0f)
					{
						allowMotions[i] = true;
					}
				}
				lerpMotions[i] = new Vector2(ctrlFlag.motions[i], num);
				lerpTimes[i] = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
			}
			string playAnimation = ((_loop != 0) ? ((_state == 0) ? "WLoop" : "D_WLoop") : ((_state == 0) ? "SLoop" : "D_SLoop"));
			if (auto.ChangeLoopMotion(_loop == 1))
			{
				setPlay(playAnimation);
				feelHit.InitTime();
				ctrlFlag.loopType = ((_loop == 0) ? 1 : 0);
			}
			else
			{
				auto.ChangeSpeed(_loop == 1, new Vector2(-1f, -1f));
				if (auto.AddSpeed(_wheel, _loop))
				{
					setPlay(playAnimation);
					feelHit.InitTime();
				}
			}
			if (_loop == 0)
			{
				ctrlFlag.speed = auto.GetSpeed(_loop: false);
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
			}
			else
			{
				ctrlFlag.speed = auto.GetSpeed(_loop: true);
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 1.5f;
			}
			feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
			ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, _loop, (_loop == 0) ? ctrlFlag.speed : (ctrlFlag.speed - 1f), resist);
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			float num2 = 0f;
			num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
			num2 *= (ctrlFlag.isGaugeHit ? 2f : 1f) * (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
			ctrlFlag.feel_m += num2;
			ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
			if (ctrlFlag.selectAnimationListInfo == null)
			{
				ctrlFlag.isAutoActionChange = auto.IsChangeActionAtLoop();
			}
			if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit && !ctrlFlag.isAutoActionChange)
			{
				if (randVoicePlays[0].Get() == 0)
				{
					ctrlFlag.voice.playVoices[0] = true;
				}
				else if (randVoicePlays[1].Get() == 0)
				{
					ctrlFlag.voice.playVoices[1] = true;
				}
				ctrlFlag.voice.dialog = false;
			}
			oldHit = ctrlFlag.isGaugeHit;
			if (ctrlFlag.feel_m >= 0.75f)
			{
				setPlay((_state == 0) ? "OLoop" : "D_OLoop");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = 2;
				ctrlFlag.nowSpeedStateFast = false;
				oldHit = false;
				feelHit.InitTime();
			}
		}
		return true;
	}

	private bool AutoOLoopHoushiProc(int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		auto.ChangeSpeed(_loop: false, new Vector2(-1f, -1f));
		auto.AddSpeed(_wheel, 2);
		ctrlFlag.speed = auto.GetSpeed(_loop: false);
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, 2, ctrlFlag.speed, resist);
		ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
		float num = 0f;
		num = Time.deltaTime * ctrlFlag.speedGuageRate;
		num *= (ctrlFlag.isGaugeHit ? 2f : 1f) * (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
		ctrlFlag.feel_m += num;
		ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
		if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit && !ctrlFlag.isAutoActionChange)
		{
			if (randVoicePlays[0].Get() == 0)
			{
				ctrlFlag.voice.playVoices[0] = true;
			}
			else if (randVoicePlays[1].Get() == 0)
			{
				ctrlFlag.voice.playVoices[1] = true;
			}
			ctrlFlag.voice.dialog = false;
		}
		oldHit = ctrlFlag.isGaugeHit;
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide || (ctrlFlag.initiative == 2 && ctrlFlag.feel_m >= 1f))
		{
			setPlay((_state == 0) ? "Orgasm_OUT" : "D_Orgasm_OUT");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.voice.dialog = false;
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.numShotF2++;
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(5);
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishDrink && _modeCtrl != 1)
		{
			setPlay("Orgasm_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.voice.dialog = false;
			finishMotion = 0;
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(4);
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishVomit && _modeCtrl != 1)
		{
			setPlay("Orgasm_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.voice.dialog = false;
			finishMotion = 1;
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(4);
		}
		return true;
	}

	private bool AutoStartSonyuProc(bool _restart, int _state, int _modeCtrl, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (!_restart)
			{
				nextPlay = 3;
			}
			else
			{
				nextPlay = 2;
				ctrlFlag.voice.playStart = 1;
			}
			return false;
		}
		if (nextPlay == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[i].state == HVoiceCtrl.VoiceKind.startVoice)
				{
					if (wheel == 0f)
					{
						return false;
					}
					Voice.Stop(ctrlFlag.voice.voiceTrs[i]);
					voice.ResetVoice();
				}
			}
		}
		nextPlay = 0;
		if (!_restart || (_restart && !auto.IsChangeActionAtRestart()))
		{
			if (_modeCtrl == 3)
			{
				setPlay((_state == 0) ? "Insert" : "D_Insert");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = -1;
				if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 3)
				{
					if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
					{
						ctrlFlag.voice.playShorts[0] = 2;
					}
					else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
					{
						ctrlFlag.voice.playShorts[0] = 1;
					}
				}
				if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 2)
				{
					if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(3))
					{
						ctrlFlag.voice.playShorts[1] = 2;
					}
					else if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
					{
						ctrlFlag.voice.playShorts[1] = 1;
					}
				}
			}
			else
			{
				setPlay((_state == 0) ? "WLoop" : "D_WLoop");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = 0;
			}
		}
		else
		{
			ctrlFlag.isAutoActionChange = true;
		}
		ctrlFlag.speed = 0f;
		ctrlFlag.nowSpeedStateFast = false;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.motions[1] = 0f;
		timeMotions[0] = 0f;
		timeMotions[1] = 0f;
		oldHit = false;
		if (_state == 0)
		{
			for (int j = 0; j < 2; j++)
			{
				timeChangeMotions[j] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[j] = 0f;
			}
		}
		ctrlFlag.isNotCtrl = false;
		auto.Reset();
		feelHit.InitTime();
		if (_restart)
		{
			voice.AfterFinish();
		}
		return true;
	}

	private bool AutoLoopSonyuProc(int _loop, int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
		if (((ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f) || (ctrlFlag.initiative == 2 && ctrlFlag.feel_m >= 1f)) && _modeCtrl == 3)
		{
			string[] array = new string[2] { "OrgasmM_IN", "D_OrgasmM_IN" };
			setPlay(array[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.numShotF2++;
			ctrlFlag.nowOrgasm = true;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f)
		{
			string[] array2 = new string[2] { "OrgasmM_OUT", "D_OrgasmM_OUT" };
			setPlay(array2[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.numShotF2++;
			ctrlFlag.nowOrgasm = true;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			string[] array3 = new string[2] { "OLoop", "D_OLoop" };
			setPlay(array3[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 2;
			ctrlFlag.nowSpeedStateFast = false;
			ctrlFlag.feel_f = 0.75f;
			if (ctrlFlag.feel_m <= 0.75f)
			{
				ctrlFlag.feel_m = 0.75f;
			}
			oldHit = false;
			feelHit.InitTime();
		}
		else
		{
			for (int i = 0; i < 2; i++)
			{
				if (!chaFemales[i].visibleAll || chaFemales[i].objBodyBone == null)
				{
					continue;
				}
				timeChangeMotionDeltaTimes[i] += Time.deltaTime;
				if (!(timeChangeMotions[i] <= timeChangeMotionDeltaTimes[i]) || enableMotions[i] || _state != 0)
				{
					continue;
				}
				timeChangeMotions[i] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[i] = 0f;
				enableMotions[i] = true;
				timeMotions[i] = 0f;
				float num2 = 0f;
				if (allowMotions[i])
				{
					num2 = 1f - ctrlFlag.motions[i];
					num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] + Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 1f);
					if (num2 >= 1f)
					{
						allowMotions[i] = false;
					}
				}
				else
				{
					num2 = ctrlFlag.motions[i];
					num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[i] - Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 0f);
					if (num2 <= 0f)
					{
						allowMotions[i] = true;
					}
				}
				lerpMotions[i] = new Vector2(ctrlFlag.motions[i], num2);
				lerpTimes[i] = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
			}
			feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
			Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, _loop, resist);
			if (auto.ChangeLoopMotion(_loop == 1))
			{
				setPlay((_loop != 0) ? ((_state == 0) ? "WLoop" : "D_WLoop") : ((_state == 0) ? "SLoop" : "D_SLoop"));
				for (int j = 0; j < 2; j++)
				{
					if (chaFemales[j].visibleAll && !(chaFemales[j].objTop == null) && (voice.nowVoices[j].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[j].state == HVoiceCtrl.VoiceKind.startVoice) && ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(0))
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[j]);
					}
				}
				feelHit.InitTime();
				ctrlFlag.loopType = ((_loop == 0) ? 1 : 0);
			}
			else
			{
				auto.ChangeSpeed(_loop == 1, hitArea);
				if (auto.AddSpeed(_wheel, _loop))
				{
					setPlay((_loop != 0) ? ((_state == 0) ? "WLoop" : "D_WLoop") : ((_state == 0) ? "SLoop" : "D_SLoop"));
					feelHit.InitTime();
					ctrlFlag.loopType = ((_loop == 0) ? 1 : 0);
				}
			}
			if (_loop == 0)
			{
				ctrlFlag.speed = auto.GetSpeed(_loop: false);
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
			}
			else
			{
				ctrlFlag.speed = auto.GetSpeed(_loop: true);
				ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 1.5f;
			}
			ctrlFlag.isGaugeHit = GlobalMethod.RangeOn((_loop == 0) ? ctrlFlag.speed : (ctrlFlag.speed - 1f), hitArea.x, hitArea.y);
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			if (_state == 1)
			{
				if (ctrlFlag.isGaugeHit)
				{
					num = Time.deltaTime * ctrlFlag.speedGuageRate;
					ctrlFlag.feel_f *= ((!ctrlFlag.stopFeelFemale) ? 1 : 0);
					if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
					{
						num = 0f;
					}
					ctrlFlag.feel_f += num;
					ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
					if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
					{
						ctrlFlag.feel_f = 0.74f;
					}
				}
			}
			else
			{
				num = Time.deltaTime * ctrlFlag.speedGuageRate;
				num *= (ctrlFlag.isGaugeHit ? 1f : 0.3f) * (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					num = 0f;
				}
				ctrlFlag.feel_f += num;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
				if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
				{
					ctrlFlag.feel_f = 0.74f;
				}
			}
			float num3 = 0f;
			if (_state != 1 || _modeCtrl != 4)
			{
				num3 = Time.deltaTime * ctrlFlag.speedGuageRate;
				num3 *= (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
				ctrlFlag.feel_m += num3;
				ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
			}
			if (ctrlFlag.selectAnimationListInfo == null)
			{
				ctrlFlag.isAutoActionChange = auto.IsChangeActionAtLoop();
			}
			if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit && !ctrlFlag.isAutoActionChange)
			{
				if (randVoicePlays[0].Get() == 0)
				{
					ctrlFlag.voice.playVoices[0] = true;
				}
				else if (randVoicePlays[1].Get() == 0)
				{
					ctrlFlag.voice.playVoices[1] = true;
				}
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && _modeCtrl != 4)
				{
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3)
					{
						ctrlFlag.voice.playShorts[0] = 0;
					}
					if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 2)
					{
						ctrlFlag.voice.playShorts[1] = 0;
					}
				}
			}
			oldHit = ctrlFlag.isGaugeHit;
			if (ctrlFlag.feel_f >= 0.75f)
			{
				setPlay((_state == 0) ? "OLoop" : "D_OLoop");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = 2;
				ctrlFlag.nowSpeedStateFast = false;
				oldHit = false;
				feelHit.InitTime();
			}
		}
		return true;
	}

	private bool AutoOLoopProc(int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			if (ctrlFlag.feel_m <= 0.75f)
			{
				ctrlFlag.feel_m = 0.75f;
				ctrlFlag.isGaugeHit = false;
				ctrlFlag.isGaugeHit_M = false;
			}
			return true;
		}
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, 2, resist);
		auto.ChangeSpeed(_loop: false, hitArea);
		auto.AddSpeed(_wheel, 2);
		ctrlFlag.speed = auto.GetSpeed(_loop: false);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		ctrlFlag.isGaugeHit = GlobalMethod.RangeOn(ctrlFlag.speed, hitArea.x, hitArea.y);
		ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
		if (_state == 1)
		{
			if (ctrlFlag.isGaugeHit)
			{
				num = Time.deltaTime * ctrlFlag.speedGuageRate;
				num *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
				ctrlFlag.feel_f += num;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
			}
		}
		else
		{
			num = Time.deltaTime * ctrlFlag.speedGuageRate;
			num *= (ctrlFlag.isGaugeHit ? 1f : 0.3f) * (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
			ctrlFlag.feel_f += num;
			ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
		}
		float num2 = 0f;
		if (_state != 1 || _modeCtrl != 4)
		{
			num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
			num2 *= (float)((!ctrlFlag.stopFeelMale) ? 1 : 0);
			ctrlFlag.feel_m += num2;
			ctrlFlag.feel_m = Mathf.Clamp01(ctrlFlag.feel_m);
		}
		if (ctrlFlag.isGaugeHit != oldHit && ctrlFlag.isGaugeHit && !ctrlFlag.isAutoActionChange)
		{
			if (randVoicePlays[0].Get() == 0)
			{
				ctrlFlag.voice.playVoices[0] = true;
			}
			else if (randVoicePlays[1].Get() == 0)
			{
				ctrlFlag.voice.playVoices[1] = true;
			}
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && _modeCtrl != 4)
			{
				if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3)
				{
					ctrlFlag.voice.playShorts[0] = 0;
				}
				if (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 2)
				{
					ctrlFlag.voice.playShorts[1] = 0;
				}
			}
		}
		oldHit = ctrlFlag.isGaugeHit;
		if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
		{
			setPlay((_state == 0) ? "OrgasmF_IN" : "D_OrgasmF_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(1);
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 0;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.numOrgasmF2++;
			ctrlFlag.rateNip = 1f;
			bool sio = Manager.Config.HData.Sio;
			bool urine = Manager.Config.HData.Urine;
			if (sio)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else
			{
				bool flag = false;
				switch (resist)
				{
				case 0:
					flag = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag)
				{
					flag &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
				{
					particle.Play(0);
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
					{
						particle.Play(1);
					}
				}
			}
			bool flag2 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag2 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag2 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag2 = false;
			}
			if (urine || flag2)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					if (ProcBase.hSceneManager.UrineType == 1)
					{
						particle.Play(3);
					}
					else if (ProcBase.hSceneManager.UrineType == 0)
					{
						ctrlObi.PlayUrine(use: true, 1);
					}
					ctrlFlag.voice.urines[1] = true;
				}
				ctrlFlag.numUrine++;
			}
		}
		else if (((ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f) || (ctrlFlag.initiative == 2 && ctrlFlag.feel_m >= 1f)) && _modeCtrl == 3)
		{
			setPlay((_state == 0) ? "OrgasmM_IN" : "D_OrgasmM_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.numShotF2++;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f)
		{
			setPlay((_state == 0) ? "OrgasmM_OUT" : "D_OrgasmM_OUT");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.numShotF2++;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishSame && ctrlFlag.feel_m >= 0.75f)
		{
			setPlay((_state == 0) ? "OrgasmS_IN" : "D_OrgasmS_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.feel_m = 0f;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(2);
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			ctrlFlag.numSameOrgasm = Mathf.Clamp(ctrlFlag.numSameOrgasm + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.numOrgasmF2++;
			if (_modeCtrl == 3)
			{
				ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			}
			else
			{
				ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			}
			ctrlFlag.voice.oldFinish = 3;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.numShotF2++;
			bool sio2 = Manager.Config.HData.Sio;
			bool urine2 = Manager.Config.HData.Urine;
			if (sio2)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					particle.Play(1);
				}
			}
			else
			{
				bool flag3 = false;
				switch (resist)
				{
				case 0:
					flag3 = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag3 = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag3 = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag3)
				{
					flag3 &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag3)
				{
					particle.Play(0);
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
					{
						particle.Play(1);
					}
				}
			}
			bool flag4 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag4 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag4 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag4 = false;
			}
			if (urine2 || flag4)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone)
				{
					if (ProcBase.hSceneManager.UrineType == 1)
					{
						particle.Play(3);
					}
					else if (ProcBase.hSceneManager.UrineType == 0)
					{
						ctrlObi.PlayUrine(use: true, 1);
					}
					ctrlFlag.voice.urines[1] = true;
				}
				ctrlFlag.numUrine++;
			}
		}
		return true;
	}

	private bool AutoAfterTheInsideWaitingProc(int _state, float _wheel)
	{
		if (auto.IsPull(ctrlFlag.isInsert))
		{
			setPlay((_state == 0) ? "Pull" : "D_Pull");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.nowSpeedStateFast = false;
			ctrlFlag.motions[0] = 0f;
			ctrlFlag.motions[1] = 0f;
			timeMotions[0] = 0f;
			timeMotions[1] = 0f;
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			voice.AfterFinish();
			oldHit = false;
			feelHit.InitTime();
			auto.ReStartInit();
			return true;
		}
		if (!auto.IsReStart())
		{
			return false;
		}
		setPlay((_state == 0) ? "WLoop" : "D_WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.nowSpeedStateFast = false;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.motions[1] = 0f;
		timeMotions[0] = 0f;
		timeMotions[1] = 0f;
		oldHit = false;
		if (_state == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				timeChangeMotions[i] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[i] = 0f;
			}
		}
		voice.AfterFinish();
		auto.ReStartInit();
		auto.PullInit();
		return true;
	}
}
