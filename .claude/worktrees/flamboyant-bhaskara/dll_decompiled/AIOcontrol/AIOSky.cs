using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

namespace AIOcontrol;

[ExecuteInEditMode]
public class AIOSky : MonoBehaviour
{
	private bool isAIOskyBox;

	[Header("AIO Sky v1.2", order = 0)]
	public Camera aioCamera;

	private Camera oldCamera;

	[Space(10f, order = 1)]
	[Header("Sun/Moon", order = 2)]
	public Light sunLight;

	public Light moonLight;

	[Range(0.025f, 1f)]
	public float moonScale = 0.1f;

	[Space(10f)]
	[Header("Animation/Only affect Runtime")]
	[Range(-10f, 10f)]
	public float masterTimeScale = 1f;

	public bool sunAnimation;

	[Range(1f, 1440f)]
	public int minPerDay = 1;

	private float sunRotateSpeed = 0.1f;

	public float sunIntensity;

	[Range(0f, 24f)]
	public float sunHour;

	[HideInInspector]
	public float sunAltitudeAngle;

	[HideInInspector]
	[Range(0f, 360f)]
	public float sunBearingAngle;

	[Space(10f)]
	[Header("Clouds")]
	public bool cloudsAdjust;

	[Range(-1f, 1f)]
	public float cloudsDensity;

	[Range(0.0001f, 1f)]
	public float cloudsThickness = 0.5f;

	private float oldCloudDesity;

	private float oldCloudThickness;

	[Range(0f, 1f)]
	public float domeCurved = 0.3f;

	[Range(0.5f, 10f)]
	public float cloudsScale = 1.8f;

	[Range(0f, 1f)]
	public float cloudsRotation;

	[Range(0f, 1f)]
	public float flowSpeed;

	[Range(0f, 1f)]
	public float cloudSpeed;

	private float oldDomeCurved;

	private float oldCloudsScale;

	private float oldCloudsRotation;

	private float oldCloudSpeed;

	private float oldFlowSpeed;

	[HideInInspector]
	public Material skyBoxEditor;

	private static Material skyBoxRuntime;

	private static Transform sunTransform;

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

	[Space(10f)]
	[Header("Fog Control")]
	public bool fogControl;

	[Range(0f, 1f)]
	public float fogAmbientWeighting = 0.5f;

	[Space(10f)]
	[Range(0f, 1f)]
	public float fogLevel = 0.1f;

	public Color aioFogColor = Color.gray;

	[Range(-0.5f, 0.85f)]
	public float groundLevel;

	private Color aioGroundColor;

	private bool setFog;

	private Color oldFogColor;

	private Color oldGroundColor;

	private Color oldAioFog;

	private float oldGroundLevel;

	private float oldFogLevel;

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
				sunIntensity = sunLight.intensity;
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
			if (RenderSettings.skybox.HasProperty("_DistortionTime"))
			{
				RenderSettings.skybox.SetFloat("_DistortionTime", 0f);
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
			Material skybox = RenderSettings.skybox;
			skyBoxRuntime = new Material(skybox)
			{
				name = skybox.name + "_Runtime"
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
			Vector3 vector = Vector3.Normalize(sunTransform.forward);
			sunLight.intensity = Mathf.SmoothStep(sunIntensity, 0f, 2f * (Mathf.Clamp(vector.y, nightRange, setRange) - nightRange) / (setRange - nightRange));
			if (sunAnimation)
			{
				SetAIOTime(sunHour);
				sunLight.transform.Rotate(Vector3.right, sunRotateSpeed * Time.deltaTime * masterTimeScale);
				sunHour = (360f - (90f + SignedAngle(ABinterSecDir(Vector3.up, sunTransform.right), sunTransform.forward, -sunTransform.right))) % 360f / 15f;
			}
		}
		if (RenderSettings.skybox != skyBoxRuntime)
		{
			InitSkybox();
			return;
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
		sunLight = RenderSettings.sun;
		if (sunLight != null)
		{
			sunTransform = sunLight.GetComponent<Transform>();
			sunIntensity = sunLight.intensity;
			sunHour = (360f - (90f + SignedAngle(ABinterSecDir(Vector3.up, sunTransform.right), sunTransform.forward, -sunTransform.right))) % 360f / 15f;
			sunAltitudeAngle = sunTransform.eulerAngles.z;
			sunBearingAngle = Vector3.Angle(ABinterSecDir(Vector3.up, sunTransform.right), new Vector3(0f, 0f, 1f));
		}
		else
		{
			sunLight = UnityEngine.Object.FindObjectOfType<Light>();
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
		storeOldCloudsSetting();
		getClouds();
		if (RenderSettings.skybox.HasProperty("_DayRange"))
		{
			dayRange = RenderSettings.skybox.GetFloat("_DayRange");
		}
		if (RenderSettings.skybox.HasProperty("_setRange"))
		{
			setRange = RenderSettings.skybox.GetFloat("_setRange");
		}
		if (RenderSettings.skybox.HasProperty("_nightRange"))
		{
			nightRange = RenderSettings.skybox.GetFloat("_nightRange");
		}
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
		oldFogColor = RenderSettings.fogColor;
		if (RenderSettings.skybox.HasProperty("_FogColor"))
		{
			oldAioFog = RenderSettings.skybox.GetColor("_FogColor");
		}
		if (RenderSettings.skybox.HasProperty("_GroundColor"))
		{
			oldGroundColor = RenderSettings.skybox.GetColor("_GroundColor");
		}
		if (RenderSettings.skybox.HasProperty("_FogLevel"))
		{
			oldFogLevel = RenderSettings.skybox.GetFloat("_FogLevel");
		}
		if (RenderSettings.skybox.HasProperty("_GroundLevel"))
		{
			oldGroundLevel = RenderSettings.skybox.GetFloat("_GroundLevel");
		}
		if (Application.isPlaying)
		{
			skyBoxEditor = RenderSettings.skybox;
			skyBoxRuntime = new Material(skyBoxEditor)
			{
				name = skyBoxEditor.name + "_Runtime"
			};
			RenderSettings.skybox = skyBoxRuntime;
		}
	}

	private void AIOupdate()
	{
		if (moonLight != null)
		{
			if (RenderSettings.skybox.HasProperty("_MoonPosition"))
			{
				RenderSettings.skybox.SetVector("_MoonPosition", Vector3.Normalize(-moonLight.transform.forward));
			}
			if (RenderSettings.skybox.HasProperty("_moonScale"))
			{
				RenderSettings.skybox.SetFloat("_moonScale", 1f / moonScale);
			}
		}
		if (cloudsAdjust)
		{
			setClouds();
		}
		else
		{
			restoreOldCloudsSetting();
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
		if (fogControl)
		{
			if (!setFog)
			{
				oldFogColor = RenderSettings.fogColor;
				if (RenderSettings.skybox.HasProperty("_FogColor"))
				{
					oldAioFog = RenderSettings.skybox.GetColor("_FogColor");
				}
				if (RenderSettings.skybox.HasProperty("_GroundColor"))
				{
					oldGroundColor = RenderSettings.skybox.GetColor("_GroundColor");
				}
				if (RenderSettings.skybox.HasProperty("_FogLevel"))
				{
					oldFogLevel = RenderSettings.skybox.GetFloat("_FogLevel");
				}
				if (RenderSettings.skybox.HasProperty("_GroundLevel"))
				{
					oldGroundLevel = RenderSettings.skybox.GetFloat("_GroundLevel");
				}
				setFog = true;
			}
			UpDateFog();
		}
		else if (setFog)
		{
			RenderSettings.fogColor = oldFogColor;
			if (RenderSettings.skybox.HasProperty("_FogColor"))
			{
				RenderSettings.skybox.SetColor("_FogColor", oldAioFog);
			}
			if (RenderSettings.skybox.HasProperty("_GroundColor"))
			{
				RenderSettings.skybox.SetColor("_GroundColor", oldGroundColor);
			}
			if (RenderSettings.skybox.HasProperty("_FogLevel"))
			{
				RenderSettings.skybox.SetFloat("_FogLevel", oldFogLevel);
			}
			if (RenderSettings.skybox.HasProperty("_GroundLevel"))
			{
				RenderSettings.skybox.SetFloat("_GroundLevel", oldGroundLevel);
			}
			DynamicGI.UpdateEnvironment();
			setFog = false;
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
		if (!(mat.shader != skyBoxRuntime.shader))
		{
			StartCoroutine(SwitchBG(mat, t));
		}
	}

	public IEnumerator SwitchBG(Material mat, float t)
	{
		if (!(mat.shader != skyBoxRuntime.shader))
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
			matTemp.SetFloat("_moonScale", 1f / moonScale);
			do
			{
				float p = 5f;
				skyboxTime1 = Mathf.Lerp(sourceTime1, targetTime1, Mathf.Pow(num, p) * 0.5f);
				skyboxTime2 = Mathf.Lerp(sourceTime2, targetTime2, Mathf.Pow(num, p) * 0.5f);
				RenderSettings.skybox.Lerp(skyBoxRuntime, matTemp, Mathf.Pow(num, p) * 0.5f);
				setRange = RenderSettings.skybox.GetFloat("_setRange");
				nightRange = RenderSettings.skybox.GetFloat("_nightRange");
				storeOldCloudsSetting();
				daySky = RenderSettings.skybox.GetColor("_DaySky");
				setSky = RenderSettings.skybox.GetColor("_SunSetSky");
				nightSky = RenderSettings.skybox.GetColor("_NightSky");
				dayAtm = RenderSettings.skybox.GetColor("_DayAtmosphere");
				setAtm = RenderSettings.skybox.GetColor("_SunSetAtmosphere");
				nightAtm = RenderSettings.skybox.GetColor("_NightAtmosphere");
				groundColor = RenderSettings.skybox.GetColor("_GroundColor");
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

	private void UpDateFog()
	{
		Vector3 vector = Vector3.Normalize(-sunTransform.forward);
		Color value = (RenderSettings.fogColor = ((1f - fogAmbientWeighting) * aioFogColor + fogAmbientWeighting * Color.Lerp(Color.Lerp(setAtm, dayAtm, Mathf.Clamp(0f + (vector.y - setRange) * 1f / (dayRange - setRange), 0f, 1f)), nightAtm, Mathf.Clamp(0f + (vector.y - setRange) * 1f / (nightRange - setRange), 0f, 1f))) / 2f);
		if (RenderSettings.skybox.HasProperty("_FogColor"))
		{
			RenderSettings.skybox.SetColor("_FogColor", value);
		}
		if (RenderSettings.skybox.HasProperty("_GroundColor"))
		{
			RenderSettings.skybox.SetColor("_GroundColor", value);
		}
		if (RenderSettings.skybox.HasProperty("_FogLevel"))
		{
			RenderSettings.skybox.SetFloat("_FogLevel", fogLevel);
		}
		if (RenderSettings.skybox.HasProperty("_GroundLevel"))
		{
			RenderSettings.skybox.SetFloat("_GroundLevel", groundLevel);
		}
		DynamicGI.UpdateEnvironment();
	}

	public static Material GetRuntimeMat()
	{
		return skyBoxRuntime;
	}

	public static float GetAioTime()
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

	private void storeOldCloudsSetting()
	{
		oldCloudDesity = RenderSettings.skybox.GetFloat("_CloudsDensity");
		oldCloudThickness = RenderSettings.skybox.GetFloat("_CloudsThickness");
		oldDomeCurved = RenderSettings.skybox.GetFloat("_DomeCurved");
		oldCloudsScale = RenderSettings.skybox.GetFloat("_CloudsScale");
		oldCloudsRotation = RenderSettings.skybox.GetFloat("_CloudsRotation") / ((float)Math.PI * 2f);
		oldCloudSpeed = RenderSettings.skybox.GetFloat("_timeScale") * 20f;
		oldFlowSpeed = RenderSettings.skybox.GetFloat("_DistortionTime") * 5f;
	}

	private void restoreOldCloudsSetting()
	{
		RenderSettings.skybox.SetFloat("_CloudsDensity", oldCloudDesity);
		RenderSettings.skybox.SetFloat("_CloudsThickness", oldCloudThickness);
		RenderSettings.skybox.SetFloat("_DomeCurved", oldDomeCurved);
		RenderSettings.skybox.SetFloat("_CloudsScale", oldCloudsScale);
		RenderSettings.skybox.SetFloat("_CloudsRotation", oldCloudsRotation * (float)Math.PI * 2f);
		RenderSettings.skybox.SetFloat("_timeScale", oldCloudSpeed / 20f);
		RenderSettings.skybox.SetFloat("_DistortionTime", oldFlowSpeed / 5f);
	}

	private void setClouds()
	{
		if (RenderSettings.skybox.HasProperty("_CloudsDensity"))
		{
			RenderSettings.skybox.SetFloat("_CloudsDensity", cloudsDensity);
		}
		if (RenderSettings.skybox.HasProperty("_CloudsThickness"))
		{
			RenderSettings.skybox.SetFloat("_CloudsThickness", cloudsThickness);
		}
		if (RenderSettings.skybox.HasProperty("_DomeCurved"))
		{
			RenderSettings.skybox.SetFloat("_DomeCurved", domeCurved);
		}
		if (RenderSettings.skybox.HasProperty("_CloudsScale"))
		{
			RenderSettings.skybox.SetFloat("_CloudsScale", cloudsScale);
		}
		if (RenderSettings.skybox.HasProperty("_CloudsRotation"))
		{
			RenderSettings.skybox.SetFloat("_CloudsRotation", cloudsRotation * (float)Math.PI * 2f);
		}
		if (RenderSettings.skybox.HasProperty("_timeScale"))
		{
			RenderSettings.skybox.SetFloat("_timeScale", cloudSpeed / 20f);
		}
		if (RenderSettings.skybox.HasProperty("_DistortionTime"))
		{
			RenderSettings.skybox.SetFloat("_DistortionTime", flowSpeed / 5f);
		}
	}

	private void getClouds()
	{
		if (RenderSettings.skybox.HasProperty("_CloudsDensity"))
		{
			cloudsDensity = RenderSettings.skybox.GetFloat("_CloudsDensity");
		}
		if (RenderSettings.skybox.HasProperty("_CloudsThickness"))
		{
			cloudsThickness = RenderSettings.skybox.GetFloat("_CloudsThickness");
		}
		if (RenderSettings.skybox.HasProperty("_DomeCurved"))
		{
			domeCurved = RenderSettings.skybox.GetFloat("_DomeCurved");
		}
		if (RenderSettings.skybox.HasProperty("_CloudsScale"))
		{
			cloudsScale = RenderSettings.skybox.GetFloat("_CloudsScale");
		}
		if (RenderSettings.skybox.HasProperty("_CloudsRotation"))
		{
			cloudsRotation = RenderSettings.skybox.GetFloat("_CloudsRotation") / ((float)Math.PI * 2f);
		}
		if (RenderSettings.skybox.HasProperty("_timeScale"))
		{
			cloudSpeed = RenderSettings.skybox.GetFloat("_timeScale") * 20f;
		}
		if (RenderSettings.skybox.HasProperty("_DistortionTime"))
		{
			flowSpeed = RenderSettings.skybox.GetFloat("_DistortionTime") * 5f;
		}
	}

	private void setSun()
	{
		sunRotateSpeed = 360f / ((float)minPerDay * 60f);
		Vector3 vector = Vector3.Normalize(sunTransform.forward);
		sunLight.intensity = Mathf.SmoothStep(sunIntensity, 0f, 2f * (Mathf.Clamp(vector.y, nightRange, setRange) - nightRange) / (setRange - nightRange));
		SetAIOTime(sunHour);
	}
}
