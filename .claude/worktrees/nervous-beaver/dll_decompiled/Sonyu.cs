using AIChara;
using Manager;
using UnityEngine;

public class Sonyu : ProcBase
{
	private float timeMotion;

	private bool enableMotion;

	private bool allowMotion = true;

	private Vector2 lerpMotion = Vector2.zero;

	private float lerpTime;

	private float addFeel = 1f;

	private int nextPlay;

	private bool oldHit;

	private bool finishMorS;

	private int resist;

	private animParm animPar;

	public bool nowInsert;

	private bool canInside;

	private bool sixnine;

	private bool shokushu;

	public Sonyu(DeliveryMember _delivery)
		: base(_delivery)
	{
		animPar.heights = new float[1];
		animPar.m = new float[1];
		CatID = 2;
	}

	public override bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		shokushu = _infoAnimList.ActionCtrl.Item1 == 3 && (_modeCtrl == 1 || _modeCtrl == 7);
		canInside = (_infoAnimList.ActionCtrl.Item1 == 2 && _modeCtrl == 0) || shokushu;
		sixnine = _infoAnimList.ActionCtrl.Item1 == 3 && _modeCtrl == 0;
		if (_isIdle)
		{
			if ((_infoAnimList.ActionCtrl.Item1 == 2 && _modeCtrl == 1) || sixnine)
			{
				setPlay(ctrlFlag.isFaintness ? "D_OrgasmM_OUT_A" : "Idle", _fade: false);
			}
			else
			{
				setPlay(ctrlFlag.isFaintness ? "D_Idle" : "Idle", _fade: false);
			}
			voice.HouchiTime = 0f;
			ctrlFlag.loopType = -1;
			nowInsert = false;
		}
		else
		{
			if (ctrlFlag.feel_f >= 0.75f)
			{
				setPlay(ctrlFlag.isFaintness ? "D_OLoop" : "OLoop", _fade: false);
				ctrlFlag.loopType = 2;
			}
			else
			{
				setPlay(ctrlFlag.isFaintness ? "D_WLoop" : "WLoop", _fade: false);
				ctrlFlag.loopType = 0;
			}
			ctrlFlag.speed = 0f;
			ctrlFlag.motions[0] = 0f;
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
		addFeel = 1f;
		if (chaFemales[0].fileParam2.hAttribute != 3 && _infoAnimList.lstSystem.Contains(4) && chaFemales[0].fileGameInfo2.resistPain < 100)
		{
			addFeel = 0f;
		}
		nextPlay = 0;
		oldHit = false;
		ctrlFlag.voice.changeTaii = true;
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
			Manual(FemaleAi, _modeCtrl, _infoAnimList);
		}
		else
		{
			Auto(FemaleAi, _modeCtrl, _infoAnimList);
		}
		if (enableMotion)
		{
			timeMotion = Mathf.Clamp(timeMotion + Time.deltaTime, 0f, lerpTime);
			float time = Mathf.Clamp01(timeMotion / lerpTime);
			time = ctrlFlag.changeMotionCurve.Evaluate(time);
			ctrlFlag.motions[0] = Mathf.Lerp(lerpMotion.x, lerpMotion.y, time);
			if (time >= 1f)
			{
				enableMotion = false;
			}
		}
		ctrlObi.Proc(FemaleAi, ctrlFlag.isInsert);
		ctrlObi.PlayUrine(FemaleAi);
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.RecoverFaintness)
		{
			bool flag = false;
			if ((eventNo == 19) ? ctrlFlag.isFaintnessVoice : (FemaleAi.IsName("D_Idle") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop") || FemaleAi.IsName("D_OLoop") || FemaleAi.IsName("D_Orgasm_IN_A") || FemaleAi.IsName("D_OrgasmM_OUT_A")))
			{
				if (eventNo != 19)
				{
					SetRecoverTaii();
					if (ctrlFlag.nowAnimationInfo == ctrlFlag.selectAnimationListInfo)
					{
						if (canInside)
						{
							setPlay("Orgasm_IN_A");
						}
						else
						{
							setPlay("OrgasmM_OUT_A");
						}
					}
					ctrlFlag.isFaintness = false;
				}
				else if (canInside)
				{
					setPlay("D_Orgasm_IN_A");
					if (voice.playAnimation != null)
					{
						voice.playAnimation.SetAllFlags(_play: true, Animator.StringToHash("D_Orgasm_IN_A"));
					}
				}
				else
				{
					setPlay("D_OrgasmM_OUT_A");
					if (voice.playAnimation != null)
					{
						voice.playAnimation.SetAllFlags(_play: true, Animator.StringToHash("D_OrgasmM_OUT_A"));
					}
				}
				ctrlFlag.FaintnessType = -1;
				ctrlFlag.isFaintnessVoice = false;
				ctrlFlag.numOrgasm = 0;
				int item = ctrlFlag.nowAnimationInfo.ActionCtrl.Item1;
				int item2 = ctrlFlag.nowAnimationInfo.ActionCtrl.Item2;
				sprite.SetFinishSelect(2, _modeCtrl, item, item2);
				sprite.SetVisibleLeaveItToYou(_visible: true, _judgeLeaveItToYou: true);
				ctrlObi.ChangeSetupInfo(0);
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
		}
		SetFinishCategoryEnable(FemaleAi);
		if (eventNo >= 50 && eventNo < 56)
		{
			addFeel = 1f;
		}
		setAnimationParamater();
		return true;
	}

	private bool Manual(AnimatorStateInfo _ai, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			StartProcTrigger(num);
			StartProc(_restart: false, 0, _modeCtrl, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("Insert"))
		{
			InsertProc(_ai.normalizedTime, 0);
		}
		else if (_ai.IsName("WLoop"))
		{
			LoopProc(0, 0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			LoopProc(1, 0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			OLoopProc(0, num, _modeCtrl, _infoAnimList);
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
			AfterTheInsideWaitingProc(0, num);
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
			StartProc(_restart: true, 0, _modeCtrl, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			StartProcTrigger(num);
			StartProc(_restart: false, 1, _modeCtrl, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("D_Insert"))
		{
			InsertProc(_ai.normalizedTime, 1);
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopProc(0, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopProc(1, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopProc(1, num, _modeCtrl, _infoAnimList);
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
			AfterTheInsideWaitingProc(1, num);
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
			StartProc(_restart: true, 1, _modeCtrl, num);
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

	private bool Auto(AnimatorStateInfo _ai, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			AutoStartProcTrigger(_start: false, num);
			AutoStartProc(_restart: false, 0, _modeCtrl, num);
		}
		else if (_ai.IsName("Insert"))
		{
			InsertProc(_ai.normalizedTime, 0);
		}
		else if (_ai.IsName("WLoop"))
		{
			AutoLoopProc(0, 0, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			AutoLoopProc(1, 0, num, _modeCtrl, _infoAnimList);
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
			AutoAfterTheInsideWaitingProc(0, num, _modeCtrl);
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
			AutoStartProc(_restart: true, 0, _modeCtrl, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			StartProcTrigger(num);
			StartProc(_restart: false, 1, _modeCtrl, num);
		}
		else if (_ai.IsName("D_Insert"))
		{
			InsertProc(_ai.normalizedTime, 1);
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopProc(0, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopProc(1, 1, num, _modeCtrl, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopProc(1, num, _modeCtrl, _infoAnimList);
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
			AfterTheInsideWaitingProc(1, num);
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
			StartProc(_restart: true, 1, _modeCtrl, num);
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
		if (chaFemales[0].visibleAll && chaFemales[0].objTop != null)
		{
			animPar.heights[0] = chaFemales[0].GetShapeBodyValue(0);
			chaFemales[0].setAnimatorParamFloat("height", animPar.heights[0]);
			chaFemales[0].setAnimatorParamFloat("speed", animPar.speed);
			chaFemales[0].setAnimatorParamFloat("motion", animPar.m[0]);
			chaFemales[0].setAnimatorParamFloat("breast", animPar.breast);
		}
		if (chaMales[0].objTop != null && chaMales[0].visibleAll)
		{
			chaMales[0].setAnimatorParamFloat("height", animPar.heights[0]);
			chaMales[0].setAnimatorParamFloat("speed", animPar.speed);
			chaMales[0].setAnimatorParamFloat("motion", animPar.m[0]);
			chaMales[0].setAnimatorParamFloat("breast", animPar.breast);
		}
		if (item.GetItem() != null)
		{
			item.setAnimatorParamFloat("height", animPar.heights[0]);
			item.setAnimatorParamFloat("speed", animPar.speed);
			item.setAnimatorParamFloat("motion", animPar.m[0]);
		}
	}

	private void setPlay(string _playAnimation, bool _fade = true)
	{
		chaFemales[0].setPlay(_playAnimation, 0);
		rootmotionOffsetF[0].Set(_playAnimation);
		if (chaMales[0].objTop != null && chaMales[0].visibleAll)
		{
			chaMales[0].setPlay(_playAnimation, 0);
			rootmotionOffsetM[0].Set(_playAnimation);
		}
		if (item != null)
		{
			item.setPlay(_playAnimation);
		}
		for (int i = 0; i < lstMotionIK.Count; i++)
		{
			lstMotionIK[i].Item3.Calc(_playAnimation);
		}
		if (_fade)
		{
			fade.FadeStart(1f);
		}
		if (ctrlYures != null && ctrlYures[0] != null)
		{
			ctrlYures[0].Proc(_playAnimation);
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
		if (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice)
		{
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		nextPlay = 1;
		return true;
	}

	private bool StartProc(bool _restart, int _state, int _modeCtrl, float wheel)
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
				int num = Singleton<Game>.Instance.eventNo;
				if (num != 7 && num != 32)
				{
					ctrlFlag.voice.playStart = 1;
				}
			}
			return false;
		}
		if (nextPlay == 2 && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
		{
			if (wheel == 0f)
			{
				return false;
			}
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		if (canInside)
		{
			setPlay((_state == 0) ? "Insert" : "D_Insert");
			ctrlFlag.loopType = -1;
			nowInsert = true;
			sprite.objMotionListPanel.SetActive(value: false);
		}
		else
		{
			setPlay((_state == 0) ? "WLoop" : "D_WLoop");
			ctrlFlag.loopType = 0;
		}
		nextPlay = 0;
		ctrlFlag.speed = 0f;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.isNotCtrl = false;
		ctrlFlag.nowSpeedStateFast = false;
		timeMotion = 0f;
		oldHit = false;
		feelHit.InitTime();
		if (_state == 0)
		{
			timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
			timeChangeMotionDeltaTimes[0] = 0f;
		}
		if (_restart)
		{
			voice.AfterFinish();
		}
		if (sixnine)
		{
			ctrlFlag.voice.playShorts[0] = 1;
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
		nowInsert = false;
		return true;
	}

	private bool LoopProc(int _loop, int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		bool flag = !sixnine || _state != 1;
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f && canInside)
		{
			string[] array = new string[2] { "OrgasmM_IN", "D_OrgasmM_IN" };
			setPlay(array[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.voice.dialog = false;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f && flag)
		{
			string[] array2 = new string[2] { "OrgasmM_OUT", "D_OrgasmM_OUT" };
			setPlay(array2[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
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
			if (chaFemales[0].visibleAll && chaFemales[0].objTop != null && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
			{
				Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
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
			if (chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null)
			{
				timeChangeMotionDeltaTimes[0] += Time.deltaTime;
				if (timeChangeMotions[0] <= timeChangeMotionDeltaTimes[0] && !enableMotion && _state == 0)
				{
					timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
					timeChangeMotionDeltaTimes[0] = 0f;
					enableMotion = true;
					timeMotion = 0f;
					float num = 0f;
					if (allowMotion)
					{
						num = 1f - ctrlFlag.motions[0];
						num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] + Random.Range(ctrlFlag.changeMotionMinRate, num)) : 1f);
						if (num >= 1f)
						{
							allowMotion = false;
						}
					}
					else
					{
						num = ctrlFlag.motions[0];
						num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] - Random.Range(ctrlFlag.changeMotionMinRate, num)) : 0f);
						if (num <= 0f)
						{
							allowMotion = true;
						}
					}
					lerpMotion = new Vector2(ctrlFlag.motions[0], num);
					lerpTime = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
				}
			}
			if (_loop == 0)
			{
				if (ctrlFlag.speed > 1f && ctrlFlag.loopType == 0)
				{
					setPlay((_state == 0) ? "SLoop" : "D_SLoop");
					ctrlFlag.nowSpeedStateFast = false;
					if (chaFemales[0].visibleAll && chaFemales[0].objTop != null && ((voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice) & sixnine))
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
					}
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
					if (chaFemales[0].visibleAll && chaFemales[0].objTop != null && ((voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice) & sixnine))
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
					}
					feelHit.InitTime();
					ctrlFlag.loopType = 0;
				}
				ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 2f);
			}
			float num2 = 0f;
			if (_state != 1 || !sixnine)
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
				if (addFeel == 0f && ctrlFlag.feel_f >= 0.74f)
				{
					num3 = 0f;
				}
				ctrlFlag.feel_f += num3;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
				if (addFeel == 0f && ctrlFlag.feel_f >= 0.74f)
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
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && !sixnine && (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3))
				{
					ctrlFlag.voice.playShorts[0] = 0;
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
				if (chaFemales[0].visibleAll && chaFemales[0].objTop != null && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
				{
					Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
				}
			}
		}
		return true;
	}

	private bool OLoopProc(int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
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
		bool flag = !sixnine || _state != 1;
		if (_state != 1 || !sixnine)
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
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && !sixnine && (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3))
			{
				ctrlFlag.voice.playShorts[0] = 0;
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
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.voice.dialog = false;
			ctrlFlag.rateNip = 1f;
			bool urine = Manager.Config.HData.Urine;
			if (Manager.Config.HData.Sio)
			{
				particle.Play(0);
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
			}
			else
			{
				bool flag2 = false;
				switch (resist)
				{
				case 0:
					flag2 = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag2 = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag2 = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag2)
				{
					flag2 &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag2)
				{
					particle.Play(0);
				}
			}
			bool flag3 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag3 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag3 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag3 = false;
			}
			if (urine || flag3)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.numUrine++;
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
			}
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f && canInside)
		{
			setPlay((_state == 0) ? "OrgasmM_IN" : "D_OrgasmM_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.voice.dialog = false;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f && flag)
		{
			setPlay((_state == 0) ? "OrgasmM_OUT" : "D_OrgasmM_OUT");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.voice.dialog = false;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishSame && ctrlFlag.feel_m >= 0.75f && flag)
		{
			setPlay((_state == 0) ? "OrgasmS_IN" : "D_OrgasmS_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(2);
			ctrlFlag.feel_m = 0f;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			ctrlFlag.numSameOrgasm = Mathf.Clamp(ctrlFlag.numSameOrgasm + 1, 0, 999999);
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.voice.dialog = false;
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			if (canInside)
			{
				ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			}
			else
			{
				ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			}
			ctrlFlag.voice.oldFinish = 3;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			bool sio = Manager.Config.HData.Sio;
			bool urine2 = Manager.Config.HData.Urine;
			if (sio)
			{
				particle.Play(0);
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
			}
			else
			{
				bool flag4 = false;
				switch (resist)
				{
				case 0:
					flag4 = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag4 = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag4 = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag4)
				{
					flag4 &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag4)
				{
					particle.Play(0);
				}
			}
			bool flag5 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag5 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag5 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag5 = false;
			}
			if (urine2 || flag5)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.numUrine++;
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
			}
		}
		return true;
	}

	private bool GotoFaintness(int _state, int _modeCtrl, int _nextAfter)
	{
		bool flag = !Manager.Config.HData.WeakStop;
		if (((_state == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount) & (eventNo < 50 || eventNo > 55)) && flag)
		{
			setPlay((_nextAfter == 0) ? "D_Orgasm_IN_A" : "D_OrgasmM_OUT_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 1;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.numFaintness = Mathf.Clamp(ctrlFlag.numFaintness + 1, 0, 999999);
			sprite.SetVisibleLeaveItToYou(_visible: false);
			ctrlObi.ChangeSetupInfo(1);
			sprite.SetAnimationMenu();
			int item = ctrlFlag.nowAnimationInfo.ActionCtrl.Item1;
			int item2 = ctrlFlag.nowAnimationInfo.ActionCtrl.Item2;
			sprite.SetFinishSelect(2, _modeCtrl, item, item2);
		}
		else if (eventNo == 19 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
		{
			setPlay((_nextAfter == 0) ? "D_Orgasm_IN_A" : "D_OrgasmM_OUT_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 1;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.numFaintness = Mathf.Clamp(ctrlFlag.numFaintness + 1, 0, 999999);
			sprite.SetVisibleLeaveItToYou(_visible: false);
			ctrlObi.ChangeSetupInfo(1);
			sprite.SetAnimationMenu();
			int item3 = ctrlFlag.nowAnimationInfo.ActionCtrl.Item1;
			int item4 = ctrlFlag.nowAnimationInfo.ActionCtrl.Item2;
			sprite.SetFinishSelect(2, _modeCtrl, item3, item4);
		}
		else
		{
			setPlay((_state != 0) ? ((_nextAfter == 0) ? "D_Orgasm_IN_A" : "D_OrgasmM_OUT_A") : ((_nextAfter == 0) ? "Orgasm_IN_A" : "OrgasmM_OUT_A"));
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
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
			GotoFaintness(_state, _modeCtrl, (!canInside) ? 1 : 0);
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

	private bool FinishNextAnimationByMorS(float _normalizedTime, float _loopCount, int _state, int _modeCtrl, bool _finishMorS)
	{
		if (ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 3 && _modeCtrl == 1)
		{
			if (_normalizedTime < _loopCount)
			{
				return false;
			}
			finishMorS = _finishMorS;
			setPlay((_state == 0) ? "Vomit" : "D_Vomit");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
		}
		else
		{
			AfterTheNextWaitingAnimation(_normalizedTime, _loopCount, _state, _modeCtrl, (!_finishMorS) ? 1 : 0);
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

	private bool AfterTheInsideWaitingProc(int _state, float _wheel)
	{
		if (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice)
		{
			if (_wheel == 0f)
			{
				return false;
			}
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		switch (nextPlay)
		{
		case 0:
			if (_wheel < 0f)
			{
				setPlay((_state == 0) ? "Pull" : "D_Pull");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = -1;
				ctrlFlag.motions[0] = 0f;
				timeMotion = 0f;
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
			setPlay((_state == 0) ? "WLoop" : "D_WLoop");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 0;
			ctrlFlag.nowSpeedStateFast = false;
			ctrlFlag.motions[0] = 0f;
			timeMotion = 0f;
			if (_state == 0)
			{
				timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
				timeChangeMotionDeltaTimes[0] = 0f;
			}
			voice.AfterFinish();
			oldHit = false;
			feelHit.InitTime();
			nextPlay = 0;
			break;
		}
		return true;
	}

	private void SetFinishCategoryEnable(AnimatorStateInfo _ai)
	{
		bool flag = _ai.IsName("WLoop") || _ai.IsName("SLoop") || _ai.IsName("D_WLoop") || _ai.IsName("D_SLoop") || _ai.IsName("OLoop") || _ai.IsName("D_OLoop");
		bool flag2 = ctrlFlag.feel_m >= 0.75f && flag;
		bool active = ctrlFlag.initiative == 0 && (ctrlFlag.feel_f < 0.75f || ctrlFlag.feel_m < 0.75f) && flag;
		sprite.categoryFinish.SetActive(active, 0);
		if (sprite.IsFinishVisible(1))
		{
			sprite.categoryFinish.SetActive(flag2, 1);
		}
		if (sprite.IsFinishVisible(5))
		{
			sprite.categoryFinish.SetActive(flag2 && ctrlFlag.feel_f >= 0.75f, 5);
		}
		if (sprite.IsFinishVisible(2))
		{
			sprite.categoryFinish.SetActive(flag2, 2);
		}
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
		if (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice)
		{
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		nextPlay = 1;
		return true;
	}

	private bool AutoStartProc(bool _restart, int _state, int _modeCtrl, float wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			nextPlay = 2;
			if (!_restart)
			{
				nextPlay = 3;
			}
			else if (_state == 1)
			{
				ctrlFlag.voice.playStart = 1;
			}
			return false;
		}
		if (nextPlay == 2 && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
		{
			if (wheel == 0f)
			{
				return false;
			}
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		nextPlay = 0;
		if (!_restart || (_restart && !auto.IsChangeActionAtRestart()))
		{
			if (canInside)
			{
				setPlay((_state == 0) ? "Insert" : "D_Insert");
				ctrlFlag.speed = 0f;
				ctrlFlag.loopType = -1;
				nowInsert = true;
				sprite.objMotionListPanel.SetActive(value: false);
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
		timeMotion = 0f;
		oldHit = false;
		if (_state == 0)
		{
			timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
			timeChangeMotionDeltaTimes[0] = 0f;
		}
		ctrlFlag.isNotCtrl = false;
		auto.Reset();
		feelHit.InitTime();
		if (_restart)
		{
			voice.AfterFinish();
		}
		if (sixnine)
		{
			ctrlFlag.voice.playShorts[0] = 1;
		}
		return true;
	}

	private bool AutoLoopProc(int _loop, int _state, float _wheel, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (((ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f) || (ctrlFlag.initiative == 2 && ctrlFlag.feel_m >= 1f)) && canInside)
		{
			string[] array = new string[2] { "OrgasmM_IN", "D_OrgasmM_IN" };
			setPlay(array[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f && (!sixnine || _state != 1))
		{
			string[] array2 = new string[2] { "OrgasmM_OUT", "D_OrgasmM_OUT" };
			setPlay(array2[_state]);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
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
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
		}
		else
		{
			if (chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null)
			{
				timeChangeMotionDeltaTimes[0] += Time.deltaTime;
				if (timeChangeMotions[0] <= timeChangeMotionDeltaTimes[0] && !enableMotion && _state == 0)
				{
					timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
					timeChangeMotionDeltaTimes[0] = 0f;
					enableMotion = true;
					timeMotion = 0f;
					float num = 0f;
					if (allowMotion)
					{
						num = 1f - ctrlFlag.motions[0];
						num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] + Random.Range(ctrlFlag.changeMotionMinRate, num)) : 1f);
						if (num >= 1f)
						{
							allowMotion = false;
						}
					}
					else
					{
						num = ctrlFlag.motions[0];
						num = ((!(num <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] - Random.Range(ctrlFlag.changeMotionMinRate, num)) : 0f);
						if (num <= 0f)
						{
							allowMotion = true;
						}
					}
					lerpMotion = new Vector2(ctrlFlag.motions[0], num);
					lerpTime = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
				}
			}
			feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
			Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, _loop, resist);
			if (auto.ChangeLoopMotion(_loop == 1))
			{
				setPlay((_loop != 0) ? ((_state == 0) ? "WLoop" : "D_WLoop") : ((_state == 0) ? "SLoop" : "D_SLoop"));
				if (chaFemales[0].visibleAll && chaFemales[0].objTop != null && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice) && sixnine)
				{
					Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
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
					if (chaFemales[0].visibleAll && chaFemales[0].objTop != null && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
					{
						Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
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
			ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
			float num2 = 0f;
			if (_state == 1)
			{
				if (ctrlFlag.isGaugeHit)
				{
					num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
					num2 *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
					if (addFeel == 0f && ctrlFlag.feel_f >= 0.74f)
					{
						num2 = 0f;
					}
					ctrlFlag.feel_f += num2;
					ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
					if (addFeel == 0f && ctrlFlag.feel_f >= 0.74f)
					{
						ctrlFlag.feel_f = 0.74f;
					}
				}
			}
			else
			{
				num2 = Time.deltaTime * ctrlFlag.speedGuageRate;
				num2 *= (ctrlFlag.isGaugeHit ? 1f : 0.3f) * (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
				if (addFeel == 0f && ctrlFlag.feel_f >= 0.74f)
				{
					num2 = 0f;
				}
				ctrlFlag.feel_f += num2;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
				if (addFeel == 0f && ctrlFlag.feel_f >= 0.74f)
				{
					ctrlFlag.feel_f = 0.74f;
				}
			}
			float num3 = 0f;
			if (_state != 1 || ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 3 || _modeCtrl != 0)
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
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && !sixnine)
				{
					ctrlFlag.voice.playShorts[0] = 0;
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
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			if (ctrlFlag.feel_m <= 0.75f)
			{
				ctrlFlag.isGaugeHit = false;
				ctrlFlag.feel_m = 0.75f;
				ctrlFlag.isGaugeHit_M = false;
			}
			return true;
		}
		bool flag = !sixnine || _state != 1;
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, 2, resist);
		auto.ChangeSpeed(_loop: false, hitArea);
		auto.AddSpeed(_wheel, 2);
		ctrlFlag.speed = auto.GetSpeed(_loop: false);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		ctrlFlag.isGaugeHit = GlobalMethod.RangeOn(ctrlFlag.speed, hitArea.x, hitArea.y);
		ctrlFlag.isGaugeHit_M = ctrlFlag.isGaugeHit;
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
		float num2 = 0f;
		if (_state != 1 || ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 != 3 || _modeCtrl != 0)
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
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && !sixnine)
			{
				ctrlFlag.voice.playShorts[0] = 0;
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
			ctrlFlag.rateNip = 1f;
			bool sio = Manager.Config.HData.Sio;
			bool urine = Manager.Config.HData.Urine;
			if (sio)
			{
				particle.Play(0);
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
			}
			else
			{
				bool flag2 = false;
				switch (resist)
				{
				case 0:
					flag2 = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag2 = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag2 = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag2)
				{
					flag2 &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag2)
				{
					particle.Play(0);
				}
			}
			bool flag3 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag3 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag3 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag3 = false;
			}
			if (urine || flag3)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.numUrine++;
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
			}
		}
		else if (((ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishInSide && ctrlFlag.feel_m >= 0.75f) || (ctrlFlag.initiative == 2 && ctrlFlag.feel_m >= 1f)) && canInside)
		{
			setPlay((_state == 0) ? "OrgasmM_IN" : "D_OrgasmM_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 1;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishOutSide && ctrlFlag.feel_m >= 0.75f && flag)
		{
			setPlay((_state == 0) ? "OrgasmM_OUT" : "D_OrgasmM_OUT");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(0);
			ctrlFlag.feel_m = 0f;
			if (ctrlFlag.feel_f > 0.5f)
			{
				ctrlFlag.feel_f = 0.5f;
			}
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 2;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
		}
		else if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishSame && ctrlFlag.feel_m >= 0.75f && flag)
		{
			setPlay((_state == 0) ? "OrgasmS_IN" : "D_OrgasmS_IN");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(2);
			ctrlFlag.feel_m = 0f;
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isInsert = true;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.isGaugeHit_M = false;
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			ctrlFlag.numSameOrgasm = Mathf.Clamp(ctrlFlag.numSameOrgasm + 1, 0, 999999);
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			if (canInside)
			{
				ctrlFlag.numInside = Mathf.Clamp(ctrlFlag.numInside + 1, 0, 999999);
			}
			else
			{
				ctrlFlag.numOutSide = Mathf.Clamp(ctrlFlag.numOutSide + 1, 0, 999999);
			}
			ctrlFlag.voice.oldFinish = 3;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			bool sio2 = Manager.Config.HData.Sio;
			bool urine2 = Manager.Config.HData.Urine;
			if (sio2)
			{
				particle.Play(0);
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
			}
			else
			{
				bool flag4 = false;
				switch (resist)
				{
				case 0:
					flag4 = chaFemales[0].fileGameInfo2.resistH >= 100;
					break;
				case 1:
					flag4 = chaFemales[0].fileGameInfo2.resistAnal >= 100;
					break;
				case 2:
					flag4 = chaFemales[0].fileGameInfo2.resistPain >= 100;
					break;
				}
				if (flag4)
				{
					flag4 &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag4)
				{
					particle.Play(0);
				}
			}
			bool flag5 = false;
			if (eventNo == 5 || eventNo == 6 || eventNo == 30 || eventNo == 31)
			{
				flag5 = peepkind == 2 || peepkind == 3 || peepkind == 5;
			}
			else if (eventNo == 17 || eventNo == 18 || eventNo == 19)
			{
				flag5 = chaFemales[0].fileGameInfo2.Toilet >= 100;
			}
			if (ctrlFlag.numUrine > 0)
			{
				flag5 = false;
			}
			if (urine2 || flag5)
			{
				if (ProcBase.hSceneManager.UrineType == 1)
				{
					particle.Play(2);
				}
				else if (ProcBase.hSceneManager.UrineType == 0)
				{
					ctrlObi.PlayUrine(use: true);
				}
				ctrlFlag.numUrine++;
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
			}
		}
		return true;
	}

	private bool AutoAfterTheInsideWaitingProc(int _state, float _wheel, int _modeCtrl)
	{
		if (auto.IsPull(ctrlFlag.isInsert))
		{
			setPlay((_state == 0) ? "Pull" : "D_Pull");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.nowSpeedStateFast = false;
			ctrlFlag.motions[0] = 0f;
			timeMotion = 0f;
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
		timeMotion = 0f;
		oldHit = false;
		if (_state == 0)
		{
			timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
			timeChangeMotionDeltaTimes[0] = 0f;
		}
		voice.AfterFinish();
		auto.ReStartInit();
		auto.PullInit();
		return true;
	}
}
