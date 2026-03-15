using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff.Demos.Demo_Assets;

public class PauseTimeline : MonoBehaviour
{
	private const float MovementSpeed = 3f;

	private const float FreeLookSensitivity = 3f;

	private bool _isCameraFree;

	private bool _isLooking;

	public bool ShowPauseTimeline = true;

	private void Start()
	{
		if (XRSettings.enabled)
		{
			GetComponent<PlayableDirector>().enabled = false;
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (_isCameraFree)
		{
			FreeLook();
		}
	}

	public void OnGUI()
	{
		if (XRSettings.enabled || !ShowPauseTimeline)
		{
			return;
		}
		PlayableDirector component = GetComponent<PlayableDirector>();
		Rect position = new Rect(20f, 50f, 130f, 30f);
		if (!_isCameraFree)
		{
			if (GUI.Button(position, "Free Camera"))
			{
				_isCameraFree = true;
				component.Pause();
			}
		}
		else if (GUI.Button(position, component.isActiveAndEnabled ? "Resume Timeline" : "Lock Camera"))
		{
			_isCameraFree = false;
			component.Resume();
		}
	}

	private void FreeLook()
	{
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			base.transform.position = base.transform.position + base.transform.forward * 3f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			base.transform.position = base.transform.position + -base.transform.right * 3f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			base.transform.position = base.transform.position + -base.transform.forward * 3f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			base.transform.position = base.transform.position + base.transform.right * 3f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.E))
		{
			base.transform.position = base.transform.position + base.transform.up * 3f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			base.transform.position = base.transform.position + -base.transform.up * 3f * Time.deltaTime;
		}
		if (_isLooking)
		{
			float y = base.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 3f;
			float x = base.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * 3f;
			base.transform.localEulerAngles = new Vector3(x, y, 0f);
		}
		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			_isLooking = true;
		}
		else if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			_isLooking = false;
		}
	}
}
