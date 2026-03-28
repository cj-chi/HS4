using System;
using UnityEngine;
using UnityEngine.UI;

namespace UploaderSystem;

[DisallowMultipleComponent]
public class NetSelectHNScrollViewInfo : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public Toggle tglItem;

		public Text text;

		public NetworkInfo.SelectHNInfo info;
	}

	[SerializeField]
	private RowInfo row;

	public void SetData(NetworkInfo.SelectHNInfo _data, Action<bool> _onValueChange)
	{
		row.tglItem.onValueChanged.RemoveAllListeners();
		row.tglItem.onValueChanged.AddListener(delegate(bool _isOn)
		{
			_onValueChange(_isOn);
		});
		row.text.text = _data.drawname;
		row.info = _data;
	}

	public void SetToggleON(bool _isOn)
	{
		row.tglItem.isOn = _isOn;
	}

	public NetworkInfo.SelectHNInfo GetListInfo()
	{
		return row.info;
	}
}
