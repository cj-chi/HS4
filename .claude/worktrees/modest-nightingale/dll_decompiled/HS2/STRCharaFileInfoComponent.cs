using System;
using System.IO;
using Illusion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

[DisallowMultipleComponent]
public class STRCharaFileInfoComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public STRCharaFileInfo info;

		public Toggle tgl;

		public RawImage imgThumb;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, bool _isSameGroupCheck, STRCharaFileInfo _info, Action<bool> _onClickAction)
	{
		RowInfo rowInfo = rows[_index];
		bool flag = _info != null;
		if (flag)
		{
			flag = (!_info.FullPath.IsNullOrEmpty() && File.Exists(_info.FullPath)) || _info.pngData != null;
		}
		rowInfo.tgl.gameObject.SetActiveIfDifferent(flag);
		rowInfo.tgl.onValueChanged.RemoveAllListeners();
		if (flag)
		{
			rowInfo.tgl.onValueChanged.AddListener(delegate(bool _isOn)
			{
				_onClickAction(_isOn);
			});
			rowInfo.tgl.SetIsOnWithoutCallback(isOn: false);
			if (rowInfo.imgThumb.texture != null)
			{
				UnityEngine.Object.Destroy(rowInfo.imgThumb.texture);
				rowInfo.imgThumb.texture = null;
			}
			if (_info.pngData != null || !_info.FullPath.IsNullOrEmpty())
			{
				rowInfo.imgThumb.texture = PngAssist.ChangeTextureFromByte(_info.pngData ?? PngFile.LoadPngBytes(_info.FullPath));
			}
			rowInfo.info = _info;
		}
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].tgl.SetIsOnWithoutCallback(_isOn);
	}

	public STRCharaFileInfo GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void SetListInfo(int _index, STRCharaFileInfo _info)
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
