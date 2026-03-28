using System;
using HS2;
using Illusion.Extensions;
using Manager;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ConciergeAchievementInfoComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public (SaveData.AchievementState, AchievementInfoData.Param) info;

		public Button btn;

		public Text text;

		public PointerEnterExitAction pointerAction;

		public Text txtUsePoint;

		public ConciergeAchievementInfoComponent conciergeAchievementInfoComponent;
	}

	[SerializeField]
	private RowInfo[] rows;

	[SerializeField]
	private Text textContent;

	[SerializeField]
	private Text textPoint;

	[SerializeField]
	private ConciergeAchievementUI conciergeAchievementUI;

	[SerializeField]
	private Color colorOK = Color.white;

	[SerializeField]
	private Color colorNO = Color.red;

	[SerializeField]
	private Color colorGET = Color.blue;

	public void SetData(int _index, (SaveData.AchievementState, AchievementInfoData.Param) _info, Action _onClickAction)
	{
		bool flag = _info.Item2 != null;
		rows[_index].btn.gameObject.SetActiveIfDifferent(flag);
		rows[_index].btn.onClick.RemoveAllListeners();
		if (!flag)
		{
			return;
		}
		rows[_index].conciergeAchievementInfoComponent = this;
		rows[_index].btn.onClick.AddListener(delegate
		{
			_onClickAction();
		});
		SetDataDraw(rows[_index], _info);
		if ((bool)rows[_index].pointerAction)
		{
			rows[_index].pointerAction.listActionEnter.Clear();
			rows[_index].pointerAction.listActionEnter.Add(delegate
			{
				textContent.text = _info.Item2.content[Singleton<GameSystem>.Instance.languageInt];
				if ((bool)textPoint)
				{
					textPoint.text = $"{_info.Item2.point}P";
				}
			});
			rows[_index].pointerAction.listActionExit.Clear();
			rows[_index].pointerAction.listActionExit.Add(delegate
			{
				textContent.text = "";
				if ((bool)textPoint)
				{
					textPoint.text = "";
				}
			});
		}
		rows[_index].info = _info;
	}

	public (SaveData.AchievementState, AchievementInfoData.Param) GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void SetListInfo(int _index, (SaveData.AchievementState, AchievementInfoData.Param) _info)
	{
		rows[_index].info = _info;
	}

	public RowInfo GetRow(int _index)
	{
		return rows[_index];
	}

	public void Disable(bool disable)
	{
	}

	public void Disvisible(bool disvisible)
	{
	}

	public void SetDataDraw(RowInfo _row, (SaveData.AchievementState, AchievementInfoData.Param) _info)
	{
		bool flag = _info.Item1 == SaveData.AchievementState.AS_Achieve;
		bool flag2 = conciergeAchievementUI.RemainingPoint - _info.Item2.point >= 0;
		_row.btn.interactable = flag2 && !flag;
		Text txtUsePoint = _row.txtUsePoint;
		if (flag)
		{
			txtUsePoint.color = colorGET;
			txtUsePoint.text = "GET";
		}
		else
		{
			txtUsePoint.color = (flag2 ? colorOK : colorNO);
			txtUsePoint.text = _info.Item2.point.ToString();
		}
		if ((bool)_row.text)
		{
			_row.text.text = _info.Item2.title[Singleton<GameSystem>.Instance.languageInt];
		}
	}
}
