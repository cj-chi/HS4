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

public class PlayerCoordinateListUI : MonoBehaviour
{
	[SerializeField]
	private bool isMainPlayer = true;

	[SerializeField]
	private Toggle tglSort;

	[SerializeField]
	private Image imgSort;

	[SerializeField]
	private Toggle tglSortKind;

	[SerializeField]
	private Image imgSortKind;

	[SerializeField]
	private GameObject objParameter;

	[SerializeField]
	private CoordinateFileScrollController listCtrl = new CoordinateFileScrollController();

	private List<CoordinateFileInfo> charaLists = new List<CoordinateFileInfo>();

	public CoordinateFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
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
		List<Toggle> list = new List<Toggle>();
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
		int index = ((!isMainPlayer) ? 1 : 0);
		CoordinateFileInfo coordinateFileInfo = listCtrl.FindInfoByFileName(saveData.playerCloths[index].file);
		if (coordinateFileInfo != null)
		{
			coordinateFileInfo.isBath = false;
			coordinateFileInfo.componetRowInfo.imgBath.gameObject.SetActiveIfDifferent(active: false);
		}
		saveData.playerCloths[index].file = _info.FileName;
		saveData.playerCloths[index].sex = (isMainPlayer ? saveData.playerChara.Sex : saveData.secondPlayerChara.Sex);
		if (_info.componetRowInfo != null)
		{
			_info.isBath = true;
			_info.componetRowInfo.imgBath.gameObject.SetActiveIfDifferent(active: true);
		}
	}

	private void Relelase(CoordinateFileInfo _info)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		int index = ((!isMainPlayer) ? 1 : 0);
		saveData.playerCloths[index].file = string.Empty;
		saveData.playerCloths[index].sex = 0;
		if (_info.componetRowInfo != null)
		{
			_info.isBath = false;
			_info.componetRowInfo.imgBath.gameObject.SetActiveIfDifferent(active: false);
		}
	}

	public void CreateList()
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		int num = 0;
		num = ((!isMainPlayer) ? saveData.secondPlayerChara.Sex : saveData.playerChara.Sex);
		charaLists = CoordinateFileInfoAssist.CreateCharaFileInfoList(num);
		SortDate(tglSort.isOn);
	}

	public void ReDrawListView()
	{
		listCtrl.Init(charaLists);
	}

	public void InitListSelect()
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		int index = ((!isMainPlayer) ? 1 : 0);
		string file = saveData.playerCloths[index].file;
		bool flag = SaveData.IsAchievementExchangeRelease(5);
		if (!isMainPlayer)
		{
			objParameter.SetActiveIfDifferent(flag);
			base.gameObject.SetActiveIfDifferent(flag);
		}
		if (isMainPlayer || (!isMainPlayer && flag && !saveData.secondPlayerChara.FileName.IsNullOrEmpty()))
		{
			CreateList();
		}
		else
		{
			charaLists.Clear();
		}
		for (int i = 0; i < charaLists.Count; i++)
		{
			CoordinateFileInfo coordinateFileInfo = charaLists[i];
			coordinateFileInfo.isBath = coordinateFileInfo.FileName == file;
		}
		listCtrl.SelectInfoClear();
		listCtrl.Init(charaLists);
	}

	public void ListSelectRelease()
	{
		listCtrl.SelectInfoClear();
		listCtrl.Init(charaLists);
	}
}
