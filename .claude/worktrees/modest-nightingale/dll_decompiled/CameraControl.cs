using System.IO;
using Manager;
using UnityEngine;

public class CameraControl : BaseCameraControl
{
	public SmartTouch smartTouch;

	public PinchInOut pinchInOut;

	private Transform targetTex;

	public bool disableShortcut;

	public bool isOutsideTargetTex { get; set; }

	public void SetCenterSC()
	{
	}

	public void ChangeDepthOfFieldSetting()
	{
	}

	public void UpdateDepthOfFieldSetting()
	{
		_ = disableShortcut;
	}

	protected override void Start()
	{
		base.Start();
		targetTex = base.transform.Find("CameraTarget");
		if ((bool)targetTex)
		{
			targetTex.localScale = Vector3.one * 0.01f;
		}
		isOutsideTargetTex = true;
	}

	protected new void LateUpdate()
	{
		UpdateDepthOfFieldSetting();
		if (!Scene.IsFadeNow && !Scene.IsOverlap)
		{
			base.LateUpdate();
			if (!disableShortcut && Input.GetKeyDown(KeyCode.Alpha6))
			{
				SetCenterSC();
			}
		}
	}

	protected new bool InputTouchProc()
	{
		if (base.InputTouchProc())
		{
			float deltaTime = Time.deltaTime;
			if ((bool)pinchInOut)
			{
				float rate = pinchInOut.Rate;
				if (pinchInOut.NowState == PinchInOut.State.ScalUp)
				{
					CamDat.Dir.z += rate * deltaTime * zoomSpeed;
				}
				else if (pinchInOut.NowState == PinchInOut.State.ScalDown)
				{
					CamDat.Dir.z -= rate * deltaTime * zoomSpeed;
				}
			}
			return true;
		}
		return false;
	}

	private void Save(BinaryWriter Writer)
	{
		Writer.Write(CamDat.Pos.x);
		Writer.Write(CamDat.Pos.y);
		Writer.Write(CamDat.Pos.z);
		Writer.Write(CamDat.Dir.x);
		Writer.Write(CamDat.Dir.y);
		Writer.Write(CamDat.Dir.z);
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		Writer.Write(eulerAngles.x);
		Writer.Write(eulerAngles.y);
		Writer.Write(eulerAngles.z);
	}

	private void Load(BinaryReader Reader)
	{
		CamDat.Pos.x = Reader.ReadSingle();
		CamDat.Pos.y = Reader.ReadSingle();
		CamDat.Pos.z = Reader.ReadSingle();
		CamDat.Dir.x = Reader.ReadSingle();
		CamDat.Dir.y = Reader.ReadSingle();
		CamDat.Dir.z = Reader.ReadSingle();
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		eulerAngles.x = Reader.ReadSingle();
		eulerAngles.y = Reader.ReadSingle();
		eulerAngles.z = Reader.ReadSingle();
		base.transform.rotation = Quaternion.Euler(eulerAngles);
	}
}
