using System;
using System.IO;
using Illusion.Extensions;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

namespace CoordinateFileSystem;

[DisallowMultipleComponent]
public class CoordinateFileInfoComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public CoordinateFileInfo info;

		public Toggle tgl;

		public RawImage imgThumb;

		public PointerEnterExitAction pointerAction;

		public Image imgBath;

		public Image imgRoomWear;

		public GameObject objEntry;
	}

	[SerializeField]
	private RowInfo[] rows;

	[SerializeField]
	private Texture2D texEmpty;

	public void SetData(int _index, CoordinateFileInfo _info, Action<bool> _onClickAction)
	{
		bool flag = _info != null;
		if (flag)
		{
			flag = !_info.FullPath.IsNullOrEmpty() && File.Exists(_info.FullPath);
		}
		rows[_index].tgl.gameObject.SetActiveIfDifferent(flag);
		rows[_index].tgl.onValueChanged.RemoveAllListeners();
		if (!flag)
		{
			return;
		}
		rows[_index].tgl.onValueChanged.AddListener(delegate(bool _isOn)
		{
			_onClickAction(_isOn);
		});
		rows[_index].tgl.SetIsOnWithoutCallback(isOn: false);
		if (rows[_index].imgThumb.texture != null && rows[_index].imgThumb.texture != texEmpty)
		{
			UnityEngine.Object.Destroy(rows[_index].imgThumb.texture);
			rows[_index].imgThumb.texture = null;
		}
		if (!_info.FullPath.IsNullOrEmpty())
		{
			rows[_index].imgThumb.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.FullPath));
		}
		else
		{
			rows[_index].imgThumb.texture = texEmpty;
		}
		rows[_index].imgBath?.gameObject.SetActiveIfDifferent(_info.isBath);
		rows[_index].imgRoomWear?.gameObject.SetActiveIfDifferent(_info.isRoomWear);
		int sel = _index;
		if ((bool)rows[_index].pointerAction)
		{
			rows[_index].pointerAction.listActionEnter.Clear();
			rows[_index].pointerAction.listActionEnter.Add(delegate
			{
				_ = rows[sel].tgl.interactable;
			});
		}
		_info.componetRowInfo = rows[_index];
		rows[_index].info = _info;
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].tgl.SetIsOnWithoutCallback(_isOn);
	}

	public CoordinateFileInfo GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void SetListInfo(int _index, CoordinateFileInfo _info)
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
