using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIChara;
using GameLoadCharaFileSystem;
using Illusion.Component.UI;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class GroupListUI : MonoBehaviour
{
	[Serializable]
	private class GroupUIInfo
	{
		public Toggle tgl;

		public Image image;
	}

	[Serializable]
	public class MenuItemUI
	{
		public Button btn;

		public List<Text> texts = new List<Text>();
	}

	[SerializeField]
	private GroupUIInfo[] groupUIInfos = new GroupUIInfo[5];

	[SerializeField]
	private Text txtGropuName;

	[SerializeField]
	private MenuItemUI itemUIRelease;

	[SerializeField]
	private GameCharaFileScrollController listCtrl = new GameCharaFileScrollController();

	[SerializeField]
	private GroupCharaParameterUI parameterUI;

	[SerializeField]
	private GroupCharaSelectUI groupCharaSelectUI;

	[SerializeField]
	private SpriteChangeCtrl sccBasePanel;

	private List<GameCharaFileInfo> charaLists = new List<GameCharaFileInfo>();

	private readonly string strGroupText = "グループ ";

	public GameCharaFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		HomeSceneManager hm = Singleton<HomeSceneManager>.Instance;
		SaveData save = Singleton<Game>.Instance.saveData;
		foreach (var bt in groupUIInfos.Select((GroupUIInfo value, int index) => new { value, index }))
		{
			bt.value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
			{
				bt.value.image.enabled = !_isOn;
				if (save.selectGroup != bt.index && _isOn)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					save.selectGroup = bt.index;
					hm.SetSelectGroupText(save.selectGroup);
					groupCharaSelectUI.ListCtrl.SelectInfoClear();
					groupCharaSelectUI.ListCtrl.RefreshShown();
					ListCtrl.SelectInfoClear();
					Create();
				}
			});
			bt.value.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		}
		itemUIRelease.btn.OnClickAsObservable().Subscribe(delegate
		{
			Remove(listCtrl.selectInfo.info);
		});
		itemUIRelease.btn.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (itemUIRelease.btn.IsInteractable())
			{
				Utils.Sound.Play(SystemSE.sel);
				itemUIRelease.texts.ForEach(delegate(Text t)
				{
					t.color = Game.selectFontColor;
				});
			}
		});
		itemUIRelease.btn.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (itemUIRelease.btn.IsInteractable())
			{
				itemUIRelease.texts.ForEach(delegate(Text t)
				{
					t.color = Game.defaultFontColor;
				});
			}
		});
		groupUIInfos[save.selectGroup].tgl.SetIsOnWithoutCallback(isOn: true);
		groupUIInfos[save.selectGroup].image.enabled = false;
		hm.SetSelectGroupText(save.selectGroup);
		txtGropuName.text = strGroupText + (save.selectGroup + 1);
		AddList(charaLists, save.roomList[save.selectGroup]);
		listCtrl.onSelect = delegate(GameCharaFileInfo _info)
		{
			itemUIRelease.btn.interactable = true;
			parameterUI.SetParameter(_info);
			groupCharaSelectUI.ListCtrl.selectInfo?.rowData.row.tgl.SetIsOnWithoutCallback(isOn: false);
			groupCharaSelectUI.ListCtrl.SelectInfoClear();
		};
		listCtrl.onDeSelect = delegate
		{
			itemUIRelease.btn.interactable = false;
			parameterUI.InitParameter();
		};
		listCtrl.onDoubleClick = delegate(GameCharaFileInfo _info)
		{
			Remove(_info);
		};
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
		listCtrl.SelectInfoClear();
		base.enabled = true;
	}

	public static void AddList(List<GameCharaFileInfo> _list, List<string> _lstFileName)
	{
		(new string[1])[0] = "*.png";
		_ = Singleton<GameSystem>.Instance.UserUUID;
		CategoryKind cateKind = CategoryKind.Female;
		int num = 0;
		int i = 0;
		while (i < _lstFileName.Count)
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			StringBuilder stringBuilder = new StringBuilder(UserData.Path + "chara/female/");
			stringBuilder.Append(_lstFileName[i]).Append(".png");
			if (!chaFileControl.LoadCharaFile(stringBuilder.ToString()))
			{
				chaFileControl.GetLastErrorCode();
			}
			else if (chaFileControl.parameter.sex == 1)
			{
				string text = "";
				text = (Voice.infoTable.TryGetValue(chaFileControl.parameter2.personality, out var value) ? value.Personality : "不明");
				HashSet<int> hashSet = new HashSet<int>(from anon in Singleton<Game>.Instance.saveData.roomList.Select((List<string> name, int index) => new { name, index })
					where anon.name.Contains(_lstFileName[i])
					select anon.index + 1);
				if (hashSet.Count == 0)
				{
					hashSet.Add(0);
				}
				_list.Add(new GameCharaFileInfo
				{
					index = num++,
					name = chaFileControl.parameter.fullname,
					personality = text,
					voice = chaFileControl.parameter2.personality,
					hair = chaFileControl.custom.hair.kind,
					birthMonth = chaFileControl.parameter.birthMonth,
					birthDay = chaFileControl.parameter.birthDay,
					strBirthDay = chaFileControl.parameter.strBirthDay,
					sex = chaFileControl.parameter.sex,
					FullPath = stringBuilder.ToString(),
					FileName = _lstFileName[i],
					state = chaFileControl.gameinfo2.nowDrawState,
					trait = chaFileControl.parameter2.trait,
					hAttribute = chaFileControl.parameter2.hAttribute,
					resistH = chaFileControl.gameinfo2.resistH,
					resistPain = chaFileControl.gameinfo2.resistPain,
					resistAnal = chaFileControl.gameinfo2.resistAnal,
					broken = chaFileControl.gameinfo2.Broken,
					dependence = chaFileControl.gameinfo2.Dependence,
					usedItem = chaFileControl.gameinfo2.usedItem,
					lockNowState = chaFileControl.gameinfo2.lockNowState,
					lockBroken = chaFileControl.gameinfo2.lockBroken,
					lockDependence = chaFileControl.gameinfo2.lockDependence,
					hcount = chaFileControl.gameinfo2.hCount,
					lstFilter = hashSet,
					cateKind = cateKind,
					data_uuid = chaFileControl.dataID
				});
			}
			int num2 = i + 1;
			i = num2;
		}
	}

	public void Create()
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		txtGropuName.text = strGroupText + (saveData.selectGroup + 1);
		charaLists.Clear();
		AddList(charaLists, saveData.roomList[saveData.selectGroup]);
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
		sccBasePanel.ChangeValue(saveData.selectGroup);
	}

	private void Remove(GameCharaFileInfo _info)
	{
		Utils.Sound.Play(SystemSE.ok_s);
		SaveData saveData = Singleton<Game>.Instance.saveData;
		saveData.roomList[saveData.selectGroup].Remove(_info.FileName);
		GameCharaFileInfo gameCharaFileInfo = groupCharaSelectUI.ListCtrl.FindInfoByFileName(_info.FileName);
		if (gameCharaFileInfo != null)
		{
			gameCharaFileInfo.lstFilter.Remove(saveData.selectGroup + 1);
			if (gameCharaFileInfo.lstFilter.Count == 0)
			{
				gameCharaFileInfo.lstFilter.Add(0);
			}
		}
		Create();
		groupCharaSelectUI.ListCtrl.RefreshShown();
		listCtrl.SelectInfoClear();
	}
}
