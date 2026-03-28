using UnityEngine;

public class WaveGenerator : MonoBehaviour
{
	public float amplitude = 2f;

	public float frequency = 2f;

	private float angle;

	private float originalX;

	private void Start()
	{
		originalX = base.transform.position.x;
	}

	private void Update()
	{
		angle += Time.deltaTime * frequency;
		Vector3 position = base.transform.position;
		position.x = originalX + Mathf.Sin(angle) * amplitude;
		base.transform.position = position;
	}
}
