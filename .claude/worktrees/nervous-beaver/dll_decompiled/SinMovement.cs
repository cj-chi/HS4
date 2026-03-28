using UnityEngine;

public class SinMovement : MonoBehaviour
{
	public float speed = 10f;

	public float magnitude = 5f;

	private Vector3 startPosition;

	private void Start()
	{
		startPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		base.transform.position = Vector3.forward * Mathf.Sin(Time.time * speed) * magnitude + startPosition;
	}
}
