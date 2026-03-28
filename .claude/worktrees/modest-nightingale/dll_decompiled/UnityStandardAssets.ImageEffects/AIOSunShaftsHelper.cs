using UnityEngine;

namespace UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class AIOSunShaftsHelper : MonoBehaviour
{
	public Transform helperTranform;

	public Camera activeCamera;

	public Light sunLight;

	public bool sunShaftsInastalled;

	private SunShafts ssComponent;

	[HideInInspector]
	public int quality;

	public float distanceFalloff = 0.5f;

	public float blurSize = 5f;

	public float intensity = 1.5f;

	private bool ssComDistory = true;

	private void OnEnable()
	{
		sunShaftsInastalled = true;
		helperTranform = GetComponent<Transform>();
		initSS();
	}

	private void OnDisable()
	{
		if (ssComponent != null)
		{
			ssComponent.enabled = false;
		}
	}

	private void Reset()
	{
		_ = Application.isPlaying;
	}

	private void Start()
	{
	}

	private void Update()
	{
		helperTranform.position = activeCamera.transform.position - sunLight.transform.forward * activeCamera.nearClipPlane * 2f;
		if (ssComponent != null)
		{
			ssComponent.maxRadius = 1f - distanceFalloff;
			ssComponent.sunShaftBlurRadius = blurSize;
			ssComponent.sunShaftIntensity = intensity;
		}
	}

	private void OnDestroy()
	{
		if (!Application.isPlaying && ssComponent != null && !ssComDistory)
		{
			Object.DestroyImmediate(ssComponent);
			ssComDistory = true;
		}
	}

	private void initSS()
	{
		if (activeCamera != null)
		{
			ssComponent = activeCamera.GetComponent<SunShafts>();
			if (ssComponent == null)
			{
				ssComponent = activeCamera.gameObject.AddComponent<SunShafts>();
				SunShafts sunShafts = ssComponent;
				sunShafts.simpleClearShader = Shader.Find("Hidden/SimpleClear");
				sunShafts.sunShaftsShader = Shader.Find("Hidden/SunShaftsComposite");
				sunShafts.sunTransform = helperTranform;
				sunShafts.CheckResources();
				sunShafts.enabled = true;
				ssComDistory = false;
			}
			else
			{
				ssComponent.enabled = true;
			}
		}
	}
}
