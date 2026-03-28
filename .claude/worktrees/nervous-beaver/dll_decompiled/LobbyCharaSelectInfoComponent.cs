using System;
using System.IO;
using AIChara;
using GameLoadCharaFileSystem;
using HS2;
using Illusion.Component;
using Illusion.Component.UI;
using Illusion.Extensions;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LobbyCharaSelectInfoComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public GameCharaFileInfo info;

		public Toggle toggle;

		public RawImage riCard;

		public SpriteChangeCtrl sccState;

		public InteractableAlphaChanger interactableAlphaChanger;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, GameCharaFileInfo _info, Action<bool> _onClickAction, Action _onPointEnter, Action _onPointExit, int _entryNo)
	{
		bool flag = _info != null;
		if (flag)
		{
			flag = !_info.FullPath.IsNullOrEmpty() && File.Exists(_info.FullPath);
		}
		rows[_index].toggle.gameObject.SetActiveIfDifferent(flag);
		rows[_index].toggle.onValueChanged.RemoveAllListeners();
		if (flag)
		{
			rows[_index].toggle.onValueChanged.AddListener(delegate(bool _isOn)
			{
				_onClickAction(_isOn);
			});
			rows[_index].toggle.SetIsOnWithoutCallback(isOn: false);
			bool flag2 = _entryNo switch
			{
				1 => _info.hcount != 0 && _info.state != ChaFileDefine.State.Broken, 
				0 => true, 
				_ => false, 
			};
			rows[_index].toggle.interactable = !_info.isEntry && flag2;
			rows[_index].interactableAlphaChanger.IsInteract.Value = rows[_index].toggle.interactable;
			rows[_index].sccState.ChangeValue(GlobalHS2Calc.GetStateIconNum((int)_info.state, _info.voice));
			if (rows[_index].riCard.texture != null)
			{
				UnityEngine.Object.Destroy(rows[_index].riCard.texture);
				rows[_index].riCard.texture = null;
			}
			if (_info.pngData != null || !_info.FullPath.IsNullOrEmpty())
			{
				rows[_index].riCard.texture = PngAssist.ChangeTextureFromByte(_info.pngData ?? PngFile.LoadPngBytes(_info.FullPath));
			}
			rows[_index].info = _info;
		}
	}

	public void SetToggleON(int _index, bool _isOn)
	{
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
