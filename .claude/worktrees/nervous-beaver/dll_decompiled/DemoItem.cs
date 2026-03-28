using UnityEngine;

public class DemoItem : MonoBehaviour
{
	private static Vector3 lookAtZ = new Vector3(0f, 0f, 1f);

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.LookAt(base.transform.position + lookAtZ);
	}
}
