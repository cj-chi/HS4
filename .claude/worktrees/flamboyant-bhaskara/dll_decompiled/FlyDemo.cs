using UnityEngine;

public class FlyDemo : MonoBehaviour
{
	public float Speed = 1f;

	public float Height = 1f;

	private Transform t;

	private float time;

	private void Start()
	{
		t = base.transform;
	}

	private void Update()
	{
		time += Time.deltaTime;
		float num = Mathf.Cos(time / Speed);
		t.localPosition = new Vector3(0f, 0f, num * Height);
	}
}
