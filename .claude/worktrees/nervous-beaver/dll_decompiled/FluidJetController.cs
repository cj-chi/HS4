using Obi;
using UnityEngine;

public class FluidJetController : MonoBehaviour
{
	private ObiEmitter emitter;

	public float emissionSpeed = 10f;

	public float moveSpeed = 2f;

	private void Start()
	{
		emitter = GetComponentInChildren<ObiEmitter>();
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.W))
		{
			emitter.speed = emissionSpeed;
		}
		else
		{
			emitter.speed = 0f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			base.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
		}
	}
}
