using System;
using System.IO;
using GameLoadCharaFileSystem;
using HS2;
using Illusion.Component;
using Illusion.Component.UI;
using Illusion.Extensions;
using Manager;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LobbyCharaSelectInfoComponent1 : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public GameCharaFileInfo info;

		public Toggle toggle;

		public Text text;

		public SpriteChangeCtrl sccState;

		public PointerEnterExitAction pointerAction;

		public InteractableAlphaChanger interactableAlphaChanger;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, GameCharaFileInfo _info, Action<bool> _onClickAction, Action _onPointEnter, Action _onPointExit)
	{
		bool flag = _info != null;
		if (flag)
		{
			flag = !_info.FullPath.IsNullOrEmpty() && File.Exists(_info.FullPath);
		}
		rows[_index].toggle.gameObject.SetActiveIfDifferent(flag);
		rows[_index].toggle.onValueChanged.RemoveAllListeners();
		if (!flag)
		{
			return;
		}
		rows[_index].toggle.onValueChanged.AddListener(delegate(bool _isOn)
		{
			_onClickAction(_isOn);
		});
		rows[_index].toggle.SetIsOnWithoutCallback(isOn: false);
		rows[_index].toggle.interactable = !_info.isEntry;
		if ((bool)rows[_index].text)
		{
			rows[_index].text.text = _info.name;
		}
		rows[_index].sccState.ChangeValue(GlobalHS2Calc.GetStateIconNum((int)_info.state, _info.voice));
		if ((bool)rows[_index].pointerAction)
		{
			rows[_index].pointerAction.listActionEnter.Clear();
			rows[_index].pointerAction.listActionEnter.Add(delegate
			{
				if (rows[_index].toggle.IsInteractable())
				{
					_onPointEnter?.Invoke();
				}
			});
			rows[_index].pointerAction.listActionExit.Clear();
			rows[_index].pointerAction.listActionExit.Add(delegate
			{
				_onPointExit?.Invoke();
			});
		}
		rows[_index].info = _info;
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].interactableAlphaChanger.IsTextColorChange = !_isOn;
		rows[_index].text.color = (_isOn ? Game.selectFontColor : Game.defaultFontColor);
		rows[_index].toggle.SetIsOnWithoutCallback(_isOn);
	}

	public GameCharaFileInfo GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public RowInfo GetRow(int _index)
	{
		return rows[_index];
	}
}
