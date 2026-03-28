using UnityEngine;

public class LuxWater_SetToGerstnerHeight : MonoBehaviour
{
	public Material WaterMaterial;

	public Vector3 Damping = new Vector3(0.3f, 1f, 0.3f);

	public float TimeOffset;

	public bool UpdateWaterMaterialPerFrame;

	[Space(8f)]
	public bool AddCircleAnim;

	public float Radius = 6f;

	public float Speed = 1f;

	[Space(8f)]
	public Transform[] ManagedWaterProjectors;

	[Header("Debug")]
	public float MaxDisp;

	private Transform trans;

	private LuxWaterUtils.GersterWavesDescription Description;

	private bool ObjectIsVisible;

	private Vector3 Offset = Vector3.zero;

	private void Start()
	{
		trans = base.transform;
		LuxWaterUtils.GetGersterWavesDescription(ref Description, WaterMaterial);
	}

	private void OnBecameVisible()
	{
		ObjectIsVisible = true;
	}

	private void OnBecameInvisible()
	{
		ObjectIsVisible = false;
	}

	private void LateUpdate()
	{
		if ((!ObjectIsVisible && !AddCircleAnim) || WaterMaterial == null)
		{
			return;
		}
		if (UpdateWaterMaterialPerFrame)
		{
			LuxWaterUtils.GetGersterWavesDescription(ref Description, WaterMaterial);
		}
		Vector3 position = trans.position;
		position -= Offset;
		if (AddCircleAnim)
		{
			position.x += Mathf.Sin(Time.time * Speed) * Time.deltaTime * Radius;
			position.z += Mathf.Cos(Time.time * Speed) * Time.deltaTime * Radius;
		}
		int num = ManagedWaterProjectors.Length;
		if (num > 0)
		{
			for (int i = 0; i != num; i++)
			{
				Vector3 position2 = ManagedWaterProjectors[i].position;
				position2.x = position.x;
				position2.z = position.z;
				ManagedWaterProjectors[i].position = position2;
			}
		}
		Offset = LuxWaterUtils.GetGestnerDisplacement(position, Description, TimeOffset);
		Offset.x += Offset.x * Damping.x;
		Offset.y += Offset.y * Damping.y;
		Offset.z += Offset.z * Damping.z;
		trans.position = position + Offset;
	}
}
