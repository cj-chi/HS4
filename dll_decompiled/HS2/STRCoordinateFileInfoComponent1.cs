using System;
using System.IO;
using Illusion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

[DisallowMultipleComponent]
public class STRCoordinateFileInfoComponent1 : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public STRCoordinateFileInfo1 info;

		public Toggle tgl;

		public RawImage imgThumb;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, STRCoordinateFileInfo1 _info, Action<bool> _onClickAction, bool _isAllnteractable)
	{
		bool flag = _info != null;
		if (flag)
		{
			flag = !_info.FullPath.IsNullOrEmpty() && File.Exists(_info.FullPath);
		}
		rows[_index].tgl.gameObject.SetActiveIfDifferent(flag);
		rows[_index].tgl.onValueChanged.RemoveAllListeners();
		if (flag)
		{
			rows[_index].tgl.onValueChanged.AddListener(delegate(bool _isOn)
			{
				_onClickAction(_isOn);
			});
			rows[_index].tgl.SetIsOnWithoutCallback(isOn: false);
			rows[_index].tgl.interactable = _isAllnteractable;
			if (rows[_index].imgThumb.texture != null)
			{
				UnityEngine.Object.Destroy(rows[_index].imgThumb.texture);
				rows[_index].imgThumb.texture = null;
			}
			if (!_info.FullPath.IsNullOrEmpty())
			{
				rows[_index].imgThumb.texture = PngAssist.ChangeTextureFromByte(PngFile.LoadPngBytes(_info.FullPath));
			}
			_info.componetRowInfo = rows[_index];
			rows[_index].info = _info;
		}
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].tgl.SetIsOnWithoutCallback(_isOn);
	}

	public STRCoordinateFileInfo1 GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void SetListInfo(int _index, STRCoordinateFileInfo1 _info)
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
