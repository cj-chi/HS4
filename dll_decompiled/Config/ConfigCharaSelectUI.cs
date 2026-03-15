using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameLoadCharaFileSystem;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class ConfigCharaSelectUI : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public int id;

		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private Toggle tglSort;

	[SerializeField]
	private Image imgSort;

	[SerializeField]
	private Toggle tglSortKind;

	[SerializeField]
	private Image imgSortKind;

	[SerializeField]
	private GameCharaFileScrollController listCtrl = new GameCharaFileScrollController();

	[SerializeField]
	private MenuItemUI[] itemUISystems = new MenuItemUI[2];

	[SerializeField]
	private CanvasGroup cgCharaSelect;

	public Action<GameCharaFileInfo> onEntry;

	private List<GameCharaFileInfo> charaLists = new List<GameCharaFileInfo>();

	public GameCharaFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		tglSort.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
		{
			imgSort.enabled = !_isON;
			if (tglSortKind.isOn)
			{
				SortDate(_isON);
			}
			else
			{
				SortName(_isON);
			}
			ReDrawListView();
		});
		tglSortKind.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
		{
			imgSortKind.enabled = !_isON;
			if (_isON)
			{
				SortDate(tglSort.isOn);
			}
			else
			{
				SortName(tglSort.isOn);
			}
			ReDrawListView();
		});
		List<Toggle> list = new List<Toggle>();
		list.Add(tglSort);
		list.Add(tglSortKind);
		list.ForEach(delegate(Toggle tgl)
		{
			tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
			tgl.OnPointerClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_s);
			});
		});
		itemUISystems[0].btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			onEntry?.Invoke(listCtrl.selectInfo.info);
		});
		itemUISystems[1].btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			cgCharaSelect.Enable(enable: false, isUseInteractable: false);
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
		listCtrl.onSelect = delegate
		{
			itemUISystems[0].btn.interactable = true;
		};
		listCtrl.onDeSelect = delegate
		{
			itemUISystems[0].btn.interactable = false;
		};
		listCtrl.onDoubleClick = delegate(GameCharaFileInfo _info)
		{
			if (Input.GetMouseButtonUp(0))
			{
				onEntry?.Invoke(_info);
			}
		};
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
		listCtrl.SelectInfoClear();
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where !Scene.Overlaps.Any((Scene.IOverlap o) => o is ExitDialog || o is ConfirmDialog)
			where cgCharaSelect.alpha > 0.5f
			select _).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.cancel);
			cgCharaSelect.Enable(enable: false, isUseInteractable: false);
		});
		base.enabled = true;
	}

	private void SortDate(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
		}
		GameCharaFileInfo gameCharaFileInfo = charaLists.Find((GameCharaFileInfo i) => i.personality == "不明");
		if (gameCharaFileInfo != null)
		{
			charaLists.Remove(gameCharaFileInfo);
		}
		using (new GameSystem.CultureScope())
		{
			if (ascend)
			{
				charaLists = (from n in charaLists
					orderby n.time, n.name, n.personality
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.time descending, n.name descending, n.personality descending
					select n).ToList();
			}
		}
		if (gameCharaFileInfo != null)
		{
			charaLists.Insert(0, gameCharaFileInfo);
		}
	}

	private void SortName(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
		}
		GameCharaFileInfo gameCharaFileInfo = charaLists.Find((GameCharaFileInfo i) => i.personality == "不明");
		if (gameCharaFileInfo != null)
		{
			charaLists.Remove(gameCharaFileInfo);
		}
		using (new GameSystem.CultureScope())
		{
			if (ascend)
			{
				charaLists = (from n in charaLists
					orderby n.name, n.time, n.personality
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.name descending, n.time descending, n.personality descending
					select n).ToList();
			}
		}
		if (gameCharaFileInfo != null)
		{
			charaLists.Insert(0, gameCharaFileInfo);
		}
	}

	public void CreateList()
	{
		_ = Singleton<Game>.Instance.saveData;
		charaLists = GameCharaFileInfoAssist.CreateCharaFileInfoList(0, useMale: false, useFemale: true);
		SortDate(tglSort.isOn);
	}

	public void ReDrawListView()
	{
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
	}
}
