using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class CoordinateGroupListUI : MonoBehaviour
{
	public enum CreateKind
	{
		NoChange,
		SaveGroup,
		Appoint
	}

	[Serializable]
	private class GroupUIInfo
	{
		public Toggle tgl;

		public Image image;
	}

	[SerializeField]
	private GroupUIInfo[] groupUIInfos = new GroupUIInfo[5];

	[SerializeField]
	private Text txtGropuName;

	[SerializeField]
	private GameCharaFileScrollController listCtrl = new GameCharaFileScrollController();

	[SerializeField]
	private GroupCharaParameterUI parameterUI;

	[SerializeField]
	private CoordinateListUI coordinateListUI;

	[SerializeField]
	private SpriteChangeCtrl sccBasePanel;

	private List<GameCharaFileInfo> charaLists = new List<GameCharaFileInfo>();

	private int selectGroup;

	private readonly string strGroupText = "グループ ";

	public GameCharaFileScrollController ListCtrl => listCtrl;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		yield return new WaitUntil(() => Singleton<HomeSceneManager>.IsInstance());
		_ = Singleton<HomeSceneManager>.Instance;
		SaveData saveData = Singleton<Game>.Instance.saveData;
		foreach (var gi in groupUIInfos.Select((GroupUIInfo value, int index) => new { value, index }))
		{
			gi.value.tgl.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(bool _isOn)
			{
				gi.value.image.enabled = !_isOn;
				if (_isOn && selectGroup != gi.index)
				{
					Utils.Sound.Play(SystemSE.ok_s);
					selectGroup = gi.index;
					coordinateListUI.selectGroup = selectGroup;
					ListCtrl.SelectInfoClear();
					Create();
					coordinateListUI.ListSelectRelease();
				}
			});
			gi.value.tgl.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
			});
		}
		selectGroup = saveData.selectGroup;
		groupUIInfos[selectGroup].tgl.SetIsOnWithoutCallback(isOn: true);
		groupUIInfos[selectGroup].image.enabled = false;
		txtGropuName.text = strGroupText + (selectGroup + 1);
		GroupListUI.AddList(charaLists, saveData.roomList[selectGroup]);
		listCtrl.onSelect = delegate(GameCharaFileInfo _info)
		{
			parameterUI.SetParameter(_info);
			coordinateListUI.InitListSelect(selectGroup, _info.FileName);
			coordinateListUI.CG.interactable = true;
		};
		listCtrl.onDeSelect = delegate
		{
			parameterUI.InitParameter();
			coordinateListUI.ListSelectRelease();
			coordinateListUI.CG.interactable = false;
		};
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
		listCtrl.SelectInfoClear();
		base.enabled = true;
	}

	public void Create(CreateKind _createKind = CreateKind.NoChange, int _group = 0)
	{
		SaveData saveData = Singleton<Game>.Instance.saveData;
		switch (_createKind)
		{
		case CreateKind.SaveGroup:
			selectGroup = saveData.selectGroup;
			break;
		case CreateKind.Appoint:
			selectGroup = _group;
			break;
		}
		txtGropuName.text = strGroupText + (selectGroup + 1);
		charaLists.Clear();
		GroupListUI.AddList(charaLists, saveData.roomList[selectGroup]);
		listCtrl.SelectInfoClear();
		listCtrl.Init(charaLists, new List<int> { 0, 1, 2, 3, 4, 5 });
		groupUIInfos[selectGroup].tgl.SetIsOnWithoutCallback(isOn: true);
		sccBasePanel.ChangeValue(selectGroup);
	}
}
