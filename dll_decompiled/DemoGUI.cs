using UnityEngine;

public class DemoGUI : MonoBehaviour
{
	public Texture HUETexture;

	public Material mat;

	public Position[] Positions;

	public GameObject[] Prefabs;

	private int currentNomber;

	private GameObject currentInstance;

	private GUIStyle guiStyleHeader = new GUIStyle();

	private float colorHUE;

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
		guiStyleHeader.fontSize = (int)(15f * dpiScale);
		guiStyleHeader.normal.textColor = new Color(1f, 1f, 1f);
		currentInstance = Object.Instantiate(Prefabs[currentNomber], base.transform.position, default(Quaternion));
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
		GUI.Label(new Rect(300f * dpiScale, 15f * dpiScale, 100f * dpiScale, 20f * dpiScale), "Prefab name is \"" + Prefabs[currentNomber].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
		GUI.DrawTexture(new Rect(12f * dpiScale, 80f * dpiScale, 220f * dpiScale, 15f * dpiScale), HUETexture, ScaleMode.StretchToFill, alphaBlend: false, 0f);
		float num = colorHUE;
		colorHUE = GUI.HorizontalSlider(new Rect(12f * dpiScale, 105f * dpiScale, 220f * dpiScale, 15f * dpiScale), colorHUE, 0f, 1530f);
		if ((double)Mathf.Abs(num - colorHUE) > 0.001)
		{
			ChangeColor();
		}
		GUI.Label(new Rect(240f * dpiScale, 105f * dpiScale, 30f * dpiScale, 30f * dpiScale), "Effect color", guiStyleHeader);
	}

	private void ChangeColor()
	{
		Color color = Hue(colorHUE / 255f);
		Renderer[] componentsInChildren = currentInstance.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material material = componentsInChildren[i].material;
			if (!(material == null) && material.HasProperty("_TintColor"))
			{
				color.a = material.GetColor("_TintColor").a;
				material.SetColor("_TintColor", color);
			}
		}
		Light componentInChildren = currentInstance.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			componentInChildren.color = color;
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
		if (currentInstance != null)
		{
			Object.Destroy(currentInstance);
		}
		Vector3 position = base.transform.position;
		if (Positions[currentNomber] == Position.Bottom)
		{
			position.y -= 1f;
		}
		if (Positions[currentNomber] == Position.Bottom02)
		{
			position.y -= 0.8f;
		}
		currentInstance = Object.Instantiate(Prefabs[currentNomber], position, default(Quaternion));
	}
}
