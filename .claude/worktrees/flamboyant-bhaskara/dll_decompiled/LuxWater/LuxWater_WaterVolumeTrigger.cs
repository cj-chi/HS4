using UnityEngine;

namespace LuxWater;

public class LuxWater_WaterVolumeTrigger : MonoBehaviour
{
	[Space(6f)]
	[LuxWater_HelpBtn("h.cetbv2etlk23")]
	public Camera cam;

	public bool active = true;

	private void OnEnable()
	{
		if (cam == null)
		{
			Camera component = GetComponent<Camera>();
			if (component != null)
			{
				cam = component;
			}
			else
			{
				active = false;
			}
		}
	}
}
