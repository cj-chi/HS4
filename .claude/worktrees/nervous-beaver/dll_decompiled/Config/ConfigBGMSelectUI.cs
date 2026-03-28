using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class ConfigBGMSelectUI : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public int id;

		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private ConfigBGMChangeScrollController listCtrl = new ConfigBGMChangeScrollController();

	[SerializeField]
	private MenuItemUI[] itemUISystems = new MenuItemUI[2];

	[SerializeField]
	private CanvasGroup cgBGMSelect;

	public Action<BGMNameInfo.Param> onEntry;

	private List<BGMNameInfo.Param> charaLists = new List<BGMNameInfo.Param>();

	public ConfigBGMChangeScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<Game>.IsInstance());
		itemUISystems[0].btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			onEntry?.Invoke(listCtrl.selectInfo.info);
		});
		itemUISystems[1].btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			cgBGMSelect.Enable(enable: false, isUseInteractable: false);
		});
		MenuItemUI[] array = itemUISystems;
		foreach (MenuItemUI ui in array)
		{
			ui.btn.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (ui.btn.IsInteractable())
				{
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.selectFontColor;
					});
				}
			});
			ui.btn.OnPointerExitAsObservable().Subscribe(delegate
			{
				if (ui.btn.IsInteractable())
				{
					ui.texts.ForEach(delegate(Text t)
					{
						t.color = Game.defaultFontColor;
					});
				}
			});
		}
		itemUISystems.ToList().ForEach(delegate(MenuItemUI menuItemUI)
		{
			menuItemUI.btn.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		});
		CreateList();
		listCtrl.Init(charaLists);
		listCtrl.SelectInfoClear();
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where cgBGMSelect.alpha > 0.5f
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			cgBGMSelect.Enable(enable: false, isUseInteractable: false);
		});
		base.enabled = true;
	}

	public void CreateList()
	{
		_ = Singleton<Game>.Instance.saveData;
		charaLists = Game.infoBGMNameTable.Values.ToList();
	}

	public void ReDrawListView()
	{
		listCtrl.Init(charaLists);
	}
}
