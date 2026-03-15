using System;
using System.Collections.Generic;
using Illusion.Extensions;
using Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

[DisallowMultipleComponent]
public class CustomSelectScrollViewInfo : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public Toggle tgl;

		public Image imgThumb;

		public Image imgNew;

		public CustomSelectInfo info;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, CustomSelectScrollController.ScrollData _data, Action<bool> _onClickAction, Action<string, int> _onPointerEnter, Action _onPointerExit)
	{
		CustomSelectInfo _info = _data?.info;
		bool flag = _info != null;
		rows[_index].tgl.gameObject.SetActiveIfDifferent(flag);
		rows[_index].tgl.onValueChanged.RemoveAllListeners();
		if (flag)
		{
			_data.toggle = rows[_index].tgl;
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
				_onPointerEnter(_info.name, _info.fontSize);
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
			Texture2D texture2D = CommonLib.LoadAsset<Texture2D>(_info.assetBundle, _info.assetName);
			if ((bool)texture2D)
			{
				rows[_index].imgThumb.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			}
			else
			{
				rows[_index].imgThumb.sprite = null;
			}
			if ((bool)rows[_index].imgNew)
			{
				rows[_index].imgNew.gameObject.SetActiveIfDifferent(_info.newItem);
			}
			rows[_index].info = _info;
		}
	}

	public void SetToggleON(int _index, bool _isOn)
	{
		rows[_index].tgl.SetIsOnWithoutCallback(_isOn);
	}

	public void SetNewFlagOff(int _index)
	{
		rows[_index].imgNew.gameObject.SetActiveIfDifferent(active: false);
		rows[_index].info.newItem = false;
		Singleton<Character>.Instance.chaListCtrl.AddItemID(rows[_index].info.category, rows[_index].info.id, 2);
	}

	public CustomSelectInfo GetListInfo(int _index)
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
