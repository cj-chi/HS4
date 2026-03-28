using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoordinateFileSystem;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class CoordinateListUI : MonoBehaviour
{
	[Serializable]
	public class MenuItemUI
	{
		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private Toggle[] tglFilters = new Toggle[2];

	[SerializeField]
	private Toggle tglSort;

	[SerializeField]
	private Image imgSort;

	[SerializeField]
	private Toggle tglSortKind;

	[SerializeField]
	private Image imgSortKind;

	[SerializeField]
	private CoordinateFileScrollController listCtrl = new CoordinateFileScrollController();

	[SerializeField]
	private MenuItemUI itemUISystem = new MenuItemUI();

	[SerializeField]
	private CanvasGroup canvasGroup;

	private List<CoordinateFileInfo> charaLists = new List<CoordinateFileInfo>();

	private int selectClothKind;

	public CoordinateFileScrollController ListCtrl => listCtrl;

	public CanvasGroup CG => canvasGroup;

	public int selectGroup { get; set; }

	public string selectChara { get; set; } = string.Empty;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		foreach (var value in tglFilters.Select((Toggle tgl, int index) => new { tgl, index }))
		{
			value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
			{
				if (_isON)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					selectClothKind = value.index;
					SaveData saveData = Singleton<Game>.Instance.saveData;
					if (saveData.dicCloths[selectGroup].ContainsKey(selectChara))
					{
						ClothPngInfo clothPngInfo = saveData.dicCloths[selectGroup][selectChara];
						if (selectClothKind == 0)
						{
							listCtrl.SetToggle(clothPngInfo.bathFile);
						}
						else
						{
							listCtrl.SetToggle(clothPngInfo.roomWearFile);
						}
					}
				}
			});
		}
		tglSort.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isON)
		{
			Utils.Sound.Play(SystemSE.ok_s);
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
			Utils.Sound.Play(SystemSE.ok_s);
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
		List<Toggle> list = new List<Toggle>(tglFilters.ToList());
		list.Add(tglSort);
		list.Add(tglSortKind);
		list.ForEach(delegate(Toggle tgl)
		{
			tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				if (tgl.IsInteractable())
				{
					Utils.Sound.Play(SystemSE.sel);
				}
			});
		});
		itemUISystem.btn.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			AllRelelase();
		});
		itemUISystem.btn.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (itemUISystem.btn.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				itemUISystem.texts.ForEach(delegate(Text t)
				{
					t.color = Game.selectFontColor;
				});
			}
		});
		itemUISystem.btn.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (itemUISystem.btn.IsInteractable())
			{
				itemUISystem.texts.ForEach(delegate(Text t)
				{
					t.color = Game.defaultFontColor;
				});
			}
		});
		CreateList();
		listCtrl.onSelect = delegate(CoordinateFileInfo _info)
		{
			Entry(_info);
		};
		listCtrl.onDeSelect = delegate(CoordinateFileInfo _info)
		{
			if (_info != null)
			{
				Relelase(_info);
			}
		};
		listCtrl.Init(charaLists);
		listCtrl.SelectInfoClear();
		canvasGroup.interactable = false;
		base.enabled = true;
	}

	private void SortDate(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
		}
		using (new GameSystem.CultureScope())
		{
			if (ascend)
			{
				charaLists = (from n in charaLists
					orderby n.time, n.name
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.time descending, n.name descending
					select n).ToList();
			}
		}
	}

	private void SortName(bool ascend)
	{
		if (charaLists.Count == 0)
		{
			return;
		}
		using (new GameSystem.CultureScope())
		{
			if (ascend)
			{
				charaLists = (from n in charaLists
					orderby n.name, n.time
					select n).ToList();
			}
			else
			{
				charaLists = (from n in charaLists
					orderby n.name descending, n.time descending
					select n).ToList();
			}
		}
	}

	private void Entry(CoordinateFileInfo _info)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (!saveData.dicCloths[selectGroup].ContainsKey(selectChara))
		{
			return;
		}
		ClothPngInfo clothPngInfo = saveData.dicCloths[selectGroup][selectChara];
		if (selectClothKind == 0)
		{
			CoordinateFileInfo coordinateFileInfo = listCtrl.FindInfoByFileName(clothPngInfo.bathFile);
			if (coordinateFileInfo != null)
			{
				coordinateFileInfo.isBath = false;
				coordinateFileInfo.componetRowInfo.imgBath.gameObject.SetActiveIfDifferent(active: false);
			}
			clothPngInfo.bathFile = _info.FileName;
			if (_info.componetRowInfo != null)
			{
				_info.isBath = true;
				_info.componetRowInfo.imgBath.gameObject.SetActiveIfDifferent(active: true);
			}
		}
		else
		{
			CoordinateFileInfo coordinateFileInfo2 = listCtrl.FindInfoByFileName(clothPngInfo.roomWearFile);
			if (coordinateFileInfo2 != null)
			{
				coordinateFileInfo2.isRoomWear = false;
				coordinateFileInfo2.componetRowInfo.imgRoomWear.gameObject.SetActiveIfDifferent(active: false);
			}
			clothPngInfo.roomWearFile = _info.FileName;
			if (_info.componetRowInfo != null)
			{
				_info.isRoomWear = true;
				_info.componetRowInfo.imgRoomWear.gameObject.SetActiveIfDifferent(active: true);
			}
		}
	}

	private void Relelase(CoordinateFileInfo _info)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (!saveData.dicCloths[selectGroup].ContainsKey(selectChara))
		{
			return;
		}
		ClothPngInfo clothPngInfo = saveData.dicCloths[selectGroup][selectChara];
		if (selectClothKind == 0)
		{
			clothPngInfo.bathFile = string.Empty;
		}
		else
		{
			clothPngInfo.roomWearFile = string.Empty;
		}
		if (_info.componetRowInfo != null)
		{
			if (selectClothKind == 0)
			{
				_info.isBath = false;
				_info.componetRowInfo.imgBath.gameObject.SetActiveIfDifferent(active: false);
			}
			else
			{
				_info.isRoomWear = false;
				_info.componetRowInfo.imgRoomWear.gameObject.SetActiveIfDifferent(active: false);
			}
		}
	}

	private void AllRelelase()
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		if (saveData.dicCloths[selectGroup].ContainsKey(selectChara))
		{
			ClothPngInfo clothPngInfo = saveData.dicCloths[selectGroup][selectChara];
			CoordinateFileInfo coordinateFileInfo = listCtrl.FindInfoByFileName(clothPngInfo.bathFile);
			if (coordinateFileInfo != null)
			{
				coordinateFileInfo.isBath = false;
				coordinateFileInfo.componetRowInfo.imgBath.gameObject.SetActiveIfDifferent(active: false);
			}
			coordinateFileInfo = listCtrl.FindInfoByFileName(clothPngInfo.roomWearFile);
			if (coordinateFileInfo != null)
			{
				coordinateFileInfo.isRoomWear = false;
				coordinateFileInfo.componetRowInfo.imgRoomWear.gameObject.SetActiveIfDifferent(active: false);
			}
			clothPngInfo.bathFile = string.Empty;
			clothPngInfo.roomWearFile = string.Empty;
		}
	}

	public void CreateList()
	{
		charaLists = CoordinateFileInfoAssist.CreateCharaFileInfoList();
		SortDate(tglSort.isOn);
	}

	public void ReDrawListView()
	{
		listCtrl.Init(charaLists);
	}

	public void InitListSelect(int _selectGroup, string _fileName)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		selectChara = _fileName;
		if (!saveData.dicCloths[_selectGroup].ContainsKey(_fileName))
		{
			ListSelectRelease();
			return;
		}
		ClothPngInfo clothPngInfo = saveData.dicCloths[_selectGroup][_fileName];
		for (int i = 0; i < charaLists.Count; i++)
		{
			CoordinateFileInfo coordinateFileInfo = charaLists[i];
			coordinateFileInfo.isBath = coordinateFileInfo.FileName == clothPngInfo.bathFile;
			coordinateFileInfo.isRoomWear = coordinateFileInfo.FileName == clothPngInfo.roomWearFile;
		}
		listCtrl.SelectInfoClear();
		listCtrl.Init(charaLists);
	}

	public void ListSelectRelease()
	{
		for (int i = 0; i < charaLists.Count; i++)
		{
			CoordinateFileInfo coordinateFileInfo = charaLists[i];
			coordinateFileInfo.isBath = false;
			coordinateFileInfo.isRoomWear = false;
		}
		selectChara = string.Empty;
		listCtrl.SelectInfoClear();
		listCtrl.Init(charaLists);
	}
}
