using System;
using System.IO;
using Illusion.Component.UI;
using Illusion.Extensions;
using Manager;
using SceneAssist;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoadCharaFileSystem;

[DisallowMultipleComponent]
public class GameCharaFileInfoComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public GameCharaFileInfo info;

		public Toggle tgl;

		public RawImage imgThumb;

		public PointerEnterExitAction pointerAction;

		public GameObject objEntry;

		public SpriteChangeCtrl spriteChangeCtrl;
	}

	[SerializeField]
	private RowInfo[] rows;

	[SerializeField]
	private Texture2D texEmpty;

	public void SetData(int _index, bool _isSameGroupCheck, GameCharaFileInfo _info, Action<bool> _onClickAction, Action<GameCharaFileInfo> _onDoubleClick)
	{
		RowInfo row = rows[_index];
		bool flag = _info != null;
		if (flag)
		{
			flag = (!_info.FullPath.IsNullOrEmpty() && File.Exists(_info.FullPath)) || _info.pngData != null;
			_info.fic = row;
		}
		row.tgl.gameObject.SetActiveIfDifferent(flag);
		row.tgl.onValueChanged.RemoveAllListeners();
		if (!flag)
		{
			return;
		}
		row.tgl.onValueChanged.AddListener(delegate(bool _isOn)
		{
			_onClickAction(_isOn);
		});
		row.tgl.SetIsOnWithoutCallback(isOn: false);
		row.tgl.interactable = !_info.isEntry;
		if (_isSameGroupCheck && !_info.isEntry)
		{
			row.tgl.interactable = !_info.lstFilter.Contains(Singleton<Game>.Instance.saveData.selectGroup + 1);
		}
		if ((bool)row.objEntry)
		{
			row.objEntry.SetActiveIfDifferent(_info.isEntry);
		}
		if (row.imgThumb.texture != null && row.imgThumb.texture != texEmpty)
		{
			UnityEngine.Object.Destroy(row.imgThumb.texture);
			row.imgThumb.texture = null;
		}
		if (_info.pngData != null || !_info.FullPath.IsNullOrEmpty())
		{
			row.imgThumb.texture = PngAssist.ChangeTextureFromByte(_info.pngData ?? PngFile.LoadPngBytes(_info.FullPath));
		}
		else
		{
			row.imgThumb.texture = texEmpty;
		}
		if (row.spriteChangeCtrl != null)
		{
			row.spriteChangeCtrl.ChangeValue(_info.sex);
		}
		if ((bool)row.pointerAction)
		{
			row.pointerAction.listActionEnter.Clear();
			row.pointerAction.listActionEnter.Add(delegate
			{
				_ = row.tgl.interactable;
			});
		}
		ClickEventListener.Get(row.tgl.gameObject).SetDoubleClickEventHandler(delegate
		{
			if (row.tgl.interactable)
			{
				_onDoubleClick?.Invoke(_info);
				row.tgl.SetIsOnWithoutCallback(isOn: false);
			}
		});
		row.info = _info;
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].tgl.SetIsOnWithoutCallback(_isOn);
	}

	public GameCharaFileInfo GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void SetListInfo(int _index, GameCharaFileInfo _info)
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
