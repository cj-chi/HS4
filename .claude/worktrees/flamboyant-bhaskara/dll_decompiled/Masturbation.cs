using AIChara;
using Manager;
using UnityEngine;

public class Masturbation : ProcBase
{
	public class MasturbationTimeInfo
	{
		public float[] Start = new float[2];

		public float[] Restart = new float[2];
	}

	private string nextAnimation;

	private RandomTimer[] aTimer = new RandomTimer[2];

	private int nextPlay;

	private bool oldHit;

	private animParm animPar;

	private int resist;

	private bool bAuto = true;

	public Masturbation(DeliveryMember _delivery)
		: base(_delivery)
	{
		animPar.heights = new float[1];
		aTimer[0] = new RandomTimer();
		aTimer[1] = new RandomTimer();
		MasturbationTimeInfo mBinfo = HSceneManager.HResourceTables.MBinfo;
		aTimer[0].Init(mBinfo.Start[0], mBinfo.Start[1]);
		aTimer[1].Init(mBinfo.Restart[0], mBinfo.Restart[1]);
		CatID = 4;
	}

	public override bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		TimerReset();
		bAuto = _modeCtrl != 4;
		if (_isIdle)
		{
			setPlay("Idle", _isFade: false);
			ctrlFlag.loopType = -1;
			ctrlFlag.speed = 1f;
			voice.HouchiTime = 0f;
		}
		else
		{
			if (ctrlFlag.feel_f >= 0.75f)
			{
				setPlay("OLoop", _isFade: false);
				ctrlFlag.loopType = 2;
			}
			else if (ctrlFlag.feel_f >= 0.5f)
			{
				setPlay("SLoop", _isFade: false);
				ctrlFlag.loopType = 1;
			}
			else if (ctrlFlag.feel_f >= 0.25f)
			{
				setPlay("MLoop", _isFade: false);
				ctrlFlag.loopType = 3;
			}
			else
			{
				setPlay("WLoop", _isFade: false);
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
		ctrlFlag.voice.changeTaii = true;
		return true;
	}

	public override bool Proc(int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (chaFemales[0].objTop == null)
		{
			return false;
		}
		AnimatorStateInfo animatorStateInfo = chaFemales[0].getAnimatorStateInfo(0);
		bool animatorParamBool = chaFemales[0].getAnimatorParamBool("change");
		if (!bAuto)
		{
			Manual(animatorStateInfo, animatorParamBool);
		}
		else
		{
			Auto(animatorStateInfo, animatorParamBool);
		}
		if (!bAuto)
		{
			bool flag = false;
			flag = animatorStateInfo.IsName("WLoop") || animatorStateInfo.IsName("MLoop") || animatorStateInfo.IsName("SLoop");
			sprite.categoryFinish.SetActive(flag, 0);
		}
		ctrlObi.PlayUrine(animatorStateInfo);
		setAnimationParamater();
		return true;
	}

	private void Manual(AnimatorStateInfo ai, bool isChangeTrigger)
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0);
		num = ((num < 0f) ? (0f - ctrlFlag.wheelActionCount) : ((num > 0f) ? ctrlFlag.wheelActionCount : 0f));
		bool flag = false;
		if ((ai.IsName("WLoop") || ai.IsName("MLoop") || ai.IsName("SLoop")) && ctrlFlag.click == HSceneFlagCtrl.ClickKind.FinishBefore)
		{
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = 2;
			nextAnimation = "OLoop";
			chaFemales[0].setAnimatorParamBool("change", _bFlag: true);
			item.setAnimatorParamBool("change", _bFlag: true);
			setPlay("OLoop");
			ctrlFlag.feel_f = 0.75f;
			flag = true;
		}
		if (!flag)
		{
			ctrlFlag.speed += num;
			ctrlFlag.speed = Mathf.Clamp(ctrlFlag.speed, 0f, 1f);
			ctrlFlag.nowSpeedStateFast = ctrlFlag.speed > 0.5f;
			if (ai.IsName("Idle"))
			{
				StartProcTrriger(num);
				StartProc(0, num);
			}
			else if (ai.IsName("WLoop"))
			{
				GotoNextLoop(0.25f, isChangeTrigger, "MLoop", ctrlFlag.nowAnimationInfo, 0, 3);
			}
			else if (ai.IsName("MLoop"))
			{
				GotoNextLoop(0.5f, isChangeTrigger, "SLoop", ctrlFlag.nowAnimationInfo, 1, 1);
			}
			else if (ai.IsName("SLoop"))
			{
				GotoNextLoop(0.75f, isChangeTrigger, "OLoop", ctrlFlag.nowAnimationInfo, 2, 2);
			}
			else if (ai.IsName("OLoop"))
			{
				ctrlFlag.isGaugeHit = feelHit.isHit(ctrlFlag.nowAnimationInfo.nFeelHit, 3, ctrlFlag.speed, resist);
				if (ctrlFlag.isGaugeHit)
				{
					float num2 = 0f;
					num2 = ctrlFlag.speedGuageRate * Time.deltaTime;
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
					if (!ctrlFlag.nowAnimationInfo.lstSystem.Contains(0) && (ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 1 || ctrlFlag.nowAnimationInfo.nShortBreahtPlay == 3))
					{
						ctrlFlag.voice.playShorts[0] = 0;
					}
					ctrlFlag.voice.dialog = false;
				}
				oldHit = ctrlFlag.isGaugeHit;
				if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
				{
					setPlay("Orgasm");
					ctrlFlag.speed = 1f;
					ctrlFlag.loopType = -1;
					ctrlFlag.AddTaiiParam();
					ctrlFlag.AddFinishResistTaii(1);
					ctrlFlag.feel_f = 0f;
					ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
					ctrlFlag.AddOrgasm();
					ctrlFlag.voice.oldFinish = 0;
					voice.SetFinish(ctrlFlag.voice.oldFinish);
					sprite.objMotionListPanel.SetActive(value: false);
					ctrlFlag.nowOrgasm = true;
					ctrlFlag.isGaugeHit = false;
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
					bool urine = Manager.Config.HData.Urine;
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
			}
			else if (ai.IsName("Orgasm"))
			{
				bool flag4 = (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice) && Voice.IsPlay(ctrlFlag.voice.voiceTrs[0]);
				if (ai.normalizedTime >= 1f && !flag4)
				{
					setPlay("Orgasm_A");
					ctrlFlag.speed = 1f;
					ctrlFlag.loopType = -1;
					ctrlFlag.nowOrgasm = false;
					ctrlObi.PlayUrine(use: false);
				}
			}
			else if (ai.IsName("Orgasm_A"))
			{
				StartProcTrriger(num);
				StartProc(1, num);
			}
		}
		if (isChangeTrigger && ai.IsName(nextAnimation))
		{
			chaFemales[0].setAnimatorParamBool("change", _bFlag: false);
		}
	}

	private void Auto(AnimatorStateInfo ai, bool isChangeTrigger)
	{
		if (ai.IsName("Idle"))
		{
			AutoStartProc(0);
		}
		else if (ai.IsName("WLoop"))
		{
			ctrlFlag.speed = ctrlFlag.feel_f;
			GotoNextLoop(0.25f, isChangeTrigger, "MLoop", 3);
		}
		else if (ai.IsName("MLoop"))
		{
			ctrlFlag.speed = ctrlFlag.feel_f;
			GotoNextLoop(0.5f, isChangeTrigger, "SLoop", 1);
		}
		else if (ai.IsName("SLoop"))
		{
			ctrlFlag.speed = ctrlFlag.feel_f;
			GotoNextLoop(0.75f, isChangeTrigger, "OLoop", 2);
		}
		else if (ai.IsName("OLoop"))
		{
			ctrlFlag.speed = ctrlFlag.feel_f;
			ctrlFlag.feel_f += ctrlFlag.speedGuageRate * Time.deltaTime;
			ctrlFlag.feel_f *= ((!ctrlFlag.stopFeelFemale) ? 1 : 0);
			ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
			if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
			{
				setPlay("Orgasm");
				ctrlFlag.speed = 1f;
				ctrlFlag.loopType = -1;
				ctrlFlag.AddTaiiParam();
				ctrlFlag.AddFinishResistTaii(1);
				ctrlFlag.feel_f = 0f;
				ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
				ctrlFlag.AddOrgasm();
				ctrlFlag.voice.oldFinish = 0;
				voice.SetFinish(ctrlFlag.voice.oldFinish);
				sprite.objMotionListPanel.SetActive(value: false);
				ctrlFlag.nowOrgasm = true;
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
			}
		}
		else if (ai.IsName("Orgasm"))
		{
			if (ai.normalizedTime >= 1f)
			{
				setPlay("Orgasm_A");
				ctrlFlag.speed = 1f;
				ctrlFlag.loopType = -1;
				ctrlFlag.nowOrgasm = false;
				ctrlObi.PlayUrine(use: false);
			}
		}
		else if (ai.IsName("Orgasm_A"))
		{
			AutoStartProc(1);
		}
		if (isChangeTrigger && ai.IsName(nextAnimation))
		{
			chaFemales[0].setAnimatorParamBool("change", _bFlag: false);
		}
	}

	public override void setAnimationParamater()
	{
		HSceneFlagCtrl.MasterbationLoopSpeeds masterbationSpeeds = ctrlFlag.masterbationSpeeds;
		float num = 0f;
		num = ctrlFlag.loopType switch
		{
			0 => Mathf.Lerp(masterbationSpeeds.minLoopSpeedW, masterbationSpeeds.maxLoopSpeedW, ctrlFlag.speed), 
			1 => Mathf.Lerp(masterbationSpeeds.minLoopSpeedW, masterbationSpeeds.maxLoopSpeedW, ctrlFlag.speed), 
			2 => Mathf.Lerp(masterbationSpeeds.minLoopSpeedO, masterbationSpeeds.maxLoopSpeedO, ctrlFlag.speed), 
			3 => Mathf.Lerp(masterbationSpeeds.minLoopSpeedM, masterbationSpeeds.maxLoopSpeedM, ctrlFlag.speed), 
			_ => ctrlFlag.speed, 
		};
		animPar.speed = num;
		if (chaFemales[0].visibleAll && chaFemales[0].objTop != null)
		{
			animPar.heights[0] = chaFemales[0].GetShapeBodyValue(0);
			animPar.breast = chaFemales[0].GetShapeBodyValue(1);
			chaFemales[0].setAnimatorParamFloat("height", animPar.heights[0]);
			chaFemales[0].setAnimatorParamFloat("speed", animPar.speed);
			chaFemales[0].setAnimatorParamFloat("breast", animPar.breast);
		}
		if (chaMales[0].objBodyBone != null && chaMales[0].animBody.runtimeAnimatorController != null)
		{
			chaMales[0].setAnimatorParamFloat("height", animPar.heights[0]);
			chaMales[0].setAnimatorParamFloat("speed", animPar.speed);
			chaMales[0].setAnimatorParamFloat("breast", animPar.breast);
		}
		if (item.GetItem() != null)
		{
			item.setAnimatorParamFloat("height", animPar.heights[0]);
			item.setAnimatorParamFloat("speed", animPar.speed);
		}
	}

	private void setPlay(string _playAnimation, bool _isFade = true)
	{
		chaFemales[0].setPlay(_playAnimation, 0);
		rootmotionOffsetF[0].Set(_playAnimation);
		if (!ctrlFlag.nowAnimationInfo.fileMale.IsNullOrEmpty() && chaMales[0].objBodyBone != null && chaMales[0].animBody.runtimeAnimatorController != null)
		{
			chaMales[0].setPlay(_playAnimation, 0);
		}
		if (item != null)
		{
			item.setPlay(_playAnimation);
		}
		for (int i = 0; i < lstMotionIK.Count; i++)
		{
			lstMotionIK[i].Item3.Calc(_playAnimation);
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

	private bool AutoStartProc(int _start)
	{
		if (!aTimer[_start].Check())
		{
			return false;
		}
		setPlay("WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.isNotCtrl = false;
		chaFemales[0].setAnimatorParamBool("change", _bFlag: false);
		item.setAnimatorParamBool("change", _bFlag: false);
		if (_start == 1)
		{
			voice.AfterFinish();
		}
		if (ctrlFlag.voice.onaniEnterLoop == 0)
		{
			ctrlFlag.voice.onaniEnterLoop = 1;
		}
		return true;
	}

	private bool StartProcTrriger(float _wheel)
	{
		if (_wheel == 0f)
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

	private bool StartProc(int _start, float _wheel)
	{
		if (nextPlay == 0)
		{
			return false;
		}
		if (nextPlay == 1)
		{
			if (_start == 0)
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
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		nextPlay = 0;
		setPlay("WLoop");
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = 0;
		ctrlFlag.isNotCtrl = false;
		chaFemales[0].setAnimatorParamBool("change", _bFlag: false);
		item.setAnimatorParamBool("change", _bFlag: false);
		if (_start == 1)
		{
			voice.AfterFinish();
		}
		return true;
	}

	private bool GotoNextLoop(float _range, bool _isChangeTrigger, string _nextAnimation, int _nextLoopType)
	{
		if (voice.nowVoices[0].state != HVoiceCtrl.VoiceKind.voice || ctrlFlag.voice.onaniEnterLoop != 1)
		{
			float num = 0f;
			num = ctrlFlag.speedGuageRate * Time.deltaTime;
			num *= (float)((!ctrlFlag.stopFeelFemale) ? 1 : 0);
			ctrlFlag.feel_f += num;
			ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
		}
		if (ctrlFlag.feel_f < _range || _isChangeTrigger)
		{
			return false;
		}
		ctrlFlag.loopType = _nextLoopType;
		nextAnimation = _nextAnimation;
		chaFemales[0].setAnimatorParamBool("change", _bFlag: true);
		item.setAnimatorParamBool("change", _bFlag: true);
		setPlay(_nextAnimation);
		return true;
	}

	private bool GotoNextLoop(float _range, bool _isChangeTrigger, string _nextAnimation, HScene.AnimationListInfo _infoAnimList, int _loop, int _nextLoopType)
	{
		ctrlFlag.isGaugeHit = feelHit.isHit(_infoAnimList.nFeelHit, _loop, ctrlFlag.speed, resist);
		if (ctrlFlag.isGaugeHit)
		{
			feelHit.ChangeHit(_infoAnimList.nFeelHit, _loop, resist);
			float num = 0f;
			num = ctrlFlag.speedGuageRate * Time.deltaTime;
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
		if (ctrlFlag.feel_f < _range || _isChangeTrigger)
		{
			return false;
		}
		ctrlFlag.speed = 0f;
		ctrlFlag.loopType = _nextLoopType;
		nextAnimation = _nextAnimation;
		chaFemales[0].setAnimatorParamBool("change", _bFlag: true);
		item.setAnimatorParamBool("change", _bFlag: true);
		setPlay(_nextAnimation);
		oldHit = false;
		return true;
	}

	private void TimerReset()
	{
		aTimer[0] = new RandomTimer();
		aTimer[1] = new RandomTimer();
		MasturbationTimeInfo mBinfo = HSceneManager.HResourceTables.MBinfo;
		aTimer[0].Init(mBinfo.Start[0], mBinfo.Start[1]);
		aTimer[1].Init(mBinfo.Restart[0], mBinfo.Restart[1]);
	}
}
