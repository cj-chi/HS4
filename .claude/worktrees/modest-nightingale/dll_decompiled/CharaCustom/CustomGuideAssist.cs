namespace CharaCustom;

public class CustomGuideAssist
{
	public static void SetCameraMoveFlag(CameraControl_Ver2 _ctrl, bool _bPlay)
	{
		if (!(_ctrl == null) && IsCameraMoveFlag(_ctrl) != _bPlay)
		{
			_ctrl.NoCtrlCondition = () => !_bPlay;
		}
	}

	public static bool IsCameraMoveFlag(CameraControl_Ver2 _ctrl)
	{
		if (_ctrl == null)
		{
			return false;
		}
		BaseCameraControl_Ver2.NoCtrlFunc noCtrlCondition = _ctrl.NoCtrlCondition;
		bool flag = true;
		if (noCtrlCondition != null)
		{
			flag = noCtrlCondition();
		}
		return !flag;
	}

	public static bool IsCameraActionFlag(CameraControl_Ver2 _ctrl)
	{
		if (_ctrl == null)
		{
			return false;
		}
		return _ctrl.isControlNow;
	}
}
