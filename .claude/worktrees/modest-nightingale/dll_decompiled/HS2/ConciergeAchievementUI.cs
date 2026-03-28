using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HS2;

public class ConciergeAchievementUI : MonoBehaviour
{
	[SerializeField]
	private Button btnBack;

	[SerializeField]
	private Text textNextCount;

	[SerializeField]
	private GameObject objComplete;

	[SerializeField]
	private ConciergeMenuUI conciergeMenuUI;

	[SerializeField]
	private ConciergeAchievementItemScrollController scrollCtrl;

	[SerializeField]
	private ConciergeAchievementScrollController scrolExchangeCtrl;

	private readonly string[] strNextCounts = new string[5] { "残り{0}P", "Remaining Point {0}P", "Remaining Point {0}P", "Remaining Point {0}P", "" };

	private readonly string[] strExchange = new string[6] { "解放しますか？", "Do you want me to let you go?", "Do you want me to let you go?", "Do you want me to let you go?", "Do you want me to let you go?", "" };

	public int RemainingPoint
	{
		get
		{
			Game game = Singleton<Game>.Instance;
			SaveData saveData = game.saveData;
			int num = (from item in saveData.achievement
				where item.Value == SaveData.AchievementState.AS_Achieve
				select game.infoAchievementDic[item.Key].point).Sum();
			int num2 = (from item in saveData.achievementExchange
				where item.Value == SaveData.AchievementState.AS_Achieve
				select game.infoAchievementExchangeDic[item.Key].point).Sum();
			return num - num2;
		}
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		btnBack.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Back());
		});
		btnBack.OnPointerEnterAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.sel);
		});
		scrolExchangeCtrl.onSelect = delegate(ConciergeAchievementScrollController.ScrollData _data)
		{
			Utils.Sound.Play(SystemSE.ok_s);
			ConfirmDialog.Status status = ConfirmDialog.status;
			status.Sentence = strExchange[Singleton<GameSystem>.Instance.languageInt];
			status.Yes = delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
				_ = Singleton<Game>.Instance;
				SaveData.SetAchievementExchangeRelease(_data.info.Item2.id);
				scrolExchangeCtrl.GetScrollData(_data.index).info.Item1 = SaveData.AchievementState.AS_Achieve;
				ConciergeAchievementInfoComponent.RowInfo row = _data.rowData.row;
				row.info.Item1 = SaveData.AchievementState.AS_Achieve;
				row.conciergeAchievementInfoComponent.SetDataDraw(row, row.info);
				textNextCount.text = string.Format(strNextCounts[Singleton<GameSystem>.Instance.languageInt], RemainingPoint);
				CompleteProc();
				scrolExchangeCtrl.UpdateNowDraw();
			};
			status.No = delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
			};
			ConfirmDialog.Load();
		};
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !Scene.IsFadeNow
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where hm.CGMain.interactable
			where hm.CGModes[3].alpha >= 0.9f
			where hm.CGAchievement.alpha >= 0.9f
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			StartCoroutine(Back());
		});
		base.enabled = true;
	}

	private IEnumerator Back()
	{
		yield return null;
		Singleton<HomeSceneManager>.Instance.SetCociergeModeCanvasGroup(0);
		EventSystem.current.SetSelectedGameObject(null);
		conciergeMenuUI.SetDefaultToggleTransition(-1);
	}

	public void Init()
	{
		_ = Singleton<Game>.Instance.saveData;
		textNextCount.text = string.Format(strNextCounts[Singleton<GameSystem>.Instance.languageInt], RemainingPoint);
		CompleteProc();
		InitAchievementView();
	}

	public void InitAchievementView()
	{
		Game game = Singleton<Game>.Instance;
		scrolExchangeCtrl.SelectInfoClear();
		scrolExchangeCtrl.Init(game.infoAchievementExchangeDic.Select((KeyValuePair<int, AchievementInfoData.Param> dic) => (game.saveData.achievementExchange[dic.Key], Value: dic.Value)).ToList());
		scrollCtrl.Init(game.infoAchievementDic.Select((KeyValuePair<int, AchievementInfoData.Param> dic) => (game.saveData.achievement[dic.Key], Value: dic.Value)).ToList());
	}

	private void CompleteProc()
	{
		bool flag = !Singleton<Game>.Instance.saveData.achievementExchange.Where((KeyValuePair<int, SaveData.AchievementState> item) => item.Value == SaveData.AchievementState.AS_NotAchieved).Any();
		textNextCount.gameObject.SetActiveIfDifferent(!flag);
		objComplete.SetActiveIfDifferent(flag);
	}
}
