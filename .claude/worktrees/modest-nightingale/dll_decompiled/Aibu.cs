using AIChara;
using Manager;
using UnityEngine;

public class Aibu : ProcBase
{
	private float timeMotion;

	private bool enableMotion;

	private bool allowMotion = true;

	private Vector2 lerpMotion = Vector2.zero;

	private float lerpTime;

	private int nextPlay;

	private bool oldHit;

	private int resist;

	private int addFeel = 1;

	private animParm animPar;

	public Aibu(DeliveryMember _delivery)
		: base(_delivery)
	{
		animPar.heights = new float[1];
		animPar.m = new float[1];
		CatID = 0;
	}

	public override bool Init(int _modeCtrl)
	{
		base.Init(_modeCtrl);
		return true;
	}

	public override bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		setAnimationParamater();
		if (_isIdle)
		{
			setPlay(ctrlFlag.isFaintness ? "D_Idle" : "Idle", _isFade: false);
			ctrlFlag.loopType = -1;
			voice.HouchiTime = 0f;
		}
		else
		{
			if (ctrlFlag.feel_f >= 0.75f)
			{
				setPlay(ctrlFlag.isFaintness ? "D_OLoop" : "OLoop", _isFade: false);
				ctrlFlag.loopType = 2;
			}
			else
			{
				setPlay(ctrlFlag.isFaintness ? "D_WLoop" : "WLoop", _isFade: false);
				ctrlFlag.loopType = 0;
			}
			ctrlFlag.speed = 0f;
			ctrlFlag.motions[0] = 0f;
			ctrlFlag.nowSpeedStateFast = false;
			auto.SetSpeed(0f);
		}
		addFeel = 1;
		if (chaFemales[0].fileParam2.hAttribute != 3 && _infoAnimList.lstSystem.Contains(4) && chaFemales[0].fileGameInfo2.resistPain < 100)
		{
			addFeel = 0;
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
		bool active = ctrlFlag.initiative == 0 && (FemaleAi.IsName("WLoop") || FemaleAi.IsName("SLoop") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop"));
		if (resist == 2 && addFeel == 0)
		{
			active = false;
		}
		sprite.categoryFinish.SetActive(active, 0);
		ctrlObi.PlayUrine(FemaleAi);
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.RecoverFaintness)
		{
			bool flag = false;
			if ((eventNo == 19) ? ctrlFlag.isFaintnessVoice : (FemaleAi.IsName("D_Idle") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop") || FemaleAi.IsName("D_OLoop") || FemaleAi.IsName("D_Orgasm_A")))
			{
				if (eventNo != 19)
				{
					setPlay("Orgasm_A");
					ctrlFlag.isFaintness = false;
				}
				else
				{
					setPlay("D_Orgasm_A");
					if (voice.playAnimation != null)
					{
						voice.playAnimation.SetAllFlags(_play: true, Animator.StringToHash("D_Orgasm_A"));
					}
				}
				ctrlFlag.FaintnessType = -1;
				ctrlFlag.isFaintnessVoice = false;
				sprite.SetVisibleLeaveItToYou(_visible: true, _judgeLeaveItToYou: true);
				ctrlFlag.numOrgasm = 0;
				sprite.SetAnimationMenu();
				ctrlFlag.isGaugeHit = false;
				sprite.SetMotionListDraw(_active: false);
				ctrlFlag.nowOrgasm = false;
				ctrlObi.PlayUrine(use: false);
			}
			else
			{
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
			}
		}
		if (eventNo >= 50 && eventNo < 56)
		{
			addFeel = 1;
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
			StartProc(_isReStart: false, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("WLoop"))
		{
			LoopProc(0, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			LoopProc(1, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			OLoopProc(0, num, _infoAnimList);
		}
		else if (_ai.IsName("Orgasm"))
		{
			OrgasmProc(0, _ai.normalizedTime, _modeCtrl);
		}
		else if (_ai.IsName("Orgasm_A"))
		{
			StartProcTrigger(num);
			StartProc(_isReStart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			FaintnessStartProcTrigger(num, _start: true, _modeCtrl);
			FaintnessStartProc(_start: true, num);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopProc(0, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopProc(1, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopProc(1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_Orgasm"))
		{
			OrgasmProc(1, _ai.normalizedTime, _modeCtrl);
		}
		else if (_ai.IsName("D_Orgasm_A"))
		{
			FaintnessStartProcTrigger(num, _start: false, _modeCtrl);
			FaintnessStartProc(_start: false, num);
		}
		return true;
	}

	private bool Auto(AnimatorStateInfo _ai, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			AutoStartProcTrigger(_start: false);
			AutoStartProc(_isReStart: false, num);
		}
		else if (_ai.IsName("WLoop"))
		{
			AutoLoopProc(0, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("SLoop"))
		{
			AutoLoopProc(1, 0, num, _infoAnimList);
		}
		else if (_ai.IsName("OLoop"))
		{
			AutoOLoopProc(0, num, _infoAnimList);
		}
		else if (_ai.IsName("Orgasm"))
		{
			OrgasmProc(0, _ai.normalizedTime, _modeCtrl);
		}
		else if (_ai.IsName("Orgasm_A"))
		{
			AutoStartProcTrigger(_start: true);
			AutoStartProc(_isReStart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			FaintnessStartProcTrigger(num, _start: true, _modeCtrl);
			FaintnessStartProc(_start: true, num);
		}
		else if (_ai.IsName("D_WLoop"))
		{
			LoopProc(0, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_SLoop"))
		{
			LoopProc(1, 1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_OLoop"))
		{
			OLoopProc(1, num, _infoAnimList);
		}
		else if (_ai.IsName("D_Orgasm"))
		{
			OrgasmProc(1, _ai.normalizedTime, _modeCtrl);
		}
		else if (_ai.IsName("D_Orgasm_A"))
		{
			FaintnessStartProcTrigger(num, _start: false, _modeCtrl);
			FaintnessStartProc(_start: false, num);
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
		if (chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null)
		{
			animPar.heights[0] = chaFemales[0].GetShapeBodyValue(0);
			chaFemales[0].setAnimatorParamFloat("height", animPar.heights[0]);
			chaFemales[0].setAnimatorParamFloat("speed", animPar.speed);
			chaFemales[0].setAnimatorParamFloat("motion", animPar.m[0]);
			chaFemales[0].setAnimatorParamFloat("breast", animPar.breast);
		}
		if (chaMales[0].objBodyBone != null)
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

	private void setPlay(string _playAnimation, bool _isFade = true)
	{
		chaFemales[0].setPlay(_playAnimation, 0);
		rootmotionOffsetF[0].Set(_playAnimation);
		if (chaMales[0].objTop != null)
		{
			chaMales[0].setPlay(_playAnimation, 0);
			rootmotionOffsetM[0].Set(_playAnimation);
		}
		for (int i = 0; i < lstMotionIK.Count; i++)
		{
			lstMotionIK[i].Item3.Calc(_playAnimation);
		}
		if (item != null)
		{
			item.setPlay(_playAnimation);
		}
		if (_isFade)
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

	private bool StartProc(bool _isReStart, float _wheel)
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
		if (nextPlay == 2 && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
		{
			if (_wheel == 0f)
			{
				return false;
			}
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		nextPlay = 0;
		setPlay("WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.nowSpeedStateFast = false;
		timeMotion = 0f;
		timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
		timeChangeMotionDeltaTimes[0] = 0f;
		ctrlFlag.isNotCtrl = false;
		oldHit = false;
		feelHit.InitTime();
		if (_isReStart)
		{
			voice.AfterFinish();
		}
		return true;
	}

	private bool FaintnessStartProcTrigger(float _wheel, bool _start, int _modeCtrl)
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

	private bool FaintnessStartProc(bool _start, float _wheel)
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
		if (nextPlay == 2 && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
		{
			if (_wheel == 0f)
			{
				return false;
			}
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		nextPlay = 0;
		setPlay("D_WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.motions[0] = 0f;
		ctrlFlag.nowSpeedStateFast = false;
		ctrlFlag.isNotCtrl = false;
		oldHit = false;
		feelHit.InitTime();
		voice.AfterFinish();
		return true;
	}

	private bool LoopProc(int _loop, int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
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
			if (_state == 0 && chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null)
			{
				timeChangeMotionDeltaTimes[0] += Time.deltaTime;
				if (timeChangeMotions[0] <= timeChangeMotionDeltaTimes[0] && !enableMotion)
				{
					timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
					timeChangeMotionDeltaTimes[0] = 0f;
					enableMotion = true;
					timeMotion = 0f;
					float num2 = 0f;
					if (allowMotion)
					{
						num2 = 1f - ctrlFlag.motions[0];
						num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] + Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 1f);
						if (num2 >= 1f)
						{
							allowMotion = false;
						}
					}
					else
					{
						num2 = ctrlFlag.motions[0];
						num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] - Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 0f);
						if (num2 <= 0f)
						{
							allowMotion = true;
						}
					}
					lerpMotion = new Vector2(ctrlFlag.motions[0], num2);
					lerpTime = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
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
				if (resist == 2)
				{
					ctrlFlag.feelPain += num;
				}
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
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3))
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
			}
		}
		return true;
	}

	private bool OLoopProc(int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
		ctrlFlag.speed = Mathf.Clamp01(ctrlFlag.speed + _wheel);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, 2, ctrlFlag.speed, resist);
		if (ctrlFlag.isGaugeHit)
		{
			num += Time.deltaTime * ctrlFlag.speedGuageRate;
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
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && (_infoAnimList.nShortBreahtPlay == 1 || _infoAnimList.nShortBreahtPlay == 3))
			{
				ctrlFlag.voice.playShorts[0] = 0;
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
			ctrlFlag.nowOrgasm = true;
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.voice.dialog = false;
			ctrlFlag.rateNip = 1f;
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
				}
			}
			bool urine = Manager.Config.HData.Urine;
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
				ctrlFlag.numUrine++;
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
			}
			sprite.objMotionListPanel.SetActive(value: false);
		}
		return true;
	}

	private bool GotoFaintness(int _state)
	{
		bool flag = !Manager.Config.HData.WeakStop;
		if (((_state == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount) & (eventNo < 50 || eventNo > 55)) && flag)
		{
			setPlay("D_Orgasm_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isFaintness = true;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.FaintnessType = 1;
			ctrlFlag.numFaintness = Mathf.Clamp(ctrlFlag.numFaintness + 1, 0, 999999);
			sprite.SetVisibleLeaveItToYou(_visible: false);
		}
		else if (eventNo == 19 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
		{
			setPlay("D_Orgasm_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isFaintness = true;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.FaintnessType = 1;
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

	private bool OrgasmProc(int _state, float _normalizedTime, int _modeCtrl)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		if ((ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 0 && _modeCtrl != 1) || ctrlFlag.nowAnimationInfo.ActionCtrl.Item1 == 3)
		{
			GotoFaintness(_state);
		}
		else
		{
			setPlay((_state == 0) ? "Orgasm_A" : "D_Orgasm_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
		}
		return true;
	}

	private bool AutoStartProcTrigger(bool _start)
	{
		if (nextPlay != 0)
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

	private bool AutoStartProc(bool _isReStart, float _wheel)
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
		if (nextPlay == 2 && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
		{
			if (_wheel == 0f)
			{
				return false;
			}
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
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
		ctrlFlag.nowSpeedStateFast = false;
		oldHit = false;
		timeMotion = 0f;
		timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
		timeChangeMotionDeltaTimes[0] = 0f;
		ctrlFlag.isNotCtrl = false;
		auto.Reset();
		feelHit.InitTime();
		if (_isReStart)
		{
			voice.AfterFinish();
		}
		return true;
	}

	private bool AutoLoopProc(int _loop, int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
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
			if (_state == 0 && chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null)
			{
				timeChangeMotionDeltaTimes[0] += Time.deltaTime;
				if (timeChangeMotions[0] <= timeChangeMotionDeltaTimes[0] && !enableMotion)
				{
					timeChangeMotions[0] = Random.Range(ctrlFlag.changeAutoMotionTimeMin, ctrlFlag.changeAutoMotionTimeMax);
					timeChangeMotionDeltaTimes[0] = 0f;
					enableMotion = true;
					timeMotion = 0f;
					float num2 = 0f;
					if (allowMotion)
					{
						num2 = 1f - ctrlFlag.motions[0];
						num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] + Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 1f);
						if (num2 >= 1f)
						{
							allowMotion = false;
						}
					}
					else
					{
						num2 = ctrlFlag.motions[0];
						num2 = ((!(num2 <= ctrlFlag.changeMotionMinRate)) ? (ctrlFlag.motions[0] - Random.Range(ctrlFlag.changeMotionMinRate, num2)) : 0f);
						if (num2 <= 0f)
						{
							allowMotion = true;
						}
					}
					lerpMotion = new Vector2(ctrlFlag.motions[0], num2);
					lerpTime = Random.Range(ctrlFlag.changeMotionTimeMin, ctrlFlag.changeMotionTimeMax);
				}
			}
			feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
			Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, _loop, resist);
			if (auto.ChangeLoopMotion(_loop == 1))
			{
				setPlay((_loop != 0) ? ((_state == 0) ? "WLoop" : "D_WLoop") : ((_state == 0) ? "SLoop" : "D_SLoop"));
				if (chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
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
					if (chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null && (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice))
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
			if (_state == 1)
			{
				if (ctrlFlag.isGaugeHit)
				{
					num = Time.deltaTime * ctrlFlag.speedGuageRate;
					num *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
					if (addFeel == 0 && ctrlFlag.feel_f >= 0.74f)
					{
						num = 0f;
					}
					ctrlFlag.feel_f += num;
					if (resist == 2)
					{
						ctrlFlag.feelPain += num;
					}
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
				if (resist == 2)
				{
					ctrlFlag.feelPain += num;
				}
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
				if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0))
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
				auto.SetSpeed(0f);
				feelHit.InitTime();
			}
		}
		return true;
	}

	private bool AutoOLoopProc(int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
		feelHit.ChangeHit(_infoAnimList.nFeelHit, 2, resist);
		Vector2 hitArea = feelHit.GetHitArea(_infoAnimList.nFeelHit, 2, resist);
		auto.ChangeSpeed(_loop: false, hitArea);
		auto.AddSpeed(_wheel, 2);
		ctrlFlag.speed = auto.GetSpeed(_loop: false);
		ctrlFlag.nowSpeedStateFast = ctrlFlag.speed >= 0.5f;
		ctrlFlag.isGaugeHit = GlobalMethod.RangeOn(ctrlFlag.speed, hitArea.x, hitArea.y);
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
			if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0))
			{
				ctrlFlag.voice.playShorts[0] = 0;
			}
		}
		oldHit = ctrlFlag.isGaugeHit;
		if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
		{
			setPlay((_state == 0) ? "Orgasm" : "D_Orgasm");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.AddTaiiParam();
			ctrlFlag.AddFinishResistTaii(1);
			ctrlFlag.feel_f = 0f;
			ctrlFlag.isGaugeHit = false;
			ctrlFlag.voice.oldFinish = 0;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
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
					flag |= chaFemales[0].fileParam2.hAttribute == 3;
					break;
				}
				if (flag)
				{
					flag &= chaFemales[0].fileGameInfo2.Libido >= 80;
				}
				if (ctrlFlag.numFaintness == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
				{
					particle.Play(0);
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
				ctrlFlag.numUrine++;
				ctrlFlag.voice.urines[0] = true;
				ctrlFlag.voice.urineFlag = true;
			}
			sprite.objMotionListPanel.SetActive(value: false);
		}
		return true;
	}
}
