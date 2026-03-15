using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illusion.Elements.Xml;
using Manager;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public sealed class Studio : Singleton<Studio>
{
	private class DuplicateParentInfo
	{
		public ObjectCtrlInfo info;

		public TreeNodeObject node;

		public DuplicateParentInfo(ObjectCtrlInfo _info, TreeNodeObject _node)
		{
			info = _info;
			node = _node;
		}
	}

	public const string savePath = "studio/scene";

	[SerializeField]
	private TreeNodeCtrl m_TreeNodeCtrl;

	[SerializeField]
	private RootButtonCtrl m_RootButtonCtrl;

	[SerializeField]
	private ManipulatePanelCtrl _manipulatePanelCtrl;

	[SerializeField]
	private CameraControl m_CameraCtrl;

	[SerializeField]
	private SystemButtonCtrl m_SystemButtonCtrl;

	[SerializeField]
	private CameraLightCtrl m_CameraLightCtrl;

	[SerializeField]
	private MapList _mapList;

	[SerializeField]
	private ColorPalette _colorPalette;

	[SerializeField]
	private WorkspaceCtrl m_WorkspaceCtrl;

	[SerializeField]
	private BackgroundCtrl m_BackgroundCtrl;

	[SerializeField]
	private BackgroundList m_BackgroundList;

	[SerializeField]
	private PatternSelectListCtrl _patternSelectListCtrl;

	[SerializeField]
	private GameScreenShot _gameScreenShot;

	[SerializeField]
	private FrameCtrl _frameCtrl;

	[SerializeField]
	private FrameList _frameList;

	[SerializeField]
	private LogoList _logoList;

	[SerializeField]
	private InputField _inputFieldNow;

	[SerializeField]
	private TMP_InputField _inputFieldTMPNow;

	[Space]
	[SerializeField]
	private RectTransform rectName;

	[SerializeField]
	private TextMeshProUGUI textName;

	private SingleAssignmentDisposable disposableName;

	[Space]
	[SerializeField]
	private Image imageCamera;

	[SerializeField]
	private TextMeshProUGUI textCamera;

	[SerializeField]
	private CameraSelector _cameraSelector;

	[Space]
	[SerializeField]
	private Texture _textureLine;

	[SerializeField]
	private RouteControl _routeControl;

	public Dictionary<TreeNodeObject, ObjectCtrlInfo> dicInfo = new Dictionary<TreeNodeObject, ObjectCtrlInfo>();

	public Dictionary<int, ObjectCtrlInfo> dicObjectCtrl = new Dictionary<int, ObjectCtrlInfo>();

	public Dictionary<int, ChangeAmount> dicChangeAmount = new Dictionary<int, ChangeAmount>();

	private const string UserPath = "studio";

	private const string FileName = "option.xml";

	private const string RootName = "Option";

	private Control xmlCtrl;

	private OCICamera _ociCamera;

	public Action<ObjectCtrlInfo> onDelete;

	public Action onChangeMap;

	private FileData fdApplicationPath;

	public WorkInfo workInfo = new WorkInfo();

	public TreeNodeCtrl treeNodeCtrl => m_TreeNodeCtrl;

	public RootButtonCtrl rootButtonCtrl => m_RootButtonCtrl;

	public ManipulatePanelCtrl manipulatePanelCtrl => _manipulatePanelCtrl;

	public CameraControl cameraCtrl => m_CameraCtrl;

	public SystemButtonCtrl systemButtonCtrl => m_SystemButtonCtrl;

	public BGMCtrl bgmCtrl => sceneInfo.bgmCtrl;

	public ENVCtrl envCtrl => sceneInfo.envCtrl;

	public OutsideSoundCtrl outsideSoundCtrl => sceneInfo.outsideSoundCtrl;

	public CameraLightCtrl cameraLightCtrl => m_CameraLightCtrl;

	public MapList mapList => _mapList;

	public ColorPalette colorPalette => _colorPalette;

	public PatternSelectListCtrl patternSelectListCtrl => _patternSelectListCtrl;

	public GameScreenShot gameScreenShot => _gameScreenShot;

	public FrameCtrl frameCtrl => _frameCtrl;

	public LogoList logoList => _logoList;

	public bool isInputNow
	{
		get
		{
			if (!_inputFieldNow)
			{
				if (!_inputFieldTMPNow)
				{
					return false;
				}
				return _inputFieldTMPNow.isFocused;
			}
			return _inputFieldNow.isFocused;
		}
	}

	public CameraSelector cameraSelector => _cameraSelector;

	public Texture textureLine => _textureLine;

	public RouteControl routeControl => _routeControl;

	public SceneInfo sceneInfo { get; private set; }

	public static OptionSystem optionSystem { get; private set; }

	public OCICamera ociCamera
	{
		get
		{
			return _ociCamera;
		}
		private set
		{
			_ociCamera = value;
		}
	}

	public int cameraCount { get; private set; }

	public bool isVRMode { get; private set; }

	public string ApplicationPath
	{
		get
		{
			if (fdApplicationPath == null)
			{
				fdApplicationPath = new FileData();
			}
			return fdApplicationPath.Path;
		}
	}

	public void AddFemale(string _path)
	{
		OCICharFemale oCICharFemale = AddObjectFemale.Add(_path);
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoHide)
		{
			rootButtonCtrl.OnClick(-1);
		}
		if (optionSystem.autoSelect && oCICharFemale != null)
		{
			m_TreeNodeCtrl.SelectSingle(oCICharFemale.treeNodeObject);
		}
	}

	private IEnumerator AddFemaleCoroutine(string _path)
	{
		AddObjectFemale.NecessaryInfo ni = new AddObjectFemale.NecessaryInfo(_path);
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "StudioWait",
			isAdd = true
		}, isLoadingImageDraw: false);
		yield return null;
		yield return AddObjectFemale.AddCoroutine(ni);
		Scene.Unload();
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoHide)
		{
			rootButtonCtrl.OnClick(-1);
		}
		if (optionSystem.autoSelect && ni.ocicf != null)
		{
			m_TreeNodeCtrl.SelectSingle(ni.ocicf.treeNodeObject);
		}
	}

	public void AddMale(string _path)
	{
		OCICharMale oCICharMale = AddObjectMale.Add(_path);
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoHide)
		{
			rootButtonCtrl.OnClick(-1);
		}
		if (optionSystem.autoSelect && oCICharMale != null)
		{
			m_TreeNodeCtrl.SelectSingle(oCICharMale.treeNodeObject);
		}
	}

	public void AddMap(int _no, bool _close = true, bool _wait = true, bool _coroutine = true)
	{
		if (_coroutine)
		{
			StartCoroutine(AddMapCoroutine(_no, _close, _wait));
			return;
		}
		Singleton<Map>.Instance.LoadMap(_no);
		SetupMap(_no, _close);
	}

	private IEnumerator AddMapCoroutine(int _no, bool _close, bool _wait)
	{
		yield return Singleton<Map>.Instance.LoadMapCoroutine(_no, _wait);
		SetupMap(_no, _close);
	}

	private async void AddMapAsync(int _no, bool _close, bool _wait)
	{
		if (_wait)
		{
			Wait.Active = true;
		}
		await BaseMap.ChangeAsync(_no, FadeCanvas.Fade.None);
		if (_wait)
		{
			Wait.Active = false;
		}
		SetupMap(_no, _close);
	}

	private void SetupMap(int _no, bool _close)
	{
		sceneInfo.mapInfo.no = _no;
		sceneInfo.mapInfo.ca.Reset();
		if (onChangeMap != null)
		{
			onChangeMap();
		}
		m_CameraCtrl.CloerListCollider();
		Info.MapLoadInfo value = null;
		if (Singleton<Info>.Instance.dicMapLoadInfo.TryGetValue(Singleton<Map>.Instance.no, out value))
		{
			m_CameraCtrl.LoadVanish(value.vanish.bundlePath, value.vanish.fileName, Singleton<Map>.Instance.MapRoot);
		}
		if (_close)
		{
			rootButtonCtrl.OnClick(-1);
		}
	}

	public void AddItem(int _group, int _category, int _no)
	{
		OCIItem oCIItem = AddObjectItem.Add(_group, _category, _no);
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoHide)
		{
			rootButtonCtrl.OnClick(-1);
		}
		if (optionSystem.autoSelect && oCIItem != null)
		{
			m_TreeNodeCtrl.SelectSingle(oCIItem.treeNodeObject);
		}
	}

	public void AddLight(int _no)
	{
		OCILight oCILight = AddObjectLight.Add(_no);
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoHide)
		{
			rootButtonCtrl.OnClick(-1);
		}
		if (optionSystem.autoSelect && oCILight != null)
		{
			m_TreeNodeCtrl.SelectSingle(oCILight.treeNodeObject);
		}
	}

	public void AddFolder()
	{
		OCIFolder oCIFolder = AddObjectFolder.Add();
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoSelect && oCIFolder != null)
		{
			m_TreeNodeCtrl.SelectSingle(oCIFolder.treeNodeObject);
		}
	}

	public void AddCamera()
	{
		if (cameraCount != int.MaxValue)
		{
			cameraCount++;
		}
		OCICamera oCICamera = AddObjectCamera.Add();
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoSelect && oCICamera != null)
		{
			m_TreeNodeCtrl.SelectSingle(oCICamera.treeNodeObject);
		}
		_cameraSelector.Init();
	}

	public void ChangeCamera(OCICamera _ociCamera, bool _active, bool _force = false)
	{
		if (_active)
		{
			if (ociCamera != null && ociCamera != _ociCamera)
			{
				ociCamera.SetActive(_active: false);
				ociCamera = null;
			}
			if (_ociCamera != null)
			{
				_ociCamera.SetActive(_active: true);
				ociCamera = _ociCamera;
			}
		}
		else if (_force)
		{
			if (ociCamera != null)
			{
				ociCamera.SetActive(_active: false);
			}
			_ociCamera?.SetActive(_active: false);
			ociCamera = null;
		}
		else if (ociCamera == _ociCamera)
		{
			_ociCamera?.SetActive(_active: false);
			ociCamera = null;
		}
		Singleton<Studio>.Instance.cameraCtrl.IsOutsideSetting = ociCamera != null;
		textCamera.text = ((ociCamera == null) ? "-" : ociCamera.cameraInfo.name);
		_cameraSelector.SetCamera(ociCamera);
	}

	public void ChangeCamera(OCICamera _ociCamera)
	{
		ChangeCamera(_ociCamera, ociCamera != _ociCamera);
	}

	public void DeleteCamera(OCICamera _ociCamera)
	{
		if (ociCamera != _ociCamera)
		{
			_cameraSelector.Init();
			return;
		}
		ociCamera.SetActive(_active: false);
		ociCamera = null;
		Singleton<Studio>.Instance.cameraCtrl.enabled = true;
		textCamera.text = "-";
		_cameraSelector.Init();
	}

	public void AddRoute()
	{
		OCIRoute oCIRoute = AddObjectRoute.Add();
		if (_routeControl.visible)
		{
			_routeControl.Init();
		}
		Singleton<UndoRedoManager>.Instance.Clear();
		if (optionSystem.autoSelect && oCIRoute != null)
		{
			m_TreeNodeCtrl.SelectSingle(oCIRoute.treeNodeObject);
		}
	}

	public void SetDepthOfFieldForcus(int _key)
	{
		m_SystemButtonCtrl.SetDepthOfFieldForcus(_key);
	}

	public void SetSunCaster(int _key)
	{
		m_SystemButtonCtrl.SetSunCaster(_key);
	}

	public void UpdateCharaFKColor()
	{
		foreach (KeyValuePair<int, ObjectCtrlInfo> item in dicObjectCtrl.Where((KeyValuePair<int, ObjectCtrlInfo> v) => v.Value is OCIChar))
		{
			(item.Value as OCIChar).UpdateFKColor(FKCtrl.parts);
		}
	}

	public void UpdateItemFKColor()
	{
		foreach (KeyValuePair<int, ObjectCtrlInfo> item in dicObjectCtrl.Where((KeyValuePair<int, ObjectCtrlInfo> v) => v.Value is OCIItem))
		{
			(item.Value as OCIItem).UpdateFKColor();
		}
	}

	public void SetVisibleGimmick()
	{
		bool visibleGimmick = workInfo.visibleGimmick;
		foreach (OCIItem item in from v in dicObjectCtrl
			where v.Value is OCIItem
			select v.Value as OCIItem)
		{
			item.VisibleIcon = visibleGimmick;
		}
	}

	public void Duplicate()
	{
		Dictionary<int, ObjectInfo> dictionary = new Dictionary<int, ObjectInfo>();
		Dictionary<int, DuplicateParentInfo> dictionary2 = new Dictionary<int, DuplicateParentInfo>();
		TreeNodeObject[] selectNodes = treeNodeCtrl.selectNodes;
		for (int i = 0; i < selectNodes.Length; i++)
		{
			SavePreprocessingLoop(selectNodes[i]);
			ObjectCtrlInfo value = null;
			if (dicInfo.TryGetValue(selectNodes[i], out value))
			{
				dictionary.Add(value.objectInfo.dicKey, value.objectInfo);
				if (value.parentInfo != null)
				{
					dictionary2.Add(value.objectInfo.dicKey, new DuplicateParentInfo(value.parentInfo, value.treeNodeObject.parent));
				}
			}
		}
		if (dictionary.Count == 0)
		{
			return;
		}
		byte[] buffer = null;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using BinaryWriter writer = new BinaryWriter(memoryStream);
			sceneInfo.Save(writer, dictionary);
			buffer = memoryStream.ToArray();
		}
		using (MemoryStream input = new MemoryStream(buffer))
		{
			using BinaryReader reader = new BinaryReader(input);
			sceneInfo.Import(reader, sceneInfo.version);
		}
		foreach (KeyValuePair<int, ObjectInfo> item in sceneInfo.dicImport)
		{
			DuplicateParentInfo value2 = null;
			if (dictionary2.TryGetValue(sceneInfo.dicChangeKey[item.Key], out value2))
			{
				AddObjectAssist.LoadChild(item.Value, value2.info, value2.node);
			}
			else
			{
				AddObjectAssist.LoadChild(item.Value);
			}
		}
		if (_routeControl.visible)
		{
			_routeControl.Init();
		}
		treeNodeCtrl.RefreshHierachy();
		_cameraSelector.Init();
	}

	public void SaveScene()
	{
		foreach (KeyValuePair<int, ObjectCtrlInfo> item in dicObjectCtrl)
		{
			item.Value.OnSavePreprocessing();
		}
		sceneInfo.cameraSaveData = m_CameraCtrl.Export();
		DateTime now = DateTime.Now;
		string text = $"{now.Year}_{now.Month:00}{now.Day:00}_{now.Hour:00}{now.Minute:00}_{now.Second:00}_{now.Millisecond:000}.png";
		string path = UserData.Create("studio/scene") + text;
		sceneInfo.Save(path);
	}

	public bool LoadScene(string _path)
	{
		if (!File.Exists(_path))
		{
			return false;
		}
		InitScene(_close: false);
		sceneInfo = new SceneInfo();
		if (!sceneInfo.Load(_path))
		{
			return false;
		}
		AddObjectAssist.LoadChild(sceneInfo.dicObject);
		ChangeAmount source = sceneInfo.mapInfo.ca.Clone();
		AddMap(sceneInfo.mapInfo.no, _close: false, _wait: false);
		mapList.UpdateInfo();
		sceneInfo.mapInfo.ca.Copy(source);
		Singleton<MapCtrl>.Instance.Reflect();
		bgmCtrl.Play(bgmCtrl.no);
		envCtrl.Play(envCtrl.no);
		outsideSoundCtrl.Play(outsideSoundCtrl.fileName);
		m_BackgroundCtrl.Load(sceneInfo.background);
		m_BackgroundList.UpdateUI();
		_frameCtrl.Load(sceneInfo.frame);
		_frameList.UpdateUI();
		m_SystemButtonCtrl.UpdateInfo();
		treeNodeCtrl.RefreshHierachy();
		if (sceneInfo.cameraSaveData != null)
		{
			m_CameraCtrl.Import(sceneInfo.cameraSaveData);
		}
		cameraLightCtrl.Reflect();
		_cameraSelector.Init();
		sceneInfo.dataVersion = sceneInfo.version;
		rootButtonCtrl.OnClick(-1);
		return true;
	}

	public IEnumerator LoadSceneCoroutine(string _path)
	{
		if (!File.Exists(_path))
		{
			yield break;
		}
		InitScene(_close: false);
		yield return null;
		sceneInfo = new SceneInfo();
		if (sceneInfo.Load(_path))
		{
			AddObjectAssist.LoadChild(sceneInfo.dicObject);
			ChangeAmount ca = sceneInfo.mapInfo.ca.Clone();
			yield return AddMapCoroutine(sceneInfo.mapInfo.no, _close: false, _wait: false);
			mapList.UpdateInfo();
			sceneInfo.mapInfo.ca.Copy(ca);
			Singleton<MapCtrl>.Instance.Reflect();
			bgmCtrl.Play(bgmCtrl.no);
			envCtrl.Play(envCtrl.no);
			outsideSoundCtrl.Play(outsideSoundCtrl.fileName);
			m_BackgroundCtrl.Load(sceneInfo.background);
			m_BackgroundList.UpdateUI();
			_frameCtrl.Load(sceneInfo.frame);
			_frameList.UpdateUI();
			m_SystemButtonCtrl.UpdateInfo();
			treeNodeCtrl.RefreshHierachy();
			if (sceneInfo.cameraSaveData != null)
			{
				m_CameraCtrl.Import(sceneInfo.cameraSaveData);
			}
			cameraLightCtrl.Reflect();
			_cameraSelector.Init();
			rootButtonCtrl.OnClick(-1);
		}
	}

	public bool ImportScene(string _path)
	{
		if (!File.Exists(_path))
		{
			return false;
		}
		if (!sceneInfo.Import(_path))
		{
			return false;
		}
		AddObjectAssist.LoadChild(sceneInfo.dicImport);
		treeNodeCtrl.RefreshHierachy();
		_cameraSelector.Init();
		return true;
	}

	public void InitScene(bool _close = true)
	{
		ChangeCamera(null, _active: false, _force: true);
		cameraCount = 0;
		treeNodeCtrl.DeleteAllNode();
		Singleton<GuideObjectManager>.Instance.DeleteAll();
		m_RootButtonCtrl.OnClick(-1);
		m_RootButtonCtrl.objectCtrlInfo = null;
		foreach (KeyValuePair<TreeNodeObject, ObjectCtrlInfo> item in dicInfo)
		{
			switch (item.Value.kind)
			{
			case 0:
				if (item.Value is OCIChar oCIChar)
				{
					oCIChar.StopVoice();
				}
				break;
			case 4:
				(item.Value as OCIRoute).DeleteLine();
				break;
			}
			UnityEngine.Object.Destroy(item.Value.guideObject.transformTarget.gameObject);
		}
		Singleton<Character>.Instance.DeleteCharaAll();
		dicInfo.Clear();
		dicChangeAmount.Clear();
		dicObjectCtrl.Clear();
		Singleton<Map>.Instance.ReleaseMap();
		cameraCtrl.CloerListCollider();
		bgmCtrl.Stop();
		envCtrl.Stop();
		outsideSoundCtrl.Stop();
		sceneInfo.Init();
		m_SystemButtonCtrl.UpdateInfo();
		cameraCtrl.Reset(0);
		cameraLightCtrl.Reflect();
		_cameraSelector.Init();
		mapList.UpdateInfo();
		if (onChangeMap != null)
		{
			onChangeMap();
		}
		m_BackgroundCtrl.Load(sceneInfo.background);
		m_BackgroundList.UpdateUI();
		_frameCtrl.Load(sceneInfo.frame);
		_frameList.UpdateUI();
		m_WorkspaceCtrl.UpdateUI();
		Singleton<UndoRedoManager>.Instance.Clear();
		if (_close)
		{
			rootButtonCtrl.OnClick(-1);
		}
	}

	public void OnDeleteNode(TreeNodeObject _node)
	{
		ObjectCtrlInfo value = null;
		if (dicInfo.TryGetValue(_node, out value))
		{
			if (onDelete != null)
			{
				onDelete(value);
			}
			value.OnDelete();
			dicInfo.Remove(_node);
		}
	}

	public void OnParentage(TreeNodeObject _parent, TreeNodeObject _child)
	{
		if ((bool)_parent)
		{
			FindLoop(_parent)?.OnAttach(_parent, dicInfo[_child]);
		}
		else
		{
			dicInfo[_child].OnDetach();
		}
	}

	public void ResetOption()
	{
		if (xmlCtrl != null)
		{
			xmlCtrl.Init();
		}
	}

	public void LoadOption()
	{
		if (xmlCtrl != null)
		{
			xmlCtrl.Read();
		}
	}

	public void SaveOption()
	{
		if (xmlCtrl != null)
		{
			xmlCtrl.Write();
		}
	}

	public static void AddInfo(ObjectInfo _info, ObjectCtrlInfo _ctrlInfo)
	{
		if (Singleton<Studio>.IsInstance() && _info != null && _ctrlInfo != null)
		{
			Singleton<Studio>.Instance.sceneInfo.dicObject.Add(_info.dicKey, _info);
			Singleton<Studio>.Instance.dicObjectCtrl[_info.dicKey] = _ctrlInfo;
		}
	}

	public static void DeleteInfo(ObjectInfo _info, bool _delKey = true)
	{
		if (!Singleton<Studio>.IsInstance() || _info == null)
		{
			return;
		}
		if (Singleton<Studio>.Instance.sceneInfo.dicObject.ContainsKey(_info.dicKey))
		{
			Singleton<Studio>.Instance.sceneInfo.dicObject.Remove(_info.dicKey);
		}
		if (_delKey)
		{
			Singleton<Studio>.Instance.dicObjectCtrl.Remove(_info.dicKey);
			_info.DeleteKey();
			if (Singleton<Studio>.Instance.sceneInfo.sunCaster == _info.dicKey)
			{
				Singleton<Studio>.Instance.SetSunCaster(-1);
			}
		}
	}

	public static ObjectInfo GetInfo(int _key)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return null;
		}
		ObjectInfo value = null;
		if (!Singleton<Studio>.Instance.sceneInfo.dicObject.TryGetValue(_key, out value))
		{
			return null;
		}
		return value;
	}

	public static void AddObjectCtrlInfo(ObjectCtrlInfo _ctrlInfo)
	{
		if (Singleton<Studio>.IsInstance() && _ctrlInfo != null)
		{
			Singleton<Studio>.Instance.dicObjectCtrl[_ctrlInfo.objectInfo.dicKey] = _ctrlInfo;
		}
	}

	public static ObjectCtrlInfo GetCtrlInfo(int _key)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return null;
		}
		ObjectCtrlInfo value = null;
		if (!Singleton<Studio>.Instance.dicObjectCtrl.TryGetValue(_key, out value))
		{
			return null;
		}
		return value;
	}

	public static TreeNodeObject AddNode(string _name, TreeNodeObject _parent = null)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return null;
		}
		return Singleton<Studio>.Instance.treeNodeCtrl.AddNode(_name, _parent);
	}

	public static void DeleteNode(TreeNodeObject _node)
	{
		if (Singleton<Studio>.IsInstance())
		{
			Singleton<Studio>.Instance.treeNodeCtrl.DeleteNode(_node);
		}
	}

	public static void AddCtrlInfo(ObjectCtrlInfo _info)
	{
		if (Singleton<Studio>.IsInstance() && _info != null)
		{
			Singleton<Studio>.Instance.dicInfo.Add(_info.treeNodeObject, _info);
		}
	}

	public static ObjectCtrlInfo GetCtrlInfo(TreeNodeObject _node)
	{
		if (!Singleton<Studio>.IsInstance() || _node == null)
		{
			return null;
		}
		ObjectCtrlInfo value = null;
		if (!Singleton<Studio>.Instance.dicInfo.TryGetValue(_node, out value))
		{
			return null;
		}
		return value;
	}

	public static int GetNewIndex()
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return -1;
		}
		return Singleton<Studio>.Instance.sceneInfo.GetNewIndex();
	}

	public static int SetNewIndex(int _index)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return _index;
		}
		if (!Singleton<Studio>.Instance.sceneInfo.SetNewIndex(_index))
		{
			return Singleton<Studio>.Instance.sceneInfo.GetNewIndex();
		}
		return _index;
	}

	public static bool DeleteIndex(int _index)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return false;
		}
		bool flag = Singleton<Studio>.Instance.sceneInfo.DeleteIndex(_index);
		return DeleteChangeAmount(_index) || flag;
	}

	public static void AddChangeAmount(int _key, ChangeAmount _ca)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return;
		}
		try
		{
			Singleton<Studio>.Instance.dicChangeAmount.Add(_key, _ca);
		}
		catch (Exception)
		{
		}
	}

	public static bool DeleteChangeAmount(int _key)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return false;
		}
		return Singleton<Studio>.Instance.dicChangeAmount.Remove(_key);
	}

	public static ChangeAmount GetChangeAmount(int _key)
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return null;
		}
		ChangeAmount value = null;
		if (!Singleton<Studio>.Instance.dicChangeAmount.TryGetValue(_key, out value))
		{
			return null;
		}
		return value;
	}

	public static ObjectCtrlInfo[] GetSelectObjectCtrl()
	{
		if (!Singleton<Studio>.IsInstance())
		{
			return null;
		}
		return Singleton<Studio>.Instance.treeNodeCtrl.selectObjectCtrl;
	}

	public void Init()
	{
		sceneInfo = new SceneInfo();
		cameraLightCtrl.Init();
		systemButtonCtrl.Init();
		mapList.Init();
		logoList.Init();
		_inputFieldNow = null;
		_inputFieldTMPNow = null;
		TreeNodeCtrl obj = treeNodeCtrl;
		obj.onDelete = (Action<TreeNodeObject>)Delegate.Combine(obj.onDelete, new Action<TreeNodeObject>(OnDeleteNode));
		TreeNodeCtrl obj2 = treeNodeCtrl;
		obj2.onParentage = (Action<TreeNodeObject, TreeNodeObject>)Delegate.Combine(obj2.onParentage, new Action<TreeNodeObject, TreeNodeObject>(OnParentage));
	}

	public void SelectInputField(InputField _input, TMP_InputField _inputTMP)
	{
		_inputFieldNow = _input;
		_inputFieldTMPNow = _inputTMP;
	}

	public void DeselectInputField(InputField _input, TMP_InputField _inputTMP)
	{
		if (_inputFieldNow == _input)
		{
			_inputFieldNow = null;
		}
		if (_inputFieldTMPNow == _inputTMP)
		{
			_inputFieldTMPNow = null;
		}
	}

	public void ShowName(Transform _transform, string _name)
	{
		rectName.gameObject.SetActive(value: true);
		rectName.position = RectTransformUtility.WorldToScreenPoint(Camera.main, _transform.position);
		textName.text = _name;
		if (disposableName != null)
		{
			disposableName.Dispose();
		}
		disposableName = new SingleAssignmentDisposable();
		IObservable<long> other = Observable.Timer(TimeSpan.FromSeconds(2.0));
		disposableName.Disposable = Observable.EveryUpdate().TakeUntil(other).Subscribe((Action<long>)delegate
		{
			if (_transform != null)
			{
				rectName.position = RectTransformUtility.WorldToScreenPoint(Camera.main, _transform.position);
			}
		}, (Action)delegate
		{
			rectName.gameObject.SetActive(value: false);
		});
	}

	private ObjectCtrlInfo FindLoop(TreeNodeObject _node)
	{
		if (_node == null)
		{
			return null;
		}
		ObjectCtrlInfo value = null;
		if (dicInfo.TryGetValue(_node, out value))
		{
			return value;
		}
		return FindLoop(_node.parent);
	}

	private void SavePreprocessingLoop(TreeNodeObject _node)
	{
		if (_node == null)
		{
			return;
		}
		ObjectCtrlInfo value = null;
		if (dicInfo.TryGetValue(_node, out value))
		{
			value.OnSavePreprocessing();
		}
		if (_node.child.IsNullOrEmpty())
		{
			return;
		}
		foreach (TreeNodeObject item in _node.child)
		{
			SavePreprocessingLoop(item);
		}
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			imageCamera.gameObject.SetActive(value: true);
			optionSystem = new OptionSystem("Option");
			xmlCtrl = new Control("studio", "option.xml", "Option", optionSystem);
			LoadOption();
			_logoList.UpdateInfo();
			if (workInfo == null)
			{
				workInfo = new WorkInfo();
			}
			workInfo.Load();
		}
	}

	private void OnApplicationQuit()
	{
		SaveOption();
		workInfo.Save();
	}
}
