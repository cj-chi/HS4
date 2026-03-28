using UnityEngine;

public class CameraEffectorColorMask : MonoBehaviour
{
	private Camera myCamera;

	private Camera effectorCamera;

	public bool Enabled
	{
		get
		{
			return base.enabled;
		}
		set
		{
			myCamera.enabled = value;
			base.enabled = value;
		}
	}

	private void Awake()
	{
		myCamera = GetComponent<Camera>();
		if (myCamera == null)
		{
			Object.Destroy(this);
		}
		myCamera.CopyFrom(effectorCamera);
		myCamera.clearFlags = CameraClearFlags.Color;
		myCamera.backgroundColor = Color.black;
		myCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		Enabled = true;
	}

	private void Update()
	{
		myCamera.fieldOfView = effectorCamera.fieldOfView;
		myCamera.rect = effectorCamera.rect;
	}
}
