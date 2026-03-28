using UnityEngine;

public class ME_DemoGUI : MonoBehaviour
{
	public GameObject Character;

	public GameObject Model;

	public int Current;

	public GameObject[] Prefabs;

	public Light Sun;

	public ReflectionProbe ReflectionProbe;

	public Light[] NightLights = new Light[0];

	public Texture HUETexture;

	public bool UseMobileVersion;

	public GameObject MobileCharacter;

	public GameObject Target;

	public Color guiColor = Color.red;

	private int currentNomber;

	private GameObject characterInstance;

	private GameObject modelInstance;

	private GUIStyle guiStyleHeader = new GUIStyle();

	private GUIStyle guiStyleHeaderMobile = new GUIStyle();

	private float dpiScale;

	private bool isDay;

	private float colorHUE;

	private float startSunIntensity;

	private Quaternion startSunRotation;

	private Color startAmbientLight;

	private float startAmbientIntencity;

	private float startReflectionIntencity;

	private LightShadows startLightShadows;

	private bool isButtonPressed;

	private GameObject instanceShieldProjectile;

	private void Start()
	{
		if (Screen.dpi < 1f)
		{
			dpiScale = 1f;
		}
		if (Screen.dpi < 200f)
		{
			dpiScale = 1f;
		}
		else
		{
			dpiScale = Screen.dpi / 200f;
		}
		guiStyleHeader.fontSize = (int)(15f * dpiScale);
		guiStyleHeader.normal.textColor = guiColor;
		guiStyleHeaderMobile.fontSize = (int)(17f * dpiScale);
		ChangeCurrent(Current);
		startSunIntensity = Sun.intensity;
		startSunRotation = Sun.transform.rotation;
		startAmbientLight = RenderSettings.ambientLight;
		startAmbientIntencity = RenderSettings.ambientIntensity;
		startReflectionIntencity = RenderSettings.reflectionIntensity;
		startLightShadows = Sun.shadows;
	}

	private void OnGUI()
	{
		if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.DownArrow))
		{
			isButtonPressed = false;
		}
		if (GUI.Button(new Rect(10f * dpiScale, 15f * dpiScale, 135f * dpiScale, 37f * dpiScale), "PREVIOUS EFFECT") || (!isButtonPressed && Input.GetKeyDown(KeyCode.LeftArrow)))
		{
			isButtonPressed = true;
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(160f * dpiScale, 15f * dpiScale, 135f * dpiScale, 37f * dpiScale), "NEXT EFFECT") || (!isButtonPressed && Input.GetKeyDown(KeyCode.RightArrow)))
		{
			isButtonPressed = true;
			ChangeCurrent(1);
		}
		float num = 0f;
		if (GUI.Button(new Rect(10f * dpiScale, 63f * dpiScale + num, 285f * dpiScale, 37f * dpiScale), "Day / Night") || (!isButtonPressed && Input.GetKeyDown(KeyCode.DownArrow)))
		{
			isButtonPressed = true;
			if (ReflectionProbe != null)
			{
				ReflectionProbe.RenderProbe();
			}
			Sun.intensity = ((!isDay) ? 0.05f : startSunIntensity);
			Sun.shadows = (isDay ? startLightShadows : LightShadows.None);
			Light[] nightLights = NightLights;
			for (int i = 0; i < nightLights.Length; i++)
			{
				nightLights[i].shadows = ((!isDay) ? startLightShadows : LightShadows.None);
			}
			Sun.transform.rotation = (isDay ? startSunRotation : Quaternion.Euler(350f, 30f, 90f));
			RenderSettings.ambientLight = ((!isDay) ? new Color(0.2f, 0.2f, 0.2f) : startAmbientLight);
			float num2 = ((!UseMobileVersion) ? 1f : 0.3f);
			RenderSettings.ambientIntensity = (isDay ? startAmbientIntencity : num2);
			RenderSettings.reflectionIntensity = (isDay ? startReflectionIntencity : 0.2f);
			isDay = !isDay;
		}
		GUI.Label(new Rect(400f * dpiScale, 15f * dpiScale + num / 2f, 100f * dpiScale, 20f * dpiScale), "Prefab name is \"" + Prefabs[currentNomber].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
		GUI.DrawTexture(new Rect(12f * dpiScale, 140f * dpiScale + num, 285f * dpiScale, 15f * dpiScale), HUETexture, ScaleMode.StretchToFill, alphaBlend: false, 0f);
		float num3 = colorHUE;
		colorHUE = GUI.HorizontalSlider(new Rect(12f * dpiScale, 147f * dpiScale + num, 285f * dpiScale, 15f * dpiScale), colorHUE, 0f, 360f);
		if ((double)Mathf.Abs(num3 - colorHUE) > 0.001)
		{
			PSMeshRendererUpdater componentInChildren = characterInstance.GetComponentInChildren<PSMeshRendererUpdater>();
			if (componentInChildren != null)
			{
				componentInChildren.UpdateColor(colorHUE / 360f);
			}
			componentInChildren = modelInstance.GetComponentInChildren<PSMeshRendererUpdater>();
			if (componentInChildren != null)
			{
				componentInChildren.UpdateColor(colorHUE / 360f);
			}
		}
	}

	private void ChangeCurrent(int delta)
	{
		currentNomber += delta;
		if (currentNomber > Prefabs.Length - 1)
		{
			currentNomber = 0;
		}
		else if (currentNomber < 0)
		{
			currentNomber = Prefabs.Length - 1;
		}
		if (characterInstance != null)
		{
			Object.Destroy(characterInstance);
			RemoveClones();
		}
		if (modelInstance != null)
		{
			Object.Destroy(modelInstance);
			RemoveClones();
		}
		characterInstance = Object.Instantiate(Character);
		characterInstance.GetComponent<ME_AnimatorEvents>().EffectPrefab = Prefabs[currentNomber];
		modelInstance = Object.Instantiate(Model);
		GameObject obj = Object.Instantiate(Prefabs[currentNomber]);
		obj.transform.parent = modelInstance.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = default(Quaternion);
		obj.GetComponent<PSMeshRendererUpdater>().UpdateMeshEffect(modelInstance);
		if (UseMobileVersion)
		{
			CancelInvoke("ReactivateEffect");
		}
	}

	private void RemoveClones()
	{
		GameObject[] array = Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.name.Contains("(Clone)"))
			{
				Object.Destroy(gameObject);
			}
		}
	}

	private void ReactivateEffect()
	{
		characterInstance.SetActive(value: false);
		characterInstance.SetActive(value: true);
		modelInstance.SetActive(value: false);
		modelInstance.SetActive(value: true);
	}
}
