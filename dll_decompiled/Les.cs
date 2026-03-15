using AIChara;
using Manager;
using UnityEngine;

public class Les : ProcBase
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

	private int nextPlay;

	private bool oldHit;

	private animParm animPar;

	private int resist;

	private int addFeel = 1;

	private int modeCtrl;

	public Les(DeliveryMember _delivery)
		: base(_delivery)
	{
		animPar.heights = new float[2];
		animPar.m = new float[2];
		CatID = 6;
	}

	public override bool Init(int _modeCtrl)
	{
		base.Init(_modeCtrl);
		modeCtrl = _modeCtrl;
		if (!ctrlFlag.isFaintness)
		{
			if (ctrlFlag.FaintnessType == modeCtrl && modeCtrl == 2)
			{
				ctrlFlag.isFaintness = true;
				ctrlFlag.isFaintnessVoice = true;
			}
		}
		else if (ctrlFlag.FaintnessType == 0 && modeCtrl != 0)
		{
			ctrlFlag.FaintnessType = 1;
		}
		return true;
	}

	public override bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (_isIdle)
		{
			if (_infoAnimList.nDownPtn != 0)
			{
				setPlay(ctrlFlag.isFaintness ? "D_Idle" : "Idle", _isFade: false);
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
		nextPlay = 0;
		oldHit = false;
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
		isHeight1Parameter = chaFemales[0].IsParameterInAnimator("height1");
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
			Manual(FemaleAi, _infoAnimList);
		}
		else
		{
			Auto(FemaleAi, _infoAnimList);
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
		bool active = ctrlFlag.initiative == 0 && (FemaleAi.IsName("WLoop") || FemaleAi.IsName("SLoop") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop"));
		sprite.categoryFinish.SetActive(active, 0);
		ctrlObi.PlayUrine(FemaleAi);
		ctrlObi.PlayUrine(FemaleAi, 1);
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.RecoverFaintness)
		{
			if (FemaleAi.IsName("D_Idle") || FemaleAi.IsName("D_WLoop") || FemaleAi.IsName("D_SLoop") || FemaleAi.IsName("D_OLoop") || FemaleAi.IsName("D_Orgasm_A"))
			{
				setPlay("Orgasm_A");
				ctrlFlag.isFaintness = false;
				ctrlFlag.FaintnessType = -1;
				ctrlFlag.isFaintnessVoice = false;
				sprite.SetVisibleLeaveItToYou(_visible: true, _judgeLeaveItToYou: true);
				ctrlFlag.numOrgasm = 0;
				ctrlFlag.numOrgasmSecond = 0;
				sprite.SetAnimationMenu();
				ctrlFlag.isGaugeHit = false;
				sprite.SetMotionListDraw(_active: false);
				ctrlFlag.nowOrgasm = false;
				ctrlObi.PlayUrine(use: false);
				if (voice.playAnimation != null)
				{
					voice.playAnimation.SetAllFlags(_play: true, Animator.StringToHash("Orgasm_A"));
				}
			}
			else
			{
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
			}
		}
		setAnimationParamater();
		return true;
	}

	private bool Manual(AnimatorStateInfo _ai, HScene.AnimationListInfo _infoAnimList)
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
			OrgasmProc(0, _ai.normalizedTime);
		}
		else if (_ai.IsName("Orgasm_A"))
		{
			StartProcTrigger(num);
			StartProc(_isReStart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			FaintnessStartProcTrigger(num, _start: true, _infoAnimList.nDownPtn != 0);
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
			OrgasmProc(1, _ai.normalizedTime);
		}
		else if (_ai.IsName("D_Orgasm_A"))
		{
			FaintnessStartProcTrigger(num, _start: false, _infoAnimList.nDownPtn != 0);
			FaintnessStartProc(_start: false, num);
		}
		return true;
	}

	private bool Auto(AnimatorStateInfo _ai, HScene.AnimationListInfo _infoAnimList)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		if (_ai.IsName("Idle"))
		{
			AutoStartProcTrigger(_start: false, num);
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
			OrgasmProc(0, _ai.normalizedTime);
		}
		else if (_ai.IsName("Orgasm_A"))
		{
			AutoStartProcTrigger(_start: true, num);
			AutoStartProc(_isReStart: true, num);
		}
		else if (_ai.IsName("D_Idle"))
		{
			FaintnessStartProcTrigger(num, _start: true, _infoAnimList.nDownPtn != 0);
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
			OrgasmProc(1, _ai.normalizedTime);
		}
		else if (_ai.IsName("D_Orgasm_A"))
		{
			FaintnessStartProcTrigger(num, _start: false, _infoAnimList.nDownPtn != 0);
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
		animPar.m[1] = ctrlFlag.motions[1];
		for (int i = 0; i < chaFemales.Length; i++)
		{
			if (chaFemales[1].visibleAll && !(chaFemales[i].objBodyBone == null))
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
				chaFemales[j].setAnimatorParamFloat("motion", animPar.m[j]);
				chaFemales[j].setAnimatorParamFloat("breast", animPar.breast);
				if (isHeight1Parameter)
				{
					chaFemales[j].setAnimatorParamFloat("height1", animPar.heights[j ^ 1]);
				}
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
		chaFemales[0].setPlay(_playAnimation, 0);
		rootmotionOffsetF[0].Set(_playAnimation);
		if (chaFemales[1].visibleAll && chaFemales[1].objTop != null)
		{
			chaFemales[1].setPlay(_playAnimation, 0);
			rootmotionOffsetF[1].Set(_playAnimation);
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
		if (nextPlay == 2)
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
		if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(1) || ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
		{
			int nShortBreahtPlay = ctrlFlag.nowAnimationInfo.nShortBreahtPlay;
			if (nShortBreahtPlay == 1 || nShortBreahtPlay == 3)
			{
				ctrlFlag.voice.playShorts[0] = 1;
			}
			if (nShortBreahtPlay == 1 || nShortBreahtPlay == 2)
			{
				ctrlFlag.voice.playShorts[1] = 1;
			}
		}
		return true;
	}

	private bool FaintnessStartProcTrigger(float _wheel, bool _start, bool canFaintNess)
	{
		if (_wheel == 0f || nextPlay != 0)
		{
			return false;
		}
		if (!_start && !canFaintNess)
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
					if (_wheel == 0f)
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
				num += Time.deltaTime * ctrlFlag.speedGuageRate;
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
				if (ctrlFlag.voice.playVoices[1] && ProcBase.hSceneManager.females[1] == null)
				{
					ctrlFlag.voice.dialog = true;
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
			if (ctrlFlag.voice.playVoices[1] && ProcBase.hSceneManager.females[1] == null)
			{
				ctrlFlag.voice.dialog = true;
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
			if (modeCtrl != 2)
			{
				ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			}
			if (modeCtrl != 1)
			{
				ctrlFlag.numOrgasmSecond = Mathf.Clamp(ctrlFlag.numOrgasmSecond + 1, 0, 10);
			}
			ctrlFlag.AddOrgasm();
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.voice.dialog = false;
			ctrlFlag.rateNip = 1f;
			if (Manager.Config.HData.Sio)
			{
				if (modeCtrl == 0 || modeCtrl == 1)
				{
					particle.Play(0);
				}
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
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
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
					{
						particle.Play(1);
					}
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
				if (modeCtrl == 0 || modeCtrl == 1)
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
				}
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
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
		}
		return true;
	}

	private bool GotoFaintness(int _state)
	{
		bool flag = !Manager.Config.HData.WeakStop;
		int num = ((modeCtrl != 2) ? ctrlFlag.numOrgasm : ctrlFlag.numOrgasmSecond);
		if (_state == 0 && num >= ctrlFlag.gotoFaintnessCount && flag)
		{
			setPlay("D_Orgasm_A");
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = modeCtrl;
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

	private bool OrgasmProc(int _state, float _normalizedTime)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		GotoFaintness(_state);
		ctrlObi.PlayUrine(use: false);
		return true;
	}

	private bool AutoStartProcTrigger(bool _start, float _wheel)
	{
		if (_wheel == 0f || nextPlay != 0)
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
		if (nextPlay == 2)
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
		if (ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(1) || ctrlFlag.nowAnimationInfo.hasVoiceCategory.Contains(2))
		{
			if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 3)
			{
				ctrlFlag.voice.playShorts[0] = 1;
			}
			if (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 2)
			{
				ctrlFlag.voice.playShorts[1] = 1;
			}
		}
		return true;
	}

	private bool AutoLoopProc(int _loop, int _state, float _wheel, HScene.AnimationListInfo _infoAnimList)
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
					if (chaFemales[j].visibleAll && !(chaFemales[j].objBodyBone == null) && (voice.nowVoices[j].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[j].state == HVoiceCtrl.VoiceKind.startVoice))
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
						if (chaFemales[k].visibleAll && !(chaFemales[k].objBodyBone == null) && (voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[k].state == HVoiceCtrl.VoiceKind.startVoice))
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
					ctrlFlag.voice.playShorts[0] = 0;
					ctrlFlag.voice.playShorts[1] = 0;
				}
				if (ctrlFlag.voice.playVoices[1] && ProcBase.hSceneManager.females[1] == null)
				{
					ctrlFlag.voice.dialog = true;
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
			if (ctrlFlag.voice.playVoices[1] && ProcBase.hSceneManager.females[1] == null)
			{
				ctrlFlag.voice.dialog = true;
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
			if (modeCtrl != 2)
			{
				ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			}
			if (modeCtrl != 1)
			{
				ctrlFlag.numOrgasmSecond = Mathf.Clamp(ctrlFlag.numOrgasmSecond + 1, 0, 10);
			}
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
				if (modeCtrl == 0 || modeCtrl == 1)
				{
					particle.Play(0);
				}
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
				{
					particle.Play(1);
				}
			}
			else if (ProcBase.hSceneManager.FemaleState[0] == ChaFileDefine.State.Dependence)
			{
				particle.Play(0);
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
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
					if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
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
				if (modeCtrl == 0 || modeCtrl == 1)
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
				}
				if (chaFemales[1].visibleAll && (bool)chaFemales[1] && (bool)chaFemales[1].objBodyBone && (modeCtrl == 0 || modeCtrl == 2))
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
		}
		return true;
	}
}
