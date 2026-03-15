using System;
using System.Collections.Generic;
using Illusion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

[DisallowMultipleComponent]
public class CustomPushScrollViewInfo : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public Button btn;

		public Image imgThumb;

		public CustomPushInfo info;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, CustomPushInfo _info, Action _onClickAction, Action<string, int> _onPointerEnter, Action _onPointerExit)
	{
		bool flag = _info != null;
		rows[_index].btn.gameObject.SetActiveIfDifferent(flag);
		rows[_index].btn.onClick.RemoveAllListeners();
		if (flag)
		{
			rows[_index].btn.onClick.RemoveAllListeners();
			rows[_index].btn.onClick.AddListener(delegate
			{
				_onClickAction();
			});
			EventTriggerNoScroll eventTriggerNoScroll = rows[_index].btn.gameObject.AddComponent<EventTriggerNoScroll>();
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
			Texture2D texture2D = CommonLib.LoadAsset<Texture2D>(_info.assetBundle, _info.assetName);
			if ((bool)texture2D)
			{
				rows[_index].imgThumb.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			}
			else
			{
				rows[_index].imgThumb.sprite = null;
			}
			rows[_index].info = _info;
		}
	}

	public CustomPushInfo GetListInfo(int _index)
	{
		return rows[_index].info;
	}
}
