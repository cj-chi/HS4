using System;
using Illusion.Extensions;
using Manager;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ConfigBGMChangeComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public BGMNameInfo.Param info;

		public Toggle tgl;

		public Text text;

		public GameObject objSelect;

		public PointerEnterExitAction pointerAction;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, BGMNameInfo.Param _info, Action<bool> _onClickAction)
	{
		bool flag = _info != null;
		rows[_index].tgl.onValueChanged.RemoveAllListeners();
		rows[_index].tgl.gameObject.SetActiveIfDifferent(flag);
		if (!flag)
		{
			return;
		}
		if ((bool)rows[_index].text)
		{
			rows[_index].text.text = _info.name[Singleton<GameSystem>.Instance.languageInt];
		}
		rows[_index].tgl.onValueChanged.AddListener(delegate(bool _isOn)
		{
			_onClickAction(_isOn);
		});
		_ = _index;
		if ((bool)rows[_index].pointerAction)
		{
			rows[_index].pointerAction.listActionEnter.Clear();
			rows[_index].pointerAction.listActionEnter.Add(delegate
			{
				rows[_index].objSelect.SetActiveIfDifferent(active: true);
			});
			rows[_index].pointerAction.listActionExit.Clear();
			rows[_index].pointerAction.listActionExit.Add(delegate
			{
				rows[_index].objSelect.SetActiveIfDifferent(active: false);
			});
		}
		rows[_index].info = _info;
	}

	public BGMNameInfo.Param GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void SetListInfo(int _index, BGMNameInfo.Param _info)
	{
		rows[_index].info = _info;
	}

	public RowInfo GetRow(int _index)
	{
		return rows[_index];
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].tgl.SetIsOnWithoutCallback(_isOn);
		rows[_index].text.color = (_isOn ? Game.selectFontColor : Game.defaultFontColor);
	}

	public void Disable(bool disable)
	{
	}

	public void Disvisible(bool disvisible)
	{
	}
}
