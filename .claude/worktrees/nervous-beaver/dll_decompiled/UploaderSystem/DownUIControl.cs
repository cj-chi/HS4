using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UploaderSystem;

public class DownUIControl : MonoBehaviour
{
	public enum MainMode
	{
		MM_Download,
		MM_MyData
	}

	public enum SortType
	{
		ST_NEW_C,
		ST_OLD_C,
		ST_NEW_U,
		ST_OLD_U,
		ST_DL_ALL,
		ST_DL_WEEK,
		ST_APPLAUSE
	}

	public class SearchSetting
	{
		public bool[] sex = new bool[2];

		public bool[] height = new bool[3];

		public bool[] bust = new bool[3];

		public bool[] hair = new bool[6];

		public bool[] voice;

		public void Reset(bool excludeSort = true)
		{
			for (int i = 0; i < sex.Length; i++)
			{
				sex[i] = false;
			}
			for (int j = 0; j < height.Length; j++)
			{
				height[j] = false;
			}
			for (int k = 0; k < bust.Length; k++)
			{
				bust[k] = false;
			}
			for (int l = 0; l < hair.Length; l++)
			{
				hair[l] = false;
			}
			if (voice != null)
			{
				for (int m = 0; m < voice.Length; m++)
				{
					voice[m] = false;
				}
			}
		}
	}

	[Serializable]
	public class SelectInfoChara
	{
		public Text textHN;

		public Text textName;

		public Text titleTextType;

		public Text textType;

		public Text textBirthDay;

		public Text textTrait;

		public Text textMind;

		public Text textHAttribute;

		public Text textComment;

		public Button btnHN;

		public Button btnHNReset;

		[HideInInspector]
		public int hnUserIndex = -1;

		public Image imgThumbnail;
	}

	[Serializable]
	public class SearchItem
	{
		public Button btnResetSearchSetting;

		public Toggle[] tglSortType;

		public Button btnHNOpen;

		public Button btnHNOpenEx;

		public Button btnHNReset;

		public Text textHN;

		public Toggle[] tglSex;

		public Toggle[] tglHeight;

		public Toggle[] tglBust;

		public Toggle[] tglHair;

		public Text titleTextVoice;

		[HideInInspector]
		public Toggle[] tglVoice;

		public GameObject objTempVoice;

		public InputField inpKeyword;

		public GameObject objKeywordDummy;

		public Text textKeywordDummy;

		public Button btnKeywordReset;
	}

	[Serializable]
	public class PageDataInfo
	{
		public NetFileComponent[] nfcChara;
	}

	[Serializable]
	public class ServerItem
	{
		public Button btnDownload;

		public Button btnDeleteCache;

		public Button btnDelete;
	}

	[Serializable]
	public class PageControlItem
	{
		public Button[] btnCtrlPage;

		public Text textPageMax;

		public InputField InpPage;

		[HideInInspector]
		public bool updatingThumb;

		[HideInInspector]
		public bool updateThumb;

		[HideInInspector]
		public int[] drawIndex;

		[HideInInspector]
		public int[] updateIndex;
	}

	public class PageSelectInfo
	{
		public int selChara = -1;
	}

	public DownloadScene downScene;

	public DownPhpControl phpCtrl;

	public SearchSetting[] searchSettings = new SearchSetting[Enum.GetNames(typeof(DataType)).Length];

	[HideInInspector]
	public int searchSortSort;

	private IntReactiveProperty _searchSortHNIdx = new IntReactiveProperty(-1);

	[Header("---< モード別表示OBJ >-----------------------")]
	private GameObject[] objModeAll;

	[SerializeField]
	private GameObject[] objModeDownload;

	[SerializeField]
	private GameObject[] objModeMyData;

	[Header("---< タイプ別表示OBJ >-----------------------")]
	private GameObject[] objTypeAll;

	[SerializeField]
	private GameObject[] objTypeChara;

	[SerializeField]
	private GameObject[] objHideH;

	[Header("---< AIServer時に非表示するOBJ >-------------")]
	[SerializeField]
	private GameObject[] objHideAI;

	[Header("---< モード・タイプ切り替え >----------------")]
	[SerializeField]
	private Toggle[] tglMainMode;

	[SerializeField]
	private Toggle[] tglDataType;

	[Header("---< 選択情報・キャラ >----------------------")]
	[SerializeField]
	private SelectInfoChara selInfoCha;

	[Header("---< 検索関連 >------------------------------")]
	[SerializeField]
	private SearchItem searchItem;

	[Header("---< データ一覧情報 >------------------------")]
	[SerializeField]
	private PageDataInfo pageDataInfo;

	[Header("---< サーバーデータ >--------------------------")]
	[SerializeField]
	private ServerItem serverItem;

	[Header("---< ページ制御 >----------------------------")]
	[SerializeField]
	private PageControlItem pageCtrlItem;

	private IntReactiveProperty _pageMax = new IntReactiveProperty(1);

	private IntReactiveProperty _pageNow = new IntReactiveProperty(1);

	[Header("---< その他 >--------------------------------")]
	[SerializeField]
	private GameObject objUpdatingThumbnailCanvas;

	[SerializeField]
	private Button btnTitle;

	[SerializeField]
	private Button btnUploader;

	[SerializeField]
	private Button btnReload;

	[SerializeField]
	private Text textNewestVersion;

	private VoiceInfo.Param[] personalities;

	private IntReactiveProperty _mainMode = new IntReactiveProperty(0);

	private IntReactiveProperty _dataType = new IntReactiveProperty(0);

	[HideInInspector]
	public bool changeSearchSetting;

	private BoolReactiveProperty _updateCharaInfo = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateAllInfo = new BoolReactiveProperty(initialValue: false);

	public PageSelectInfo downloadSel = new PageSelectInfo();

	public List<NetworkInfo.CharaInfo> lstSearchCharaInfo = new List<NetworkInfo.CharaInfo>();

	private Dictionary<int, int> dictVoiceInfo = new Dictionary<int, int>();

	private NetworkInfo.BaseIndex selBaseIndex;

	private bool skipToggleChange;

	private NetworkInfo netInfo => Singleton<NetworkInfo>.Instance;

	private NetCacheControl cacheCtrl
	{
		get
		{
			if (!Singleton<NetworkInfo>.IsInstance())
			{
				return null;
			}
			return netInfo.cacheCtrl;
		}
	}

	private bool isHS2
	{
		get
		{
			if (Singleton<GameSystem>.Instance.networkType != 0)
			{
				return false;
			}
			return true;
		}
	}

	public int searchSortHNIdx
	{
		get
		{
			return _searchSortHNIdx.Value;
		}
		set
		{
			_searchSortHNIdx.Value = value;
		}
	}

	public int pageMax
	{
		get
		{
			return _pageMax.Value;
		}
		set
		{
			_pageMax.Value = value;
		}
	}

	public int pageNow
	{
		get
		{
			return _pageNow.Value;
		}
		set
		{
			_pageNow.Value = value;
		}
	}

	public int mainMode
	{
		get
		{
			return _mainMode.Value;
		}
		set
		{
			_mainMode.Value = value;
		}
	}

	public int dataType
	{
		get
		{
			return _dataType.Value;
		}
		set
		{
			_dataType.Value = value;
		}
	}

	public bool updateCharaInfo
	{
		get
		{
			return _updateCharaInfo.Value;
		}
		set
		{
			_updateCharaInfo.Value = value;
		}
	}

	public bool updateAllInfo
	{
		get
		{
			return _updateAllInfo.Value;
		}
		set
		{
			_updateAllInfo.Value = value;
		}
	}

	public void ShowNewestVersion()
	{
		if ((bool)textNewestVersion)
		{
			textNewestVersion.gameObject.SetActiveIfDifferent(active: true);
		}
	}

	public void UpdateInfoChara()
	{
		NetworkInfo.CharaInfo charaInfo = null;
		string text = "";
		MainMode mainMode = (MainMode)this.mainMode;
		if ((uint)mainMode <= 1u)
		{
			selInfoCha.hnUserIndex = -1;
			if (-1 != downloadSel.selChara && lstSearchCharaInfo.Count > downloadSel.selChara)
			{
				charaInfo = lstSearchCharaInfo[downloadSel.selChara];
				text = GetHandleNameFromUserIndex(charaInfo.user_idx);
				selInfoCha.hnUserIndex = charaInfo.user_idx;
			}
		}
		if (charaInfo == null)
		{
			if ((bool)selInfoCha.textHN)
			{
				selInfoCha.textHN.text = "";
			}
			if (null != selInfoCha.textName)
			{
				selInfoCha.textName.text = "";
			}
			if (null != selInfoCha.textType)
			{
				selInfoCha.textType.text = "";
			}
			if (null != selInfoCha.textBirthDay)
			{
				selInfoCha.textBirthDay.text = "";
			}
			if (null != selInfoCha.textTrait)
			{
				selInfoCha.textTrait.text = "";
			}
			if (null != selInfoCha.textMind)
			{
				selInfoCha.textMind.text = "";
			}
			if (null != selInfoCha.textHAttribute)
			{
				selInfoCha.textHAttribute.text = "";
			}
			if ((bool)selInfoCha.textComment)
			{
				selInfoCha.textComment.text = "";
			}
			return;
		}
		bool flag = 1 == charaInfo.sex;
		if ((bool)selInfoCha.textHN)
		{
			selInfoCha.textHN.text = text;
		}
		if (null != selInfoCha.textName)
		{
			selInfoCha.textName.text = charaInfo.name;
		}
		if (null != selInfoCha.textType)
		{
			selInfoCha.textType.text = ((!flag) ? "" : Singleton<Character>.Instance.GetCharaTypeName(charaInfo.type));
		}
		if (null != selInfoCha.textBirthDay)
		{
			selInfoCha.textBirthDay.text = charaInfo.strBirthDay;
		}
		if (null != selInfoCha.textTrait)
		{
			if (Game.infoTraitTable.TryGetValue(charaInfo.trait, out var value))
			{
				selInfoCha.textTrait.text = value;
			}
			else
			{
				selInfoCha.textTrait.text = "---------------";
			}
		}
		if (null != selInfoCha.textMind)
		{
			if (Game.infoMindTable.TryGetValue(charaInfo.mind, out var value2))
			{
				selInfoCha.textMind.text = value2;
			}
			else
			{
				selInfoCha.textMind.text = "---------------";
			}
		}
		if (null != selInfoCha.textHAttribute)
		{
			if (Game.infoHAttributeTable.TryGetValue(charaInfo.hAttribute, out var value3))
			{
				selInfoCha.textHAttribute.text = value3;
			}
			else
			{
				selInfoCha.textHAttribute.text = "---------------";
			}
		}
		if ((bool)selInfoCha.textComment)
		{
			selInfoCha.textComment.text = charaInfo.comment;
		}
	}

	public void UpdateCharaSearchList()
	{
		int num = -1;
		int selChara = downloadSel.selChara;
		if (-1 != downloadSel.selChara)
		{
			num = lstSearchCharaInfo[downloadSel.selChara].idx;
		}
		lstSearchCharaInfo.Clear();
		bool[] array = new bool[2];
		bool flag = false;
		for (int i = 0; i < 2; i++)
		{
			if (!(null == searchItem.tglSex[i]))
			{
				array[i] = searchItem.tglSex[i].isOn;
				if (array[i])
				{
					flag = true;
				}
			}
		}
		bool[] array2 = new bool[3];
		bool flag2 = false;
		for (int j = 0; j < 3; j++)
		{
			if (!(null == searchItem.tglHeight[j]))
			{
				array2[j] = searchItem.tglHeight[j].isOn;
				if (array2[j])
				{
					flag2 = true;
				}
			}
		}
		bool[] array3 = new bool[3];
		bool flag3 = false;
		for (int k = 0; k < 3; k++)
		{
			if (!(null == searchItem.tglBust[k]))
			{
				array3[k] = searchItem.tglBust[k].isOn;
				if (array3[k])
				{
					flag3 = true;
				}
			}
		}
		bool[] array4 = new bool[6];
		bool flag4 = false;
		for (int l = 0; l < 6; l++)
		{
			if (!(null == searchItem.tglHair[l]))
			{
				array4[l] = searchItem.tglHair[l].isOn;
				if (array4[l])
				{
					flag4 = true;
				}
			}
		}
		bool flag5 = false;
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, int> item in dictVoiceInfo)
		{
			int key = item.Key;
			if (!(null == searchItem.tglVoice[key]) && searchItem.tglVoice[key].isOn)
			{
				list.Add(item.Value);
				flag5 = true;
			}
		}
		int num2 = -1;
		num2 = ((mainMode != 1) ? searchSortHNIdx : netInfo.profile.userIdx);
		int count = netInfo.lstCharaInfo.Count;
		for (int m = 0; m < count; m++)
		{
			NetworkInfo.CharaInfo charaInfo = netInfo.lstCharaInfo[m];
			if (!MathfEx.IsRange(0, charaInfo.sex, 1, isEqual: true) || (flag && !array[charaInfo.sex]) || (charaInfo.sex == 0 && (flag5 || flag3 || flag2)) || !MathfEx.IsRange(0, charaInfo.height, array2.Length - 1, isEqual: true) || (flag2 && !array2[charaInfo.height]) || (1 == charaInfo.sex && !MathfEx.IsRange(0, charaInfo.bust, array3.Length - 1, isEqual: true)) || (flag3 && !array3[charaInfo.bust]) || !MathfEx.IsRange(0, charaInfo.hair, array4.Length - 1, isEqual: true) || (flag4 && !array4[charaInfo.hair]) || (flag5 && !list.Contains(charaInfo.type)) || (-1 != num2 && num2 != charaInfo.user_idx))
			{
				continue;
			}
			if (searchItem.textKeywordDummy.text != "")
			{
				string text = searchItem.textKeywordDummy.text;
				if (!charaInfo.name.Contains(text) && !charaInfo.comment.Contains(text))
				{
					continue;
				}
			}
			lstSearchCharaInfo.Add(charaInfo);
		}
		UpdateSortChara();
		UpdatePageMax();
		downloadSel.selChara = -1;
		for (int n = 0; n < lstSearchCharaInfo.Count; n++)
		{
			if (lstSearchCharaInfo[n].idx == num)
			{
				downloadSel.selChara = n;
				break;
			}
		}
		int num3 = CheckSelectInPage(downloadSel.selChara);
		if (-1 == num3)
		{
			downloadSel.selChara = -1;
			if (-1 != selChara)
			{
				NetFileComponent[] nfcChara = pageDataInfo.nfcChara;
				for (int num4 = 0; num4 < nfcChara.Length; num4++)
				{
					nfcChara[num4].tglItem.isOn = false;
				}
			}
		}
		else
		{
			pageDataInfo.nfcChara[num3].tglItem.isOn = true;
		}
		UpdateInfoChara();
		UpdatePage();
	}

	public void UpdateSortChara()
	{
		switch (searchSortSort)
		{
		case 0:
			lstSearchCharaInfo = (from n in lstSearchCharaInfo
				orderby n.createTime descending, n.idx
				select n).ToList();
			break;
		case 1:
			lstSearchCharaInfo = (from n in lstSearchCharaInfo
				orderby n.createTime, n.idx
				select n).ToList();
			break;
		case 2:
			lstSearchCharaInfo = (from n in lstSearchCharaInfo
				orderby n.updateTime descending, n.idx
				select n).ToList();
			break;
		case 3:
			lstSearchCharaInfo = (from n in lstSearchCharaInfo
				orderby n.updateTime, n.idx
				select n).ToList();
			break;
		case 4:
			lstSearchCharaInfo = (from n in lstSearchCharaInfo
				orderby n.rankingT, n.idx
				select n).ToList();
			break;
		case 5:
			lstSearchCharaInfo = (from n in lstSearchCharaInfo
				orderby n.rankingW, n.idx
				select n).ToList();
			break;
		case 6:
			lstSearchCharaInfo = (from n in lstSearchCharaInfo
				orderby n.applause descending, n.idx
				select n).ToList();
			break;
		}
	}

	private int GetPageDataCount()
	{
		if (dataType == 0)
		{
			return lstSearchCharaInfo.Count;
		}
		return 0;
	}

	private int GetPageDrawCount()
	{
		if (dataType == 0)
		{
			return pageDataInfo.nfcChara.Length;
		}
		return 0;
	}

	private NetFileComponent[] GetPageFileComponents()
	{
		if (dataType == 0)
		{
			return pageDataInfo.nfcChara;
		}
		return null;
	}

	private List<NetworkInfo.BaseIndex> GetBaseIndexListFromSearch()
	{
		if (dataType == 0)
		{
			if (lstSearchCharaInfo.Count == 0)
			{
				return null;
			}
			return lstSearchCharaInfo.OfType<NetworkInfo.BaseIndex>().ToList();
		}
		return null;
	}

	private void UpdatePageMax()
	{
		int pageDrawCount = GetPageDrawCount();
		int pageDataCount = GetPageDataCount();
		int num = pageDataCount / pageDrawCount;
		num += ((pageDataCount % pageDrawCount != 0) ? 1 : 0);
		pageMax = Mathf.Max(num, 1);
		if (pageNow >= pageMax)
		{
			pageNow = 0;
		}
	}

	private int CheckSelectInPage(int _sel)
	{
		int pageDrawCount = GetPageDrawCount();
		int pageDataCount = GetPageDataCount();
		int num = pageNow * pageDrawCount;
		for (int i = 0; i < pageDrawCount; i++)
		{
			int num2 = num + i;
			if (pageDataCount <= num2)
			{
				break;
			}
			if (num2 == _sel)
			{
				return num2 - num;
			}
		}
		return -1;
	}

	private void UpdatePage()
	{
		NetFileComponent[] cmpNetFile = GetPageFileComponents();
		if (cmpNetFile == null)
		{
			return;
		}
		int num = cmpNetFile.Length;
		pageCtrlItem.drawIndex = new int[num];
		pageCtrlItem.updateIndex = new int[num];
		for (int i = 0; i < num; i++)
		{
			cmpNetFile[i].SetState(interactable: false, enable: false);
			pageCtrlItem.drawIndex[i] = -1;
			pageCtrlItem.updateIndex[i] = 0;
		}
		int pageDataCount = GetPageDataCount();
		if (pageDataCount == 0)
		{
			return;
		}
		List<NetworkInfo.BaseIndex> lstBaseIdx = GetBaseIndexListFromSearch();
		int num2 = pageNow * num;
		for (int j = 0; j < num; j++)
		{
			int index = num2 + j;
			if (pageDataCount <= index)
			{
				break;
			}
			pageCtrlItem.drawIndex[j] = lstBaseIdx[index].idx;
			pageCtrlItem.updateIndex[j] = lstBaseIdx[index].update_idx;
			cmpNetFile[j].SetState(interactable: true, enable: true);
			cmpNetFile[j].UpdateSortType(searchSortSort);
			switch (searchSortSort)
			{
			case 0:
				cmpNetFile[j].SetUpdateTime(lstBaseIdx[index].createTime, 0);
				break;
			case 1:
				cmpNetFile[j].SetUpdateTime(lstBaseIdx[index].createTime, 0);
				break;
			case 2:
				cmpNetFile[j].SetUpdateTime(lstBaseIdx[index].updateTime, 1);
				break;
			case 3:
				cmpNetFile[j].SetUpdateTime(lstBaseIdx[index].updateTime, 1);
				break;
			case 4:
				cmpNetFile[j].SetRanking(lstBaseIdx[index].rankingT + 1);
				break;
			case 5:
				cmpNetFile[j].SetRanking(lstBaseIdx[index].rankingW + 1);
				break;
			case 6:
				cmpNetFile[j].SetApplauseNum(lstBaseIdx[index].applause);
				break;
			}
			if (Singleton<GameSystem>.Instance.IsApplause((DataType)dataType, isHS2, lstBaseIdx[index].data_uid))
			{
				cmpNetFile[j].actApplause = null;
				continue;
			}
			int no = j;
			cmpNetFile[j].actApplause = delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				netInfo.BlockUI();
				Observable.FromCoroutine((IObserver<bool> res) => phpCtrl.AddApplauseCount(res, (DataType)dataType, lstBaseIdx[index])).Subscribe(delegate
				{
					Singleton<GameSystem>.Instance.AddApplause((DataType)dataType, isHS2, lstBaseIdx[index].data_uid);
					lstBaseIdx[index].applause++;
					changeSearchSetting = true;
					cmpNetFile[no].SetApplauseNum(lstBaseIdx[index].applause);
				}, delegate
				{
					netInfo.UnblockUI();
				}, delegate
				{
					netInfo.UnblockUI();
				});
			};
		}
		if (!pageCtrlItem.updatingThumb)
		{
			pageCtrlItem.updatingThumb = true;
			objUpdatingThumbnailCanvas.SetActiveIfDifferent(active: true);
			Observable.FromCoroutine((IObserver<bool> res) => phpCtrl.GetThumbnail(res, (DataType)dataType)).Subscribe(delegate
			{
			}, delegate
			{
				pageCtrlItem.updatingThumb = false;
				objUpdatingThumbnailCanvas.SetActiveIfDifferent(active: false);
				netInfo.popupMsg.EndMessage();
			}, delegate
			{
				pageCtrlItem.updatingThumb = false;
				objUpdatingThumbnailCanvas.SetActiveIfDifferent(active: false);
				netInfo.popupMsg.EndMessage();
			});
		}
		else
		{
			pageCtrlItem.updateThumb = true;
		}
	}

	public void ChangeThumbnail(byte[][] data)
	{
		NetFileComponent[] pageFileComponents = GetPageFileComponents();
		if (pageFileComponents != null)
		{
			Texture texture = null;
			int length = data.GetLength(0);
			for (int i = 0; i < pageFileComponents.Length; i++)
			{
				texture = ((i >= length) ? null : ((data[i] != null) ? PngAssist.ChangeTextureFromByte(data[i]) : null));
				pageFileComponents[i].SetImage(texture);
			}
		}
	}

	public void UpdateSearchSettingUI()
	{
		SearchSetting searchSetting = searchSettings[dataType];
		int num = searchSortSort;
		for (int i = 0; i < searchItem.tglSortType.Length; i++)
		{
			if ((bool)searchItem.tglSortType[i])
			{
				searchItem.tglSortType[i].isOn = false;
			}
		}
		searchItem.tglSortType[num].isOn = true;
		for (int j = 0; j < searchItem.tglSex.Length; j++)
		{
			if ((bool)searchItem.tglSex[j])
			{
				searchItem.tglSex[j].isOn = searchSetting.sex[j];
			}
		}
		for (int k = 0; k < searchItem.tglHeight.Length; k++)
		{
			if ((bool)searchItem.tglHeight[k])
			{
				searchItem.tglHeight[k].isOn = searchSetting.height[k];
			}
		}
		for (int l = 0; l < searchItem.tglBust.Length; l++)
		{
			if ((bool)searchItem.tglBust[l])
			{
				searchItem.tglBust[l].isOn = searchSetting.bust[l];
			}
		}
		for (int m = 0; m < searchItem.tglHair.Length; m++)
		{
			if ((bool)searchItem.tglHair[m])
			{
				searchItem.tglHair[m].isOn = searchSetting.hair[m];
			}
		}
		for (int n = 0; n < searchItem.tglVoice.Length; n++)
		{
			if ((bool)searchItem.tglVoice[n])
			{
				searchItem.tglVoice[n].isOn = searchSetting.voice[n];
			}
		}
	}

	public Tuple<int, int>[] GetThumbnailIndex(DataType type)
	{
		Tuple<int, int>[] array = new Tuple<int, int>[pageCtrlItem.drawIndex.Count((int x) => -1 != x)];
		for (int num = 0; num < array.Length; num++)
		{
			array[num] = new Tuple<int, int>(pageCtrlItem.drawIndex[num], pageCtrlItem.updateIndex[num]);
		}
		return array;
	}

	public NetworkInfo.BaseIndex GetSelectServerInfo(DataType type)
	{
		int num = (new int[1] { downloadSel.selChara })[(int)type];
		if (-1 == num)
		{
			return null;
		}
		return GetBaseIndexListFromSearch()?[num];
	}

	public string GetHandleNameFromUserIndex(int index)
	{
		if (!netInfo.dictUserInfo.TryGetValue(index, out var value))
		{
			return NetworkDefine.msgDownUnknown[Singleton<GameSystem>.Instance.languageInt];
		}
		return value.handleName;
	}

	public void UpdateDownloadList()
	{
		if (dataType == 0)
		{
			UpdateCharaSearchList();
		}
	}

	private void SaveDownloadFile(byte[] bytes, NetworkInfo.BaseIndex info)
	{
		if (dataType == 0)
		{
			NetworkInfo.CharaInfo charaInfo = info as NetworkInfo.CharaInfo;
			string[] array = new string[2]
			{
				UserData.Path + "chara/male/",
				UserData.Path + "chara/female/"
			};
			string text = (new string[2] { "HS2ChaM_", "HS2ChaF_" })[charaInfo.sex] + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
			string filename = array[charaInfo.sex] + text;
			ChaFileControl chaFileControl = new ChaFileControl();
			chaFileControl.LoadFromBytes(bytes);
			chaFileControl.InitGameInfoParam();
			chaFileControl.SaveCharaFile(filename, chaFileControl.parameter.sex);
		}
	}

	private void Awake()
	{
		HashSet<GameObject> source = new HashSet<GameObject>(objTypeChara);
		objTypeAll = source.ToArray();
		HashSet<GameObject> hashSet = new HashSet<GameObject>(objModeDownload);
		hashSet.UnionWith(objModeMyData);
		objModeAll = hashSet.ToArray();
		if (1 != Singleton<GameSystem>.Instance.networkType)
		{
			return;
		}
		GameObject[] array = objHideAI;
		foreach (GameObject gameObject in array)
		{
			if (null != gameObject)
			{
				gameObject.SetActiveIfDifferent(active: false);
			}
		}
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		if (!isHS2)
		{
			if (selInfoCha != null && null != selInfoCha.titleTextType)
			{
				selInfoCha.titleTextType.text = "タイプ";
			}
			if (searchItem != null && null != searchItem.titleTextVoice)
			{
				searchItem.titleTextVoice.text = "タイプ";
			}
		}
		personalities = Voice.infoTable.Values.Where((VoiceInfo.Param x) => 0 <= x.No).ToArray();
		for (int num = 0; num < searchSettings.Length; num++)
		{
			searchSettings[num] = new SearchSetting
			{
				voice = new bool[personalities.Length]
			};
		}
		searchItem.tglVoice = new Toggle[personalities.Length];
		if ((bool)searchItem.objTempVoice)
		{
			for (int num2 = 0; num2 < personalities.Length; num2++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(searchItem.objTempVoice, searchItem.objTempVoice.transform.parent, worldPositionStays: false);
				Toggle component = obj.GetComponent<Toggle>();
				if ((bool)component)
				{
					searchItem.tglVoice[num2] = component;
				}
				Text componentInChildren = obj.GetComponentInChildren<Text>();
				if ((bool)componentInChildren)
				{
					componentInChildren.text = personalities[num2].Get(Singleton<GameSystem>.Instance.languageInt);
				}
				obj.SetActiveIfDifferent(active: true);
				dictVoiceInfo[num2] = personalities[num2].No;
			}
		}
		if ((bool)serverItem.btnDownload)
		{
			Text text = serverItem.btnDownload.GetComponentInChildren<Text>(includeInactive: true);
			serverItem.btnDownload.OnClickAsObservable().Subscribe(delegate
			{
				netInfo.popupMsg.StartMessage(0.2f, 1f, 0.2f, NetworkDefine.msgDownDownloadData[Singleton<GameSystem>.Instance.languageInt], 2);
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				NetworkInfo.BaseIndex dlinfo = GetSelectServerInfo((DataType)dataType);
				if (dlinfo != null)
				{
					netInfo.BlockUI();
					Observable.FromCoroutine((IObserver<byte[]> res) => phpCtrl.DownloadPNG(res, (DataType)dataType)).Subscribe(delegate(byte[] res)
					{
						SaveDownloadFile(res, dlinfo);
						Singleton<GameSystem>.Instance.AddDownload((DataType)dataType, isHS2, dlinfo.data_uid);
						netInfo.popupMsg.StartMessage(0.2f, 1f, 0.2f, NetworkDefine.msgDownDownloaded[Singleton<GameSystem>.Instance.languageInt], 0);
					}, delegate
					{
						netInfo.popupMsg.StartMessage(0.2f, 1f, 0.2f, NetworkDefine.msgDownFailed[Singleton<GameSystem>.Instance.languageInt], 0);
						netInfo.UnblockUI();
					}, delegate
					{
						netInfo.UnblockUI();
					});
				}
			});
			serverItem.btnDownload.UpdateAsObservable().Subscribe(delegate
			{
				bool flag = selBaseIndex != null;
				if (serverItem.btnDownload.interactable != flag)
				{
					serverItem.btnDownload.interactable = flag;
					text.color = new Color(text.color.r, text.color.g, text.color.b, flag ? 1f : 0.5f);
				}
			});
		}
		if ((bool)serverItem.btnDeleteCache)
		{
			serverItem.btnDeleteCache.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Observable.FromCoroutine((IObserver<bool> res) => phpCtrl.DeleteCache(res, (DataType)dataType)).Subscribe(delegate
				{
					UpdatePage();
					netInfo.DrawMessage(NetworkDefine.colorWhite, NetworkDefine.msgDownDeleteCache[Singleton<GameSystem>.Instance.languageInt]);
				}, delegate
				{
				}, delegate
				{
				});
			});
		}
		if ((bool)serverItem.btnDelete)
		{
			Text text2 = serverItem.btnDelete.GetComponentInChildren<Text>(includeInactive: true);
			serverItem.btnDelete.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				NetworkInfo.BaseIndex selectServerInfo = GetSelectServerInfo((DataType)dataType);
				if (selectServerInfo != null)
				{
					Observable.FromCoroutine((IObserver<bool> res) => phpCtrl.DeleteMyData(res, (DataType)dataType)).Subscribe(delegate
					{
						netInfo.DrawMessage(NetworkDefine.colorWhite, NetworkDefine.msgDownDeleteData[Singleton<GameSystem>.Instance.languageInt]);
					}, delegate
					{
					}, delegate
					{
					});
				}
			});
			serverItem.btnDelete.UpdateAsObservable().Subscribe(delegate
			{
				bool flag = selBaseIndex != null;
				if (serverItem.btnDelete.interactable != flag)
				{
					serverItem.btnDelete.interactable = flag;
					text2.color = new Color(text2.color.r, text2.color.g, text2.color.b, flag ? 1f : 0.5f);
				}
			});
		}
		if ((bool)searchItem.btnResetSearchSetting)
		{
			searchItem.btnResetSearchSetting.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				searchSettings[dataType].Reset();
				searchSortSort = 0;
				searchSortHNIdx = -1;
				searchItem.inpKeyword.text = "";
				searchItem.textKeywordDummy.text = "";
				changeSearchSetting = true;
				UpdateSearchSettingUI();
			});
		}
		if ((bool)searchItem.btnHNOpen)
		{
			searchItem.btnHNOpen.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				netInfo.netSelectHN.ShowSelectHNWindow(show: true);
			});
		}
		if ((bool)searchItem.btnHNOpenEx)
		{
			searchItem.btnHNOpenEx.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				netInfo.netSelectHN.ShowSelectHNWindow(show: true);
			});
		}
		if ((bool)searchItem.btnHNReset)
		{
			searchItem.btnHNReset.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				searchSortHNIdx = -1;
				changeSearchSetting = true;
			});
		}
		_searchSortHNIdx.Subscribe(delegate(int x)
		{
			searchItem.textHN.text = ((-1 == x) ? NetworkDefine.strHandleNameNoSelect[Singleton<GameSystem>.Instance.languageInt] : GetHandleNameFromUserIndex(x));
		});
		if ((bool)searchItem.inpKeyword)
		{
			searchItem.inpKeyword.OnEndEditAsObservable().Subscribe(delegate(string text3)
			{
				searchItem.textKeywordDummy.text = text3;
				changeSearchSetting = true;
			});
			searchItem.inpKeyword.UpdateAsObservable().Subscribe(delegate
			{
				bool isFocused = searchItem.inpKeyword.isFocused;
				searchItem.objKeywordDummy.SetActiveIfDifferent(!isFocused);
			}).AddTo(this);
		}
		if ((bool)searchItem.btnKeywordReset)
		{
			searchItem.btnKeywordReset.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				searchItem.textKeywordDummy.text = "";
				searchItem.inpKeyword.text = "";
				changeSearchSetting = true;
			});
		}
		if (searchItem.tglSortType.Any())
		{
			(from item in searchItem.tglSortType.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.tgl.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					if (searchSortSort != item.idx)
					{
						searchSortSort = item.idx;
						changeSearchSetting = true;
					}
				});
			});
		}
		if ((bool)selInfoCha.btnHN)
		{
			selInfoCha.btnHN.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				searchSortHNIdx = selInfoCha.hnUserIndex;
				changeSearchSetting = true;
			});
		}
		if ((bool)selInfoCha.btnHNReset)
		{
			selInfoCha.btnHNReset.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				searchSortHNIdx = -1;
				changeSearchSetting = true;
			});
		}
		if (searchItem.tglSex.Any())
		{
			(from item in searchItem.tglSex.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				item.tgl.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					if (searchSettings[dataType].sex[item.idx] != isOn)
					{
						searchSettings[dataType].sex[item.idx] = isOn;
						changeSearchSetting = true;
					}
				});
			});
		}
		if (searchItem.tglHeight.Any())
		{
			(from item in searchItem.tglHeight.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				item.tgl.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					if (searchSettings[dataType].height[item.idx] != isOn)
					{
						searchSettings[dataType].height[item.idx] = isOn;
						changeSearchSetting = true;
					}
				});
			});
		}
		if (searchItem.tglBust.Any())
		{
			(from item in searchItem.tglBust.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				item.tgl.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					if (searchSettings[dataType].bust[item.idx] != isOn)
					{
						searchSettings[dataType].bust[item.idx] = isOn;
						changeSearchSetting = true;
					}
				});
			});
		}
		if (searchItem.tglHair.Any())
		{
			(from item in searchItem.tglHair.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				item.tgl.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					if (searchSettings[dataType].hair[item.idx] != isOn)
					{
						searchSettings[dataType].hair[item.idx] = isOn;
						changeSearchSetting = true;
					}
				});
			});
		}
		if (searchItem.tglVoice.Any())
		{
			(from item in searchItem.tglVoice.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				item.tgl.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					if (searchSettings[dataType].voice[item.idx] != isOn)
					{
						searchSettings[dataType].voice[item.idx] = isOn;
						changeSearchSetting = true;
					}
				});
			});
		}
		if (tglMainMode.Any())
		{
			(from item in tglMainMode.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.tgl.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					if (mainMode != item.idx)
					{
						Utils.Sound.Play(SystemSE.sel);
					}
					mainMode = item.idx;
				});
			});
		}
		if (tglDataType.Any())
		{
			(from item in tglDataType.Select((Toggle tgl, int idx) => new { tgl, idx })
				where item.tgl != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.tgl.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					if (dataType != item.idx)
					{
						Utils.Sound.Play(SystemSE.sel);
					}
					dataType = item.idx;
				});
			});
		}
		_mainMode.Subscribe(delegate(int no)
		{
			switch ((MainMode)no)
			{
			case MainMode.MM_Download:
			{
				GameObject[] array = objModeAll;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActiveIfDifferent(active: false);
				}
				array = objModeDownload;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActiveIfDifferent(active: true);
				}
				break;
			}
			case MainMode.MM_MyData:
			{
				GameObject[] array = objModeAll;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActiveIfDifferent(active: false);
				}
				array = objModeMyData;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActiveIfDifferent(active: true);
				}
				break;
			}
			}
			searchSortHNIdx = -1;
			if (netInfo.changeCharaList)
			{
				Observable.FromCoroutine((IObserver<bool> res) => phpCtrl.UpdateBaseInfo(res)).Subscribe(delegate
				{
					updateAllInfo = true;
					UpdateSearchSettingUI();
					changeSearchSetting = true;
				}, delegate
				{
				}, delegate
				{
				});
			}
			else
			{
				updateAllInfo = true;
				UpdateSearchSettingUI();
				changeSearchSetting = true;
			}
		});
		_dataType.Subscribe(delegate(int no)
		{
			if (no == 0)
			{
				GameObject[] array = objTypeAll;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActiveIfDifferent(active: false);
				}
				array = objTypeChara;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActiveIfDifferent(active: true);
				}
			}
			UpdateSearchSettingUI();
			changeSearchSetting = true;
		});
		_updateCharaInfo.Where((bool f) => f).Subscribe(delegate
		{
			UpdateInfoChara();
			updateCharaInfo = false;
		});
		_updateAllInfo.Where((bool f) => f).Subscribe(delegate
		{
			updateCharaInfo = true;
			updateAllInfo = false;
		});
		pageDataInfo.nfcChara.Select((NetFileComponent val, int idx) => new
		{
			tgl = val.tglItem,
			idx = idx
		}).ToList().ForEach(item =>
		{
			(from _ in item.tgl.OnValueChangedAsObservable()
				where !skipToggleChange
				select _).Subscribe(delegate(bool isOn)
			{
				if (isOn)
				{
					skipToggleChange = true;
					foreach (NetFileComponent item in pageDataInfo.nfcChara.Where((NetFileComponent x) => x.tglItem != item.tgl))
					{
						item.tglItem.isOn = false;
					}
					skipToggleChange = false;
				}
				int pageDrawCount = GetPageDrawCount();
				int selChara = ((!isOn) ? (-1) : (pageNow * pageDrawCount + item.idx));
				downloadSel.selChara = selChara;
				UpdateInfoChara();
			});
		});
		_pageMax.Subscribe(delegate(int no)
		{
			bool interactable = no != 0;
			pageCtrlItem.textPageMax.text = no.ToString();
			pageCtrlItem.InpPage.interactable = interactable;
			foreach (Button item2 in pageCtrlItem.btnCtrlPage.Where((Button x) => null != x))
			{
				item2.interactable = interactable;
			}
		});
		_pageNow.Subscribe(delegate
		{
			int num3 = pageNow + 1;
			pageCtrlItem.InpPage.text = num3.ToString();
			if (dataType == 0)
			{
				downloadSel.selChara = -1;
				UpdateInfoChara();
				NetFileComponent[] nfcChara = pageDataInfo.nfcChara;
				for (int i = 0; i < nfcChara.Length; i++)
				{
					nfcChara[i].tglItem.isOn = false;
				}
			}
			UpdatePage();
		});
		pageCtrlItem.InpPage.onEndEdit.AddListener(delegate(string s)
		{
			int.TryParse(s, out var result);
			result = Mathf.Clamp(result, 1, pageMax);
			if (result - 1 != pageNow)
			{
				pageNow = result - 1;
			}
			pageCtrlItem.InpPage.text = result.ToString();
		});
		pageCtrlItem.btnCtrlPage.Select((Button btn, int idx) => new { btn, idx }).ToList().ForEach(item =>
		{
			item.btn.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
				switch (item.idx)
				{
				case 0:
					pageNow = 0;
					break;
				case 1:
					pageNow = Mathf.Max(0, pageNow - 1);
					break;
				case 2:
					pageNow = Mathf.Min(pageMax - 1, pageNow + 1);
					break;
				case 3:
					pageNow = pageMax - 1;
					break;
				}
			});
		});
		if ((bool)btnReload)
		{
			btnReload.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				Observable.FromCoroutine(() => phpCtrl.GetBaseInfo(upload: false)).Subscribe(delegate
				{
				}, delegate
				{
				}, delegate
				{
					changeSearchSetting = true;
				});
			});
		}
		if ((bool)btnTitle)
		{
			btnTitle.OnClickAsObservable().Subscribe(delegate
			{
				netInfo.BlockUI();
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "Title",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: false);
			});
		}
		if (null != btnUploader)
		{
			btnUploader.OnClickAsObservable().Subscribe(delegate
			{
				netInfo.BlockUI();
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				Singleton<GameSystem>.Instance.networkSceneName = "Uploader";
				Singleton<GameSystem>.Instance.networkType = 0;
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "NetworkCheckScene",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: false);
			});
		}
		this.UpdateAsObservable().Subscribe(delegate
		{
			selBaseIndex = GetSelectServerInfo((DataType)dataType);
			if (changeSearchSetting)
			{
				UpdateDownloadList();
				changeSearchSetting = false;
			}
			if (pageCtrlItem.updateThumb && !pageCtrlItem.updatingThumb)
			{
				pageCtrlItem.updateThumb = false;
				pageCtrlItem.updatingThumb = true;
				objUpdatingThumbnailCanvas.SetActiveIfDifferent(active: true);
				Observable.FromCoroutine((IObserver<bool> res) => phpCtrl.GetThumbnail(res, (DataType)dataType)).Subscribe(delegate
				{
				}, delegate
				{
					pageCtrlItem.updatingThumb = false;
					objUpdatingThumbnailCanvas.SetActiveIfDifferent(active: false);
					netInfo.popupMsg.EndMessage();
				}, delegate
				{
					pageCtrlItem.updatingThumb = false;
					objUpdatingThumbnailCanvas.SetActiveIfDifferent(active: false);
					netInfo.popupMsg.EndMessage();
				});
			}
		});
	}
}
