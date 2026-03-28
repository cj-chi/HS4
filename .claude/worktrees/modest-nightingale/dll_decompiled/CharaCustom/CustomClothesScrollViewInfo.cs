using System;
using System.Collections.Generic;
using Illusion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

[DisallowMultipleComponent]
public class CustomClothesScrollViewInfo : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public Toggle tgl;

		public RawImage imgThumb;

		public CustomClothesFileInfo info;
	}

	[SerializeField]
	private RowInfo[] rows;

	[SerializeField]
	private Texture2D texEmpty;

	public void SetData(int _index, CustomClothesScrollController.ScrollData _data, Action<bool> _onClickAction, Action<string> _onPointerEnter, Action _onPointerExit)
	{
		CustomClothesFileInfo _info = _data?.info;
		bool flag = _info != null;
		rows[_index].tgl.gameObject.SetActiveIfDifferent(flag);
		rows[_index].tgl.onValueChanged.RemoveAllListeners();
		if (!flag)
		{
			return;
		}
		rows[_index].tgl.onValueChanged.RemoveAllListeners();
		rows[_index].tgl.onValueChanged.AddListener(delegate(bool _isOn)
		{
			_onClickAction(_isOn);
		});
		EventTriggerNoScroll eventTriggerNoScroll = rows[_index].tgl.gameObject.AddComponent<EventTriggerNoScroll>();
		eventTriggerNoScroll.triggers = new List<EventTriggerNoScroll.Entry>();
		EventTriggerNoScroll.Entry entry = new EventTriggerNoScroll.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener(delegate
		{
			_onPointerEnter(_info.name);
		});
		eventTriggerNoScroll.triggers.Add(entry);
		entry = new EventTriggerNoScroll.Entry();
		entry.eventID = EventTriggerType.PointerExit;
		entry.callback.AddListener(delegate
		{
			_onPointerExit();
		});
		eventTriggerNoScroll.triggers.Add(entry);
		rows[_index].tgl.SetIsOnWithoutCallback(isOn: false);
		if (_info.pngData != null || !_info.FullPath.IsNullOrEmpty())
		{
			if ((bool)rows[_index].imgThumb.texture)
			{
				UnityEngine.Object.Destroy(rows[_index].imgThumb.texture);
			}
			rows[_index].imgThumb.texture = PngAssist.ChangeTextureFromByte(_info.pngData ?? PngFile.LoadPngBytes(_info.FullPath));
		}
		else
		{
			rows[_index].imgThumb.texture = texEmpty;
		}
		rows[_index].info = _info;
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].tgl.SetIsOnWithoutCallback(_isOn);
	}

	public CustomClothesFileInfo GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void Disable(bool disable)
	{
	}

	public void Disvisible(bool disvisible)
	{
	}
}
