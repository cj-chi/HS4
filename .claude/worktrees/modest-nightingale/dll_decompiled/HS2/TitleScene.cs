using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Actor;
using CameraEffector;
using CharaCustom;
using Config;
using Illusion.Anime;
using Illusion.Component.UI;
using Illusion.Extensions;
using Illusion.Game;
using Manager;
using UIAnimatorCore;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class TitleScene : MonoBehaviour
{
	[Serializable]
	public class ButtonGroup
	{
		public int group;

		public Button button;

		public Image imgNoSelect;

		public Image imgOnSelect;
	}

	public class ButtonStack<T> : Stack<T> where T : List<Button>
	{
		public new void Push(T item)
		{
			base.Push(item);
		}

		public new T Pop()
		{
			return base.Pop();
		}
	}

	[SerializeField]
	private GameObject objCamera;

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private ButtonGroup[] buttons;

	[SerializeField]
	private ConfigEffectorWet cofigEffector;

	[SerializeField]
	private Text version;

	[SerializeField]
	private GameObject objDof;

	[SerializeField]
	private CrossFade crossFade;

	[SerializeField]
	private List<UIAnimator> uiAnimators;

	[SerializeField]
	private SpriteChangeCtrl sccTitle;

	[SerializeField]
	private ObjectCategoryBehaviour obcLight;

	private Dictionary<int, List<Button>> buttonDic;

	private ButtonStack<List<Button>> buttonStack = new ButtonStack<List<Button>>();

	private ValueDictionary<int, int, List<TitleCharaStateInfo.Param>> dicCharaState = new ValueDictionary<int, int, List<TitleCharaStateInfo.Param>>();

	private Heroine heroine;

	private Controller ctrl;

	private bool isCustomMale;

	private List<(string, List<string>)> titleVoiceList = new List<(string, List<string>)>();

	private List<AudioClip> clipList = new List<AudioClip>();

	public void OnPlay()
	{
		Enter("Home");
	}

	public void OnMake()
	{
		buttonStack.Push(buttonDic[1]);
		canvasGroup.blocksRaycasts = false;
		uiAnimators[0].PlayAnimation(AnimSetupType.Outro, delegate
		{
			canvasGroup.blocksRaycasts = true;
		});
		uiAnimators[1].PlayAnimation(AnimSetupType.Intro);
		Enter(string.Empty);
	}

	public void OnLoader()
	{
		buttonStack.Push(buttonDic[2]);
		canvasGroup.blocksRaycasts = false;
		uiAnimators[0].PlayAnimation(AnimSetupType.Outro, delegate
		{
			canvasGroup.blocksRaycasts = true;
		});
		uiAnimators[2].PlayAnimation(AnimSetupType.Intro);
		Enter(string.Empty);
	}

	public void OnConfig()
	{
		Enter("Config");
	}

	public void OnEnd()
	{
		Enter("End");
	}

	public void OnMakeFemale()
	{
		isCustomMale = false;
		Enter("CharaCustom");
	}

	public void OnMakeMale()
	{
		isCustomMale = true;
		Enter("CharaCustom");
	}

	public void OnUpload()
	{
		Singleton<GameSystem>.Instance.networkType = 0;
		Enter("Uploader");
	}

	public void OnDownload()
	{
		Singleton<GameSystem>.Instance.networkType = 0;
		Enter("Downloader");
	}

	public void OnDownloadAI()
	{
		Singleton<GameSystem>.Instance.networkType = 1;
		Enter("Downloader");
	}

	public void OnBack()
	{
		buttonStack.Pop();
		canvasGroup.blocksRaycasts = false;
		uiAnimators[0].PlayAnimation(AnimSetupType.Intro, delegate
		{
			canvasGroup.blocksRaycasts = true;
		});
		if (uiAnimators[1].CurrentAnimType == AnimSetupType.Intro)
		{
			uiAnimators[1].PlayAnimation(AnimSetupType.Outro);
		}
		if (uiAnimators[2].CurrentAnimType == AnimSetupType.Intro)
		{
			uiAnimators[2].PlayAnimation(AnimSetupType.Outro);
		}
		Enter("Back");
	}

	private void ButtonInteractable(string name, Func<bool> func)
	{
		ButtonGroup[] array = buttons;
		foreach (ButtonGroup buttonGroup in array)
		{
			if (buttonGroup.button.name.IndexOf(name) != -1)
			{
				if (!func.IsNullOrEmpty())
				{
					buttonGroup.button.interactable = func();
				}
				break;
			}
		}
	}

	private void Enter(string next)
	{
		if (!graphicRaycaster.enabled)
		{
			return;
		}
		if (next == "Back")
		{
			Utils.Sound.Play(SystemSE.cancel);
		}
		else
		{
			Utils.Sound.Play(SystemSE.ok_s);
		}
		switch (next)
		{
		case "Home":
			if (Singleton<Game>.Instance.saveData.TutorialNo == 0)
			{
				Singleton<ADVManager>.Instance.filenames[0] = "";
				Singleton<ADVManager>.Instance.filenames[1] = "";
				Singleton<ADVManager>.Instance.advDelivery.Set("0", -10, 0);
				Singleton<Game>.Instance.mapNo = 500;
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "ADV",
					fadeType = FadeCanvas.Fade.In,
					onLoad = delegate
					{
						Singleton<Character>.Instance.DeleteCharaAll();
					}
				}, isLoadingImageDraw: true);
			}
			else if (Singleton<Game>.Instance.saveData.TutorialNo == 8)
			{
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "LobbyScene",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
			}
			else
			{
				Scene.LoadReserve(new Scene.Data
				{
					levelName = "Home",
					fadeType = FadeCanvas.Fade.In
				}, isLoadingImageDraw: true);
				Singleton<Game>.Instance.CharaEventShuffle();
			}
			break;
		case "CharaCustom":
			global::CharaCustom.CharaCustom.modeNew = true;
			global::CharaCustom.CharaCustom.modeSex = ((!isCustomMale) ? ((byte)1) : ((byte)0));
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "CharaCustom",
				fadeType = FadeCanvas.Fade.In,
				onLoad = delegate
				{
					Singleton<Character>.Instance.DeleteCharaAll();
				}
			}, isLoadingImageDraw: true);
			break;
		case "Downloader":
			Singleton<GameSystem>.Instance.networkSceneName = "Downloader";
			ChangeScene();
			break;
		case "Uploader":
			Singleton<GameSystem>.Instance.networkSceneName = "Uploader";
			ChangeScene();
			break;
		case "Config":
			ConfigWindow.UnLoadAction = delegate
			{
				StartCoroutine(ConfigEnd());
			};
			ConfigWindow.Load();
			break;
		case "End":
			ExitDialog.GameEnd(isCheck: true);
			break;
		}
	}

	private void ChangeScene()
	{
		if (!Singleton<GameSystem>.Instance.HandleName.IsNullOrEmpty())
		{
			Scene.LoadReserveAsyncForget(new Scene.Data
			{
				levelName = "NetworkCheckScene",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		}
		else
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "EntryHandleName",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
		}
	}

	private IEnumerator LoadChara()
	{
		heroine = null;
		TitleSystem titleData = Manager.Config.TitleData;
		if (!titleData.isTitleCharaLoad)
		{
			yield break;
		}
		ChaFileControl chaFileControl = new ChaFileControl();
		int fixCharaID = 0;
		ChaControl chaControl;
		if (!titleData.isUseUserCharaCard || (titleData.isUseUserCharaCard && !chaFileControl.LoadCharaFile(titleData.charaCardFileNameFullPath, 1)))
		{
			chaControl = Singleton<Character>.Instance.GetChara(-1);
			if (chaControl == null)
			{
				chaControl = Singleton<Character>.Instance.CreateChara(1, base.gameObject, -1);
				Singleton<Character>.Instance.LoadConciergeCharaFile(chaControl);
				fixCharaID = -1;
			}
		}
		else
		{
			chaControl = Singleton<Character>.Instance.CreateChara(1, base.gameObject, 0, chaFileControl);
		}
		chaControl.ChangeNowCoordinate();
		chaControl.releaseCustomInputTexture = false;
		chaControl.Load();
		chaControl.ChangeLookEyesPtn(1);
		chaControl.ChangeLookNeckPtn(1);
		ctrl = new Controller(chaControl);
		chaControl.visibleAll = false;
		heroine = new Heroine(chaControl.chaFile, isRandomize: false);
		heroine.SetRoot(chaControl.gameObject);
		heroine.fixCharaID = fixCharaID;
		GameObject referenceInfo = chaControl.GetReferenceInfo(ChaReference.RefObjKey.HeadParent);
		if ((bool)referenceInfo)
		{
			cofigEffector.dof.focalTransform = referenceInfo.transform;
		}
		yield return null;
	}

	private void SetMotionIKAndAnimationPlay(TitleCharaStateInfo.Param _param)
	{
		if (heroine == null)
		{
			return;
		}
		ChaControl chaCtrl = heroine.chaCtrl;
		if (!(chaCtrl == null) && _param != null)
		{
			if (!PersonalPoseInfoTable.InfoTable.TryGetValue(heroine.voiceNo, out var value))
			{
				ctrl.PlayID(0);
				return;
			}
			if (!value.TryGetValue(_param.animationID, out var value2))
			{
				ctrl.PlayID(0);
				return;
			}
			ctrl.PlayID(value2.poseIDs.Shuffle().FirstOrDefault());
			Singleton<Game>.Instance.GetExpression(heroine?.voiceNo ?? 0, value2.exp)?.Change(chaCtrl);
		}
	}

	private void SetPosition(TitleCharaStateInfo.Param _param)
	{
		if (_param == null)
		{
			return;
		}
		if (_param.id == -1)
		{
			TransformInfoData transformInfoData = CommonLib.LoadAsset<TransformInfoData>(_param.posBundle, _param.posFile);
			if ((bool)transformInfoData && (bool)objDof)
			{
				transformInfoData.Reflect(objDof.transform);
				cofigEffector.dof.focalTransform = objDof.transform;
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.posBundle, isUnloadForceRefCount: true);
			}
			transformInfoData = CommonLib.LoadAsset<TransformInfoData>(_param.camBundle, _param.camFile);
			if ((bool)transformInfoData && (bool)objCamera)
			{
				transformInfoData.Reflect(objCamera.transform);
			}
			if ((bool)transformInfoData)
			{
				AssetBundleManager.UnloadAssetBundle(_param.camBundle, isUnloadForceRefCount: true);
			}
		}
		else
		{
			if (heroine == null)
			{
				return;
			}
			ChaControl chaCtrl = heroine.chaCtrl;
			if (!(chaCtrl == null))
			{
				TransformCompositeInfoData transformCompositeInfoData = CommonLib.LoadAsset<TransformCompositeInfoData>(_param.camBundle, _param.camFile);
				if ((bool)transformCompositeInfoData && (bool)chaCtrl && (bool)objCamera)
				{
					transformCompositeInfoData.Reflect(objCamera.transform, chaCtrl.GetShapeBodyValue(0));
				}
				if ((bool)transformCompositeInfoData)
				{
					AssetBundleManager.UnloadAssetBundle(_param.camBundle, isUnloadForceRefCount: true);
				}
				TransformInfoData transformInfoData2 = CommonLib.LoadAsset<TransformInfoData>(_param.posBundle, _param.posFile);
				if ((bool)transformInfoData2 && (bool)chaCtrl)
				{
					transformInfoData2.Reflect(chaCtrl.transform);
				}
				if ((bool)transformInfoData2)
				{
					AssetBundleManager.UnloadAssetBundle(_param.posBundle, isUnloadForceRefCount: true);
				}
				chaCtrl.resetDynamicBoneAll = true;
			}
		}
	}

	private void SetCharaAnimationAndPosition()
	{
		TitleSystem titleData = Manager.Config.TitleData;
		TitleCharaStateInfo.Param param = null;
		param = ((!titleData.isTitleCharaLoad || heroine == null || !(heroine.chaCtrl != null)) ? dicCharaState[-1][-1].Shuffle().FirstOrDefault() : ((UnityEngine.Random.Range(0, 2) != 0) ? dicCharaState[1][(heroine.gameinfo2.nowDrawState == ChaFileDefine.State.Broken) ? 1 : 0].Shuffle().FirstOrDefault() : dicCharaState[0][(int)heroine.gameinfo2.nowDrawState].Shuffle().FirstOrDefault()));
		SetMotionIKAndAnimationPlay(param);
		SetPosition(param);
	}

	public IEnumerator ConfigEnd()
	{
		crossFade.FadeStart();
		Singleton<Character>.Instance.DeleteCharaAll();
		yield return StartCoroutine(LoadChara());
		yield return null;
		SetCharaAnimationAndPosition();
		if (heroine != null)
		{
			heroine.chaCtrl.visibleAll = true;
		}
	}

	private IEnumerator LoadList()
	{
		foreach (TitleCharaStateInfo item in GlobalMethod.LoadAllFolder<TitleCharaStateInfo>(AssetBundleNames.TitleListPath, "titlecharapos", null, _subdirCheck: true))
		{
			foreach (TitleCharaStateInfo.Param p in item.param)
			{
				if (!dicCharaState.ContainsKey(p.pose))
				{
					dicCharaState[p.pose] = dicCharaState.New();
				}
				ValueDictionary<int, List<TitleCharaStateInfo.Param>> valueDictionary = dicCharaState[p.pose];
				if (!valueDictionary.ContainsKey(p.state))
				{
					valueDictionary[p.state] = new List<TitleCharaStateInfo.Param>();
				}
				List<TitleCharaStateInfo.Param> list = valueDictionary[p.state];
				TitleCharaStateInfo.Param param = list.Find((TitleCharaStateInfo.Param f) => f.id == p.id);
				if (param == null)
				{
					list.Add(new TitleCharaStateInfo.Param(p));
				}
				else
				{
					param.Copy(p);
				}
			}
		}
		yield return null;
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		TitleSystem title = Manager.Config.TitleData;
		while (!Singleton<GameSystem>.IsInstance())
		{
			yield return null;
		}
		version.text = "Ver " + Singleton<GameSystem>.Instance.GameVersion;
		Voice.StopAll();
		Manager.Sound.Stop(Manager.Sound.Type.GameSE3D);
		Utils.Sound.Play(new Utils.Sound.SettingBGM(GameSystem.isAdd50 ? BGM.title2 : BGM.title));
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		buttonDic = buttons.ToLookup((ButtonGroup p) => p.group, (ButtonGroup p) => p.button).ToDictionary((IGrouping<int, Button> p) => p.Key, (IGrouping<int, Button> p) => p.ToList());
		buttonStack.Push(buttonDic[0]);
		ButtonGroup[] array = buttons;
		foreach (ButtonGroup b in array)
		{
			b.button.OnPointerEnterAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.sel);
				b.imgOnSelect.enabled = true;
				b.imgNoSelect.enabled = false;
			});
			b.button.OnPointerExitAsObservable().Subscribe(delegate
			{
				b.imgOnSelect.enabled = false;
				b.imgNoSelect.enabled = true;
			});
		}
		this.ObserveEveryValueChanged((TitleScene _) => !Scene.IsNowLoadingFade).Subscribe(delegate(bool isOn)
		{
			canvasGroup.blocksRaycasts = isOn;
		});
		HomeScene.startCanvas = 0;
		HomeScene.startCharaEdit = -1;
		Scene.UnloadBaseScene();
		Scene.sceneFadeCanvas.DefaultColor();
		Scene.UnloadAddScene();
		yield return StartCoroutine(LoadList());
		Singleton<Character>.Instance.DeleteCharaAll();
		Singleton<Game>.Instance.GameParameterInit();
		yield return StartCoroutine(LoadChara());
		int no = (GameSystem.isAdd50 ? 18 : 0);
		yield return BaseMap.ChangeAsync(no, FadeCanvas.Fade.None).ToCoroutine();
		SetCharaAnimationAndPosition();
		if (heroine != null)
		{
			heroine.chaCtrl.visibleAll = true;
		}
		string[] source = new string[5] { "abdata", "sound/data/mixer/00.unity3d", "adv/00_base.unity3d", "chara/00/mt_ramp_00.unity3d", "sound/data/bgm/bgm_00.unity3d" };
		foreach (KeyValuePair<string, AssetBundleManager.BundlePack> item in AssetBundleManager.ManifestBundlePack)
		{
			string key = item.Key;
			KeyValuePair<string, LoadedAssetBundle>[] array2 = item.Value.LoadedAssetBundles.ToArray();
			foreach (KeyValuePair<string, LoadedAssetBundle> keyValuePair in array2)
			{
				string key2 = keyValuePair.Key;
				if (!(key2 == key) && !source.Contains(key2))
				{
					AssetBundleManager.UnloadAssetBundle(key2, isUnloadForceRefCount: true, key);
				}
			}
		}
		UnityEngine.Resources.UnloadUnusedAssets();
		GC.Collect();
		Scene.isReturnTitle = false;
		this.ObserveEveryValueChanged((TitleScene _) => Scene.AddSceneName).Subscribe(delegate(string sceneName)
		{
			graphicRaycaster.enabled = sceneName == string.Empty;
		});
		string text = "sound/data/systemse/titlecall/";
		AssetBundleData.GetAssetBundleNameListFromPath(text, subdirCheck: true).ForEach(delegate(string file)
		{
			if (GameSystem.IsPathAdd50(file))
			{
				titleVoiceList.Add((file, GlobalMethod.GetAllAssetName(file, _WithExtension: true, null, isAllCheck: false, _forceAssetBundleCheck: true).ToList()));
				AssetBundleManager.UnloadAssetBundle(file, isUnloadForceRefCount: true);
			}
		});
		if (title.isTitleCharaLoad)
		{
			string chaVoice = heroine.ChaVoice;
			string voiceFile = chaVoice + ".wav";
			(string, List<string>) callVoice = titleVoiceList.FirstOrDefault(((string, List<string>) l) => l.Item2.Contains(voiceFile));
			if (callVoice.Item1.IsNullOrEmpty())
			{
				callVoice.Item1 = text + "30.unity3d";
				chaVoice = "c00";
			}
			(from _ in this.UpdateAsObservable()
				where !Scene.IsFadeNow
				select _).Take(1).Subscribe(delegate
			{
				AudioSource voiceTransform = Voice.OncePlay(new Voice.Loader
				{
					no = heroine.FixCharaIDOrPersonality,
					bundle = callVoice.Item1,
					asset = chaVoice
				});
				if (heroine != null && (bool)heroine.chaCtrl)
				{
					heroine.chaCtrl.SetVoiceTransform(voiceTransform);
				}
				canvasGroup.blocksRaycasts = false;
				uiAnimators[0].PlayAnimation(AnimSetupType.Intro, delegate
				{
					canvasGroup.blocksRaycasts = true;
				});
			});
		}
		else
		{
			(string, List<string>) _bundle = titleVoiceList.Shuffle().FirstOrDefault();
			if (_bundle.Item2 != null)
			{
				string _file = _bundle.Item2.Shuffle().FirstOrDefault();
				int.TryParse(YS_Assist.GetStringRight(_file, 2), out var _);
				(from _ in this.UpdateAsObservable()
					where !Scene.IsFadeNow
					select _).Take(1).Subscribe(delegate
				{
					Utils.Sound.Play(new Utils.Sound.Setting(Manager.Sound.Type.SystemSE)
					{
						bundle = _bundle.Item1,
						asset = _file
					});
					canvasGroup.blocksRaycasts = false;
					uiAnimators[0].PlayAnimation(AnimSetupType.Intro, delegate
					{
						canvasGroup.blocksRaycasts = true;
					});
				});
			}
		}
		uiAnimators[1].SetAnimType(AnimSetupType.Outro);
		uiAnimators[1].ResetToEnd();
		uiAnimators[2].SetAnimType(AnimSetupType.Outro);
		uiAnimators[2].ResetToEnd();
		Manager.Sound.Listener = mainCamera.transform;
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1)
			where buttonStack.Count > 1
			where canvasGroup.blocksRaycasts
			select _).Subscribe(delegate
		{
			OnBack();
		});
		if (GameSystem.isAdd50)
		{
			sccTitle.ChangeValue(1);
			obcLight.SetActiveToggle(1);
		}
		base.enabled = true;
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
	}
}
