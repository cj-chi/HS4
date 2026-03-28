using UnityEngine;

public class LiquidRotate : MonoBehaviour
{
	private Quaternion TargetRot;

	public float RotateEverySecond = 1f;

	private void Start()
	{
		RandomRot();
		InvokeRepeating("RandomRot", 0f, RotateEverySecond);
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, TargetRot, Time.time * Time.deltaTime);
	}

	private void RandomRot()
	{
		TargetRot = Random.rotation;
	}
}
