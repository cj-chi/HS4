using System.Collections;
using UnityEngine;

public class DistortionMobileCamera : MonoBehaviour
{
	public float TextureScale = 1f;

	public RenderTextureFormat RenderTextureFormat;

	public FilterMode FilterMode;

	public LayerMask CullingMask = -17;

	public RenderingPath RenderingPath;

	public int FPSWhenMoveCamera = 40;

	public int FPSWhenStaticCamera = 20;

	public bool UseRealTime;

	private RenderTexture renderTexture;

	private Camera cameraInstance;

	private GameObject goCamera;

	private Vector3 oldPosition;

	private Quaternion oldRotation;

	private Transform instanceCameraTransform;

	private bool canUpdateCamera;

	private bool isStaticUpdate;

	private WaitForSeconds fpsMove;

	private WaitForSeconds fpsStatic;

	private const int DropedFrames = 50;

	private int frameCountWhenCameraIsStatic;

	private void Start()
	{
		if (UseRealTime)
		{
			Initialize();
			return;
		}
		fpsMove = new WaitForSeconds(1f / (float)FPSWhenMoveCamera);
		fpsStatic = new WaitForSeconds(1f / (float)FPSWhenStaticCamera);
		canUpdateCamera = true;
		if (FPSWhenMoveCamera > 0)
		{
			StartCoroutine(RepeatCameraMove());
		}
		if (FPSWhenStaticCamera > 0)
		{
			StartCoroutine(RepeatCameraStatic());
		}
		Initialize();
	}

	private void Update()
	{
		if (UseRealTime || cameraInstance == null)
		{
			return;
		}
		if (Vector3.SqrMagnitude(instanceCameraTransform.position - oldPosition) <= 1E-05f && instanceCameraTransform.rotation == oldRotation)
		{
			frameCountWhenCameraIsStatic++;
			if (frameCountWhenCameraIsStatic >= 50)
			{
				isStaticUpdate = true;
			}
		}
		else
		{
			frameCountWhenCameraIsStatic = 0;
			isStaticUpdate = false;
		}
		oldPosition = instanceCameraTransform.position;
		oldRotation = instanceCameraTransform.rotation;
		if (canUpdateCamera)
		{
			if (!cameraInstance.enabled)
			{
				cameraInstance.enabled = true;
			}
			if (FPSWhenMoveCamera > 0)
			{
				canUpdateCamera = false;
			}
		}
		else if (cameraInstance.enabled)
		{
			cameraInstance.enabled = false;
		}
	}

	private IEnumerator RepeatCameraMove()
	{
		while (true)
		{
			if (!isStaticUpdate)
			{
				canUpdateCamera = true;
			}
			yield return fpsMove;
		}
	}

	private IEnumerator RepeatCameraStatic()
	{
		while (true)
		{
			if (isStaticUpdate)
			{
				canUpdateCamera = true;
			}
			yield return fpsStatic;
		}
	}

	private void OnBecameVisible()
	{
		if (goCamera != null)
		{
			goCamera.SetActive(value: true);
		}
	}

	private void OnBecameInvisible()
	{
		if (goCamera != null)
		{
			goCamera.SetActive(value: false);
		}
	}

	private void Initialize()
	{
		goCamera = new GameObject("RenderTextureCamera");
		cameraInstance = goCamera.AddComponent<Camera>();
		Camera main = Camera.main;
		cameraInstance.CopyFrom(main);
		cameraInstance.depth++;
		cameraInstance.cullingMask = CullingMask;
		cameraInstance.renderingPath = RenderingPath;
		goCamera.transform.parent = main.transform;
		renderTexture = new RenderTexture(Mathf.RoundToInt((float)Screen.width * TextureScale), Mathf.RoundToInt((float)Screen.height * TextureScale), 16, RenderTextureFormat);
		renderTexture.DiscardContents();
		renderTexture.filterMode = FilterMode;
		cameraInstance.targetTexture = renderTexture;
		instanceCameraTransform = cameraInstance.transform;
		oldPosition = instanceCameraTransform.position;
		Shader.SetGlobalTexture("_GrabTextureMobile", renderTexture);
	}

	private void OnDisable()
	{
		if ((bool)goCamera)
		{
			Object.DestroyImmediate(goCamera);
			goCamera = null;
		}
		if ((bool)renderTexture)
		{
			Object.DestroyImmediate(renderTexture);
			renderTexture = null;
		}
	}
}
