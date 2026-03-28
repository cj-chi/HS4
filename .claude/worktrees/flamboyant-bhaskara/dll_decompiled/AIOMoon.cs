using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class AIOMoon : MonoBehaviour
{
	private Transform moonLight;

	private Material skybox;

	private bool isAIOskybox;

	[Range(0f, 39f)]
	public float MoonSize;

	private void OnEnable()
	{
		moonLight = base.gameObject.GetComponent<Transform>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (moonLight != null && RenderSettings.skybox != null)
		{
			if (RenderSettings.skybox.HasProperty("_MoonPosition"))
			{
				RenderSettings.skybox.SetVector("_MoonPosition", Vector3.Normalize(-moonLight.transform.forward));
			}
			if (RenderSettings.skybox.HasProperty("_moonScale"))
			{
				RenderSettings.skybox.SetFloat("_moonScale", 40f - MoonSize);
			}
		}
	}
}
