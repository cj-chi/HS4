using System;
using Illusion.Extensions;
using Manager;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FurRoomAchievementItemInfoComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public (SaveData.AchievementState, AchievementInfoData.Param) info;

		public Toggle tgl;

		public Text text;

		public PointerEnterExitAction pointerAction;
	}

	[SerializeField]
	private RowInfo[] rows;

	[SerializeField]
	private Text textContent;

	[SerializeField]
	private Text textPoint;

	public void SetData(int _index, (SaveData.AchievementState, AchievementInfoData.Param) _info)
	{
		bool flag = _info.Item2 != null;
		rows[_index].tgl.gameObject.SetActiveIfDifferent(flag);
		if (!flag)
		{
			return;
		}
		bool isOn = _info.Item1 == SaveData.AchievementState.AS_Achieve;
		rows[_index].tgl.isOn = isOn;
		if ((bool)rows[_index].text)
		{
			rows[_index].text.text = _info.Item2.title[Singleton<GameSystem>.Instance.languageInt];
		}
		if ((bool)rows[_index].pointerAction)
		{
			rows[_index].pointerAction.listActionEnter.Clear();
			rows[_index].pointerAction.listActionEnter.Add(delegate
			{
				textContent.text = _info.Item2.content[Singleton<GameSystem>.Instance.languageInt];
				textPoint.text = $"{_info.Item2.point}P";
			});
			rows[_index].pointerAction.listActionExit.Clear();
			rows[_index].pointerAction.listActionExit.Add(delegate
			{
				textContent.text = "";
				textPoint.text = "";
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
}
