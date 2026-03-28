using System.Collections;
using UniRx;
using UnityEngine;

public class Peeping : ProcBase
{
	private int oldFrame;

	private animParm animPar;

	private bool cloth = true;

	public Peeping(DeliveryMember _delivery)
		: base(_delivery)
	{
		animPar.heights = new float[1];
		CatID = 5;
	}

	public override bool SetStartMotion(bool _isIdle, int _modeCtrl, HScene.AnimationListInfo _infoAnimList)
	{
		ctrlFlag.isNotCtrl = false;
		oldFrame = 0;
		cloth = true;
		for (int i = 0; i < lstMotionIK.Count; i++)
		{
			if (ctrlFlag.nowAnimationInfo.id == 105 && ctrlFlag.nowAnimationInfo.id == 106)
			{
				lstMotionIK[i].Item3.Calc("In");
			}
			else
			{
				lstMotionIK[i].Item3.Calc("Loop");
			}
		}
		Observable.NextFrame().Subscribe(delegate
		{
			voice.AfterFinish();
		});
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
		float normalizedTime = FemaleAi.normalizedTime;
		int num = Mathf.FloorToInt(normalizedTime);
		if (ctrlFlag.nowAnimationInfo.id == 105)
		{
			if (!sprite.isFade)
			{
				HSceneSprite.FadeKindProc fadeKindProc = sprite.GetFadeKindProc();
				if (fadeKindProc != HSceneSprite.FadeKindProc.OutEnd)
				{
					if (FemaleAi.IsName("In"))
					{
						if (FemaleAi.normalizedTime >= 1f)
						{
							setPlay("Loop", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
							if (ProcBase.hSceneManager.UrineType == 1)
							{
								particle.Play(4);
							}
						}
						if (cloth && FemaleAi.normalizedTime >= 0.5f)
						{
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 2, _isForce: true);
							cloth = false;
						}
					}
					else if (FemaleAi.IsName("Loop"))
					{
						if (FemaleAi.normalizedTime >= (float)ctrlFlag.peepingLoopNumY)
						{
							setPlay("Out", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
						}
						if (ProcBase.hSceneManager.UrineType == 0)
						{
							ctrlObi.PlayUrine(use: true);
						}
						ctrlObi.PlayUrine(FemaleAi);
					}
					else if (FemaleAi.IsName("Out"))
					{
						if (normalizedTime > ctrlFlag.peepingFadeY)
						{
							sprite.FadeState(HSceneSprite.FadeKind.Out, 1.5f);
						}
						if (!cloth && FemaleAi.normalizedTime >= 0.85f)
						{
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
							cloth = true;
						}
						if (FemaleAi.normalizedTime >= 1f)
						{
							setPlay("Out_Loop", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
						}
						ctrlObi.PlayUrine(use: false);
					}
					else if (FemaleAi.IsName("Out_Loop") && FemaleAi.normalizedTime >= (float)ctrlFlag.peepingOutLoopNumY && !ConfirmDialog.active && ctrlFlag.click != HSceneFlagCtrl.ClickKind.PeepingRestart)
					{
						ConfirmDialog.Status status = ConfirmDialog.status;
						status.Sentence = "トイレ覗きを終了しますか？";
						status.YesText = "終了する";
						status.NoText = "もう一度見る";
						status.Yes = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
						};
						status.No = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.PeepingRestart;
						};
						ConfirmDialog.Load();
					}
				}
				else if (fadeKindProc == HSceneSprite.FadeKindProc.OutEnd)
				{
					GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
					if (FemaleAi.IsName("Out"))
					{
						if (!cloth && FemaleAi.normalizedTime >= 0.85f)
						{
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
							cloth = true;
						}
						if (FemaleAi.normalizedTime >= 1f)
						{
							setPlay("Out_Loop", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
						}
					}
					else if (FemaleAi.IsName("Out_Loop") && FemaleAi.normalizedTime >= (float)ctrlFlag.peepingOutLoopNumY && !ConfirmDialog.active && ctrlFlag.click != HSceneFlagCtrl.ClickKind.PeepingRestart)
					{
						ConfirmDialog.Status status2 = ConfirmDialog.status;
						status2.Sentence = "トイレ覗きを終了しますか？";
						status2.YesText = "終了する";
						status2.NoText = "もう一度見る";
						status2.Yes = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
						};
						status2.No = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.PeepingRestart;
						};
						ConfirmDialog.Load();
					}
					if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.PeepingRestart)
					{
						setPlay("In", 0f, _isFade: false);
						sprite.FadeState(HSceneSprite.FadeKind.In, 0.5f);
						chaFemales[0].animBody.speed = 1f;
						voice.StartCoroutine(InitOldMemberCoroutine());
						ctrlFlag.AddTaiiParam();
					}
				}
			}
			else if (FemaleAi.IsName("Out"))
			{
				if (!cloth && FemaleAi.normalizedTime >= 0.85f)
				{
					GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
					cloth = true;
				}
				if (FemaleAi.normalizedTime >= 1f)
				{
					setPlay("Out_Loop", 0f, _isFade: false);
					ctrlFlag.speed = 0f;
					ctrlFlag.loopType = 0;
					ctrlFlag.motions[0] = 0f;
					ctrlFlag.nowSpeedStateFast = false;
				}
				ctrlObi.PlayUrine(use: false);
			}
			else if (FemaleAi.IsName("Out_Loop") && FemaleAi.normalizedTime >= (float)ctrlFlag.peepingOutLoopNumY && !ConfirmDialog.active && ctrlFlag.click != HSceneFlagCtrl.ClickKind.PeepingRestart)
			{
				ConfirmDialog.Status status3 = ConfirmDialog.status;
				status3.Sentence = "トイレ覗きを終了しますか？";
				status3.YesText = "終了する";
				status3.NoText = "もう一度見る";
				status3.Yes = delegate
				{
					ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
				};
				status3.No = delegate
				{
					ctrlFlag.click = HSceneFlagCtrl.ClickKind.PeepingRestart;
				};
				ConfirmDialog.Load();
			}
		}
		else if (ctrlFlag.nowAnimationInfo.id == 106)
		{
			if (!sprite.isFade)
			{
				HSceneSprite.FadeKindProc fadeKindProc2 = sprite.GetFadeKindProc();
				if (fadeKindProc2 != HSceneSprite.FadeKindProc.OutEnd)
				{
					if (FemaleAi.IsName("In"))
					{
						if (FemaleAi.normalizedTime >= 1f)
						{
							setPlay("Loop", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
							if (ProcBase.hSceneManager.UrineType == 1)
							{
								particle.Play(4);
							}
						}
						if (cloth && FemaleAi.normalizedTime >= 0.5f)
						{
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 2, _isForce: true);
							cloth = false;
						}
					}
					else if (FemaleAi.IsName("Loop"))
					{
						if (FemaleAi.normalizedTime >= (float)ctrlFlag.peepingLoopNumW)
						{
							setPlay("Out", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
						}
						if (ProcBase.hSceneManager.UrineType == 0)
						{
							ctrlObi.PlayUrine(use: true);
						}
						ctrlObi.PlayUrine(FemaleAi);
					}
					else if (FemaleAi.IsName("Out"))
					{
						if (normalizedTime > ctrlFlag.peepingFadeW)
						{
							sprite.FadeState(HSceneSprite.FadeKind.Out, 1.5f);
						}
						if (!cloth && FemaleAi.normalizedTime >= 0.85f)
						{
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
							cloth = true;
						}
						if (FemaleAi.normalizedTime >= 1f)
						{
							setPlay("Out_Loop", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
						}
						ctrlObi.PlayUrine(use: false);
					}
					else if (FemaleAi.IsName("Out_Loop") && FemaleAi.normalizedTime >= (float)ctrlFlag.peepingOutLoopNumW && !ConfirmDialog.active && ctrlFlag.click != HSceneFlagCtrl.ClickKind.PeepingRestart)
					{
						ConfirmDialog.Status status4 = ConfirmDialog.status;
						status4.Sentence = "トイレ覗きを終了しますか？";
						status4.YesText = "終了する";
						status4.NoText = "もう一度見る";
						status4.Yes = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
						};
						status4.No = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.PeepingRestart;
						};
						ConfirmDialog.Load();
					}
				}
				else if (fadeKindProc2 == HSceneSprite.FadeKindProc.OutEnd)
				{
					GlobalMethod.setCameraMoveFlag(ctrlFlag.cameraCtrl, _bPlay: false);
					if (FemaleAi.IsName("Out"))
					{
						if (!cloth && FemaleAi.normalizedTime >= 0.85f)
						{
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
							cloth = true;
						}
						if (FemaleAi.normalizedTime >= 1f)
						{
							setPlay("Out_Loop", 0f, _isFade: false);
							ctrlFlag.speed = 0f;
							ctrlFlag.loopType = 0;
							ctrlFlag.motions[0] = 0f;
							ctrlFlag.nowSpeedStateFast = false;
							GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
						}
					}
					else if (FemaleAi.IsName("Out_Loop") && FemaleAi.normalizedTime >= (float)ctrlFlag.peepingOutLoopNumW && !ConfirmDialog.active && ctrlFlag.click != HSceneFlagCtrl.ClickKind.PeepingRestart)
					{
						ConfirmDialog.Status status5 = ConfirmDialog.status;
						status5.Sentence = "トイレ覗きを終了しますか？";
						status5.YesText = "終了する";
						status5.NoText = "もう一度見る";
						status5.Yes = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
						};
						status5.No = delegate
						{
							ctrlFlag.click = HSceneFlagCtrl.ClickKind.PeepingRestart;
						};
						ConfirmDialog.Load();
					}
					if (ctrlFlag.click == HSceneFlagCtrl.ClickKind.PeepingRestart)
					{
						setPlay("In", 0f, _isFade: false);
						sprite.FadeState(HSceneSprite.FadeKind.In, 0.5f);
						chaFemales[0].animBody.speed = 1f;
						voice.StartCoroutine(InitOldMemberCoroutine());
						ctrlFlag.AddTaiiParam();
					}
				}
			}
			else if (FemaleAi.IsName("Out"))
			{
				if (!cloth && FemaleAi.normalizedTime >= 0.85f)
				{
					GlobalMethod.SetAllClothState(chaFemales[0], _isUpper: false, 0, _isForce: true);
					cloth = true;
				}
				if (FemaleAi.normalizedTime >= 1f)
				{
					setPlay("Out_Loop", 0f, _isFade: false);
					ctrlFlag.speed = 0f;
					ctrlFlag.loopType = 0;
					ctrlFlag.motions[0] = 0f;
					ctrlFlag.nowSpeedStateFast = false;
				}
				ctrlObi.PlayUrine(use: false);
			}
			else if (FemaleAi.IsName("Out_Loop") && FemaleAi.normalizedTime >= (float)ctrlFlag.peepingOutLoopNumW && !ConfirmDialog.active && ctrlFlag.click != HSceneFlagCtrl.ClickKind.PeepingRestart)
			{
				ConfirmDialog.Status status6 = ConfirmDialog.status;
				status6.Sentence = "トイレ覗きを終了しますか？";
				status6.YesText = "終了する";
				status6.NoText = "もう一度見る";
				status6.Yes = delegate
				{
					ctrlFlag.click = HSceneFlagCtrl.ClickKind.SceneEnd;
				};
				status6.No = delegate
				{
					ctrlFlag.click = HSceneFlagCtrl.ClickKind.PeepingRestart;
				};
				ConfirmDialog.Load();
			}
		}
		else if (FemaleAi.IsName("Loop") && oldFrame != num)
		{
			voice.AfterFinish();
			ctrlFlag.AddTaiiParam();
		}
		oldFrame = num;
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
		if (item.GetItem() != null)
		{
			item.setAnimatorParamFloat("height", animPar.heights[0]);
		}
	}

	private void setPlay(string _playAnimation, float _normalizetime, bool _isFade = true)
	{
		chaFemales[0].syncPlay(_playAnimation, 0, _normalizetime);
		for (int i = 0; i < lstMotionIK.Count; i++)
		{
			lstMotionIK[i].Item3.Calc(_playAnimation);
		}
		if (item != null)
		{
			item.syncPlay(_playAnimation, _normalizetime);
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

	private IEnumerator InitOldMemberCoroutine()
	{
		yield return new WaitForEndOfFrame();
		se.InitOldMember(1);
		oldFrame = 0;
		voice.AfterFinish();
		yield return null;
	}
}
