using System;
using System.Collections;
using System.Collections.Generic;
using AIChara;
using GameLoadCharaFileSystem;
using Illusion.Component.UI;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UIAnimatorCore;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class LobbyParameterUI : MonoBehaviour
{
	[Serializable]
	public class ExperienceUI
	{
		public Image image;

		public SpriteChangeCtrl scc;

		public Button btnChange;
	}

	[SerializeField]
	private CanvasGroup cgParameterWindow;

	[SerializeField]
	private Text txtAction;

	[SerializeField]
	private Text txtCharaName;

	[SerializeField]
	private Toggle tglOpenClose;

	[SerializeField]
	private Image imgOpenCloseBase;

	[SerializeField]
	private UIAnimator paramUIAnimator;

	[SerializeField]
	private SpriteChangeCtrl sccState;

	[SerializeField]
	private Text txtState;

	[SerializeField]
	private Toggle tglStateLock;

	[SerializeField]
	private Text txtPersonal;

	[SerializeField]
	private Text txtTrait;

	[SerializeField]
	private Text txtHAttribute;

	[SerializeField]
	private ExperienceUI expH;

	[SerializeField]
	private ExperienceUI expPain;

	[SerializeField]
	private ExperienceUI expHip;

	[SerializeField]
	private Image imgBroken;

	[SerializeField]
	private Toggle tglBrokenLock;

	[SerializeField]
	private Image imgDependence;

	[SerializeField]
	private Toggle tglDependenceLock;

	private int entryNO;

	private GameCharaFileInfo gameCharainfo;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<LobbySceneManager>.IsInstance());
		LobbySceneManager lm = Singleton<LobbySceneManager>.Instance;
		tglOpenClose.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			imgOpenCloseBase.enabled = !_isOn;
			cgParameterWindow.blocksRaycasts = false;
			paramUIAnimator.PlayAnimation((paramUIAnimator.CurrentAnimType != AnimSetupType.Outro) ? AnimSetupType.Outro : AnimSetupType.Intro, delegate
			{
				cgParameterWindow.blocksRaycasts = true;
			});
		});
		(from _ in tglStateLock.OnValueChangedAsObservable().Skip(1)
			where lm.heroines[entryNO] != null
			select _).Subscribe(delegate(bool _isOn)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			if (gameCharainfo != null)
			{
				gameCharainfo.lockNowState = _isOn;
				if (gameCharainfo.lockBroken != _isOn)
				{
					gameCharainfo.lockBroken = _isOn;
				}
				if (gameCharainfo.lockDependence != _isOn)
				{
					gameCharainfo.lockDependence = _isOn;
				}
			}
			ChaFileGameInfo2 gameinfo = lm.heroines[entryNO].gameinfo2;
			gameinfo.lockNowState = _isOn;
			gameinfo.lockBroken = _isOn;
			gameinfo.lockDependence = _isOn;
			tglBrokenLock.SetIsOnWithoutCallback(_isOn);
			tglBrokenLock.interactable = !_isOn;
			tglDependenceLock.SetIsOnWithoutCallback(_isOn);
			tglDependenceLock.interactable = !_isOn;
			lm.heroines[entryNO].chaFile.SaveCharaFile(lm.heroines[entryNO].chaFile.charaFileName);
		});
		(from _ in expH.btnChange.OnClickAsObservable()
			where lm.heroines[entryNO] != null
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ChaFileGameInfo2 gameinfo = lm.heroines[entryNO].gameinfo2;
			gameinfo.resistH = ((gameinfo.resistH < 100) ? 100 : 0);
			if (gameCharainfo != null)
			{
				gameCharainfo.resistH = gameinfo.resistH;
			}
			lm.heroines[entryNO].chaFile.SaveCharaFile(lm.heroines[entryNO].chaFile.charaFileName);
			SetExperienceUI(expH, gameinfo.resistH);
		});
		(from _ in expPain.btnChange.OnClickAsObservable()
			where lm.heroines[entryNO] != null
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ChaFileGameInfo2 gameinfo = lm.heroines[entryNO].gameinfo2;
			gameinfo.resistPain = ((gameinfo.resistPain < 100) ? 100 : 0);
			if (gameCharainfo != null)
			{
				gameCharainfo.resistPain = gameinfo.resistPain;
			}
			lm.heroines[entryNO].chaFile.SaveCharaFile(lm.heroines[entryNO].chaFile.charaFileName);
			SetExperienceUI(expPain, gameinfo.resistPain);
		});
		(from _ in expHip.btnChange.OnClickAsObservable()
			where lm.heroines[entryNO] != null
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ChaFileGameInfo2 gameinfo = lm.heroines[entryNO].gameinfo2;
			gameinfo.resistAnal = ((gameinfo.resistAnal < 100) ? 100 : 0);
			if (gameCharainfo != null)
			{
				gameCharainfo.resistAnal = gameinfo.resistAnal;
			}
			lm.heroines[entryNO].chaFile.SaveCharaFile(lm.heroines[entryNO].chaFile.charaFileName);
			SetExperienceUI(expHip, gameinfo.resistAnal);
		});
		(from _ in tglBrokenLock.OnValueChangedAsObservable().Skip(1)
			where lm.heroines[entryNO] != null
			select _).Subscribe(delegate(bool _isOn)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			if (gameCharainfo != null)
			{
				gameCharainfo.lockBroken = _isOn;
			}
			lm.heroines[entryNO].gameinfo2.lockBroken = _isOn;
			lm.heroines[entryNO].chaFile.SaveCharaFile(lm.heroines[entryNO].chaFile.charaFileName);
		});
		(from _ in tglDependenceLock.OnValueChangedAsObservable().Skip(1)
			where lm.heroines[entryNO] != null
			select _).Subscribe(delegate(bool _isOn)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			if (gameCharainfo != null)
			{
				gameCharainfo.lockDependence = _isOn;
			}
			lm.heroines[entryNO].gameinfo2.lockDependence = _isOn;
			lm.heroines[entryNO].chaFile.SaveCharaFile(lm.heroines[entryNO].chaFile.charaFileName);
		});
		List<Toggle> list = new List<Toggle>();
		list.Add(tglOpenClose);
		list.Add(tglStateLock);
		list.Add(tglBrokenLock);
		list.Add(tglDependenceLock);
		list.ForEach(delegate(Toggle t)
		{
			t.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (t.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
				}
			});
		});
		List<Button> list2 = new List<Button>();
		list2.Add(expH.btnChange);
		list2.Add(expPain.btnChange);
		list2.Add(expHip.btnChange);
		list2.ForEach(delegate(Button b)
		{
			b.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (b.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
				}
			});
		});
		InitParamUIAnimator();
		base.enabled = true;
	}

	public void InitParamUIAnimator()
	{
		paramUIAnimator.SetAnimType(AnimSetupType.Intro);
		paramUIAnimator.ResetToEnd();
		tglOpenClose.SetIsOnWithoutCallback(isOn: true);
		imgOpenCloseBase.enabled = false;
	}

	public void SetParameter(GameCharaFileInfo _info, int _eventNo, int _entryNo)
	{
		entryNO = _entryNo;
		txtCharaName.text = _info.name;
		gameCharainfo = _info;
		sccState.ChangeValue(GlobalHS2Calc.GetStateIconNum((int)_info.state, _info.voice));
		string text = "";
		if (Singleton<Game>.Instance.saveData.TutorialNo == -1 && _entryNo == 0 && Singleton<Game>.Instance.infoEventContentDic.TryGetValue(_eventNo, out var value))
		{
			text = value.eventNames[Singleton<GameSystem>.Instance.languageInt];
		}
		txtAction.text = text;
		txtState.text = Game.infoStateTable[(int)_info.state];
		tglStateLock.SetIsOnWithoutCallback(_info.lockNowState);
		txtPersonal.text = _info.personality;
		txtTrait.text = Game.infoTraitTable[_info.trait];
		txtHAttribute.text = Game.infoHAttributeTable[_info.hAttribute];
		SetExperienceUI(expH, _info.resistH);
		SetExperienceUI(expPain, _info.resistPain);
		SetExperienceUI(expHip, _info.resistAnal);
		imgBroken.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _info.broken));
		tglBrokenLock.SetIsOnWithoutCallback(_info.lockBroken);
		imgDependence.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _info.dependence));
		tglDependenceLock.SetIsOnWithoutCallback(_info.lockDependence);
		bool lockNowState = _info.lockNowState;
		tglBrokenLock.interactable = !lockNowState;
		tglDependenceLock.interactable = !lockNowState;
		SetAchievement();
	}

	public void SetParameter(ChaFileControl _info, int _eventNo, int _entryNo)
	{
		if (_info != null)
		{
			gameCharainfo = null;
			entryNO = _entryNo;
			ChaFileParameter parameter = _info.parameter;
			ChaFileParameter2 parameter2 = _info.parameter2;
			ChaFileGameInfo2 gameinfo = _info.gameinfo2;
			txtCharaName.text = parameter.fullname;
			sccState.ChangeValue(GlobalHS2Calc.GetStateIconNum((int)gameinfo.nowDrawState, parameter2.personality));
			string text = "";
			if (Singleton<Game>.Instance.saveData.TutorialNo == -1 && _entryNo == 0 && Singleton<Game>.Instance.infoEventContentDic.TryGetValue(_eventNo, out var value))
			{
				text = value.eventNames[Singleton<GameSystem>.Instance.languageInt];
			}
			txtAction.text = text;
			txtState.text = Game.infoStateTable[(int)gameinfo.nowDrawState];
			tglStateLock.SetIsOnWithoutCallback(gameinfo.lockNowState);
			txtPersonal.text = (Voice.infoTable.TryGetValue(parameter2.personality, out var value2) ? value2.Personality : "");
			txtTrait.text = Game.infoTraitTable[parameter2.trait];
			txtHAttribute.text = Game.infoHAttributeTable[parameter2.hAttribute];
			SetExperienceUI(expH, gameinfo.resistH);
			SetExperienceUI(expPain, gameinfo.resistPain);
			SetExperienceUI(expHip, gameinfo.resistAnal);
			imgBroken.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, gameinfo.Broken));
			tglBrokenLock.SetIsOnWithoutCallback(gameinfo.lockBroken);
			imgDependence.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, gameinfo.Dependence));
			tglDependenceLock.SetIsOnWithoutCallback(gameinfo.lockDependence);
			bool lockNowState = gameinfo.lockNowState;
			tglBrokenLock.interactable = !lockNowState;
			tglDependenceLock.interactable = !lockNowState;
			SetAchievement();
		}
	}

	public void InitParameter()
	{
		txtCharaName.text = "";
		InitParamUIAnimator();
	}

	public void SetExperienceUI(ExperienceUI _ui, int _resist)
	{
		_ = Singleton<GameSystem>.Instance.languageInt;
		_ui.image.fillAmount = Mathf.Clamp01(Mathf.InverseLerp(0f, 100f, _resist));
		_ui.scc.ChangeValue((_resist >= 100) ? 1 : 0);
	}

	private void SetAchievement()
	{
		bool active = SaveData.IsAchievementExchangeRelease(14);
		tglStateLock.gameObject.SetActiveIfDifferent(active);
		tglBrokenLock.gameObject.SetActiveIfDifferent(active);
		tglDependenceLock.gameObject.SetActiveIfDifferent(active);
		active = SaveData.IsAchievementExchangeRelease(15);
		expH.btnChange.gameObject.SetActiveIfDifferent(active);
		expPain.btnChange.gameObject.SetActiveIfDifferent(active);
		expHip.btnChange.gameObject.SetActiveIfDifferent(active);
	}
}
