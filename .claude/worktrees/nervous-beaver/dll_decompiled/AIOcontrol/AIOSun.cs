using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

namespace AIOcontrol;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class AIOSun : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	public Material AIOSkyBox;

	private Material skyBoxRuntime;

	private bool isAIOskyBox;

	[Header("AIO Sky v1.2", order = 0)]
	public Camera aioCamera;

	private Camera oldCamera;

	[Space(10f, order = 1)]
	[Header("Sun/Moon", order = 2)]
	public Light sunLight;

	public Light moonLight;

	[Space(10f)]
	[Header("Animation/Only affect Runtime")]
	[Range(-10f, 10f)]
	public float masterTimeScale = 1f;

	public bool sunAnimation;

	[Range(1f, 1440f)]
	public int minPerDay = 1;

	private float sunRotateSpeed = 0.1f;

	public float sunIntensity = 1f;

	[Range(0f, 24f)]
	public float sunHour;

	[HideInInspector]
	public float sunAltitudeAngle;

	[HideInInspector]
	[Range(0f, 360f)]
	public float sunBearingAngle;

	[HideInInspector]
	private static Transform sunTransform;

	[HideInInspector]
	private static Transform moonTransform;

	private float skyboxTime1;

	private float skyboxTime2;

	private Vector2 skyboxPan1;

	private Vector2 skyboxPan2;

	private Vector2 skyboxDir1 = new Vector2(1f, 1f);

	private Vector2 skyboxDir2 = new Vector2(1f, 1f);

	private float dayRange;

	private float setRange = 0.2f;

	private float nightRange = -0.2f;

	private Color daySky;

	private Color setSky;

	private Color nightSky;

	private Color dayAtm;

	private Color setAtm;

	private Color nightAtm;

	private Color groundColor;

	private bool setAmbientMode;

	private AmbientMode oldAMode;

	private Color amb1;

	private Color amb2;

	private Color amb3;

	private Color amb4;

	[Space(10f)]
	[Header("Ambient/Environment Lighting Overrider")]
	public bool ambientOverrider;

	[Range(0f, 1f)]
	public float ambientIntensity = 0.5f;

	[Range(0f, 1f)]
	public float ambientOffset = 0.2f;

	private bool isTransition;

	private Color oldFogColor;

	private Color oldGroundColor;

	private Color oldAioFog;

	[Space(10f)]
	[Header("Sun Shafts")]
	public bool sunShaftsEnable;

	[Range(0.1f, 1f)]
	public float ssDistanceFalloff = 0.75f;

	[Range(1f, 10f)]
	public float ssBlurSize = 2.5f;

	[Range(0f, 10f)]
	public float ssIntensity = 1.15f;

	private GameObject SsHelperGO;

	private AIOSunShaftsHelper aioSSHelper;

	private Color cTop;

	private Color cSide;

	private Color cBottom;

	[Space(10f)]
	[Header("Sun Gizmos")]
	public bool drawGizmos = true;

	public float size = 10f;

	public Color textColor = Color.blue;

	public Color iconColor = Color.red;

	public bool IsTransition => isTransition;

	private void OnEnable()
	{
		sunLight = base.gameObject.GetComponent<Light>();
		RenderSettings.sun = sunLight;
		if (aioCamera == null)
		{
			aioCamera = Camera.main;
			oldCamera = aioCamera;
		}
		if (!Application.isPlaying)
		{
			InitSkybox();
			_ = isAIOskyBox;
			return;
		}
		InitSkybox();
		if (isAIOskyBox)
		{
			if (sunLight != null)
			{
				sunIntensity = Mathf.Max(sunLight.intensity, sunIntensity);
				sunTransform = sunLight.GetComponent<Transform>();
			}
			if (RenderSettings.skybox.HasProperty("_timeScale"))
			{
				skyboxTime1 = RenderSettings.skybox.GetFloat("_timeScale");
			}
			if (RenderSettings.skybox.HasProperty("_DistortionTime"))
			{
				skyboxTime2 = RenderSettings.skybox.GetFloat("_DistortionTime");
			}
			if (RenderSettings.skybox.HasProperty("_timeScale"))
			{
				RenderSettings.skybox.SetFloat("_timeScale", 0f);
			}
			if (RenderSettings.skybox.HasProperty("_CloudsPan"))
			{
				skyboxPan1 = RenderSettings.skybox.GetVector("_CloudsPan");
			}
			isTransition = false;
		}
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			skyBoxRuntime = new Material(AIOSkyBox)
			{
				name = AIOSkyBox.name + "_Runtime"
			};
			RenderSettings.skybox = skyBoxRuntime;
		}
	}

	private void LateUpdate()
	{
		if (sunLight != null)
		{
			setSun();
		}
		if (!isAIOskyBox)
		{
			InitSkybox();
			return;
		}
		if (!Application.isPlaying)
		{
			AIOupdate();
			return;
		}
		if (sunLight != null)
		{
			Vector3.Normalize(sunTransform.forward);
			if (sunAnimation)
			{
				SetAIOTime(sunHour);
				sunLight.transform.Rotate(Vector3.right, sunRotateSpeed * Time.deltaTime * masterTimeScale);
				sunHour = (360f - (90f + SignedAngle(ABinterSecDir(Vector3.up, sunTransform.right), sunTransform.forward, -sunTransform.right))) % 360f / 15f;
			}
		}
		skyboxPan1 += Time.deltaTime * skyboxTime1 * skyboxDir1 * masterTimeScale;
		skyboxPan2 += Time.deltaTime * skyboxTime2 * skyboxDir2 * masterTimeScale;
		skyboxPan2 = new Vector2(skyboxPan2.x % 1f, skyboxPan2.y % 1f);
		RenderSettings.skybox.SetVector("_CloudsPan", skyboxPan1);
		RenderSettings.skybox.SetVector("_DistortionPan", skyboxPan2);
		AIOupdate();
	}

	private void InitSkybox()
	{
		if (sunLight != null)
		{
			sunTransform = sunLight.GetComponent<Transform>();
			RenderSettings.sun = sunLight;
			sunHour = (360f - (90f + SignedAngle(ABinterSecDir(Vector3.up, sunTransform.right), sunTransform.forward, -sunTransform.right))) % 360f / 15f;
			sunAltitudeAngle = sunTransform.eulerAngles.z;
			sunBearingAngle = Vector3.Angle(ABinterSecDir(Vector3.up, sunTransform.right), new Vector3(0f, 0f, 1f));
		}
		else
		{
			sunLight = Object.FindObjectOfType<Light>();
			sunTransform = sunLight.GetComponent<Transform>();
		}
		if (moonLight != null)
		{
			moonTransform = moonLight.GetComponent<Transform>();
			if (RenderSettings.skybox.HasProperty("_MoonPosition"))
			{
				RenderSettings.skybox.SetVector("_MoonPosition", Vector3.Normalize(-moonLight.transform.forward));
			}
		}
		if (RenderSettings.skybox == null)
		{
			isAIOskyBox = false;
			return;
		}
		if (RenderSettings.skybox.shader.name.Substring(0, 7) != "AIOsky/")
		{
			isAIOskyBox = false;
			return;
		}
		isAIOskyBox = true;
		AIOSkyBox = RenderSettings.skybox;
		if (RenderSettings.skybox.HasProperty("_DaySky"))
		{
			daySky = RenderSettings.skybox.GetColor("_DaySky");
		}
		if (RenderSettings.skybox.HasProperty("_SunSetSky"))
		{
			setSky = RenderSettings.skybox.GetColor("_SunSetSky");
		}
		if (RenderSettings.skybox.HasProperty("_NightSky"))
		{
			nightSky = RenderSettings.skybox.GetColor("_NightSky");
		}
		if (RenderSettings.skybox.HasProperty("_DayAtmosphere"))
		{
			dayAtm = RenderSettings.skybox.GetColor("_DayAtmosphere");
		}
		if (RenderSettings.skybox.HasProperty("_SunSetAtmosphere"))
		{
			setAtm = RenderSettings.skybox.GetColor("_SunSetAtmosphere");
		}
		if (RenderSettings.skybox.HasProperty("_NightAtmosphere"))
		{
			nightAtm = RenderSettings.skybox.GetColor("_NightAtmosphere");
		}
		if (RenderSettings.skybox.HasProperty("_GroundColor"))
		{
			groundColor = RenderSettings.skybox.GetColor("_GroundColor");
		}
		oldAMode = RenderSettings.ambientMode;
	}

	private void AIOupdate()
	{
		if (moonLight != null && RenderSettings.skybox.HasProperty("_MoonPosition"))
		{
			RenderSettings.skybox.SetVector("_MoonPosition", Vector3.Normalize(-moonLight.transform.forward));
		}
		if (ambientOverrider)
		{
			if (!setAmbientMode)
			{
				oldAMode = RenderSettings.ambientMode;
				amb1 = RenderSettings.ambientEquatorColor;
				amb2 = RenderSettings.ambientGroundColor;
				amb3 = RenderSettings.ambientLight;
				amb4 = RenderSettings.ambientSkyColor;
			}
			setAmbientMode = true;
			RenderSettings.ambientMode = AmbientMode.Trilight;
			UpDateAmbient();
		}
		else if (setAmbientMode)
		{
			RenderSettings.ambientMode = oldAMode;
			RenderSettings.ambientEquatorColor = amb1;
			RenderSettings.ambientGroundColor = amb2;
			RenderSettings.ambientLight = amb3;
			RenderSettings.ambientSkyColor = amb4;
			DynamicGI.UpdateEnvironment();
			setAmbientMode = false;
		}
		if (sunShaftsEnable && aioSSHelper != null)
		{
			aioSSHelper.distanceFalloff = ssDistanceFalloff;
			aioSSHelper.blurSize = ssBlurSize;
			aioSSHelper.intensity = ssIntensity;
		}
	}

	private void OnApplicationQuit()
	{
		OnDisable();
	}

	private void OnDisable()
	{
		if (sunLight != null)
		{
			sunLight.intensity = sunIntensity;
		}
	}

	public float Remap(float minIn, float maxIn, float minOut, float maxOut, float valueIn)
	{
		return minOut + (maxOut - minOut) * ((valueIn - minIn) / (maxIn - minIn));
	}

	public void SkyboxLerp(Material mat, float t)
	{
		Material skybox = RenderSettings.skybox;
		if (!(mat.shader != skybox.shader))
		{
			StartCoroutine(SwitchBG(mat, t));
		}
	}

	public IEnumerator SwitchBG(Material mat, float t)
	{
		if (!(mat.shader != RenderSettings.skybox.shader))
		{
			isTransition = true;
			if (t <= 0f)
			{
				t = 0f;
			}
			float StartTime = Time.time;
			float num = 0f;
			float sourceTime1 = skyboxTime1;
			float sourceTime2 = skyboxTime2;
			float targetTime1 = mat.GetFloat("_timeScale");
			float targetTime2 = mat.GetFloat("_DistortionTime");
			Material matTemp = new Material(mat);
			matTemp.SetFloat("_timeScale", 0f);
			matTemp.SetFloat("_DistortionTime", 0f);
			matTemp.SetVector("_MoonPosition", Vector3.Normalize(-moonLight.transform.forward));
			do
			{
				float p = 5f;
				skyboxTime1 = Mathf.Lerp(sourceTime1, targetTime1, Mathf.Pow(num, p) * 0.5f);
				skyboxTime2 = Mathf.Lerp(sourceTime2, targetTime2, Mathf.Pow(num, p) * 0.5f);
				RenderSettings.skybox.Lerp(RenderSettings.skybox, matTemp, Mathf.Pow(num, p) * 0.5f);
				setRange = RenderSettings.skybox.GetFloat("_setRange");
				nightRange = RenderSettings.skybox.GetFloat("_nightRange");
				daySky = RenderSettings.skybox.GetColor("_DaySky");
				setSky = RenderSettings.skybox.GetColor("_SunSetSky");
				nightSky = RenderSettings.skybox.GetColor("_NightSky");
				dayAtm = RenderSettings.skybox.GetColor("_DayAtmosphere");
				setAtm = RenderSettings.skybox.GetColor("_SunSetAtmosphere");
				nightAtm = RenderSettings.skybox.GetColor("_NightAtmosphere");
				groundColor = RenderSettings.skybox.GetColor("_GroundColor");
				UpDateAmbient();
				yield return null;
				num = (Time.time - StartTime) / t;
			}
			while (num < 1f);
			isTransition = false;
		}
	}

	private void UpDateAmbient()
	{
		if (!(sunTransform == null))
		{
			Vector3 vector = Vector3.Normalize(-sunTransform.forward);
			cTop = Color.Lerp(Color.Lerp(setSky, daySky, Mathf.Clamp(0f + (vector.y - setRange) * 1f / (dayRange - setRange), 0f, 1f)), nightSky, Mathf.Clamp(0f + (vector.y - setRange) * 1f / (nightRange - setRange), 0f, 1f));
			cSide = Color.Lerp(Color.Lerp(setAtm, dayAtm, Mathf.Clamp(0f + (vector.y - setRange) * 1f / (dayRange - setRange), 0f, 1f)), nightAtm, Mathf.Clamp(0f + (vector.y - setRange) * 1f / (nightRange - setRange), 0f, 1f));
			cBottom = groundColor;
			RenderSettings.ambientSkyColor = cTop * ambientIntensity + new Color(ambientOffset, ambientOffset, ambientOffset);
			RenderSettings.ambientEquatorColor = cSide * ambientIntensity + new Color(ambientOffset, ambientOffset, ambientOffset);
			RenderSettings.ambientGroundColor = cBottom * ambientIntensity + new Color(ambientOffset, ambientOffset, ambientOffset);
			DynamicGI.UpdateEnvironment();
		}
	}

	public static Material GetRuntimeMat()
	{
		return RenderSettings.skybox;
	}

	public float GetAioTime()
	{
		if (sunTransform == null)
		{
			return -1f;
		}
		return (360f - (90f + SignedAngle(ABinterSecDir(Vector3.up, sunTransform.right), sunTransform.forward, -sunTransform.right))) % 360f / 15f;
	}

	public void SetAIOTime(float t)
	{
		sunHour = t;
		t = t % 24f * 15f;
		if (sunTransform != null)
		{
			sunTransform.Rotate(Vector3.right, (0f - GetAioTime()) * 15f);
			sunTransform.Rotate(Vector3.right, t);
			if (ambientOverrider)
			{
				UpDateAmbient();
			}
		}
	}

	private static Vector3 ABinterSecDir(Vector3 A, Vector3 B)
	{
		Vector3 vector = Vector3.Normalize(Vector3.Cross(A, B));
		if (vector == new Vector3(0f, 0f, 0f))
		{
			vector = Vector3.forward;
		}
		return vector;
	}

	private static float SignedAngle(Vector3 A, Vector3 B, Vector3 planeUp)
	{
		float num = Vector3.Angle(A, B);
		float num2 = Mathf.Sign(Vector3.Dot(planeUp, Vector3.Cross(A, B)));
		return num * num2;
	}

	private void Flare()
	{
	}

	private float GetClouds(Vector3 P)
	{
		return 0f;
	}

	private bool CreateSSHelper()
	{
		if (aioCamera == null)
		{
			return false;
		}
		Transform transform = aioCamera.GetComponent<Transform>().Find("AioSunShaftsHelper");
		if (transform == null)
		{
			SsHelperGO = new GameObject();
			SsHelperGO.name = "AioSunShaftsHelper";
			SsHelperGO.transform.SetParent(aioCamera.transform);
			aioSSHelper = SsHelperGO.AddComponent<AIOSunShaftsHelper>();
			aioSSHelper.enabled = false;
			aioSSHelper.activeCamera = aioCamera;
			aioSSHelper.sunLight = sunLight;
			return aioSSHelper.sunShaftsInastalled;
		}
		aioSSHelper = transform.GetComponent<AIOSunShaftsHelper>();
		return aioSSHelper.sunShaftsInastalled;
	}

	private void setSun()
	{
		sunRotateSpeed = 360f / ((float)minPerDay * 60f);
		Vector3 vector = Vector3.Normalize(sunTransform.forward);
		sunLight.intensity = Mathf.SmoothStep(sunIntensity, 0f, 2f * (Mathf.Clamp(vector.y, nightRange, setRange) - nightRange) / (setRange - nightRange));
		SetAIOTime(sunHour);
	}
}
