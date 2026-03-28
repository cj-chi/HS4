using UnityEngine;

public class AimController : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100f));
		base.transform.position = position;
	}
}
