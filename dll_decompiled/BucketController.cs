using Obi;
using UnityEngine;

public class BucketController : MonoBehaviour
{
	public ObiEmitter emitter;

	private void Update()
	{
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.AngleAxis(90f, -base.transform.forward), Time.deltaTime * 50f);
		}
		else
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.identity, Time.deltaTime * 100f);
		}
		if (Input.GetKey(KeyCode.R))
		{
			emitter.KillAll();
		}
	}
}
