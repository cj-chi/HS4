using UnityEngine;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff.Demos.Demo_Assets;

public class VrCameraSwitcher : MonoBehaviour
{
	public bool EnableInVr;

	public Camera Camera;

	private void Start()
	{
		if (Camera == null)
		{
			Camera = GetComponent<Camera>();
		}
		if (EnableInVr)
		{
			Camera.enabled = XRSettings.enabled;
		}
		else
		{
			Camera.enabled = !XRSettings.enabled;
		}
	}
}
