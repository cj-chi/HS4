using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharaCustom;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UploaderSystem;

public class UpUIControl : MonoBehaviour
{
	[Serializable]
	public class SelectInfoChara
	{
		public Text textName;

		public Text textType;

		public Text textBirthDay;

		public Text textTrait;

		public Text textMind;

		public Text textHAttribute;

		public Image imgThumbnail;
	}

	[Serializable]
	public class UploadItem
	{
		public InputField inpTitle;

		public InputField inpComment;

		[HideInInspector]
		public string[] bkComment = new string[Enum.GetNames(typeof(DataType)).Length];

		public Toggle tglAgreePolicy;

		public Button btnPolicy;

		public Button btnUpload;

		public UIBehaviour exitPolicy;

		public GameObject objPolicy;

		[HideInInspector]
		public bool modeUpdate;
	}

	[Serializable]
	public class ProfileItem
	{
		public Text textHandle;

		public Button btnChangeHandle;
	}

	public UpPhpControl phpCtrl;

	[Header("---< タイプ別表示OBJ >-----------------------")]
	[SerializeField]
	private GameObject objCharaTop;

	private GameObject[] objSexAll;

	[SerializeField]
	private GameObject[] objMale;

	[SerializeField]
	private GameObject[] objFemale;

	[SerializeField]
	private GameObject[] objHideH;

	[SerializeField]
	private Toggle tglFemale;

	[Header("---< モード・タイプ切り替え >----------------")]
	[SerializeField]
	private Toggle[] tglDataType;

	[Header("---< 選択情報・キャラ >----------------------")]
	[SerializeField]
	private SelectInfoChara selInfoCha;

	[Header("---< アップロード >--------------------------")]
	[SerializeField]
	private UploadItem uploadItem;

	[Header("---< プロフィール >--------------------------")]
	[SerializeField]
	private ProfileItem profileItem;

	[Header("---< その他 >--------------------------------")]
	[SerializeField]
	private Button btnTitle;

	[SerializeField]
	private Button btnDownloaderHS2;

	[SerializeField]
	private Button btnDownloaderAI;

	[SerializeField]
	private Text textNewestVersion;

	private VoiceInfo.Param[] personalities;

	private IntReactiveProperty _dataType = new IntReactiveProperty(0);

	private BoolReactiveProperty _updateCharaInfo = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateFemale = new BoolReactiveProperty(initialValue: true);

	private BoolReactiveProperty _updateAllInfo = new BoolReactiveProperty(initialValue: false);

	private Dictionary<int, int> dictVoiceInfo = new Dictionary<int, int>();

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

	public bool updateFemale
	{
		get
		{
			return _updateFemale.Value;
		}
		set
		{
			_updateFemale.Value = value;
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
		if (null != textNewestVersion)
		{
			textNewestVersion.gameObject.SetActiveIfDifferent(active: true);
		}
	}

	public void ChangeUploadData()
	{
		uploadItem.modeUpdate = false;
		int user_idx = netInfo.profile.userIdx;
		if (dataType == 0)
		{
			updateCharaInfo = true;
			CustomCharaScrollController.ScrollData info = null;
			if (updateFemale)
			{
				info = netInfo.selectCharaFWindow.GetSelectInfo();
			}
			else
			{
				info = netInfo.selectCharaMWindow.GetSelectInfo();
			}
			if (info != null)
			{
				NetworkInfo.CharaInfo[] array = netInfo.lstCharaInfo.Where((NetworkInfo.CharaInfo x) => x.data_uid == info.info.data_uuid && x.user_idx == user_idx).ToArray();
				if (array.Length != 0)
				{
					uploadItem.modeUpdate = true;
				}
				else if (netInfo.dictUploaded[0].ContainsKey(info.info.data_uuid))
				{
					uploadItem.modeUpdate = true;
				}
			}
		}
		if ((bool)uploadItem.btnUpload)
		{
			Text componentInChildren = uploadItem.btnUpload.GetComponentInChildren<Text>(includeInactive: true);
			int languageInt = Singleton<GameSystem>.Instance.languageInt;
			if ((bool)componentInChildren)
			{
				componentInChildren.text = (uploadItem.modeUpdate ? NetworkDefine.strBtnUpload[languageInt, 0] : NetworkDefine.strBtnUpload[languageInt, 1]);
			}
		}
	}

	private void UpdatePreview(DataType type, string path)
	{
		Image image = null;
		if (type != DataType.Chara)
		{
			return;
		}
		image = selInfoCha.imgThumbnail;
		if (null == image)
		{
			return;
		}
		if (null != image.sprite)
		{
			if (null != image.sprite.texture)
			{
				UnityEngine.Object.Destroy(image.sprite.texture);
			}
			UnityEngine.Object.Destroy(image.sprite);
			image.sprite = null;
		}
		if (!path.IsNullOrEmpty())
		{
			Sprite sprite = PngAssist.LoadSpriteFromFile(path);
			if (null != sprite)
			{
				image.sprite = sprite;
			}
			image.enabled = true;
		}
		else
		{
			image.enabled = false;
		}
	}

	public void UpdateInfoChara()
	{
		string path = "";
		CustomCharaScrollController.ScrollData scrollData = null;
		scrollData = ((!updateFemale) ? netInfo.selectCharaMWindow.GetSelectInfo() : netInfo.selectCharaFWindow.GetSelectInfo());
		if (scrollData != null)
		{
			CustomCharaFileInfo info = scrollData.info;
			if (null != selInfoCha.textName)
			{
				selInfoCha.textName.text = info.name;
			}
			if (null != selInfoCha.textType)
			{
				selInfoCha.textType.text = ((info.sex == 0) ? "" : Singleton<Character>.Instance.GetCharaTypeName(info.voice));
			}
			if (null != selInfoCha.textBirthDay)
			{
				selInfoCha.textBirthDay.text = info.strBirthDay;
			}
			if (null != selInfoCha.textTrait)
			{
				if (Game.infoTraitTable.TryGetValue(info.trait, out var value))
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
				if (Game.infoMindTable.TryGetValue(info.mind, out var value2))
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
				if (Game.infoHAttributeTable.TryGetValue(info.hAttribute, out var value3))
				{
					selInfoCha.textHAttribute.text = value3;
				}
				else
				{
					selInfoCha.textHAttribute.text = "---------------";
				}
			}
			path = info.FullPath;
		}
		else
		{
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
		}
		UpdatePreview(DataType.Chara, path);
	}

	public string GetUploadFile(DataType type)
	{
		if (type == DataType.Chara)
		{
			CustomCharaScrollController.ScrollData scrollData = null;
			scrollData = ((!updateFemale) ? netInfo.selectCharaMWindow.GetSelectInfo() : netInfo.selectCharaFWindow.GetSelectInfo());
			if (scrollData == null)
			{
				return "";
			}
			return scrollData.info.FullPath;
		}
		return "";
	}

	public string GetComment(DataType type)
	{
		return uploadItem.bkComment[(int)type];
	}

	public string GetTitle()
	{
		string result = "NO_TITLE";
		if (null != uploadItem.inpTitle)
		{
			string text = uploadItem.inpTitle.text;
			if (!text.IsNullOrEmpty())
			{
				result = text;
			}
		}
		return result;
	}

	public void UpdateProfile()
	{
		profileItem.textHandle.text = Singleton<GameSystem>.Instance.HandleName;
	}

	private void Awake()
	{
		HashSet<GameObject> hashSet = new HashSet<GameObject>(objMale);
		hashSet.UnionWith(objFemale);
		objSexAll = hashSet.ToArray();
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		netInfo.selectCharaFWindow.UpdateWindowInUploader(modeNew: true, 1, save: false);
		netInfo.selectCharaFWindow.cscChara.onSelect = delegate
		{
			ChangeUploadData();
		};
		netInfo.selectCharaFWindow.cscChara.onDeSelect = delegate
		{
			ChangeUploadData();
		};
		netInfo.selectCharaMWindow.UpdateWindowInUploader(modeNew: true, 0, save: false);
		netInfo.selectCharaMWindow.cscChara.onSelect = delegate
		{
			ChangeUploadData();
		};
		netInfo.selectCharaMWindow.cscChara.onDeSelect = delegate
		{
			ChangeUploadData();
		};
		personalities = Voice.infoTable.Values.Where((VoiceInfo.Param x) => 0 <= x.No).ToArray();
		if (null != profileItem.btnChangeHandle)
		{
			profileItem.btnChangeHandle.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "EntryHandleName",
					isAdd = true,
					isFade = false,
					onLoad = delegate
					{
						string backHandleName = Singleton<GameSystem>.Instance.HandleName;
						EntryHandleName rootComponent = Scene.GetRootComponent<EntryHandleName>("EntryHandleName");
						if (rootComponent != null)
						{
							rootComponent.backSceneName = "Uploader";
						}
						rootComponent.onEnd = delegate
						{
							if (Singleton<GameSystem>.IsInstance() && backHandleName != Singleton<GameSystem>.Instance.HandleName)
							{
								netInfo.BlockUI();
								Observable.FromCoroutine((IObserver<string> res) => phpCtrl.UpdateHandleName(res)).Subscribe(delegate
								{
								}, delegate
								{
									Singleton<GameSystem>.Instance.SaveHandleName(backHandleName);
									netInfo.UnblockUI();
								}, delegate
								{
									UpdateProfile();
									if (netInfo.dictUserInfo.TryGetValue(netInfo.profile.userIdx, out var value))
									{
										value.handleName = Singleton<GameSystem>.Instance.HandleName;
									}
									netInfo.UnblockUI();
								});
							}
						};
					}
				}, isLoadingImageDraw: false);
			});
		}
		if (null != uploadItem.inpComment)
		{
			uploadItem.inpComment.OnEndEditAsObservable().Subscribe(delegate(string val)
			{
				uploadItem.bkComment[dataType] = val;
			});
		}
		if (null != uploadItem.tglAgreePolicy)
		{
			uploadItem.tglAgreePolicy.isOn = Singleton<GameSystem>.Instance.agreePolicy;
			uploadItem.tglAgreePolicy.OnValueChangedAsObservable().Subscribe(delegate(bool on)
			{
				if (Singleton<Game>.IsInstance())
				{
					if (Singleton<GameSystem>.Instance.agreePolicy != on)
					{
						Utils.Sound.Play(SystemSE.sel);
					}
					Singleton<GameSystem>.Instance.agreePolicy = on;
					Singleton<GameSystem>.Instance.SaveNetworkSetting();
				}
			});
		}
		if (null != uploadItem.btnPolicy)
		{
			uploadItem.btnPolicy.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.sel);
				if (null != uploadItem.objPolicy)
				{
					uploadItem.objPolicy.SetActiveIfDifferent(active: true);
				}
			});
		}
		if (null != uploadItem.exitPolicy)
		{
			(from _ in uploadItem.exitPolicy.UpdateAsObservable()
				where Input.anyKey
				select _).Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				if (null != uploadItem.objPolicy)
				{
					uploadItem.objPolicy.SetActiveIfDifferent(active: false);
				}
			});
		}
		if (null != uploadItem.btnUpload)
		{
			Text text = uploadItem.btnUpload.GetComponentInChildren<Text>(includeInactive: true);
			uploadItem.btnUpload.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				if (dataType == 0)
				{
					Observable.FromCoroutine((IObserver<bool> res) => phpCtrl.UploadChara(res, uploadItem.modeUpdate)).Subscribe(delegate
					{
					}, delegate
					{
					}, delegate
					{
					});
				}
			});
			uploadItem.btnUpload.UpdateAsObservable().Subscribe(delegate
			{
				bool flag = "" != GetUploadFile((DataType)dataType) && uploadItem.tglAgreePolicy.isOn;
				if (uploadItem.btnUpload.interactable != flag)
				{
					uploadItem.btnUpload.interactable = flag;
					text.color = new Color(text.color.r, text.color.g, text.color.b, flag ? 1f : 0.5f);
				}
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
		_dataType.Subscribe(delegate(int no)
		{
			if (no == 0)
			{
				if (null != objCharaTop)
				{
					objCharaTop.SetActiveIfDifferent(active: true);
				}
				if (null != uploadItem.inpComment)
				{
					uploadItem.inpComment.text = uploadItem.bkComment[0];
				}
			}
			ChangeUploadData();
		});
		_updateCharaInfo.Where((bool f) => f).Subscribe(delegate
		{
			UpdateInfoChara();
			updateCharaInfo = false;
		});
		_updateFemale.Subscribe(delegate(bool f)
		{
			GameObject[] array = objSexAll;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActiveIfDifferent(active: false);
			}
			array = objMale;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActiveIfDifferent(!f);
			}
			array = objFemale;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActiveIfDifferent(f);
			}
		});
		if (null != tglFemale)
		{
			tglFemale.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				updateFemale = isOn;
				updateCharaInfo = true;
			});
		}
		_updateAllInfo.Where((bool f) => f).Subscribe(delegate
		{
			updateCharaInfo = true;
			updateAllInfo = false;
		});
		if (null != btnTitle)
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
				}, isLoadingImageDraw: true);
			});
		}
		if (null != btnDownloaderHS2)
		{
			btnDownloaderHS2.OnClickAsObservable().Subscribe(delegate
			{
				netInfo.BlockUI();
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				Singleton<GameSystem>.Instance.networkType = 0;
				Singleton<GameSystem>.Instance.networkSceneName = "Downloader";
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "NetworkCheckScene",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			});
		}
		if (null != btnDownloaderAI)
		{
			btnDownloaderAI.OnClickAsObservable().Subscribe(delegate
			{
				netInfo.BlockUI();
				EventSystem.current.SetSelectedGameObject(null);
				Utils.Sound.Play(SystemSE.ok_s);
				Singleton<GameSystem>.Instance.networkType = 1;
				Singleton<GameSystem>.Instance.networkSceneName = "Downloader";
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "NetworkCheckScene",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			});
		}
	}
}
