using System.Linq;
using ADV.Backup;
using AIChara;
using Config;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ADV;

public class ADVScene : MonoBehaviour
{
	private CameraData bkCamDat;

	[SerializeField]
	private TextScenario scenario;

	[SerializeField]
	private Transform stand;

	[SerializeField]
	private SceneFadeCanvas _fadeFront;

	[SerializeField]
	private SceneFadeCanvas _fadeBack;

	[SerializeField]
	private Image _filterImage;

	[SerializeField]
	private EMTransition _fadeFrontTransition;

	[SerializeField]
	private EMTransition _fadeBackTransition;

	[SerializeField]
	private bool _isAspect;

	[SerializeField]
	private BackGroundParam bgParam;

	[SerializeField]
	private Camera backCamera;

	[SerializeField]
	private bool _isCameraLock = true;

	private bool isReleased;

	public string startAddSceneName { get; private set; }

	public CorrectLightAngle correctLightAngle { get; private set; }

	public TextScenario Scenario => scenario;

	public Transform Stand => stand;

	public SceneFadeCanvas fadeFront => _fadeFront;

	public SceneFadeCanvas fadeBack => _fadeBack;

	public Image filterImage => _filterImage;

	public EMTransition fadeFrontTransition => _fadeFrontTransition;

	public EMTransition fadeBackTransition => _fadeBackTransition;

	public bool isCameraPosDontMoveFirst { get; set; }

	public bool isCameraPosDontMoveRelease { get; set; }

	public bool isAspect
	{
		get
		{
			return _isAspect;
		}
		set
		{
			SetAspect(_isAspect = value);
		}
	}

	public Camera advCamera => _advCamera;

	private Camera _advCamera { get; set; }

	public BackGroundParam BGParam => bgParam;

	public Camera BackCamera => backCamera;

	public bool isCameraLock
	{
		get
		{
			return _isCameraLock;
		}
		set
		{
			_isCameraLock = value;
		}
	}

	private BackupPosRot backCameraBackup { get; set; }

	private static string CreateCameraName { get; } = "FrontCamera";

	private static string CameraAssetName { get; } = "ActionCamera";

	private void SetAspect(bool active)
	{
		if (!(_advCamera == null))
		{
			_advCamera.rect = (active ? MathfEx.AspectRect() : new Rect(0f, 0f, 1f, 1f));
		}
	}

	public void SetCamera(Camera camera)
	{
		_advCamera = camera;
	}

	public void DefaultDOF()
	{
		bkCamDat.DefaultDOF(_advCamera);
	}

	private void Init()
	{
		isReleased = false;
		stand.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		_advCamera = Camera.main;
		if (_advCamera == null)
		{
			_advCamera = AssetBundleManager.LoadAsset(AssetBundleNames.AdvCameraAction, CameraAssetName, typeof(Camera)).GetAsset<Camera>();
			_advCamera.name = CreateCameraName;
			Manager.Sound.Listener = _advCamera.transform;
			AssetBundleManager.UnloadAssetBundle(AssetBundleNames.AdvCameraAction, isUnloadForceRefCount: false);
		}
		bkCamDat = new CameraData(_advCamera);
		ADVCameraSetting(_advCamera);
		SetAspect(isAspect);
		correctLightAngle = ((_advCamera == null) ? null : _advCamera.GetComponent<CorrectLightAngle>());
		if (correctLightAngle != null)
		{
			correctLightAngle.condition = delegate
			{
				ChaControl[] array = (from p in scenario.commandController.Characters.Values
					select p.chaCtrl into p
					where p != null
					where p.visibleAll
					select p).ToArray();
				return (array.Length != 1) ? null : array[0];
			};
		}
		InitFade(_fadeFront);
		InitFade(_fadeBack);
		InitEMTransition(_fadeFrontTransition);
		InitEMTransition(_fadeBackTransition);
		ParameterList.Init();
		scenario.ConfigProc();
		static void InitEMTransition(EMTransition transition)
		{
			if (!(transition.curve.Evaluate(0f) > 0.5f))
			{
				transition.FlipAnimationCurve();
			}
			transition.Set(1f);
		}
		static void InitFade(SceneFadeCanvas fade)
		{
			fade.StartFade(FadeCanvas.Fade.Out);
			fade.Cancel();
		}
	}

	public void Release()
	{
		if (isReleased)
		{
			return;
		}
		isReleased = true;
		scenario.Release();
		if (_advCamera != null)
		{
			if (_advCamera.name == CreateCameraName)
			{
				Object.Destroy(_advCamera.gameObject);
			}
			else if (bkCamDat != null)
			{
				bkCamDat.Load(_advCamera, isCameraPosDontMoveRelease);
			}
		}
		isCameraPosDontMoveFirst = false;
		isCameraPosDontMoveRelease = false;
		_advCamera = null;
		ParameterList.Release();
		if (correctLightAngle != null)
		{
			correctLightAngle.condition = null;
			correctLightAngle.offset = Vector2.zero;
			correctLightAngle.enabled = true;
			correctLightAngle = null;
		}
	}

	private void ADVCameraSetting(Camera advCamera)
	{
		if (advCamera != null)
		{
			Transform transform = advCamera.transform;
			Transform transform2 = BackCamera.transform;
			transform.SetParent(transform2.parent, isCameraPosDontMoveFirst);
			if (!isCameraPosDontMoveFirst)
			{
				transform.SetPositionAndRotation(transform2.position, transform2.rotation);
			}
			transform.localScale = Vector3.one;
			if (!isCameraPosDontMoveFirst)
			{
				advCamera.fieldOfView = BackCamera.fieldOfView;
			}
			advCamera.farClipPlane = 10000f;
		}
		if (bgParam != null)
		{
			bgParam.visible = false;
		}
	}

	private void Awake()
	{
		if (backCamera != null)
		{
			backCameraBackup = new BackupPosRot(backCamera.transform);
		}
	}

	private void OnEnable()
	{
		SceneParameter.advScene = this;
		if (backCameraBackup != null && backCamera != null)
		{
			backCameraBackup.Set(backCamera.transform);
		}
		scenario.OnInitializedAsync.TakeUntilDestroy(this).Subscribe(delegate
		{
			Init();
		});
		startAddSceneName = Scene.AddSceneName;
	}

	private void OnDisable()
	{
		SceneParameter.advScene = null;
	}

	private void Update()
	{
		if (Scene.Overlaps.Any((Scene.IOverlap x) => x is ConfigWindow))
		{
			scenario.ConfigProc();
		}
	}
}
