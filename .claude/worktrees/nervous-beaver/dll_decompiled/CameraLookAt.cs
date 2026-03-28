using UnityEngine;

public class CameraLookAt : MonoBehaviour
{
	public GameObject target;

	public float radius = 10f;

	public float vel = 5f;

	private void Start()
	{
	}

	private void Update()
	{
		float x = radius * Mathf.Cos(Time.time * vel);
		float y = radius * Mathf.Sin(Time.time * vel);
		base.transform.position = new Vector3(x, y, -10f);
		if ((bool)base.transform && target != null)
		{
			base.transform.LookAt(target.transform);
		}
	}
}
