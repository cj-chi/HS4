using System;
using System.Collections.Generic;
using System.IO;
using AIChara;
using CharaCustom;
using Manager;
using SceneAssist;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HSceneSpriteCoordinatesCard : MonoBehaviour
{
	private class ListComparer : IComparer<HSceneSpriteCoordinatesNode>
	{
		public int nCompare;

		public bool bAscending;

		public int Compare(HSceneSpriteCoordinatesNode a, HSceneSpriteCoordinatesNode b)
		{
			switch (nCompare)
			{
			case 0:
				if (bAscending)
				{
					return SortCompare(a.coodeName.text, b.coodeName.text);
				}
				return SortCompare(b.coodeName.text, a.coodeName.text);
			case 1:
				if (bAscending)
				{
					return SortCompare(a.CreateCoodeTime, b.CreateCoodeTime);
				}
				return SortCompare(b.CreateCoodeTime, a.CreateCoodeTime);
			default:
				return 0;
			}
		}

		private int SortCompare<T>(T a, T b) where T : IComparable
		{
			return a.CompareTo(b);
		}
	}

	[SerializeField]
	private HSceneSpriteChaChoice hSceneSpriteChaChoice;

	[SerializeField]
	private HSceneSpriteClothCondition hSceneSpriteCloth;

	[SerializeField]
	private RawImage CardImage;

	private Texture CardImageDef;

	[SerializeField]
	private Text SelectedLabel;

	private string filename;

	[SerializeField]
	private Button[] Sort;

	[SerializeField]
	private Button[] SortUpDown;

	[SerializeField]
	private HSceneSpriteCoordinatesNode CoordinatesNode;

	[SerializeField]
	private Transform Content;

	private List<HSceneSpriteCoordinatesNode> lstCoordinates = new List<HSceneSpriteCoordinatesNode>();

	private List<CustomClothesFileInfo> lstCoordinatesBase = new List<CustomClothesFileInfo>();

	[SerializeField]
	private Button BeforeCoode;

	[SerializeField]
	private Button DecideCoode;

	private int sortKind;

	private bool Ascending;

	private HScene hScene;

	private HSceneManager hSceneManager;

	private ChaControl[] femailes;

	private ChaControl[] mailes;

	private IntReactiveProperty SelectedID = new IntReactiveProperty(-1);

	private IntReactiveProperty targetSex = new IntReactiveProperty(1);

	private List<CustomClothesFileInfo>[] CoordinatesBasees = new List<CustomClothesFileInfo>[2]
	{
		new List<CustomClothesFileInfo>(),
		new List<CustomClothesFileInfo>()
	};

	private bool Bath;

	private bool Room;

	private int eventNo = -1;

	private int _SelectedID
	{
		get
		{
			return SelectedID.Value;
		}
		set
		{
			SelectedID.Value = value;
		}
	}

	public int TargetSex
	{
		get
		{
			return targetSex.Value;
		}
		set
		{
			targetSex.Value = value;
		}
	}

	private void Start()
	{
		CardImageDef = CardImage.texture;
		SelectedID.Where((int x) => x >= 0 && x < lstCoordinates.Count).Subscribe(delegate(int x)
		{
			for (int i = 0; i < lstCoordinates.Count; i++)
			{
				if (lstCoordinates[i].id == x)
				{
					SelectedLabel.text = lstCoordinates[i].coodeName.text;
					filename = lstCoordinates[i].fileName;
					CardImage.texture = PngAssist.ChangeTextureFromByte((lstCoordinatesBase[x].pngData != null) ? lstCoordinatesBase[x].pngData : PngFile.LoadPngBytes(lstCoordinatesBase[x].FullPath));
				}
			}
			if (!CardImage.gameObject.activeSelf)
			{
				CardImage.gameObject.SetActive(value: true);
			}
			for (int j = 0; j < lstCoordinates.Count; j++)
			{
				if (lstCoordinates[j].id != x)
				{
					lstCoordinates[j].coodeName.color = Game.defaultFontColor;
				}
				else
				{
					lstCoordinates[j].coodeName.color = Game.selectFontColor;
				}
			}
		});
		targetSex.Where((int _) => hSceneManager != null).Subscribe(delegate(int sex)
		{
			ChangeTargetSex(sex);
		});
	}

	public void Init()
	{
		hScene = Singleton<HSceneFlagCtrl>.Instance.GetComponent<HScene>();
		femailes = hScene.GetFemales();
		mailes = hScene.GetMales();
		hSceneManager = Singleton<HSceneManager>.Instance;
		Sort[0].onClick.AddListener(delegate
		{
			Sort[0].gameObject.SetActive(value: false);
			Sort[1].gameObject.SetActive(value: true);
			sortKind = 0;
			ListSort(sortKind);
		});
		Sort[1].onClick.AddListener(delegate
		{
			Sort[0].gameObject.SetActive(value: true);
			Sort[1].gameObject.SetActive(value: false);
			sortKind = 1;
			ListSort(sortKind);
		});
		SortUpDown[0].onClick.AddListener(delegate
		{
			SortUpDown[0].gameObject.SetActive(value: false);
			SortUpDown[1].gameObject.SetActive(value: true);
			ListSortUpDown(1);
		});
		SortUpDown[1].onClick.AddListener(delegate
		{
			SortUpDown[0].gameObject.SetActive(value: true);
			SortUpDown[1].gameObject.SetActive(value: false);
			ListSortUpDown(0);
		});
		CoordinatesBasees[0] = CustomClothesFileInfoAssist.CreateClothesFileInfoList(useMale: true, useFemale: false);
		CoordinatesBasees[1] = CustomClothesFileInfoAssist.CreateClothesFileInfoList(useMale: false, useFemale: true);
		lstCoordinatesBase = CoordinatesBasees[1];
		lstCoordinates.Clear();
		for (int num = 0; num < lstCoordinatesBase.Count; num++)
		{
			int no = num;
			HSceneSpriteCoordinatesNode hSceneSpriteCoordinatesNode = UnityEngine.Object.Instantiate(CoordinatesNode, Content);
			hSceneSpriteCoordinatesNode.gameObject.SetActive(value: true);
			lstCoordinates.Add(hSceneSpriteCoordinatesNode);
			lstCoordinates[no].id = no;
			lstCoordinates[no].coodeName.text = lstCoordinatesBase[no].name;
			lstCoordinates[no].coodeName.color = Game.defaultFontColor;
			lstCoordinates[no].CreateCoodeTime = lstCoordinatesBase[no].time;
			lstCoordinates[no].GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool val)
			{
				if (val)
				{
					_SelectedID = no;
					lstCoordinates[no].coodeName.color = Game.defaultFontColor;
				}
				else
				{
					lstCoordinates[no].coodeName.color = Game.selectFontColor;
				}
			});
			lstCoordinates[no].image = lstCoordinates[no].GetComponent<Image>();
			lstCoordinates[no].fileName = lstCoordinatesBase[no].FullPath;
		}
		ListSort(1);
		ListSortUpDown(1);
		Bath = hSceneManager.mapID == 4 || hSceneManager.mapID == 52 || hSceneManager.mapID == 53;
		Room = hSceneManager.mapID == 3;
		eventNo = Singleton<Game>.Instance.eventNo;
		BeforeCoode.onClick.AddListener(delegate
		{
			ChangeDefCoode();
		});
		DecideCoode.onClick.AddListener(delegate
		{
			ChaControl chaControl = ((hSceneManager.numFemaleClothCustom < 2) ? femailes[hSceneManager.numFemaleClothCustom] : mailes[hSceneManager.numFemaleClothCustom - 2]);
			if (SelectedID.Value != -1 && !filename.IsNullOrEmpty())
			{
				chaControl.ChangeNowCoordinate(filename, reload: true);
				hSceneSpriteCloth.SetClothCharacter();
			}
		});
		hSceneSpriteChaChoice.SetAction(delegate
		{
			SetCoordinatesCharacter();
		});
	}

	public void SetDownAction(UnityAction action)
	{
		PointerDownAction[] componentsInChildren = GetComponentsInChildren<PointerDownAction>(includeInactive: true);
		foreach (PointerDownAction pointerDownAction in componentsInChildren)
		{
			if (!(pointerDownAction == null) && !pointerDownAction.listAction.Contains(action))
			{
				pointerDownAction.listAction.Add(action);
			}
		}
	}

	private void SetCoordinatesCharacter()
	{
		if (hSceneManager.numFemaleClothCustom < 2)
		{
			TargetSex = 1;
		}
		else
		{
			TargetSex = mailes[hSceneManager.numFemaleClothCustom - 2].sex;
		}
	}

	public void ListSort(int sortkind)
	{
		ListComparer listComparer = new ListComparer();
		sortKind = sortkind;
		listComparer.nCompare = sortkind;
		listComparer.bAscending = Ascending;
		lstCoordinates.Sort(listComparer);
		for (int i = 0; i < lstCoordinates.Count; i++)
		{
			int siblingIndex = i;
			lstCoordinates[i].transform.SetSiblingIndex(siblingIndex);
		}
	}

	public void ListSortUpDown(int Ascending)
	{
		ListComparer listComparer = new ListComparer();
		this.Ascending = Ascending == 0;
		listComparer.nCompare = sortKind;
		listComparer.bAscending = this.Ascending;
		lstCoordinates.Sort(listComparer);
		for (int i = 0; i < lstCoordinates.Count; i++)
		{
			int siblingIndex = i;
			lstCoordinates[i].transform.SetSiblingIndex(siblingIndex);
		}
	}

	public void EndProc()
	{
		for (int i = 0; i < lstCoordinates.Count; i++)
		{
			UnityEngine.Object.Destroy(lstCoordinates[i].gameObject);
		}
		lstCoordinates.Clear();
		SortUpDown[0].onClick.RemoveAllListeners();
		SortUpDown[1].onClick.RemoveAllListeners();
		BeforeCoode.onClick.RemoveAllListeners();
		DecideCoode.onClick.RemoveAllListeners();
	}

	private void ChangeTargetSex(int sex)
	{
		for (int i = 0; i < lstCoordinates.Count; i++)
		{
			UnityEngine.Object.Destroy(lstCoordinates[i].gameObject);
			lstCoordinates[i] = null;
		}
		lstCoordinates.Clear();
		ChaControl chaControl = ((hSceneManager.numFemaleClothCustom < 2) ? femailes[hSceneManager.numFemaleClothCustom] : mailes[hSceneManager.numFemaleClothCustom - 2]);
		lstCoordinatesBase = CoordinatesBasees[chaControl.sex];
		for (int j = 0; j < lstCoordinatesBase.Count; j++)
		{
			int no = j;
			HSceneSpriteCoordinatesNode hSceneSpriteCoordinatesNode = UnityEngine.Object.Instantiate(CoordinatesNode, Content);
			hSceneSpriteCoordinatesNode.gameObject.SetActive(value: true);
			lstCoordinates.Add(hSceneSpriteCoordinatesNode);
			lstCoordinates[no].id = no;
			lstCoordinates[no].coodeName.text = lstCoordinatesBase[no].name;
			lstCoordinates[no].coodeName.color = Game.defaultFontColor;
			lstCoordinates[no].CreateCoodeTime = lstCoordinatesBase[no].time;
			Toggle component = lstCoordinates[no].GetComponent<Toggle>();
			component.onValueChanged.RemoveAllListeners();
			component.onValueChanged.AddListener(delegate(bool val)
			{
				if (val)
				{
					_SelectedID = no;
					lstCoordinates[no].coodeName.color = Game.defaultFontColor;
				}
				else
				{
					lstCoordinates[no].coodeName.color = Game.selectFontColor;
				}
			});
			lstCoordinates[no].image = lstCoordinates[no].GetComponent<Image>();
			lstCoordinates[no].fileName = lstCoordinatesBase[no].FullPath;
		}
		ListSort(sortKind);
		ListSortUpDown((!Ascending) ? 1 : 0);
		_SelectedID = -1;
		SelectedLabel.text = "";
		filename = "";
		CardImage.texture = CardImageDef;
	}

	private void ChangeDefCoode()
	{
		ChaControl chaControl = ((hSceneManager.numFemaleClothCustom < 2) ? femailes[hSceneManager.numFemaleClothCustom] : mailes[hSceneManager.numFemaleClothCustom - 2]);
		bool flag = true;
		if (hSceneManager.numFemaleClothCustom < 2)
		{
			if (chaControl.chaID != -1 && chaControl.chaID != -2)
			{
				if (Bath || Room)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(chaControl.chaFile.charaFileName);
					if (fileNameWithoutExtension != null)
					{
						Dictionary<string, ClothPngInfo> dictionary = Singleton<Game>.Instance.saveData.dicCloths[Singleton<Game>.Instance.saveData.selectGroup];
						if (!dictionary.ContainsKey(fileNameWithoutExtension) || (Bath ? dictionary[fileNameWithoutExtension].bathFile.IsNullOrEmpty() : dictionary[fileNameWithoutExtension].roomWearFile.IsNullOrEmpty()))
						{
							string assetName = (Bath ? "bath" : "roomwear");
							TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(AssetBundleNames.CustomCustom_Etc, assetName);
							if (textAsset != null)
							{
								chaControl.nowCoordinate.LoadFile(textAsset);
								chaControl.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
								AssetBundleManager.UnloadAssetBundle(AssetBundleNames.CustomCustom_Etc, isUnloadForceRefCount: true);
								flag = false;
							}
						}
						else
						{
							ClothPngInfo clothPngInfo = Singleton<Game>.Instance.saveData.dicCloths[Singleton<Game>.Instance.saveData.selectGroup][fileNameWithoutExtension];
							chaControl.ChangeNowCoordinate(Bath ? clothPngInfo.bathFile : clothPngInfo.roomWearFile, reload: true);
							flag = false;
						}
					}
				}
				if (eventNo == 50 || eventNo == 51 || eventNo == 52 || eventNo == 53 || eventNo == 54 || eventNo == 55)
				{
					string appendCoordinateFemale = Singleton<Game>.Instance.appendCoordinateFemale;
					if (!appendCoordinateFemale.IsNullOrEmpty())
					{
						chaControl.ChangeNowCoordinate(appendCoordinateFemale, reload: true);
						flag = false;
					}
				}
			}
		}
		else
		{
			if (Bath)
			{
				Game instance = Singleton<Game>.Instance;
				int index = hSceneManager.numFemaleClothCustom - 2;
				if (instance.saveData.playerCloths[index] != null && !instance.saveData.playerCloths[index].file.IsNullOrEmpty())
				{
					chaControl.ChangeNowCoordinate(instance.saveData.playerCloths[index].file, reload: true);
					flag = false;
				}
			}
			if (eventNo == 50 || eventNo == 51 || eventNo == 52 || eventNo == 53 || eventNo == 54 || eventNo == 55)
			{
				string appendCoordinatePlayer = Singleton<Game>.Instance.appendCoordinatePlayer;
				if (!appendCoordinatePlayer.IsNullOrEmpty())
				{
					chaControl.ChangeNowCoordinate(appendCoordinatePlayer, reload: true);
					flag = false;
				}
			}
		}
		if (flag)
		{
			chaControl.ChangeNowCoordinate(reload: true);
		}
		hSceneSpriteCloth.SetClothCharacter();
	}
}
