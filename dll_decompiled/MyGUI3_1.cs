using System;
using UnityEngine;

public class MyGUI3_1 : MonoBehaviour
{
	public enum GuiStat
	{
		Ball,
		BallRotate,
		BallRotatex4,
		Bottom,
		Middle,
		MiddleWithoutRobot,
		Top,
		TopTarget
	}

	public struct HSBColor
	{
		public float h;

		public float s;

		public float b;

		public float a;

		public HSBColor(float h, float s, float b, float a)
		{
			this.h = h;
			this.s = s;
			this.b = b;
			this.a = a;
		}
	}

	public Texture HUETexture;

	public int CurrentPrefabNomber;

	public float UpdateInterval = 0.5f;

	public Light DirLight;

	public GameObject Target;

	public GameObject TargetForRay;

	public GameObject TopPosition;

	public GameObject MiddlePosition;

	public Vector3 defaultRobotPos;

	public GameObject BottomPosition;

	public GameObject Plane1;

	public GameObject Plane2;

	public Material[] PlaneMaterials;

	public GuiStat[] GuiStats;

	public GameObject[] Prefabs;

	private float oldLightIntensity;

	private Color oldAmbientColor;

	private GameObject currentGo;

	private bool isDay;

	private bool isDefaultPlaneTexture;

	private int current;

	private EffectSettings effectSettings;

	private bool isReadyEffect;

	private Quaternion defaultRobotRotation;

	private float colorHUE;

	private GUIStyle guiStyleHeader = new GUIStyle();

	private float dpiScale;

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
		oldAmbientColor = RenderSettings.ambientLight;
		oldLightIntensity = DirLight.intensity;
		guiStyleHeader.fontSize = (int)(15f * dpiScale);
		guiStyleHeader.normal.textColor = new Color(1f, 1f, 1f);
		current = CurrentPrefabNomber;
		InstanceCurrent(GuiStats[CurrentPrefabNomber]);
	}

	private void InstanceEffect(Vector3 pos)
	{
		currentGo = UnityEngine.Object.Instantiate(Prefabs[current], pos, Prefabs[current].transform.rotation);
		effectSettings = currentGo.GetComponent<EffectSettings>();
		effectSettings.Target = GetTargetObject(GuiStats[current]);
		effectSettings.EffectDeactivated += effectSettings_EffectDeactivated;
		if (GuiStats[current] == GuiStat.Middle)
		{
			currentGo.transform.parent = GetTargetObject(GuiStat.Middle).transform;
			currentGo.transform.position = GetInstancePosition(GuiStat.Middle);
		}
		else
		{
			currentGo.transform.parent = base.transform;
		}
		effectSettings.CollisionEnter += delegate(object n, CollisionInfo e)
		{
			_ = e.Hit.transform != null;
		};
	}

	private GameObject GetTargetObject(GuiStat stat)
	{
		switch (stat)
		{
		case GuiStat.Ball:
			return Target;
		case GuiStat.BallRotate:
			return Target;
		case GuiStat.Bottom:
			return BottomPosition;
		case GuiStat.Top:
			return TopPosition;
		case GuiStat.TopTarget:
			return BottomPosition;
		case GuiStat.Middle:
			MiddlePosition.transform.localPosition = defaultRobotPos;
			MiddlePosition.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			return MiddlePosition;
		case GuiStat.MiddleWithoutRobot:
			return MiddlePosition.transform.parent.gameObject;
		default:
			return base.gameObject;
		}
	}

	private void effectSettings_EffectDeactivated(object sender, EventArgs e)
	{
		if (GuiStats[current] != GuiStat.Middle)
		{
			currentGo.transform.position = GetInstancePosition(GuiStats[current]);
		}
		isReadyEffect = true;
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f * dpiScale, 15f * dpiScale, 105f * dpiScale, 30f * dpiScale), "Previous Effect"))
		{
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(130f * dpiScale, 15f * dpiScale, 105f * dpiScale, 30f * dpiScale), "Next Effect"))
		{
			ChangeCurrent(1);
		}
		if (Prefabs[current] != null)
		{
			GUI.Label(new Rect(300f * dpiScale, 15f * dpiScale, 100f * dpiScale, 20f * dpiScale), "Prefab name is \"" + Prefabs[current].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
		}
		if (GUI.Button(new Rect(10f * dpiScale, 60f * dpiScale, 225f * dpiScale, 30f * dpiScale), "Day/Night"))
		{
			DirLight.intensity = ((!isDay) ? 0f : oldLightIntensity);
			DirLight.transform.rotation = (isDay ? Quaternion.Euler(400f, 30f, 90f) : Quaternion.Euler(350f, 30f, 90f));
			RenderSettings.ambientLight = ((!isDay) ? new Color(0.1f, 0.1f, 0.1f) : oldAmbientColor);
			RenderSettings.ambientIntensity = (isDay ? 0.5f : 0.1f);
			RenderSettings.reflectionIntensity = (isDay ? 1f : 0.1f);
			isDay = !isDay;
		}
		GUI.DrawTexture(new Rect(12f * dpiScale, 110f * dpiScale, 220f * dpiScale, 15f * dpiScale), HUETexture, ScaleMode.StretchToFill, alphaBlend: false, 0f);
		float num = colorHUE;
		colorHUE = GUI.HorizontalSlider(new Rect(12f * dpiScale, 135f * dpiScale, 220f * dpiScale, 15f * dpiScale), colorHUE, 0f, 360f);
		if ((double)Mathf.Abs(num - colorHUE) > 0.001)
		{
			ChangeColor();
		}
		GUI.Label(new Rect(240f * dpiScale, 105f * dpiScale, 30f * dpiScale, 30f * dpiScale), "Effect color", guiStyleHeader);
	}

	private void Update()
	{
		if (isReadyEffect)
		{
			isReadyEffect = false;
			currentGo.SetActive(value: true);
		}
		if (GuiStats[current] == GuiStat.BallRotate)
		{
			currentGo.transform.localRotation = Quaternion.Euler(0f, Mathf.PingPong(Time.time * 5f, 60f) - 50f, 0f);
		}
		if (GuiStats[current] == GuiStat.BallRotatex4)
		{
			currentGo.transform.localRotation = Quaternion.Euler(0f, Mathf.PingPong(Time.time * 30f, 100f) - 70f, 0f);
		}
	}

	private void InstanceCurrent(GuiStat stat)
	{
		switch (stat)
		{
		case GuiStat.Ball:
			MiddlePosition.SetActive(value: false);
			InstanceEffect(base.transform.position);
			break;
		case GuiStat.BallRotate:
			MiddlePosition.SetActive(value: false);
			InstanceEffect(base.transform.position);
			break;
		case GuiStat.BallRotatex4:
			MiddlePosition.SetActive(value: false);
			InstanceEffect(base.transform.position);
			break;
		case GuiStat.Bottom:
			MiddlePosition.SetActive(value: false);
			InstanceEffect(BottomPosition.transform.position);
			break;
		case GuiStat.Top:
			MiddlePosition.SetActive(value: false);
			InstanceEffect(TopPosition.transform.position);
			break;
		case GuiStat.TopTarget:
			MiddlePosition.SetActive(value: false);
			InstanceEffect(TopPosition.transform.position);
			break;
		case GuiStat.Middle:
			MiddlePosition.SetActive(value: true);
			InstanceEffect(MiddlePosition.transform.parent.transform.position);
			break;
		case GuiStat.MiddleWithoutRobot:
			MiddlePosition.SetActive(value: false);
			InstanceEffect(MiddlePosition.transform.position);
			break;
		}
	}

	private Vector3 GetInstancePosition(GuiStat stat)
	{
		return stat switch
		{
			GuiStat.Ball => base.transform.position, 
			GuiStat.BallRotate => base.transform.position, 
			GuiStat.BallRotatex4 => base.transform.position, 
			GuiStat.Bottom => BottomPosition.transform.position, 
			GuiStat.Top => TopPosition.transform.position, 
			GuiStat.TopTarget => TopPosition.transform.position, 
			GuiStat.MiddleWithoutRobot => MiddlePosition.transform.parent.transform.position, 
			GuiStat.Middle => MiddlePosition.transform.parent.transform.position, 
			_ => base.transform.position, 
		};
	}

	private void ChangeCurrent(int delta)
	{
		UnityEngine.Object.Destroy(currentGo);
		CancelInvoke("InstanceDefaulBall");
		current += delta;
		if (current > Prefabs.Length - 1)
		{
			current = 0;
		}
		else if (current < 0)
		{
			current = Prefabs.Length - 1;
		}
		if (effectSettings != null)
		{
			effectSettings.EffectDeactivated -= effectSettings_EffectDeactivated;
		}
		MiddlePosition.SetActive(GuiStats[current] == GuiStat.Middle);
		InstanceEffect(GetInstancePosition(GuiStats[current]));
		if (TargetForRay != null)
		{
			if (current == 14 || current == 22)
			{
				TargetForRay.SetActive(value: true);
			}
			else
			{
				TargetForRay.SetActive(value: false);
			}
		}
	}

	private Color Hue(float H)
	{
		Color result = new Color(1f, 0f, 0f);
		if (H >= 0f && H < 1f)
		{
			result = new Color(1f, 0f, H);
		}
		if (H >= 1f && H < 2f)
		{
			result = new Color(2f - H, 0f, 1f);
		}
		if (H >= 2f && H < 3f)
		{
			result = new Color(0f, H - 2f, 1f);
		}
		if (H >= 3f && H < 4f)
		{
			result = new Color(0f, 1f, 4f - H);
		}
		if (H >= 4f && H < 5f)
		{
			result = new Color(H - 4f, 1f, 0f);
		}
		if (H >= 5f && H < 6f)
		{
			result = new Color(1f, 6f - H, 0f);
		}
		return result;
	}

	public HSBColor ColorToHSV(Color color)
	{
		HSBColor result = new HSBColor(0f, 0f, 0f, color.a);
		float r = color.r;
		float g = color.g;
		float b = color.b;
		float num = Mathf.Max(r, Mathf.Max(g, b));
		if (num <= 0f)
		{
			return result;
		}
		float num2 = Mathf.Min(r, Mathf.Min(g, b));
		float num3 = num - num2;
		if (num > num2)
		{
			if (g == num)
			{
				result.h = (b - r) / num3 * 60f + 120f;
			}
			else if (b == num)
			{
				result.h = (r - g) / num3 * 60f + 240f;
			}
			else if (b > g)
			{
				result.h = (g - b) / num3 * 60f + 360f;
			}
			else
			{
				result.h = (g - b) / num3 * 60f;
			}
			if (result.h < 0f)
			{
				result.h += 360f;
			}
		}
		else
		{
			result.h = 0f;
		}
		result.h *= 0.0027777778f;
		result.s = num3 / num * 1f;
		result.b = num;
		return result;
	}

	public Color HSVToColor(HSBColor hsbColor)
	{
		float value = hsbColor.b;
		float value2 = hsbColor.b;
		float value3 = hsbColor.b;
		if (hsbColor.s != 0f)
		{
			float b = hsbColor.b;
			float num = hsbColor.b * hsbColor.s;
			float num2 = hsbColor.b - num;
			float num3 = hsbColor.h * 360f;
			if (num3 < 60f)
			{
				value = b;
				value2 = num3 * num / 60f + num2;
				value3 = num2;
			}
			else if (num3 < 120f)
			{
				value = (0f - (num3 - 120f)) * num / 60f + num2;
				value2 = b;
				value3 = num2;
			}
			else if (num3 < 180f)
			{
				value = num2;
				value2 = b;
				value3 = (num3 - 120f) * num / 60f + num2;
			}
			else if (num3 < 240f)
			{
				value = num2;
				value2 = (0f - (num3 - 240f)) * num / 60f + num2;
				value3 = b;
			}
			else if (num3 < 300f)
			{
				value = (num3 - 240f) * num / 60f + num2;
				value2 = num2;
				value3 = b;
			}
			else if (num3 <= 360f)
			{
				value = b;
				value2 = num2;
				value3 = (0f - (num3 - 360f)) * num / 60f + num2;
			}
			else
			{
				value = 0f;
				value2 = 0f;
				value3 = 0f;
			}
		}
		return new Color(Mathf.Clamp01(value), Mathf.Clamp01(value2), Mathf.Clamp01(value3), hsbColor.a);
	}

	private void ChangeColor()
	{
		Color color = Hue(colorHUE / 255f);
		Renderer[] componentsInChildren = currentGo.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material material = componentsInChildren[i].material;
			if (!(material == null))
			{
				if (material.HasProperty("_TintColor"))
				{
					Color color2 = material.GetColor("_TintColor");
					HSBColor hsbColor = ColorToHSV(color2);
					hsbColor.h = colorHUE / 360f;
					color = HSVToColor(hsbColor);
					color.a = color2.a;
					material.SetColor("_TintColor", color);
				}
				if (material.HasProperty("_CoreColor"))
				{
					Color color3 = material.GetColor("_CoreColor");
					HSBColor hsbColor2 = ColorToHSV(color3);
					hsbColor2.h = colorHUE / 360f;
					color = HSVToColor(hsbColor2);
					color.a = color3.a;
					material.SetColor("_CoreColor", color);
				}
			}
		}
		Projector[] componentsInChildren2 = currentGo.GetComponentsInChildren<Projector>();
		foreach (Projector projector in componentsInChildren2)
		{
			Material material2 = projector.material;
			if (!(material2 == null) && material2.HasProperty("_TintColor"))
			{
				Color color4 = material2.GetColor("_TintColor");
				HSBColor hsbColor3 = ColorToHSV(color4);
				hsbColor3.h = colorHUE / 360f;
				color = HSVToColor(hsbColor3);
				color.a = color4.a;
				projector.material.SetColor("_TintColor", color);
			}
		}
		Light componentInChildren = currentGo.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			componentInChildren.color = color;
		}
	}
}
