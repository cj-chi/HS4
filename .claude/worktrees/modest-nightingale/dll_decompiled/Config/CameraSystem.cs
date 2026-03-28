namespace Config;

public class CameraSystem : BaseSystem
{
	public const float SENSITIVITY_X = 0.5f;

	public const float SENSITIVITY_Y = 0.5f;

	public float SensitivityX = 0.5f;

	public float SensitivityY = 0.5f;

	public bool InvertMoveX;

	public bool InvertMoveY;

	public bool Look = true;

	public CameraSystem(string elementName)
		: base(elementName)
	{
	}

	public override void Init()
	{
		SensitivityX = 0.5f;
		SensitivityY = 0.5f;
		InvertMoveX = false;
		InvertMoveY = false;
		Look = true;
	}
}
