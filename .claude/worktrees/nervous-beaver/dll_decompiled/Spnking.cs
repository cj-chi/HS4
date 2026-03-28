using AIChara;
using Manager;
using UnityEngine;

public class Spnking : ProcBase
{
	private bool upFeel;

	private float backupFeel;

	private float backupFeelFlag;

	private float timeFeelUp;

	private animParm animPar;

	private bool isAddFeel = true;

	public Spnking(DeliveryMember _delivery)
		: base(_delivery)
	{
		animPar.heights = new float[1];
		CatID = 3;
	}

	public override bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		if (_isIdle)
		{
			setPlay(ctrlFlag.isFaintness ? "D_Orgasm_A" : "WIdle", _isFade: false);
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			voice.HouchiTime = 0f;
		}
		else
		{
			if (ctrlFlag.isFaintness)
			{
				setPlay("D_Orgasm_A", _isFade: false);
			}
			else
			{
				setPlay((ctrlFlag.feel_f >= 0.5f) ? "SIdle" : "WIdle", _isFade: false);
			}
			ctrlFlag.speed = 0f;
			ctrlFlag.loopType = -1;
			voice.HouchiTime = 0f;
			ctrlFlag.motions[0] = 0f;
			ctrlFlag.motions[1] = 0f;
		}
		isAddFeel = !_infoAnimList.lstSystem.Contains(4) || chaFemales[0].fileGameInfo2.resistPain >= 100 || chaFemales[0].fileParam2.hAttribute == 3;
		ctrlFlag.voice.changeTaii = true;
		return true;
	}

	public override bool Proc(int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		float num = 0f;
		if (chaMales[0].objTop == null || chaFemales[0].objTop == null)
		{
			return false;
		}
		FemaleAi = chaFemales[0].getAnimatorStateInfo(0);
		if (FemaleAi.IsName("WIdle"))
		{
			SpankingProc(0);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (FemaleAi.IsName("WAction"))
		{
			ActionProc(FemaleAi.normalizedTime, 0, _infoAnimList);
		}
		if (FemaleAi.IsName("SIdle"))
		{
			SpankingProc(1);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		else if (FemaleAi.IsName("SAction"))
		{
			ActionProc(FemaleAi.normalizedTime, 1, _infoAnimList);
		}
		else if (FemaleAi.IsName("Orgasm"))
		{
			AfterWaitingAnimation(FemaleAi.normalizedTime, 0);
		}
		else if (FemaleAi.IsName("D_Action"))
		{
			ActionProc(FemaleAi.normalizedTime, 2, _infoAnimList);
		}
		else if (FemaleAi.IsName("D_Orgasm"))
		{
			AfterWaitingAnimation(FemaleAi.normalizedTime, 1);
		}
		else if (FemaleAi.IsName("D_Orgasm_A"))
		{
			SpankingProc(2);
			voice.HouchiTime += Time.unscaledDeltaTime;
		}
		ctrlObi.PlayUrine(FemaleAi);
		if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.RecoverFaintness)
		{
			bool flag = false;
			if ((eventNo == 19) ? (FemaleAi.IsName("D_Orgasm_A") && ctrlFlag.isFaintnessVoice) : FemaleAi.IsName("D_Orgasm_A"))
			{
				if (eventNo != 19)
				{
					setPlay("WIdle");
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
				voice.HouchiTime = 0f;
				sprite.SetVisibleLeaveItToYou(_visible: true, _judgeLeaveItToYou: true);
				ctrlFlag.numOrgasm = 0;
				sprite.SetAnimationMenu();
				sprite.SetMotionListDraw(_active: false);
			}
			else
			{
				ctrlFlag.click = HSceneFlagCtrl.ClickKind.None;
			}
		}
		if (upFeel)
		{
			timeFeelUp = Mathf.Clamp(timeFeelUp + Time.deltaTime, 0f, 0.3f);
			float num2 = Mathf.Clamp01(timeFeelUp / 0.3f);
			if (!ctrlFlag.stopFeelFemale)
			{
				num = Mathf.Lerp(0f, ctrlFlag.feelSpnking, num2);
				ctrlFlag.feel_f = backupFeel + num;
				ctrlFlag.feelPain = backupFeelFlag + num;
				ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f);
				if (!isAddFeel && ctrlFlag.feel_f >= 0.74f)
				{
					ctrlFlag.feel_f = 0.74f;
				}
			}
			if (num2 >= 1f)
			{
				upFeel = false;
			}
		}
		setAnimationParamater();
		return true;
	}

	public override void setAnimationParamater()
	{
		if (chaFemales[0].visibleAll && chaFemales[0].objTop != null)
		{
			animPar.heights[0] = chaFemales[0].GetShapeBodyValue(0);
			chaFemales[0].setAnimatorParamFloat("height", animPar.heights[0]);
		}
		if (chaMales[0].objTop != null)
		{
			chaMales[0].setAnimatorParamFloat("height", animPar.heights[0]);
		}
		if (item.GetItem() != null)
		{
			item.setAnimatorParamFloat("height", animPar.heights[0]);
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

	private bool SpankingProc(int _state)
	{
		if (!ctrlFlag.stopFeelFemale)
		{
			ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f - ctrlFlag.guageDecreaseRate * Time.deltaTime);
		}
		if (_state == 1 && ctrlFlag.feel_f < 0.5f)
		{
			setPlay("WIdle");
			voice.HouchiTime = 0f;
			return true;
		}
		if (Input.GetAxis("Mouse ScrollWheel") * (float)((!sprite.IsSpriteOver()) ? 1 : 0) == 0f)
		{
			return false;
		}
		if (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice)
		{
			Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
			voice.ResetVoice();
		}
		setPlay(_state switch
		{
			1 => "SAction", 
			0 => "WAction", 
			_ => "D_Action", 
		}, _isFade: false);
		upFeel = true;
		float value = Mathf.Clamp01(chaFemales[0].siriAkaRate + ctrlFlag.siriakaAddRate);
		chaFemales[0].ChangeSiriAkaRate(value);
		timeFeelUp = 0f;
		backupFeel = ctrlFlag.feel_f;
		backupFeelFlag = ctrlFlag.feelPain;
		ctrlFlag.isNotCtrl = false;
		if (randVoicePlays[0].Get() == 0)
		{
			ctrlFlag.voice.playVoices[0] = true;
		}
		ctrlFlag.voice.playShorts[0] = 0;
		return true;
	}

	private bool ActionProc(float _normalizedTime, int _state, HScene.AnimationListInfo _infoAnimList)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		if (_state == 0)
		{
			setPlay(((double)ctrlFlag.feel_f >= 0.5) ? "SIdle" : "WIdle", _isFade: false);
			voice.HouchiTime = 0f;
		}
		else if (ctrlFlag.selectAnimationListInfo == null && ctrlFlag.feel_f >= 1f)
		{
			setPlay((_state == 1) ? "Orgasm" : "D_Orgasm");
			ctrlFlag.feel_f = 0f;
			ctrlFlag.numOrgasm = Mathf.Clamp(ctrlFlag.numOrgasm + 1, 0, 10);
			ctrlFlag.AddOrgasm();
			sprite.objMotionListPanel.SetActive(value: false);
			ctrlFlag.voice.oldFinish = 0;
			voice.SetFinish(ctrlFlag.voice.oldFinish);
			ctrlFlag.nowOrgasm = true;
			ctrlFlag.AddTaiiParam();
			chaFemales[0].ChangeSiriAkaRate(1f);
			if (!ctrlFlag.isPainAction && ctrlFlag.nowAnimationInfo.lstSystem.Contains(4))
			{
				ctrlFlag.isPainAction = true;
			}
			ctrlFlag.rateNip = 1f;
			ctrlFlag.rateTuya = 1f;
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
				flag = chaFemales[0].fileGameInfo2.resistPain >= 100;
				flag |= chaFemales[0].fileParam2.hAttribute == 3;
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
		}
		else
		{
			setPlay((_state == 1) ? "SIdle" : "D_Orgasm_A", _isFade: false);
		}
		return true;
	}

	private bool AfterWaitingAnimation(float _normalizedTime, int _state)
	{
		if (_normalizedTime < 1f)
		{
			return false;
		}
		bool flag = !Manager.Config.HData.WeakStop;
		if (_state == 0 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
		{
			setPlay("D_Orgasm_A");
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 1;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.numFaintness = Mathf.Clamp(ctrlFlag.numFaintness + 1, 0, 999999);
			sprite.SetVisibleLeaveItToYou(_visible: false);
			sprite.SetAnimationMenu();
		}
		else if (eventNo == 19 && ctrlFlag.numOrgasm >= ctrlFlag.gotoFaintnessCount && flag)
		{
			setPlay("D_Orgasm_A");
			ctrlFlag.isFaintness = true;
			ctrlFlag.FaintnessType = 1;
			ctrlFlag.isFaintnessVoice = true;
			ctrlFlag.numFaintness = Mathf.Clamp(ctrlFlag.numFaintness + 1, 0, 999999);
			sprite.SetVisibleLeaveItToYou(_visible: false);
			sprite.SetAnimationMenu();
		}
		else
		{
			setPlay((_state == 0) ? "WIdle" : "D_Orgasm_A", _isFade: false);
			voice.HouchiTime = 0f;
		}
		ctrlFlag.nowOrgasm = false;
		ctrlObi.PlayUrine(use: false);
		voice.AfterFinish();
		return true;
	}
}
