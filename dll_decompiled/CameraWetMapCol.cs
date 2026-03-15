using UnityEngine;

public class CameraWetMapCol : Singleton<CameraWetMapCol>
{
	[SerializeField]
	private Collider[] hitWater;

	public Collider[] HitWater => hitWater;
}
