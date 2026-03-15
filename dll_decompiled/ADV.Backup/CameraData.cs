using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace ADV.Backup;

internal class CameraData
{
	private class BlurBK
	{
		private bool enabled { get; }

		private int iterations { get; }

		private float blurSpread { get; }

		public BlurBK(Blur blur)
		{
			if (!(blur == null))
			{
				enabled = blur.enabled;
				iterations = blur.iterations;
				blurSpread = blur.blurSpread;
			}
		}

		public void Set(Blur blur)
		{
			if (!(blur == null))
			{
				blur.enabled = enabled;
				blur.iterations = iterations;
				blur.blurSpread = blurSpread;
			}
		}
	}

	private class DOFBK
	{
		private bool enabled { get; }

		private Transform focalTransform { get; }

		private float focalLength { get; }

		private float focalSize { get; }

		private float aperture { get; }

		public DOFBK(DepthOfField dof)
		{
			if (!(dof == null))
			{
				enabled = dof.enabled;
				focalTransform = dof.focalTransform;
				focalLength = dof.focalLength;
				focalSize = dof.focalSize;
				aperture = dof.aperture;
			}
		}

		public void Set(DepthOfField dof)
		{
			if (!(dof == null))
			{
				dof.enabled = enabled;
				dof.focalTransform = focalTransform;
				dof.focalLength = focalLength;
				dof.focalSize = focalSize;
				dof.aperture = aperture;
			}
		}

		public void SetFocalTransform(DepthOfField dof)
		{
			if (!(dof == null))
			{
				dof.focalTransform = focalTransform;
			}
		}
	}

	private Rect rect { get; }

	private Vector3 pos { get; }

	private Quaternion rot { get; }

	private Transform parent { get; }

	private float fov { get; }

	private float far { get; }

	private BlurBK blurBK { get; }

	private DOFBK dofBK { get; }

	public void DefaultDOF(Component component)
	{
		dofBK?.SetFocalTransform(component.GetComponent<DepthOfField>());
	}

	public CameraData(Camera cam)
	{
		blurBK = null;
		dofBK = null;
		if (!(cam == null))
		{
			rect = cam.rect;
			pos = cam.transform.position;
			rot = cam.transform.rotation;
			parent = cam.transform.parent;
			fov = cam.fieldOfView;
			far = cam.farClipPlane;
			Blur component = cam.GetComponent<Blur>();
			if (component != null)
			{
				blurBK = new BlurBK(component);
			}
			DepthOfField component2 = cam.GetComponent<DepthOfField>();
			if (component2 != null)
			{
				dofBK = new DOFBK(component2);
			}
		}
	}

	public void Load(Camera cam, bool _isBackupPos = false)
	{
		if (!(cam == null))
		{
			cam.rect = rect;
			if (_isBackupPos)
			{
				cam.transform.position = pos;
				cam.transform.rotation = rot;
			}
			cam.transform.parent = parent;
			cam.fieldOfView = fov;
			cam.farClipPlane = far;
			blurBK?.Set(cam.GetComponent<Blur>());
			dofBK?.Set(cam.GetComponent<DepthOfField>());
		}
	}
}
