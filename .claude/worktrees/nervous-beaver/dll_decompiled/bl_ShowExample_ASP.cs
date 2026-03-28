using UnityEngine;

public class bl_ShowExample_ASP : MonoBehaviour
{
	private bl_AllOptionsPro AllSettings;

	private void Awake()
	{
		AllSettings = Object.FindObjectOfType<bl_AllOptionsPro>();
	}

	private void Update()
	{
		if (bl_Input.GetKeyDown("Pause"))
		{
			AllSettings.ShowMenu();
		}
	}
}
