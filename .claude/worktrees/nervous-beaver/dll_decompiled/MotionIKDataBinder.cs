using UnityEngine;

public class MotionIKDataBinder : MonoBehaviour
{
	public MotionIK motionIK { get; set; }

	public MotionIKData data { get; set; }

	public MotionIKData.State state { get; set; }

	public string stateName
	{
		get
		{
			if (state != null)
			{
				return state.name;
			}
			return string.Empty;
		}
	}
}
